using System.Drawing.Design;
using System.Windows.Forms;

namespace UiTools.WinForms.Designer.Core
{
    /// <summary>
    /// Contains methods and properties which are called from MyToolboxService (and/or via MyToolboxService).
    /// </summary>
    public interface IMyToolbox
    {
        ToolboxItem SelectedItem { get; set; }                                     // called from MyToolboxService as well as from DesignSurfaceEx (via MyToolboxService)
        void DoDragDrop(ToolboxItem toolboxItem, DragDropEffects dragDropEffects); // called from DesignSurfaceEx (via MyToolboxService)
        void SelectPointerTool();                                                  // called from MyToolboxService
    }
}
