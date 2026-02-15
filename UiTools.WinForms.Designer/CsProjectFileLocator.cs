using System.IO;
using UiTools.WinForms.Designer.Core;

namespace UiTools.WinForms.Designer
{
    public static class CsProjectFileLocator
    {
        /// <summary>
        /// Locates the .csproj file associated with the given .cs file.
        /// </summary>
        /// <param name="designerCsFilePath">The full path to a .designer.cs file.</param>
        /// <returns>The full path to the .csproj file, or null if a matching file cannot be found.</returns>
        public static string FindContainingCsProjFile(string designerCsFilePath)
        {
            if (string.IsNullOrEmpty(designerCsFilePath))
                return null;

            string currentDir = Path.GetDirectoryName(designerCsFilePath);

            // Traverse up the directory tree to the disk root
            while (currentDir != null)
            {
                // Look for all .csproj files in the current directory
                string[] csprojFiles = Directory.GetFiles(currentDir, "*.csproj");

                foreach (string projPath in csprojFiles)
                {
                    if (IsFileInProject(projPath, designerCsFilePath))
                        return projPath;
                }

                currentDir = Path.GetDirectoryName(currentDir);
            }

            return null;
        }

        private static bool IsFileInProject(string csprojPath, string csFilePath)
        {
            return new CsProjectFileWrapper(csprojPath).IsFileInProject(csFilePath);
        }
    }
}
