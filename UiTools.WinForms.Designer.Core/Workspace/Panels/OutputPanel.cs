using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using UiTools.WinForms.Designer.Core.Properties;
using static UiTools.WinForms.Designer.Core.MessageLogger;

namespace UiTools.WinForms.Designer.Core
{
    public partial class OutputPanel : UserControl, ILogTarget
    {
        private static readonly Color DefaultLogBackColor = SystemColors.Window;
        private static readonly Color DefaultLogForeColor = SystemColors.ControlText;
        private static readonly Color DefaultLogLineBorderColor = ColorTranslator.FromHtml("#F2F2F2");
        private static readonly Color DefaultLogTraceLineColor = ColorTranslator.FromHtml("#A9A9A9");
        private static readonly Color DefaultLogWarningLineColor = Color.Chocolate;
        private static readonly Color DefaultLogErrorLineColor = Color.Red;
        private static readonly Color DefaultLogLinkColor = Color.Blue;

        public event EventHandler WordWrapChanged;
        public event EventHandler ShowTimestampChanged;

        private bool ignoreCheckedChangedEvent = false;
        private bool wordWrap = false;
        private bool showTimestamp = false;
        private readonly Dictionary<string, Exception> exceptionCache = new Dictionary<string, Exception>();
        private bool isDarkTheme = false;

        private WebView2 webView2;
        private TaskCompletionSource<bool> browserReadyTcs = new TaskCompletionSource<bool>();
        private bool isFirstLoad = true;

        public OutputPanel()
        {
            InitializeComponent();

            webView2 = new WebView2 { Dock = DockStyle.Fill, Visible = false };
            Controls.Add(webView2);
            webView2.BringToFront();

            foreach (ToolStripItem item in toolStrip1.Items)
            {
                item.MouseEnter += (s, e) => toolStrip1.Refresh();
                item.MouseLeave += (s, e) => toolStrip1.Refresh();
                if (item is ToolStripButton btn)
                    btn.CheckedChanged += (s, e) => toolStrip1.Refresh();
            }

            InitializeWebView();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ScaleToolStripIcons(toolStrip1);
        }

        private void ScaleToolStripIcons(ToolStrip ts)
        {
            float scaleFactor = DeviceDpi / 120f;
            int size = (int)(20 * scaleFactor);
            ts.ImageScalingSize = new Size(size, size);
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

            webView2.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "app.assets",
                AppDomain.CurrentDomain.BaseDirectory,
                CoreWebView2HostResourceAccessKind.Allow);

            webView2.CoreWebView2.ContextMenuRequested += CoreWebView2_ContextMenuRequested;
            webView2.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;

            webView2.CoreWebView2.DOMContentLoaded += (s, e) =>
            {
                if (isFirstLoad)
                {
                    isFirstLoad = false;
                    webView2.Visible = true;
                    browserReadyTcs.TrySetResult(true);
                }
            };

            ResetContent();
        }

        private async void CoreWebView2_ContextMenuRequested(object sender, CoreWebView2ContextMenuRequestedEventArgs e)
        {
            var deferral = e.GetDeferral();
            try
            {
                await InitContextMenuAsync(e.MenuItems);
            }
            finally
            {
                deferral.Complete();
            }
        }

        private async void ResetContent()
        {
            exceptionCache.Clear();

            if (isFirstLoad)
            {
                string initialClass = wordWrap ? "wrap" : "no-wrap";
                var css = ComposeDynamicStylesInnerHtml();
                string html = $@"<html><head><style id='dynamicStyles'>{css}</style></head><body class='{initialClass}'></body></html>";
                webView2.CoreWebView2?.NavigateToString(html);
            }
            else
            {
                await browserReadyTcs.Task;
                _ = webView2.CoreWebView2.ExecuteScriptAsync("document.body.innerHTML = '';");
            }
        }

        public bool WordWrap
        {
            get => wordWrap;
            set
            {
                if (wordWrap == value)
                    return;
                wordWrap = value;

                ignoreCheckedChangedEvent = true;
                tsbToggleWrap.Checked = value;
                ignoreCheckedChangedEvent = false;

                ApplyWordWrapAsync();

                WordWrapChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool ShowTimestamp
        {
            get => showTimestamp;
            set
            {
                if (showTimestamp == value)
                    return;
                showTimestamp = value;

                ignoreCheckedChangedEvent = true;
                tsbTimestamp.Checked = value;
                ignoreCheckedChangedEvent = false;

                ShowTimestampChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private async void ApplyWordWrapAsync()
        {
            await browserReadyTcs.Task;
            string className = wordWrap ? "wrap" : "no-wrap";
            await webView2.CoreWebView2.ExecuteScriptAsync($"document.body.className = '{className}';");
        }

        private void tsbToggleWrap_CheckedChanged(object sender, EventArgs e)
        {
            if (!ignoreCheckedChangedEvent)
                WordWrap = tsbToggleWrap.Checked;
        }

        private void tsbTimestamp_CheckStateChanged(object sender, EventArgs e)
        {
            if (!ignoreCheckedChangedEvent)
                ShowTimestamp = tsbTimestamp.Checked;
        }

        private void tsbClear_Click(object sender, EventArgs e)
        {
            ResetContent();
        }

        private void tsbSearch_Click(object sender, EventArgs e)
        {
            ShowSearchDialog();
        }

        public async void ShowSearchDialog()
        {
            await browserReadyTcs.Task;
            webView2.Focus();
            SendKeys.Send("^{f}");
        }

        public async void WriteLine(LogLevel level, string message, Exception ex)
        {
            if (IsDisposed)
                return;

            if (InvokeRequired)
            {
                Invoke(() => WriteLine(level, message, ex));
                return;
            }

            await browserReadyTcs.Task;

            string escapedMessage = WebUtility.HtmlEncode(message);
            string htmlToAdd = "";
            string lineClassName = "";

            switch (level)
            {
                case LogLevel.Verbose:
                    htmlToAdd = escapedMessage;
                    lineClassName = "trace-line";
                    break;
                case LogLevel.Info:
                    htmlToAdd = escapedMessage;
                    lineClassName = "info-line";
                    break;
                case LogLevel.Warning:
                    htmlToAdd = $"<span class='log-level'>[Warning]</span> {escapedMessage}";
                    lineClassName = "warning-line";
                    break;
                case LogLevel.Error:
                    if (ex == null)
                        htmlToAdd = $"<span class='log-level'>[Error]</span> {escapedMessage}";
                    else
                    {
                        string id = Guid.NewGuid().ToString();
                        exceptionCache[id] = ex;
                        htmlToAdd = $"<span class='log-level'>[Error]</span> {escapedMessage} <a href='app://show-ex/{id}' class='error-link'>[<span>Details</span>]</a>";
                    }
                    lineClassName = "error-line";
                    break;
            }

            string timeStamp = ShowTimestamp ? $"{DateTime.Now:HH:mm:ss:fff}  " : string.Empty;
            string fullLineHtml = $"{timeStamp}{htmlToAdd}";

            // Constructing the script to append the new div and scroll to bottom:
            string jsSnippet = $@"
                (function() {{
                    var div = document.createElement('div');
                    div.className = '{lineClassName}';
                    div.innerHTML = '{CommonStuff.EscapeJavaScriptString(fullLineHtml)}';
                    document.body.appendChild(div);
                    window.scrollTo(0, document.body.scrollHeight);
                }})();";

            await webView2.CoreWebView2.ExecuteScriptAsync(jsSnippet);
        }

        private void CoreWebView2_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            string url = e.Uri.ToString();
            if (url.StartsWith("app://show-ex/"))
            {
                e.Cancel = true;
                string id = url.Replace("app://show-ex/", "").TrimEnd('/');
                if (exceptionCache.TryGetValue(id, out Exception ex))
                    ShowExceptionDetails(ex);
            }
        }

        private void ShowExceptionDetails(Exception ex)
        {
            using (var frm = new ExceptionViewer())
            {
                frm.Exception = ex;
                frm.ShowDialog(FindForm());
            }
        }

        private async Task InitContextMenuAsync(ICollection<CoreWebView2ContextMenuItem> menuItems)
        {
            // Check for selected text:
            string result = await webView2.CoreWebView2.ExecuteScriptAsync("window.getSelection().toString().length > 0");
            bool hasSelection = (result == "true");

            // Clear all standard items (Copy, Paste, Print etc):
            menuItems.Clear();

            var environment = webView2.CoreWebView2.Environment;

            // Create "Copy" item:
            var copyItem = environment.CreateContextMenuItem("Copy", null, CoreWebView2ContextMenuItemKind.Command);
            copyItem.IsEnabled = hasSelection;
            copyItem.CustomItemSelected += async (s, args) => await webView2.CoreWebView2.ExecuteScriptAsync("document.execCommand('copy');");

            // Create "Select All" item:
            var selectAllItem = environment.CreateContextMenuItem("Select All", null, CoreWebView2ContextMenuItemKind.Command);
            selectAllItem.CustomItemSelected += async (s, args) => await webView2.CoreWebView2.ExecuteScriptAsync("document.execCommand('selectAll');");

            // Create "Clear All" item:
            var clearAllItem = environment.CreateContextMenuItem("Clear All", null, CoreWebView2ContextMenuItemKind.Command);
            clearAllItem.CustomItemSelected += (s, args) => ResetContent();

            // Add items to the menu:
            menuItems.Add(copyItem);
            menuItems.Add(selectAllItem);
            menuItems.Add(environment.CreateContextMenuItem("", null, CoreWebView2ContextMenuItemKind.Separator));
            menuItems.Add(clearAllItem);
        }

        public override void Refresh()
        {
            base.Refresh();
            webView2.Invalidate();
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
                    ApplyUiThemeToToolbarImages();

                    // Prevent white flash by setting the default background color of the engine:
                    webView2.DefaultBackgroundColor = isDarkTheme ? Color.FromArgb(31, 31, 31) : Color.White;

                    _ = ApplyUiThemeToBrowser();
                }
            }
        }

        private void ApplyUiThemeToToolbarImages()
        {
            tsbClear.Image = IsDarkTheme ? Resources.ClearWindowContent_DarkTheme : Resources.ClearWindowContent;
            tsbToggleWrap.Image = IsDarkTheme ? Resources.WordWrap_DarkTheme : Resources.WordWrap;
            tsbTimestamp.Image = IsDarkTheme ? Resources.TimeStamp_DarkTheme : Resources.TimeStamp;
            tsbSearch.Image = IsDarkTheme ? Resources.SearchText_DarkTheme : Resources.SearchText;
        }

        private async Task ApplyUiThemeToBrowser()
        {
            await browserReadyTcs.Task;

            webView2.CoreWebView2.Profile.PreferredColorScheme = isDarkTheme
                ? CoreWebView2PreferredColorScheme.Dark
                : CoreWebView2PreferredColorScheme.Light;

            string css = CommonStuff.EscapeJavaScriptString(ComposeDynamicStylesInnerHtml());
            string script = $@"
                (function() {{
                    var styleTag = document.getElementById('dynamicStyles');
                    if (styleTag) styleTag.innerHTML = '{css}';
                }})();";

            await webView2.ExecuteScriptAsync(script);
        }

        private string ComposeDynamicStylesInnerHtml()
        {
            var logBackColor = ColorTranslator.ToHtml(LogBackColor);
            var logForeColor = ColorTranslator.ToHtml(LogForeColor);
            var logLineBorderColor = ColorTranslator.ToHtml(LogLineBorderColor);
            var logTraceLineColor = ColorTranslator.ToHtml(LogTraceLineColor);
            var logWarningLineColor = ColorTranslator.ToHtml(LogWarningLineColor);
            var logErrorLineColor = ColorTranslator.ToHtml(LogErrorLineColor);
            var logLinkColor = ColorTranslator.ToHtml(LogLinkColor);

            var scrollFace = isDarkTheme ? ThemeApplier.SCROLL_FACE_COLOR_DARK : ThemeApplier.SCROLL_FACE_COLOR_LIGHT;
            var scrollTrack = logBackColor;

            return $@"
        body {{
            font-family: 'Consolas', monospace;
            font-size: 9pt;
            margin: 0;
            padding: 2px;
            background-color: {logBackColor};
            color: {logForeColor};
        }}
        html {{
            scrollbar-color: {scrollFace} {scrollTrack};
        }}
        body.wrap {{ white-space: pre-wrap; word-wrap: break-word; overflow-x: hidden; }}
        body.no-wrap {{ white-space: pre; overflow-x: auto; }}
        div.trace-line {{ margin-bottom: 2px; border-bottom: 1px solid {logLineBorderColor}; color: {logTraceLineColor}; }}
        div.info-line {{ margin-bottom: 2px; border-bottom: 1px solid {logLineBorderColor}; }}
        div.warning-line {{ margin-bottom: 2px; border-bottom: 1px solid {logLineBorderColor}; color: {logWarningLineColor}; }}
        div.error-line {{ margin-bottom: 2px; border-bottom: 1px solid {logLineBorderColor}; color: {logErrorLineColor}; }}
        a.error-link {{ text-decoration: none; color: {logLinkColor}; }}
        a.error-link span {{ text-decoration: underline; cursor: pointer; color: {logLinkColor}; }}
        span.log-level {{ font-weight: bold; }}";
        }

        [Category("Appearance")]
        public Color LogBackColor { get; set; } = DefaultLogBackColor;
        [Category("Appearance")]
        public Color LogForeColor { get; set; } = DefaultLogForeColor;
        [Category("Appearance")]
        public Color LogLineBorderColor { get; set; } = DefaultLogLineBorderColor;
        [Category("Appearance")]
        public Color LogTraceLineColor { get; set; } = DefaultLogTraceLineColor;
        [Category("Appearance")]
        public Color LogWarningLineColor { get; set; } = DefaultLogWarningLineColor;
        [Category("Appearance")]
        public Color LogErrorLineColor { get; set; } = DefaultLogErrorLineColor;
        [Category("Appearance")]
        public Color LogLinkColor { get; set; } = DefaultLogLinkColor;

        #region Support for default values of Color properties

        private bool ShouldSerializeLogBackColor() => LogBackColor != DefaultLogBackColor;
        private void ResetLogBackColor() => LogBackColor = DefaultLogBackColor;

        private bool ShouldSerializeLogForeColor() => LogForeColor != DefaultLogForeColor;
        private void ResetLogForeColor() => LogForeColor = DefaultLogForeColor;

        private bool ShouldSerializeLogLineBorderColor() => LogLineBorderColor != DefaultLogLineBorderColor;
        private void ResetLogLineBorderColor() => LogLineBorderColor = DefaultLogLineBorderColor;

        private bool ShouldSerializeLogTraceLineColor() => LogTraceLineColor != DefaultLogTraceLineColor;
        private void ResetLogTraceLineColor() => LogTraceLineColor = DefaultLogTraceLineColor;

        private bool ShouldSerializeLogWarningLineColor() => LogWarningLineColor != DefaultLogWarningLineColor;
        private void ResetLogWarningLineColor() => LogWarningLineColor = DefaultLogWarningLineColor;

        private bool ShouldSerializeLogErrorLineColor() => LogErrorLineColor != DefaultLogErrorLineColor;
        private void ResetLogErrorLineColor() => LogErrorLineColor = DefaultLogErrorLineColor;

        private bool ShouldSerializeLogLinkColor() => LogLinkColor != DefaultLogLinkColor;
        private void ResetLogLinkColor() => LogLinkColor = DefaultLogLinkColor;

        #endregion Support for default values of Color properties
    }
}
