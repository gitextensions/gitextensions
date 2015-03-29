using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using ResourceManager.Xliff;

namespace ResourceManager
{
    public static class Translator
    {
        private static readonly string EnglishTranslationName = "English";
        //Try to cache the translation as long as possible
        private static IDictionary<string, TranslationFile> _translation = new Dictionary<string, TranslationFile>();
        private static string _name;

        public static IDictionary<string, TranslationFile> GetTranslation(string translationName)
        {
            if (string.IsNullOrEmpty(translationName))
            {
                _translation = new Dictionary<string, TranslationFile>();
            }
            else if (!translationName.Equals(_name))
            {
                _translation = new Dictionary<string, TranslationFile>();
                var result = Directory.EnumerateFiles(GetTranslationDir(), translationName + "*.xlf");
                foreach (var file in result)
                {
                    var name = Path.GetFileNameWithoutExtension(file).Substring(translationName.Length);
                    var t = TranslationSerializer.Deserialize(file) ??
                            new TranslationFile();
                    _translation[name] = t;
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
            List<string> translations = new List<string>();
            try
            {
                string translationDir = GetTranslationDir();
                if (!Directory.Exists(translationDir))
                {
                    return new string[0];
                }

                foreach (string fileName in Directory.GetFiles(translationDir, "*.xlf"))
                {
                    var name = Path.GetFileNameWithoutExtension(fileName);
                    if (name.IndexOf(".") > 0)
                        continue;
                    if (String.Compare(EnglishTranslationName, name, StringComparison.CurrentCultureIgnoreCase) == 0)
                        continue;
                    translations.Add(name);
                }
            } catch
            {

            }
            return translations.ToArray();
        }

        public static void Translate(ITranslate obj, string translationName)
        {
            var translation = GetTranslation(translationName);
            if (translation.Count == 0)
                return;
            foreach (var pair in translation)
            {
                obj.TranslateItems(pair.Value);
            }
        }
    }
}
