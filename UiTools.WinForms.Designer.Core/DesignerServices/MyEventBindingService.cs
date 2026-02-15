using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Runtime.CompilerServices;
using static UiTools.WinForms.Designer.Core.CommonStuff;

namespace UiTools.WinForms.Designer.Core
{
    public class MyEventBindingService : IEventBindingService
    {
        // We store bindings in a ConditionalWeakTable: for each component - a dictionary of eventName -> handlerName.
        private readonly ConditionalWeakTable<IComponent, Dictionary<string, string>> eventBindings =
            new ConditionalWeakTable<IComponent, Dictionary<string, string>>();

        public PropertyDescriptor GetEventProperty(EventDescriptor e)
            => new BindingPropertyDescriptor(e, this);

        // Returns (or creates) the dictionary for the component.
        internal Dictionary<string, string> GetOrCreateBindingsFor(IComponent component)
        {
            ThrowIfNullOrEmpty(component);

            if (!eventBindings.TryGetValue(component, out var dict))
            {
                dict = new Dictionary<string, string>();
                eventBindings.Add(component, dict);
            }
            return dict;
        }

        internal bool TryGetBinding(IComponent component, string eventName, out string handlerName)
        {
            handlerName = null;
            if (component == null || string.IsNullOrEmpty(eventName))
                return false;
            if (eventBindings.TryGetValue(component, out var dict))
                return dict.TryGetValue(eventName, out handlerName);
            return false;
        }

        internal void SetBinding(IComponent component, string eventName, string handlerName)
        {
            ThrowIfNullOrEmpty(component);
            ThrowIfNullOrEmpty(eventName);

            var dict = GetOrCreateBindingsFor(component);
            if (handlerName == null)
                dict.Remove(eventName);
            else
                dict[eventName] = handlerName;
        }

        // Auxiliary internal descriptor for IEventBindingService.GetEventProperty()
        private class BindingPropertyDescriptor : PropertyDescriptor
        {
            private readonly EventDescriptor eventDescriptor;
            private readonly MyEventBindingService service;

            public BindingPropertyDescriptor(EventDescriptor e, MyEventBindingService srv)
                : base(e.Name, new Attribute[] { DesignOnlyAttribute.Yes, BrowsableAttribute.Yes })
            {
                eventDescriptor = e ?? throw new ArgumentNullException(nameof(e));
                service = srv ?? throw new ArgumentNullException(nameof(srv));
            }

            public override Type ComponentType => eventDescriptor.ComponentType ?? typeof(object);
            public override bool IsReadOnly => false;
            public override Type PropertyType => typeof(string);
            public override bool CanResetValue(object component) => true;

            public override object GetValue(object component)
            {
                if (component is IComponent comp)
                    return service.TryGetBinding(comp, eventDescriptor.Name, out var name) ? name : null;
                return null;
            }

            public override void SetValue(object component, object value)
            {
                if (!(component is IComponent comp))
                    throw new ArgumentException("Component must be IComponent", nameof(component));

                string handlerName = value as string;
                if (string.IsNullOrEmpty(handlerName))
                    service.SetBinding(comp, eventDescriptor.Name, null);
                else
                    service.SetBinding(comp, eventDescriptor.Name, handlerName);
                //MessageLogger.Log(this, $"SET VALUE: {component} -> {value}");
            }

            public override void ResetValue(object component)
            {
                if (component is IComponent comp)
                    service.SetBinding(comp, eventDescriptor.Name, null);
            }

            public override bool ShouldSerializeValue(object component)
            {
                return component is IComponent comp && service.TryGetBinding(comp, eventDescriptor.Name, out _);
            }

            // Set the EventDescriptor so that IEventBindingService.GetEvent can return it
            public EventDescriptor EventDescriptor => eventDescriptor;
        }

        public string CreateUniqueMethodName(IComponent component, EventDescriptor e)
        {
            return EventHelper.CreateUniqueMethodName(component, e, null, null, null); // returns the simplest possible implementation: componentName_eventName
        }

        public ICollection GetCompatibleMethods(EventDescriptor e)
        {
            // We use our own mechanism (EventHelper.GetEventHandlerCandidates()),
            // so we return an empty collection here.
            return Array.Empty<string>();
        }

        public EventDescriptor GetEvent(PropertyDescriptor pd)
        {
            return pd is BindingPropertyDescriptor bpd
                ? bpd.EventDescriptor
                : null;
        }

        public PropertyDescriptorCollection GetEventProperties(EventDescriptorCollection events)
        {
            ThrowIfNullOrEmpty(events);

            PropertyDescriptor[] props = new PropertyDescriptor[events.Count];
            for (int i = 0; i < events.Count; i++)
            {
                props[i] = GetEventProperty(events[i]);
            }
            return new PropertyDescriptorCollection(props);
        }

        public bool ShowCode() => false;
        public bool ShowCode(int lineNumber) => false;
        public bool ShowCode(IComponent component, EventDescriptor e) => false;
    }
}
