using FluentAssertions;
using GitCommands.UserRepositoryHistory;

namespace GitCommandsTests.UserRepositoryHistory;

[TestFixture]
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

        RecentRepoSplitter sut = new();

        sut.ShorteningStrategy = GitCommands.ShorteningRecentRepoPathStrategy.MostSignDir;
        List<RecentRepoInfo> topRepoList = new();
        List<RecentRepoInfo> recentRepoList = new();

        sut.SplitRecentRepos(history, topRepoList, recentRepoList);

        topRepoList.Count.Should().Be(1);
        topRepoList[0].Caption.Should().Be("repo_anchored_in_top_path1");
        recentRepoList.Count.Should().Be(1);
    }

    [Test]
    public void SplitRecentRepos_Should_not_shorten_as_caption()
    {
        List<Repository> history =
        [
            new Repository(repoAnchoredInTopPath1) { Anchor = Repository.RepositoryAnchor.AnchoredInTop },
        ];

        RecentRepoSplitter sut = new();

        sut.ShorteningStrategy = GitCommands.ShorteningRecentRepoPathStrategy.None;
        List<RecentRepoInfo> topRepoList = new();
        List<RecentRepoInfo> recentRepoList = new();

        sut.SplitRecentRepos(history, topRepoList, recentRepoList);

        topRepoList.Count.Should().Be(1);
        topRepoList[0].Caption.Should().Be(repoAnchoredInTopPath1);
        recentRepoList.Count.Should().Be(1);
    }

    [Test]
    public void SplitRecentRepos_Should_not_shorten_but_handle_user_folder_as_caption()
    {
        List<Repository> history =
        [
            new Repository(repoPathInUserFolder) { Anchor = Repository.RepositoryAnchor.AnchoredInTop },
        ];

        RecentRepoSplitter sut = new();

        sut.ShorteningStrategy = GitCommands.ShorteningRecentRepoPathStrategy.None;
        List<RecentRepoInfo> topRepoList = new();
        List<RecentRepoInfo> recentRepoList = new();

        sut.SplitRecentRepos(history, topRepoList, recentRepoList);

        topRepoList.Count.Should().Be(1);
        topRepoList[0].Caption.Should().StartWith(@"~\AppData").And.EndWith(_relativeLongRepoPath);
        recentRepoList.Count.Should().Be(1);
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

        RecentRepoSplitter sut = new();

        sut.ShorteningStrategy = GitCommands.ShorteningRecentRepoPathStrategy.MiddleDots;
        List<RecentRepoInfo> topRepoList = new();
        List<RecentRepoInfo> recentRepoList = new();

        sut.SplitRecentRepos(history, topRepoList, recentRepoList);

        topRepoList.Count.Should().Be(1);
        topRepoList[0].Caption.Should().Be(@"~\AppData\..\long\repo_path");
        recentRepoList.Count.Should().Be(1);
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

        RecentRepoSplitter sut = new();

        sut.ShorteningStrategy = GitCommands.ShorteningRecentRepoPathStrategy.MostSignDir;
        sut.SortTopRepos = false;
        sut.SortRecentRepos = false;
        List<RecentRepoInfo> topRepoList = new();
        List<RecentRepoInfo> recentRepoList = new();

        sut.SplitRecentRepos(history, topRepoList, recentRepoList);

        topRepoList.Count.Should().Be(2);
        topRepoList[0].Caption.Should().Be("repo_anchored_in_top_path1");
        topRepoList[1].Caption.Should().Be("repo_anchored_in_top_path2");
        recentRepoList.Count.Should().Be(4);
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

        RecentRepoSplitter sut = new();

        sut.ShorteningStrategy = GitCommands.ShorteningRecentRepoPathStrategy.MostSignDir;
        sut.SortTopRepos = true;
        sut.SortRecentRepos = true;
        List<RecentRepoInfo> topRepoList = new();
        List<RecentRepoInfo> recentRepoList = new();

        sut.SplitRecentRepos(history, topRepoList, recentRepoList);

        topRepoList.Count.Should().Be(2);
        topRepoList[0].Caption.Should().Be("repo_anchored_in_top_path1");
        topRepoList[1].Caption.Should().Be("repo_anchored_in_top_path2");
        recentRepoList.Count.Should().Be(4);
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

        RecentRepoSplitter sut = new();

        sut.ShorteningStrategy = GitCommands.ShorteningRecentRepoPathStrategy.MostSignDir;
        sut.SortTopRepos = true;
        sut.SortRecentRepos = true;
        sut.HideTopRepositoriesFromRecentList = true;
        List<RecentRepoInfo> topRepoList = new();
        List<RecentRepoInfo> recentRepoList = new();

        sut.SplitRecentRepos(history, topRepoList, recentRepoList);

        topRepoList.Count.Should().Be(2);
        topRepoList[0].Caption.Should().Be("repo_anchored_in_top_path1");
        topRepoList[1].Caption.Should().Be("repo_anchored_in_top_path2");
        recentRepoList.Count.Should().Be(2);
        recentRepoList[0].Caption.Should().Be("repo_anchored_in_recent_path");
        recentRepoList[1].Caption.Should().Be("repo_not_anchored_path");
    }
    #endregion
}
