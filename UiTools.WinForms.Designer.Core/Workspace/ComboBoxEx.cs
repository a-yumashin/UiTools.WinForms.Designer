using System;
using System.Diagnostics;
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
    internal class ComboBoxEx : ThemedComboBox
    {
        private readonly NativeDropDownList dropDown;
        private readonly ToolTip tooltip = new ToolTip();
        private Font boldFont;
        private TextMeasurer textMeasurer;

        public ComboBoxEx() : base()
        {
            DropDownStyle = ComboBoxStyle.DropDownList;
            DrawMode = DrawMode.OwnerDrawFixed;
            dropDown = new NativeDropDownList(this, tooltip);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            boldFont?.Dispose();
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
            textMeasurer = new TextMeasurer(this);
        }
        protected override void OnHandleDestroyed(EventArgs e)
        {
            dropDown.ReleaseHandle();
            base.OnHandleDestroyed(e);
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            boldFont = new Font(Font, FontStyle.Bold);
            textMeasurer = new TextMeasurer(this);
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);
            if (!IsHandleCreated)
                return;
            tooltip.RemoveAll(); // important!
            if (!DroppedDown && SelectedIndex >= 0 && IsItemTextTruncated(SelectedIndex, considerArrowButtonWidth: true))
                tooltip.SetToolTip(this, Items[SelectedIndex].ToString());
            else
                tooltip.SetToolTip(this, string.Empty);
        }

        /// <summary>
        /// Draws text of the selected item in the TEXT AREA.
        /// </summary>
        protected override void DrawText(Graphics graphics)
        {
            Color textColor = DrawFocusBackground(graphics);
            int btnWidth = SystemInformation.VerticalScrollBarWidth;
            var itemBounds = new Rectangle(3, 3, ClientRectangle.Width - btnWidth - 6, ClientRectangle.Height - 6);
            DrawItemInternal(graphics, SelectedIndex, itemBounds, textColor);
        }

        /// <summary>
        /// Draws item in the DROPDOWN LIST.
        /// </summary>
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            e.DrawBackground();
            DrawItemInternal(e.Graphics, e.Index, e.Bounds, Enabled ? e.ForeColor : DisabledForeColor);
        }

        private void DrawItemInternal(Graphics graphics, int itemIndex, Rectangle itemBounds, Color foreColor)
        {
            if (itemIndex >= 0 && IsHandleCreated)
            {
                var text = Items[itemIndex].ToString();
                var flags = TextFormatFlags.NoPadding | TextFormatFlags.Left;
                if (text.Contains(' '))
                {
                    if (textMeasurer == null)
                        throw new InvalidOperationException(
                            $"{nameof(ComboBoxEx)}.{nameof(OnDrawItem)}(): {nameof(textMeasurer)} field not initialized!"); // developer's error
                    var parts = text.Split(" ".ToCharArray(), 2);
                    var firstPartSize = textMeasurer.MeasureBoldText(parts[0]);
                    var firstPartBounds = new Rectangle(new Point(itemBounds.X + LEFT_PADDING, itemBounds.Y), firstPartSize);
                    var spaceSize = textMeasurer.MeasureRegularText(" ");
                    TextRenderer.DrawText(graphics, parts[0], boldFont, firstPartBounds, foreColor, flags);
                    var secondPartLeft = firstPartBounds.Right + 2 * spaceSize.Width;
                    var secondPartSize = textMeasurer.MeasureRegularText(parts[1]);
                    var secondPartBounds = new Rectangle(new Point(secondPartLeft, itemBounds.Y), secondPartSize);
                    TextRenderer.DrawText(graphics, parts[1], Font, secondPartBounds, foreColor, flags);
                }
                else
                {
                    var bounds = itemBounds;
                    bounds.Offset(LEFT_PADDING, 0);
                    TextRenderer.DrawText(graphics, text, Font, bounds, foreColor, flags);
                }
            }
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
            info.cbSize = Marshal.SizeOf(info);
            Win32.GetComboBoxInfo(Handle, ref info);
            return info.hwndList;
        }

        private int GetItemWidth()
        {
            var info = new Win32.COMBOBOXINFO();
            info.cbSize = Marshal.SizeOf(info);
            Win32.GetComboBoxInfo(Handle, ref info);
            return info.rcItem.Right - info.rcItem.Left;
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
                Win32.GetCursorPos(out Win32.POINT cursorPos);
                if (m.Msg == Win32.WM_MOUSEMOVE && Handle == Win32.WindowFromPoint(cursorPos))
                {
                    // Mouse cursor is moving over the dropdown portion
                    int itemIndex = (int)Win32.SendMessage(Handle, Win32.LB_GETCURSEL, IntPtr.Zero, IntPtr.Zero);
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
    }
}
