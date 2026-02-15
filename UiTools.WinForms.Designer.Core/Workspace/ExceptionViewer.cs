using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace UiTools.WinForms.Designer.Core
{
    public partial class ExceptionViewer : Form
    {
        public ExceptionViewer()
        {
            InitializeComponent();
            pgrDetails.PropertySort = PropertySort.NoSort;
            pgrDetails.HelpVisible = false;
            pgrDetails.ToolbarVisible = false;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            pgrDetails.SetLabelColumnWidth(170);
        }

        public Exception Exception { set => pgrDetails.SelectedObject = value; }
    }

    public class ExceptionWithStackTraceConverter : ExpandableObjectConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is Exception ex)
            {
                return ex.GetType().Name;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            var properties = TypeDescriptor.GetProperties(value.GetType(), attributes);
            var newProperties = new PropertyDescriptorCollection(null);

            PropertyDescriptor stackTraceDescriptor = null;
            foreach (PropertyDescriptor pd in properties)
            {
                if (pd.Name == "StackTrace")
                    stackTraceDescriptor = pd;
                else
                    newProperties.Add(pd);
            }

            if (stackTraceDescriptor != null)
            {
                // Add our StackTraceEditor:
                var customStackTraceDescriptor = TypeDescriptor.CreateProperty(
                    stackTraceDescriptor.ComponentType,
                    stackTraceDescriptor.Name,
                    stackTraceDescriptor.PropertyType,
                    stackTraceDescriptor.Attributes.Cast<Attribute>()
                        .Concat(new Attribute[] { new EditorAttribute(typeof(StackTraceEditor), typeof(UITypeEditor)) })
                        .ToArray()
                );
                newProperties.Add(customStackTraceDescriptor);
            }
            else
                return properties;

            return newProperties;
        }
    }
    
    public class StackTraceEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal; // button [...] will be shown
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var stackTrace = value?.ToString();
            if (!string.IsNullOrEmpty(stackTrace))
            {
                using (var frm = new Form())
                {
                    frm.Text = "Stack Trace";
                    frm.Size = new Size(600, 400);
                    frm.StartPosition = FormStartPosition.CenterParent;
                    frm.FormBorderStyle = FormBorderStyle.SizableToolWindow;
                    frm.KeyPreview = true;
                    frm.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) frm.Close(); };

                    frm.Controls.Add(new TextBox
                    {
                        Multiline = true,
                        ReadOnly = true,
                        Dock = DockStyle.Fill,
                        ScrollBars = ScrollBars.Vertical,
                        Text = stackTrace,
                        Font = new Font(new FontFamily("Consolas"), 9f),
                        SelectionStart = 0
                    });

                    frm.ShowDialog();
                }
            }
            return value;
        }
    }
}
