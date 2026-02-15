namespace UiTools.WinForms.Designer
{
    partial class OptionsForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.cboAlignControlsMode = new System.Windows.Forms.ComboBox();
            this.labGridSize = new System.Windows.Forms.Label();
            this.txtGridSize = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cboDefaultRootComponentFontName = new System.Windows.Forms.ComboBox();
            this.cboDefaultRootComponentFontSize = new System.Windows.Forms.ComboBox();
            this.cboCodeViewerFontSize = new System.Windows.Forms.ComboBox();
            this.cboCodeViewerFontName = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txtMRUListMaxSize = new System.Windows.Forms.TextBox();
            this.cmdSave = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.cboLogLevel = new System.Windows.Forms.ComboBox();
            this.chkRemoveUnnecessaryUsings = new System.Windows.Forms.CheckBox();
            this.picRemoveUnnecessaryUsingsHelp = new System.Windows.Forms.PictureBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.picRemoveUnnecessaryUsingsHelp)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(147, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Align controls mode:";
            // 
            // cboAlignControlsMode
            // 
            this.cboAlignControlsMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboAlignControlsMode.FormattingEnabled = true;
            this.cboAlignControlsMode.Location = new System.Drawing.Point(230, 14);
            this.cboAlignControlsMode.Name = "cboAlignControlsMode";
            this.cboAlignControlsMode.Size = new System.Drawing.Size(217, 28);
            this.cboAlignControlsMode.TabIndex = 1;
            // 
            // labGridSize
            // 
            this.labGridSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labGridSize.AutoSize = true;
            this.labGridSize.Location = new System.Drawing.Point(461, 18);
            this.labGridSize.Name = "labGridSize";
            this.labGridSize.Size = new System.Drawing.Size(71, 20);
            this.labGridSize.TabIndex = 2;
            this.labGridSize.Text = "Grid Size:";
            // 
            // txtGridSize
            // 
            this.txtGridSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtGridSize.Location = new System.Drawing.Point(538, 14);
            this.txtGridSize.Name = "txtGridSize";
            this.txtGridSize.Size = new System.Drawing.Size(51, 27);
            this.txtGridSize.TabIndex = 3;
            this.txtGridSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 67);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(204, 20);
            this.label3.TabIndex = 4;
            this.label3.Text = "New component default font:";
            // 
            // cboDefaultRootComponentFontName
            // 
            this.cboDefaultRootComponentFontName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboDefaultRootComponentFontName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDefaultRootComponentFontName.FormattingEnabled = true;
            this.cboDefaultRootComponentFontName.Location = new System.Drawing.Point(230, 63);
            this.cboDefaultRootComponentFontName.Name = "cboDefaultRootComponentFontName";
            this.cboDefaultRootComponentFontName.Size = new System.Drawing.Size(302, 28);
            this.cboDefaultRootComponentFontName.TabIndex = 5;
            // 
            // cboDefaultRootComponentFontSize
            // 
            this.cboDefaultRootComponentFontSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cboDefaultRootComponentFontSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDefaultRootComponentFontSize.FormattingEnabled = true;
            this.cboDefaultRootComponentFontSize.Location = new System.Drawing.Point(538, 63);
            this.cboDefaultRootComponentFontSize.Name = "cboDefaultRootComponentFontSize";
            this.cboDefaultRootComponentFontSize.Size = new System.Drawing.Size(51, 28);
            this.cboDefaultRootComponentFontSize.TabIndex = 6;
            // 
            // cboCodeViewerFontSize
            // 
            this.cboCodeViewerFontSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cboCodeViewerFontSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCodeViewerFontSize.FormattingEnabled = true;
            this.cboCodeViewerFontSize.Location = new System.Drawing.Point(538, 112);
            this.cboCodeViewerFontSize.Name = "cboCodeViewerFontSize";
            this.cboCodeViewerFontSize.Size = new System.Drawing.Size(51, 28);
            this.cboCodeViewerFontSize.TabIndex = 9;
            // 
            // cboCodeViewerFontName
            // 
            this.cboCodeViewerFontName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboCodeViewerFontName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCodeViewerFontName.FormattingEnabled = true;
            this.cboCodeViewerFontName.Location = new System.Drawing.Point(230, 112);
            this.cboCodeViewerFontName.Name = "cboCodeViewerFontName";
            this.cboCodeViewerFontName.Size = new System.Drawing.Size(302, 28);
            this.cboCodeViewerFontName.TabIndex = 8;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 116);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(172, 20);
            this.label4.TabIndex = 7;
            this.label4.Text = "Source code viewer font:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 165);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(128, 20);
            this.label5.TabIndex = 10;
            this.label5.Text = "MRU list max size:";
            // 
            // txtMRUListMaxSize
            // 
            this.txtMRUListMaxSize.Location = new System.Drawing.Point(230, 162);
            this.txtMRUListMaxSize.Name = "txtMRUListMaxSize";
            this.txtMRUListMaxSize.Size = new System.Drawing.Size(54, 27);
            this.txtMRUListMaxSize.TabIndex = 11;
            this.txtMRUListMaxSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // cmdSave
            // 
            this.cmdSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmdSave.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.cmdSave.Location = new System.Drawing.Point(13, 318);
            this.cmdSave.Name = "cmdSave";
            this.cmdSave.Size = new System.Drawing.Size(109, 41);
            this.cmdSave.TabIndex = 15;
            this.cmdSave.Text = "Save";
            this.cmdSave.UseVisualStyleBackColor = true;
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(480, 318);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(109, 41);
            this.cmdCancel.TabIndex = 16;
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 214);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(189, 20);
            this.label2.TabIndex = 12;
            this.label2.Text = "Output panel min log level:";
            // 
            // cboLogLevel
            // 
            this.cboLogLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLogLevel.FormattingEnabled = true;
            this.cboLogLevel.Location = new System.Drawing.Point(230, 211);
            this.cboLogLevel.Name = "cboLogLevel";
            this.cboLogLevel.Size = new System.Drawing.Size(114, 28);
            this.cboLogLevel.TabIndex = 13;
            // 
            // chkRemoveUnnecessaryUsings
            // 
            this.chkRemoveUnnecessaryUsings.AutoSize = true;
            this.chkRemoveUnnecessaryUsings.Location = new System.Drawing.Point(13, 261);
            this.chkRemoveUnnecessaryUsings.Name = "chkRemoveUnnecessaryUsings";
            this.chkRemoveUnnecessaryUsings.Size = new System.Drawing.Size(267, 24);
            this.chkRemoveUnnecessaryUsings.TabIndex = 14;
            this.chkRemoveUnnecessaryUsings.Text = "Remove unnecessary usings on save";
            this.chkRemoveUnnecessaryUsings.UseVisualStyleBackColor = true;
            // 
            // picRemoveUnnecessaryUsingsHelp
            // 
            this.picRemoveUnnecessaryUsingsHelp.Image = global::UiTools.WinForms.Designer.Properties.Resources.Hint;
            this.picRemoveUnnecessaryUsingsHelp.Location = new System.Drawing.Point(281, 261);
            this.picRemoveUnnecessaryUsingsHelp.Name = "picRemoveUnnecessaryUsingsHelp";
            this.picRemoveUnnecessaryUsingsHelp.Size = new System.Drawing.Size(24, 24);
            this.picRemoveUnnecessaryUsingsHelp.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picRemoveUnnecessaryUsingsHelp.TabIndex = 17;
            this.picRemoveUnnecessaryUsingsHelp.TabStop = false;
            this.toolTip1.SetToolTip(this.picRemoveUnnecessaryUsingsHelp, "This may cause a brief delay on slower machines, as it requires\r\nbuilding a seman" +
        "tic model to detect unused namespaces (CS8019).");
            // 
            // OptionsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(600, 371);
            this.Controls.Add(this.picRemoveUnnecessaryUsingsHelp);
            this.Controls.Add(this.chkRemoveUnnecessaryUsings);
            this.Controls.Add(this.cboLogLevel);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdSave);
            this.Controls.Add(this.txtMRUListMaxSize);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.cboCodeViewerFontSize);
            this.Controls.Add(this.cboCodeViewerFontName);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cboDefaultRootComponentFontSize);
            this.Controls.Add(this.cboDefaultRootComponentFontName);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtGridSize);
            this.Controls.Add(this.labGridSize);
            this.Controls.Add(this.cboAlignControlsMode);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OptionsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Options";
            ((System.ComponentModel.ISupportInitialize)(this.picRemoveUnnecessaryUsingsHelp)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cboAlignControlsMode;
        private System.Windows.Forms.Label labGridSize;
        private System.Windows.Forms.TextBox txtGridSize;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cboDefaultRootComponentFontName;
        private System.Windows.Forms.ComboBox cboDefaultRootComponentFontSize;
        private System.Windows.Forms.ComboBox cboCodeViewerFontSize;
        private System.Windows.Forms.ComboBox cboCodeViewerFontName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtMRUListMaxSize;
        private System.Windows.Forms.Button cmdSave;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cboLogLevel;
        private System.Windows.Forms.CheckBox chkRemoveUnnecessaryUsings;
        private System.Windows.Forms.PictureBox picRemoveUnnecessaryUsingsHelp;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}