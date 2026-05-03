using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace UiTools.WinForms.Designer.Core
{
    [XmlRoot("UiThemes")]
    public class KnownUiThemes
    {
        public const string NONE = "None (requires restart)";

        private const string UI_THEMES_FILE_NAME = "UiTools.WinForms.Designer.UiThemes.xml";
        private const string UI_THEMES_SCHEMA_FILE_NAME = "UiTools.WinForms.Designer.UiThemes.xsd";
        
        [XmlElement("UiTheme")]
        public List<UiTheme> UiThemes { get; set; }

        /// <summary>
        /// Loads the UI themes configuration by attempting to read from the local file first.
        /// Falls back to embedded resources if the local file is missing, invalid, or corrupted.
        /// Orchestrates XSD validation and triggers a self-healing process if errors are detected.
        /// </summary>
        /// <returns>
        /// A <see cref="KnownUiThemes"/> instance containing the loaded themes, 
        /// or <see langword="null"/> if the configuration cannot be loaded, validated, or recovered.
        /// </returns>
        public static KnownUiThemes Load()
        {
            string uiThemesConfigContents = null;
            var uiThemesFilePath = GetUiThemesFilePath();
            var uiThemesSchemaFilePath = Path.Combine(Path.GetDirectoryName(uiThemesFilePath), UI_THEMES_SCHEMA_FILE_NAME);

            var xmlResourceName = "UiTools.WinForms.Designer.Core.UiThemes." + UI_THEMES_FILE_NAME;
            var xsdResourceName = "UiTools.WinForms.Designer.Core.UiThemes." + UI_THEMES_SCHEMA_FILE_NAME;

            // Export the XSD schema from resources to a local file to enable IntelliSense 
            // and validation in XML editors, making manual configuration more reliable:
            ExportSchema(xsdResourceName, uiThemesSchemaFilePath);

            bool loadedFromDisk = false;
            if (File.Exists(uiThemesFilePath))
            {
                try
                {
                    uiThemesConfigContents = File.ReadAllText(uiThemesFilePath);
                    Debug.WriteLine("✽ Loaded file with configuration of UI Themes from disk.");
                    loadedFromDisk = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"✽ Failed to read file with configuration of UI Themes from disk ({uiThemesFilePath}): {ex.Message}");
                }
            }
            else
            {
                Debug.WriteLine($"✽ File with configuration of UI Themes not found ({uiThemesFilePath}).");
            }
            if (string.IsNullOrWhiteSpace(uiThemesConfigContents))
            {
                // No file or failed to read it --> extract from resources:
                uiThemesConfigContents = LoadFromResourcesAndSaveToDisk(xmlResourceName, uiThemesFilePath);
                if (uiThemesConfigContents == null)
                    return null;
            }
            // If we're here, uiThemesConfigContents was successfully read either from file or from resources
            var validationErrors = XmlValidator.Validate(uiThemesConfigContents, CommonStuff.GetEmbeddedResource(xsdResourceName));
            if (validationErrors.Count > 0)
            {
                var errMsg = "Configuration of UI Themes is not valid:";
                Debug.WriteLine("✽ " + errMsg);
                foreach (var error in validationErrors)
                {
                    errMsg += $"\n* {error}";
                    Debug.WriteLine("✽   " + error);
                }
                if (loadedFromDisk)
                {
                    return TrySelfHealing(errMsg, xmlResourceName, uiThemesFilePath);
                }
                else
                {
                    // There's very little chance that we get here - XML-file in *resources* is expected to be 100% valid; however, let it be.
                    MessageBox.Show(errMsg, "UI Themes Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return null;
                }
            }
            try
            {
                var knownUiThemes = XmlHelper.Deserialize<KnownUiThemes>(uiThemesConfigContents);
                Debug.WriteLine($"✽ Successfully deserialized UI Themes from {(loadedFromDisk ? "file" : "resources")}");
                return knownUiThemes;
            }
            catch (Exception ex)
            {
                if (loadedFromDisk)
                {
                    var errMsg = $"Failed to deserialize configuration of UI Themes from file '{uiThemesFilePath}': {ex.Message}";
                    Debug.WriteLine("✽ " + errMsg);
                    return TrySelfHealing(errMsg, xmlResourceName, uiThemesFilePath);
                }
                else
                {
                    Debug.WriteLine($"✽ Failed to deserialize default configuration of UI Themes from resources: {ex.Message}");
                    return null;
                }
            }
        }

        /// <summary>
        /// Attempts to recover a corrupted local configuration file by restoring the default version from embedded resources.
        /// Returns the successfully deserialized configuration.
        /// </summary>
        /// <param name="errMsg">The error message describing the corruption or validation failure.</param>
        /// <param name="resourceName">The name of the embedded resource to restore from.</param>
        /// <param name="uiThemesFilePath">The path of the local configuration file to recover.</param>
        /// <returns>A deserialized <see cref="KnownUiThemes"/> instance, or <see langword="null"/> if the resource cannot be loaded or deserialized.</returns>
        private static KnownUiThemes TrySelfHealing(string errMsg, string resourceName, string uiThemesFilePath)
        {
            bool overwriteFile = false;
            if (MessageBox.Show($"{errMsg}\n\nDefault configuration of UI Themes will be extracted from resources.\n" +
                "Overwrite existing configuration file with these defaults?", "UI Themes Configuration Error",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                overwriteFile = true;
            }
            // fallback: extract from resources (and possibly "heal" the broken file)
            var uiThemesConfigContents = LoadFromResourcesAndSaveToDisk(resourceName, uiThemesFilePath, selfHealing: true, overwriteFile: overwriteFile);
            if (uiThemesConfigContents == null)
                return null;
            try
            {
                var knownUiThemes = XmlHelper.Deserialize<KnownUiThemes>(uiThemesConfigContents);
                Debug.WriteLine("✽ Successfully deserialized UI Themes from resources");
                return knownUiThemes;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"✽ Failed to deserialize default configuration of UI Themes from resources: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Extracts the default configuration from embedded resources and optionally persists it to the local disk.
        /// </summary>
        /// <param name="resourceName">The full name of the embedded resource to extract.</param>
        /// <param name="uiThemesFilePath">The target path where the configuration file should be saved.</param>
        /// <param name="selfHealing">If set to <see langword="true"/>, the operation is logged as a recovery process.</param>
        /// <param name="overwriteFile">If <see langword="true"/>, the existing file on disk will be overwritten with the resource content.</param>
        /// <returns>
        /// The raw XML content of the configuration as a string, 
        /// or <see langword="null"/> if the resource cannot be found or read.
        /// </returns>
        private static string LoadFromResourcesAndSaveToDisk(string resourceName, string uiThemesFilePath, bool selfHealing = false, bool overwriteFile = true)
        {
            string uiThemesConfigContents;
            // Extract from resources:
            try
            {
                uiThemesConfigContents = CommonStuff.GetEmbeddedResource(resourceName);
                Debug.WriteLine($"✽ Extracted default configuration of UI Themes from resources{(selfHealing ? " (better than nothing)" : "")}.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"✽ CRITICAL: Failed to read embedded resource '{resourceName}': {ex.Message}");
                return null;
            }
            if (overwriteFile)
            {
                // Save to file:
                try
                {
                    File.WriteAllText(uiThemesFilePath, uiThemesConfigContents);
                    Debug.WriteLine($"✽ {(selfHealing ? "Rec" : "C")}reated file with default configuration of UI Themes ({uiThemesFilePath}).");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"✽ Failed to create file '{uiThemesFilePath}' with default configuration of UI Themes: {ex.Message}.");
                }
            }
            return uiThemesConfigContents;
        }

        public UiTheme GetUiTheme(string name) => name == NONE ? null : UiThemes.FirstOrDefault(p => p.Name == name);

        public static string GetUiThemesFilePath() => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), UI_THEMES_FILE_NAME);

        /// <summary>
        /// Exports the embedded XSD schema to a local file.
        /// This enables IntelliSense, auto-completion, and real-time validation in XML editors,
        /// making manual configuration more reliable and user-friendly.
        /// </summary>
        /// <param name="schemaName">The full name of the embedded XSD resource to extract.</param>
        /// <param name="schemaFilePath">The target path where the XSD file should be saved.</param>
        private static void ExportSchema(string xsdResourceName, string uiThemesSchemaFilePath)
        {
            try
            {
                var uiThemesSchemaContents = CommonStuff.GetEmbeddedResource(xsdResourceName);
                if (!string.IsNullOrEmpty(uiThemesSchemaContents))
                {
                    File.WriteAllText(uiThemesSchemaFilePath, uiThemesSchemaContents);
                    Debug.WriteLine($"✽ Updated XSD schema file: {uiThemesSchemaFilePath}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"✽ Failed to export XSD schema: {ex.Message}");
            }
        }
    }

    public class UiTheme : IDisposable
    {
        public CommonTheme AllControls { get; set; }

        [XmlElement("Control")]
        public List<ControlTheme> ControlThemes { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("font")]
        public string FontString { get; set; }

        private Font font;
        public Font GetFont()
        {
            if (font == null)
                font = FontFromString(FontString);
            return font;
        }

        public bool IsProbablyDark(out Color backColor)
        {
            // NOTE: "probably" — because the BackColor defined in the <AllControls> tag can be overridden
            //       in specific <Control> tags. This is a quick, non-critical check intended solely
            //       to prevent flickering; it only examines the <AllControls> tag without scanning
            //       all individual <Control> tags (which would effectively duplicate ThemeApplier.ApplyInternal).
            var backColorString = AllControls.ColorProperties.FirstOrDefault(cp => cp.Name == "BackColor")?.Value;
            if (backColorString != null)
            {
                backColor = ThemeApplier.ResolveColor(backColorString);
                return ThemeApplier.IsDark(backColor);
            }
            backColor = Color.Empty;
            return false;
        }

        private static Font FontFromString(string fontString)
        {
            if (string.IsNullOrEmpty(fontString))
                return null;
            try
            {
                return new FontConverter().ConvertFromInvariantString(fontString) as Font;
                // ConvertFromInvariantString() expects "English" format, e.g. "Microsoft Sans Serif, 10.5pt, style=Bold" (comma between parts and dot as decimal separator)
            }
            catch
            {
                return null;
            }
        }

        public void Dispose()
        {
            font?.Dispose();
        }

        public override string ToString()
        {
            return $"Name = '{Name}'";
        }
    }

    public class CommonTheme
    {
        [XmlElement("ColorProperty")]
        public List<ColorProperty> ColorProperties { get; set; }
    }
    
    public class ControlTheme
    {
        [XmlElement("ColorProperty")]
        public List<ColorProperty> ColorProperties { get; set; }

        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlAttribute("includeInheritedTypes")]
        public bool IncludeInheritedTypes { get; set; }

        public override string ToString()
        {
            return $"ControlTheme: type = '{Type}', includeInheritedTypes = {IncludeInheritedTypes}";
        }
    }
    
    public class ColorProperty
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("value")]
        public string Value { get; set; }

        public override string ToString()
        {
            return $"ColorProperty: name = '{Name}', value = '{Value}'";
        }
    }
}
