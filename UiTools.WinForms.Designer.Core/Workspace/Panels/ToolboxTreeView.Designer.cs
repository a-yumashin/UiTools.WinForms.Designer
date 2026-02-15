namespace UiTools.WinForms.Designer.Core
{
    partial class ToolboxTreeView
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ToolboxTreeView));
            this.cboSearch = new System.Windows.Forms.ComboBox();
            this.imageList16px = new System.Windows.Forms.ImageList(this.components);
            this.picSearch = new System.Windows.Forms.PictureBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.imageList20px = new System.Windows.Forms.ImageList(this.components);
            this.labNoResults = new System.Windows.Forms.Label();
            this.treeView1 = new UiTools.WinForms.Designer.Core.DoubleBufferedTreeView();
            ((System.ComponentModel.ISupportInitialize)(this.picSearch)).BeginInit();
            this.SuspendLayout();
            // 
            // cboSearch
            // 
            this.cboSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.cboSearch.FormattingEnabled = true;
            this.cboSearch.Location = new System.Drawing.Point(0, 0);
            this.cboSearch.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cboSearch.Name = "cboSearch";
            this.cboSearch.Size = new System.Drawing.Size(220, 28);
            this.cboSearch.TabIndex = 0;
            // 
            // imageList16px
            // 
            this.imageList16px.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList16px.ImageStream")));
            this.imageList16px.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList16px.Images.SetKeyName(0, "CollapsedTreeNode");
            this.imageList16px.Images.SetKeyName(1, "ExpandedTreeNode");
            this.imageList16px.Images.SetKeyName(2, "SelectedAndCollapsedTreeNode");
            this.imageList16px.Images.SetKeyName(3, "SelectedAndExpandedTreeNode");
            this.imageList16px.Images.SetKeyName(4, "Pointer");
            // 
            // picSearch
            // 
            this.picSearch.BackColor = System.Drawing.SystemColors.Window;
            this.picSearch.Location = new System.Drawing.Point(170, 3);
            this.picSearch.Name = "picSearch";
            this.picSearch.Size = new System.Drawing.Size(20, 20);
            this.picSearch.TabIndex = 2;
            this.picSearch.TabStop = false;
            // 
            // imageList20px
            // 
            this.imageList20px.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList20px.ImageStream")));
            this.imageList20px.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList20px.Images.SetKeyName(0, "ClearSearch");
            this.imageList20px.Images.SetKeyName(1, "HoveredClearSearch");
            this.imageList20px.Images.SetKeyName(2, "HoveredSearch");
            this.imageList20px.Images.SetKeyName(3, "Search");
            // 
            // labNoResults
            // 
            this.labNoResults.AutoSize = true;
            this.labNoResults.BackColor = System.Drawing.SystemColors.Window;
            this.labNoResults.Location = new System.Drawing.Point(48, 42);
            this.labNoResults.Name = "labNoResults";
            this.labNoResults.Size = new System.Drawing.Size(121, 20);
            this.labNoResults.TabIndex = 3;
            this.labNoResults.Text = "No results found.";
            this.labNoResults.Visible = false;
            // 
            // treeView1
            // 
            this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView1.Location = new System.Drawing.Point(0, 28);
            this.treeView1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(220, 394);
            this.treeView1.TabIndex = 1;
            // 
            // ToolboxTreeView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.labNoResults);
            this.Controls.Add(this.picSearch);
            this.Controls.Add(this.treeView1);
            this.Controls.Add(this.cboSearch);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "ToolboxTreeView";
            this.Size = new System.Drawing.Size(220, 422);
            ((System.ComponentModel.ISupportInitialize)(this.picSearch)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cboSearch;
        private DoubleBufferedTreeView treeView1;
        private System.Windows.Forms.ImageList imageList16px;
        private System.Windows.Forms.PictureBox picSearch;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ImageList imageList20px;
        private System.Windows.Forms.Label labNoResults;
    }
}
