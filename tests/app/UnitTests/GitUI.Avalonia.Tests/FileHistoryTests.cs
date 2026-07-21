using System.ComponentModel.Design;
using System.Diagnostics;
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
using GitUI.Editor;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using ResourceManager;

namespace GitExtensionsTests;

[TestFixture]
public sealed class FileHistoryTests
{
    private ServiceContainer _serviceContainer = null!;
    private string _workingDirectory = null!;
    private bool _detectCopyInAllOnBlame;
    private bool _detectCopyInFileOnBlame;
    private bool _displayAuthorFirst;
    private bool _followRenames;
    private bool _followRenamesExactOnly;
    private bool _fullHistory;
    private bool _ignoreWhitespaceOnBlame;
    private bool _loadBlameOnShow;
    private bool _loadHistoryOnShow;
    private bool _showAuthor;
    private bool _showAuthorAvatar;
    private bool _showAuthorDate;
    private bool _showAuthorTime;
    private bool _showLineNumbers;
    private bool _showOriginalFilePath;
    private bool _simplifyMerges;

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

        _detectCopyInAllOnBlame = AppSettings.DetectCopyInAllOnBlame;
        _detectCopyInFileOnBlame = AppSettings.DetectCopyInFileOnBlame;
        _displayAuthorFirst = AppSettings.BlameDisplayAuthorFirst;
        _followRenames = AppSettings.FollowRenamesInFileHistory;
        _followRenamesExactOnly = AppSettings.FollowRenamesInFileHistoryExactOnly;
        _fullHistory = AppSettings.FullHistoryInFileHistory;
        _ignoreWhitespaceOnBlame = AppSettings.IgnoreWhitespaceOnBlame;
        _loadBlameOnShow = AppSettings.LoadBlameOnShow;
        _loadHistoryOnShow = AppSettings.LoadFileHistoryOnShow;
        _showAuthor = AppSettings.BlameShowAuthor;
        _showAuthorAvatar = AppSettings.BlameShowAuthorAvatar;
        _showAuthorDate = AppSettings.BlameShowAuthorDate;
        _showAuthorTime = AppSettings.BlameShowAuthorTime;
        _showLineNumbers = AppSettings.BlameShowLineNumbers;
        _showOriginalFilePath = AppSettings.BlameShowOriginalFilePath;
        _simplifyMerges = AppSettings.SimplifyMergesInFileHistory;

        AppSettings.FollowRenamesInFileHistory = true;
        AppSettings.LoadBlameOnShow = true;
        AppSettings.LoadFileHistoryOnShow = true;
    }

    [TearDown]
    public void TearDown()
    {
        AppSettings.DetectCopyInAllOnBlame = _detectCopyInAllOnBlame;
        AppSettings.DetectCopyInFileOnBlame = _detectCopyInFileOnBlame;
        AppSettings.BlameDisplayAuthorFirst = _displayAuthorFirst;
        AppSettings.FollowRenamesInFileHistory = _followRenames;
        AppSettings.FollowRenamesInFileHistoryExactOnly = _followRenamesExactOnly;
        AppSettings.FullHistoryInFileHistory = _fullHistory;
        AppSettings.IgnoreWhitespaceOnBlame = _ignoreWhitespaceOnBlame;
        AppSettings.LoadBlameOnShow = _loadBlameOnShow;
        AppSettings.LoadFileHistoryOnShow = _loadHistoryOnShow;
        AppSettings.BlameShowAuthor = _showAuthor;
        AppSettings.BlameShowAuthorAvatar = _showAuthorAvatar;
        AppSettings.BlameShowAuthorDate = _showAuthorDate;
        AppSettings.BlameShowAuthorTime = _showAuthorTime;
        AppSettings.BlameShowLineNumbers = _showLineNumbers;
        AppSettings.BlameShowOriginalFilePath = _showOriginalFilePath;
        AppSettings.SimplifyMergesInFileHistory = _simplifyMerges;
        _serviceContainer.Dispose();
        TestDirectory.Delete(_workingDirectory);
    }

    [AvaloniaTest]
    public void FormFileHistory_should_construct()
    {
        FormFileHistory form = new();

        form.FindControl<RevisionGridControl>("RevisionGrid").Should().NotBeNull();
        form.FindControl<TabControl>("tabControl1").Should().NotBeNull();
        form.FindControl<GitUI.UserControls.FilterToolBar>("ToolStripFilters").Should().NotBeNull();
        form.FindControl<GitUI.UserControls.CommitDiff>("CommitDiff").Should().NotBeNull();
        form.FindControl<FileViewer>("Diff").Should().NotBeNull();
        form.FindControl<FileViewer>("View").Should().NotBeNull();
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
        translation.Received(1).AddTranslationItem(nameof(FormFileHistory), "openWithDifftoolToolStripMenuItem", "Text", "Open with difftool");
        translation.Received(1).AddTranslationItem(nameof(FormFileHistory), "diffToolRemoteLocalStripMenuItem", "Text", "Difftool selected < - > local");
        translation.Received(1).AddTranslationItem(nameof(FormFileHistory), "saveAsToolStripMenuItem", "Text", "Save as");
        translation.Received(1).AddTranslationItem(nameof(FormFileHistory), "followFileHistoryToolStripMenuItem", "Text", "Detect and follow renames");
        translation.Received(1).AddTranslationItem(nameof(FormFileHistory), "showAuthorAvatarToolStripMenuItem", "Text", "Show author avatar");
        translation.Received(1).AddTranslationItem(nameof(FormFileHistory), "showFullHistoryToolStripMenuItem", "Text", "Show full history");
        translation.Received(1).AddTranslationItem(nameof(FormFileHistory), "toolStripSplitLoad", "ToolTipText", "Load file history");
        translation.Received(1).AddTranslationItem(nameof(FormFileHistory), "ShowFullHistory", "ToolTipText", "Show Full History");
        translation.Received(1).AddTranslationItem(nameof(FormFileHistory), "CommitInfoTabPage", "Text", "Commit");
        translation.Received(1).AddTranslationItem(nameof(FormFileHistory), "DiffTab", "Text", "Diff");
        translation.Received(1).AddTranslationItem(nameof(FormFileHistory), "ViewTab", "Text", "View");
        translation.Received(1).AddTranslationItem(nameof(FormFileHistory), "BlameTab", "Text", "Blame");
        translation.DidNotReceive().AddTranslationItem(nameof(FormFileHistory), "ShowFullHistory", "toolTip", Arg.Any<string>());
        translation.DidNotReceive().AddTranslationItem(nameof(FormFileHistory), "toolStripSplitLoad", "toolTip", Arg.Any<string>());

        string[] emittedKeys = translation.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(ITranslation.AddTranslationItem))
            .Select(call => string.Join('.', call.GetArguments().Take(3)))
            .ToArray();
        emittedKeys.Distinct(StringComparer.Ordinal).Count().Should().Be(emittedKeys.Length);
    }

    [AvaloniaTest]
    public void FormFileHistory_should_toggle_author_avatars_from_blame_options()
    {
        FormFileHistory form = new();
        MenuItem showAvatars = form.FindControl<MenuItem>("showAuthorAvatarToolStripMenuItem")
            ?? throw new InvalidOperationException("The author-avatar blame option was not created.");
        bool original = AppSettings.BlameShowAuthorAvatar;

        showAvatars.IsChecked.Should().Be(original);
        showAvatars.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

        AppSettings.BlameShowAuthorAvatar.Should().Be(!original);
        showAvatars.IsChecked.Should().Be(!original);
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
    public async Task FormFileHistory_should_show_the_selected_blob_and_commit_tabs()
    {
        GitModule module = CreateRepositoryWithFileHistory();
        FormFileHistory form = new(CreateCommands(module), "tracked.txt");
        form.Show();
        try
        {
            RevisionGridControl revisionGrid = form.FindControl<RevisionGridControl>("RevisionGrid")!;
            TabControl tabs = form.FindControl<TabControl>("tabControl1")!;
            TabItem viewTab = form.FindControl<TabItem>("ViewTab")!;
            TabItem commitTab = form.FindControl<TabItem>("CommitInfoTabPage")!;
            FileViewer view = form.FindControl<FileViewer>("View")!;
            GitUI.UserControls.CommitDiff commitDiff = form.FindControl<GitUI.UserControls.CommitDiff>("CommitDiff")!;

            await WaitUntilAsync(() => revisionGrid.SelectedRevision is not null);

            tabs.SelectedItem = viewTab;
            await WaitUntilAsync(() => view.TextEditor.Text.Contains("second line", StringComparison.Ordinal));

            tabs.SelectedItem = commitTab;
            await WaitUntilAsync(() => commitDiff.CommitInformation.Revision?.ObjectId == revisionGrid.SelectedRevision!.ObjectId);
        }
        finally
        {
            form.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [AvaloniaTest]
    public async Task FormFileHistory_should_allow_two_revisions_for_comparison()
    {
        GitModule module = CreateRepositoryWithFileHistory();
        FormFileHistory form = new(CreateCommands(module), "tracked.txt");
        form.Show();
        try
        {
            RevisionGridControl revisionGrid = form.FindControl<RevisionGridControl>("RevisionGrid")!;
            TextBlock loadingStatus = revisionGrid.FindControl<TextBlock>("lblLoadingStatus")!;
            ListBox revisions = revisionGrid.FindControl<ListBox>("lstRevisions")!;

            await WaitUntilAsync(() => loadingStatus.Text == "2 revisions");
            GitRevision[] items = [.. revisions.ItemsSource!.OfType<GitRevision>()];
            revisions.SelectedItems!.Clear();
            revisions.SelectedItems.Add(items[0]);
            revisions.SelectedItems.Add(items[1]);

            revisionGrid.GetSelectedRevisions().Should().Equal(items);
        }
        finally
        {
            form.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [AvaloniaTest]
    public async Task FormFileHistory_should_follow_renames_and_use_the_historical_file_name()
    {
        GitModule module = CreateRepositoryWithRenamedFileHistory();
        FormFileHistory form = new(CreateCommands(module), "tracked.txt");
        form.Show();
        try
        {
            RevisionGridControl revisionGrid = form.FindControl<RevisionGridControl>("RevisionGrid")!;
            TextBlock loadingStatus = revisionGrid.FindControl<TextBlock>("lblLoadingStatus")!;
            TabControl tabs = form.FindControl<TabControl>("tabControl1")!;
            TabItem viewTab = form.FindControl<TabItem>("ViewTab")!;
            FileViewer view = form.FindControl<FileViewer>("View")!;
            ListBox revisions = revisionGrid.FindControl<ListBox>("lstRevisions")!;

            await WaitUntilAsync(() => loadingStatus.Text == "3 revisions");
            GitRevision originalRevision = revisions.ItemsSource!
                .OfType<GitRevision>()
                .Single(revision => revision.Subject == "initial");

            revisionGrid.SetSelectedRevision(originalRevision.ObjectId).Should().BeTrue();
            revisionGrid.GetRevisionFileName("tracked.txt", originalRevision.ObjectId).Should().Be("old.txt");

            tabs.SelectedItem = viewTab;
            await WaitUntilAsync(() => view.TextEditor.Text.Contains("initial line", StringComparison.Ordinal));
            form.Text.Should().Contain("(old.txt)");
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public async Task FormFileHistory_should_wait_for_explicit_load_when_auto_load_is_disabled()
    {
        AppSettings.LoadFileHistoryOnShow = false;
        GitModule module = CreateRepositoryWithFileHistory();
        FormFileHistory form = new(CreateCommands(module), "tracked.txt");
        form.Show();
        try
        {
            RevisionGridControl revisionGrid = form.FindControl<RevisionGridControl>("RevisionGrid")!;
            TextBlock loadingStatus = revisionGrid.FindControl<TextBlock>("lblLoadingStatus")!;
            FileViewer diff = form.FindControl<FileViewer>("Diff")!;
            GitUI.Compat.IconSplitButton loadButton = form.FindControl<GitUI.Compat.IconSplitButton>("toolStripSplitLoad")!;

            revisionGrid.IsVisible.Should().BeFalse();
            loadButton.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(SplitButton.ClickEvent));

            await WaitUntilAsync(() =>
                revisionGrid.IsVisible
                && loadingStatus.Text == "2 revisions"
                && diff.TextEditor.Text.Contains("+second line", StringComparison.Ordinal));
        }
        finally
        {
            form.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [AvaloniaTest]
    public void FormFileHistory_history_and_blame_options_should_update_shared_settings()
    {
        AppSettings.FullHistoryInFileHistory = false;
        AppSettings.SimplifyMergesInFileHistory = true;
        bool previousIgnoreWhitespace = AppSettings.IgnoreWhitespaceOnBlame;
        FormFileHistory form = new();

        MenuItem fullHistory = form.FindControl<MenuItem>("showFullHistoryToolStripMenuItem")!;
        MenuItem simplifyMerges = form.FindControl<MenuItem>("simplifyMergesToolStripMenuItem")!;
        MenuItem ignoreWhitespace = form.FindControl<MenuItem>("ignoreWhitespaceToolStripMenuItem")!;

        fullHistory.IsChecked.Should().BeFalse();
        simplifyMerges.IsEnabled.Should().BeFalse();

        fullHistory.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent));
        AppSettings.FullHistoryInFileHistory.Should().BeTrue();
        simplifyMerges.IsEnabled.Should().BeTrue();

        simplifyMerges.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent));
        AppSettings.SimplifyMergesInFileHistory.Should().BeFalse();

        ignoreWhitespace.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent));
        AppSettings.IgnoreWhitespaceOnBlame.Should().Be(!previousIgnoreWhitespace);
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

    [AvaloniaTest]
    public async Task Custom_difftool_item_should_route_the_selected_revision_and_tool()
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
            FileViewer diff = form.FindControl<FileViewer>("Diff")
                ?? throw new InvalidOperationException("Diff viewer was not created.");
            await WaitUntilAsync(() => revisionGrid.SelectedRevision is not null);
            await WaitUntilAsync(() => diff.TextEditor.Text.Contains("+second line", StringComparison.Ordinal));

            MenuItem openWithDifftool = form.FindControl<MenuItem>("openWithDifftoolToolStripMenuItem")
                ?? throw new InvalidOperationException("Difftool menu item was not created.");
            openWithDifftool.Tag = "meld";
            openWithDifftool.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent));

            commands.Received(1).OpenWithDifftool(
                form,
                Arg.Is<IReadOnlyList<GitRevision?>>(revisions => revisions.Count == 1 && revisions[0] == revisionGrid.SelectedRevision),
                "tracked.txt",
                "tracked.txt",
                RevisionDiffKind.DiffAB,
                true,
                "meld");
        }
        finally
        {
            form.Close();
        }
    }

    private IGitUICommands CreateCommands(GitModule module)
    {
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.GetService(Arg.Any<Type>()).Returns(call => _serviceContainer.GetService(call.Arg<Type>()));
        return commands;
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

    private GitModule CreateRepositoryWithRenamedFileHistory()
    {
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet", "-b", "main" }).Should().BeTrue();
        module.SetSetting("user.name", "Avalonia Test");
        module.SetSetting("user.email", "avalonia@example.com");

        File.WriteAllText(Path.Combine(_workingDirectory, "old.txt"), "initial line\n");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "old.txt" }).Should().BeTrue();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" }).Should().BeTrue();

        module.GitExecutable.RunCommand(new GitArgumentBuilder("mv") { "old.txt", "tracked.txt" }).Should().BeTrue();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "rename" }).Should().BeTrue();

        File.AppendAllText(Path.Combine(_workingDirectory, "tracked.txt"), "second line\n");
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
