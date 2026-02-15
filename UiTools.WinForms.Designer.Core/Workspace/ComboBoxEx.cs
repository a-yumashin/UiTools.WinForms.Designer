using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace UiTools.WinForms.Designer.Core
{
    /// <summary>
    /// ComboBox which can paint two parts of item text (separated with a space) with different font styles - bold and regular.
    /// Also, if item text is too long and so gets truncated, it is displayed in a tooltip.
    /// </summary>
    internal class ComboBoxEx : ComboBox
    {
        private readonly NativeDropDownList dropDown;
        private readonly ToolTip tooltip = new ToolTip();
        private Font boldFont;
        private TextMeasurer textMeasurer;
        private const int LEFT_PADDING = 4;

        public ComboBoxEx() : base()
        {
            DropDownStyle = ComboBoxStyle.DropDownList;
            DrawMode = DrawMode.OwnerDrawFixed;
            dropDown = new NativeDropDownList(this, tooltip);
        }

        public void SetTextMeasurer(TextMeasurer textMeasurer)
        {
            this.textMeasurer = textMeasurer;
        }

        public void Clear()
        {
            Items.Clear();
            tooltip.RemoveAll();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            dropDown.AssignHandle(GetListPortionHandle());
            boldFont = new Font(Font, FontStyle.Bold);
        }
        protected override void OnHandleDestroyed(EventArgs e)
        {
            dropDown.ReleaseHandle();
            base.OnHandleDestroyed(e);
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);
            tooltip.RemoveAll(); // important!
            if (!DroppedDown && SelectedIndex >= 0 && IsItemTextTruncated(SelectedIndex, considerArrowButtonWidth: true))
                tooltip.SetToolTip(this, Items[SelectedIndex].ToString());
            else
                tooltip.SetToolTip(this, string.Empty);
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            e.DrawBackground();
            // NOTE: TextRenderer.DrawText() renders text better than g.DrawString() does.
            if (e.Index >= 0)
            {
                var text = Items[e.Index].ToString();
                var flags = TextFormatFlags.NoPadding | TextFormatFlags.Left;
                if (text.Contains(' '))
                {
                    if (textMeasurer == null)
                        throw new InvalidOperationException(
                            $"{nameof(ComboBoxEx)}.{nameof(OnDrawItem)}(): {nameof(textMeasurer)} field not initialized!"); // developer's error
                    var parts = text.Split(" ".ToCharArray(), 2);
                    var firstPartSize = textMeasurer.MeasureBoldText(parts[0]);
                    var firstPartBounds = new Rectangle(new Point(e.Bounds.X + LEFT_PADDING, e.Bounds.Y), firstPartSize);
                    var spaceSize = textMeasurer.MeasureRegularText(" ");
                    TextRenderer.DrawText(e.Graphics, parts[0], boldFont, firstPartBounds, e.ForeColor, flags);
                    var secondPartLeft = firstPartBounds.Right + 2 * spaceSize.Width;
                    var secondPartSize = textMeasurer.MeasureRegularText(parts[1]);
                    var secondPartBounds = new Rectangle(new Point(secondPartLeft, e.Bounds.Y), secondPartSize);
                    TextRenderer.DrawText(e.Graphics, parts[1], e.Font, secondPartBounds, e.ForeColor, flags);
                }
                else
                {
                    var bounds = e.Bounds;
                    bounds.Offset(LEFT_PADDING, 0);
                    TextRenderer.DrawText(e.Graphics, text, e.Font, bounds, e.ForeColor, flags);
                }
            }
            if (Focused)
                e.DrawFocusRectangle();
        }

        protected override void OnDropDownClosed(EventArgs e)
        {
            base.OnDropDownClosed(e);
            tooltip.Hide(this);
            dropDown.ResetOnCloseUp();
        }

        private bool IsItemTextTruncated(int itemIndex, bool considerArrowButtonWidth)
        {
            if (textMeasurer == null)
                throw new InvalidOperationException(
                    $"{nameof(ComboBoxEx)}.{nameof(IsItemTextTruncated)}(): {nameof(textMeasurer)} field not initialized!"); // developer's error
            var text = Items[itemIndex].ToString();
            int textWidth;
            if (text.Contains(' '))
            {
                var parts = text.Split(" ".ToCharArray(), 2);
                var firstPartSize = textMeasurer.MeasureBoldText(parts[0]);
                var spaceSize = textMeasurer.MeasureRegularText(" ");
                var secondPartSize = textMeasurer.MeasureRegularText(parts[1]);
                textWidth = LEFT_PADDING + firstPartSize.Width + 2 * spaceSize.Width + secondPartSize.Width;
            }
            else
                textWidth = LEFT_PADDING + textMeasurer.MeasureRegularText(text).Width;
            return textWidth > (considerArrowButtonWidth ? GetItemWidth() : ClientSize.Width);
        }

        private IntPtr GetListPortionHandle()
        {
            var info = new Win32.COMBOBOXINFO();
            info.size = Marshal.SizeOf(info);
            Win32.GetComboBoxInfo(Handle, out info);
            return info.listHwnd;
        }

        private int GetItemWidth()
        {
            var info = new Win32.COMBOBOXINFO();
            info.size = Marshal.SizeOf(info);
            Win32.GetComboBoxInfo(Handle, out info);
            return info.item.right - info.item.left;
        }

        private class NativeDropDownList : NativeWindow
        {
            private int lastIndex = -1;
            
            private ComboBoxEx parentControl;
            private ToolTip toolTip;

            public NativeDropDownList(ComboBoxEx parentControl, ToolTip toolTip)
            {
                this.parentControl = parentControl;
                this.toolTip = toolTip;
            }

            public void ResetOnCloseUp()
            {
                lastIndex = -1;
            }

            protected override void WndProc(ref Message m)
            {
                Win32.POINT cursorPos;
                Win32.GetCursorPos(out cursorPos);
                if (m.Msg == Win32.WM_MOUSEMOVE && Handle == Win32.WindowFromPoint(cursorPos))
                {
                    // Mouse cursor is moving over the dropdown portion
                    int itemIndex = Win32.SendMessage(Handle, Win32.LB_GETCURSEL, 0, 0);
                    if (itemIndex >= 0 && lastIndex != itemIndex)
                    {
                        lastIndex = itemIndex;
                        if (parentControl.IsItemTextTruncated(itemIndex, considerArrowButtonWidth: false))
                        {
                            int lParamInt32 = m.LParam.ToInt32();
                            int xPos = (short)(lParamInt32 & 0xFFFF);
                            int yPos = (short)(lParamInt32 >> 16);
                            toolTip.Show(parentControl.Items[itemIndex].ToString(), parentControl, xPos, yPos + 50);
                        }
                        else
                            toolTip.Hide(parentControl);
                    }
                }
                base.WndProc(ref m);
            }
        }

        private static class Win32
        {
            internal const int WM_MOUSEMOVE = 0x0200;
            internal const int LB_GETCURSEL = 0x188;

            internal struct RECT
            {
                public int left, top, right, bottom;
            }

            internal struct COMBOBOXINFO
            {
                public int size;
                public RECT item;
                public RECT button;
                public int state;
                public IntPtr comboHwnd;
                public IntPtr itemHwnd;
                public IntPtr listHwnd;
            }

            internal struct POINT
            {
                public int X, Y;
            }

            [DllImport("user32")]
            internal static extern bool GetComboBoxInfo(IntPtr hwnd, out COMBOBOXINFO info);

            [DllImport("user32.dll")]
            internal static extern IntPtr WindowFromPoint(POINT Point);

            [DllImport("user32.dll")]
            internal static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

            [DllImport("user32.dll")]
            internal static extern bool GetCursorPos(out POINT lpPoint);
        }
    }
}
