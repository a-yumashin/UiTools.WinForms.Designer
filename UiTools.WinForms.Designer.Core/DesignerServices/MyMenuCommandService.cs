using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using UiTools.WinForms.Designer.Core.Properties;

namespace UiTools.WinForms.Designer.Core
{
    public class MyMenuCommandService : IMenuCommandService
    {
        public event EventHandler<string> CustomMenuCommandExecuted;

        private readonly DesignSurfaceEx designSurface;
        private readonly MenuCommandService menuCommandService;
        private Func<Type, Bitmap> componentTypeIconResolver;

        public MyMenuCommandService(DesignSurfaceEx designSurface)
        {
            this.designSurface = designSurface;
            menuCommandService = new MenuCommandService(designSurface);
        }

        public void SetComponentTypeIconResolver(Func<Type, Bitmap> componentTypeIconResolver)
        {
            this.componentTypeIconResolver = componentTypeIconResolver;
        }

        public void ShowContextMenu(CommandID menuID, int x, int y)
        {
            var designerHost = designSurface.GetDesignerHost();
            var selectionService = designSurface.GetSelectionService();
            var referenceService = designSurface.GetReferenceService();

            if (designerHost == null || selectionService == null || referenceService == null)
            {
                // Cannot provide a full context menu without these services.
                return;
            }

            var view = designSurface.View as Control;
            if (view == null)
                return;
            
            var screenPoint = new Point(x, y);
            var tray = view.FindControlByType("System.Windows.Forms.Design.ComponentTray");
            bool isTrayClick = false;
            if (tray != null && tray.Visible)
            {
                var trayRect = tray.RectangleToScreen(tray.ClientRectangle);
                isTrayClick = trayRect.Contains(screenPoint);
            }

            var menu = new ThemedContextMenuStrip();
            ThemeApplier.Apply(menu, CommonStuff.CurrentUiTheme);
            var isDarkTheme = ThemeApplier.IsDark(menu.BackColor);

            if (isTrayClick)
            {
                PopulateTrayMenuItems(menu, tray, isDarkTheme);
            }
            else
            {
                var primarySelection = selectionService.PrimarySelection as IComponent;

                if (primarySelection is Control currentControl && currentControl != designerHost.RootComponent)
                {
                    AddStandardCommand(menu, StandardCommands.BringToFront, "Bring to Front", isDarkTheme ? Resources.BringToFront_DarkTheme : Resources.BringToFront);
                    AddStandardCommand(menu, StandardCommands.SendToBack, "Send to Back", isDarkTheme ? Resources.SendToBack_DarkTheme : Resources.SendtoBack);
                    menu.Items.Add(new ToolStripSeparator());
                    // Add "Select 'Parent'" menu items
                    var current = currentControl;
                    while (current != null && current != designerHost.RootComponent)
                    {
                        var parentControl = current.IsTopLevelComponent(designerHost)
                            ? current.Parent
                            : referenceService.GetComponent(current) as Control;
                        if (parentControl != null && parentControl != current)
                        {
                            current = parentControl;
                            var parentName = parentControl.GetQualifiedControlName(designerHost);
                            var selectParentMenuItem = new ToolStripMenuItem($"Select '{parentName}'")
                            {
                                Tag = parentControl,
                                Image = TryToFindControlTypeImage(parentControl.GetType(), isDarkTheme)
                            };
                            selectParentMenuItem.Click += (s, args) =>
                            {
                                selectionService.SetSelectedComponents(new IComponent[] { (IComponent)((ToolStripMenuItem)s).Tag });
                            };
                            menu.Items.Add(selectParentMenuItem);
                        }
                        else
                            break;
                    }
                    menu.Items.Add(new ToolStripSeparator());
                }

                if (primarySelection == designerHost.RootComponent)
                    AddStandardCommand(menu, StandardCommands.Paste, "Paste", isDarkTheme ? Resources.Paste_DarkTheme : Resources.Paste,
                        Keys.Control | Keys.V, DesignSurfaceEx.HasDesignerComponentInClipboard());
                else
                {
                    AddStandardCommand(menu, StandardCommands.Cut, "Cut", isDarkTheme ? Resources.Cut_DarkTheme : Resources.Cut,
                        Keys.Control | Keys.X);
                    AddStandardCommand(menu, StandardCommands.Copy, "Copy", isDarkTheme ? Resources.Copy_DarkTheme : Resources.Copy,
                        Keys.Control | Keys.C);
                    AddStandardCommand(menu, StandardCommands.Paste, "Paste", isDarkTheme ? Resources.Paste_DarkTheme : Resources.Paste,
                        Keys.Control | Keys.V, DesignSurfaceEx.HasDesignerComponentInClipboard());
                    AddStandardCommand(menu, StandardCommands.Delete, "Delete", isDarkTheme ? Resources.Delete_DarkTheme : Resources.Delete,
                        Keys.Delete);
                }
            }
            menu.Items.Add(new ToolStripSeparator());
            AddCustomCommand(menu, "Properties", isDarkTheme ? Resources.Property_DarkTheme : Resources.Property);

            menu.Show(view, view.PointToClient(screenPoint));
        }

        private void PopulateTrayMenuItems(ContextMenuStrip menu, Control tray, bool isDarkTheme)
        {
            AddStandardCommand(menu, StandardCommands.Paste, "Paste", isDarkTheme ? Resources.Paste_DarkTheme : Resources.Paste,
                Keys.Control | Keys.V, DesignSurfaceEx.HasDesignerComponentInClipboard());
            menu.Items.Add(new ToolStripSeparator());

            // Item "Line Up Icons":
            var lineUpItem = new ToolStripMenuItem("Line Up Icons");
            lineUpItem.Click += (s, e) => {
                var method = tray.GetType().GetMethod("DoLineupIcons", BindingFlags.Instance | BindingFlags.NonPublic);
                if (method != null)
                    method.Invoke(tray, null);
            };

            var componentTray = tray as System.Windows.Forms.Design.ComponentTray;

            // Item "Auto Arrange Icons":
            var autoArrangeItem = new ToolStripMenuItem("Auto Arrange Icons");
            autoArrangeItem.CheckOnClick = true;
            autoArrangeItem.Checked = componentTray.AutoArrange;
            autoArrangeItem.CheckedChanged += (s, e) => componentTray.AutoArrange = autoArrangeItem.Checked;

            menu.Items.Add(lineUpItem);
            menu.Items.Add(autoArrangeItem);
        }

        private void AddStandardCommand(ContextMenuStrip menu, CommandID commandID, string menuItemText, Image image, Keys shortcutKeys = Keys.None, bool enabled = true)
        {
            MenuCommand command = FindCommand(commandID);
            if (command != null && command.Supported)
            {
                var menuItem = new ToolStripMenuItem(menuItemText)
                {
                    Enabled = command.Enabled && enabled,
                    Image = image,
                    ShortcutKeys = shortcutKeys,
                    Tag = command
                };
                menuItem.Click += OnMenuClicked;
                menu.Items.Add(menuItem);
            }
        }

        private void AddCustomCommand(ContextMenuStrip menu, string menuItemText, Image image, bool enabled = true)
        {
            var menuItem = new ToolStripMenuItem(menuItemText)
            {
                Enabled = enabled,
                Image = image
            };
            menuItem.Click += OnMenuClicked;
            menu.Items.Add(menuItem);
        }

        private Image TryToFindControlTypeImage(Type controlType, bool isDarkTheme)
        {
            // Tries to find appropriate image for "Select 'Parent'" menu items.
            // The problem is that some controls are not shown in the Toolbox - e.g. Form, TabPage and the non-toplevel ones (such as SplitterPanel).
            Image img = null;
            if (componentTypeIconResolver != null)
            {
                // try to pick image from the Toolbox:
                img = componentTypeIconResolver(controlType);
            }
            if (img == null)
            {
                // try to pick image for "special cases":
                if (controlType == typeof(Form))
                    img = isDarkTheme ? Resources.Form_DarkTheme : Resources.Form;
                else if (controlType == typeof(UserControl))
                    img = isDarkTheme ? DarkThemeToolboxItems.UserControl : LightThemeToolboxItems.UserControl;
                else if (controlType == typeof(TabPage))
                    img = isDarkTheme ? Resources.TabPage_DarkTheme : Resources.TabPage;
                else if (controlType == typeof(SplitterPanel))
                    img = isDarkTheme ? Resources.SplitterPanel_DarkTheme : Resources.SplitterPanel;
                else
                {
                    // fallback to the "built-in" image (better than nothing):
                    var imageAttr = (ToolboxBitmapAttribute)TypeDescriptor.GetAttributes(controlType)[typeof(ToolboxBitmapAttribute)];
                    img = imageAttr?.GetImage(controlType);
                }
            }
            return img;
        }

        private void OnMenuClicked(object sender, EventArgs e)
        {
            var menuItem = sender as ToolStripMenuItem;
            if (menuItem.Tag is MenuCommand command)
                command.Invoke();
            else
                CustomMenuCommandExecuted?.Invoke(this, menuItem.Text);
        }

        public void AddCommand(MenuCommand command)
        {
            menuCommandService.AddCommand(command);
        }

        public void AddVerb(DesignerVerb verb)
        {
            menuCommandService.AddVerb(verb);
        }

        public MenuCommand FindCommand(CommandID commandID)
        {
            return menuCommandService.FindCommand(commandID);
        }

        public bool GlobalInvoke(CommandID commandID)
        {
            return menuCommandService.GlobalInvoke(commandID);
        }

        public void RemoveCommand(MenuCommand command)
        {
            menuCommandService.RemoveCommand(command);
        }

        public void RemoveVerb(DesignerVerb verb)
        {
            menuCommandService.RemoveVerb(verb);
        }

        public DesignerVerbCollection Verbs => menuCommandService.Verbs;
    }
}
