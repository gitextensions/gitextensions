using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GitUI.UserControls.RevisionGridClasses
{
    class CopyToClipboardMenuHelper
    {
        /// <summary>
        /// ...
        /// sets also the visibility of captionItem
        /// ...
        /// </summary>
        /// <param name="targetMenu"></param>
        /// <param name="captionItem"></param>
        /// <param name="gitNameList"></param>
        /// <param name="itemFlag"></param>
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
        /// <param name="target"></param>
        /// <param name="postfix"></param>
        public static void AddOrUpdateTextPostfix(ToolStripItem target, string postfix)
        {
            if (target.Text.EndsWith(")"))
            {
                target.Text = target.Text.Substring(0, target.Text.IndexOf("     ("));
            }

            target.Text += string.Format("     ({0})", postfix);
        }

        /// <summary>
        /// Substring with elipses but OK if shorter, will take 3 characters off character count if necessary
        /// from http://blog.abodit.com/2010/02/string-extension-methods-for-truncating-and-adding-ellipsis/
        /// </summary>
        public static string StrLimitWithElipses(string str, int characterCount)
        {
            if (characterCount < 5)
                return StrLimit(str, characterCount); // Can’t do much with such a short limit
            if (str.Length <= characterCount - 3)
                return str;
            else
                return str.Substring(0, characterCount - 3) + "...";
        }

        /// <summary>
        /// Substring but OK if shorter
        /// </summary>
        /// <param name="str"></param>
        /// <param name="characterCount"></param>
        /// <returns></returns>
        public static string StrLimit(string str, int characterCount)
        {
            if (str.Length <= characterCount)
                return str;
            else
                return str.Substring(0, characterCount).TrimEnd(' ');
        }
    }
}
