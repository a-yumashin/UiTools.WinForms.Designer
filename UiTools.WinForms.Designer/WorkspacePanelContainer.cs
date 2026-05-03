using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using UiTools.WinForms.Designer.Properties;

namespace UiTools.WinForms.Designer
{
    public partial class WorkspacePanelContainer : UserControl
    {
        private static readonly Color DefaultCaptionBackColor = Color.LightBlue;
        private static readonly Color DefaultCaptionForeColor = SystemColors.ControlText;
        private static readonly Color DefaultCloseButtonHoverBackColor = Color.SkyBlue;

        private bool isDarkTheme = false;

        public event EventHandler Closed;

        public string Title { get => labHeader.Text; set => labHeader.Text = value; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Control WorkspacePanel
        {
            get => pnlContainer.Controls.Count == 1 ? null : pnlContainer.Controls["WorkspacePanel"];
            set
            {
                if (pnlContainer.Controls.Count > 1)
                    pnlContainer.Controls.RemoveByKey("WorkspacePanel");
                value.Name = "WorkspacePanel";
                pnlContainer.Controls.Add(value);
                value.BringToFront();
                pnlContainer.Refresh();
            }
        }

        public WorkspacePanelContainer()
        {
            InitializeComponent();
            picClose.Click += (s, e) => Closed?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ApplyUiThemeToCaptionArea();
        }

        [Category("Appearance")]
        [DefaultValue(false)]
        public bool IsDarkTheme
        {
            get => isDarkTheme;
            set
            {
                if (isDarkTheme != value)
                {
                    isDarkTheme = value;
                    ApplyUiThemeToCaptionArea();
                }
            }
        }

        private void ApplyUiThemeToCaptionArea()
        {
            // (except last assignment, all others can be done in setters instead; doesn't matter)
            pnlHeader.BackColor = CaptionBackColor;
            labHeader.BackColor = CaptionBackColor;
            picClose.BackColor = CaptionBackColor;
            labHeader.ForeColor = CaptionForeColor;
            picClose.HoverBackColor = CloseButtonHoverBackColor;
            picClose.Image = IsDarkTheme ? Resources.Close_DarkTheme : Resources.Close;
        }

        [Category("Appearance")]
        public Color CaptionBackColor { get; set; } = DefaultCaptionBackColor;

        [Category("Appearance")]
        public Color CaptionForeColor { get; set; } = DefaultCaptionForeColor;
        
        [Category("Appearance")]
        public Color CloseButtonHoverBackColor { get; set; } = DefaultCloseButtonHoverBackColor;

        #region Support for default values of Color properties

        private bool ShouldSerializeCaptionBackColor() => CaptionBackColor != DefaultCaptionBackColor;
        private void ResetCaptionBackColor() => CaptionBackColor = DefaultCaptionBackColor;

        private bool ShouldSerializeCaptionForeColor() => CaptionForeColor != DefaultCaptionForeColor;
        private void ResetCaptionForeColor() => CaptionForeColor = DefaultCaptionForeColor;

        private bool ShouldSerializeCloseButtonHoverBackColor() => CloseButtonHoverBackColor != DefaultCloseButtonHoverBackColor;
        private void ResetCloseButtonHoverBackColor() => CloseButtonHoverBackColor = DefaultCloseButtonHoverBackColor;

        #endregion Support for default values of Color properties
    }
}
