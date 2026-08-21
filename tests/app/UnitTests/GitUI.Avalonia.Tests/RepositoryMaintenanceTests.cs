using Avalonia.Controls;
using Avalonia.Controls.Primitives;
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
    public void FormCleanupRepository_should_use_the_native_96_dpi_WinForms_layout()
    {
        FormCleanupRepository form = new();
        form.Show();
        Dispatcher.UIThread.RunJobs();

        form.ClientSize.Should().Be(new Avalonia.Size(434, 582));
        AssertBounds(form.FindControl<HeaderedContentControl>("groupBox1"), 12, 12, 410, 100);
        AssertBounds(form.FindControl<CheckBox>("RemoveDirectories"), 19, 118, 183, 19);
        AssertBounds(form.FindControl<CheckBox>("CleanSubmodules"), 19, 142, 183, 19);
        AssertBounds(form.FindControl<CheckBox>("checkBoxIncludePathFilter"), 19, 167, 250, 19);
        AssertBounds(form.FindControl<Button>("AddInclusivePath"), 302, 162, 120, 25);
        AssertBounds(form.FindControl<TextBox>("textBoxIncludePaths"), 48, 192, 374, 63);
        AssertBounds(form.FindControl<TextBlock>("labelPathHintInclude"), 50, 258, 104, 15);
        AssertBounds(form.FindControl<CheckBox>("checkBoxExcludePathFilter"), 19, 288, 201, 19);
        AssertBounds(form.FindControl<Button>("AddExclusivePath"), 302, 283, 120, 25);
        AssertBounds(form.FindControl<TextBox>("textBoxExcludePaths"), 48, 313, 374, 63);
        AssertBounds(form.FindControl<TextBlock>("labelPathHintExclude"), 50, 380, 104, 15);
        AssertBounds(form.FindControl<Button>("Preview"), 48, 423, 120, 25);
        AssertBounds(form.FindControl<Button>("Cleanup"), 174, 423, 120, 25);
        AssertBounds(form.FindControl<Button>("_NO_TRANSLATE_Close"), 301, 423, 120, 25);
        AssertBounds(form.FindControl<TextBlock>("label1"), 14, 458, 30, 15);
        AssertBounds(form.FindControl<TextBox>("PreviewOutput"), 14, 478, 410, 94);

        form.Close();
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
    public void FormBisect_should_use_the_native_96_dpi_WinForms_layout()
    {
        FormBisect form = new();
        form.Show();
        Dispatcher.UIThread.RunJobs();

        form.ClientSize.Should().Be(new Avalonia.Size(248, 169));
        AssertBounds(form.FindControl<Button>("Start"), 12, 12, 224, 25);
        AssertBounds(form.FindControl<Button>("Bad"), 12, 41, 224, 25);
        AssertBounds(form.FindControl<Button>("Good"), 12, 70, 224, 25);
        AssertBounds(form.FindControl<Button>("btnSkip"), 12, 101, 224, 25);
        AssertBounds(form.FindControl<Button>("Stop"), 12, 132, 224, 25);

        form.Close();
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
    public void FormSparseWorkingCopy_should_use_the_WinForms_native_client_minimum_and_load_rules()
    {
        string root = Path.Combine(Path.GetTempPath(), $"ge-sparse-{Guid.NewGuid():N}");
        string info = Path.Combine(root, ".git", "info");
        Directory.CreateDirectory(info);
        File.WriteAllText(Path.Combine(info, "sparse-checkout"), "/*");
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(root);
        module.ResolveGitInternalPath("info").Returns(info);
        module.GetEffectiveSetting(FormSparseWorkingCopyViewModel.SettingCoreSparseCheckout).Returns("true");
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);

        FormSparseWorkingCopy form = new(commands);
        form.Show();
        Dispatcher.UIThread.RunJobs();

        form.MinWidth.Should().Be(784);
        form.MinHeight.Should().Be(561);
        form.GetTestAccessor().Editor.Should().NotBeNull();

        form.Close();
        Directory.Delete(root, recursive: true);
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

    [Test]
    public void FormSparseWorkingCopyViewModel_should_adjust_rules_when_disabling_sparse_checkout()
    {
        string root = Path.Combine(Path.GetTempPath(), $"ge-sparse-vm-{Guid.NewGuid():N}");
        string info = Path.Combine(root, ".git", "info");
        Directory.CreateDirectory(info);
        string sparseFile = Path.Combine(info, "sparse-checkout");
        File.WriteAllText(sparseFile, "/src\n#existing");
        IGitModule module = Substitute.For<IGitModule>();
        module.ResolveGitInternalPath("info").Returns(info);
        module.GetEffectiveSetting(FormSparseWorkingCopyViewModel.SettingCoreSparseCheckout).Returns("true");
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        FormSparseWorkingCopyViewModel model = new(commands)
        {
            IsRefreshWorkingCopyOnSave = false,
            RulesText = "/src\n#existing",
        };
        model.SetRulesTextAsOnDisk(model.RulesText);
        bool confirmationRaised = false;
        model.ComfirmAdjustingRulesOnDeactRequested += (sender, args) =>
        {
            confirmationRaised = true;
            args.IsCurrentRuleSetEmpty.Should().BeFalse();
        };

        model.IsSparseCheckoutEnabled = false;
        model.SaveChanges();

        confirmationRaised.Should().BeTrue();
        File.ReadAllText(sparseFile).Should().Be($"/*{Environment.NewLine}#/src{Environment.NewLine}#existing");
        module.Received(1).SetSetting(FormSparseWorkingCopyViewModel.SettingCoreSparseCheckout, "false");
        model.IsWithUnsavedChanges().Should().BeFalse();
        Directory.Delete(root, recursive: true);
    }

    private static void AssertBounds(Control? control, double x, double y, double width, double height)
    {
        control.Should().NotBeNull();
        control!.Bounds.Should().Be(new Avalonia.Rect(x, y, width, height));
    }
}
