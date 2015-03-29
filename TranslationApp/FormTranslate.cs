using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using GitUI;
using ResourceManager;
using ResourceManager.Xliff;
using TranslationUtl = ResourceManager.Xliff.TranslationUtl;

namespace TranslationApp
{
    public partial class FormTranslate : GitExtensionsForm
    {
        //TranslationStrings
        readonly TranslationString translateProgressText = new TranslationString("Translated {0} out of {1}");
        readonly TranslationString allText = new TranslationString("All");
        readonly TranslationString saveCurrentChangesText = new TranslationString("Do you want to save the current changes?");
        readonly TranslationString saveCurrentChangesCaption = new TranslationString("Save changes");
        readonly TranslationString saveAsText = new TranslationString("Save as");
        readonly TranslationString saveAsTextFilter = new TranslationString("Translation file (*.xlf)");
        readonly TranslationString noLanguageCodeSelected = new TranslationString("There is no language code selected." +
            Environment.NewLine + "Do you want to select a language code first?");
        readonly TranslationString noLanguageCodeSelectedCaption = new TranslationString("Language code");
        readonly TranslationString editingCellPrefixText = new TranslationString("[EDITING]");

        private IDictionary<string, List<TranslationItemWithCategory>> translationItems;

        readonly IDictionary<string, TranslationFile> _neutralTranslation = new Dictionary<string, TranslationFile>();
        IDictionary<string, TranslationFile> _translation = new Dictionary<string, TranslationFile>();
        private readonly TranslationCategory _allCategories = new TranslationCategory();

        private bool _changesMade;

        public FormTranslate()
            : base(true)
        {
            InitializeComponent(); Translate();
        }

        private void FormTranslate_Load(object sender, EventArgs e)
        {
            translations.Items.Clear();
            translations.Sorted = true;
            translations.Items.AddRange(Translator.GetAllTranslations());

            FillNeutralTranslation();
            _allCategories.Name = allText.Text;
            UpdateCategoriesList();
            translations.SelectedItem = GitCommands.AppSettings.Translation; // should be called after FillNeutralTranslation()
            if (_translation == null)
                LoadTranslation();
            translateCategories.SelectedItem = _allCategories;
            FillTranslateGrid(_allCategories);

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
            int translatedCount = translationItems.Sum(p => p.Value.Count(translateItem => !string.IsNullOrEmpty(translateItem.TranslatedValue)));
            int totalCount = translationItems.Count();
            var progresMsg = string.Format(translateProgressText.Text, translatedCount, totalCount);
            if (translateProgress.Text != progresMsg)
            {
                translateProgress.Text = progresMsg;
                toolStrip1.Refresh();
            }
        }

        private void LoadTranslation()
        {
            if (_translation.Count == 0)
            {
                var neutralItems = TranslationHelpers.GetItemsDictionary(_translation);
                translationItems = TranslationHelpers.LoadTranslation(_translation, neutralItems);
            }
            else
            {
                var neutralItems = new Dictionary<string, List<TranslationItemWithCategory>>();
                foreach (var pair in _neutralTranslation)
                {
                    var list = from item in pair.Value.TranslationCategories
                               from translationItem in item.Body.TranslationItems
                               select new TranslationItemWithCategory(item.Name, translationItem.Clone());
                    neutralItems.Add(pair.Key, list.ToList());
                }
                translationItems = neutralItems;
            }

            UpdateProgress();
        }

        private void FillTranslateGrid(TranslationCategory filter)
        {
            if (translationItems == null)
                return;

            translateItemBindingSource.DataSource = null;

            if (filter == _allCategories)
                filter = null;

            translateItemBindingSource.DataSource = GetCategoryItems(filter).ToList();

            UpdateProgress();
        }

        private IEnumerable<TranslationItemWithCategory> GetCategoryItems(TranslationCategory filter)
        {
            var filteredByCategory = translationItems.SelectMany(p => p.Value).Where(
                translateItem => filter == null || filter.Name.Equals(translateItem.Category));
            var filteredItems = filteredByCategory.Where(
                translateItem => !hideTranslatedItems.Checked);
            return filteredItems;
        }

        private IEnumerable<TranslationCategory> GetCategories(IDictionary<string, TranslationFile> translation)
        {
            return translation.SelectMany(pair => pair.Value.TranslationCategories);
        }


        public void UpdateCategoriesList()
        {
            var tc = translateCategories.SelectedItem as TranslationCategory;
            translateCategories.Items.Clear();
            translateCategories.Items.Add(_allCategories);

            if (!hideTranslatedItems.Checked)
                translateCategories.Items.AddRange(GetCategories(_neutralTranslation).ToArray());
            else
            {
                var categories = GetCategories(_neutralTranslation).Where(cat => GetCategoryItems(cat).Any());
                translateCategories.Items.AddRange(categories.ToArray());
            }
            if (hideTranslatedItems.Checked && !GetCategoryItems(tc).Any())
                tc = _allCategories;
            translateCategories.SelectedItem = tc;
        }

        private void FillNeutralTranslation()
        {
            try
            {
                //Set language to neutral to get neutral translations
                GitCommands.AppSettings.CurrentTranslation = "";

                var translatableTypes = TranslationUtl.GetTranslatableTypes();
                progressBar.Maximum = translatableTypes.Sum(types => types.Value.Count);
                progressBar.Visible = true;

                int index = 0;
                foreach (var types in translatableTypes)
                {
                    var translation = new TranslationFile();
                    try
                    {
                        foreach (Type type in types.Value)
                        {
                            ITranslate obj = TranslationUtl.CreateInstanceOfClass(type) as ITranslate;
                            if (obj != null)
                                obj.AddTranslationItems(translation);

                            progressBar.Value = index;
                            index++;
                            if (index % 10 == 0)
                                Update();
                        }
                    }
                    finally
                    {
                        translation.Sort();
                        _neutralTranslation[types.Key] = translation;
                    }
                }
            }
            finally
            {
                //Restore translation
                GitCommands.AppSettings.CurrentTranslation = null;
                progressBar.Visible = false;
            }
        }


        private void translateCategories_SelectedIndexChanged(object sender, EventArgs e)
        {
            categoryDataGridViewTextBoxColumn.Visible = (translateCategories.SelectedItem == _allCategories);
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

        #region Move translations
        /*
        //methods used to move translations from FormSettings to new settings pages
        private void MoveTranslations()
        {
            string[] translationsNames = Translator.GetAllTranslations();
            foreach (string translationName in translationsNames)
            {
                translation = Translator.GetTranslation(translationName);
                if (translation == null)
                    continue;

                LoadTranslation();

                MoveTranslationItems("FormSettings", "AppearanceSettingsPage");
                MoveTranslationItems("FormSettings", "ChecklistSettingsPage");
                MoveTranslationItems("FormSettings", "ColorsSettingsPage");
                MoveTranslationItems("FormSettings", "GitExtensionsSettingsPage");
                MoveTranslationItems("FormSettings", "GitSettingsPage");
                MoveTranslationItems("FormSettings", "GlobalSettingsSettingsPage");
                MoveTranslationItems("FormSettings", "HotkeysSettingsPage");
                MoveTranslationItems("FormSettings", "LocalSettingsSettingsPage");
                MoveTranslationItems("FormSettings", "ScriptsSettingsPage");
                MoveTranslationItems("FormSettings", "ShellExtensionSettingsPage");
                MoveTranslationItems("FormSettings", "SshSettingsPage");
                MoveTranslationItems("FormSettings", "StartPageSettingsPage");
                MoveTranslationItems("FormSettings", "CommonLogic");
                MoveTranslationItems("FormSettings", "CheckSettingsLogic");

                SaveAs();
            }
        }

        private void MoveTranslationItems(string fromCategoryName, string toCategoryName)
        {

                TranslationCategory fromCategory = translation.GetTranslationCategory(fromCategoryName);

                if (fromCategory == null)
                    return;

                Dictionary<string, TranslationItem> exactMatch = new Dictionary<string, TranslationItem>();
                Dictionary<string, TranslationItem> byNeutralValueMatch = new Dictionary<string, TranslationItem>();
                Dictionary<string, TranslationItem> ambiguous = new Dictionary<string, TranslationItem>();

                foreach (var item in fromCategory.TranslationItems)
                {
                    if (!item.Value.IsNullOrEmpty())
                    {
                        string neutralValue = item.Source;
                        if (neutralValue != null)
                        {
                            string neutralValueKey = neutralValue + ":" + item.Property;
                            TranslationItem foo;
                            if (byNeutralValueMatch.TryGetValue(neutralValueKey, out foo))
                                ambiguous[neutralValueKey] = item;
                            else
                                byNeutralValueMatch.Add(neutralValueKey, item);

                            var exactKey = item.Name + ":" + neutralValueKey;
                            exactMatch.Add(exactKey, item);
                        }
                    }
                }

                foreach (var key in ambiguous.Keys)
                    byNeutralValueMatch.Remove(key);

                foreach (var item in translate.Where(itm => itm.Category.Equals(toCategoryName)))
                {
                    if (item.Status != TranslationType.Translated)
                    {
                        string neutralValueKey = item.NeutralValue + ":" + item.Property;
                        string exactKey = item.Name + ":" + neutralValueKey;
                        TranslationItem fromItem;
                        if (exactMatch.TryGetValue(exactKey, out fromItem))
                        {
                            item.TranslatedValue = fromItem.Value;
                            item.Status = fromItem.Status;
                        }
                        else
                        {
                            if (byNeutralValueMatch.TryGetValue(neutralValueKey, out fromItem))
                            {
                                item.TranslatedValue = fromItem.Value;
                                item.Status = fromItem.Status;
                            }
                        }
                    }
                }
        }

        */
        #endregion

        private string GetSelectedLanguageCode()
        {
            if (string.IsNullOrEmpty(_NO_TRANSLATE_languageCode.Text) || _NO_TRANSLATE_languageCode.Text.Length < 2)
                return null;

            return _NO_TRANSLATE_languageCode.Text.Substring(0, 2);
        }

        private void SaveAs()
        {
            using (var fileDialog =
                new SaveFileDialog
                    {
                        Title = saveAsText.Text,
                        FileName = translations.Text + ".xlf",
                        Filter = saveAsTextFilter.Text + "|*.xlf",
                        DefaultExt = ".xlf",
                        AddExtension = true
                    })
            {
                if (fileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    TranslationHelpers.SaveTranslation(GetSelectedLanguageCode(), translationItems, fileDialog.FileName);
                    _changesMade = false;
                }
            }
        }

        private void translations_SelectedIndexChanged(object sender, EventArgs e)
        {
            AskForSave();
            _changesMade = false;

            _translation = Translator.GetTranslation(translations.Text);
            LoadTranslation();
            UpdateCategoriesList();
            FillTranslateGrid(translateCategories.SelectedItem as TranslationCategory);

            if (_translation == null)
            {
                _NO_TRANSLATE_languageCode.Text = "";
                return;
            }

            try
            {
                var culture = new CultureInfo(_translation.First().Value.LanguageCode);
                _NO_TRANSLATE_languageCode.Text = string.Concat(culture.TwoLetterISOLanguageName, " (", culture.DisplayName, ")");
            }
            catch
            {
                _NO_TRANSLATE_languageCode.Text = _translation.First().Value.LanguageCode;
            }
        }

        private void hideTranslatedItems_CheckedChanged(object sender, EventArgs e)
        {
            UpdateCategoriesList();
            FillTranslateGrid(translateCategories.SelectedItem as TranslationCategory);
        }

        private void AskForSave()
        {
            if (_changesMade)
            {
                if (MessageBox.Show(this, saveCurrentChangesText.Text, saveCurrentChangesCaption.Text, MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    SaveAs();
                }
            }
        }

        private void translateGrid_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            _changesMade = true;

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
                _changesMade = true;

                UpdateProgress();
            }
        }

        TranslationItemWithCategory _translationItemWithCategoryInEditing;

        private void translatedText_Enter(object sender, System.EventArgs e)
        {
            if (_translationItemWithCategoryInEditing != null)
            {
                _translationItemWithCategoryInEditing.TranslatedValue = translatedText.Text;
            }

            _translationItemWithCategoryInEditing = (TranslationItemWithCategory)translateItemBindingSource.Current;

            if (_translationItemWithCategoryInEditing != null)
            {
                _translationItemWithCategoryInEditing.TranslatedValue = editingCellPrefixText.Text + " " + _translationItemWithCategoryInEditing.TranslatedValue;
            }
        }

        private void translatedText_Leave(object sender, System.EventArgs e)
        {
            //Debug.Assert(_translationItemWithCategoryInEditing != null);

            if (_translationItemWithCategoryInEditing != null)
            {
                _translationItemWithCategoryInEditing.TranslatedValue = translatedText.Text;

                _translationItemWithCategoryInEditing = null;
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

                var translateItem = (TranslationItemWithCategory)translateGrid.SelectedRows[0].DataBoundItem;

                if (translateItem == null) return;

                neutralText.Text = translateItem.NeutralValue;
                translatedText.Text = translateItem.TranslatedValue;
            }
            else
            {
                splitContainer2.Panel2Collapsed = true;
            }
        }

        private void translateGrid_SelectionChanged(object sender, EventArgs e)
        {
            bool nowTranslatedTextInEditing = _translationItemWithCategoryInEditing != null;

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
                translatedText.SelectAll();
                translatedText.SelectedText = neutralText.Text;
            }
        }
    }
}
