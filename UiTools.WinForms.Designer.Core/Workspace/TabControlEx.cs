using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using UiTools.WinForms.Designer.Core.Properties;

namespace UiTools.WinForms.Designer.Core
{
    public class TabControlEx : TabControl
    {
        private static readonly Color DefaultTabHeaderBackColor = SystemColors.Control;
        private static readonly Color DefaultTabHeaderSelectedBackColor = ColorTranslator.FromHtml("#006CBE");
        private static readonly Color DefaultTabHeaderForeColor = SystemColors.ControlText;
        private static readonly Color DefaultTabHeaderSelectedForeColor = ColorTranslator.FromHtml("#FAFAFA");
        
        public event EventHandler<int> TabPageCloseRequested;

        protected Font regularFont = null;
        private Color backColor = Control.DefaultBackColor;
        private readonly ToolTip closeToolTip;
        private int lastHoveredTabIdx = -1;
        private bool isOverCloseButton = false;
        private readonly bool isInDesignMode;

        public TabControlEx()
        {
            isInDesignMode = LicenseManager.UsageMode == LicenseUsageMode.Designtime; // this check is reliable only in constructor, so we store its result
            if (!isInDesignMode) // otherwise we may get OOM in design mode
                DrawMode = TabDrawMode.OwnerDrawFixed;
            Padding = new Point(21, 3);
            ShowToolTips = false;
            closeToolTip = new ToolTip();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            if (regularFont == null)
                regularFont = new Font(Font, FontStyle.Regular);
            // Force the TabControl to calculate header widths based on bold text, ensuring enough space for active tab titles:
            Font = new Font(Font, FontStyle.Bold);
            base.OnHandleCreated(e);
            float scaleFactor = DeviceDpi / 120f;
            ItemSize = new Size(120, (int)(30 * scaleFactor));
            // Restore regular font on child controls:
            RestoreRegularFont(Controls.Cast<Control>().ToList(), "OnHandleCreated");
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            regularFont?.Dispose();
            closeToolTip?.Dispose();
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            // Restore regular font:
            RestoreRegularFont(Enumerable.Repeat(e.Control, 1).ToList(), "OnControlAdded");
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            regularFont = new Font(Font, FontStyle.Regular);
            // Force the TabControl to calculate header widths based on bold text, ensuring enough space for active tab titles:
            Font = new Font(Font, FontStyle.Bold);
            // Restore regular font on child controls:
            RestoreRegularFont(Controls.Cast<Control>().ToList(), "OnFontChanged");
        }

        private void RestoreRegularFont(List<Control> controls, string callerName)
        {
            //var msg = $"➽ {callerName} (BEFORE): Type = '{GetType().Name}', Font.Bold = {Font.Bold}, ";
            //msg += controls.Any()
            //    ? $"Controls[0].Text = {(controls.Count > 0 ? controls[0].Text : "n/a")}, Controls[0].Font.Bold = {(controls.Count > 0 ? controls[0].Font.Bold : "n/a")}"
            //    : "no child controls";
            //Debug.WriteLine(msg);

            controls.ForEach(c => c.Font = regularFont);

            //if (controls.Any())
            //{
            //    msg = $"  ➽ {callerName} (AFTER): Type = '{GetType().Name}', Font.Bold = {Font.Bold}, ";
            //    msg += $"Controls[0].Text = {(controls.Count > 0 ? controls[0].Text : "n/a")}, Controls[0].Font.Bold = {(controls.Count > 0 ? controls[0].Font.Bold : "n/a")}";
            //    Debug.WriteLine(msg);
            //}
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= TabPages.Count)
                return;

            var currentPage = TabPages[e.Index];
            var isSelected = e.State == DrawItemState.Selected;
            var tabsOnBottom = Alignment == TabAlignment.Bottom; // only Top and Bottom are supported
            var tabRect = e.Bounds;

            using (var brush = new SolidBrush(isSelected ? TabHeaderSelectedBackColor : TabHeaderBackColor))
                e.Graphics.FillRectangle(brush, tabRect);

            var dx = isSelected ? 2 : 0;
            var dy = isSelected ? -1 : (tabsOnBottom ? -2 : 1);
            TextRenderer.DrawText(e.Graphics,
                currentPage.Text,
                isSelected ? e.Font : regularFont,
                new Rectangle(tabRect.X + 3 + dx, tabRect.Y + dy, tabRect.Width - 3 - dx, tabRect.Height),
                isSelected ? TabHeaderSelectedForeColor : TabHeaderForeColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

            bool isDarkTheme = CommonStuff.CurrentUiTheme == null ? false : ThemeApplier.IsDark(BackColor);
            var imgClose = isDarkTheme
                ? Resources.CloseSelectedTab
                : (isSelected ? Resources.CloseSelectedTab : Resources.CloseTab);
            e.Graphics.DrawImage(imgClose, CalculateCloseIconRectangle(tabRect, isSelected));
        }

        private Rectangle CalculateCloseIconRectangle(Rectangle tabRect, bool isSelected)
        {
            var tabsOnBottom = Alignment == TabAlignment.Bottom; // only Top and Bottom are supported
            float scaleFactor = DeviceDpi / 120f;
            var w = (int)(Resources.CloseTab.Width * scaleFactor);
            var dx = (int)(-4 * scaleFactor) + (isSelected ? 0 : 4); // tested with scaleFactor 125% and 200% (the latter - on 4K display)
            var dy = isSelected ? 1 : (tabsOnBottom ? 0 : 2);
            return new Rectangle(tabRect.Right - w - 2 + dx, tabRect.Top + (tabRect.Height - w) / 2 + dy, w, w);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            for (int i = 0; i < TabPages.Count; i++)
            {
                var closeRect = CalculateCloseIconRectangle(GetTabRect(i), SelectedIndex == i);
                if (closeRect.Contains(e.Location))
                {
                    TabPageCloseRequested?.Invoke(this, i);
                    break;
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            int hoveredIdx = -1;
            bool overCloseNow = false;

            for (int i = 0; i < TabPages.Count; i++)
            {
                var tabRect = GetTabRect(i);
                if (tabRect.Contains(e.Location))
                {
                    hoveredIdx = i;
                    var closeRect = CalculateCloseIconRectangle(tabRect, SelectedIndex == i);
                    if (closeRect.Contains(e.Location))
                    {
                        overCloseNow = true;
                    }
                    break;
                }
            }

            string targetTooltip = null;
            if (hoveredIdx != -1)
            {
                targetTooltip = overCloseNow ? "close" : TabPages[hoveredIdx].ToolTipText;
            }
            Cursor targetCursor = overCloseNow ? Cursors.Hand : Cursors.Default;

            if (Cursor != targetCursor)
                Cursor = targetCursor;

            if (lastHoveredTabIdx != hoveredIdx || isOverCloseButton != overCloseNow)
            {
                if (targetTooltip != closeToolTip.GetToolTip(this))
                {
                    closeToolTip.RemoveAll();
                    closeToolTip.SetToolTip(this, targetTooltip);
                }
                lastHoveredTabIdx = hoveredIdx;
                isOverCloseButton = overCloseNow;
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (Cursor != Cursors.Default)
                Cursor = Cursors.Default;
            closeToolTip.RemoveAll();
            isOverCloseButton = false;
            lastHoveredTabIdx = -1;
        }

        protected override void OnClick(EventArgs e)
        {
            SelectedTab?.Focus();
        }

        protected override void OnEnter(EventArgs e)
        {
            SelectedTab?.Focus();
        }

        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public override Color BackColor
        {
            // NOTE: have to override this property because the base control property (TabControl.BackColor) has empty setter and
            //       getter always returning SystemColors.Control, with comment "This member is not meaningful for this control".
            get => backColor;
            set
            {
                backColor = value;
                TryApplyThemeToTabScroller(backColor); // could be done in IsDarkTheme property setter as well
            }
        }

        [Category("Appearance")]
        public Color TabHeaderBackColor { get; set; } = DefaultTabHeaderBackColor;

        [Category("Appearance")]
        public Color TabHeaderSelectedBackColor { get; set; } = DefaultTabHeaderSelectedBackColor;

        [Category("Appearance")]
        public Color TabHeaderForeColor { get; set; } = DefaultTabHeaderForeColor;

        [Category("Appearance")]
        public Color TabHeaderSelectedForeColor { get; set; } = DefaultTabHeaderSelectedForeColor;

        protected override void WndProc(ref Message m)
        {
            if (isInDesignMode)
            {
                base.WndProc(ref m);
                return;
            }
            if (Cursor == Cursors.Hand && m.Msg == Win32.WM_SETCURSOR)
            {
                // Fix ugly cursor
                Win32.SetCursor(Win32.LoadCursor(IntPtr.Zero, Win32.IDC_HAND));
                m.Result = IntPtr.Zero;
                return;
            }
            else if (m.Msg == Win32.WM_ERASEBKGND)
            {
                using (var g = Graphics.FromHdc(m.WParam))
                {
                    g.Clear(BackColor);
                }
                m.Result = (IntPtr)1;
                return;
            }
            else if (m.Msg == Win32.WM_PARENTNOTIFY && (m.WParam.ToInt32() & 0xFFFF) == Win32.WM_CREATE)
            {
                if (CommonStuff.CurrentUiTheme != null)
                    TryApplyThemeToTabScroller(BackColor);
            }

            base.WndProc(ref m);

            if (m.Msg == Win32.WM_PAINT)
            {
                using (Graphics g = CreateGraphics())
                {
                    // Remove 3D-border
                    DrawControlBorder(g);
                }
            }
        }

        private void TryApplyThemeToTabScroller(Color backColor)
        {
            IntPtr hwndUpDown = Win32.FindWindowEx(Handle, IntPtr.Zero, "msctls_updown32", null);
            if (hwndUpDown != IntPtr.Zero)
                ThemeApplier.ApplyScrollBarTheme(hwndUpDown, ThemeApplier.IsDark(backColor));
        }

        private void DrawControlBorder(Graphics g)
        {
            // NOTE: While this method may cause flickering, using WS_EX_COMPOSITED to prevent it is not feasible here.
            //       That style causes UI glitches if the DesignSurface contains a ListView control.
            //       Opting for slight flickering over UI corruption was the lesser of two evils.
            if (Width <= 0 || Height <= 0)
                return;

            using (Pen p = new Pen(BackColor, 2))
            {
                g.DrawRectangle(p, 0, 0, Width, Height);

                Rectangle displayRect = DisplayRectangle;
                displayRect.Inflate(2, 1);
                displayRect.Height += 1;
                g.DrawRectangle(p, displayRect);
            }
        }

        #region Support for default values of Color properties

        private bool ShouldSerializeTabHeaderBackColor() => TabHeaderBackColor != DefaultTabHeaderBackColor;
        private void ResetTabHeaderBackColor() => TabHeaderBackColor = DefaultTabHeaderBackColor;

        private bool ShouldSerializeTabHeaderSelectedBackColor() => TabHeaderSelectedBackColor != DefaultTabHeaderSelectedBackColor;
        private void ResetTabHeaderSelectedBackColor() => TabHeaderSelectedBackColor = DefaultTabHeaderSelectedBackColor;

        private bool ShouldSerializeTabHeaderForeColor() => TabHeaderForeColor != DefaultTabHeaderForeColor;
        private void ResetTabHeaderForeColor() => TabHeaderForeColor = DefaultTabHeaderForeColor;

        private bool ShouldSerializeTabHeaderSelectedForeColor() => TabHeaderSelectedForeColor != DefaultTabHeaderSelectedForeColor;
        private void ResetTabHeaderSelectedForeColor() => TabHeaderSelectedForeColor = DefaultTabHeaderSelectedForeColor;

        #endregion Support for default values of Color properties
    }
}
