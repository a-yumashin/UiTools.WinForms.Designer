using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UiTools.WinForms.Designer.Core;
using UiTools.WinForms.Designer.Properties;

namespace UiTools.WinForms.Designer
{
    public partial class MainForm : ThemedForm
    {
        private const string NOT_SAVED_YET = "(not saved yet)";
        private KnownUiThemes knownUiThemes;

        private MyDesignSurfaceManager myDesignSurfaceManager;
        private readonly List<DesignerWorkspace> workspaces = new List<DesignerWorkspace>();
        private readonly IRecentFilesManager recentFilesManager;
        private readonly string vsixVersion;

        /*
         * NOTE:
           splitContainer1: toolboxPanelContainer + splitContainer2 (vertical)
           splitContainer2: splitContainer3 + propertiesPanelContainer (vertical)
           splitContainer3: tcDesigners + outputPanelContainer (horizontal)
         */

        public MainForm(string vsixVersion)
		{
			InitializeComponent();

            Icon = Resources.AppLogo;
            tsiOptions.Image = Resources.OptionsDialog.ToBitmap();
            this.vsixVersion = vsixVersion;
            if (!string.IsNullOrEmpty(vsixVersion))
                Text += " (running as VS Code extension)";
            LoadSettings();
            tcDesigners.ShowToolTips = false;
            tcDesigners.TabPageCloseRequested += OnTabPageCloseRequested;
            UpdateEnabledForSaveAndCloseMenuItems();

            //(tsiOpenRecent.DropDown as ToolStripDropDownMenu).ShowImageMargin = false;
            recentFilesManager = new RecentFilesManager(tsiOpenRecent,
                AppSettings.Instance.MainFormMRUList,
                AppSettings.Instance.MainFormMRUListMaxSize,
                AppSettings.Instance.Save);
            recentFilesManager.RecentFileClicked += OnRecentFileClicked;

            splitContainer1.Panel1Collapsed = true;
            splitContainer2.Panel2Collapsed = true;
            splitContainer3.Panel2Collapsed = true;

            toolboxPanelContainer.Closed += (s, e) => tsiViewToolbox.Checked = false;
            propertiesPanelContainer.Closed += (s, e) => tsiViewProperties.Checked = false;
            outputPanelContainer.Closed += (s, e) => tsiViewOutput.Checked = false;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            // NOTE: 'base.OnHandleCreated(e);' should go BEFORE 'ThemeApplier.Apply()' - otherwise ThemeApplier.Apply() will be called twice
            //       (from ThemedForm.OnHandleCreated() as well).
            knownUiThemes = KnownUiThemes.Load();
            if (knownUiThemes != null)
            {
                CommonStuff.CurrentUiTheme = knownUiThemes.GetUiTheme(AppSettings.Instance.UiThemeName);
                if (CommonStuff.CurrentUiTheme != null)
                {
                    ThemeApplier.Apply(this, CommonStuff.CurrentUiTheme, applyProcessWideSettings: true);
                    ApplyUiThemeToMenuItemImages();
                    ApplyUiThemeToRecentFilesManager();
                }
                else
                {
                    recentFilesManager.DeleteIcon = Resources.Remove;
                    recentFilesManager.OpenFolderIcon = Resources.OpenContainingFolder;
                }
            }
        }

        private void SaveSettings()
        {
            var fs = new FormSettings();
            if (WindowState == FormWindowState.Normal)
            {
                fs.Left = Left;
                fs.Top = Top;
                fs.Width = Width;
                fs.Height = Height;
            }
            else
            {
                // [MSDN] "The value of the RestoreBounds property is valid only when the WindowState property of the Form class is NOT equal to Normal."
                fs.Left = RestoreBounds.Left;
                fs.Top = RestoreBounds.Top;
                fs.Width = RestoreBounds.Width;
                fs.Height = RestoreBounds.Height;
            }
            fs.WindowState = WindowState;
            AppSettings.Instance.MainFormSettings = fs;

            SaveWorkspaceSettings();

            AppSettings.Instance.Save();
        }

        private void LoadSettings()
        {
            var fs = AppSettings.Instance.MainFormSettings;
            Left = fs.Left;
            Top = fs.Top;
            Width = fs.Width;
            Height = fs.Height;
            WindowState = fs.WindowState;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            tcDesigners.Selected += OnTabPageSelected;
            myDesignSurfaceManager = new MyDesignSurfaceManager();
        }

        private void OnTabPageSelected(object sender, TabControlEventArgs e)
        {
            MessageLogger.Init(GetCurrentOutputPanel(), AppSettings.Instance.LogLevel);

            if (e.TabPageIndex >= 0)
            {
                toolboxPanelContainer.WorkspacePanel = GetCurrentToolbox();
                propertiesPanelContainer.WorkspacePanel = GetCurrentPropertiesExplorer();
                outputPanelContainer.WorkspacePanel = GetCurrentOutputPanel();
                GetCurrentOutputPanel().WordWrap = AppSettings.Instance.WorkspaceSettings.OutputWordWrap;
                GetCurrentOutputPanel().ShowTimestamp = AppSettings.Instance.WorkspaceSettings.OutputShowTimestamp;

                tsiUnDo.ToolTipText = GetCurrentWorkspace().LastUndoTransaction;
                tsiReDo.ToolTipText = GetCurrentWorkspace().LastRedoTransaction;
            }
            else
            {
                tsiUnDo.ToolTipText = null;
                tsiReDo.ToolTipText = null;
            }
        }

        private DesignerWorkspace GetCurrentWorkspace()
        {
            if (tcDesigners.SelectedIndex < 0 || tcDesigners.SelectedIndex >= workspaces.Count)
                return null;
            return workspaces[tcDesigners.SelectedIndex];
        }

        private DesignSurfaceEx GetCurrentDesigner()
		{
			return GetCurrentWorkspace()?.Designer;
		}

        private ToolboxTreeView GetCurrentToolbox()
        {
            return GetCurrentWorkspace()?.Toolbox;
        }

        private ComponentPropertiesExplorer GetCurrentPropertiesExplorer()
        {
            return GetCurrentWorkspace()?.PropertiesExplorer;
        }

        private OutputPanel GetCurrentOutputPanel()
        {
            return GetCurrentWorkspace()?.OutputPanel;
        }

        private void OnMenuClick(object sender, EventArgs e)
		{
            var workspace = GetCurrentWorkspace();
            if (workspace == null)
                return;
            if (workspace.IsDesignerActive)
            {
                // Designer-related menu items
                DesignSurfaceEx designer = GetCurrentDesigner();
                if (designer != null)
                    designer.ProcessMenuItemClick((sender as ToolStripMenuItem).Tag);
            }
        }

        private DesignerWorkspace CreateWorkspace(IDesignerControl designerControl)
        {
            var ws = new DesignerWorkspace(CreateDesignSurface(), CreateToolbox(), CreatePropertiesExplorer(), CreateOutputPanel(), designerControl);
            ws.RemoveUnnecessaryUsingsOnSave =
                AppSettings.Instance.RemoveUnnecessaryUsings; // can be changed later (from the Options form), so there's no much point to inject it via constructor
            return ws;
        }

        private ToolboxTreeView CreateToolbox()
        {
            var toolbox = new ToolboxTreeView() { Dock = DockStyle.Fill };
            toolbox.HandleCreated += (s, e) =>
            {
                if (CommonStuff.CurrentUiTheme != null)
                    ThemeApplier.Apply(toolbox, CommonStuff.CurrentUiTheme);
            };
            return toolbox;
        }

        private ComponentPropertiesExplorer CreatePropertiesExplorer()
        {
            var pe = new ComponentPropertiesExplorer { Dock = DockStyle.Fill };
            pe.HandleCreated += (s, e) =>
            {
                if (CommonStuff.CurrentUiTheme != null)
                    ThemeApplier.Apply(pe, CommonStuff.CurrentUiTheme);
            };
            return pe;
        }

        private OutputPanel CreateOutputPanel()
        {
            var op = new OutputPanel { Dock = DockStyle.Fill };
            op.HandleCreated += (s, e) =>
            {
                if (CommonStuff.CurrentUiTheme != null)
                    ThemeApplier.Apply(op, CommonStuff.CurrentUiTheme);
            };
            return op;
        }

        private DesignSurfaceEx CreateDesignSurface()
		{
            //var designer = new DesignSurfaceEx();
            var designer = (DesignSurfaceEx)myDesignSurfaceManager.CreateDesignSurface(); // creation via DesignSurfaceManager should ensure the injection of required services
            myDesignSurfaceManager.ActiveDesignSurface = designer;
            designer.GetUndoEngine().Enabled = true;

            var gridSize = AppSettings.Instance.GridSize;
            if (AppSettings.Instance.AlignControlsMode == AlignControlsModeEnum.UseSnapLines)
                designer.UseSnapLines();
            else if (AppSettings.Instance.AlignControlsMode == AlignControlsModeEnum.UseGrid)
                designer.UseGridWithoutSnapping(new Size(gridSize, gridSize));
            else if (AppSettings.Instance.AlignControlsMode == AlignControlsModeEnum.SnapToGrid)
                designer.UseGrid(new Size(gridSize, gridSize));
            else // AlignControlsModeEnum.AlignByHand
                designer.UseNoGuides();

            return designer;
        }

        private void tsiNewForm_Click(object sender, EventArgs e)
        {
            CreateNewRootComponent(typeof(Form));
        }

        private void tsiNewUserControl_Click(object sender, EventArgs e)
        {
            CreateNewRootComponent(typeof(UserControl));
        }

        private void tsiOpen_Click(object sender, EventArgs e)
        {
            OpenExistingDesignerFile();
        }

        private void PrepareWorkspace(string tabPageCaption, string tabPageToolTipText)
        {
            TabPage tp = new TabPage(tabPageCaption) { ToolTipText = tabPageToolTipText, Padding = Padding.Empty };
            var dc = new MyDesignerControl { Dock = DockStyle.Fill };
            tp.Controls.Add(dc);
            tp.HandleCreated += (s, e) =>
            {
                if (CommonStuff.CurrentUiTheme != null)
                    ThemeApplier.Apply(tp, CommonStuff.CurrentUiTheme);
            };
            var workspace = CreateWorkspace(dc);
            workspaces.Add(workspace);
            workspace.PropertiesWindowNeeded += Workspace_PropertiesWindowNeeded;
            workspace.UndoStackChanged += Workspace_UndoStackChanged;
            workspace.RedoStackChanged += Workspace_RedoStackChanged;
            workspace.OutputPanel.WordWrapChanged += OutputPanel_WordWrapChanged;
            workspace.OutputPanel.ShowTimestampChanged += OutputPanel_ShowTimestampChanged;
            bool isFirstTab = tcDesigners.TabPages.Count == 0;
            tcDesigners.TabPages.Add(tp);
            tcDesigners.SelectedTab = tp;
            if (isFirstTab)
            {
                //BeginInvoke(new MethodInvoker(() => ApplyWorkspaceSettings()));
                ApplyWorkspaceSettings();
                OnTabPageSelected(tcDesigners, new TabControlEventArgs(tp, 0, TabControlAction.Selected)); // force event (for the very first tab it doesn't get fired!)
            }
            tcDesigners.Refresh();
            UpdateEnabledForSaveAndCloseMenuItems();
            MessageLogger.Init(GetCurrentOutputPanel(), AppSettings.Instance.LogLevel);
        }

        private void OutputPanel_ShowTimestampChanged(object sender, EventArgs e)
        {
            AppSettings.Instance.WorkspaceSettings.OutputShowTimestamp = GetCurrentOutputPanel().ShowTimestamp;
            AppSettings.Instance.Save();
        }

        private void OutputPanel_WordWrapChanged(object sender, EventArgs e)
        {
            AppSettings.Instance.WorkspaceSettings.OutputWordWrap = GetCurrentOutputPanel().WordWrap;
            AppSettings.Instance.Save();
        }

        private void Workspace_RedoStackChanged(object sender, string lastRedoTransaction)
        {
            tsiReDo.ToolTipText = lastRedoTransaction;
        }

        private void Workspace_UndoStackChanged(object sender, string lastUndoTransaction)
        {
            tsiUnDo.ToolTipText = lastUndoTransaction;
        }

        private void Workspace_PropertiesWindowNeeded(object sender, EventArgs e)
        {
            tsiViewProperties.Checked = true;
        }

        private void CreateNewRootComponent(Type rootComponentType)
        {
            using (var frm = new DesignerCsFileContextForm())
            {
                frm.Text = $"Create new designer file ({rootComponentType.Name})";
                frm.RootComponentTypeName = rootComponentType.Name;
                frm.Icon = Icon.FromHandle(PickMenuItemImage(rootComponentType == typeof(Form) ? "AddForm" : "AddControl").GetHicon());
                frm.Initiator = DesignerCsFileContextForm.InitiatorEnum.FileNewMenu;
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    PrepareWorkspace($"(new {rootComponentType.Name})", NOT_SAVED_YET);
                    GetCurrentWorkspace().CreateNewRootComponent(
                        rootComponentType,
                        frm.DesignerCsFileContext,
                        new Font(AppSettings.Instance.DefaultNewRootComponentFontName, AppSettings.Instance.DefaultNewRootComponentFontSize),
                        new Font(AppSettings.Instance.SourceCodeViewerFontName, AppSettings.Instance.SourceCodeViewerFontSize));
                    var undoEngine = GetCurrentDesigner().GetUndoEngine();
                    undoEngine.Clear();
                }
            }
        }

        private void OpenExistingDesignerFile()
        {
            using (var frm = new DesignerCsFileContextForm())
            {
                frm.Text = "Open existing designer file";
                frm.Icon = Icon.FromHandle(PickMenuItemImage("OpenFile").GetHicon());
                frm.Initiator = DesignerCsFileContextForm.InitiatorEnum.FileOpenMenu;
                if (frm.ShowDialog(this) == DialogResult.OK)
                    OpenExistingDesignerFileInner(frm.DesignerCsFileContext);
            }
        }

        public void OpenExistingDesignerFileFromVsCode(string designerCsFileFullPath)
        {
            using (var frm = new DesignerCsFileContextForm())
            {
                frm.Text = "Opening existing designer file from VS Code";
                frm.Icon = Icon.FromHandle(PickMenuItemImage("OpenFile").GetHicon());
                frm.Initiator = DesignerCsFileContextForm.InitiatorEnum.VSCode;
                frm.SetDesignerCsFileFullPath(designerCsFileFullPath);
                if (frm.ShowDialog(this) == DialogResult.OK)
                    OpenExistingDesignerFileInner(frm.DesignerCsFileContext);
            }
        }

        private void OnRecentFileClicked(object sender, string recentFilePath)
        {
            if (File.Exists(recentFilePath))
            {
                if ((ModifierKeys & Keys.Shift) != 0)
                {
                    var key = recentFilePath.ToLower();
                    var dfContext = AppSettings.Instance.KnownDesignerCsFileContexts.ContainsKey(key)
                        ? AppSettings.Instance.KnownDesignerCsFileContexts[key]
                        : new DesignerCsFileContext
                        {
                            DesignerCsFileFullPath = recentFilePath,
                            CsProjectFileFullPath = CsProjectFileLocator.FindContainingCsProjFile(recentFilePath)
                        };
                    OpenExistingDesignerFileInner(dfContext);
                }
                else
                {
                    using (var frm = new DesignerCsFileContextForm())
                    {
                        frm.Text = "Opening recent designer file";
                        frm.Icon = Icon.FromHandle(PickMenuItemImage("OpenFile").GetHicon());
                        frm.Initiator = DesignerCsFileContextForm.InitiatorEnum.FileOpenMenu;
                        frm.SetDesignerCsFileFullPath(recentFilePath);
                        if (frm.ShowDialog(this) == DialogResult.OK)
                            OpenExistingDesignerFileInner(frm.DesignerCsFileContext);
                    }
                }
            }
            else
                recentFilesManager.HandleNonExistingFileInList(recentFilePath);
        }

        private void OpenExistingDesignerFileInner(DesignerCsFileContext dfContext)
        {
            recentFilesManager.AddOrMoveFileToTheTop(dfContext.DesignerCsFileFullPath);
            // Check if this file is already open:
            var tp = tcDesigners.TabPages.Cast<TabPage>()
                .FirstOrDefault(p => string.Compare(p.ToolTipText, dfContext.DesignerCsFileFullPath, ignoreCase: true) == 0);
            if (tp != null)
            {
                tcDesigners.SelectedTab = tp;
                return;
            }
            // If not - open it in designer:
            PrepareWorkspace(Path.GetFileName(dfContext.DesignerCsFileFullPath), dfContext.DesignerCsFileFullPath);
            GetCurrentWorkspace().OpenExistingDesignerFile(
                dfContext,
                new Font(AppSettings.Instance.SourceCodeViewerFontName, AppSettings.Instance.SourceCodeViewerFontSize));
            var undoEngine = GetCurrentDesigner().GetUndoEngine();
            undoEngine.Clear();
        }

        private void UpdateEnabledForSaveAndCloseMenuItems()
        {
            tsiSave.Enabled = tsiSaveAs.Enabled = tsiClose.Enabled = GetCurrentWorkspace() != null;
        }

        private void tsiSave_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void tsiSaveAs_Click(object sender, EventArgs e)
        {
            SaveAs();
        }

        private void Save()
        {
            if (tcDesigners.TabCount == 0)
                return;
            var workspace = GetCurrentWorkspace();
            var currentFilePath = tcDesigners.TabPages[tcDesigners.SelectedIndex].ToolTipText;
            string code = null;
            if (currentFilePath == NOT_SAVED_YET)
            {
                using (var dlg = new SaveFileDialog())
                {
                    var rootComponentType = GetCurrentDesigner().GetDesignerHost().RootComponent;
                    dlg.Title = $"Saving {rootComponentType.GetType().Name} '{rootComponentType.Site.Name}'";
                    dlg.Filter = "WinForms Designer Files|*.Designer.cs;*.designer.cs|C# Files|*.cs|All files|*.*";
                    dlg.FilterIndex = 0;
                    dlg.FileName = rootComponentType.Site.Name;
                    if (!string.IsNullOrEmpty(workspace.DesignerCsFileContext.CsProjectFileFullPath))
                        dlg.InitialDirectory = Path.GetDirectoryName(workspace.DesignerCsFileContext.CsProjectFileFullPath);
                    var result = dlg.ShowDialog(this);
                    if (result == DialogResult.OK)
                    {
                        if (!GenerateCodeFromDesigner(workspace, ref code))
                            return;
                        var filePath = dlg.FileName;
                        File.WriteAllText(filePath, code, CommonStuff.Utf8WithoutBom);
                        var tp = tcDesigners.TabPages[tcDesigners.SelectedIndex];
                        tp.Text = Path.GetFileName(filePath);
                        tp.ToolTipText = filePath;
                        workspace.IsDirty = false;
                        recentFilesManager.AddOrMoveFileToTheTop(filePath);
                        // Update AppSettings.Instance.KnownDesignerCsFileContexts:
                        workspace.DesignerCsFileContext.DesignerCsFileFullPath = filePath;
                        var key = filePath.ToLower();
                        AppSettings.Instance.KnownDesignerCsFileContexts[key] = workspace.DesignerCsFileContext;
                        AppSettings.Instance.Save();
                        MessageLogger.Log(this, $"Saved as '{filePath}'");
                        
                        CreateMainFileIfSoDesired(filePath,
                            workspace.DesignerCsFileContext.Namespace,
                            workspace.Designer.GetDesignerHost().RootComponent.Site.Name,
                            workspace.Designer.GetDesignerHost().RootComponent.GetType().Name);
                    }
                }
            }
            else
            {
                if (!GenerateCodeFromDesigner(workspace, ref code))
                    return;
                File.WriteAllText(currentFilePath, code, CommonStuff.Utf8WithoutBom);
                workspace.IsDirty = false;
                MessageLogger.LogVerbose(this, $"Saved '{currentFilePath}'");
            }
        }

        private void SaveAs()
        {
            if (tcDesigners.TabCount == 0)
                return;
            var workspace = GetCurrentWorkspace();
            var currentFilePath = tcDesigners.TabPages[tcDesigners.SelectedIndex].ToolTipText;
            using (var dlg = new SaveFileDialog())
            {
                var rootComponentType = GetCurrentDesigner().GetDesignerHost().RootComponent;
                dlg.Title = $"Saving {rootComponentType.GetType().Name} '{rootComponentType.Site.Name}'";
                dlg.Filter = "WinForms Designer Files|*.Designer.cs;*.designer.cs|C# Files|*.cs|All files|*.*";
                dlg.FilterIndex = 0;
                if (currentFilePath == NOT_SAVED_YET)
                {
                    if (!string.IsNullOrEmpty(workspace.DesignerCsFileContext.CsProjectFileFullPath))
                        dlg.InitialDirectory = Path.GetDirectoryName(workspace.DesignerCsFileContext.CsProjectFileFullPath);
                    dlg.FileName = rootComponentType.Site.Name;
                }
                else
                {
                    dlg.InitialDirectory = Path.GetDirectoryName(currentFilePath);
                    dlg.FileName = Path.GetFileName(currentFilePath);
                }
                var result = dlg.ShowDialog(this);
                if (result == DialogResult.OK)
                {
                    string code = null;
                    if (!GenerateCodeFromDesigner(workspace, ref code))
                        return;
                    var filePath = dlg.FileName;
                    File.WriteAllText(filePath, code, CommonStuff.Utf8WithoutBom);
                    var tp = tcDesigners.TabPages[tcDesigners.SelectedIndex];
                    tp.Text = Path.GetFileName(filePath);
                    tp.ToolTipText = filePath;
                    workspace.IsDirty = false;
                    recentFilesManager.AddOrMoveFileToTheTop(filePath);
                    if (!string.Equals(currentFilePath, filePath, StringComparison.OrdinalIgnoreCase))
                    {
                        // Update AppSettings.Instance.KnownDesignerCsFileContexts:
                        workspace.DesignerCsFileContext.DesignerCsFileFullPath = filePath;
                        var key = filePath.ToLower();
                        AppSettings.Instance.KnownDesignerCsFileContexts[key] = workspace.DesignerCsFileContext;
                        AppSettings.Instance.Save();
                    }
                    MessageLogger.Log(this, $"Saved as '{filePath}'");

                    if (currentFilePath == NOT_SAVED_YET)
                        CreateMainFileIfSoDesired(filePath,
                            workspace.DesignerCsFileContext.Namespace,
                            workspace.Designer.GetDesignerHost().RootComponent.Site.Name,
                            workspace.Designer.GetDesignerHost().RootComponent.GetType().Name);
                }
            }
        }

        private void CreateMainFileIfSoDesired(string designerCsFilePath, string ns, string componentName, string componentType)
        {
            var mainFilePath = CommonStuff.MainCsFilePathFromDesignerCsFilePath(designerCsFilePath);
            if (MessageBox.Show(this,
                $"Designer file '{Path.GetFileName(designerCsFilePath)}' saved successfully.\n" +
                $"Would you like to generate the companion '{Path.GetFileName(mainFilePath)}' file in the same folder?",
                Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

            var mainFileContents = $@"using System;
using System.Windows.Forms;

namespace {ns}
{{
    public partial class {componentName} : {componentType}
    {{
        public {componentName}()
        {{
            InitializeComponent();
        }}
    }}
}}";
            try
            {
                File.WriteAllText(mainFilePath, mainFileContents, CommonStuff.Utf8WithoutBom);
                var msg = $"Successfully created file '{mainFilePath}'.";
                MessageLogger.Log(this, msg);
                MessageBox.Show(this, msg, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                var msg = $"Failed to create file '{mainFilePath}': {ex.Message}";
                MessageLogger.LogError(this, msg, ex);
                MessageBox.Show(this, msg, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool GenerateCodeFromDesigner(DesignerWorkspace workspace, ref string code)
        {
            try
            {
                code = workspace.GenerateCodeFromDesigner((status) => MessageLogger.LogVerbose(this, status));
                return true;
            }
            catch (Exception ex)
            {
                MessageLogger.LogError(this, ex.Message, ex);
                return false;
            }
        }

        private void ApplyWorkspaceSettings()
        {
            tsiViewToolbox.Checked = AppSettings.Instance.WorkspaceSettings.ViewToolbox;
            tsiViewProperties.Checked = AppSettings.Instance.WorkspaceSettings.ViewProperties;
            tsiViewOutput.Checked = AppSettings.Instance.WorkspaceSettings.ViewOutput;
            // Force update of Panel1Collapsed and Panel2Collapsed properties:
            tsiViewToolbox_CheckedChanged(tsiViewToolbox, EventArgs.Empty);
            tsiViewProperties_CheckedChanged(tsiViewProperties, EventArgs.Empty);
            tsiViewOutput_CheckedChanged(tsiViewOutput, EventArgs.Empty);

            splitContainer1.SplitterDistance = AppSettings.Instance.WorkspaceSettings.ToolboxWidth;
            splitContainer2.SplitterDistance = splitContainer2.Width - AppSettings.Instance.WorkspaceSettings.PropertiesWidth;
            splitContainer3.SplitterDistance = splitContainer3.Height - AppSettings.Instance.WorkspaceSettings.OutputHeight;

            GetCurrentOutputPanel().WordWrap = AppSettings.Instance.WorkspaceSettings.OutputWordWrap;
            GetCurrentOutputPanel().ShowTimestamp = AppSettings.Instance.WorkspaceSettings.OutputShowTimestamp;

            //Refresh(); // to redraw all SplitContainers
        }

        private void SaveWorkspaceSettings()
        {
            if (tcDesigners.TabPages.Count == 0)
                return;

            AppSettings.Instance.WorkspaceSettings.ToolboxWidth = splitContainer1.SplitterDistance;
            AppSettings.Instance.WorkspaceSettings.PropertiesWidth = splitContainer2.Width - splitContainer2.SplitterDistance;
            AppSettings.Instance.WorkspaceSettings.OutputHeight = splitContainer3.Height - splitContainer3.SplitterDistance;

            AppSettings.Instance.WorkspaceSettings.ViewToolbox = tsiViewToolbox.Checked;
            AppSettings.Instance.WorkspaceSettings.ViewProperties = tsiViewProperties.Checked;
            AppSettings.Instance.WorkspaceSettings.ViewOutput = tsiViewOutput.Checked;

            AppSettings.Instance.WorkspaceSettings.OutputWordWrap = GetCurrentOutputPanel().WordWrap;
            AppSettings.Instance.WorkspaceSettings.OutputShowTimestamp = GetCurrentOutputPanel().ShowTimestamp;

            AppSettings.Instance.Save();
        }

        #region Closing stuff

        private void OnTabPageCloseRequested(object sender, int tabPageIndex)
        {
            // TabPage is closed with mouse click on "x" icon
            CloseTabPage(tabPageIndex);
            UpdateEnabledForSaveAndCloseMenuItems();
        }

        private void tsiClose_Click(object sender, EventArgs e)
        {
            // TabPage is closed with "File/Close" menu click
            if (tcDesigners.TabCount == 0)
                return;
            CloseTabPage(tcDesigners.SelectedIndex);
            UpdateEnabledForSaveAndCloseMenuItems();
        }

        private void CloseTabPage(int tabPageIndex)
        {
            if (workspaces[tabPageIndex].IsDirty)
            {
                var result = MessageBox.Show(this, "Current designer contains unsaved changes.\nAre you sure you want to close it anyway?",
                    Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (result == DialogResult.No)
                    return;
            }

            SaveWorkspaceSettings();
            RemoveWorkspace(tabPageIndex);
            tcDesigners.TabPages.RemoveAt(tabPageIndex);
            if (tcDesigners.TabPages.Count == 0)
                HideAllPanels();
        }

        private void tsiExit_Click(object sender, EventArgs e)
        {
            // "File/Exit" menu is clicked
            Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // Form is closing because of one of the following reasons:
            // - "File/Exit" menu is clicked
            // - form's "Close" button (in the ControlBox) is clicked
            // - Alt+F4 key combination is pressed
            if (workspaces.Any(wsp => wsp.IsDirty))
            {
                var result = MessageBox.Show(this, "One or more designers contain unsaved changes.\nAre you sure you want to exit anyway?",
                    Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }

            SaveSettings();
            for (int tabPageIndex = tcDesigners.TabPages.Count - 1; tabPageIndex >= 0; tabPageIndex--)
            {
                RemoveWorkspace(tabPageIndex);
                //tcDesigners.TabPages.RemoveAt(tcDesigners.SelectedIndex);
            }
        }

        private void RemoveWorkspace(int tabIndex)
        {
            var workspace = workspaces[tabIndex];
            workspace.PropertiesWindowNeeded -= Workspace_PropertiesWindowNeeded;
            workspace.UndoStackChanged -= Workspace_UndoStackChanged;
            workspace.RedoStackChanged -= Workspace_RedoStackChanged;
            workspace.OutputPanel.WordWrapChanged -= OutputPanel_WordWrapChanged;
            workspace.Clear();
            workspaces.RemoveAt(tabIndex);
        }

        private void HideAllPanels()
        {
            tsiViewToolbox.Checked = false;
            tsiViewProperties.Checked = false;
            tsiViewOutput.Checked = false;
        }

        #endregion Closing stuff

        private void tsiViewToolbox_CheckedChanged(object sender, EventArgs e)
        {
            splitContainer1.Panel1Collapsed = !tsiViewToolbox.Checked;
        }

        private void tsiViewProperties_CheckedChanged(object sender, EventArgs e)
        {
            splitContainer2.Panel2Collapsed = !tsiViewProperties.Checked;
            Refresh();
        }

        private void tsiViewOutput_CheckedChanged(object sender, EventArgs e)
        {
            splitContainer3.Panel2Collapsed = !tsiViewOutput.Checked;
        }

        private void tsiOptions_Click(object sender, EventArgs e)
        {
            using (var frm = new OptionsForm(knownUiThemes?.UiThemes?.Select(t => t.Name).ToList()))
            {
                var opts = frm as IOptions;
                opts.AlignControlsMode = AppSettings.Instance.AlignControlsMode;
                opts.GridSize = AppSettings.Instance.GridSize;
                opts.DefaultNewRootComponentFontName = AppSettings.Instance.DefaultNewRootComponentFontName;
                opts.DefaultNewRootComponentFontSize = AppSettings.Instance.DefaultNewRootComponentFontSize;
                opts.SourceCodeViewerFontName = AppSettings.Instance.SourceCodeViewerFontName;
                opts.SourceCodeViewerFontSize = AppSettings.Instance.SourceCodeViewerFontSize;
                opts.MainFormMRUListMaxSize = AppSettings.Instance.MainFormMRUListMaxSize;
                opts.LogLevel = AppSettings.Instance.LogLevel;
                opts.RemoveUnnecessaryUsings = AppSettings.Instance.RemoveUnnecessaryUsings;
                opts.UiThemeName = AppSettings.Instance.UiThemeName;
                var result = frm.ShowDialog(this);
                if (result == DialogResult.OK)
                {
                    AppSettings.Instance.AlignControlsMode = opts.AlignControlsMode;
                    AppSettings.Instance.GridSize = opts.GridSize;
                    AppSettings.Instance.DefaultNewRootComponentFontName = opts.DefaultNewRootComponentFontName;
                    AppSettings.Instance.DefaultNewRootComponentFontSize = opts.DefaultNewRootComponentFontSize;
                    AppSettings.Instance.SourceCodeViewerFontName = opts.SourceCodeViewerFontName;
                    AppSettings.Instance.SourceCodeViewerFontSize = opts.SourceCodeViewerFontSize;
                    AppSettings.Instance.MainFormMRUListMaxSize = opts.MainFormMRUListMaxSize;
                    AppSettings.Instance.LogLevel = opts.LogLevel;
                    AppSettings.Instance.RemoveUnnecessaryUsings = opts.RemoveUnnecessaryUsings;
                    if (AppSettings.Instance.UiThemeName != opts.UiThemeName && !string.IsNullOrEmpty(opts.UiThemeName))
                    {
                        // UI Theme has changed
                        AppSettings.Instance.UiThemeName = opts.UiThemeName;
                        ApplyNewTheme(opts.UiThemeName);
                        if (opts.UiThemeName == KnownUiThemes.NONE)
                            MessageBox.Show(this, "Please note that the UI theme will be reset after you restart the application.",
                                "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    AppSettings.Instance.Save();
                    workspaces.ForEach(ws => ws.RemoveUnnecessaryUsingsOnSave = opts.RemoveUnnecessaryUsings);
                }
            }
        }

        private void ApplyNewTheme(string uiThemeName)
        {
            if (knownUiThemes != null)
            {
                CommonStuff.CurrentUiTheme = knownUiThemes.GetUiTheme(uiThemeName);
                if (CommonStuff.CurrentUiTheme != null)
                {
                    ThemeApplier.Apply(this, CommonStuff.CurrentUiTheme, applyProcessWideSettings: true);
                    ApplyUiThemeToMenuItemImages();
                    ApplyUiThemeToRecentFilesManager();
                    BeginInvoke(Refresh);
                }
                else
                {
                    recentFilesManager.DeleteIcon = Resources.Remove;
                    recentFilesManager.OpenFolderIcon = Resources.OpenContainingFolder;
                }
            }
        }

        #region Handling menu items state

        private void tsiEditRoot_DropDownOpening(object sender, EventArgs e)
        {
            OnMenuDropDownOpening(tsiEditRoot);
        }

        private void tsiViewRoot_DropDownOpening(object sender, EventArgs e)
        {
            OnMenuDropDownOpening(tsiViewRoot);
        }

        private void tsiFormatRoot_DropDownOpening(object sender, EventArgs e)
        {
            OnMenuDropDownOpening(tsiFormatRoot);
        }

        private void OnMenuDropDownOpening(ToolStripMenuItem menuItem)
        {
            DesignSurfaceEx designer = GetCurrentDesigner();
            var isRootComponentLoaded = designer?.GetDesignerHost()?.RootComponent != null;
            var ims = designer != null && IsDesignerActive()
                ? designer.GetMenuCommandService()
                : null;
            if (menuItem == tsiEditRoot)
            {
                // "Edit" menu
                bool isRootComponentSelected = isRootComponentLoaded
                    ? designer != null && designer.GetSelectionService().GetComponentSelected(designer.GetDesignerHost().RootComponent)
                    : false;

                SyncMenuWithCommand(ims, tsiUnDo, isRootComponentLoaded);
                SyncMenuWithCommand(ims, tsiReDo, isRootComponentLoaded);

                SyncMenuWithCommand(ims, tsiCut, !isRootComponentSelected);
                SyncMenuWithCommand(ims, tsiCopy, !isRootComponentSelected);
                SyncMenuWithCommand(ims, tsiPaste, isRootComponentLoaded && DesignSurfaceEx.HasDesignerComponentInClipboard());
                SyncMenuWithCommand(ims, tsiDelete, !isRootComponentSelected);

                SyncMenuWithCommand(ims, tsiSelectAll, isRootComponentLoaded);
            }
            else if (menuItem == tsiViewRoot)
            {
                // "View" menu
                SyncMenuWithCommand(ims, tsiTabOrder, isRootComponentLoaded);
            }
            else if (menuItem == tsiFormatRoot)
            {
                // "Format" menu
                GetFormatMenuItems().ToList().ForEach(mi => SyncMenuWithCommand(ims, mi, isRootComponentLoaded));
            }
        }

        private void SyncMenuWithCommand(IMenuCommandService ims, ToolStripMenuItem menuItem, bool additionalEnabledCondition = true)
        {
            if (menuItem.Tag == null || ims == null)
            {
                menuItem.Enabled = false;
                return;
            }

            var field = typeof(StandardCommands).GetField(menuItem.Tag.ToString(), BindingFlags.Public | BindingFlags.Static);
            if (field != null)
            {
                var cmd = ims.FindCommand((CommandID)field.GetValue(null));
                // NOTE: The designer handles command activation automatically! For example, 'AlignLeft' is enabled only when multiple components are selected.
                menuItem.Enabled = cmd != null && cmd.Enabled && additionalEnabledCondition;
                if (cmd != null)
                    menuItem.Checked = cmd.Checked;
            }
        }

        private IEnumerable<ToolStripMenuItem> GetFormatMenuItems()
        {
            foreach (var menuItem in tsiFormatRoot.DropDownItems.OfType<ToolStripMenuItem>())
            {
                if (!menuItem.HasDropDownItems)
                    yield return menuItem;
                else
                {
                    foreach (var menuSubItem in menuItem.DropDownItems.OfType<ToolStripMenuItem>())
                    {
                        if (!menuSubItem.HasDropDownItems)
                            yield return menuSubItem;
                    }
                }
            }
        }

        private bool IsDesignerActive()
        {
            var workspace = GetCurrentWorkspace();
            if (workspace == null)
                return false;
            return workspace.IsDesignerActive;
        }

        #endregion Handling menu items state

        private void tsiAbout_Click(object sender, EventArgs e)
        {
            using (var frm = new AboutForm(vsixVersion))
            {
                frm.ShowDialog(this);
            }
        }

        [Category("Appearance")]
        [DefaultValue(false)]
        public bool IsDarkTheme { get; set; } = false;

        private void ApplyUiThemeToRecentFilesManager()
        {
            recentFilesManager.DeleteIcon = IsDarkTheme ? Resources.Remove_DarkTheme : Resources.Remove;
            recentFilesManager.OpenFolderIcon = IsDarkTheme ? Resources.OpenContainingFolder_DarkTheme : Resources.OpenContainingFolder;
            recentFilesManager.Font = Font;
        }

        private void ApplyUiThemeToMenuItemImages()
        {
            // "File" menu:
            tsiNewForm.Image = PickMenuItemImage("AddForm");
            tsiNewUserControl.Image = PickMenuItemImage("AddControl");
            tsiOpen.Image = PickMenuItemImage("OpenFile");
            tsiSave.Image = PickMenuItemImage("Save");
            // "Edit" menu:
            tsiUnDo.Image = PickMenuItemImage("Undo");
            tsiReDo.Image = PickMenuItemImage("Redo");
            tsiCut.Image = PickMenuItemImage("Cut");
            tsiCopy.Image = PickMenuItemImage("Copy");
            tsiPaste.Image = PickMenuItemImage("Paste");
            tsiDelete.Image = PickMenuItemImage("Delete");
            tsiSelectAll.Image = PickMenuItemImage("SelectAll");
            // "View" menu:
            tsiViewToolbox.Image = PickMenuItemImage("Toolbox");
            tsiViewProperties.Image = PickMenuItemImage("Property");
            tsiViewOutput.Image = PickMenuItemImage("Output");
            tsiTabOrder.Image = PickMenuItemImage("TabOrder");
            // "Format" menu:
            tsiAlignLefts.Image = PickMenuItemImage("AlignLeft");
            tsiAlignCenters.Image = PickMenuItemImage("AlignCenter");
            tsiAlignRights.Image = PickMenuItemImage("AlignRight");
            tsiAlignTops.Image = PickMenuItemImage("AlignTop");
            tsiAlignMiddles.Image = PickMenuItemImage("AlignMiddle");
            tsiAlignBottoms.Image = PickMenuItemImage("AlignBottom");
            tsiMakeSameWidth.Image = PickMenuItemImage("MakeSameWidth");
            tsiMakeSameHeight.Image = PickMenuItemImage("MakeSameHeight");
            tsiMakeSameWidthAndHeight.Image = PickMenuItemImage("MakeSameWidthAndHeight");
            tsiHorizSpacingMakeEqual.Image = PickMenuItemImage("HorizontalSpacingMakeEqual");
            tsiHorizSpacingIncrease.Image = PickMenuItemImage("IncreaseHorizontalSpacing");
            tsiHorizSpacingDecrease.Image = PickMenuItemImage("DecreaseHorizontalSpacing");
            tsiHorizSpacingRemove.Image = PickMenuItemImage("RemoveHorizontalSpacing");
            tsiVertSpacingMakeEqual.Image = PickMenuItemImage("VerticalSpacingMakeEqual");
            tsiVertSpacingIncrease.Image = PickMenuItemImage("IncreaseVerticalSpacing");
            tsiVertSpacingDecrease.Image = PickMenuItemImage("DecreaseVerticalSpacing");
            tsiVertSpacingRemove.Image = PickMenuItemImage("RemoveVerticalSpacing");
            tsiCenterInFormHorizontally.Image = PickMenuItemImage("CenterHorizontally");
            tsiCenterInFormVertically.Image = PickMenuItemImage("CenterVertically");
            tsiOrderBringToFront.Image = PickMenuItemImage("BringToFront");
            tsiOrderSendToBack.Image = PickMenuItemImage("SendToBack");
        }

        private Bitmap PickMenuItemImage(string resourceName)
        {
            return (IsDarkTheme
                ? DarkThemeMenuItems.ResourceManager.GetObject(resourceName)
                : LightThemeMenuItems.ResourceManager.GetObject(resourceName)) as Bitmap;
        }
    }

    public enum AlignControlsModeEnum
    {
        [Display(Name = "Use SnapLines")]
        UseSnapLines = 0,
        [Display(Name = "Use Grid")]
        UseGrid,
        [Display(Name = "Use Grid and Snap to Grid")]
        SnapToGrid,
        [Display(Name = "Align Controls by Hand")]
        AlignByHand
    }
}
