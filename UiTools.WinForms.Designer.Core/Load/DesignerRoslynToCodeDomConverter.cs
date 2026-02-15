using System;
using System.Linq;
using System.Collections.Generic;
using System.CodeDom;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.IO;

namespace UiTools.WinForms.Designer.Core
{
    /// <summary>
    /// Converts Roslyn syntax tree from a .designer.cs file into a CodeDom structure.
    /// </summary>
    internal static class DesignerRoslynToCodeDomConverter
    {
        /// <summary>
        /// Parses the source code of a designer file (.designer.cs) into a Roslyn CompilationUnitSyntax.
        /// </summary>
        /// <param name="source">The source code of the designer file.</param>
        /// <returns>The root node of the Roslyn syntax tree.</returns>
        public static CompilationUnitSyntax ParseDesignerFile(string source)
        {
            var tree = CSharpSyntaxTree.ParseText(source);
            return tree.GetCompilationUnitRoot();
        }

        /// <summary>
        /// Converts a Roslyn CompilationUnitSyntax structure into a CodeDom CodeTypeDeclaration structure and extracts 'using' directives and namespace.
        /// </summary>
        /// <param name="cu">The Roslyn CompilationUnitSyntax.</param>
        /// <param name="designerFilePath">The full path to the .designer.cs file being parsed. Used to determine the path to the main .cs file.</param>
        /// <param name="csProjFileWrapper">Wrapper for the .csproj file. Used to locate the generated .GlobalUsings.g.cs file ().</param>
        /// <returns>The conversion result, containing the CodeTypeDeclaration, list of 'using' directives and namespace.</returns>
        public static ConversionResult ConvertCompilationUnitToCodeTypeDeclarationWithUsings(CompilationUnitSyntax cu, string designerFilePath, CsProjectFileWrapper csProjFileWrapper)
        {
            var classNode = cu.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            if (classNode == null)
                throw new InvalidOperationException("No class declaration found in the provided source.");

            // Support both explicit and implicit usings:
            var usings = ExtractUsings(cu, csProjFileWrapper);

            // Support both classic (with braces) and modern (file-scoped) namespaces:
            var namespaceNode = cu.DescendantNodes().OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault();
            var @namespace = namespaceNode?.Name.ToString();

            // First, collect all class field names (to correctly distinguish between a field/type by name)
            var classFieldNames = new HashSet<string>(StringComparer.Ordinal);
            foreach (var fd in classNode.Members.OfType<FieldDeclarationSyntax>())
                foreach (var v in fd.Declaration.Variables)
                    classFieldNames.Add(v.Identifier.Text);

            var classIdentifier = classNode.Identifier.Text; // class name, e.g. "Form1"

            var codeType = new CodeTypeDeclaration(classIdentifier)
            {
                IsClass = true,
                IsPartial = classNode.Modifiers.Any(SyntaxKind.PartialKeyword)
            };

            if (classNode.Modifiers.Any(SyntaxKind.PublicKeyword))
                codeType.Attributes = MemberAttributes.Public;
            else if (classNode.Modifiers.Any(SyntaxKind.InternalKeyword) || classNode.Modifiers.Any(SyntaxKind.PrivateKeyword) == false)
                codeType.Attributes = MemberAttributes.Assembly;

            foreach (var baseType in classNode.BaseList?.Types ?? Enumerable.Empty<BaseTypeSyntax>())
                codeType.BaseTypes.Add(TypeSyntaxToTypeReference(baseType.Type));

            foreach (var attrList in classNode.AttributeLists)
                foreach (var attr in attrList.Attributes)
                    codeType.CustomAttributes.Add(AttributeSyntaxToCodeAttribute(attr));

            // Fields (private components created by the designer)
            foreach (var field in classNode.Members.OfType<FieldDeclarationSyntax>())
            {
                foreach (var variable in field.Declaration.Variables)
                {
                    var fld = new CodeMemberField(TypeSyntaxToTypeReference(field.Declaration.Type), variable.Identifier.Text);
                    fld.Attributes = FieldModifiersToMemberAttributes(field.Modifiers);
                    if (variable.Initializer != null)
                        // Pass the full context, including CodeTypeDeclaration, file path and class name
                        fld.InitExpression = ExpressionSyntaxToCodeExpression(variable.Initializer.Value, new ConversionContext(classFieldNames, codeType, designerFilePath, classIdentifier));

                    foreach (var al in field.AttributeLists)
                        foreach (var a in al.Attributes)
                            fld.CustomAttributes.Add(AttributeSyntaxToCodeAttribute(a));

                    codeType.Members.Add(fld);
                }
            }

            // Methods
            foreach (var method in classNode.Members.OfType<MethodDeclarationSyntax>())
            {
                var cm = new CodeMemberMethod { Name = method.Identifier.Text };
                cm.Attributes = MemberAttributes.Private; // Default for designer methods
                cm.Attributes = MethodModifiersToMemberAttributes(method.Modifiers);
                cm.ReturnType = TypeSyntaxToTypeReference(method.ReturnType);

                foreach (var al in method.AttributeLists)
                    foreach (var a in al.Attributes)
                        cm.CustomAttributes.Add(AttributeSyntaxToCodeAttribute(a));

                // Context with field names and CodeTypeDeclaration for the current class
                var ctx = new ConversionContext(classFieldNames, codeType, designerFilePath, classIdentifier);
                foreach (var p in method.ParameterList.Parameters)
                {
                    cm.Parameters.Add(new CodeParameterDeclarationExpression(TypeSyntaxToTypeReference(p.Type), p.Identifier.Text));
                    ctx.ParameterNames.Add(p.Identifier.Text);
                }

                if (method.Body != null)
                {
                    // Special handling to ensure the "correct" order for AutoScaleDimensions/AutoScaleMode.
                    // Specifically: if within the InitializeComponent() method, AutoScaleMode is set to AutoScaleMode.Dpi, AND this line
                    // comes AFTER the AutoScaleDimensions line, the form scaling might be incorrect (compared to VS's default designer behavior).
                    // Therefore, we reorder these two lines to the "correct" sequence:
                    if (method.Identifier.Text == "InitializeComponent")
                    {
                        CodeAssignStatement autoScaleDimensionsStatement = null;
                        CodeAssignStatement autoAutoScaleModeStatement = null;
                        List<CodeStatement> otherStatements = new List<CodeStatement>();

                        foreach (var stmt in method.Body.Statements)
                        {
                            var cstmt = StatementSyntaxToCodeStatement(stmt, ctx);
                            if (cstmt is CodeAssignStatement assignStmt)
                            {
                                if (assignStmt.Left is CodePropertyReferenceExpression propRef)
                                {
                                    if (propRef.PropertyName == "AutoScaleMode")
                                    {
                                        autoAutoScaleModeStatement = assignStmt;
                                        continue; // do not add to otherStatements yet
                                    }
                                    else if (propRef.PropertyName == "AutoScaleDimensions")
                                    {
                                        autoScaleDimensionsStatement = assignStmt;
                                        continue; // do not add to otherStatements yet
                                    }
                                }
                            }
                            if (cstmt != null) // add all other statements
                                otherStatements.Add(cstmt);
                        }

                        // Now add the lines in the correct order: first AutoAutoScaleMode, then AutoScaleDimensions
                        if (autoAutoScaleModeStatement != null)
                            cm.Statements.Add(autoAutoScaleModeStatement);
                        if (autoScaleDimensionsStatement != null)
                            cm.Statements.Add(autoScaleDimensionsStatement);

                        // Add the remaining statements
                        cm.Statements.AddRange(otherStatements.ToArray());
                    }
                    else // for all other methods (except InitializeComponent)
                    {
                        foreach (var stmt in method.Body.Statements)
                        {
                            // Process nested blocks within If/Block etc. (StatementSyntaxToCodeStatement handles this partially)
                            var cstmt = StatementSyntaxToCodeStatement(stmt, ctx);
                            if (cstmt != null)
                                cm.Statements.Add(cstmt);
                        }
                    }
                }
                else if (method.ExpressionBody != null)
                {
                    var expr = ExpressionSyntaxToCodeExpression(method.ExpressionBody.Expression, ctx);
                    if (expr != null)
                        cm.Statements.Add(new CodeExpressionStatement(expr));
                }

                codeType.Members.Add(cm);
            }

            // Constructors (e.g. public Form1() { this.InitializeComponent(); })
            foreach (var ctor in classNode.Members.OfType<ConstructorDeclarationSyntax>())
            {
                var cc = new CodeConstructor
                {
                    Name = classIdentifier, // constructor name must match class name
                    Attributes = ConstructorModifiersToMemberAttributes(ctor.Modifiers)
                };

                // Context with field names and CodeTypeDeclaration for the current class
                var ctx = new ConversionContext(classFieldNames, codeType, designerFilePath, classIdentifier);
                foreach (var p in ctor.ParameterList.Parameters)
                {
                    cc.Parameters.Add(new CodeParameterDeclarationExpression(TypeSyntaxToTypeReference(p.Type), p.Identifier.Text));
                    ctx.ParameterNames.Add(p.Identifier.Text);
                }

                if (ctor.Body != null)
                {
                    foreach (var stmt in ctor.Body.Statements)
                    {
                        var s = StatementSyntaxToCodeStatement(stmt, ctx);
                        if (s != null) cc.Statements.Add(s);
                    }
                }

                codeType.Members.Add(cc);
            }

            // Properties (if any)
            foreach (var prop in classNode.Members.OfType<PropertyDeclarationSyntax>())
            {
                var cp = new CodeMemberProperty
                {
                    Name = prop.Identifier.Text,
                    Type = TypeSyntaxToTypeReference(prop.Type),
                    Attributes = PropertyModifiersToMemberAttributes(prop.Modifiers)
                };
                codeType.Members.Add(cp);
            }

            return new ConversionResult(codeType, usings, @namespace);
        }

        #region Helpers and conversion utilities

        /// <summary>
        /// Extracts all using directives from a C# CompilationUnitSyntax.
        /// It includes explicit usings found within the CompilationUnit and attempts to supplement them with
        /// implicit usings from the MSBuild-generated .GlobalUsings.g.cs file, if available and accessible.
        /// </summary>
        /// <param name="cu">The Roslyn CompilationUnitSyntax to parse.</param>
        /// <param name="csProjFileWrapper">Wrapper for the .csproj file. Used to locate the generated .GlobalUsings.g.cs file.</param>
        /// <returns>A distinct list of fully qualified namespace strings.</returns>
        private static List<string> ExtractUsings(CompilationUnitSyntax cu, CsProjectFileWrapper csProjFileWrapper)
        {
            // Get explicit usings from the current file
            var usings = cu.DescendantNodes().OfType<UsingDirectiveSyntax>()
                           .Select(u => u.Name.ToString())
                           .ToList();

            if (string.IsNullOrEmpty(csProjFileWrapper.ProjectFilePath))
                return usings;

            // Try to supplement with "Implicit Usings" from the MSBuild-generated file (makes sense for SDK-style project files only)
            var globalUsings = new List<string>();
            try
            {
                if (csProjFileWrapper.IsSdkStyle)
                    MessageLogger.LogVerbose(typeof(DesignerRoslynToCodeDomConverter), $"Project file '{csProjFileWrapper.ProjectFilePath}' is an SDK-style project");

                if (csProjFileWrapper.IsSdkStyle && csProjFileWrapper.ImplicitUsingsEnabled)
                {
                    MessageLogger.LogVerbose(typeof(DesignerRoslynToCodeDomConverter), $"Project file '{csProjFileWrapper.ProjectFilePath}' has <ImplicitUsings> set to 'enable'");

                    // Search for the project file to locate the 'obj' folder
                    string projectDir = Path.GetDirectoryName(csProjFileWrapper.ProjectFilePath);
                    string projectName = Path.GetFileNameWithoutExtension(csProjFileWrapper.ProjectFilePath);
                    string objPath = Path.Combine(projectDir, "obj");

                    if (Directory.Exists(objPath))
                    {
                        // The file is typically named [ProjectName].GlobalUsings.g.cs
                        var globalUsingsFile = Directory.GetFiles(objPath, $"{projectName}.GlobalUsings.g.cs", SearchOption.AllDirectories).FirstOrDefault();
                        if (globalUsingsFile != null)
                        {
                            var globalContent = File.ReadAllLines(globalUsingsFile);
                            foreach (var line in globalContent)
                            {
                                // Simple parsing: extract "System.Drawing" from "global using global::System.Drawing;"
                                var match = System.Text.RegularExpressions.Regex.Match(line, @"using\s+(?:global::)?([\w\.]+);");
                                if (match.Success)
                                    globalUsings.Add(match.Groups[1].Value);
                            }
                        }
                    }
                    if (globalUsings.Count == 0)
                        MessageLogger.LogVerbose(typeof(DesignerRoslynToCodeDomConverter), "No implicit usings detected");
                }
            }
            catch { /* Implicit usings are optional; skip if file is missing or locked */ }

            if (globalUsings.Count > 0)
            {
                usings.AddRange(globalUsings);
                MessageLogger.LogVerbose(typeof(DesignerRoslynToCodeDomConverter), $"Detected implicit usings: {string.Join(", ", globalUsings)}");
            }

            return usings.Distinct().ToList();
        }

        /// <summary>
        /// Conversion context, containing information about local variables, parameters, and class fields.
        /// It also holds a reference to the CodeTypeDeclaration, designer file path, and class name,
        /// allowing modification of the main .cs file and CodeDOM.
        /// </summary>
        private class ConversionContext
        {
            public HashSet<string> ParameterNames { get; } = new HashSet<string>(StringComparer.Ordinal);
            public HashSet<string> LocalNames { get; } = new HashSet<string>(StringComparer.Ordinal);
            public HashSet<string> FieldNames { get; }

            public CodeTypeDeclaration TargetCodeType { get; } // the CodeDOM type being built (e.g. Form1)
            public string DesignerFilePath { get; } // path to the .designer.cs file (e.g. Form1.designer.cs)
            public string ClassIdentifier { get; } // class name (e.g. "Form1")

            /// <summary>
            /// Default constructor for cases where full context is not needed (e.g. parsing attributes).
            /// </summary>
            public ConversionContext()
            {
                FieldNames = new HashSet<string>(StringComparer.Ordinal);
                TargetCodeType = null;
                DesignerFilePath = null;
                ClassIdentifier = null;
            }

            /// <summary>
            /// Full constructor for main conversion, providing all necessary information.
            /// </summary>
            public ConversionContext(HashSet<string> fieldNames, CodeTypeDeclaration targetCodeType, string designerFilePath, string classIdentifier)
            {
                FieldNames = new HashSet<string>(fieldNames, StringComparer.Ordinal);
                TargetCodeType = targetCodeType ?? throw new ArgumentNullException(nameof(targetCodeType));
                DesignerFilePath = designerFilePath ?? throw new ArgumentNullException(nameof(designerFilePath));
                ClassIdentifier = classIdentifier ?? throw new ArgumentNullException(nameof(classIdentifier));
            }
        }

        /// <summary>
        /// Converts Roslyn field modifiers to CodeDom member attributes.
        /// </summary>
        private static MemberAttributes FieldModifiersToMemberAttributes(SyntaxTokenList modifiers)
        {
            if (modifiers.Any(SyntaxKind.PublicKeyword)) return MemberAttributes.Public;
            if (modifiers.Any(SyntaxKind.ProtectedKeyword)) return MemberAttributes.Family;
            if (modifiers.Any(SyntaxKind.InternalKeyword)) return MemberAttributes.Assembly;
            return MemberAttributes.Private;
        }

        /// <summary>
        /// Converts Roslyn method modifiers to CodeDom member attributes.
        /// </summary>
        private static MemberAttributes MethodModifiersToMemberAttributes(SyntaxTokenList modifiers)
        {
            MemberAttributes result = 0;
            if (modifiers.Any(SyntaxKind.PublicKeyword)) result |= MemberAttributes.Public;
            else if (modifiers.Any(SyntaxKind.ProtectedKeyword)) result |= MemberAttributes.Family;
            else if (modifiers.Any(SyntaxKind.InternalKeyword)) result |= MemberAttributes.Assembly;
            else result |= MemberAttributes.Private;

            if (modifiers.Any(SyntaxKind.StaticKeyword)) result |= MemberAttributes.Static;
            if (modifiers.Any(SyntaxKind.OverrideKeyword)) result |= MemberAttributes.Override;
            if (modifiers.Any(SyntaxKind.AbstractKeyword)) result |= MemberAttributes.Abstract;
            // Sealed and ReadOnly for methods translate to Final (not overridable)
            if (modifiers.Any(SyntaxKind.SealedKeyword) || modifiers.Any(SyntaxKind.ReadOnlyKeyword)) result |= MemberAttributes.Final;

            return result;
        }

        /// <summary>
        /// Converts Roslyn constructor modifiers to CodeDom member attributes.
        /// </summary>
        private static MemberAttributes ConstructorModifiersToMemberAttributes(SyntaxTokenList modifiers)
        {
            if (modifiers.Any(SyntaxKind.PublicKeyword)) return MemberAttributes.Public;
            if (modifiers.Any(SyntaxKind.ProtectedKeyword)) return MemberAttributes.Family;
            if (modifiers.Any(SyntaxKind.InternalKeyword)) return MemberAttributes.Assembly;
            return MemberAttributes.Public; // default to public if none specified, common for constructors
        }

        /// <summary>
        /// Converts Roslyn property modifiers to CodeDom member attributes (uses method logic).
        /// </summary>
        private static MemberAttributes PropertyModifiersToMemberAttributes(SyntaxTokenList modifiers) => MethodModifiersToMemberAttributes(modifiers);

        /// <summary>
        /// Converts a Roslyn attribute syntax into a CodeDom attribute declaration.
        /// </summary>
        private static CodeAttributeDeclaration AttributeSyntaxToCodeAttribute(AttributeSyntax attr)
        {
            var name = attr.Name.ToString();
            var cad = new CodeAttributeDeclaration(name);
            if (attr.ArgumentList != null)
            {
                foreach (var arg in attr.ArgumentList.Arguments)
                {
                    if (arg.NameEquals != null)
                    {
                        cad.Arguments.Add(new CodeAttributeArgument(new CodeSnippetExpression(arg.Expression.ToString()))
                        {
                            Name = arg.NameEquals.Name.Identifier.Text
                        });
                    }
                    else
                    {
                        // For attribute arguments, context is not required, use an empty one
                        cad.Arguments.Add(new CodeAttributeArgument(ExpressionSyntaxToCodeExpression(arg.Expression, new ConversionContext()) ?? new CodeSnippetExpression(arg.Expression.ToString())));
                    }
                }
            }
            return cad;
        }

        /// <summary>
        /// Converts a Roslyn type into a CodeDom type reference.
        /// </summary>
        private static CodeTypeReference TypeSyntaxToTypeReference(TypeSyntax type)
        {
            if (type == null) return new CodeTypeReference(typeof(void));
            if (type is PredefinedTypeSyntax pts)
            {
                var kw = pts.Keyword.Text;
                switch (kw)
                {
                    case "void": return new CodeTypeReference(typeof(void));
                    case "int": return new CodeTypeReference(typeof(int));
                    case "string": return new CodeTypeReference(typeof(string));
                    case "bool": return new CodeTypeReference(typeof(bool));
                    case "float": return new CodeTypeReference(typeof(float));
                    case "double": return new CodeTypeReference(typeof(double));
                    case "decimal": return new CodeTypeReference(typeof(decimal));
                    case "object": return new CodeTypeReference(typeof(object));
                    case "char": return new CodeTypeReference(typeof(char));
                    case "byte": return new CodeTypeReference(typeof(byte));
                    case "long": return new CodeTypeReference(typeof(long));
                    case "short": return new CodeTypeReference(typeof(short));
                    case "sbyte": return new CodeTypeReference(typeof(sbyte));
                    case "ushort": return new CodeTypeReference(typeof(ushort));
                    case "uint": return new CodeTypeReference(typeof(uint));
                    case "ulong": return new CodeTypeReference(typeof(ulong));
                    default:
                        return new CodeTypeReference(pts.ToString());
                }
            }
            // For other types, use the string representation (supports QualifiedName and others)
            return new CodeTypeReference(type.ToString());
        }

        /// <summary>
        /// Attempts to recognize an ExpressionSyntax as a type name (fully qualified or short) and returns a CodeTypeReferenceExpression.
        /// </summary>
        private static CodeTypeReferenceExpression TryCreateTypeReferenceExpression(ExpressionSyntax expr)
        {
            if (expr == null)
                return null;

            // Recognize 'global::'
            if (expr is AliasQualifiedNameSyntax aqn)
            {
                return new CodeTypeReferenceExpression(new CodeTypeReference(aqn.ToString()));
            }

            // IdentifierNameSyntax: "Color", "Form"
            if (expr is IdentifierNameSyntax id)
            {
                var name = id.Identifier.Text;
                // Heuristic: if it starts with an uppercase letter, it might be a type
                if (!string.IsNullOrEmpty(name) && char.IsUpper(name[0]))
                {
                    return new CodeTypeReferenceExpression(new CodeTypeReference(name));
                }
            }
            // MemberAccessExpressionSyntax: "System.Drawing.Color"
            else if (expr is MemberAccessExpressionSyntax ma)
            {
                var leftPart = TryCreateTypeReferenceExpression(ma.Expression); // recursively for System.Drawing
                if (leftPart != null)
                {
                    // If the left part is already recognized as a type, the current MemberAccessExpression might be a fully qualified type name.
                    // For example, System.Drawing -> CodeTypeReferenceExpression("System.Drawing")
                    // - then System.Drawing.Color -> CodeTypeReferenceExpression("System.Drawing.Color")
                    return new CodeTypeReferenceExpression(new CodeTypeReference($"{leftPart.Type.BaseType}.{ma.Name.Identifier.Text}"));
                }
                // If the left part is not a type (e.g. "this.ListView1"), then it's not a type name.
            }
            // QualifiedNameSyntax: "System.Windows.Forms.Form"
            else if (expr is QualifiedNameSyntax qn)
            {
                return new CodeTypeReferenceExpression(new CodeTypeReference(qn.ToString()));
            }

            return null;
        }

        /// <summary>
        /// Converts a Roslyn type syntax into a CodeDom type reference expression.
        /// </summary>
        private static CodeTypeReferenceExpression TypeSyntaxToTypeReferenceExpression(TypeSyntax type)
        {
            return new CodeTypeReferenceExpression(TypeSyntaxToTypeReference(type));
        }

        /// <summary>
        /// Converts a Roslyn statement syntax into a CodeDom statement.
        /// </summary>
        private static CodeStatement StatementSyntaxToCodeStatement(StatementSyntax stmt, ConversionContext ctx)
        {
            switch (stmt)
            {
                case LocalDeclarationStatementSyntax localDecl:
                    foreach (var v in localDecl.Declaration.Variables)
                        ctx.LocalNames.Add(v.Identifier.Text);

                    var decl = localDecl.Declaration;
                    var first = decl.Variables.First();
                    var initExpr = first.Initializer != null ? ExpressionSyntaxToCodeExpression(first.Initializer.Value, ctx) : null;
                    return new CodeVariableDeclarationStatement(TypeSyntaxToTypeReference(decl.Type), first.Identifier.Text, initExpr);

                case ExpressionStatementSyntax exprStmt:
                    return ExpressionStatementToCodeStatement(exprStmt.Expression, ctx);

                case IfStatementSyntax ifStmt:
                    var condition = ExpressionSyntaxToCodeExpression(ifStmt.Condition, ctx);
                    var cs = new CodeConditionStatement(condition);
                    if (ifStmt.Statement is BlockSyntax thenBlock)
                    {
                        foreach (var inner in thenBlock.Statements)
                        {
                            var s = StatementSyntaxToCodeStatement(inner, ctx);
                            if (s != null) cs.TrueStatements.Add(s);
                        }
                    }
                    else
                    {
                        var ts = StatementSyntaxToCodeStatement(ifStmt.Statement, ctx);
                        if (ts != null) cs.TrueStatements.Add(ts);
                    }

                    if (ifStmt.Else != null)
                    {
                        if (ifStmt.Else.Statement is BlockSyntax elseBlock)
                        {
                            foreach (var inner in elseBlock.Statements)
                            {
                                var s = StatementSyntaxToCodeStatement(inner, ctx);
                                if (s != null) cs.FalseStatements.Add(s);
                            }
                        }
                        else
                        {
                            var es = StatementSyntaxToCodeStatement(ifStmt.Else.Statement, ctx);
                            if (es != null) cs.FalseStatements.Add(es);
                        }
                    }
                    return cs;

                case ReturnStatementSyntax retStmt:
                    var expr = retStmt.Expression != null ? ExpressionSyntaxToCodeExpression(retStmt.Expression, ctx) : null;
                    return new CodeMethodReturnStatement(expr);

                case BlockSyntax _: // blocks are processed recursively within other statements
                    return null;

                case TryStatementSyntax tryStmt:
                    return ConvertTryStatement(tryStmt, ctx);

                default:
                    // For unrecognized statements, return a CodeSnippetStatement
                    return new CodeSnippetStatement(stmt.ToString());
            }
        }

        /// <summary>
        /// Converts a Roslyn TryStatement syntax into a CodeDom Try-Catch-Finally statement.
        /// </summary>
        private static CodeTryCatchFinallyStatement ConvertTryStatement(TryStatementSyntax tryStmt, ConversionContext ctx)
        {
            var ct = new CodeTryCatchFinallyStatement();

            // Try block
            if (tryStmt.Block != null)
            {
                foreach (var s in tryStmt.Block.Statements)
                {
                    var cs = StatementSyntaxToCodeStatement(s, ctx);
                    if (cs != null) ct.TryStatements.Add(cs);
                }
            }

            // Catch clauses
            foreach (var catchClause in tryStmt.Catches)
            {
                var cc = ConvertCatchClause(catchClause, ctx);
                if (cc != null) ct.CatchClauses.Add(cc);
            }

            // Finally block
            if (tryStmt.Finally != null && tryStmt.Finally.Block != null)
            {
                foreach (var s in tryStmt.Finally.Block.Statements)
                {
                    var cs = StatementSyntaxToCodeStatement(s, ctx);
                    if (cs != null) ct.FinallyStatements.Add(cs);
                }
            }

            return ct;
        }

        /// <summary>
        /// Converts a Roslyn CatchClause syntax into a CodeDom Catch block.
        /// </summary>
        private static CodeCatchClause ConvertCatchClause(CatchClauseSyntax catchClause, ConversionContext ctx)
        {
            // Exception variable name and type (if specified)
            string varName = "ex"; // default name
            CodeTypeReference catchType = null;

            if (catchClause.Declaration != null)
            {
                if (!string.IsNullOrEmpty(catchClause.Declaration.Identifier.Text))
                    varName = catchClause.Declaration.Identifier.Text;
                if (catchClause.Declaration.Type != null)
                    catchType = TypeSyntaxToTypeReference(catchClause.Declaration.Type);
            }
            else
            {
                // Catch without declaration (catch { ... })
                varName = null; // CodeCatchClause constructor accepts null as variable name
            }

            CodeCatchClause cc;
            if (varName != null)
                cc = new CodeCatchClause(varName) { CatchExceptionType = catchType };
            else
                cc = new CodeCatchClause(null) { CatchExceptionType = catchType };

            if (catchClause.Block != null)
            {
                foreach (var s in catchClause.Block.Statements)
                {
                    var cs = StatementSyntaxToCodeStatement(s, ctx);
                    if (cs != null) cc.Statements.Add(cs);
                }
            }
            return cc;
        }

        /// <summary>
        /// Converts a Roslyn expression syntax into a CodeDom event reference expression.
        /// </summary>
        private static CodeEventReferenceExpression ExpressionSyntaxToCodeEventReference(ExpressionSyntax expr, ConversionContext ctx)
        {
            if (expr == null) return null;
            if (expr is MemberAccessExpressionSyntax maes)
            {
                var targetExpr = ExpressionSyntaxToCodeExpression(maes.Expression, ctx) ?? new CodeThisReferenceExpression();
                var eventName = maes.Name.Identifier.Text;
                return new CodeEventReferenceExpression(targetExpr, eventName);
            }
            if (expr is IdentifierNameSyntax id)
                return new CodeEventReferenceExpression(new CodeThisReferenceExpression(), id.Identifier.Text);
            return null;
        }

        /// <summary>
        /// Converts a Roslyn expression statement syntax into a CodeDom statement.
        /// </summary>
        private static CodeStatement ExpressionStatementToCodeStatement(ExpressionSyntax expr, ConversionContext ctx)
        {
            switch (expr)
            {
                case AssignmentExpressionSyntax assign:
                    if (assign.Kind() == SyntaxKind.AddAssignmentExpression) // handling '+=' (event subscription)
                    {
                        var leftEventRef = ExpressionSyntaxToCodeEventReference(assign.Left, ctx);
                        var right = assign.Right;

                        string handlerName = null;
                        CodeExpression targetForDelegate = null;
                        string delegateType = "System.EventHandler"; // default value

                        // Case 1: Classic WinForms (new System.EventHandler(this.OnClick))
                        if (right is ObjectCreationExpressionSyntax oce && leftEventRef != null)
                        {
                            delegateType = oce.Type.ToString();
                            var argExpr = oce.ArgumentList?.Arguments.FirstOrDefault()?.Expression;

                            if (argExpr is MemberAccessExpressionSyntax handlerAccess)
                            {
                                handlerName = handlerAccess.Name.Identifier.Text;
                                targetForDelegate = ExpressionSyntaxToCodeExpression(handlerAccess.Expression, ctx);
                            }
                            else if (argExpr is IdentifierNameSyntax identifier)
                            {
                                handlerName = identifier.Identifier.Text;
                                targetForDelegate = new CodeThisReferenceExpression();
                            }
                        }
                        // Case 2: Shorthand syntax (method group: this.Click += this.MyHandler or this.Click += MyHandler)
                        else if ((right is MemberAccessExpressionSyntax || right is IdentifierNameSyntax) && leftEventRef != null)
                        {
                            if (right is MemberAccessExpressionSyntax ma)
                            {
                                handlerName = ma.Name.Identifier.Text;
                                targetForDelegate = ExpressionSyntaxToCodeExpression(ma.Expression, ctx);
                            }
                            else if (right is IdentifierNameSyntax id)
                            {
                                handlerName = id.Identifier.Text;
                                targetForDelegate = new CodeThisReferenceExpression();
                            }
                        }

                        // If handler name was successfully determined
                        if (!string.IsNullOrEmpty(handlerName) && leftEventRef != null)
                        {
                            // Check (just for reference) if the handler method exists in the main .cs file:
                            EventHelper.CheckEventHandlerExistsInMainFile(ctx.DesignerFilePath, ctx.ClassIdentifier, handlerName);

                            // Create the actual event subscription expression in CodeDOM:
                            var delegateCreate = new CodeDelegateCreateExpression(new CodeTypeReference(delegateType),
                                                                                 targetForDelegate ?? new CodeThisReferenceExpression(),
                                                                                 handlerName);
                            return new CodeAttachEventStatement(leftEventRef, delegateCreate);
                        }
                        else
                        {
                            // If the structure could not be recognized, return as a snippet to avoid losing the line.
                            return new CodeSnippetStatement(assign.ToString());
                        }
                    }
                    else // regular assignment '='
                    {
                        if (assign.OperatorToken.IsKind(SyntaxKind.EqualsToken))
                        {
                            var left = ExpressionSyntaxToCodeExpression(assign.Left, ctx);
                            var right = ExpressionSyntaxToCodeExpression(assign.Right, ctx);
                            return new CodeAssignStatement(left, right);
                        }
                        else
                        {
                            // Non-standard assignment operator
                            return new CodeSnippetStatement(expr.ToString());
                        }
                    }

                case InvocationExpressionSyntax inv:
                    // Method invocation
                    var (targetExpr, methodName2) = InvocationTargetAndName(inv.Expression, ctx);
                    var args = inv.ArgumentList?.Arguments.Select(a => ExpressionSyntaxToCodeExpression(a.Expression, ctx)).ToArray() ?? new CodeExpression[0];
                    return new CodeExpressionStatement(new CodeMethodInvokeExpression(targetExpr, methodName2, args));

                default:
                    // For all other expressions that can be statements (e.g. a simple method call)
                    var ce = ExpressionSyntaxToCodeExpression(expr, ctx);
                    if (ce != null)
                    {
                        return new CodeExpressionStatement(ce);
                    }
                    else
                    {
                        // If the expression cannot be converted to CodeExpression, use Snippet
                        return new CodeSnippetStatement(expr.ToString());
                    }
            }
        }

        /// <summary>
        /// Determines the target object and method name for an invocation.
        /// </summary>
        private static (CodeExpression targetExpr, string methodName) InvocationTargetAndName(ExpressionSyntax expr, ConversionContext ctx)
        {
            if (expr is MemberAccessExpressionSyntax maes)
            {
                // Here, we need to correctly determine if maes.Expression is a type or an instance
                var typeRefExpr = TryCreateTypeReferenceExpression(maes.Expression); // attempt to recognize the left part as a type
                if (typeRefExpr != null)
                {
                    // If the left part is a TYPE (e.g. System.Drawing.Color), then the method is static
                    // TargetObject should be CodeTypeReferenceExpression.
                    return (typeRefExpr, maes.Name.Identifier.Text);
                }
                else
                {
                    // If the left part is not a type (e.g. "this.ListView1"), it's an instance member access
                    var target = ExpressionSyntaxToCodeExpression(maes.Expression, ctx);
                    var name = maes.Name.Identifier.Text;
                    return (target, name);
                }
            }
            else if (expr is IdentifierNameSyntax id)
                // If it's just an identifier (e.g. SomeMethod()), the target is 'this'
                return (new CodeThisReferenceExpression(), id.Identifier.Text);
            else
                // Fallback for unrecognized scenarios
                return (new CodeThisReferenceExpression(), expr.ToString());
        }

        /// <summary>
        /// Converts a Roslyn expression syntax into a CodeDom expression.
        /// </summary>
        private static CodeExpression ExpressionSyntaxToCodeExpression(ExpressionSyntax expr, ConversionContext ctx)
        {
            if (expr == null)
                return null;

            switch (expr)
            {
                // Support for global and fully qualified names:
                case AliasQualifiedNameSyntax aqn:
                    return new CodeTypeReferenceExpression(new CodeTypeReference(aqn.ToString()));
                case QualifiedNameSyntax qn:
                    return new CodeTypeReferenceExpression(new CodeTypeReference(qn.ToString()));

                case LiteralExpressionSyntax lit:
                    return LiteralToCodePrimitive(lit);

                case ObjectCreationExpressionSyntax oce:
                    var typeRef = TypeSyntaxToTypeReference(oce.Type);
                    var args = oce.ArgumentList?.Arguments.Select(a => ExpressionSyntaxToCodeExpression(a.Expression, ctx)).ToArray() ?? new CodeExpression[0];
                    return new CodeObjectCreateExpression(typeRef, args);

                case IdentifierNameSyntax id:
                    {
                        var name = id.Identifier.Text;
                        if (ctx.LocalNames.Contains(name))
                            return new CodeVariableReferenceExpression(name);
                        if (ctx.ParameterNames.Contains(name))
                            return new CodeArgumentReferenceExpression(name);

                        // If IdentifierName is a class field name (ListView1, Button1),
                        // it should be a CodeFieldReferenceExpression relative to 'this'.
                        if (ctx.FieldNames.Contains(name))
                            return new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), name);

                        // If IdentifierName is not resolved as a local variable/parameter/field and starts with an uppercase letter, it's likely a type.
                        if (!string.IsNullOrEmpty(name) && char.IsUpper(name[0]))
                            return new CodeTypeReferenceExpression(new CodeTypeReference(name));

                        // If IdentifierName cannot be resolved as a local variable, parameter, field, or type,
                        // return it as a CodeSnippetExpression.
                        MessageLogger.LogWarning(typeof(DesignerRoslynToCodeDomConverter),
                            $"Identifier '{name}' could not be resolved as local, parameter, field, or type. Using CodeSnippetExpression.");
                        return new CodeSnippetExpression(name);
                    }

                case MemberAccessExpressionSyntax ma:
                    {
                        var memberName = ma.Name.Identifier.Text;
                        var leftExpr = ExpressionSyntaxToCodeExpression(ma.Expression, ctx); // recursively process left side

                        // 1. Attempt to recognize the left side as a TYPE name (e.g. "System.Drawing.Color", "System.Windows.Forms.ColorDepth").
                        CodeTypeReferenceExpression typeRefTarget = TryCreateTypeReferenceExpression(ma.Expression);

                        if (typeRefTarget != null)
                        {
                            // If the left side is a TYPE (e.g. System.Drawing.Color, System.Windows.Forms.ColorDepth),
                            // then memberName is a static member of that type.
                            //
                            // System.Drawing.Color.FromArgb(...) -> this is a method
                            // System.Drawing.Color.Empty -> this is a property (static) or field
                            // System.Windows.Forms.ColorDepth.Depth8Bit -> this is a field (enum member)
                            //
                            // For CodeDOM, enum members and static readonly fields are best represented as CodeFieldReferenceExpression.
                            // For static properties (like Color.Empty), CodeFieldReferenceExpression can also work,
                            // as the deserializer can often access them via reflection as "fields" to get their value.
                            // Thus, CodeFieldReferenceExpression is safer here, especially for enum members.
                            //
                            // HOWEVER, for project-level resources (i.e. from Properties\Resources.resx), it is CRITICAL to use
                            // CodePropertyReferenceExpression specifically (otherwise resources won't be read).

                            string baseType = typeRefTarget.Type.BaseType;
                            if (baseType.EndsWith(".Resources") || baseType.Contains(".Properties.")) // heuristic
                            {
                                return new CodePropertyReferenceExpression(typeRefTarget, memberName);
                            }

                            return new CodeFieldReferenceExpression(typeRefTarget, memberName);
                        }

                        // 2. If the left side is 'this' or 'base', and memberName is a class field.
                        //    Example: this.ListView1.BackColor, where ListView1 is a field.
                        if ((leftExpr is CodeThisReferenceExpression || leftExpr is CodeBaseReferenceExpression) && ctx.FieldNames.Contains(memberName))
                        {
                            // This is an access to a class field (ListView1, components).
                            return new CodeFieldReferenceExpression(leftExpr, memberName);
                        }

                        // 3. In all other cases, it's likely an instance property.
                        //    Example: this.ClientSize (ClientSize is a property of Form),
                        //    or this.ListView1.BackColor (BackColor is a property of ListView1),
                        //    where leftExpr is already a CodeFieldReferenceExpression for ListView1.
                        return new CodePropertyReferenceExpression(leftExpr, memberName);
                    }

                case InvocationExpressionSyntax inv:
                    // Method invocation
                    var tn = InvocationTargetAndName(inv.Expression, ctx);
                    var args2 = inv.ArgumentList?.Arguments.Select(a => ExpressionSyntaxToCodeExpression(a.Expression, ctx)).ToArray() ?? new CodeExpression[0];
                    return new CodeMethodInvokeExpression(tn.targetExpr, tn.methodName, args2);

                case BinaryExpressionSyntax bin:
                    // Binary operations (addition, comparison, etc.)
                    var leftBin = ExpressionSyntaxToCodeExpression(bin.Left, ctx);
                    var rightBin = ExpressionSyntaxToCodeExpression(bin.Right, ctx);
                    var op = SyntaxKindToCodeBinaryOperatorType(bin.Kind());
                    if (op.HasValue)
                        return new CodeBinaryOperatorExpression(leftBin, op.Value, rightBin);
                    else
                        // For unrecognized operators
                        return new CodeSnippetExpression(bin.ToString());

                case ParenthesizedExpressionSyntax par:
                    // Expressions in parentheses
                    return ExpressionSyntaxToCodeExpression(par.Expression, ctx);

                case CastExpressionSyntax cast:
                    // Type casting
                    var targetType = TypeSyntaxToTypeReference(cast.Type);
                    var inner = ExpressionSyntaxToCodeExpression(cast.Expression, ctx);
                    return new CodeCastExpression(targetType, inner);

                case TypeOfExpressionSyntax to:
                    // typeof(MyType).
                    return new CodeTypeOfExpression(TypeSyntaxToTypeReference(to.Type));

                case ThisExpressionSyntax _:
                    // this.
                    return new CodeThisReferenceExpression();

                case BaseExpressionSyntax _:
                    // base.
                    return new CodeBaseReferenceExpression();

                case ElementAccessExpressionSyntax eae:
                    // Element access (e.g. array[index]).
                    var target = ExpressionSyntaxToCodeExpression(eae.Expression, ctx);
                    var idxs = eae.ArgumentList.Arguments.Select(a => ExpressionSyntaxToCodeExpression(a.Expression, ctx)).ToArray();
                    return new CodeArrayIndexerExpression(target, idxs);

                case ArrayCreationExpressionSyntax ace:
                    {
                        // Array type (e.g. System.Windows.Forms.ColumnHeader[]).
                        var arrayType = TypeSyntaxToTypeReference(ace.Type.ElementType);

                        // If there's an initializer ({ this.columnHeader1 }).
                        if (ace.Initializer != null)
                        {
                            var initializers = ace.Initializer.Expressions
                                .Select(e => ExpressionSyntaxToCodeExpression(e, ctx))
                                .Where(e => e != null)
                                .ToArray();
                            return new CodeArrayCreateExpression(arrayType, initializers);
                        }
                        else // if no initializer, but size is specified (new int[10]).
                        {
                            if (ace.Type.RankSpecifiers.Any() && ace.Type.RankSpecifiers.First().Sizes.Any())
                            {
                                // Take the first size (for single-dimensional arrays).
                                var sizeExpression = ExpressionSyntaxToCodeExpression(ace.Type.RankSpecifiers.First().Sizes.First(), ctx);
                                return new CodeArrayCreateExpression(arrayType, sizeExpression);
                            }
                            return new CodeArrayCreateExpression(arrayType, 0); // default to an empty array.
                        }
                    }

                default:
                    // Fallback for unrecognized expressions.
                    return new CodeSnippetExpression(expr.ToString());
            }
        }

        /// <summary>
        /// Converts a Roslyn literal expression into a CodeDom primitive expression.
        /// </summary>
        private static CodeExpression LiteralToCodePrimitive(LiteralExpressionSyntax lit)
        {
            var token = lit.Token;
            if (token.Value == null) return new CodePrimitiveExpression(null);
            return new CodePrimitiveExpression(token.Value);
        }

        /// <summary>
        /// Converts a Roslyn syntax node kind into a CodeDom binary operator type.
        /// </summary>
        private static CodeBinaryOperatorType? SyntaxKindToCodeBinaryOperatorType(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.EqualsExpression: return CodeBinaryOperatorType.ValueEquality;
                case SyntaxKind.NotEqualsExpression: return CodeBinaryOperatorType.IdentityInequality;
                case SyntaxKind.GreaterThanExpression: return CodeBinaryOperatorType.GreaterThan;
                case SyntaxKind.LessThanExpression: return CodeBinaryOperatorType.LessThan;
                case SyntaxKind.GreaterThanOrEqualExpression: return CodeBinaryOperatorType.GreaterThanOrEqual;
                case SyntaxKind.LessThanOrEqualExpression: return CodeBinaryOperatorType.LessThanOrEqual;
                case SyntaxKind.LogicalAndExpression: return CodeBinaryOperatorType.BooleanAnd; // C# &&
                case SyntaxKind.LogicalOrExpression: return CodeBinaryOperatorType.BooleanOr;   // C# ||
                case SyntaxKind.AmpersandToken: // C# & (bitwise AND)
                case SyntaxKind.BitwiseAndExpression: return CodeBinaryOperatorType.BitwiseAnd;
                case SyntaxKind.BarToken: // C# | (bitwise OR)
                case SyntaxKind.BitwiseOrExpression: return CodeBinaryOperatorType.BitwiseOr;
                case SyntaxKind.AddExpression: return CodeBinaryOperatorType.Add;
                case SyntaxKind.SubtractExpression: return CodeBinaryOperatorType.Subtract;
                case SyntaxKind.MultiplyExpression: return CodeBinaryOperatorType.Multiply;
                case SyntaxKind.DivideExpression: return CodeBinaryOperatorType.Divide;
                default: return null;
            }
        }

        #endregion

        /// <summary>
        /// The result of the conversion, containing the CodeTypeDeclaration, list of using directives, and namespace.
        /// </summary>
        public class ConversionResult
        {
            public CodeTypeDeclaration TypeDeclaration { get; }
            public List<string> Usings { get; } // List of fully qualified imported namespaces, e.g. "System.Windows.Forms"
            public string Namespace { get; set; } // Namespace of the processed class, e.g. "WindowsFormsApp1"

            public ConversionResult(CodeTypeDeclaration typeDeclaration, List<string> usings, string @namespace)
            {
                TypeDeclaration = typeDeclaration;
                Usings = usings;
                Namespace = @namespace;
            }
        }
    }
}
