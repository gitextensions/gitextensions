using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
public sealed class RepositoryMaintenanceTests
{
    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
    }

    [AvaloniaTest]
    public void FormCleanupRepository_should_construct_with_the_original_controls()
    {
        FormCleanupRepository form = new();
        FormCleanupRepository.TestAccessor accessor = form.GetTestAccessor();

        accessor.RemoveAll.Should().NotBeNull();
        accessor.RemoveNonIgnored.Should().NotBeNull();
        accessor.RemoveIgnored.Should().NotBeNull();
        accessor.RemoveDirectories.Should().NotBeNull();
        accessor.CleanSubmodules.Should().NotBeNull();
        accessor.IncludePathFilter.Should().NotBeNull();
        accessor.ExcludePathFilter.Should().NotBeNull();
        accessor.PreviewOutput.Should().NotBeNull();

        // RemoveAll is checked by default, mirroring the original.
        accessor.RemoveAll.IsChecked.Should().BeTrue();
    }

    [AvaloniaTest]
    public void FormCleanupRepository_should_emit_its_translation_keys()
    {
        FormCleanupRepository form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormCleanupRepository), "$this", "Text", "Clean working directory");
        translation.Received(1).AddTranslationItem(nameof(FormCleanupRepository), "groupBox1", "Text", "Remove untracked files from working directory");
        translation.Received(1).AddTranslationItem(nameof(FormCleanupRepository), "RemoveAll", "Text", "Remove all untracked files");
        translation.Received(1).AddTranslationItem(nameof(FormCleanupRepository), "RemoveNonIgnored", "Text", "Remove only non-ignored untracked files");
        translation.Received(1).AddTranslationItem(nameof(FormCleanupRepository), "RemoveIgnored", "Text", "Remove only ignored untracked files");
        translation.Received(1).AddTranslationItem(nameof(FormCleanupRepository), "RemoveDirectories", "Text", "Remove untracked directories");
        translation.Received(1).AddTranslationItem(nameof(FormCleanupRepository), "CleanSubmodules", "Text", "Clean submodules recursively");
        translation.Received(1).AddTranslationItem(nameof(FormCleanupRepository), "checkBoxIncludePathFilter", "Text", "Affect the following directory path(s) only:");
        translation.Received(1).AddTranslationItem(nameof(FormCleanupRepository), "checkBoxExcludePathFilter", "Text", "Exclude the following file path(s):");
        translation.Received(1).AddTranslationItem(nameof(FormCleanupRepository), "label1", "Text", "Log:");
        translation.Received(1).AddTranslationItem(nameof(FormCleanupRepository), "Preview", "Text", "Preview");
        translation.Received(1).AddTranslationItem(nameof(FormCleanupRepository), "Cleanup", "Text", "Cleanup");
    }

    [AvaloniaTest]
    public void FormCleanupRepository_should_map_radio_selection_to_clean_mode()
    {
        FormCleanupRepository form = new();
        FormCleanupRepository.TestAccessor accessor = form.GetTestAccessor();

        accessor.RemoveAll.IsChecked = true;
        accessor.RemoveNonIgnored.IsChecked = false;
        accessor.RemoveIgnored.IsChecked = false;
        accessor.GetCleanMode().Should().Be(CleanMode.All);

        accessor.RemoveAll.IsChecked = false;
        accessor.RemoveNonIgnored.IsChecked = true;
        accessor.GetCleanMode().Should().Be(CleanMode.OnlyNonIgnored);

        accessor.RemoveNonIgnored.IsChecked = false;
        accessor.RemoveIgnored.IsChecked = true;
        accessor.GetCleanMode().Should().Be(CleanMode.OnlyIgnored);
    }

    [AvaloniaTest]
    public void FormCleanupRepository_should_build_path_arguments_from_the_text_boxes()
    {
        FormCleanupRepository form = new();
        FormCleanupRepository.TestAccessor accessor = form.GetTestAccessor();

        accessor.GetInclusivePathArgumentFromGui().Should().BeNull();
        accessor.GetExclusivePathArgumentFromGui().Should().BeNull();

        accessor.IncludePathFilter.IsChecked = true;
        accessor.IncludePaths.Text = "src\r\ndocs\r\n";
        accessor.GetInclusivePathArgumentFromGui().Should().Be("\"src\" \"docs\"");

        accessor.ExcludePathFilter.IsChecked = true;
        accessor.ExcludePaths.Text = "a b.txt";
        accessor.GetExclusivePathArgumentFromGui().Should().Be("--exclude=a?b.txt");
    }

    [AvaloniaTest]
    public void FormBisect_should_construct_with_the_original_buttons()
    {
        FormBisect form = new();
        FormBisect.TestAccessor accessor = form.GetTestAccessor();

        accessor.Start.Should().NotBeNull();
        accessor.Good.Should().NotBeNull();
        accessor.Bad.Should().NotBeNull();
        accessor.Stop.Should().NotBeNull();
        accessor.btnSkip.Should().NotBeNull();
    }

    [AvaloniaTest]
    public void FormBisect_should_emit_its_translation_keys()
    {
        FormBisect form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormBisect), "$this", "Text", "Bisect");
        translation.Received(1).AddTranslationItem(nameof(FormBisect), "Start", "Text", "Start bisect");
        translation.Received(1).AddTranslationItem(nameof(FormBisect), "Bad", "Text", "Mark current revision &bad");
        translation.Received(1).AddTranslationItem(nameof(FormBisect), "Good", "Text", "Mark current revision &good");
        translation.Received(1).AddTranslationItem(nameof(FormBisect), "Stop", "Text", "Stop bisect");
        translation.Received(1).AddTranslationItem(nameof(FormBisect), "btnSkip", "Text", "&Skip current revision");
    }

    [AvaloniaTest]
    public void FormSparseWorkingCopy_should_construct_with_ui_commands()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(Path.Combine(Path.GetTempPath(), "ge-sparse"));
        module.ResolveGitInternalPath("info").Returns(Path.Combine(Path.GetTempPath(), "ge-sparse", ".git", "info"));
        module.GetEffectiveSetting(FormSparseWorkingCopyViewModel.SettingCoreSparseCheckout).Returns((string?)null);
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);

        FormSparseWorkingCopy form = new(commands);

        form.GetTestAccessor().Root.Should().NotBeNull();

        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaTest]
    public void FormSparseWorkingCopy_should_not_emit_translation_keys_of_its_own()
    {
        FormSparseWorkingCopy form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);

        // The form self-translates through the nested Globalized helper (XLF category "Globalized"),
        // so the form itself contributes no keys under a "FormSparseWorkingCopy" category.
        translation.DidNotReceiveWithAnyArgs().AddTranslationItem(default!, default!, default!, default!);
    }

    [Test]
    public void FormSparseWorkingCopyViewModel_should_describe_the_sparse_checkout_file()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.ResolveGitInternalPath("info").Returns(Path.Combine(Path.GetTempPath(), "ge-sparse-vm", ".git", "info"));
        module.GetEffectiveSetting(FormSparseWorkingCopyViewModel.SettingCoreSparseCheckout).Returns((string?)null);
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);

        FormSparseWorkingCopyViewModel model = new(commands);

        model.GetPathToSparseCheckoutFile().FullName.Should().EndWith("sparse-checkout");
        model.IsSparseCheckoutEnabled.Should().BeFalse();
    }

    [Test]
    public void FormSparseWorkingCopyViewModel_should_track_unsaved_changes()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.ResolveGitInternalPath("info").Returns(Path.Combine(Path.GetTempPath(), "ge-sparse-vm2", ".git", "info"));
        module.GetEffectiveSetting(FormSparseWorkingCopyViewModel.SettingCoreSparseCheckout).Returns("true");
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);

        FormSparseWorkingCopyViewModel model = new(commands);

        model.IsSparseCheckoutEnabled.Should().BeTrue();
        model.IsWithUnsavedChanges().Should().BeFalse();

        model.SetRulesTextAsOnDisk("/*");
        model.RulesText = "/*";
        model.IsRulesTextChanged.Should().BeFalse();
        model.IsWithUnsavedChanges().Should().BeFalse();

        model.RulesText = "/docs";
        model.IsRulesTextChanged.Should().BeTrue();
        model.IsWithUnsavedChanges().Should().BeTrue();

        model.RulesText = "/*";
        model.IsWithUnsavedChanges().Should().BeFalse();

        model.IsSparseCheckoutEnabled = false;
        model.IsWithUnsavedChanges().Should().BeTrue();
    }
}
