using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using ResourceManager;

namespace GitUI.UserControls.RevisionGridClasses
{
    public partial class CopyContextMenuItem : ToolStripMenuItem
    {
        public CopyContextMenuItem()
        {
            InitializeComponent();
            AddDefaultCopyCommitHashMenuItemWithCtrlCShortcut();
        }

        /// <remarks>
        /// This menu item is needed to override the default Ctrl+C handler of the grid control.
        /// Because menu items' caption is a string value, this menu item must be updated
        /// or replaced when the menu is opened to display updated commit hash in its label.
        /// </remarks>
        private void AddDefaultCopyCommitHashMenuItemWithCtrlCShortcut()
        {
            DropDownItems.Add(CreateCopyCommitHashMenuItem("(tmp)"));
        }

        [Browsable(false)]
        public Func<CopyContextMenuViewModel> GetViewModel { get; set; }

        [Browsable(false)]
        private CopyContextMenuViewModel ViewModel => GetViewModel?.Invoke();

        private void copyToClipboardToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            var r = ViewModel;
            if (r == null)
            {
                return;
            }

            MenuUtil.SetAsCaptionMenuItem(branchNameCopyToolStripMenuItem, Owner);
            MenuUtil.SetAsCaptionMenuItem(tagNameCopyToolStripMenuItem, Owner);

            CleanupDynamicallyAddedItems();

            AddRefNameItems(branchNameCopyToolStripMenuItem, ViewModel.BranchNames);
            AddRefNameItems(tagNameCopyToolStripMenuItem, ViewModel.TagNames);
            AddDetailItems();
        }

        private void CleanupDynamicallyAddedItems()
        {
            DropDownItems
                .OfType<CopyToClipboardToolStripMenuItem>()
                .ToArray()
                .ForEach(i => DropDownItems.Remove(i));
        }

        private void AddDetailItems()
        {
            InsertItemsAfterItem(separatorAfterRefNames, CommitHashFollowedByAllOtherDetailItems());
            separatorAfterRefNames.Visible = ViewModel.SeparatorVisible;
        }

        private CopyToClipboardToolStripMenuItem[] CommitHashFollowedByAllOtherDetailItems()
        {
            var commitHashCaption = new CopyContextMenuViewModel.DetailItem(Strings.GetCommitHashText(), ViewModel?.CommitHash, 15).Text;
            var copyCommitHashItem = CreateCopyCommitHashMenuItem(commitHashCaption);

            var items =
                new[] { copyCommitHashItem }
                    .Concat(ViewModel.DetailItems.Select(i => new CopyToClipboardToolStripMenuItem(i.Text, () => i.Value)));

            return items.ToArray();
        }

        private CopyToClipboardToolStripMenuItem CreateCopyCommitHashMenuItem(string caption)
        {
            return new CopyToClipboardToolStripMenuItem(caption, () => ViewModel?.CommitHash, Keys.Control | Keys.C);
        }

        private void AddRefNameItems(ToolStripItem captionItem, IReadOnlyList<string> gitNameList)
        {
            InsertItemsAfterItem(captionItem, gitNameList.Select(name => new CopyToClipboardToolStripMenuItem(name, () => name)).ToArray());
            captionItem.Visible = gitNameList.Any();
        }

        private void InsertItemsAfterItem(ToolStripItem anchorItem, CopyToClipboardToolStripMenuItem[] items)
        {
            var startIndex = DropDownItems.IndexOf(anchorItem) + 1;
            for (var i = 0; i < items.Length; ++i)
            {
                DropDownItems.Insert(startIndex + i, items[i]);
            }
        }
    }
}
