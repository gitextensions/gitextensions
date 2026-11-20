using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitUI;
using GitUI.CommandsDialogs.CommitDialog;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class CommitDialogExtrasTests
{
    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
    }

    [Test]
    public void WordWrapper_should_wrap_at_the_line_limit()
    {
        string wrapped = WordWrapper.WrapSingleLine("one two three four five", 10);

        string[] lines = wrapped.Split(Environment.NewLine);
        lines.Should().OnlyContain(line => line.Length <= 10);
        string.Join(' ', lines).Should().Be("one two three four five");
    }

    [Test]
    public void WordWrapper_should_keep_a_single_long_word_on_its_own_line()
    {
        WordWrapper.WrapSingleLine("supercalifragilistic", 5)
            .Should().Be("supercalifragilistic");
    }

    [Test]
    public void WordWrapper_should_return_short_text_unchanged()
    {
        WordWrapper.WrapSingleLine("short text", 80).Should().Be("short text");
    }

    [AvaloniaTest]
    public void FormCommitTemplateSettings_should_construct_with_the_original_controls()
    {
        FormCommitTemplateSettings form = new();

        form.FindControl<ComboBox>("_NO_TRANSLATE_comboBoxCommitTemplates")!.Should().NotBeNull();
        form.FindControl<TextBox>("_NO_TRANSLATE_textBoxCommitTemplateName")!.Should().NotBeNull();
        form.FindControl<TextBox>("_NO_TRANSLATE_textCommitTemplateText")!.Should().NotBeNull();
        form.FindControl<CheckBox>("checkBoxRegexEnabled")!.Should().NotBeNull();
        form.FindControl<CheckBox>("checkBoxAutoWrap")!.Should().NotBeNull();
        form.FindControl<NumericUpDown>("_NO_TRANSLATE_numericMaxFirstLineLength")!.Should().NotBeNull();
        form.FindControl<Button>("buttonOk")!.Should().NotBeNull();
        form.FindControl<Button>("buttonCancel")!.Should().NotBeNull();
    }

    [AvaloniaTest]
    public void FormCommitTemplateSettings_should_emit_its_translation_keys()
    {
        FormCommitTemplateSettings form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormCommitTemplateSettings), "$this", "Text", "Commit message settings");
        translation.Received(1).AddTranslationItem(nameof(FormCommitTemplateSettings), "buttonOk", "Text", "OK");
        translation.Received(1).AddTranslationItem(nameof(FormCommitTemplateSettings), "buttonCancel", "Text", "Cancel");
        translation.Received(1).AddTranslationItem(nameof(FormCommitTemplateSettings), "tabPage1", "Text", "Commit templates");
        translation.Received(1).AddTranslationItem(nameof(FormCommitTemplateSettings), "tabPage2", "Text", "Commit validation");
        translation.Received(1).AddTranslationItem(nameof(FormCommitTemplateSettings), "checkBoxRegexEnabled", "Text", "Enable regex");
        string regexTooltip = """
            Use {{regex}}[regex group number] to extract branch name part
            Group number is optional, default is 1

            Examples on branch name: "feature/ABC-4587-commitMessageRegex"
            "Commit from: {{^feature/(.*)$}} branch" -> "Commit from: ABC-4587-commitMessageRegex branch"
            "{{([A-Z]+-\d+)}}: My message is" -> "ABC-4587: My message is "
            "Name: {{([A-Z]+-\d+)-(.*)}}[2], issue: {{([A-Z]+-\d+)-(.*)}}[1]" -> "Name: commitMessageRegex, issue: ABC-4587"
            """.ReplaceLineEndings("\r\n");
        translation.Received(1).AddTranslationItem(nameof(FormCommitTemplateSettings), "checkBoxRegexEnabled", "toolTipRegex", regexTooltip);
        translation.Received(1).AddTranslationItem(nameof(FormCommitTemplateSettings), "labelAutoWrap", "Text", "Auto-wrap commit message (except subject line)");
        translation.Received(1).AddTranslationItem(nameof(FormCommitTemplateSettings), "labelMaxFirstLineLength", "Text", "Maximum number of characters in the first line (0 = check disabled):");
        translation.Received(1).AddTranslationItem(nameof(FormCommitTemplateSettings), "labelSecondLineEmpty", "Text", "Second line must be empty:");

        // The bare validation checkboxes carried no Text in the original and must emit no key.
        translation.DidNotReceive().AddTranslationItem(nameof(FormCommitTemplateSettings), "checkBoxAutoWrap", "Text", Arg.Any<string>());
        translation.DidNotReceive().AddTranslationItem(nameof(FormCommitTemplateSettings), "checkBoxUseIndent", "Text", Arg.Any<string>());
        translation.DidNotReceive().AddTranslationItem(nameof(FormCommitTemplateSettings), "checkBoxSecondLineEmpty", "Text", Arg.Any<string>());
    }

    [AvaloniaTest]
    public void FormCommitTemplateSettings_should_load_ten_template_slots()
    {
        IGitModule module = Substitute.For<IGitModule>();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);

        FormCommitTemplateSettings form = new(commands);

        form.FindControl<ComboBox>("_NO_TRANSLATE_comboBoxCommitTemplates")!.ItemCount.Should().Be(10);
        form.FindControl<ComboBox>("_NO_TRANSLATE_comboBoxCommitTemplates")!.SelectedIndex.Should().Be(0);

        // The template-name box keeps the original 80-character cap.
        form.FindControl<TextBox>("_NO_TRANSLATE_textBoxCommitTemplateName")!.MaxLength.Should().Be(80);

        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaTest]
    public void FormCommitTemplateSettings_should_match_native_96_dpi_designer_geometry()
    {
        FormCommitTemplateSettings form = new(CreateCommands());
        form.Show();
        Dispatcher.UIThread.RunJobs();

        form.ClientSize.Should().Be(new Size(698, 361));
        form.FindControl<Grid>("ControlsPanel")!.Bounds.Should().Be(new Rect(0, 320, 698, 41));
        form.FindControl<Button>("buttonOk")!.Bounds.Should().Be(new Rect(529, 8, 75, 25));
        form.FindControl<Button>("buttonCancel")!.Bounds.Should().Be(new Rect(610, 8, 75, 25));
        form.FindControl<TabControl>("tabControl1")!.Bounds.Should().Be(new Rect(9, 9, 680, 302));
        form.FindControl<Grid>("tableLayoutPanel5")!.Bounds.Should().Be(new Rect(3, 3, 666, 262));
        form.FindControl<ComboBox>("_NO_TRANSLATE_comboBoxCommitTemplates")!.Bounds.Should().Be(new Rect(3, 3, 660, 23));
        form.FindControl<TextBox>("_NO_TRANSLATE_textBoxCommitTemplateName")!.Bounds.Should().Be(new Rect(113, 32, 550, 23));
        form.FindControl<TextBox>("_NO_TRANSLATE_textCommitTemplateText")!.Bounds.Should().Be(new Rect(113, 61, 550, 198));
        form.FindControl<CheckBox>("checkBoxRegexEnabled")!.Bounds.Should().Be(new Rect(3, 243, 92, 19));

        form.FindControl<TabControl>("tabControl1")!.SelectedItem = form.FindControl<TabItem>("tabPage2")!;
        Dispatcher.UIThread.RunJobs();

        form.FindControl<Grid>("tableLayoutPanel3")!.Bounds.Should().Be(new Rect(3, 3, 666, 262));
        form.FindControl<NumericUpDown>("_NO_TRANSLATE_numericMaxFirstLineLength")!.Bounds.Should().Be(new Rect(377, 3, 60, 23));
        form.FindControl<NumericUpDown>("_NO_TRANSLATE_numericMaxLineLength")!.Bounds.Should().Be(new Rect(377, 32, 60, 23));
        form.FindControl<CheckBox>("checkBoxAutoWrap")!.Bounds.Should().Be(new Rect(377, 65, 15, 14));
        form.FindControl<TextBox>("_NO_TRANSLATE_textBoxCommitValidationRegex")!.Bounds.Should().Be(new Rect(377, 89, 286, 23));
        form.FindControl<CheckBox>("checkBoxUseIndent")!.Bounds.Should().Be(new Rect(377, 122, 15, 14));
        form.FindControl<CheckBox>("checkBoxSecondLineEmpty")!.Bounds.Should().Be(new Rect(377, 150, 15, 14));
        form.Close();
    }

    [AvaloniaTest]
    public void FormCommitTemplateSettings_should_migrate_and_edit_independent_template_slots()
    {
        CommitSettingsSnapshot snapshot = CommitSettingsSnapshot.Capture();
        try
        {
            CommitTemplateItem.SaveToSettings([new CommitTemplateItem("Release", "release text", icon: null, isRegex: true)]);
            FormCommitTemplateSettings form = new(CreateCommands());
            ComboBox commitTemplates = form.FindControl<ComboBox>("_NO_TRANSLATE_comboBoxCommitTemplates")!;
            TextBox templateName = form.FindControl<TextBox>("_NO_TRANSLATE_textBoxCommitTemplateName")!;
            TextBox templateText = form.FindControl<TextBox>("_NO_TRANSLATE_textCommitTemplateText")!;
            CheckBox regexEnabled = form.FindControl<CheckBox>("checkBoxRegexEnabled")!;
            form.Show();
            Dispatcher.UIThread.RunJobs();

            commitTemplates.ItemCount.Should().Be(10);
            commitTemplates.Items[0].Should().Be("1 : Release");
            templateName.Text.Should().Be("Release");
            templateText.Text.Should().Be("release text");
            regexEnabled.IsChecked.Should().BeTrue();

            commitTemplates.SelectedItem = commitTemplates.Items[1];
            Dispatcher.UIThread.RunJobs();
            commitTemplates.SelectedIndex.Should().Be(1);
            string longName = new('x', 60);
            templateName.Text = longName;
            templateName.RaiseEvent(new TextChangedEventArgs(TextBox.TextChangedEvent));
            templateText.Text = "second text";
            templateText.RaiseEvent(new TextChangedEventArgs(TextBox.TextChangedEvent));
            regexEnabled.IsChecked = true;
            Dispatcher.UIThread.RunJobs();

            commitTemplates.Items[1].Should().Be($"2 : {new string('x', 47)}...");
            form.FindControl<Button>("buttonOk")!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            CommitTemplateItem[] templates = CommitTemplateItem.LoadFromSettings()!;
            templates.Should().HaveCount(10);
            templates[0].Name.Should().Be("Release");
            templates[1].Name.Should().Be(longName);
            templates[1].Text.Should().Be("second text");
            templates[1].IsRegex.Should().BeTrue();
        }
        finally
        {
            snapshot.Restore();
        }
    }

    [AvaloniaTest]
    public void FormCommitTemplateSettings_should_round_trip_all_persisted_values()
    {
        CommitSettingsSnapshot snapshot = CommitSettingsSnapshot.Capture();
        try
        {
            AppSettings.CommitTemplates = string.Empty;
            FormCommitTemplateSettings form = new(CreateCommands());
            form.Show();
            form.FindControl<TextBox>("_NO_TRANSLATE_textBoxCommitTemplateName")!.Text = "Feature";
            form.FindControl<TextBox>("_NO_TRANSLATE_textCommitTemplateText")!.Text = "feature text";
            form.FindControl<CheckBox>("checkBoxRegexEnabled")!.IsChecked = true;
            form.FindControl<NumericUpDown>("_NO_TRANSLATE_numericMaxFirstLineLength")!.Value = 52;
            form.FindControl<NumericUpDown>("_NO_TRANSLATE_numericMaxLineLength")!.Value = 72;
            form.FindControl<TextBox>("_NO_TRANSLATE_textBoxCommitValidationRegex")!.Text = "^[A-Z]+-[0-9]+";
            form.FindControl<CheckBox>("checkBoxAutoWrap")!.IsChecked = false;
            form.FindControl<CheckBox>("checkBoxUseIndent")!.IsChecked = false;
            form.FindControl<CheckBox>("checkBoxSecondLineEmpty")!.IsChecked = true;
            Dispatcher.UIThread.RunJobs();

            form.FindControl<Button>("buttonOk")!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            AppSettings.CommitValidationMaxCntCharsFirstLine.Should().Be(52);
            AppSettings.CommitValidationMaxCntCharsPerLine.Should().Be(72);
            AppSettings.CommitValidationRegEx.Should().Be("^[A-Z]+-[0-9]+");
            AppSettings.CommitValidationAutoWrap.Should().BeFalse();
            AppSettings.CommitValidationIndentAfterFirstLine.Should().BeFalse();
            AppSettings.CommitValidationSecondLineMustBeEmpty.Should().BeTrue();
            CommitTemplateItem[] templates = CommitTemplateItem.LoadFromSettings()!;
            templates[0].Name.Should().Be("Feature");
            templates[0].Text.Should().Be("feature text");
            templates[0].IsRegex.Should().BeTrue();
            form.IsVisible.Should().BeFalse();
        }
        finally
        {
            snapshot.Restore();
        }
    }

    [AvaloniaTest]
    public void FormCommitTemplateSettings_cancel_should_not_save_edits()
    {
        CommitSettingsSnapshot snapshot = CommitSettingsSnapshot.Capture();
        try
        {
            AppSettings.CommitTemplates = string.Empty;
            AppSettings.CommitValidationMaxCntCharsFirstLine = 50;
            FormCommitTemplateSettings form = new(CreateCommands());
            form.Show();
            form.FindControl<TextBox>("_NO_TRANSLATE_textBoxCommitTemplateName")!.Text = "unsaved";
            form.FindControl<NumericUpDown>("_NO_TRANSLATE_numericMaxFirstLineLength")!.Value = 99;

            form.FindControl<Button>("buttonCancel")!.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));

            form.IsVisible.Should().BeFalse();
            AppSettings.CommitTemplates.Should().BeEmpty();
            AppSettings.CommitValidationMaxCntCharsFirstLine.Should().Be(50);
        }
        finally
        {
            snapshot.Restore();
        }
    }

    private static IGitUICommands CreateCommands()
    {
        IGitModule module = Substitute.For<IGitModule>();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        return commands;
    }

    private sealed record CommitSettingsSnapshot(
        string Templates,
        int MaxFirstLineLength,
        int MaxLineLength,
        bool SecondLineEmpty,
        bool UseIndent,
        bool AutoWrap,
        string ValidationRegex)
    {
        public static CommitSettingsSnapshot Capture() => new(
            AppSettings.CommitTemplates,
            AppSettings.CommitValidationMaxCntCharsFirstLine,
            AppSettings.CommitValidationMaxCntCharsPerLine,
            AppSettings.CommitValidationSecondLineMustBeEmpty,
            AppSettings.CommitValidationIndentAfterFirstLine,
            AppSettings.CommitValidationAutoWrap,
            AppSettings.CommitValidationRegEx);

        public void Restore()
        {
            AppSettings.CommitTemplates = Templates;
            AppSettings.CommitValidationMaxCntCharsFirstLine = MaxFirstLineLength;
            AppSettings.CommitValidationMaxCntCharsPerLine = MaxLineLength;
            AppSettings.CommitValidationSecondLineMustBeEmpty = SecondLineEmpty;
            AppSettings.CommitValidationIndentAfterFirstLine = UseIndent;
            AppSettings.CommitValidationAutoWrap = AutoWrap;
            AppSettings.CommitValidationRegEx = ValidationRegex;
        }
    }
}
