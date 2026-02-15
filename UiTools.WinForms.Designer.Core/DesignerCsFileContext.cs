using System;
using System.ComponentModel;

namespace UiTools.WinForms.Designer.Core
{
    public class DesignerCsFileContext
    {
        public event EventHandler DesignerCsFileFullPathChanged;

        private string csProjectFileFullPath;
        private CsProjectFileWrapper csProjectFileWrapper;
        private string designerCsFileFullPath;

        public CsProjectFileWrapper CsProjectFileWrapper => csProjectFileWrapper;

        public string DesignerCsFileFullPath  // full path to .designer.cs file
        {
            get => designerCsFileFullPath;
            set
            {
                designerCsFileFullPath = value;
                DesignerCsFileFullPathChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        [DefaultValue("")]
        public string Namespace { get; set; } // namespace (used for new component only)

        // The following 4 optional parameters are used only in the BinDirectoryReferenceResolver.GetReferencedAssembliesFromProjectBin() method:
        [DefaultValue("")]
        public string CsProjectFileFullPath   // full path to the containing .csproj file
        {
            get => csProjectFileFullPath;
            set
            {
                csProjectFileFullPath = value;
                if (!string.IsNullOrEmpty(value))
                    csProjectFileWrapper = new CsProjectFileWrapper(value);
            }
        }

        [DefaultValue("")]
        public string Configuration { get; set; } = "Debug";    // configuration ("Debug", "Release" etc)
        
        [DefaultValue("")]
        public string Platform { get; set; } = "Any CPU";       // platform ("Any CPU", "x64" etc)
        
        [DefaultValue("")]
        public string ExtraAssembliesFileFullPath { get; set; } // full path to text file with extra assemblies to load (optional)

        public bool IsDesignerCsFileFullPathValid => CommonStuff.IsDesignerCsFilePathValid(DesignerCsFileFullPath);

        public string CalcMainCsFilePath() => CommonStuff.MainCsFilePathFromDesignerCsFilePath(DesignerCsFileFullPath);
        public string CalcResxFilePath() => CommonStuff.ResxFilePathFromDesignerCsFilePath(DesignerCsFileFullPath);
    }
}
