using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UiTools.WinForms.Designer.Core;
using UiTools.WinForms.Designer.Properties;

namespace UiTools.WinForms.Designer
{
    public partial class OptionsForm : ThemedForm, IOptions
    {
        private readonly List<string> knownUiThemesNames;
        private readonly KeyCodeValidator integerInputValidator = KeyCodeValidator.IntegerInputValidator;

        public OptionsForm(List<string> knownUiThemesNames)
        {
            this.knownUiThemesNames = knownUiThemesNames;

            InitializeComponent();

            CenterToParent(); // center early to prevent visual flickering during the population of controls
            Icon = Resources.OptionsDialog;

            PopulateAlignControlsModeCombo();
            PopulateFontNameCombo(cboDefaultRootComponentFontName);
            PopulateFontNameCombo(cboCodeViewerFontName);
            PopulateFontNameSize(cboDefaultRootComponentFontSize);
            PopulateFontNameSize(cboCodeViewerFontSize);
            PopulateMinLogLevel();
            PopulateUiThemeCombo();

            cboAlignControlsMode.SelectedIndexChanged += cboAlignControlsMode_SelectedIndexChanged;
            txtGridSize.KeyDown += txtGridSize_KeyDown;
            txtMRUListMaxSize.KeyDown += txtMRUListMaxSize_KeyDown;
        }

        protected override void OnUiThemeApplied()
        {
            picUiTheme.Image = ThemeApplier.IsDark(BackColor) ? Resources.UiThemeConfigFolder_DarkTheme : Resources.UiThemeConfigFolder;
            AdjustLayout();
            Refresh();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            if (IsHandleCreated)
                BeginInvoke(AdjustLayout);
        }

        private void AdjustLayout()
        {
            picUiTheme.Left = cboUiTheme.Right + 6;
            picRemoveUnnecessaryUsingsHelp.Left = chkRemoveUnnecessaryUsings.Right + 1;
            labGridSize.Left = txtGridSize.Left - labGridSize.Width - 1;
        }

        private void txtMRUListMaxSize_KeyDown(object sender, KeyEventArgs e)
        {
            if (!integerInputValidator.Validate(e.KeyCode))
            {
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        }

        private void txtGridSize_KeyDown(object sender, KeyEventArgs e)
        {
            if (!integerInputValidator.Validate(e.KeyCode))
            {
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        }

        private void cboAlignControlsMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedMode = (AlignControlsModeEnum)cboAlignControlsMode.SelectedIndex;
            txtGridSize.Enabled = labGridSize.Enabled = selectedMode == AlignControlsModeEnum.UseGrid || selectedMode == AlignControlsModeEnum.SnapToGrid;
            //if (!txtGridSize.Enabled)
            //    txtGridSize.Clear();
        }

        #region IOptions members

        AlignControlsModeEnum IOptions.AlignControlsMode
        {
            get => (AlignControlsModeEnum)cboAlignControlsMode.SelectedIndex;
            set => cboAlignControlsMode.SelectedIndex = (int)value;
        }

        int IOptions.GridSize
        {
            get => int.Parse(txtGridSize.Text);
            set => txtGridSize.Text = value.ToString();
        }

        string IOptions.DefaultNewRootComponentFontName
        {
            get => cboDefaultRootComponentFontName.Text;
            set => cboDefaultRootComponentFontName.Text = value;
        }

        float IOptions.DefaultNewRootComponentFontSize
        {
            get => float.Parse(cboDefaultRootComponentFontSize.Text);
            set => cboDefaultRootComponentFontSize.Text = value.ToString();
        }

        string IOptions.SourceCodeViewerFontName
        {
            get => cboCodeViewerFontName.Text;
            set => cboCodeViewerFontName.Text = value;
        }

        float IOptions.SourceCodeViewerFontSize
        {
            get => float.Parse(cboCodeViewerFontSize.Text);
            set => cboCodeViewerFontSize.Text = value.ToString();
        }

        int IOptions.MainFormMRUListMaxSize
        {
            get => int.Parse(txtMRUListMaxSize.Text);
            set => txtMRUListMaxSize.Text = value.ToString();
        }

        MessageLogger.LogLevel IOptions.LogLevel
        {
            get => (MessageLogger.LogLevel)cboLogLevel.SelectedIndex;
            set => cboLogLevel.SelectedIndex = (int)value;
        }

        bool IOptions.RemoveUnnecessaryUsings
        {
            get => chkRemoveUnnecessaryUsings.Checked;
            set => chkRemoveUnnecessaryUsings.Checked = value;
        }

        string IOptions.UiThemeName
        {
            get => cboUiTheme.Text;
            set => cboUiTheme.Text = value;
        }

        #endregion IOptions members

        private void PopulateAlignControlsModeCombo()
        {
            var t = typeof(AlignControlsModeEnum);
            foreach (var enumValue in t.GetEnumValues())
            {
                var memberInfo = t.GetMember(enumValue.ToString()).First();
                var displayAttr = memberInfo.GetCustomAttribute<DisplayAttribute>();
                cboAlignControlsMode.Items.Add(displayAttr == null ? enumValue.ToString() : displayAttr.Name);
            }
        }

        private void PopulateFontNameCombo(ComboBox cb)
        {
            cb.Items.AddRange(new InstalledFontCollection().Families.Select(f => f.Name).ToArray());
        }

        private void PopulateFontNameSize(ComboBox cb)
        {
            cb.Items.Add(8);
            cb.Items.Add(9);
            cb.Items.Add(10);
            cb.Items.Add(11);
            cb.Items.Add(12);
            cb.Items.Add(14);
        }

        private void PopulateMinLogLevel()
        {
            cboLogLevel.Items.Add("Verbose");
            cboLogLevel.Items.Add("Info");
            cboLogLevel.Items.Add("Warning");
            cboLogLevel.Items.Add("Error");
        }

        private void PopulateUiThemeCombo()
        {
            cboUiTheme.Items.Add(KnownUiThemes.NONE);
            knownUiThemesNames.ForEach(t => cboUiTheme.Items.Add(t));
        }

        private void picUiTheme_Click(object sender, EventArgs e)
        {
            try
            {
                var uiThemesFilePath = KnownUiThemes.GetUiThemesFilePath();
                if (!File.Exists(uiThemesFilePath))
                {
                    MessageBox.Show(this, $"File not found:\n{uiThemesFilePath}", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                Process.Start("explorer.exe", $"/select,\"{uiThemesFilePath}\"");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open folder: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
