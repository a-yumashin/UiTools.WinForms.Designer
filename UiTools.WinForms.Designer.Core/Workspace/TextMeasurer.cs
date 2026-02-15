using System.Drawing;
using System.Windows.Forms;

namespace UiTools.WinForms.Designer.Core
{
    public class TextMeasurer
    {
        private readonly Size proposedSize = new Size(int.MaxValue, int.MaxValue);
        private readonly TextFormatFlags flags = TextFormatFlags.NoPadding | TextFormatFlags.Left;
        private readonly Graphics graphics;
        private readonly Font font;
        private readonly Font boldFont;

        public TextMeasurer(Graphics graphics, Font font)
        {
            this.graphics = graphics;
            this.font = font;
            boldFont = new Font(font, FontStyle.Bold);
        }

        public Size MeasureBoldText(string text)
        {
            return TextRenderer.MeasureText(graphics, text, boldFont, proposedSize, flags);
        }

        public Size MeasureRegularText(string text)
        {
            return TextRenderer.MeasureText(graphics, text, font, proposedSize, flags);
        }
    }
}
