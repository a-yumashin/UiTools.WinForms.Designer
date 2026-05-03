using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using UiTools.WinForms.Designer.Core;
using UiTools.WinForms.Designer.Properties;

namespace UiTools.WinForms.Designer
{
    public partial class DesignerCsFileContextForm : ThemedForm
    {
        private static readonly Color DefaultNotesTextColor = SystemColors.ControlDarkDark;
        
        public enum InitiatorEnum
        {
            VSCode,       // always "open existing" (not "create new") --> fpeDesignerFile is visible but disabled (and is filled); txtNamespace is not visible (namespace will be extracted from code)
            FileOpenMenu, // "open existing" --> fpeDesignerFile is visible and enabled; txtNamespace is not visible (namespace will be extracted from code)
            FileNewMenu   // "create new" --> fpeDesignerFile is not visible; txtNamespace is visible
        }
        private InitiatorEnum initiator;
        private bool isProgrammaticUpdate;
        private const string VALUE_WAS_RESTORED_HINT = "restored value which was last used with the selected designer file";
        private const string SUGGESTED_CSPROJ_FILE_HINT = "suggested project file (found while scanning file system)";

        public InitiatorEnum Initiator
        {
            get => initiator;
            set
            {
                if (value == InitiatorEnum.VSCode)
                {
                    labDesignerFile.Visible = true;
                    fpeDesignerFile.Visible = true;
                    fpeDesignerFile.Enabled = false;
                    labNamespace.Visible = false;
                    txtNamespace.Visible = false;
                    labNewFormNotice.Visible = false;
                }
                else if (value == InitiatorEnum.FileOpenMenu)
                {
                    labDesignerFile.Visible = true;
                    fpeDesignerFile.Visible = true;
                    fpeDesignerFile.Enabled = true;
                    labNamespace.Visible = false;
                    txtNamespace.Visible = false;
                    labNewFormNotice.Visible = false;
                }
                else // InitiatorEnum.FileNewMenu
                {
                    labDesignerFile.Visible = false;
                    fpeDesignerFile.Visible = false;
                    fpeDesignerFile.Enabled = false;
                    labNamespace.Visible = true;
                    txtNamespace.Visible = true;
                    labNewFormNotice.Visible = true;
                    Height += labNewFormNotice.Height + 5;
                    MinimumSize = new Size(888, Height);
                    labNewFormNotice.Text = labNewFormNotice.Text.Replace("<root_component_type>", RootComponentTypeName.ToLower());
                }
                initiator = value;
            }
        }

        public string RootComponentTypeName { get; set; } // used only when Initiator = InitiatorEnum.FileNewMenu

        public void SetDesignerCsFileFullPath(string designerCsFileFullPath)
        {
            fpeDesignerFile.Text = designerCsFileFullPath;
            CheckForCompanionFile();
        }

        private void CheckForCompanionFile()
        {
            var mainFilePath = CommonStuff.MainCsFilePathFromDesignerCsFilePath(fpeDesignerFile.Text);
            bool mainFileExists = File.Exists(mainFilePath);
            picDesignerFileHint.Visible = !mainFileExists;
            if (!mainFileExists)
                ShowHint(picDesignerFileHint, $"Companion file '{Path.GetFileName(mainFilePath)}' not found in the same folder.\nAutomatic event handler creation is disabled.");
            else
                HideHint(picDesignerFileHint);
        }

        public DesignerCsFileContextForm()
        {
            InitializeComponent();

            label1.ForeColor = DefaultNotesTextColor;
            labNewFormNotice.ForeColor = DefaultNotesTextColor;
            label1.Tag = "NoTheme";
            labNewFormNotice.Tag = "NoTheme";

            CenterToParent(); // center early to prevent visual flickering during the population of controls
            
            fpeDesignerFile.Title = $"Select designer file of your Form/UserControl";
            fpeDesignerFile.DialogType = FilePathEdit.FileDialogType.Open;
            fpeDesignerFile.Filter = "C# Designer Files|*.Designer.cs;*.designer.cs|All files|*.*";
            fpeDesignerFile.FilterIndex = 1;

            fpeProjectFile.Title = $"Select .csproj file of the C# Project containing your Form/UserControl";
            fpeProjectFile.DialogType = FilePathEdit.FileDialogType.Open;
            fpeProjectFile.Filter = "C# Project Files|*.csproj|All files|*.*";
            fpeProjectFile.FilterIndex = 1;

            fpeExtraAssemblies.Title = "Select optional text file with extra assemblies to load";
            fpeExtraAssemblies.DialogType = FilePathEdit.FileDialogType.Open;
            fpeExtraAssemblies.Filter = "Text Files|*.txt|All files|*.*";
            fpeExtraAssemblies.FilterIndex = 1;

            cboConfiguration.Items.Add("Debug");
            cboConfiguration.Items.Add("Release");
            cboPlatform.Items.Add("Any CPU");
            cboPlatform.Items.Add("x64");
            cboPlatform.Items.Add("x86");

            fpeDesignerFile.TextChanged += fpeDesignerFile_TextChanged;
            fpeProjectFile.TextChanged += fpeProjectFile_TextChanged;
            fpeExtraAssemblies.TextChanged += (s, e) => { if (!isProgrammaticUpdate) HideHint(picExtraAssembliesHint); };
            cboConfiguration.TextChanged += (s, e) => { if (!isProgrammaticUpdate) HideHint(picConfigurationHint); };
            cboPlatform.TextChanged += (s, e) => { if (!isProgrammaticUpdate) HideHint(picPlatformHint); };

            cmdOK.Click += cmdOK_Click;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (txtNamespace.Visible)
                txtNamespace.Focus();
        }

        private void fpeProjectFile_TextChanged(object sender, EventArgs e)
        {
            if (!isProgrammaticUpdate)
                HideHint(picProjectFileHint);
            SetInitialDirectoryForProjectFile();
        }

        private void fpeDesignerFile_TextChanged(object sender, EventArgs e)
        {
            isProgrammaticUpdate = true;
            if (string.IsNullOrEmpty(fpeDesignerFile.Text) || !File.Exists(fpeDesignerFile.Text))
            {
                fpeProjectFile.Text = string.Empty;
                HideHint(picProjectFileHint);
                HideHint(picConfigurationHint);
                HideHint(picPlatformHint);
                HideHint(picExtraAssembliesHint);
            }
            else
            {
                var key = fpeDesignerFile.Text.ToLower();
                if (AppSettings.Instance.KnownDesignerCsFileContexts.ContainsKey(key))
                {
                    var dfContext = AppSettings.Instance.KnownDesignerCsFileContexts[key];
                    fpeProjectFile.Text = dfContext.CsProjectFileFullPath;
                    if (string.IsNullOrWhiteSpace(fpeProjectFile.Text))
                    {
                        fpeProjectFile.Text = CsProjectFileLocator.FindContainingCsProjFile(fpeDesignerFile.Text);
                        ShowHint(picProjectFileHint, SUGGESTED_CSPROJ_FILE_HINT);
                    }
                    else
                        ShowHint(picProjectFileHint, VALUE_WAS_RESTORED_HINT);
                    cboConfiguration.Text = dfContext.Configuration;
                    cboPlatform.Text = dfContext.Platform;
                    fpeExtraAssemblies.Text = dfContext.ExtraAssembliesFileFullPath;
                    // Show hints:
                    ShowHint(picConfigurationHint, VALUE_WAS_RESTORED_HINT);
                    ShowHint(picPlatformHint, VALUE_WAS_RESTORED_HINT);
                    ShowHint(picExtraAssembliesHint, VALUE_WAS_RESTORED_HINT);
                }
                else
                {
                    fpeProjectFile.Text = CsProjectFileLocator.FindContainingCsProjFile(fpeDesignerFile.Text);
                    // Show/hide hints:
                    ShowHint(picProjectFileHint, SUGGESTED_CSPROJ_FILE_HINT);
                    HideHint(picConfigurationHint);
                    HideHint(picPlatformHint);
                    HideHint(picExtraAssembliesHint);
                }
                CheckForCompanionFile();
            }
            isProgrammaticUpdate = false;
            SetInitialDirectoryForProjectFile();
        }

        private void ShowHint(PictureBox pb, string caption)
        {
            toolTip1.SetToolTip(pb, caption);
            pb.Visible = true;
        }
        private void HideHint(PictureBox pb)
        {
            toolTip1.SetToolTip(pb, "");
            pb.Visible = false;
        }

        private void SetInitialDirectoryForProjectFile()
        {
            if (string.IsNullOrWhiteSpace(fpeProjectFile.Text))
            {
                if (!string.IsNullOrWhiteSpace(fpeDesignerFile.Text))
                    fpeProjectFile.InitialDirectory = Path.GetDirectoryName(fpeDesignerFile.Text);
            }
            else
                fpeProjectFile.InitialDirectory = Path.GetDirectoryName(fpeProjectFile.Text);
        }

        public DesignerCsFileContext DesignerCsFileContext
        {
            get
            {
                return new DesignerCsFileContext
                {
                    DesignerCsFileFullPath = fpeDesignerFile.Text,
                    Namespace = txtNamespace.Text,
                    CsProjectFileFullPath = fpeProjectFile.Text,
                    Configuration = cboConfiguration.Text,
                    Platform = cboPlatform.Text,
                    ExtraAssembliesFileFullPath = fpeExtraAssemblies.Text
                };
            }
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            var errors = CheckInput();
            if (string.IsNullOrEmpty(errors))
            {
                DialogResult = DialogResult.OK;
                if (!string.IsNullOrEmpty(fpeDesignerFile.Text))
                    SaveContext();
            }
            else
            {
                DialogResult = DialogResult.None;
                MessageBox.Show(this, $"Error(s):\n{errors}", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string CheckInput()
        {
            var errors = new StringBuilder();
            // At the same time either txtNamespace or fpeDesignerFile is visible:
            if (txtNamespace.Visible)
            {
                if (string.IsNullOrEmpty(txtNamespace.Text))
                    errors.AppendLine("- 'Namespace' is not specified");
            }
            else
            {
                if (string.IsNullOrEmpty(fpeDesignerFile.Text))
                    errors.AppendLine("- 'Designer file (full path)' is not specified");
                else if (!File.Exists(fpeDesignerFile.Text))
                    errors.AppendLine($"- Specified designer file not found: {fpeDesignerFile.Text}");
            }
            // All other parameters are visible always and they are optional:
            if (!string.IsNullOrEmpty(fpeProjectFile.Text) && !File.Exists(fpeProjectFile.Text))
                errors.AppendLine($"- Specified project file not found: {fpeProjectFile.Text}");
            if (!string.IsNullOrEmpty(fpeExtraAssemblies.Text) && !File.Exists(fpeExtraAssemblies.Text))
                errors.AppendLine($"- Specified extra assemblies file not found: {fpeExtraAssemblies.Text}");
            return errors.ToString();
        }

        private void SaveContext()
        {
            var key = fpeDesignerFile.Text.ToLower();
            if (!AppSettings.Instance.KnownDesignerCsFileContexts.ContainsKey(key))
                AppSettings.Instance.KnownDesignerCsFileContexts.Add(key, new DesignerCsFileContext());
            DesignerCsFileContext dfContext = AppSettings.Instance.KnownDesignerCsFileContexts[key];
            dfContext.DesignerCsFileFullPath = fpeDesignerFile.Text;
            dfContext.Namespace = txtNamespace.Text;
            dfContext.CsProjectFileFullPath = fpeProjectFile.Text;
            dfContext.Configuration = cboConfiguration.Text;
            dfContext.Platform = cboPlatform.Text;
            dfContext.ExtraAssembliesFileFullPath = fpeExtraAssemblies.Text;
            AppSettings.Instance.Save();
        }

        [Category("Appearance")]
        public Color NotesTextColor { get; set; }

        protected override void OnUiThemeApplied()
        {
            label1.ForeColor = NotesTextColor;
            labNewFormNotice.ForeColor = NotesTextColor;
            label1.Font = new Font("Segoe UI", 9);
            labNewFormNotice.Font = label1.Font;
            picExtraAssembliesHelp.Image = IsDarkTheme ? Resources.Help_DarkTheme : Resources.Help;
        }

        [Category("Appearance")]
        [DefaultValue(false)]
        public bool IsDarkTheme { get; set; } = false;

        #region Support for default values of Color properties

        private bool ShouldSerializeNotesTextColor() => NotesTextColor != DefaultNotesTextColor;
        private void ResetNotesTextColor() => NotesTextColor = DefaultNotesTextColor;

        #endregion Support for default values of Color properties
    }
}
