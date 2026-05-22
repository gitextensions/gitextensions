using GitCommands.Utils;

namespace GitCommands;

public sealed class CommitTemplateItem
{
    public string Name { get; set; }
    public string Text { get; set; }
    public Image? Icon { get; set; }
    public bool IsRegex { get; set; }

    public CommitTemplateItem(string name, string text, Image? icon, bool isRegex)
    {
        Name = name;
        Text = text;
        Icon = icon;
        IsRegex = isRegex;
    }

    public CommitTemplateItem()
    {
        Name = string.Empty;
        Text = string.Empty;
        Icon = null;
        IsRegex = false;
    }

    public static void SaveToSettings(CommitTemplateItem[]? items)
    {
        string strVal = SerializeCommitTemplates(items);
        AppSettings.CommitTemplates = strVal;
    }

    public static CommitTemplateItem[]? LoadFromSettings()
    {
        string? serializedString = AppSettings.CommitTemplates;
        CommitTemplateItem[]? templates = DeserializeCommitTemplates(serializedString, out bool shouldBeUpdated);
        if (shouldBeUpdated)
        {
            SaveToSettings(templates!);
        }

        return templates;
    }

    private static string SerializeCommitTemplates(CommitTemplateItem[]? items)
    {
        return JsonSerializer.Serialize(items);
    }

    private static CommitTemplateItem[]? DeserializeCommitTemplates(string serializedString, out bool shouldBeUpdated)
    {
        shouldBeUpdated = false;
        if (string.IsNullOrEmpty(serializedString))
        {
            return null;
        }

        CommitTemplateItem[]? commitTemplateItem = null;
        try
        {
            commitTemplateItem = JsonSerializer.Deserialize<CommitTemplateItem[]>(serializedString);
        }
        catch
        {
            // do nothing
        }

        return commitTemplateItem;
    }
}
