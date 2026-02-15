using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using System.Xml;

namespace UiTools.WinForms.Designer
{
    internal static class XmlHelper
    {
        public static string Serialize(object obj, Encoding enc, bool omitXmlDeclaration = false, bool indent = true)
        {
            string result;
            XmlSerializer x = new XmlSerializer(obj.GetType());
            var ns = new XmlSerializerNamespaces();
            ns.Add("", ""); // (avoids "xmlns" attributes in the generated XML)
            using (Stream stream = new MemoryStream())
            {
                XmlWriter xwr = XmlWriter.Create(stream,
                    new XmlWriterSettings { Encoding = enc, OmitXmlDeclaration = omitXmlDeclaration, Indent = indent });
                x.Serialize(xwr, obj, ns);
                xwr.Flush();
                stream.Seek(0, SeekOrigin.Begin);
                using (StreamReader reader = new StreamReader(stream, enc))
                {
                    result = reader.ReadToEnd();
                    reader.Close();
                }
                stream.Close();
            }
            return result;
        }

        public static T Deserialize<T>(string objXml) where T : class
        {
            if (string.IsNullOrEmpty(objXml))
                return null;
            try
            {
                T obj = null;
                XmlSerializer xs = new XmlSerializer(typeof(T));
                using (StringReader sr = new StringReader(objXml))
                {
                    XmlTextReader xtr = new XmlTextReader(sr);
                    obj = xs.Deserialize(xtr) as T;
                    xtr.Close();
                    sr.Close();
                }
                return obj;
            }
            catch (Exception ex)
            {
                throw new Exception("Deserialization failed: " + ex.Message, ex);
            }
        }
    }
}
