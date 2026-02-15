using System.ComponentModel.Design;
using System.Drawing;
using System.Windows.Forms.Design;

namespace UiTools.WinForms.Designer.Core
{
    internal class DesignerOptionServiceGrid : DesignerOptionService
    {
        private readonly Size gridSize;

        public DesignerOptionServiceGrid(Size gridSize) : base()
        {
            this.gridSize = gridSize;
        }

        protected override void PopulateOptionCollection(DesignerOptionCollection options)
        {
            if (options.Parent != null)
                return;
            DesignerOptions ops = new DesignerOptions()
            {
                GridSize = gridSize,
                SnapToGrid = true,
                ShowGrid = true,
                UseSnapLines = false,
                UseSmartTags = true
            };
            DesignerOptionCollection wfd = CreateOptionCollection(options, "WindowsFormsDesigner", null);
            CreateOptionCollection(wfd, "General", ops);
        }
    }
}
