using System;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using JetBrains.Annotations;
using ResourceManager;

namespace GitUI.UserControls.RevisionGrid
{
    public sealed class CopyContextMenuItem : ToolStripMenuItem
    {
        [CanBeNull] private Func<GitRevision> _revisionFunc;

        public CopyContextMenuItem()
        {
            Image = Properties.Resources.IconCopyToClipboard;
            Text = "Copy to clipboard";

            // Add a dummy copy item, so that the shortcut key works.
            // This item will never be seen by the user, as the submenu is rebuilt on opening.
            AddItem("Dummy item", r => r.Guid, Keys.Control | Keys.C, showShortcutKeys: false);

            DropDownOpening += OnDropDownOpening;

            void OnDropDownOpening(object sender, EventArgs e)
            {
                var revision = _revisionFunc?.Invoke();

                if (revision == null)
                {
                    HideDropDown();
                    return;
                }

                DropDownItems.Clear();

                var refLists = new GitRefListsForRevision(revision);
                var branchNames = refLists.GetAllBranchNames();
                var tagNames = refLists.GetAllTagNames();

                // Add items for branches
                if (branchNames.Any())
                {
                    // TODO translate text
                    var caption = new ToolStripMenuItem { Text = "Branch name:" };
                    MenuUtil.SetAsCaptionMenuItem(caption, Owner);
                    DropDownItems.Add(caption);

                    foreach (var name in branchNames)
                    {
                        AddItem(name, _ => name);
                    }

                    DropDownItems.Add(new ToolStripSeparator());
                }

                // Add items for tags
                if (tagNames.Any())
                {
                    // TODO translate text
                    var caption = new ToolStripMenuItem { Text = "Tag name:" };
                    MenuUtil.SetAsCaptionMenuItem(caption, Owner);
                    DropDownItems.Add(caption);

                    foreach (var name in tagNames)
                    {
                        AddItem(name, _ => name);
                    }

                    DropDownItems.Add(new ToolStripSeparator());
                }

                // Add other items
                AddItem($"{Strings.CommitHash}     ({revision.Guid.ShortenTo(15)})", r => r.Guid, Keys.Control | Keys.C);
                AddItem($"{Strings.Message}     ({revision.Subject.ShortenTo(30)})", r => r.Body ?? r.Subject);
                AddItem($"{Strings.Author}     ({revision.Author})", r => r.Author);

                if (revision.AuthorDate == revision.CommitDate)
                {
                    AddItem($"{Strings.Date}     ({revision.CommitDate})", r => r.CommitDate.ToString());
                }
                else
                {
                    AddItem($"{Strings.AuthorDate}     ({revision.AuthorDate})", r => r.AuthorDate.ToString());
                    AddItem($"{Strings.CommitDate}     ({revision.CommitDate})", r => r.CommitDate.ToString());
                }
            }

            void AddItem(string displayText, Func<GitRevision, string> clipboardText, Keys shortcutKeys = Keys.None, bool showShortcutKeys = true)
            {
                var item = new ToolStripMenuItem
                {
                    Text = displayText,
                    ShortcutKeys = shortcutKeys,
                    ShowShortcutKeys = showShortcutKeys
                };

                item.Click += delegate
                {
                    var revision = _revisionFunc?.Invoke();
                    if (revision != null)
                    {
                        Clipboard.SetText(clipboardText(revision));
                    }
                };

                DropDownItems.Add(item);
            }
        }

        public void SetRevisionFunc(Func<GitRevision> revisionFunc)
        {
            _revisionFunc = revisionFunc;
        }
    }
}
