using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.GitIgnoreDialog;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
public sealed class IgnoreAttributesMailmapTests
{
    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
    }

    [AvaloniaTest]
    public void FormAddToGitIgnore_should_construct_and_preview_matches()
    {
        FormAddToGitIgnore form = new();
        FormAddToGitIgnore.TestAccessor accessor = form.GetTestAccessor();

        accessor.FilePattern.Should().NotBeNull();
        accessor.Preview.Should().NotBeNull();

        accessor.UpdatePreview(["a.txt", "b.txt"]);
        accessor.Preview.ItemCount.Should().Be(2);
        accessor.NoMatchPanel.IsVisible.Should().BeFalse();

        accessor.UpdatePreview([]);
        accessor.NoMatchPanel.IsVisible.Should().BeTrue();
    }

    [AvaloniaTest]
    public void FormAddToGitIgnore_should_emit_its_translation_keys()
    {
        FormAddToGitIgnore form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormAddToGitIgnore), "$this", "Text", "Add file(s) to .gitignore");
        translation.Received(1).AddTranslationItem(nameof(FormAddToGitIgnore), "AddToIgnore", "Text", "Ignore");
        translation.Received(1).AddTranslationItem(nameof(FormAddToGitIgnore), "btnCancel", "Text", "Cancel");
        translation.Received(1).AddTranslationItem(nameof(FormAddToGitIgnore), "groupBox1", "Text", "Preview");
        translation.Received(1).AddTranslationItem(nameof(FormAddToGitIgnore), "groupFilePattern", "Text", "Enter a file pattern to ignore:");
        translation.Received(1).AddTranslationItem(nameof(FormAddToGitIgnore), "label2", "Text", "No existing files match that pattern.");
    }

    [AvaloniaTest]
    public void FormGitIgnore_should_construct_with_the_original_controls()
    {
        FormGitIgnore form = new();
        FormGitIgnore.TestAccessor accessor = form.GetTestAccessor();

        accessor.Editor.Should().NotBeNull();
        accessor.Save.Should().NotBeNull();
        accessor.AddDefault.Should().NotBeNull();
        accessor.AddPattern.Should().NotBeNull();
        accessor.Cancel.Should().NotBeNull();
    }

    [AvaloniaTest]
    public void FormGitIgnore_should_emit_its_translation_keys()
    {
        FormGitIgnore form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormGitIgnore), "$this", "Text", "Edit .gitignore");
        translation.Received(1).AddTranslationItem(nameof(FormGitIgnore), "Save", "Text", "Save");
        translation.Received(1).AddTranslationItem(nameof(FormGitIgnore), "AddDefault", "Text", "Add default ignores");
        translation.Received(1).AddTranslationItem(nameof(FormGitIgnore), "AddPattern", "Text", "Add pattern");
        translation.Received(1).AddTranslationItem(nameof(FormGitIgnore), "btnCancel", "Text", "Cancel");
        translation.Received(1).AddTranslationItem(nameof(FormGitIgnore), "lnkGitIgnorePatterns", "Text", "Example ignore patterns");
        translation.Received(1).AddTranslationItem(nameof(FormGitIgnore), "lnkGitIgnoreGenerate", "Text", "Generate a custom ignore file for git");
    }

    [AvaloniaTest]
    public void FormGitAttributes_should_construct_and_emit_keys()
    {
        FormGitAttributes form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.GetTestAccessor().Editor.Should().NotBeNull();
        form.GetTestAccessor().Save.Should().NotBeNull();

        form.AddTranslationItems(translation);
        translation.Received(1).AddTranslationItem(nameof(FormGitAttributes), "$this", "Text", "Edit .gitattributes");
        translation.Received(1).AddTranslationItem(nameof(FormGitAttributes), "Save", "Text", "Save");
    }

    [AvaloniaTest]
    public void FormMailMap_should_construct_and_emit_keys()
    {
        FormMailMap form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.GetTestAccessor().Editor.Should().NotBeNull();
        form.GetTestAccessor().Save.Should().NotBeNull();

        form.AddTranslationItems(translation);
        translation.Received(1).AddTranslationItem(nameof(FormMailMap), "$this", "Text", "Edit .mailmap");
        translation.Received(1).AddTranslationItem(nameof(FormMailMap), "Save", "Text", "Save");
    }

    [AvaloniaTest]
    public void GitIgnoreModel_should_describe_the_gitignore_file()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(Path.Combine(Path.GetTempPath(), "ge-ignore-model"));

        GitIgnoreModel model = new(module);

        model.FormCaption.Should().Be("Edit .gitignore");
        model.ExcludeFile.Should().EndWith(".gitignore");
        model.SaveFileQuestion.Should().Be("Save changes to .gitignore?");
    }

    [AvaloniaTest]
    public void GitLocalExcludeModel_should_describe_the_local_exclude_file()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.ResolveGitInternalPath("info").Returns(Path.Combine(Path.GetTempPath(), "ge-exclude", ".git", "info"));

        GitLocalExcludeModel model = new(module);

        model.FormCaption.Should().Be("Edit .git/info/exclude");
        model.ExcludeFile.Should().EndWith("exclude");
        model.SaveFileQuestion.Should().Be("Save changes to .git/info/exclude?");
    }

    [AvaloniaTest]
    public void FormMailMap_should_construct_with_ui_commands()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(Path.Combine(Path.GetTempPath(), "ge-mailmap"));
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);

        FormMailMap form = new(commands);

        form.GetTestAccessor().Editor.Should().NotBeNull();

        Dispatcher.UIThread.RunJobs();
    }
}
