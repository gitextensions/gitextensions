using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ResourceManager.Xliff;

namespace ResourceManager
{
    public static class Translator
    {
        private const string EnglishTranslationName = "English";

        // Try to cache the translation as long as possible
        private static IDictionary<string, TranslationFile> _translation = new Dictionary<string, TranslationFile>();
        private static string _name;

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
                    var result = Directory.EnumerateFiles(translationsDir, translationName + "*.xlf");
                    foreach (var file in result)
                    {
                        var name = Path.GetFileNameWithoutExtension(file).Substring(translationName.Length);
                        var t = TranslationSerializer.Deserialize(file) ??
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
            var translations = new List<string>();
            try
            {
                string translationDir = GetTranslationDir();
                if (!Directory.Exists(translationDir))
                {
                    return Array.Empty<string>();
                }

                foreach (string fileName in Directory.GetFiles(translationDir, "*.xlf"))
                {
                    var name = Path.GetFileNameWithoutExtension(fileName);
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
            var translation = GetTranslation(translationName);
            if (translation.Count == 0)
            {
                return;
            }

            foreach (var pair in translation)
            {
                obj.TranslateItems(pair.Value);
            }
        }
    }
}
