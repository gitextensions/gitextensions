using System.Collections.Generic;

namespace GitCommands.Git
{
    /// <summary>Commit comparison between two branches.</summary>
    public class BranchComparison
    {
        /// <summary>Gets the commit comparison.</summary>
        public BranchCompareStatus State { get; private set; }
        /// <summary>Gets the "control" branch.</summary>
        public CommitDiffCount Branch { get; private set; }
        /// <summary>Gets the other, "test" branch to compare to.</summary>
        public CommitDiffCount Other { get; private set; }

        public BranchComparison(CommitDiffCount branch, CommitDiffCount other)
        {
            Branch = branch;
            Other = other;

            if (branch.DiffCount == 0 && other.DiffCount == 0)
            {// both have NO "extra" commits
                State = BranchCompareStatus.Same;
            }
            else if (other.DiffCount == 0)
            {// (control has extra commits AND) other has NO extra commits
                State = BranchCompareStatus.AheadPublishable;
            }
            else if (branch.DiffCount == 0)
            {// (other has extra commits AND) control has NO extra commits
                State = BranchCompareStatus.BehindFastForwardable;
            }
            else
            {// (both have extra commits)
                State = BranchCompareStatus.Diverged;
            }
        }
        public BranchComparison(string branch, int nBranch, string other, int nOther)
            : this(new CommitDiffCount(branch, nBranch), new CommitDiffCount(other, nOther)) { }

    }

    /// <summary>Commit differential count for a branch.</summary>
    public class CommitDiffCount
    {
        /// <summary>Gets the name of the branch.</summary>
        public string Branch { get; private set; }
        /// <summary>Gets the number of different commits.</summary>
        public int DiffCount { get; private set; }

        public CommitDiffCount(string branch, int diffCount)
        {
            Branch = branch;
            DiffCount = diffCount;
        }
    }

    ///// <summary>Commit differential for a branch.</summary>
    //public class CommitDiff : CommitDiffCount
    //{
    //    /// <summary>Gets the SHAs of the commits that are different.</summary>
    //    public IEnumerable<string> SHAs { get; private set; }
   
    //    public CommitDiff(string branch, ICollection<string> shas)
    //        : base(branch, shas.Count)
    //    {
    //        SHAs = shas;
    //    }
    //}

    /// <summary>Specifies the commit comparison of a branch compared to another branch.</summary>
    public enum BranchCompareStatus
    {
        /// <summary>Both branches are on the same commit.</summary>
        Same,
        /// <summary>Branch is ahead and can be cleanly published (e.g. 'git push').</summary>
        AheadPublishable,
        /// <summary>Branch is behind and can be fast-forwarded via merge or pull.</summary>
        BehindFastForwardable,
        /// <summary>Branches have diverged and have different commits.</summary>
        Diverged,
    }
}
