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
using System.Net;
using System.Globalization;

namespace GitUI
{
    public partial class FormTranslate : GitExtensionsForm
    {
        //TranslationStrings
        TranslationString translateProgressText = new TranslationString("Translated {0} out of {1}");
        TranslationString allText = new TranslationString("All");
        TranslationString saveCurrentChangesText = new TranslationString("Do you want to save the current changes?");
        TranslationString saveCurrentChangesCaption = new TranslationString("Save changes");
        TranslationString saveAsText = new TranslationString("Save as");

        public class TranslateItem : INotifyPropertyChanged
        {
            public string Category { get; set; }
            public string Name { get; set; }
            public string Property { get; set; }
            public string NeutralValue { get; set; }
            private string _translatedValue;
            public string TranslatedValue
            {
                get { return _translatedValue; }
                set
                {
                    var pc = PropertyChanged;
                    if (pc != null)
                    {
                        pc(this, new PropertyChangedEventArgs("TranslatedValue"));
                    }
                    _translatedValue = value;
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }

        private List<TranslateItem> translate;

        Translation neutralTranslation = new Translation();
        Translator translator;

        private bool changesMade = false;

        public FormTranslate()
        {
            InitializeComponent(); Translate();

            translations.Items.Clear();
            translations.Sorted = true;
            translations.Items.AddRange(Translator.GetAllTranslations());

            GetPropertiesToTranslate();
            LoadTranslation();
            FillTranslateGrid(allText.Text);

            foreach(CultureInfo cultureInfo in CultureInfo.GetCultures(CultureTypes.AllCultures))
            {
                if (!_NO_TRANSLATE_languageCode.Items.Contains(cultureInfo.TwoLetterISOLanguageName))
                {
                    _NO_TRANSLATE_languageCode.Items.Add(string.Concat(cultureInfo.TwoLetterISOLanguageName, " (", cultureInfo.DisplayName, ")"));
                }
            }

            FormClosing += new FormClosingEventHandler(FormTranslate_FormClosing);
        }

        void FormTranslate_FormClosing(object sender, FormClosingEventArgs e)
        {
            AskForSave();
        }

        private void UpdateProgress()
        {
            int translatedCount = 0;
            foreach (TranslateItem translateItem in translate)
            {
                if (!string.IsNullOrEmpty(translateItem.TranslatedValue))
                    translatedCount++;
            }
            var progresMsg = string.Format(translateProgressText.Text, translatedCount, translate.Count);
            if (translateProgress.Text != progresMsg)
            {
                translateProgress.Text = progresMsg;
                toolStrip1.Refresh();    
            }
            
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
            if (translate == null)
                return;

            List<TranslateItem> filterTranslate = new List<TranslateItem>();

            translateItemBindingSource.DataSource = null;

            foreach (TranslateItem translateItem in translate)
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

            translateItemBindingSource.DataSource = filterTranslate;

            UpdateProgress();
        }

        public void GetPropertiesToTranslate()
        {
            translateCategories.Items.Clear();
            translateCategories.Items.Add(allText.Text);

            string currentTranslation = GitCommands.Settings.Translation;

            try
            {
                //Set language to neutral to get neutral translations
                GitCommands.Settings.Translation = null;

                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        if (!assembly.FullName.StartsWith("ICSharpCode", StringComparison.OrdinalIgnoreCase))
                            foreach (Type type in assembly.GetTypes())
                            {
                                if (typeof(GitExtensionsControl).IsAssignableFrom(type) ||
                                    typeof(GitExtensionsForm).IsAssignableFrom(type) ||
                                    typeof(ITranslate).IsAssignableFrom(type))
                                {
                                    object control = null;


                                    if (type == this.GetType())
                                        control = this;
                                    else
                                        // try to find parameter less constructor first
                                        foreach (ConstructorInfo constructor in type.GetConstructors())
                                        {
                                            if (constructor.GetParameters().Length == 0)
                                                control = (object)Activator.CreateInstance(type);
                                        }

                                    if (control == null && type.GetConstructors().Length > 0)
                                    {
                                        ConstructorInfo parameterConstructor = type.GetConstructors()[0];
                                        List<object> parameters = new List<object>(parameterConstructor.GetParameters().Length);
                                        for (int i = 0; i < parameterConstructor.GetParameters().Length; i++)
                                            parameters.Add(null);
                                        control = (object)parameterConstructor.Invoke(parameters.ToArray());
                                    }

                                    if (control == null)
                                        continue;

                                    string name;

                                    if (control is Control)
                                        name = ((Control)control).Name;
                                    else
                                        name = control.GetType().Name;

                                    if (control is Form && !string.IsNullOrEmpty(name))
                                    {
                                        if (!translateCategories.Items.Contains(name))
                                            translateCategories.Items.Add(name);

                                        AddTranslationItem(name, "$this", "Text", ((Form)control).Text);
                                    }

                                    foreach (FieldInfo fieldInfo in control.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
                                    {
                                        //Skip controls with a name started with "_NO_TRANSLATE_"
                                        //this is a naming convention, these are not translated
                                        if (fieldInfo.Name.StartsWith("_NO_TRANSLATE_"))
                                            continue;

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
                                                        AddTranslationItem(name, fieldInfo.Name, propertyInfo.Name, value);
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
                    catch (Exception)
                    {
                    }
                }

            }
            finally
            {
                //Restore translation
                GitCommands.Settings.Translation = currentTranslation;
            }
        }

        private static bool ShouldBeTranslated(PropertyInfo propertyInfo)
        {
            if (propertyInfo.Name.Equals("Caption", StringComparison.CurrentCultureIgnoreCase))
                return true;
            if (propertyInfo.Name.Equals("Text", StringComparison.CurrentCultureIgnoreCase))
                return true;
            if (propertyInfo.Name.Equals("ToolTipText", StringComparison.CurrentCultureIgnoreCase))
                return true;
            if (propertyInfo.Name.Equals("Title", StringComparison.CurrentCultureIgnoreCase))
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
            if (string.IsNullOrEmpty(_NO_TRANSLATE_languageCode.Text))
                if (MessageBox.Show("There is no languagecode selected." + Environment.NewLine + "Do you want to select a language code first?", "Language code", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    return;

            SaveAs();
        }

        private string GetSelectedLanguageCode()
        {
            if (string.IsNullOrEmpty(_NO_TRANSLATE_languageCode.Text) || _NO_TRANSLATE_languageCode.Text.Length < 2)
                return null;

            return _NO_TRANSLATE_languageCode.Text.Substring(0, 2);
        }

        private void SaveAs()
        {
            Translation foreignTranslation = new Translation();
            foreignTranslation.LanguageCode = GetSelectedLanguageCode();
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
            fileDialog.FileName = translations.Text + ".xml";

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                TranslationSerializer.Serialize(foreignTranslation, fileDialog.FileName);
                changesMade = false;
            }
        }

        private void translations_SelectedIndexChanged(object sender, EventArgs e)
        {
            AskForSave();
            changesMade = false;

            translator = new Translator((string)translations.Text);
            LoadTranslation();
            FillTranslateGrid(allText.Text);

            try
            {
                CultureInfo culture = new CultureInfo(translator.LanguageCode);
                _NO_TRANSLATE_languageCode.Text = string.Concat(culture.TwoLetterISOLanguageName, " (", culture.DisplayName, ")");
            }
            catch
            {
                _NO_TRANSLATE_languageCode.Text = translator.LanguageCode;
            }
        }

        private void hideTranslatedItems_CheckedChanged(object sender, EventArgs e)
        {
            FillTranslateGrid(allText.Text);
        }

        private void AskForSave()
        {
            if (changesMade)
            {
                if (MessageBox.Show(saveCurrentChangesText.Text, saveCurrentChangesCaption.Text, MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    SaveAs();
                }
            }
        }

        private void translateGrid_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            changesMade = true;

            UpdateProgress();
        }

        private void toolStripButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (_toolStripButton1.Checked)
                splitContainer2.Panel2Collapsed = true;
            else
                splitContainer2.Panel2Collapsed = false;
        }

        private void translatedText_TextChanged(object sender, EventArgs e)
        {
            if (translateGrid.SelectedRows.Count == 1)
            {
                var translateItem = (TranslateItem)translateItemBindingSource.Current;
                translateItem.TranslatedValue = translatedText.Text;

                changesMade = true;

                UpdateProgress();
            }
        }

        private void translateGrid_Click(object sender, EventArgs e)
        {
            if (translateGrid.SelectedRows.Count == 1)
            {
                if (_toolStripButton1.Checked)
                {
                    splitContainer2.Panel2Collapsed = false;
                }

                TranslateItem translateItem = (TranslateItem)translateGrid.SelectedRows[0].DataBoundItem;

                if (translateItem == null) return;

                neutralTekst.Text = translateItem.NeutralValue;
                translatedText.Text = translateItem.TranslatedValue;
            }
            else
            {
                splitContainer2.Panel2Collapsed = true;
            }
        }

        private void translateGrid_SelectionChanged(object sender, EventArgs e)
        {
            translateGrid_Click(null, null);
        }

        private void nextButton_Click(object sender, EventArgs e)
        {
            if (translateGrid.SelectedRows.Count == 1)
            {
                if (translateGrid.SelectedRows[0].Index < translateGrid.Rows.Count - 1)
                {
                    int newIndex = translateGrid.SelectedRows[0].Index + 1;
                    translateGrid.SelectedRows[0].Selected = false;
                    translateGrid.Rows[newIndex].Selected = true;
                }
            }
            else
                if (translateGrid.Rows.Count > 0)
                {
                    translateGrid.Rows[0].Selected = true;
                }
        }

        private void previousButton_Click(object sender, EventArgs e)
        {
            if (translateGrid.SelectedRows.Count == 1)
            {
                if (translateGrid.SelectedRows[0].Index > 0)
                {
                    int newIndex = translateGrid.SelectedRows[0].Index - 1;
                    translateGrid.SelectedRows[0].Selected = false;
                    translateGrid.Rows[newIndex].Selected = true;
                }
            }
            else
                if (translateGrid.Rows.Count > 0)
                {
                    translateGrid.Rows[0].Selected = true;
                }
        }

        private void googleTranslate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_NO_TRANSLATE_languageCode.Text))
            {
                MessageBox.Show("Select a language code first.");
                return;
            }

            if (translateGrid.SelectedRows.Count == 1)
            {
                TranslateItem translateItem = ((TranslateItem)translateGrid.SelectedRows[0].DataBoundItem);

                translateItem.TranslatedValue = Google.TranslateText(translateItem.NeutralValue, "en", GetSelectedLanguageCode());
                
                translateGrid_Click(null, null);
                translateGrid.Refresh();
            }
        }

        private void googleAll_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_NO_TRANSLATE_languageCode.Text))
            {
                MessageBox.Show("Select a language code first.");
                return;
            }

            foreach (TranslateItem translateItem in translate)
            {
                if (string.IsNullOrEmpty(translateItem.TranslatedValue))
                    translateItem.TranslatedValue = Google.TranslateText(translateItem.NeutralValue, "en", GetSelectedLanguageCode());

                UpdateProgress();
                translateGrid.Refresh();
            }

            translateGrid_Click(null, null);
        }

        private void toolStripButtonNew_Click(object sender, EventArgs e)
        {
            translations.Text = "";
            translations_SelectedIndexChanged(null, null);
        }
    }
}
