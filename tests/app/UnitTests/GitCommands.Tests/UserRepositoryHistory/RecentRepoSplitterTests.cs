using GitCommands.UserRepositoryHistory;

namespace GitCommandsTests.UserRepositoryHistory;
public class RecentRepoSplitterTests
{
    private const string _relativeLongRepoPath = @"this\is\a\very_very_very_very_very_very_very\long\repo_path";
    private static readonly string repoPathInUserFolder = Path.Combine(Path.GetTempPath(), _relativeLongRepoPath);
    private static readonly string repoAnchoredInTopPath1 = @"C:\this\is\a\repo_anchored_in_top_path1\";
    private static readonly string repoAnchoredInTopPath2 = @"C:\this\is\a\repo_anchored_in_top_path2\";
    private static readonly string repoAnchoredInRecentPath = @"C:\this\is\a\repo_anchored_in_recent_path\";
    private static readonly string repoNotAnchoredPath = @"C:\this\is\a\repo_not_anchored_path\";

    #region Shortening strategy
    [Test]
    public void SplitRecentRepos_Should_use_most_significant_folder_as_caption()
    {
        List<Repository> history =
        [
            new Repository(repoAnchoredInTopPath1) { Anchor = Repository.RepositoryAnchor.AnchoredInTop },
        ];

        RecentRepoSplitter sut = new()
        {
            ShorteningStrategy = GitCommands.ShorteningRecentRepoPathStrategy.MostSignDir
        };
        List<RecentRepoInfo> topRepoList = [];
        List<RecentRepoInfo> recentRepoList = [];

        sut.SplitRecentRepos(history, topRepoList, recentRepoList);

        topRepoList.Should().ContainSingle();
        topRepoList[0].Caption.Should().Be("repo_anchored_in_top_path1");
        recentRepoList.Should().ContainSingle();
    }

    [TestCase(@"\\wsl$\Ubuntu\home\user\repo\")]
    [TestCase(@"\\wsl.localhost\Ubuntu\home\user\repo\")]
    public void SplitRecentRepos_should_not_mark_unique_wsl_captions(string path)
    {
        List<Repository> history = [new Repository(path) { Anchor = Repository.RepositoryAnchor.AnchoredInTop }];
        RecentRepoSplitter sut = new() { ShorteningStrategy = GitCommands.ShorteningRecentRepoPathStrategy.MostSignDir };
        List<RecentRepoInfo> topRepoList = [];
        List<RecentRepoInfo> recentRepoList = [];

        sut.SplitRecentRepos(history, topRepoList, recentRepoList);

        topRepoList.Should().ContainSingle().Which.Caption.Should().Be("repo");
    }

    [TestCase(@"\\wsl$\Ubuntu\home\user\repo\", false)]
    [TestCase(@"\\wsl$\Ubuntu\home\user\repo\", true)]
    [TestCase(@"\\wsl.localhost\Ubuntu\home\user\repo\", false)]
    [TestCase(@"\\WSL$\Ubuntu\home\user\repo\", true)]
    public void SplitRecentRepos_should_mark_colliding_filesystems(string wslPath, bool reverseOrder)
    {
        const string windowsPath = @"X:\home\user\repo\";
        List<Repository> history =
        [
            new Repository(wslPath) { Anchor = Repository.RepositoryAnchor.AnchoredInTop },
            new Repository(windowsPath) { Anchor = Repository.RepositoryAnchor.AnchoredInTop }
        ];
        if (reverseOrder)
        {
            history.Reverse();
        }

        RecentRepoSplitter sut = new() { ShorteningStrategy = GitCommands.ShorteningRecentRepoPathStrategy.MostSignDir };
        List<RecentRepoInfo> topRepoList = [];
        List<RecentRepoInfo> recentRepoList = [];

        sut.SplitRecentRepos(history, topRepoList, recentRepoList);

        topRepoList.Single(repo => repo.Repo.Path == wslPath).Caption.Should().Be("repo (WSL)");
        topRepoList.Single(repo => repo.Repo.Path == windowsPath).Caption.Should().Be("repo (X:)");
    }

    [TestCase(@"X:\home\other\repo\", "repo (user)", "repo (other)", false)]
    [TestCase(@"X:\home\other\repo\", "repo (user)", "repo (other)", true)]
    [TestCase(@"X:\projects\user\repo\", @"repo (home\user)", @"repo (projects\user)", false)]
    [TestCase(@"X:\projects\user\repo\", @"repo (home\user)", @"repo (projects\user)", true)]
    public void SplitRecentRepos_should_not_mark_filesystems_with_distinct_path_suffixes(string windowsPath, string wslCaption, string windowsCaption, bool reverseOrder)
    {
        const string wslPath = @"\\wsl$\Ubuntu\home\user\repo\";
        List<Repository> history =
        [
            new Repository(wslPath) { Anchor = Repository.RepositoryAnchor.AnchoredInTop },
            new Repository(windowsPath) { Anchor = Repository.RepositoryAnchor.AnchoredInTop }
        ];
        if (reverseOrder)
        {
            history.Reverse();
        }

        RecentRepoSplitter sut = new() { ShorteningStrategy = GitCommands.ShorteningRecentRepoPathStrategy.MostSignDir };
        List<RecentRepoInfo> topRepoList = [];
        List<RecentRepoInfo> recentRepoList = [];

        sut.SplitRecentRepos(history, topRepoList, recentRepoList);

        topRepoList.Single(repo => repo.Repo.Path == wslPath).Caption.Should().Be(wslCaption);
        topRepoList.Single(repo => repo.Repo.Path == windowsPath).Caption.Should().Be(windowsCaption);
    }

    [TestCase(@"\\wsl$", false)]
    [TestCase(@"\\wsl$", true)]
    [TestCase(@"\\wsl.localhost", false)]
    [TestCase(@"\\wsl.localhost", true)]
    public void SplitRecentRepos_should_distinguish_wsl_distributions_by_path(string wslPrefix, bool includeWindowsRepo)
    {
        string ubuntuPath = $@"{wslPrefix}\Ubuntu\home\user\repo\";
        string debianPath = $@"{wslPrefix}\Debian\home\user\repo\";
        List<Repository> history =
        [
            new Repository(ubuntuPath) { Anchor = Repository.RepositoryAnchor.AnchoredInTop },
            new Repository(debianPath) { Anchor = Repository.RepositoryAnchor.AnchoredInTop }
        ];
        if (includeWindowsRepo)
        {
            history.Add(new Repository(@"X:\home\user\repo\") { Anchor = Repository.RepositoryAnchor.AnchoredInTop });
        }

        RecentRepoSplitter sut = new() { ShorteningStrategy = GitCommands.ShorteningRecentRepoPathStrategy.MostSignDir };
        List<RecentRepoInfo> topRepoList = [];
        List<RecentRepoInfo> recentRepoList = [];

        sut.SplitRecentRepos(history, topRepoList, recentRepoList);

        string fileSystemSuffix = includeWindowsRepo ? " (WSL)" : "";
        topRepoList.Single(repo => repo.Repo.Path == ubuntuPath).Caption.Should().Be($"repo (Ubuntu){fileSystemSuffix}");
        topRepoList.Single(repo => repo.Repo.Path == debianPath).Caption.Should().Be($"repo (Debian){fileSystemSuffix}");
        topRepoList.Select(repo => repo.Caption).Should().OnlyHaveUniqueItems();
    }

    [Test]
    public void SplitRecentRepos_Should_not_shorten_as_caption()
    {
        List<Repository> history =
        [
            new Repository(repoAnchoredInTopPath1) { Anchor = Repository.RepositoryAnchor.AnchoredInTop },
        ];

        RecentRepoSplitter sut = new()
        {
            ShorteningStrategy = GitCommands.ShorteningRecentRepoPathStrategy.None
        };
        List<RecentRepoInfo> topRepoList = [];
        List<RecentRepoInfo> recentRepoList = [];

        sut.SplitRecentRepos(history, topRepoList, recentRepoList);

        topRepoList.Should().ContainSingle();
        topRepoList[0].Caption.Should().Be(repoAnchoredInTopPath1);
        recentRepoList.Should().ContainSingle();
    }

    [Test]
    public void SplitRecentRepos_Should_not_shorten_but_handle_user_folder_as_caption()
    {
        List<Repository> history =
        [
            new Repository(repoPathInUserFolder) { Anchor = Repository.RepositoryAnchor.AnchoredInTop },
        ];

        RecentRepoSplitter sut = new()
        {
            ShorteningStrategy = GitCommands.ShorteningRecentRepoPathStrategy.None
        };
        List<RecentRepoInfo> topRepoList = [];
        List<RecentRepoInfo> recentRepoList = [];

        sut.SplitRecentRepos(history, topRepoList, recentRepoList);

        topRepoList.Should().ContainSingle();
        topRepoList[0].Caption.Should().StartWith(@"~\AppData").And.EndWith(_relativeLongRepoPath);
        recentRepoList.Should().ContainSingle();
    }

    [Test]
    public void SplitRecentRepos_Should_display_middle_dots_in_caption()
    {
        // Warning: Able to shorten only an existing folder path
        Directory.CreateDirectory(repoPathInUserFolder);

        List<Repository> history =
        [
            new Repository(repoPathInUserFolder) { Anchor = Repository.RepositoryAnchor.AnchoredInTop },
        ];

        RecentRepoSplitter sut = new()
        {
            ShorteningStrategy = GitCommands.ShorteningRecentRepoPathStrategy.MiddleDots
        };
        List<RecentRepoInfo> topRepoList = [];
        List<RecentRepoInfo> recentRepoList = [];

        sut.SplitRecentRepos(history, topRepoList, recentRepoList);

        topRepoList.Should().ContainSingle();
        topRepoList[0].Caption.Should().Be(@"~\AppData\..\long\repo_path");
        recentRepoList.Should().ContainSingle();
    }
    #endregion

    #region Split repositories
    [Test]
    public void SplitRecentRepos_Should_split_depending_anchor()
    {
        List<Repository> history =
        [
            new Repository(repoAnchoredInTopPath1) { Anchor = Repository.RepositoryAnchor.AnchoredInTop },
            new Repository(repoAnchoredInTopPath2) { Anchor = Repository.RepositoryAnchor.AnchoredInTop },
            new Repository(repoAnchoredInRecentPath) { Anchor = Repository.RepositoryAnchor.AnchoredInRecent },
            new Repository(repoNotAnchoredPath) { Anchor = Repository.RepositoryAnchor.None },
        ];

        RecentRepoSplitter sut = new()
        {
            ShorteningStrategy = GitCommands.ShorteningRecentRepoPathStrategy.MostSignDir,
            SortTopRepos = false,
            SortRecentRepos = false
        };
        List<RecentRepoInfo> topRepoList = [];
        List<RecentRepoInfo> recentRepoList = [];

        sut.SplitRecentRepos(history, topRepoList, recentRepoList);

        topRepoList.Should().HaveCount(2);
        topRepoList[0].Caption.Should().Be("repo_anchored_in_top_path1");
        topRepoList[1].Caption.Should().Be("repo_anchored_in_top_path2");
        recentRepoList.Should().HaveCount(4);
        recentRepoList[0].Caption.Should().Be("repo_anchored_in_top_path1");
        recentRepoList[1].Caption.Should().Be("repo_anchored_in_top_path2");
        recentRepoList[2].Caption.Should().Be("repo_anchored_in_recent_path");
        recentRepoList[3].Caption.Should().Be("repo_not_anchored_path");
    }

    [Test]
    public void SplitRecentRepos_Should_split_depending_anchor_and_sort_alphabetically()
    {
        List<Repository> history =
        [
            // Unsorted!
            new Repository(repoNotAnchoredPath) { Anchor = Repository.RepositoryAnchor.None },
            new Repository(repoAnchoredInRecentPath) { Anchor = Repository.RepositoryAnchor.AnchoredInRecent },
            new Repository(repoAnchoredInTopPath2) { Anchor = Repository.RepositoryAnchor.AnchoredInTop },
            new Repository(repoAnchoredInTopPath1) { Anchor = Repository.RepositoryAnchor.AnchoredInTop },
        ];

        RecentRepoSplitter sut = new()
        {
            ShorteningStrategy = GitCommands.ShorteningRecentRepoPathStrategy.MostSignDir,
            SortTopRepos = true,
            SortRecentRepos = true
        };
        List<RecentRepoInfo> topRepoList = [];
        List<RecentRepoInfo> recentRepoList = [];

        sut.SplitRecentRepos(history, topRepoList, recentRepoList);

        topRepoList.Should().HaveCount(2);
        topRepoList[0].Caption.Should().Be("repo_anchored_in_top_path1");
        topRepoList[1].Caption.Should().Be("repo_anchored_in_top_path2");
        recentRepoList.Should().HaveCount(4);
        recentRepoList[0].Caption.Should().Be("repo_anchored_in_recent_path");
        recentRepoList[1].Caption.Should().Be("repo_anchored_in_top_path1");
        recentRepoList[2].Caption.Should().Be("repo_anchored_in_top_path2");
        recentRepoList[3].Caption.Should().Be("repo_not_anchored_path");
    }

    [Test]
    public void SplitRecentRepos_Should_split_depending_anchor_and_sort_alphabetically_Hiding_Top_Repo_In_Recent_list()
    {
        List<Repository> history =
        [
            // Unsorted!
            new Repository(repoNotAnchoredPath) { Anchor = Repository.RepositoryAnchor.None },
            new Repository(repoAnchoredInRecentPath) { Anchor = Repository.RepositoryAnchor.AnchoredInRecent },
            new Repository(repoAnchoredInTopPath2) { Anchor = Repository.RepositoryAnchor.AnchoredInTop },
            new Repository(repoAnchoredInTopPath1) { Anchor = Repository.RepositoryAnchor.AnchoredInTop },
        ];

        RecentRepoSplitter sut = new()
        {
            ShorteningStrategy = GitCommands.ShorteningRecentRepoPathStrategy.MostSignDir,
            SortTopRepos = true,
            SortRecentRepos = true,
            HideTopRepositoriesFromRecentList = true
        };
        List<RecentRepoInfo> topRepoList = [];
        List<RecentRepoInfo> recentRepoList = [];

        sut.SplitRecentRepos(history, topRepoList, recentRepoList);

        topRepoList.Should().HaveCount(2);
        topRepoList[0].Caption.Should().Be("repo_anchored_in_top_path1");
        topRepoList[1].Caption.Should().Be("repo_anchored_in_top_path2");
        recentRepoList.Should().HaveCount(2);
        recentRepoList[0].Caption.Should().Be("repo_anchored_in_recent_path");
        recentRepoList[1].Caption.Should().Be("repo_not_anchored_path");
    }
    #endregion
}
