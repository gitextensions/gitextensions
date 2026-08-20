using System.ComponentModel.Design;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Git;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Plugins;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.RepoHosting;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class RepositoryHostCreatePullRequestTests
{
    private ServiceContainer _serviceContainer = null!;
    private string _workingDirectory = null!;
    private string _originalApplicationExecutablePath = null!;
    private StubMessageBoxHost _messageBoxHost = null!;
    private IRepositoryHostPlugin[] _originalGitHosters = null!;

    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        AppSettings.TestAccessor settingsAccessor = AppSettings.GetTestAccessor();
        _originalApplicationExecutablePath = settingsAccessor.ApplicationExecutablePath;
        settingsAccessor.ApplicationExecutablePath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "GitExtensions.Avalonia.exe");

        _serviceContainer = new ServiceContainer();
        GitExtUtils.ServiceContainerRegistry.RegisterServices(_serviceContainer);

        System.IO.Abstractions.FileSystem fileSystem = new();
        GitDirectoryResolver gitDirectoryResolver = new(fileSystem);
        RepositoryDescriptionProvider repositoryDescriptionProvider = new(gitDirectoryResolver);
        _serviceContainer.AddService<System.IO.Abstractions.IFileSystem>(fileSystem);
        _serviceContainer.AddService<IGitDirectoryResolver>(gitDirectoryResolver);
        _serviceContainer.AddService<IRepositoryDescriptionProvider>(repositoryDescriptionProvider);
        _serviceContainer.AddService<IAppTitleGenerator>(
            new AppTitleGenerator(repositoryDescriptionProvider));
        _serviceContainer.AddService<ILinkFactory>(new LinkFactory());
        GitCommands.ServiceContainerRegistry.RegisterServices(_serviceContainer);
        GitUI.ServiceContainerRegistry.RegisterServices(_serviceContainer);

        _messageBoxHost = new StubMessageBoxHost();
        WinFormsShims.ShimHost.MessageBoxHost = _messageBoxHost;
        _originalGitHosters = [.. PluginRegistry.GitHosters];
        _workingDirectory = Path.Combine(
            Path.GetTempPath(),
            $"GitExtensions.Avalonia.CreatePullRequestTests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Path.Combine(_workingDirectory, ".github"));
    }

    [TearDown]
    public void TearDown()
    {
        PluginRegistry.GitHosters.Clear();
        PluginRegistry.GitHosters.AddRange(_originalGitHosters);
        AppSettings.GetTestAccessor().ApplicationExecutablePath = _originalApplicationExecutablePath;
        _serviceContainer.Dispose();
        TestDirectory.Delete(_workingDirectory);
    }

    [AvaloniaTest]
    public void CreatePullRequestForm_should_preserve_layout_and_translation_identities()
    {
        using CreatePullRequestForm form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);
        form.TranslateItems(translation);

        form.Width.Should().Be(546);
        form.Height.Should().Be(323);
        form.FindControl<ComboBox>("_pullReqTargetsCB").Should().NotBeNull();
        form.FindControl<ComboBox>("_yourBranchesCB").Should().NotBeNull();
        form.FindControl<ComboBox>("_remoteBranchesCB").Should().NotBeNull();
        form.FindControl<GitUI.SpellChecker.EditNetSpell>("_bodyTB").Should().NotBeNull();

        translation.Received(1).AddTranslationItem(
            nameof(CreatePullRequestForm), "$this", "Text", "Create Pull Request");
        translation.Received(1).AddTranslationItem(
            nameof(CreatePullRequestForm), "_createBtn", "Text", "Create");
        translation.Received(1).AddTranslationItem(
            nameof(CreatePullRequestForm), "groupBox1", "Text", "Pull request data");
        translation.Received(1).AddTranslationItem(
            nameof(CreatePullRequestForm), "label1", "Text", "Title:");
        translation.Received(1).AddTranslationItem(
            nameof(CreatePullRequestForm), "label2", "Text", "Body:");
        translation.Received(1).AddTranslationItem(
            nameof(CreatePullRequestForm), "label3", "Text", "Target repository:");
        translation.Received(1).AddTranslationItem(
            nameof(CreatePullRequestForm), "label4", "Text", "Your branch:");
        translation.Received(1).AddTranslationItem(
            nameof(CreatePullRequestForm), "label5", "Text", "Target branch:");
        translation.Received(1).AddTranslationItem(
            nameof(CreatePullRequestForm), "_strFailedToLoadTemplate", "Text", "Failed to load PR template from file.");
    }

    [AvaloniaTest]
    public void CreatePullRequestForm_should_focus_the_hosted_body_editor()
    {
        using CreatePullRequestForm form = CreateForm(CreateFixture(), chooseRemote: null, chooseBranch: null);
        GitUI.SpellChecker.EditNetSpell body = form.FindControl<GitUI.SpellChecker.EditNetSpell>("_bodyTB")!;
        form.Show();
        form.SetRenderScaling(1.25);
        Dispatcher.UIThread.RunJobs();

        body.Focus().Should().BeTrue();
        Dispatcher.UIThread.RunJobs();
        body.GetTestAccessor().TextBox.IsFocused.Should().BeTrue();

        form.Close();
    }

    [AvaloniaTest]
    public async Task CreatePullRequestForm_should_load_selected_remote_current_branch_title_and_template()
    {
        PullRequestFixture fixture = CreateFixture();
        await File.WriteAllTextAsync(
            Path.Combine(_workingDirectory, ".github", "PULL_REQUEST_TEMPLATE.md"),
            "Template body");
        using CreatePullRequestForm form = CreateForm(fixture, chooseRemote: "upstream", chooseBranch: "feature");
        CreatePullRequestForm.TestAccessor accessor = form.GetTestAccessor();

        await accessor.InitializeAsync().WaitAsync(TimeSpan.FromSeconds(5));
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        accessor.TargetRepositories.SelectedItem.Should().BeSameAs(fixture.TargetRemote);
        accessor.SourceBranches.SelectedItem.Should().Be("main");
        accessor.TargetBranches.SelectedItem.Should().Be("develop");
        accessor.Title.Should().Be("Suggested title");
        accessor.Body.Should().Be("Template body");
        accessor.CreateEnabled.Should().BeTrue();
    }

    [AvaloniaTest]
    public async Task CreatePullRequestForm_should_create_synchronously_with_selected_provider_values()
    {
        PullRequestFixture fixture = CreateFixture();
        using CreatePullRequestForm form = CreateForm(fixture, chooseRemote: "upstream", chooseBranch: "feature");
        CreatePullRequestForm.TestAccessor accessor = form.GetTestAccessor();
        await accessor.InitializeAsync().WaitAsync(TimeSpan.FromSeconds(5));
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));
        accessor.Title = "Portable PR";
        accessor.Body = "Created from Avalonia";

        accessor.Create();

        fixture.TargetRepository.Received(1).CreatePullRequest(
            "main",
            "develop",
            "Portable PR",
            "Created from Avalonia");
        _messageBoxHost.Messages.Should().Contain("Done");
    }

    [AvaloniaTest]
    public async Task CreatePullRequestForm_should_require_a_non_empty_title()
    {
        PullRequestFixture fixture = CreateFixture();
        using CreatePullRequestForm form = CreateForm(fixture, chooseRemote: "upstream", chooseBranch: "feature");
        CreatePullRequestForm.TestAccessor accessor = form.GetTestAccessor();
        await accessor.InitializeAsync().WaitAsync(TimeSpan.FromSeconds(5));
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));
        accessor.Title = "  ";

        accessor.Create();

        _messageBoxHost.Messages.Should().ContainSingle()
            .Which.Should().Be("You must specify a title.");
        fixture.TargetRepository.DidNotReceiveWithAnyArgs()
            .CreatePullRequest(default!, default!, default!, default!);
    }

    [AvaloniaTest]
    public async Task CreatePullRequestForm_should_report_a_provider_failure_and_restore_create()
    {
        PullRequestFixture fixture = CreateFixture();
        fixture.TargetRepository
            .When(repository => repository.CreatePullRequest("main", "develop", "Portable PR", "Body"))
            .Do(_ => throw new InvalidOperationException("provider failed"));
        using CreatePullRequestForm form = CreateForm(fixture, chooseRemote: "upstream", chooseBranch: "feature");
        CreatePullRequestForm.TestAccessor accessor = form.GetTestAccessor();
        await accessor.InitializeAsync().WaitAsync(TimeSpan.FromSeconds(5));
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));
        accessor.Title = "Portable PR";
        accessor.Body = "Body";

        accessor.Create();

        _messageBoxHost.Messages.Should().ContainSingle()
            .Which.Should().Be("Failed to create pull request." + Environment.NewLine + "provider failed");
        accessor.CreateEnabled.Should().BeTrue();
    }

    [AvaloniaTest]
    public void StartCreatePullRequest_should_open_provider_configuration_when_required()
    {
        IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
        host.ConfigurationOk.Returns(false);
        GitUICommands commands = new(_serviceContainer, Substitute.For<IGitModule>());

        commands.StartCreatePullRequest(owner: null, host);

        host.Received(1).Execute(Arg.Any<GitUIEventArgs>());
    }

    [AvaloniaTest]
    public void StartCreatePullRequest_should_route_the_single_relevant_host()
    {
        IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
        host.GitModuleIsRelevantToMe().Returns(true);
        host.ConfigurationOk.Returns(false);
        PluginRegistry.GitHosters.Add(host);
        GitUICommands commands = new(_serviceContainer, Substitute.For<IGitModule>());

        commands.StartCreatePullRequest(owner: null);

        host.Received(1).Execute(Arg.Any<GitUIEventArgs>());
    }

    [AvaloniaTest]
    public async Task AddUpstreamRemote_should_run_the_provider_under_the_browse_owner()
    {
        IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
        host.ConfigurationOk.Returns(true);
        host.AddUpstreamRemoteAsync().Returns(Task.FromResult<string?>(null));
        GitModule module = new(
            _serviceContainer.GetRequiredService<IGitExecutorProvider>(),
            _workingDirectory);
        GitUICommands commands = new(_serviceContainer, module);
        FormBrowse owner = new(commands);
        try
        {
            commands.AddUpstreamRemote(owner, host);
            await owner.JoinLoadOperationsForTestAsync()
                .WaitAsync(TimeSpan.FromSeconds(5));

            await host.Received(1).AddUpstreamRemoteAsync();
        }
        finally
        {
            owner.Close();
        }
    }

    private CreatePullRequestForm CreateForm(
        PullRequestFixture fixture,
        string? chooseRemote,
        string? chooseBranch)
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(_workingDirectory);
        module.IsValidGitWorkingDir().Returns(true);
        module.GetSelectedBranch().Returns("main");
        module.GetPreviousCommitMessages(
                count: 1,
                revision: "origin/main",
                authorPattern: string.Empty)
            .Returns(["Suggested title\nDetails"]);
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        return new CreatePullRequestForm(
            commands,
            fixture.Host,
            chooseRemote,
            chooseBranch);
    }

    private static PullRequestFixture CreateFixture()
    {
        IHostedRepository sourceRepository = CreateRepository(
            defaultBranch: "main",
            branches: ["main", "feature"]);
        IHostedRemote sourceRemote = CreateRemote(
            name: "origin",
            displayData: "owner/repository",
            isOwnedByMe: true,
            sourceRepository);
        IHostedRepository targetRepository = CreateRepository(
            defaultBranch: "develop",
            branches: ["main", "develop"]);
        IHostedRemote targetRemote = CreateRemote(
            name: "upstream",
            displayData: "project/repository",
            isOwnedByMe: false,
            targetRepository);
        IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
        host.GetHostedRemotesForModule().Returns([sourceRemote, targetRemote]);
        return new PullRequestFixture(
            host,
            sourceRemote,
            targetRemote,
            sourceRepository,
            targetRepository);
    }

    private static IHostedRepository CreateRepository(
        string defaultBranch,
        IReadOnlyList<string> branches)
    {
        IHostedRepository repository = Substitute.For<IHostedRepository>();
        repository.GetDefaultBranch().Returns(defaultBranch);
        IHostedBranch[] hostedBranches = branches.Select(
                name =>
                {
                    IHostedBranch branch = Substitute.For<IHostedBranch>();
                    branch.Name.Returns(name);
                    return branch;
                })
            .ToArray();
        repository.GetBranches().Returns(hostedBranches);
        return repository;
    }

    private static IHostedRemote CreateRemote(
        string name,
        string displayData,
        bool isOwnedByMe,
        IHostedRepository repository)
    {
        IHostedRemote remote = Substitute.For<IHostedRemote>();
        remote.Name.Returns(name);
        remote.DisplayData.Returns(displayData);
        remote.IsOwnedByMe.Returns(isOwnedByMe);
        remote.GetHostedRepository().Returns(repository);
        return remote;
    }

    private sealed record PullRequestFixture(
        IRepositoryHostPlugin Host,
        IHostedRemote SourceRemote,
        IHostedRemote TargetRemote,
        IHostedRepository SourceRepository,
        IHostedRepository TargetRepository);

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
