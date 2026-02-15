namespace UiTools.WinForms.Designer
{
    partial class DesignerCsFileContextForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DesignerCsFileContextForm));
            this.labProjectFile = new System.Windows.Forms.Label();
            this.labExtraAssemblies = new System.Windows.Forms.Label();
            this.labConfiguration = new System.Windows.Forms.Label();
            this.labPlatform = new System.Windows.Forms.Label();
            this.cboConfiguration = new System.Windows.Forms.ComboBox();
            this.cboPlatform = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.picPlatformHint = new System.Windows.Forms.PictureBox();
            this.picConfigurationHint = new System.Windows.Forms.PictureBox();
            this.picProjectFileHint = new System.Windows.Forms.PictureBox();
            this.fpeProjectFile = new UiTools.WinForms.Designer.FilePathEdit();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.picExtraAssembliesHelp = new System.Windows.Forms.PictureBox();
            this.picExtraAssembliesHint = new System.Windows.Forms.PictureBox();
            this.fpeExtraAssemblies = new UiTools.WinForms.Designer.FilePathEdit();
            this.cmdOK = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.labDesignerFile = new System.Windows.Forms.Label();
            this.labNamespace = new System.Windows.Forms.Label();
            this.txtNamespace = new System.Windows.Forms.TextBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.labNewFormNotice = new System.Windows.Forms.Label();
            this.fpeDesignerFile = new UiTools.WinForms.Designer.FilePathEdit();
            this.picDesignerFileHint = new System.Windows.Forms.PictureBox();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPlatformHint)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picConfigurationHint)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picProjectFileHint)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picExtraAssembliesHelp)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picExtraAssembliesHint)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picDesignerFileHint)).BeginInit();
            this.SuspendLayout();
            // 
            // labProjectFile
            // 
            this.labProjectFile.AutoSize = true;
            this.labProjectFile.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labProjectFile.Location = new System.Drawing.Point(14, 132);
            this.labProjectFile.Name = "labProjectFile";
            this.labProjectFile.Size = new System.Drawing.Size(152, 20);
            this.labProjectFile.TabIndex = 0;
            this.labProjectFile.Text = "Project file (full path):";
            // 
            // labExtraAssemblies
            // 
            this.labExtraAssemblies.AutoSize = true;
            this.labExtraAssemblies.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labExtraAssemblies.Location = new System.Drawing.Point(14, 41);
            this.labExtraAssemblies.Name = "labExtraAssemblies";
            this.labExtraAssemblies.Size = new System.Drawing.Size(215, 20);
            this.labExtraAssemblies.TabIndex = 0;
            this.labExtraAssemblies.Text = "Extra assemblies file (full path):";
            // 
            // labConfiguration
            // 
            this.labConfiguration.AutoSize = true;
            this.labConfiguration.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labConfiguration.Location = new System.Drawing.Point(14, 170);
            this.labConfiguration.Name = "labConfiguration";
            this.labConfiguration.Size = new System.Drawing.Size(103, 20);
            this.labConfiguration.TabIndex = 2;
            this.labConfiguration.Text = "Configuration:";
            // 
            // labPlatform
            // 
            this.labPlatform.AutoSize = true;
            this.labPlatform.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labPlatform.Location = new System.Drawing.Point(14, 209);
            this.labPlatform.Name = "labPlatform";
            this.labPlatform.Size = new System.Drawing.Size(69, 20);
            this.labPlatform.TabIndex = 4;
            this.labPlatform.Text = "Platform:";
            // 
            // cboConfiguration
            // 
            this.cboConfiguration.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cboConfiguration.FormattingEnabled = true;
            this.cboConfiguration.Location = new System.Drawing.Point(207, 167);
            this.cboConfiguration.Name = "cboConfiguration";
            this.cboConfiguration.Size = new System.Drawing.Size(137, 28);
            this.cboConfiguration.TabIndex = 3;
            // 
            // cboPlatform
            // 
            this.cboPlatform.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cboPlatform.FormattingEnabled = true;
            this.cboPlatform.Location = new System.Drawing.Point(207, 206);
            this.cboPlatform.Name = "cboPlatform";
            this.cboPlatform.Size = new System.Drawing.Size(137, 28);
            this.cboPlatform.TabIndex = 5;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.BackColor = System.Drawing.SystemColors.Control;
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.picPlatformHint);
            this.groupBox1.Controls.Add(this.picConfigurationHint);
            this.groupBox1.Controls.Add(this.picProjectFileHint);
            this.groupBox1.Controls.Add(this.fpeProjectFile);
            this.groupBox1.Controls.Add(this.cboPlatform);
            this.groupBox1.Controls.Add(this.labProjectFile);
            this.groupBox1.Controls.Add(this.cboConfiguration);
            this.groupBox1.Controls.Add(this.labConfiguration);
            this.groupBox1.Controls.Add(this.labPlatform);
            this.groupBox1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.groupBox1.Location = new System.Drawing.Point(14, 66);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(839, 252);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "These parameters are used to determine location of the \"bin\" folder";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label1.Location = new System.Drawing.Point(14, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(819, 84);
            this.label1.TabIndex = 11;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // picPlatformHint
            // 
            this.picPlatformHint.Image = global::UiTools.WinForms.Designer.Properties.Resources.Hint;
            this.picPlatformHint.Location = new System.Drawing.Point(180, 208);
            this.picPlatformHint.Name = "picPlatformHint";
            this.picPlatformHint.Size = new System.Drawing.Size(24, 24);
            this.picPlatformHint.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picPlatformHint.TabIndex = 10;
            this.picPlatformHint.TabStop = false;
            this.picPlatformHint.Visible = false;
            // 
            // picConfigurationHint
            // 
            this.picConfigurationHint.Image = global::UiTools.WinForms.Designer.Properties.Resources.Hint;
            this.picConfigurationHint.Location = new System.Drawing.Point(180, 169);
            this.picConfigurationHint.Name = "picConfigurationHint";
            this.picConfigurationHint.Size = new System.Drawing.Size(24, 24);
            this.picConfigurationHint.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picConfigurationHint.TabIndex = 9;
            this.picConfigurationHint.TabStop = false;
            this.picConfigurationHint.Visible = false;
            // 
            // picProjectFileHint
            // 
            this.picProjectFileHint.Image = global::UiTools.WinForms.Designer.Properties.Resources.Hint;
            this.picProjectFileHint.Location = new System.Drawing.Point(180, 131);
            this.picProjectFileHint.Name = "picProjectFileHint";
            this.picProjectFileHint.Size = new System.Drawing.Size(24, 24);
            this.picProjectFileHint.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picProjectFileHint.TabIndex = 8;
            this.picProjectFileHint.TabStop = false;
            this.picProjectFileHint.Visible = false;
            // 
            // fpeProjectFile
            // 
            this.fpeProjectFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fpeProjectFile.BackColor = System.Drawing.SystemColors.Control;
            this.fpeProjectFile.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.fpeProjectFile.Location = new System.Drawing.Point(207, 128);
            this.fpeProjectFile.Margin = new System.Windows.Forms.Padding(0);
            this.fpeProjectFile.Name = "fpeProjectFile";
            this.fpeProjectFile.Size = new System.Drawing.Size(615, 28);
            this.fpeProjectFile.TabIndex = 1;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.picExtraAssembliesHelp);
            this.groupBox2.Controls.Add(this.picExtraAssembliesHint);
            this.groupBox2.Controls.Add(this.fpeExtraAssemblies);
            this.groupBox2.Controls.Add(this.labExtraAssemblies);
            this.groupBox2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.groupBox2.Location = new System.Drawing.Point(14, 337);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(838, 81);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "This optional parameter allows to load assemblies other than those found in the \"" +
    "bin\" folder";
            // 
            // picExtraAssembliesHelp
            // 
            this.picExtraAssembliesHelp.Image = global::UiTools.WinForms.Designer.Properties.Resources.Help;
            this.picExtraAssembliesHelp.Location = new System.Drawing.Point(263, 38);
            this.picExtraAssembliesHelp.Name = "picExtraAssembliesHelp";
            this.picExtraAssembliesHelp.Size = new System.Drawing.Size(24, 24);
            this.picExtraAssembliesHelp.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picExtraAssembliesHelp.TabIndex = 10;
            this.picExtraAssembliesHelp.TabStop = false;
            this.toolTip1.SetToolTip(this.picExtraAssembliesHelp, resources.GetString("picExtraAssembliesHelp.ToolTip"));
            // 
            // picExtraAssembliesHint
            // 
            this.picExtraAssembliesHint.Image = global::UiTools.WinForms.Designer.Properties.Resources.Hint;
            this.picExtraAssembliesHint.Location = new System.Drawing.Point(237, 38);
            this.picExtraAssembliesHint.Name = "picExtraAssembliesHint";
            this.picExtraAssembliesHint.Size = new System.Drawing.Size(24, 24);
            this.picExtraAssembliesHint.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picExtraAssembliesHint.TabIndex = 9;
            this.picExtraAssembliesHint.TabStop = false;
            this.picExtraAssembliesHint.Visible = false;
            // 
            // fpeExtraAssemblies
            // 
            this.fpeExtraAssemblies.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fpeExtraAssemblies.BackColor = System.Drawing.SystemColors.Control;
            this.fpeExtraAssemblies.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.fpeExtraAssemblies.Location = new System.Drawing.Point(290, 35);
            this.fpeExtraAssemblies.Margin = new System.Windows.Forms.Padding(0);
            this.fpeExtraAssemblies.Name = "fpeExtraAssemblies";
            this.fpeExtraAssemblies.Size = new System.Drawing.Size(532, 28);
            this.fpeExtraAssemblies.TabIndex = 1;
            // 
            // cmdOK
            // 
            this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmdOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.cmdOK.Location = new System.Drawing.Point(14, 438);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(112, 45);
            this.cmdOK.TabIndex = 6;
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(741, 438);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(112, 45);
            this.cmdCancel.TabIndex = 7;
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            // 
            // labDesignerFile
            // 
            this.labDesignerFile.AutoSize = true;
            this.labDesignerFile.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labDesignerFile.Location = new System.Drawing.Point(16, 16);
            this.labDesignerFile.Name = "labDesignerFile";
            this.labDesignerFile.Size = new System.Drawing.Size(165, 20);
            this.labDesignerFile.TabIndex = 0;
            this.labDesignerFile.Text = "Designer file (full path):";
            // 
            // labNamespace
            // 
            this.labNamespace.AutoSize = true;
            this.labNamespace.Location = new System.Drawing.Point(16, 16);
            this.labNamespace.Name = "labNamespace";
            this.labNamespace.Size = new System.Drawing.Size(90, 20);
            this.labNamespace.TabIndex = 2;
            this.labNamespace.Text = "Namespace:";
            // 
            // txtNamespace
            // 
            this.txtNamespace.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtNamespace.Location = new System.Drawing.Point(221, 12);
            this.txtNamespace.Name = "txtNamespace";
            this.txtNamespace.Size = new System.Drawing.Size(631, 27);
            this.txtNamespace.TabIndex = 3;
            // 
            // labNewFormNotice
            // 
            this.labNewFormNotice.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labNewFormNotice.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.labNewFormNotice.Location = new System.Drawing.Point(28, 433);
            this.labNewFormNotice.Name = "labNewFormNotice";
            this.labNewFormNotice.Size = new System.Drawing.Size(819, 44);
            this.labNewFormNotice.TabIndex = 8;
            this.labNewFormNotice.Text = resources.GetString("labNewFormNotice.Text");
            // 
            // fpeDesignerFile
            // 
            this.fpeDesignerFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fpeDesignerFile.BackColor = System.Drawing.SystemColors.Control;
            this.fpeDesignerFile.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.fpeDesignerFile.Location = new System.Drawing.Point(221, 12);
            this.fpeDesignerFile.Margin = new System.Windows.Forms.Padding(0);
            this.fpeDesignerFile.Name = "fpeDesignerFile";
            this.fpeDesignerFile.Size = new System.Drawing.Size(631, 28);
            this.fpeDesignerFile.TabIndex = 1;
            // 
            // picDesignerFileHint
            // 
            this.picDesignerFileHint.Image = global::UiTools.WinForms.Designer.Properties.Resources.Warning;
            this.picDesignerFileHint.Location = new System.Drawing.Point(194, 15);
            this.picDesignerFileHint.Name = "picDesignerFileHint";
            this.picDesignerFileHint.Size = new System.Drawing.Size(24, 24);
            this.picDesignerFileHint.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picDesignerFileHint.TabIndex = 9;
            this.picDesignerFileHint.TabStop = false;
            this.picDesignerFileHint.Visible = false;
            // 
            // DesignerCsFileContextForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(870, 495);
            this.Controls.Add(this.picDesignerFileHint);
            this.Controls.Add(this.labNewFormNotice);
            this.Controls.Add(this.txtNamespace);
            this.Controls.Add(this.labNamespace);
            this.Controls.Add(this.fpeDesignerFile);
            this.Controls.Add(this.labDesignerFile);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdOK);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(888, 542);
            this.Name = "DesignerCsFileContextForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "<Title>";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPlatformHint)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picConfigurationHint)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picProjectFileHint)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picExtraAssembliesHelp)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picExtraAssembliesHint)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picDesignerFileHint)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labProjectFile;
        private FilePathEdit fpeProjectFile;
        private FilePathEdit fpeExtraAssemblies;
        private System.Windows.Forms.Label labExtraAssemblies;
        private System.Windows.Forms.Label labConfiguration;
        private System.Windows.Forms.Label labPlatform;
        private System.Windows.Forms.ComboBox cboConfiguration;
        private System.Windows.Forms.ComboBox cboPlatform;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Button cmdCancel;
        private FilePathEdit fpeDesignerFile;
        private System.Windows.Forms.Label labDesignerFile;
        private System.Windows.Forms.Label labNamespace;
        private System.Windows.Forms.TextBox txtNamespace;
        private System.Windows.Forms.PictureBox picProjectFileHint;
        private System.Windows.Forms.PictureBox picPlatformHint;
        private System.Windows.Forms.PictureBox picConfigurationHint;
        private System.Windows.Forms.PictureBox picExtraAssembliesHint;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.PictureBox picExtraAssembliesHelp;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labNewFormNotice;
        private System.Windows.Forms.PictureBox picDesignerFileHint;
    }
}