using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace UiTools.WinForms.Designer.Core
{
    public partial class DesignSurfaceEx : DesignSurface
    {
        public event EventHandler<string> CustomMenuCommandExecuted;

        private Point dragPoint = Point.Empty;

        #region Constructors

        public DesignSurfaceEx() : base()
        {
            InitServices();
        }
        public DesignSurfaceEx(IServiceProvider parentProvider) : base(parentProvider)
        {
            InitServices();
        }
        public DesignSurfaceEx(Type rootComponentType) : base(rootComponentType)
        {
            InitServices();
        }
        public DesignSurfaceEx(IServiceProvider parentProvider, Type rootComponentType) : base(parentProvider, rootComponentType)
        {
            InitServices();
        }

        #endregion Constructors

        #region Designer Options

        public void UseSnapLines()
        {
            AddService(typeof(DesignerOptionService), new DesignerOptionServiceSnapLines());
        }

        public void UseGrid(Size gridSize)
        {
            AddService(typeof(DesignerOptionService), new DesignerOptionServiceGrid(gridSize));
        }

        public void UseGridWithoutSnapping(Size gridSize)
        {
            AddService(typeof(DesignerOptionService), new DesignerOptionServiceGridWithoutSnapping(gridSize));
        }

        public void UseNoGuides()
        {
            AddService(typeof(DesignerOptionService), new DesignerOptionServiceNoGuides());
        }

        #endregion Designer Options

        public IComponent CreateRootComponent(Type controlType, Size controlSize)
        {
            try
            {
                IDesignerHost host = GetDesignerHost();
                if (host == null)
                    return null;
                if (host.RootComponent != null)
                    return null;
                BeginLoad(controlType);
                if (LoadErrors.Count > 0)
                {
                    var errList = string.Join("\n", LoadErrors.Cast<object>()
                        .Select(e => e is Exception ex ? ex.Message : e.ToString())
                        .Select(s => "    " + s));
                    var errMsg = $"DesignSurface.BeginLoad() method failed (root component type: {controlType})";
                    MessageLogger.LogError(this, $"{errMsg}:\n{errList}");
                    throw new Exception(errMsg + ". See Output pane for details.");
                }
                // Set size:
                Control ctrl = View as Control;
                if (controlSize != new Size())
                {
                    PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(host.RootComponent);
                    PropertyDescriptor pdS = pdc.Find("Size", false);
                    if (pdS != null)
                        pdS.SetValue(host.RootComponent, controlSize);
                }
                return host.RootComponent;
            }
            catch (Exception ex)
            {
                MessageLogger.LogError(this, ex.Message, ex);
                throw;
            }
        }

        public IDesignerHost GetDesignerHost()
        {
            return (IDesignerHost)GetService(typeof(IDesignerHost));
        }

        public MyToolboxService GetToolboxService()
        {
            return (MyToolboxService)GetService(typeof(IToolboxService));
        }

        public ISelectionService GetSelectionService()
        {
            return (ISelectionService)GetService(typeof(ISelectionService));
        }

        public IReferenceService GetReferenceService()
        {
            return (IReferenceService)GetService(typeof(IReferenceService));
        }

        public MyUndoEngine GetUndoEngine()
        {
            return (MyUndoEngine)GetService(typeof(UndoEngine));
        }

        public IMenuCommandService GetMenuCommandService()
        {
            return (IMenuCommandService)GetService(typeof(IMenuCommandService));
        }

        public MyTypeResolutionService GetTypeResolutionService()
        {
            return (MyTypeResolutionService)GetService(typeof(ITypeResolutionService));
        }

        public MyEventBindingService GetEventBindingService()
        {
            return (MyEventBindingService)GetService(typeof(IEventBindingService));
        }

        public IComponentChangeService GetComponentChangeService()
        {
            return (IComponentChangeService)GetService(typeof(IComponentChangeService));
        }

        public void AddService(Type serviceType, object serviceInstance)
        {
            ServiceContainer.RemoveService(serviceType, false);
            ServiceContainer.AddService(serviceType, serviceInstance);
        }

        private void InitServices()
        {
            AddService(typeof(INameCreationService), new MyNameCreationService());

            // NOTE: As experimentally determined, ComponentSerializationService is used in UndoEngine for serializing the state of a component
            //       or group of components in a format that can be easily saved and restored for "undo/redo" operations, as well as in
            //       DesignerSerializationService for Cut/Copy/Paste operations.
            AddService(typeof(ComponentSerializationService), new CodeDomComponentSerializationService(ServiceContainer));

            // NOTE: DesignerSerializationService is used in Cut/Copy/Paste operations.
            AddService(typeof(IDesignerSerializationService), new MyDesignerSerializationService(ServiceContainer));
            
            var ims = new MyMenuCommandService(this);
            ims.CustomMenuCommandExecuted += (s, e) => CustomMenuCommandExecuted?.Invoke(s, e); // forwarding to DesignerWorkspace
            AddService(typeof(IMenuCommandService), ims);

            // NOTE: everything related to UndoEngine must come AFTER the creation of the IMenuCommandService.
            var undoEngine = new MyUndoEngine(ServiceContainer) { Enabled = false };
            // Bind Undo/Redo commands to the corresponding undoEngine methods:
            ims.AddCommand(new MenuCommand((s, e) => undoEngine.Undo(), StandardCommands.Undo));
            ims.AddCommand(new MenuCommand((s, e) => undoEngine.Redo(), StandardCommands.Redo));
            AddService(typeof(UndoEngine), undoEngine);
            
            AddService(typeof(IToolboxService), new MyToolboxService(GetDesignerHost()));

            // NOTE: MyTypeDiscoveryService is used, for example, in DataGridViewAddColumnDialog (see comments in the MyTypeDiscoveryService class).
            AddService(typeof(ITypeDiscoveryService), new MyTypeDiscoveryService(ServiceContainer));

            // NOTE: without MyEventBindingService, it's not possible to work with component event subscriptions (+=).
            AddService(typeof(IEventBindingService), new MyEventBindingService());
        }

        public void ProcessMenuItemClick(object menuItemTag)
        {
            if (menuItemTag == null || !(menuItemTag is string commandName))
                return;
            var ims = GetMenuCommandService();
            if (ims == null)
                return;
            try
            {
                var stdCommandField = typeof(StandardCommands).GetField(commandName, BindingFlags.Public | BindingFlags.Static);
                if (stdCommandField == null)
                    MessageLogger.LogError(this, $"Command \"{commandName}\" is not supported");
                else
                    ims.GlobalInvoke((CommandID)stdCommandField.GetValue(null));
            }
            catch (Exception ex)
            {
                MessageLogger.LogError(this, $"Failed to process command \"{commandName}\": {ex.Message}", ex);
                throw;
            }
        }

        #region Drag'n'Drop support

        public void EnableDragAndDrop()
        {
            MyToolboxService tbs = GetToolboxService();
            if (tbs == null)
                return;
            (tbs.Toolbox as ToolboxTreeView).MouseDown += OnToolboxMouseDown;
            (tbs.Toolbox as ToolboxTreeView).MouseMove += OnToolboxMouseMove;
        }

        private void OnToolboxMouseDown(object sender, MouseEventArgs e)
        {
            MyToolboxService tbs = GetToolboxService();
            if (tbs == null || tbs.Toolbox == null || tbs.Toolbox.SelectedItem == null)
                return;
            dragPoint = new Point(e.X, e.Y);
        }

        private void OnToolboxMouseMove(object sender, MouseEventArgs e)
        {
            MyToolboxService tbs = GetToolboxService();
            if (tbs == null || tbs.Toolbox == null || tbs.Toolbox.SelectedItem == null)
                return;
            if (e.Button == MouseButtons.Left)
            {
                if (dragPoint != Point.Empty && (Math.Abs(e.X - dragPoint.X) > SystemInformation.DragSize.Width || Math.Abs(e.Y - dragPoint.Y) > SystemInformation.DragSize.Height))
                {
                    tbs.Toolbox.DoDragDrop(tbs.Toolbox.SelectedItem, DragDropEffects.Copy | DragDropEffects.Move);
                }
            }
        }

        #endregion Drag'n'Drop support

        public static bool HasDesignerComponentInClipboard()
        {
            IDataObject dataObject = Clipboard.GetDataObject();
            return dataObject == null
                ? false
                : dataObject.GetDataPresent("CF_DESIGNERCOMPONENTS_V2");
        }

        protected override void Dispose(bool disposing)
        {
            if (GetDesignerHost()?.RootComponent != null)
            {
                var designerView = View as Control;
                if (designerView != null)
                {
                    designerView.Visible = false;
                    designerView.Parent = null; // detach from the UI tree; this removes a potential NRE on forms with ToolStrip/MenuStrip
                }
            }
            base.Dispose(disposing);
        }
    }
}
