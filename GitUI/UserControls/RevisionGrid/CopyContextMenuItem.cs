using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace GitUI.UserControls.RevisionGrid
{
    public partial class CopyContextMenuItem : ToolStripMenuItem
    {
        public CopyContextMenuItem()
        {
            InitializeComponent();
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
            InsertItemsAfterItem(separatorAfterRefNames, ViewModel.DetailItems.Select(i => new CopyToClipboardToolStripMenuItem(i.Text, i.Value, i.ShortcutKeys)).ToArray());
            separatorAfterRefNames.Visible = ViewModel.SeparatorVisible;
        }

        private void AddRefNameItems(ToolStripItem captionItem, IReadOnlyList<string> gitNameList)
        {
            InsertItemsAfterItem(captionItem, gitNameList.Select(name => new CopyToClipboardToolStripMenuItem(name, name, Keys.None)).ToArray());
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
