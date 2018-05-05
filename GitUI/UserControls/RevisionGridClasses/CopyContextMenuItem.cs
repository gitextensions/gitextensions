using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using GitCommands;

namespace GitUI.UserControls.RevisionGridClasses
{
    public partial class CopyContextMenuItem : ToolStripMenuItem
    {
        public CopyContextMenuItem()
        {
            InitializeComponent();
        }

        [Browsable(false)]
        public Func<GitRevision> GetLatestSelectedRevision { get; set; }

        [Browsable(false)]
        private GitRevision LatestSelectedRevision => GetLatestSelectedRevision?.Invoke();

        private void copyToClipboardToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            var r = LatestSelectedRevision;
            if (r != null)
            {
                AddOrUpdateTextPostfix(hashCopyToolStripMenuItem, r.Guid.ShortenTo(15));
                AddOrUpdateTextPostfix(messageCopyToolStripMenuItem, r.Subject.ShortenTo(30));
                AddOrUpdateTextPostfix(authorCopyToolStripMenuItem, r.Author);
                AddOrUpdateTextPostfix(dateCopyToolStripMenuItem, r.CommitDate.ToString());
            }
        }

        private void MessageToolStripMenuItemClick(object sender, EventArgs e)
        {
            Clipboard.SetText(LatestSelectedRevision.Subject);
        }

        private void AuthorToolStripMenuItemClick(object sender, EventArgs e)
        {
            Clipboard.SetText(LatestSelectedRevision.Author);
        }

        private void DateToolStripMenuItemClick(object sender, EventArgs e)
        {
            Clipboard.SetText(LatestSelectedRevision.CommitDate.ToString());
        }

        private void HashToolStripMenuItemClick(object sender, EventArgs e)
        {
            Clipboard.SetText(LatestSelectedRevision.Guid);
        }

        internal void UpdateItems(GitRefListsForRevision gitRefListsForRevision, ContextMenuStrip mainContextMenu)
        {
            branchNameCopyToolStripMenuItem.Tag = "caption";
            tagNameCopyToolStripMenuItem.Tag = "caption";
            MenuUtil.SetAsCaptionMenuItem(branchNameCopyToolStripMenuItem, mainContextMenu);
            MenuUtil.SetAsCaptionMenuItem(tagNameCopyToolStripMenuItem, mainContextMenu);

            var branchNames = gitRefListsForRevision.GetAllBranchNames();
            SetCopyToClipboardMenuItems(this, branchNameCopyToolStripMenuItem, branchNames, "branchNameItem");

            var tagNames = gitRefListsForRevision.GetAllTagNames();
            SetCopyToClipboardMenuItems(this, tagNameCopyToolStripMenuItem, tagNames, "tagNameItem");

            separatorAfterRefNames.Visible = branchNames.Any() || tagNames.Any();
        }

        /// <summary>
        /// ...
        /// sets also the visibility of captionItem
        /// ...
        /// </summary>
        private static void SetCopyToClipboardMenuItems(
            ToolStripMenuItem targetMenu,
            ToolStripMenuItem captionItem,
            string[] gitNameList,
            string itemFlag)
        {
            // remove previous items
            targetMenu.DropDownItems.OfType<ToolStripMenuItem>()
                .Where(item => (item.Tag as string) == itemFlag)
                .ToArray()
                .ForEach(item => targetMenu.DropDownItems.Remove(item));

            // insert items
            var branchNameItemInsertAfter = captionItem;
            gitNameList.ForEach(branchName =>
            {
                var branchNameItem = new ToolStripMenuItem(branchName);
                branchNameItem.Tag = itemFlag; // to delete items from previous opening
                branchNameItem.Click += CopyToClipBoard;
                int insertAfterIndex = targetMenu.DropDownItems.IndexOf(branchNameItemInsertAfter);
                targetMenu.DropDownItems.Insert(insertAfterIndex + 1, branchNameItem);
                branchNameItemInsertAfter = branchNameItem;
            });

            // visibility of caption
            // TODO: why is the Visible property always false when it is read from?
            captionItem.Visible = gitNameList.Any();
        }

        private static void CopyToClipBoard(object sender, EventArgs e)
        {
            Clipboard.SetText(sender.ToString());
        }

        /// <summary>
        /// adds or updates text in parentheses (...)
        /// </summary>
        private static void AddOrUpdateTextPostfix(ToolStripItem target, string postfix)
        {
            if (target.Text.EndsWith(")"))
            {
                target.Text = target.Text.Substring(0, target.Text.IndexOf("     ("));
            }

            target.Text += string.Format("     ({0})", postfix);
        }
    }
}
