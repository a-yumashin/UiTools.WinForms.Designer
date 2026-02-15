namespace UiTools.WinForms.Designer.Core
{
    partial class OutputPanel
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tsbClear = new System.Windows.Forms.ToolStripButton();
            this.tsbToggleWrap = new System.Windows.Forms.ToolStripButton();
            this.tsbTimestamp = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbSearch = new System.Windows.Forms.ToolStripButton();
            this.browserOutput = new System.Windows.Forms.WebBrowser();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbClear,
            this.tsbToggleWrap,
            this.tsbTimestamp,
            this.toolStripSeparator1,
            this.tsbSearch});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(417, 27);
            this.toolStrip1.TabIndex = 0;
            // 
            // tsbClear
            // 
            this.tsbClear.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbClear.Image = global::UiTools.WinForms.Designer.Core.Properties.Resources.ClearWindowContent;
            this.tsbClear.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbClear.Name = "tsbClear";
            this.tsbClear.Size = new System.Drawing.Size(29, 24);
            this.tsbClear.ToolTipText = "Clear All";
            this.tsbClear.Click += new System.EventHandler(this.tsbClear_Click);
            // 
            // tsbToggleWrap
            // 
            this.tsbToggleWrap.CheckOnClick = true;
            this.tsbToggleWrap.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbToggleWrap.Image = global::UiTools.WinForms.Designer.Core.Properties.Resources.WordWrap;
            this.tsbToggleWrap.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbToggleWrap.Name = "tsbToggleWrap";
            this.tsbToggleWrap.Size = new System.Drawing.Size(29, 24);
            this.tsbToggleWrap.ToolTipText = "Toggle Word Wrap";
            this.tsbToggleWrap.CheckedChanged += new System.EventHandler(this.tsbToggleWrap_CheckedChanged);
            // 
            // tsbTimestamp
            // 
            this.tsbTimestamp.CheckOnClick = true;
            this.tsbTimestamp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbTimestamp.Image = global::UiTools.WinForms.Designer.Core.Properties.Resources.TimeStamp;
            this.tsbTimestamp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbTimestamp.Name = "tsbTimestamp";
            this.tsbTimestamp.Size = new System.Drawing.Size(29, 24);
            this.tsbTimestamp.ToolTipText = "Show Timestamp";
            this.tsbTimestamp.CheckStateChanged += new System.EventHandler(this.tsbTimestamp_CheckStateChanged);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 27);
            // 
            // tsbSearch
            // 
            this.tsbSearch.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbSearch.Image = global::UiTools.WinForms.Designer.Core.Properties.Resources.Search;
            this.tsbSearch.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbSearch.Name = "tsbSearch";
            this.tsbSearch.Size = new System.Drawing.Size(29, 24);
            this.tsbSearch.Text = "toolStripButton1";
            this.tsbSearch.ToolTipText = "Search";
            this.tsbSearch.Click += new System.EventHandler(this.tsbSearch_Click);
            // 
            // browserOutput
            // 
            this.browserOutput.AllowWebBrowserDrop = false;
            this.browserOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.browserOutput.Location = new System.Drawing.Point(0, 27);
            this.browserOutput.MinimumSize = new System.Drawing.Size(20, 20);
            this.browserOutput.Name = "browserOutput";
            this.browserOutput.ScriptErrorsSuppressed = true;
            this.browserOutput.Size = new System.Drawing.Size(417, 195);
            this.browserOutput.TabIndex = 1;
            this.browserOutput.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(this.browserOutput_Navigating);
            // 
            // OutputPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.browserOutput);
            this.Controls.Add(this.toolStrip1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Name = "OutputPanel";
            this.Size = new System.Drawing.Size(417, 222);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton tsbClear;
        private System.Windows.Forms.ToolStripButton tsbToggleWrap;
        private System.Windows.Forms.WebBrowser browserOutput;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton tsbSearch;
        private System.Windows.Forms.ToolStripButton tsbTimestamp;
    }
}
