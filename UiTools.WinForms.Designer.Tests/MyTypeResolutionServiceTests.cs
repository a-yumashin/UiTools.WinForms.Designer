using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UiTools.WinForms.Designer.Core;

namespace UiTools.WinForms.Designer.Tests
{
    [TestClass]
    public class MyTypeResolutionServiceTests
    {
        /*
         * Folder structure:
         * %Temp%
         *   UiTools.WinForms.Designer.Tests // constant value from GlobalTestSetup.RootDirShortName; this folder is deleted before each run
         *     MyTypeResolutionServiceTests  // name of this class
         *       lotjgnxg.d0k                // unique for each run; gets generated with Path.GetRandomFileName(); [rootDir ends here]
         *         Case.1                    // [testDir ends here]
         *           TestProject.csproj
         *           bin
         *             Debug
         *               net48               // [dllDir ends here]; the whole folder chain gets created with Directory.CreateDirectory(dllDir)
         *                 TestLib.1.dll
         */
        private string rootDir;

        [TestInitialize]
        public void Setup()
        {
            rootDir = Path.Combine(Path.GetTempPath(), GlobalTestSetup.RootDirShortName, nameof(MyTypeResolutionServiceTests), Path.GetRandomFileName());
        }

        [TestMethod("Case 1: MyTypeResolutionService rejects null passed as a type name")]
        public void TRS_Rejects_Null_Type_Name()
        {
            var trs = new MyTypeResolutionService(null, null);
            Assert.ThrowsException<ArgumentNullException>(() => trs.GetType(null, throwOnError: true));
        }

        [TestMethod("Case 2: MyTypeResolutionService resolves a type from the 'bin' folder (no namespace)")]
        public void TRS_Resolves_Type_With_No_Namespace_From_Bin_Folder()
        {
            var testDir = Path.Combine(rootDir, "Case.2");
            var dllDir = Path.Combine(testDir, "bin", "Debug", "net48");
            Directory.CreateDirectory(dllDir);
            CreateTestAssembly(Path.Combine(dllDir, "TestLib.2.dll"),
                "public class MyType { }");

            var projectFilePath = CreateProjectFile(testDir);
            var dfContext = new DesignerCsFileContext { CsProjectFileFullPath = projectFilePath, Configuration = "Debug", Platform = "Any CPU" };

            var trs = MyTypeResolutionService.CreateWithReferencedAssemblies(null, dfContext);
            var type = trs.GetType("MyType");

            Assert.IsNotNull(type, "Type 'MyType' should be resolved");
            Assert.AreEqual("MyType", type.FullName);
        }

        [TestMethod("Case 3: MyTypeResolutionService resolves a type from the 'bin' folder (with namespace)")]
        public void TRS_Resolves_Type_With_Namespace_From_Bin_Folder()
        {
            var testDir = Path.Combine(rootDir, "Case.3");
            var dllDir = Path.Combine(testDir, "bin", "Debug", "net48");
            Directory.CreateDirectory(dllDir);
            CreateTestAssembly(Path.Combine(dllDir, "TestLib.3.dll"),
                "namespace MyNamespace { public class MyType { } }");

            var projectFilePath = CreateProjectFile(testDir);
            var dfContext = new DesignerCsFileContext { CsProjectFileFullPath = projectFilePath, Configuration = "Debug", Platform = "Any CPU" };

            var trs = MyTypeResolutionService.CreateWithReferencedAssemblies(null, dfContext);
            var type = trs.GetType("MyNamespace.MyType");

            Assert.IsNotNull(type, "Type 'MyNamespace.MyType' should be resolved");
            Assert.AreEqual("MyNamespace.MyType", type.FullName);
        }

        [TestMethod("Case 4: MyTypeResolutionService resolves a type from the 'bin' folder (current namespace)")]
        public void TRS_Resolves_Type_From_Current_Namespace_From_Bin_Folder()
        {
            var testDir = Path.Combine(rootDir, "Case.4");
            var dllDir = Path.Combine(testDir, "bin", "Debug", "net48");
            Directory.CreateDirectory(dllDir);
            CreateTestAssembly(Path.Combine(dllDir, "TestLib.4.dll"),
                "namespace MyNamespace { public class MyType { } }");

            var projectFilePath = CreateProjectFile(testDir);
            var dfContext = new DesignerCsFileContext { CsProjectFileFullPath = projectFilePath, Configuration = "Debug", Platform = "Any CPU",
                Namespace = "MyNamespace" };

            var trs = MyTypeResolutionService.CreateWithReferencedAssemblies(null, dfContext);
            var type = trs.GetType("MyType"); // name is short - but DesignerCsFileContext.Namespace will help to resolve

            Assert.IsNotNull(type, "Type 'MyNamespace.MyType' should be resolved");
            Assert.AreEqual("MyNamespace.MyType", type.FullName);
        }

        [TestMethod("Case 5: MyTypeResolutionService returns the expected list of known assemblies")]
        public void TRS_Returns_Expected_Assemblies()
        {
            var testDir = Path.Combine(rootDir, "Case.5");
            var dllDir = Path.Combine(testDir, "bin", "Debug", "net48");
            Directory.CreateDirectory(dllDir);
            CreateTestAssembly(Path.Combine(dllDir, "TestLib.5.dll"),
                "namespace MyNamespace { public class MyType { } }");

            var projectFilePath = CreateProjectFile(testDir);
            var dfContext = new DesignerCsFileContext { CsProjectFileFullPath = projectFilePath, Configuration = "Debug", Platform = "Any CPU" };

            var trs = MyTypeResolutionService.CreateWithReferencedAssemblies(null, dfContext);
            var knownAssembliesNames = trs.GetKnownAssemblyNames().Select(an => an.Name).ToList();
            var expectedAssembliesNames = new List<string> { "mscorlib", "System.Drawing", "System.Windows.Forms", "System", "System.Design", "TestLib.5" };

            CollectionAssert.AreEquivalent(expectedAssembliesNames, knownAssembliesNames, "List of known assemblies should match the expected one ");
        }

        [TestMethod("Case 6: MyTypeResolutionService resolves a GAC type using provided 'using' directives")]
        public void TRS_With_Usings_Resolves_Type_Short_Name()
        {
            var usings = new List<string> { "System.Windows.Forms" };
            var trs = MyTypeResolutionService.CreateWithReferencedAssemblies(usings, new DesignerCsFileContext());
            var type = trs.GetType("ComboBox"); // name is short - but usings will help to resolve

            Assert.IsNotNull(type, "Type 'System.Windows.Forms.ComboBox' should be resolved");
            Assert.AreEqual("System.Windows.Forms.ComboBox", type.FullName);
        }

        [TestMethod("Case 7: MyTypeResolutionService resolves a simple type")]
        public void TRS_Resolves_Simple_Type()
        {
            var trs = MyTypeResolutionService.CreateWithReferencedAssemblies(null, new DesignerCsFileContext());
            var type = trs.GetType("int");

            Assert.IsNotNull(type, "Type 'System.Int32' should be resolved");
            Assert.AreEqual("System.Int32", type.FullName);
        }

        [TestMethod("Case 8: MyTypeResolutionService correctly handles the global namespace alias qualifier ('global::')")]
        public void TRS_Correctly_Handles_Global_Alias()
        {
            var trs = MyTypeResolutionService.CreateWithReferencedAssemblies(null, new DesignerCsFileContext());
            var type = trs.GetType("global::System.String");

            Assert.IsNotNull(type, "Type 'System.String' should be resolved");
            Assert.AreEqual("System.String", type.FullName);
        }

        [TestMethod("Case 9: MyTypeResolutionService resolves a type specified with a 'full AQN'")]
        public void TRS_Resolve_Type_From_Full_AQN()
        {
            var trs = MyTypeResolutionService.CreateWithReferencedAssemblies(null, new DesignerCsFileContext());
            var type = trs.GetType("System.Windows.Forms.Button, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");

            Assert.IsNotNull(type, "Type 'System.Windows.Forms.Button' should be resolved");
            Assert.AreEqual("System.Windows.Forms.Button", type.FullName);
        }

        [TestMethod("Case 10: MyTypeResolutionService resolves a type specified with a 'partial AQN'")]
        public void TRS_Resolve_Type_From_Partial_AQN()
        {
            var trs = MyTypeResolutionService.CreateWithReferencedAssemblies(null, new DesignerCsFileContext());
            var type = trs.GetType("System.Windows.Forms.Design.ControlDesigner, System.Design"); // e.g. the DesignerAttribute can be used with such type name

            Assert.IsNotNull(type, "Type 'System.Windows.Forms.Design.ControlDesigner' should be resolved");
            Assert.AreEqual("System.Windows.Forms.Design.ControlDesigner", type.FullName);
        }

        [TestMethod("Case 11: MyTypeResolutionService resolves a nested type")]
        public void TRS_Resolve_Nested_Type()
        {
            var testDir = Path.Combine(rootDir, "Case.11");
            var dllDir = Path.Combine(testDir, "bin", "Debug", "net48");
            Directory.CreateDirectory(dllDir);
            CreateTestAssembly(Path.Combine(dllDir, "TestLib.11.dll"),
                "namespace MyNamespace { public class MyType { public enum MyEnum { First, Second } } }");

            var projectFilePath = CreateProjectFile(testDir);
            var dfContext = new DesignerCsFileContext { CsProjectFileFullPath = projectFilePath, Configuration = "Debug", Platform = "Any CPU" };

            var trs = MyTypeResolutionService.CreateWithReferencedAssemblies(null, dfContext);
            var type = trs.GetType("MyNamespace.MyType.MyEnum");

            Assert.IsNotNull(type, "Type 'MyNamespace.MyType+MyEnum' should be resolved");
            Assert.AreEqual("MyNamespace.MyType+MyEnum", type.FullName);
        }

        [TestMethod("Case 12: MyTypeResolutionService resolves a type from an assembly specified in the 'extra assemblies' file")]
        public void TRS_Resolve_Type_With_Extra_Assemblies_File()
        {
            var testDir = Path.Combine(rootDir, "Case.12");
            var dllDir = Path.Combine(testDir, "ExtraAssemblies");
            Directory.CreateDirectory(dllDir);
            var dllPath = Path.Combine(dllDir, "TestLib.12.dll");
            CreateTestAssembly(dllPath,
                "namespace MyNamespace { public class MyType { } }");

            var xtraAsmFilePath = Path.Combine(testDir, "xtraAsmList.txt");
            File.WriteAllText(xtraAsmFilePath, $"// Some comment\n{dllPath}\n");
            var dfContext = new DesignerCsFileContext { ExtraAssembliesFileFullPath = xtraAsmFilePath };

            var trs = MyTypeResolutionService.CreateWithReferencedAssemblies(null, dfContext);
            var type = trs.GetType("MyNamespace.MyType");

            Assert.IsNotNull(type, "Type 'MyNamespace.MyType' should be resolved");
            Assert.AreEqual("MyNamespace.MyType", type.FullName);
        }

        #region Helpers

        private string CreateProjectFile(string testDir)
        {
            var projectFilePath = Path.Combine(testDir, "TestProject.csproj");
            File.WriteAllText(projectFilePath,
                @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net48</TargetFramework>
  </PropertyGroup>
</Project>");
            return projectFilePath;
        }

        private void CreateTestAssembly(string outputPath, string sourceCode)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

            var references = new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location), "System.Runtime.dll"))
            };

            var compilation = CSharpCompilation.Create(
                Path.GetFileNameWithoutExtension(outputPath),
                new[] { syntaxTree },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (var stream = new FileStream(outputPath, FileMode.Create))
            {
                var result = compilation.Emit(stream);
                if (!result.Success)
                {
                    var failures = string.Join("\n", result.Diagnostics.Select(d => d.ToString()));
                    throw new Exception("Compilation failed:\n" + failures);
                }
            }
        }

        #endregion Helpers
    }
}
