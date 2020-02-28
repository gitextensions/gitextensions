using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace GitUITests.UserControls.CommitInfo
{
    public class BranchComparerTests
    {
        [Test]
        public void BranchComparer([Values(null, "current")] string currentBranch)
        {
            var expectedBranches = new List<string>
            {
                currentBranch,

                // local important
                "master",
                "master2",

                // remote important, important repos
                "remotes/origin/master",
                "remotes/upstream/master",

                // remote important, other repos
                "remotes/myrepo/master",
                "remotes/myrepo/master2",
                "remotes/other/master",
                "remotes/z_other/master",

                // local branches
                "1234_issue",
                "current/2",
                "current_2",
                "feature/1234_issue",
                "fix/master",
                "mastr",
                "repro/issue",

                // important repos
                "remotes/origin/b1",
                "remotes/origin/b2",
                "remotes/upstream/b1",
                "remotes/upstream/b2",

                // other repos
                "remotes/myrepo/b1",
                "remotes/myrepo/b2",
                "remotes/other/b1",
                "remotes/other/b2",
                "remotes/z_other/b1",
                "remotes/z_other/b2",
            };

            if (currentBranch == null)
            {
                expectedBranches.RemoveAt(0);
            }

            var branches = new List<string>(expectedBranches);

            SortAndCheckListsForEquality();

            branches.Sort();

            SortAndCheckListsForEquality();

            branches.Reverse();

            SortAndCheckListsForEquality();

            return;

            void SortAndCheckListsForEquality()
            {
                branches.Sort(new GitUI.CommitInfo.CommitInfo.BranchComparer(currentBranch));

                branches.Count.Should().Be(expectedBranches.Count);
                for (int index = 0; index < branches.Count; ++index)
                {
                    branches[index].Should().BeSameAs(expectedBranches[index]);
                }
            }
        }
    }
}
