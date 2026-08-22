using Avalonia;
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
        FormCommitTemplateSettings.TestAccessor accessor = form.GetTestAccessor();

        accessor.CommitTemplates.Should().NotBeNull();
        accessor.TemplateName.Should().NotBeNull();
        accessor.TemplateText.Should().NotBeNull();
        accessor.RegexEnabled.Should().NotBeNull();
        accessor.AutoWrap.Should().NotBeNull();
        accessor.MaxFirstLineLength.Should().NotBeNull();
        accessor.Ok.Should().NotBeNull();
        accessor.Cancel.Should().NotBeNull();
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
        translation.Received(1).AddTranslationItem(nameof(FormCommitTemplateSettings), "checkBoxRegexEnabled", "toolTipRegex", Arg.Any<string>());
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
        FormCommitTemplateSettings.TestAccessor accessor = form.GetTestAccessor();

        accessor.CommitTemplates.ItemCount.Should().Be(10);
        accessor.CommitTemplates.SelectedIndex.Should().Be(0);

        // The template-name box keeps the original 80-character cap.
        accessor.TemplateName.MaxLength.Should().Be(80);

        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaTest]
    public void FormCommitTemplateSettings_should_match_native_96_dpi_designer_geometry()
    {
        FormCommitTemplateSettings form = new(CreateCommands());
        FormCommitTemplateSettings.TestAccessor accessor = form.GetTestAccessor();
        form.Show();
        Dispatcher.UIThread.RunJobs();

        form.ClientSize.Should().Be(new Size(698, 361));
        accessor.ControlsPanel.Bounds.Should().Be(new Rect(0, 320, 698, 41));
        accessor.Ok.Bounds.Should().Be(new Rect(529, 8, 75, 25));
        accessor.Cancel.Bounds.Should().Be(new Rect(610, 8, 75, 25));
        accessor.Tabs.Bounds.Should().Be(new Rect(9, 9, 680, 302));
        accessor.TemplateLayout.Bounds.Should().Be(new Rect(3, 3, 666, 262));
        accessor.CommitTemplates.Bounds.Should().Be(new Rect(3, 3, 660, 23));
        accessor.TemplateName.Bounds.Should().Be(new Rect(113, 32, 550, 23));
        accessor.TemplateText.Bounds.Should().Be(new Rect(113, 61, 550, 198));
        accessor.RegexEnabled.Bounds.Should().Be(new Rect(3, 243, 92, 19));

        accessor.Tabs.SelectedItem = accessor.ValidationTab;
        Dispatcher.UIThread.RunJobs();

        accessor.ValidationLayout.Bounds.Should().Be(new Rect(3, 3, 666, 262));
        accessor.MaxFirstLineLength.Bounds.Should().Be(new Rect(377, 3, 60, 23));
        accessor.MaxLineLength.Bounds.Should().Be(new Rect(377, 32, 60, 23));
        accessor.AutoWrap.Bounds.Should().Be(new Rect(377, 65, 15, 14));
        accessor.ValidationRegex.Bounds.Should().Be(new Rect(377, 89, 286, 23));
        accessor.UseIndent.Bounds.Should().Be(new Rect(377, 122, 15, 14));
        accessor.SecondLineEmpty.Bounds.Should().Be(new Rect(377, 150, 15, 14));
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
            FormCommitTemplateSettings.TestAccessor accessor = form.GetTestAccessor();
            form.Show();
            Dispatcher.UIThread.RunJobs();

            accessor.Templates.Should().HaveCount(10);
            accessor.Templates[0].Name.Should().Be("Release");
            accessor.TemplateName.Text.Should().Be("Release");
            accessor.TemplateText.Text.Should().Be("release text");
            accessor.RegexEnabled.IsChecked.Should().BeTrue();

            accessor.CommitTemplates.SelectedItem = accessor.CommitTemplates.Items[1];
            Dispatcher.UIThread.RunJobs();
            accessor.CommitTemplates.SelectedIndex.Should().Be(1);
            string longName = new('x', 60);
            accessor.TemplateName.Text = longName;
            accessor.TemplateName.RaiseEvent(new Avalonia.Controls.TextChangedEventArgs(Avalonia.Controls.TextBox.TextChangedEvent));
            accessor.TemplateText.Text = "second text";
            accessor.TemplateText.RaiseEvent(new Avalonia.Controls.TextChangedEventArgs(Avalonia.Controls.TextBox.TextChangedEvent));
            accessor.RegexEnabled.IsChecked = true;
            Dispatcher.UIThread.RunJobs();

            accessor.Templates[0].Name.Should().Be("Release");
            accessor.Templates[1].Name.Should().Be(longName);
            accessor.Templates[1].Text.Should().Be("second text");
            accessor.Templates[1].IsRegex.Should().BeTrue();
            accessor.CommitTemplates.Items[1].Should().Be($"2 : {new string('x', 47)}...");
            form.Close();
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
            FormCommitTemplateSettings.TestAccessor accessor = form.GetTestAccessor();
            accessor.TemplateName.Text = "Feature";
            accessor.TemplateText.Text = "feature text";
            accessor.RegexEnabled.IsChecked = true;
            accessor.MaxFirstLineLength.Value = 52;
            accessor.MaxLineLength.Value = 72;
            accessor.ValidationRegex.Text = "^[A-Z]+-[0-9]+";
            accessor.AutoWrap.IsChecked = false;
            accessor.UseIndent.IsChecked = false;
            accessor.SecondLineEmpty.IsChecked = true;
            Dispatcher.UIThread.RunJobs();

            accessor.SaveSettings();

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
            form.Close();
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
            FormCommitTemplateSettings.TestAccessor accessor = form.GetTestAccessor();
            form.Show();
            accessor.TemplateName.Text = "unsaved";
            accessor.MaxFirstLineLength.Value = 99;

            accessor.Cancel.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));

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
