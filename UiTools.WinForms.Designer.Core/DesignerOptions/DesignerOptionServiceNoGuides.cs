using System.ComponentModel.Design;
using System.Drawing;
using System.Windows.Forms.Design;

namespace UiTools.WinForms.Designer.Core
{
    internal class DesignerOptionServiceNoGuides : DesignerOptionService
    {
        public DesignerOptionServiceNoGuides() : base()
        {
        }

        protected override void PopulateOptionCollection(DesignerOptionCollection options)
        {
            if (options.Parent != null)
                return;
            DesignerOptions ops = new DesignerOptions()
            {
                GridSize = new Size(8, 8),
                SnapToGrid = false,
                ShowGrid = false,
                UseSnapLines = false,
                UseSmartTags = true
            };
            DesignerOptionCollection wfd = CreateOptionCollection(options, "WindowsFormsDesigner", null);
            CreateOptionCollection(wfd, "General", ops);
        }
    }
}
