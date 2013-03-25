using System;
using GitCommands;
using GitCommands.Properties;
using ResourceManager.Translation;

namespace GitUI.CommandsDialogs.CommitDialog
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
            _NO_TRANSLATE_numericMaxFirstLineLength.Value = Settings.Default.CommitValidationMaxCntCharsFirstLine;
            _NO_TRANSLATE_numericMaxLineLength.Value = Settings.Default.CommitValidationMaxCntCharsPerLine;
            checkBoxSecondLineEmpty.Checked = Settings.Default.CommitValidationSecondLineMustBeEmpty;
            checkBoxUseIndent.Checked = Settings.Default.CommitValidationIndentAfterFirstLine;
            _NO_TRANSLATE_textBoxCommitValidationRegex.Text = Settings.Default.CommitValidationRegEx;

            _commitTemplates = CommitTemplateItem.DeserializeCommitTemplates(Settings.Default.CommitTemplates);

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
            checkBoxAutoWrap.Checked = Settings.Default.CommitValidationAutoWrap;

        }

        private void SaveSettings()
        {
            Settings.Default.CommitValidationMaxCntCharsFirstLine = Convert.ToInt32(_NO_TRANSLATE_numericMaxFirstLineLength.Value);
            Settings.Default.CommitValidationMaxCntCharsPerLine = Convert.ToInt32(_NO_TRANSLATE_numericMaxLineLength.Value);
            Settings.Default.CommitValidationSecondLineMustBeEmpty = checkBoxSecondLineEmpty.Checked;
            Settings.Default.CommitValidationIndentAfterFirstLine = checkBoxUseIndent.Checked;
            Settings.Default.CommitValidationRegEx = _NO_TRANSLATE_textBoxCommitValidationRegex.Text;

            string serializedCommitTemplates = CommitTemplateItem.SerializeCommitTemplates(_commitTemplates);
            if (null == serializedCommitTemplates)
                Settings.Default.CommitTemplates = "";
            else
                Settings.Default.CommitTemplates = serializedCommitTemplates;
            Settings.Default.CommitValidationAutoWrap = checkBoxAutoWrap.Checked;
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
