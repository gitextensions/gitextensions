using System.ComponentModel.Design;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
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
    }

    [TearDown]
    public void TearDown()
    {
        WinFormsShims.ShimHost.MessageBoxHost = _originalMessageBoxHost ?? new StubMessageBoxHost();
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
    public void StartCloneForkFromHoster_should_open_provider_configuration_when_required()
    {
        IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
        host.ConfigurationOk.Returns(false);
        GitUICommands commands = new(_serviceContainer, Substitute.For<IGitModule>());

        commands.StartCloneForkFromHoster(owner: null, host, gitModuleChanged: null);

        host.Received(1).Execute(Arg.Any<GitUIEventArgs>());
    }

    private static ForkAndCloneForm CreateForm(IRepositoryHostPlugin host)
    {
        host.Name.Returns("Test host");
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        return new ForkAndCloneForm(commands, host, gitModuleChanged: null);
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
