using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GitUI.UserControls.RevisionGridClasses
{
    class CopyToClipboardMenuHelper
    {
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
            captionItem.Visible = gitNameList.Any();
        }

        private static void CopyToClipBoard(object sender, EventArgs e)
        {
            Clipboard.SetText(sender.ToString());
        }
    }
}
