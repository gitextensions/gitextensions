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

        private readonly TranslationString _copyToClipboardText = new TranslationString("&Copy to clipboard");
        private readonly Func<int> _getShortenedHashLength;

        public CopyContextMenuItem(Func<int> getShortenedHashLength)
        {
            _getShortenedHashLength = getShortenedHashLength;

            Image = Images.CopyToClipboard;
            Text = _copyToClipboardText.Text;

            // Create a dummy menu, so that the shortcut keys work.
            OnDropDownOpening(null, null);

            DropDownOpening += OnDropDownOpening;

            void OnDropDownOpening(object sender, EventArgs e)
            {
                var revisions = _revisionFunc?.Invoke();
                if (revisions == null || revisions.Count == 0)
                {
                    if (sender == null)
                    {
                        // create the initial dummy menu on a dummy revision
                        revisions = new List<GitRevision> { new GitRevision(GitUIPluginInterfaces.ObjectId.WorkTreeId) };
                    }
                    else
                    {
                        HideDropDown();
                        return;
                    }
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

                uint refItemNumber = 0;
                string prependItemNumber(string name)
                {
                    return ++refItemNumber > 10 ? name : "&" + (refItemNumber % 10) + ":   " + name;
                }

                // Add items for branches
                if (branchNames.Any())
                {
                    var caption = new ToolStripMenuItem { Text = Strings.Branches };
                    MenuUtil.SetAsCaptionMenuItem(caption, Owner);
                    DropDownItems.Add(caption);

                    foreach (var name in branchNames)
                    {
                        AddItem(name, extractRevisionText: null, Images.Branch, hotkey: null);
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
                        AddItem(name, extractRevisionText: null, Images.Tag, hotkey: null);
                    }

                    DropDownItems.Add(new ToolStripSeparator());
                }

                // Add other items
                int count = revisions.Count();
                AddItem(Strings.GetCommitHash(count), r => r.Guid, Images.CommitId, 'C', Keys.Control | Keys.C);
                AddItem(Strings.GetCommitHashShortened(count), r => r.ObjectId.ToShortString(_getShortenedHashLength()), Images.CommitId, 'H', Keys.Control | Keys.H);
                AddItem(Strings.GetMessage(count), r => r.Body ?? r.Subject, Images.Message, 'M');
                AddItem(Strings.GetAuthor(count), r => r.Author, Images.Author, 'A');

                if (count == 1 && revisions.First().AuthorDate == revisions.First().CommitDate)
                {
                    AddItem(Strings.Date, r => r.AuthorDate.ToString(), Images.Date, 'D');
                }
                else
                {
                    AddItem(Strings.GetAuthorDate(count), r => r.AuthorDate.ToString(), Images.Date, 'T');
                    AddItem(Strings.GetCommitDate(count), r => r.CommitDate.ToString(), Images.Date, 'D');
                }

                void AddItem(string displayText, Func<GitRevision, string> extractRevisionText, Image image, char? hotkey, Keys shortcutKeys = Keys.None)
                {
                    string name = displayText; // keep an undecorated copy for extractRevisionTexts()

                    IEnumerable<string> extractRevisionTexts()
                    {
                        if (extractRevisionText == null)
                        {
                            return new List<string> { name };
                        }

                        var gitRevisions = _revisionFunc?.Invoke();
                        if (gitRevisions == null || gitRevisions.Count == 0)
                        {
                            return null;
                        }

                        return gitRevisions.Select(r => extractRevisionText(r)).Distinct();
                    }

                    if (hotkey.HasValue)
                    {
                        int position = displayText.IndexOf(hotkey.Value.ToString(), StringComparison.InvariantCultureIgnoreCase);
                        if (position >= 0)
                        {
                            displayText = displayText.Insert(position, "&");
                        }
                    }
                    else
                    {
                        displayText = prependItemNumber(displayText);
                    }

                    if (extractRevisionText != null)
                    {
                        var texts = extractRevisionTexts();
                        if (texts != null)
                        {
                            displayText += ":   " + texts.Select(t => t.SubstringUntil('\n')).Join(", ").ShortenTo(40);
                        }
                    }

                    var item = new ToolStripMenuItem
                    {
                        Text = displayText,
                        ShortcutKeys = shortcutKeys,
                        ShowShortcutKeys = true,
                        Image = image
                    };
                    item.Click += delegate
                    {
                        var texts = extractRevisionTexts();
                        if (texts != null)
                        {
                            Clipboard.SetText(texts.Join("\n"));
                        }
                    };

                    DropDownItems.Add(item);
                }
            }
        }

        public void SetRevisionFunc(Func<IReadOnlyList<GitRevision>> revisionFunc)
        {
            _revisionFunc = revisionFunc;
        }
    }
}
