using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace UiTools.WinForms.Designer.Core
{
    public interface IRecentFilesManager
    {
        event EventHandler<string> RecentFileClicked;
        void AddOrMoveFileToTheTop(string filePath);
        void HandleNonExistingFileInList(string filePath);
    }

    public class RecentFilesManager : IRecentFilesManager
    {
        private readonly ToolStripDropDownItem recentFilesRootMenu;
        private readonly List<string> mruList;
        private readonly int mruListMaxSize;
        private readonly Action saveAction;
        private readonly Image deleteIcon;
        private readonly Image openFolderIcon;

        private readonly ContextMenuStrip itemContextMenu;
        private ToolStripMenuItem lastRightClickedItem;
        private bool isContextMenuVisible;

        public event EventHandler<string> RecentFileClicked;

        public RecentFilesManager(ToolStripDropDownItem recentFilesRootMenu, List<string> mruList, int mruListMaxSize, Action saveAction,
            Image deleteIcon = null, Image openFolderIcon = null)
        {
            this.recentFilesRootMenu = recentFilesRootMenu ?? throw new ArgumentNullException(nameof(recentFilesRootMenu));
            this.mruList = mruList ?? throw new ArgumentNullException(nameof(mruList));
            this.mruListMaxSize = mruListMaxSize;
            this.saveAction = saveAction ?? throw new ArgumentNullException(nameof(saveAction));
            this.deleteIcon = deleteIcon;
            this.openFolderIcon = openFolderIcon;

            itemContextMenu = PrepareContextMenu();

            // Initial menu build from the configuration:
            RebuildMenu();
        }

        private ContextMenuStrip PrepareContextMenu()
        {
            var itemContextMenu = new ContextMenuStrip();

            var miOpenContainingFolder = new ToolStripMenuItem("Open containing folder", openFolderIcon);
            miOpenContainingFolder.Click += OnOpenContainingFolder; ;
            itemContextMenu.Items.Add(miOpenContainingFolder);

            itemContextMenu.Items.Add(new ToolStripSeparator());

            var miRemoveFromList = new ToolStripMenuItem("Remove from this list", deleteIcon);
            miRemoveFromList.Click += (s, e) => RemoveItemWithConfirmation(lastRightClickedItem);
            itemContextMenu.Items.Add(miRemoveFromList);

            // To keep the menu open when removing items via the right-click context menu:
            itemContextMenu.Closed += (s, e) => isContextMenuVisible = false;
            this.recentFilesRootMenu.DropDown.Closing += DropDownChain_Closing;
            if (this.recentFilesRootMenu.Owner is ToolStripDropDown parentDropDown)
                parentDropDown.Closing += DropDownChain_Closing;

            return itemContextMenu;
        }

        private void OnOpenContainingFolder(object sender, EventArgs e)
        {
            OpenContainingFolder(lastRightClickedItem.Text);
            //isContextMenuVisible = false; // seems it's not needed
            recentFilesRootMenu.DropDown.Close();
            if (recentFilesRootMenu.Owner is ToolStripDropDown parentDropDown)
                parentDropDown.Close();
        }

        private void DropDownChain_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            // If the menu tries to close due to focus loss (context menu opening) - cancel the closure:
            if (isContextMenuVisible && e.CloseReason == ToolStripDropDownCloseReason.AppFocusChange)
                e.Cancel = true;
        }

        /// <summary>
        /// Adds the file to the top of the list or moves the existing entry up.
        /// Called by external code only upon *successful* open or save operations.
        /// </summary>
        public void AddOrMoveFileToTheTop(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            int existingIndex = mruList.FindIndex(p => p.Equals(filePath, StringComparison.OrdinalIgnoreCase)); // ignoring case
            if (existingIndex != -1)
                mruList.RemoveAt(existingIndex); // if the file is already in the list, remove it first to move it to the top later

            // Insert the actual path (preserving original casing) at the very beginning of the list:
            mruList.Insert(0, filePath);
            // Enforce the maximum list size limit:
            if (mruList.Count > mruListMaxSize)
                mruList.RemoveAt(mruList.Count - 1);

            SaveAndRebuild();
        }

        /// <summary>
        /// Handles cases where a file from the list is not found on disk.
        /// </summary>
        public void HandleNonExistingFileInList(string filePath)
        {
            if (MessageBox.Show($"A recently open file \"{filePath}\" does not exist.\r\nDo you want to remove it from the \"Open recent\" list?",
                                "File Not Found", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                RemoveFromFileList(filePath);
        }

        private void RebuildMenu()
        {
            // Unsubscribe from old item events before clearing:
            foreach (ToolStripItem oldItem in recentFilesRootMenu.DropDownItems)
            {
                if (oldItem is ToolStripMenuItem mi)
                {
                    mi.Click -= OnItemClick;
                    mi.MouseDown -= OnItemMouseDown;
                    mi.Click -= ClearRecentFiles_Click;
                }
            }
            recentFilesRootMenu.DropDownItems.Clear();

            if (mruList != null && mruList.Count > 0)
            {
                var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase); // would work with List<string> as well - but a bit slower: as O(n) vs O(1)
                foreach (var path in mruList)
                {
                    if (!seenPaths.Add(path))
                        continue; // duplicate protection

                    var item = new ToolStripMenuItem(path);
                    item.Click += OnItemClick;
                    item.MouseDown += OnItemMouseDown;

                    recentFilesRootMenu.DropDownItems.Add(item);
                }

                // Add a separator and the "Clear List" item:
                recentFilesRootMenu.DropDownItems.Add(new ToolStripSeparator());
                var clearItem = new ToolStripMenuItem("Clear recent files list");
                clearItem.Click += ClearRecentFiles_Click;
                recentFilesRootMenu.DropDownItems.Add(clearItem);

                recentFilesRootMenu.Enabled = true;
            }
            else
            {
                recentFilesRootMenu.Enabled = false;
            }
        }

        private void OnItemClick(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem mi)
                RecentFileClicked?.Invoke(this, mi.Text);
        }

        private void OnItemMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && sender is ToolStripMenuItem item)
            {
                lastRightClickedItem = item;
                isContextMenuVisible = true;
                itemContextMenu.Show(Cursor.Position);
            }
        }

        private void RemoveItemWithConfirmation(ToolStripMenuItem item)
        {
            if (item == null)
                return;
            string path = item.Text;
            if (MessageBox.Show($"Remove \"{path}\" from the recent files list?",
                                "Confirm Removal", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                RemoveFromFileList(path);
        }

        private void RemoveFromFileList(string filePath)
        {
            mruList.RemoveAll(p => p.Equals(filePath, StringComparison.OrdinalIgnoreCase));
            SaveAndRebuild();
        }

        private void ClearRecentFiles_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to clear the entire recent files list?",
                                "Clear List", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                mruList.Clear();
                SaveAndRebuild();
            }
        }

        private void SaveAndRebuild()
        {
            saveAction();
            //isContextMenuVisible = false; // seems it's not needed
            recentFilesRootMenu.DropDown.Close();
            if (recentFilesRootMenu.Owner is ToolStripDropDown parentDropDown)
                parentDropDown.Close();
            RebuildMenu();
        }

        private void OpenContainingFolder(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return;

            if (!File.Exists(filePath))
            {
                if (MessageBox.Show($"File \"{filePath}\" no longer exists.\r\nDo you want to remove it from the list?",
                                    "File Not Found", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    RemoveFromFileList(filePath);
                return;
            }

            try
            {
                System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{filePath}\"");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open folder: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
