using System;
using System.ComponentModel;
using System.Reflection;
using System.Drawing.Design;
using System.Linq;
using System.ComponentModel.Design;

namespace UiTools.WinForms.Designer.Core
{
    /// <summary>
    /// Custom PropertyDescriptor for events, allowing the PropertyGrid
    /// to display and edit the event handler name.
    /// </summary>
    public class EventAsPropertyDescriptor : PropertyDescriptor
    {
        private readonly EventInfo eventInfo;
        private readonly EventBrowser eventBrowser;

        public EventInfo EventInfo => eventInfo;
        public EventBrowser EventBrowser => eventBrowser;

        public EventAsPropertyDescriptor(EventInfo eventInfo, EventBrowser eventBrowser)
            : base(eventInfo.Name, eventInfo.GetCustomAttributes().Cast<Attribute>().ToArray())
        {
            this.eventInfo = eventInfo;
            this.eventBrowser = eventBrowser;
        }

        public override Type ComponentType => eventBrowser.Component.GetType();
        public override bool IsReadOnly => false; // events can be edited
        public override Type PropertyType => typeof(string); // we are editing the handler method name (string)
        public override bool CanResetValue(object component) => true; // can be reset (i.e. remove the handler)
        public override object GetValue(object component)
        {
            var realComponent = eventBrowser.Component; // component here is EventBrowser, but we need the actual component
            if (realComponent == null)
                return null;

            var sp = realComponent.Site as IServiceProvider;
            if (sp == null)
                return null;

            var ebs = sp.GetService(typeof(IEventBindingService)) as IEventBindingService;
            if (ebs != null)
            {
                try
                {
                    EventDescriptor ed = TypeDescriptor.GetEvents(realComponent)[eventInfo.Name];
                    var bindingProp = ebs.GetEventProperty(ed);
                    // Return as is: either a string with the name, or null
                    return bindingProp?.GetValue(realComponent) as string;
                }
                catch (Exception ex)
                {
                    MessageLogger.LogError(this, $"Failed to get event handler from IEventBindingService: {ex.Message}", ex);
                }
            }
            else
                MessageLogger.LogVerbose(this, $"IEventBindingService is not available; returning null");
            return null;
        }

        public override void SetValue(object component, object value)
        {
            string inputString = value as string;
            bool removeEventSubscription = string.IsNullOrEmpty(inputString);

            // The name we WANT to see. If this is a removal, the name doesn't matter (passing null):
            string desiredHandlerName = removeEventSubscription
                ? null
                : inputString;
            string currentHandlerName = GetValue(component) as string; // current handler value

            if (string.Equals(currentHandlerName, desiredHandlerName, StringComparison.Ordinal))
                return;

            var realComponent = eventBrowser.Component;
            if (realComponent == null)
            {
                MessageLogger.LogError(this, "EventBrowser.Component is null");
                return;
            }

            var sp = realComponent.Site as IServiceProvider;
            if (sp == null)
                return;

            var trs = sp.GetService(typeof(ITypeResolutionService)) as ITypeResolutionService;

            if (!EventHelper.UpdateEventSubscription(
                realComponent,
                eventInfo.Name,
                removeEventSubscription,
                desiredHandlerName, // during removal - null will be passed
                eventBrowser.MainFilePath,
                eventBrowser.ClassIdentifier,
                trs,
                out _))
            {
                return;
            }
        }

        public override void ResetValue(object component)
        {
            // Reset value (remove the handler)
            SetValue(component, null);
        }

        public override bool ShouldSerializeValue(object component) => GetValue(component) != null;

        // Override GetEditor so PropertyGrid uses our EventHandlerEditor
        public override object GetEditor(Type editorBaseType)
        {
            if (editorBaseType == typeof(UITypeEditor))
                return new EventHandlerEditor();
            return base.GetEditor(editorBaseType);
        }
    }
}
