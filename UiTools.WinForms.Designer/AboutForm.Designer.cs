namespace UiTools.WinForms.Designer
{
    partial class AboutForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.labProductName = new System.Windows.Forms.Label();
            this.lnkLicense = new UiTools.WinForms.Designer.Core.LinkLabelEx();
            this.labExeVersionCaption = new System.Windows.Forms.Label();
            this.labExeVersion = new System.Windows.Forms.Label();
            this.labExePathCaption = new System.Windows.Forms.Label();
            this.labRunModeCaption = new System.Windows.Forms.Label();
            this.labRunMode = new System.Windows.Forms.Label();
            this.labVsixVersion = new System.Windows.Forms.Label();
            this.labVsixVersionCaption = new System.Windows.Forms.Label();
            this.lnkCopyrightInfo = new UiTools.WinForms.Designer.Core.LinkLabelEx();
            this.cmdOK = new UiTools.WinForms.Designer.Core.ThemedButton();
            this.txtExePath = new System.Windows.Forms.TextBox();
            this.lnkSourcesRepo = new UiTools.WinForms.Designer.Core.LinkLabelEx();
            this.SuspendLayout();
            // 
            // labProductName
            // 
            this.labProductName.AutoSize = true;
            this.labProductName.Font = new System.Drawing.Font("Segoe UI", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labProductName.ForeColor = System.Drawing.Color.Indigo;
            this.labProductName.Location = new System.Drawing.Point(2, 9);
            this.labProductName.Name = "labProductName";
            this.labProductName.Size = new System.Drawing.Size(232, 38);
            this.labProductName.TabIndex = 0;
            this.labProductName.Text = "<product_name>";
            // 
            // lnkLicense
            // 
            this.lnkLicense.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lnkLicense.AutoSize = true;
            this.lnkLicense.Location = new System.Drawing.Point(517, 53);
            this.lnkLicense.Name = "lnkLicense";
            this.lnkLicense.Size = new System.Drawing.Size(106, 20);
            this.lnkLicense.TabIndex = 1;
            this.lnkLicense.TabStop = true;
            this.lnkLicense.Text = "<license_info>";
            // 
            // labExeVersionCaption
            // 
            this.labExeVersionCaption.AutoSize = true;
            this.labExeVersionCaption.Location = new System.Drawing.Point(5, 184);
            this.labExeVersionCaption.Name = "labExeVersionCaption";
            this.labExeVersionCaption.Size = new System.Drawing.Size(135, 20);
            this.labExeVersionCaption.TabIndex = 7;
            this.labExeVersionCaption.Text = "Executable version:";
            // 
            // labExeVersion
            // 
            this.labExeVersion.AutoSize = true;
            this.labExeVersion.Location = new System.Drawing.Point(146, 184);
            this.labExeVersion.Name = "labExeVersion";
            this.labExeVersion.Size = new System.Drawing.Size(105, 20);
            this.labExeVersion.TabIndex = 8;
            this.labExeVersion.Text = "<exe_version>";
            // 
            // labExePathCaption
            // 
            this.labExePathCaption.AutoSize = true;
            this.labExePathCaption.Location = new System.Drawing.Point(5, 160);
            this.labExePathCaption.Name = "labExePathCaption";
            this.labExePathCaption.Size = new System.Drawing.Size(118, 20);
            this.labExePathCaption.TabIndex = 5;
            this.labExePathCaption.Text = "Executable path:";
            // 
            // labRunModeCaption
            // 
            this.labRunModeCaption.AutoSize = true;
            this.labRunModeCaption.Location = new System.Drawing.Point(5, 135);
            this.labRunModeCaption.Name = "labRunModeCaption";
            this.labRunModeCaption.Size = new System.Drawing.Size(128, 20);
            this.labRunModeCaption.TabIndex = 3;
            this.labRunModeCaption.Text = "Current run mode:";
            // 
            // labRunMode
            // 
            this.labRunMode.AutoSize = true;
            this.labRunMode.Location = new System.Drawing.Point(146, 135);
            this.labRunMode.Name = "labRunMode";
            this.labRunMode.Size = new System.Drawing.Size(95, 20);
            this.labRunMode.TabIndex = 4;
            this.labRunMode.Text = "<run_mode>";
            // 
            // labVsixVersion
            // 
            this.labVsixVersion.AutoSize = true;
            this.labVsixVersion.Location = new System.Drawing.Point(200, 208);
            this.labVsixVersion.Name = "labVsixVersion";
            this.labVsixVersion.Size = new System.Drawing.Size(106, 20);
            this.labVsixVersion.TabIndex = 10;
            this.labVsixVersion.Text = "<vsix_version>";
            this.labVsixVersion.Visible = false;
            // 
            // labVsixVersionCaption
            // 
            this.labVsixVersionCaption.AutoSize = true;
            this.labVsixVersionCaption.Location = new System.Drawing.Point(5, 208);
            this.labVsixVersionCaption.Name = "labVsixVersionCaption";
            this.labVsixVersionCaption.Size = new System.Drawing.Size(186, 20);
            this.labVsixVersionCaption.TabIndex = 9;
            this.labVsixVersionCaption.Text = "VS Code extension version:";
            this.labVsixVersionCaption.Visible = false;
            // 
            // lnkCopyrightInfo
            // 
            this.lnkCopyrightInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lnkCopyrightInfo.AutoSize = true;
            this.lnkCopyrightInfo.Location = new System.Drawing.Point(499, 76);
            this.lnkCopyrightInfo.Name = "lnkCopyrightInfo";
            this.lnkCopyrightInfo.Size = new System.Drawing.Size(124, 20);
            this.lnkCopyrightInfo.TabIndex = 2;
            this.lnkCopyrightInfo.TabStop = true;
            this.lnkCopyrightInfo.Text = "<copyright_info>";
            // 
            // cmdOK
            // 
            this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.cmdOK.Location = new System.Drawing.Point(520, 254);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(100, 37);
            this.cmdOK.TabIndex = 11;
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            // 
            // txtExePath
            // 
            this.txtExePath.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtExePath.Location = new System.Drawing.Point(149, 160);
            this.txtExePath.Name = "txtExePath";
            this.txtExePath.ReadOnly = true;
            this.txtExePath.Size = new System.Drawing.Size(471, 20);
            this.txtExePath.TabIndex = 12;
            this.txtExePath.Text = "<exe_path>";
            // 
            // lnkSourcesRepo
            // 
            this.lnkSourcesRepo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lnkSourcesRepo.AutoSize = true;
            this.lnkSourcesRepo.Location = new System.Drawing.Point(508, 99);
            this.lnkSourcesRepo.Name = "lnkSourcesRepo";
            this.lnkSourcesRepo.Size = new System.Drawing.Size(115, 20);
            this.lnkSourcesRepo.TabIndex = 13;
            this.lnkSourcesRepo.TabStop = true;
            this.lnkSourcesRepo.Text = "<sources_repo>";
            // 
            // AboutForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.cmdOK;
            this.ClientSize = new System.Drawing.Size(632, 303);
            this.ControlBox = false;
            this.Controls.Add(this.lnkSourcesRepo);
            this.Controls.Add(this.txtExePath);
            this.Controls.Add(this.cmdOK);
            this.Controls.Add(this.lnkCopyrightInfo);
            this.Controls.Add(this.labVsixVersion);
            this.Controls.Add(this.labVsixVersionCaption);
            this.Controls.Add(this.labRunMode);
            this.Controls.Add(this.labRunModeCaption);
            this.Controls.Add(this.labExePathCaption);
            this.Controls.Add(this.labExeVersion);
            this.Controls.Add(this.labExeVersionCaption);
            this.Controls.Add(this.lnkLicense);
            this.Controls.Add(this.labProductName);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1200, 350);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(650, 350);
            this.Name = "AboutForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "About";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labProductName;
        private Core.LinkLabelEx lnkLicense;
        private System.Windows.Forms.Label labExeVersionCaption;
        private System.Windows.Forms.Label labExeVersion;
        private System.Windows.Forms.Label labExePathCaption;
        private System.Windows.Forms.Label labRunModeCaption;
        private System.Windows.Forms.Label labRunMode;
        private System.Windows.Forms.Label labVsixVersion;
        private System.Windows.Forms.Label labVsixVersionCaption;
        private Core.LinkLabelEx lnkCopyrightInfo;
        private Core.ThemedButton cmdOK;
        private System.Windows.Forms.TextBox txtExePath;
        private Core.LinkLabelEx lnkSourcesRepo;
    }
}