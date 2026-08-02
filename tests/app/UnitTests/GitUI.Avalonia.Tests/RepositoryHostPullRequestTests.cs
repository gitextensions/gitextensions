using System.ComponentModel.Design;
using Avalonia.Controls;
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
using GitUI.CommandsDialogs.RepoHosting;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class RepositoryHostPullRequestTests
{
    private const string BaseSha = "1111111111111111111111111111111111111111";
    private const string HeadSha = "2222222222222222222222222222222222222222";
    private const string Diff = """
        diff --git a/src/file.txt b/src/file.txt
        index 1111111..2222222 100644
        --- a/src/file.txt
        +++ b/src/file.txt
        @@ -1 +1 @@
        -old
        +new
        """;

    private ServiceContainer _serviceContainer = null!;

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
    }

    [TearDown]
    public void TearDown()
    {
        _serviceContainer.Dispose();
    }

    [AvaloniaTest]
    public void ViewPullRequestsForm_should_preserve_layout_and_translation_identities()
    {
        using ViewPullRequestsForm form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);
        form.TranslateItems(translation);

        form.Width.Should().Be(754);
        form.Height.Should().Be(511);
        form.FindControl<TabControl>("tabControl1")!.ItemCount.Should().Be(2);
        form.FindControl<FileStatusList>("_fileStatusList").Should().NotBeNull();
        form.FindControl<GitUI.Editor.FileViewer>("_diffViewer").Should().NotBeNull();
        form.FindControl<GitUI.SpellChecker.EditNetSpell>("_postCommentText").Should().NotBeNull();

        translation.Received(1).AddTranslationItem(
            nameof(ViewPullRequestsForm), "$this", "Text", "View Pull Requests");
        translation.Received(1).AddTranslationItem(
            nameof(ViewPullRequestsForm), "tabPage1", "Text", "Diffs");
        translation.Received(1).AddTranslationItem(
            nameof(ViewPullRequestsForm), "tabPage2", "Text", "Comments");
        translation.Received(1).AddTranslationItem(
            nameof(ViewPullRequestsForm), "columnHeaderHeading", "Text", "Heading");
        translation.Received(1).AddTranslationItem(
            nameof(ViewPullRequestsForm), "columnHeaderBy", "Text", "By");
        translation.Received(1).AddTranslationItem(
            nameof(ViewPullRequestsForm), "columnHeaderCreated", "Text", "Created");
        translation.Received(1).AddTranslationItem(
            nameof(ViewPullRequestsForm), "columnHeaderBranch", "Text", "Will be fetched to branch");
        translation.DidNotReceive().AddTranslationItem(
            nameof(ViewPullRequestsForm), "columnHeaderId", Arg.Any<string>(), Arg.Any<string>());
    }

    [AvaloniaTest]
    public async Task ViewPullRequestsForm_should_load_current_hosted_remote_and_pull_requests()
    {
        IPullRequestInformation pullRequest = CreatePullRequest();
        IHostedRepository repository = CreateRepository(pullRequest);
        IHostedRemote remote = Substitute.For<IHostedRemote>();
        remote.Name.Returns("origin");
        remote.DisplayData.Returns("owner/repository (origin)");
        remote.GetHostedRepository().Returns(repository);
        IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
        host.GetHostedRemotesForModule().Returns([remote]);
        IGitModule module = Substitute.For<IGitModule>();
        module.GetCurrentRemote().Returns("origin");
        module.GetRemotesAsync().Returns([]);

        using ViewPullRequestsForm form = CreateForm(host, module);
        ViewPullRequestsForm.TestAccessor accessor = form.GetTestAccessor();

        await accessor.InitializeAsync().WaitAsync(TimeSpan.FromSeconds(5));
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        accessor.HostedRepositories.ItemCount.Should().Be(1);
        accessor.HostedRepositories.SelectedIndex.Should().Be(0);
        accessor.PullRequests.ItemCount.Should().Be(1);
        accessor.DiffItems.Should().ContainSingle(item => item.Name == "src/file.txt");
    }

    [AvaloniaTest]
    public async Task ViewPullRequestsForm_should_load_diff_and_native_discussion_rows()
    {
        IPullRequestInformation pullRequest = CreatePullRequest();
        using ViewPullRequestsForm form = CreateForm(
            Substitute.For<IRepositoryHostPlugin>(),
            Substitute.For<IGitModule>());
        ViewPullRequestsForm.TestAccessor accessor = form.GetTestAccessor();

        accessor.SelectPullRequest(pullRequest);
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        accessor.DiffItems.Should().ContainSingle(item => item.Name == "src/file.txt");
        accessor.Discussion.ItemCount.Should().Be(1);
        pullRequest.HeadRepo.Received().CloneProtocol = GitProtocol.Https;
    }

    [AvaloniaTest]
    public async Task ViewPullRequestsForm_should_post_and_refresh_comments()
    {
        IPullRequestInformation pullRequest = CreatePullRequest();
        IPullRequestDiscussion discussion = pullRequest.GetDiscussion();
        using ViewPullRequestsForm form = CreateForm(
            Substitute.For<IRepositoryHostPlugin>(),
            Substitute.For<IGitModule>());
        ViewPullRequestsForm.TestAccessor accessor = form.GetTestAccessor();
        accessor.SelectPullRequest(pullRequest);
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        accessor.Comment = "Looks good";
        accessor.PostComment();
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        discussion.Received(1).Post("Looks good");
        discussion.Received(1).ForceReload();
        accessor.Comment.Should().BeEmpty();

        accessor.RefreshDiscussion();
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));
        discussion.Received(2).ForceReload();
    }

    [Test]
    public void ViewPullRequestsForm_should_split_provider_diff_into_file_rows()
    {
        IReadOnlyList<GitItemStatus> items = ViewPullRequestsForm.TestAccessor.ParseDiffForTesting(
            Diff,
            BaseSha,
            HeadSha);

        items.Should().ContainSingle();
        items[0].Name.Should().Be("src/file.txt");
        items[0].IsChanged.Should().BeTrue();
        items[0].IsTracked.Should().BeTrue();
    }

    [AvaloniaTest]
    public void StartPullRequestsDialog_should_open_provider_configuration_when_required()
    {
        IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
        host.ConfigurationOk.Returns(false);
        GitUICommands commands = new(_serviceContainer, Substitute.For<IGitModule>());

        commands.StartPullRequestsDialog(owner: null, host);

        host.Received(1).Execute(Arg.Any<GitUIEventArgs>());
    }

    private static ViewPullRequestsForm CreateForm(IRepositoryHostPlugin host, IGitModule module)
    {
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        return new ViewPullRequestsForm(commands, host);
    }

    private static IHostedRepository CreateRepository(IPullRequestInformation pullRequest)
    {
        IHostedRepository repository = Substitute.For<IHostedRepository>();
        repository.GetPullRequests().Returns([pullRequest]);
        return repository;
    }

    private static IPullRequestInformation CreatePullRequest()
    {
        IHostedRepository headRepository = Substitute.For<IHostedRepository>();
        headRepository.CloneUrl.Returns("https://example.test/contributor/repository.git");

        IDiscussionEntry entry = Substitute.For<IDiscussionEntry>();
        entry.Author.Returns("Contributor");
        entry.Created.Returns(new DateTime(2026, 7, 27, 12, 0, 0, DateTimeKind.Utc));
        entry.Body.Returns("Discussion body");
        IPullRequestDiscussion discussion = Substitute.For<IPullRequestDiscussion>();
        discussion.Entries.Returns([entry]);

        IPullRequestInformation pullRequest = Substitute.For<IPullRequestInformation>();
        pullRequest.Id.Returns("42");
        pullRequest.Title.Returns("Portable pull request viewer");
        pullRequest.Owner.Returns("contributor");
        pullRequest.Created.Returns(new DateTime(2026, 7, 27, 11, 0, 0, DateTimeKind.Utc));
        pullRequest.FetchBranch.Returns("pr/42");
        pullRequest.BaseSha.Returns(BaseSha);
        pullRequest.HeadSha.Returns(HeadSha);
        pullRequest.HeadRef.Returns("feature");
        pullRequest.HeadRepo.Returns(headRepository);
        pullRequest.GetDiffDataAsync().Returns(Task.FromResult(Diff));
        pullRequest.GetDiscussion().Returns(discussion);
        return pullRequest;
    }
}
