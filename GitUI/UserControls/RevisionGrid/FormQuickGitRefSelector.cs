using System.Collections.Generic;
using System.Linq;
using GitUIPluginInterfaces;
using JetBrains.Annotations;
using ResourceManager;

namespace GitUI.UserControls.RevisionGrid
{
    public class FormQuickGitRefSelector : FormQuickItemSelector
    {
        private readonly TranslationString _actionRename = new TranslationString("Rename");
        private readonly TranslationString _actionDelete = new TranslationString("Delete");
        private readonly TranslationString _actionSelect = new TranslationString("Select");
        private readonly TranslationString _tag = new TranslationString("tag");

        /// <summary>
        /// Gets the ref selected by the user.
        /// </summary>
        [CanBeNull]
        public IGitRef SelectedRef => SelectedItem as IGitRef;

        public void Init(Action action, IReadOnlyList<IGitRef> refs)
        {
            var items = refs.OrderBy(r => r.IsTag).ThenBy(r => r.Name).Select(GetItemData).ToList();

            ItemData GetItemData(IGitRef gitRef)
            {
                var suffix = gitRef.IsTag ? $" ({_tag.Text})" : string.Empty;
                return new ItemData
                {
                    Label = $"{gitRef.Name}{suffix}",
                    Item = gitRef
                };
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
