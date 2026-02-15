using System.ComponentModel;

namespace UiTools.WinForms.Designer
{
    public class WorkspaceSettings
    {
        [DefaultValue(true)]
        public bool ViewToolbox { get; set; } = true;
        [DefaultValue(true)]
        public bool ViewProperties { get; set; } = true;
        [DefaultValue(true)]
        public bool ViewOutput { get; set; } = true;
        [DefaultValue(true)]
        public bool OutputWordWrap { get; set; } = true;
        [DefaultValue(false)]
        public bool OutputShowTimestamp { get; set; } = false;

        [DefaultValue(300)]
        public int ToolboxWidth { get; set; } = 300;
        [DefaultValue(300)]
        public int PropertiesWidth { get; set; } = 300;
        [DefaultValue(200)]
        public int OutputHeight { get; set; } = 200;
    }
}
