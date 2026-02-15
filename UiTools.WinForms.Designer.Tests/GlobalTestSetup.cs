using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace UiTools.WinForms.Designer.Tests
{
    [TestClass]
    public class GlobalTestSetup
    {
        public static string RootDirShortName = "UiTools.WinForms.Designer.Tests";

        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            string rootDirFullName = Path.Combine(Path.GetTempPath(), RootDirShortName);
            if (Directory.Exists(rootDirFullName))
            {
                try
                {
                    Directory.Delete(rootDirFullName, true);
                }
                catch
                {
                }
            }
        }
    }
}
