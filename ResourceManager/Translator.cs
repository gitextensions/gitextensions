using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.IO;

namespace ResourceManager.Translation
{
    public class Translator
    {
        //Try to cache the translation as long as possible
        private static Translation translation;
        private static string name;

        public Translator(string translationName)
        {
            if (string.IsNullOrEmpty(translationName))
            {
                Translator.translation = null;
            } else
            if (!translationName.Equals(Translator.name))
            {
                Translator.translation = TranslationSerializer.Deserialize(Translator.GetTranslationDir() + @"\" + translationName + ".xml");
            }
            Translator.name = translationName;
        }

        public string Name
        {
            get
            {
                return Translator.name;
            }
        }

        public static string GetTranslationDir()
        {
            return typeof(Translator).Assembly.Location.Substring(0, typeof(Translator).Assembly.Location.LastIndexOf("\\")) + @"\Translation";
        }

        public static string[] GetAllTranslations()
        {
            List<string> translations = new List<string>();
            foreach (string fileName in Directory.GetFiles(Translator.GetTranslationDir(), "*.xml"))
            {
                FileInfo fileInfo = new FileInfo(fileName);
                translations.Add(fileInfo.Name.Substring(0, fileInfo.Name.Length-fileInfo.Extension.Length));
            }
            return translations.ToArray();
        }

        public string GetString(string category, string control, string property)
        {
            if (Translator.translation == null)
                return null;
            if (!Translator.translation.HasTranslationCategory(category))
                return null;


            TranslationCategory translationCategory = Translator.translation.GetTranslationCategory(category);
            if (!translationCategory.HasTranslationItem(control, property))
                return null;

            return translationCategory.GetTranslationItem(control, property).Value;
        }

        public void TranslateControl(object controlToTranslate)
        {
            if (Translator.translation == null)
                return;

            string name;

            if (controlToTranslate is Control)
                name = ((Control)controlToTranslate).Name;
            else
                name = controlToTranslate.GetType().Name;

            if (!Translator.translation.HasTranslationCategory(name))
                return;

            TranslationCategory translationCategory = Translator.translation.GetTranslationCategory(name);
            foreach (TranslationItem translationItem in translationCategory.GetTranslationItems())
            {
                object subControl = null;
                
                if (translationItem.Name.Equals("$this"))
                {
                    subControl = controlToTranslate;
                }
                else
                {
                    FieldInfo fieldInfo = controlToTranslate.GetType().GetField(translationItem.Name, BindingFlags.NonPublic | BindingFlags.Instance);
                    if (fieldInfo != null)
                        subControl = fieldInfo.GetValue(controlToTranslate);
                }

                if (subControl == null)
                    return;

                PropertyInfo propertyInfo = subControl.GetType().GetProperty(translationItem.Property, BindingFlags.Public | BindingFlags.Instance);
                if (propertyInfo != null)
                    propertyInfo.SetValue(subControl, translationItem.Value, null);
            }
        }
    }
}
