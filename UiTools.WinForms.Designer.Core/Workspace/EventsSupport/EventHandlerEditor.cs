using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace UiTools.WinForms.Designer.Core
{
    /// <summary>
    /// UI editor for selecting an event handler method in the PropertyGrid.
    /// </summary>
    public class EventHandlerEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            if (context != null && context.Instance != null)
                return UITypeEditorEditStyle.DropDown;
            return base.GetEditStyle(context);
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (context == null || provider == null || context.Instance == null)
                return value;

            var editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (editorService == null)
                return value;

            if (context.PropertyDescriptor is EventAsPropertyDescriptor eventPropertyDescriptor)
            {
                var eventBrowser = eventPropertyDescriptor.EventBrowser;
                var trs = eventBrowser.Component.Site.GetService(typeof(ITypeResolutionService)) as ITypeResolutionService;

                var candidates = EventHelper.GetEventHandlerCandidates(
                    eventBrowser.MainFilePath, eventBrowser.ClassIdentifier, eventPropertyDescriptor.EventInfo, trs);
                var currentValue = value as string;

                if (!string.IsNullOrEmpty(currentValue) && !candidates.Contains(currentValue))
                    candidates.Insert(0, currentValue);

                if (!candidates.Contains(string.Empty))
                    candidates.Insert(0, string.Empty);

                var listBox = new ListBox
                {
                    BorderStyle = BorderStyle.None,
                    IntegralHeight = false,
                    SelectionMode = SelectionMode.One
                };
                listBox.Items.AddRange(candidates.ToArray());
                listBox.Height = Math.Min(300, candidates.Count * Math.Max(20, listBox.ItemHeight) + 6);

                if (!string.IsNullOrEmpty(currentValue) && candidates.Contains(currentValue))
                    listBox.SelectedItem = currentValue;
                else
                    listBox.SelectedIndex = 0;

                void OnListClick(object s, EventArgs ea)
                {
                    if (listBox.SelectedIndex >= 0)
                        editorService.CloseDropDown();
                }

                listBox.Click += OnListClick;
                editorService.DropDownControl(listBox);
                listBox.Click -= OnListClick;

                string selectedValue = listBox.SelectedItem as string;
                return string.IsNullOrEmpty(selectedValue) ? null : selectedValue;
            }

            return value;
        }
    }
}
