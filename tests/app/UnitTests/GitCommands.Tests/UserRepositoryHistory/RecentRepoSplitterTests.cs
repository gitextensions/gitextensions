using FluentAssertions;
using GitCommands.UserRepositoryHistory;

namespace GitCommandsTests.UserRepositoryHistory;

[TestFixture]
public class RecentRepoSplitterTests
{
    private const string _relativeLongRepoPath = @"this\is\a\very_very_very_very_very_very_very\long\repo_path";
    private static readonly string repoPathInUserFolder = Path.Combine(Path.GetTempPath(), _relativeLongRepoPath);
    private static readonly string repoPinnedPath1 = @"C:\this\is\a\repo_pinned_path1\";
    private static readonly string repoPinnedPath2 = @"C:\this\is\a\repo_pinned_path2\";
    private static readonly string repoUnpinnedPath1 = @"C:\this\is\a\repo_unpinned_path1\";
    private static readonly string repoUnpinnedPath2 = @"C:\this\is\a\repo_unpinned_path2\";

    #region Shortening strategy
    [Test]
    public void SplitRecentRepos_Should_use_most_significant_folder_as_caption()
    {
        List<Repository> history =
        [
            new Repository(repoPinnedPath1) { Anchor = Repository.RepositoryAnchor.Pinned },
        ];

        RecentRepoSplitter sut = new();

        sut.ShorteningStrategy = GitCommands.ShorteningRecentRepoPathStrategy.MostSignDir;
        List<RecentRepoInfo> pinnedRepoList = new();
        List<RecentRepoInfo> allRecentRepoList = new();

        sut.SplitRecentRepos(history, pinnedRepoList, allRecentRepoList);

        pinnedRepoList.Count.Should().Be(1);
        pinnedRepoList[0].Caption.Should().Be("repo_pinned_path1");
        allRecentRepoList.Count.Should().Be(1);
    }

    [Test]
    public void SplitRecentRepos_Should_not_shorten_as_caption()
    {
        List<Repository> history =
        [
            new Repository(repoPinnedPath1) { Anchor = Repository.RepositoryAnchor.Pinned },
        ];

        RecentRepoSplitter sut = new();

        sut.ShorteningStrategy = GitCommands.ShorteningRecentRepoPathStrategy.None;
        List<RecentRepoInfo> pinnedRepoList = new();
        List<RecentRepoInfo> allRecentRepoList = new();

        sut.SplitRecentRepos(history, pinnedRepoList, allRecentRepoList);

        pinnedRepoList.Count.Should().Be(1);
        pinnedRepoList[0].Caption.Should().Be(repoPinnedPath1);
        allRecentRepoList.Count.Should().Be(1);
    }

    [Test]
    public void SplitRecentRepos_Should_not_shorten_but_handle_user_folder_as_caption()
    {
        List<Repository> history =
        [
            new Repository(repoPathInUserFolder) { Anchor = Repository.RepositoryAnchor.Pinned },
        ];

        RecentRepoSplitter sut = new();

        sut.ShorteningStrategy = GitCommands.ShorteningRecentRepoPathStrategy.None;
        List<RecentRepoInfo> pinnedRepoList = new();
        List<RecentRepoInfo> allRecentRepoList = new();

        sut.SplitRecentRepos(history, pinnedRepoList, allRecentRepoList);

        pinnedRepoList.Count.Should().Be(1);
        pinnedRepoList[0].Caption.Should().StartWith(@"~\AppData").And.EndWith(_relativeLongRepoPath);
        allRecentRepoList.Count.Should().Be(1);
    }

    [Test]
    public void SplitRecentRepos_Should_display_middle_dots_in_caption()
    {
        // Warning: Able to shorten only an existing folder path
        Directory.CreateDirectory(repoPathInUserFolder);

        List<Repository> history =
        [
            new Repository(repoPathInUserFolder) { Anchor = Repository.RepositoryAnchor.Pinned },
        ];

        RecentRepoSplitter sut = new();

        sut.ShorteningStrategy = GitCommands.ShorteningRecentRepoPathStrategy.MiddleDots;
        List<RecentRepoInfo> pinnedRepoList = new();
        List<RecentRepoInfo> allRecentRepoList = new();

        sut.SplitRecentRepos(history, pinnedRepoList, allRecentRepoList);

        pinnedRepoList.Count.Should().Be(1);
        pinnedRepoList[0].Caption.Should().Be(@"~\AppData\..\long\repo_path");
        allRecentRepoList.Count.Should().Be(1);
    }
    #endregion

    #region Split repositories
    [Test]
    public void SplitRecentRepos_Should_split_depending_anchor()
    {
        List<Repository> history =
        [
            new Repository(repoPinnedPath1) { Anchor = Repository.RepositoryAnchor.Pinned },
            new Repository(repoPinnedPath2) { Anchor = Repository.RepositoryAnchor.Pinned },
            new Repository(repoUnpinnedPath1) { Anchor = Repository.RepositoryAnchor.AllRecent },
            new Repository(repoUnpinnedPath2) { Anchor = Repository.RepositoryAnchor.None },
        ];

        RecentRepoSplitter sut = new();

        sut.ShorteningStrategy = GitCommands.ShorteningRecentRepoPathStrategy.MostSignDir;
        sut.SortPinnedRepos = false;
        sut.SortAllRecentRepos = false;
        List<RecentRepoInfo> pinnedRepoList = new();
        List<RecentRepoInfo> allRecentRepoList = new();

        sut.SplitRecentRepos(history, pinnedRepoList, allRecentRepoList);

        pinnedRepoList.Count.Should().Be(2);
        pinnedRepoList[0].Caption.Should().Be("repo_pinned_path1");
        pinnedRepoList[1].Caption.Should().Be("repo_pinned_path2");
        allRecentRepoList.Count.Should().Be(4);
        allRecentRepoList[0].Caption.Should().Be("repo_pinned_path1");
        allRecentRepoList[1].Caption.Should().Be("repo_pinned_path2");
        allRecentRepoList[2].Caption.Should().Be("repo_unpinned_path1");
        allRecentRepoList[3].Caption.Should().Be("repo_unpinned_path2");
    }

    [Test]
    public void SplitRecentRepos_Should_split_depending_anchor_and_sort_alphabetically()
    {
        List<Repository> history =
        [
            // Unsorted!
            new Repository(repoUnpinnedPath2) { Anchor = Repository.RepositoryAnchor.None },
            new Repository(repoUnpinnedPath1) { Anchor = Repository.RepositoryAnchor.AllRecent },
            new Repository(repoPinnedPath2) { Anchor = Repository.RepositoryAnchor.Pinned },
            new Repository(repoPinnedPath1) { Anchor = Repository.RepositoryAnchor.Pinned },
        ];

        RecentRepoSplitter sut = new();

        sut.ShorteningStrategy = GitCommands.ShorteningRecentRepoPathStrategy.MostSignDir;
        sut.SortAllRecentRepos = true;
        sut.SortPinnedRepos = true;
        List<RecentRepoInfo> pinnedRepoList = new();
        List<RecentRepoInfo> allRecentRepoList = new();

        sut.SplitRecentRepos(history, pinnedRepoList, allRecentRepoList);

        pinnedRepoList.Count.Should().Be(2);
        pinnedRepoList[0].Caption.Should().Be("repo_pinned_path1");
        pinnedRepoList[1].Caption.Should().Be("repo_pinned_path2");
        allRecentRepoList.Count.Should().Be(4);
        allRecentRepoList[0].Caption.Should().Be("repo_pinned_path1");
        allRecentRepoList[1].Caption.Should().Be("repo_pinned_path2");
        allRecentRepoList[2].Caption.Should().Be("repo_unpinned_path1");
        allRecentRepoList[3].Caption.Should().Be("repo_unpinned_path2");
    }
    #endregion
}
