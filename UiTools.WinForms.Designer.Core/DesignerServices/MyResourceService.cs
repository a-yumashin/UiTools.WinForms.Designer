using System.Collections;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Resources;

namespace UiTools.WinForms.Designer.Core
{
    internal class MyResourceService : IResourceService
    {
        private readonly string resxFilePath;

        public MyResourceService(string resxFilePath)
        {
            this.resxFilePath = resxFilePath;
        }

        public IResourceReader GetResourceReader(CultureInfo info)
        {
            if (File.Exists(resxFilePath))
                return new ResXResourceReader(resxFilePath);

            return new EmptyResourceReader();
        }

        public IResourceWriter GetResourceWriter(CultureInfo info)
        {
            return new ResXResourceWriter(resxFilePath);
        }

        private class EmptyResourceReader : IResourceReader
        {
            public IDictionaryEnumerator GetEnumerator() => new Hashtable().GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            public void Close() { }
            public void Dispose() { }
        }
    }
}
