using System;
using System.Runtime.InteropServices;

namespace UiTools.WinForms.Designer.Core
{
    public class NativeWaitCursor : IDisposable
    {
        [DllImport("user32.dll")]
        private static extern IntPtr SetCursor(IntPtr hCursor);

        [DllImport("user32.dll")]
        private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

        private const int IDC_WAIT = 32514;
        private IntPtr oldCursor;

        public NativeWaitCursor()
        {
            IntPtr waitCursor = LoadCursor(IntPtr.Zero, IDC_WAIT);
            oldCursor = SetCursor(waitCursor);
        }

        public void Dispose()
        {
            if (oldCursor != IntPtr.Zero)
            {
                SetCursor(oldCursor);
            }
        }
    }
}
