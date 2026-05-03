using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace UiTools.WinForms.Designer.Core
{
    internal class ThemedContextMenuStrip : ContextMenuStrip, IThemedMenuStrip
    {
        private static readonly Color DefaultTextColor = SystemColors.ControlText;
        private static readonly Color DefaultHoverColor = ColorTranslator.FromHtml("#B5D7F3");
        private static readonly Color DefaultHoverTextColor = SystemColors.ControlText;
        private static readonly Color DefaultSelectedColor = ColorTranslator.FromHtml("#FDFDFD");
        private static readonly Color DefaultItemAccentColor = ColorTranslator.FromHtml("#0178D7");
        private static readonly Color DefaultDropDownBackColor = ColorTranslator.FromHtml("#FDFDFD");
        private static readonly Color DefaultDropDownBorderColor = ColorTranslator.FromHtml("#808080");
        private static readonly Color DefaultImageMarginColor = ColorTranslator.FromHtml("#F1F1F1");
        private static readonly Color DefaultSeparatorColor = ColorTranslator.FromHtml("#BDBDBD");
        private static readonly Color DefaultCheckedColor = ColorTranslator.FromHtml("#B5D7F3");
        private static readonly Color DefaultCheckedAndSelectedColor = ColorTranslator.FromHtml("#80BCEB");

        private bool isDarkTheme = false;

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (CommonStuff.CurrentUiTheme != null)
                BeginInvoke(ApplyMenuStripTheme);
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
                    ApplyMenuStripTheme();
                }
            }
        }

        private void ApplyMenuStripTheme()
        {
            Renderer = new ThemedMenuStripRenderer(this);
            Refresh();
        }

        [Category("Appearance")]
        public Color TextColor { get; set; } = DefaultTextColor;
        [Category("Appearance")]
        public Color HoverColor { get; set; } = DefaultHoverColor;
        [Category("Appearance")]
        public Color HoverTextColor { get; set; } = DefaultHoverTextColor;
        [Category("Appearance")]
        public Color SelectedColor { get; set; } = DefaultSelectedColor;
        [Category("Appearance")]
        public Color ItemAccentColor { get; set; } = DefaultItemAccentColor;
        [Category("Appearance")]
        public Color DropDownBackColor { get; set; } = DefaultDropDownBackColor;
        [Category("Appearance")]
        public Color DropDownBorderColor { get; set; } = DefaultDropDownBorderColor;
        [Category("Appearance")]
        public Color ImageMarginColor { get; set; } = DefaultImageMarginColor;
        [Category("Appearance")]
        public Color SeparatorColor { get; set; } = DefaultSeparatorColor;
        [Category("Appearance")]
        public Color CheckedColor { get; set; } = DefaultCheckedColor;
        [Category("Appearance")]
        public Color CheckedAndSelectedColor { get; set; } = DefaultCheckedAndSelectedColor;

        #region Support for default values of Color properties

        private bool ShouldSerializeTextColor() => TextColor != DefaultTextColor;
        private void ResetTextColor() => TextColor = DefaultTextColor;

        private bool ShouldSerializeHoverColor() => HoverColor != DefaultHoverColor;
        private void ResetHoverColor() => HoverColor = DefaultHoverColor;

        private bool ShouldSerializeHoverTextColor() => HoverTextColor != DefaultHoverTextColor;
        private void ResetHoverTextColor() => HoverTextColor = DefaultHoverTextColor;

        private bool ShouldSerializeSelectedColor() => SelectedColor != DefaultSelectedColor;
        private void ResetSelectedColor() => SelectedColor = DefaultSelectedColor;

        private bool ShouldSerializeItemAccentColor() => ItemAccentColor != DefaultItemAccentColor;
        private void ResetItemAccentColor() => ItemAccentColor = DefaultItemAccentColor;

        private bool ShouldSerializeDropDownBackColor() => DropDownBackColor != DefaultDropDownBackColor;
        private void ResetDropDownBackColor() => DropDownBackColor = DefaultDropDownBackColor;

        private bool ShouldSerializeDropDownBorderColor() => DropDownBorderColor != DefaultDropDownBorderColor;
        private void ResetDropDownBorderColor() => DropDownBorderColor = DefaultDropDownBorderColor;

        private bool ShouldSerializeImageMarginColor() => ImageMarginColor != DefaultImageMarginColor;
        private void ResetImageMarginColor() => ImageMarginColor = DefaultImageMarginColor;

        private bool ShouldSerializeSeparatorColor() => SeparatorColor != DefaultSeparatorColor;
        private void ResetSeparatorColor() => SeparatorColor = DefaultSeparatorColor;

        private bool ShouldSerializeCheckedColor() => CheckedColor != DefaultCheckedColor;
        private void ResetCheckedColor() => CheckedColor = DefaultCheckedColor;

        private bool ShouldSerializeCheckedAndSelectedColor() => CheckedAndSelectedColor != DefaultCheckedAndSelectedColor;
        private void ResetCheckedAndSelectedColor() => CheckedAndSelectedColor = DefaultCheckedAndSelectedColor;

        #endregion Support for default values of Color properties
    }
}
