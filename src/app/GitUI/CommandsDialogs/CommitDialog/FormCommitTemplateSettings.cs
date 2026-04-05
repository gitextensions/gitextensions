using GitCommands;
using GitExtensions.Extensibility.Git;
using Microsoft;
using ResourceManager;

namespace GitUI.CommandsDialogs.CommitDialog;

public sealed partial class FormCommitTemplateSettings : GitExtensionsDialog
{
    private readonly TranslationString _emptyTemplate =
        new("empty");

    private CommitTemplateItem[]? _commitTemplates;

    private const int _maxCommitTemplates = 10;
    private const int _maxShownCharsForName = 50;
    private const int _maxUsedCharsForName = 80;

    public FormCommitTemplateSettings(IGitUICommands commands)
        : base(commands, enablePositionRestore: false)
    {
        InitializeComponent();
        InitializeComplete();

        _NO_TRANSLATE_textBoxCommitTemplateName.MaxLength = _maxUsedCharsForName;

        LoadSettings();
    }

    private void LoadSettings()
    {
        _NO_TRANSLATE_numericMaxFirstLineLength.Value = AppSettings.CommitValidationMaxCntCharsFirstLine.Value;
        _NO_TRANSLATE_numericMaxLineLength.Value = AppSettings.CommitValidationMaxCntCharsPerLine.Value;
        checkBoxSecondLineEmpty.Checked = AppSettings.CommitValidationSecondLineMustBeEmpty.Value;
        checkBoxUseIndent.Checked = AppSettings.CommitValidationIndentAfterFirstLine.Value;
        _NO_TRANSLATE_textBoxCommitValidationRegex.Text = AppSettings.CommitValidationRegEx.Value;

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
            CommitTemplateItem[] previousCommitTemplates = _commitTemplates;
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
        checkBoxAutoWrap.Checked = AppSettings.CommitValidationAutoWrap.Value;
    }

    private void SaveSettings()
    {
        AppSettings.CommitValidationMaxCntCharsFirstLine.Value = Convert.ToInt32(_NO_TRANSLATE_numericMaxFirstLineLength.Value);
        AppSettings.CommitValidationMaxCntCharsPerLine.Value = Convert.ToInt32(_NO_TRANSLATE_numericMaxLineLength.Value);
        AppSettings.CommitValidationSecondLineMustBeEmpty.Value = checkBoxSecondLineEmpty.Checked;
        AppSettings.CommitValidationIndentAfterFirstLine.Value = checkBoxUseIndent.Checked;
        AppSettings.CommitValidationRegEx.Value = _NO_TRANSLATE_textBoxCommitValidationRegex.Text;

        CommitTemplateItem.SaveToSettings(_commitTemplates);
        AppSettings.CommitValidationAutoWrap.Value = checkBoxAutoWrap.Checked;
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
        checkBoxRegexEnabled.Checked = _commitTemplates[_NO_TRANSLATE_comboBoxCommitTemplates.SelectedIndex].IsRegex;
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

    private void checkBoxRegexEnabled_CheckedChanged(object sender, EventArgs e)
    {
        _commitTemplates[_NO_TRANSLATE_comboBoxCommitTemplates.SelectedIndex].IsRegex = checkBoxRegexEnabled.Checked;
    }
}
