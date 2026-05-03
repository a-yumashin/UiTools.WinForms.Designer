using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace UiTools.WinForms.Designer.Core
{
    public class MyDesignerControl : TabControlEx, IDesignerControl
    {
        private static readonly Color DefaultComponentTrayBackColor = SystemColors.ControlLight;
        private static readonly Color DefaultComponentTrayForeColor = SystemColors.ControlText;
        private static readonly Color DefaultSourceCodePanelHeaderBackColor = SystemColors.ControlLight;

        private EventHandler<RequestSourceCodeArgs> requestSourceCodeEvent;
        private Control designSurfaceView;
        private Color componentTrayBackColor = DefaultComponentTrayBackColor;
        private Color componentTrayForeColor = DefaultComponentTrayForeColor;
        private bool isDarkTheme = false;
        private Func<Type, Bitmap> componentTypeIconResolver;

        private readonly TabPage tabPageDesigner = new TabPage { Text = "Designer", Padding = System.Windows.Forms.Padding.Empty };
        private readonly TabPage tabPageSourceCode = new TabPage { Text = "Source Code", Padding = System.Windows.Forms.Padding.Empty };

        private readonly Panel pnlSourceCode = new Panel { Dock = DockStyle.Fill };
        private readonly Panel pnlSourceCodeHeader = new Panel
        {
            Dock = DockStyle.Top,
            BackColor = DefaultSourceCodePanelHeaderBackColor
        };
        private readonly LinkLabelEx lnkGenerateSourceCode = new LinkLabelEx
        {
            Text = "Generate source code",
            AutoSize = true,
            Left = 5,
        };
        private readonly LinkLabelEx lnkSearchInSourceCode = new LinkLabelEx
        {
            Text = "Search in source code",
            AutoSize = true,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
        };
        private readonly Label labSourceCodeStatus = new Label
        {
            AutoSize = true,
            Left = 5
        };
        private readonly CheckBox chkWordWrap = new CheckBox
        {
            Text = "Word Wrap",
            AutoSize = true,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
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

            float scaleFactor = DeviceDpi / 120f;
            pnlSourceCodeHeader.Height = (int)(40 * scaleFactor);
            pnlSourceCodeHeader.Controls.Add(lnkGenerateSourceCode);
            pnlSourceCodeHeader.Controls.Add(lnkSearchInSourceCode);
            pnlSourceCodeHeader.Controls.Add(chkWordWrap);

            chkWordWrap.Top = (int)(9 * scaleFactor);
            chkWordWrap.Checked = cSharpCodeViewer.WordWrap;
            chkWordWrap.CheckedChanged += (s, e) => cSharpCodeViewer.WordWrap = chkWordWrap.Checked;

            pnlSourceCode.Controls.Add(pnlSourceCodeHeader);
            pnlSourceCode.Controls.Add(labSourceCodeStatus);
            pnlSourceCode.Controls.Add(cSharpCodeViewer as CSharpCodeViewer);
            cSharpCodeViewer.BringToFront(); // otherwise pnlSourceCodeHeader overlaps the top part of cSharpCodeViewer
            labSourceCodeStatus.Top = pnlSourceCodeHeader.Height + (int)(10 * scaleFactor);

            tabPageSourceCode.Controls.Add(pnlSourceCode);
            Controls.AddRange(new[] { tabPageDesigner, tabPageSourceCode });

            lnkGenerateSourceCode.Top = (int)(9 * scaleFactor);
            lnkGenerateSourceCode.LinkClicked += lnkGenerateSourceCode_LinkClicked;
            lnkSearchInSourceCode.Top = (int)(9 * scaleFactor);
            lnkSearchInSourceCode.LinkClicked += (s, e) => cSharpCodeViewer.ShowSearchDialog();

            lnkSearchInSourceCode.Links[0].Enabled = false;

            CreateBusyIndicator();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            // Update ComponentTray font:
            var tray = FindComponentTray();
            if (tray != null)
                tray.Font = base.regularFont;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            AdjustLayout();
        }

        private void AdjustLayout()
        {
            chkWordWrap.Left = pnlSourceCodeHeader.ClientSize.Width - chkWordWrap.Width - 5;
            lnkSearchInSourceCode.Left = chkWordWrap.Left - lnkSearchInSourceCode.Width - 10;
        }

        private void lnkGenerateSourceCode_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            cSharpCodeViewer.Visible = false; // otherwise WebView2 control (the only one in CSharpCodeViewer) changes cursor at will
            lnkGenerateSourceCode.Links[0].Enabled = false;
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
                    lnkGenerateSourceCode.Links[0].Enabled = true;
                }
            }
        }

        private void OnCodeRenderComplete(object sender, EventArgs e)
        {
            labSourceCodeStatus.Text = "";
            cSharpCodeViewer.Visible = true;
            lnkGenerateSourceCode.Links[0].Enabled = true;
            lnkSearchInSourceCode.Links[0].Enabled = true;
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

        private void CustomizeOverlayControl()
        {
            if (designSurfaceView == null)
                return;
            var overlayControl = FindOverlayControl();
            if (overlayControl != null)
                ThemeApplier.ApplyScrollBarTheme(overlayControl, ThemeApplier.IsDark(BackColor));
        }

        private Control FindOverlayControl() => designSurfaceView?.FindControlByType("System.Windows.Forms.Design.DesignerFrame+OverlayControl");
        private Control FindComponentTray() => designSurfaceView?.FindControlByType("System.Windows.Forms.Design.ComponentTray");
        private Control FindComponentTraySplitter() => designSurfaceView?.FindControlByType("System.Windows.Forms.Splitter");

        private void CustomizeComponentTray()
        {
            if (designSurfaceView == null)
                return;
            var tray = FindComponentTray();
            if (tray == null)
            {
                designSurfaceView.ControlAdded -= OnViewControlAdded;
                designSurfaceView.ControlAdded += OnViewControlAdded;
            }
            else
            {
                if (CommonStuff.CurrentUiTheme != null)
                    ApplyTrayStyles(tray);
                tray.Font = base.regularFont;
                float scaleFactor = DeviceDpi / 120f;
                tray.Height = (int)(50 * scaleFactor);
                (tray as System.Windows.Forms.Design.ComponentTray).AutoArrange = true; // but it can always be turned off via context menu
            }
        }

        private void OnViewControlAdded(object sender, ControlEventArgs e)
        {
            if (e.Control.GetType().FullName == "System.Windows.Forms.Design.ComponentTray")
                ApplyTrayStyles(e.Control);
        }

        private void ApplyTrayStyles(Control tray)
        {
            // it was always annoying that the ComponentTray blends with the main DesignSurface area
            // (where the Form or UserControl is displayed), so we highlight the ComponentTray with a different color:
            if (tray.BackColor != ComponentTrayBackColor)
            {
                tray.BackColor = ComponentTrayBackColor;
                var splitter = FindComponentTraySplitter();
                if (splitter != null)
                    splitter.BackColor = tray.BackColor;
            }
            if (tray.ForeColor != ComponentTrayForeColor)
                tray.ForeColor = ComponentTrayForeColor;
        }

        private void ApplyUiThemeToSrcHeaderArea()
        {
            pnlSourceCodeHeader.BackColor = SourceCodePanelHeaderBackColor;
            lnkGenerateSourceCode.BackColor = SourceCodePanelHeaderBackColor;
            lnkSearchInSourceCode.BackColor = SourceCodePanelHeaderBackColor;
            chkWordWrap.BackColor = SourceCodePanelHeaderBackColor;
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
                    BeginInvoke(AdjustLayout);
                    CustomizeOverlayControl();
                    ApplyUiThemeToSrcHeaderArea();
                    (this as IDesignerControl).SyncComponentTrayIcons();
                }
            }
        }

        [Category("Appearance")]
        public Color ComponentTrayBackColor
        {
            get => componentTrayBackColor;
            set
            {
                if (componentTrayBackColor != value)
                {
                    componentTrayBackColor = value;
                    var tray = FindComponentTray();
                    if (tray != null)
                    {
                        tray.BackColor = componentTrayBackColor;
                        var splitter = FindComponentTraySplitter();
                        if (splitter != null)
                            splitter.BackColor = tray.BackColor; // should have same BackColor
                    }
                }
            }
        }

        [Category("Appearance")]
        public Color ComponentTrayForeColor
        {
            get => componentTrayForeColor;
            set
            {
                if (componentTrayForeColor != value)
                {
                    componentTrayForeColor = value;
                    var tray = FindComponentTray();
                    if (tray != null)
                        tray.ForeColor = componentTrayForeColor;
                }
            }
        }

        [Category("Appearance")]
        public Color SourceCodePanelHeaderBackColor { get; set; } = DefaultSourceCodePanelHeaderBackColor;

        #region Support for default values of Color properties

        private bool ShouldSerializeComponentTrayBackColor() => ComponentTrayBackColor != DefaultComponentTrayBackColor;
        private void ResetComponentTrayBackColor() => ComponentTrayBackColor = DefaultComponentTrayBackColor;

        private bool ShouldSerializeComponentTrayForeColor() => ComponentTrayForeColor != DefaultComponentTrayForeColor;
        private void ResetComponentTrayForeColor() => ComponentTrayForeColor = DefaultComponentTrayForeColor;

        private bool ShouldSerializeSourceCodePanelHeaderBackColor() => SourceCodePanelHeaderBackColor != DefaultSourceCodePanelHeaderBackColor;
        private void ResetSourceCodePanelHeaderBackColor() => SourceCodePanelHeaderBackColor = DefaultSourceCodePanelHeaderBackColor;

        #endregion Support for default values of Color properties

        #region IDesignerControl members

        event EventHandler<RequestSourceCodeArgs> IDesignerControl.RequestSourceCode
        {
            add { requestSourceCodeEvent += value; }
            remove { requestSourceCodeEvent -= value; }
        }

        void IDesignerControl.AdoptViewAsChild(Control designSurfaceView)
        {
            this.designSurfaceView = designSurfaceView;
            designSurfaceView.Font = rootComponentFont;
            tabPageDesigner.Controls.Add(designSurfaceView);
            CustomizeComponentTray();
            if (CommonStuff.CurrentUiTheme != null)
                CustomizeOverlayControl();
            (this as IDesignerControl).SyncComponentTrayIcons();
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

        void IDesignerControl.SetComponentTypeIconResolver(Func<Type, Bitmap> componentTypeIconResolver)
        {
            this.componentTypeIconResolver = componentTypeIconResolver;
        }

        void IDesignerControl.SyncComponentTrayIcons(IComponent onlyForGivenComponent)
        {
            // Syncs ComponentTray icons with those used in the 'Toolbox' panel.
            if (designSurfaceView == null)
                return;
            var tray = FindComponentTray();
            if (tray == null)
                return;
            var trayControl = tray.FindControlByType("System.Windows.Forms.Design.ComponentTray+TrayControl");
            var trayControlType = trayControl?.GetType();
            if (trayControlType == null)
                return;
            var fiComponent = trayControlType.GetField("component", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fiComponent == null)
                return;
            foreach (Control trayItem in tray.Controls)
            {
                var underlyingComponent = fiComponent.GetValue(trayItem);
                if (onlyForGivenComponent != null && !onlyForGivenComponent.Equals(underlyingComponent))
                    continue;
                var componentType = underlyingComponent?.GetType();
                if (componentType != null)
                {
                    Bitmap img = componentTypeIconResolver(componentType);
                    if (img != null)
                    {
                        // Substitute image with the "themed" one (used in the 'Toolbox' panel):
                        trayItem.GetType()
                            .GetField("toolboxBitmap", BindingFlags.NonPublic | BindingFlags.Instance)?
                            .SetValue(trayItem, img);
                        trayItem.Refresh();
                    }
                }
            }
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
        void SetComponentTypeIconResolver(Func<Type, Bitmap> componentTypeIconResolver);
        void SyncComponentTrayIcons(IComponent onlyForGivenComponent = null);
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
