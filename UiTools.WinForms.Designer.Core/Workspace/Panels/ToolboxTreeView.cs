using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using UiTools.WinForms.Designer.Core.Properties;

namespace UiTools.WinForms.Designer.Core
{
    public partial class ToolboxTreeView : UserControl, IMyToolbox
    {
        private static readonly Color DefaultDisabledBackColor = ColorTranslator.FromHtml("#FBFBFB");
        private static readonly Color DefaultDisabledForeColor = SystemColors.GrayText;
        
        private static readonly Color DefaultHoverBackColor = Color.FromArgb(201, 222, 245);
        private static readonly Color DefaultHoverForeColor = Color.Black;
        private static readonly Color DefaultSelectedBackColor = SystemColors.Highlight;
        private static readonly Color DefaultSelectedForeColor = SystemColors.HighlightText;

        private static readonly Color DefaultSearchButtonHoverBackColor = ColorTranslator.FromHtml("#C9DEF5");

        // NOTE: moving ToolboxTreeView control to "Workspace" project folder resulted in MissingManifestResourceException when assigning ImageList.ImageStream property.
        //       The solution was to introduce the "EmbeddedResourceUseDependentUponConvention" setting (set to true) in the .csproj file.
        public event EventHandler<ToolboxItem> ToolboxItemDoubleClick;

        private bool isSearchButtonHovered = false;
        private TreeNode hoveredNode = null;
        private Dictionary<string, List<ToolboxItem>> itemsByCategories;
        private bool isDarkTheme = false;

        public ToolboxTreeView()
        {
            InitializeComponent();

            treeView1.FullRowSelect = true;
            treeView1.HideSelection = false;
            treeView1.ShowLines = false;
            treeView1.ShowNodeToolTips = true;

            float scaleFactor = DeviceDpi / 120f;
            treeView1.Indent = (int)(22 * scaleFactor);
            treeView1.ItemHeight = (int)(26 * scaleFactor);
            treeView1.Tag = "NoTheme";

            treeView1.DrawMode = TreeViewDrawMode.OwnerDrawAll;
            treeView1.DrawNode += treeView1_DrawNode;
            treeView1.MouseMove += treeView1_MouseMove;
            treeView1.MouseLeave += treeView1_MouseLeave;
            treeView1.KeyDown += ProcessKeyDownEvent;
            treeView1.MouseDown += treeView1_MouseDown;
            treeView1.NodeMouseClick += treeView1_NodeMouseClick;
            treeView1.NodeMouseDoubleClick += treeView1_NodeMouseDoubleClick;

            cboSearch.TabStop = false;
            cboSearch.CueBannerText = "Search Toolbox";
            cboSearch.TextChanged += cboSearch_TextChanged;
            cboSearch.LostFocus += cboSearch_LostFocus;
            cboSearch.KeyDown += ProcessKeyDownEvent;
            cboSearch.BackColorChanged += (s, e) => UpdateSearchButtonBackColor();
            cboSearch.EnabledChanged += (s, e) => UpdateSearchButtonBackColor();

            picSearch.SizeMode = PictureBoxSizeMode.CenterImage;
            //picSearch.Height = cboSearch.Height - 2; // has no effect here; moved to OnHandleCreated()
            picSearch.Width = (int)(22 * scaleFactor);
            picSearch.Tag = "NoTheme";
            picSearch.MouseEnter += picSearch_MouseEnter;
            picSearch.MouseLeave += picSearch_MouseLeave;
            picSearch.Click += picSearch_Click;
            UpdateSearchButtonToolTipAndImage();
            picSearch.Visible = false; // to prevent "jumping" of its centered image on nearest picSearch.Height change (in OnHandleCreated)
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            BeginInvoke(() =>
            {
                picSearch.Height = cboSearch.Height - 2;
                picSearch.Visible = true;
                ThemeApplier.ApplyScrollBarTheme(treeView1, IsDarkTheme);
                // Fix issue with cboSearch cue banner:
                _ = Task.Run(async () =>
                {
                    await Task.Delay(100);
                    BeginInvoke(cboSearch.Refresh);
                });
            });
            UpdateTreeViewBackColor();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            if (IsHandleCreated)
                BeginInvoke(() => picSearch.Height = cboSearch.Height - 2);
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            if (IsHandleCreated)
                BeginInvoke(UpdateTreeViewBackColor);
        }

        protected override void OnForeColorChanged(EventArgs e)
        {
            base.OnForeColorChanged(e);
            if (IsHandleCreated)
                BeginInvoke(UpdateTreeViewForeColor);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            if (IsHandleCreated)
                BeginInvoke(() => { UpdateTreeViewBackColor(); UpdateTreeViewForeColor(); });
        }

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (ToolboxItemDoubleClick == null || e.Button != MouseButtons.Left || e.Node.Parent == null || e.Node.Text == "Pointer")
                return;
            ToolboxItemDoubleClick(sender, e.Node.Tag as ToolboxItem);
        }

        private void PopulateTree(string filter = null)
        {
            treeView1.Nodes.Clear();
            foreach (var kvp in itemsByCategories)
            {
                var categoryNode = treeView1.Nodes.Add(kvp.Key);
                foreach (var item in kvp.Value)
                {
                    if (filter == null || item.DisplayName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        var itemNode = categoryNode.Nodes.Add(item.DisplayName);
                        itemNode.Tag = item;
                        var toolType = item.GetType(/*DesignerHost*/null);
                        itemNode.ToolTipText = toolType == null // will be null for "Pointer" items
                            ? item.DisplayName
                            : ComposeComponentToolTipText(toolType);
                        if (toolType != null)
                        {
                            bool isFromReferencedAssembly =
                                item?.Properties[ToolboxHelper.TOOLBOXITEM_ORIGIN_PROP_NAME]?.ToString() == ToolboxHelper.TOOLBOXITEM_ORIGIN_FROM_REF_ASM;
                            item.Bitmap = item.OriginalBitmap = PickToolboxItemImage(toolType, isFromReferencedAssembly);
                        }
                    }
                }
                if (categoryNode.Nodes.Count == 0)
                    categoryNode.Remove();
                else if (filter != null)
                    categoryNode.Expand();
            }
        }

        private Bitmap PickToolboxItemImage(Type toolType, bool isFromReferencedAssembly)
        {
            // Try to pick image from resources:
            object image = isDarkTheme
                ? (isFromReferencedAssembly ? DarkThemeToolboxItems.UserControl : DarkThemeToolboxItems.ResourceManager.GetObject(toolType.Name))
                : (isFromReferencedAssembly ? LightThemeToolboxItems.UserControl : LightThemeToolboxItems.ResourceManager.GetObject(toolType.Name));
            if (image != null)
                return (Bitmap)image;
            
            // Try to fallback to "built-in" default image:
            var imageAttr = (ToolboxBitmapAttribute)TypeDescriptor.GetAttributes(toolType)[typeof(ToolboxBitmapAttribute)];
            if (imageAttr != null)
                return (Bitmap)imageAttr.GetImage(toolType);

            // Use stub image as last chance:
            return isDarkTheme ? DarkThemeToolboxItems.UserControl : LightThemeToolboxItems.UserControl;
        }

        public Bitmap GetComponentTypeImage(Type componentType)
        {
            return itemsByCategories == null
                ? null
                : (itemsByCategories
                    .SelectMany(kvp => kvp.Value)
                    .FirstOrDefault(t => t.GetType(/*DesignerHost*/null) == componentType)?
                    .Bitmap);
        }

        private void ApplyUiThemeToToolboxItemImages()
        {
            if (itemsByCategories == null)
                return;
            foreach (var kvp in itemsByCategories)
            {
                foreach (var item in kvp.Value)
                {
                    var toolType = item.GetType(/*DesignerHost*/null);
                    if (toolType != null)
                    {
                        bool isFromReferencedAssembly =
                            item?.Properties[ToolboxHelper.TOOLBOXITEM_ORIGIN_PROP_NAME]?.ToString() == ToolboxHelper.TOOLBOXITEM_ORIGIN_FROM_REF_ASM;
                        item.Bitmap = item.OriginalBitmap = PickToolboxItemImage(toolType, isFromReferencedAssembly);
                    }
                    else // that's a "Pointer"
                        item.Bitmap = isDarkTheme ? DarkThemeToolboxItems.Pointer : LightThemeToolboxItems.Pointer;
                }
            }
        }

        public IEnumerable<string> GetComponentTypeNamesFromReferencedAssemblies()
        {
            return treeView1.Nodes.Cast<TreeNode>()
                .SelectMany(node => node.Nodes.Cast<TreeNode>())
                .Where(subNode =>
                    (subNode.Tag as ToolboxItem)?.Properties[ToolboxHelper.TOOLBOXITEM_ORIGIN_PROP_NAME]?.ToString() == ToolboxHelper.TOOLBOXITEM_ORIGIN_FROM_REF_ASM)
                .Select(subNode => subNode.Text)
                .Distinct()
                .OrderBy(p => p);
        }

        private static string ComposeComponentToolTipText(Type componentType)
        {
            var sb = new StringBuilder();
            sb.AppendLine(componentType.Name);
            sb.Append("Version ");
            sb.Append(componentType.Assembly.GetName().Version); // NOTE: not sure I'm picking the right Version
            object[] attribs = componentType.Assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), true);
            if (attribs.Length > 0)
            {
                sb.Append(" from ");
                sb.Append(((AssemblyCompanyAttribute)attribs[0]).Company);
            }
            sb.AppendLine();
            sb.AppendLine(".NET Component");
            var desc = XmlDocumentationHelper.GetTypeDescription(componentType);
            if (desc != null)
            {
                sb.AppendLine();
                sb.AppendLine(WrapText(desc, 60));
            }
            return sb.ToString();
        }

        private static string WrapText(string text, int maxCharsPerLine)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            text = Regex.Replace(text, @"\s+", " ").Trim();

            if (text.Length <= maxCharsPerLine)
                return text;

            var sb = new StringBuilder();
            int position = 0;

            while (position < text.Length)
            {
                if (text.Length - position <= maxCharsPerLine)
                {
                    sb.Append(text.Substring(position).Trim());
                    break;
                }

                int lookLength = maxCharsPerLine;
                int breakPos = text.LastIndexOf(' ', position + lookLength, lookLength);

                if (breakPos <= position)
                {
                    breakPos = text.IndexOf(' ', position + lookLength);
                    if (breakPos == -1)
                    {
                        sb.Append(text.Substring(position).Trim());
                        break;
                    }
                }

                string line = text.Substring(position, breakPos - position).Trim();
                if (!string.IsNullOrEmpty(line))
                    sb.AppendLine(line);

                position = breakPos + 1;
            }

            return sb.ToString();
        }

        private void SetFilter(string filterString)
        {
            PopulateTree(filterString);
            if (treeView1.Nodes.Count > 0)
            {
                labNoResults.Visible = false;
                if (treeView1.Nodes[0].Nodes.Count > 0) // this check is redundant (thanks to the PopulateTree method code), but let it be
                    treeView1.SelectedNode = treeView1.Nodes[0].Nodes[0];
            }
            else
                labNoResults.Visible = true;
        }

        private void RemoveFilter()
        {
            labNoResults.Visible = false;
            PopulateTree(string.Empty);
            treeView1.CollapseAll();
        }

        private void ProcessKeyDownEvent(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape && !string.IsNullOrEmpty(cboSearch.Text))
                cboSearch.Text = string.Empty; // will lead to calling the RemoveFilter() method and updating the search button
        }

        private void treeView1_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            bool isHovered = (e.Node == hoveredNode && e.Node.Level > 0);
            bool isSelected = (e.State & TreeNodeStates.Selected) == TreeNodeStates.Selected;
            bool isExpanded = e.Node.IsExpanded;
            float scaleFactor = e.Graphics.DpiY / 120f;
            float textPadding = 6 * scaleFactor;

            Color backColor = BackColor;
            Color textColor = ForeColor;

            if (Enabled)
            {
                if (isSelected)
                {
                    backColor = SelectedBackColor;
                    textColor = SelectedForeColor;
                }
                else if (isHovered)
                {
                    backColor = HoverBackColor;
                    textColor = HoverForeColor;
                }
            }
            else
            {
                backColor = DisabledBackColor;
                textColor = DisabledForeColor;
            }

            using (Brush backBrush = new SolidBrush(backColor))
            using (Brush textBrush = new SolidBrush(textColor))
            {
                // Draw background across the entire row width:
                Rectangle fullRowBounds = new Rectangle(0, e.Bounds.Y, treeView1.Width, e.Bounds.Height);
                e.Graphics.FillRectangle(backBrush, fullRowBounds);

                if (e.Node.Level == 0)
                {
                    // This is a root node - draw a custom icon (instead of "plus" and "minus"):
                    var resourceName = isExpanded ? "ExpandedTreeNode" : "CollapsedTreeNode";
                    if (isSelected)
                        resourceName = "SelectedAnd" + resourceName;
                    if (IsDarkTheme)
                        resourceName += "_DarkTheme";
                    var img = (Image)Resources.ResourceManager.GetObject(resourceName);
                    float iconPadding = 3 * scaleFactor;
                    e.Graphics.DrawImage(img, e.Bounds.X + iconPadding, e.Bounds.Y + (e.Bounds.Height - img.Height) / 2, img.Width * scaleFactor, img.Height * scaleFactor);
                    // Draw text:
                    var textBounds = e.Bounds;
                    textBounds.X += img.Width + (int)textPadding; // indent text after the icon
                    TextRenderer.DrawText(e.Graphics, e.Node.Text, e.Node.TreeView.Font, textBounds, textColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
                }
                else
                {
                    // This is a non-root node
                    int currentX = e.Bounds.X + treeView1.Indent; // indent before the icon
                    // Draw the node's icon (if any):
                    Image nodeImage = (e.Node.Tag as ToolboxItem).Bitmap;
                    if (nodeImage != null && e.Bounds.Height > 0) // (it's unclear why e.Bounds.Height is *sometimes* 0 here)
                    {
                        float iconOffsetY = -5 * scaleFactor + 5; // tested with scaleFactor 125% and 200% (the latter - on 4K display)
                        e.Graphics.DrawImage(nodeImage, currentX, e.Bounds.Y + (e.Bounds.Height - nodeImage.Height) / 2 + iconOffsetY, nodeImage.Width * scaleFactor, nodeImage.Height * scaleFactor);
                        currentX += nodeImage.Width + 2; // indent before the text
                    }
                    // Draw text:
                    Rectangle nodeTextBounds = new Rectangle(currentX + (int)textPadding, e.Bounds.Y, e.Bounds.Width - (currentX - e.Bounds.X), e.Bounds.Height);
                    TextRenderer.DrawText(e.Graphics, e.Node.Text, e.Node.TreeView.Font, nodeTextBounds, textColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
                }
            }
        }

        private void treeView1_MouseMove(object sender, MouseEventArgs e)
        {
            var node = treeView1.GetNodeAt(e.X, e.Y);
            if (node != hoveredNode)
            {
                // Redraw previous hoveredNode
                if (hoveredNode != null)
                {
                    Rectangle prevFullRowBounds = new Rectangle(0, hoveredNode.Bounds.Y, treeView1.Width, hoveredNode.Bounds.Height);
                    treeView1.Invalidate(prevFullRowBounds);
                }

                hoveredNode = node;

                // Redraw new hoveredNode
                if (hoveredNode != null)
                {
                    // Invalidate(fullRowBounds) for the new node
                    Rectangle newFullRowBounds = new Rectangle(0, hoveredNode.Bounds.Y, treeView1.Width, hoveredNode.Bounds.Height);
                    treeView1.Invalidate(newFullRowBounds);
                }
            }
            OnMouseMove(e); // needed for drag'n'drop
        }

        private void treeView1_MouseLeave(object sender, EventArgs e)
        {
            if (hoveredNode != null)
            {
                // Redraw previous hoveredNode
                Rectangle prevFullRowBounds = new Rectangle(0, hoveredNode.Bounds.Y, (sender as TreeView).Width, hoveredNode.Bounds.Height);
                treeView1.Invalidate(prevFullRowBounds);
                hoveredNode = null;
            }
        }

        private void treeView1_MouseDown(object sender, MouseEventArgs e)
        {
            // On MouseDown - select tree node immediately:
            TreeNode nodeClicked = treeView1.GetNodeAt(e.X, e.Y);
            treeView1.SelectedNode = nodeClicked == null
                ? null
                : nodeClicked;
            OnMouseDown(e); // needed for drag'n'drop
        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;
            var hitTest = e.Node.TreeView.HitTest(e.Location);
            if (hitTest.Location == TreeViewHitTestLocations.PlusMinus)
                return;
            if (e.Node.IsExpanded)
                e.Node.Collapse();
            else
                e.Node.Expand();
        }

        Color picSearchBackColor;
        private void picSearch_MouseEnter(object sender, EventArgs e)
        {
            picSearchBackColor = picSearch.BackColor; // store color
            if (!isDarkTheme)
                picSearch.BackColor = SearchButtonHoverBackColor;
            isSearchButtonHovered = true;
            UpdateSearchButtonToolTipAndImage();
        }

        private void picSearch_MouseLeave(object sender, EventArgs e)
        {
            if (!isDarkTheme)
                picSearch.BackColor = picSearchBackColor; // restore color
            isSearchButtonHovered = false;
            UpdateSearchButtonToolTipAndImage();
        }

        private void picSearch_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(cboSearch.Text))
                cboSearch.Text = string.Empty;
            cboSearch.Focus();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            float scaleFactor = DeviceDpi / 120f;
            picSearch.Left = cboSearch.Right - picSearch.Width - (int)(21 * scaleFactor);
            picSearch.Top = 1;
            labNoResults.Top = cboSearch.Bottom + 20;
            labNoResults.Left = (ClientSize.Width - labNoResults.Width) / 2;
        }

        private void cboSearch_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(cboSearch.Text))
                RemoveFilter();
            else
                SetFilter(cboSearch.Text);
            UpdateSearchButtonToolTipAndImage();
        }

        private void UpdateSearchButtonBackColor()
        {
            picSearch.BackColor = cboSearch.Enabled
                ? cboSearch.BackColor
                : cboSearch.DisabledBackColor;
        }

        private void UpdateTreeViewBackColor()
        {
            treeView1.BackColor = labNoResults.BackColor = Enabled ? BackColor : DisabledBackColor;
            // NOTE: We explicitly set 'labNoResults.BackColor' to match the TreeView's background. 
            //       Relying on 'Color.Transparent' for 'labNoResults' does not yield the expected visual result
            //       when the underlying TreeView is in a disabled state due to WinForms' transparency limitations.
        }

        private void UpdateTreeViewForeColor()
        {
            treeView1.ForeColor = labNoResults.ForeColor = Enabled ? ForeColor : DisabledForeColor;
        }

        private void UpdateSearchButtonToolTipAndImage()
        {
            bool filterPresent = !string.IsNullOrEmpty(cboSearch.Text);
            toolTip1.SetToolTip(picSearch, filterPresent ? "Clear search" : "Search");
            var resourceName = filterPresent ? "ClearSearch" : "Search";
            if (isSearchButtonHovered)
                resourceName = "Hovered" + resourceName;
            if (IsDarkTheme)
                resourceName += "_DarkTheme";
            picSearch.Image = (Image)Resources.ResourceManager.GetObject(resourceName);
        }

        private void cboSearch_LostFocus(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(cboSearch.Text))
                return;
            int idx = cboSearch.Items.IndexOf(cboSearch.Text);
            if (idx == 0)
                return; // found: already on top of the list - nothing to do
            if (idx > 0)
                cboSearch.Items.RemoveAt(idx); // found: remove from the list (as it's not on top of the list)
            cboSearch.Items.Insert(0, cboSearch.Text); // add to the top of the list
        }

        public ToolboxItem CreatePointer()
        {
            var bmp = isDarkTheme ? DarkThemeToolboxItems.Pointer : LightThemeToolboxItems.Pointer;
            return new ToolboxItem
            {
                DisplayName = "Pointer",
                Bitmap = bmp,
                OriginalBitmap = bmp
            };
        }

        public ToolboxItem CreateComponentItem(Type type)
        {
            return new ToolboxItem(type);
        }

        public List<ToolboxItem> CreateComponentItems(IEnumerable<Type> types)
        {
            return types.Select(type => CreateComponentItem(type)).ToList();
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IDesignerHost DesignerHost { get; set; }

        public void Populate(Dictionary<string, List<ToolboxItem>> itemsByCategories)
        {
            this.itemsByCategories = itemsByCategories;
            PopulateTree();
        }

        public void Clear()
        {
            treeView1.Nodes.Clear();
        }

        [Category("Appearance")]
        [DefaultValue(false)]
        public bool IsDarkTheme
        {
            get => isDarkTheme;
            set
            {
                if (isDarkTheme != value)
                {
                    isDarkTheme = value;
                    UpdateSearchButtonToolTipAndImage();
                    UpdateSearchButtonBackColor();
                    ApplyUiThemeToToolboxItemImages();
                    ThemeApplier.ApplyScrollBarTheme(treeView1, isDarkTheme);
                }
            }
        }

        public override Color BackColor { get; set; } = SystemColors.Window;
        public override Color ForeColor { get; set; } = SystemColors.ControlText;

        [Category("Appearance")]
        public Color DisabledBackColor { get; set; } = DefaultDisabledBackColor;
        [Category("Appearance")]
        public Color DisabledForeColor { get; set; } = DefaultDisabledForeColor;

        [Category("Appearance")]
        public Color HoverBackColor { get; set; } = DefaultHoverBackColor;
        [Category("Appearance")]
        public Color HoverForeColor { get; set; } = DefaultHoverForeColor;
        [Category("Appearance")]
        public Color SelectedBackColor { get; set; } = DefaultSelectedBackColor;
        [Category("Appearance")]
        public Color SelectedForeColor { get; set; } = DefaultSelectedForeColor;

        [Category("Appearance")]
        public Color SearchButtonHoverBackColor { get; set; } = DefaultSearchButtonHoverBackColor;

        #region Support for default values of Color properties

        private bool ShouldSerializeDisabledBackColor() => DisabledBackColor != DefaultDisabledBackColor;
        private void ResetDisabledBackColor() => DisabledBackColor = DefaultDisabledBackColor;

        private bool ShouldSerializeDisabledForeColor() => DisabledForeColor != DefaultDisabledForeColor;
        private void ResetDisabledForeColor() => DisabledForeColor = DefaultDisabledForeColor;

        private bool ShouldSerializeHoverBackColor() => HoverBackColor != DefaultHoverBackColor;
        private void ResetHoverBackColor() => HoverBackColor = DefaultHoverBackColor;

        private bool ShouldSerializeHoverForeColor() => HoverForeColor != DefaultHoverForeColor;
        private void ResetHoverForeColor() => HoverForeColor = DefaultHoverForeColor;

        private bool ShouldSerializeSelectedBackColor() => SelectedBackColor != DefaultSelectedBackColor;
        private void ResetSelectedBackColor() => SelectedBackColor = DefaultSelectedBackColor;

        private bool ShouldSerializeSelectedForeColor() => SelectedForeColor != DefaultSelectedForeColor;
        private void ResetSelectedForeColor() => SelectedForeColor = DefaultSelectedForeColor;

        private bool ShouldSerializeSearchButtonHoverBackColor() => SearchButtonHoverBackColor != DefaultSearchButtonHoverBackColor;
        private void ResetSearchButtonHoverBackColor() => SearchButtonHoverBackColor = DefaultSearchButtonHoverBackColor;

        #endregion Support for default values of Color properties

        #region IMyToolbox interface members

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ToolboxItem SelectedItem
        {
            get => (ToolboxItem)treeView1.SelectedNode?.Tag;
            set
            {
                if (value == null)
                {
                    treeView1.SelectedNode = null;
                    return;
                }
                foreach (TreeNode categoryNode in treeView1.Nodes)
                    foreach (TreeNode itemNode in categoryNode.Nodes)
                    {
                        var item = (ToolboxItem)itemNode.Tag;
                        if (Equals(item, value))
                        {
                            itemNode.Parent.Expand();
                            treeView1.SelectedNode = itemNode;
                        }
                    }
            }
        }

        public void DoDragDrop(ToolboxItem toolboxItem, DragDropEffects dragDropEffects)
        {
            base.DoDragDrop(toolboxItem, dragDropEffects);
        }

        public void SelectPointerTool()
        {
            if (treeView1.SelectedNode == null || treeView1.SelectedNode.Parent == null)
                return;
            treeView1.SelectedNode = treeView1.SelectedNode.Parent.Nodes.Cast<TreeNode>()
                .FirstOrDefault(n => (n.Tag as ToolboxItem).DisplayName == "Pointer");
        }

        #endregion IMyToolbox interface members
    }

    public class DoubleBufferedTreeView : TreeView
    {
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            Win32.SendMessage(Handle, Win32.TVM_SETEXTENDEDSTYLE, (IntPtr)Win32.TVS_EX_DOUBLEBUFFER, (IntPtr)Win32.TVS_EX_DOUBLEBUFFER);
        }
    }
}
