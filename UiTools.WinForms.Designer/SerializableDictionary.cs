using System.Collections.Generic;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml;

namespace UiTools.WinForms.Designer
{
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
    {
        public const string ITEM_ELEMENT = "DictionaryEntry";
        public const string KEY_ELEMENT = "Key";
        public const string VALUE_ELEMENT = "Value";

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            if (reader.IsEmptyElement)
                return;

            var keySerializer = new XmlSerializer(typeof(TKey));
            var valueSerializer = new XmlSerializer(typeof(TValue));

            reader.ReadStartElement();

            while (reader.IsStartElement(ITEM_ELEMENT))
            {
                reader.ReadStartElement(ITEM_ELEMENT);

                reader.ReadStartElement(KEY_ELEMENT);
                TKey key = (TKey)keySerializer.Deserialize(reader);
                reader.ReadEndElement();

                reader.ReadStartElement(VALUE_ELEMENT);
                TValue value = (TValue)valueSerializer.Deserialize(reader);
                reader.ReadEndElement();

                reader.ReadEndElement();
                Add(key, value);
            }
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            var keySerializer = new XmlSerializer(typeof(TKey));
            var valueSerializer = new XmlSerializer(typeof(TValue));

            var ns = new XmlSerializerNamespaces();
            ns.Add("", ""); // (avoids "xmlns" attributes in the generated XML)

            foreach (var kvp in this)
            {
                writer.WriteStartElement(ITEM_ELEMENT);

                writer.WriteStartElement(KEY_ELEMENT);
                keySerializer.Serialize(writer, kvp.Key, ns);
                writer.WriteEndElement();

                writer.WriteStartElement(VALUE_ELEMENT);
                valueSerializer.Serialize(writer, kvp.Value, ns);
                writer.WriteEndElement();

                writer.WriteEndElement();
            }
        }
    }
}
