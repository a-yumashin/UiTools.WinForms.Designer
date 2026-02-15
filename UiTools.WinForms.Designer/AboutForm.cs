using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace UiTools.WinForms.Designer
{
    public partial class AboutForm : Form
    {
        public AboutForm(string vsixVersion)
        {
            InitializeComponent();

            labProductName.Text = Application.ProductName;
            lnkLicense.Text = "License: MIT";
            lnkLicense.Left = ClientSize.Width - lnkLicense.Width - 8;
            var linkStartPos = lnkLicense.Text.IndexOf("MIT");
            lnkLicense.Links.Add(new LinkLabel.Link(
                linkStartPos, lnkLicense.Text.Length - linkStartPos) { Description = "view license text" });
            lnkLicense.LinkClicked += lnkLicense_LinkClicked;

            var attr = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyCopyrightAttribute>();
            lnkCopyrightInfo.Text = attr == null
                ? "Copyright © 2026 Alexey Yumashin"
                : attr.Copyright;
            lnkCopyrightInfo.Left = ClientSize.Width - lnkCopyrightInfo.Width - 8;
            linkStartPos = lnkCopyrightInfo.Text.IndexOf("Alexey");
            lnkCopyrightInfo.Links.Add(new LinkLabel.Link(
                linkStartPos, lnkCopyrightInfo.Text.Length - linkStartPos,
                $"mailto:a.yumashin@yandex.ru?subject={Application.ProductName}") { Description = "contact author" });
            lnkCopyrightInfo.LinkClicked += (s, e) => System.Diagnostics.Process.Start(e.Link.LinkData.ToString());

            labRunMode.Text = vsixVersion == null
                ? "running as standalone app"
                : "running as VS Code extension";
            txtExePath.Text = Assembly.GetExecutingAssembly().Location;
            labExeVersion.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            if (vsixVersion != null)
            {
                labVsixVersionCaption.Visible = labVsixVersion.Visible = true;
                labVsixVersion.Text = vsixVersion;
            }
        }

        private void lnkLicense_LinkClicked(object sender, LinkLabelLinkClickedEventArgs args)
        {
            var licenseForm = new Form
            {
                Text = "License Information",
                Size = new Size(750, 600),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                KeyPreview = true
            };
            licenseForm.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) licenseForm.Close(); };
            licenseForm.Controls.Add(new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                Dock = DockStyle.Fill,
                ScrollBars = ScrollBars.Vertical,
                Text = GetLicenseText(),
                Font = new Font("Consolas", 9),
                SelectionStart = 0
            });
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
