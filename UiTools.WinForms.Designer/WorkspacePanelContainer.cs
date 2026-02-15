using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace UiTools.WinForms.Designer
{
    public partial class WorkspacePanelContainer : UserControl
    {
        public event EventHandler Closed;

        public string Title { get => labHeader.Text; set => labHeader.Text = value; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Control WorkspacePanel
        {
            get => pnlContainer.Controls.Count == 1 ? null : pnlContainer.Controls["WorkspacePanel"];
            set
            {
                if (pnlContainer.Controls.Count > 1)
                    pnlContainer.Controls.RemoveByKey("WorkspacePanel");
                value.Name = "WorkspacePanel";
                pnlContainer.Controls.Add(value);
                value.BringToFront();
                pnlContainer.Refresh();
            }
        }

        public WorkspacePanelContainer()
        {
            InitializeComponent();
            picClose.Click += (s, e) => Closed?.Invoke(this, EventArgs.Empty);
        }
    }
}
