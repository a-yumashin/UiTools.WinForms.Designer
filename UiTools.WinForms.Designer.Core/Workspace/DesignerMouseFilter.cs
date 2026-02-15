using System;
using System.ComponentModel.Design;
using System.ComponentModel;
using System.Windows.Forms;

namespace UiTools.WinForms.Designer.Core
{
    internal class DesignerMouseFilter : IMessageFilter
    {
        private const int WM_LBUTTONDBLCLK = 0x0203;

        private readonly Action<IComponent> onDefaultAction;
        private readonly ISelectionService selectionService;
        private readonly Control view;

        public DesignerMouseFilter(DesignSurfaceEx surface, Action<IComponent> onDefaultAction)
        {
            selectionService = surface.GetSelectionService();
            view = surface.View as Control;
            this.onDefaultAction = onDefaultAction;
        }

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == WM_LBUTTONDBLCLK)
            {
                // Check that the double-click occurred specifically within our designer area:
                if (view != null && view.Visible)
                {
                    // Find the control that was double-clicked:
                    var clickedControl = Control.FromChildHandle(m.HWnd);
                    // If the double-click was inside our View:
                    if (clickedControl != null && clickedControl.IsChildOf(view))
                    {
                        var selectedComponent = selectionService?.PrimarySelection as IComponent;
                        if (selectedComponent != null)
                        {
                            onDefaultAction(selectedComponent);
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}