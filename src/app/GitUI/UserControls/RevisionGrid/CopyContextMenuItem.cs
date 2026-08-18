using GitExtensions.Extensibility;
using GitExtUtils;
using GitExtUtils.GitUI.Theming;
using GitUI.Properties;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.UserControls.RevisionGrid;

public sealed class CopyContextMenuItem : ToolStripMenuItem
{
    private readonly TranslationString _copyToClipboardText = new("&Copy to clipboard");
    private Func<IEnumerable<string>, IEnumerable<string>> _filterRefsFunc = refs => refs;
    private Func<IReadOnlyList<GitRevision>>? _revisionFunc;
    private uint _itemNumber;

    // Persistent named items for the fixed sub-actions, exposed for toolbar introspection.
    internal readonly ToolStripMenuItem CommitHashMenuItem = new() { Name = "copyCommitHashToolStripMenuItem", Image = Images.CommitId, Text = ResourceManager.TranslatedStrings.CommitHash };
    internal readonly ToolStripMenuItem MessageMenuItem = new() { Name = "copyMessageToolStripMenuItem", Image = Images.Message, Text = ResourceManager.TranslatedStrings.GetMessage(1) };
    internal readonly ToolStripMenuItem AuthorMenuItem = new() { Name = "copyAuthorToolStripMenuItem", Image = Images.Author.AdaptLightness(), Text = ResourceManager.TranslatedStrings.Author };
    internal readonly ToolStripMenuItem DateMenuItem = new() { Name = "copyDateToolStripMenuItem", Image = Images.Date, Text = ResourceManager.TranslatedStrings.Date };

    public CopyContextMenuItem()
    {
        Name = "copyToClipboardToolStripMenuItem";
        Image = Images.CopyToClipboard;
        Text = _copyToClipboardText.Text;

        // Seed DropDownItems with named stubs so the menu appears expandable and sub-actions
        // are visible to toolbar introspection before any revision is selected.
        DropDownItems.AddRange(new ToolStripItem[]
        {
            CommitHashMenuItem,
            MessageMenuItem,
            AuthorMenuItem,
            DateMenuItem,
        });

        DropDownOpening += OnDropDownOpening;
    }

    public void SetFilterRefsFunc(Func<IEnumerable<string>, IEnumerable<string>> filterRefsFunc)
    {
        _filterRefsFunc = filterRefsFunc;
    }

    public void SetRevisionFunc(Func<IReadOnlyList<GitRevision>> revisionFunc)
    {
        _revisionFunc = revisionFunc;
    }

    private void InsertItem(int index, string displayText, string textToCopy, Image image, char? hotkey)
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
            Text = displayText.TrimEnd(Delimiters.LineFeedAndCarriageReturn),
            ShowShortcutKeys = true,
            Image = image
        };

        item.Click += delegate
        {
            ClipboardUtil.TrySetText(textToCopy);
        };

        DropDownItems.Insert(index, item);
    }

    private string[]? ExtractRevisionTexts(Func<GitRevision, string>? extractRevisionText)
    {
        if (extractRevisionText is null)
        {
            return null;
        }

        IReadOnlyList<GitRevision>? gitRevisions = _revisionFunc?.Invoke();
        if (gitRevisions?.Count is not > 0)
        {
            return null;
        }

        return [.. gitRevisions.Select(extractRevisionText).Distinct()];
    }

    private void OnDropDownOpening(object? sender, EventArgs e)
    {
        IReadOnlyList<GitRevision>? revisions = _revisionFunc?.Invoke();
        if (revisions?.Count is not > 0)
        {
            HideDropDown();
            return;
        }

        DropDown.SuspendLayout();

        RemoveTransientItems();
        CollectRefNames(revisions, out List<string> branchNames, out List<string> tagNames);

        _itemNumber = 0;

        // Transient items (branches, tags) are inserted before the persistent fixed items.
        int insertionIndex = 0;
        InsertCaptionedRefItems(TranslatedStrings.Branches, branchNames, Images.Branch.AdaptLightness(), ref insertionIndex);
        InsertCaptionedRefItems(TranslatedStrings.Tags, tagNames, Images.Tag, ref insertionIndex);

        UpdateFixedRevisionItems(revisions);

        DropDown.ResumeLayout();
    }

    // Remove all items except the persistent named stubs, which must survive for toolbar
    // introspection. Transient items (branches, tags, separators, captions) are re-added later.
    private void RemoveTransientItems()
    {
        ToolStripItem[] persistentItems = [CommitHashMenuItem, MessageMenuItem, AuthorMenuItem, DateMenuItem];
        for (int i = DropDownItems.Count - 1; i >= 0; i--)
        {
            if (!Array.Exists(persistentItems, p => p == DropDownItems[i]))
            {
                DropDownItems.RemoveAt(i);
            }
        }
    }

    private void CollectRefNames(IReadOnlyList<GitRevision> revisions, out List<string> branchNames, out List<string> tagNames)
    {
        branchNames = [];
        tagNames = [];
        foreach (GitRevision revision in revisions)
        {
            GitRefListsForRevision refLists = new(revision);
            branchNames.AddRange(_filterRefsFunc(refLists.GetAllBranchNames()));
            tagNames.AddRange(_filterRefsFunc(refLists.GetAllTagNames()));
        }
    }

    private void InsertCaptionedRefItems(string captionText, List<string> names, Image image, ref int insertionIndex)
    {
        if (names.Count == 0)
        {
            return;
        }

        ToolStripMenuItem caption = new() { Text = captionText };
        MenuUtil.SetAsCaptionMenuItem(caption, Owner!);
        DropDownItems.Insert(insertionIndex++, caption);

        foreach (string name in names)
        {
            InsertItem(insertionIndex++, name, textToCopy: name, image, hotkey: null);
        }

        DropDownItems.Insert(insertionIndex++, new ToolStripSeparator());
    }

    private void UpdateFixedRevisionItems(IReadOnlyList<GitRevision> revisions)
    {
        int count = revisions.Count;
        UpdateFixedItem(CommitHashMenuItem, ResourceManager.TranslatedStrings.GetCommitHash(count), r => r.Guid, 'C');
        UpdateFixedItem(MessageMenuItem,    ResourceManager.TranslatedStrings.GetMessage(count),    r => r.Body ?? r.Subject, 'M');
        UpdateFixedItem(AuthorMenuItem,     ResourceManager.TranslatedStrings.GetAuthor(count),     r => $"{r.Author} <{r.AuthorEmail}>", 'A');

        if (count == 1 && revisions[0].AuthorDate == revisions[0].CommitDate)
        {
            // Single date: reuse the persistent DateMenuItem.
            UpdateFixedItem(DateMenuItem, ResourceManager.TranslatedStrings.Date, r => r.AuthorDate.ToString(), 'D');
            return;
        }

        // Two distinct dates: DateMenuItem shows AuthorDate, insert a transient CommitDate after it.
        UpdateFixedItem(DateMenuItem, ResourceManager.TranslatedStrings.GetAuthorDate(count), r => r.AuthorDate.ToString(), 'T');
        InsertTransientCommitDate(count);
    }

    private void InsertTransientCommitDate(int count)
    {
        string[]? commitDates = ExtractRevisionTexts(r => r.CommitDate.ToString());
        if (commitDates is null)
        {
            return;
        }

        string commitDateText = ResourceManager.TranslatedStrings.GetCommitDate(count)
            + ":   " + commitDates.Select(t => t.SubstringUntil('\n')).Join(", ").ShortenTo(40);
        int position = commitDateText.IndexOf("D", StringComparison.InvariantCultureIgnoreCase);
        if (position >= 0)
        {
            commitDateText = commitDateText.Insert(position, "&");
        }

        ToolStripMenuItem transientCommitDate = new()
        {
            Text = commitDateText.TrimEnd(Delimiters.LineFeedAndCarriageReturn),
            ShowShortcutKeys = true,
            Image = Images.Date,
        };
        string joined = commitDates.Join("\n");
        transientCommitDate.Click += (_, _) => ClipboardUtil.TrySetText(joined);
        DropDownItems.Insert(DropDownItems.IndexOf(DateMenuItem) + 1, transientCommitDate);
    }

    private void UpdateFixedItem(ToolStripMenuItem item, string displayText, Func<GitRevision, string> extractRevisionText, char hotkey)
    {
        string[]? textToCopy = ExtractRevisionTexts(extractRevisionText);
        if (textToCopy is null)
        {
            item.Visible = false;
            return;
        }

        item.Visible = true;

        string fullText = displayText + ":   " + textToCopy.Select(t => t.SubstringUntil('\n')).Join(", ").ShortenTo(40);
        int position = fullText.IndexOf(hotkey.ToString(), StringComparison.InvariantCultureIgnoreCase);
        if (position >= 0)
        {
            fullText = fullText.Insert(position, "&");
        }

        item.Text = fullText.TrimEnd(Delimiters.LineFeedAndCarriageReturn);
        item.ShowShortcutKeys = true;

        string joined = textToCopy.Join("\n");
        item.Click -= OnFixedItemClick;
        item.Tag = joined;
        item.Click += OnFixedItemClick;
    }

    private static void OnFixedItemClick(object? sender, EventArgs e)
    {
        if (sender is ToolStripMenuItem item && item.Tag is string text)
        {
            ClipboardUtil.TrySetText(text);
        }
    }

    private string PrependItemNumber(string name)
    {
        return ++_itemNumber > 10 ? name : "&" + (_itemNumber % 10) + ":   " + name;
    }
}
