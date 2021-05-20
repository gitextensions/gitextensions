using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitExtUtils;
using GitExtUtils.GitUI.Theming;
using GitUI.Properties;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.UserControls.RevisionGrid
{
    public sealed class CopyContextMenuItem : ToolStripMenuItem
    {
        private readonly TranslationString _copyToClipboardText = new("&Copy to clipboard");
        private Func<IReadOnlyList<GitRevision>>? _revisionFunc;
        private uint _itemNumber;

        public CopyContextMenuItem()
        {
            Image = Images.CopyToClipboard;
            Text = _copyToClipboardText.Text;

            DropDownOpening += OnDropDownOpening;
        }

        public void SetRevisionFunc(Func<IReadOnlyList<GitRevision>> revisionFunc)
        {
            _revisionFunc = revisionFunc;

            // Add dummy item for the menu entry to appear expandable (triangle on the right)
            DropDownItems.Add(new ToolStripMenuItem());
        }

        private void AddItem(string displayText, Func<GitRevision, string> extractRevisionText, Image image, char? hotkey)
        {
            var textToCopy = ExtractRevisionTexts(extractRevisionText);
            if (textToCopy is null)
            {
                return;
            }

            displayText += ":   " + textToCopy.Select(t => t.SubstringUntil('\n')).Join(", ").ShortenTo(40);
            AddItem(displayText, textToCopy.Join("\n"), image, hotkey);
        }

        private void AddItem(string displayText, string textToCopy, Image image, char? hotkey)
        {
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
                displayText = PrependItemNumber(displayText);
            }

            ToolStripMenuItem item = new()
            {
                Text = displayText.TrimEnd('\r', '\n'),
                ShowShortcutKeys = true,
                Image = image
            };

            item.Click += delegate
            {
                ClipboardUtil.TrySetText(textToCopy);
            };

            DropDownItems.Add(item);
        }

        private string[]? ExtractRevisionTexts(Func<GitRevision, string>? extractRevisionText)
        {
            if (extractRevisionText is null)
            {
                return null;
            }

            var gitRevisions = _revisionFunc?.Invoke();
            if (gitRevisions is null || gitRevisions.Count == 0)
            {
                return null;
            }

            return gitRevisions.Select(extractRevisionText).Distinct().ToArray();
        }

        private void OnDropDownOpening(object sender, EventArgs e)
        {
            var revisions = _revisionFunc?.Invoke();
            if (revisions is null || revisions.Count == 0)
            {
                HideDropDown();
                return;
            }

            DropDownItems.Clear();

            List<string> branchNames = new();
            List<string> tagNames = new();
            foreach (var revision in revisions)
            {
                GitRefListsForRevision refLists = new(revision);
                branchNames.AddRange(refLists.GetAllBranchNames());
                tagNames.AddRange(refLists.GetAllTagNames());
            }

            _itemNumber = 0;

            // Add items for branches
            if (branchNames.Any())
            {
                ToolStripMenuItem caption = new() { Text = TranslatedStrings.Branches };
                MenuUtil.SetAsCaptionMenuItem(caption, Owner);
                DropDownItems.Add(caption);

                foreach (var name in branchNames)
                {
                    AddItem(name, textToCopy: name, Images.Branch.AdaptLightness(), hotkey: null);
                }

                DropDownItems.Add(new ToolStripSeparator());
            }

            // Add items for tags
            if (tagNames.Any())
            {
                ToolStripMenuItem caption = new() { Text = TranslatedStrings.Tags };
                MenuUtil.SetAsCaptionMenuItem(caption, Owner);
                DropDownItems.Add(caption);

                foreach (var name in tagNames)
                {
                    AddItem(name, textToCopy: name, Images.Tag, hotkey: null);
                }

                DropDownItems.Add(new ToolStripSeparator());
            }

            // Add other items
            int count = revisions.Count();
            AddItem(ResourceManager.TranslatedStrings.GetCommitHash(count), r => r.Guid, Images.CommitId, 'C');
            AddItem(ResourceManager.TranslatedStrings.GetMessage(count), r => r.Body ?? r.Subject, Images.Message, 'M');
            AddItem(ResourceManager.TranslatedStrings.GetAuthor(count), r => $"{r.Author} <{r.AuthorEmail}>", Images.Author, 'A');

            if (count == 1 && revisions.First().AuthorDate == revisions.First().CommitDate)
            {
                AddItem(ResourceManager.TranslatedStrings.Date, r => r.AuthorDate.ToString(), Images.Date, 'D');
            }
            else
            {
                AddItem(ResourceManager.TranslatedStrings.GetAuthorDate(count), r => r.AuthorDate.ToString(), Images.Date, 'T');
                AddItem(ResourceManager.TranslatedStrings.GetCommitDate(count), r => r.CommitDate.ToString(), Images.Date, 'D');
            }
        }

        private string PrependItemNumber(string name)
        {
            return ++_itemNumber > 10 ? name : "&" + (_itemNumber % 10) + ":   " + name;
        }
    }
}
