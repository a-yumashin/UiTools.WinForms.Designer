using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using System.Reflection;

namespace UiTools.WinForms.Designer.Core
{
    public partial class ExceptionViewer : ThemedForm
    {
        public ExceptionViewer()
        {
            InitializeComponent();
            pgrDetails.PropertySort = PropertySort.NoSort;
            pgrDetails.HelpVisible = false;
            pgrDetails.ToolbarVisible = false;
            var vScrollBar = pgrDetails.GetVScrollBar();
            if (vScrollBar != null)
                vScrollBar.HandleCreated += (s, e) => ThemeApplier.ApplyScrollBarTheme(s as Control, ThemeApplier.IsDark(BackColor));
        }

        protected override void OnUiThemeApplied()
        {
            BeginInvoke(() => ThemeApplier.ApplyScrollBarTheme(pgrDetails.GetVScrollBar(), ThemeApplier.IsDark(BackColor)));
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
                using (var frm = new ThemedForm())
                {
                    frm.AutoScaleMode = AutoScaleMode.Dpi;
                    float scaleFactor = frm.DeviceDpi / 120f;
                    frm.Text = "Stack Trace";
                    frm.Size = new Size((int)(600 * scaleFactor), (int)(400 * scaleFactor));
                    frm.FormBorderStyle = FormBorderStyle.SizableToolWindow;
                    frm.KeyPreview = true;
                    frm.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) frm.Close(); };
                    var textBox = new ThemedTextBox
                    {
                        Multiline = true,
                        ReadOnly = true,
                        Dock = DockStyle.Fill,
                        ScrollBars = ScrollBars.Vertical,
                        Text = stackTrace,
                        Font = new Font(new FontFamily("Consolas"), 9f),
                        SelectionStart = 0
                    };
                    frm.Controls.Add(textBox);
                    frm.UiThemeApplied += (s, e) =>
                    {
                        textBox.Font = new Font("Consolas", 9); // restore monospace font because it could be changed by ThemeApplier
                        frm.BeginInvoke(() => ThemeApplier.ApplyScrollBarTheme(textBox, ThemeApplier.IsDark(frm.BackColor)));
                    };

                    var mi = frm.GetType().GetMethod("CenterToParent", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (mi != null)
                        mi.Invoke(frm, null); // center early to prevent visual flickering during the population of controls
                    frm.ShowDialog();
                }
            }
            return value;
        }
    }
}
