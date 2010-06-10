using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using ResourceManager.Translation;
using ResourceManager;

namespace GitUI
{
    public partial class FormTranslate : GitExtensionsForm
    {
        //TranslationStrings
        TranslationString translateProgressText = new TranslationString("Translated {0} out of {1}");
        TranslationString allText = new TranslationString("All");
        TranslationString saveAsText = new TranslationString("Save as");

        protected class TranslateItem
        {
            public string Category { get; set; }
            public string Name { get; set; }
            public string Property { get; set; }
            public string NeutralValue { get; set; }
            public string TranslatedValue { get; set; }
        }

        private List<TranslateItem> translate;

        Translation neutralTranslation = new Translation();
        Translator translator;

        public FormTranslate()
        {
            InitializeComponent(); Translate();

            translations.Items.Clear();
            translations.Items.Add("Translations_nl");
            translations.Items.Add("Translations_ja");

            GetPropertiesToTranslate();
            LoadTranslation();
            FillTranslateGrid(allText.Text);
        }

        private void UpdateProgress()
        {
            int translatedCount = 0;
            foreach(TranslateItem translateItem in translate)
            {
                if (!string.IsNullOrEmpty(translateItem.TranslatedValue))
                    translatedCount++;
            }
            translateProgress.Text = string.Format(translateProgressText.Text, translatedCount, translate.Count);
        }

        private void LoadTranslation()
        {
            translate = new List<TranslateItem>();

            foreach (TranslationCategory translationCategory in neutralTranslation.GetTranslationCategories())
            {

                foreach (TranslationItem translationItem in translationCategory.GetTranslationItems())
                {
                    TranslateItem translateItem = new TranslateItem();
                    translateItem.Category = translationCategory.Name;
                    translateItem.Name = translationItem.Name;
                    translateItem.Property = translationItem.Property;
                    translateItem.NeutralValue = translationItem.Value;

                    if (translator != null)
                        translateItem.TranslatedValue = translator.GetString(translationCategory.Name, translateItem.Name, translateItem.Property);

                    translate.Add(translateItem);
                }
            }
            
            UpdateProgress();
        }

        private void FillTranslateGrid(string filter)
        {
            translations.Items.Clear();
            translations.Items.AddRange(Translator.GetAllTranslations());

            if (translate == null)
                return;

            translations.SelectedText = translator.Name;

            List<TranslateItem> filterTranslate = new List<TranslateItem>();

            foreach(TranslateItem translateItem in translate)
            {
                if (!string.IsNullOrEmpty(filter) &&
                    !filter.Equals(allText.Text) &&
                    !filter.Equals(translateItem.Category))
                                    continue;

                //Skip translated items if filter is on
                if (hideTranslatedItems.Checked && !string.IsNullOrEmpty(translateItem.TranslatedValue))
                    continue;

                filterTranslate.Add(translateItem);
            }

            translateGrid.DataSource = filterTranslate;

            UpdateProgress();
        }

        public void GetPropertiesToTranslate()
        {
            translateCategories.Items.Clear();
            translateCategories.Items.Add(allText.Text);


            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (typeof(GitExtensionsControl).IsAssignableFrom(type) ||
                        typeof(GitExtensionsForm).IsAssignableFrom(type))
                    {
                        Control control = null;


                        if (type == this.GetType())
                            control = this;
                        else
                        // try to find parameter less constructor first
                        foreach (ConstructorInfo constructor in type.GetConstructors())
                        {
                            if (constructor.GetParameters().Length == 0)
                                control = (Control)Activator.CreateInstance(type);
                        }

                        if (control == null && type.GetConstructors().Length > 0)
                        {
                            ConstructorInfo parameterConstructor = type.GetConstructors()[0];
                            List<object> parameters = new List<object>(parameterConstructor.GetParameters().Length);
                            for (int i = 0; i < parameterConstructor.GetParameters().Length; i++)
                                parameters.Add(null);
                            control = (Control)parameterConstructor.Invoke(parameters.ToArray());
                        }

                        if (control == null)
                            continue;

                        if (control is Form && !string.IsNullOrEmpty(control.Name))
                        {
                            if (!translateCategories.Items.Contains(control.Name))
                                translateCategories.Items.Add(control.Name);

                            AddTranslationItem(control.Name, "$this", "Text", ((Form)control).Text);
                        }

                        foreach (FieldInfo fieldInfo in control.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
                        {
                            Component component = fieldInfo.GetValue(control) as Component;

                            if (component != null)
                            {
                                foreach (PropertyInfo propertyInfo in fieldInfo.FieldType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                                {
                                    if (propertyInfo.PropertyType == typeof(string) && ShouldBeTranslated(propertyInfo))
                                    {
                                        string value = (string)propertyInfo.GetValue(component, null);

                                        //Only translate properties that have a neutral value
                                        if (!string.IsNullOrEmpty(value))
                                        {
                                            AddTranslationItem(control.Name, fieldInfo.Name, propertyInfo.Name, value);
                                        }
                                    }

                                    /*
                                    var t = propertyInfo.GetCustomAttributes(true);
                                    if (t.Length > 0)
                                    {

                                    }
                                    */
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool ShouldBeTranslated(PropertyInfo propertyInfo)
        {
            if (propertyInfo.Name.Equals("Caption", StringComparison.CurrentCultureIgnoreCase))
                return true;
            if (propertyInfo.Name.Equals("Text", StringComparison.CurrentCultureIgnoreCase))
                return true;
            if (propertyInfo.Name.Equals("ToolTipText", StringComparison.CurrentCultureIgnoreCase))
                return true;

            return false;
        }

        private void AddTranslationItem(string category, string item, string property, string neutralValue)
        {
            if (!neutralTranslation.HasTranslationCategory(category))
                neutralTranslation.AddTranslationCategory(new TranslationCategory(category));


            neutralTranslation.GetTranslationCategory(category).AddTranslationItem(new TranslationItem(item, property, neutralValue));
        }

        private void translateCategories_SelectedIndexChanged(object sender, EventArgs e)
        {
            FillTranslateGrid(translateCategories.SelectedItem.ToString());
        }

        private void saveAs_Click(object sender, EventArgs e)
        {
            Translation foreignTranslation = new Translation();
            foreach (TranslateItem translateItem in translate)
            {
                //Item is not translated (yet), skip it
                if (string.IsNullOrEmpty(translateItem.TranslatedValue))
                    continue;

                if (!foreignTranslation.HasTranslationCategory(translateItem.Category))
                    foreignTranslation.AddTranslationCategory(new TranslationCategory(translateItem.Category));

                foreignTranslation.GetTranslationCategory(translateItem.Category).AddTranslationItem(new TranslationItem(translateItem.Name, translateItem.Property, translateItem.TranslatedValue));
            }

            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.Title = saveAsText.Text;

            if (fileDialog.ShowDialog() == DialogResult.OK)
                TranslationSerializer.Serialize(foreignTranslation, fileDialog.FileName);
        }

        private void translations_SelectedIndexChanged(object sender, EventArgs e)
        {
            translator = new Translator((string)translations.SelectedItem);
            LoadTranslation();
            FillTranslateGrid(allText.Text);
        }

        private void hideTranslatedItems_CheckedChanged(object sender, EventArgs e)
        {
            FillTranslateGrid(allText.Text);
        }
    }
}
