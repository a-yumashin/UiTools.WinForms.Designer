using System;
using System.Reflection;

namespace UiTools.WinForms.Designer.Core
{
    /// <summary>
    /// This "proxy" class is used to access internal properties of the project-level resource class (usually Properties.Resources.Designer.cs),
    /// i.e. to resources stored in the Properties\***.resx file (usually Properties\Resources.resx). An instance of this class is created only for types
    /// marked with the [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", ...)] attribute — see the
    /// MyTypeResolutionService.IsStronglyTypedResourceClass() method; and the Properties.Resources class is one of such types.
    /// </summary>
    internal class InternalAccessibleType : TypeDelegator
    {
        public InternalAccessibleType(Type delegatingType) : base(delegatingType) { }

        protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            // Add NonPublic to gain access to internal properties:
            return base.GetPropertyImpl(name, bindingAttr | BindingFlags.NonPublic, binder, returnType, types, modifiers);
        }

        public override FieldInfo GetField(string name, BindingFlags bindingAttr)
        {
            return base.GetField(name, bindingAttr | BindingFlags.NonPublic);
        }
    }
}
