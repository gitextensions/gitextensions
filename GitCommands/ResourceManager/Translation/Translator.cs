using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace ResourceManager.Translation
{
    public class Translator
    {
        private Translation translation;

        public Translator(string translationName)
        {
            translation = TranslationSerializer.Deserialize(@"C:\Development\GitUI\Translations\" + translationName + ".xml");
        }

        public string GetString(string category, string control, string property)
        {
            if (!translation.HasTranslationCategory(category))
                return null;


            TranslationCategory translationCategory = translation.GetTranslationCategory(category);
            if (!translationCategory.HasTranslationItem(control, property))
                return null;

            return translationCategory.GetTranslationItem(control, property).Value;
        }

        public void TranslateControl(Control controlToTranslate)
        {
            if (!translation.HasTranslationCategory(controlToTranslate.Name))
                return;

            TranslationCategory translationCategory = translation.GetTranslationCategory(controlToTranslate.Name);
            foreach (TranslationItem translationItem in translationCategory.GetTranslationItems())
            {
                object subControl;
                
                if (translationItem.Name.Equals("$this"))
                {
                    subControl = controlToTranslate;
                }
                else
                {
                    FieldInfo fieldInfo = controlToTranslate.GetType().GetField(translationItem.Name, BindingFlags.NonPublic | BindingFlags.Instance);
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
