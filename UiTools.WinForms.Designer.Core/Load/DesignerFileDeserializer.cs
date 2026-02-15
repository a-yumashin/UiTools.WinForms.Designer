using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace UiTools.WinForms.Designer.Core
{
    public class DesignerFileDeserializer
    {
        private BinDirectoryReferenceResolver binDirectoryReferenceResolver = new BinDirectoryReferenceResolver();

        public IComponent CreateDesignerComponentFromCode(
            IDesignerHost designerHost,
            DesignerCsFileContext dfContext,
            CodeObjectsToPreserveWhenEditing objectsToPreserve,
            out Font rootComponentFontFromCode)
        {
            IComponent rootComponent;
            DesignerSerializationManager manager = null;
            try
            {
                var dpf = new DesignerFilePreProcessor();
                string designerCsCode = dpf.ReadDesignerFileAndFixClassDeclarationLine(dfContext.DesignerCsFileFullPath);
                CompilationUnitSyntax cu = DesignerRoslynToCodeDomConverter.ParseDesignerFile(designerCsCode);
                DesignerRoslynToCodeDomConverter.ConversionResult conversionResult =
                    DesignerRoslynToCodeDomConverter.ConvertCompilationUnitToCodeTypeDeclarationWithUsings(
                        cu, dfContext.DesignerCsFileFullPath, dfContext.CsProjectFileWrapper);
                CodeTypeDeclaration codeDomType = conversionResult.TypeDeclaration;
                List<string> usings = conversionResult.Usings;

                objectsToPreserve.DisposeMethod = codeDomType.Members.OfType<CodeMemberMethod>().FirstOrDefault(m => m.Name == "Dispose");
                objectsToPreserve.OtherMethods = codeDomType.Members.OfType<CodeMemberMethod>().Where(m => m.Name != "Dispose" && m.Name != "InitializeComponent").ToList();
                objectsToPreserve.UsingNamespaces = usings;
                objectsToPreserve.Namespace = conversionResult.Namespace;
                rootComponentFontFromCode = ExtractRootComponentFont(codeDomType);

                dfContext.Namespace = conversionResult.Namespace; // namespace is known only after conversion is performed
                var trs = MyTypeResolutionService.CreateWithReferencedAssemblies(usings, dfContext);

                string resxFilePath = dfContext.CalcResxFilePath();
                var resService = new MyResourceService(resxFilePath);
                designerHost.RemoveService(typeof(IResourceService));
                designerHost.AddService(typeof(IResourceService), resService);

                designerHost.RemoveService(typeof(ITypeResolutionService), true);
                designerHost.AddService(typeof(ITypeResolutionService), trs, true);
                manager = new DesignerSerializationManager(designerHost);
                using (manager.CreateSession())
                {
                    // Register a custom provider to override the default ResourceCodeDomSerializer:
                    var resourceProvider = new MyResourceSerializationProvider(resxFilePath);
                    ((IDesignerSerializationManager)manager).AddSerializationProvider(resourceProvider);

                    var serializer = (TypeCodeDomSerializer)manager.GetSerializer(typeof(Form), typeof(TypeCodeDomSerializer));
                    rootComponent = (IComponent)serializer.Deserialize(manager, codeDomType);
                    if (manager.Errors.Count > 0)
                    {
                        MessageLogger.LogError(this,
                            $"{nameof(DesignerSerializationManager)} produced the following error(s) while deserializing the supplied CodeTypeDeclaration instance:");
                        foreach (var error in manager.Errors)
                        {
                            var ex = error as Exception;
                            MessageLogger.LogError(this, ex == null ? "  " + error.ToString() : "  " + ex.Message, ex);
                        }
                    }
                }
                return rootComponent;
            }
            catch (Exception ex)
            {
                MessageLogger.LogError(this, ex.Message, ex);
                rootComponentFontFromCode = null;
                return null;
            }
        }

        private Font ExtractRootComponentFont(CodeTypeDeclaration codeDomType)
        {
            var initializeComponentMethod = codeDomType.Members.OfType<CodeMemberMethod>().FirstOrDefault(m => m.Name == "InitializeComponent");
            if (initializeComponentMethod == null)
            {
                MessageLogger.LogError(this, "InitializeComponent() method not found");
                return null;
            }
            var statement = initializeComponentMethod.Statements.OfType<CodeAssignStatement>()
                .FirstOrDefault(st => st.Left is CodePropertyReferenceExpression cpre &&
                                      cpre.PropertyName.StartsWith("Font") &&
                                      cpre.TargetObject is CodeThisReferenceExpression &&
                                      st.Right is CodeObjectCreateExpression exp &&
                                      exp.CreateType is CodeTypeReference typeRef &&
                                      typeRef.BaseType.In("System.Drawing.Font", "Font"));
            if (statement == null)
                return null;
            var typeRefParams = (statement.Right as CodeObjectCreateExpression).Parameters;
            var fontName = (typeRefParams[0] as CodePrimitiveExpression).Value.ToString();
            float fontSize;
            if (!float.TryParse((typeRefParams[1] as CodePrimitiveExpression).Value.ToString(), out fontSize))
                return null;
            return new Font(fontName, fontSize);
        }
    }

    public class CodeObjectsToPreserveWhenEditing
    {
        public CodeMemberMethod DisposeMethod { get; set; }
        public List<CodeMemberMethod> OtherMethods { get; set; } // methods other than InitializeComponent and Dispose (e.g. user-defined methods)
        public List<string> UsingNamespaces { get; set; } // 'using' directives
        public string Namespace { get; set; } // the namespace of the class being processed, e.g. "WindowsFormsApp1"
    }
}
