using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitUI;
using GitUI.CommandsDialogs.CommitDialog;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
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
}
