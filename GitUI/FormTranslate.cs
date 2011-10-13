﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using System.Reflection;
using ResourceManager.Translation;
using ResourceManager;
using System.Globalization;

namespace GitUI
{
    public partial class FormTranslate : GitExtensionsForm
    {
        //TranslationStrings
        readonly TranslationString translateProgressText = new TranslationString("Translated {0} out of {1}");
        readonly TranslationString allText = new TranslationString("All");
        readonly TranslationString saveCurrentChangesText = new TranslationString("Do you want to save the current changes?");
        readonly TranslationString saveCurrentChangesCaption = new TranslationString("Save changes");
        readonly TranslationString saveAsText = new TranslationString("Save as");
        readonly TranslationString selectLanguageCode = new TranslationString("Select a language code first.");
        readonly TranslationString noLanguageCodeSelected = new TranslationString("There is no languagecode selected." + 
            Environment.NewLine + "Do you want to select a language code first?");
        readonly TranslationString noLanguageCodeSelectedCaption = new TranslationString("Language code");

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

        readonly Translation neutralTranslation = new Translation();
        Translator translator;

        private bool changesMade;

        public FormTranslate()
        {
            InitializeComponent(); Translate();

            translations.Items.Clear();
            translations.Sorted = true;
            translations.Items.AddRange(Translator.GetAllTranslations());

            GetPropertiesToTranslate();
            LoadTranslation();
            translateCategories.SelectedItem = allText.Text;
            FillTranslateGrid(translateCategories.SelectedItem.ToString());

            foreach (CultureInfo cultureInfo in CultureInfo.GetCultures(CultureTypes.AllCultures))
            {
                if (!_NO_TRANSLATE_languageCode.Items.Contains(cultureInfo.TwoLetterISOLanguageName))
                {
                    _NO_TRANSLATE_languageCode.Items.Add(string.Concat(cultureInfo.TwoLetterISOLanguageName, " (", cultureInfo.DisplayName, ")"));
                }
            }

            FormClosing += FormTranslate_FormClosing;
        }

        void FormTranslate_FormClosing(object sender, FormClosingEventArgs e)
        {
            AskForSave();
        }

        private void UpdateProgress()
        {
            int translatedCount = translate.Count(translateItem => !string.IsNullOrEmpty(translateItem.TranslatedValue));
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
                    var translateItem = new TranslateItem
                                            {
                                                Category = translationCategory.Name,
                                                Name = translationItem.Name,
                                                Property = translationItem.Property,
                                                NeutralValue = translationItem.Value
                                            };

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

            translateItemBindingSource.DataSource = null;


            var filterTranslate = translate.Where(translateItem => string.IsNullOrEmpty(filter) || filter.Equals(allText.Text) || filter.Equals(translateItem.Category)).Where(translateItem => !hideTranslatedItems.Checked || string.IsNullOrEmpty(translateItem.TranslatedValue)).ToList();

            translateItemBindingSource.DataSource = filterTranslate;

            UpdateProgress();
        }

        private bool IsAssemblyTranslatable(Assembly assembly)
        {
            if ((assembly.FullName.StartsWith("mscorlib", StringComparison.OrdinalIgnoreCase)) ||
                (assembly.FullName.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase)) ||
                (assembly.FullName.StartsWith("Presentation", StringComparison.OrdinalIgnoreCase)) ||
                (assembly.FullName.StartsWith("WindowsBase", StringComparison.OrdinalIgnoreCase)) ||
                (assembly.FullName.StartsWith("ICSharpCode", StringComparison.OrdinalIgnoreCase)) ||
                (assembly.FullName.StartsWith("access", StringComparison.OrdinalIgnoreCase)) ||
                (assembly.FullName.StartsWith("SMDiag", StringComparison.OrdinalIgnoreCase)) ||
                (assembly.FullName.StartsWith("System", StringComparison.OrdinalIgnoreCase)) ||
                (assembly.FullName.StartsWith("vshost", StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }
            return true;
        }

        private bool IsTranslatableType(Type type)
        {
            if ((typeof(GitExtensionsControl).IsAssignableFrom(type)) ||
                (typeof(GitExtensionsForm).IsAssignableFrom(type)) ||
                (typeof(ITranslate).IsAssignableFrom(type)))
            {
                return true;
            }
            return false;
        }

        private List<Type> GetTranslatableTypes()
        {
            List<Type> translatableTypes = new List<Type>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (IsAssemblyTranslatable(assembly))
                {
                    foreach (Type type in assembly.GetTypes())
                    {
                        if (IsTranslatableType(type))
                        {
                            translatableTypes.Add(type);
                        }
                    }
                }
            }
            return translatableTypes;
        }

        private object CreateInstanceOfClass(Type type)
        {
            object obj = null;
            if (type == GetType())
                obj = this;
            else
            {
                // try to find parameter less constructor first
                foreach (ConstructorInfo constructor in type.GetConstructors())
                {
                    if (constructor.GetParameters().Length == 0)
                        obj = Activator.CreateInstance(type);
                }
            }
            if (obj == null && type.GetConstructors().Length > 0)
            {
                ConstructorInfo parameterConstructor = type.GetConstructors()[0];
                var parameters = new List<object>(parameterConstructor.GetParameters().Length);
                for (int i = 0; i < parameterConstructor.GetParameters().Length; i++)
                    parameters.Add(null);
                obj = parameterConstructor.Invoke(parameters.ToArray());
            }

            return obj;
        }

        private void GetTranslatableFromComponent(string className, object obj, FieldInfo fieldInfo)
        {
            if (!(fieldInfo.GetValue(obj) is Component))
            {
                return;
            }
            Component component = fieldInfo.GetValue(obj) as Component;
            foreach (PropertyInfo propertyInfo in fieldInfo.FieldType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic))
            {
                if (IsTranslatableItemInComponent(propertyInfo))
                {
                    var value = (string)propertyInfo.GetValue(component, null);
                    if (!string.IsNullOrEmpty(value))
                    {
                        AddTranslationItem(className, fieldInfo.Name, propertyInfo.Name, value);
                    }
                }
            }
        }

        private void GetTranslatableFromDataGridViewElement(string className, object obj, FieldInfo fieldInfo)
        {
            if (!(fieldInfo.GetValue(obj) is DataGridViewElement))
            {
                return;
            }
            DataGridViewColumn dataGridViewColumn = fieldInfo.GetValue(obj) as DataGridViewColumn;
            foreach (PropertyInfo propertyInfo in fieldInfo.FieldType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic))
            {
                if (IsTranslatableItemInDataGridViewElement(propertyInfo, fieldInfo, obj))
                {
                    var value = (string)propertyInfo.GetValue(dataGridViewColumn, null);
                    if (!string.IsNullOrEmpty(value))
                    {
                        AddTranslationItem(className, fieldInfo.Name, propertyInfo.Name, value);
                    }
                }
            }
        }

        public void GetPropertiesToTranslate()
        {
            translateCategories.Items.Clear();
            translateCategories.Items.Add(allText.Text);

            string currentTranslation = GitCommands.Settings.Translation;

            try
            {
                //Set language to neutral to get neutral translations
                GitCommands.Settings.Translation = "";

                List<Type> translatableTypes = GetTranslatableTypes();

                foreach (Type type in translatableTypes)
                {
                    string className = type.Name;
                    if (!translateCategories.Items.Contains(className))
                        translateCategories.Items.Add(className);

                    object obj = CreateInstanceOfClass(type);
                    if (obj == null)
                    {
                        continue;
                    }

                    if (obj is Form)
                    {
                        if (!string.IsNullOrEmpty(((Form)obj).Text))
                        {
                            AddTranslationItem(className, "$this", "Text", ((Form)obj).Text);
                        }
                    }

                    foreach (FieldInfo fieldInfo in obj.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                    {
                        //Skip controls with a name started with "_NO_TRANSLATE_"
                        //this is a naming convention, these are not translated
                        if (fieldInfo.Name.StartsWith("_NO_TRANSLATE_"))
                            continue;

                        GetTranslatableFromComponent(className, obj, fieldInfo);
                        GetTranslatableFromDataGridViewElement(className, obj, fieldInfo);
                    }
                }
            }
            finally
            {
                //Restore translation
                GitCommands.Settings.Translation = currentTranslation;
            }
        }

        private bool IsTranslatableItemInComponent(PropertyInfo propertyInfo)
        {
            if (propertyInfo.PropertyType != typeof(string))
                return false;
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

        private bool IsTranslatableItemInDataGridViewElement(PropertyInfo propertyInfo, FieldInfo fieldInfo, Object obj)
        {
            if (propertyInfo.Name.Equals("HeaderText", StringComparison.CurrentCultureIgnoreCase))
            {
                DataGridViewColumn viewCol = fieldInfo.GetValue(obj) as DataGridViewColumn;
                if (viewCol.Visible)
                    return true;
                else
                    return false;
            }
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
                if (MessageBox.Show(noLanguageCodeSelected.Text, noLanguageCodeSelectedCaption.Text, MessageBoxButtons.YesNo) == DialogResult.Yes)
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
            var foreignTranslation = new Translation { LanguageCode = GetSelectedLanguageCode() };
            foreach (TranslateItem translateItem in translate)
            {
                //Item is not translated (yet), skip it
                if (string.IsNullOrEmpty(translateItem.TranslatedValue))
                    continue;

                if (!foreignTranslation.HasTranslationCategory(translateItem.Category))
                    foreignTranslation.AddTranslationCategory(new TranslationCategory(translateItem.Category));

                foreignTranslation.GetTranslationCategory(translateItem.Category).AddTranslationItem(new TranslationItem(translateItem.Name, translateItem.Property, translateItem.TranslatedValue));
            }

            var fileDialog = new SaveFileDialog { Title = saveAsText.Text, FileName = translations.Text + ".xml" };

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

            translator = new Translator(translations.Text);
            LoadTranslation();
            FillTranslateGrid(translateCategories.SelectedItem.ToString());

            try
            {
                var culture = new CultureInfo(translator.LanguageCode);
                _NO_TRANSLATE_languageCode.Text = string.Concat(culture.TwoLetterISOLanguageName, " (", culture.DisplayName, ")");
            }
            catch
            {
                _NO_TRANSLATE_languageCode.Text = translator.LanguageCode;
            }
        }

        private void hideTranslatedItems_CheckedChanged(object sender, EventArgs e)
        {
            FillTranslateGrid(translateCategories.SelectedItem.ToString());
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
            splitContainer2.Panel2Collapsed = _toolStripButton1.Checked;
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

                var translateItem = (TranslateItem)translateGrid.SelectedRows[0].DataBoundItem;

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
                MessageBox.Show(selectLanguageCode.Text);
                return;
            }

            if (translateGrid.SelectedRows.Count == 1)
            {
                var translateItem = ((TranslateItem)translateGrid.SelectedRows[0].DataBoundItem);

                translateItem.TranslatedValue = Google.TranslateText(translateItem.NeutralValue, "en", GetSelectedLanguageCode());

                translateGrid_Click(null, null);
                translateGrid.Refresh();
            }
        }

        private void googleAll_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_NO_TRANSLATE_languageCode.Text))
            {
                MessageBox.Show(selectLanguageCode.Text);
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
