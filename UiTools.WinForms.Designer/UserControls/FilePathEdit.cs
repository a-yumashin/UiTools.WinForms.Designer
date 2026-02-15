using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace UiTools.WinForms.Designer
{
    public partial class FilePathEdit : ButtonEdit
    {
        public FilePathEdit()
        {
            InitializeComponent();
            ButtonClick += FilePathEdit_ButtonClick;
            ButtonToolTipText = "browse...";
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public string Title { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public string Filter { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public int FilterIndex { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public FileDialogType DialogType { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public string InitialDirectory { get; set; }

        private void FilePathEdit_ButtonClick(object sender, EventArgs e)
        {
            if (DialogType == FileDialogType.Open)
            {
                using (var dlg = new OpenFileDialog())
                {
                    dlg.Title = Title;
                    dlg.Filter = Filter;
                    dlg.FilterIndex = FilterIndex;
                    dlg.Multiselect = false;
                    dlg.InitialDirectory = InitialDirectory;
                    if (dlg.ShowDialog(this) == DialogResult.OK)
                        Text = dlg.FileName;
                }
            }
            else // FileDialogType.Save
            {
                using (var dlg = new SaveFileDialog())
                {
                    dlg.Title = Title;
                    dlg.Filter = Filter;
                    dlg.FilterIndex = FilterIndex;
                    dlg.FileName = Text;
                    dlg.InitialDirectory = InitialDirectory;
                    if (dlg.ShowDialog(this) == DialogResult.OK)
                        Text = dlg.FileName;
                }
            }
        }

        public enum FileDialogType
        {
            Open = 0,
            Save
        }
    }
}
