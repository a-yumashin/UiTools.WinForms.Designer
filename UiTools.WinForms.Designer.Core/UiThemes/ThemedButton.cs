using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace UiTools.WinForms.Designer.Core
{
    public class ThemedButton : Button
    {
        private static readonly Color DefaultDisabledBackColor = ColorTranslator.FromHtml("#333333");
        private static readonly Color DefaultDisabledForeColor = ColorTranslator.FromHtml("#5C5C5C");

        private bool isDarkTheme = false;

        [Category("Appearance")]
        public Color DisabledBackColor { get; set; } = DefaultDisabledBackColor;

        [Category("Appearance")]
        public Color DisabledForeColor { get; set; } = DefaultDisabledForeColor;

        [Category("Appearance")]
        [DefaultValue(false)]
        public bool IsDarkTheme
        {
            get => isDarkTheme;
            set
            {
                // NOTE: no "if (isDarkTheme != value)" check here!!! we need to assign UseVisualStyleBackColor = true for
                //       light themes always, otherwise BackColor will be drawn as a solid flat color instead of using 
                //       system visual styles. This is because assigning a value to the BackColor property (which the 
                //       ThemeApplier does) implicitly resets UseVisualStyleBackColor to false, and we must force it 
                //       back to true to restore the standard themed appearance.
                isDarkTheme = value;
                FlatStyle = IsDarkTheme ? FlatStyle.Flat : FlatStyle.Standard;
                UseVisualStyleBackColor = !IsDarkTheme;
            }
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            if (!Enabled && isDarkTheme)
            {
                var g = pevent.Graphics;
                var rect = new Rectangle(0, 0, Width, Height);

                using (var brush = new SolidBrush(DisabledBackColor))
                {
                    g.FillRectangle(brush, rect);
                }

                TextRenderer.DrawText(g, Text, Font, rect, DisabledForeColor, GetTextFormatFlags());
            }
            else
            {
                base.OnPaint(pevent);
            }
        }

        private TextFormatFlags GetTextFormatFlags()
        {
            TextFormatFlags flags = TextFormatFlags.EndEllipsis | TextFormatFlags.WordBreak;

            switch (TextAlign)
            {
                case ContentAlignment.TopLeft:
                    return flags | TextFormatFlags.Top | TextFormatFlags.Left;
                case ContentAlignment.TopCenter:
                    return flags | TextFormatFlags.Top | TextFormatFlags.HorizontalCenter;
                case ContentAlignment.TopRight:
                    return flags | TextFormatFlags.Top | TextFormatFlags.Right;
                case ContentAlignment.MiddleLeft:
                    return flags | TextFormatFlags.VerticalCenter | TextFormatFlags.Left;
                case ContentAlignment.MiddleCenter:
                    return flags | TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter;
                case ContentAlignment.MiddleRight:
                    return flags | TextFormatFlags.VerticalCenter | TextFormatFlags.Right;
                case ContentAlignment.BottomLeft:
                    return flags | TextFormatFlags.Bottom | TextFormatFlags.Left;
                case ContentAlignment.BottomCenter:
                    return flags | TextFormatFlags.Bottom | TextFormatFlags.HorizontalCenter;
                case ContentAlignment.BottomRight:
                    return flags | TextFormatFlags.Bottom | TextFormatFlags.Right;
                default:
                    return flags | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;
            }
        }

        #region Support for default values of Color properties

        private bool ShouldSerializeDisabledBackColor() => DisabledBackColor != DefaultDisabledBackColor;
        private void ResetDisabledBackColor() => DisabledBackColor = DefaultDisabledBackColor;

        private bool ShouldSerializeDisabledForeColor() => DisabledForeColor != DefaultDisabledForeColor;
        private void ResetDisabledForeColor() => DisabledForeColor = DefaultDisabledForeColor;
        
        #endregion Support for default values of Color properties
    }
}
