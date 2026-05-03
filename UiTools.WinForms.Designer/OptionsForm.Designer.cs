using UiTools.WinForms.Designer.Core;

namespace UiTools.WinForms.Designer
{
    partial class OptionsForm : ThemedForm
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
            this.cboAlignControlsMode = new UiTools.WinForms.Designer.Core.ThemedComboBox();
            this.labGridSize = new UiTools.WinForms.Designer.Core.ThemedLabel();
            this.txtGridSize = new UiTools.WinForms.Designer.Core.ThemedTextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cboDefaultRootComponentFontName = new UiTools.WinForms.Designer.Core.ThemedComboBox();
            this.cboDefaultRootComponentFontSize = new UiTools.WinForms.Designer.Core.ThemedComboBox();
            this.cboCodeViewerFontSize = new UiTools.WinForms.Designer.Core.ThemedComboBox();
            this.cboCodeViewerFontName = new UiTools.WinForms.Designer.Core.ThemedComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txtMRUListMaxSize = new UiTools.WinForms.Designer.Core.ThemedTextBox();
            this.cmdSave = new UiTools.WinForms.Designer.Core.ThemedButton();
            this.cmdCancel = new UiTools.WinForms.Designer.Core.ThemedButton();
            this.label2 = new System.Windows.Forms.Label();
            this.cboLogLevel = new UiTools.WinForms.Designer.Core.ThemedComboBox();
            this.chkRemoveUnnecessaryUsings = new System.Windows.Forms.CheckBox();
            this.picRemoveUnnecessaryUsingsHelp = new System.Windows.Forms.PictureBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.picUiTheme = new UiTools.WinForms.Designer.PictureBoxEx();
            this.cboUiTheme = new UiTools.WinForms.Designer.Core.ThemedComboBox();
            this.label6 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picRemoveUnnecessaryUsingsHelp)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picUiTheme)).BeginInit();
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
            this.cboAlignControlsMode.CueBannerText = null;
            this.cboAlignControlsMode.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cboAlignControlsMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboAlignControlsMode.FormattingEnabled = true;
            this.cboAlignControlsMode.Location = new System.Drawing.Point(237, 14);
            this.cboAlignControlsMode.Name = "cboAlignControlsMode";
            this.cboAlignControlsMode.Size = new System.Drawing.Size(217, 28);
            this.cboAlignControlsMode.TabIndex = 1;
            // 
            // labGridSize
            // 
            this.labGridSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labGridSize.AutoSize = true;
            this.labGridSize.BackColor = System.Drawing.Color.Transparent;
            this.labGridSize.Location = new System.Drawing.Point(489, 18);
            this.labGridSize.Name = "labGridSize";
            this.labGridSize.Size = new System.Drawing.Size(71, 20);
            this.labGridSize.TabIndex = 2;
            this.labGridSize.Text = "Grid Size:";
            // 
            // txtGridSize
            // 
            this.txtGridSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtGridSize.Location = new System.Drawing.Point(566, 15);
            this.txtGridSize.Name = "txtGridSize";
            this.txtGridSize.Size = new System.Drawing.Size(61, 27);
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
            this.cboDefaultRootComponentFontName.CueBannerText = null;
            this.cboDefaultRootComponentFontName.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cboDefaultRootComponentFontName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDefaultRootComponentFontName.FormattingEnabled = true;
            this.cboDefaultRootComponentFontName.Location = new System.Drawing.Point(237, 63);
            this.cboDefaultRootComponentFontName.Name = "cboDefaultRootComponentFontName";
            this.cboDefaultRootComponentFontName.Size = new System.Drawing.Size(323, 28);
            this.cboDefaultRootComponentFontName.TabIndex = 5;
            // 
            // cboDefaultRootComponentFontSize
            // 
            this.cboDefaultRootComponentFontSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cboDefaultRootComponentFontSize.CueBannerText = null;
            this.cboDefaultRootComponentFontSize.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cboDefaultRootComponentFontSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDefaultRootComponentFontSize.FormattingEnabled = true;
            this.cboDefaultRootComponentFontSize.Location = new System.Drawing.Point(566, 63);
            this.cboDefaultRootComponentFontSize.Name = "cboDefaultRootComponentFontSize";
            this.cboDefaultRootComponentFontSize.Size = new System.Drawing.Size(61, 28);
            this.cboDefaultRootComponentFontSize.TabIndex = 6;
            // 
            // cboCodeViewerFontSize
            // 
            this.cboCodeViewerFontSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cboCodeViewerFontSize.CueBannerText = null;
            this.cboCodeViewerFontSize.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cboCodeViewerFontSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCodeViewerFontSize.FormattingEnabled = true;
            this.cboCodeViewerFontSize.Location = new System.Drawing.Point(566, 112);
            this.cboCodeViewerFontSize.Name = "cboCodeViewerFontSize";
            this.cboCodeViewerFontSize.Size = new System.Drawing.Size(61, 28);
            this.cboCodeViewerFontSize.TabIndex = 9;
            // 
            // cboCodeViewerFontName
            // 
            this.cboCodeViewerFontName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboCodeViewerFontName.CueBannerText = null;
            this.cboCodeViewerFontName.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cboCodeViewerFontName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCodeViewerFontName.FormattingEnabled = true;
            this.cboCodeViewerFontName.Location = new System.Drawing.Point(237, 112);
            this.cboCodeViewerFontName.Name = "cboCodeViewerFontName";
            this.cboCodeViewerFontName.Size = new System.Drawing.Size(323, 28);
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
            this.txtMRUListMaxSize.Location = new System.Drawing.Point(237, 162);
            this.txtMRUListMaxSize.Name = "txtMRUListMaxSize";
            this.txtMRUListMaxSize.Size = new System.Drawing.Size(54, 27);
            this.txtMRUListMaxSize.TabIndex = 11;
            this.txtMRUListMaxSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // cmdSave
            // 
            this.cmdSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmdSave.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.cmdSave.Location = new System.Drawing.Point(13, 389);
            this.cmdSave.Name = "cmdSave";
            this.cmdSave.Size = new System.Drawing.Size(109, 41);
            this.cmdSave.TabIndex = 17;
            this.cmdSave.Text = "Save";
            this.cmdSave.UseVisualStyleBackColor = true;
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(518, 389);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(109, 41);
            this.cmdCancel.TabIndex = 18;
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
            this.cboLogLevel.CueBannerText = null;
            this.cboLogLevel.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cboLogLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLogLevel.FormattingEnabled = true;
            this.cboLogLevel.Location = new System.Drawing.Point(237, 211);
            this.cboLogLevel.Name = "cboLogLevel";
            this.cboLogLevel.Size = new System.Drawing.Size(323, 28);
            this.cboLogLevel.TabIndex = 13;
            // 
            // chkRemoveUnnecessaryUsings
            // 
            this.chkRemoveUnnecessaryUsings.AutoSize = true;
            this.chkRemoveUnnecessaryUsings.Location = new System.Drawing.Point(13, 316);
            this.chkRemoveUnnecessaryUsings.Name = "chkRemoveUnnecessaryUsings";
            this.chkRemoveUnnecessaryUsings.Size = new System.Drawing.Size(267, 24);
            this.chkRemoveUnnecessaryUsings.TabIndex = 16;
            this.chkRemoveUnnecessaryUsings.Text = "Remove unnecessary usings on save";
            this.chkRemoveUnnecessaryUsings.UseVisualStyleBackColor = true;
            // 
            // picRemoveUnnecessaryUsingsHelp
            // 
            this.picRemoveUnnecessaryUsingsHelp.Image = global::UiTools.WinForms.Designer.Properties.Resources.Hint;
            this.picRemoveUnnecessaryUsingsHelp.Location = new System.Drawing.Point(281, 316);
            this.picRemoveUnnecessaryUsingsHelp.Name = "picRemoveUnnecessaryUsingsHelp";
            this.picRemoveUnnecessaryUsingsHelp.Size = new System.Drawing.Size(24, 24);
            this.picRemoveUnnecessaryUsingsHelp.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picRemoveUnnecessaryUsingsHelp.TabIndex = 17;
            this.picRemoveUnnecessaryUsingsHelp.TabStop = false;
            this.toolTip1.SetToolTip(this.picRemoveUnnecessaryUsingsHelp, "This may cause a brief delay on slower machines, as it requires\r\nbuilding a seman" +
        "tic model to detect unused namespaces (CS8019).");
            // 
            // picUiTheme
            // 
            this.picUiTheme.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picUiTheme.HoverBackColor = System.Drawing.Color.Empty;
            this.picUiTheme.Image = global::UiTools.WinForms.Designer.Properties.Resources.UiThemeConfigFolder;
            this.picUiTheme.Location = new System.Drawing.Point(566, 264);
            this.picUiTheme.Name = "picUiTheme";
            this.picUiTheme.Size = new System.Drawing.Size(20, 20);
            this.picUiTheme.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.picUiTheme.TabIndex = 19;
            this.picUiTheme.TabStop = false;
            this.toolTip1.SetToolTip(this.picUiTheme, "show themes config in Explorer");
            this.picUiTheme.Click += new System.EventHandler(this.picUiTheme_Click);
            // 
            // cboUiTheme
            // 
            this.cboUiTheme.CueBannerText = null;
            this.cboUiTheme.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cboUiTheme.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboUiTheme.FormattingEnabled = true;
            this.cboUiTheme.Location = new System.Drawing.Point(237, 260);
            this.cboUiTheme.Name = "cboUiTheme";
            this.cboUiTheme.Size = new System.Drawing.Size(323, 28);
            this.cboUiTheme.TabIndex = 15;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 263);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(72, 20);
            this.label6.TabIndex = 14;
            this.label6.Text = "UI theme:";
            // 
            // OptionsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(641, 442);
            this.Controls.Add(this.picUiTheme);
            this.Controls.Add(this.cboUiTheme);
            this.Controls.Add(this.label6);
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
            this.Text = "Options";
            ((System.ComponentModel.ISupportInitialize)(this.picRemoveUnnecessaryUsingsHelp)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picUiTheme)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private Core.ThemedComboBox cboAlignControlsMode;
        private Core.ThemedLabel labGridSize;
        private Core.ThemedTextBox txtGridSize;
        private System.Windows.Forms.Label label3;
        private Core.ThemedComboBox cboDefaultRootComponentFontName;
        private Core.ThemedComboBox cboDefaultRootComponentFontSize;
        private Core.ThemedComboBox cboCodeViewerFontSize;
        private Core.ThemedComboBox cboCodeViewerFontName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private Core.ThemedTextBox txtMRUListMaxSize;
        private Core.ThemedButton cmdSave;
        private Core.ThemedButton cmdCancel;
        private System.Windows.Forms.Label label2;
        private Core.ThemedComboBox cboLogLevel;
        private System.Windows.Forms.CheckBox chkRemoveUnnecessaryUsings;
        private System.Windows.Forms.PictureBox picRemoveUnnecessaryUsingsHelp;
        private System.Windows.Forms.ToolTip toolTip1;
        private Core.ThemedComboBox cboUiTheme;
        private System.Windows.Forms.Label label6;
        private PictureBoxEx picUiTheme;
    }
}