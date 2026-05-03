using System.ComponentModel;
using System.Windows.Forms;

namespace UiTools.WinForms.Designer.Core
{
    public class ThemedTextBox : TextBox
    {
        private bool isDarkTheme = false;

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
                    BorderStyle = isDarkTheme ? BorderStyle.FixedSingle : BorderStyle.Fixed3D;
                }
            }
        }
    }
}
