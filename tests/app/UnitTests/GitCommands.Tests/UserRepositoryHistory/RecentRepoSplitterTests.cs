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

    [TestCase(@"\\wsl$\Ubuntu\home\user\repo\", "repo (wsl)", false)]
    [TestCase(@"\\wsl$\Ubuntu\home\user\repo\", "repo (wsl)", true)]
    [TestCase(@"\\wsl.localhost\Ubuntu\home\user\repo\", "repo (wsl)", false)]
    [TestCase(@"\\WSL$\Ubuntu\home\user\repo\", "repo (WSL)", true)]
    public void SplitRecentRepos_should_mark_colliding_filesystems(string wslPath, string expectedCaption, bool reverseOrder)
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

        topRepoList.Single(repo => repo.Repo.Path == wslPath).Caption.Should().Be(expectedCaption);
        topRepoList.Single(repo => repo.Repo.Path == windowsPath).Caption.Should().Be("repo (X:)");
    }

    [TestCase(@"\\wsl$\Ubuntu\home\user\repo\", @"\\wsl$\Debian\home\user\repo\", "repo (Ubuntu)", "repo (Debian)")]
    [TestCase(@"\\srv.a.tld\share\home\user\repo\", @"\\srv.b.tld\share\home\user\repo\", "repo (a)", "repo (b)")]
    [TestCase(@"\\srv\one\home\user\repo\", @"\\srv\two\home\user\repo\", "repo (one)", "repo (two)")]
    public void SplitRecentRepos_should_omit_common_server_parts_of_unc_labels(string path1, string path2, string caption1, string caption2)
    {
        List<Repository> history =
        [
            new Repository(path1) { Anchor = Repository.RepositoryAnchor.AnchoredInTop },
            new Repository(path2) { Anchor = Repository.RepositoryAnchor.AnchoredInTop }
        ];
        RecentRepoSplitter sut = new() { ShorteningStrategy = GitCommands.ShorteningRecentRepoPathStrategy.MostSignDir };
        List<RecentRepoInfo> topRepoList = [];
        List<RecentRepoInfo> recentRepoList = [];

        sut.SplitRecentRepos(history, topRepoList, recentRepoList);

        topRepoList.Single(repo => repo.Repo.Path == path1).Caption.Should().Be(caption1);
        topRepoList.Single(repo => repo.Repo.Path == path2).Caption.Should().Be(caption2);
    }

    [Test]
    public void SplitRecentRepos_should_extend_unc_labels_until_unique()
    {
        string[] paths =
        [
            @"\\srv.x\s1\home\user\repo\",
            @"\\srv.x\s2\home\user\repo\",
            @"\\srv.y\s1\home\user\repo\"
        ];
        List<Repository> history = [.. paths.Select(path => new Repository(path) { Anchor = Repository.RepositoryAnchor.AnchoredInTop })];
        RecentRepoSplitter sut = new() { ShorteningStrategy = GitCommands.ShorteningRecentRepoPathStrategy.MostSignDir };
        List<RecentRepoInfo> topRepoList = [];
        List<RecentRepoInfo> recentRepoList = [];

        sut.SplitRecentRepos(history, topRepoList, recentRepoList);

        topRepoList.Single(repo => repo.Repo.Path == paths[0]).Caption.Should().Be(@"repo (x\s1)");
        topRepoList.Single(repo => repo.Repo.Path == paths[1]).Caption.Should().Be(@"repo (x\s2)");
        topRepoList.Single(repo => repo.Repo.Path == paths[2]).Caption.Should().Be(@"repo (y\s1)");
    }

    [Test]
    public void SplitRecentRepos_should_label_drive_and_unc_with_same_suffix()
    {
        const string drivePath = @"W:\home\user\repo\";
        const string uncPath = @"\\server.domain.tld\share\home\user\repo\";
        List<Repository> history =
        [
            new Repository(drivePath) { Anchor = Repository.RepositoryAnchor.AnchoredInTop },
            new Repository(uncPath) { Anchor = Repository.RepositoryAnchor.AnchoredInTop }
        ];
        RecentRepoSplitter sut = new() { ShorteningStrategy = GitCommands.ShorteningRecentRepoPathStrategy.MostSignDir };
        List<RecentRepoInfo> topRepoList = [];
        List<RecentRepoInfo> recentRepoList = [];

        sut.SplitRecentRepos(history, topRepoList, recentRepoList);

        topRepoList.Single(repo => repo.Repo.Path == drivePath).Caption.Should().Be("repo (W:)");
        topRepoList.Single(repo => repo.Repo.Path == uncPath).Caption.Should().Be("repo (server)");
    }

    [TestCase(false)]
    [TestCase(true)]
    public void SplitRecentRepos_should_mark_colliding_drives(bool reverseOrder)
    {
        const string pathX = @"X:\home\user\repo\";
        const string pathY = @"Y:\home\user\repo\";
        List<Repository> history =
        [
            new Repository(pathX) { Anchor = Repository.RepositoryAnchor.AnchoredInTop },
            new Repository(pathY) { Anchor = Repository.RepositoryAnchor.AnchoredInTop }
        ];
        if (reverseOrder)
        {
            history.Reverse();
        }

        RecentRepoSplitter sut = new() { ShorteningStrategy = GitCommands.ShorteningRecentRepoPathStrategy.MostSignDir };
        List<RecentRepoInfo> topRepoList = [];
        List<RecentRepoInfo> recentRepoList = [];

        sut.SplitRecentRepos(history, topRepoList, recentRepoList);

        topRepoList.Single(repo => repo.Repo.Path == pathX).Caption.Should().Be("repo (X:)");
        topRepoList.Single(repo => repo.Repo.Path == pathY).Caption.Should().Be("repo (Y:)");
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

    [TestCase(@"\\wsl$", "wsl", false)]
    [TestCase(@"\\wsl$", "wsl", true)]
    [TestCase(@"\\wsl.localhost", "wsl.localhost", false)]
    [TestCase(@"\\wsl.localhost", "wsl.localhost", true)]
    public void SplitRecentRepos_should_distinguish_wsl_distributions_by_path(string wslPrefix, string expectedWsl, bool includeWindowsRepo)
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

        string ubuntuCaption = includeWindowsRepo ? $@"repo ({expectedWsl}\Ubuntu)" : "repo (Ubuntu)";
        string debianCaption = includeWindowsRepo ? $@"repo ({expectedWsl}\Debian)" : "repo (Debian)";
        topRepoList.Single(repo => repo.Repo.Path == ubuntuPath).Caption.Should().Be(ubuntuCaption);
        topRepoList.Single(repo => repo.Repo.Path == debianPath).Caption.Should().Be(debianCaption);
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
