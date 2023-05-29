using FluentAssertions;
using GitCommands;

namespace GitUITests.UserControls.CommitInfo
{
    public class BranchComparerTests
    {
        [SetCulture("en-US")]
        [SetUICulture("en-US")]
        [Test]
        public void BranchComparer([Values(null, "", "current", "(no branch)")] string? currentBranch)
        {
            AppSettings.PrioritizedBranchNames = "master;dummy;main[^/]*|master[^/]*;release/.*";
            AppSettings.PrioritizedRemoteNames = "zzz;origin|upstream";

            List<string> expectedBranches = new()
            {
                currentBranch,

                // local branch important
                // order 0
                "master",
                // order 2
                "master2",
                // order 3
                "release/v1.0",

                // remote branch important, group by branch priority order
                // order 0
                "remotes/zzz/master",
                "remotes/origin/master",
                "remotes/upstream/master",
                "remotes/123AAA/master",
                "remotes/AAA/master",
                "remotes/myrepo/master",
                "remotes/other/master",
                "remotes/z_other/master",
                // order 2
                "remotes/myrepo/master2",
                // order 3
                "remotes/upstream/release/v1.0",
                "remotes/upstream/release/v1.1",
                "remotes/123AAA/release/tmp",
                "remotes/myrepo/release/v1.1",

                // local branches
                "1234_issue",
                "current/2",
                "current_2",
                "feature/1234_issue",
                "fix/master",
                "mastr",
                "repro/issue",

                // important repos
                "remotes/zzz/b1",
                "remotes/origin/b1",
                "remotes/origin/b2",
                "remotes/upstream/b1",
                "remotes/upstream/b2",

                // other repos
                "remotes/123AAA/123",
                "remotes/myrepo/b1",
                "remotes/myrepo/b2",
                "remotes/other/b1",
                "remotes/other/b2",
                "remotes/z_other/b1",
                "remotes/z_other/b2",
            };

            if (currentBranch is null)
            {
                expectedBranches.RemoveAt(0);
            }

            List<string> branches = new(expectedBranches);

            SortAndCheckListsForEquality();

            branches.Sort();

            SortAndCheckListsForEquality();

            branches.Reverse();

            SortAndCheckListsForEquality();

            return;

            void SortAndCheckListsForEquality()
            {
                branches.Sort(new GitUI.CommitInfo.CommitInfo.BranchComparer(branches, currentBranch ?? ""));

                branches.Count.Should().Be(expectedBranches.Count);
                for (int index = 0; index < branches.Count; ++index)
                {
                    branches[index].Should().BeSameAs(expectedBranches[index]);
                }
            }
        }
    }
}
