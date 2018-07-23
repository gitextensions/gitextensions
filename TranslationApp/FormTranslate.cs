using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using GitUI;
using JetBrains.Annotations;
using ResourceManager;
using ResourceManager.Xliff;

namespace TranslationApp
{
    public partial class FormTranslate : GitExtensionsForm
    {
        // TranslationStrings
        private readonly TranslationString _translateProgressText = new TranslationString("Translated {0} out of {1}");
        private readonly TranslationString _allText = new TranslationString("All");
        private readonly TranslationString _saveCurrentChangesText = new TranslationString("Do you want to save the current changes?");
        private readonly TranslationString _saveCurrentChangesCaption = new TranslationString("Save changes");
        private readonly TranslationString _saveAsText = new TranslationString("Save as");
        private readonly TranslationString _saveAsTextFilter = new TranslationString("Translation file (*.xlf)");
        private readonly TranslationString _noLanguageCodeSelected = new TranslationString("There is no language code selected." +
            Environment.NewLine + "Do you want to select a language code first?");
        private readonly TranslationString _noLanguageCodeSelectedCaption = new TranslationString("Language code");
        private readonly TranslationString _editingCellPrefixText = new TranslationString("[EDITING]");

        private IDictionary<string, List<TranslationItemWithCategory>> _translationItems;

        private readonly IDictionary<string, TranslationFile> _neutralTranslation = new Dictionary<string, TranslationFile>();
        private IDictionary<string, TranslationFile> _translation = new Dictionary<string, TranslationFile>();
        private readonly TranslationCategory _allCategories = new TranslationCategory();

        private bool _changesMade;

        public FormTranslate()
            : base(true)
        {
            InitializeComponent();
            InitializeComplete();

            translateCategories.DisplayMember = nameof(TranslationCategory.Name);

            categoryDataGridViewTextBoxColumn.DataPropertyName = nameof(TranslationItemWithCategory.Category);
            nameDataGridViewTextBoxColumn.DataPropertyName = nameof(TranslationItemWithCategory.Name);
            propertyDataGridViewTextBoxColumn.DataPropertyName = nameof(TranslationItemWithCategory.Property);
            neutralValueDataGridViewTextBoxColumn.DataPropertyName = nameof(TranslationItemWithCategory.NeutralValue);
            translatedValueDataGridViewTextBoxColumn.DataPropertyName = nameof(TranslationItemWithCategory.TranslatedValue);
        }

        private void FormTranslate_Load(object sender, EventArgs e)
        {
            translations.Items.Clear();
            translations.Sorted = true;
            translations.Items.AddRange(Translator.GetAllTranslations());

            FillNeutralTranslation();
            _allCategories.Name = _allText.Text;
            UpdateCategoriesList();
            translations.SelectedItem = GitCommands.AppSettings.Translation; // should be called after FillNeutralTranslation()
            if (_translation == null)
            {
                LoadTranslation();
            }

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

        private void FormTranslate_FormClosing(object sender, FormClosingEventArgs e)
        {
            AskForSave();
        }

        private void UpdateProgress()
        {
            int translatedCount = _translationItems.Sum(p => p.Value.Count(translateItem => !string.IsNullOrEmpty(translateItem.TranslatedValue)));
            int totalCount = _translationItems.Count;
            var message = string.Format(_translateProgressText.Text, translatedCount, totalCount);
            if (translateProgress.Text != message)
            {
                translateProgress.Text = message;
                toolStrip1.Refresh();
            }
        }

        private void LoadTranslation()
        {
            if (_translation.Count != 0)
            {
                var items = TranslationHelpers.GetItemsDictionary(_translation);
                _translationItems = TranslationHelpers.LoadTranslation(_translation, items);
            }
            else
            {
                var neutralItems = new Dictionary<string, List<TranslationItemWithCategory>>();
                foreach (var (key, file) in _neutralTranslation)
                {
                    var list = from item in file.TranslationCategories
                               from translationItem in item.Body.TranslationItems
                               select new TranslationItemWithCategory(item.Name, translationItem.Clone());
                    neutralItems.Add(key, list.ToList());
                }

                _translationItems = neutralItems;
            }

            UpdateProgress();
        }

        private void FillTranslateGrid(TranslationCategory filter)
        {
            if (_translationItems == null)
            {
                return;
            }

            translateItemBindingSource.DataSource = null;

            if (filter == _allCategories)
            {
                filter = null;
            }

            translateItemBindingSource.DataSource = GetCategoryItems(filter).ToList();

            UpdateProgress();
        }

        private IEnumerable<TranslationItemWithCategory> GetCategoryItems(TranslationCategory filter)
        {
            var filteredByCategory = _translationItems.SelectMany(p => p.Value).Where(
                translateItem => filter == null || filter.Name == translateItem.Category);
            var filteredItems = filteredByCategory.Where(
                translateItem => !hideTranslatedItems.Checked);
            return filteredItems;
        }

        private static IEnumerable<TranslationCategory> GetCategories(IDictionary<string, TranslationFile> translation)
        {
            return translation.SelectMany(pair => pair.Value.TranslationCategories);
        }

        public void UpdateCategoriesList()
        {
            var tc = translateCategories.SelectedItem as TranslationCategory;
            translateCategories.Items.Clear();
            translateCategories.Items.Add(_allCategories);

            if (!hideTranslatedItems.Checked)
            {
                translateCategories.Items.AddRange(GetCategories(_neutralTranslation).ToArray());
            }
            else
            {
                var categories = GetCategories(_neutralTranslation).Where(cat => GetCategoryItems(cat).Any());
                translateCategories.Items.AddRange(categories.ToArray());
            }

            if (hideTranslatedItems.Checked && !GetCategoryItems(tc).Any())
            {
                tc = _allCategories;
            }

            translateCategories.SelectedItem = tc;
        }

        private void FillNeutralTranslation()
        {
            try
            {
                // Set language to neutral to get neutral translations
                GitCommands.AppSettings.CurrentTranslation = "";

                var translatableTypes = TranslationUtil.GetTranslatableTypes();
                progressBar.Maximum = translatableTypes.Sum(types => types.Value.Count);
                progressBar.Visible = true;

                int index = 0;
                foreach (var (key, types) in translatableTypes)
                {
                    var translation = new TranslationFile();
                    try
                    {
                        foreach (Type type in types)
                        {
                            if (TranslationUtil.CreateInstanceOfClass(type) is ITranslate obj)
                            {
                                obj.AddTranslationItems(translation);
                            }

                            progressBar.Value = index;
                            index++;
                            if (index % 10 == 0)
                            {
                                Update();
                            }
                        }
                    }
                    finally
                    {
                        translation.Sort();
                        _neutralTranslation[key] = translation;
                    }
                }
            }
            finally
            {
                // Restore translation
                GitCommands.AppSettings.CurrentTranslation = null;
                progressBar.Visible = false;
            }
        }

        private void translateCategories_SelectedIndexChanged(object sender, EventArgs e)
        {
            categoryDataGridViewTextBoxColumn.Visible = translateCategories.SelectedItem == _allCategories;
            FillTranslateGrid(translateCategories.SelectedItem as TranslationCategory);
        }

        private void saveAs_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_NO_TRANSLATE_languageCode.Text))
            {
                if (MessageBox.Show(this, _noLanguageCodeSelected.Text, _noLanguageCodeSelectedCaption.Text, MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    return;
                }
            }

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
                MoveTranslationItems("FormSettings", "GeneralSettingsPage");
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

        [CanBeNull]
        private string GetSelectedLanguageCode()
        {
            if (string.IsNullOrEmpty(_NO_TRANSLATE_languageCode.Text) || _NO_TRANSLATE_languageCode.Text.Length < 2)
            {
                return null;
            }

            return _NO_TRANSLATE_languageCode.Text.Substring(0, 2);
        }

        private void SaveAs()
        {
            using (var fileDialog =
                new SaveFileDialog
                {
                    Title = _saveAsText.Text,
                    FileName = translations.Text + ".xlf",
                    Filter = _saveAsTextFilter.Text + "|*.xlf",
                    DefaultExt = ".xlf",
                    AddExtension = true
                })
            {
                if (fileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    TranslationHelpers.SaveTranslation(GetSelectedLanguageCode(), _translationItems, fileDialog.FileName);
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

            var languageCode = _translation.First().Value.TargetLanguage;
            try
            {
                var culture = new CultureInfo(languageCode);
                _NO_TRANSLATE_languageCode.Text = string.Concat(culture.TwoLetterISOLanguageName, " (", culture.DisplayName, ")");
            }
            catch
            {
                _NO_TRANSLATE_languageCode.Text = languageCode;
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
                if (MessageBox.Show(this, _saveCurrentChangesText.Text, _saveCurrentChangesCaption.Text, MessageBoxButtons.YesNo) == DialogResult.Yes)
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

        private TranslationItemWithCategory _translationItemWithCategoryInEditing;

        private void translatedText_Enter(object sender, EventArgs e)
        {
            if (_translationItemWithCategoryInEditing != null)
            {
                _translationItemWithCategoryInEditing.TranslatedValue = translatedText.Text;
            }

            _translationItemWithCategoryInEditing = (TranslationItemWithCategory)translateItemBindingSource.Current;

            if (_translationItemWithCategoryInEditing != null)
            {
                _translationItemWithCategoryInEditing.TranslatedValue = _editingCellPrefixText.Text + " " + _translationItemWithCategoryInEditing.TranslatedValue;
            }
        }

        private void translatedText_Leave(object sender, EventArgs e)
        {
            ////Debug.Assert(_translationItemWithCategoryInEditing != null);

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

                if (translateItem == null)
                {
                    return;
                }

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
                {
                    translateItemBindingSource.MoveNext();
                }
            }
            else if (translateGrid.Rows.Count > 0)
            {
                translateGrid.Rows[0].Selected = true;
            }
        }

        private void previousButton_Click(object sender, EventArgs e)
        {
            if (translateGrid.SelectedRows.Count == 1)
            {
                if (translateGrid.CurrentCell.RowIndex > 0)
                {
                    translateItemBindingSource.MovePrevious();
                }
            }
            else if (translateGrid.Rows.Count > 0)
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
            else if ((e.Alt && e.KeyCode == Keys.Down) ||
                (e.Control && e.KeyCode == Keys.Enter))
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
