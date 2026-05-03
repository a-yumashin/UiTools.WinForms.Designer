using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace UiTools.WinForms.Designer.Core
{
    internal class ThemedToolStrip : ToolStrip
    {
        private static readonly Color DefaultTextColor = SystemColors.ControlText;
        private static readonly Color DefaultHoverColor = Color.FromArgb(229, 243, 255);
        private static readonly Color DefaultCheckedColor = Color.FromArgb(204, 232, 255);
        private static readonly Color DefaultAccentBorderColor = Color.FromArgb(0, 120, 215);

        private bool isDarkTheme = false;

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            BeginInvoke(ApplyToolStripTheme);
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
                    ApplyToolStripTheme();
                }
            }
        }

        private void ApplyToolStripTheme()
        {
            Renderer = new ThemedToolStripRenderer(this);
            Refresh();
        }

        [Category("Appearance")]
        public Color TextColor { get; set; } = DefaultTextColor;
        [Category("Appearance")]
        public Color HoverColor { get; set; } = DefaultHoverColor;
        [Category("Appearance")]
        public Color CheckedColor { get; set; } = DefaultCheckedColor;
        [Category("Appearance")]
        public Color AccentBorderColor { get; set; } = DefaultAccentBorderColor;

        #region Support for default values of Color properties

        private bool ShouldSerializeTextColor() => TextColor != DefaultTextColor;
        private void ResetTextColor() => TextColor = DefaultTextColor;

        private bool ShouldSerializeHoverColor() => HoverColor != DefaultHoverColor;
        private void ResetHoverColor() => HoverColor = DefaultHoverColor;

        private bool ShouldSerializeAccentBorderColor() => AccentBorderColor != DefaultAccentBorderColor;
        private void ResetAccentBorderColor() => AccentBorderColor = DefaultAccentBorderColor;

        #endregion Support for default values of Color properties

        private class ThemedToolStripRenderer : ToolStripProfessionalRenderer
        {
            private readonly ThemedToolStrip owner;

            public ThemedToolStripRenderer(ThemedToolStrip owner)
                : base(new ThemedToolStripColorTable(owner))
            {
                this.owner = owner;
            }

            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e) { } // suppress border

            protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
            {
                var item = e.Item;
                var g = e.Graphics;
                var rect = new Rectangle(0, 0, item.Width, item.Height);

                bool isChecked = (item is ToolStripButton btn) && btn.Checked;
                bool isSelected = item.Selected;
                bool isPressed = item.Pressed;

                using (var b = new SolidBrush(owner.BackColor))
                {
                    g.FillRectangle(b, rect);
                }

                if (isChecked || isPressed)
                {
                    using (var b = new SolidBrush(owner.CheckedColor)) g.FillRectangle(b, rect);
                    using (var p = new Pen(owner.AccentBorderColor)) g.DrawRectangle(p, 0, 0, rect.Width - 1, rect.Height - 1);
                }
                else if (isSelected)
                {
                    using (var b = new SolidBrush(owner.HoverColor)) g.FillRectangle(b, rect);
                    using (var p = new Pen(owner.AccentBorderColor)) g.DrawRectangle(p, 0, 0, rect.Width - 1, rect.Height - 1);
                }
            }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                e.TextColor = owner.TextColor;
                base.OnRenderItemText(e);
            }

            protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
            {
                e.ArrowColor = owner.TextColor;
                base.OnRenderArrow(e);
            }
        }

        private class ThemedToolStripColorTable : ProfessionalColorTable
        {
            private readonly ThemedToolStrip owner;

            public ThemedToolStripColorTable(ThemedToolStrip owner)
            {
                this.owner = owner;
            }

            public override Color ToolStripGradientBegin => owner.BackColor;
            public override Color ToolStripGradientMiddle => owner.BackColor;
            public override Color ToolStripGradientEnd => owner.BackColor;
            public override Color MenuBorder => owner.BackColor;
            public override Color ToolStripBorder => owner.BackColor;

            public override Color ButtonSelectedBorder => Color.Transparent;
            public override Color ButtonPressedBorder => Color.Transparent;
            public override Color ButtonSelectedHighlight => Color.Transparent;
            public override Color ButtonPressedHighlight => Color.Transparent;
            public override Color CheckBackground => owner.BackColor;
            public override Color CheckSelectedBackground => owner.BackColor;
            public override Color CheckPressedBackground => owner.BackColor;
        }
    }
}
