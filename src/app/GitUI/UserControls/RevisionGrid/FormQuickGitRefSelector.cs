using GitExtensions.Extensibility.Git;
using ResourceManager;

namespace GitUI.UserControls.RevisionGrid
{
    internal class FormQuickGitRefSelector : FormQuickItemSelector
    {
        private const string _separator = "――――――――――――――――――";
        private readonly TranslationString _actionRename = new("Rename");
        private readonly TranslationString _actionDelete = new("Delete");
        private readonly TranslationString _actionSelect = new("Select");
        private readonly TranslationString _local = new("local");
        private readonly TranslationString _remote = new("remote");
        private readonly TranslationString _tag = new("tag");

        /// <summary>
        /// Gets the ref selected by the user.
        /// </summary>
        public IGitRef? SelectedRef => SelectedItem as IGitRef;

        public void Init(QuickAction action, IReadOnlyList<IGitRef> refs)
        {
            List<ItemData> items = Filter(refs, _local, _remote, _tag, r => r.IsHead);
            items.AddRange(Filter(refs, _local, _remote, _tag, r => r.IsRemote));
            items.AddRange(Filter(refs, _local, _remote, _tag, r => r.IsTag));

            // if there are any items, then skip the header and select the first actual item.
            int selectedIndex = 0;
            if (items.Count > 0)
            {
                selectedIndex = 1;
            }

            switch (action)
            {
                case QuickAction.Delete:
                    {
                        Init(items, _actionDelete.Text, selectedIndex);
                    }

                    break;
                case QuickAction.Rename:
                    {
                        Init(items, _actionRename.Text, selectedIndex);
                    }

                    break;
                case QuickAction.Select:
                    {
                        Init(items, _actionSelect.Text, selectedIndex);
                    }

                    break;
            }

            return;

            static List<ItemData> Filter(IReadOnlyList<IGitRef> sourceRefs, TranslationString localText, TranslationString remoteText, TranslationString tagText, Func<IGitRef, bool> selector)
            {
                List<ItemData> list = sourceRefs.Where(r => selector(r))
                                                .OrderBy(r => r.Name)
                                                .Select(r => new ItemData(label: r.Name, item: r))
                                                .ToList();

                if (list.Count > 0)
                {
                    IGitRef gitRef = (IGitRef)list[0].Item;

                    TranslationString chosenText = gitRef switch
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
        }

        public enum QuickAction
        {
            Rename = 0,
            Delete,
            Select,
        }
    }
}
