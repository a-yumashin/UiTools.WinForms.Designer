using System;

namespace UiTools.WinForms.Designer.Core
{
    public class NativeWaitCursor : IDisposable
    {
        private IntPtr oldCursor;

        public NativeWaitCursor()
        {
            oldCursor = Win32.SetCursor(Win32.LoadCursor(IntPtr.Zero, Win32.IDC_WAIT));
        }

        public void Dispose()
        {
            if (oldCursor != IntPtr.Zero)
            {
                Win32.SetCursor(oldCursor);
            }
        }
    }
}
