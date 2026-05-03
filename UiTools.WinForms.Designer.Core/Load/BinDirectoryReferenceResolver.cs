using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace UiTools.WinForms.Designer.Core
{
    /// <summary>
    /// A simple implementation of a bin directory reference resolver with the ability to load additional references from a file/list.
    /// This is the simplest way to assemble a list of AssemblyName objects to pass to MyTypeResolutionService.
    /// </summary>
    public class BinDirectoryReferenceResolver
    {
        /// <summary>
        /// Collects AssemblyName objects from the target project's bin directory + adds optional additional references.
        /// The path to the bin directory is automatically calculated based on projectFilePath, configuration, and platform.
        /// </summary>
        /// <param name="dfContext">Context containing project file path, namespace, configuration, platform, etc.</param>
        /// <param name="searchRecursively">Whether to scan subfolders within the bin directory recursively.</param>
        /// <param name="additionalAssemblyNames">Optional AssemblyNames that must be included.</param>
        /// <returns>Unique AssemblyName objects suitable for ReferenceAssembly method in MyTypeResolutionService.</returns>
        public IEnumerable<AssemblyName> GetReferencedAssembliesFromProjectBin(
            DesignerCsFileContext dfContext,
            bool searchRecursively = true,
            IEnumerable<AssemblyName> additionalAssemblyNames = null)
        {
            var result = new List<AssemblyName>();

            if (string.IsNullOrEmpty(dfContext.CsProjectFileFullPath) || !File.Exists(dfContext.CsProjectFileFullPath) ||
                string.IsNullOrEmpty(dfContext.Configuration) || string.IsNullOrEmpty(dfContext.Platform))
            {
                string csProjFileStatus = string.IsNullOrEmpty(dfContext.CsProjectFileFullPath)
                    ? "is not specified"
                    : $"does not exist at '{dfContext.CsProjectFileFullPath}'";
                MessageLogger.Log(this, $"Cannot determine bin directory: project file {csProjFileStatus}, and/or Configuration/Platform parameters are empty.");
                if (additionalAssemblyNames != null)
                    result.AddRange(additionalAssemblyNames.Where(x => x != null));
                if (!string.IsNullOrEmpty(dfContext.ExtraAssembliesFileFullPath) && File.Exists(dfContext.ExtraAssembliesFileFullPath))
                    result.AddRange(ParseExtraAssembliesFile(dfContext.ExtraAssembliesFileFullPath));
                return DeduplicateAssemblyNames(result);
            }

            // 1. Determine the path to the bin directory (for SDK-style projects, the platform is usually ignored in the bin path)
            string binDir = dfContext.CsProjectFileWrapper.DetermineBinDirectory(dfContext.Configuration, dfContext.Platform);
            if (string.IsNullOrEmpty(binDir) || !Directory.Exists(binDir))
            {
                MessageLogger.LogWarning(this, $"Bin directory '{binDir}' not found. " +
                    "Probably the project was not compiled or OutputPath was set up in a non-standard way.");
                // If the bin directory is not found, return only the additional assemblies (if any)
                if (additionalAssemblyNames != null)
                    result.AddRange(additionalAssemblyNames.Where(x => x != null));
                if (!string.IsNullOrEmpty(dfContext.ExtraAssembliesFileFullPath) && File.Exists(dfContext.ExtraAssembliesFileFullPath))
                    result.AddRange(ParseExtraAssembliesFile(dfContext.ExtraAssembliesFileFullPath));
                return DeduplicateAssemblyNames(result);
            }

            MessageLogger.Log(this, $"Scanning bin directory: '{binDir}'...");

            var searchOption = searchRecursively ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var files = Directory.EnumerateFiles(binDir, "*.dll", searchOption)
                                 .Concat(Directory.EnumerateFiles(binDir, "*.exe", searchOption))
                                 .Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            if (files.Any())
                MessageLogger.Log(this, $"Found {files.Count} .dll/.exe files in bin directory '{binDir}'.");
            else
                MessageLogger.LogWarning(this, $"No .dll or .exe files found in bin directory '{binDir}'. " +
                    "Probably the project was not compiled or OutputPath was set up in a non-standard way.");

            foreach (var file in files)
            {
                try
                {
                    // Get AssemblyName without loading the assembly.
                    var an = AssemblyName.GetAssemblyName(file);
                    result.Add(an);
                }
                catch (BadImageFormatException)
                {
                    // This is not a .NET assembly (or an unsupported architecture) - skip it
                    MessageLogger.LogVerbose(this, $"Skipped file as it is not a .NET assembly: {file}");
                }
                catch (Exception ex)
                {
                    MessageLogger.LogError(this, $"Error reading AssemblyName from file '{file}': {ex.Message}", ex);
                }
            }

            // Add additional AssemblyNames, if provided
            if (additionalAssemblyNames != null)
                result.AddRange(additionalAssemblyNames.Where(x => x != null));

            // If a file with assembly specifications is provided - parse and add them
            if (!string.IsNullOrEmpty(dfContext.ExtraAssembliesFileFullPath) && File.Exists(dfContext.ExtraAssembliesFileFullPath))
                result.AddRange(ParseExtraAssembliesFile(dfContext.ExtraAssembliesFileFullPath));

            var currentResolved = DeduplicateAssemblyNames(result).ToList();

            // For names where the file is not present in the bin directory, try to find them in Reference Assemblies
            // (Program Files (x86)\Reference Assemblies\...). This might help with system assemblies that are not copied
            // to the output by default but are needed for deserialization.
            var assembliesToCheckForReferencePaths = currentResolved
                .Where(a => !IsAssemblyFilePresentInDirectory(a, binDir))
                .ToArray(); // check only those not found in the bin directory.

            foreach (var an in assembliesToCheckForReferencePaths)
            {
                // Avoid re-searching for mscorlib/System.Private.CoreLib and other core assemblies
                if (IsCoreRuntimeAssembly(an.Name))
                    continue;

                try
                {
                    var foundPath = TryFindInReferenceAssemblies(an.Name);
                    if (!string.IsNullOrEmpty(foundPath))
                    {
                        try
                        {
                            var anFromPath = AssemblyName.GetAssemblyName(foundPath);
                            // If the name matches according to ReferenceMatchesDefinition - replace or add
                            if (!currentResolved.Any(r => AssemblyName.ReferenceMatchesDefinition(r, anFromPath)))
                            {
                                currentResolved.Add(anFromPath);
                                MessageLogger.Log(this, $"Added system assembly from 'Reference Assemblies' folder: {anFromPath.FullName} ('{foundPath}')");
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageLogger.LogError(this, $"Failed to get AssemblyName from file '{foundPath}': {ex.Message}", ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageLogger.LogError(this, $"Error while searching within 'Reference Assemblies' folder for assembly name '{an.Name}': {ex.Message}", ex);
                }
            }

            return DeduplicateAssemblyNames(currentResolved);
        }

        /// <summary>
        /// Checks if the assembly is one of the core runtime assemblies (mscorlib, System.Private.CoreLib).
        /// </summary>
        private bool IsCoreRuntimeAssembly(string assemblySimpleName)
        {
            return assemblySimpleName.Equals("mscorlib", StringComparison.OrdinalIgnoreCase) ||
                   assemblySimpleName.Equals("System.Private.CoreLib", StringComparison.OrdinalIgnoreCase) ||
                   assemblySimpleName.Equals("System", StringComparison.OrdinalIgnoreCase); // Added System.dll as it's fundamental.
        }

        /// <summary>
        /// Parses a text file of additional specifications.
        /// Each line can be:
        /// - A full path to a .dll file
        /// - A full assembly name
        /// - A simple assembly name ("System.Windows.Forms")
        /// Commented lines (starting with # or //) are skipped.
        /// </summary>
        private IEnumerable<AssemblyName> ParseExtraAssembliesFile(string extraAssembliesFilePath)
        {
            var list = new List<AssemblyName>();
            foreach (var rawLine in File.ReadAllLines(extraAssembliesFilePath))
            {
                var line = rawLine?.Trim();
                if (string.IsNullOrEmpty(line)) continue;
                if (line.StartsWith("#") || line.StartsWith("//")) continue;

                // If it's a file path - use AssemblyName.GetAssemblyName
                if (Path.IsPathRooted(line) && File.Exists(line))
                {
                    try
                    {
                        var an = AssemblyName.GetAssemblyName(line);
                        list.Add(an);
                        continue;
                    }
                    catch (Exception ex)
                    {
                        MessageLogger.LogError(this, $"Couldn't read AssemblyName from file '{line}': {ex.Message}", ex);
                        continue;
                    }
                }

                // Try to parse as full name (if it contains a comma), otherwise assume it's a simple name
                if (line.Contains(","))
                {
                    try
                    {
                        var an = new AssemblyName(line);
                        list.Add(an);
                    }
                    catch (Exception ex)
                    {
                        MessageLogger.LogError(this, $"Couldn't parse string as AssemblyName ('{line}'): {ex.Message}", ex);
                    }
                }
                else
                {
                    // Simple assembly name - create an AssemblyName with this name
                    try
                    {
                        var an = new AssemblyName(line);
                        list.Add(an);
                    }
                    catch (Exception ex)
                    {
                        MessageLogger.LogError(this, $"Failed to create AssemblyName from simple name ('{line}'): {ex.Message}", ex);
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Checks if an assembly file with this name exists in the specified directory (including subfolders).
        /// Required to determine if searching reference assemblies is necessary.
        /// </summary>
        private bool IsAssemblyFilePresentInDirectory(AssemblyName an, string dir)
        {
            if (an == null || string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
                return false;
            // By filename (Name.dll)
            var expected = an.Name + ".dll";
            try
            {
                var found = Directory.EnumerateFiles(dir, expected, SearchOption.AllDirectories).Any();
                return found;
            }
            catch
            {
                return false; // ignore exceptions like UnauthorizedAccess
            }
        }

        /// <summary>
        /// Attempts to find assemblyName.dll in the standard .NET Framework Reference Assemblies.
        /// Returns the file path if found, otherwise null.
        /// This is NOT a universal mechanism (target frameworks and versions may vary), but it's useful for system assemblies.
        /// </summary>
        private string TryFindInReferenceAssemblies(string assemblySimpleName)
        {
            if (string.IsNullOrEmpty(assemblySimpleName))
                return null;

            var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            if (string.IsNullOrEmpty(programFilesX86))
                return null;

            // Standard path: Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\{vX.Y}
            var basePath = Path.Combine(programFilesX86, "Reference Assemblies", "Microsoft", "Framework", ".NETFramework");
            if (!Directory.Exists(basePath))
                return null;

            // Search in subdirectories (versions) for the file assemblySimpleName.dll
            try
            {
                var candidates = Directory.EnumerateDirectories(basePath)
                    .OrderByDescending(d =>
                    {
                        // Attempt to parse the folder name as a version for more precise ordering
                        Version v;
                        if (Version.TryParse(Path.GetFileName(d).TrimStart('v'), out v))
                            return v;
                        return new Version(0, 0); // unknown version at the end of the list
                    })
                    .Select(dir => Path.Combine(dir, assemblySimpleName + ".dll"));

                foreach (var candidate in candidates)
                {
                    if (File.Exists(candidate))
                        return candidate;
                }
            }
            catch (Exception ex)
            {
                MessageLogger.LogError(this, $"Error while searching within 'Reference Assemblies' folder for assembly simple name '{assemblySimpleName}': {ex.Message}", ex);
            }

            return null;
        }

        /// <summary>
        /// Removes duplicate AssemblyName objects based on ReferenceMatchesDefinition semantics (name + version + token, etc.).
        /// Returns an IEnumerable in the order in which the AssemblyName was first encountered.
        /// </summary>
        private IEnumerable<AssemblyName> DeduplicateAssemblyNames(IEnumerable<AssemblyName> list)
        {
            var result = new List<AssemblyName>();
            foreach (var an in list)
            {
                if (an == null)
                    continue;
                if (!result.Any(r => AssemblyName.ReferenceMatchesDefinition(r, an)))
                    result.Add(an);
            }
            return result;
        }
    }
}
