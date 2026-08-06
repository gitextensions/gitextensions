using System.Collections.ObjectModel;
using Avalonia.Controls;
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

    // The WinForms ComboBox mutated its Items collection directly; Avalonia binds an observable list.
    private readonly ObservableCollection<string> _commitTemplateNames = [];

    // parity-scaffolding: Avalonia's view inventory and designer require a parameterless constructor.
    public FormCommitTemplateSettings()
    {
        InitializeComponent();
        InitializeComplete();
    }

    public FormCommitTemplateSettings(IGitUICommands commands)
        : base(commands, enablePositionRestore: false)
    {
        InitializeComponent();
        buttonOk.Click += buttonOk_Click;
        buttonCancel.Click += buttonCancel_Click;
        _NO_TRANSLATE_comboBoxCommitTemplates.ItemsSource = _commitTemplateNames;
        _NO_TRANSLATE_comboBoxCommitTemplates.SelectionChanged += comboBoxCommitTemplates_SelectedIndexChanged;
        _NO_TRANSLATE_textCommitTemplateText.TextChanged += textCommitTemplateText_TextChanged;
        _NO_TRANSLATE_textBoxCommitTemplateName.TextChanged += textBoxCommitTemplateName_TextChanged;
        checkBoxRegexEnabled.IsCheckedChanged += checkBoxRegexEnabled_CheckedChanged;
        AcceptButton = buttonOk;
        InitializeComplete();

        _NO_TRANSLATE_textBoxCommitTemplateName.MaxLength = _maxUsedCharsForName;

        LoadSettings();
    }

    private void LoadSettings()
    {
        _NO_TRANSLATE_numericMaxFirstLineLength.Value = AppSettings.CommitValidationMaxCntCharsFirstLine;
        _NO_TRANSLATE_numericMaxLineLength.Value = AppSettings.CommitValidationMaxCntCharsPerLine;
        checkBoxSecondLineEmpty.IsChecked = AppSettings.CommitValidationSecondLineMustBeEmpty;
        checkBoxUseIndent.IsChecked = AppSettings.CommitValidationIndentAfterFirstLine;
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
            CommitTemplateItem[] previousCommitTemplates = _commitTemplates;
            _commitTemplates = new CommitTemplateItem[_maxCommitTemplates];
            for (int i = 0; i < _commitTemplates.Length; i++)
            {
                _commitTemplates[i] = i < previousCommitTemplates.Length ? previousCommitTemplates[i] : new CommitTemplateItem();
            }
        }

        _commitTemplateNames.Clear();

        for (int i = 0; i < _commitTemplates.Length; i++)
        {
            _commitTemplateNames.Add(string.Empty);
            RefreshLineInListBox(i);
        }

        _NO_TRANSLATE_comboBoxCommitTemplates.SelectedIndex = 0;
        checkBoxAutoWrap.IsChecked = AppSettings.CommitValidationAutoWrap;
    }

    private void SaveSettings()
    {
        AppSettings.CommitValidationMaxCntCharsFirstLine = Convert.ToInt32(_NO_TRANSLATE_numericMaxFirstLineLength.Value ?? 0);
        AppSettings.CommitValidationMaxCntCharsPerLine = Convert.ToInt32(_NO_TRANSLATE_numericMaxLineLength.Value ?? 0);
        AppSettings.CommitValidationSecondLineMustBeEmpty = checkBoxSecondLineEmpty.IsChecked == true;
        AppSettings.CommitValidationIndentAfterFirstLine = checkBoxUseIndent.IsChecked == true;
        AppSettings.CommitValidationRegEx = _NO_TRANSLATE_textBoxCommitValidationRegex.Text ?? string.Empty;

        CommitTemplateItem.SaveToSettings(_commitTemplates);
        AppSettings.CommitValidationAutoWrap = checkBoxAutoWrap.IsChecked == true;
    }

    private void buttonOk_Click(object? sender, EventArgs e)
    {
        SaveSettings();
        Close();
    }

    private void buttonCancel_Click(object? sender, EventArgs e)
    {
        Close();
    }

    private void textCommitTemplateText_TextChanged(object? sender, EventArgs e)
    {
        Validates.NotNull(_commitTemplates);
        if (_NO_TRANSLATE_comboBoxCommitTemplates.SelectedIndex < 0)
        {
            return;
        }

        _commitTemplates[_NO_TRANSLATE_comboBoxCommitTemplates.SelectedIndex].Text = _NO_TRANSLATE_textCommitTemplateText.Text ?? string.Empty;
    }

    private void textBoxCommitTemplateName_TextChanged(object? sender, EventArgs e)
    {
        Validates.NotNull(_commitTemplates);
        if (_NO_TRANSLATE_comboBoxCommitTemplates.SelectedIndex < 0)
        {
            return;
        }

        _commitTemplates[_NO_TRANSLATE_comboBoxCommitTemplates.SelectedIndex].Name = _NO_TRANSLATE_textBoxCommitTemplateName.Text ?? string.Empty;
        RefreshLineInListBox(_NO_TRANSLATE_comboBoxCommitTemplates.SelectedIndex);
    }

    private void comboBoxCommitTemplates_SelectedIndexChanged(object? sender, EventArgs e)
    {
        Validates.NotNull(_commitTemplates);

        // Avalonia raises SelectionChanged with -1 while the item list is (re)assigned.
        if (_NO_TRANSLATE_comboBoxCommitTemplates.SelectedIndex < 0)
        {
            return;
        }

        _NO_TRANSLATE_textCommitTemplateText.Text = _commitTemplates[_NO_TRANSLATE_comboBoxCommitTemplates.SelectedIndex].Text;
        _NO_TRANSLATE_textBoxCommitTemplateName.Text = _commitTemplates[_NO_TRANSLATE_comboBoxCommitTemplates.SelectedIndex].Name;
        checkBoxRegexEnabled.IsChecked = _commitTemplates[_NO_TRANSLATE_comboBoxCommitTemplates.SelectedIndex].IsRegex;
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

        _commitTemplateNames[line] = $"{line + 1} : {comboBoxText}";
    }

    private void checkBoxRegexEnabled_CheckedChanged(object? sender, EventArgs e)
    {
        if (_NO_TRANSLATE_comboBoxCommitTemplates.SelectedIndex < 0)
        {
            return;
        }

        _commitTemplates![_NO_TRANSLATE_comboBoxCommitTemplates.SelectedIndex].IsRegex = checkBoxRegexEnabled.IsChecked == true;
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(FormCommitTemplateSettings form)
    {
        public ComboBox CommitTemplates => form._NO_TRANSLATE_comboBoxCommitTemplates;
        public TextBox TemplateName => form._NO_TRANSLATE_textBoxCommitTemplateName;
        public TextBox TemplateText => form._NO_TRANSLATE_textCommitTemplateText;
        public CheckBox RegexEnabled => form.checkBoxRegexEnabled;
        public CheckBox AutoWrap => form.checkBoxAutoWrap;
        public NumericUpDown MaxFirstLineLength => form._NO_TRANSLATE_numericMaxFirstLineLength;
        public Button Ok => form.buttonOk;
        public Button Cancel => form.buttonCancel;

        public void LoadSettings() => form.LoadSettings();
        public void SaveSettings() => form.SaveSettings();
    }
}
