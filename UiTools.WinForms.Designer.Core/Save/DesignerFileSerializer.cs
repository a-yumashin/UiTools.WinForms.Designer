using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.IO;
using System.Text;

namespace UiTools.WinForms.Designer.Core
{
    public class DesignerFileSerializer
    {
        public string GenerateCodeFromDesigner(IDesignerHost designerHost, CodeObjectsToPreserveWhenEditing objectsToPreserve, MyTypeResolutionService trs,
            bool removeUnnecessaryUsingsOnSave, Action<string> statusUpdater)
        {
            CodeTypeDeclaration type;
            var root = designerHost.RootComponent;
            var manager = new DesignerSerializationManager(designerHost);
            using (manager.CreateSession())
            {
                var serializer = (TypeCodeDomSerializer)manager.GetSerializer(root.GetType(), typeof(TypeCodeDomSerializer));
                statusUpdater?.Invoke("Serializing component to CodeDom");
                type = serializer.Serialize(manager, root, designerHost.Container.Components);
                type.IsPartial = true;
                // Post processing (code as CodeDom):
                statusUpdater?.Invoke("Post-processing CodeDom");
                DesignerFilePostProcessor.ProcessCodeDom(type, objectsToPreserve);
            }
            statusUpdater?.Invoke("Generating code from CodeDom");
            var csharpCode = CodeTypeDeclarationToString(type);
            // Post processing (code as string):
            statusUpdater?.Invoke("Post-processing generated code");
            DesignerFilePostProcessor.ProcessCodeString(ref csharpCode, objectsToPreserve, trs, removeUnnecessaryUsingsOnSave);
            return csharpCode;
        }

        private string CodeTypeDeclarationToString(CodeTypeDeclaration type)
        {
            var builder = new StringBuilder();
            CodeGeneratorOptions codeGeneratorOptions = new CodeGeneratorOptions
            {
                BracingStyle = "C",
                BlankLinesBetweenMembers = false,
                VerbatimOrder = true
            };
            using (var writer = new StringWriter(builder, CultureInfo.InvariantCulture))
            {
                using (var codeDomProvider = new CSharpCodeProvider())
                {
                    codeDomProvider.GenerateCodeFromType(type, writer, codeGeneratorOptions);
                }
                return builder.ToString();
            }
        }
    }
}
