using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Reflection;
using System.Linq;

namespace UiTools.WinForms.Designer.Core
{
    /// <summary>
    /// Discovers available types at design time.
    /// </summary>
    /// <remarks>
    /// This class is used, for example, by System.Windows.Forms.Design.DataGridViewAddColumnDialog to populate
    /// combobox "Type" of the "Add column..." dialog (available via smart tag "DataGridView Tasks"). It uses
    /// MyTypeResolutionService to get list of all referenced assemblies.
    /// MSDN: "The ITypeDiscoveryService is used to discover available types at design time, when a client
    ///        of the service does not know the names of existing types or referenced assemblies"
    /// </remarks>
    public class MyTypeDiscoveryService : ITypeDiscoveryService
    {
        private readonly IServiceProvider serviceProvider;
        private Dictionary<Type, ICollection> cachedTypes = new Dictionary<Type, ICollection>();

        public MyTypeDiscoveryService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public ICollection GetTypes(Type baseType, bool excludeGlobalTypes)
        {
            var trs = serviceProvider.GetService(typeof(ITypeResolutionService)) as MyTypeResolutionService;
            if (trs == null)
            {
                var errMsg = "MyTypeResolutionService not found (it is not registered in the DesignSurface's ServiceProvider)"; // developer's error
                MessageLogger.LogError(this, errMsg);
                throw new InvalidOperationException($"{nameof(MyTypeDiscoveryService)}.{nameof(GetTypes)}: {errMsg}");
            }

            if (cachedTypes.ContainsKey(baseType))
                return cachedTypes[baseType];

            List<Type> discoveredTypes = new List<Type>();
            IEnumerable<AssemblyName> knownAssemblyNames = trs.GetKnownAssemblyNames();
            foreach (AssemblyName asmName in knownAssemblyNames)
            {
                Assembly asm = null;
                try
                {
                    // Perform "lazy" assembly loading via MyTypeResolutionService
                    asm = trs.GetAssembly(asmName);
                }
                catch (Exception ex)
                {
                    MessageLogger.LogError(this, $"Failed to get assembly {asmName.FullName} from MyTypeResolutionService (during type discovery): {ex.Message}", ex);
                    // Continue without stopping for a single problematic assembly
                }
                if (asm == null)
                    continue; // assembly failed to load or was not found
                ScanAssemblyForTypes(asm, baseType, discoveredTypes);
            }

            // Cache the result for this baseType
            cachedTypes[baseType] = discoveredTypes;
            return discoveredTypes;
        }

        private void ScanAssemblyForTypes(Assembly asm, Type baseType, List<Type> discoveredTypes)
        {
            try
            {
                discoveredTypes.AddRange(asm.GetTypes().Where(t => baseType.IsAssignableFrom(t) && t.IsPublic && !t.IsAbstract));
            }
            catch (ReflectionTypeLoadException ex)
            {
                // Some types within the assembly might fail to load (e.g. due to missing dependencies),
                // but others might still be useful. Attempt to extract successfully loaded types.
                if (ex.Types != null)
                    discoveredTypes.AddRange(ex.Types.Where(t => t != null && baseType.IsAssignableFrom(t) && t.IsPublic && !t.IsAbstract));
                MessageLogger.LogWarning(this, $"Partial load for assembly {asm.FullName}: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageLogger.LogError(this, $"Failed to scan assembly {asm.FullName}: {ex.Message}", ex);
            }
        }
    }
}
