using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Windows.Forms;

namespace UiTools.WinForms.Designer.Core
{
    internal class MyNameCreationService : INameCreationService
    {
        public string CreateName(IContainer container, Type type)
        {
            if (container == null)
                return string.Empty;
            
            var compName = CreateName(type); // e.g. DataGridView --> dataGridView
            var suffixes = container.Components.OfType<Component>()
                .Where(c => c.GetType() == type && c.Site.Name.StartsWith(compName) && c.Site.Name != compName)
                .Select(c => int.TryParse(c.Site.Name.Substring(compName.Length), out int n) ? n : -1)
                .Where(n => n >= 0)
                .OrderBy(s => s)
                .ToList();
            if (suffixes.Count == 0)
                return $"{compName}1";
            if (suffixes[0] > 1)
                suffixes.Insert(0, 0);
            return $"{compName}{CommonStuff.FindFirstMissingNumberInSortedArray(suffixes.ToArray())}";
        }

        public bool IsValidName(string name)
        {
            return !string.IsNullOrEmpty(name) && char.IsLetter(name, 0) && !name.StartsWith("_");
        }

        public void ValidateName(string name)
        {
            if (!IsValidName(name))
                throw new ArgumentException($"{nameof(MyNameCreationService)}.{nameof(IsValidName)}: Invalid name: '{name}'");
        }

        private string CreateName(Type type)
        {
            return type == typeof(Form) || type == typeof(UserControl)
                ? type.Name
                : type.Name.Substring(0, 1).ToLower() + type.Name.Substring(1); // e.g. DataGridView --> dataGridView
        }
    }
}
