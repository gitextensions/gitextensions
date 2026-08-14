using Avalonia.Controls;
using Avalonia.Media;
using GitExtensions.Extensibility;
using GitExtUtils;
using GitUI.Compat;
using GitUI.Properties;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.UserControls.RevisionGrid;

public sealed class CopyContextMenuItem : MenuItem
{
    private readonly TranslationString _copyToClipboardText = new("&Copy to clipboard");
    private Func<IEnumerable<string>, IEnumerable<string>> _filterRefsFunc = refs => refs;
    private Func<IReadOnlyList<GitRevision>>? _revisionFunc;
    private uint _itemNumber;

    // Avalonia requires derived controls to opt into the base MenuItem theme.
    protected override Type StyleKeyOverride => typeof(MenuItem);

    public CopyContextMenuItem()
    {
        Icon = CreateIcon(Images.CopyToClipboard);
        Header = AvaloniaTranslationUtils.ToAvaloniaMnemonics(_copyToClipboardText.Text);

        SubmenuOpened += OnDropDownOpening;
    }

    public void SetFilterRefsFunc(Func<IEnumerable<string>, IEnumerable<string>> filterRefsFunc)
    {
        _filterRefsFunc = filterRefsFunc;
    }

    public void SetRevisionFunc(Func<IReadOnlyList<GitRevision>> revisionFunc)
    {
        _revisionFunc = revisionFunc;

        // Add dummy item for the menu entry to appear expandable (triangle on the right)
        Items.Add(new MenuItem());
    }

    public void RefreshItems()
    {
        OnDropDownOpening(this, EventArgs.Empty);
    }

    private void AddItem(string displayText, Func<GitRevision, string> extractRevisionText, IImage image, char? hotkey)
    {
        string[]? textToCopy = ExtractRevisionTexts(extractRevisionText);
        if (textToCopy is null)
        {
            return;
        }

        displayText += ":   " + textToCopy.Select(t => t.SubstringUntil('\n')).Join(", ").ShortenTo(40);
        AddItem(displayText, textToCopy.Join("\n"), image, hotkey);
    }

    private void AddItem(string displayText, string textToCopy, IImage image, char? hotkey)
    {
        if (hotkey.HasValue)
        {
            int position = displayText.IndexOf(hotkey.Value.ToString(), StringComparison.InvariantCultureIgnoreCase);
            if (position >= 0)
            {
                displayText = displayText.Insert(position, "_");
            }
        }
        else
        {
            displayText = PrependItemNumber(displayText);
        }

        MenuItem item = new()
        {
            Header = EscapeHeader(displayText.TrimEnd(Delimiters.LineFeedAndCarriageReturn)),
            Icon = CreateIcon(image),
        };

        item.Click += delegate
        {
            ClipboardUtil.TrySetText(textToCopy);
        };

        Items.Add(item);
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
            IsSubMenuOpen = false;
            return;
        }

        Items.Clear();

        List<string> branchNames = [];
        List<string> tagNames = [];
        foreach (GitRevision revision in revisions)
        {
            GitRefListsForRevision refLists = new(revision);
            branchNames.AddRange(_filterRefsFunc(refLists.GetAllBranchNames()));
            tagNames.AddRange(_filterRefsFunc(refLists.GetAllTagNames()));
        }

        _itemNumber = 0;

        // Add items for branches
        if (branchNames.Count != 0)
        {
            MenuItem caption = new() { Header = TranslatedStrings.Branches };
            MenuUtil.SetAsCaptionMenuItem(caption, this);
            Items.Add(caption);

            foreach (string name in branchNames)
            {
                AddItem(name, textToCopy: name, Images.Branch.AdaptLightness(), hotkey: null);
            }

            Items.Add(new Separator());
        }

        // Add items for tags
        if (tagNames.Count != 0)
        {
            MenuItem caption = new() { Header = TranslatedStrings.Tags };
            MenuUtil.SetAsCaptionMenuItem(caption, this);
            Items.Add(caption);

            foreach (string name in tagNames)
            {
                AddItem(name, textToCopy: name, Images.Tag, hotkey: null);
            }

            Items.Add(new Separator());
        }

        // Add other items
        int count = revisions.Count;
        AddItem(ResourceManager.TranslatedStrings.GetCommitHash(count), r => r.Guid, Images.CommitId, 'C');
        AddItem(ResourceManager.TranslatedStrings.GetMessage(count), r => r.Body ?? r.Subject, Images.Message, 'M');
        AddItem(ResourceManager.TranslatedStrings.GetAuthor(count), r => $"{r.Author} <{r.AuthorEmail}>", Images.Author.AdaptLightness(), 'A');

        if (count == 1 && revisions[0].AuthorDate == revisions[0].CommitDate)
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
        return ++_itemNumber > 10 ? name : "_" + (_itemNumber % 10) + ":   " + name;
    }

    private static Image CreateIcon(IImage image)
        => new()
        {
            Width = 16,
            Height = 16,
            Stretch = Stretch.Uniform,
            Source = image,
        };

    private static string EscapeHeader(string header)
    {
        int accessKeyIndex = header.IndexOf('_');
        return accessKeyIndex < 0
            ? header.Replace("_", "__", StringComparison.Ordinal)
            : header[..accessKeyIndex].Replace("_", "__", StringComparison.Ordinal)
                + "_"
                + header[(accessKeyIndex + 1)..].Replace("_", "__", StringComparison.Ordinal);
    }
}
