using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Linq;

namespace UiTools.WinForms.Designer.Core
{
    public partial class ToolboxTreeView : UserControl, IMyToolbox
    {
        // NOTE: moving ToolboxTreeView control to "Workspace" project folder resulted in MissingManifestResourceException when assigning ImageList.ImageStream property.
        //       The solution was to introduce the "EmbeddedResourceUseDependentUponConvention" setting (set to true) in the .csproj file.
        public event EventHandler<ToolboxItem> ToolboxItemDoubleClick;

        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        public static extern int SendMessageStr(IntPtr hWnd, int msg, IntPtr wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        private const int CB_SETCUEBANNER = 0x1703;

        private bool isSearchButtonHovered = false;
        private TreeNode hoveredNode = null;
        private Dictionary<string, List<ToolboxItem>> itemsByCategories;

        public ToolboxTreeView()
        {
            InitializeComponent();

            treeView1.FullRowSelect = true;
            treeView1.HideSelection = false;
            treeView1.ShowLines = false;
            treeView1.ShowNodeToolTips = true;

            treeView1.ImageList = imageList16px;
            treeView1.Indent = 22;
            treeView1.ItemHeight = 26;

            treeView1.DrawMode = TreeViewDrawMode.OwnerDrawAll;
            treeView1.DrawNode += treeView1_DrawNode;
            treeView1.MouseMove += treeView1_MouseMove;
            treeView1.MouseLeave += treeView1_MouseLeave;
            treeView1.KeyDown += ProcessKeyDownEvent;
            treeView1.MouseDown += treeView1_MouseDown;
            treeView1.NodeMouseClick += treeView1_NodeMouseClick;
            treeView1.NodeMouseDoubleClick += treeView1_NodeMouseDoubleClick;

            cboSearch.TabStop = false;
            cboSearch.TextChanged += cboSearch_TextChanged;
            cboSearch.LostFocus += cboSearch_LostFocus;
            cboSearch.KeyDown += ProcessKeyDownEvent;
            SendMessageStr(cboSearch.Handle, CB_SETCUEBANNER, IntPtr.Zero, "Search Toolbox");

            picSearch.Height = cboSearch.Height - 2;
            picSearch.Width = 20;
            picSearch.SizeMode = PictureBoxSizeMode.CenterImage;
            picSearch.MouseEnter += picSearch_MouseEnter;
            picSearch.MouseLeave += picSearch_MouseLeave;
            picSearch.Click += picSearch_Click;
            UpdateSearchButton();
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
                        var toolType = item.GetType(DesignerHost);
                        itemNode.ToolTipText = toolType == null // will be null for "Pointer" items
                            ? item.DisplayName
                            : ComposeComponentToolTipText(toolType);
                    }
                }
                if (categoryNode.Nodes.Count == 0)
                    categoryNode.Remove();
                else if (filter != null)
                    categoryNode.Expand();
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

        private string ComposeComponentToolTipText(Type componentType)
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
                sb.AppendLine(desc);
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

            Color backColor = Color.White;
            Color textColor = Color.Black;
            Color hoverBackColor = Color.FromArgb(201, 222, 245);

            if (isSelected)
            {
                backColor = SystemColors.Highlight;
                textColor = SystemColors.HighlightText;
            }
            else if (isHovered)
            {
                backColor = hoverBackColor;
                textColor = Color.Black;
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
                    var img = isSelected
                        ? imageList16px.Images[isExpanded ? "SelectedAndExpandedTreeNode" : "SelectedAndCollapsedTreeNode"]
                        : imageList16px.Images[isExpanded ? "ExpandedTreeNode" : "CollapsedTreeNode"];
                    if (img == null)
                    {
                        // this happens during the closing of the parent form
                        return;
                    }
                    e.Graphics.DrawImage(img, e.Bounds.X + 3, e.Bounds.Y + (e.Bounds.Height - img.Height) / 2);
                    // Draw text:
                    var textBounds = e.Bounds;
                    textBounds.X += img.Width + 2; // indent text after the icon
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
                        e.Graphics.DrawImage(nodeImage, currentX, e.Bounds.Y + (e.Bounds.Height - nodeImage.Height) / 2);
                        currentX += nodeImage.Width + 2; // indent before the text
                    }
                    // Draw text:
                    Rectangle nodeTextBounds = new Rectangle(currentX, e.Bounds.Y, e.Bounds.Width - (currentX - e.Bounds.X), e.Bounds.Height);
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

        private void treeView1_MouseLeave(object sender, System.EventArgs e)
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

        private void picSearch_MouseEnter(object sender, EventArgs e)
        {
            picSearch.BackColor = Color.FromArgb(229, 241, 251);
            isSearchButtonHovered = true;
            UpdateSearchButton();
        }

        private void picSearch_MouseLeave(object sender, EventArgs e)
        {
            picSearch.BackColor = SystemColors.Window;
            isSearchButtonHovered = false;
            UpdateSearchButton();
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
            picSearch.Left = cboSearch.Right - picSearch.Width - 22;
            picSearch.Top = cboSearch.Top + (cboSearch.Height - picSearch.Height) / 2;
            labNoResults.Top = cboSearch.Bottom + 20;
            labNoResults.Left = (ClientSize.Width - labNoResults.Width) / 2;
        }

        private void cboSearch_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(cboSearch.Text))
                RemoveFilter();
            else
                SetFilter(cboSearch.Text);
            UpdateSearchButton();
        }

        private void UpdateSearchButton()
        {
            bool filterPresent = !string.IsNullOrEmpty(cboSearch.Text);
            toolTip1.SetToolTip(picSearch, filterPresent ? "Clear search" : "Search");
            picSearch.Image = isSearchButtonHovered
                ? imageList20px.Images[filterPresent ? "HoveredClearSearch" : "HoveredSearch"]
                : imageList20px.Images[filterPresent ? "ClearSearch" : "Search"];
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
            var bmp = new Bitmap(imageList16px.Images["Pointer"]);
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
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);

        private const int TVM_SETEXTENDEDSTYLE = 0x1100 + 44;
        private const int TVS_EX_DOUBLEBUFFER = 0x0004;

        protected override void OnHandleCreated(EventArgs e)
        {
            SendMessage(this.Handle, TVM_SETEXTENDEDSTYLE, (IntPtr)TVS_EX_DOUBLEBUFFER, (IntPtr)TVS_EX_DOUBLEBUFFER);
            base.OnHandleCreated(e);
        }
    }
}
