using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Plugins;
using GitExtensions.Extensibility.Settings;

namespace WinFormsParityCapture;

// parity-scaffolding: Gives all three repository-host dialogs one deterministic provider model.
internal static class RepositoryHostCaptureFixture
{
    public static IRepositoryHostPlugin Create(IGitUICommands commands)
    {
        ObjectId head = commands.Module.RevParse("HEAD");
        ObjectId parent = commands.Module.RevParse("HEAD~1");
        HostedRepository mine = new("contributor", "repository", isMine: true, head);
        HostedRepository target = new("gitextensions", "gitextensions", isMine: false, parent);
        PullRequest pullRequest = new(target, mine, parent, head);
        target.PullRequests = [pullRequest];
        return new RepositoryHostPlugin(
            [mine, target],
            [
                new HostedRemote("origin", target, isOwnedByMe: false),
                new HostedRemote("contributor", mine, isOwnedByMe: true),
            ]);
    }

    private sealed class RepositoryHostPlugin(
        IReadOnlyList<IHostedRepository> repositories,
        IReadOnlyList<IHostedRemote> remotes) : IRepositoryHostPlugin
    {
        public Guid Id { get; } = new("A312D913-9DA0-4655-9A80-6C65D55E2A2A");

        public string Name => "Parity Host";

        public string Description => "Deterministic repository-host capture provider";

        public Image? Icon => null;

        public IGitPluginSettingsContainer? SettingsContainer { get; set; }

        public bool HasSettings => false;

        public bool ConfigurationOk => true;

        public string OwnerLogin => "contributor";

        public IReadOnlyList<IHostedRepository> SearchForRepository(string search)
            => repositories.Where(repository => repository.Name.Contains(search, StringComparison.OrdinalIgnoreCase)).ToArray();

        public IReadOnlyList<IHostedRepository> GetRepositoriesOfUser(string user)
            => repositories.Where(repository => string.Equals(repository.Owner, user, StringComparison.OrdinalIgnoreCase)).ToArray();

        public IHostedRepository GetRepository(string user, string repositoryName)
            => repositories.Single(repository => repository.Owner == user && repository.Name == repositoryName);

        public IReadOnlyList<IHostedRepository> GetMyRepos()
            => repositories.Where(repository => repository.IsMine).ToArray();

        public void ConfigureContextMenu(ContextMenuStrip contextMenu)
        {
        }

        public bool GitModuleIsRelevantToMe() => true;

        public IReadOnlyList<IHostedRemote> GetHostedRemotesForModule() => remotes;

        public Task<string?> AddUpstreamRemoteAsync() => Task.FromResult<string?>("upstream");

        public IEnumerable<ISetting> GetSettings() => [];

        public void Register(IGitUICommands gitUiCommands)
        {
        }

        public void Unregister(IGitUICommands gitUiCommands)
        {
        }

        public bool Execute(GitUIEventArgs args) => false;
    }

    private sealed class HostedRemote(
        string name,
        IHostedRepository repository,
        bool isOwnedByMe) : IHostedRemote
    {
        public string Name => name;

        public string Data => $"{repository.Owner}/{repository.Name}";

        public string DisplayData => $"{Data} ({Name})";

        public bool IsOwnedByMe => isOwnedByMe;

        public string Owner => repository.Owner ?? string.Empty;

        public string RemoteRepositoryName => repository.Name;

        public string RemoteUrl => repository.CloneUrl;

        public GitProtocol CloneProtocol => repository.CloneProtocol;

        public IHostedRepository GetHostedRepository() => repository;

        public string GetBlameUrl(string commitHash, string fileName, int lineIndex)
            => $"{repository.Homepage}/blame/{commitHash}/{fileName}#L{lineIndex + 1}";
    }

    private sealed class HostedRepository(
        string owner,
        string name,
        bool isMine,
        ObjectId branchSha) : IHostedRepository
    {
        public string Owner => owner;

        public string Name => name;

        public string Description => "Representative repository used by the parity capture harness.";

        public bool IsAFork => isMine;

        public bool IsMine => isMine;

        public bool IsPrivate => false;

        public int Forks => 42;

        public string Homepage => $"https://example.test/{Owner}/{Name}";

        public string? ParentUrl => isMine ? "https://example.test/gitextensions/gitextensions" : null;

        public string? ParentOwner => isMine ? "gitextensions" : null;

        public string CloneUrl => $"https://example.test/{Owner}/{Name}.git";

        public GitProtocol CloneProtocol { get; set; } = GitProtocol.Https;

        public IReadOnlyList<GitProtocol> SupportedCloneProtocols { get; set; } = [GitProtocol.Https, GitProtocol.Ssh];

        public IReadOnlyList<IPullRequestInformation> PullRequests { get; set; } = [];

        public IReadOnlyList<IHostedBranch> GetBranches()
            => [new HostedBranch("main", branchSha), new HostedBranch("feature/visual-parity", branchSha)];

        public string GetDefaultBranch() => "main";

        public IHostedRepository Fork() => this;

        public IReadOnlyList<IPullRequestInformation> GetPullRequests() => PullRequests;

        public int CreatePullRequest(string myBranch, string remoteBranch, string title, string body) => 42;
    }

    private sealed class HostedBranch(string name, ObjectId sha) : IHostedBranch
    {
        public string Name => name;

        public ObjectId Sha => sha;
    }

    private sealed class PullRequest(
        IHostedRepository baseRepository,
        IHostedRepository headRepository,
        ObjectId baseSha,
        ObjectId headSha) : IPullRequestInformation
    {
        private readonly PullRequestDiscussion _discussion = new();

        public string Title => "Complete repository-host parity";

        public string Body => "Keep provider workflows aligned across desktop platforms.";

        public string Owner => "contributor";

        public DateTime Created => new(2026, 8, 8, 12, 0, 0, DateTimeKind.Utc);

        public IHostedRepository BaseRepo => baseRepository;

        public IHostedRepository HeadRepo => headRepository;

        public string BaseSha => baseSha.ToString();

        public string HeadSha => headSha.ToString();

        public string BaseRef => "main";

        public string HeadRef => "feature/visual-parity";

        public string Id => "42";

        public string DetailedInfo => "#42 Complete repository-host parity";

        public string FetchBranch => "pr/42";

        public Task<string> GetDiffDataAsync()
            => Task.FromResult(
                "diff --git a/src/App.cs b/src/App.cs\n"
                + "index 1111111..2222222 100644\n"
                + "--- a/src/App.cs\n"
                + "+++ b/src/App.cs\n"
                + "@@ -1 +1 @@\n"
                + "-old\n"
                + "+new\n");

        public void Close()
        {
        }

        public IPullRequestDiscussion GetDiscussion() => _discussion;
    }

    private sealed class PullRequestDiscussion : IPullRequestDiscussion
    {
        public List<IDiscussionEntry> Entries { get; } =
        [
            new DiscussionEntry(
                "Contributor",
                new DateTime(2026, 8, 8, 12, 30, 0, DateTimeKind.Utc),
                "The native discussion surface preserves multiline text.\nReady for review."),
            new CommitDiscussionEntry(
                "Git Extensions Team",
                new DateTime(2026, 8, 8, 13, 0, 0, DateTimeKind.Utc),
                "Updated the repository-host integration.",
                "2222222"),
        ];

        public void Post(string data)
        {
        }

        public void ForceReload()
        {
        }
    }

    private class DiscussionEntry(
        string author,
        DateTime created,
        string body) : IDiscussionEntry
    {
        public string Author => author;

        public DateTime Created => created;

        public string Body => body;
    }

    private sealed class CommitDiscussionEntry(
        string author,
        DateTime created,
        string body,
        string sha) : DiscussionEntry(author, created, body), ICommitDiscussionEntry
    {
        public string Sha => sha;
    }
}
