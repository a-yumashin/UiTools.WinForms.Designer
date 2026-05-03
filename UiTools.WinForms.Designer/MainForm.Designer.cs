using System.Diagnostics;
using System.Windows.Forms;

namespace UiTools.WinForms.Designer
{
    public partial class MainForm : UiTools.WinForms.Designer.Core.ThemedForm
	{
		//Form overrides dispose to clean up the component list.
		[DebuggerNonUserCode()]
		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing && components != null)
				{
					components.Dispose();
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		//Required by the Windows Form Designer
		private System.ComponentModel.IContainer components;

		//NOTE: The following procedure is required by the Windows Form Designer
		//It can be modified using the Windows Form Designer.  
		//Do not modify it using the code editor.
		[System.Diagnostics.DebuggerStepThrough()]
		private void InitializeComponent()
		{
            this.tsiToolsRoot = new System.Windows.Forms.ToolStripMenuItem();
            this.tsiOptions = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.tsiEditRoot = new System.Windows.Forms.ToolStripMenuItem();
            this.tsiUnDo = new System.Windows.Forms.ToolStripMenuItem();
            this.tsiReDo = new System.Windows.Forms.ToolStripMenuItem();
            this.tsiCut = new System.Windows.Forms.ToolStripMenuItem();
            this.tsiCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.tsiPaste = new System.Windows.Forms.ToolStripMenuItem();
            this.tsiDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.tsiSelectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1 = new UiTools.WinForms.Designer.Core.ThemedMenuStrip();
            this.tsiFileRoot = new System.Windows.Forms.ToolStripMenuItem();
            this.tsiNewRoot = new System.Windows.Forms.ToolStripMenuItem();
            this.tsiNewForm = new System.Windows.Forms.ToolStripMenuItem();
            this.tsiNewUserControl = new System.Windows.Forms.ToolStripMenuItem();
            this.tsiOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.tsiOpenRecent = new System.Windows.Forms.ToolStripMenuItem();
            this.tsiSave = new System.Windows.Forms.ToolStripMenuItem();
            this.tsiSaveAs = new System.Windows.Forms.ToolStripMenuItem();
            this.tsiClose = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsiExit = new System.Windows.Forms.ToolStripMenuItem();
            this.tsiViewRoot = new System.Windows.Forms.ToolStripMenuItem();
            this.tsiViewToolbox = new System.Windows.Forms.ToolStripMenuItem();
            this.tsiViewProperties = new System.Windows.Forms.ToolStripMenuItem();
            this.tsiViewOutput = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsiTabOrder = new System.Windows.Forms.ToolStripMenuItem();
            this.tsiFormatRoot = new System.Windows.Forms.ToolStripMenuItem();
            this.tsiAlign = new System.Windows.Forms.ToolStripMenuItem();
            this.tsiAlignLefts = new System.Windows.Forms.ToolStripMenuItem();
            this.tsiAlignCenters = new System.Windows.Forms.ToolStripMenuItem();
            this.tsiAlignRights = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.tsiAlignTops = new System.Windows.Forms.ToolStripMenuItem();
            this.tsiAlignMiddles = new System.Windows.Forms.ToolStripMenuItem();
            this.tsiAlignBottoms = new System.Windows.Forms.ToolStripMenuItem();
            this.tsiMakeSameSize = new System.Windows.Forms.ToolStripMenuItem();
            this.tsiMakeSameWidth = new System.Windows.Forms.ToolStripMenuItem();
            this.tsiMakeSameHeight = new System.Windows.Forms.ToolStripMenuItem();
            this.tsiMakeSameWidthAndHeight = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.tsiHorizontalSpacing = new System.Windows.Forms.ToolStripMenuItem();
            this.tsiHorizSpacingMakeEqual = new System.Windows.Forms.ToolStripMenuItem();
            this.tsiHorizSpacingIncrease = new System.Windows.Forms.ToolStripMenuItem();
            this.tsiHorizSpacingDecrease = new System.Windows.Forms.ToolStripMenuItem();
            this.tsiHorizSpacingRemove = new System.Windows.Forms.ToolStripMenuItem();
            this.tsiVerticalSpacing = new System.Windows.Forms.ToolStripMenuItem();
            this.tsiVertSpacingMakeEqual = new System.Windows.Forms.ToolStripMenuItem();
            this.tsiVertSpacingIncrease = new System.Windows.Forms.ToolStripMenuItem();
            this.tsiVertSpacingDecrease = new System.Windows.Forms.ToolStripMenuItem();
            this.tsiVertSpacingRemove = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.tsiCenterInForm = new System.Windows.Forms.ToolStripMenuItem();
            this.tsiCenterInFormHorizontally = new System.Windows.Forms.ToolStripMenuItem();
            this.tsiCenterInFormVertically = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.tsiOrder = new System.Windows.Forms.ToolStripMenuItem();
            this.tsiOrderBringToFront = new System.Windows.Forms.ToolStripMenuItem();
            this.tsiOrderSendToBack = new System.Windows.Forms.ToolStripMenuItem();
            this.tsiHelpRoot = new System.Windows.Forms.ToolStripMenuItem();
            this.tsiAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.toolboxPanelContainer = new UiTools.WinForms.Designer.WorkspacePanelContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.tcDesigners = new UiTools.WinForms.Designer.Core.TabControlEx();
            this.outputPanelContainer = new UiTools.WinForms.Designer.WorkspacePanelContainer();
            this.propertiesPanelContainer = new UiTools.WinForms.Designer.WorkspacePanelContainer();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tsiToolsRoot
            // 
            this.tsiToolsRoot.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsiOptions});
            this.tsiToolsRoot.Name = "tsiToolsRoot";
            this.tsiToolsRoot.Size = new System.Drawing.Size(58, 24);
            this.tsiToolsRoot.Text = "&Tools";
            // 
            // tsiOptions
            // 
            this.tsiOptions.Name = "tsiOptions";
            this.tsiOptions.Size = new System.Drawing.Size(224, 26);
            this.tsiOptions.Text = "Options...";
            this.tsiOptions.Click += new System.EventHandler(this.tsiOptions_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(203, 6);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(203, 6);
            // 
            // tsiEditRoot
            // 
            this.tsiEditRoot.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsiUnDo,
            this.tsiReDo,
            this.toolStripSeparator3,
            this.tsiCut,
            this.tsiCopy,
            this.tsiPaste,
            this.tsiDelete,
            this.toolStripSeparator4,
            this.tsiSelectAll});
            this.tsiEditRoot.Name = "tsiEditRoot";
            this.tsiEditRoot.Size = new System.Drawing.Size(49, 24);
            this.tsiEditRoot.Text = "&Edit";
            this.tsiEditRoot.DropDownOpening += new System.EventHandler(this.tsiEditRoot_DropDownOpening);
            // 
            // tsiUnDo
            // 
            this.tsiUnDo.Image = global::UiTools.WinForms.Designer.Properties.LightThemeMenuItems.Undo;
            this.tsiUnDo.Name = "tsiUnDo";
            this.tsiUnDo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
            this.tsiUnDo.Size = new System.Drawing.Size(206, 26);
            this.tsiUnDo.Tag = "Undo";
            this.tsiUnDo.Text = "Undo";
            this.tsiUnDo.Click += new System.EventHandler(this.OnMenuClick);
            // 
            // tsiReDo
            // 
            this.tsiReDo.Image = global::UiTools.WinForms.Designer.Properties.LightThemeMenuItems.Redo;
            this.tsiReDo.Name = "tsiReDo";
            this.tsiReDo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
            this.tsiReDo.Size = new System.Drawing.Size(206, 26);
            this.tsiReDo.Tag = "Redo";
            this.tsiReDo.Text = "Redo";
            this.tsiReDo.Click += new System.EventHandler(this.OnMenuClick);
            // 
            // tsiCut
            // 
            this.tsiCut.Image = global::UiTools.WinForms.Designer.Properties.LightThemeMenuItems.Cut;
            this.tsiCut.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsiCut.Name = "tsiCut";
            this.tsiCut.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
            this.tsiCut.Size = new System.Drawing.Size(206, 26);
            this.tsiCut.Tag = "Cut";
            this.tsiCut.Text = "Cut";
            this.tsiCut.Click += new System.EventHandler(this.OnMenuClick);
            // 
            // tsiCopy
            // 
            this.tsiCopy.Image = global::UiTools.WinForms.Designer.Properties.LightThemeMenuItems.Copy;
            this.tsiCopy.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsiCopy.Name = "tsiCopy";
            this.tsiCopy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.tsiCopy.Size = new System.Drawing.Size(206, 26);
            this.tsiCopy.Tag = "Copy";
            this.tsiCopy.Text = "Copy";
            this.tsiCopy.Click += new System.EventHandler(this.OnMenuClick);
            // 
            // tsiPaste
            // 
            this.tsiPaste.Image = global::UiTools.WinForms.Designer.Properties.LightThemeMenuItems.Paste;
            this.tsiPaste.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsiPaste.Name = "tsiPaste";
            this.tsiPaste.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.tsiPaste.Size = new System.Drawing.Size(206, 26);
            this.tsiPaste.Tag = "Paste";
            this.tsiPaste.Text = "Paste";
            this.tsiPaste.Click += new System.EventHandler(this.OnMenuClick);
            // 
            // tsiDelete
            // 
            this.tsiDelete.Image = global::UiTools.WinForms.Designer.Properties.LightThemeMenuItems.Delete;
            this.tsiDelete.Name = "tsiDelete";
            this.tsiDelete.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.tsiDelete.Size = new System.Drawing.Size(206, 26);
            this.tsiDelete.Tag = "Delete";
            this.tsiDelete.Text = "Delete";
            this.tsiDelete.Click += new System.EventHandler(this.OnMenuClick);
            // 
            // tsiSelectAll
            // 
            this.tsiSelectAll.Image = global::UiTools.WinForms.Designer.Properties.LightThemeMenuItems.SelectAll;
            this.tsiSelectAll.Name = "tsiSelectAll";
            this.tsiSelectAll.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
            this.tsiSelectAll.Size = new System.Drawing.Size(206, 26);
            this.tsiSelectAll.Tag = "SelectAll";
            this.tsiSelectAll.Text = "Select All";
            this.tsiSelectAll.Click += new System.EventHandler(this.OnMenuClick);
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsiFileRoot,
            this.tsiEditRoot,
            this.tsiViewRoot,
            this.tsiFormatRoot,
            this.tsiToolsRoot,
            this.tsiHelpRoot});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(982, 28);
            this.menuStrip1.TabIndex = 4;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // tsiFileRoot
            // 
            this.tsiFileRoot.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsiNewRoot,
            this.tsiOpen,
            this.tsiOpenRecent,
            this.tsiSave,
            this.tsiSaveAs,
            this.tsiClose,
            this.toolStripSeparator1,
            this.tsiExit});
            this.tsiFileRoot.Name = "tsiFileRoot";
            this.tsiFileRoot.Size = new System.Drawing.Size(46, 24);
            this.tsiFileRoot.Text = "&File";
            // 
            // tsiNewRoot
            // 
            this.tsiNewRoot.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsiNewForm,
            this.tsiNewUserControl});
            this.tsiNewRoot.Name = "tsiNewRoot";
            this.tsiNewRoot.Size = new System.Drawing.Size(190, 26);
            this.tsiNewRoot.Text = "New";
            // 
            // tsiNewForm
            // 
            this.tsiNewForm.Image = global::UiTools.WinForms.Designer.Properties.LightThemeMenuItems.AddForm;
            this.tsiNewForm.Name = "tsiNewForm";
            this.tsiNewForm.Size = new System.Drawing.Size(170, 26);
            this.tsiNewForm.Text = "Form";
            this.tsiNewForm.Click += new System.EventHandler(this.tsiNewForm_Click);
            // 
            // tsiNewUserControl
            // 
            this.tsiNewUserControl.Image = global::UiTools.WinForms.Designer.Properties.LightThemeMenuItems.AddControl;
            this.tsiNewUserControl.Name = "tsiNewUserControl";
            this.tsiNewUserControl.Size = new System.Drawing.Size(170, 26);
            this.tsiNewUserControl.Text = "UserControl";
            this.tsiNewUserControl.Click += new System.EventHandler(this.tsiNewUserControl_Click);
            // 
            // tsiOpen
            // 
            this.tsiOpen.Image = global::UiTools.WinForms.Designer.Properties.LightThemeMenuItems.OpenFile;
            this.tsiOpen.Name = "tsiOpen";
            this.tsiOpen.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.tsiOpen.Size = new System.Drawing.Size(190, 26);
            this.tsiOpen.Text = "Open...";
            this.tsiOpen.Click += new System.EventHandler(this.tsiOpen_Click);
            // 
            // tsiOpenRecent
            // 
            this.tsiOpenRecent.Name = "tsiOpenRecent";
            this.tsiOpenRecent.Size = new System.Drawing.Size(190, 26);
            this.tsiOpenRecent.Text = "Open recent";
            // 
            // tsiSave
            // 
            this.tsiSave.Image = global::UiTools.WinForms.Designer.Properties.LightThemeMenuItems.Save;
            this.tsiSave.Name = "tsiSave";
            this.tsiSave.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.tsiSave.Size = new System.Drawing.Size(190, 26);
            this.tsiSave.Text = "Save";
            this.tsiSave.Click += new System.EventHandler(this.tsiSave_Click);
            // 
            // tsiSaveAs
            // 
            this.tsiSaveAs.Name = "tsiSaveAs";
            this.tsiSaveAs.Size = new System.Drawing.Size(190, 26);
            this.tsiSaveAs.Text = "Save as...";
            this.tsiSaveAs.Click += new System.EventHandler(this.tsiSaveAs_Click);
            // 
            // tsiClose
            // 
            this.tsiClose.Name = "tsiClose";
            this.tsiClose.Size = new System.Drawing.Size(190, 26);
            this.tsiClose.Text = "Close";
            this.tsiClose.Click += new System.EventHandler(this.tsiClose_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(187, 6);
            // 
            // tsiExit
            // 
            this.tsiExit.Name = "tsiExit";
            this.tsiExit.Size = new System.Drawing.Size(190, 26);
            this.tsiExit.Text = "Exit";
            this.tsiExit.Click += new System.EventHandler(this.tsiExit_Click);
            // 
            // tsiViewRoot
            // 
            this.tsiViewRoot.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsiViewToolbox,
            this.tsiViewProperties,
            this.tsiViewOutput,
            this.toolStripSeparator2,
            this.tsiTabOrder});
            this.tsiViewRoot.Name = "tsiViewRoot";
            this.tsiViewRoot.Size = new System.Drawing.Size(55, 24);
            this.tsiViewRoot.Text = "&View";
            this.tsiViewRoot.DropDownOpening += new System.EventHandler(this.tsiViewRoot_DropDownOpening);
            // 
            // tsiViewToolbox
            // 
            this.tsiViewToolbox.CheckOnClick = true;
            this.tsiViewToolbox.Image = global::UiTools.WinForms.Designer.Properties.LightThemeMenuItems.Toolbox;
            this.tsiViewToolbox.Name = "tsiViewToolbox";
            this.tsiViewToolbox.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt) 
            | System.Windows.Forms.Keys.X)));
            this.tsiViewToolbox.Size = new System.Drawing.Size(261, 26);
            this.tsiViewToolbox.Text = "Toolbox";
            this.tsiViewToolbox.CheckedChanged += new System.EventHandler(this.tsiViewToolbox_CheckedChanged);
            // 
            // tsiViewProperties
            // 
            this.tsiViewProperties.CheckOnClick = true;
            this.tsiViewProperties.Image = global::UiTools.WinForms.Designer.Properties.LightThemeMenuItems.Property;
            this.tsiViewProperties.Name = "tsiViewProperties";
            this.tsiViewProperties.ShortcutKeys = System.Windows.Forms.Keys.F4;
            this.tsiViewProperties.Size = new System.Drawing.Size(261, 26);
            this.tsiViewProperties.Text = "Properties window";
            this.tsiViewProperties.CheckedChanged += new System.EventHandler(this.tsiViewProperties_CheckedChanged);
            // 
            // tsiViewOutput
            // 
            this.tsiViewOutput.CheckOnClick = true;
            this.tsiViewOutput.Image = global::UiTools.WinForms.Designer.Properties.LightThemeMenuItems.Output;
            this.tsiViewOutput.Name = "tsiViewOutput";
            this.tsiViewOutput.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt) 
            | System.Windows.Forms.Keys.O)));
            this.tsiViewOutput.Size = new System.Drawing.Size(261, 26);
            this.tsiViewOutput.Text = "Output panel";
            this.tsiViewOutput.CheckedChanged += new System.EventHandler(this.tsiViewOutput_CheckedChanged);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(258, 6);
            // 
            // tsiTabOrder
            // 
            this.tsiTabOrder.CheckOnClick = true;
            this.tsiTabOrder.Image = global::UiTools.WinForms.Designer.Properties.LightThemeMenuItems.TabOrder;
            this.tsiTabOrder.Name = "tsiTabOrder";
            this.tsiTabOrder.Size = new System.Drawing.Size(261, 26);
            this.tsiTabOrder.Tag = "TabOrder";
            this.tsiTabOrder.Text = "Tab Order";
            this.tsiTabOrder.Click += new System.EventHandler(this.OnMenuClick);
            // 
            // tsiFormatRoot
            // 
            this.tsiFormatRoot.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsiAlign,
            this.tsiMakeSameSize,
            this.toolStripSeparator6,
            this.tsiHorizontalSpacing,
            this.tsiVerticalSpacing,
            this.toolStripSeparator7,
            this.tsiCenterInForm,
            this.toolStripSeparator8,
            this.tsiOrder});
            this.tsiFormatRoot.Name = "tsiFormatRoot";
            this.tsiFormatRoot.Size = new System.Drawing.Size(70, 24);
            this.tsiFormatRoot.Text = "Format";
            this.tsiFormatRoot.DropDownOpening += new System.EventHandler(this.tsiFormatRoot_DropDownOpening);
            // 
            // tsiAlign
            // 
            this.tsiAlign.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsiAlignLefts,
            this.tsiAlignCenters,
            this.tsiAlignRights,
            this.toolStripSeparator5,
            this.tsiAlignTops,
            this.tsiAlignMiddles,
            this.tsiAlignBottoms});
            this.tsiAlign.Name = "tsiAlign";
            this.tsiAlign.Size = new System.Drawing.Size(219, 26);
            this.tsiAlign.Text = "Align";
            // 
            // tsiAlignLefts
            // 
            this.tsiAlignLefts.Image = global::UiTools.WinForms.Designer.Properties.LightThemeMenuItems.AlignLeft;
            this.tsiAlignLefts.Name = "tsiAlignLefts";
            this.tsiAlignLefts.Size = new System.Drawing.Size(148, 26);
            this.tsiAlignLefts.Tag = "AlignLeft";
            this.tsiAlignLefts.Text = "Lefts";
            this.tsiAlignLefts.Click += new System.EventHandler(this.OnMenuClick);
            // 
            // tsiAlignCenters
            // 
            this.tsiAlignCenters.Image = global::UiTools.WinForms.Designer.Properties.LightThemeMenuItems.AlignCenter;
            this.tsiAlignCenters.Name = "tsiAlignCenters";
            this.tsiAlignCenters.Size = new System.Drawing.Size(148, 26);
            this.tsiAlignCenters.Tag = "AlignVerticalCenters";
            this.tsiAlignCenters.Text = "Centers";
            this.tsiAlignCenters.Click += new System.EventHandler(this.OnMenuClick);
            // 
            // tsiAlignRights
            // 
            this.tsiAlignRights.Image = global::UiTools.WinForms.Designer.Properties.LightThemeMenuItems.AlignRight;
            this.tsiAlignRights.Name = "tsiAlignRights";
            this.tsiAlignRights.Size = new System.Drawing.Size(148, 26);
            this.tsiAlignRights.Tag = "AlignRight";
            this.tsiAlignRights.Text = "Rights";
            this.tsiAlignRights.Click += new System.EventHandler(this.OnMenuClick);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(145, 6);
            // 
            // tsiAlignTops
            // 
            this.tsiAlignTops.Image = global::UiTools.WinForms.Designer.Properties.LightThemeMenuItems.AlignTop;
            this.tsiAlignTops.Name = "tsiAlignTops";
            this.tsiAlignTops.Size = new System.Drawing.Size(148, 26);
            this.tsiAlignTops.Tag = "AlignTop";
            this.tsiAlignTops.Text = "Tops";
            this.tsiAlignTops.Click += new System.EventHandler(this.OnMenuClick);
            // 
            // tsiAlignMiddles
            // 
            this.tsiAlignMiddles.Image = global::UiTools.WinForms.Designer.Properties.LightThemeMenuItems.AlignMiddle;
            this.tsiAlignMiddles.Name = "tsiAlignMiddles";
            this.tsiAlignMiddles.Size = new System.Drawing.Size(148, 26);
            this.tsiAlignMiddles.Tag = "AlignHorizontalCenters";
            this.tsiAlignMiddles.Text = "Middles";
            this.tsiAlignMiddles.Click += new System.EventHandler(this.OnMenuClick);
            // 
            // tsiAlignBottoms
            // 
            this.tsiAlignBottoms.Image = global::UiTools.WinForms.Designer.Properties.LightThemeMenuItems.AlignBottom;
            this.tsiAlignBottoms.Name = "tsiAlignBottoms";
            this.tsiAlignBottoms.Size = new System.Drawing.Size(148, 26);
            this.tsiAlignBottoms.Tag = "AlignBottom";
            this.tsiAlignBottoms.Text = "Bottoms";
            this.tsiAlignBottoms.Click += new System.EventHandler(this.OnMenuClick);
            // 
            // tsiMakeSameSize
            // 
            this.tsiMakeSameSize.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsiMakeSameWidth,
            this.tsiMakeSameHeight,
            this.tsiMakeSameWidthAndHeight});
            this.tsiMakeSameSize.Name = "tsiMakeSameSize";
            this.tsiMakeSameSize.Size = new System.Drawing.Size(219, 26);
            this.tsiMakeSameSize.Text = "Make Same Size";
            // 
            // tsiMakeSameWidth
            // 
            this.tsiMakeSameWidth.Image = global::UiTools.WinForms.Designer.Properties.LightThemeMenuItems.MakeSameWidth;
            this.tsiMakeSameWidth.Name = "tsiMakeSameWidth";
            this.tsiMakeSameWidth.Size = new System.Drawing.Size(137, 26);
            this.tsiMakeSameWidth.Tag = "SizeToControlWidth";
            this.tsiMakeSameWidth.Text = "Width";
            this.tsiMakeSameWidth.Click += new System.EventHandler(this.OnMenuClick);
            // 
            // tsiMakeSameHeight
            // 
            this.tsiMakeSameHeight.Image = global::UiTools.WinForms.Designer.Properties.LightThemeMenuItems.MakeSameHeight;
            this.tsiMakeSameHeight.Name = "tsiMakeSameHeight";
            this.tsiMakeSameHeight.Size = new System.Drawing.Size(137, 26);
            this.tsiMakeSameHeight.Tag = "SizeToControlHeight";
            this.tsiMakeSameHeight.Text = "Height";
            this.tsiMakeSameHeight.Click += new System.EventHandler(this.OnMenuClick);
            // 
            // tsiMakeSameWidthAndHeight
            // 
            this.tsiMakeSameWidthAndHeight.Image = global::UiTools.WinForms.Designer.Properties.LightThemeMenuItems.MakeSameWidthAndHeight;
            this.tsiMakeSameWidthAndHeight.Name = "tsiMakeSameWidthAndHeight";
            this.tsiMakeSameWidthAndHeight.Size = new System.Drawing.Size(137, 26);
            this.tsiMakeSameWidthAndHeight.Tag = "SizeToControl";
            this.tsiMakeSameWidthAndHeight.Text = "Both";
            this.tsiMakeSameWidthAndHeight.Click += new System.EventHandler(this.OnMenuClick);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(216, 6);
            // 
            // tsiHorizontalSpacing
            // 
            this.tsiHorizontalSpacing.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsiHorizSpacingMakeEqual,
            this.tsiHorizSpacingIncrease,
            this.tsiHorizSpacingDecrease,
            this.tsiHorizSpacingRemove});
            this.tsiHorizontalSpacing.Name = "tsiHorizontalSpacing";
            this.tsiHorizontalSpacing.Size = new System.Drawing.Size(219, 26);
            this.tsiHorizontalSpacing.Text = "Horizontal Spacing";
            // 
            // tsiHorizSpacingMakeEqual
            // 
            this.tsiHorizSpacingMakeEqual.Image = global::UiTools.WinForms.Designer.Properties.LightThemeMenuItems.HorizontalSpacingMakeEqual;
            this.tsiHorizSpacingMakeEqual.Name = "tsiHorizSpacingMakeEqual";
            this.tsiHorizSpacingMakeEqual.Size = new System.Drawing.Size(169, 26);
            this.tsiHorizSpacingMakeEqual.Tag = "HorizSpaceMakeEqual";
            this.tsiHorizSpacingMakeEqual.Text = "Make Equal";
            this.tsiHorizSpacingMakeEqual.Click += new System.EventHandler(this.OnMenuClick);
            // 
            // tsiHorizSpacingIncrease
            // 
            this.tsiHorizSpacingIncrease.Image = global::UiTools.WinForms.Designer.Properties.LightThemeMenuItems.IncreaseHorizontalSpacing;
            this.tsiHorizSpacingIncrease.Name = "tsiHorizSpacingIncrease";
            this.tsiHorizSpacingIncrease.Size = new System.Drawing.Size(169, 26);
            this.tsiHorizSpacingIncrease.Tag = "HorizSpaceIncrease";
            this.tsiHorizSpacingIncrease.Text = "Increase";
            this.tsiHorizSpacingIncrease.Click += new System.EventHandler(this.OnMenuClick);
            // 
            // tsiHorizSpacingDecrease
            // 
            this.tsiHorizSpacingDecrease.Image = global::UiTools.WinForms.Designer.Properties.LightThemeMenuItems.DecreaseHorizontalSpacing;
            this.tsiHorizSpacingDecrease.Name = "tsiHorizSpacingDecrease";
            this.tsiHorizSpacingDecrease.Size = new System.Drawing.Size(169, 26);
            this.tsiHorizSpacingDecrease.Tag = "HorizSpaceDecrease";
            this.tsiHorizSpacingDecrease.Text = "Decrease";
            this.tsiHorizSpacingDecrease.Click += new System.EventHandler(this.OnMenuClick);
            // 
            // tsiHorizSpacingRemove
            // 
            this.tsiHorizSpacingRemove.Image = global::UiTools.WinForms.Designer.Properties.LightThemeMenuItems.RemoveHorizontalSpacing;
            this.tsiHorizSpacingRemove.Name = "tsiHorizSpacingRemove";
            this.tsiHorizSpacingRemove.Size = new System.Drawing.Size(169, 26);
            this.tsiHorizSpacingRemove.Tag = "HorizSpaceConcatenate";
            this.tsiHorizSpacingRemove.Text = "Remove";
            this.tsiHorizSpacingRemove.Click += new System.EventHandler(this.OnMenuClick);
            // 
            // tsiVerticalSpacing
            // 
            this.tsiVerticalSpacing.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsiVertSpacingMakeEqual,
            this.tsiVertSpacingIncrease,
            this.tsiVertSpacingDecrease,
            this.tsiVertSpacingRemove});
            this.tsiVerticalSpacing.Name = "tsiVerticalSpacing";
            this.tsiVerticalSpacing.Size = new System.Drawing.Size(219, 26);
            this.tsiVerticalSpacing.Text = "Vertical Spacing";
            // 
            // tsiVertSpacingMakeEqual
            // 
            this.tsiVertSpacingMakeEqual.Image = global::UiTools.WinForms.Designer.Properties.LightThemeMenuItems.VerticalSpacingMakeEqual;
            this.tsiVertSpacingMakeEqual.Name = "tsiVertSpacingMakeEqual";
            this.tsiVertSpacingMakeEqual.Size = new System.Drawing.Size(169, 26);
            this.tsiVertSpacingMakeEqual.Tag = "VertSpaceMakeEqual";
            this.tsiVertSpacingMakeEqual.Text = "Make Equal";
            this.tsiVertSpacingMakeEqual.Click += new System.EventHandler(this.OnMenuClick);
            // 
            // tsiVertSpacingIncrease
            // 
            this.tsiVertSpacingIncrease.Image = global::UiTools.WinForms.Designer.Properties.LightThemeMenuItems.IncreaseVerticalSpacing;
            this.tsiVertSpacingIncrease.Name = "tsiVertSpacingIncrease";
            this.tsiVertSpacingIncrease.Size = new System.Drawing.Size(169, 26);
            this.tsiVertSpacingIncrease.Tag = "VertSpaceIncrease";
            this.tsiVertSpacingIncrease.Text = "Increase";
            this.tsiVertSpacingIncrease.Click += new System.EventHandler(this.OnMenuClick);
            // 
            // tsiVertSpacingDecrease
            // 
            this.tsiVertSpacingDecrease.Image = global::UiTools.WinForms.Designer.Properties.LightThemeMenuItems.DecreaseVerticalSpacing;
            this.tsiVertSpacingDecrease.Name = "tsiVertSpacingDecrease";
            this.tsiVertSpacingDecrease.Size = new System.Drawing.Size(169, 26);
            this.tsiVertSpacingDecrease.Tag = "VertSpaceDecrease";
            this.tsiVertSpacingDecrease.Text = "Decrease";
            this.tsiVertSpacingDecrease.Click += new System.EventHandler(this.OnMenuClick);
            // 
            // tsiVertSpacingRemove
            // 
            this.tsiVertSpacingRemove.Image = global::UiTools.WinForms.Designer.Properties.LightThemeMenuItems.RemoveVerticalSpacing;
            this.tsiVertSpacingRemove.Name = "tsiVertSpacingRemove";
            this.tsiVertSpacingRemove.Size = new System.Drawing.Size(169, 26);
            this.tsiVertSpacingRemove.Tag = "VertSpaceConcatenate";
            this.tsiVertSpacingRemove.Text = "Remove";
            this.tsiVertSpacingRemove.Click += new System.EventHandler(this.OnMenuClick);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(216, 6);
            // 
            // tsiCenterInForm
            // 
            this.tsiCenterInForm.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsiCenterInFormHorizontally,
            this.tsiCenterInFormVertically});
            this.tsiCenterInForm.Name = "tsiCenterInForm";
            this.tsiCenterInForm.Size = new System.Drawing.Size(219, 26);
            this.tsiCenterInForm.Text = "Center in Form";
            // 
            // tsiCenterInFormHorizontally
            // 
            this.tsiCenterInFormHorizontally.Image = global::UiTools.WinForms.Designer.Properties.LightThemeMenuItems.CenterHorizontally;
            this.tsiCenterInFormHorizontally.Name = "tsiCenterInFormHorizontally";
            this.tsiCenterInFormHorizontally.Size = new System.Drawing.Size(173, 26);
            this.tsiCenterInFormHorizontally.Tag = "CenterHorizontally";
            this.tsiCenterInFormHorizontally.Text = "Horizontally";
            this.tsiCenterInFormHorizontally.Click += new System.EventHandler(this.OnMenuClick);
            // 
            // tsiCenterInFormVertically
            // 
            this.tsiCenterInFormVertically.Image = global::UiTools.WinForms.Designer.Properties.LightThemeMenuItems.CenterVertically;
            this.tsiCenterInFormVertically.Name = "tsiCenterInFormVertically";
            this.tsiCenterInFormVertically.Size = new System.Drawing.Size(173, 26);
            this.tsiCenterInFormVertically.Tag = "CenterVertically";
            this.tsiCenterInFormVertically.Text = "Vertically";
            this.tsiCenterInFormVertically.Click += new System.EventHandler(this.OnMenuClick);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(216, 6);
            // 
            // tsiOrder
            // 
            this.tsiOrder.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsiOrderBringToFront,
            this.tsiOrderSendToBack});
            this.tsiOrder.Name = "tsiOrder";
            this.tsiOrder.Size = new System.Drawing.Size(219, 26);
            this.tsiOrder.Text = "Order";
            // 
            // tsiOrderBringToFront
            // 
            this.tsiOrderBringToFront.Image = global::UiTools.WinForms.Designer.Properties.LightThemeMenuItems.BringToFront;
            this.tsiOrderBringToFront.Name = "tsiOrderBringToFront";
            this.tsiOrderBringToFront.Size = new System.Drawing.Size(183, 26);
            this.tsiOrderBringToFront.Tag = "BringToFront";
            this.tsiOrderBringToFront.Text = "Bring to Front";
            this.tsiOrderBringToFront.Click += new System.EventHandler(this.OnMenuClick);
            // 
            // tsiOrderSendToBack
            // 
            this.tsiOrderSendToBack.Image = global::UiTools.WinForms.Designer.Properties.LightThemeMenuItems.SendToBack;
            this.tsiOrderSendToBack.Name = "tsiOrderSendToBack";
            this.tsiOrderSendToBack.Size = new System.Drawing.Size(183, 26);
            this.tsiOrderSendToBack.Tag = "SendToBack";
            this.tsiOrderSendToBack.Text = "Send to Back";
            this.tsiOrderSendToBack.Click += new System.EventHandler(this.OnMenuClick);
            // 
            // tsiHelpRoot
            // 
            this.tsiHelpRoot.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tsiHelpRoot.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsiAbout});
            this.tsiHelpRoot.Name = "tsiHelpRoot";
            this.tsiHelpRoot.Size = new System.Drawing.Size(55, 24);
            this.tsiHelpRoot.Text = "Help";
            // 
            // tsiAbout
            // 
            this.tsiAbout.Name = "tsiAbout";
            this.tsiAbout.Size = new System.Drawing.Size(133, 26);
            this.tsiAbout.Text = "About";
            this.tsiAbout.Click += new System.EventHandler(this.tsiAbout_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.BackColor = System.Drawing.Color.Silver;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 28);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.toolboxPanelContainer);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(982, 525);
            this.splitContainer1.SplitterDistance = 204;
            this.splitContainer1.TabIndex = 9;
            // 
            // toolboxPanelContainer
            // 
            this.toolboxPanelContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolboxPanelContainer.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.toolboxPanelContainer.Location = new System.Drawing.Point(0, 0);
            this.toolboxPanelContainer.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.toolboxPanelContainer.Name = "toolboxPanelContainer";
            this.toolboxPanelContainer.Size = new System.Drawing.Size(204, 525);
            this.toolboxPanelContainer.TabIndex = 0;
            this.toolboxPanelContainer.Title = "Toolbox";
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.BackColor = System.Drawing.SystemColors.Window;
            this.splitContainer2.Panel1.Controls.Add(this.splitContainer3);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.propertiesPanelContainer);
            this.splitContainer2.Size = new System.Drawing.Size(774, 525);
            this.splitContainer2.SplitterDistance = 560;
            this.splitContainer2.TabIndex = 3;
            // 
            // splitContainer3
            // 
            this.splitContainer3.BackColor = System.Drawing.Color.Silver;
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.tcDesigners);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.outputPanelContainer);
            this.splitContainer3.Size = new System.Drawing.Size(560, 525);
            this.splitContainer3.SplitterDistance = 398;
            this.splitContainer3.TabIndex = 0;
            // 
            // tcDesigners
            // 
            this.tcDesigners.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tcDesigners.Location = new System.Drawing.Point(0, 0);
            this.tcDesigners.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tcDesigners.Name = "tcDesigners";
            this.tcDesigners.SelectedIndex = 0;
            this.tcDesigners.Size = new System.Drawing.Size(560, 398);
            this.tcDesigners.TabIndex = 0;
            // 
            // outputPanelContainer
            // 
            this.outputPanelContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.outputPanelContainer.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.outputPanelContainer.Location = new System.Drawing.Point(0, 0);
            this.outputPanelContainer.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.outputPanelContainer.Name = "outputPanelContainer";
            this.outputPanelContainer.Size = new System.Drawing.Size(560, 123);
            this.outputPanelContainer.TabIndex = 0;
            this.outputPanelContainer.Title = "Output";
            // 
            // propertiesPanelContainer
            // 
            this.propertiesPanelContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertiesPanelContainer.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.propertiesPanelContainer.Location = new System.Drawing.Point(0, 0);
            this.propertiesPanelContainer.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.propertiesPanelContainer.Name = "propertiesPanelContainer";
            this.propertiesPanelContainer.Size = new System.Drawing.Size(210, 525);
            this.propertiesPanelContainer.TabIndex = 0;
            this.propertiesPanelContainer.Title = "Properties";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(982, 553);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "MainForm";
            this.Text = "WinForms Designer";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

	}
		private ToolStripMenuItem tsiToolsRoot;
		private ToolStripSeparator toolStripSeparator4;
		private ToolStripMenuItem tsiDelete;
		private ToolStripMenuItem tsiPaste;
		private ToolStripMenuItem tsiCopy;
		private ToolStripMenuItem tsiCut;
		private ToolStripSeparator toolStripSeparator3;
		private ToolStripMenuItem tsiReDo;
		private ToolStripMenuItem tsiUnDo;
		private ToolStripMenuItem tsiEditRoot;
		private Core.ThemedMenuStrip menuStrip1;
        private ToolStripMenuItem tsiFileRoot;
        private ToolStripMenuItem tsiNewRoot;
        private ToolStripMenuItem tsiOpen;
        private ToolStripMenuItem tsiNewForm;
        private ToolStripMenuItem tsiNewUserControl;
        private ToolStripMenuItem tsiSave;
        private ToolStripMenuItem tsiClose;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem tsiExit;
        private ToolStripMenuItem tsiSelectAll;
        private SplitContainer splitContainer1;
        private ToolStripMenuItem tsiViewRoot;
        private ToolStripMenuItem tsiViewToolbox;
        private ToolStripMenuItem tsiViewProperties;
        private ToolStripMenuItem tsiViewOutput;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem tsiTabOrder;
        private SplitContainer splitContainer2;
        private SplitContainer splitContainer3;
        private Core.TabControlEx tcDesigners;
        private ToolStripMenuItem tsiOpenRecent;
        private ToolStripMenuItem tsiOptions;
        private ToolStripMenuItem tsiFormatRoot;
        private ToolStripMenuItem tsiAlign;
        private ToolStripMenuItem tsiMakeSameSize;
        private ToolStripMenuItem tsiHorizontalSpacing;
        private ToolStripMenuItem tsiVerticalSpacing;
        private ToolStripMenuItem tsiCenterInForm;
        private ToolStripMenuItem tsiOrder;
        private ToolStripMenuItem tsiAlignLefts;
        private ToolStripMenuItem tsiAlignCenters;
        private ToolStripMenuItem tsiAlignRights;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripMenuItem tsiAlignTops;
        private ToolStripMenuItem tsiAlignMiddles;
        private ToolStripMenuItem tsiAlignBottoms;
        private ToolStripMenuItem tsiMakeSameWidth;
        private ToolStripMenuItem tsiMakeSameHeight;
        private ToolStripMenuItem tsiMakeSameWidthAndHeight;
        private ToolStripMenuItem tsiHorizSpacingMakeEqual;
        private ToolStripMenuItem tsiHorizSpacingIncrease;
        private ToolStripMenuItem tsiHorizSpacingDecrease;
        private ToolStripMenuItem tsiHorizSpacingRemove;
        private ToolStripMenuItem tsiVertSpacingMakeEqual;
        private ToolStripMenuItem tsiVertSpacingIncrease;
        private ToolStripMenuItem tsiVertSpacingDecrease;
        private ToolStripMenuItem tsiVertSpacingRemove;
        private ToolStripMenuItem tsiCenterInFormHorizontally;
        private ToolStripMenuItem tsiCenterInFormVertically;
        private ToolStripMenuItem tsiOrderBringToFront;
        private ToolStripMenuItem tsiOrderSendToBack;
        private ToolStripSeparator toolStripSeparator6;
        private ToolStripSeparator toolStripSeparator7;
        private ToolStripSeparator toolStripSeparator8;
        private ToolStripMenuItem tsiSaveAs;
        private ToolStripMenuItem tsiHelpRoot;
        private ToolStripMenuItem tsiAbout;
        private WorkspacePanelContainer toolboxPanelContainer;
        private WorkspacePanelContainer outputPanelContainer;
        private WorkspacePanelContainer propertiesPanelContainer;
    }
}
