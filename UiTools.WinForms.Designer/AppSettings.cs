using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using UiTools.WinForms.Designer.Core;

namespace UiTools.WinForms.Designer
{
    public class AppSettings : IOptions
    {
        public static readonly string ConfigFilePath;

        public static AppSettings Instance { get; private set; }

        static AppSettings()
        {
            var configFolderPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                Application.ProductName);
            try
            {
                if (!Directory.Exists(configFolderPath))
                    Directory.CreateDirectory(configFolderPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to create directory \"{configFolderPath}\":\n{ex.Message}\n\nApplication will terminate.",
                    Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Application.Exit();
            }
            ConfigFilePath = Path.Combine(
                configFolderPath,
                Application.ProductName + ".config.xml");
            LoadSettings();
        }

        private static void LoadSettings()
        {
            if (ConfigFileExists())
            {
                string xml;
                try
                {
                    xml = File.ReadAllText(ConfigFilePath, CommonStuff.Utf8WithoutBom);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to read config file \"{ConfigFilePath}\":\n{ex.Message}\n\nDefault configuration parameters were applied.",
                        Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    Reset();
                    return;
                }
                try
                {
                    Instance = XmlHelper.Deserialize<AppSettings>(xml);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to deserialize config file \"{ConfigFilePath}\":\n{ex.Message}\n\nDefault configuration parameters were applied.",
                        Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    Reset();
                }
            }
            else
            {
                Reset();
            }
        }

        public static void Reload()
        {
            LoadSettings();
        }

        public static void Reset()
        {
            Instance = new AppSettings();
            Instance.Save();
        }

        public void Save()
        {
            File.WriteAllText(ConfigFilePath, XmlHelper.Serialize(this, CommonStuff.Utf8WithoutBom), CommonStuff.Utf8WithoutBom);
        }

        private static bool ConfigFileExists()
        {
            return File.Exists(ConfigFilePath) && new FileInfo(ConfigFilePath).Length > 0;
        }

        public FormSettings MainFormSettings { get; set; } = new FormSettings();
        public WorkspaceSettings WorkspaceSettings { get; set; } = new WorkspaceSettings();
        public SerializableDictionary<string, DesignerCsFileContext> KnownDesignerCsFileContexts { get; set; } = new SerializableDictionary<string, DesignerCsFileContext>();

        #region IOptions members

        [DefaultValue(AlignControlsModeEnum.UseSnapLines)]
        public AlignControlsModeEnum AlignControlsMode { get; set; } = AlignControlsModeEnum.UseSnapLines;
        [DefaultValue(32)]
        public int GridSize { get; set; } = 32;

        [DefaultValue("Segoe UI")]
        public string DefaultNewRootComponentFontName { get; set; } = "Segoe UI";
        [DefaultValue(9F)]
        public float DefaultNewRootComponentFontSize { get; set; } = 9F;

        [DefaultValue("Consolas")]
        public string SourceCodeViewerFontName { get; set; } = "Consolas";
        [DefaultValue(10F)]
        public float SourceCodeViewerFontSize { get; set; } = 10F;

        [DefaultValue(16)]
        public int MainFormMRUListMaxSize { get; set; } = 16;
        public List<string> MainFormMRUList { get; set; } = new List<string>();

        [DefaultValue(MessageLogger.LogLevel.Verbose)]
        public MessageLogger.LogLevel LogLevel { get; set; }

        [DefaultValue(true)]
        public bool RemoveUnnecessaryUsings { get; set; } = true;

        [DefaultValue(KnownUiThemes.NONE)]
        public string UiThemeName { get; set; } = KnownUiThemes.NONE;

        #endregion IOptions members
    }

    public class FormSettings
    {
        [DefaultValue(FormWindowState.Normal)]
        public FormWindowState WindowState { get; set; } = FormWindowState.Normal;
        [DefaultValue(0)]
        public int Left { get; set; } = 0;
        [DefaultValue(0)]
        public int Top { get; set; } = 0;
        [DefaultValue(1000)]
        public int Width { get; set; } = 1000;
        [DefaultValue(600)]
        public int Height { get; set; } = 600;
    }

    public interface IOptions // options editable via OptionsForm
    {
        AlignControlsModeEnum AlignControlsMode { get; set; }
        int GridSize { get; set; }
        string DefaultNewRootComponentFontName { get; set; }
        float DefaultNewRootComponentFontSize { get; set; }
        string SourceCodeViewerFontName { get; set; }
        float SourceCodeViewerFontSize { get; set; }
        int MainFormMRUListMaxSize { get; set; }
        MessageLogger.LogLevel LogLevel { get; set; }
        bool RemoveUnnecessaryUsings { get; set; }
        string UiThemeName { get; set; }
    }
}
