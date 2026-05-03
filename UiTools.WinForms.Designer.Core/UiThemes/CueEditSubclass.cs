using System;
using System.Drawing;
using System.Windows.Forms;

namespace UiTools.WinForms.Designer.Core
{
    internal sealed class CueEditSubclass : NativeWindow, IDisposable // TODO: rename (because it's not only about cue banner)
    {
        private readonly ThemedComboBox owner;
        private bool disposed;

        public CueEditSubclass(ThemedComboBox owner, IntPtr editHandle)
        {
            this.owner = owner ?? throw new ArgumentNullException(nameof(owner));
            if (editHandle == IntPtr.Zero)
                throw new ArgumentException("editHandle");
            AssignHandle(editHandle);
        }

        protected override void WndProc(ref Message m)
        {
            if (disposed)
            {
                base.WndProc(ref m);
                return;
            }

            switch (m.Msg)
            {
                case Win32.WM_SETTEXT:
                case Win32.WM_ENABLE:
                    base.WndProc(ref m);
                    Invalidate();
                    return;

                case Win32.WM_SETFOCUS:
                case Win32.WM_KILLFOCUS:
                    base.WndProc(ref m);
                    Invalidate();
                    return;

                case Win32.WM_ERASEBKGND:
                    // Ignore this message to prevent "white flashes"
                    m.Result = (IntPtr)1;
                    break;

                case Win32.WM_PAINT:
                    PaintWithCue();
                    m.Result = IntPtr.Zero;
                    return;

                default:
                    base.WndProc(ref m);
                    return;
            }
        }

        private void PaintWithCue()
        {
            if (IsHandleNullOrInvalid())
                return;

            Win32.GetClientRect(Handle, out Win32.RECT rect);
            var rc = new Rectangle(0, 0, rect.Right - rect.Left, rect.Bottom - rect.Top);

            using (var g = Graphics.FromHwnd(Handle))
            using (BufferedGraphicsContext ctx = BufferedGraphicsManager.Current)
            using (BufferedGraphics buffer = ctx.Allocate(g, rc))
            {
                // Draw the Edit window in buffer:
                IntPtr hdc = buffer.Graphics.GetHdc();
                Win32.SendMessage(Handle, Win32.WM_PRINTCLIENT, hdc, (IntPtr)(Win32.PRF_CLIENT | Win32.PRF_ERASEBKGND));
                buffer.Graphics.ReleaseHdc(hdc);

                // Fill background:
                using (var brush = new SolidBrush(owner.Enabled ? owner.BackColor : owner.DisabledBackColor))
                {
                    buffer.Graphics.FillRectangle(brush, rc);
                }

                // Draw text (if any):
                if (!string.IsNullOrEmpty(owner.Text))
                {
                    Color textColor = owner.Enabled ? owner.ForeColor : owner.DisabledForeColor;
                    var textRect = new Rectangle(0, -1, rc.Width - 4, rc.Height);
                    TextFormatFlags flags = TextFormatFlags.Left |
                                            TextFormatFlags.VerticalCenter |
                                            TextFormatFlags.EndEllipsis |
                                            TextFormatFlags.NoPadding;

                    TextRenderer.DrawText(buffer.Graphics, owner.Text, owner.Font, textRect,
                                          textColor, flags);
                }

                // Draw cue banner if needed:
                if (ShouldShowCue())
                {
                    var textRect = rc;
                    int leftPadding = 2;
                    int rightPadding = 2;
                    textRect.Inflate(-leftPadding, 0);
                    textRect.Width -= rightPadding;

                    Color cueColor = owner.CueBannerForeColor;
                    TextFormatFlags flags = TextFormatFlags.Left |
                                            TextFormatFlags.VerticalCenter |
                                            TextFormatFlags.EndEllipsis |
                                            TextFormatFlags.NoPadding;
                    TextRenderer.DrawText(buffer.Graphics, owner.CueBannerText, owner.Font, textRect, cueColor, flags);
                }

                // Render buffer to screen in one go:
                buffer.Render(g);
            }

            Win32.ValidateRect(Handle, IntPtr.Zero); // this will break the WM_PAINT loop
        }

        private bool ShouldShowCue()
        {
            return string.IsNullOrEmpty(owner.Text)
                   && !owner.Focused
                   && owner.Enabled
                   && !string.IsNullOrEmpty(owner.CueBannerText);
        }

        private bool IsHandleNullOrInvalid()
        {
            return Handle == IntPtr.Zero || disposed;
        }

        internal void Invalidate()
        {
            if (!IsHandleNullOrInvalid())
                Win32.InvalidateRect(Handle, IntPtr.Zero, true);
        }

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                try { ReleaseHandle(); } catch { }
            }
        }
    }
}
