using System;
using System.Runtime.InteropServices;

namespace UiTools.WinForms.Designer.Core
{
    public static class Win32
    {
        public const int WM_SETTEXT = 0x000C;
        public const int WM_KILLFOCUS = 0x0008;
        public const int WM_SETFOCUS = 0x0007;
        public const int WM_ENABLE = 0x000A;

        public const int WM_PAINT = 0x000F;
        public const int WM_ERASEBKGND = 0x0014;
        public const int WM_PRINTCLIENT = 0x0318;
        public const int WM_MOUSEMOVE = 0x0200;
        public const int WM_MOUSELEAVE = 0x02A3;
        public const int WM_SETREDRAW = 11;
        public const int WM_PARENTNOTIFY = 0x0210;
        public const int WM_CREATE = 0x0001;

        public const int WM_SETCURSOR = 0x0020;
        public const int IDC_HAND = 32649;
        public const int IDC_WAIT = 32514;

        // Flags for WM_PRINTCLIENT:
        public const int PRF_CLIENT = 0x00000004;
        public const int PRF_ERASEBKGND = 0x00000008;
        public const int PRF_NONCLIENT = 0x00000002;

        public const int TV_FIRST = 0x1100;
        public const int TVM_SETEXTENDEDSTYLE = TV_FIRST + 44;
        public const int TVS_EX_DOUBLEBUFFER = 0x0004;

        public const int LB_GETCURSEL = 0x188;
        public const int HTCLIENT = 1;

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct COMBOBOXINFO
        {
            public int cbSize;
            public RECT rcItem;
            public RECT rcButton;
            public int stateButton;
            public IntPtr hwndCombo;
            public IntPtr hwndItem;
            public IntPtr hwndList;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X, Y;
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetComboBoxInfo(IntPtr hWnd, ref COMBOBOXINFO pcbi);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool ValidateRect(IntPtr hWnd, IntPtr lpRect);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string lclassName, string windowTitle);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr WindowFromPoint(POINT Point);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetCursor(IntPtr hCursor);

        #region UI Themes support

        // DWMWINDOWATTRIBUTE enum values
        public const int DWMWA_CAPTION_COLOR = 35; // supported in Windows 11+
        public const int DWMWA_TEXT_COLOR = 36;    // supported in Windows 11+
        public const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20; // supported in Windows 10 v1809+

        public enum PreferredAppMode
        {
            Default,
            AllowDark,
            ForceDark,
            ForceLight,
            Max
        }

        [DllImport("uxtheme.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList); // "DarkMode_Explorer" theme is supported in Windows 10 v1809+

        [DllImport("uxtheme.dll", EntryPoint = "#135", SetLastError = true)]
        public static extern int SetPreferredAppMode(PreferredAppMode appMode); // supported in Windows 10 v1809+

        [DllImport("uxtheme.dll", EntryPoint = "#136", SetLastError = true)]
        public static extern void FlushMenuThemes(); // supported in Windows 10 v1809+

        [DllImport("DwmApi.dll", SetLastError = true)]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        #endregion UI Themes support
    }
}
