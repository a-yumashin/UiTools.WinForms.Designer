using System;
using System.Drawing;
using System.Windows.Forms;

namespace UiTools.WinForms.Designer.Core
{
    public class TextMeasurer : IDisposable
    {
        private readonly Size proposedSize = new Size(int.MaxValue, int.MaxValue);
        private readonly TextFormatFlags flags = TextFormatFlags.NoPadding | TextFormatFlags.Left;
        private readonly Control owner;
        private readonly Font boldFont;

        public TextMeasurer(Control owner)
        {
            this.owner = owner;
            boldFont = new Font(owner.Font, FontStyle.Bold);
        }

        public Size MeasureBoldText(string text)
        {
            using (var graphics = owner.CreateGraphics())
            {
                return TextRenderer.MeasureText(graphics, text, boldFont, proposedSize, flags);
            }
        }

        public Size MeasureRegularText(string text)
        {
            using (var graphics = owner.CreateGraphics())
            {
                return TextRenderer.MeasureText(graphics, text, owner.Font, proposedSize, flags);
            }
        }

        public void Dispose()
        {
            boldFont?.Dispose();
        }
    }
}
