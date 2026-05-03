using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace UiTools.WinForms.Designer.Core
{
    public class ThemedForm : Form
    {
        public event EventHandler UiThemeApplied;

        private static readonly Color DefaultTitleBarBackColor = SystemColors.Window;
        private static readonly Color DefaultTitleBarForeColor = SystemColors.WindowText;

        public ThemedForm() : base()
        {
            PreventFlickeringWhenDarkThemeIsApplied();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (CommonStuff.CurrentUiTheme != null)
            {
                ThemeApplier.Apply(this, CommonStuff.CurrentUiTheme);
                OnUiThemeApplied();
            }
        }

        protected virtual void OnUiThemeApplied()
        {
            UiThemeApplied?.Invoke(this, EventArgs.Empty);
        }

        private void PreventFlickeringWhenDarkThemeIsApplied()
        {
            if (CommonStuff.CurrentUiTheme != null && CommonStuff.CurrentUiTheme.IsProbablyDark(out Color backColor))
            {
                BackColor = backColor;
            }
        }

        [Category("Appearance")]
        public Color TitleBarBackColor { get; set; } = DefaultTitleBarBackColor;
        
        [Category("Appearance")]
        public Color TitleBarForeColor { get; set; } = DefaultTitleBarForeColor;

        #region Support for default values of Color properties

        private bool ShouldSerializeTitleBarBackColor() => TitleBarBackColor != DefaultTitleBarBackColor;
        private void ResetTitleBarBackColor() => TitleBarBackColor = DefaultTitleBarBackColor;

        private bool ShouldSerializeTitleBarForeColor() => TitleBarForeColor != DefaultTitleBarForeColor;
        private void ResetTitleBarForeColor() => TitleBarForeColor = DefaultTitleBarForeColor;

        #endregion Support for default values of Color properties
    }
}
