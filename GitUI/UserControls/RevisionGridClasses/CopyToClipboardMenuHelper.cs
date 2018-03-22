using System;
using System.Linq;
using System.Windows.Forms;

namespace GitUI.UserControls.RevisionGridClasses
{
    internal static class CopyToClipboardMenuHelper
    {
        /// <summary>
        /// ...
        /// sets also the visibility of captionItem
        /// ...
        /// </summary>
        public static void SetCopyToClipboardMenuItems(
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
        public static void AddOrUpdateTextPostfix(ToolStripItem target, string postfix)
        {
            if (target.Text.EndsWith(")"))
            {
                target.Text = target.Text.Substring(0, target.Text.IndexOf("     ("));
            }

            target.Text += string.Format("     ({0})", postfix);
        }
    }
}
