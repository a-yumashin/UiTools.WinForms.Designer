using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using UiTools.WinForms.Designer.Core;

namespace UiTools.WinForms.Designer.Tests
{
    [TestClass]
    public class EventHelperTests
    {
        /*
         * Folder structure:
         * %Temp%
         *   UiTools.WinForms.Designer.Tests // constant value from GlobalTestSetup.RootDirShortName; this folder is deleted before each run
         *     EventHelperTests              // name of this class
         *       sqficvzf.04q                // unique for each run; gets generated with Path.GetRandomFileName(); [rootDir ends here]
         *         Case.1                    // [testDir ends here]
         *           Form1.cs
         */
        private string rootDir;
        private string csFilePath;

        private IContainer components;
        private Button btn;
        private Form frm;

        private readonly string inputCs = @"using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class TestForm1 : Form
    {
        public TestForm1()
        {
            InitializeComponent();
        }

        private void button1_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
        }

        private void TestForm1_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            // I'm not empty!
        }
    }
}";

        [TestInitialize]
        public void Setup()
        {
            rootDir = Path.Combine(Path.GetTempPath(), GlobalTestSetup.RootDirShortName, nameof(EventHelperTests), Path.GetRandomFileName());

            components = new Container();
            btn = new Button();
            frm = new Form { Name = "Form1" };
            components.Add(btn, "button1"); // otherwise btn.Site?.Name is null
        }

        [TestMethod("Case 1: creating new event handler in the main file")]
        public void TryCreateEventHandlerInMainFile_NewHandler()
        {
            csFilePath = CreateMainCsFile(Path.Combine(rootDir, "Case.1"), inputCs);
            var trs = MyTypeResolutionService.CreateWithReferencedAssemblies(null, new DesignerCsFileContext());
            bool success = EventHelper.TryCreateEventHandlerInMainFile(csFilePath, "TestForm1", btn, "Click", "button1_Click", trs, out _);
            // We expect that empty method "private void button1_Click(object sender, System.EventArgs e)"
            // will be created in the main file.
            Assert.IsTrue(success);
            var csFileContentsActual = File.ReadAllText(csFilePath);
            var csFileContentsExpected = inputCs.Replace(
                "            // I'm not empty!\r\n        }\r\n",
                "            // I'm not empty!\r\n        }\r\n\r\n        private void button1_Click(object sender, System.EventArgs e)\r\n        {\r\n\r\n        }\r\n");
            Assert.AreEqual(csFileContentsExpected,
                            csFileContentsActual,
                            "Main file content mismatch");
        }

        [TestMethod("Case 2: delete existing handler method in the main file (when it is EMPTY)")]
        public void TryDeleteEventHandlerInMainFile_HandlerIsEmpty()
        {
            // Line "private void button1_Paint(object sender, System.Windows.Forms.PaintEventArgs e) { }" (empty!) is present in the main .cs file
            csFilePath = CreateMainCsFile(Path.Combine(rootDir, "Case.2"), inputCs);
            var trs = MyTypeResolutionService.CreateWithReferencedAssemblies(null, new DesignerCsFileContext());
            bool success = EventHelper.TryDeleteEventHandlerInMainFile(csFilePath, "TestForm1", btn, "Paint", "button1_Paint", trs);
            // We expect that handler method button1_Paint will be removed from the main .cs file (because this method is EMPTY).
            Assert.IsTrue(success);
            var csFileContentsActual = File.ReadAllText(csFilePath);
            var csFileContentsExpected = inputCs.Replace(
                "        private void button1_Paint(object sender, System.Windows.Forms.PaintEventArgs e)\r\n        {\r\n        }\r\n\r\n",
                "");
            Assert.AreEqual(csFileContentsExpected,
                            csFileContentsActual,
                            "Main file content mismatch");
        }

        [TestMethod("Case 3: delete existing handler method in the main file (when it is NOT EMPTY)")]
        public void TryDeleteEventHandlerInMainFile_HandlerIsNotEmpty()
        {
            // Line "private void TestForm1_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e) { ... }" (not empty!) is present in the main .cs file
            csFilePath = CreateMainCsFile(Path.Combine(rootDir, "Case.3"), inputCs);
            var trs = MyTypeResolutionService.CreateWithReferencedAssemblies(null, new DesignerCsFileContext());
            bool success = EventHelper.TryDeleteEventHandlerInMainFile(csFilePath, "TestForm1", frm, "KeyDown", "TestForm1_KeyDown", trs);
            // We expect that handler method TestForm1_KeyDown will be NOT removed from the main .cs file (because this method is NOT EMPTY).
            Assert.IsFalse(success);
            var csFileContentsActual = File.ReadAllText(csFilePath);
            var csFileContentsExpected = inputCs;
            Assert.AreEqual(csFileContentsExpected,
                            csFileContentsActual,
                            "Main file content mismatch");
        }

        private string CreateMainCsFile(string testDir, string fileContents)
        {
            Directory.CreateDirectory(testDir);
            var mainCsFilePath = Path.Combine(testDir, "Form1.cs");
            File.WriteAllText(mainCsFilePath, fileContents, CommonStuff.Utf8WithoutBom);
            return mainCsFilePath;
        }
    }
}
