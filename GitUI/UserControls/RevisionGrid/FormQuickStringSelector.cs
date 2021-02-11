using System.Collections.Generic;
using System.Linq;
using ResourceManager;

namespace GitUI.UserControls.RevisionGrid
{
    public class FormQuickStringSelector : FormQuickItemSelector
    {
        private readonly TranslationString _actionSelect = new("Select");

        /// <summary>
        /// Gets the string selected by the user.
        /// </summary>
        public string? SelectedString => SelectedItem as string;

        public void Init(IReadOnlyList<string> strings)
        {
            var items = strings.OrderBy(s => s).Select(s => new ItemData(s, s)).ToList();

            Init(items, _actionSelect.Text);
        }
    }
}
