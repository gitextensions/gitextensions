using System;
using System.Collections.Generic;
using System.IO;

namespace ResourceManager.Translation
{
    public static class Translator
    {
        //Try to cache the translation as long as possible
        private static ITranslation _translation;
        private static string _name;
        private const bool UseXliff = false;

        public static ITranslation GetTranslation(string translationName)
        {
            if (string.IsNullOrEmpty(translationName))
            {
                _translation = null;
            }
            else if (!translationName.Equals(_name))
            {
                if (UseXliff)
                    _translation = Xliff.TranslationSerializer.Deserialize(Path.Combine(Translator.GetTranslationDir(), translationName + ".xliff"));
                else
                    _translation = Xml.TranslationSerializer.Deserialize(Path.Combine(GetTranslationDir(), translationName + ".xml"));
                
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

                foreach (string fileName in Directory.GetFiles(translationDir, UseXliff ? "*.xliff" : "*.xml"))
                {
                    FileInfo fileInfo = new FileInfo(fileName);
                    translations.Add(fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length));
                }
            } catch
            {
                
            }
            return translations.ToArray();
        }

        public static void Translate(ITranslate obj, string translationName)
        {
            ITranslation translation = GetTranslation(translationName);
            if (translation == null)
                return;
            obj.TranslateItems(translation);
        }
    }
}
