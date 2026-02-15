using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UiTools.WinForms.Designer.Core;
using UiTools.WinForms.Designer.Properties;

namespace UiTools.WinForms.Designer
{
    public partial class OptionsForm : Form, IOptions
    {
        private KeyCodeValidator IntegerInputValidator = KeyCodeValidator.IntegerInputValidator;

        public OptionsForm()
        {
            InitializeComponent();
            Icon = Icon.FromHandle(Resources.Options.GetHicon());

            PopulateAlignControlsModeCombo();
            PopulateFontNameCombo(cboDefaultRootComponentFontName);
            PopulateFontNameCombo(cboCodeViewerFontName);
            PopulateFontNameSize(cboDefaultRootComponentFontSize);
            PopulateFontNameSize(cboCodeViewerFontSize);
            PopulateMinLogLevel();

            cboAlignControlsMode.SelectedIndexChanged += cboAlignControlsMode_SelectedIndexChanged;
            txtGridSize.KeyDown += txtGridSize_KeyDown;
            txtMRUListMaxSize.KeyDown += txtMRUListMaxSize_KeyDown;
        }

        private void txtMRUListMaxSize_KeyDown(object sender, KeyEventArgs e)
        {
            if (!IntegerInputValidator.Validate(e.KeyCode))
            {
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        }

        private void txtGridSize_KeyDown(object sender, KeyEventArgs e)
        {
            if (!IntegerInputValidator.Validate(e.KeyCode))
            {
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        }

        private void cboAlignControlsMode_SelectedIndexChanged(object sender, System.EventArgs e)
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
    }
}
