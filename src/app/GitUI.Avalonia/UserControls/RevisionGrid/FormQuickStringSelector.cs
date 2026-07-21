using ResourceManager;

namespace GitUI.UserControls.RevisionGrid;

internal sealed class FormQuickStringSelector : FormQuickItemSelector
{
    private readonly TranslationString _actionSelect = new("Select");

    public string? SelectedString => SelectedItem as string;

    public void Init(IReadOnlyList<string> strings)
    {
        List<ItemData> items = [.. strings.OrderBy(value => value).Select(value => new ItemData(value, value))];
        Init(items, _actionSelect.Text);
    }
}
