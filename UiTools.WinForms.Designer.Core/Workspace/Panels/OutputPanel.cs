using System;
using System.Collections.Generic;
using System.Net;
using System.Windows.Forms;
using UiTools.WinForms.Designer.Core.Properties;
using static UiTools.WinForms.Designer.Core.MessageLogger;

namespace UiTools.WinForms.Designer.Core
{
    public partial class OutputPanel : UserControl, ILogTarget
    {
        public event EventHandler WordWrapChanged;
        public event EventHandler ShowTimestampChanged;

        private bool ignoreCheckedChangedEvent = false;
        private bool wordWrap = false;
        private bool showTimestamp = false;
        private readonly Dictionary<string, Exception> exceptionCache = new Dictionary<string, Exception>();
        private ContextMenuStrip outputContextMenu;

        public OutputPanel()
        {
            InitializeComponent();
            ResetContent();
            InitContextMenu();
        }

        private void ResetContent()
        {
            exceptionCache.Clear();
            string initialClass = wordWrap ? "wrap" : "no-wrap";

            string html = $@"<html>
<meta http-equiv='X-UA-Compatible' content='IE=edge'>
<head>
    <style>
        body {{
            font-family: 'Consolas', monospace;
            font-size: 9pt;
            margin: 5px;
            background-color: white;
        }}
        body.wrap {{ white-space: pre-wrap; word-wrap: break-word; overflow-x: hidden; }}
        body.no-wrap {{ white-space: pre; overflow-x: auto; }}
        div.trace-line {{ margin-bottom: 2px; border-bottom: 1px solid #f2f2f2; color: #A9A9A9; }}
        div.info-line {{ margin-bottom: 2px; border-bottom: 1px solid #f2f2f2; }}
        div.warning-line {{ margin-bottom: 2px; border-bottom: 1px solid #f2f2f2; color: chocolate; }}
        div.error-line {{ margin-bottom: 2px; border-bottom: 1px solid #f2f2f2; color: red; }}
        a.error-link {{ text-decoration: none; color: blue; }}
        a.error-link span {{ text-decoration: underline; cursor: pointer; color: blue; }}
        span.log-level {{ font-weight: bold; }}
    </style>
</head>
<body class='{initialClass}'></body>
</html>";

            browserOutput.DocumentText = html;
            while (browserOutput.ReadyState != WebBrowserReadyState.Complete)
            {
                Application.DoEvents();
                System.Threading.Thread.Sleep(1);
            }
            browserOutput.Document.Body.InnerHtml = ""; // otherwise we get an empty line in the very beginning
            browserOutput.Document.Focus(); // otherwise selection with double click doesn't work

            browserOutput.Document.ContextMenuShowing += (s, e) =>
            {
                e.ReturnValue = false; // turn off the built-in menu
                if (outputContextMenu == null)
                    InitContextMenu();
                outputContextMenu.Show(Cursor.Position);
            };
        }

        public bool WordWrap
        {
            get => wordWrap;
            set
            {
                if (wordWrap == value)
                    return;
                wordWrap = value;

                if (browserOutput.Document != null && browserOutput.Document.Body != null)
                    browserOutput.Document.Body.SetAttribute("className", value ? "wrap" : "no-wrap");

                ignoreCheckedChangedEvent = true;
                tsbToggleWrap.Checked = value;
                ignoreCheckedChangedEvent = false;

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
            browserOutput.Focus();
            SendKeys.Send("^{f}");
        }

        public void WriteLine(LogLevel level, string message, Exception ex)
        {
            if (browserOutput.InvokeRequired)
            {
                browserOutput.Invoke(new Action(() => WriteLine(level, message, ex)));
                return;
            }

            if (browserOutput.Document == null || browserOutput.Document.Body == null)
                return;

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

            var div = browserOutput.Document.CreateElement("div");
            div.SetAttribute("className", lineClassName);
            string timeStamp = ShowTimestamp ? $"{DateTime.Now:HH:mm:ss:fff}  " : string.Empty;
            div.InnerHtml = $"{timeStamp}{htmlToAdd}";
            browserOutput.Document.Body.AppendChild(div);
            //Debug.WriteLine(browserOutput.Document.GetElementsByTagName("html")[0].InnerHtml);

            browserOutput.Document.Window.ScrollTo(0, browserOutput.Document.Body.ScrollRectangle.Height);
        }

        private void browserOutput_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            string url = e.Url.ToString();
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

        private void InitContextMenu()
        {
            outputContextMenu = new ContextMenuStrip();

            var miCopy = new ToolStripMenuItem("Copy", Resources.Copy, (s, e) => browserOutput.Document.ExecCommand("Copy", false, null));
            var miSelectAll = new ToolStripMenuItem("Select All", null, (s, e) => browserOutput.Document.ExecCommand("SelectAll", false, null));
            var miClearAll = new ToolStripMenuItem("Clear All", Resources.ClearWindowContent, (s, e) => ResetContent());

            outputContextMenu.Items.Add(miCopy);
            outputContextMenu.Items.Add(miSelectAll);
            outputContextMenu.Items.Add(new ToolStripSeparator());
            outputContextMenu.Items.Add(miClearAll);

            outputContextMenu.Opening += (s, e) => miCopy.Enabled = !string.IsNullOrEmpty(
                browserOutput.Document?.InvokeScript("eval", new object[] { "window.getSelection().toString()" })?.ToString());
        }

        public override void Refresh()
        {
            base.Refresh();
            browserOutput.Invalidate();
        }
    }
}
