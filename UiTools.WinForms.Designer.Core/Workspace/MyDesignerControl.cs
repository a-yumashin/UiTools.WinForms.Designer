using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace UiTools.WinForms.Designer.Core
{
    public class MyDesignerControl : TabControl, IDesignerControl
    {
        private EventHandler<RequestSourceCodeArgs> requestSourceCodeEvent;

        private readonly TabPage tabPageDesigner = new TabPage { Text = "Designer" };
        private readonly TabPage tabPageSourceCode = new TabPage { Text = "Source Code" };

        private readonly Panel pnlSourceCode = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
        private readonly Panel pnlSourceCodeHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 40,
            BackColor = SystemColors.ControlLight
        };
        private readonly LinkLabelEx lnkGenerateSourceCode = new LinkLabelEx
        {
            Text = "Generate source code",
            AutoSize = true,
            Left = 5,
            Top = 9
        };
        private readonly LinkLabelEx lnkSearchInSourceCode = new LinkLabelEx
        {
            Text = "Search in source code",
            Enabled = false,
            AutoSize = true,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Top = 9
        };
        private readonly Label labSourceCodeStatus = new Label
        {
            AutoSize = true,
            Left = 5
        };
        private readonly CheckBox chkWordWrap = new CheckBox
        {
            Text = "Word Wrap",
            Enabled = false,
            AutoSize = true,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Top = 9
        };
        private readonly ICSharpCodeViewer cSharpCodeViewer = new CSharpCodeViewer
        {
            Dock = DockStyle.Fill,
            WordWrap = true
        };

        public MyDesignerControl()
        {
            Alignment = TabAlignment.Bottom;
            //Appearance = TabAppearance.FlatButtons; // NOTE: unfortunately, this makes the TabControl non-working

            pnlSourceCodeHeader.Controls.Add(lnkGenerateSourceCode);
            pnlSourceCodeHeader.Controls.Add(lnkSearchInSourceCode);
            pnlSourceCodeHeader.Controls.Add(chkWordWrap);
            chkWordWrap.Checked = cSharpCodeViewer.WordWrap;
            chkWordWrap.CheckedChanged += (s, e) => cSharpCodeViewer.WordWrap = chkWordWrap.Checked;

            pnlSourceCode.Controls.Add(pnlSourceCodeHeader);
            pnlSourceCode.Controls.Add(labSourceCodeStatus);
            pnlSourceCode.Controls.Add(cSharpCodeViewer as CSharpCodeViewer);
            (cSharpCodeViewer as CSharpCodeViewer).BringToFront(); // otherwise pnlSourceCodeHeader overlaps the top part of cSharpCodeViewer
            labSourceCodeStatus.Top = pnlSourceCodeHeader.Height + 10;

            tabPageSourceCode.Controls.Add(pnlSourceCode);
            Controls.AddRange(new[] { tabPageDesigner, tabPageSourceCode });

            lnkGenerateSourceCode.LinkClicked += lnkGenerateSourceCode_LinkClicked;
            lnkSearchInSourceCode.LinkClicked += (s, e) => cSharpCodeViewer.ShowSearchDialog();

            CreateBusyIndicator();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            chkWordWrap.Left = pnlSourceCodeHeader.ClientSize.Width - chkWordWrap.Width - 5;
            lnkSearchInSourceCode.Left = chkWordWrap.Left - lnkSearchInSourceCode.Width - 10;
        }

        private void lnkGenerateSourceCode_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            cSharpCodeViewer.Visible = false; // otherwise WebView2 control (the only one in CSharpCodeViewer) changes cursor at will
            lnkGenerateSourceCode.Enabled = false;
            if (requestSourceCodeEvent != null)
            {
                var args = new RequestSourceCodeArgs();
                args.StatusUpdater = (status) => { labSourceCodeStatus.Text += status + "...\n"; labSourceCodeStatus.Refresh(); };
                requestSourceCodeEvent(this, args);
                if (args.SourceCode != null)
                {
                    cSharpCodeViewer.RenderComplete += OnCodeRenderComplete;
                    cSharpCodeViewer.View(args.SourceCode, args.ComponentTypeNames);
                }
                else
                {
                    labSourceCodeStatus.Text = "";
                    cSharpCodeViewer.Visible = true;
                    lnkGenerateSourceCode.Enabled = true;
                }
            }
        }

        private void OnCodeRenderComplete(object sender, EventArgs e)
        {
            labSourceCodeStatus.Text = "";
            cSharpCodeViewer.Visible = true;
            lnkGenerateSourceCode.Enabled = true;
            lnkSearchInSourceCode.Enabled = true;
            chkWordWrap.Enabled = true;
            cSharpCodeViewer.RenderComplete -= OnCodeRenderComplete;
        }

        private void CreateBusyIndicator()
        {
            if (tabPageDesigner.Controls.ContainsKey("BusyIndicator"))
                return;
            var label = new Label { Name = "BusyIndicator", AutoSize = true, BackColor = Color.Transparent, Visible = false };
            CenterLabel();
            tabPageDesigner.Controls.Add(label);
            Resize += (s, e) => CenterLabel();
            label.TextChanged += (s, e) => CenterLabel();

            void CenterLabel()
            {
                var textSize = TextRenderer.MeasureText(label.Text, label.Font); // label.Size gives wrong values here (seems AutoSize works with some delay)
                label.Left = (tabPageDesigner.ClientSize.Width - textSize.Width) / 2;
                label.Top = (tabPageDesigner.ClientSize.Height - textSize.Height) / 2;
            }
        }

        #region IDesignerControl members

        event EventHandler<RequestSourceCodeArgs> IDesignerControl.RequestSourceCode
        {
            add { requestSourceCodeEvent += value; }
            remove { requestSourceCodeEvent -= value; }
        }

        void IDesignerControl.AdoptViewAsChild(Control designSurfaceView)
        {
            designSurfaceView.Font = rootComponentFont;
            tabPageDesigner.Controls.Add(designSurfaceView);
        }

        void IDesignerControl.ShowBusyIndicator(string text)
        {
            if (!tabPageDesigner.Controls.ContainsKey("BusyIndicator"))
                return;
            var label = tabPageDesigner.Controls["BusyIndicator"] as Label;
            label.Text = text;
            label.Visible = true;
            Refresh();
        }

        void IDesignerControl.HideBusyIndicator()
        {
            if (!tabPageDesigner.Controls.ContainsKey("BusyIndicator"))
                return;
            var label = tabPageDesigner.Controls["BusyIndicator"] as Label;
            label.Visible = false;
        }

        private Font rootComponentFont;
        void IDesignerControl.SetRootComponentFont(Font font)
        {
            rootComponentFont = font;
        }

        void IDesignerControl.SetSourceCodeViewerFont(Font font)
        {
            cSharpCodeViewer.SetCodeFont(font.Name, font.Size);
        }

        bool IDesignerControl.IsDirty
        {
            get
            {
                var tp = Parent as TabPage;
                if (tp == null)
                    return true;
                return tp.Text.EndsWith("*");
            }
            set
            {
                var tp = Parent as TabPage;
                if (tp == null)
                    return;
                if (value)
                {
                    if (!tp.Text.EndsWith("*"))
                        tp.Text += "*";
                }
                else
                {
                    if (tp.Text.EndsWith("*"))
                        tp.Text = tp.Text.Substring(0, tp.Text.Length - 2);
                }
            }
        }

        bool IDesignerControl.IsDesignerActive { get => SelectedTab == tabPageDesigner; }

        #endregion IDesignerControl members
    }

    public interface IDesignerControl
    {
        event EventHandler<RequestSourceCodeArgs> RequestSourceCode;
        void AdoptViewAsChild(Control designSurfaceView);
        void ShowBusyIndicator(string text);
        void HideBusyIndicator();
        void SetRootComponentFont(Font font);
        void SetSourceCodeViewerFont(Font font);
        bool IsDirty { get; set; }
        bool IsDesignerActive { get; }
    }

    public class RequestSourceCodeArgs : EventArgs
    {
        public string SourceCode { get; set; }
        public IEnumerable<string> ComponentTypeNames { get; set; }
        public Action<string> StatusUpdater { get; set; }
    }
}
