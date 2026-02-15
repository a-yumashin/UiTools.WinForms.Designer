using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static UiTools.WinForms.Designer.Core.CommonStuff;

namespace UiTools.WinForms.Designer.Core
{
    public class DesignerWorkspace : IDisposable
    {
        /// <summary>
        /// Used to automatically set .AutoSize = true for certain types of Controls
        /// </summary>
        private IComponent lastComponentAdded;
        /// <summary>
        /// Used to calculate Location offset for Controls added with double click on a Toolbox item
        /// </summary>
        private IComponent lastComponentAdded2;

        private bool isDeserializationRunning { get; set; }

        public DesignSurfaceEx Designer { get; private set; }
        public ToolboxTreeView Toolbox { get; private set; }
        public ComponentPropertiesExplorer PropertiesExplorer { get; private set; }
        public OutputPanel OutputPanel { get; private set; }
        private IDesignerControl designerControl;
        public bool RemoveUnnecessaryUsingsOnSave { get; set; }

        public DesignerCsFileContext DesignerCsFileContext { get; private set; }
        public CodeObjectsToPreserveWhenEditing ObjectsToPreserve { get; set; } = new CodeObjectsToPreserveWhenEditing();
        public event EventHandler PropertiesWindowNeeded;
        public event EventHandler<string> UndoStackChanged;
        public event EventHandler<string> RedoStackChanged;
        public string LastUndoTransaction { get; private set; }
        public string LastRedoTransaction { get; private set; }

        public DesignerWorkspace(DesignSurfaceEx designer, ToolboxTreeView toolbox, ComponentPropertiesExplorer propertiesExplorer, OutputPanel outputPanel,
            IDesignerControl designerControl)
        {
            Designer = designer ?? throw new ArgumentNullException(nameof(designer));
            Toolbox = toolbox ?? throw new ArgumentNullException(nameof(toolbox));
            PropertiesExplorer = propertiesExplorer ?? throw new ArgumentNullException(nameof(propertiesExplorer));
            OutputPanel = outputPanel ?? throw new ArgumentNullException(nameof(outputPanel));
            this.designerControl = designerControl ?? throw new ArgumentNullException(nameof(designerControl));

            Designer.CustomMenuCommandExecuted += OnCustomMenuCommandExecuted;
            this.designerControl.RequestSourceCode += OnRequestSourceCode;

            var selectionService = designer.GetSelectionService();
            if (selectionService != null)
            {
                selectionService.SelectionChanged += OnSelectionChanged;
            }
            var changeService = designer.GetComponentChangeService();
            if (changeService != null)
            {
                changeService.ComponentAdded += OnComponentAdded;
                changeService.ComponentChanged += OnComponentChanged;
                changeService.ComponentRename += OnComponentRename;
                changeService.ComponentRemoved += OnComponentRemoved;

            }
            var toolboxService = designer.GetToolboxService();
            if (toolboxService != null)
            {
                toolboxService.SelectedToolboxItemUsed += OnSelectedToolboxItemUsed;
            }
            var undoEngine = designer.GetUndoEngine();
            if (undoEngine != null)
            {
                undoEngine.UndoStackChanged += OnUndoStackChanged;
                undoEngine.RedoStackChanged += OnRedoStackChanged;
            }

            Toolbox.ToolboxItemDoubleClick += OnToolboxItemDoubleClick;
            PropertiesExplorer.ComponentPropertiesRequired += OnComponentPropertiesRequired;
            designer.GetDesignerHost().TransactionClosed += DesignerHost_TransactionClosed;
        }

        private void DesignerHost_TransactionClosed(object sender, DesignerTransactionCloseEventArgs e)
        {
            if ((sender as IDesignerHost).RootComponent != null)
                PropertiesExplorer.RefreshGrid(); // otherwise after Undo/Redo (in "Show Events" mode) handler method name is not updated in time
        }

        private void CreateDefaultEventForComponent(IComponent component)
        {
            if (component == null)
                return;

            // Find the default event (e.g. Click for Button)
            EventDescriptor defaultEvent = TypeDescriptor.GetDefaultEvent(component);
            if (defaultEvent == null)
                return;

            var ebs = Designer.GetEventBindingService();
            var trs = Designer.GetTypeResolutionService();

            if (ebs == null || trs == null)
                return;

            // Get current handler name
            PropertyDescriptor bindingProp = ebs.GetEventProperty(defaultEvent);
            string handlerName = bindingProp.GetValue(component) as string;

            string mainFilePath = null;
            string classIdentifier = null;
            if (string.IsNullOrEmpty(handlerName))
            {
                // If empty — try to generate a new one
                if (DesignerCsFileContext.IsDesignerCsFileFullPathValid)
                {
                    mainFilePath = DesignerCsFileContext.CalcMainCsFilePath();
                    classIdentifier = Designer.GetDesignerHost().RootComponent.Site.Name;
                    handlerName = EventHelper.CreateUniqueMethodName(component, defaultEvent, mainFilePath, classIdentifier, trs);
                }
                else
                {
                    handlerName = $"{component.Site.Name}_{defaultEvent.Name}";
                    MessageLogger.LogVerbose(this, $"Cannot generate *unique* handler name for default event '{defaultEvent.Name}': " +
                        $"could not resolve the main file path from '{DesignerCsFileContext.DesignerCsFileFullPath}' (expected a path ending in '.designer.cs'); " +
                        $"fallback to '{handlerName}'.");
                }
            }

            // Call UpdateEventSubscription
            bool success = EventHelper.UpdateEventSubscription(
                component,
                defaultEvent.Name,
                false, // create/update
                handlerName,
                mainFilePath,
                classIdentifier,
                trs,
                out _
            );

            // Update the PropertyGrid
            if (success)
                PropertiesExplorer.RefreshGrid();
        }

        private void OnRedoStackChanged(object sender, string lastRedoTransaction)
        {
            LastRedoTransaction = lastRedoTransaction;
            RedoStackChanged?.Invoke(this, lastRedoTransaction);
        }

        private void OnUndoStackChanged(object sender, string lastUndoTransaction)
        {
            LastUndoTransaction = lastUndoTransaction;
            UndoStackChanged?.Invoke(this, lastUndoTransaction);
        }

        public bool IsDesignerActive { get => designerControl.IsDesignerActive; }

        public string GenerateCodeFromDesigner(Action<string> statusUpdater)
        {
            var trs = Designer.GetTypeResolutionService();
            using (new NativeWaitCursor())
            {
                return new DesignerFileSerializer().GenerateCodeFromDesigner(Designer.GetDesignerHost(), ObjectsToPreserve, trs, RemoveUnnecessaryUsingsOnSave, statusUpdater);
            }
        }

        private void OnCustomMenuCommandExecuted(object sender, string menuText)
        {
            if (menuText == "Properties")
            {
                // No need to call OnSelectionChanged() here because right click (used to show context menu) also *selects* the component under cursor
                PropertiesWindowNeeded?.Invoke(this, EventArgs.Empty);
            }
        }

        private void OnRequestSourceCode(object sender, RequestSourceCodeArgs e)
        {
            var isRootComponentLoaded = Designer?.GetDesignerHost()?.RootComponent != null;
            if (isRootComponentLoaded)
            {
                e.ComponentTypeNames = Toolbox.GetComponentTypeNamesFromReferencedAssemblies(); // needed for syntax highlighting
                e.SourceCode = GenerateCodeFromDesigner(e.StatusUpdater);
            }
            else
            {
                MessageLogger.LogWarning(this, "Cannot generate source code: root component is not available");
            }
        }

        public void OpenExistingDesignerFile(DesignerCsFileContext dfContext, Font sourceCodeViewerFont)
        {
            if (DesignerCsFileContext != null)
                DesignerCsFileContext.DesignerCsFileFullPathChanged -= OnDesignerCsFileChanged;
            DesignerCsFileContext = dfContext;
            DesignerCsFileContext.DesignerCsFileFullPathChanged += OnDesignerCsFileChanged;
            designerControl.ShowBusyIndicator("Please wait...");
            using (new NativeWaitCursor())
            {
                try
                {
                    var dfd = new DesignerFileDeserializer();

                    isDeserializationRunning = true;
                    Font rootComponentFontFromCode;
                    dfd.CreateDesignerComponentFromCode(
                        Designer.GetDesignerHost(),
                        dfContext,
                        ObjectsToPreserve,
                        out rootComponentFontFromCode);

                    // MyTypeResolutionService has been already created and registered inside the DesignerFileDeserializer.CreateDesignerComponentFromCode() method,
                    // so we just get its instance from our DesignSurface:
                    var trs = Designer.GetTypeResolutionService();
                    CreateAndPopulateToolbox(trs, dfContext.CsProjectFileWrapper == null ? null : dfContext.CsProjectFileWrapper.GetProjectAssemblyName);

                    var rootComponentFontFallback = dfContext.CsProjectFileWrapper?.TryDetectApplicationDefaultFont() ?? Control.DefaultFont;
                    if (rootComponentFontFromCode == null)
                    {
                        MessageLogger.Log(this, "No explicit Font property assignment was found for the root component. Setting the Design Surface View font " +
                            $"to \"{rootComponentFontFallback.Name}; {rootComponentFontFallback.Size}pt\" to match the expected default and ensure correct inheritance.");
                    }
                    EmbedDesignerViewInUI(Designer, trs,
                        rootComponentFont: rootComponentFontFromCode ?? rootComponentFontFallback,
                        sourceCodeViewerFont: sourceCodeViewerFont);
                    Designer.EnableDragAndDrop();
                    SelectRootComponentInDesigner();
                    CustomizeComponentTray();
                    isDeserializationRunning = false;

                    lastComponentAdded2 = null;
                }
                catch (Exception ex)
                {
                    MessageLogger.LogError(this, $"Failed to open designer file '{dfContext.DesignerCsFileFullPath}': {ex.Message}", ex);
                }
                finally
                {
                    designerControl.HideBusyIndicator();
                }
            }
        }

        public void CreateNewRootComponent(Type rootComponentType, DesignerCsFileContext dfContext, Font defaultRootComponentFont, Font sourceCodeViewerFont)
        {
            if (DesignerCsFileContext != null)
                DesignerCsFileContext.DesignerCsFileFullPathChanged -= OnDesignerCsFileChanged;
            DesignerCsFileContext = dfContext;
            DesignerCsFileContext.DesignerCsFileFullPathChanged += OnDesignerCsFileChanged;
            designerControl.ShowBusyIndicator("Please wait...");
            using (new NativeWaitCursor())
            {
                try
                {
                    // (also tried creating DesignSurface via its constructor taking Type rootComponentType, but then INameCreationService does not trigger)
                    var rootComponent = (Control)Designer.CreateRootComponent(rootComponentType, new Size(600, 400));
                    rootComponent.Text = rootComponent.Name;

                    // Create and register MyTypeResolutionService:
                    var trs = MyTypeResolutionService.CreateWithReferencedAssemblies(null, dfContext);
                    var host = Designer.GetDesignerHost();
                    host.RemoveService(typeof(ITypeResolutionService), true);
                    host.AddService(typeof(ITypeResolutionService), trs, true);
                    CreateAndPopulateToolbox(trs, dfContext.CsProjectFileWrapper == null ? null : dfContext.CsProjectFileWrapper.GetProjectAssemblyName);

                    ObjectsToPreserve = new CodeObjectsToPreserveWhenEditing();
                    ObjectsToPreserve.Namespace = dfContext.Namespace;

                    EmbedDesignerViewInUI(Designer, trs,
                        rootComponentFont: defaultRootComponentFont,
                        sourceCodeViewerFont: sourceCodeViewerFont);
                    Designer.EnableDragAndDrop();
                    SelectRootComponentInDesigner();
                    CustomizeComponentTray();

                    lastComponentAdded2 = null;
                }
                catch (Exception ex)
                {
                    MessageLogger.LogError(this, $"Failed to create new {rootComponentType.Name}: {ex.Message}", ex);
                }
                finally
                {
                    designerControl.HideBusyIndicator();
                }
            }
        }

        private void OnDesignerCsFileChanged(object sender, EventArgs e)
        {
            // DesignerCsFileContext.DesignerCsFileFullPath has changed --> PropertiesExplorer should be informed,
            // otherwise event handler methods will be not created from its UI:
            PrepareForBrowsingEvents();
        }

        private void PrepareForBrowsingEvents()
        {
            string mainFilePath = DesignerCsFileContext.IsDesignerCsFileFullPathValid
                ? DesignerCsFileContext.CalcMainCsFilePath()
                : null;
            var classIdentifier = Designer.GetDesignerHost().RootComponent.Site.Name;
            PropertiesExplorer.PrepareForBrowsingEvents(mainFilePath, classIdentifier);
        }

        public void Dispose()
        {
            Clear();
        }

        public void Clear()
        {
            Toolbox.Clear();
            Designer.GetToolboxService().Toolbox = null;
            Designer.GetDesignerHost().TransactionClosed -= DesignerHost_TransactionClosed;
            Designer.Dispose(); // DesignerMouseFilter will be removed here
            PropertiesExplorer.ClearComponentsList();
            Toolbox.ToolboxItemDoubleClick -= OnToolboxItemDoubleClick;
            PropertiesExplorer.ComponentPropertiesRequired -= OnComponentPropertiesRequired;
            if (DesignerCsFileContext != null)
                DesignerCsFileContext.DesignerCsFileFullPathChanged -= OnDesignerCsFileChanged;
        }

        public bool IsDirty
        {
            get => designerControl.IsDirty;
            set => designerControl.IsDirty = value;
        }

        private void CreateAndPopulateToolbox(MyTypeResolutionService trs, Func<string> projectAssemblyNameResolver)
        {
            new ToolboxHelper(Toolbox).CreateAndPopulateToolbox(Designer, trs, projectAssemblyNameResolver);
        }

        private void SelectRootComponentInDesigner()
        {
            var selectionService = Designer.GetSelectionService();
            selectionService.SetSelectedComponents(new[] { Designer.GetDesignerHost().RootComponent });
        }

        private void EmbedDesignerViewInUI(DesignSurfaceEx designer, ITypeResolutionService trs, Font rootComponentFont, Font sourceCodeViewerFont)
        {
            var view = designer.View as Control;
            if (view == null)
            {
                var errMsg = "DesignSurfaceEx.View returned null";
                MessageLogger.LogError(this, errMsg);
                throw new Exception(errMsg);
            }
            view.Dock = DockStyle.Fill;

            Tuple<AutoScaleMode, SizeF, Size> scalingStuff = null;
            var rootComponent = designer.GetDesignerHost().RootComponent as ContainerControl;
            if (rootComponent != null) // NOTE: not all "user controls" derive from ContainerControl! (e.g. 'class MyButton : Button' vs 'class MyButton : UserControl')
            {
                scalingStuff = StoreScalingStuff(rootComponent);
                rootComponent.AutoScaleMode = AutoScaleMode.None;
            }

            designerControl.SetRootComponentFont(rootComponentFont); // << DesignerControl.Font will affect scaling of DesignerControl's children
            designerControl.AdoptViewAsChild(view); // << setting DesignerControl as parent affects scaling of the root component and its children
            designerControl.SetSourceCodeViewerFont(sourceCodeViewerFont);

            if (rootComponent != null)
            {
                RestoreScalingStuff(rootComponent, scalingStuff);
                rootComponent.PerformAutoScale();
            }

            PrepareForBrowsingEvents();

            // Support creating handler methods for *default* events on component double click:
            var designerMouseFilter = new DesignerMouseFilter(Designer, (comp) => CreateDefaultEventForComponent(comp));
            Application.AddMessageFilter(designerMouseFilter);
            Designer.Disposed += (s, e) => Application.RemoveMessageFilter(designerMouseFilter);

            view.Focus();
        }

        private static Tuple<AutoScaleMode, SizeF, Size> StoreScalingStuff(ContainerControl ctl)
        {
            var autoScaleMode = ctl.AutoScaleMode;
            if (autoScaleMode == AutoScaleMode.None || autoScaleMode == AutoScaleMode.Inherit)
                autoScaleMode = AutoScaleMode.Font;
            var autoScaleDimensions = ctl.AutoScaleDimensions;
            if (autoScaleDimensions == Size.Empty)
                autoScaleDimensions = new SizeF(6F, 13F);
            return new Tuple<AutoScaleMode, SizeF, Size>(autoScaleMode, autoScaleDimensions, ctl.ClientSize);
        }
        private static void RestoreScalingStuff(ContainerControl ctl, Tuple<AutoScaleMode, SizeF, Size> scalingStuff)
        {
            ctl.AutoScaleMode = scalingStuff.Item1;
            ctl.AutoScaleDimensions = scalingStuff.Item2;
            ctl.ClientSize = scalingStuff.Item3;
        }

        private void OnToolboxItemDoubleClick(object sender, ToolboxItem toolboxItem)
        {
            var host = Designer.GetDesignerHost();
            if (host == null)
                return;

            Type componentType = toolboxItem.GetType(host);
            if (componentType == null)
                return;

            DesignerTransaction transaction = null;
            try
            {
                transaction = host.CreateTransaction($"Add '{toolboxItem.DisplayName}' from Toolbox (with double click)"); // start a transaction for Undo/Redo

                var prevComponentLocation = lastComponentAdded2 is Control ctl // since host.CreateComponent() will call OnComponentAdded(), where lastComponentAdded2 will be updated
                    ? ctl.Location
                    : new Point(-8, -8);
                IComponent newComponent = host.CreateComponent(componentType);
                if (newComponent is Control control)
                {
                    // SEQUENTIAL double-clicks on a control (visual component) in the toolbox result in an automatic Location shift of (8; 8) from the previous control;
                    // but the very first creation of any non-visual component - resets the shift base to (0; 0).
                    prevComponentLocation.Offset(8, 8);
                    control.Location = prevComponentLocation;

                    SetAutoSizeIfApplicable(control);
                    control.Text = control.Name;

                    var rootComponent = host.RootComponent as Control;
                    if (rootComponent != null)
                    {
                        rootComponent.Controls.Add(control);
                        control.BringToFront();
                    }
                }

                // Notify IComponentChangeService about the change (for Undo/Redo):
                var changeService = Designer.GetComponentChangeService();
                if (changeService != null)
                {
                    changeService?.OnComponentChanging(host.RootComponent, null);
                    changeService?.OnComponentChanged(host.RootComponent, null, null, null);
                }

                // Select the new component:
                ISelectionService selectionService = Designer.GetSelectionService();
                selectionService?.SetSelectedComponents(new IComponent[] { newComponent });

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction?.Cancel();
                MessageBox.Show($"Error creating component: {ex.Message}");
            }
            Toolbox.SelectPointerTool();
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            var selectionService = Designer.GetSelectionService();
            if (selectionService.SelectionCount == 0)
                return;
            var components = selectionService.GetSelectedComponents();
            if (components.Count == 1)
            {
                var comp = components.Cast<IComponent>().First();
                if (comp.IsTopLevelComponent(Designer.GetDesignerHost()))
                    PropertiesExplorer.ShowComponentPropertiesOrEvents($"{comp.Site.Name} {comp.GetType().FullName}");
                else
                {
                    // This is a nested control (SplitterPanel, etc.) - we need to generate a name qualified by its parent's name for it
                    // (e.g. "splitContainer1.Panel1" etc.), and then add it to the Properties window component list, as nested controls
                    // are initially NOT present in this list:
                    var qualifiedName = (comp as Control).GetQualifiedControlName(Designer.GetDesignerHost());
                    PropertiesExplorer.AddComponentToList($"{qualifiedName} {comp.GetType().FullName}", ComponentPropertiesExplorer.SelectAfterAddEnum.Select);
                    PropertiesExplorer.ShowComponentPropertiesOrEvents(comp);
                    // However, this addition is temporary: at the first opportunity, this nested control will be removed from the list
                    // using the ComponentPropertiesExplorer.RemoveNonTopLevelComponentsFromList() method.
                }
            }
            else
            {
                PropertiesExplorer.ShowSeveralComponentsProperties(components.Cast<IComponent>().ToArray());
            }
        }

        private void OnComponentAdded(object sender, ComponentEventArgs e)
        {
            lastComponentAdded = e.Component;
            lastComponentAdded2 = e.Component;
            if (e.Component.IsTopLevelComponent(Designer.GetDesignerHost()))
                PropertiesExplorer.AddComponentToList($"{e.Component.Site.Name} {e.Component.GetType().FullName}",
                    isDeserializationRunning ? ComponentPropertiesExplorer.SelectAfterAddEnum.DoNotSelect : ComponentPropertiesExplorer.SelectAfterAddEnum.SelectAsync);
            // (add only "top-level" components to the "Properties" window combobox - i.e. things like "splitContainer1.Panel1" are NOT added)

            if (!isDeserializationRunning)
                designerControl.IsDirty = true;
        }

        private void OnComponentChanged(object sender, ComponentChangedEventArgs e)
        {
            if (!isDeserializationRunning)
                designerControl.IsDirty = true;
        }

        private void OnComponentRename(object sender, ComponentRenameEventArgs e)
        {
            if ((e.Component as IComponent).IsTopLevelComponent(Designer.GetDesignerHost()))
            {
                PropertiesExplorer.RenameComponentInList(
                    $"{e.OldName} {e.Component.GetType().FullName}",
                    $"{e.NewName} {e.Component.GetType().FullName}");
                if (e.Component == Designer.GetDesignerHost().RootComponent)
                {
                    // form class identifier has changed (because it is equal to form name) --> two things should be done:
                    // (1) form class should be renamed in the main file as well (if it is present):
                    if (DesignerCsFileContext.IsDesignerCsFileFullPathValid)
                        RenameRootComponentClassInMainFile(DesignerCsFileContext.CalcMainCsFilePath(), e.OldName, e.NewName);
                    else
                        MessageLogger.LogWarning(this, $"Cannot rename class '{e.OldName}' to '{e.NewName}' in the main file: " +
                            $"could not resolve the main file path from '{DesignerCsFileContext.DesignerCsFileFullPath}' (expected a path ending in '.designer.cs').");
                    // (2) PropertiesExplorer should be informed, otherwise event handler methods will be not created from its UI:
                    PrepareForBrowsingEvents();
                }
            }
            else
            {
                // this is a non-TopLevel component whose Name property is displayed in the Properties window (and therefore rename is possible); for example,
                // it could be toolStripContainer1.RightToolStripPanel (unlike splitContainer1.Panel1, whose Name is NOT displayed in the Properties window, and
                // therefore the IComponentChangeService.ComponentRename event cannot occur at the user's initiative)
                var parent = FindParentComponent(e.Component as IComponent);
                if (parent != null)
                    PropertiesExplorer.RenameComponentInList(
                        $"{parent.Site.Name}.{e.OldName} {e.Component.GetType().FullName}",
                        $"{parent.Site.Name}.{e.NewName} {e.Component.GetType().FullName}");
            }
        }

        private void OnComponentRemoved(object sender, ComponentEventArgs e)
        {
            PropertiesExplorer.RemoveComponentFromList($"{e.Component.Site.Name} {e.Component.GetType().FullName}");
            designerControl.IsDirty = true;
        }

        private IComponent FindParentComponent(IComponent childComponent)
        {
            ThrowIfNullOrEmpty(childComponent);

            var referenceService = Designer.GetReferenceService();
            if (referenceService == null)
                return null;

            return referenceService.GetComponent(childComponent);
        }

        private void OnSelectedToolboxItemUsed(object sender, Type componentTypeFromToolbox)
        {
            if (lastComponentAdded != null && lastComponentAdded.GetType() == componentTypeFromToolbox && lastComponentAdded is Control ctl)
                SetAutoSizeIfApplicable(ctl); // we set AutoSize only if control has been added from Toolbox (not by copy/paste)
            lastComponentAdded = null;
        }

        private void OnComponentPropertiesRequired(object sender, ComponentPropertiesRequiredArgs args)
        {
            var parts = args.ComponentNameAndType.Split(" ".ToCharArray(), 2);
            if (parts.Length != 2)
                return;
            args.Component = Designer.ComponentContainer.Components.Cast<IComponent>()
                .FirstOrDefault(c => c.Site.Name == parts[0] && c.GetType().FullName == parts[1]);
            if (args.Component != null)
            {
                var selectionService = Designer.GetSelectionService();
                selectionService?.SetSelectedComponents(new IComponent[] { args.Component });
            }
        }

        private static void SetAutoSizeIfApplicable(Control ctl)
        {
            // or we could also check if ToolboxItemAttribute.ToolboxItemType is System.Windows.Forms.Design.AutoSizeToolboxItem
            if (ctl.GetType() == typeof(Label))
                (ctl as Label).AutoSize = true;
            else if (ctl.GetType() == typeof(LinkLabel))
                (ctl as LinkLabel).AutoSize = true;
            else if (ctl.GetType() == typeof(CheckBox))
                (ctl as CheckBox).AutoSize = true;
        }

        private void CustomizeComponentTray()
        {
            var view = Designer.View as Control;
            if (view == null)
                return;
            var tray = view.FindControlByType("System.Windows.Forms.Design.ComponentTray");
            if (tray == null)
            {
                view.ControlAdded -= OnViewControlAdded;
                view.ControlAdded += OnViewControlAdded;
            }
            else
                ApplyTrayStyles(tray);
        }

        private void OnViewControlAdded(object sender, ControlEventArgs e)
        {
            if (e.Control.GetType().FullName == "System.Windows.Forms.Design.ComponentTray")
                ApplyTrayStyles(e.Control);
        }

        private void ApplyTrayStyles(Control tray)
        {
            // it was always annoying that the ComponentTray blends with the main DesignSurface area
            // (where the Form or UserControl is displayed), so we highlight the ComponentTray with a different color:
            if (tray.BackColor != SystemColors.ControlLight)
            {
                tray.BackColor = SystemColors.ControlLight;
                tray.Height = 50;
                //tray.MaximumSize = new Size(0, 50);
                (tray as System.Windows.Forms.Design.ComponentTray).AutoArrange = true; // but it can always be turned off via the context menu
            }
        }

        /// <summary>
        /// Renames the root component class and its constructors in the main code file.
        /// </summary>
        private void RenameRootComponentClassInMainFile(string filePath, string oldClassName, string newClassName)
        {
            if (!File.Exists(filePath))
                return;

            try
            {
                string sourceCode = File.ReadAllText(filePath);
                SyntaxTree tree = CSharpSyntaxTree.ParseText(sourceCode);
                var root = (CompilationUnitSyntax)tree.GetRoot();

                // Search for a class declaration with the required name
                var classDeclaration = root.DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .FirstOrDefault(c => c.Identifier.ValueText == oldClassName);

                if (classDeclaration == null)
                {
                    MessageLogger.LogWarning(this, $"Class '{oldClassName}' not found in '{Path.GetFileName(filePath)}'.");
                    return;
                }

                // First, update the constructors within this class
                // (a C# class requires the constructor name to strictly match the class name)
                var updatedClass = classDeclaration.ReplaceNodes(
                    classDeclaration.DescendantNodes().OfType<ConstructorDeclarationSyntax>()
                        .Where(ctor => ctor.Identifier.ValueText == oldClassName),
                    (oldCtor, newCtor) => newCtor.WithIdentifier(SyntaxFactory.Identifier(newClassName))
                );

                // Now update the class name itself
                updatedClass = updatedClass.WithIdentifier(SyntaxFactory.Identifier(newClassName));

                // Replace the old class with the updated one in the document tree
                var updatedRoot = root.ReplaceNode(classDeclaration, updatedClass);

                // Save the result
                File.WriteAllText(filePath, updatedRoot.ToFullString(), CommonStuff.Utf8WithoutBom);

                MessageLogger.Log(this, $"Successfully renamed class '{oldClassName}' to '{newClassName}' in '{Path.GetFileName(filePath)}'.");
            }
            catch (Exception ex)
            {
                MessageLogger.LogError(this, $"Failed to rename class in file: {ex.Message}", ex);
            }
        }
    }
}
