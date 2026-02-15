using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using static UiTools.WinForms.Designer.Core.CommonStuff;

namespace UiTools.WinForms.Designer.Core
{
    public class DesignerFilePreProcessor
    {
        // Regular expression to find the class declaration in a .cs file.
        // Matches: (modifiers) partial class ClassName (: BaseTypes)
        private static readonly Regex ClassDeclarationRegex = new Regex(
            @"^\s*(?<modifiers>[\w\s]*?)\s*partial\s+class\s+(?<className>\w+)\s*(?<baseList>:\s*[\w\.\s,<>]+)?",
            RegexOptions.Multiline | RegexOptions.Compiled
        );

        // Regular expression for 'using' directives (full line)
        private static readonly Regex UsingRegex = new Regex(
            @"^\s*using\s+[^\r\n;]+;\s*$",
            RegexOptions.Multiline | RegexOptions.Compiled
        );

        /// <summary>
        /// Adjusts the content of the .designer.cs file by adding missing access modifiers, base types,
        /// and 'using' directives from the corresponding .cs file (the other part of the partial class).
        /// Changes are made only in the string content without writing to disk.
        /// </summary>
        /// <param name="designerCsFilePath">The full path to the .designer.cs file.</param>
        /// <returns>The adjusted string content of the .designer.cs file.</returns>
        public string ReadDesignerFileAndFixClassDeclarationLine(string designerCsFilePath)
        {
            ThrowIfNullOrEmpty(designerCsFilePath);

            // Read the content of the .designer.cs file:
            string designerCsFileContent;
            try
            {
                designerCsFileContent = File.ReadAllText(designerCsFilePath);
            }
            catch (Exception ex)
            {
                var errMsg = $"Failed to read designer file '{designerCsFilePath}': {ex.Message}.";
                MessageLogger.LogError(this, errMsg, ex);
                throw new Exception(errMsg, ex);
            }

            if (!CommonStuff.IsDesignerCsFilePathValid(designerCsFilePath))
            {
                MessageLogger.LogWarning(this, $"could not resolve the main file path from '{designerCsFilePath}' (expected a path ending in '.designer.cs')." +
                    $"Returning '{designerCsFilePath}' contents as is.");
                return designerCsFileContent;
            }

            // Compose the corresponding .cs file name (e.g. "Form1.Designer.cs" --> "Form1.cs")
            string mainFilePath = CommonStuff.MainCsFilePathFromDesignerCsFilePath(designerCsFilePath);
            if (!File.Exists(mainFilePath))
            {
                // If the corresponding .cs file is not found - simply return the original file content:
                MessageLogger.LogWarning(this, $"Main file not found ('{mainFilePath}'). Returning '{designerCsFilePath}' contents as is.");
                return designerCsFileContent;
            }

            // Read the content of the .cs file:
            string mainFileContent;
            try
            {
                mainFileContent = File.ReadAllText(mainFilePath);
            }
            catch (Exception ex)
            {
                MessageLogger.LogError(this, $"Failed to read main file '{mainFilePath}': {ex.Message}. Returning '{designerCsFilePath}' contents as is.", ex);
                return designerCsFileContent;
            }

            // Look for the class declaration in both "parts":
            var designerCsFileMatch = ClassDeclarationRegex.Match(designerCsFileContent);
            var mainFileMatch = ClassDeclarationRegex.Match(mainFileContent);

            if (!designerCsFileMatch.Success)
            {
                MessageLogger.LogWarning(this, $"Couldn't find class declaration line in the supplied .designer.cs file. Returning '{designerCsFilePath}' contents as is.");
                return designerCsFileContent;
            }
            if (!mainFileMatch.Success)
            {
                MessageLogger.LogWarning(this, $"Couldn't find class declaration line in the main file ('{mainFilePath}'). Returning '{designerCsFilePath}' contents as is.");
                return designerCsFileContent;
            }

            // If the .cs file contains base type declarations (e.g. "Form") to be copied into the .designer.cs file,
            // we should also copy missing 'using' directives to ensure short names of base types are correctly resolved:
            string mainFileBaseTypesList = mainFileMatch.Groups["baseList"].Value.Trim();
            string designerCsFileBaseTypesList = designerCsFileMatch.Groups["baseList"].Value.Trim();
            if (!string.IsNullOrWhiteSpace(mainFileBaseTypesList))
            {
                if (string.IsNullOrWhiteSpace(designerCsFileBaseTypesList))
                {
                    // Collect 'using' directives from both files (as full lines)
                    var designerCsFileUsings = ExtractUsingDirectives(designerCsFileContent);
                    var mainFileUsings = ExtractUsingDirectives(mainFileContent);

                    // Look for 'using' directives that are missing in the .designer.cs file but present in the .cs file:
                    var missingUsings = mainFileUsings
                        .Where(u => !designerCsFileUsings.Contains(u, StringComparer.Ordinal))
                        .ToList();
                    if (missingUsings.Count > 0)
                    {
                        // Insert these missing 'using' directives into the designerCsFileContent (the returned text),
                        // then recalculate designerCsFileMatch as offsets will obviously change:
                        designerCsFileContent = InsertMissingUsings(designerCsFileContent, missingUsings);
                        designerCsFileMatch = ClassDeclarationRegex.Match(designerCsFileContent);
                        if (!designerCsFileMatch.Success)
                        {
                            MessageLogger.LogWarning(this, "Couldn't find class declaration line in the supplied .designer.cs file content (after usings from its " +
                                "2nd part were inserted). Returning .designer.cs file content with inserted usings (can't proceed to modifiers and base types analysis).");
                            return designerCsFileContent;
                        }
                    }
                }
            }

            // Determine what needs to be added to the .designer.cs file (after potentially inserting 'using' directives)
            string designerDeclarationLine = designerCsFileMatch.Value;
            string newDeclarationLine = designerDeclarationLine;
            bool modified = false;

            // Add access modifier if it's missing
            string mainFileModifiers = mainFileMatch.Groups["modifiers"].Value.Trim();
            string accessModifier = mainFileModifiers
                .Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault(m => m.In("public", "internal", "protected", "private"));

            if (!string.IsNullOrEmpty(accessModifier))
            {
                // Check if an access modifier already exists in the .designer.cs file
                string designerPartModifiers = designerCsFileMatch.Groups["modifiers"].Value.Trim();
                bool designerPartHasAccessModifier = designerPartModifiers.Contains("public") ||
                                                     designerPartModifiers.Contains("internal") ||
                                                     designerPartModifiers.Contains("protected") ||
                                                     designerPartModifiers.Contains("private");

                if (!designerPartHasAccessModifier)
                {
                    // Insert the modifier before 'partial'
                    // Example: "partial class Form1" --> "public partial class Form1"
                    newDeclarationLine = newDeclarationLine.Replace("partial class", $"{accessModifier} partial class");
                    modified = true;
                }
            }

            // Add base type if it's missing
            if (!string.IsNullOrEmpty(mainFileBaseTypesList))
            {
                //string designerBaseList = designerCsFileMatch.Groups["baseList"].Value.Trim();

                if (string.IsNullOrEmpty(designerCsFileBaseTypesList))
                {
                    // Insert the base list after the class name
                    // Example: "partial class Form1" --> "partial class Form1 : Form" (or --> "partial class Form1 : System.Windows.Forms.Form")
                    string className = designerCsFileMatch.Groups["className"].Value;

                    // Look for the insertion point: after the class name and before a potential space
                    int insertIndex = newDeclarationLine.IndexOf(className, StringComparison.Ordinal);
                    if (insertIndex >= 0)
                    {
                        insertIndex += className.Length;
                        newDeclarationLine = newDeclarationLine.Insert(insertIndex, $" {mainFileBaseTypesList}");
                        modified = true;
                    }
                    else
                    {
                        // Fallback: simply append to the end of the declaration line
                        newDeclarationLine = newDeclarationLine.TrimEnd() + " " + mainFileBaseTypesList;
                        modified = true;
                    }
                }
            }

            // Replace the line in the code:
            if (modified)
            {
                // Replace only the first occurrence (expecting only one class)
                int startIndex = designerCsFileMatch.Index;
                int length = designerCsFileMatch.Length;

                return designerCsFileContent.Remove(startIndex, length).Insert(startIndex, newDeclarationLine);
            }

            return designerCsFileContent;
        }

        // Extracts 'using' directives (in the format "using X;" without extra spaces) in order of appearance
        private static List<string> ExtractUsingDirectives(string content)
        {
            var list = new List<string>();
            foreach (Match m in UsingRegex.Matches(content))
            {
                string u = m.Value.Trim(); // full line "using System.Windows.Forms;"
                if (!list.Contains(u, StringComparer.Ordinal))
                    list.Add(u);
            }
            return list;
        }

        // Inserts missing 'using' directives after the last 'using' directive, or at the beginning of the file.
        private static string InsertMissingUsings(string content, List<string> missingUsings)
        {
            if (missingUsings == null || missingUsings.Count == 0)
                return content;

            var allUsingsMatches = UsingRegex.Matches(content);
            if (allUsingsMatches.Count > 0)
            {
                // Insert after the last 'using' directive
                var last = allUsingsMatches[allUsingsMatches.Count - 1];
                int insertPos = last.Index + last.Length;
                string insertText = Environment.NewLine + string.Join(Environment.NewLine, missingUsings) + Environment.NewLine;
                return content.Insert(insertPos, insertText);
            }
            else
            {
                // Insert at the beginning of the file
                string insertText = string.Join(Environment.NewLine, missingUsings) + Environment.NewLine + Environment.NewLine;
                return insertText + content;
            }
        }
    }
}
