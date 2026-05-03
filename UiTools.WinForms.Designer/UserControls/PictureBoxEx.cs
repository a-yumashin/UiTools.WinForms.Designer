using System;
using System.Drawing;
using System.Windows.Forms;
using UiTools.WinForms.Designer.Core;

namespace UiTools.WinForms.Designer
{
    public class PictureBoxEx : PictureBox
    {
        protected override void WndProc(ref Message m)
        {
            if (Cursor == Cursors.Hand && m.Msg == Win32.WM_SETCURSOR)
            {
                // Fix ugly cursor
                Win32.SetCursor(Win32.LoadCursor(IntPtr.Zero, Win32.IDC_HAND));
                m.Result = IntPtr.Zero;
                return;
            }
            base.WndProc(ref m);
        }

        public Color HoverBackColor { get; set; } = Color.Empty;

        private Color memBackColor;
        protected override void OnMouseEnter(EventArgs e)
        {
            if (!HoverBackColor.IsEmpty)
            {
                memBackColor = BackColor;
                BackColor = HoverBackColor;
            }
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            if (!HoverBackColor.IsEmpty)
                BackColor = memBackColor;
        }
    }
}
