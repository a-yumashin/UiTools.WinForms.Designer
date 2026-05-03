using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UiTools.WinForms.Designer.Core
{
    public static class ThemeApplier
    {
        public const string SCROLL_FACE_COLOR_LIGHT = "#C2C3C9"; // used in CSS
        public const string SCROLL_FACE_COLOR_DARK = "#959595"; // used in CSS

        public static void Apply(Control root, UiTheme theme, bool applyProcessWideSettings = false)
        {
            bool isToolStrip = root is ToolStrip;
            if (!isToolStrip && !root.IsHandleCreated)
                return;

            if (theme == null)
                return;

            if (root.IsHandleCreated)
                Win32.SendMessage(root.Handle, Win32.WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);

            // 1. Apply font and colors from the given theme:
            ApplyInternal(root, theme, - 1);

            bool isDark = IsDark(root.BackColor);

            // 2. Apply process-wide settings (affects only context menu in the title bar, as I can see):
            if (applyProcessWideSettings)
                ApplyContextMenuTheme(isDark);

            // 3. Apply theme to form's title bar:
            if (root is Form form)
                TryApplyTitleBarTheme(form, isDark);

            if (root.IsHandleCreated)
            {
                Win32.SendMessage(root.Handle, Win32.WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
                root.Invalidate(true);
                root.Update();
            }

            // Fix issue with TreeView border painting:
            _ = Task.Run(async () =>
            {
                await Task.Delay(100);
                root.BeginInvoke(root.Refresh);
            });
        }

        private static void ApplyInternal(Control root, UiTheme theme, int nestingLevel)
        {
            nestingLevel++; // used for debug output only
            ApplyToControl(root, theme, nestingLevel);
            foreach (Control child in root.Controls)
            {
                if (child.GetType().FullName != "System.Windows.Forms.Design.DesignerFrame") // UI Theme should NOT be applied to DesignSurface contents!
                    ApplyInternal(child, theme, nestingLevel);
            }
            TryApplyIsDarkThemeProperty(root, IsDark(root.BackColor));
        }

        private static void ApplyToControl(Control control, UiTheme theme, int nestingLevel)
        {
            if (control.Tag != null && control.Tag.ToString() == "NoTheme") // quick'n'dirty solution for cases when some control must be colored in a special manner
                return;

            var font = theme.GetFont();
            if (font != null)
                control.Font = font;

            var ctlType = control.GetType();
            var ctlTypeFullName = ctlType.FullName;
            var ctlThemeSpecific = theme.ControlThemes.FirstOrDefault(p => p.Type == ctlTypeFullName);
            var ctlThemesInherited = theme.ControlThemes.Where(p => p.IncludeInheritedTypes && TypeInheritsOther(ctlType, p.Type)).ToList();
            var overridenProps = new List<string>();
            var commonTheme = theme.AllControls;
            if (commonTheme != null)
            {
                foreach (var commonColorProp in commonTheme.ColorProperties)
                {
                    string colorValue = null; // overriden value
                    // First try to pick color property value from <Control> tags of base types (if any):
                    ctlThemesInherited.ForEach(theme => colorValue = GetColorValueFromTheme(theme, commonColorProp.Name) ?? colorValue);
                    // Now try to pick color property value from <Control> tag of this exact type:
                    colorValue = GetColorValueFromTheme(ctlThemeSpecific, commonColorProp.Name) ?? colorValue;
                    SetProperty(
                        control,
                        commonColorProp.Name,
                        ResolveColor(colorValue ?? commonColorProp.Value), // << if no override found - fallback to the "all-controls" (common) theme
                        nestingLevel);
                    if (colorValue != null)
                        overridenProps.Add(commonColorProp.Name);
                }
            }
            // Now process the "chain" from base type(s) theme(s) down to the "control-specific" one (skipping properties which were already picked above):
            foreach (var ctlThemeInherited in ctlThemesInherited)
            {
                foreach (var colorProp in ctlThemeInherited.ColorProperties.Where(p => !overridenProps.Contains(p.Name)))
                    SetProperty(
                        control,
                        colorProp.Name,
                        ResolveColor(colorProp.Value),
                        nestingLevel);
            }
            if (ctlThemeSpecific != null)
            {
                foreach (var colorProp in ctlThemeSpecific.ColorProperties.Where(p => !overridenProps.Contains(p.Name)))
                    SetProperty(
                        control,
                        colorProp.Name,
                        ResolveColor(colorProp.Value),
                        nestingLevel);
            }
        }

        private static string GetColorValueFromTheme(ControlTheme ctlTheme, string colorPropName)
        {
            if (ctlTheme == null)
                return null;
            var ctlColorProp = ctlTheme.ColorProperties.FirstOrDefault(cp => cp.Name == colorPropName);
            return ctlColorProp?.Value;
        }

        private static bool TypeInheritsOther(Type t, string otherTypeName)
        {
            Type otherType = ResolveType(otherTypeName);
            if (otherType == null)
            {
                Debug.WriteLine("{0}.{1}(): failed to resolve type '{2}'", nameof(ThemeApplier), nameof(TypeInheritsOther), otherTypeName);
                return false;
            }
            return otherType.IsAssignableFrom(t);
        }

        private static Type ResolveType(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type != null)
                return type;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(typeName);
                if (type != null)
                    return type;
            }

            return null;
        }

        private static void SetProperty(Control target, string propertyPath, Color color, int nestingLevel)
        {
            var currentOwner = GetPropertyOwnerAndDescriptor(target, propertyPath, out PropertyDescriptor pd);
            if (pd == null || currentOwner == null)
                return;
            pd.SetValue(currentOwner, color);
            //var indent = new string(' ', nestingLevel * 2);
            //Debug.WriteLine($"{indent}{target.GetType().Name} {target.Name}: applied '{color}' to property '{propertyPath}'");
        }

        private static void TryApplyIsDarkThemeProperty(Control target, bool isDarkTheme)
        {
            var pi = target.GetType().GetProperty("IsDarkTheme", BindingFlags.Instance | BindingFlags.Public);
            if (pi != null && pi.PropertyType == typeof(bool))
                pi.SetValue(target, isDarkTheme);
        }

        /// <summary>
        /// Scans property path (e.g. "DefaultCellStyle.BackColor") and returns object which owns the rightmost property
        /// as well as the corresponding property descriptor.
        /// </summary>
        private static object GetPropertyOwnerAndDescriptor(Control target, string propertyPath, out PropertyDescriptor pd)
        {
            pd = null;
            string[] parts = propertyPath.Split('.');
            object currentTarget = target;

            for (int i = 0; i < parts.Length; i++)
            {
                var props = TypeDescriptor.GetProperties(currentTarget);
                pd = props.Find(parts[i], false);
                if (pd == null)
                    return null;
                if (i < parts.Length - 1)
                {
                    currentTarget = pd.GetValue(currentTarget);
                    if (currentTarget == null)
                        return null;
                }
                else
                {
                    if (pd.PropertyType != typeof(Color))
                        return null;
                    return currentTarget;
                }
            }
            return null;
        }

        private static readonly Regex rxHtmlCode = new Regex("^#[a-f0-9]{6}$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex rxArgb32value = new Regex(@"^\d+$", RegexOptions.Compiled);
        private static readonly Regex rxArgbValue = new Regex(@"^(?:(?<A>\d+),[ ]?)?(?<R>\d+),[ ]?(?<G>\d+),[ ]?(?<B>\d+)$", RegexOptions.Compiled);
        public static Color ResolveColor(string color)
        {
            // Supported color formats:
            // 1. "Known color":      "Yellow", "WindowText"
            // 2. HTML code:          "#FFAA55" (or "#ffaa55")
            // 3. RGB or ARGB value:  "128, 255, 128" or "255, 128, 255, 128"
            // 4. 32-bit ARGB value:  "11200750"
            if (rxHtmlCode.IsMatch(color))
                return ColorTranslator.FromHtml(color); // case 2
            if (rxArgb32value.IsMatch(color))
                return Color.FromArgb(int.Parse(color)); // case 4
            var matches = rxArgbValue.Matches(color);
            if (matches.Count == 1)
            {
                // case 3
                var a = matches[0].Groups["A"].Value;
                var r = matches[0].Groups["R"].Value;
                var g = matches[0].Groups["G"].Value;
                var b = matches[0].Groups["B"].Value;
                return string.IsNullOrEmpty(a)
                    ? Color.FromArgb(int.Parse(r), int.Parse(g), int.Parse(b))
                    : Color.FromArgb(int.Parse(a), int.Parse(r), int.Parse(g), int.Parse(b));
            }
            try
            {
                return Color.FromKnownColor((KnownColor)Enum.Parse(typeof(KnownColor), color)); // case 1
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to resolve color '{color}': {ex.Message}");
                return Color.Empty;
            }
            // NOTE: cases 1, 2 and 4 can be also processed using System.Drawing.ColorConverter.ConvertFromString() method,
            //       so - in cases 2 and 4 - regular expressions could be avoided; however, left this function unchanged.
        }

        public static void ApplyScrollBarTheme(Control control, bool isDark)
        {
            if (control == null || !control.IsHandleCreated)
                return;
            ApplyScrollBarTheme(control.Handle, isDark);
        }

        public static void ApplyScrollBarTheme(IntPtr hWnd, bool isDark)
        {
            if (CommonStuff.CurrentUiTheme == null)
                return;
            if (IsWindows10Version1809OrLater())
            {
                try
                {
                    // The "DarkMode_Explorer" theme is supported in Windows 10 v1809+
                    Win32.SetWindowTheme(hWnd, isDark ? "DarkMode_Explorer" : null, null);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("SetWindowTheme() failed: " + ex.Message);
                }
            }
        }

        private static void ApplyContextMenuTheme(bool isDark)
        {
            // NOTE: must be called from MainForm.OnHandleCreated() and from MainForm.OnUiThemeChanged(),
            //       *after* Apply() async method has finished (and so we know whether the selected theme is dark or not)
            if (CommonStuff.CurrentUiTheme == null)
                return;
            if (IsWindows10Version1809OrLater())
            {
                try
                {
                    // Both functions are supported in Windows 10 v1809+
                    Win32.SetPreferredAppMode(isDark ? Win32.PreferredAppMode.ForceDark : Win32.PreferredAppMode.ForceLight);
                    Win32.FlushMenuThemes();
                }
                catch
                {
                }
            }
        }

        private static void TryApplyTitleBarTheme(Form form, bool isDarkTheme)
        {
            // NOTE: must be called *after* SetPreferredAppMode() has been called, from each form's OnHandleCreated().
            if (IsWindows10Version1809OrLater())
            {
                try
                {
                    // DWMWA_USE_IMMERSIVE_DARK_MODE is supported in Windows 10 v1809+
                    int darkMode = isDarkTheme ? 1 : 0;
                    Win32.DwmSetWindowAttribute(form.Handle, Win32.DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int));

                    if (IsWindows11OrLater())
                    {
                        // DWMWA_CAPTION_COLOR and DWMWA_TEXT_COLOR are supported in Windows 11+
                        var pi = form.GetType().GetProperty("TitleBarBackColor", BindingFlags.Instance | BindingFlags.Public);
                        if (pi != null && pi.PropertyType == typeof(Color))
                        {
                            var captionColor = (Color)pi.GetValue(form, null);
                            int captionColorRef = ColorTranslator.ToWin32(captionColor);
                            Win32.DwmSetWindowAttribute(form.Handle, Win32.DWMWA_CAPTION_COLOR, ref captionColorRef, Marshal.SizeOf(typeof(int)));
                        }

                        pi = form.GetType().GetProperty("TitleBarForeColor", BindingFlags.Instance | BindingFlags.Public);
                        if (pi != null && pi.PropertyType == typeof(Color))
                        {
                            var textColor = (Color)pi.GetValue(form, null);
                            int textColorRef = ColorTranslator.ToWin32(textColor);
                            Win32.DwmSetWindowAttribute(form.Handle, Win32.DWMWA_TEXT_COLOR, ref textColorRef, Marshal.SizeOf(typeof(int)));
                        }
                    }

                    // Hack to redraw title bar on some Win10 versions:
                    var oldStyle = form.FormBorderStyle;
                    form.FormBorderStyle = FormBorderStyle.None;
                    form.FormBorderStyle = oldStyle;
                }
                catch
                {
                }
                form.Invalidate(true);
            }
        }

        public static bool IsDark(Color color)
        {
            double luminance = (0.2126 * color.R) + (0.7152 * color.G) + (0.0722 * color.B);
            return luminance < 128;
        }

        private static bool IsWindows10Version1809OrLater()
        {
            var osVersion = Environment.OSVersion.Version;
            return (osVersion.Major == 10 && osVersion.Build >= 17763) || osVersion.Major > 10;
        }

        private static bool IsWindows11OrLater()
        {
            var osVersion = Environment.OSVersion.Version;
            return (osVersion.Major == 10 && osVersion.Build >= 22000) || osVersion.Major > 10;
        }
    }
}
