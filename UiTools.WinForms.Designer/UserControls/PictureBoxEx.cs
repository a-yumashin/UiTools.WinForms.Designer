using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace UiTools.WinForms.Designer
{
    public class PictureBoxEx : PictureBox
    {
        private const int WM_SETCURSOR = 0x0020;
        private const int IDC_HAND = 32649;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SetCursor(IntPtr hCursor);

        protected override void WndProc(ref Message m)
        {
            if (Cursor == Cursors.Hand && m.Msg == WM_SETCURSOR)
            {
                // Fix ugly cursor
                SetCursor(LoadCursor(IntPtr.Zero, IDC_HAND));
                m.Result = IntPtr.Zero;
                return;
            }
            base.WndProc(ref m);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            BackColor = Color.SkyBlue;
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            BackColor = Color.LightBlue;
        }
    }
}
