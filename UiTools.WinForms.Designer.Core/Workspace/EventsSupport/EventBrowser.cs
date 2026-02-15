using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace UiTools.WinForms.Designer.Core
{
    /// <summary>
    /// Class for displaying component events in the PropertyGrid.
    /// Implements ICustomTypeDescriptor to override PropertyGrid behavior.
    /// </summary>
    public class EventBrowser : ICustomTypeDescriptor
    {
        private readonly Component component;
        private readonly string mainFilePath;
        private readonly string classIdentifier;

        public Component Component => component;
        public string MainFilePath => mainFilePath;
        public string ClassIdentifier => classIdentifier;

        public EventBrowser(Component component, string mainFilePath, string classIdentifier)
        {
            this.component = component;
            this.mainFilePath = mainFilePath;
            this.classIdentifier = classIdentifier;
        }

        #region ICustomTypeDescriptor members

        public AttributeCollection GetAttributes() => TypeDescriptor.GetAttributes(component);
        public string GetClassName() => TypeDescriptor.GetClassName(component);
        public string GetComponentName() => TypeDescriptor.GetComponentName(component);
        public TypeConverter GetConverter() => TypeDescriptor.GetConverter(component);
        public EventDescriptor GetDefaultEvent() => TypeDescriptor.GetDefaultEvent(component);
        public PropertyDescriptor GetDefaultProperty() => null;
        public object GetEditor(Type editorBaseType) => TypeDescriptor.GetEditor(component, editorBaseType);
        public object GetPropertyOwner(PropertyDescriptor pd) => component;
        public EventDescriptorCollection GetEvents() => EventDescriptorCollection.Empty;
        public EventDescriptorCollection GetEvents(Attribute[] attributes) => EventDescriptorCollection.Empty;

        /// <summary>
        /// Returns a collection of PropertyDescriptors that represent component *events*.
        /// The PropertyGrid will display them as regular *properties*.
        /// </summary>
        public PropertyDescriptorCollection GetProperties()
        {
            var eventInfos = EventHelper.GetPublicEvents(component);
            var propertyDescriptors = new List<PropertyDescriptor>();
            foreach (EventInfo eventInfo in eventInfos)
            {
                propertyDescriptors.Add(new EventAsPropertyDescriptor(eventInfo, this));
            }
            return new PropertyDescriptorCollection(propertyDescriptors.ToArray());
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            return GetProperties(); // I don't need filtering by attributes
        }

        #endregion ICustomTypeDescriptor members
    }
}
