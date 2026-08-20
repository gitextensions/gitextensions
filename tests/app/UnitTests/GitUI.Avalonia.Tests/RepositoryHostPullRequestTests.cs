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
    public async Task ViewPullRequestsForm_should_skip_broken_and_empty_remotes_during_first_load()
    {
        IHostedRemote brokenRemote = Substitute.For<IHostedRemote>();
        brokenRemote.Name.Returns("origin");
        brokenRemote.DisplayData.Returns("broken/repository (origin)");
        brokenRemote.GetHostedRepository().Returns(_ => throw new InvalidOperationException("remote failed"));
        IHostedRepository emptyRepository = CreateRepository();
        IHostedRemote emptyRemote = CreateRemote("empty", emptyRepository);
        IPullRequestInformation pullRequest = CreatePullRequest();
        IHostedRemote populatedRemote = CreateRemote("populated", CreateRepository(pullRequest));
        IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
        host.GetHostedRemotesForModule().Returns([brokenRemote, emptyRemote, populatedRemote]);
        IGitModule module = Substitute.For<IGitModule>();
        module.GetCurrentRemote().Returns("origin");
        module.GetRemotesAsync().Returns([]);
        using ViewPullRequestsForm form = CreateForm(host, module);
        ViewPullRequestsForm.TestAccessor accessor = form.GetTestAccessor();

        await accessor.InitializeAsync().WaitAsync(TimeSpan.FromSeconds(5));
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        accessor.HostedRepositories.SelectedIndex.Should().Be(2);
        accessor.PullRequestTitles.Should().Equal("Portable pull request viewer");
        _messageBoxHost.Messages.Should().ContainSingle()
            .Which.Should().Contain("remote failed");
    }

    [AvaloniaTest]
    public async Task ViewPullRequestsForm_should_discard_a_superseded_remote_load()
    {
        TaskCompletionSource firstLoadStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource releaseFirstLoad = new(TaskCreationOptions.RunContinuationsAsynchronously);
        IPullRequestInformation stalePullRequest = CreatePullRequest();
        stalePullRequest.Title.Returns("Stale pull request");
        IHostedRepository firstRepository = Substitute.For<IHostedRepository>();
        firstRepository.GetPullRequests().Returns(_ =>
        {
            firstLoadStarted.TrySetResult();
            releaseFirstLoad.Task.GetAwaiter().GetResult();
            return [stalePullRequest];
        });
        IPullRequestInformation currentPullRequest = CreatePullRequest();
        currentPullRequest.Title.Returns("Current pull request");
        IHostedRemote firstRemote = CreateRemote("origin", firstRepository);
        IHostedRemote secondRemote = CreateRemote("upstream", CreateRepository(currentPullRequest));
        IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
        host.GetHostedRemotesForModule().Returns([firstRemote, secondRemote]);
        IGitModule module = Substitute.For<IGitModule>();
        module.GetCurrentRemote().Returns("origin");
        module.GetRemotesAsync().Returns([]);
        using ViewPullRequestsForm form = CreateForm(host, module);
        ViewPullRequestsForm.TestAccessor accessor = form.GetTestAccessor();

        await accessor.InitializeAsync().WaitAsync(TimeSpan.FromSeconds(5));
        await firstLoadStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        accessor.SelectHostedRepository(1);
        releaseFirstLoad.TrySetResult();
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        accessor.HostedRepositories.SelectedIndex.Should().Be(1);
        accessor.PullRequestTitles.Should().Equal("Current pull request");
        _messageBoxHost.Messages.Should().BeEmpty();
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

    [AvaloniaTest]
    public async Task ViewPullRequestsForm_should_cancel_discussion_refresh_when_the_hosted_repository_clears()
    {
        TaskCompletionSource refreshStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource releaseRefresh = new(TaskCreationOptions.RunContinuationsAsynchronously);
        IPullRequestInformation pullRequest = CreatePullRequest();
        IPullRequestDiscussion discussion = pullRequest.GetDiscussion();
        discussion.When(candidate => candidate.ForceReload()).Do(_ =>
        {
            refreshStarted.TrySetResult();
            releaseRefresh.Task.GetAwaiter().GetResult();
        });
        using ViewPullRequestsForm form = CreateForm(
            Substitute.For<IRepositoryHostPlugin>(),
            Substitute.For<IGitModule>());
        ViewPullRequestsForm.TestAccessor accessor = form.GetTestAccessor();
        accessor.SelectPullRequest(pullRequest);
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        accessor.RefreshDiscussion();
        await refreshStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        accessor.ClearHostedRepositorySelection();
        releaseRefresh.TrySetResult();
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        accessor.Discussion.ItemCount.Should().Be(0);
        accessor.FetchEnabled.Should().BeFalse();
        accessor.AddAndFetchEnabled.Should().BeFalse();
        accessor.CloseEnabled.Should().BeFalse();
        accessor.RefreshEnabled.Should().BeFalse();
        accessor.PostEnabled.Should().BeFalse();
        _messageBoxHost.Messages.Should().BeEmpty();
    }

    [AvaloniaTest]
    public async Task ViewPullRequestsForm_should_cancel_diff_loading_when_pull_request_selection_clears()
    {
        TaskCompletionSource<string> diffSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        IPullRequestInformation pullRequest = CreatePullRequest();
        pullRequest.GetDiffDataAsync().Returns(diffSource.Task);
        using ViewPullRequestsForm form = CreateForm(
            Substitute.For<IRepositoryHostPlugin>(),
            Substitute.For<IGitModule>());
        ViewPullRequestsForm.TestAccessor accessor = form.GetTestAccessor();

        accessor.SelectPullRequest(pullRequest);
        accessor.ClearPullRequestSelection();
        diffSource.TrySetResult(Diff);
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        accessor.DiffItems.Should().BeEmpty();
        accessor.Discussion.ItemCount.Should().Be(0);
        accessor.FetchEnabled.Should().BeFalse();
        _messageBoxHost.Messages.Should().BeEmpty();
    }

    [AvaloniaTest]
    public async Task ViewPullRequestsForm_should_ignore_an_empty_comment()
    {
        IPullRequestInformation pullRequest = CreatePullRequest();
        IPullRequestDiscussion discussion = pullRequest.GetDiscussion();
        using ViewPullRequestsForm form = CreateForm(
            Substitute.For<IRepositoryHostPlugin>(),
            Substitute.For<IGitModule>());
        ViewPullRequestsForm.TestAccessor accessor = form.GetTestAccessor();
        accessor.SelectPullRequest(pullRequest);
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        accessor.Comment = "   ";
        accessor.PostComment();
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        discussion.DidNotReceive().Post(Arg.Any<string>());
        accessor.Comment.Should().Be("   ");
        accessor.PostEnabled.Should().BeTrue();
    }

    [AvaloniaTest]
    public async Task ViewPullRequestsForm_should_close_the_selected_pull_request_and_reload()
    {
        IPullRequestInformation pullRequest = CreatePullRequest();
        using ViewPullRequestsForm form = CreateForm(
            Substitute.For<IRepositoryHostPlugin>(),
            Substitute.For<IGitModule>());
        ViewPullRequestsForm.TestAccessor accessor = form.GetTestAccessor();
        accessor.SelectPullRequest(pullRequest);
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        accessor.ClosePullRequest();
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        pullRequest.Received(1).Close();
    }

    [AvaloniaTest]
    public async Task ViewPullRequestsForm_should_report_a_close_failure_and_restore_the_action()
    {
        IPullRequestInformation pullRequest = CreatePullRequest();
        pullRequest.When(candidate => candidate.Close())
            .Do(_ => throw new InvalidOperationException("close failed"));
        using ViewPullRequestsForm form = CreateForm(
            Substitute.For<IRepositoryHostPlugin>(),
            Substitute.For<IGitModule>());
        ViewPullRequestsForm.TestAccessor accessor = form.GetTestAccessor();
        accessor.SelectPullRequest(pullRequest);
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        accessor.ClosePullRequest();
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        _messageBoxHost.Messages.Should().ContainSingle()
            .Which.Should().Be("Failed to close pull request!" + Environment.NewLine + "close failed");
        accessor.CloseEnabled.Should().BeTrue();
    }

    [AvaloniaTest]
    public async Task ViewPullRequestsForm_should_report_a_discussion_load_failure()
    {
        IPullRequestInformation pullRequest = CreatePullRequest();
        pullRequest.GetDiscussion().Returns(_ => throw new InvalidOperationException("discussion failed"));
        using ViewPullRequestsForm form = CreateForm(
            Substitute.For<IRepositoryHostPlugin>(),
            Substitute.For<IGitModule>());
        ViewPullRequestsForm.TestAccessor accessor = form.GetTestAccessor();

        accessor.SelectPullRequest(pullRequest);
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        _messageBoxHost.Messages.Should().ContainSingle()
            .Which.Should().Be("Could not load discussion!" + Environment.NewLine + "discussion failed");
        accessor.Discussion.ItemCount.Should().Be(0);
    }

    [AvaloniaTest]
    public async Task ViewPullRequestsForm_should_report_a_post_failure_and_keep_the_comment()
    {
        IPullRequestInformation pullRequest = CreatePullRequest();
        IPullRequestDiscussion discussion = pullRequest.GetDiscussion();
        discussion.When(candidate => candidate.Post("Keep this comment"))
            .Do(_ => throw new InvalidOperationException("post failed"));
        using ViewPullRequestsForm form = CreateForm(
            Substitute.For<IRepositoryHostPlugin>(),
            Substitute.For<IGitModule>());
        ViewPullRequestsForm.TestAccessor accessor = form.GetTestAccessor();
        accessor.SelectPullRequest(pullRequest);
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        accessor.Comment = "Keep this comment";
        accessor.PostComment();
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        _messageBoxHost.Messages.Should().ContainSingle()
            .Which.Should().Be("Failed to post discussion item!" + Environment.NewLine + "post failed");
        accessor.Comment.Should().Be("Keep this comment");
        accessor.PostEnabled.Should().BeTrue();
        accessor.RefreshEnabled.Should().BeTrue();
    }

    [AvaloniaTest]
    public async Task ViewPullRequestsForm_should_fetch_the_selected_pull_request_into_its_local_branch()
    {
        string root = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.PullRequestFetch-{Guid.NewGuid():N}");
        string sourceDirectory = Path.Combine(root, "source");
        string targetDirectory = Path.Combine(root, "target");
        try
        {
            GitModule sourceModule = CreateCommittedRepository(sourceDirectory, "feature");
            GitModule targetModule = CreateCommittedRepository(targetDirectory, "main");
            ILockableNotifier notifier = Substitute.For<ILockableNotifier>();
            IGitUICommands commands = CreateCommands(targetModule, notifier);
            commands.StartGitCommandProcessDialog(
                    Arg.Any<WinFormsShims.IWin32Window>(),
                    Arg.Any<ArgumentString>())
                .Returns(call => targetModule.GitExecutable.RunCommand(call.ArgAt<ArgumentString>(1)));
            IPullRequestInformation pullRequest = CreatePullRequest(sourceDirectory);
            using ViewPullRequestsForm form = new(commands, Substitute.For<IRepositoryHostPlugin>());
            ViewPullRequestsForm.TestAccessor accessor = form.GetTestAccessor();
            accessor.SelectPullRequest(pullRequest);
            await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));
            accessor.FetchEnabled.Should().BeTrue();

            accessor.FetchPullRequest();

            targetModule.GetRefs(RefsFilter.Heads)
                .Select(gitRef => gitRef.LocalName)
                .Should().Contain("pr/42");
            notifier.Received(1).Notify();
        }
        finally
        {
            TestDirectory.Delete(root);
        }
    }

    [AvaloniaTest]
    public async Task ViewPullRequestsForm_should_add_remote_fetch_and_checkout_the_selected_pull_request()
    {
        string root = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.PullRequestCheckout-{Guid.NewGuid():N}");
        string sourceDirectory = Path.Combine(root, "source");
        string targetDirectory = Path.Combine(root, "target");
        try
        {
            GitModule sourceModule = CreateCommittedRepository(sourceDirectory, "feature");
            GitModule targetModule = CreateCommittedRepository(targetDirectory, "main");
            ILockableNotifier notifier = Substitute.For<ILockableNotifier>();
            IGitUICommands commands = CreateCommands(targetModule, notifier);
            commands.StartGitCommandProcessDialog(
                    Arg.Any<WinFormsShims.IWin32Window>(),
                    Arg.Any<ArgumentString>())
                .Returns(call => targetModule.GitExecutable.RunCommand(call.ArgAt<ArgumentString>(1)));
            IPullRequestInformation pullRequest = CreatePullRequest(sourceDirectory);
            using ViewPullRequestsForm form = new(commands, Substitute.For<IRepositoryHostPlugin>());
            ViewPullRequestsForm.TestAccessor accessor = form.GetTestAccessor();
            accessor.SelectPullRequest(pullRequest);
            await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));
            accessor.AddAndFetchEnabled.Should().BeTrue();

            accessor.AddRemoteFetchAndCheckout();

            IReadOnlyList<Remote> remotes = await targetModule.GetRemotesAsync();
            Remote contributor = remotes.Should().ContainSingle(remote => remote.Name == "contributor").Which;
            Path.GetFullPath(contributor.FetchUrl).Should().Be(Path.GetFullPath(sourceDirectory));
            targetModule.GetCurrentCheckout().Should().Be(sourceModule.GetCurrentCheckout());
            notifier.Received(1).Lock();
            notifier.Received(3).Notify();
            notifier.Received(1).UnLock(false);
        }
        finally
        {
            TestDirectory.Delete(root);
        }
    }

    [AvaloniaTest]
    public async Task ViewPullRequestsForm_should_not_notify_when_fetch_is_cancelled_or_fails()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.FetchCmd(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), fetchTags: false)
            .Returns((ArgumentString)"fetch-command");
        ILockableNotifier notifier = Substitute.For<ILockableNotifier>();
        IGitUICommands commands = CreateCommands(module, notifier);
        commands.StartGitCommandProcessDialog(
                Arg.Any<WinFormsShims.IWin32Window>(),
                Arg.Any<ArgumentString>())
            .Returns(false);
        using ViewPullRequestsForm form = new(commands, Substitute.For<IRepositoryHostPlugin>());
        ViewPullRequestsForm.TestAccessor accessor = form.GetTestAccessor();
        accessor.SelectPullRequest(CreatePullRequest());
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        accessor.FetchPullRequest();

        notifier.DidNotReceive().Notify();
    }

    [AvaloniaTest]
    public async Task ViewPullRequestsForm_should_report_add_remote_failure_and_unlock_notifications()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.AddRemote("contributor", "https://example.test/contributor/repository.git")
            .Returns("add failed");
        ILockableNotifier notifier = Substitute.For<ILockableNotifier>();
        IGitUICommands commands = CreateCommands(module, notifier);
        using ViewPullRequestsForm form = new(commands, Substitute.For<IRepositoryHostPlugin>());
        ViewPullRequestsForm.TestAccessor accessor = form.GetTestAccessor();
        accessor.SelectPullRequest(CreatePullRequest());
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        accessor.AddRemoteFetchAndCheckout();

        _messageBoxHost.Messages.Should().ContainSingle().Which.Should().Be("add failed");
        commands.DidNotReceive().StartGitCommandProcessDialog(
            Arg.Any<WinFormsShims.IWin32Window>(),
            Arg.Any<ArgumentString>());
        notifier.Received(1).Lock();
        notifier.Received(1).UnLock(false);
        notifier.DidNotReceive().Notify();
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

    [Test]
    public void ViewPullRequestsForm_should_reject_an_invalid_head_revision()
    {
        Action action = () => ViewPullRequestsForm.TestAccessor.ParseDiffForTesting(
            Diff,
            BaseSha,
            "not-an-object-id");

        action.Should().Throw<InvalidDataException>();
    }

    [Test]
    public void ViewPullRequestsForm_should_reject_an_unrecognised_file_patch()
    {
        const string malformedDiff = "diff --git this is not a file header with enough content";

        Action action = () => ViewPullRequestsForm.TestAccessor.ParseDiffForTesting(
            malformedDiff,
            BaseSha,
            HeadSha);

        action.Should().Throw<InvalidDataException>();
    }

    [Test]
    public void DiscussionHtmlCreator_should_project_comment_and_commit_entries_to_native_rows()
    {
        IDiscussionEntry comment = Substitute.For<IDiscussionEntry>();
        comment.Author.Returns((string?)null);
        comment.Created.Returns(new DateTime(2026, 7, 27, 12, 0, 0, DateTimeKind.Utc));
        comment.Body.Returns("First line\nSecond line");
        ICommitDiscussionEntry commit = Substitute.For<ICommitDiscussionEntry>();
        commit.Author.Returns("Contributor");
        commit.Created.Returns(new DateTime(2026, 7, 27, 13, 0, 0, DateTimeKind.Utc));
        commit.Body.Returns((string?)null);
        commit.Sha.Returns((string?)null);

        IReadOnlyList<DiscussionHtmlCreator.DiscussionEntryPresentation> rows =
            DiscussionHtmlCreator.CreateFor([comment, commit]);

        rows.Should().Equal(
            new DiscussionHtmlCreator.DiscussionEntryPresentation(
                "[UNKNOWN]",
                comment.Created.ToString(),
                "First line\nSecond line",
                null),
            new DiscussionHtmlCreator.DiscussionEntryPresentation(
                "Contributor",
                commit.Created.ToString(),
                "[UNKNOWN]",
                "[UNKNOWN]"));
        DiscussionHtmlCreator.CreateFor().Should().BeEmpty();
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

    private IGitUICommands CreateCommands(IGitModule module, ILockableNotifier notifier)
    {
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.RepoChangedNotifier.Returns(notifier);
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
        File.WriteAllText(Path.Combine(workingDirectory, "tracked.txt"), branch);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "tracked.txt" }).Should().BeTrue();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" }).Should().BeTrue();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("branch") { "-M", branch }).Should().BeTrue();
        return module;
    }

    private static IHostedRepository CreateRepository(IPullRequestInformation pullRequest)
    {
        IHostedRepository repository = Substitute.For<IHostedRepository>();
        repository.GetPullRequests().Returns([pullRequest]);
        return repository;
    }

    private static IHostedRepository CreateRepository(params IPullRequestInformation[] pullRequests)
    {
        IHostedRepository repository = Substitute.For<IHostedRepository>();
        repository.GetPullRequests().Returns(pullRequests);
        return repository;
    }

    private static IHostedRemote CreateRemote(string name, IHostedRepository repository)
    {
        IHostedRemote remote = Substitute.For<IHostedRemote>();
        remote.Name.Returns(name);
        remote.DisplayData.Returns($"owner/repository ({name})");
        remote.GetHostedRepository().Returns(repository);
        return remote;
    }

    private static IPullRequestInformation CreatePullRequest(string? cloneUrl = null)
    {
        IHostedRepository headRepository = Substitute.For<IHostedRepository>();
        headRepository.CloneUrl.Returns(cloneUrl ?? "https://example.test/contributor/repository.git");

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
