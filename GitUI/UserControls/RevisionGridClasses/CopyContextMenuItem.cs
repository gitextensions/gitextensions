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
                CopyToClipboardMenuHelper.AddOrUpdateTextPostfix(hashCopyToolStripMenuItem, r.Guid.ShortenTo(15));
                CopyToClipboardMenuHelper.AddOrUpdateTextPostfix(messageCopyToolStripMenuItem, r.Subject.ShortenTo(30));
                CopyToClipboardMenuHelper.AddOrUpdateTextPostfix(authorCopyToolStripMenuItem, r.Author);
                CopyToClipboardMenuHelper.AddOrUpdateTextPostfix(dateCopyToolStripMenuItem, r.CommitDate.ToString());
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
            CopyToClipboardMenuHelper.SetCopyToClipboardMenuItems(this, branchNameCopyToolStripMenuItem, branchNames, "branchNameItem");

            var tagNames = gitRefListsForRevision.GetAllTagNames();
            CopyToClipboardMenuHelper.SetCopyToClipboardMenuItems(this, tagNameCopyToolStripMenuItem, tagNames, "tagNameItem");

            toolStripSeparator6.Visible = branchNames.Any() || tagNames.Any();
        }
    }
}
