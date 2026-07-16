using System.ComponentModel.Design;
using System.Diagnostics;
using System.Text;
using Avalonia.Controls;
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
public sealed class StashTests
{
    private static readonly ObjectId HeadId = ObjectId.Parse("1111111111111111111111111111111111111111");
    private static readonly ObjectId StashParentId = ObjectId.Parse("2222222222222222222222222222222222222222");
    private static readonly ObjectId StashId = ObjectId.Parse("3333333333333333333333333333333333333333");
    private ServiceContainer _serviceContainer = null!;
    private string _workingDirectory = null!;

    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();

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

        _workingDirectory = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.StashTests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_workingDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        _serviceContainer.Dispose();
        Directory.Delete(_workingDirectory, recursive: true);
    }

    [AvaloniaTest]
    public void FormStash_should_construct_and_use_existing_translation_keys()
    {
        FormStash form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);
        form.TranslateItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormStash), "$this", "Text", "Stash");
        translation.Received(1).AddTranslationItem(nameof(FormStash), "Apply", "Text", "&Apply Selected Stash");
        translation.Received(1).AddTranslationItem(nameof(FormStash), "Clear", "Text", "&Drop Selected Stash");
        translation.Received(1).AddTranslationItem(nameof(FormStash), "Stash", "Text", "S&tash all changes");
        translation.Received(1).AddTranslationItem(nameof(FormStash), "StashKeepIndex", "Text", "&Keep index");
        translation.Received(1).AddTranslationItem(nameof(FormStash), "StashSelectedFiles", "Text", "Stash &selected changes");
        translation.Received(1).AddTranslationItem(nameof(FormStash), "Stashes", "ToolTipText", "Select a stash");
        translation.Received(1).AddTranslationItem(nameof(FormStash), "_currentWorkingDirChanges", "Text", "Current working directory changes");
        translation.Received(1).AddTranslationItem(nameof(FormStash), "_noStashes", "Text", "There are no stashes.");
        translation.Received(1).AddTranslationItem(nameof(FormStash), "chkIncludeUntrackedFiles", "Text", "&Include untracked files");
        translation.Received(1).AddTranslationItem(nameof(FormStash), "messageLabel", "Text", "&Message:");
        translation.Received(1).AddTranslationItem(nameof(FormStash), "showToolStripLabel", "Text", "S&how:");

        string[] emittedKeys = translation.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(ITranslation.AddTranslationItem))
            .Select(call => string.Join('.', call.GetArguments().Take(3)))
            .ToArray();
        emittedKeys.Distinct(StringComparer.Ordinal).Count().Should().Be(
            emittedKeys.Length,
            "each field must be routed through exactly one translation path");
    }

    [AvaloniaTest]
    public async Task FormStash_should_show_worktree_changes_and_stash_selected_files()
    {
        (IGitUICommands commands, IGitModule module) = CreateCommands();
        GitItemStatus staged = new("staged.txt") { IsTracked = true, Staged = StagedStatus.Index };
        GitItemStatus unstaged = new("unstaged.txt") { IsTracked = true, Staged = StagedStatus.WorkTree };
        module.GetStashes(false).Returns([]);
        module.GetAllChangedFiles().Returns([staged, unstaged]);
        module.RevParse("HEAD").Returns(HeadId);

        FormStash form = new(commands);
        form.Show();
        await WaitUntilAsync(() => !form.Loading.IsVisible && form.Stashed.GitItemStatuses.Count == 2);

        form.Stashes.SelectedIndex.Should().Be(0);
        form.StashMessage.IsReadOnly.Should().BeFalse();
        form.Clear.IsEnabled.Should().BeFalse();
        form.Apply.IsEnabled.Should().BeFalse();
        form.StashSelectedFiles.IsEnabled.Should().BeTrue("the first changed file is selected automatically");

        form.StashSelectedFiles.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        commands.Received(1).StashSave(
            form,
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<string>(),
            Arg.Is<IReadOnlyList<string>>(files => files.SequenceEqual(new[] { "unstaged.txt" })));
        form.Close();
    }

    [AvaloniaTest]
    public async Task FormStash_manage_mode_should_select_and_display_the_latest_stash()
    {
        (IGitUICommands commands, IGitModule module) = CreateCommands();
        GitStash stash = new(0, "saved work");
        GitItemStatus item = new("saved.txt") { IsTracked = true };
        module.GetStashes(false).Returns([stash]);
        module.GetStashDiffFiles(stash.Name).Returns([item]);
        module.RevParse(stash.Name + "^").Returns(StashParentId);
        module.RevParse(stash.Name).Returns(StashId);

        FormStash form = new(commands) { ManageStashes = true };
        form.Show();
        await WaitUntilAsync(() => !form.Loading.IsVisible && form.Stashed.GitItemStatuses.Count == 1);

        form.Stashes.SelectedIndex.Should().Be(1);
        form.StashMessage.Text.Should().Be("saved work");
        form.Clear.IsEnabled.Should().BeTrue();
        form.Apply.IsEnabled.Should().BeTrue();
        form.View.TextEditor.Text.Should().Contain("diff --git");
        form.Close();
    }

    [AvaloniaTest]
    public async Task FormStash_should_inspect_a_real_repository_stash()
    {
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet" });
        module.SetSetting("user.name", "Avalonia Test");
        module.SetSetting("user.email", "avalonia@example.com");

        string trackedFile = Path.Combine(_workingDirectory, "tracked.txt");
        File.WriteAllText(trackedFile, "initial line\n");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "tracked.txt" });
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" });
        File.AppendAllText(trackedFile, "stashed line\n");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("stash") { "push", "-m", "saved work".Quote() });

        GitUICommands commands = new(_serviceContainer, module);
        FormStash form = new(commands) { ManageStashes = true };
        try
        {
            form.Show();
            await WaitUntilAsync(() =>
                !form.Loading.IsVisible
                && form.Stashes.SelectedIndex == 1
                && form.Stashed.GitItemStatuses.Count == 1
                && form.View.TextEditor.Text.Contains("stashed line", StringComparison.Ordinal));

            form.StashMessage.Text.Should().Contain("saved work");
            form.Stashed.GitItemStatuses.Should().ContainSingle(item => item.Name == "tracked.txt");
            form.CaptureRenderedFrame().Should().NotBeNull("the real stash-management dialog should render headlessly");
        }
        finally
        {
            form.Close();
        }
    }

    private static (IGitUICommands Commands, IGitModule Module) CreateCommands()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(Path.GetTempPath());
        module.FilesEncoding.Returns(Encoding.UTF8);
        module.GetSingleDiffAsync(
                Arg.Any<ObjectId>(),
                Arg.Any<ObjectId>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string>(),
                Arg.Any<Encoding>(),
                Arg.Any<bool>(),
                Arg.Any<bool>(),
                Arg.Any<bool>(),
                Arg.Any<IGitCommandConfiguration>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<(Patch?, string?)>((
                new Patch(
                    "diff --git a/file b/file",
                    null,
                    PatchFileType.Text,
                    "file",
                    "file",
                    PatchChangeType.ChangeFile,
                    "diff --git a/file b/file\n-old\n+new\n"),
                null)));

        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        return (commands, module);
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (!condition() && stopwatch.Elapsed < TimeSpan.FromSeconds(10))
        {
            Dispatcher.UIThread.RunJobs();
            await Task.Delay(10);
        }

        condition().Should().BeTrue("the stash list and selected diff should load before the timeout");
    }
}
