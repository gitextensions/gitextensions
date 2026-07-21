using GitExtensions.Extensibility.Git;
using ResourceManager;

namespace GitUI.UserControls.RevisionGrid;

internal sealed class FormQuickGitRefSelector : FormQuickItemSelector
{
    private const string _separator = "――――――――――――――――――";
    private readonly TranslationString _actionRename = new("Rename");
    private readonly TranslationString _actionDelete = new("Delete");
    private readonly TranslationString _actionSelect = new("Select");
    private readonly TranslationString _local = new("local");
    private readonly TranslationString _remote = new("remote");
    private readonly TranslationString _tag = new("tag");

    public IGitRef? SelectedRef => SelectedItem as IGitRef;

    public void Init(QuickAction action, IReadOnlyList<IGitRef> refs)
    {
        List<ItemData> items = Filter(refs, _local, _remote, _tag, gitRef => gitRef.IsHead);
        items.AddRange(Filter(refs, _local, _remote, _tag, gitRef => gitRef.IsRemote));
        items.AddRange(Filter(refs, _local, _remote, _tag, gitRef => gitRef.IsTag));
        int selectedIndex = items.Count > 0 ? 1 : 0;

        Init(
            items,
            action switch
            {
                QuickAction.Delete => _actionDelete.Text,
                QuickAction.Rename => _actionRename.Text,
                _ => _actionSelect.Text,
            },
            selectedIndex);
    }

    private static List<ItemData> Filter(
        IReadOnlyList<IGitRef> sourceRefs,
        TranslationString localText,
        TranslationString remoteText,
        TranslationString tagText,
        Func<IGitRef, bool> selector)
    {
        List<ItemData> list = [.. sourceRefs.Where(selector).OrderBy(gitRef => gitRef.Name).Select(gitRef => new ItemData(gitRef.Name, gitRef))];
        if (list.Count > 0)
        {
            IGitRef gitRef = (IGitRef)list[0].Item;
            TranslationString? chosenText = gitRef switch
            {
                { IsHead: true } => localText,
                { IsRemote: true } => remoteText,
                { IsTag: true } => tagText,
                _ => null,
            };
            string label = $"{chosenText} {_separator}"[.._separator.Length];
            list.Insert(0, new ItemData(label, _separator));
        }

        return list;
    }

    public enum QuickAction
    {
        Rename = 0,
        Delete,
        Select,
    }
}
