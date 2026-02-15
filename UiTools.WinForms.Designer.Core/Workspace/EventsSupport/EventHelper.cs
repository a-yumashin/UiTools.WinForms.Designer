using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Reflection;
using static UiTools.WinForms.Designer.Core.CommonStuff;

namespace UiTools.WinForms.Designer.Core
{
    internal static class EventHelper
    {
        /// <summary>
        /// Checks for the existence of an event handler method within a specific class in the main .cs file.
        /// </summary>
        /// <param name="designerFilePath">Path to the .designer.cs file.</param>
        /// <param name="classIdentifier">Class name (e.g. "Form1").</param>
        /// <param name="handlerName">Event handler method name (e.g. "button1_Click").</param>
        public static void CheckEventHandlerExistsInMainFile(string designerFilePath, string classIdentifier, string handlerName)
        {
            if (!CommonStuff.IsDesignerCsFilePathValid(designerFilePath))
            {
                MessageLogger.LogWarning(typeof(EventHelper), $"could not resolve the main file path from '{designerFilePath}' (expected a path ending in '.designer.cs').");
                return;
            }
            string mainFilePath = CommonStuff.MainCsFilePathFromDesignerCsFilePath(designerFilePath);
            if (!File.Exists(mainFilePath))
            {
                MessageLogger.LogWarning(typeof(EventHelper), $"Main file '{mainFilePath}' not found");
                return;
            }

            try
            {
                string code = File.ReadAllText(mainFilePath);
                SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
                var root = tree.GetRoot();

                // Find class declaration with a name matching classIdentifier
                var classDeclaration = root.DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .FirstOrDefault(c => c.Identifier.ValueText == classIdentifier);

                if (classDeclaration == null)
                {
                    MessageLogger.LogWarning(typeof(EventHelper),
                        $"Class '{classIdentifier}' not found in '{Path.GetFileName(mainFilePath)}'. Cannot verify handler '{handlerName}'.");
                    return;
                }

                // Look for the handler method only among members of this specific class
                if (classDeclaration.Members.OfType<MethodDeclarationSyntax>().Any(m => m.Identifier.ValueText == handlerName))
                    MessageLogger.LogVerbose(typeof(EventHelper),
                        $"Event handler '{handlerName}' found in class '{classIdentifier}' of '{Path.GetFileName(mainFilePath)}'.");
                else
                    MessageLogger.LogWarning(typeof(EventHelper),
                        $"Event handler '{handlerName}' is missing in class '{classIdentifier}' of '{Path.GetFileName(mainFilePath)}'. " +
                        "It should be created manually or via designer action.");
            }
            catch (Exception ex)
            {
                MessageLogger.LogError(typeof(EventHelper),
                    $"Error while checking event handler '{handlerName}' in class '{classIdentifier}': {ex.Message}", ex);
            }
        }

        public static bool TryCreateEventHandlerInMainFile(string mainFilePath, string classIdentifier, Component comp,
            string eventName, string handlerMethodName, ITypeResolutionService trs, out string handlerCode)
        {
            handlerCode = null;

            ThrowIfNullOrEmpty(mainFilePath);
            ThrowIfFileNotFound(mainFilePath, $"Main file not found: {mainFilePath}");
            ThrowIfNullOrEmpty(classIdentifier);
            ThrowIfNullOrEmpty(comp);
            ThrowIfNullOrEmpty(eventName);
            ThrowIfNullOrEmpty(handlerMethodName);

            try
            {
                string sourceCode = File.ReadAllText(mainFilePath);
                SyntaxTree tree = CSharpSyntaxTree.ParseText(sourceCode);
                var root = (CompilationUnitSyntax)tree.GetRoot();

                var classDeclaration = root.DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .FirstOrDefault(c => c.Identifier.ValueText == classIdentifier);

                if (classDeclaration == null)
                    throw new Exception($"Class '{classIdentifier}' not found.");

                EventDescriptor ed = TypeDescriptor.GetEvents(comp)[eventName];
                if (ed == null)
                {
                    MessageLogger.LogError(typeof(EventHelper), $"Event '{eventName}' not found on component '{comp.GetType().Name}'.");
                    return false;
                }

                EventInfo eventInfo = comp.GetType().GetEvent(eventName);
                if (eventInfo == null)
                    throw new Exception($"Event '{eventName}' not found on component '{comp.GetType().Name}'.");

                MethodInfo invokeMethod = eventInfo.EventHandlerType.GetMethod("Invoke");
                ParameterInfo[] eventParameters = invokeMethod.GetParameters();

                // Event return type (usually void)
                string expectedReturnTypeName = GetFriendlyTypeName(invokeMethod.ReturnType);

                if (classDeclaration.FindMethodWithNameAndSignature(handlerMethodName, eventParameters, expectedReturnTypeName, trs) != null)
                    return false; // same method already exists

                // Parameter preparation
                var parameters = invokeMethod.GetParameters().Select(p =>
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier(p.Name))
                        .WithType(SyntaxFactory.ParseTypeName(GetFriendlyTypeName(p.ParameterType)))
                ).ToList();

                // Determine indentation for the new method
                string classIndent = "";
                var firstClassLeadingTrivia = classDeclaration.GetLeadingTrivia().FirstOrDefault(t => t.IsKind(SyntaxKind.WhitespaceTrivia));
                if (firstClassLeadingTrivia.IsKind(SyntaxKind.WhitespaceTrivia))
                {
                    classIndent = firstClassLeadingTrivia.ToString();
                }

                string memberIndentLevel = "    "; // standard indentation for a class member (4 spaces)

                // Try to infer member indentation from existing class members
                if (classDeclaration.Members.Any())
                {
                    var firstMember = classDeclaration.Members.First();
                    string fullMemberIndent = "";
                    var firstMemberLeadingTrivia = firstMember.GetLeadingTrivia().FirstOrDefault(t => t.IsKind(SyntaxKind.WhitespaceTrivia));
                    if (firstMemberLeadingTrivia.IsKind(SyntaxKind.WhitespaceTrivia))
                    {
                        fullMemberIndent = firstMemberLeadingTrivia.ToString();
                    }

                    // If the first member has an indentation and it starts with the class indentation,
                    // calculate the difference as a single member indentation level.
                    if (fullMemberIndent.StartsWith(classIndent) && fullMemberIndent.Length > classIndent.Length)
                    {
                        memberIndentLevel = fullMemberIndent.Substring(classIndent.Length);
                    }
                }

                // Resulting method indentation: class indentation + one member indentation level
                string methodFullIndent = classIndent + memberIndentLevel;

                // Generate the method as a string rather than via SyntaxFactory.MethodDeclaration (couldn't overcome all the issues with LeadingTrivia, TrailingTrivia, etc.)
                string formattedMethodString;
                {
                    // Assemble parameters as a string
                    string parametersString = string.Join(", ", parameters.Select(p => $"{p.Type} {p.Identifier}"));

                    // Compose the full method string including indentation and empty lines
                    formattedMethodString = $"\r\n{methodFullIndent}"; // empty line and indentation before the method
                    formattedMethodString += $"private void {handlerMethodName}({parametersString})\r\n"; // method signature
                    formattedMethodString += $"{methodFullIndent}{{\r\n"; // opening brace with indentation and line break
                    formattedMethodString += "\r\n"; // empty line in the method body
                    formattedMethodString += $"{methodFullIndent}}}\r\n"; // closing brace with indentation and line break
                }

                handlerCode = formattedMethodString;

                // Parse this fully formatted string back into a SyntaxNode
                var newMethod = SyntaxFactory.ParseMemberDeclaration(formattedMethodString);

                // We replace only the class node, adding the new method to it
                var updatedClass = classDeclaration.AddMembers(newMethod);
                var updatedRoot = root.ReplaceNode(classDeclaration, updatedClass);

                // Save to file:
                File.WriteAllText(mainFilePath, updatedRoot.ToFullString(), CommonStuff.Utf8WithoutBom);

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create event handler: {ex.Message}", ex);
            }
        }

        public static List<EventInfo> GetPublicEvents(Component comp)
        {
            ThrowIfNullOrEmpty(comp);

            List<EventInfo> browsableEvents = new List<EventInfo>();
            Type componentType = comp.GetType();

            foreach (EventInfo eventInfo in componentType.GetEvents(BindingFlags.Public | BindingFlags.Instance))
            {
                BrowsableAttribute browsableAttr = eventInfo.GetCustomAttribute<BrowsableAttribute>();
                if (browsableAttr == null || browsableAttr.Browsable)
                    browsableEvents.Add(eventInfo);
            }

            return browsableEvents;
        }

        public static List<string> GetEventHandlerCandidates(string mainFilePath, string classIdentifier, EventInfo eventInfo, ITypeResolutionService trs)
        {
            ThrowIfNullOrEmpty(eventInfo);
            ThrowIfNullOrEmpty(trs, "ITypeResolutionService must be provided for accurate type resolution.");

            if (string.IsNullOrEmpty(mainFilePath) || string.IsNullOrEmpty(classIdentifier))
            {
                MessageLogger.LogWarning(typeof(EventHelper),
                    "Cannot retrieve event handler candidates from the main file because parameters mainFilePath and/or classIdentifier are null or empty.");
                return new List<string>();
            }

            if (!File.Exists(mainFilePath))
            {
                MessageLogger.LogError(typeof(EventHelper), $"Main file not found: {mainFilePath}");
                return new List<string>();
            }

            var candidates = new List<string>();

            string sourceCode = File.ReadAllText(mainFilePath);
            SyntaxTree tree = CSharpSyntaxTree.ParseText(sourceCode);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var classDeclaration = root.DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .FirstOrDefault(c => c.Identifier.ValueText == classIdentifier);

            if (classDeclaration == null)
                return candidates;

            MethodInfo invokeMethod = eventInfo.EventHandlerType.GetMethod("Invoke");
            ParameterInfo[] eventParameters = invokeMethod.GetParameters();

            // Event return type (usually void)
            string expectedReturnTypeName = GetFriendlyTypeName(invokeMethod.ReturnType);

            candidates.AddRange(
                classDeclaration.FindMethodsWithSignature(eventParameters, expectedReturnTypeName, trs)
                    .Select(m => m.Identifier.ValueText));

            return candidates;
        }

        /// <summary>
        /// Deletes the handler method with the specified name from the form class in the "main" form file,
        /// but only if its body is empty and the signature matches the expected event signature.
        /// </summary>
        /// <param name="mainFilePath">Path to the main form file (e.g. to Form1.cs).</param>
        /// <param name="classIdentifier">Form class identifier (e.g. "Form1").</param>
        /// <param name="comp">Component for which the handler is being removed.</param>
        /// <param name="eventName">Event name.</param>
        /// <param name="eventHandlerName">Name of the handler method to be deleted.</param>
        /// <param name="trs">Service for resolving type names to System.Type.</param>
        /// <returns>True if the method was found, had an empty body, and was successfully deleted; otherwise, False.</returns>
        public static bool TryDeleteEventHandlerInMainFile(string mainFilePath, string classIdentifier, Component comp,
            string eventName, string eventHandlerName, ITypeResolutionService trs)
        {
            ThrowIfNullOrEmpty(mainFilePath);
            ThrowIfFileNotFound(mainFilePath, $"Main file not found: {mainFilePath}");
            ThrowIfNullOrEmpty(classIdentifier);
            ThrowIfNullOrEmpty(comp);
            ThrowIfNullOrEmpty(eventName);
            ThrowIfNullOrEmpty(eventHandlerName);
            ThrowIfNullOrEmpty(trs, "ITypeResolutionService must be provided for accurate type resolution.");

            try
            {
                string sourceCode = File.ReadAllText(mainFilePath);
                SyntaxTree tree = CSharpSyntaxTree.ParseText(sourceCode);
                var root = (CompilationUnitSyntax)tree.GetRoot();

                var classDeclaration = root.DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .FirstOrDefault(c => c.Identifier.ValueText == classIdentifier);

                if (classDeclaration == null)
                    return false; // form class not found

                EventInfo eventInfo = comp.GetType().GetEvent(eventName);
                if (eventInfo == null)
                    return false; // event not found on the component

                MethodInfo invokeMethod = eventInfo.EventHandlerType.GetMethod("Invoke");
                ParameterInfo[] expectedEventParameters = invokeMethod.GetParameters();
                string expectedReturnTypeName = GetFriendlyTypeName(invokeMethod.ReturnType);

                var matchingMethod = classDeclaration.FindMethodWithNameAndSignature(eventHandlerName, expectedEventParameters, expectedReturnTypeName, trs);
                if (matchingMethod == null || !matchingMethod.IsEmpty())
                    return false; // not found or not empty - cannot delete it!

                var updatedClass = classDeclaration.RemoveNode(matchingMethod, SyntaxRemoveOptions.KeepNoTrivia); // KeepNoTrivia removes associated comments and whitespaces
                var updatedRoot = root.ReplaceNode(classDeclaration, updatedClass);

                // Save the modified file
                File.WriteAllText(mainFilePath, updatedRoot.ToFullString(), CommonStuff.Utf8WithoutBom);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete event handler '{eventHandlerName}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Creates/modifies/deletes an event binding for the design-time environment via IEventBindingService. Returns true on success.
        /// </summary>
        /// <param name="component">IComponent - the target component (must have a Site with an IServiceProvider).</param>
        /// <param name="eventName">Event name (e.g. "Click").</param>
        /// <param name="removeEventSubscription">If true, removes the subscription.</param>
        /// <param name="desiredHandlerName">Desired method name (pointless when removeEventSubscription == true, null can be passed).</param>
        /// <param name="mainFilePath">Path to the main file (e.g. to Form1.cs) — required to create the method body.</param>
        /// <param name="classIdentifier">Class name (e.g. "Form1") — required to create the method body.</param>
        /// <param name="trs">ITypeResolutionService — required for TryDeleteEventHandlerInMainFile, TryCreateEventHandlerInMainFile and GetEventHandlerCandidates.</param>
        /// <param name="generatedHandlerCode">out: code returned by EventHelper.TryCreateEventHandlerInMainFile (if created).</param>
        public static bool UpdateEventSubscription(
            IComponent component,
            string eventName,
            bool removeEventSubscription,
            string desiredHandlerName,
            string mainFilePath,
            string classIdentifier,
            ITypeResolutionService trs,
            out string generatedHandlerCode)
        {
            ThrowIfNullOrEmpty(component);
            ThrowIfNullOrEmpty(eventName);
            if (!removeEventSubscription) ThrowIfNullOrEmpty(desiredHandlerName);
            ThrowIfNullOrEmpty(trs, "ITypeResolutionService must be provided for accurate type resolution.");
            // NOTE: mainFilePath (and consequently classIdentifier) can be missing for new unsaved form/usercontrol

            generatedHandlerCode = null;

            IServiceProvider sp = component.Site;
            var ebs = sp?.GetService(typeof(IEventBindingService)) as MyEventBindingService;
            var host = sp?.GetService(typeof(IDesignerHost)) as IDesignerHost;
            var changeService = sp?.GetService(typeof(IComponentChangeService)) as IComponentChangeService;

            if (ebs == null)
            {
                MessageLogger.LogError(typeof(EventHelper), "Event binding service is not available. Cannot create subscription in design-time.");
                return false;
            }

            try
            {
                // 1. EventDescriptor
                EventDescriptor ed = TypeDescriptor.GetEvents(component)[eventName];
                if (ed == null)
                {
                    MessageLogger.LogError(typeof(EventHelper), $"Event '{eventName}' not found on component '{component.GetType().Name}'.");
                    return false;
                }

                // 2. Binding property (from IEventBindingService)
                PropertyDescriptor bindingProp = ebs.GetEventProperty(ed);
                if (bindingProp == null)
                {
                    MessageLogger.LogError(typeof(EventHelper), "IEventBindingService.GetEventProperty() returned null");
                    return false;
                }

                // 3. Current handler (old)
                string oldHandler = null;
                try { oldHandler = bindingProp.GetValue(component) as string; } catch { oldHandler = null; }

                string newValue = removeEventSubscription
                    ? null
                    : desiredHandlerName;

                if (string.Equals(oldHandler, newValue, StringComparison.Ordinal))
                    return false; // nothing changed (values are equal or both are null) --> nothing to do

                if (!removeEventSubscription)
                {
                    // 4. Try create handler method in the main file (if path/class provided)
                    if (string.IsNullOrEmpty(mainFilePath) || string.IsNullOrEmpty(classIdentifier))
                    {
                        MessageLogger.LogWarning(typeof(EventHelper),
                            $"Cannot create event handler '{desiredHandlerName}' in the main file because parameters mainFilePath and/or classIdentifier are null or empty.");
                    }
                    else
                    {
                        try
                        {
                            bool success = TryCreateEventHandlerInMainFile(
                                mainFilePath,
                                classIdentifier,
                                component as Component,
                                eventName,
                                desiredHandlerName,
                                trs,
                                out string handlerCode);
                            if (success)
                                MessageLogger.Log(typeof(EventHelper),
                                    $"Created handler method in '{Path.GetFileName(mainFilePath)}' file: {handlerCode.CondenseWhitespaces()}");
                            generatedHandlerCode = handlerCode;
                        }
                        catch (Exception ex)
                        {
                            MessageLogger.LogError(typeof(EventHelper), $"Failed to create event handler method in the main file: {ex.Message}", ex);
                            return false;
                        }
                    }
                }

                // 5. Set binding in transaction + component change notifications
                var action = removeEventSubscription
                    ? "Remove"
                    : (string.IsNullOrEmpty(oldHandler) ? "Create" : "Update");
                using (DesignerTransaction tx = host?.CreateTransaction($"{action} subscription for event '{component.Site.Name}.{eventName}'"))
                {
                    try
                    {
                        changeService?.OnComponentChanging(component, bindingProp);
                        bindingProp.SetValue(component, newValue); // newValue is either desiredHandlerName or null
                        changeService?.OnComponentChanged(component, bindingProp, oldHandler, newValue);
                        tx?.Commit();
                        MessageLogger.Log(typeof(EventHelper), $"{action}d subscription for event '{component.Site.Name}.{eventName}'");
                    }
                    catch (Exception ex)
                    {
                        MessageLogger.LogError(typeof(EventHelper), $"Designer Transaction failed: {ex.Message}", ex);
                        return false;
                    }
                }

                // 6. Try to delete old handler method if it was different and is empty (safe cleanup)
                if (!string.IsNullOrEmpty(oldHandler))
                {
                    if (string.IsNullOrEmpty(mainFilePath) || string.IsNullOrEmpty(classIdentifier))
                    {
                        MessageLogger.LogWarning(typeof(EventHelper),
                            $"Cannot delete old event handler '{oldHandler}' from the main file because parameters mainFilePath and/or classIdentifier are null or empty.");
                    }
                    else
                    {
                        try
                        {
                            bool success = TryDeleteEventHandlerInMainFile(
                                mainFilePath,
                                classIdentifier,
                                component as Component,
                                eventName,
                                oldHandler,
                                trs);
                            if (success)
                                MessageLogger.Log(typeof(EventHelper),
                                    $"Removed empty handler method '{oldHandler}' from '{Path.GetFileName(mainFilePath)}' file");
                        }
                        catch (Exception ex)
                        {
                            MessageLogger.LogWarning(typeof(EventHelper), $"Failed to delete old handler: {ex.Message}");
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageLogger.LogError(typeof(EventHelper), $"Unexpected error: {ex.Message}", ex);
                return false;
            }
        }

        public static string CreateUniqueMethodName(
            IComponent component,
            EventDescriptor e,
            string filePath,
            string classIdentifier,
            ITypeResolutionService trs)
        {
            // 1. Compose the base name (e.g. "Form1_Load")
            string compName = component?.Site?.Name ?? component?.GetType().Name ?? "handler";
            string safeCompName = System.Text.RegularExpressions.Regex.Replace(compName, @"\W", "_");
            string baseName = $"{safeCompName.CapitalizeFirstLetter()}_{e.Name}";

            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath) || string.IsNullOrEmpty(classIdentifier) || trs == null)
                return baseName;

            // 2. Parse the file
            string sourceCode = File.ReadAllText(filePath);
            SyntaxTree tree = CSharpSyntaxTree.ParseText(sourceCode);
            var root = (CompilationUnitSyntax)tree.GetRoot();
            var classDecl = root.DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .FirstOrDefault(c => c.Identifier.ValueText == classIdentifier);

            if (classDecl == null)
                return baseName;

            // 3. Get the expected event signature
            MethodInfo invokeMethod = e.EventType.GetMethod("Invoke");
            ParameterInfo[] eventParams = invokeMethod.GetParameters();
            string returnType = GetFriendlyTypeName(invokeMethod.ReturnType);

            // 4. Collect a list of all used suffixes for methods with the SAME signature.
            //    Suffix 0 is the base name without a digit.
            var usedSuffixes = new List<int>();

            var methodNodes = classDecl.Members.OfType<MethodDeclarationSyntax>().ToList();
            foreach (var method in methodNodes)
            {
                string name = method.Identifier.ValueText;

                // Check if the name fits the pattern "BaseName" or "BaseName_N"
                int suffix = -1;
                if (name == baseName)
                    suffix = 0;
                else if (name.StartsWith(baseName + "_"))
                {
                    string suffixPart = name.Substring(baseName.Length + 1);
                    if (int.TryParse(suffixPart, out int n))
                        suffix = n;
                }

                // If the name matches - verify the signature
                if (suffix != -1)
                {
                    if (method.HasSignature(eventParams, returnType, trs))
                        usedSuffixes.Add(suffix);
                }
            }

            // 5. Find the first available number (0, 1, 2...)
            int bestNumber = usedSuffixes.Contains(0)
                ? FindFirstMissingNumberInSortedArray(usedSuffixes.OrderBy(s => s).ToArray())
                : 0;

            // 6. Form the result
            return bestNumber == 0
                ? baseName
                : $"{baseName}_{bestNumber}";
        }
    }
}
