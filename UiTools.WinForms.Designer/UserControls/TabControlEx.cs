using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UiTools.WinForms.Designer.Properties;

namespace UiTools.WinForms.Designer
{
    internal class TabControlEx : TabControl
    {
        public event EventHandler<int> TabPageCloseRequested;

        private Font boldFont;

        private const int WM_SETCURSOR = 0x0020;
        private const int IDC_HAND = 32649;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SetCursor(IntPtr hCursor);

        public TabControlEx()
        {
            if (LicenseManager.UsageMode != LicenseUsageMode.Designtime) // otherwise we may get OOM in design mode
                DrawMode = TabDrawMode.OwnerDrawFixed;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            boldFont = new Font(Font, FontStyle.Bold);
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            var currentPage = TabPages[e.Index];
            var isSelected = e.State == DrawItemState.Selected;
            var tabRect = e.Bounds;

            Padding = new Point(21, 3);

            if (isSelected)
                e.Graphics.FillRectangle(Brushes.RoyalBlue, tabRect);

            var dx = isSelected ? 4 : 0;
            var dy = isSelected ? -1 : 1;
            TextRenderer.DrawText(e.Graphics,
                currentPage.Text,
                isSelected ? boldFont : e.Font,
                new Rectangle(tabRect.X + 3 + dx, tabRect.Y + dy, tabRect.Width - 3 - dx, tabRect.Height),
                isSelected ? Color.White : currentPage.ForeColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

            e.Graphics.DrawImage(isSelected ? Resources.CloseSelectedTab : Resources.CloseTab, CalculateCloseIconRectangle(tabRect, isSelected));
        }

        private Rectangle CalculateCloseIconRectangle(Rectangle tabRect, bool isSelected)
        {
            var w = Resources.CloseTab.Width;
            var dx = isSelected ? -3 : 0;
            var dy = isSelected ? 1 : 2;
            return new Rectangle(tabRect.Right - w - 2 + dx, tabRect.Top + (tabRect.Height - w) / 2 + dy, w, w);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            for (int i = 0; i < TabPages.Count; i++)
            {
                var closeRect = CalculateCloseIconRectangle(GetTabRect(i), SelectedIndex == i);
                if (closeRect.Contains(e.Location))
                {
                    TabPageCloseRequested?.Invoke(this, i);
                    break;
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            for (int i = 0; i < TabPages.Count; i++)
            {
                var closeRect = CalculateCloseIconRectangle(GetTabRect(i), SelectedIndex == i);
                if (closeRect.Contains(e.Location))
                {
                    if (Cursor != Cursors.Hand)
                        Cursor = Cursors.Hand;
                    break;
                }
                else
                {
                    if (Cursor == Cursors.Hand)
                        Cursor = Cursors.Default;
                }
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (Cursor == Cursors.Hand)
                Cursor = Cursors.Default;
        }

        protected override void OnClick(EventArgs e)
        {
            SelectedTab.Focus();
        }

        protected override void OnEnter(EventArgs e)
        {
            if (SelectedTab != null)
                SelectedTab.Focus();
        }

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
    }
}
