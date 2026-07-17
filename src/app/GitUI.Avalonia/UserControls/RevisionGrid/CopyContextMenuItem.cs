using Avalonia.Controls;
using Avalonia.Media;
using GitExtensions.Extensibility;
using GitExtUtils;
using GitUI.Properties;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGrid;

public sealed class CopyContextMenuItem : MenuItem
{
    private Func<IEnumerable<string>, IEnumerable<string>> _filterRefsFunc = refs => refs;
    private Func<IReadOnlyList<GitRevision>>? _revisionFunc;
    private uint _itemNumber;

    public CopyContextMenuItem()
    {
        Header = "_Copy to clipboard";
        Icon = CreateIcon(Images.CopyToClipboard);
    }

    public void SetFilterRefsFunc(Func<IEnumerable<string>, IEnumerable<string>> filterRefsFunc)
    {
        _filterRefsFunc = filterRefsFunc;
    }

    public void SetRevisionFunc(Func<IReadOnlyList<GitRevision>> revisionFunc)
    {
        _revisionFunc = revisionFunc;
    }

    public void RefreshItems()
    {
        Items.Clear();
        IReadOnlyList<GitRevision>? revisions = _revisionFunc?.Invoke();
        if (revisions?.Count is not > 0)
        {
            IsEnabled = false;
            return;
        }

        IsEnabled = true;

        List<string> branchNames = [];
        List<string> tagNames = [];
        foreach (GitRevision revision in revisions)
        {
            GitRefListsForRevision refLists = new(revision);
            branchNames.AddRange(_filterRefsFunc(refLists.GetAllBranchNames()));
            tagNames.AddRange(_filterRefsFunc(refLists.GetAllTagNames()));
        }

        _itemNumber = 0;
        AddRefItems(TranslatedStrings.Branches, branchNames, Images.Branch);
        AddRefItems(TranslatedStrings.Tags, tagNames, Images.Tag);

        int count = revisions.Count;
        AddRevisionItem(ResourceManager.TranslatedStrings.GetCommitHash(count), revision => revision.Guid, Images.CommitId, 'C');
        AddRevisionItem(ResourceManager.TranslatedStrings.GetMessage(count), revision => revision.Body ?? revision.Subject, Images.Message, 'M');
        AddRevisionItem(ResourceManager.TranslatedStrings.GetAuthor(count), revision => $"{revision.Author} <{revision.AuthorEmail}>", Images.Author, 'A');

        if (count == 1 && revisions[0].AuthorDate == revisions[0].CommitDate)
        {
            AddRevisionItem(ResourceManager.TranslatedStrings.Date, revision => revision.AuthorDate.ToString(), Images.Date, 'D');
        }
        else
        {
            AddRevisionItem(ResourceManager.TranslatedStrings.GetAuthorDate(count), revision => revision.AuthorDate.ToString(), Images.Date, 'T');
            AddRevisionItem(ResourceManager.TranslatedStrings.GetCommitDate(count), revision => revision.CommitDate.ToString(), Images.Date, 'D');
        }
    }

    private static Image CreateIcon(IImage image)
        => new()
        {
            Width = 16,
            Height = 16,
            Stretch = Stretch.Uniform,
            Source = image,
        };

    private void AddRefItems(string caption, IReadOnlyList<string> names, IImage image)
    {
        if (names.Count == 0)
        {
            return;
        }

        Items.Add(new MenuItem { Header = caption, IsEnabled = false });
        foreach (string name in names)
        {
            AddItem(name, name, image, hotkey: null);
        }

        Items.Add(new Separator());
    }

    private void AddRevisionItem(
        string displayText,
        Func<GitRevision, string> extractRevisionText,
        IImage image,
        char hotkey)
    {
        string[] textToCopy = [.. _revisionFunc!().Select(extractRevisionText).Distinct()];
        string preview = string.Join(", ", textToCopy.Select(text => text.SubstringUntil('\n'))).ShortenTo(40);
        AddItem($"{displayText}:   {preview}", string.Join('\n', textToCopy), image, hotkey);
    }

    private void AddItem(string displayText, string textToCopy, IImage image, char? hotkey)
    {
        string header = hotkey.HasValue
            ? InsertAccessKey(displayText, hotkey.Value)
            : PrependItemNumber(displayText);
        MenuItem item = new()
        {
            Header = EscapeHeader(header.TrimEnd(Delimiters.LineFeedAndCarriageReturn)),
            Icon = CreateIcon(image),
        };

        item.Click += delegate { ClipboardUtil.TrySetText(textToCopy); };
        Items.Add(item);
    }

    private static string EscapeHeader(string header)
    {
        int accessKeyIndex = header.IndexOf('_');
        return accessKeyIndex < 0
            ? header.Replace("_", "__", StringComparison.Ordinal)
            : header[..accessKeyIndex].Replace("_", "__", StringComparison.Ordinal)
                + "_"
                + header[(accessKeyIndex + 1)..].Replace("_", "__", StringComparison.Ordinal);
    }

    private static string InsertAccessKey(string text, char hotkey)
    {
        int position = text.IndexOf(hotkey.ToString(), StringComparison.InvariantCultureIgnoreCase);
        return position >= 0 ? text.Insert(position, "_") : text;
    }

    private string PrependItemNumber(string name)
        => ++_itemNumber > 10 ? name : $"_{_itemNumber % 10}:   {name}";
}
