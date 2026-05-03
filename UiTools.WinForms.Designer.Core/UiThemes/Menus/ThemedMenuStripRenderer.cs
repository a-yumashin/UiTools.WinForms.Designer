using System.Drawing;
using System.Windows.Forms;

namespace UiTools.WinForms.Designer.Core
{
    public class ThemedMenuStripRenderer : ToolStripProfessionalRenderer
    {
        private readonly IThemedMenuStrip owner;

        public ThemedMenuStripRenderer(IThemedMenuStrip owner)
            : base(new ThemedMenuStripColorTable(owner))
        {
            this.owner = owner;
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = e.Item.Selected || e.Item.Pressed
                ? owner.HoverTextColor
                : owner.TextColor;
            base.OnRenderItemText(e);
        }

        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            e.ArrowColor = owner.TextColor;
            base.OnRenderArrow(e);
        }

        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
        {
            var g = e.Graphics;
            var rect = new Rectangle(e.ImageRectangle.X - 2, e.ImageRectangle.Y - 1, e.ImageRectangle.Width + 3, e.ImageRectangle.Height + 1);

            Color backColor = e.Item.Selected
                ? owner.CheckedAndSelectedColor
                : owner.CheckedColor;

            using (var brush = new SolidBrush(backColor))
            {
                g.FillRectangle(brush, rect);
            }

            using (var pen = new Pen(owner.ItemAccentColor))
            {
                g.DrawRectangle(pen, rect);
            }

            if (e.Item.Image == null)
            {
                // Turn on HQ drawing:
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                float scaleFactor = g.DpiX / 120f;
                using (var pen = new Pen(owner.TextColor, 2f * scaleFactor))
                {
                    float left = rect.Left + (rect.Width * 0.25f);
                    float top = rect.Top + (rect.Height * 0.5f);
                    float bottom = rect.Bottom - (rect.Height * 0.3f);
                    float right = rect.Right - (rect.Width * 0.25f);
                    float middleX = rect.Left + (rect.Width * 0.45f);

                    PointF[] points = new PointF[]
                    {
                        new PointF(left, top),
                        new PointF(middleX, bottom),
                        new PointF(right, rect.Top + (rect.Height * 0.3f))
                    };

                    g.DrawLines(pen, points);
                }

                // Turn off HQ drawing:
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
            }
        }
    }
}
