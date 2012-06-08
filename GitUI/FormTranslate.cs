using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using ResourceManager;
using ResourceManager.Translation;

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
        readonly TranslationString saveAsTextFilter = new TranslationString("Translation file (*.xml)");
        readonly TranslationString selectLanguageCode = new TranslationString("Select a language code first.");
        readonly TranslationString noLanguageCodeSelected = new TranslationString("There is no languagecode selected." + 
            Environment.NewLine + "Do you want to select a language code first?");
        readonly TranslationString noLanguageCodeSelectedCaption = new TranslationString("Language code");
        readonly TranslationString editingCellPrefixText = new TranslationString("[EDITING]");

        [DebuggerDisplay("{Category} - {NeutralValue}")]
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
        Translation translation;
        private TranslationCategory allCategories = new TranslationCategory();

        private bool changesMade;

        public FormTranslate()
        {
            InitializeComponent(); Translate();

            translations.Items.Clear();
            translations.Sorted = true;
            translations.Items.AddRange(Translator.GetAllTranslations());

            GetPropertiesToTranslate();
            LoadTranslation();
            translateCategories.SelectedItem = allCategories;
            FillTranslateGrid(allCategories);

            foreach (CultureInfo cultureInfo in CultureInfo.GetCultures(CultureTypes.AllCultures))
            {
                if (!_NO_TRANSLATE_languageCode.Items.Contains(cultureInfo.TwoLetterISOLanguageName))
                {
                    _NO_TRANSLATE_languageCode.Items.Add(string.Concat(cultureInfo.TwoLetterISOLanguageName, " (", cultureInfo.DisplayName, ")"));
                }
            }

            FormClosing += FormTranslate_FormClosing;
        }

        private void FormTranslate_Load(object sender, EventArgs e)
        {
            RestorePosition("translate");
        }

        void FormTranslate_FormClosing(object sender, FormClosingEventArgs e)
        {
            AskForSave();
            SavePosition("translate");
        }

        private void UpdateProgress()
        {
            int translatedCount = translate.Count(translateItem => !string.IsNullOrEmpty(translateItem.TranslatedValue));
            var progresMsg = string.Format(translateProgressText.Text, translatedCount.ToString(), translate.Count.ToString());
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

                    if (translation != null)
                        translateItem.TranslatedValue = translation.TranslateItem(translationCategory.Name, translateItem.Name, translateItem.Property, string.Empty);

                    translate.Add(translateItem);
                }
            }

            UpdateProgress();
        }

        private void FillTranslateGrid(TranslationCategory filter)
        {
            if (translate == null)
                return;

            translateItemBindingSource.DataSource = null;


            var filterTranslate = translate.Where(translateItem => filter == null || filter == allCategories || filter.Name.Equals(translateItem.Category)).Where(translateItem => !hideTranslatedItems.Checked || string.IsNullOrEmpty(translateItem.TranslatedValue)).ToList();

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

        private List<Type> GetTranslatableTypes()
        {
            List<Type> translatableTypes = new List<Type>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (IsAssemblyTranslatable(assembly))
                {
                    foreach (Type type in assembly.GetTypes())
                    {
                        if (type.IsClass && typeof(ITranslate).IsAssignableFrom(type))
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
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            object obj = null;
            if (type == GetType())
                obj = this;
            else
            {
                // try to find parameter less constructor first
                foreach (ConstructorInfo constructor in type.GetConstructors(flags))
                {
                    if (constructor.GetParameters().Length == 0)
                        obj = Activator.CreateInstance(type, true);
                }
            }
            if (obj == null && type.GetConstructors().Length > 0)
            {
                ConstructorInfo parameterConstructor = type.GetConstructors(flags)[0];
                var parameters = new List<object>(parameterConstructor.GetParameters().Length);
                for (int i = 0; i < parameterConstructor.GetParameters().Length; i++)
                    parameters.Add(null);
                obj = parameterConstructor.Invoke(parameters.ToArray());
            }

            Debug.Assert(obj != null);
            return obj;
        }


        public void GetPropertiesToTranslate()
        {
            translateCategories.Items.Clear();
            allCategories.Name = allText.Text;
            translateCategories.Items.Add(allCategories);

            string currentTranslation = GitCommands.Settings.Translation;

            try
            {
                //Set language to neutral to get neutral translations
                GitCommands.Settings.Translation = "";

                List<Type> translatableTypes = GetTranslatableTypes();

                foreach (Type type in translatableTypes)
                {
                    ITranslate obj = CreateInstanceOfClass(type) as ITranslate;
                    if (obj != null)
                        obj.AddTranslationItems(neutralTranslation);                                                                  
                }
            }
            finally
            {
                neutralTranslation.Sort();
                translateCategories.Items.AddRange(neutralTranslation.GetTranslationCategories().ToArray());
                //Restore translation
                GitCommands.Settings.Translation = currentTranslation;
            }
        }


        private void translateCategories_SelectedIndexChanged(object sender, EventArgs e)
        {
            FillTranslateGrid(translateCategories.SelectedItem as TranslationCategory);
        }

        private void saveAs_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_NO_TRANSLATE_languageCode.Text))
                if (MessageBox.Show(this, noLanguageCodeSelected.Text, noLanguageCodeSelectedCaption.Text, MessageBoxButtons.YesNo) == DialogResult.Yes)
                    return;

            toolStrip1.Select();

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
            if (foreignTranslation.LanguageCode != null)
            {
                foreach (TranslateItem translateItem in translate)
                {
                    //Item is not translated (yet), skip it
                    if (string.IsNullOrEmpty(translateItem.TranslatedValue))
                        continue;

                    TranslationItem ti = new TranslationItem(translateItem.Name, translateItem.Property, translateItem.TranslatedValue);
                    foreignTranslation.FindOrAddTranslationCategory(translateItem.Category).AddTranslationItem(ti);
                }
            }
            else
            {
                // English language
                foreach (TranslateItem translateItem in translate)
                {
                    TranslationItem ti = new TranslationItem(translateItem.Name, translateItem.Property, translateItem.NeutralValue);
                    foreignTranslation.FindOrAddTranslationCategory(translateItem.Category).AddTranslationItem(ti);
                }
            }
            
            var fileDialog =
                new SaveFileDialog
                    {
                        Title = saveAsText.Text,
                        FileName = translations.Text + ".xml",
                        Filter = saveAsTextFilter.Text + "|*.xml", 
                        DefaultExt = ".xml",
                        AddExtension = true
                    };

            if (fileDialog.ShowDialog(this) == DialogResult.OK)
            {
                TranslationSerializer.Serialize(foreignTranslation, fileDialog.FileName);
                changesMade = false;
            }
        }

        private void translations_SelectedIndexChanged(object sender, EventArgs e)
        {
            AskForSave();
            changesMade = false;

            translation = Translator.GetTranslation(translations.Text);
            LoadTranslation();
            FillTranslateGrid(translateCategories.SelectedItem as TranslationCategory);

            if (translation == null)
            {
                _NO_TRANSLATE_languageCode.Text = "";
                return;
            }

            try
            {
                var culture = new CultureInfo(translation.LanguageCode);
                _NO_TRANSLATE_languageCode.Text = string.Concat(culture.TwoLetterISOLanguageName, " (", culture.DisplayName, ")");
            }
            catch
            {
                _NO_TRANSLATE_languageCode.Text = translation.LanguageCode;
            }
        }

        private void hideTranslatedItems_CheckedChanged(object sender, EventArgs e)
        {
            FillTranslateGrid(translateCategories.SelectedItem as TranslationCategory);
        }

        private void AskForSave()
        {
            if (changesMade)
            {
                if (MessageBox.Show(this, saveCurrentChangesText.Text, saveCurrentChangesCaption.Text, MessageBoxButtons.YesNo) == DialogResult.Yes)
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
                changesMade = true;

                UpdateProgress();
            }
        }

        TranslateItem translateItemInEditing;

        private void translatedText_Enter(object sender, System.EventArgs e)
        {
            if (translateItemInEditing != null)
            {
                translateItemInEditing.TranslatedValue = translatedText.Text;
            }

            translateItemInEditing = (TranslateItem)translateItemBindingSource.Current;

            if (translateItemInEditing != null)
            {
                translateItemInEditing.TranslatedValue = editingCellPrefixText.Text + " " + translateItemInEditing.TranslatedValue;
            }
        }

        private void translatedText_Leave(object sender, System.EventArgs e)
        {
            //Debug.Assert(translateItemInEditing != null);

            if (translateItemInEditing != null)
            {
                translateItemInEditing.TranslatedValue = translatedText.Text;

                translateItemInEditing = null;
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
            bool nowTranslatedTextInEditing = translateItemInEditing != null;

            translatedText_Leave(null, null);

            translateGrid_Click(null, null);

            if (nowTranslatedTextInEditing)
            {
                translatedText_Enter(null, null);
            }
        }

        private void nextButton_Click(object sender, EventArgs e)
        {
            if (translateGrid.SelectedRows.Count == 1)
            {
                if (translateGrid.CurrentCell.RowIndex < translateGrid.Rows.Count - 1)
                    translateItemBindingSource.MoveNext();
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
                if (translateGrid.CurrentCell.RowIndex > 0)
                    translateItemBindingSource.MovePrevious();
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
                MessageBox.Show(this, selectLanguageCode.Text);
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
                MessageBox.Show(this, selectLanguageCode.Text);
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

        private void translatedText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Alt && e.KeyCode == Keys.Up)
            {
                e.Handled = true;
                previousButton_Click(sender, e);
            }
            else if (e.Alt && e.KeyCode == Keys.Down ||
                e.Control && e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                nextButton_Click(sender, e);
            }
            else if (e.Control && e.KeyCode == Keys.Down)
            {
                e.Handled = true;
                translatedText.SelectionStart = 0;
                translatedText.SelectionLength = translatedText.TextLength;
                translatedText.SelectedText = neutralTekst.Text;
            }
        }
    }
}
