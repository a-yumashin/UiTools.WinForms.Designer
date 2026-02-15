using System.ComponentModel.Design;
using System.Windows.Forms.Design;

namespace UiTools.WinForms.Designer.Core
{
    internal class DesignerOptionServiceSnapLines : DesignerOptionService
    {
        protected override void PopulateOptionCollection(DesignerOptionCollection options)
        {
            if (options.Parent != null)
                return;
            DesignerOptions ops = new DesignerOptions()
            {
                UseSnapLines = true,
                UseSmartTags = true
            };
            DesignerOptionCollection wfd = CreateOptionCollection(options, "WindowsFormsDesigner", null);
            CreateOptionCollection(wfd, "General", ops);
        }
    }
}
