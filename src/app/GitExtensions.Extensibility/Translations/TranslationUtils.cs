using GitExtensions.Extensibility.Translations.Xliff;

namespace GitExtensions.Extensibility.Translations;

public static class TranslationUtils
{
    public static IEnumerable<(string Name, object Item)> GetObjFields(object obj, string objName)
    {
        return TranslationUtil.GetObjFields(obj, objName);
    }

    public static void AddTranslationItem(string category, object obj, string property, ITranslation translation)
    {
        TranslationUtil.AddTranslationItem(category, obj, property, translation);
    }

    public static void AddTranslationItemsFromFields(string? category, object obj, ITranslation translation)
    {
        if (!string.IsNullOrEmpty(category))
        {
            TranslationUtil.AddTranslationItemsFromFields(category, obj, translation);
        }
    }

    public static void AddTranslationItemsFromList(string category, ITranslation translation, IEnumerable<(string Name, object Item)> items)
    {
        TranslationUtil.AddTranslationItemsFromList(category, translation, items);
    }

    public static void TranslateProperty(string category, object obj, string property, ITranslation translation)
    {
        TranslationUtil.TranslateProperty(category, obj, property, translation);
    }

    public static void TranslateItemsFromList(string category, ITranslation translation, IEnumerable<(string Name, object Item)> items)
    {
        TranslationUtil.TranslateItemsFromList(category, translation, items);
    }

    public static void TranslateItemsFromFields(string? category, object obj, ITranslation translation)
    {
        if (!string.IsNullOrEmpty(category))
        {
            TranslationUtil.TranslateItemsFromFields(category, obj, translation);
        }
    }
}
