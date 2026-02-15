using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Linq;

namespace UiTools.WinForms.Designer.Core
{
    internal class MyDesignerSerializationService : IDesignerSerializationService
    {
        private readonly IServiceProvider serviceProvider;

        public MyDesignerSerializationService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public ICollection Deserialize(object serializationData)
        {
            var serializationStore = serializationData as SerializationStore;
            if (serializationStore != null)
            {
                var componentSerializationService = serviceProvider.GetService(typeof(ComponentSerializationService)) as ComponentSerializationService;
                return componentSerializationService.Deserialize(serializationStore);
            }
            return Array.Empty<object>();
        }

        public object Serialize(ICollection objects)
        {
            var componentSerializationService = serviceProvider.GetService(typeof(ComponentSerializationService)) as ComponentSerializationService;
            using (var serializationStore = componentSerializationService.CreateStore())
            {
                foreach (object obj in objects.OfType<Component>())
                    componentSerializationService.Serialize(serializationStore, obj);
                return serializationStore;
            }
            // NOTE: Using a "using" statement (or calling SerializationStore.Dispose() explicitly right after the loop) is mandatory here.
            // This method essentially acts as a "flush", persisting serialized objects into the private SerializationStore._objectState field.
        }
    }
}
