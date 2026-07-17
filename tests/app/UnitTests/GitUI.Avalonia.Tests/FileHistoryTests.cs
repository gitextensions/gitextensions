using System.ComponentModel.Design;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
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
using GitUI.Editor;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using ResourceManager;

namespace GitExtensionsTests;

[TestFixture]
public sealed class FileHistoryTests
{
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

        _workingDirectory = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.FileHistoryTests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_workingDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        _serviceContainer.Dispose();
        Directory.Delete(_workingDirectory, recursive: true);
    }

    [AvaloniaTest]
    public void FormFileHistory_should_construct()
    {
        FormFileHistory form = new();

        form.FindControl<RevisionGridControl>("RevisionGrid").Should().NotBeNull();
        form.FindControl<TabControl>("tabControl1").Should().NotBeNull();
        form.FindControl<FileViewer>("Diff").Should().NotBeNull();
        form.FindControl<GitUI.Blame.BlameControl>("Blame").Should().NotBeNull();
    }

    [AvaloniaTest]
    public void FormFileHistory_should_use_existing_translation_keys_once()
    {
        FormFileHistory form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);
        form.TranslateItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormFileHistory), "$this", "Text", "File History");
        translation.Received(1).AddTranslationItem(nameof(FormFileHistory), "DiffTab", "Text", "Diff");
        translation.Received(1).AddTranslationItem(nameof(FormFileHistory), "BlameTab", "Text", "Blame");

        string[] emittedKeys = translation.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(ITranslation.AddTranslationItem))
            .Select(call => string.Join('.', call.GetArguments().Take(3)))
            .ToArray();
        emittedKeys.Distinct(StringComparer.Ordinal).Count().Should().Be(emittedKeys.Length);
    }

    [AvaloniaTest]
    public async Task FormFileHistory_should_list_only_the_file_commits_and_show_the_diff()
    {
        GitModule module = CreateRepositoryWithFileHistory();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.GetService(Arg.Any<Type>()).Returns(call => _serviceContainer.GetService(call.Arg<Type>()));

        FormFileHistory form = new(commands, "tracked.txt");
        form.Show();
        try
        {
            RevisionGridControl revisionGrid = form.FindControl<RevisionGridControl>("RevisionGrid")
                ?? throw new InvalidOperationException("Revision grid was not created.");
            TextBlock loadingStatus = revisionGrid.FindControl<TextBlock>("lblLoadingStatus")
                ?? throw new InvalidOperationException("Revision loading status was not created.");
            FileViewer diff = form.FindControl<FileViewer>("Diff")
                ?? throw new InvalidOperationException("Diff viewer was not created.");

            // Three commits exist, but only two touch tracked.txt.
            await WaitUntilAsync(() => loadingStatus.Text == "2 revisions" && revisionGrid.SelectedRevision is not null);

            form.Text.Should().StartWith("File History - tracked.txt");

            await WaitUntilAsync(() => diff.TextEditor.Text.Contains("+second line", StringComparison.Ordinal));

            form.CaptureRenderedFrame().Should().NotBeNull("the file history shell should render headlessly");
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public async Task FormFileHistory_blame_tab_should_show_the_file_with_its_authors()
    {
        GitModule module = CreateRepositoryWithFileHistory();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.GetService(Arg.Any<Type>()).Returns(call => _serviceContainer.GetService(call.Arg<Type>()));

        FormFileHistory form = new(commands, "tracked.txt", showBlame: true);
        form.Show();
        try
        {
            RevisionGridControl revisionGrid = form.FindControl<RevisionGridControl>("RevisionGrid")
                ?? throw new InvalidOperationException("Revision grid was not created.");
            GitUI.Blame.BlameControl blame = form.FindControl<GitUI.Blame.BlameControl>("Blame")
                ?? throw new InvalidOperationException("Blame control was not created.");
            GitUI.CommitInfo.CommitInfo commitInfo = blame.FindControl<GitUI.CommitInfo.CommitInfo>("CommitInfo")
                ?? throw new InvalidOperationException("Commit info was not created.");

            await WaitUntilAsync(() => revisionGrid.SelectedRevision is not null);

            await WaitUntilAsync(() =>
                blame.BlameFile.TextEditor.Text.Contains("second line", StringComparison.Ordinal)
                && commitInfo.Revision is not null);

            GitBlame gitBlame = blame.GetTestAccessor().Blame
                ?? throw new InvalidOperationException("The blame was not stored.");
            gitBlame.Lines.Should().HaveCount(2, "tracked.txt has two lines");
            gitBlame.Lines.Should().OnlyContain(line => line.Commit.Author == "Avalonia Test");

            // The grid revision stays displayed in the commit details after loading.
            commitInfo.Revision!.ObjectId.Should().Be(revisionGrid.SelectedRevision!.ObjectId);

            form.CaptureRenderedFrame().Should().NotBeNull("the blame view should render headlessly");
            blame.BlameAuthor.Bounds.Width.Should().BeGreaterThan(0, "the author margin shows the author lines");
        }
        finally
        {
            form.Close();
        }
    }

    private GitModule CreateRepositoryWithFileHistory()
    {
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet", "-b", "main" }).Should().BeTrue();
        module.SetSetting("user.name", "Avalonia Test");
        module.SetSetting("user.email", "avalonia@example.com");

        string trackedFile = Path.Combine(_workingDirectory, "tracked.txt");
        File.WriteAllText(trackedFile, "initial line\n");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "tracked.txt" }).Should().BeTrue();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" }).Should().BeTrue();

        File.WriteAllText(Path.Combine(_workingDirectory, "other.txt"), "other\n");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "other.txt" }).Should().BeTrue();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "other" }).Should().BeTrue();

        File.AppendAllText(trackedFile, "second line\n");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-am", "second" }).Should().BeTrue();

        return module;
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (!condition() && stopwatch.Elapsed < TimeSpan.FromSeconds(15))
        {
            Dispatcher.UIThread.RunJobs();
            await Task.Delay(10);
        }

        condition().Should().BeTrue("the condition should be met within the timeout");
    }
}
