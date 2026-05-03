using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace UiTools.WinForms.Designer.Core
{
    /// <summary>
    /// A custom label control designed as a replacement for the standard <see cref="Label"/>. 
    /// Unlike the stock control, it bypasses the default system 'gray-out' rendering 
    /// triggered by the disabled state, allowing text to be drawn with the specified 
    /// <see cref="Control.ForeColor"/> even when <see cref="Control.Enabled"/> is false.
    /// This is essential for maintaining consistent appearance in custom UI themes.
    /// </summary>
    public class ThemedLabel : Control
    {
        private static readonly Color DefaultDisabledForeColor = SystemColors.GrayText;
        
        private Color disabledForeColor = DefaultDisabledForeColor;

        public ThemedLabel()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);

            BackColor = Color.Transparent;
        }

        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override string Text
        {
            get => base.Text;
            set
            {
                base.Text = value;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (string.IsNullOrEmpty(Text))
                return;

            TextFormatFlags flags = TextFormatFlags.Left |
                                    TextFormatFlags.VerticalCenter |
                                    TextFormatFlags.EndEllipsis |
                                    TextFormatFlags.NoPadding;

            TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, Enabled ? ForeColor : DisabledForeColor, flags);
        }

        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override bool AutoSize
        {
            get => base.AutoSize;
            set
            {
                base.AutoSize = value;
                if (value)
                    AdjustSize();
            }
        }

        private void AdjustSize()
        {
            if (AutoSize)
                Size = GetPreferredSize(Size.Empty);
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            if (string.IsNullOrEmpty(Text))
                return new Size(10, 10);

            Size textSize = TextRenderer.MeasureText(Text, Font);
            return textSize;
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            if (AutoSize)
                AdjustSize();
            Invalidate();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            if (AutoSize)
                AdjustSize();
            Invalidate();
        }

        protected override void OnForeColorChanged(EventArgs e)
        {
            base.OnForeColorChanged(e);
            Invalidate();
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Invalidate();
        }

        [Category("Appearance")]
        public Color DisabledForeColor
        {
            get => disabledForeColor;
            set
            {
                disabledForeColor = value;
                Invalidate();
            }
        }

        #region Support for default values of Color properties

        private bool ShouldSerializeDisabledForeColor() => DisabledForeColor != DefaultDisabledForeColor;
        private void ResetDisabledForeColor() => DisabledForeColor = DefaultDisabledForeColor;

        #endregion Support for default values of Color properties
    }
}
