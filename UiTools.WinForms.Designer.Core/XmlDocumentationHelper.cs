using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.Versioning;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static UiTools.WinForms.Designer.Core.CommonStuff;

namespace UiTools.WinForms.Designer.Core
{
    public static class XmlDocumentationHelper
    {
        private static readonly ConcurrentDictionary<Assembly, WeakReference<XDocument>> assemblyXmlCache = new ConcurrentDictionary<Assembly, WeakReference<XDocument>>();
        private static readonly ConcurrentDictionary<Type, string> typeSummaryCache = new ConcurrentDictionary<Type, string>();
        private static readonly ConcurrentDictionary<Assembly, ResourceManager> resourceManagerCache = new ConcurrentDictionary<Assembly, ResourceManager>();
        private static readonly XDocument NotFoundSentinel = new XDocument();

        /// <summary>
        /// Returns a description for the specified type, first trying to find SRDescriptionAttribute,
        /// and then (on failure) falling back to XML documentation (<summary>).
        /// </summary>
        /// <param name="type">The type to get a description for.</param>
        /// <returns>A description string, or null if documentation is missing or errors occur.</returns>
        public static string GetTypeDescription(Type type)
        {
            ThrowIfNullOrEmpty(type);

            if (typeSummaryCache.TryGetValue(type, out string cachedDescription))
                return cachedDescription;

            string description;
            try
            {
                // Attempt to get description from SRDescriptionAttribute
                description = GetDescriptionFromSrAttribute(type);
                // If not found - fallback to XML documentation
                if (string.IsNullOrEmpty(description))
                    description = GetSummaryFromXml(type);
            }
            catch (Exception ex)
            {
                MessageLogger.LogError(typeof(XmlDocumentationHelper), $"Error getting description for type {type.FullName}: {ex.Message}", ex);
                description = null; // return null in case of error
            }

            typeSummaryCache.TryAdd(type, description);
            return description;
        }

        /// <summary>
        /// Tries to get the type description from SRDescriptionAttribute by reading the corresponding resources from the assembly.
        /// Takes into account that SRDescriptionAttribute is an internal type (uses reflection).
        /// </summary>
        /// <param name="type">The type to look for SRDescriptionAttribute.</param>
        /// <returns>A description string, or null if the attribute is not found or the resource could not be read.</returns>
        private static string GetDescriptionFromSrAttribute(Type type)
        {
            try
            {
                var srDescriptionAttributeData = type.GetCustomAttributesData()
                                                     .FirstOrDefault(ad => ad.AttributeType.Name == "SRDescriptionAttribute");
                if (srDescriptionAttributeData != null)
                {
                    // Get the argument passed to the constructor, as it is the resource name (e.g. "DescriptionButton").
                    if (srDescriptionAttributeData.ConstructorArguments.Count > 0)
                    {
                        var resourceKeyArg = srDescriptionAttributeData.ConstructorArguments[0];
                        if (resourceKeyArg.ArgumentType == typeof(string) && resourceKeyArg.Value is string resourceKey)
                        {
                            // Now that we have the resourceKey, we need to get the ResourceManager
                            // from the assembly where the System.Windows.Forms.Button (and SR) type is defined.
                            ResourceManager rm = GetResourceManagerForAssembly(type.Assembly);
                            if (rm != null)
                                return rm.GetString(resourceKey, CultureInfo.GetCultureInfo("en-US"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageLogger.LogError(typeof(XmlDocumentationHelper), $"Error reading SRDescriptionAttribute for type {type.FullName}: {ex.Message}", ex);
            }
            return null;
        }

        /// <summary>
        /// Tries to find a ResourceManager for the given assembly
        /// </summary>
        private static ResourceManager GetResourceManagerForAssembly(Assembly assembly)
        {
            if (resourceManagerCache.TryGetValue(assembly, out ResourceManager rm))
                return rm;

            try
            {
                string[] manifestResourceNames = assembly.GetManifestResourceNames();
                string assemblyName = assembly.GetName().Name;

                // The most reliable way is to find a resource that ends with ".resources"
                // and has a name similar to AssemblyName.SR or AssemblyName (for common resources)

                // First look for "AssemblyName.SR.resources" or "AssemblyName.SR.<culture>.resources"
                string baseNameWithSR = manifestResourceNames
                    .FirstOrDefault(name => name.StartsWith($"{assemblyName}.SR.", StringComparison.OrdinalIgnoreCase) &&
                                            name.EndsWith(".resources", StringComparison.OrdinalIgnoreCase))?
                    .Replace(".resources", ""); // remove extension and culture

                if (baseNameWithSR != null)
                {
                    // Example: "System.Windows.Forms.SR"
                    var newRm = new ResourceManager(baseNameWithSR, assembly);
                    resourceManagerCache.TryAdd(assembly, newRm);
                    return newRm;
                }

                // Fallback: If "AssemblyName.SR.resources" is not found, try just "AssemblyName.resources"
                // (less likely for WinForms, but possible for other components)
                string baseNameNoSR = manifestResourceNames
                    .FirstOrDefault(name => name.StartsWith($"{assemblyName}.", StringComparison.OrdinalIgnoreCase) &&
                                    name.EndsWith(".resources", StringComparison.OrdinalIgnoreCase))?
                    .Replace(".resources", "");

                if (baseNameNoSR != null && baseNameNoSR.Equals(assemblyName, StringComparison.OrdinalIgnoreCase)) // ensure that this is indeed the root resource
                {
                    var newRm = new ResourceManager(baseNameNoSR, assembly);
                    resourceManagerCache.TryAdd(assembly, newRm);
                    return newRm;
                }

                // Last fallback: explicitly for System.Windows.Forms.dll if all else failed
                if (assembly.GetName().Name.Equals("System.Windows.Forms", StringComparison.OrdinalIgnoreCase))
                {
                    var newRm = new ResourceManager("System.Windows.Forms.SR", assembly);
                    resourceManagerCache.TryAdd(assembly, newRm);
                    return newRm;
                }
            }
            catch (Exception ex)
            {
                MessageLogger.LogError(typeof(XmlDocumentationHelper), $"Error creating ResourceManager for assembly {assembly.FullName}: {ex.Message}", ex);
            }
            resourceManagerCache.TryAdd(assembly, null); // cache null
            return null;
        }

        /// <summary>
        /// Internal method to get summary from XML documentation.
        /// </summary>
        private static string GetSummaryFromXml(Type type)
        {
            try
            {
                Assembly assembly = type.Assembly;
                XDocument xmlDoc = GetAssemblyXmlDocument(assembly);

                if (xmlDoc == null)
                    return null;

                string memberId = GetXmlMemberId(type);
                var memberNode = xmlDoc.Root?.Element("members")?
                                             .Elements("member")
                                             .FirstOrDefault(m => m.Attribute("name")?.Value == memberId);
                if (memberNode != null)
                {
                    var summaryNode = memberNode.Element("summary");
                    if (summaryNode != null)
                    {
                        var sb = new StringBuilder();
                        foreach (var node in summaryNode.Nodes())
                        {
                            if (node is XText textNode)
                                sb.Append(textNode.Value);
                            else if (node is XElement element)
                            {
                                if (element.Name == "see" || element.Name == "seealso")
                                {
                                    string cref = element.Attribute("cref")?.Value ?? "";
                                    int colonIndex = cref.IndexOf(':');
                                    sb.Append(colonIndex != -1 ? cref.Substring(colonIndex + 1) : cref);
                                }
                                else if (element.Name == "paramref" || element.Name == "typeparamref")
                                {
                                    sb.Append(element.Attribute("name")?.Value ?? "");
                                }
                                else
                                {
                                    sb.Append(element.Value);
                                }
                            }
                        }
                        var summary = sb.ToString();
                        summary = Regex.Replace(summary, @"\s+", " ").Trim();
                        return summary;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageLogger.LogError(typeof(XmlDocumentationHelper), $"Error getting XML documentation for type {type.FullName}: {ex.Message}", ex);
            }
            return null;
        }

        private static XDocument GetAssemblyXmlDocument(Assembly assembly)
        {
            if (assembly == null)
                return null;

            if (assemblyXmlCache.TryGetValue(assembly, out WeakReference<XDocument> weakRef) && weakRef.TryGetTarget(out XDocument cachedDoc))
                return cachedDoc == NotFoundSentinel ? null : cachedDoc;

            string xmlDocPath = FindXmlDocumentationPath(assembly);
            if (xmlDocPath != null)
                return LoadXmlDocumentAndCache(assembly, xmlDocPath);

            assemblyXmlCache.TryAdd(assembly, new WeakReference<XDocument>(NotFoundSentinel));
            return null;
        }

        /// <summary>
        /// Finds the path to the XML documentation file for the given assembly.
        /// This is the most complex part, requiring adaptation for different frameworks and environments.
        /// </summary>
        private static string FindXmlDocumentationPath(Assembly assembly)
        {
            // 1. Try to find the XML next to the DLL
            if (!string.IsNullOrEmpty(assembly.Location) && File.Exists(assembly.Location))
            {
                string localXmlPath = Path.ChangeExtension(assembly.Location, ".xml");
                if (File.Exists(localXmlPath))
                    return localXmlPath;
            }

            // 2. For .NET Framework system assemblies (including GAC)
            string netFxXmlPath = GetDotNetFrameworkAssemblyXmlPath(assembly);
            if (netFxXmlPath != null)
                return netFxXmlPath;

            // 3. For .NET (Core/5+/6+/8+) system assemblies
            string netCoreXmlPath = GetDotNetCoreAssemblyXmlPath(assembly);
            if (netCoreXmlPath != null)
                return netCoreXmlPath;

            // 4. Fallback: Search in the current application domain
            // (may be useful for assemblies loaded from other locations, or when Assembly.Location is unavailable)
            // For GAC assemblies, Assembly.Location may be empty
            if (string.IsNullOrEmpty(assembly.Location))
            {
                var appDomainBase = AppDomain.CurrentDomain.BaseDirectory;
                string potentialPath = Path.Combine(appDomainBase, assembly.GetName().Name + ".xml");
                if (File.Exists(potentialPath))
                    return potentialPath;
            }

            return null; // XML file not found
        }

        private static XDocument LoadXmlDocumentAndCache(Assembly assembly, string path)
        {
            try
            {
                XDocument doc = XDocument.Load(path);
                assemblyXmlCache.TryAdd(assembly, new WeakReference<XDocument>(doc));
                return doc;
            }
            catch (Exception ex)
            {
                MessageLogger.LogError(typeof(XmlDocumentationHelper), $"Error loading XML documentation from {path}: {ex.Message}", ex);
                assemblyXmlCache.TryAdd(assembly, new WeakReference<XDocument>(NotFoundSentinel));
                return null;
            }
        }

        /// <summary>
        /// Generates XML member ID for a type (T:FullTypeName).
        /// </summary>
        private static string GetXmlMemberId(Type type)
        {
            string typeFullName = type.FullName;
            if (type.IsGenericType)
            {
                // For Generic types, the XML-ID uses `1, `2, etc.
                // For example: System.Collections.Generic.List`1
                typeFullName = type.GetGenericTypeDefinition().FullName;
                typeFullName = typeFullName.Substring(0, typeFullName.IndexOf('`')); // remove `1
                typeFullName += "`" + type.GetGenericArguments().Length;
            }
            // Nested types use '+' in Type.FullName, but '.' in XML
            typeFullName = typeFullName.Replace('+', '.');
            return $"T:{typeFullName}";
        }

        /// <summary>
        /// Tries to find XML documentation for .NET Framework system assemblies (GAC).
        /// </summary>
        private static string GetDotNetFrameworkAssemblyXmlPath(Assembly assembly)
        {
            if (assembly.IsDynamic || string.IsNullOrEmpty(assembly.Location)) return null;

            // Determine if the assembly is a .NET Framework system assembly
            bool isNetFxSystemAssembly = assembly.GlobalAssemblyCache ||
                                        assembly.FullName.StartsWith("System.") ||
                                        assembly.FullName.StartsWith("Microsoft.") && !assembly.FullName.Contains("VisualStudio"); // exclude VS-specific ones
            if (!isNetFxSystemAssembly)
                return null;

            // Get the assembly name without extension
            string assemblyFileName = Path.GetFileNameWithoutExtension(assembly.Location);

            // Search in Reference Assemblies and Framework Directories
            List<string> searchPaths = new List<string>();

            // 1. Reference Assemblies (for most .NET 4.x)
            string programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            if (!string.IsNullOrEmpty(programFilesX86))
            {
                // Approximate paths for .NET Framework versions
                string[] netFxVersions = new[] { "v4.0", "v4.0.30319", "v4.5", "v4.5.1", "v4.5.2", "v4.6", "v4.6.1", "v4.6.2", "v4.7", "v4.7.1", "v4.7.2", "v4.8" };
                foreach (var version in netFxVersions.Reverse()) // start with newer versions
                    searchPaths.Add(Path.Combine(programFilesX86, @"Reference Assemblies\Microsoft\Framework\.NETFramework\", version));
            }

            // 2. Standard Framework directories (C:\Windows\Microsoft.NET\Framework)
            string windowsPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            if (!string.IsNullOrEmpty(windowsPath))
            {
                string[] netFrameworkDirs = Directory.GetDirectories(Path.Combine(windowsPath, @"Microsoft.NET\Framework"), "v*", SearchOption.TopDirectoryOnly);
                string[] netFramework64Dirs = Directory.GetDirectories(Path.Combine(windowsPath, @"Microsoft.NET\Framework64"), "v*", SearchOption.TopDirectoryOnly);

                searchPaths.AddRange(netFrameworkDirs.OrderByDescending(d => d));
                searchPaths.AddRange(netFramework64Dirs.OrderByDescending(d => d));
            }

            foreach (string path in searchPaths.Distinct()) // remove duplicates
            {
                if (Directory.Exists(path))
                {
                    string xmlDocPath = Path.Combine(path, assemblyFileName + ".xml");
                    if (File.Exists(xmlDocPath))
                        return xmlDocPath;
                }
            }
            return null;
        }

        /// <summary>
        /// Tries to find XML documentation for .NET (Core/5+/...) system assemblies.
        /// </summary>
        private static string GetDotNetCoreAssemblyXmlPath(Assembly assembly)
        {
            if (assembly.IsDynamic || string.IsNullOrEmpty(assembly.Location))
                return null;

            // If this is not a .NET Framework assembly
            if (IsNetFrameworkRuntime(assembly))
                return null;

            // Look in SDK folders
            string sdkPath = Environment.GetEnvironmentVariable("DOTNET_ROOT");
            if (string.IsNullOrEmpty(sdkPath) || !Directory.Exists(sdkPath))
            {
                // Try to get dotnet.exe path and find the SDK root
                try
                {
                    string dotnetExePath = DotNetExeResolver.ResolveDotNetExePath();
                    if (!string.IsNullOrEmpty(dotnetExePath))
                    {
                        sdkPath = Path.GetDirectoryName(dotnetExePath); // .../dotnet
                    }
                }
                catch { /* ignore */ }

                if (string.IsNullOrEmpty(sdkPath) || !Directory.Exists(sdkPath))
                {
                    // Last attempt, standard path
                    sdkPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet");
                }
            }

            if (string.IsNullOrEmpty(sdkPath) || !Directory.Exists(sdkPath))
                return null;

            string assemblyName = assembly.GetName().Name;

            // 1. In "shared" folders (runtime assemblies)
            // Example: C:\Program Files\dotnet\shared\Microsoft.NETCore.App\8.0.0\System.Private.CoreLib.xml
            //          ^sdkPath               ^shared ^framework            ^version
            var sharedPath = Path.Combine(sdkPath, "shared");
            if (Directory.Exists(sharedPath))
            {
                foreach (var frameworkDir in Directory.GetDirectories(sharedPath))
                {
                    foreach (var versionDir in Directory.GetDirectories(frameworkDir))
                    {
                        string xmlPath = Path.Combine(versionDir, assemblyName + ".xml");
                        if (File.Exists(xmlPath))
                            return xmlPath;
                    }
                }
            }

            // 2. In "packs" folders (reference assemblies)
            // Example: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.0\ref\net8.0\System.Runtime.xml
            //          ^sdkPath               ^packs ^packName                 ^packVersion ^ref ^tfm
            var packsPath = Path.Combine(sdkPath, "packs");
            if (Directory.Exists(packsPath))
            {
                foreach (var packDir in Directory.GetDirectories(packsPath))
                {
                    foreach (var packVersionDir in Directory.GetDirectories(packDir))
                    {
                        // Search in 'ref' and 'lib' folders
                        foreach (var subDir in new[] { "ref", "lib" })
                        {
                            string refLibPath = Path.Combine(packVersionDir, subDir);
                            if (Directory.Exists(refLibPath))
                            {
                                foreach (var tfmDir in Directory.GetDirectories(refLibPath))
                                {
                                    string xmlPath = Path.Combine(tfmDir, assemblyName + ".xml");
                                    if (File.Exists(xmlPath))
                                        return xmlPath;
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Checks if the assembly is part of the .NET Framework Runtime.
        /// </summary>
        private static bool IsNetFrameworkRuntime(Assembly assembly)
        {
            // The simplest way: check by TargetFrameworkAttribute
            var targetFrameworkAttribute = assembly.GetCustomAttribute<TargetFrameworkAttribute>();
            if (targetFrameworkAttribute != null)
            {
                var frameworkName = new FrameworkName(targetFrameworkAttribute.FrameworkName);
                return frameworkName.Identifier == ".NETFramework";
            }

            // Fallback: by location
            string location = assembly.Location;
            return !string.IsNullOrEmpty(location) &&
                   (location.Contains(@"Microsoft.NET\Framework") ||
                    location.Contains(@"Windows\Microsoft.NET\Framework"));
        }
    }

    // Helper class for finding dotnet.exe if DOTNET_ROOT is not set
    internal static class DotNetExeResolver
    {
        private static string dotnetExePath;

        public static string ResolveDotNetExePath()
        {
            if (dotnetExePath != null) return dotnetExePath;

            // Try from the PATH environment variable
            string pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (!string.IsNullOrEmpty(pathEnv))
            {
                foreach (var path in pathEnv.Split(Path.PathSeparator))
                {
                    string fullPath = Path.Combine(path.Trim(), "dotnet.exe");
                    if (File.Exists(fullPath))
                    {
                        dotnetExePath = fullPath;
                        return fullPath;
                    }
                }
            }

            // If not found in PATH, try the standard 'Program Files' location
            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string defaultPath = Path.Combine(programFiles, "dotnet", "dotnet.exe");
            if (File.Exists(defaultPath))
            {
                dotnetExePath = defaultPath;
                return defaultPath;
            }

            return null; // not found
        }
    }
}
