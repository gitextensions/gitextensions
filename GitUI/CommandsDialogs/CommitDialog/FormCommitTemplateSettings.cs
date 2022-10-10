﻿using GitCommands;
using Microsoft;
using ResourceManager;

namespace GitUI.CommandsDialogs.CommitDialog
{
    public partial class FormCommitTemplateSettings : GitExtensionsForm
    {
        private readonly TranslationString _emptyTemplate =
            new("empty");

        private CommitTemplateItem[]? _commitTemplates;

        private const int _maxCommitTemplates = 10;
        private const int _maxShownCharsForName = 50;
        private const int _maxUsedCharsForName = 80;

        public FormCommitTemplateSettings()
        {
            InitializeComponent();
            InitializeComplete();

            _NO_TRANSLATE_textBoxCommitTemplateName.MaxLength = _maxUsedCharsForName;

            LoadSettings();
        }

        private void LoadSettings()
        {
            _NO_TRANSLATE_numericMaxFirstLineLength.Value = AppSettings.CommitValidationMaxCntCharsFirstLine;
            _NO_TRANSLATE_numericMaxLineLength.Value = AppSettings.CommitValidationMaxCntCharsPerLine;
            checkBoxSecondLineEmpty.Checked = AppSettings.CommitValidationSecondLineMustBeEmpty;
            checkBoxUseIndent.Checked = AppSettings.CommitValidationIndentAfterFirstLine;
            _NO_TRANSLATE_textBoxCommitValidationRegex.Text = AppSettings.CommitValidationRegEx;

            _commitTemplates = CommitTemplateItem.LoadFromSettings();

            if (_commitTemplates is null)
            {
                _commitTemplates = new CommitTemplateItem[_maxCommitTemplates];
                for (int i = 0; i < _commitTemplates.Length; i++)
                {
                    _commitTemplates[i] = new CommitTemplateItem();
                }
            }
            else if (_commitTemplates.Length < _maxCommitTemplates)
            {
                // Migration: keep the one configured and complete with empty ones
                var previousCommitTemplates = _commitTemplates;
                _commitTemplates = new CommitTemplateItem[_maxCommitTemplates];
                for (int i = 0; i < _commitTemplates.Length; i++)
                {
                    _commitTemplates[i] = i < previousCommitTemplates.Length ? previousCommitTemplates[i] : new CommitTemplateItem();
                }
            }

            _NO_TRANSLATE_comboBoxCommitTemplates.Items.Clear();

            for (int i = 0; i < _commitTemplates.Length; i++)
            {
                _NO_TRANSLATE_comboBoxCommitTemplates.Items.Add(string.Empty);
                RefreshLineInListBox(i);
            }

            _NO_TRANSLATE_comboBoxCommitTemplates.SelectedIndex = 0;
            checkBoxAutoWrap.Checked = AppSettings.CommitValidationAutoWrap;
        }

        private void SaveSettings()
        {
            AppSettings.CommitValidationMaxCntCharsFirstLine = Convert.ToInt32(_NO_TRANSLATE_numericMaxFirstLineLength.Value);
            AppSettings.CommitValidationMaxCntCharsPerLine = Convert.ToInt32(_NO_TRANSLATE_numericMaxLineLength.Value);
            AppSettings.CommitValidationSecondLineMustBeEmpty = checkBoxSecondLineEmpty.Checked;
            AppSettings.CommitValidationIndentAfterFirstLine = checkBoxUseIndent.Checked;
            AppSettings.CommitValidationRegEx = _NO_TRANSLATE_textBoxCommitValidationRegex.Text;

            CommitTemplateItem.SaveToSettings(_commitTemplates);
            AppSettings.CommitValidationAutoWrap = checkBoxAutoWrap.Checked;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            SaveSettings();
            Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void textCommitTemplateText_TextChanged(object sender, EventArgs e)
        {
            Validates.NotNull(_commitTemplates);
            _commitTemplates[_NO_TRANSLATE_comboBoxCommitTemplates.SelectedIndex].Text = _NO_TRANSLATE_textCommitTemplateText.Text;
        }

        private void textBoxCommitTemplateName_TextChanged(object sender, EventArgs e)
        {
            Validates.NotNull(_commitTemplates);
            _commitTemplates[_NO_TRANSLATE_comboBoxCommitTemplates.SelectedIndex].Name = _NO_TRANSLATE_textBoxCommitTemplateName.Text;
            RefreshLineInListBox(_NO_TRANSLATE_comboBoxCommitTemplates.SelectedIndex);
        }

        private void comboBoxCommitTemplates_SelectedIndexChanged(object sender, EventArgs e)
        {
            Validates.NotNull(_commitTemplates);
            _NO_TRANSLATE_textCommitTemplateText.Text = _commitTemplates[_NO_TRANSLATE_comboBoxCommitTemplates.SelectedIndex].Text;
            _NO_TRANSLATE_textBoxCommitTemplateName.Text = _commitTemplates[_NO_TRANSLATE_comboBoxCommitTemplates.SelectedIndex].Name;
        }

        private void RefreshLineInListBox(int line)
        {
            Validates.NotNull(_commitTemplates);

            string comboBoxText;

            if (!string.IsNullOrEmpty(_commitTemplates[line].Name))
            {
                comboBoxText = _commitTemplates[line].Name.ShortenTo(_maxShownCharsForName);
            }
            else
            {
                comboBoxText = "<" + _emptyTemplate.Text + ">";
            }

            _NO_TRANSLATE_comboBoxCommitTemplates.Items[line] = $"{line + 1} : {comboBoxText}";
        }
    }
}
