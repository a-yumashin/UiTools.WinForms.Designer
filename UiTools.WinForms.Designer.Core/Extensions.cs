using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;

namespace UiTools.WinForms.Designer.Core
{
    public static class Extensions
    {
        public static bool In(this string s, params string[] values)
        {
            return values.Contains(s);
        }

        public static string CondenseWhitespaces(this string s)
        {
            return Regex.Replace(s.Trim(), @"\s+", " ");
        }

        public static string CapitalizeFirstLetter(this string s)
        {
            if (s.Length == 0)
                return s;
            if (s.Length == 1)
                return s.ToUpper();
            return s.Substring(0, 1).ToUpper() + s.Substring(1);
        }

        public static bool ContainsIgnoringCase(this List<string> list, string value)
        {
            return list.Any(s => s.Equals(value, StringComparison.OrdinalIgnoreCase));
        }

        public static Dictionary<T, U> MergeWithOtherDictionary<T, U>(this Dictionary<T, U> dict, Dictionary<T, U> other)
        {
            // Merge dict and other, keeping the value from dict in case of collision
            return dict.Concat(other)
                .GroupBy(kvp => kvp.Key)
                .ToDictionary(g => g.Key, g => g.First().Value);
        }

        public static bool ShowAsToolboxItem(this Type type)
        {
            var attr = (ToolboxItemAttribute)TypeDescriptor.GetAttributes(type)[typeof(ToolboxItemAttribute)];
            if (attr == null || attr.Equals(ToolboxItemAttribute.None))
                return false;
            if (attr.IsDefaultAttribute())
                return true;
            return !string.IsNullOrEmpty(attr.ToolboxItemTypeName) || attr.ToolboxItemType != null;
        }

        public static bool IsDesignTimeVisible(this Type type)
        {
            var attr = (DesignTimeVisibleAttribute)TypeDescriptor.GetAttributes(type)[typeof(DesignTimeVisibleAttribute)];
            return attr == null || attr.Visible;
        }

        public static bool IsBrowsable(this Type type)
        {
            var attr = (BrowsableAttribute)TypeDescriptor.GetAttributes(type)[typeof(BrowsableAttribute)];
            return attr == null || attr.Browsable;
        }

        public static bool HasDefaultCtor(this Type type)
        {
            return type.GetConstructors().Any(ci => !ci.GetParameters().Any());
        }

        public static bool IsInternal(this Type t)
        {
            if (t == null)
                return false;

            if (!t.IsNested)
                return !t.IsPublic; // for top-level types: "non-public" = "internal" (in the context of the assembly)

            return t.IsNestedAssembly || t.IsNestedFamORAssem; // for nested types: IsNestedAssembly = "internal", IsNestedFamORAssem = "protected internal"
        }

        public static Control FindControlByType(this Control parent, string typeFullName)
        {
            if (parent.GetType().FullName == typeFullName)
                return parent;
            foreach (Control child in parent.Controls)
            {
                var result = FindControlByType(child, typeFullName);
                if (result != null)
                    return result;
            }
            return null;
        }

        /// <summary>
        /// Gets the control name, qualified by its parent container's name if present
        /// (e.g. "splitContainer1.Panel1", "toolStripContainer1.RightToolStripPanel").
        /// </summary>
        /// <param name="control"></param>
        /// <param name="host"></param>
        /// <returns></returns>
        public static string GetQualifiedControlName(this Control control, IDesignerHost host)
        {
            if (control == null)
                return null;

            // If the control has a Site and is top-level, then this is its name.
            // But we get here when IsTopLevelComponent == false, so
            // Site.Name will either be null, or refer to the parent's Site.
            if (control.Site?.Name != null && control.IsTopLevelComponent(host))
                return control.Site.Name;

            var parts = new List<string>();
            var currentControl = control;
            while (currentControl != null)
            {
                // Use Site.Name for components that have a Site within the designer container
                // and Control.Name for controls that are not designer components
                var name = currentControl.Site?.Name ?? currentControl.Name;
                if (!string.IsNullOrEmpty(name))
                    parts.Insert(0, name);
                // Stop if we reach the root component of the DesignSurface
                // or a component that is itself a top-level component
                if (currentControl == host.RootComponent || currentControl.IsTopLevelComponent(host))
                    break;
                currentControl = currentControl.Parent;
            }
            return string.Join(".", parts);
        }

        public static bool IsTopLevelComponent(this IComponent component, IDesignerHost designerHost)
        {
            /*
             * NOTE:
             * Perhaps this method should have been named IsManagedByDesignerHostContainer or IsDesignerManagedComponent, as it checks that
             * component "is a component managed directly by the designer host and intended for user/code interaction",
             * rather than checking that component "is a direct child element of the root form".
             * Examples of return values for this method:
                • Form (RootComponent) -> IsTopLevelComponent = true
                • Button (directly on Form) -> IsTopLevelComponent = true
                • TabControl (directly on Form) -> IsTopLevelComponent = true
                • TabPage (inside TabControl) -> IsTopLevelComponent = true (because it is a direct member of designerHost.Container)
                • ToolStripMenuItem (inside MenuStrip) -> IsTopLevelComponent = true (similarly to TabPage, MenuStrip adds ToolStripMenuItem to the main Container)
                • Timer (non-visual component) -> IsTopLevelComponent = true (added to the main Container)
                • SplitterPanel (inside SplitContainer) -> IsTopLevelComponent = false (because SplitterPanel is not added to the main Container directly; it is considered an
                  internal part of SplitContainer, and its Site.Container is either null or is the internal container of SplitContainer, if any, but it is not designerHost.Container).
             */
            if (component == null || designerHost == null)
                return false;
            if (component == designerHost.RootComponent)
                return true;

            // A component is considered "top-level" for IDesignerHost.Container if it has an ISite, and this Site belongs to IDesignerHost.Container.
            // This means that the IContainer managing this component is the main designer container.
            return component.Site?.Container == designerHost.Container;
        }

        /// <summary>
        /// A substitute for the Assembly.GetType() method with support for nested types (Namespace.Class.NestedClass).
        /// </summary>
        public static Type GetTypeIncludingNested(this Assembly asm, string name, bool ignoreCase)
        {
            if (asm == null)
                throw new ArgumentNullException(nameof(asm));

            // Attempt to find the type in the "standard" way
            Type t = asm.GetType(name, false, ignoreCase);
            if (t != null)
                return t;

            // Try replacing dots with pluses from right to left
            return TryResolveNested(name, (combinedName) => asm.GetType(combinedName, false, ignoreCase));
        }

        internal static Type TryResolveNested(string name, Func<string, Type> typeResolver)
        {
            // internal - because it's also used from the MyTypeResolutionService.GetTypeIncludingNested() method
            if (string.IsNullOrEmpty(name) || !name.Contains("."))
                return null;

            string currentName = name;
            int lastDotIndex = currentName.LastIndexOf('.');

            while (lastDotIndex > 0)
            {
                char[] chars = currentName.ToCharArray();
                chars[lastDotIndex] = '+';
                currentName = new string(chars);

                Type t = typeResolver(currentName);
                if (t != null)
                    return t;

                lastDotIndex = currentName.LastIndexOf('.');
            }
            return null;
        }

        public static void SetLabelColumnWidth(this PropertyGrid grid, int value)
        {
            if (value == 0)
                return;
            var fi = grid.GetType().GetField("gridView", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fi == null)
                return;
            object gridView = fi.GetValue(grid);
            if (gridView == null)
                return;
            var mi = gridView.GetType().GetMethod("MoveSplitterTo", BindingFlags.NonPublic | BindingFlags.Instance);
            if (mi == null)
                return;
            mi.Invoke(gridView, new object[] { value });
        }

        public static Control GetGridView(this PropertyGrid grid)
        {
            return grid.Controls.Cast<Control>().FirstOrDefault(c => c.GetType().Name == "PropertyGridView");
        }

        public static GridItem GetGridItemFromLabel(this PropertyGrid propertyGrid, string labelText)
        {
            GridItem GetRootItem(PropertyGrid propertyGrid)
            {
                var root = propertyGrid.SelectedGridItem;
                do
                {
                    root = root.Parent ?? root;
                } while (root.Parent != null);
                return root;
            }
            IList<GridItem> GetAllChildGridItems(PropertyGrid propertyGrid, GridItem parent)
            {
                var items = new List<GridItem>();
                foreach (GridItem item in parent.GridItems)
                {
                    items.Add(item);
                    if (item.GridItems.Count > 0)
                        items.AddRange(GetAllChildGridItems(propertyGrid, item));
                }
                return items;
            }
            return GetAllChildGridItems(propertyGrid, GetRootItem(propertyGrid)).FirstOrDefault(p => p.Label == labelText); ;
        }

        public static MethodDeclarationSyntax FindMethodWithNameAndSignature(this ClassDeclarationSyntax classDeclaration, string methodName,
            ParameterInfo[] parameters, string expectedReturnTypeName, ITypeResolutionService trs)
        {
            foreach (MethodDeclarationSyntax method in classDeclaration.Members.OfType<MethodDeclarationSyntax>())
            {
                if (method.Identifier.ValueText == methodName && method.HasSignature(parameters, expectedReturnTypeName, trs))
                    return method;
            }
            return null;
        }

        public static IEnumerable<MethodDeclarationSyntax> FindMethodsWithSignature(this ClassDeclarationSyntax classDeclaration,
            ParameterInfo[] parameters, string expectedReturnTypeName, ITypeResolutionService trs)
        {
            foreach (MethodDeclarationSyntax method in classDeclaration.Members.OfType<MethodDeclarationSyntax>())
            {
                if (method.HasSignature(parameters, expectedReturnTypeName, trs))
                    yield return method;
            }
        }

        public static bool IsEmpty(this MethodDeclarationSyntax method)
        {
            return method.Body == null || (method.Body.Statements.Count == 0 && string.IsNullOrWhiteSpace(method.Body.ToString().Trim('{', '}')));
        }

        public static bool HasSignature(
            this MethodDeclarationSyntax method,
            ParameterInfo[] expectedEventParameters,
            string expectedReturnTypeName,
            ITypeResolutionService trs)
        {

            // Compare return types:
            if (method.ReturnType.ToString() != expectedReturnTypeName)
                return false;

            // Compare the number of parameters:
            var paramList = method.ParameterList?.Parameters ?? default(SeparatedSyntaxList<ParameterSyntax>);
            int actualCount = paramList.Count;
            int expectedCount = expectedEventParameters?.Length ?? 0;
            if (actualCount != expectedCount)
                return false;

            // Compare types for each parameter:
            for (int i = 0; i < expectedCount; i++)
            {
                string expectedEventParamTypeName = CommonStuff.GetFriendlyTypeName(expectedEventParameters[i].ParameterType);
                string actualParamTypeNameInCode = paramList[i].Type?.ToString();

                Type resolvedExpectedType = trs.GetType(expectedEventParamTypeName, throwOnError: false);
                Type resolvedActualType = trs.GetType(actualParamTypeNameInCode, throwOnError: false);

                if (resolvedExpectedType == null || resolvedActualType == null || resolvedExpectedType != resolvedActualType)
                    return false;
            }

            return true;
        }

        public static bool IsChildOf(this Control child, Control parent)
        {
            while (child != null)
            {
                if (child == parent)
                    return true;
                child = child.Parent;
            }
            return false;
        }
    }
}
