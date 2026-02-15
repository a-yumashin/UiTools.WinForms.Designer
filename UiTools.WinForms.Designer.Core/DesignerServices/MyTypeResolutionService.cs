using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;

namespace UiTools.WinForms.Designer.Core
{
    /// <summary>
    /// Allows to retrieve an assembly or type by name.
    /// </summary>
    public class MyTypeResolutionService : ITypeResolutionService
    {
        private static readonly Dictionary<string, string> keywordToFullTypeNameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "bool", "System.Boolean" },
            { "byte", "System.Byte" },
            { "char", "System.Char" },
            { "decimal", "System.Decimal" },
            { "double", "System.Double" },
            { "float", "System.Single" }, // float -> System.Single
            { "int", "System.Int32" },
            { "long", "System.Int64" },
            { "object", "System.Object" },
            { "sbyte", "System.SByte" },
            { "short", "System.Int16" },
            { "string", "System.String" },
            { "uint", "System.UInt32" },
            { "ulong", "System.UInt64" },
            { "ushort", "System.UInt16" },
            { "void", "System.Void" } // "void" is not typically passed here as a type, but added for completeness.
        };

        private readonly List<AssemblyName> assemblyNames = new List<AssemblyName>();
        private readonly ConcurrentDictionary<string, Type> typeCache = new ConcurrentDictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        private readonly List<string> usings;
        private readonly string currentNamespace; // The namespace of the currently deserialized class.

        public MyTypeResolutionService(IEnumerable<string> usings, string currentNamespace)
        {
            this.usings = usings?.ToList() ?? new List<string>();
            this.currentNamespace = currentNamespace;
        }

        public static MyTypeResolutionService CreateWithReferencedAssemblies(IEnumerable<string> usings, DesignerCsFileContext dfContext)
        {
            var trs = new MyTypeResolutionService(usings, dfContext.Namespace);

            var keyAssemblies = new AssemblyName[] // Key assemblies that need to be added but are not in the bin directory, as they reside in the GAC.
            {
                typeof(object).Assembly.GetName(), // mscorlib.dll/System.Private.CoreLib.dll
                typeof(System.Drawing.Point).Assembly.GetName(), // System.Drawing.dll
				typeof(System.Windows.Forms.Form).Assembly.GetName(), // System.Windows.Forms.dll
				typeof(System.ComponentModel.Component).Assembly.GetName(), // System.dll
				typeof(System.Diagnostics.Design.LogConverter).Assembly.GetName() // System.Design.dll
            };
            var binDirectoryReferenceResolver = new BinDirectoryReferenceResolver();
            var referencedAssemblies = binDirectoryReferenceResolver.GetReferencedAssembliesFromProjectBin(
                dfContext,
                searchRecursively: true,
                additionalAssemblyNames: keyAssemblies);

            foreach (var asmName in referencedAssemblies)
            {
                trs.ReferenceAssembly(asmName);
            }
            return trs;
        }

        public IEnumerable<AssemblyName> GetKnownAssemblyNames() => assemblyNames;

        #region ITypeResolutionService implementation

        /// <summary>
        /// Gets the requested assembly.
        /// </summary>
        /// <param name="name">The name of the assembly to retrieve.</param>
        /// <returns>An instance of the requested assembly, or null if no assembly can be located.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public Assembly GetAssembly(AssemblyName name)
        {
            if (name == null)
            {
                var errMsg = "assembly name cannot be null";
                MessageLogger.LogError(this, errMsg);
                throw new ArgumentNullException(nameof(name), $"{nameof(MyTypeResolutionService)}.{nameof(GetAssembly)}(): {errMsg}");
            }

            lock (assemblyNames)
            {
                var asmName = assemblyNames.FirstOrDefault(a => AssemblyName.ReferenceMatchesDefinition(a, name));
                if (asmName == null)
                    return null;
                var loadedAssembly = GetLoadedAssembly(asmName);
                return loadedAssembly == null
                    ? Assembly.Load(asmName)
                    : loadedAssembly;
            }
            // For extra robustness, one could subscribe to the AppDomain.CurrentDomain.AssemblyResolve event.
            // Inside the event handler, the assembly could be loaded via Assembly.LoadFrom(<assembly_file_full_path>).
            // This would require modifying BinDirectoryReferenceResolver to return a pair of AssemblyName + <assembly_file_full_path>
            // instead of just AssemblyName. However, I have not yet encountered a situation where Assembly.Load()
            // cannot resolve an assembly.
        }

        /// <summary>
        /// Gets the requested assembly.
        /// </summary>
        /// <param name="name">The name of the assembly to retrieve.</param>
        /// <param name="throwOnError">true if this method should throw an exception if the assembly cannot be located; otherwise, false, and this method returns null if the assembly cannot be located.</param>
        /// <returns>An instance of the requested assembly, or null if no assembly can be located.</returns>
        /// <exception cref="Exception"></exception>
        public Assembly GetAssembly(AssemblyName name, bool throwOnError)
        {
            var asm = GetAssembly(name);
            if (asm == null)
            {
                var errMsg = $"{nameof(MyTypeResolutionService)}.{nameof(GetAssembly)}(): AssemblyName '{name}' not found among referenced assemblies";
                MessageLogger.LogError(this, errMsg);
                if (throwOnError)
                    throw new InvalidOperationException(errMsg);
            }
            return asm;
        }

        /// <summary>
        /// Gets the path to the file from which the assembly was loaded.
        /// </summary>
        /// <param name="name">The name of the assembly.</param>
        /// <returns>The path to the file from which the assembly was loaded.</returns>
        public string GetPathOfAssembly(AssemblyName name)
        {
            var asm = GetAssembly(name);
            return asm?.Location;
        }

        /// <summary>
        /// Loads a type with the specified name.
        /// </summary>
        /// <param name="name">The name of the type. If the type name is not a fully qualified name that indicates an assembly, this service will search its internal set of referenced assemblies.</param>
        /// <returns>An instance of System.Type that corresponds to the specified name, or null if no type can be found.</returns>
        public Type GetType(string name)
        {
            return GetType(name, throwOnError: false);
        }

        /// <summary>
        /// Loads a type with the specified name.
        /// </summary>
        /// <param name="name">The name of the type. If the type name is not a fully qualified name that indicates an assembly, this service will search its internal set of referenced assemblies.</param>
        /// <param name="throwOnError">true if this method should throw an exception if the assembly cannot be located; otherwise, false, and this method returns null if the assembly cannot be located.</param>
        /// <returns>An instance of System.Type that corresponds to the specified name, or null if no type can be found.</returns>
        public Type GetType(string name, bool throwOnError)
        {
            return GetType(name, throwOnError, ignoreCase: false);
        }

        /// <summary>
        /// Loads a type with the specified name.
        /// </summary>
        /// <param name="name">The name of the type. If the type name is not a fully qualified name that indicates an assembly, this service will search its internal set of referenced assemblies.</param>
        /// <param name="throwOnError">true if this method should throw an exception if the assembly cannot be located; otherwise, false, and this method returns null if the assembly cannot be located.</param>
        /// <param name="ignoreCase">true to ignore case when searching for types; otherwise, false.</param>
        /// <returns>An instance of System.Type that corresponds to the specified name, or null if no type can be found.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="TypeLoadException"></exception>
        public Type GetType(string name, bool throwOnError, bool ignoreCase)
        {
            var realType = GetTypeInternal(name, throwOnError, ignoreCase);
            return realType != null && IsStronglyTypedResourceClass(realType)
                ? new InternalAccessibleType(realType) // Returns a proxy that allows access to internal members (a must-have for project-level resources).
                : realType;
        }

        private Type GetTypeInternal(string name, bool throwOnError, bool ignoreCase)
        {
            //System.Diagnostics.Debug.WriteLine("##### " + name);

            string errMsg;
            if (string.IsNullOrEmpty(name))
            {
                errMsg = "Type name cannot be null";
                MessageLogger.LogError(this, errMsg);
                if (throwOnError)
                    throw new ArgumentNullException(nameof(name), $"{nameof(MyTypeResolutionService)}.{nameof(GetType)}: {errMsg}");
                return null;
            }

            // Normalize C# keywords (object, int, ...) to full type names (System.Object, System.Int32, ...):
            string nameForResolution = name.Replace("global::", "");
            if (keywordToFullTypeNameMap.TryGetValue(name, out string fullTypeNameFromKeyword))
            {
                nameForResolution = fullTypeNameFromKeyword;
            }

            // 1. Quick cache check:
            if (typeCache.TryGetValue(name, out var cached))
                return cached;

            if (IsProbablyAQN(nameForResolution))
            {
                // 2. Looks like an AQN (contains a comma), but this could be either a "full" AQN (e.g. "System.Windows.Forms.Button, System.Windows.Forms, Version=4.0.0.0,
                // Culture=neutral, PublicKeyToken=b77a5c561934e089") or a "partial" AQN (e.g. "System.Windows.Forms.Design.ControlDesigner, System.Design" - a common
                // case for the DesignerAttribute of some control).
                if (IsProbablyFullAQN(nameForResolution))
                {
                    // Looks like a "full" AQN - "System.Windows.Forms.Button, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089".
                    // 2a. Try Type.GetType() (this method primarily expects a "full" AQN):
                    try
                    {
                        var t = GetTypeIncludingNested(nameForResolution, ignoreCase: ignoreCase);
                        if (t != null)
                        {
                            typeCache[name] = t;
                            return t;
                        }
                        MessageLogger.LogError(this, $"no type found by the provided AQN ({name})");
                        return null; // In the case of a "full" AQN - no fallback!
                    }
                    catch (Exception ex)
                    {
                        MessageLogger.LogError(this, $"exception fired while getting type by the provided AQN ({name}): {ex.Message}", ex);
                        if (throwOnError) throw;
                        return null;
                    }
                }
                else if (IsProbablyPartialAQN(nameForResolution))
                {
                    // Looks like a "partial" AQN - "System.Windows.Forms.Design.ControlDesigner, System.Design".
                    // 2b. Search for the assembly among those already loaded, then search for the type within it:
                    var parts = nameForResolution.Split(new[] { ',' });
                    string typeFullName = parts[0].Trim();
                    string assemblySimpleOrPartialName = parts[1].Trim();
                    var asmName = assemblyNames.FirstOrDefault(an => an.Name == assemblySimpleOrPartialName || an.FullName == assemblySimpleOrPartialName);
                    if (asmName == null)
                    {
                        MessageLogger.LogError(this, $"no assembly found by the provided name ({assemblySimpleOrPartialName}); " +
                            $"type name being processed: '{name}'");
                        return null;
                    }
                    var asm = GetAssembly(asmName, throwOnError: false);
                    if (asm != null)
                    {
                        var t = asm.GetTypeIncludingNested(typeFullName, ignoreCase);
                        if (t != null)
                        {
                            typeCache[name] = t;
                            return t;
                        }
                        MessageLogger.LogError(this, $"no type found in assembly '{asmName}' by the provided name ({typeFullName})");
                        return null;
                    }
                    return null; // LogError() was already called from GetAssembly()
                }
                else
                {
                    MessageLogger.LogError(this, $"Unexpected type name: '{name}' (looks like neither full AQN nor partial AQN)");
                    return null;
                }
            }
            else
            {
                // This is not an AQN; it's either a full name (e.g. "System.Windows.Forms.Button") or a short name (e.g. "Form", without namespace).
                // A "partially qualified" name like "MyControls.MyUserControl" is also possible, where the "MyControls" namespace actually lives within "WindowsFormsApp1",
                // but since the form itself lives within the same "WindowsFormsApp1" namespace, Visual Studio (or another IDE) generates the control declaration as:
                // "private MyControls.MyUserControl myUserControl1;" - instead of the full version "private WindowsFormsApp1.MyControls.MyUserControl myUserControl1;".
                // However, SYNTACTICALLY distinguishing such a "partially qualified" name from a "true" full name is impossible without parsing the code of all UserControls
                // in the project, i.e. without a full SEMANTIC analysis of the project (performed, for example, by Roslyn for the compiler).
                // It's also important to remember that "WindowsFormsApp1.MyControls.MyUserControl" could reside in ANOTHER (ALSO OUR OWN) ASSEMBLY – nothing prevents
                // using the same namespace ("WindowsFormsApp1") in MULTIPLE assemblies.
                // 3. Try Type.GetType():
                if (IsShortName(nameForResolution)) // In reality, a short name has never been received here from TypeCodeDomSerializer or its descendants; but it's included for completeness.
                {
                    // 3a. Passing the name "as is" to Type.GetType() is pointless, as it won't find anything by a short name.
                    //     We need to combine the name with each namespace from 'using' directives, as well as with the form's own namespace –
                    //     to see if one of the resulting full names points to an existing type:
                    var t = TryFindByShortOrPartialNameInGivenNamespaces(nameForResolution, ignoreCase, usings.Union(new[] { currentNamespace }));
                    if (t != null)
                    {
                        typeCache[name] = t;
                        return t;
                    }
                    // Not found by any of the resulting full names - we will search in the registered assemblies.
                }
                else
                {
                    // 3b. 'name' is either a full name or a "partially qualified" name (SYNTACTICALLY indistinguishable, as mentioned in the comment above).
                    //     Type.GetType() will work for a full name, IF the type is from the current assembly or from mscorlib/System.Private.CoreLib:
                    try
                    {
                        // First, search assuming it's a "true" full name ("WindowsFormsApp1.MyControls.MyUserControl", "System.Windows.Forms.Button", etc.):
                        var t = GetTypeIncludingNested(nameForResolution, ignoreCase: ignoreCase);
                        if (t != null)
                        {
                            typeCache[name] = t;
                            return t;
                        }
                        // Not found by "true" full name - assume it's a "partially qualified" name ("MyControls.MyUserControl", when the "MyControls" namespace
                        // lives within the same "WindowsFormsApp1" namespace as the form itself). Therefore, combine this "partially qualified" name with
                        // each namespace from 'using' directives, as well as with the form's own namespace – to see if one of the resulting full names points to an existing type:
                        t = TryFindByShortOrPartialNameInGivenNamespaces(name, ignoreCase, new[] { currentNamespace }/*usings.Union(new[] { currentNamespace })*/);
                        if (t != null)
                        {
                            typeCache[name] = t;
                            return t;
                        }
                        // Found by neither full name nor "partially qualified" name - continue searching in registered assemblies.
                    }
                    catch
                    {
                        // Ignore - will search in the registered assemblies.
                    }
                }

                // 4. Try Assembly.GetType(name) to search in the registered assemblies:
                AssemblyName[] snapshot;
                lock (assemblyNames) { snapshot = assemblyNames.ToArray(); }
                foreach (var asmName in snapshot)
                {
                    try
                    {
                        var asm = GetAssembly(asmName, throwOnError: false);
                        if (asm == null)
                            continue;
                        if (IsShortName(nameForResolution)) // In reality, a short name has never been received here from TypeCodeDomSerializer or its descendants; but it's included for completeness.
                        {
                            // 4a. Passing the name "as is" to Assembly.GetType() is pointless, as it won't find anything by a short name.
                            //     We need to combine the name with each namespace from 'using' directives, as well as with the form's own namespace –
                            //     to see if one of the resulting full names points to an existing type:
                            var t = TryFindByShortOrPartialNameInGivenNamespaces(asm, nameForResolution, ignoreCase, usings.Union(new[] { currentNamespace }));
                            if (t != null)
                            {
                                typeCache[name] = t;
                                return t;
                            }
                        }
                        else
                        {
                            // 4b. 'name' is either a full name or a "partially qualified" name. Assembly.GetType(name) expects a "true" full name.
                            // First, search assuming it's a "true" full name ("WindowsFormsApp1.MyControls.MyUserControl", "ThirdParty.Company.SomeControl", etc.):
                            var t = asm.GetTypeIncludingNested(nameForResolution, ignoreCase);
                            if (t != null)
                            {
                                typeCache[name] = t;
                                return t;
                            }
                            // Not found by "true" full name - assume it's a "partially qualified" name ("MyControls.MyUserControl", when the "MyControls" namespace
                            // lives within the same "WindowsFormsApp1" namespace as the form itself). Therefore, combine this "partially qualified" name with
                            // each namespace from 'using' directives, as well as with the form's own namespace – to see if one of the resulting full names points to an existing type:
                            t = TryFindByShortOrPartialNameInGivenNamespaces(asm, nameForResolution, ignoreCase, usings.Union(new[] { currentNamespace }));
                            if (t != null)
                            {
                                typeCache[name] = t;
                                return t;
                            }
                            // Found by neither full name nor "partially qualified" name - continue searching in other registered assemblies.
                        }
                    }
                    catch
                    {
                        // Ignore and continue searching in other registered assemblies.
                    }
                }
            }

            errMsg = $"Could not find type '{name}' in current domain or referenced assemblies";
            MessageLogger.LogError(this, errMsg);
            if (throwOnError)
                throw new TypeLoadException($"{nameof(MyTypeResolutionService)}.{nameof(GetType)}: {errMsg}");

            return null;
        }

        /// <summary>
        /// Adds a reference to the specified assembly.
        /// </summary>
        /// <param name="name">An System.Reflection.AssemblyName that indicates the assembly to reference.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void ReferenceAssembly(AssemblyName name)
        {
            if (name == null)
            {
                var errMsg = $"{nameof(MyTypeResolutionService)}.{nameof(ReferenceAssembly)}(): assembly name cannot be null";
                MessageLogger.LogError(this, errMsg);
                throw new ArgumentNullException("name", errMsg);
            }

            lock (assemblyNames)
            {
                if (!assemblyNames.Exists(a => AssemblyName.ReferenceMatchesDefinition(a, name)))
                {
                    assemblyNames.Add(name);
                    typeCache.Clear(); // Clear cache as new types have become available.
                }
            }
        }

        #endregion ITypeResolutionService implementation

        /// <summary>
        /// Checks if the given type name is probably an Assembly Qualified Named (AQN),
        /// e.g. "System.Windows.Forms.Button, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" ("full" AQN)
        /// or "System.Windows.Forms.Design.ControlDesigner, System.Design" ("partial AQN", is oftenly used in DesignerAttribute)
        /// </summary>
        /// <param name="typeName">The name of the type.</param>
        /// <returns>Returns true if the given type name is probably an Assembly Qualified Named (AQN)</returns>
        private static bool IsProbablyAQN(string typeName)
        {
            return typeName.IndexOf(',') >= 0;
        }

        /// <summary>
        /// Checks if the given type name is probably a FULL Assembly Qualified Named (AQN),
        /// e.g. "System.Windows.Forms.Button, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" (and not
        /// a "partial AQN", which is oftenly used in DesignerAttribute - like "System.Windows.Forms.Design.ControlDesigner, System.Design").
        /// </summary>
        /// <param name="typeName">The name of the type.</param>
        /// <returns>Returns true if the given type name is probably a full Assembly Qualified Named (AQN)</returns>
        private static bool IsProbablyFullAQN(string typeName)
        {
            return typeName.Count(c => c == ',') == 4;
            // maybe Regex would be better ;)
        }

        /// <summary>
        /// Checks if the given type name is probably a PARTIAL Assembly Qualified Named (AQN) (oftenly used in DesignerAttribute),
        /// e.g. "System.Windows.Forms.Design.ControlDesigner, System.Design" (and not a "full AQN" like
        /// "System.Windows.Forms.Button, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089").
        /// </summary>
        /// <param name="typeName">The name of the type.</param>
        /// <returns>Returns true if the given type name is probably a partial Assembly Qualified Named (AQN)</returns>
        private static bool IsProbablyPartialAQN(string typeName)
        {
            return typeName.Count(c => c == ',') == 1;
            // maybe Regex would be better ;)
        }

        /// <summary>
        /// Checks if the given type name is a short name, e.g. "Form", "UserControl", "Button" etc.
        /// </summary>
        /// <param name="typeName">The name of the type.</param>
        /// <returns>Returns true if the given type name is a short name (without namespace)</returns>
        private static bool IsShortName(string typeName)
        {
            return typeName.IndexOf('.') == -1;
        }

        /// <summary>
        /// Tries to find Type in the given Assembly from its short name and a list of namespaces to combine these names with.
        /// </summary>
        /// <param name="asm">Assembly to be searched for Type</param>
        /// <param name="typeShortName">Type short name (without namespace)</param>
        /// <param name="ignoreCase">true to ignore case when searching for types; otherwise, false.</param>
        /// <param name="namespaces">List of namespaces to be combined with Type short name</param>
        /// <returns>Type object - if search succeeds, null - otherwise</returns>
        private static Type TryFindByShortOrPartialNameInGivenNamespaces(Assembly asm, string typeShortName, bool ignoreCase, IEnumerable<string> namespaces)
        {
            foreach (string ns in namespaces)
            {
                string typeFullName = $"{ns}.{typeShortName}";
                var t = asm.GetTypeIncludingNested(typeFullName, ignoreCase);
                if (t != null)
                    return t;
            }
            return null;
        }

        /// <summary>
        /// Tries to find Type from its short name and a list of namespaces to combine these names with.
        /// </summary>
        /// <param name="typeShortName">Type short name (without namespace)</param>
        /// <param name="ignoreCase">true to ignore case when searching for types; otherwise, false.</param>
        /// <param name="namespaces">List of namespaces to be combined with Type short name</param>
        /// <returns>Type object - if search succeeds, null - otherwise</returns>
        private static Type TryFindByShortOrPartialNameInGivenNamespaces(string typeShortName, bool ignoreCase, IEnumerable<string> namespaces)
        {
            foreach (string ns in namespaces)
            {
                string typeFullName = $"{ns}.{typeShortName}";
                var t = GetTypeIncludingNested(typeFullName, ignoreCase);
                if (t != null)
                    return t;
            }
            return null;
        }

        /// <summary>
        /// Returns Assembly given its AssemblyName if such Assembly is currently loaded into the application domain.
        /// </summary>
        /// <param name="assemblyName">Name of the Assembly to load</param>
        /// <returns>Returns Assembly given its AssemblyName if such Assembly is currently loaded into the application domain. Returns null otherwise.</returns>
        private static Assembly GetLoadedAssembly(AssemblyName assemblyName)
        {
            Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            return loadedAssemblies.FirstOrDefault(a => AssemblyName.ReferenceMatchesDefinition(a.GetName(), assemblyName));
        }

        /// <summary>
        /// Replacement for the static Type.GetType() method with support for nested types (Namespace.Class.NestedClass).
        /// </summary>
        private static Type GetTypeIncludingNested(string name, bool ignoreCase)
        {
            // 1. Attempt to find the type "normally".
            Type t = Type.GetType(name, false, ignoreCase);
            if (t != null)
                return t;

            // 2. Try replacing dots with pluses from right to left (for nested types).
            return Extensions.TryResolveNested(name, (combinedName) => Type.GetType(combinedName, false, ignoreCase));
        }

        private static bool IsStronglyTypedResourceClass(Type type)
        {
            if (type == null)
                return false;

            var attr = type.GetCustomAttribute<System.CodeDom.Compiler.GeneratedCodeAttribute>();
            if (attr != null)
            {
                return !string.IsNullOrEmpty(attr.Tool) &&
                       attr.Tool.EndsWith("StronglyTypedResourceBuilder", StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }
    }
}
