using System.Xml.Serialization;

namespace GitExtensions.Extensibility.Translations.Xliff;

public class TranslationBody
{
    public TranslationBody()
    {
        TranslationItems = [];
    }

    [XmlElement(ElementName = "trans-unit")]
    public List<TranslationItem> TranslationItems { get; set; }

    public void AddTranslationItem(TranslationItem translationItem)
    {
        if (string.IsNullOrEmpty(translationItem.Name))
        {
            throw new InvalidOperationException($"Cannot add {nameof(TranslationItem)} without name");
        }

        TranslationItems.Add(translationItem);
    }

    public void AddTranslationItemIfNotExist(TranslationItem translationItem)
    {
        if (string.IsNullOrEmpty(translationItem.Name))
        {
            throw new InvalidOperationException($"Cannot add {nameof(TranslationItem)} without name");
        }

        TranslationItem? ti = GetTranslationItem(translationItem.Name, translationItem.Property);
        if (ti is null)
        {
            if (translationItem.Property == "ToolTipText")
            {
                ti = GetTranslationItem(translationItem.Name, "Text");
                if (ti is null || translationItem.Source != ti.Source)
                {
                    TranslationItems.Add(translationItem);
                }
            }
            else
            {
                TranslationItems.Add(translationItem);
            }
        }
        else
        {
            DebugHelpers.Assert(ti.Value == translationItem.Value, "ti.Value == translationItem.Value");
        }
    }

    public TranslationItem? GetTranslationItem(string name, string? property)
    {
        return TranslationItems.Find(t => t.Name is not null && t.Name.TrimStart('_') == name.TrimStart('_') && t.Property == property);
    }
}
