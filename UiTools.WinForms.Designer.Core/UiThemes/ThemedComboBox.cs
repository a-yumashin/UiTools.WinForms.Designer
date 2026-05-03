using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace UiTools.WinForms.Designer.Core
{
    public class ThemedComboBox : ComboBox
    {
        private static readonly Color DefaultDisabledBackColor = SystemColors.Control;
        private static readonly Color DefaultDisabledForeColor = SystemColors.GrayText;
        private static readonly Color DefaultBorderColor = SystemColors.WindowFrame;
        private static readonly Color DefaultFocusedBackColor = SystemColors.Highlight;
        private static readonly Color DefaultFocusedForeColor = SystemColors.HighlightText;

        private static readonly Color DefaultArrowButtonBackColor = SystemColors.Control;
        private static readonly Color DefaultArrowButtonHoverBackColor = ColorTranslator.FromHtml("#C9DEF5");
        private static readonly Color DefaultArrowButtonDisabledBackColor = SystemColors.Control;
        private static readonly Color DefaultArrowButtonBorderColor = Color.FromArgb(0, 120, 212);
        private static readonly Color DefaultArrowColor = Color.Black;
        private static readonly Color DefaultArrowDisabledColor = ColorTranslator.FromHtml("#B8B8B8");

        private static readonly Color DefaultCueBannerForeColor = Color.Gray;

        private CueEditSubclass cueEditSubclass;
        private bool isMouseOverButton = false;
        private string cueBannerText;
        private Color cueBannerForeColor = DefaultCueBannerForeColor;
        protected const int LEFT_PADDING = 4;

        // Common:
        [Category("Appearance")]
        public Color DisabledBackColor { get; set; } = DefaultDisabledBackColor;

        [Category("Appearance")]
        public Color DisabledForeColor { get; set; } = DefaultDisabledForeColor;

        [Category("Appearance")]
        public Color BorderColor { get; set; } = DefaultBorderColor;

        [Category("Appearance")]
        public Color FocusedBackColor { get; set; } = DefaultFocusedBackColor;

        [Category("Appearance")]
        public Color FocusedForeColor { get; set; } = DefaultFocusedForeColor;

        // Button with an arrow:
        [Category("Appearance")]
        public Color ArrowButtonBackColor { get; set; } = DefaultArrowButtonBackColor;

        [Category("Appearance")]
        public Color ArrowButtonHoverBackColor { get; set; } = DefaultArrowButtonHoverBackColor;

        [Category("Appearance")]
        public Color ArrowButtonDisabledBackColor { get; set; } = DefaultArrowButtonDisabledBackColor;

        [Category("Appearance")]
        public Color ArrowButtonBorderColor { get; set; } = DefaultArrowButtonBorderColor;

        [Category("Appearance")]
        public Color ArrowColor { get; set; } = DefaultArrowColor;

        [Category("Appearance")]
        public Color ArrowDisabledColor { get; set; } = DefaultArrowDisabledColor;

        [Category("Appearance")]
        public string CueBannerText
        {
            get => cueBannerText;
            set
            {
                cueBannerText = value;
                if (DropDownStyle == ComboBoxStyle.DropDownList)
                    Invalidate();
                else
                    cueEditSubclass?.Invalidate();
            }
        }

        [Category("Appearance")]
        public Color CueBannerForeColor
        {
            get => cueBannerForeColor;
            set
            {
                cueBannerForeColor = value;
                if (DropDownStyle == ComboBoxStyle.DropDownList)
                    Invalidate();
                else
                    cueEditSubclass?.Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(false)]
        public bool IsDarkTheme { get; set; } = false;

        public ThemedComboBox()
        {
            DrawMode = DrawMode.OwnerDrawFixed;
        }

        #region Subclassing EDIT window

        // NOTE: we need to subclass the EDIT window to be able to paint "cue banner" when DropDownStyle == ComboBoxStyle.DropDown

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            BeginInvoke(AttachEditSubclass);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            DetachEditSubclass();
            base.OnHandleDestroyed(e);
        }

        private void AttachEditSubclass()
        {
            DetachEditSubclass();

            if (DropDownStyle == ComboBoxStyle.DropDown)
            {
                IntPtr editHwnd = Win32.FindWindowEx(Handle, IntPtr.Zero, "Edit", null);
                if (editHwnd != IntPtr.Zero)
                    cueEditSubclass = new CueEditSubclass(this, editHwnd);
            }
        }

        private void DetachEditSubclass()
        {
            if (cueEditSubclass != null)
            {
                cueEditSubclass.Dispose();
                cueEditSubclass = null;
            }
        }

        protected override void OnDropDownStyleChanged(EventArgs e)
        {
            base.OnDropDownStyleChanged(e);
            AttachEditSubclass();
        }

        #endregion Subclassing EDIT window

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Invalidate();
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case Win32.WM_ERASEBKGND:
                    // Ignore this message to prevent "white flashes"
                    m.Result = (IntPtr)1;
                    break;

                case Win32.WM_MOUSEMOVE:
                case Win32.WM_MOUSELEAVE:
                    base.WndProc(ref m);

                    int btnWidth = SystemInformation.VerticalScrollBarWidth;
                    Rectangle btnRect = new Rectangle(Width - btnWidth, 0, btnWidth, Height);

                    Point clientMousePos = PointToClient(Cursor.Position);
                    bool isOverNow = btnRect.Contains(clientMousePos) && m.Msg != Win32.WM_MOUSELEAVE;

                    if (isOverNow != isMouseOverButton)
                    {
                        isMouseOverButton = isOverNow;
                        Invalidate(btnRect); // redraw only the button area
                    }
                    break;

                case Win32.WM_PAINT:
                    // Draw with buffer and WM_PRINTCLIENT to prevent flickering
                    DrawWithBuffer();
                    m.Result = IntPtr.Zero; // because we've painted everything ourselves
                    break;

                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        /// <summary>
        /// Draws the focus highlight (selection background) in the TEXT AREA.
        /// </summary>
        /// <returns>The appropriate text color for the current UI Theme to ensure proper contrast against the focus background.</returns>
        protected virtual Color DrawFocusBackground(Graphics graphics)
        {
            int btnWidth = (DropDownStyle != ComboBoxStyle.Simple) ? SystemInformation.VerticalScrollBarWidth : 0;
            Color textColor = Enabled ? ForeColor : DisabledForeColor;
            if (Focused && Enabled)
            {
                var focusRect = new Rectangle(3, 3, ClientRectangle.Width - btnWidth - 6, ClientRectangle.Height - 6);
                using (var brush = new SolidBrush(FocusedBackColor))
                {
                    graphics.FillRectangle(brush, focusRect);
                }
                textColor = FocusedForeColor;
            }
            return textColor;
        }

        /// <summary>
        /// Draws text of the selected item in the TEXT AREA.
        /// </summary>
        protected virtual void DrawText(Graphics graphics)
        {
            // Draw text only when DropDownList style is set; DropDown style is handled by CueEditSubclass.
            if (DropDownStyle == ComboBoxStyle.DropDownList && !string.IsNullOrEmpty(Text))
            {
                int btnWidth = (DropDownStyle != ComboBoxStyle.Simple) ? SystemInformation.VerticalScrollBarWidth : 0;
                Color textColor = DrawFocusBackground(graphics);
                var textRect = new Rectangle(4, 0, ClientRectangle.Width - btnWidth - 5, ClientRectangle.Height);
                TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis;
                TextRenderer.DrawText(graphics, Text, Font, textRect, textColor, flags);
            }
        }

        /// <summary>
        /// Draws item in the DROPDOWN LIST.
        /// </summary>
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index >= 0)
            {
                var text = Items[e.Index].ToString();
                var flags = TextFormatFlags.NoPadding | TextFormatFlags.Left;
                var bounds = e.Bounds;
                bounds.Offset(LEFT_PADDING, 0);
                TextRenderer.DrawText(e.Graphics, text, Font, bounds, Enabled ? e.ForeColor : DisabledForeColor, flags);
            }
        }

        /// <summary>
        /// Draws everything except the dropdown part.
        /// </summary>
        private void DrawWithBuffer()
        {
            if (IsDisposed || !IsHandleCreated)
                return;

            using (var g = CreateGraphics())
            {
                Rectangle rect = ClientRectangle;
                using (BufferedGraphicsContext context = BufferedGraphicsManager.Current)
                using (BufferedGraphics buffer = context.Allocate(g, rect))
                {
                    // Draw ComboBox in buffer (background, border, text):
                    IntPtr hdc = buffer.Graphics.GetHdc();
                    Win32.SendMessage(Handle, Win32.WM_PRINTCLIENT, hdc, (IntPtr)(Win32.PRF_CLIENT | Win32.PRF_ERASEBKGND | Win32.PRF_NONCLIENT));
                    buffer.Graphics.ReleaseHdc(hdc);

                    // Remove 2px white padding in the client area:
                    using (var brush = new SolidBrush(Enabled ? BackColor : DisabledBackColor))
                    {
                        buffer.Graphics.FillRectangle(brush, rect.X + 1, rect.Y + 1, rect.Width - 2, rect.Height - 2);
                    }

                    if (IsDarkTheme)
                    {
                        using (var pen = new Pen(BorderColor))
                        {
                            buffer.Graphics.DrawRectangle(pen, 0, 0, rect.Width - 1, rect.Height - 1);
                        }
                    }

                    // Draw arrow button ON TOP of all:
                    if (DropDownStyle != ComboBoxStyle.Simple)
                    {
                        DrawArrowButton(buffer.Graphics, rect);
                    }

                    // Draw text:
                    DrawText(buffer.Graphics);

                    // Draw cue banner text:
                    if (DropDownStyle == ComboBoxStyle.DropDownList &&
                        string.IsNullOrEmpty(Text) &&
                        !Focused &&
                        Enabled &&
                        !string.IsNullOrEmpty(CueBannerText))
                    {
                        DrawCueBanner(buffer.Graphics, rect);
                    }

                    // Render buffer to screen in one go:
                    buffer.Render(g);
                }
            }

            Win32.ValidateRect(Handle, IntPtr.Zero); // this will break the WM_PAINT loop
        }

        private void DrawCueBanner(Graphics graphics, Rectangle rect)
        {
            int btnWidth = (DropDownStyle != ComboBoxStyle.Simple) ? SystemInformation.VerticalScrollBarWidth : 0;
            Rectangle textRect = new Rectangle(4, 0, rect.Width - btnWidth - 5, rect.Height);

            TextFormatFlags flags = TextFormatFlags.Left |
                                    TextFormatFlags.VerticalCenter |
                                    TextFormatFlags.EndEllipsis |
                                    TextFormatFlags.NoPadding;

            TextRenderer.DrawText(graphics, CueBannerText, Font, textRect, CueBannerForeColor, flags);
        }

        private void DrawArrowButton(Graphics graphics, Rectangle rect)
        {
            int btnWidth = SystemInformation.VerticalScrollBarWidth;
            Rectangle btnRect = new Rectangle(rect.Width - btnWidth, 1, btnWidth - 1, rect.Height - 2);
            Color buttonColor = isMouseOverButton ? ArrowButtonHoverBackColor : ArrowButtonBackColor;
            using (var brush = new SolidBrush(Enabled ? buttonColor : ArrowButtonDisabledBackColor))
            {
                graphics.FillRectangle(brush, btnRect);
            }

            if (isMouseOverButton)
            {
                // Draw border
                using (var pen = new Pen(ArrowButtonBorderColor))
                {
                    graphics.DrawRectangle(pen, btnRect.X, 0, btnRect.Width, btnRect.Height);
                }
            }

            // Turn on HQ drawing:
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

            float centerX = btnRect.X + (btnRect.Width / 2f);
            float centerY = btnRect.Y + (btnRect.Height / 2f);

            using (var brush = new SolidBrush(Enabled ? ArrowColor : ArrowDisabledColor))
            {
                PointF[] arrowPoints = {
                    new PointF(centerX - 3.5f, centerY - 2f),
                    new PointF(centerX + 3.5f, centerY - 2f),
                    new PointF(centerX, centerY + 2f)
                };
                graphics.FillPolygon(brush, arrowPoints);
            }

            // Turn off HQ drawing:
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
        }

        protected override void OnDropDown(EventArgs e)
        {
            base.OnDropDown(e);

            var info = new Win32.COMBOBOXINFO();
            info.cbSize = Marshal.SizeOf(info);
            if (Win32.GetComboBoxInfo(Handle, ref info))
                ThemeApplier.ApplyScrollBarTheme(info.hwndList, IsDarkTheme);
        }

        #region Support for default values of Color properties

        private bool ShouldSerializeDisabledBackColor() => DisabledBackColor != DefaultDisabledBackColor;
        private void ResetDisabledBackColor() => DisabledBackColor = DefaultDisabledBackColor;

        private bool ShouldSerializeDisabledForeColor() => DisabledForeColor != DefaultDisabledForeColor;
        private void ResetDisabledForeColor() => DisabledForeColor = DefaultDisabledForeColor;

        private bool ShouldSerializeBorderColor() => BorderColor != DefaultBorderColor;
        private void ResetBorderColor() => BorderColor = DefaultBorderColor;

        private bool ShouldSerializeFocusedBackColor() => FocusedBackColor != DefaultFocusedBackColor;
        private void ResetFocusedBackColor() => FocusedBackColor = DefaultFocusedBackColor;

        private bool ShouldSerializeFocusedForeColor() => FocusedForeColor != DefaultFocusedForeColor;
        private void ResetFocusedForeColor() => FocusedForeColor = DefaultFocusedForeColor;

        private bool ShouldSerializeArrowButtonBackColor() => ArrowButtonBackColor != DefaultArrowButtonBackColor;
        private void ResetArrowButtonBackColor() => ArrowButtonBackColor = DefaultArrowButtonBackColor;

        private bool ShouldSerializeArrowButtonHoverBackColor() => ArrowButtonHoverBackColor != DefaultArrowButtonHoverBackColor;
        private void ResetArrowButtonHoverBackColor() => ArrowButtonHoverBackColor = DefaultArrowButtonHoverBackColor;

        private bool ShouldSerializeArrowButtonDisabledBackColor() => ArrowButtonDisabledBackColor != DefaultArrowButtonDisabledBackColor;
        private void ResetArrowButtonDisabledBackColor() => ArrowButtonDisabledBackColor = DefaultArrowButtonDisabledBackColor;

        private bool ShouldSerializeArrowButtonBorderColor() => ArrowButtonBorderColor != DefaultArrowButtonBorderColor;
        private void ResetArrowButtonBorderColor() => ArrowButtonBorderColor = DefaultArrowButtonBorderColor;

        private bool ShouldSerializeArrowColor() => ArrowColor != DefaultArrowColor;
        private void ResetArrowColor() => ArrowColor = DefaultArrowColor;

        private bool ShouldSerializeArrowDisabledColor() => ArrowDisabledColor != DefaultArrowDisabledColor;
        private void ResetArrowDisabledColor() => ArrowDisabledColor = DefaultArrowDisabledColor;

        private bool ShouldSerializeCueBannerForeColor() => CueBannerForeColor != DefaultCueBannerForeColor;
        private void ResetCueBannerForeColor() => CueBannerForeColor = DefaultCueBannerForeColor;

        #endregion Support for default values of Color properties
    }
}
