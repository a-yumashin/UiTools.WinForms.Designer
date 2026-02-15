using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Resources;

namespace UiTools.WinForms.Designer.Core
{
    internal class DesignerResourceManager : ComponentResourceManager
    {
        private readonly string resxFilePath;

        public DesignerResourceManager(Type componentType, string resxFilePath) : base(componentType)
        {
            this.resxFilePath = resxFilePath;
        }

        public override string GetString(string name) => GetString(name, CultureInfo.InvariantCulture);

        public override string GetString(string name, CultureInfo culture)
        {
            return GetObject(name, culture) as string;
        }

        public override object GetObject(string name) => GetObject(name, CultureInfo.InvariantCulture);

        public override object GetObject(string name, CultureInfo culture)
        {
            if (!File.Exists(resxFilePath))
                return null;

            try
            {
                using (var reader = new ResXResourceReader(resxFilePath))
                {
                    reader.BasePath = Path.GetDirectoryName(resxFilePath);
                    // If the .resx file contains relative paths to external files (such as icons, not embedded as Base64
                    // but located alongside it), then by specifying a BasePath, the ResXResourceReader can locate them.

                    var enumerator = reader.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        if (string.Equals(enumerator.Key.ToString(), name, StringComparison.OrdinalIgnoreCase))
                            return enumerator.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageLogger.LogError(null, $"Error getting resource '{name}' from '{resxFilePath}': {ex.Message}", ex);
            }
            return null;
        }
    }
}
