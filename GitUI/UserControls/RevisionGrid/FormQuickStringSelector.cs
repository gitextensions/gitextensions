using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using ResourceManager;

namespace GitUI.UserControls.RevisionGrid
{
    public class FormQuickStringSelector : FormQuickItemSelector
    {
        private readonly TranslationString _actionSelect = new TranslationString("Select");

        /// <summary>
        /// Gets the string selected by the user.
        /// </summary>
        [CanBeNull]
        public string SelectedString => SelectedItem as string;

        public void Init(IReadOnlyList<string> strings)
        {
            var items = strings.OrderBy(s => s).Select(s => new ItemData() { Label = s, Item = s }).ToList();

            Init(items, _actionSelect.Text);
        }
    }
}
