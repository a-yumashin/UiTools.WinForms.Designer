using System;
using System.Collections;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Windows.Forms;

namespace UiTools.WinForms.Designer.Core
{
    public class MyToolboxService : IToolboxService
    {
        private readonly IDesignerHost host;

        public event EventHandler<Type> SelectedToolboxItemUsed;

        public IMyToolbox Toolbox { get; set; }

        public MyToolboxService(IDesignerHost host)
        {
            this.host = host;
        }

        ToolboxItem IToolboxService.GetSelectedToolboxItem(IDesignerHost host) => ((IToolboxService)this).GetSelectedToolboxItem();
        ToolboxItem IToolboxService.GetSelectedToolboxItem()
        {
            if (Toolbox == null || Toolbox.SelectedItem == null)
                return null;
            ToolboxItem tbItem = Toolbox.SelectedItem;
            if (tbItem.DisplayName == "Pointer")
                return null;
            return tbItem;
        }
        
        void IToolboxService.SelectedToolboxItemUsed()
        {
            ToolboxItem tbItem = Toolbox.SelectedItem;
            if (tbItem != null && tbItem.DisplayName != "Pointer")
                SelectedToolboxItemUsed?.Invoke(this, tbItem.GetType(host));
            if (Toolbox != null)
                Toolbox.SelectPointerTool();
        }

        bool IToolboxService.SetCursor()
        {
            if (Toolbox == null || Toolbox.SelectedItem == null)
                return false;
            ToolboxItem tbItem = Toolbox.SelectedItem;
            if (tbItem.DisplayName == "Pointer")
                return false;
            if (Toolbox.SelectedItem != null)
            {
                Cursor.Current = Cursors.Cross;
                return true;
            }
            return false;
        }

        void IToolboxService.SetSelectedToolboxItem(ToolboxItem toolboxItem)
        {
            if (Toolbox != null)
                Toolbox.SelectedItem = toolboxItem;
        }

        // NOTE: both DeserializeToolboxItem() methods are needed for drag'n'drop support:
        ToolboxItem IToolboxService.DeserializeToolboxItem(object serializedObject, IDesignerHost host) => ((IToolboxService)this).DeserializeToolboxItem(serializedObject);
        ToolboxItem IToolboxService.DeserializeToolboxItem(object serializedObject) => (ToolboxItem)(((DataObject)serializedObject).GetData(typeof(ToolboxItem)));

        #region Not used

        void IToolboxService.AddCreator(ToolboxItemCreatorCallback creator, string format, IDesignerHost host) { }
        void IToolboxService.AddCreator(ToolboxItemCreatorCallback creator, string format) { }
        void IToolboxService.AddLinkedToolboxItem(ToolboxItem toolboxItem, string category, IDesignerHost host) { }
        void IToolboxService.AddLinkedToolboxItem(ToolboxItem toolboxItem, IDesignerHost host) { }
        void IToolboxService.AddToolboxItem(ToolboxItem toolboxItem, string category) { }
        void IToolboxService.AddToolboxItem(ToolboxItem toolboxItem) { }
        void IToolboxService.Refresh() { }
        void IToolboxService.RemoveCreator(string format, IDesignerHost host) { }
        void IToolboxService.RemoveCreator(string format) { }
        void IToolboxService.RemoveToolboxItem(ToolboxItem toolboxItem, string category) { }
        void IToolboxService.RemoveToolboxItem(ToolboxItem toolboxItem) { }

        CategoryNameCollection IToolboxService.CategoryNames => null;
        ToolboxItemCollection IToolboxService.GetToolboxItems(string category, IDesignerHost host) => null;
        ToolboxItemCollection IToolboxService.GetToolboxItems(string category) => null;
        ToolboxItemCollection IToolboxService.GetToolboxItems(IDesignerHost host) => null;
        ToolboxItemCollection IToolboxService.GetToolboxItems() => null;
        object IToolboxService.SerializeToolboxItem(ToolboxItem toolboxItem) => null;

        string IToolboxService.SelectedCategory { get => null; set { } }

        bool IToolboxService.IsSupported(object serializedObject, ICollection filterAttributes) => true;
        bool IToolboxService.IsSupported(object serializedObject, IDesignerHost host) => true;
        bool IToolboxService.IsToolboxItem(object serializedObject, IDesignerHost host) => ((IToolboxService)this).IsToolboxItem(serializedObject);
        bool IToolboxService.IsToolboxItem(object serializedObject) => throw new NotImplementedException();

        #endregion Not used
    }
}
