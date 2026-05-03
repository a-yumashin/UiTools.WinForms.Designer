using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UiTools.WinForms.Designer.Core
{
    public class CSharpCodeViewer : UserControl, ICSharpCodeViewer
    {
        public event EventHandler RenderComplete;

        private WebView2 webView2;
        private bool wordWrap;
        private string fontFamily;
        private float fontSizeInPoints;
        private List<string> typeNames = new List<string>();
        private bool isDarkTheme = false;

        public CSharpCodeViewer()
        {
            webView2 = new WebView2 { Dock = DockStyle.Fill };
            Controls.Add(webView2);
            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            var userDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "UiTools.WinForms.Designer");
            var env = await CoreWebView2Environment.CreateAsync(null, userDataFolder);
            await webView2.EnsureCoreWebView2Async(env);

            webView2.CoreWebView2.Settings.IsStatusBarEnabled = false;
            webView2.CoreWebView2.Profile.PreferredColorScheme = isDarkTheme
                ? CoreWebView2PreferredColorScheme.Dark
                : CoreWebView2PreferredColorScheme.Light;

            //webView2.CoreWebView2.OpenDevToolsWindow();
            webView2.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "app.assets",
                AppDomain.CurrentDomain.BaseDirectory,
                CoreWebView2HostResourceAccessKind.Allow);

            webView2.CoreWebView2.ContextMenuRequested += CoreWebView2_ContextMenuRequested;

            webView2.CoreWebView2.DOMContentLoaded += (s, e) =>
            {
                HighlightCodeInBrowser();
                SetCodeFont(fontFamily, fontSizeInPoints);
                SetWordWrap(wordWrap);
                RenderComplete?.Invoke(this, EventArgs.Empty);
            };
        }

        private async void CoreWebView2_ContextMenuRequested(object sender, CoreWebView2ContextMenuRequestedEventArgs e)
        {
            var deferral = e.GetDeferral();
            try
            {
                // Check for selected text:
                string result = await webView2.CoreWebView2.ExecuteScriptAsync("window.getSelection().toString().length > 0");
                bool hasSelection = (result == "true");

                // Clear all standard items (Copy, Paste, Print etc):
                var menuItems = e.MenuItems;
                menuItems.Clear();

                var environment = webView2.CoreWebView2.Environment;

                // Create "Copy" item:
                var copyItem = environment.CreateContextMenuItem("Copy", null, CoreWebView2ContextMenuItemKind.Command);
                copyItem.IsEnabled = hasSelection;
                copyItem.CustomItemSelected += async (s, args) =>
                {
                    await webView2.CoreWebView2.ExecuteScriptAsync("document.execCommand('copy');");
                };

                // Create "Select All" item:
                var selectAllItem = environment.CreateContextMenuItem("Select All", null, CoreWebView2ContextMenuItemKind.Command);
                selectAllItem.CustomItemSelected += async (s, args) =>
                {
                    await webView2.CoreWebView2.ExecuteScriptAsync("document.execCommand('selectAll');");
                };

                // Add items to the menu:
                menuItems.Add(copyItem);
                menuItems.Add(environment.CreateContextMenuItem("", null, CoreWebView2ContextMenuItemKind.Separator));
                menuItems.Add(selectAllItem);
            }
            finally
            {
                deferral.Complete();
            }
        }

        private async void HighlightCodeInBrowser()
        {
            await Task.Delay(100);
            await webView2.CoreWebView2.ExecuteScriptAsync("hljs.highlightAll();");

            var typeNamesAsJsonArray = "[" + string.Join(",", typeNames.Select(n => $"\"{n}\"")) + "]";
            await webView2.CoreWebView2.ExecuteScriptAsync($"highlightTypeNames({typeNamesAsJsonArray});");
        }

        private async void SetWordWrap(bool isEnabled)
        {
            if (webView2 != null && webView2.CoreWebView2 != null)
            {
                await webView2.CoreWebView2.ExecuteScriptAsync($"setWordWrap({isEnabled.ToString().ToLower()});");
            }
        }

        #region ICSharpCodeViewer members

        public async void SetCodeFont(string fontFamily, float fontSizeInPoints)
        {
            this.fontFamily = fontFamily;
            this.fontSizeInPoints = fontSizeInPoints;
            if (webView2 != null && webView2.CoreWebView2 != null)
            {
                await webView2.CoreWebView2.ExecuteScriptAsync($"setFont('{fontFamily}', '{fontSizeInPoints}pt');");
            }
        }

        public bool WordWrap
        {
            get => wordWrap;
            set
            {
                wordWrap = value;
                SetWordWrap(wordWrap);
            }
        }

        public void View(string sourceCode, IEnumerable<string> dynamicClassNames)
        {
            var tempList = new List<string>(2000);
            tempList.AddRange(dynamicClassNames);
            tempList.AddRange(GetFixedTypeNames());
            tempList.AddRange(GetTypeNamesFromAssembly(typeof(Button).Assembly)); // assembly System.Windows.Forms.dll
            tempList.AddRange(GetTypeNamesFromAssembly(typeof(Color).Assembly)); // assembly System.Drawing.dll
            tempList.AddRange(GetTypeNamesFromAssembly(typeof(IContainer).Assembly, "System.ComponentModel")); // assembly System.dll, namespace System.ComponentModel
            typeNames = new List<string>(tempList.Distinct().OrderBy(n => n));

            var htmlTemplate = GetEmbeddedResource("SyntaxHighlighting.SourceCodePageTemplate.html");
            var themeName = CommonStuff.CurrentUiTheme == null ? "Light" : CommonStuff.CurrentUiTheme.Name;
            var scrollFace = isDarkTheme ? ThemeApplier.SCROLL_FACE_COLOR_DARK : ThemeApplier.SCROLL_FACE_COLOR_LIGHT;
            var scrollTrack = ColorTranslator.ToHtml(BackColor);
            htmlTemplate = htmlTemplate
                .Replace("{{CSS_PATH}}", "https://app.assets/")
                .Replace("{{THEME_NAME}}", themeName)
                .Replace("{{SCROLL_FACE}}", scrollFace)
                .Replace("{{SCROLL_TRACK}}", scrollTrack)
                .Replace("{{JS_PATH}}", "https://app.assets/")
                .Replace("{{SOURCE_CODE}}", WebUtility.HtmlEncode(sourceCode));
            webView2.CoreWebView2.NavigateToString(htmlTemplate);
        }

        public void ShowSearchDialog()
        {
            if (webView2.CoreWebView2 != null)
            {
                webView2.Focus();
                SendKeys.Send("^{f}");
            }
        }

        #endregion ICSharpCodeViewer members

        private string GetEmbeddedResource(string res)
        {
            using (var reader = new StreamReader(this.GetType().Assembly
                .GetManifestResourceStream("UiTools.WinForms.Designer.Core.Resources." + res)))
            {
                return reader.ReadToEnd();
            }
        }

        private List<string> GetTypeNamesFromAssembly(Assembly asm, string typeNamespace = "")
        {
            return asm.GetTypes()
                .Where(t => (t.IsPublic || t.IsNestedPublic) && !t.IsGenericType && (t.IsInterface || (t.IsClass && !t.IsAbstract) || t.IsEnum || t.IsValueType))
                .Where(t => typeNamespace == "" || (t.Namespace != null && t.Namespace.StartsWith(typeNamespace))) // t.Namespace will be null for nested types
                .Where(t => !t.Name.Contains("_") && !t.Name.All(c => Char.IsUpper(c))) // skip structs like _POINTL, <szCSDVersion>e__FixedBuffer, BITMAPINFO, MEASUREITEMSTRUCT etc
                .Select(t => t.Name)
                .ToList();
        }

        private List<string> GetFixedTypeNames()
        {
            return new string[] { "ArgumentException", "ArgumentNullException", "ArgumentOutOfRangeException", "Array", "Attribute", "BitConverter", "Buffer", "Boolean",
                "Byte", "Char", "Convert", "Console", "CultureInfo", "DateTime", "DateTimeOffset", "TimeSpan", "Decimal", "Double", "Enum", "Environment", "Exception",
                "Guid", "Int16", "Int32", "Int64", "IntPtr", "UInt16", "UInt32", "UInt64", "UIntPtr", "Math", "MessageBox", "NullReferenceException", "Object", "Random",
                "Single", "String", "StringBuilder", "Type", "Uri", "Version", "Void", "EventHandler", "Delegate", "Action", "Func", "EventArgs",
                // Collections
                "ArraySegment", "BitArray", "BlockingCollection", "ConcurrentBag", "ConcurrentDictionary", "ConcurrentQueue", "ConcurrentStack", "Dictionary", "HashSet",
                "List", "Queue", "SortedList", "Stack", "ImmutableArray", "ImmutableDictionary", "ImmutableList", "ImmutableQueue", "ImmutableStack",
                // Attributes
                "DebuggerNonUserCode", "DebuggerStepThrough",
                // I/O (System.IO)
                "File", "Directory", "Path", "FileInfo", "FileStream", "DirectoryInfo", "DriveInfo", "Stream", "StreamReader", "StreamWriter", "BinaryReader",
                "BinaryWriter", "MemoryStream", "StringReader", "StringWriter",
                // Networking (System.Net)
                "HttpClient", "HttpRequestMessage", "HttpResponseMessage", "IPAddress", "IPEndPoint", "Socket", "TcpClient", "TcpListener", "UdpClient",
                // Threading (System.Threading)
                "Thread", "Task", "CancellationToken", "CancellationTokenSource", "Monitor", "Mutex", "ReaderWriterLockSlim", "SemaphoreSlim", "Timer"
            }.ToList();
        }

        [Category("Appearance")]
        [DefaultValue(false)]
        public bool IsDarkTheme
        {
            get => isDarkTheme;
            set
            {
                if (isDarkTheme != value)
                {
                    isDarkTheme = value;
                    _ = ApplyUiThemeToWebView2();
                }
            }
        }

        private async Task ApplyUiThemeToWebView2()
        {
            if (webView2.CoreWebView2 == null)
                return;

            webView2.CoreWebView2.Profile.PreferredColorScheme = isDarkTheme
                ? CoreWebView2PreferredColorScheme.Dark
                : CoreWebView2PreferredColorScheme.Light;

            var css = CommonStuff.EscapeJavaScriptString(ComposeDynamicStylesInnerHtml());

            var themeName = CommonStuff.CurrentUiTheme == null ? "Light" : CommonStuff.CurrentUiTheme.Name;
            var cssPath = CommonStuff.EscapeJavaScriptString($"https://app.assets/{themeName}.css");

            var script = $@"
    (function() {{
        var styleTag = document.getElementById('dynamicStyles');
        if (styleTag) styleTag.innerHTML = '{css}';

        var link = document.getElementById('dynamicThemeLink');
        if (link) link.href = '{cssPath}';
    }})();";
            await webView2.ExecuteScriptAsync(script);
        }

        private string ComposeDynamicStylesInnerHtml()
        {
            var scrollFace = isDarkTheme ? ThemeApplier.SCROLL_FACE_COLOR_DARK : ThemeApplier.SCROLL_FACE_COLOR_LIGHT;
            var scrollTrack = ColorTranslator.ToHtml(BackColor);
            return $"html {{ scrollbar-color: {scrollFace} {scrollTrack}; }}";
        }
    }

    public interface ICSharpCodeViewer
    {
        bool WordWrap { get; set; }
        bool Visible { get; set; }
        event EventHandler RenderComplete;
        void SetCodeFont(string fontFamily, float fontSizeInPoints);
        void View(string sourceCode, IEnumerable<string> dynamicClassNames);
        void ShowSearchDialog();
        void BringToFront();
    }
}
