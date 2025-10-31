using System.Runtime.Serialization;
using GitCommands.Utils;

namespace GitCommands;

[Serializable]
public sealed class CommitTemplateItem : ISerializable
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
    }

    private CommitTemplateItem(SerializationInfo info, StreamingContext context)
    {
        Name = (string)info.GetValue("Name", typeof(string));
        Text = (string)info.GetValue("Text", typeof(string));

        if (HasKey(info, "IsRegex"))
        {
            IsRegex = (bool)info.GetValue("IsRegex", typeof(bool));
        }
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("Name", Name);
        info.AddValue("Text", Text);
        info.AddValue("IsRegex", IsRegex);
    }

    private bool HasKey(SerializationInfo info, string key)
    {
        foreach (SerializationEntry entry in info)
        {
            if (entry.Name == key)
            {
                return true;
            }
        }

        return false;
    }

    public static void SaveToSettings(CommitTemplateItem[]? items)
    {
        string strVal = SerializeCommitTemplates(items);
        AppSettings.CommitTemplates = strVal;
    }

    public static CommitTemplateItem[]? LoadFromSettings()
    {
        string serializedString = AppSettings.CommitTemplates;
        CommitTemplateItem[] templates = DeserializeCommitTemplates(serializedString, out bool shouldBeUpdated);
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
