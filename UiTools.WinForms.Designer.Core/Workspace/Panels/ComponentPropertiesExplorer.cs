using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UiTools.WinForms.Designer.Core.Properties;

namespace UiTools.WinForms.Designer.Core
{
    public partial class ComponentPropertiesExplorer : UserControl
    {
        public enum SelectAfterAddEnum
        {
            DoNotSelect,
            Select,
            SelectAsync // with BeginInvoke() - to be used from DesignerWorkspace.OnComponentAdded()
        }

        public event ComponentPropertiesRequiredHandler ComponentPropertiesRequired;

        private bool ignoreComponentSelectionEvent = false;
        private readonly ToolStrip ts;

        private Component component;
        private string mainFilePath;
        private string classIdentifier;

        public delegate void ComponentPropertiesRequiredHandler(object sender, ComponentPropertiesRequiredArgs args);

        public ComponentPropertiesExplorer()
        {
            InitializeComponent();
            comboBoxEx1.Sorted = true;
            comboBoxEx1.SelectedIndexChanged += comboBoxEx1_SelectedIndexChanged;
            propertyGrid1.PropertySort = PropertySort.Alphabetical;
            propertyGrid1.GetGridView().MouseDoubleClick += GridView_MouseDoubleClick;
            ts = propertyGrid1.Controls.OfType<ToolStrip>().FirstOrDefault();
            if (ts != null)
            {
                ts.Items.RemoveAt(ts.Items.Count - 1); // remove "Property Pages" button
                ts.Items.Add(new ToolStripButton(
                    "", Resources.Property, OnShowPropertiesClick, "tsbShowProperties") { ToolTipText = "show properties", CheckOnClick = true, Checked = true });
                ts.Items.Add(new ToolStripButton(
                    "", Resources.Event, OnShowEventsClick, "tsbShowEvents") { ToolTipText = "show events", CheckOnClick = true });
            }
        }

        private void GridView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var tsbShowEvents = ts.Items["tsbShowEvents"] as ToolStripButton;
            if (!tsbShowEvents.Checked)
                return;

            GridItem entry = GetGridItemFromMouse((Control)sender, e.X, e.Y);
            if (entry == null)
                return;
            string eventName = entry.Label; // e.g. "Click"

            IServiceProvider sp = component.Site;
            var trs = sp?.GetService(typeof(ITypeResolutionService)) as ITypeResolutionService;
            EventDescriptor ed = TypeDescriptor.GetEvents(component)[eventName];

            EventHelper.UpdateEventSubscription(
                component,
                eventName,
                false,
                EventHelper.CreateUniqueMethodName(component, ed, mainFilePath, classIdentifier, trs),
                mainFilePath,
                classIdentifier,
                trs,
                out _);
            propertyGrid1.Focus();
        }

        private GridItem GetGridItemFromMouse(Control gridView, int x, int y)
        {
            Point InvalidPosition = new Point(int.MinValue, int.MinValue);
            try
            {
                var miFindPosition = gridView.GetType().GetMethod("FindPosition", BindingFlags.Instance | BindingFlags.NonPublic);
                Point pt = (Point)miFindPosition.Invoke(gridView, new object[] { x, y });
                if (pt != InvalidPosition)
                {
                    var miGetGridEntryFromRow = gridView.GetType().GetMethod("GetGridEntryFromRow", BindingFlags.Instance | BindingFlags.NonPublic);
                    return (GridItem)miGetGridEntryFromRow.Invoke(gridView, new object[] { pt.Y });
                }
            }
            catch (Exception ex)
            {
                MessageLogger.LogError(this, $"Failed to get GridItem from MouseDoubleClick position: {ex.Message}", ex);
            }
            return null;
        }

        private void OnShowPropertiesClick(object sender, EventArgs e)
        {
            var tsbShowProperties = ts.Items["tsbShowProperties"] as ToolStripButton;
            var tsbShowEvents = ts.Items["tsbShowEvents"] as ToolStripButton;
            tsbShowEvents.Checked = !tsbShowProperties.Checked;
            if (component != null)
                ShowComponentPropertiesOrEvents(component);
        }

        private void OnShowEventsClick(object sender, EventArgs e)
        {
            var tsbShowEvents = ts.Items["tsbShowEvents"] as ToolStripButton;
            var tsbShowProperties = ts.Items["tsbShowProperties"] as ToolStripButton;
            tsbShowProperties.Checked = !tsbShowEvents.Checked;
            if (component != null)
                ShowComponentPropertiesOrEvents(component);
        }

        public void PrepareForBrowsingEvents(string mainFilePath, string classIdentifier)
        {
            this.mainFilePath = mainFilePath;
            this.classIdentifier = classIdentifier;
        }

        public void RefreshGrid()
        {
            propertyGrid1.Refresh();
        }

        public void AddComponentToList(string nameAndType, SelectAfterAddEnum selectAfterAdd)
        {
            if (comboBoxEx1.Items.Contains(nameAndType))
                return;
            var index = comboBoxEx1.Items.Add(nameAndType);
            if (selectAfterAdd == SelectAfterAddEnum.Select)
                comboBoxEx1.SelectedIndex = index;
            else if (selectAfterAdd == SelectAfterAddEnum.SelectAsync)
                BeginInvoke(new MethodInvoker(() => comboBoxEx1.SelectedIndex = index));
            // NOTE: If setting SelectedIndex without BeginInvoke(), then when adding a ToolStripButton to a ToolStrip via the "InSitu Editor"
            //       - this ToolStripButton (1) is not drawn, (2) is not "selected", (3) is subsequently not deleted along with its ToolStrip (whereas
            //       when adding via the "Edit items..." context menu item - everything works fine).
            //       It seems that changing SelectedIndex mysteriously breaks the designer transaction, as a result of which the ToolStripButton does
            //       not end up in the ToolStrip.Items collection (which is evident in the "Edit items..." dialog - the button is missing from the list).
            //       Therefore, when calling from the DesignerWorkspace.OnComponentAdded() method - I use SelectAfterAddEnum.SelectAsync!
        }

        public void ClearComponentsList()
        {
            ignoreComponentSelectionEvent = true;
            comboBoxEx1.Clear();
            propertyGrid1.SelectedObject = null;
            ignoreComponentSelectionEvent = false;
        }

        public void RemoveComponentFromList(string nameAndType)
        {
            var index = comboBoxEx1.Items.IndexOf(nameAndType);
            if (index >= 0)
                comboBoxEx1.Items.RemoveAt(index);
        }

        public void RenameComponentInList(string nameAndTypeOld, string nameAndTypeNew)
        {
            var index = comboBoxEx1.Items.IndexOf(nameAndTypeOld);
            if (index < 0)
            {
                MessageLogger.LogError(this, $"Item '{nameAndTypeOld}' not found in components list");
                return;
            }
            ignoreComponentSelectionEvent = true;
            // NOTE: for some reason, rename leads to SelectedIndexChanged, which interferes with my logic; specifically, when renaming a non-TopLevel
            //       component, I would have to look up this component (in the ComponentPropertiesRequired event handler) by its composite name
            //       (like "toolStripContainer1.RightToolStripPanel") - which is extra effort that can be easily avoided with the ignoreComponentSelectionEvent flag.
            comboBoxEx1.Items[index] = nameAndTypeNew;
            ignoreComponentSelectionEvent = false;
        }

        private void SelectDefaultEvent()
        {
            string defaultEventName = TypeDescriptor.GetDefaultEvent(component.GetType())?.Name;
            if (!string.IsNullOrEmpty(defaultEventName))
            {
                var gridItem = propertyGrid1.GetGridItemFromLabel(defaultEventName);
                if (gridItem != null)
                    propertyGrid1.SelectedGridItem = gridItem;
            }
        }

        /// <summary>
        /// Used only for non-TopLevel components such as splitContainer1.Panel1 and toolStripContainer1.RightToolStripPanel, since the second overload works
        /// via the ComponentPropertiesRequired event, and searching for a non-TopLevel component by its composite name adds unnecessary complexity.
        /// </summary>
        public void ShowComponentPropertiesOrEvents(IComponent component)
        {
            this.component = component as Component;
            var tsbShowProperties = ts.Items["tsbShowProperties"] as ToolStripButton;
            if (tsbShowProperties.Checked)
                propertyGrid1.SelectedObject = component; // show properties
            else
            {
                propertyGrid1.SelectedObject = new EventBrowser(this.component, mainFilePath, classIdentifier); // show events
                SelectDefaultEvent();
                propertyGrid1.Focus();
            }
        }

        /// <summary>
        /// Used only for TopLevel components - all those that are added to comboBoxEx1 in the OnComponentAdded method (Button, ImageList, Form etc).
        /// Selecting a component by a given nameAndType ("button1 System.Windows.Forms.Button" etc) triggers the comboBoxEx1_SelectedIndexChanged event,
        /// which, in turn, triggers the ComponentPropertiesRequired event; through its arguments, external code returns the component itself
        /// for displaying its properties in propertyGrid1 (after finding this component by nameAndType).
        /// This same mechanism (comboBoxEx1_SelectedIndexChanged --> ComponentPropertiesRequired event) also works when the component is selected by the user.
        /// </summary>
        public void ShowComponentPropertiesOrEvents(string nameAndType)
        {
            comboBoxEx1.SelectedIndex = comboBoxEx1.Items.IndexOf(nameAndType); // raises comboBoxEx1_SelectedIndexChanged event
        }

        public void SetTextMeasurer(TextMeasurer textMeasurer)
        {
            comboBoxEx1.SetTextMeasurer(textMeasurer);
        }

        public void ShowSeveralComponentsProperties(IComponent[] components)
        {
            ignoreComponentSelectionEvent = true;
            comboBoxEx1.SelectedIndex = -1;
            ignoreComponentSelectionEvent = false;
            propertyGrid1.SelectedObjects = components;
        }

        private void comboBoxEx1_SelectedIndexChanged(object sender, EventArgs e)
        {
            OnSelectedComponentChanged();
        }

        private void OnSelectedComponentChanged()
        {
            if (ignoreComponentSelectionEvent)
                return;
            if (comboBoxEx1.SelectedIndex < 0)
            {
                propertyGrid1.SelectedObject = null;
                return;
            }
            var args = new ComponentPropertiesRequiredArgs(comboBoxEx1.Items[comboBoxEx1.SelectedIndex].ToString());
            ComponentPropertiesRequired?.Invoke(this, args);
            ShowComponentPropertiesOrEvents(args.Component);
            RemoveNonTopLevelComponentsFromList();
        }

        public void RemoveNonTopLevelComponentsFromList()
        {
            for (int i = comboBoxEx1.Items.Count - 1; i >= 0; i--)
            {
                if (i != comboBoxEx1.SelectedIndex && !IsTopLevelComponent(comboBoxEx1.Items[i].ToString()))
                    comboBoxEx1.Items.RemoveAt(i);
            }
        }

        private bool IsTopLevelComponent(string nameAndType)
        {
            var parts = nameAndType.Split(" ".ToCharArray(), 2);
            if (parts.Length != 2)
                return true;
            return !parts[0].Contains("."); // i.e. when it does NOT look like "splitContainer1.Panel1"
        }
    }

    public class ComponentPropertiesRequiredArgs : EventArgs
    {
        public ComponentPropertiesRequiredArgs(string nameAndType)
        {
            ComponentNameAndType = nameAndType;
        }

        public string ComponentNameAndType { get; private set; }
        public IComponent Component { get; set; }
    }
}
