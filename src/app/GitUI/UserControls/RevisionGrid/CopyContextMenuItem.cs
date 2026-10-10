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
    private Func<string, bool> _copyText = ClipboardUtil.TrySetText;
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

        // A toolbar button can click these items without the Copy menu ever being opened, so
        // what they copy is read from the selection at click time, never kept from the last time
        // the menu was shown.
        CommitHashMenuItem.Click += (_, _) => CopyRevisionTexts(CommitHashOf);
        MessageMenuItem.Click += (_, _) => CopyRevisionTexts(MessageOf);
        AuthorMenuItem.Click += (_, _) => CopyRevisionTexts(AuthorOf);
        DateMenuItem.Click += (_, _) => CopyRevisionTexts(AuthorDateOf);

        DropDownOpening += OnDropDownOpening;
    }

    private static string CommitHashOf(GitRevision revision) => revision.Guid;

    private static string MessageOf(GitRevision revision) => revision.Body ?? revision.Subject;

    private static string AuthorOf(GitRevision revision) => $"{revision.Author} <{revision.AuthorEmail}>";

    private static string AuthorDateOf(GitRevision revision) => revision.AuthorDate.ToString();

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
            _copyText(textToCopy);
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
        UpdateFixedItem(CommitHashMenuItem, ResourceManager.TranslatedStrings.GetCommitHash(count), CommitHashOf, 'C');
        UpdateFixedItem(MessageMenuItem,    ResourceManager.TranslatedStrings.GetMessage(count),    MessageOf, 'M');
        UpdateFixedItem(AuthorMenuItem,     ResourceManager.TranslatedStrings.GetAuthor(count),     AuthorOf, 'A');

        if (count == 1 && revisions[0].AuthorDate == revisions[0].CommitDate)
        {
            // Single date: reuse the persistent DateMenuItem.
            UpdateFixedItem(DateMenuItem, ResourceManager.TranslatedStrings.Date, AuthorDateOf, 'D');
            return;
        }

        // Two distinct dates: DateMenuItem shows AuthorDate, insert a transient CommitDate after it.
        UpdateFixedItem(DateMenuItem, ResourceManager.TranslatedStrings.GetAuthorDate(count), AuthorDateOf, 'T');
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
        transientCommitDate.Click += (_, _) => _copyText(joined);
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
    }

    private void CopyRevisionTexts(Func<GitRevision, string> extractRevisionText)
    {
        string[]? textToCopy = ExtractRevisionTexts(extractRevisionText);
        if (textToCopy is not null)
        {
            _copyText(textToCopy.Join("\n"));
        }
    }

    private string PrependItemNumber(string name)
    {
        return ++_itemNumber > 10 ? name : "&" + (_itemNumber % 10) + ":   " + name;
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(CopyContextMenuItem item)
    {
        // Stands in for the clipboard, so a test can see what would be copied without touching it.
        public Func<string, bool> CopyText
        {
            set => item._copyText = value;
        }
    }
}
