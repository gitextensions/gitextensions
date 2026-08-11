using GitUI.CommandsDialogs.BrowseDialog.DashboardControl;

namespace GitUITests.CommandsDialogs.BrowseDialog;

public sealed class MultiRepositoryStatusProviderTests
{
    private static readonly DateTimeOffset CheckedUtc = new(2026, 8, 11, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset FetchUtc = new(2026, 8, 11, 9, 30, 0, TimeSpan.Zero);

    [Test]
    public void ParsePorcelainV2_reads_branch_sync_and_working_tree_counts()
    {
        const string output = """
            # branch.oid 1111111111111111111111111111111111111111
            # branch.head feature/dashboard
            # branch.upstream origin/feature/dashboard
            # branch.ab +2 -3
            1 M. N... 100644 100644 100644 1111111 2222222 staged.txt
            1 .M N... 100644 100644 100644 1111111 2222222 modified.txt
            2 MM N... 100644 100644 100644 1111111 2222222 R100 renamed.txt old.txt
            ? untracked.txt
            """;

        MultiRepositoryStatus status = MultiRepositoryStatusProvider.ParsePorcelainV2(@"C:\repo", output, CheckedUtc, FetchUtc);

        status.RepositoryPath.Should().Be(@"C:\repo");
        status.Branch.Should().Be("feature/dashboard");
        status.Upstream.Should().Be("origin/feature/dashboard");
        status.Ahead.Should().Be(2);
        status.Behind.Should().Be(3);
        status.StagedCount.Should().Be(2);
        status.ModifiedCount.Should().Be(2);
        status.UntrackedCount.Should().Be(1);
        status.HasWorkingTreeChanges.Should().BeTrue();
        status.LastCheckedUtc.Should().Be(CheckedUtc);
        status.LastFetchUtc.Should().Be(FetchUtc);
    }

    [Test]
    public void ParsePorcelainV2_reports_detached_head_without_upstream()
    {
        const string output = """
            # branch.oid 1111111111111111111111111111111111111111
            # branch.head (detached)
            """;

        MultiRepositoryStatus status = MultiRepositoryStatusProvider.ParsePorcelainV2(@"C:\repo", output, CheckedUtc, lastFetchUtc: null);

        status.Branch.Should().Be("(detached)");
        status.IsDetached.Should().BeTrue();
        status.Upstream.Should().BeNull();
        status.Ahead.Should().BeNull();
        status.Behind.Should().BeNull();
        status.HasWorkingTreeChanges.Should().BeFalse();
    }

    [Test]
    public void ParsePorcelainV2_counts_unmerged_entry_as_staged_and_modified()
    {
        const string output = """
            # branch.head main
            u UU N... 100644 100644 100644 100644 1111111 2222222 3333333 conflicted.txt
            """;

        MultiRepositoryStatus status = MultiRepositoryStatusProvider.ParsePorcelainV2(@"C:\repo", output, CheckedUtc, lastFetchUtc: null);

        status.StagedCount.Should().Be(1);
        status.ModifiedCount.Should().Be(1);
    }
}
