using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;

namespace UiTools.WinForms.Designer.Core
{
    internal static class XmlValidator
    {
        public static List<string> Validate(string xmlContents, string xmlSchemaContents)
        {
            var errors = new List<string>();

            try
            {
                var settings = new XmlReaderSettings();
                using (var schemaReader = new StringReader(xmlSchemaContents))
                {
                    settings.Schemas.Add(null, XmlReader.Create(schemaReader));
                }

                settings.ValidationType = ValidationType.Schema;

                settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
                settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessIdentityConstraints;
                settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;

                settings.ValidationEventHandler += (sender, args) =>
                {
                    string severity = args.Severity == XmlSeverityType.Warning ? "Warning" : "Error";
                    errors.Add($"[{severity}] Line {args.Exception.LineNumber}, Pos {args.Exception.LinePosition}: {args.Message}");
                };

                using (StringReader xmlReader = new StringReader(xmlContents))
                using (XmlReader reader = XmlReader.Create(xmlReader, settings))
                {
                    while (reader.Read()) { }
                }
            }
            catch (XmlException ex)
            {
                // XML is broken (unclosed tags etc)
                errors.Add($"[Critical XML Error]: {ex.Message}");
            }
            catch (Exception ex)
            {
                errors.Add($"[General Error]: {ex.Message}");
            }

            return errors;
        }
    }
}
