using System.Drawing;
using System.Windows.Forms;

namespace UiTools.WinForms.Designer.Core
{
    public class ThemedMenuStripColorTable : ProfessionalColorTable
    {
        private readonly IThemedMenuStrip owner;

        public ThemedMenuStripColorTable(IThemedMenuStrip owner)
        {
            this.owner = owner;
        }

        // Dropdown menu backcolor:
        public override Color ToolStripDropDownBackground => owner.DropDownBackColor;

        // Side area for icons:
        public override Color ImageMarginGradientBegin => owner.ImageMarginColor;
        public override Color ImageMarginGradientMiddle => owner.ImageMarginColor;
        public override Color ImageMarginGradientEnd => owner.ImageMarginColor;

        // Dropdown menu border color:
        public override Color MenuBorder => owner.DropDownBorderColor;
        public override Color ToolStripBorder => owner.DropDownBorderColor;

        // Menu item hover backcolor:
        public override Color MenuItemSelected => owner.HoverColor;
        public override Color MenuItemSelectedGradientBegin => owner.HoverColor;
        public override Color MenuItemSelectedGradientEnd => owner.HoverColor;

        // Selected (pressed) menu item backcolor:
        public override Color MenuItemPressedGradientBegin => owner.SelectedColor;
        public override Color MenuItemPressedGradientMiddle => owner.SelectedColor;
        public override Color MenuItemPressedGradientEnd => owner.SelectedColor;

        // Selected (pressed) menu item border color:
        public override Color MenuItemBorder => owner.ItemAccentColor;

        // Item separator color:
        public override Color SeparatorDark => owner.SeparatorColor;
        public override Color SeparatorLight => Color.Transparent;

        // Checked menu item backcolor:
        public override Color CheckBackground => owner.CheckedColor;
        public override Color CheckSelectedBackground => owner.CheckedAndSelectedColor;
        public override Color CheckPressedBackground => owner.SelectedColor;
    }
}
