using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using GitUI.Properties;
using JetBrains.Annotations;
using ResourceManager;

namespace GitUI.UserControls.RevisionGrid
{
    public sealed class CopyContextMenuItem : ToolStripMenuItem
    {
        [CanBeNull] private Func<IReadOnlyList<GitRevision>> _revisionFunc;

        public CopyContextMenuItem()
        {
            Image = Images.CopyToClipboard;
            Text = "Copy to clipboard";

            // Action template
            Action<Func<GitRevision, string>> extractText = (Func<GitRevision, string> extractRevisionText) =>
            {
                var gitRevisions = _revisionFunc?.Invoke();
                var sb = new StringBuilder();
                foreach (var revision in gitRevisions)
                {
                    sb.AppendLine(extractRevisionText(revision));
                }

                Clipboard.SetText(sb.ToString());
            };

            // Add a dummy copy item, so that the shortcut key works.
            // This item will never be seen by the user, as the submenu is rebuilt on opening.
            AddItem("Dummy item", () => extractText(r => r.Guid), image: null, Keys.Control | Keys.C, showShortcutKeys: false);

            DropDownOpening += OnDropDownOpening;

            void OnDropDownOpening(object sender, EventArgs e)
            {
                var revisions = _revisionFunc?.Invoke();
                if (revisions == null || revisions.Count == 0)
                {
                    HideDropDown();
                    return;
                }

                DropDownItems.Clear();

                List<string> branchNames = new List<string>();
                List<string> tagNames = new List<string>();
                foreach (var revision in revisions)
                {
                    var refLists = new GitRefListsForRevision(revision);
                    branchNames.AddRange(refLists.GetAllBranchNames());
                    tagNames.AddRange(refLists.GetAllTagNames());
                }

                // Add items for branches
                if (branchNames.Any())
                {
                    var caption = new ToolStripMenuItem { Text = Strings.Branches };
                    MenuUtil.SetAsCaptionMenuItem(caption, Owner);
                    DropDownItems.Add(caption);

                    foreach (var name in branchNames)
                    {
                        AddItem(name, () => Clipboard.SetText(name), Images.Branch);
                    }

                    DropDownItems.Add(new ToolStripSeparator());
                }

                // Add items for tags
                if (tagNames.Any())
                {
                    var caption = new ToolStripMenuItem { Text = Strings.Tags };
                    MenuUtil.SetAsCaptionMenuItem(caption, Owner);
                    DropDownItems.Add(caption);

                    foreach (var name in tagNames)
                    {
                        AddItem(name, () => Clipboard.SetText(name), Images.Tag);
                    }

                    DropDownItems.Add(new ToolStripSeparator());
                }

                var count = revisions.Count();
                AddItem(Strings.GetCommitHash(count), () => extractText(r => r.Guid), Images.CommitId, Keys.Control | Keys.C);
                AddItem(Strings.GetMessage(count), () => extractText(r => r.Body ?? r.Subject), Images.Message);
                AddItem(Strings.GetAuthor(count), () => extractText(r => r.Author), Images.Author);
                AddItem(Strings.GetAuthorDate(count), () => extractText(r => r.AuthorDate.ToString()), Images.Date);
                AddItem(Strings.GetCommitDate(count), () => extractText(r => r.CommitDate.ToString()), Images.Date);
            }

            void AddItem(string displayText, Action clickAction, Image image, Keys shortcutKeys = Keys.None, bool showShortcutKeys = true)
            {
                var item = new ToolStripMenuItem
                {
                    Text = displayText,
                    ShortcutKeys = shortcutKeys,
                    ShowShortcutKeys = showShortcutKeys,
                    Image = image
                };
                item.Click += delegate
                {
                    clickAction();
                };

                DropDownItems.Add(item);
            }
        }

        public void SetRevisionFunc(Func<IReadOnlyList<GitRevision>> revisionFunc)
        {
            _revisionFunc = revisionFunc;
        }
    }
}
