using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace UiTools.WinForms.Designer.Core
{
    public static class CommonStuff
    {
        public static readonly Encoding Utf8WithoutBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        public static UiTheme CurrentUiTheme { get; set; }

        public static string GetEmbeddedResource(string res)
        {
            using (var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(res)))
            {
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// Calculates "main" file path from the "designer" file path (e.g. "C:\Path\to\Form1.cs" from "C:\Path\to\Form1.Designer.cs")
        /// </summary>
        public static string MainCsFilePathFromDesignerCsFilePath(string designerCsFilePath)
        {
            if (!IsDesignerCsFilePathValid(designerCsFilePath))
                return null;
            string designerCsFileName = Path.GetFileName(designerCsFilePath);
            string baseFileName = designerCsFileName.Substring(0, designerCsFileName.Length - ".designer.cs".Length); // remove ".designer.cs" (".Designer.cs") from the file name
            return Path.Combine(Path.GetDirectoryName(designerCsFilePath), baseFileName + ".cs");
        }

        internal static bool IsDesignerCsFilePathValid(string designerCsFilePath)
        {
            return designerCsFilePath != null && designerCsFilePath.EndsWith(".designer.cs", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Calculates resource file path from the "designer" file path (e.g. "C:\Path\to\Form1.resx" from "C:\Path\to\Form1.Designer.cs")
        /// </summary>
        internal static string ResxFilePathFromDesignerCsFilePath(string designerCsFilePath)
        {
            if (!IsDesignerCsFilePathValid(designerCsFilePath))
                return null;
            string designerCsFileName = Path.GetFileName(designerCsFilePath);
            string baseFileName = designerCsFileName.Substring(0, designerCsFileName.Length - ".designer.cs".Length); // remove ".designer.cs" (".Designer.cs") from the file name
            return Path.Combine(Path.GetDirectoryName(designerCsFilePath), baseFileName + ".resx");
        }

        internal static int FindFirstMissingNumberInSortedArray(int[] sortedArray)
        {
            for (int i = 0; i < sortedArray.Length - 1; i++)
            {
                if (sortedArray[i + 1] != sortedArray[i] + 1)
                    return sortedArray[i] + 1;
            }
            return sortedArray[sortedArray.Length - 1] + 1;
        }

        /// <summary>
        /// Throws ArgumentNullException if the supplied value is null or an empty string.
        /// Mimics ArgumentNullException.ThrowIfNull() method available in .NET 6+, extending it with empty string check.
        /// </summary>
        public static void ThrowIfNullOrEmpty(object argument, string message = null, [CallerArgumentExpression(nameof(argument))] string argumentName = null)
        {
            if (argument == null)
            {
                if (message == null)
                    throw new ArgumentNullException(argumentName);
                else
                    throw new ArgumentNullException(argumentName, message);
            }
            if (argument is string s && s.Length == 0)
            {
                if (message == null)
                    throw new ArgumentNullException(argumentName, "Value cannot be an empty string");
                else
                    throw new ArgumentNullException(argumentName, message);
            }
        }

        public static void ThrowIfFileNotFound(string argument, string message = null, [CallerArgumentExpression(nameof(argument))] string argumentName = null)
        {
            if (!File.Exists(argument))
            {
                string msg = string.IsNullOrWhiteSpace(message) ? $"File not found: {argument}" : message;
                throw new FileNotFoundException($"{msg} (parameter name: {argumentName})");
            }
        }

        // Dictionary mapping system types to C# keywords
        private static readonly Dictionary<Type, string> SimpleTypes = new Dictionary<Type, string>
        {
            { typeof(bool), "bool" },
            { typeof(byte), "byte" },
            { typeof(char), "char" },
            { typeof(decimal), "decimal" },
            { typeof(double), "double" },
            { typeof(float), "float" },
            { typeof(int), "int" },
            { typeof(long), "long" },
            { typeof(object), "object" },
            { typeof(sbyte), "sbyte" },
            { typeof(short), "short" },
            { typeof(string), "string" },
            { typeof(uint), "uint" },
            { typeof(ulong), "ulong" },
            { typeof(ushort), "ushort" },
            { typeof(void), "void" }
        };

        internal static string GetFriendlyTypeName(Type type)
        {
            ThrowIfNullOrEmpty(type);

            if (SimpleTypes.TryGetValue(type, out var name))
                return name;

            // For Nullable types (e.g. int?)
            Type nullableType = Nullable.GetUnderlyingType(type);
            if (nullableType != null)
                return GetFriendlyTypeName(nullableType) + "?";

            return type.FullName;
        }

        internal static string EscapeJavaScriptString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var sb = new StringBuilder(input.Length + 10);

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                switch (c)
                {
                    case '\\':
                        sb.Append(@"\\");
                        break;
                    case '"':
                        sb.Append(@"\""");
                        break;
                    case '\'':
                        sb.Append(@"\'");
                        break;
                    case '\n':
                        sb.Append(@"\n");
                        break;
                    case '\r':
                        sb.Append(@"\r");
                        break;
                    case '\t':
                        sb.Append(@"\t");
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }
    }
}
