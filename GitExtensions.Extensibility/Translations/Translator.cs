using GitExtensions.Extensibility.Translations.Xliff;

namespace GitExtensions.Extensibility.Translations;

public static class Translator
{
    private const string EnglishTranslationName = "English";

    // Try to cache the translation as long as possible
    private static IDictionary<string, TranslationFile> _translation = new Dictionary<string, TranslationFile>();
    private static string? _name;

    public static IDictionary<string, TranslationFile> GetTranslation(string translationName)
    {
        if (string.IsNullOrEmpty(translationName))
        {
            _translation = new Dictionary<string, TranslationFile>();
        }
        else if (translationName != _name)
        {
            _translation = new Dictionary<string, TranslationFile>();
            string translationsDir = GetTranslationDir();
            if (Directory.Exists(translationsDir))
            {
                IEnumerable<string> result = Directory.EnumerateFiles(translationsDir, translationName + "*.xlf");
                foreach (string file in result)
                {
                    string name = Path.GetFileNameWithoutExtension(file)[translationName.Length..];
                    TranslationFile t = TranslationSerializer.Deserialize(file) ??
                            new TranslationFile();
                    t.SourceLanguage = t.TranslationCategories.FirstOrDefault()?.SourceLanguage;
                    t.TargetLanguage = t.TranslationCategories.FirstOrDefault()?.TargetLanguage;
                    _translation[name] = t;
                }
            }
        }

        _name = translationName;
        return _translation;
    }

    public static string GetTranslationDir()
    {
        return Path.Combine(Path.GetDirectoryName(typeof(Translator).Assembly.Location), "Translation");
    }

    public static string[] GetAllTranslations()
    {
        List<string> translations = [];
        try
        {
            string translationDir = GetTranslationDir();
            if (!Directory.Exists(translationDir))
            {
                return Array.Empty<string>();
            }

            foreach (string fileName in Directory.GetFiles(translationDir, "*.xlf"))
            {
                string name = Path.GetFileNameWithoutExtension(fileName);
                if (name.IndexOf(".") > 0)
                {
                    continue;
                }

                if (string.Compare(EnglishTranslationName, name, StringComparison.CurrentCultureIgnoreCase) == 0)
                {
                    continue;
                }

                translations.Add(name);
            }
        }
        catch
        {
        }

        return translations.ToArray();
    }

    public static void Translate(ITranslate obj, string translationName)
    {
        IDictionary<string, TranslationFile> translation = GetTranslation(translationName);
        if (translation.Count == 0)
        {
            return;
        }

        foreach (KeyValuePair<string, TranslationFile> pair in translation)
        {
            obj.TranslateItems(pair.Value);
        }
    }
}
