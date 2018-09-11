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

            // Add a dummy copy item, so that the shortcut key works.
            // This item will never be seen by the user, as the submenu is rebuilt on opening.
            AddItem("Dummy item", r => r.Guid, image: null, Keys.Control | Keys.C, showShortcutKeys: false);

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
                        AddImmutableItem(name, Images.Branch);
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
                        AddImmutableItem(name, Images.Tag);
                    }

                    DropDownItems.Add(new ToolStripSeparator());
                }

                // Add other items
                AddItem(Strings.CommitHash,  (GitRevision r) => r.Guid, Images.CommitId, Keys.Control | Keys.C);
                AddItem(Strings.Message,     r => r.Body ?? r.Subject, Images.Message);
                AddItem(Strings.Author,      r => r.Author, Images.Author);
                AddItem(Strings.AuthorDate,  r => r.AuthorDate.ToString(), Images.Date);
                AddItem(Strings.CommitDate,  r => r.CommitDate.ToString(), Images.Date);
            }

            void AddImmutableItem(string displayText, Image image, Keys shortcutKeys = Keys.None, bool showShortcutKeys = true)
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
                    Clipboard.SetText(displayText);
                };

                DropDownItems.Add(item);
            }

            void AddItem(string displayText, Func<GitRevision, string> extractTextFunc, Image image, Keys shortcutKeys = Keys.None, bool showShortcutKeys = true)
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
                    var revisions = _revisionFunc?.Invoke();
                    var sb = new StringBuilder();
                    foreach (var revision in revisions)
                    {
                        sb.AppendLine(extractTextFunc(revision));
                    }

                    Clipboard.SetText(sb.ToString());
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
