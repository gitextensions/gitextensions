using ResourceManager;

namespace GitUI.UserControls.RevisionGrid
{
    internal class FormQuickStringSelector : FormQuickItemSelector
    {
        private readonly TranslationString _actionSelect = new("Select");

        /// <summary>
        ///  Gets the string selected by the user.
        /// </summary>
        public string? SelectedString => SelectedItem as string;

        public void Init(IReadOnlyList<string> strings)
        {
            List<ItemData> items = strings.OrderBy(s => s).Select(s => new ItemData(s, s)).ToList();

            Init(items, _actionSelect.Text);
        }
    }
}
