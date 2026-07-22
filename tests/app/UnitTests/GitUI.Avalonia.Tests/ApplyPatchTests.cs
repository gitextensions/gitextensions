using System.ComponentModel.Design;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Git;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class ApplyPatchTests
{
    private ServiceContainer _serviceContainer = null!;
    private string _workingDirectory = null!;
    private bool _applyPatchIgnoreWhitespace;
    private bool _applyPatchSignOff;
    private bool _closeProcessDialog;

    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();

        _applyPatchIgnoreWhitespace = AppSettings.ApplyPatchIgnoreWhitespace;
        _applyPatchSignOff = AppSettings.ApplyPatchSignOff;
        _closeProcessDialog = AppSettings.CloseProcessDialog;
        AppSettings.CloseProcessDialog = true;

        _serviceContainer = new ServiceContainer();
        GitExtUtils.ServiceContainerRegistry.RegisterServices(_serviceContainer);

        System.IO.Abstractions.FileSystem fileSystem = new();
        GitDirectoryResolver gitDirectoryResolver = new(fileSystem);
        RepositoryDescriptionProvider repositoryDescriptionProvider = new(gitDirectoryResolver);
        _serviceContainer.AddService<System.IO.Abstractions.IFileSystem>(fileSystem);
        _serviceContainer.AddService<IGitDirectoryResolver>(gitDirectoryResolver);
        _serviceContainer.AddService<IRepositoryDescriptionProvider>(repositoryDescriptionProvider);
        GitCommands.ServiceContainerRegistry.RegisterServices(_serviceContainer);
        GitUI.ServiceContainerRegistry.RegisterServices(_serviceContainer);

        _workingDirectory = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.ApplyPatchTests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_workingDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        AppSettings.ApplyPatchIgnoreWhitespace = _applyPatchIgnoreWhitespace;
        AppSettings.ApplyPatchSignOff = _applyPatchSignOff;
        AppSettings.CloseProcessDialog = _closeProcessDialog;
        _serviceContainer.Dispose();
        TestDirectory.Delete(_workingDirectory);
    }

    [AvaloniaTest]
    public void FormApplyPatch_should_construct_and_use_existing_translation_keys()
    {
        FormApplyPatch form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);
        form.TranslateItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormApplyPatch), "$this", "Text", "Apply patch");
        translation.Received(1).AddTranslationItem(nameof(FormApplyPatch), "Abort", "Text", "A&bort patch");
        translation.Received(1).AddTranslationItem(nameof(FormApplyPatch), "Apply", "Text", "Apply patch");
        translation.Received(1).AddTranslationItem(nameof(FormApplyPatch), "PatchDirMode", "Text", "Patch &directory");
        translation.Received(1).AddTranslationItem(nameof(FormApplyPatch), "PatchFileMode", "Text", "Patch &file");
        translation.Received(1).AddTranslationItem(nameof(FormApplyPatch), "Skip", "Text", "S&kip patch");
        translation.Received(1).AddTranslationItem(nameof(FormApplyPatch), "_noFileSelectedText", "Text", "Please select a patch to apply");

        string[] emittedKeys = translation.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(ITranslation.AddTranslationItem))
            .Select(call => call.GetArguments())
            .Where(arguments => Equals(arguments[0], nameof(FormApplyPatch)))
            .Select(arguments => string.Join('.', arguments.Take(3)))
            .ToArray();
        emittedKeys.Distinct(StringComparer.Ordinal).Count().Should().Be(
            emittedKeys.Length,
            "each FormApplyPatch field must be routed through exactly one translation path");
    }

    [AvaloniaTest]
    public void FormApplyPatch_should_switch_between_file_and_directory_sources()
    {
        GitModule module = CreateRepository();
        GitUICommands commands = new(_serviceContainer, module);
        FormApplyPatch form = new(commands);
        form.SetPatchDir(_workingDirectory);

        form.Show();

        form.PatchDirMode.IsChecked.Should().BeTrue();
        form.PatchDir.Text.Should().Be(_workingDirectory);
        form.PatchDir.IsEnabled.Should().BeTrue();
        form.BrowseDir.IsEnabled.Should().BeTrue();
        form.PatchFile.IsEnabled.Should().BeFalse();
        form.Apply.IsEnabled.Should().BeTrue();
        form.CaptureRenderedFrame().Should().NotBeNull();
        form.Close();
    }

    [AvaloniaTest]
    public void FormApplyPatch_should_apply_a_real_raw_diff()
    {
        GitModule module = CreateRepository();
        string trackedFile = Path.Combine(_workingDirectory, "tracked.txt");
        File.WriteAllText(trackedFile, "patched line\n");
        string patch = module.GitExecutable.GetOutput(new GitArgumentBuilder("diff"));
        string patchFile = Path.Combine(_workingDirectory, "change.patch");
        File.WriteAllText(patchFile, patch);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("checkout") { "--", "tracked.txt" }).Should().BeTrue();

        GitUICommands commands = new(_serviceContainer, module);
        FormApplyPatch form = new(commands);
        form.SetPatchFile(patchFile);
        form.Show();

        form.Apply.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));

        File.ReadAllText(trackedFile).Should().Be("patched line\n");
        module.GitExecutable.GetOutput(new GitArgumentBuilder("diff")).Should().Contain("+patched line");
        form.IsVisible.Should().BeFalse("a completed patch application closes the dialog like WinForms");
    }

    [AvaloniaTest]
    public void FormApplyPatch_should_control_and_abort_a_real_conflicted_mailbox_patch()
    {
        GitModule module = CreateConflictedMailboxPatch();
        module.InTheMiddleOfPatch().Should().BeTrue();
        module.InTheMiddleOfConflictedMerge().Should().BeTrue();

        GitUICommands commands = new(_serviceContainer, module);
        FormApplyPatch form = new(commands);
        form.Show();

        form.Apply.IsEnabled.Should().BeFalse();
        form.PatchFileMode.IsEnabled.Should().BeFalse();
        form.AddFiles.IsEnabled.Should().BeTrue();
        form.Mergetool.IsEnabled.Should().BeTrue();
        form.Resolved.IsEnabled.Should().BeFalse();
        form.Skip.IsEnabled.Should().BeTrue();
        form.Abort.IsEnabled.Should().BeTrue();
        form.SolveMergeConflicts.IsVisible.Should().BeTrue();
        form.PatchGrid.PatchFiles.Should().ContainSingle(patchFile => patchFile.IsNext);
        form.CaptureRenderedFrame().Should().NotBeNull("the conflicted patch controller should render headlessly");

        form.Abort.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));

        module.InTheMiddleOfPatch().Should().BeFalse();
        module.InTheMiddleOfConflictedMerge().Should().BeFalse();
        File.ReadAllText(Path.Combine(_workingDirectory, "tracked.txt")).Should().Be("target line\n");
        form.Apply.IsEnabled.Should().BeTrue();
        form.Close();
    }

    private GitModule CreateRepository()
    {
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet", "-b", "main" }).Should().BeTrue();
        module.SetSetting("user.name", "Avalonia Test");
        module.SetSetting("user.email", "avalonia@example.com");
        module.SetSetting("core.autocrlf", "false");
        File.WriteAllText(Path.Combine(_workingDirectory, "tracked.txt"), "initial line\n");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "tracked.txt" }).Should().BeTrue();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" }).Should().BeTrue();
        return module;
    }

    private GitModule CreateConflictedMailboxPatch()
    {
        GitModule module = CreateRepository();
        string trackedFile = Path.Combine(_workingDirectory, "tracked.txt");

        module.GitExecutable.RunCommand(new GitArgumentBuilder("checkout") { "--quiet", "-b", "patch-source" }).Should().BeTrue();
        File.WriteAllText(trackedFile, "patch line\n");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-am", "patch change".Quote() }).Should().BeTrue();
        string mailboxPatch = module.GitExecutable.GetOutput(new GitArgumentBuilder("format-patch") { "-1", "--stdout" });
        string patchFile = Path.Combine(_workingDirectory, "mailbox.patch");
        File.WriteAllText(patchFile, mailboxPatch);

        module.GitExecutable.RunCommand(new GitArgumentBuilder("checkout") { "--quiet", "main" }).Should().BeTrue();
        File.WriteAllText(trackedFile, "target line\n");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-am", "target change".Quote() }).Should().BeTrue();

        ExecutionResult result = module.GitExecutable.Execute(
            new GitArgumentBuilder("am") { "--3way", module.GetPathForGitExecution(patchFile).Quote() },
            throwOnErrorExit: false);
        result.ExitCode.Should().NotBe(0);
        return module;
    }
}
