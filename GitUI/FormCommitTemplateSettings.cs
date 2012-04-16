using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;


namespace GitUI
{
    public partial class FormCommitTemplateSettings : GitExtensionsForm
    {
        private readonly TranslationString _emptyTemplate =
            new TranslationString("empty");

        private CommitTemplateItem[] _commitTemplates;

        private const int _maxCommitTemplates = 5;
        private const int _maxShownCharsForName = 15;
        private const int _maxUsedCharsForName = 80;

        public FormCommitTemplateSettings()
        {
            InitializeComponent();
            Translate();

            _NO_TRANSLATE_textBoxCommitTemplateName.MaxLength = _maxUsedCharsForName;

            LoadSettings();
        }

        private void LoadSettings()
        {
            _NO_TRANSLATE_numericMaxFirstLineLength.Value = Settings.CommitValidationMaxCntCharsFirstLine;
            _NO_TRANSLATE_numericMaxLineLength.Value = Settings.CommitValidationMaxCntCharsPerLine;
            checkBoxSecondLineEmpty.Checked = Settings.CommitValidationSecondLineMustBeEmpty;

            _commitTemplates = CommitTemplateItem.DeserializeCommitTemplatesFromXml(Settings.CommitTemplates);

            if (null == _commitTemplates)
            {
                _commitTemplates = new CommitTemplateItem[_maxCommitTemplates];
                for (int i = 0; i < _commitTemplates.Length; i++)
                    _commitTemplates[i] = new CommitTemplateItem();
            }

            _NO_TRANSLATE_comboBoxCommitTemplates.Items.Clear();

            for (int i = 0; i < _commitTemplates.Length; i++)
            {           
                _NO_TRANSLATE_comboBoxCommitTemplates.Items.Add(String.Empty);
                RefreshLineInListBox(i);
            }

            _NO_TRANSLATE_comboBoxCommitTemplates.SelectedIndex = 0;
        }

        private void SaveSettings()
        {
            Settings.CommitValidationMaxCntCharsFirstLine = Convert.ToInt32(_NO_TRANSLATE_numericMaxFirstLineLength.Value);
            Settings.CommitValidationMaxCntCharsPerLine = Convert.ToInt32(_NO_TRANSLATE_numericMaxLineLength.Value);
            Settings.CommitValidationSecondLineMustBeEmpty = checkBoxSecondLineEmpty.Checked;

            string serializedCommitTemplates = CommitTemplateItem.SerializeCommitTemplatesIntoXml(_commitTemplates);
            if (null == serializedCommitTemplates)
                Settings.CommitTemplates = "";
            else
                Settings.CommitTemplates = serializedCommitTemplates;
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
            _commitTemplates[_NO_TRANSLATE_comboBoxCommitTemplates.SelectedIndex].Text = _NO_TRANSLATE_textCommitTemplateText.Text;
        }

        private void textBoxCommitTemplateName_TextChanged(object sender, EventArgs e)
        {
            _commitTemplates[_NO_TRANSLATE_comboBoxCommitTemplates.SelectedIndex].Name = _NO_TRANSLATE_textBoxCommitTemplateName.Text;
            RefreshLineInListBox(_NO_TRANSLATE_comboBoxCommitTemplates.SelectedIndex);
        }

        private void comboBoxCommitTemplates_SelectedIndexChanged(object sender, EventArgs e)
        {
            _NO_TRANSLATE_textCommitTemplateText.Text = _commitTemplates[_NO_TRANSLATE_comboBoxCommitTemplates.SelectedIndex].Text;
            _NO_TRANSLATE_textBoxCommitTemplateName.Text = _commitTemplates[_NO_TRANSLATE_comboBoxCommitTemplates.SelectedIndex].Name;
        }

        private void RefreshLineInListBox(int line)
        {
            string comboBoxText;

            if (!_commitTemplates[line].Name.IsNullOrEmpty())
            {
                comboBoxText = _commitTemplates[line].Name.Substring(0, _commitTemplates[line].Name.Length > _maxShownCharsForName ? (_maxShownCharsForName - 3) : _commitTemplates[line].Name.Length);
                comboBoxText += _commitTemplates[line].Name.Length > _maxShownCharsForName ? "..." : "";
            }
            else
                comboBoxText = "<" + _emptyTemplate.Text + ">";

            _NO_TRANSLATE_comboBoxCommitTemplates.Items[line] = String.Format("{0} : {1}", (line + 1), comboBoxText);
        }

    }
}
