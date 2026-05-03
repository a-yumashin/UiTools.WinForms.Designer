using System;
using System.Windows.Forms;

namespace UiTools.WinForms.Designer.Core
{
    public class LinkLabelEx : LinkLabel
    {
        private readonly ToolTip toolTip = new ToolTip();
        private Link lastHoveredLink = null;

        public LinkLabelEx()
        {
            toolTip.InitialDelay = 500;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // Determine the link under the cursor:
            Link currentLink = PointInLink(e.X, e.Y);
            if (currentLink != lastHoveredLink)
            {
                // State changed: moved from text to a link, from a link to text, or to another link
                lastHoveredLink = currentLink;
                if (currentLink != null)
                {
                    string hint = currentLink.Description ?? currentLink.LinkData?.ToString();
                    if (!string.IsNullOrEmpty(hint))
                        toolTip.Show(hint, this, e.X, e.Y + 20); // show slightly below the cursor
                }
                else
                    toolTip.Hide(this);
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            lastHoveredLink = null;
            toolTip.Hide(this);
        }

        protected override void WndProc(ref Message m)
        {
            // Allow the base control to perform its logic: LinkLabel itself will determine if the mouse is over a LinkArea, 
            // and set its standard ("ugly") Cursors.Hand cursor:
            base.WndProc(ref m);

            if (m.Msg == Win32.WM_SETCURSOR)
            {
                // Check that the mouse is in the control's client area (HTCLIENT):
                int hitTest = (int)((long)m.LParam & 0xFFFF);
                if (hitTest == Win32.HTCLIENT)
                {
                    // If the base control decided to show the "hand" (Cursors.Hand), then Cursor.Current will
                    // be equal to Cursors.Hand. In this (and only this!) case, we replace it with the system IDC_HAND:
                    if (Cursor.Current == Cursors.Hand)
                    {
                        Win32.SetCursor(Win32.LoadCursor(IntPtr.Zero, Win32.IDC_HAND));
                        // Indicate that we have handled the cursor setting ourselves:
                        m.Result = (IntPtr)1;
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                toolTip.Dispose();
            base.Dispose(disposing);
        }
    }
}
