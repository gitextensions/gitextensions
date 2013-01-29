using System;
using System.Collections.Generic;
using System.IO;

namespace ResourceManager.Translation
{
    public static class Translator
    {
        //Try to cache the translation as long as possible
        private static Translation _translation;
        private static string _name;

        public static Translation GetTranslation(string translationName)
        {
            if (string.IsNullOrEmpty(translationName))
            {
                Translator._translation = null;
            }
            else if (!translationName.Equals(Translator._name))
            {                
                Translator._translation = TranslationSerializer.Deserialize(Path.Combine(Translator.GetTranslationDir(), translationName + ".xml"));
            }
            Translator._name = translationName;
            return Translator._translation;
        }

        public static bool RunningOnWindows()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                    return true;
                default:
                    return false;
            }
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
                string translationDir = Translator.GetTranslationDir();
                if (!Directory.Exists(translationDir))
                {
                    return new string[0];
                }

                foreach (string fileName in Directory.GetFiles(translationDir, "*.xml"))
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
            Translation translation = GetTranslation(translationName);
            if (translation == null)
                return;
            obj.TranslateItems(translation);
        }
    }
}
