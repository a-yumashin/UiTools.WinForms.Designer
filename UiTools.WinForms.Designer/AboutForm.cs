using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using UiTools.WinForms.Designer.Core;

namespace UiTools.WinForms.Designer
{
    public partial class AboutForm : ThemedForm
    {
        private static readonly Color DefaultAppNameColor = Color.Indigo;
        
        public AboutForm(string vsixVersion)
        {
            InitializeComponent();

            labProductName.Tag = "NoTheme";
            labProductName.Text = Application.ProductName;
            labProductName.ForeColor = DefaultAppNameColor;

            lnkLicense.Text = "License: MIT";
            var linkStartPos = lnkLicense.Text.IndexOf("MIT");
            lnkLicense.Links.Add(new LinkLabel.Link(
                linkStartPos, lnkLicense.Text.Length - linkStartPos) { Description = "view license text" });
            lnkLicense.LinkClicked += lnkLicense_LinkClicked;

            var attr = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyCopyrightAttribute>();
            lnkCopyrightInfo.Text = attr == null
                ? "Copyright © 2026 Alexey Yumashin"
                : attr.Copyright;
            linkStartPos = lnkCopyrightInfo.Text.IndexOf("Alexey");
            lnkCopyrightInfo.Links.Add(new LinkLabel.Link(
                linkStartPos, lnkCopyrightInfo.Text.Length - linkStartPos,
                $"mailto:a.yumashin@yandex.ru?subject={Application.ProductName}") { Description = "contact author" });
            lnkCopyrightInfo.LinkClicked += (s, e) => System.Diagnostics.Process.Start(e.Link.LinkData.ToString());

            lnkSourcesRepo.Text = "Sources: GitHub repo";
            linkStartPos = lnkSourcesRepo.Text.IndexOf("GitHub");
            lnkSourcesRepo.Links.Add(new LinkLabel.Link(
                linkStartPos, lnkSourcesRepo.Text.Length - linkStartPos,
                "https://github.com/a-yumashin/UiTools.WinForms.Designer") { Description = "open in browser" });
            lnkSourcesRepo.LinkClicked += (s, e) => System.Diagnostics.Process.Start(e.Link.LinkData.ToString());

            labRunMode.Text = vsixVersion == null
                ? "running as standalone app"
                : "running as VS Code extension";
            txtExePath.AutoSize = false;
            float scaleFactor = DeviceDpi / 120f;
            txtExePath.Height = (int)(22 * scaleFactor);
            txtExePath.Text = Assembly.GetExecutingAssembly().Location;
            labExeVersion.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            if (vsixVersion != null)
            {
                labVsixVersionCaption.Visible = labVsixVersion.Visible = true;
                labVsixVersion.Text = vsixVersion;
            }
            CenterToParent(); // center early to prevent visual flickering during the population of controls
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            AdjustLayout();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            AdjustLayout();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            AdjustLayout();
        }

        private void AdjustLayout()
        {
            lnkLicense.Left = ClientSize.Width - lnkLicense.Width - 8;
            lnkCopyrightInfo.Left = ClientSize.Width - lnkCopyrightInfo.Width - 8;
            lnkSourcesRepo.Left = ClientSize.Width - lnkSourcesRepo.Width - 8;
            labExeVersion.Left = txtExePath.Left = labRunMode.Left = labExeVersionCaption.Right + 6;
            txtExePath.Left = labExeVersionCaption.Right + 11;
            txtExePath.Width = ClientSize.Width - txtExePath.Left - 14;
            labVsixVersion.Left = labVsixVersionCaption.Right + 6;
        }

        [Category("Appearance")]
        public Color AppNameColor
        {
            get => labProductName.ForeColor;
            set => labProductName.ForeColor = value;
        }

        #region Support for default values of Color properties

        private bool ShouldSerializeAppNameColor() => AppNameColor != DefaultAppNameColor;
        private void ResetAppNameColor() => AppNameColor = DefaultAppNameColor;

        #endregion Support for default values of Color properties

        private void lnkLicense_LinkClicked(object sender, LinkLabelLinkClickedEventArgs args)
        {
            float scaleFactor = DeviceDpi / 120f;
            var licenseForm = new ThemedForm
            {
                AutoScaleMode = AutoScaleMode.Dpi,
                Text = "License Information",
                Size = new Size((int)(750 * scaleFactor), (int)(600 * scaleFactor)),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                KeyPreview = true
            };
            licenseForm.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) licenseForm.Close(); };
            var textBox = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                Dock = DockStyle.Fill,
                ScrollBars = ScrollBars.Vertical,
                Text = GetLicenseText(),
                Font = new Font("Consolas", 9),
                SelectionStart = 0
            };
            licenseForm.Controls.Add(textBox);
            licenseForm.UiThemeApplied += (s, e) =>
            {
                textBox.Font = new Font("Consolas", 9); // restore monospace font because it could be changed by ThemeApplier
                licenseForm.BeginInvoke(() => ThemeApplier.ApplyScrollBarTheme(textBox, ThemeApplier.IsDark(licenseForm.BackColor)));
            };
            var mi = licenseForm.GetType().GetMethod("CenterToParent", BindingFlags.Instance | BindingFlags.NonPublic);
            if (mi != null)
                mi.Invoke(licenseForm, null); // center early to prevent visual flickering during the population of controls
            licenseForm.ShowDialog(this);
            licenseForm.Dispose();
        }

        private string GetLicenseText()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = $"{assembly.GetName().Name}.LICENSE.txt";
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    return "License file not found.";
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
