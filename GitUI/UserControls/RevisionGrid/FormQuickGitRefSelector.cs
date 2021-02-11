using System.Collections.Generic;
using System.Linq;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.UserControls.RevisionGrid
{
    public class FormQuickGitRefSelector : FormQuickItemSelector
    {
        private readonly TranslationString _actionRename = new("Rename");
        private readonly TranslationString _actionDelete = new("Delete");
        private readonly TranslationString _actionSelect = new("Select");
        private readonly TranslationString _tag = new("tag");

        /// <summary>
        /// Gets the ref selected by the user.
        /// </summary>
        public IGitRef? SelectedRef => SelectedItem as IGitRef;

        public void Init(Action action, IReadOnlyList<IGitRef> refs)
        {
            var items = refs.OrderBy(r => r.IsTag).ThenBy(r => r.Name).Select(GetItemData).ToList();

            ItemData GetItemData(IGitRef gitRef)
            {
                var suffix = gitRef.IsTag ? $" ({_tag.Text})" : string.Empty;
                return new ItemData($"{gitRef.Name}{suffix}", gitRef);
            }

            switch (action)
            {
                case Action.Delete:
                    {
                        Init(items, _actionDelete.Text);
                    }

                    break;
                case Action.Rename:
                    {
                        Init(items, _actionRename.Text);
                    }

                    break;
                case Action.Select:
                    {
                        Init(items, _actionSelect.Text);
                    }

                    break;
            }
        }

        public enum Action
        {
            Rename = 0,
            Delete,
            Select,
        }
    }
}
