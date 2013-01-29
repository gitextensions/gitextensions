using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using GitUI;
using ResourceManager.Translation;

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
        readonly TranslationString saveAsTextFilter = new TranslationString("Translation file (*.xml)");
        readonly TranslationString selectLanguageCode = new TranslationString("Select a language code first.");
        readonly TranslationString noLanguageCodeSelected = new TranslationString("There is no language code selected." + 
            Environment.NewLine + "Do you want to select a language code first?");
        readonly TranslationString noLanguageCodeSelectedCaption = new TranslationString("Language code");
        readonly TranslationString editingCellPrefixText = new TranslationString("[EDITING]");

        private List<TranslateItem> translationItems;

        readonly Translation neutralTranslation = new Translation();
        Translation translation;
        private TranslationCategory allCategories = new TranslationCategory();

        private bool changesMade;

        public FormTranslate()
            : base(true)
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

        void FormTranslate_FormClosing(object sender, FormClosingEventArgs e)
        {
            AskForSave();
        }

        private void UpdateProgress()
        {
            int translatedCount = translationItems.Count(translateItem => translateItem.Status != TranslationType.Obsolete && 
                !string.IsNullOrEmpty(translateItem.TranslatedValue));
            int totalCount = translationItems.Count(translateItem => translateItem.Status != TranslationType.Obsolete);
            var progresMsg = string.Format(translateProgressText.Text, translatedCount.ToString(), totalCount.ToString());
            if (translateProgress.Text != progresMsg)
            {
                translateProgress.Text = progresMsg;
                toolStrip1.Refresh();
            }
        }

        private bool CompareSourceString(string origin, string current)
        {
            bool equal = (origin == current);
            if (!equal)
                return origin == current.Replace("\n", Environment.NewLine);
            return equal;
        }

        private void LoadTranslation()
        {
            translationItems = new List<TranslateItem>();

            var neutralItems =
                from translationCategory in neutralTranslation.GetTranslationCategories()
                from translationItem in translationCategory.GetTranslationItems()
                select new
                {
                    Category = translationCategory.Name,
                    Name = translationItem.Name,
                    Property = translationItem.Property,
                    Source = translationItem.Source,
                    Value = translationItem.Value
                };
            var allItems = translation != null ? 
                (from translationCategory in translation.GetTranslationCategories()
                 from translationItem in translationCategory.GetTranslationItems()
                 select new
                 {
                     Category = translationCategory.Name,
                     Name = translationItem.Name,
                     Property = translationItem.Property,
                     Source = translationItem.Source,
                     Value = translationItem.Value
                 }).ToList() : null;

            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (var item in neutralItems)
            {
                var translateItem = new TranslateItem
                                        {
                                            Category = item.Category,
                                            Name = item.Name,
                                            Property = item.Property,
                                            NeutralValue = item.Value,
                                            Status = TranslationType.New
                                        };

                if (translation != null)
                {
                    var curItem =
                        (from trItem in allItems
                         where trItem.Category.TrimStart('_') == item.Category.TrimStart('_') &&
                         trItem.Name.TrimStart('_') == item.Name.TrimStart('_') &&
                         trItem.Property == item.Property
                         select trItem).FirstOrDefault();

                    if (curItem != null)
                    {
                        translateItem.TranslatedValue = curItem.Value;
                        if (curItem.Source == null || CompareSourceString(item.Value, curItem.Source))
                        {
                            if (!String.IsNullOrEmpty(curItem.Value))
                                translateItem.Status = TranslationType.Translated;
                            else
                                translateItem.Status = TranslationType.Unfinished;
                        }
                        else
                            translateItem.Status = TranslationType.Obsolete;
                        allItems.Remove(curItem);
                        string source = curItem.Source ?? item.Value;
                        if (!String.IsNullOrEmpty(curItem.Value) && !dict.ContainsKey(source))
                            dict.Add(source, curItem.Value);
                    }
                }

                translationItems.Add(translateItem);
            }

            if (allItems != null)
            {
                foreach (var item in allItems)
                {
                    if (!String.IsNullOrEmpty(item.Value))
                    {
                        var translateItem = new TranslateItem
                            {
                                Category = item.Category,
                                Name = item.Name,
                                Property = item.Property,
                                NeutralValue = item.Source,
                                TranslatedValue = item.Value,
                                Status = TranslationType.Obsolete
                            };

                        translationItems.Add(translateItem);
                        if (item.Source != null && !dict.ContainsKey(item.Source))
                            dict.Add(item.Source, item.Value);
                    }
                }
            }

            var untranlatedItems = from trItem in translationItems
                                   where trItem.Status == TranslationType.New &&
                                   dict.ContainsKey(trItem.NeutralValue)
                                   select trItem;

            foreach (var untranlatedItem in untranlatedItems)
            {
                untranlatedItem.Status = TranslationType.Unfinished;
                untranlatedItem.TranslatedValue = dict[untranlatedItem.NeutralValue];
            }

            UpdateProgress();
        }

        private void FillTranslateGrid(TranslationCategory filter)
        {
            if (translationItems == null)
                return;

            translateItemBindingSource.DataSource = null;

            if (filter == allCategories)
                filter = null;

            var filterTranslate = translationItems.Where(translateItem => filter == null || filter.Name.Equals(translateItem.Category)).
                Where(translateItem => translateItem.Status != TranslationType.Obsolete && (!hideTranslatedItems.Checked || string.IsNullOrEmpty(translateItem.TranslatedValue))).ToList();

            translateItemBindingSource.DataSource = filterTranslate;

            UpdateProgress();
        }

        public object CreateInstanceOfClass(Type type)
        {
            if (type == GetType())
                return this;
            else
                return TranslationUtl.CreateInstanceOfClass(type);
        }

        public void GetPropertiesToTranslate()
        {
            translateCategories.Items.Clear();
            allCategories.Name = allText.Text;
            translateCategories.Items.Add(allCategories);
            FillNeutralTranslation(neutralTranslation);
            translateCategories.Items.AddRange(neutralTranslation.GetTranslationCategories().ToArray());
        }

        private void FillNeutralTranslation(Translation neutralTranslation)
        {
            string currentTranslation = GitCommands.Settings.Translation;

            try
            {
                //Set language to neutral to get neutral translations
                GitCommands.Settings.Translation = "";
                Translate();

                List<Type> translatableTypes = TranslationUtl.GetTranslatableTypes();

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
                
                //Restore translation
                GitCommands.Settings.Translation = currentTranslation;
                Translate();
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

                foreach (var item in fromCategory.GetTranslationItems())
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
            var foreignTranslation = new Translation { GitExVersion = GitCommands.Settings.GitExtensionsVersionString, LanguageCode = GetSelectedLanguageCode() };
            foreach (TranslateItem translateItem in translationItems)
            {
                string value = translateItem.TranslatedValue ?? String.Empty;
                TranslationItem ti = new TranslationItem(translateItem.Name, translateItem.Property,
                    translateItem.NeutralValue, value);
                ti.Status = translateItem.Status;
                if (ti.Status == TranslationType.Obsolete && 
                    (String.IsNullOrEmpty(value) || String.IsNullOrEmpty(translateItem.NeutralValue)))
                    continue;
                if (string.IsNullOrEmpty(value))
                {
                    if (ti.Status == TranslationType.Translated || ti.Status == TranslationType.New)
                        ti.Status = TranslationType.Unfinished;
                }
                else
                {
                    // TODO: Support in form
                    if (ti.Status == TranslationType.Unfinished)
                        ti.Status = TranslationType.Translated;
                }
                foreignTranslation.FindOrAddTranslationCategory(translateItem.Category).AddTranslationItem(ti);
            }

            using (var fileDialog =
                new SaveFileDialog
                    {
                        Title = saveAsText.Text,
                        FileName = translations.Text + ".xml",
                        Filter = saveAsTextFilter.Text + "|*.xml",
                        DefaultExt = ".xml",
                        AddExtension = true
                    })
            {

                if (fileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    TranslationSerializer.Serialize(foreignTranslation, fileDialog.FileName);
                    changesMade = false;
                }
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

            foreach (TranslateItem translateItem in translationItems)
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
