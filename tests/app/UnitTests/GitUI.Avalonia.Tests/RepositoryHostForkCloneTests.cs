using System.ComponentModel.Design;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Git;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Plugins;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs.RepoHosting;
using GitUI.Compat;
using GitUI.UserControls;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class RepositoryHostForkCloneTests
{
    private ServiceContainer _serviceContainer = null!;
    private StubMessageBoxHost _messageBoxHost = null!;
    private WinFormsShims.IMessageBoxHost? _originalMessageBoxHost;
    private StubFolderPicker _folderPicker = null!;
    private WinFormsShims.IFolderPicker? _originalFolderPicker;

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

        _originalMessageBoxHost = TryGetMessageBoxHost();
        _messageBoxHost = new StubMessageBoxHost();
        WinFormsShims.ShimHost.MessageBoxHost = _messageBoxHost;
        _originalFolderPicker = TryGetFolderPicker();
        _folderPicker = new StubFolderPicker();
        WinFormsShims.ShimHost.FolderPicker = _folderPicker;
    }

    [TearDown]
    public void TearDown()
    {
        WinFormsShims.ShimHost.MessageBoxHost = _originalMessageBoxHost ?? new StubMessageBoxHost();
        WinFormsShims.ShimHost.FolderPicker = _originalFolderPicker ?? new StubFolderPicker();
        _serviceContainer.Dispose();
    }

    [AvaloniaTest]
    public void ForkAndCloneForm_should_preserve_layout_and_translation_identities()
    {
        using ForkAndCloneForm form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);
        form.TranslateItems(translation);

        form.Width.Should().Be(744);
        form.Height.Should().Be(552);
        form.FindControl<TabControl>("tabControl")!.ItemCount.Should().Be(2);
        form.FindControl<ListBox>("myReposLV").Should().NotBeNull();
        form.FindControl<ListBox>("searchResultsLV").Should().NotBeNull();
        form.FindControl<NumericUpDown>("depthUpDown")!.Maximum.Should().Be(999);
        form.FindControl<Grid>("tableLayoutPanel2")!.RowDefinitions[1].Height.Value.Should().Be(183);
        form.FindControl<TextBox>("destinationTB")!.Width.Should().Be(294);
        FolderBrowserButton browse = form.FindControl<FolderBrowserButton>("browseForCloneToDirbtn")!;
        browse.Height.Should().Be(23);
        browse.Text.Should().BeEmpty();
        browse.PathShowingControl.Should().BeSameAs(form.FindControl<TextBox>("createDirTB"));
        IconButton browseButton = browse.FindControl<IconButton>("buttonBrowse")!;
        browseButton.Content.Should().Be("_Browse...");
        browseButton.Icon.Should().NotBeNull();
        form.FindControl<TextBox>("createDirTB")!.Width.Should().Be(183);
        form.FindControl<ComboBox>("addUpstreamRemoteAsCB")!.Width.Should().Be(200);
        Grid.GetRow(form.FindControl<NumericUpDown>("depthUpDown")!).Should().Be(3);
        Grid myRepositoriesHeader = (Grid)form.FindControl<Border>("columnHeaderMyReposName")!.Parent!;
        myRepositoriesHeader.ColumnDefinitions.Select(column => column.Width.Value)
            .Should().Equal(180, 45, 50, 45);
        Grid searchHeader = (Grid)form.FindControl<Border>("columnHeaderSearchName")!.Parent!;
        searchHeader.ColumnDefinitions.Select(column => column.Width.Value)
            .Should().Equal(180, 110, 41, 40);

        translation.Received(1).AddTranslationItem(
            nameof(ForkAndCloneForm), "$this", "Text", "Remote repository fork and clone");
        translation.Received(1).AddTranslationItem(
            nameof(ForkAndCloneForm), "myReposPage", "Text", "My repositories");
        translation.Received(1).AddTranslationItem(
            nameof(ForkAndCloneForm), "searchReposPage", "Text", "Search for repositories");
        translation.Received(1).AddTranslationItem(
            nameof(ForkAndCloneForm), "columnHeaderMyReposName", "Text", "Name");
        translation.Received(1).AddTranslationItem(
            nameof(ForkAndCloneForm), "columnHeaderMyReposIsFork", "Text", "Is fork");
        translation.Received(1).AddTranslationItem(
            nameof(ForkAndCloneForm), "columnHeaderMyReposForks", "Text", "# Forks");
        translation.Received(1).AddTranslationItem(
            nameof(ForkAndCloneForm), "columnHeaderMyReposIsPrivate", "Text", "Private");
        translation.Received(1).AddTranslationItem(
            nameof(ForkAndCloneForm), "columnHeaderSearchName", "Text", "Name");
        translation.Received(1).AddTranslationItem(
            nameof(ForkAndCloneForm), "columnHeaderSearchOwner", "Text", "Owner");
        translation.Received(1).AddTranslationItem(
            nameof(ForkAndCloneForm), "columnHeaderSearchIsFork", "Text", "Is fork");
        translation.Received(1).AddTranslationItem(
            nameof(ForkAndCloneForm), "columnHeaderSearchForks", "Text", "# Forks");
        translation.Received(1).AddTranslationItem(
            nameof(ForkAndCloneForm), "_strWillCloneInfo", "Text",
            "Will clone {0} into {1}.\r\nYou can not push unless you are a collaborator. {2}");
    }

    [AvaloniaTest]
    public async Task ForkAndCloneForm_should_load_sort_and_select_owned_repositories()
    {
        IHostedRepository beta = CreateRepository("beta", owner: "me", isFork: false);
        IHostedRepository alpha = CreateRepository("alpha", owner: "me", isFork: true);
        IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
        host.GetMyRepos().Returns([beta, alpha]);
        using ForkAndCloneForm form = CreateForm(host);
        ForkAndCloneForm.TestAccessor accessor = form.GetTestAccessor();
        accessor.Destination = Path.Combine(Path.GetTempPath(), "fork-clone");

        await accessor.LoadMyRepositoriesAsync().WaitAsync(TimeSpan.FromSeconds(5));
        accessor.MyRepositoryNames.Should().Equal("alpha", "beta");

        accessor.SelectMyRepository(0);
        accessor.CloneEnabled.Should().BeTrue();
        accessor.CreateDirectory.Should().Be("alpha");
        accessor.TargetDirectory.Should().Be(Path.Combine(accessor.Destination, "alpha"));
        accessor.CloneInfo.Should().Contain("https://example.test/alpha.git");
        accessor.CloneInfo.Should().Contain("push access");
    }

    [AvaloniaTest]
    public async Task ForkAndCloneForm_should_autosize_content_columns_instead_of_reusing_capture_widths()
    {
        IHostedRepository repository = CreateRepository(
            "repository-name-that-is-wider-than-the-Designer-column",
            owner: "owner-name-that-is-wider-than-the-Designer-column",
            isFork: false);
        IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
        host.GetMyRepos().Returns([repository]);
        host.SearchForRepository("wide").Returns([repository]);
        using ForkAndCloneForm form = CreateForm(host);
        ForkAndCloneForm.TestAccessor accessor = form.GetTestAccessor();

        await accessor.LoadMyRepositoriesAsync().WaitAsync(TimeSpan.FromSeconds(5));
        Grid myHeader = (Grid)form.FindControl<Border>("columnHeaderMyReposName")!.Parent!;
        myHeader.ColumnDefinitions[0].Width.Value.Should().BeGreaterThan(180);

        accessor.StartSearch("wide", byUser: false);
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));
        Grid searchHeader = (Grid)form.FindControl<Border>("columnHeaderSearchName")!.Parent!;
        searchHeader.ColumnDefinitions[0].Width.Value.Should().BeGreaterThan(180);
        searchHeader.ColumnDefinitions[1].Width.Value.Should().BeGreaterThan(110);
    }

    [AvaloniaTest]
    public async Task ForkAndCloneForm_should_search_sort_and_show_repository_details()
    {
        IHostedRepository zulu = CreateRepository("zulu", owner: "other", isFork: false);
        IHostedRepository alpha = CreateRepository("alpha", owner: "other", isFork: false);
        IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
        host.SearchForRepository("query").Returns([zulu, alpha]);
        using ForkAndCloneForm form = CreateForm(host);
        ForkAndCloneForm.TestAccessor accessor = form.GetTestAccessor();
        accessor.Destination = Path.Combine(Path.GetTempPath(), "fork-clone");

        await accessor.SearchAsync("query", byUser: false).WaitAsync(TimeSpan.FromSeconds(5));
        accessor.SearchResultNames.Should().Equal("alpha", "zulu");

        accessor.SelectSearchResult(0);
        accessor.CloneEnabled.Should().BeTrue();
        accessor.Description.Should().Be("alpha description");
        accessor.CloneInfo.Should().Contain("can not push");
    }

    [AvaloniaTest]
    public async Task ForkAndCloneForm_should_publish_only_the_latest_search_result()
    {
        TaskCompletionSource firstSearchStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource releaseFirstSearch = new(TaskCreationOptions.RunContinuationsAsynchronously);
        IHostedRepository stale = CreateRepository("stale", owner: "other", isFork: false);
        IHostedRepository current = CreateRepository("current", owner: "other", isFork: false);
        IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
        host.SearchForRepository("first").Returns(_ =>
        {
            firstSearchStarted.TrySetResult();
            releaseFirstSearch.Task.GetAwaiter().GetResult();
            return [stale];
        });
        host.SearchForRepository("second").Returns([current]);
        using ForkAndCloneForm form = CreateForm(host);
        ForkAndCloneForm.TestAccessor accessor = form.GetTestAccessor();

        accessor.StartSearch("first", byUser: false);
        await firstSearchStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        accessor.StartSearch("second", byUser: false);
        releaseFirstSearch.TrySetResult();
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        accessor.SearchResultNames.Should().Equal("current");
        accessor.SearchEnabled.Should().BeTrue();
        accessor.GetFromUserEnabled.Should().BeTrue();
        _messageBoxHost.Messages.Should().BeEmpty();
    }

    [AvaloniaTest]
    public async Task ForkAndCloneForm_should_report_owned_repository_load_failure_in_the_help_text()
    {
        IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
        host.GetMyRepos().Returns(_ => throw new InvalidOperationException("owned repositories failed"));
        using ForkAndCloneForm form = CreateForm(host);
        ForkAndCloneForm.TestAccessor accessor = form.GetTestAccessor();

        await accessor.LoadMyRepositoriesAsync().WaitAsync(TimeSpan.FromSeconds(5));

        accessor.MyRepositoryNames.Should().BeEmpty();
        accessor.HelpText.Should().Contain("Failed to get repositories");
        accessor.HelpText.Should().Contain("owned repositories failed");
    }

    [AvaloniaTest]
    public async Task ForkAndCloneForm_should_preserve_the_selected_protocol_across_repository_changes()
    {
        IHostedRepository first = CreateRepository("first", owner: "me", isFork: false);
        IHostedRepository second = CreateRepository("second", owner: "me", isFork: false);
        IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
        host.GetMyRepos().Returns([first, second]);
        using ForkAndCloneForm form = CreateForm(host);
        ForkAndCloneForm.TestAccessor accessor = form.GetTestAccessor();
        accessor.Destination = Path.GetTempPath();
        await accessor.LoadMyRepositoriesAsync().WaitAsync(TimeSpan.FromSeconds(5));

        accessor.SelectMyRepository(0);
        accessor.SelectedProtocol = GitProtocol.Ssh;
        accessor.SelectMyRepository(1);

        accessor.SelectedProtocol.Should().Be(GitProtocol.Ssh);
        second.Received().CloneProtocol = GitProtocol.Ssh;
        accessor.CloneInfo.Should().Contain("second.git");
    }

    [AvaloniaTest]
    public void ForkAndCloneForm_should_preserve_target_directory_and_depth_rules()
    {
        using ForkAndCloneForm form = CreateForm(Substitute.For<IRepositoryHostPlugin>());
        ForkAndCloneForm.TestAccessor accessor = form.GetTestAccessor();
        accessor.Destination = Path.Combine(Path.GetTempPath(), "destination");
        accessor.CreateDirectory = "project";

        accessor.TargetDirectory.Should().Be(Path.Combine(accessor.Destination, "project"));
        accessor.Depth.Should().BeNull();

        accessor.SetDepth(42);
        accessor.Depth.Should().Be(42);
    }

    [AvaloniaTest]
    public async Task ForkAndCloneForm_should_disable_clone_for_invalid_native_path_characters()
    {
        IHostedRepository repository = CreateRepository("project", owner: "me", isFork: false);
        IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
        host.GetMyRepos().Returns([repository]);
        using ForkAndCloneForm form = CreateForm(host);
        ForkAndCloneForm.TestAccessor accessor = form.GetTestAccessor();
        accessor.Destination = Path.GetTempPath();
        await accessor.LoadMyRepositoriesAsync().WaitAsync(TimeSpan.FromSeconds(5));
        accessor.SelectMyRepository(0);
        accessor.CloneEnabled.Should().BeTrue();

        accessor.CreateDirectory = "project" + Path.GetInvalidPathChars()[0];
        accessor.ValidatePaths();

        accessor.CloneEnabled.Should().BeFalse();
    }

    [AvaloniaTest]
    public void ForkAndCloneForm_should_report_an_empty_clone_destination()
    {
        using ForkAndCloneForm form = CreateForm(Substitute.For<IRepositoryHostPlugin>());
        ForkAndCloneForm.TestAccessor accessor = form.GetTestAccessor();
        accessor.Destination = string.Empty;
        accessor.CreateDirectory = "project";

        accessor.GetTargetDirectoryWithValidation().Should().BeNull();

        _messageBoxHost.Messages.Should().ContainSingle()
            .Which.Should().Be("Clone folder can not be empty");
    }

    [AvaloniaTest]
    public async Task ForkAndCloneForm_should_route_user_not_found_and_restore_search_actions()
    {
        IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
        host.GetRepositoriesOfUser("missing-user")
            .Returns(_ => throw new InvalidOperationException("HTTP 404"));
        using ForkAndCloneForm form = CreateForm(host);
        ForkAndCloneForm.TestAccessor accessor = form.GetTestAccessor();

        accessor.StartSearch("missing-user", byUser: true);
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        _messageBoxHost.Messages.Should().ContainSingle()
            .Which.Should().Be("User not found!");
        accessor.SearchEnabled.Should().BeTrue();
        accessor.GetFromUserEnabled.Should().BeTrue();
    }

    [AvaloniaTest]
    public async Task ForkAndCloneForm_should_fork_the_selected_repository_and_reload_owned_repositories()
    {
        IHostedRepository repository = CreateRepository("project", owner: "other", isFork: false);
        IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
        host.SearchForRepository("project").Returns([repository]);
        host.GetMyRepos().Returns([repository]);
        using ForkAndCloneForm form = CreateForm(host);
        ForkAndCloneForm.TestAccessor accessor = form.GetTestAccessor();

        accessor.StartSearch("project", byUser: false);
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));
        accessor.SelectSearchResult(0);
        accessor.ForkSelectedRepository();
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        repository.Received(1).Fork();
        accessor.IsMyRepositoriesTabSelected.Should().BeTrue();
        accessor.MyRepositoryNames.Should().Equal("project");
    }

    [AvaloniaTest]
    public async Task ForkAndCloneForm_should_report_a_fork_failure_and_restore_the_action()
    {
        IHostedRepository repository = CreateRepository("project", owner: "other", isFork: false);
        repository.When(candidate => candidate.Fork())
            .Do(_ => throw new InvalidOperationException("provider failed"));
        IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
        host.SearchForRepository("project").Returns([repository]);
        using ForkAndCloneForm form = CreateForm(host);
        ForkAndCloneForm.TestAccessor accessor = form.GetTestAccessor();

        accessor.StartSearch("project", byUser: false);
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));
        accessor.SelectSearchResult(0);
        accessor.ForkSelectedRepository();
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        _messageBoxHost.Messages.Should().ContainSingle()
            .Which.Should().Be("Failed to fork:" + Environment.NewLine + "provider failed");
        accessor.ForkEnabled.Should().BeTrue();
    }

    [AvaloniaTest]
    public void ForkAndCloneForm_should_route_folder_picker_accept_and_cancel()
    {
        using ForkAndCloneForm form = CreateForm(Substitute.For<IRepositoryHostPlugin>());
        ForkAndCloneForm.TestAccessor accessor = form.GetTestAccessor();
        string initialDirectory = Path.Combine(Path.GetTempPath(), "initial-clone-root");
        string selectedDirectory = Path.Combine(Path.GetTempPath(), "selected-clone-root");
        accessor.Destination = initialDirectory;
        _folderPicker.Result = selectedDirectory;

        accessor.BrowseForCloneDirectory();

        accessor.Destination.Should().Be(selectedDirectory);
        _folderPicker.RequestedPaths.Should().Equal(initialDirectory);

        _folderPicker.Result = null;
        form.FindControl<FolderBrowserButton>("browseForCloneToDirbtn")!
            .FindControl<Button>("buttonBrowse")!
            .RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        accessor.Destination.Should().Be(selectedDirectory);
        _folderPicker.RequestedPaths.Should().Equal(initialDirectory, selectedDirectory);

        accessor.Destination = string.Empty;
        accessor.BrowseForCloneDirectory();

        string expectedRoot = Path.GetPathRoot(Environment.CurrentDirectory) ?? Environment.CurrentDirectory;
        _folderPicker.RequestedPaths.Should().Equal(initialDirectory, selectedDirectory, expectedRoot);
    }

    [AvaloniaTest]
    public async Task ForkAndCloneForm_should_clone_and_add_the_selected_upstream_remote()
    {
        string root = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.RepositoryHostClone-{Guid.NewGuid():N}");
        string sourceDirectory = Path.Combine(root, "source");
        string destinationRoot = Path.Combine(root, "clones");
        string targetDirectory = Path.Combine(destinationRoot, "project");
        Directory.CreateDirectory(destinationRoot);
        try
        {
            GitModule sourceModule = CreateCommittedRepository(sourceDirectory, "main");
            IGitUICommands commands = CreateCommands(sourceModule);
            commands.StartGitCommandProcessDialog(
                    Arg.Any<WinFormsShims.IWin32Window>(),
                    Arg.Any<ArgumentString>())
                .Returns(call => sourceModule.GitExecutable.RunCommand(call.ArgAt<ArgumentString>(1)));
            IGitModule? selectedModule = null;
            IHostedRepository repository = CreateRepository("project", owner: "me", isFork: true);
            repository.CloneUrl.Returns(sourceDirectory);
            repository.ParentOwner.Returns("parent");
            repository.ParentUrl.Returns("https://example.test/parent/project.git");
            IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
            host.GetMyRepos().Returns([repository]);
            using ForkAndCloneForm form = new(
                commands,
                host,
                (_, args) => selectedModule = args.GitModule);
            ForkAndCloneForm.TestAccessor accessor = form.GetTestAccessor();
            accessor.Destination = destinationRoot;
            await accessor.LoadMyRepositoriesAsync().WaitAsync(TimeSpan.FromSeconds(5));
            accessor.SelectMyRepository(0);
            accessor.UpstreamRemoteName = "upstream";

            accessor.CloneSelectedRepository();

            selectedModule.Should().NotBeNull();
            Path.TrimEndingDirectorySeparator(selectedModule!.WorkingDir)
                .Should().Be(Path.TrimEndingDirectorySeparator(targetDirectory));
            selectedModule.GetCurrentCheckout().Should().Be(sourceModule.GetCurrentCheckout());
            IReadOnlyList<Remote> remotes = await selectedModule.GetRemotesAsync();
            Remote origin = remotes.Should().ContainSingle(remote => remote.Name == "origin").Which;
            Path.GetFullPath(origin.FetchUrl).Should().Be(Path.GetFullPath(sourceDirectory));
            remotes.Should().Contain(remote => remote.Name == "upstream" && remote.FetchUrl == repository.ParentUrl);
        }
        finally
        {
            TestDirectory.Delete(root);
        }
    }

    [AvaloniaTest]
    public async Task ForkAndCloneForm_should_stop_when_the_clone_process_fails()
    {
        string root = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.RepositoryHostCloneFailure-{Guid.NewGuid():N}");
        string sourceDirectory = Path.Combine(root, "source");
        string destinationRoot = Path.Combine(root, "clones");
        Directory.CreateDirectory(destinationRoot);
        try
        {
            GitModule sourceModule = CreateCommittedRepository(sourceDirectory, "main");
            IGitUICommands commands = CreateCommands(sourceModule);
            commands.StartGitCommandProcessDialog(
                    Arg.Any<WinFormsShims.IWin32Window>(),
                    Arg.Any<ArgumentString>())
                .Returns(false);
            bool moduleChanged = false;
            IHostedRepository repository = CreateRepository("project", owner: "me", isFork: false);
            repository.CloneUrl.Returns(sourceDirectory);
            IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
            host.GetMyRepos().Returns([repository]);
            using ForkAndCloneForm form = new(
                commands,
                host,
                (_, _) => moduleChanged = true);
            ForkAndCloneForm.TestAccessor accessor = form.GetTestAccessor();
            accessor.Destination = destinationRoot;
            await accessor.LoadMyRepositoriesAsync().WaitAsync(TimeSpan.FromSeconds(5));
            accessor.SelectMyRepository(0);

            accessor.CloneSelectedRepository();

            moduleChanged.Should().BeFalse();
            Directory.Exists(Path.Combine(destinationRoot, "project")).Should().BeFalse();
        }
        finally
        {
            TestDirectory.Delete(root);
        }
    }

    [AvaloniaTest]
    public void StartCloneForkFromHoster_should_open_provider_configuration_when_required()
    {
        IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
        host.ConfigurationOk.Returns(false);
        GitUICommands commands = new(_serviceContainer, Substitute.For<IGitModule>());

        commands.StartCloneForkFromHoster(owner: null, host, gitModuleChanged: null);

        host.Received(1).Execute(Arg.Any<GitUIEventArgs>());
    }

    private ForkAndCloneForm CreateForm(IRepositoryHostPlugin host)
    {
        host.Name.Returns("Test host");
        IGitUICommands commands = CreateCommands(Substitute.For<IGitModule>());
        return new ForkAndCloneForm(commands, host, gitModuleChanged: null);
    }

    private IGitUICommands CreateCommands(IGitModule module)
    {
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.GetService(Arg.Any<Type>()).Returns(call => _serviceContainer.GetService(call.Arg<Type>()));
        return commands;
    }

    private GitModule CreateCommittedRepository(string workingDirectory, string branch)
    {
        Directory.CreateDirectory(workingDirectory);
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet" }).Should().BeTrue();
        module.SetSetting("user.name", "Avalonia Test");
        module.SetSetting("user.email", "avalonia@example.com");
        File.WriteAllText(Path.Combine(workingDirectory, "tracked.txt"), "content");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "tracked.txt" }).Should().BeTrue();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" }).Should().BeTrue();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("branch") { "-M", branch }).Should().BeTrue();
        return module;
    }

    private static IHostedRepository CreateRepository(string name, string owner, bool isFork)
    {
        IHostedRepository repository = Substitute.For<IHostedRepository>();
        repository.Name.Returns(name);
        repository.Owner.Returns(owner);
        repository.Description.Returns($"{name} description");
        repository.IsAFork.Returns(isFork);
        repository.IsPrivate.Returns(false);
        repository.Forks.Returns(3);
        repository.Homepage.Returns($"https://example.test/{name}");
        repository.CloneUrl.Returns($"https://example.test/{name}.git");
        repository.SupportedCloneProtocols.Returns([GitProtocol.Https, GitProtocol.Ssh]);
        repository.CloneProtocol.Returns(GitProtocol.Https);
        return repository;
    }

    private static WinFormsShims.IMessageBoxHost? TryGetMessageBoxHost()
    {
        try
        {
            return WinFormsShims.ShimHost.MessageBoxHost;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private static WinFormsShims.IFolderPicker? TryGetFolderPicker()
    {
        try
        {
            return WinFormsShims.ShimHost.FolderPicker;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private sealed class StubFolderPicker : WinFormsShims.IFolderPicker
    {
        public string? Result { get; set; }

        public List<string?> RequestedPaths { get; } = [];

        public string? PickFolder(WinFormsShims.IWin32Window? owner, string? selectedPath)
        {
            RequestedPaths.Add(selectedPath);
            return Result;
        }
    }

    private sealed class StubMessageBoxHost : WinFormsShims.IMessageBoxHost
    {
        public List<string> Messages { get; } = [];

        public WinFormsShims.DialogResult Show(
            WinFormsShims.IWin32Window? owner,
            string? text,
            string? caption,
            WinFormsShims.MessageBoxButtons buttons,
            WinFormsShims.MessageBoxIcon icon,
            WinFormsShims.MessageBoxDefaultButton defaultButton)
        {
            Messages.Add(text ?? string.Empty);
            return WinFormsShims.DialogResult.OK;
        }
    }
}
