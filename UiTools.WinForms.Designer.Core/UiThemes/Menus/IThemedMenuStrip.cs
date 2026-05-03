using System.Drawing;

namespace UiTools.WinForms.Designer.Core
{
    public interface IThemedMenuStrip
    {
        Color CheckedAndSelectedColor { get; set; }
        Color CheckedColor { get; set; }
        Color DropDownBackColor { get; set; }
        Color DropDownBorderColor { get; set; }
        Color HoverColor { get; set; }
        Color HoverTextColor { get; set; }
        Color ImageMarginColor { get; set; }
        Color ItemAccentColor { get; set; }
        Color SelectedColor { get; set; }
        Color SeparatorColor { get; set; }
        Color TextColor { get; set; }
    }
}