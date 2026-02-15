using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace UiTools.WinForms.Designer.Core
{
    public class CsProjectFileWrapper
    {
        private readonly string csprojPath;
        private readonly string projectDir;
        private readonly XDocument doc;
        private readonly XElement root;
        private readonly XNamespace ns;
        private readonly bool isSdkStyle;

        public string ProjectFilePath => csprojPath;
        public bool IsSdkStyle => isSdkStyle;

        public CsProjectFileWrapper(string csprojPath)
        {
            this.csprojPath = csprojPath ?? throw new ArgumentNullException(nameof(csprojPath));
            projectDir = Path.GetDirectoryName(this.csprojPath);

            if (!File.Exists(this.csprojPath))
                throw new FileNotFoundException($"Project file '{csprojPath}' not found");

            doc = XDocument.Load(this.csprojPath);
            root = doc.Root;

            if (root == null || root.Name.LocalName != "Project")
                throw new InvalidOperationException($"Invalid .csproj format: {csprojPath}");

            // "Legacy" projects have the namespace "http://schemas.microsoft.com/developer/msbuild/2003"
            // In new (SDK-style) projects, it is usually absent.
            ns = root.GetDefaultNamespace();

            // Determine whether it is SDK-style or not
            isSdkStyle = root.Attribute("Sdk") != null || root.Elements().Any(e => e.Name.LocalName == "Sdk");
        }

        /// <summary>
        /// Gets a value indicating whether implicit usings are enabled in this project.
        /// This is determined by the presence and value of the &lt;ImplicitUsings&gt; tag in the .csproj file.
        /// This property is only relevant for SDK-style projects.
        /// </summary>
        public bool ImplicitUsingsEnabled
        {
            get
            {
                if (!isSdkStyle)
                {
                    return false; // Implicit usings are a feature of SDK-style projects only
                }

                try
                {
                    // Look for <ImplicitUsings> tag
                    var implicitUsingsElement = root.Descendants(ns + "ImplicitUsings").FirstOrDefault();

                    // If the tag is present and its value is "enable" (case-insensitive)
                    if (implicitUsingsElement != null &&
                        implicitUsingsElement.Value.Trim().Equals("enable", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    MessageLogger.LogError(this, $"Error checking ImplicitUsings in project file '{csprojPath}': {ex.Message}", ex);
                }

                return false; // Default to false if tag is absent or has a different value
            }
        }

        /// <summary>
        /// Determines the path to the bin directory based on the configuration and platform.
        /// Supports both legacy (.NET Framework) and SDK-style projects.
        /// </summary>
        public string DetermineBinDirectory(string configuration, string platform)
        {
            string outputPath = null;

            try
            {
                if (string.Equals(platform, "Any CPU", StringComparison.OrdinalIgnoreCase))
                    platform = "AnyCPU";
                MessageLogger.Log(this, $"Looking for bin directory (Configuration: '{configuration}', Platform: '{platform}')...");

                // 1. Look for OutputPath in a PropertyGroup with a matching Condition.
                //    We are looking for a group whose condition contains BOTH the configuration AND the platform.
                //    Example: Condition=" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' "
                var conditionalGroup = root.Elements(ns + "PropertyGroup")
                    .FirstOrDefault(g => {
                        var condition = g.Attribute("Condition")?.Value;
                        return condition != null && Regex.IsMatch(condition,
                            $@"==\s*['""]{Regex.Escape(configuration)}\|{Regex.Escape(platform)}['""]", RegexOptions.IgnoreCase);
                    });

                outputPath = conditionalGroup?.Element(ns + "OutputPath")?.Value;

                // 2. If not found in a specific block, look in a more general one (configuration only)
                if (string.IsNullOrEmpty(outputPath))
                {
                    MessageLogger.LogVerbose(this, $"  PropertyGroup/Condition with '$(Configuration)|$(Platform)' == '{configuration}|{platform}' not found");
                    outputPath = root.Elements(ns + "PropertyGroup")
                        .Where(g => {
                            var condition = g.Attribute("Condition")?.Value;
                            return condition != null && Regex.IsMatch(condition,
                                $@"==\s*['""]({Regex.Escape(configuration)}|{Regex.Escape(configuration)}\|[^'"" ]*|[^'"" ]*\|{Regex.Escape(configuration)})['""]",
                                RegexOptions.IgnoreCase);
                        })
                        .Select(g => g.Element(ns + "OutputPath")?.Value)
                        .FirstOrDefault(val => !string.IsNullOrEmpty(val));
                    if (string.IsNullOrEmpty(outputPath))
                        MessageLogger.LogVerbose(this, $"  PropertyGroup/Condition with '$(Configuration)' == '{configuration}' and nested OutputPath not found");
                    else
                        MessageLogger.LogVerbose(this, $"  At least found PropertyGroup/Condition with '$(Configuration)' == '{configuration}'");
                }
                else
                    MessageLogger.LogVerbose(this, $"  Found PropertyGroup/Condition with '$(Configuration)|$(Platform)' == '{configuration}|{platform}': OutputPath is '{outputPath}'.");

                // 3. If still not found, look for a global OutputPath (without conditions)
                if (string.IsNullOrEmpty(outputPath))
                {
                    outputPath = root.Elements(ns + "PropertyGroup")
                        .Where(g => g.Attribute("Condition") == null)
                        .Select(g => g.Element(ns + "OutputPath")?.Value)
                        .FirstOrDefault(val => !string.IsNullOrEmpty(val));
                    if (string.IsNullOrEmpty(outputPath))
                        MessageLogger.LogVerbose(this, "  PropertyGroup without Condition but with OutputPath not found");
                    else
                        MessageLogger.LogVerbose(this, $"  At least found PropertyGroup without Condition but with OutputPath: OutputPath is '{outputPath}'.");
                }

                // 4. Special logic for SDK-style projects
                if (isSdkStyle)
                {
                    // If an explicit OutputPath is not specified, construct it according to SDK rules
                    if (string.IsNullOrEmpty(outputPath))
                    {
                        // Look for BaseOutputPath (defaults to "bin\")
                        string baseOutputPath = root.Elements(ns + "PropertyGroup")
                            .Select(g => g.Element(ns + "BaseOutputPath")?.Value)
                            .FirstOrDefault(val => !string.IsNullOrEmpty(val));
                        MessageLogger.LogVerbose(this, "  SDK-style project detected; BaseOutputPath is " + (baseOutputPath ?? "null") +
                            (baseOutputPath == null ? ", so standard name 'bin' is assumed" : ""));

                        outputPath = Path.Combine(baseOutputPath ?? "bin", configuration);
                    }

                    // In SDK-style projects, TargetFramework (net8.0-windows, etc.) is appended by default
                    string appendTfm = root.Elements(ns + "PropertyGroup")
                        .Select(g => g.Element(ns + "AppendTargetFrameworkToOutputPath")?.Value)
                        .FirstOrDefault(val => !string.IsNullOrEmpty(val));

                    // If not specified as "false", then append the TFM to the path
                    if (appendTfm == null || appendTfm.Equals("true", StringComparison.OrdinalIgnoreCase))
                    {
                        string tfm = GetTargetFrameworkFromCsproj();
                        if (!string.IsNullOrEmpty(tfm))
                        {
                            outputPath = Path.Combine(outputPath, tfm);
                            MessageLogger.LogVerbose(this, $"  Appended TFM ('{tfm}') to OutputPath");
                        }
                        else
                            MessageLogger.LogVerbose(this, "  No TFM detected");
                    }
                }

                // 5. Final Fallback
                if (string.IsNullOrEmpty(outputPath))
                {
                    // If the platform is AnyCPU — it is not customary to add it to the path.
                    // If the platform is specific (x86, x64, ARM), it USUALLY comes BEFORE the configuration in legacy projects.
                    outputPath = string.IsNullOrEmpty(platform) || platform.Equals("AnyCPU", StringComparison.OrdinalIgnoreCase)
                        ? Path.Combine("bin", configuration)
                        : Path.Combine("bin", platform, configuration); // for x86 this will give bin\x86\Debug, which matches the MSBuild standard
                    MessageLogger.LogVerbose(this, $"  Fallback to '{outputPath}'");
                }

                outputPath = outputPath.TrimEnd('\\', '/');
                // Convert the relative path from the .csproj file to an absolute one
                if (!Path.IsPathRooted(outputPath))
                    outputPath = Path.GetFullPath(Path.Combine(projectDir, outputPath));

                MessageLogger.Log(this, $"Phew! Bin directory is determined as '{outputPath}'"); // we've made it!!! ))
                return outputPath;
            }
            catch (Exception ex)
            {
                MessageLogger.LogError(this, $"Error parsing project file '{csprojPath}'. Falling back to default paths. Error: {ex.Message}", ex);
                return Path.Combine(projectDir, "bin", configuration);
            }
        }

        /// <summary>
        /// Reads TargetFramework (or the first from TargetFrameworks) from the .csproj file
        /// to help determine the bin directory path for SDK-style projects.
        /// </summary>
        private string GetTargetFrameworkFromCsproj()
        {
            try
            {
                // For SDK-style projects: <TargetFramework> or <TargetFrameworks>
                var targetFrameworkElement = doc.Descendants(ns + "TargetFramework").FirstOrDefault();
                if (targetFrameworkElement != null)
                    return targetFrameworkElement.Value.Trim();

                var targetFrameworksElement = doc.Descendants(ns + "TargetFrameworks").FirstOrDefault();
                if (targetFrameworksElement != null)
                {
                    // Take the first TargetFramework from the list
                    return targetFrameworksElement.Value.Split(';').FirstOrDefault()?.Trim();
                }

                // For legacy .NET Framework projects: <TargetFrameworkVersion>
                var targetFrameworkVersionElement = doc.Descendants(ns + "TargetFrameworkVersion").FirstOrDefault();
                if (targetFrameworkVersionElement != null)
                {
                    // Example: v4.7.2. Sometimes "v" needs to be removed for paths.
                    string version = targetFrameworkVersionElement.Value.Trim().Replace("v", "");
                    // Map to common short names like "net472"
                    var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "4.0", "net40" }, { "4.5", "net45" }, { "4.5.1", "net451" }, { "4.5.2", "net452" },
                        { "4.6", "net46" }, { "4.6.1", "net461" }, { "4.6.2", "net462" },
                        { "4.7", "net47" }, { "4.7.1", "net471" }, { "4.7.2", "net472" }, { "4.8", "net48" }
                    };
                    if (map.TryGetValue(version, out var tf)) return tf;
                    return "net" + version.Replace(".", ""); // fallback, e.g. "4.7.2" -> "net472"
                }
            }
            catch (Exception ex)
            {
                MessageLogger.LogError(this, $"Error reading TargetFramework from project file '{csprojPath}': {ex.Message}", ex);
            }

            return null;
        }

        public bool IsFileInProject(string csFilePath)
        {
            try
            {
                // 1. Check for SDK-style project (modern .NET 5+, .NET Core projects); in such
                //    projects, files are included automatically if they are in subfolders:
                if (isSdkStyle)
                {
                    // If the file is inside the project folder or deeper — it is part of the project.
                    return csFilePath.StartsWith(projectDir + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
                }

                // 2. Check for Legacy-style project (classic .NET Framework)
                string relativePath = GetRelativePath(projectDir, csFilePath);
                string searchPath = relativePath.Replace('/', '\\');
                return root.Descendants()
                    .Where(e => e.Name.LocalName == "Compile")
                    .Any(e => string.Equals((string)e.Attribute("Include"), searchPath, StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                return false;
            }
        }

        public Font TryDetectApplicationDefaultFont()
        {
            try
            {
                if (!isSdkStyle)
                    return null;

                var fontElement = root.Descendants().FirstOrDefault(e => e.Name.LocalName == "ApplicationDefaultFont");
                if (fontElement == null || string.IsNullOrWhiteSpace(fontElement.Value))
                    return null;

                string rawValue = fontElement.Value.Trim(); // e.g. "Microsoft Sans Serif, 8.25pt"

                var converter = TypeDescriptor.GetConverter(typeof(Font)); // TypeDescriptor.GetConverter will find the FontConverter automatically
                return (Font)converter.ConvertFromInvariantString(rawValue); // ConvertFromInvariantString - so as not to depend on the separator
            }
            catch
            {
                // Parsing errors — assume the font is not defined
            }

            return null;
        }

        public string GetProjectAssemblyName()
        {
            try
            {
                var assemblyNameElement = root.Descendants(XName.Get("AssemblyName", ns.NamespaceName)).LastOrDefault();
                if (assemblyNameElement != null && !string.IsNullOrWhiteSpace(assemblyNameElement.Value))
                    return assemblyNameElement.Value.Trim();
            }
            catch
            {
                // (fallback to default value)
            }

            return Path.GetFileNameWithoutExtension(csprojPath); // default value
        }

        /// <summary>
        /// Calculates the relative path (similar to Path.GetRelativePath from .NET 5+)
        /// </summary>
        private static string GetRelativePath(string fromPath, string toPath)
        {
            Uri fromUri = new Uri(AppendSlash(fromPath));
            Uri toUri = new Uri(toPath);

            if (fromUri.Scheme != toUri.Scheme)
                return toPath; // different drives

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            return relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }

        private static string AppendSlash(string path)
        {
            if (path == null)
                return null;
            return !path.EndsWith(Path.DirectorySeparatorChar.ToString())
                ? path + Path.DirectorySeparatorChar
                : path;
        }
    }
}
