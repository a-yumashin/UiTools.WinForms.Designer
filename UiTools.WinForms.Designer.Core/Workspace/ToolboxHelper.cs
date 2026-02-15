using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Printing;
using System.IO.Ports;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Windows.Forms;

namespace UiTools.WinForms.Designer.Core
{
    internal class ToolboxHelper
    {
        public const string TOOLBOXITEM_ORIGIN_PROP_NAME = "ToolboxItem_Origin";
        public const string TOOLBOXITEM_ORIGIN_FROM_REF_ASM = "From_Referenced_Assembly";

        private readonly ToolboxTreeView toolboxTreeView1;

        public ToolboxHelper(ToolboxTreeView toolboxTreeView1)
        {
            this.toolboxTreeView1 = toolboxTreeView1;
        }

        public void CreateAndPopulateToolbox(DesignSurfaceEx designer, MyTypeResolutionService trs, Func<string> projectAssemblyNameResolver)
        {
            MyToolboxService tbox = designer.GetToolboxService();
            if (tbox == null)
                return;
            tbox.Toolbox = toolboxTreeView1;
            var itemsFromWinForms = new Dictionary<string, List<ToolboxItem>>
            {
                { "All Windows Forms", new List<ToolboxItem>
                    {
                        AddPointer(),
                    }
                },
                { "Common Controls", new List<ToolboxItem>
                    {
                        AddPointer(),
                        Add(typeof(Button)),
                        Add(typeof(CheckBox)),
                        Add(typeof(CheckedListBox)),
                        Add(typeof(ComboBox)),
                        Add(typeof(DateTimePicker)),
                        Add(typeof(Label)),
                        Add(typeof(LinkLabel)),
                        Add(typeof(ListBox)),
                        Add(typeof(ListView)),
                        Add(typeof(MaskedTextBox)),
                        Add(typeof(MonthCalendar)),
                        Add(typeof(NotifyIcon)),
                        Add(typeof(NumericUpDown)),
                        Add(typeof(PictureBox)),
                        Add(typeof(ProgressBar)),
                        Add(typeof(RadioButton)),
                        Add(typeof(RichTextBox)),
                        Add(typeof(TextBox)),
                        Add(typeof(ToolTip)),
                        Add(typeof(TreeView)),
                        Add(typeof(WebBrowser))
                    }
                },
                { "Containers", new List<ToolboxItem>
                    {
                        AddPointer(),
                        Add(typeof(FlowLayoutPanel)),
                        Add(typeof(GroupBox)),
                        Add(typeof(Panel)),
                        Add(typeof(SplitContainer)),
                        Add(typeof(TabControl)),
                        Add(typeof(TableLayoutPanel))
                    }
                },
                { "Menus && Toolbars", new List<ToolboxItem>
                    {
                        AddPointer(),
                        Add(typeof(ContextMenuStrip)),
                        Add(typeof(MenuStrip)),
                        Add(typeof(StatusStrip)),
                        Add(typeof(ToolStrip)),
                        Add(typeof(ToolStripContainer))
                    }
                },
                { "Data", new List<ToolboxItem>
                    {
                        AddPointer(),
                        Add(typeof(System.Windows.Forms.DataVisualization.Charting.Chart)),
                        Add(typeof(BindingNavigator)),
                        Add(typeof(BindingSource)),
                        Add(typeof(DataGridView)),
                        Add(typeof(DataSet))
                    }
                },
                { "Components", new List<ToolboxItem>
                    {
                        AddPointer(),
                        Add(typeof(BackgroundWorker)),
                        Add(typeof(DirectoryEntry)),
                        Add(typeof(System.DirectoryServices.DirectorySearcher)),
                        Add(typeof(ErrorProvider)),
                        Add(typeof(EventLog)),
                        Add(typeof(FileSystemWatcher)),
                        Add(typeof(HelpProvider)),
                        Add(typeof(ImageList)),
                        //Add(typeof(System.Messaging.MessageQueue)), // needs MSMQ to be installed
                        Add(typeof(PerformanceCounter)),
                        Add(typeof(Process)),
                        Add(typeof(SerialPort)),
                        Add(typeof(System.ServiceProcess.ServiceController)),
                        Add(typeof(Timer))
                    }
                },
                { "Printing", new List<ToolboxItem>
                    {
                        AddPointer(),
                        Add(typeof(PageSetupDialog)),
                        Add(typeof(PrintDialog)),
                        Add(typeof(PrintDocument)),
                        Add(typeof(PrintPreviewControl)),
                        Add(typeof(PrintPreviewDialog))
                    }
                },
                { "Dialogs", new List<ToolboxItem>
                    {
                        AddPointer(),
                        Add(typeof(ColorDialog)),
                        Add(typeof(FolderBrowserDialog)),
                        Add(typeof(FontDialog)),
                        Add(typeof(OpenFileDialog)),
                        Add(typeof(SaveFileDialog))
                    }
                },
                { "WPF Interoperability", new List<ToolboxItem>
                    {
                        AddPointer(),
                        Add(typeof(System.Windows.Forms.Integration.ElementHost))
                    }
                }
            };

            itemsFromWinForms["All Windows Forms"].AddRange(ExtractWinFormsComponents().OrderBy(tbi => tbi.DisplayName));

            var itemsFromReferencedAssemblies = new Dictionary<string, List<ToolboxItem>>();
            var currentProjectAssemblyName = projectAssemblyNameResolver == null
                ? null
                : projectAssemblyNameResolver();
            foreach (var referencedAssembly in trs.GetKnownAssemblyNames())
            {
                if (referencedAssembly.Name.StartsWith("System"))
                    continue;
                var types = ExtractComponentTypes(referencedAssembly, trs, currentProjectAssemblyName);
                if (!types.Any())
                    continue;
                var items = new List<ToolboxItem> { AddPointer() };
                items.AddRange(toolboxTreeView1.CreateComponentItems(types));
                items.ForEach(item => { if (item.DisplayName != "Pointer") item.Properties.Add(TOOLBOXITEM_ORIGIN_PROP_NAME, TOOLBOXITEM_ORIGIN_FROM_REF_ASM); });
                itemsFromReferencedAssemblies.Add(referencedAssembly.Name, items);
            }
            var itemsByCategories = itemsFromReferencedAssemblies.MergeWithOtherDictionary(itemsFromWinForms);
            toolboxTreeView1.Populate(itemsByCategories);
            toolboxTreeView1.DesignerHost = designer.GetDesignerHost();
        }

        private ToolboxItem Add(Type type) // just a shorthand
        {
            return toolboxTreeView1.CreateComponentItem(type);
        }

        private ToolboxItem AddPointer() // just a shorthand
        {
            return toolboxTreeView1.CreatePointer();
        }

        private IEnumerable<Type> ExtractComponentTypes(AssemblyName asmName, MyTypeResolutionService trs, string currentProjectAssemblyName)
        {
            var asm = trs.GetAssembly(asmName);
            if (asm == null)
                return Enumerable.Empty<Type>();
            return asm.GetTypes()
                .Where(t => (t.IsPublic || (currentProjectAssemblyName != null && t.IsInternal() &&
                                            string.Equals(asmName.Name, currentProjectAssemblyName, StringComparison.OrdinalIgnoreCase))) &&
                            !t.IsAbstract && !t.IsGenericType && t.HasDefaultCtor())
                .Where(t => typeof(IComponent).IsAssignableFrom(t) &&
                            !t.IsSubclassOf(typeof(Form)) &&
                            t.ShowAsToolboxItem() && t.IsDesignTimeVisible() && t.IsBrowsable());
        }

        private List<ToolboxItem> ExtractWinFormsComponents()
        {
            var items = new List<ToolboxItem>();
            var asm = typeof(Button).Assembly; // assembly System.Windows.Forms.dll
            var candidateTypes = asm.GetTypes()
                .Where(t => t.IsPublic && !t.IsAbstract && !t.IsGenericType && t.HasDefaultCtor())
                .Where(t => typeof(IComponent).IsAssignableFrom(t) &&
                            !t.IsSubclassOf(typeof(Form)) &&
                            !typeof(IDataGridViewEditingControl).IsAssignableFrom(t));
            foreach (Type t in candidateTypes)
            {
                if (t.Name.In("Control", "UserControl", "ContainerControl", "ScrollableControl", "ToolStripDropDown", "ToolStripDropDownMenu", "ToolStripPanel"))
                    continue; // couldn't find a way to filter out these components based on their attributes or any other formal criteria, so had to hardcode their names
                if (!t.ShowAsToolboxItem() || !t.IsDesignTimeVisible() || !t.IsBrowsable())
                    continue;
                var item = new ToolboxItem(t);
                var imageAttr = (ToolboxBitmapAttribute)TypeDescriptor.GetAttributes(t)[typeof(ToolboxBitmapAttribute)];
                if (imageAttr != null)
                {
                    item.Bitmap = (Bitmap)imageAttr.GetImage(t);
                    item.OriginalBitmap = (Bitmap)imageAttr.GetImage(t);
                }
                items.Add(item);
            }
            return items;
        }
    }
}
