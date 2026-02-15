using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;

namespace UiTools.WinForms.Designer.Core
{
    // A provider that says: "For ComponentResourceManager, use my serializer"
    internal class MyResourceSerializationProvider : IDesignerSerializationProvider
    {
        private readonly string resxFilePath;
        public MyResourceSerializationProvider(string resxFilePath) => this.resxFilePath = resxFilePath;

        public object GetSerializer(IDesignerSerializationManager manager, object currentSerializer, Type objectType, Type serializerType)
        {
            if (objectType == typeof(ComponentResourceManager) && serializerType == typeof(CodeDomSerializer))
                return new MyResourceCodeDomSerializer(resxFilePath);
            return null; // for all other cases, use standard serializers
        }
    }

    // A serializer that creates our DesignerResourceManager instead of the standard manager
    internal class MyResourceCodeDomSerializer : CodeDomSerializer
    {
        private readonly string resxFilePath;
        public MyResourceCodeDomSerializer(string resxFilePath) => this.resxFilePath = resxFilePath;

        public override object Deserialize(IDesignerSerializationManager manager, object codeObject)
        {
            object resManager = manager.GetInstance("resources");
            if (resManager == null)
            {
                resManager = new DesignerResourceManager(typeof(object), resxFilePath);
                manager.SetName(resManager, "resources");
            }
            return resManager;
        }
    }
}
