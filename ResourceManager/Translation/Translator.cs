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
        private Translation translation;

        public Translator(string translationName)
        {
            name = translationName;
            if (!string.IsNullOrEmpty(translationName))
            {
                translation = TranslationSerializer.Deserialize(Translator.GetTranslationDir() + @"\" + translationName + ".xml");
            }
        }

        private string name;
        public string Name
        {
            get 
            {
                return name;
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
            if (translation == null)
                return null; 
            if (!translation.HasTranslationCategory(category))
                return null;


            TranslationCategory translationCategory = translation.GetTranslationCategory(category);
            if (!translationCategory.HasTranslationItem(control, property))
                return null;

            return translationCategory.GetTranslationItem(control, property).Value;
        }

        public void TranslateControl(Control controlToTranslate)
        {
            if (translation == null)
                return; 
            if (!translation.HasTranslationCategory(controlToTranslate.Name))
                return;

            TranslationCategory translationCategory = translation.GetTranslationCategory(controlToTranslate.Name);
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
