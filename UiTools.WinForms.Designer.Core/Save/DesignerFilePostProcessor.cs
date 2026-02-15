using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using static UiTools.WinForms.Designer.Core.CommonStuff;

namespace UiTools.WinForms.Designer.Core
{
    internal static class DesignerFilePostProcessor
    {
        public static void ProcessCodeDom(CodeTypeDeclaration type, CodeObjectsToPreserveWhenEditing objectsToPreserve)
        {
            ThrowIfNullOrEmpty(type);

            RemoveConstructor(type.Members); // removes constructor (TypeCodeDomSerializer creates it always, but usually it lives in the 2nd part of this partial class)
            RestoreOrCreateCertainMethods(objectsToPreserve, type.Members);
            InsertAdditionalBlankLines(type.Members); // inserts blank lines between methods
            MoveFieldsDeclarationsToTheBottom(type.Members);
            AddComponentsFieldIfMissing(type.Members); // adds private field System.ComponentModel.IContainer components
            AddPerformLayoutMethodCallIfNeeded(type.Members); // adds PerformLayout() call if needed
            RestoreDocCommentsAndRegions(type.Members);
        }

        public static void ProcessCodeString(ref string csharpCode, CodeObjectsToPreserveWhenEditing objectsToPreserve, MyTypeResolutionService trs,
            bool removeUnnecessaryUsingsOnSave)
        {
            if (string.IsNullOrWhiteSpace(csharpCode))
                throw new ArgumentException($"'{nameof(csharpCode)}' cannot be null or whitespace.", nameof(csharpCode));

            FixFormattingOfDisposeMethod(ref csharpCode);
            if (objectsToPreserve != null)
            {
                RestoreNamespace(objectsToPreserve.Namespace, ref csharpCode);
                RestoreUsings(objectsToPreserve, ref csharpCode);
            }
            if (removeUnnecessaryUsingsOnSave)
                RemoveUnnecessaryUsings(ref csharpCode, trs);
        }

        private static void MoveFieldsDeclarationsToTheBottom(CodeTypeMemberCollection members)
        {
            var fieldsToMove = new List<CodeMemberField>();

            for (int i = members.Count - 1; i >= 0; i--)
            {
                if (members[i] is CodeMemberField field && field.Name != "components")
                {
                    fieldsToMove.Insert(0, field);
                    members.RemoveAt(i);
                }
            }

            foreach (var field in fieldsToMove)
                members.Add(field);
        }

        private static void RemoveUnnecessaryUsings(ref string csharpCode, MyTypeResolutionService trs)
        {
            var metadataReferences = new List<MetadataReference>();
            foreach (var asmName in trs.GetKnownAssemblyNames())
            {
                var assembly = trs.GetAssembly(asmName);
                if (assembly != null && !string.IsNullOrWhiteSpace(assembly.Location))
                    metadataReferences.Add(MetadataReference.CreateFromFile(assembly.Location));
            }

            var optimizationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithConcurrentBuild(true)
                .WithOptimizationLevel(OptimizationLevel.Release)
                .WithMetadataImportOptions(MetadataImportOptions.Public)
                .WithGeneralDiagnosticOption(ReportDiagnostic.Suppress) // suppress all diagnostics by default
                .WithSpecificDiagnosticOptions(new[] { new KeyValuePair<string, ReportDiagnostic>("CS8019", ReportDiagnostic.Warn) }); // enable only CS8019

            var syntaxTree = CSharpSyntaxTree.ParseText(csharpCode);
            var compilation = CSharpCompilation.Create("Cleanup",
                new[] { syntaxTree }, metadataReferences, optimizationOptions);

            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var diagnostic = semanticModel.GetDiagnostics();
            var usingsToRemove = diagnostic
                .Where(d => d.Id == "CS8019")
                .Select(d => syntaxTree.GetRoot().FindNode(d.Location.SourceSpan))
                .OfType<UsingDirectiveSyntax>()
                .ToList();

            SyntaxNode root = usingsToRemove.Count > 0
                ? syntaxTree.GetRoot().RemoveNodes(usingsToRemove, SyntaxRemoveOptions.KeepNoTrivia) // remove usings, along with line breaks
                : syntaxTree.GetRoot();
            SyntaxNode formattedRoot = Formatter.Format(root, new AdhocWorkspace()); // also format code (why not?)
            csharpCode = formattedRoot.ToFullString();

            // Empty lines might still remain at the beginning - remove them:
            csharpCode = TrimLeadingEmptyLines(csharpCode);
        }

        private static string TrimLeadingEmptyLines(string code)
        {
            if (string.IsNullOrEmpty(code))
                return code;
            int i = 0;
            int lastLineBreak = 0;
            while (i < code.Length)
            {
                char c = code[i];
                if (c == '\n' || c == '\r')
                    lastLineBreak = i + 1;
                else if (!char.IsWhiteSpace(c))
                    return code.Substring(lastLineBreak);
                i++;
            }
            return "";
        }

        private static readonly Regex redundantCarriageReturnsRegex = new Regex(@"(disposing)\s+(&& \((?:this\.)?components[ ]+!=[ ]+null\))", RegexOptions.Multiline | RegexOptions.Compiled);
        private static readonly Regex redundantParenthesesRegex = new Regex(@"\((\(disposing[ ]+&&[ ]+\(this\.components[ ]+!=[ ]+null\)\))\)", RegexOptions.Multiline | RegexOptions.Compiled);
        private static void FixFormattingOfDisposeMethod(ref string csharpCode)
        {
            /*
             * Sometimes it may look like this:
             * 
                if ((disposing 
                            && (this.components != null)))
                {
                    this.components.Dispose();
                }
             */
            var matches = redundantCarriageReturnsRegex.Matches(csharpCode);
            if (matches.Count > 0)
                csharpCode = redundantCarriageReturnsRegex.Replace(csharpCode, "$1 $2");
            matches = redundantParenthesesRegex.Matches(csharpCode);
            if (matches.Count > 0)
                csharpCode = redundantParenthesesRegex.Replace(csharpCode, "$1");
        }

        private static void RestoreNamespace(string ns, ref string csharpCode)
        {
            if (ns != null)
            {
                var indentString = new string(' ', 4);
                IndentAllLines(indentString, ref csharpCode);
                csharpCode = $"namespace {ns}\n{{\n{csharpCode}";
                if (!csharpCode.EndsWith("\n"))
                    csharpCode += "\n";
                csharpCode += "}";
            }
        }

        private static void IndentAllLines(string indentString, ref string csharpCode)
        {
            if (!csharpCode.StartsWith("\n"))
                csharpCode = indentString + csharpCode;
            csharpCode = csharpCode.Replace("\n", $"\n{indentString}");
            // Last \n should not be replaced:
            if (csharpCode.EndsWith($"\n{indentString}"))
                csharpCode = csharpCode.Substring(0, csharpCode.Length - 1 - indentString.Length);
        }

        private static Regex nsExtractorRegex = new Regex(@"^using (?<ns>[^;]+);$", RegexOptions.Multiline | RegexOptions.Compiled);
        private static void RestoreUsings(CodeObjectsToPreserveWhenEditing objectsToPreserve, ref string csharpCode)
        {
            if (objectsToPreserve?.UsingNamespaces != null)
            {
                // On the one hand, restoring previous usings is meaningless, as they could have changed during form editing.
                // On the other hand, among them there might be those necessary for methods other than InitializeComponent() and Dispose() (if, of course, such methods
                // are present; this is an uncommon occurrence, as the .designer.cs file is usually preferred not to be touched). In particular, if the Dispose()
                // method is marked with the [DebuggerNonUserCode()] attribute, then "using System.Diagnostics;" will definitely be present, and we will restore it here.
                var existingNamespaces = new List<string>();
                var matches = nsExtractorRegex.Matches(csharpCode);
                if (matches.Count > 0)
                    existingNamespaces = matches.Cast<Match>().Select(m => m.Groups["ns"].Value).ToList();
                var namespacesToAdd = objectsToPreserve.UsingNamespaces.Except(existingNamespaces).ToList();
                if (namespacesToAdd.Any())
                {
                    if (!csharpCode.StartsWith(Environment.NewLine))
                        csharpCode = Environment.NewLine + csharpCode; // we need blank line between usings and class declaration line
                    namespacesToAdd.Sort();
                    foreach (var ns in namespacesToAdd)
                    {
                        csharpCode = csharpCode.Insert(0, "using " + ns + ";" + Environment.NewLine);
                    }
                }
            }
        }

        /// <summary>
        /// Adds the 'components' field and its initialization to the InitializeComponent() method.
        /// </summary>
        private static void AddComponentsFieldIfMissing(CodeTypeMemberCollection members)
        {
            // Check if the 'components' field exists
            bool hasComponentsField = members.OfType<CodeMemberField>()
                .Any(field => field.Name == "components" && field.Type.BaseType == typeof(IContainer).FullName);

            if (!hasComponentsField)
            {
                // Add line 'private System.ComponentModel.IContainer components = null;'
                CodeMemberField componentsField = new CodeMemberField(typeof(IContainer), "components")
                {
                    Attributes = MemberAttributes.Private,
                    InitExpression = new CodePrimitiveExpression(null)
                };
                members.Insert(0, componentsField);
            }

            // Initialize components inside the InitializeComponent() method (line 'this.components = new System.ComponentModel.Container();')
            CodeMemberMethod initializeComponentMethod = FindInitializeComponent(members);
            if (initializeComponentMethod != null)
            {
                // Check that initialization was not added previously
                bool hasContainerInit = initializeComponentMethod.Statements.OfType<CodeAssignStatement>()
                    .Any(assign =>
                        assign.Left is CodeFieldReferenceExpression fieldRef &&
                        fieldRef.FieldName == "components" &&
                        fieldRef.TargetObject is CodeThisReferenceExpression
                    );

                if (!hasContainerInit)
                {
                    // Add initialization at the beginning of the InitializeComponent() method
                    initializeComponentMethod.Statements.Insert(0,
                        new CodeAssignStatement(
                            new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "components"),
                            new CodeObjectCreateExpression(typeof(Container))
                        )
                    );
                }
            }
        }

        private static void AddPerformLayoutMethodCallIfNeeded(CodeTypeMemberCollection members)
        {
            var initMethod = members.OfType<CodeMemberMethod>().FirstOrDefault(m => m.Name == "InitializeComponent");
            if (initMethod != null && initMethod.Statements != null)
            {
                // Check if 'this.PerformLayout();' line is already present:
                bool hasPerformLayout = initMethod.Statements.OfType<CodeExpressionStatement>()
                    .Any(s => s.Expression is CodeMethodInvokeExpression invoke &&
                              invoke.Method.MethodName == "PerformLayout");

                // Index of the last ResumeLayout(false) call:
                int resumeLayoutIndexWithFalse = -1;
                for (int i = 0; i < initMethod.Statements.Count; i++)
                {
                    if (initMethod.Statements[i] is CodeExpressionStatement stmt &&
                        stmt.Expression is CodeMethodInvokeExpression invoke)
                    {
                        if (invoke.Method.MethodName == "ResumeLayout")
                        {
                            // Check that there's exactly one argument, that it's a primitive expresssion and its value is boolean false:
                            if (invoke.Parameters.Count == 1 &&
                                invoke.Parameters[0] is CodePrimitiveExpression primitiveArg &&
                                primitiveArg.Value is bool boolValue &&
                                boolValue == false)
                            {
                                resumeLayoutIndexWithFalse = i;
                            }
                        }
                    }
                }

                // If PerformLayout is missing and ResumeLayout(false) is present - add PerformLayout() right after ResumeLayout(false):
                if (!hasPerformLayout && resumeLayoutIndexWithFalse >= 0)
                {
                    initMethod.Statements.Insert(resumeLayoutIndexWithFalse + 1,
                        new CodeExpressionStatement(
                            new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "PerformLayout")));
                }
            }
        }

        private static void RestoreDocCommentsAndRegions(CodeTypeMemberCollection members)
        {
            // Doc Comment "Required designer variable." for line "private System.ComponentModel.IContainer components = null;"
            var componentsField = members.OfType<CodeMemberField>().FirstOrDefault(f => f.Name == "components");
            if (componentsField != null)
            {
                componentsField.Comments.Clear();
                componentsField.Comments.Add(new CodeCommentStatement("<summary>", true));
                componentsField.Comments.Add(new CodeCommentStatement("Required designer variable.", true));
                componentsField.Comments.Add(new CodeCommentStatement("</summary>", true));
            }

            // Doc Comment "Clean up any resources being used." (and <param name="disposing">...) for the Dispose() method
            var disposeMethod = members.OfType<CodeMemberMethod>().FirstOrDefault(m => m.Name == "Dispose");
            if (disposeMethod != null)
            {
                disposeMethod.Comments.Clear();
                disposeMethod.Comments.Add(new CodeCommentStatement("<summary>", true));
                disposeMethod.Comments.Add(new CodeCommentStatement("Clean up any resources being used.", true));
                disposeMethod.Comments.Add(new CodeCommentStatement("</summary>", true));
                disposeMethod.Comments.Add(new CodeCommentStatement("<param name=\"disposing\">true if managed resources should be disposed; otherwise, false.</param>", true));
            }

            // Doc Comment "Required method for Designer support..." and Region for the InitializeComponent() method
            var initMethod = members.OfType<CodeMemberMethod>().FirstOrDefault(m => m.Name == "InitializeComponent");
            if (initMethod != null)
            {
                int index = members.IndexOf(initMethod);

                members.Insert(index + 1, new CodeSnippetTypeMember(string.Empty));
                members.Insert(index + 2, new CodeSnippetTypeMember("#endregion\n"));

                initMethod.Comments.Clear();
                initMethod.Comments.Add(new CodeCommentStatement("<summary>", true));
                initMethod.Comments.Add(new CodeCommentStatement("Required method for Designer support - do not modify", true));
                initMethod.Comments.Add(new CodeCommentStatement("the contents of this method with the code editor.", true));
                initMethod.Comments.Add(new CodeCommentStatement("</summary>", true));

                members.Insert(index, new CodeSnippetTypeMember(string.Empty));
                members.Insert(index, new CodeSnippetTypeMember("#region Windows Form Designer generated code"));
            }
        }

        private static void RemoveConstructor(CodeTypeMemberCollection members)
        {
            var ctorMember = members.OfType<CodeConstructor>().FirstOrDefault();
            if (ctorMember != null)
                members.Remove(ctorMember);
        }

        private static CodeMemberMethod FindInitializeComponent(CodeTypeMemberCollection members)
        {
            return members.OfType<CodeMemberMethod>().FirstOrDefault(method => method.Name == "InitializeComponent");
        }

        /// <summary>
        /// Ensures the Dispose method is present by either restoring its original implementation or generating a new one.
        /// Also preserves any other methods found in the source code during the loading process.
        /// </summary>
        /// <param name="objectsToPreserve">Contains methods found in the source code during the loading process (including the Dispose() method).</param>
        private static void RestoreOrCreateCertainMethods(CodeObjectsToPreserveWhenEditing objectsToPreserve, CodeTypeMemberCollection members)
        {
            if (objectsToPreserve == null)
                return;
            bool hasDispose = members.OfType<CodeMemberMethod>()
                .Any(method =>
                    method.Name == "Dispose" &&
                    method.Parameters.Count == 1 &&
                    method.Parameters[0].Type.BaseType == typeof(bool).FullName
                );
            if (!hasDispose)
            {
                // Dispose() method is missing
                CodeMemberMethod disposeMethod;
                if (objectsToPreserve.DisposeMethod == null)
                {
                    // Create it
                    disposeMethod = new CodeMemberMethod
                    {
                        Name = "Dispose",
                        Attributes = MemberAttributes.Family | MemberAttributes.Override, // protected override
                        ReturnType = new CodeTypeReference(typeof(void))
                    };
                    disposeMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(bool), "disposing"));

                    // "if (disposing && (components != null))"
                    CodeConditionStatement ifDispose = new CodeConditionStatement(
                        new CodeBinaryOperatorExpression(
                            new CodeArgumentReferenceExpression("disposing"),
                            CodeBinaryOperatorType.BooleanAnd,
                            new CodeBinaryOperatorExpression(
                                new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "components"),
                                CodeBinaryOperatorType.IdentityInequality,
                                new CodePrimitiveExpression(null)
                            )
                        ),
                        // then: "components.Dispose();"
                        new CodeStatement[] {
                        new CodeExpressionStatement(new CodeMethodInvokeExpression(
                            new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "components"),
                            "Dispose"
                        ))
                        }
                    );
                    disposeMethod.Statements.Add(ifDispose);

                    // "base.Dispose(disposing);"
                    disposeMethod.Statements.Add(
                        new CodeExpressionStatement(new CodeMethodInvokeExpression(
                            new CodeBaseReferenceExpression(),
                            "Dispose",
                            new CodeArgumentReferenceExpression("disposing")
                        ))
                    );

                }
                else
                {
                    // Restore its original implementation
                    disposeMethod = objectsToPreserve.DisposeMethod;
                }
                var initCompMemberIndex = members.Cast<CodeTypeMember>().ToList()
                    .FindIndex(m => m is CodeMemberMethod && m.Name == "InitializeComponent");
                if (initCompMemberIndex == -1)
                    members.Add(disposeMethod);
                else
                    members.Insert(initCompMemberIndex, disposeMethod); // place it before the InitializeComponent() method, as VS does
            }
            // Restore other methods (if any):
            if (objectsToPreserve.OtherMethods != null)
                members.AddRange(objectsToPreserve.OtherMethods.ToArray());
        }

        private static void InsertAdditionalBlankLines(CodeTypeMemberCollection members)
        {
            for (int i = members.Count - 1; i > 0; i--)
            {
                if (members[i] is CodeMemberMethod)
                    members.Insert(i, new CodeSnippetTypeMember());
            }
        }
    }
}
