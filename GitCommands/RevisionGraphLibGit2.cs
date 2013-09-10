using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using LibGit2Sharp;

namespace GitCommands
{
    // TODO: Support RefsOptions
    // TODO: Support Filter
    // TODO: Support BranchFilter
    // TODO: Support InMemFilter
    public sealed class RevisionGraphLibGit2 : RevisionGraph
    {
        public RevisionGraphLibGit2(GitModule module)
            : base(module)
        {
        }

        protected Dictionary<string, List<GitRef>> _refs;

        protected override void ProccessGitLog(CancellationToken taskState)
        {
            RevisionCount = 0;
            _refs = GetRefs().ToDictionaryOfList(head => head.Guid);

            CommitFilter filter;
            if (AppSettings.OrderRevisionByDate)
            {
                filter = new CommitFilter { SortBy = CommitSortStrategies.Time };
            }
            else
            {
                filter = new CommitFilter { SortBy = CommitSortStrategies.Topological };
            }

            foreach (var commit in _module.Repository.Commits.QueryBy(filter))
            {
                GitRevision revision = new GitRevision(_module, null);
                revision.Author = commit.Author.Name;
                revision.AuthorEmail = commit.Author.Email;
                revision.AuthorDate = commit.Author.When.UtcDateTime;
                revision.Committer = commit.Committer.Name;
                revision.CommitterEmail = commit.Committer.Email;
                revision.CommitDate = commit.Committer.When.UtcDateTime;
                revision.Guid = commit.Id.Sha;

                List<GitRef> refsList;
                if (_refs.TryGetValue(revision.Guid, out refsList))
                    revision.Refs.AddRange(refsList);
                revision.Message = commit.MessageShort;
                // TODO: Support notes
                //revision.MessageEncoding = ??
                revision.TreeGuid = commit.Tree.Sha;
                revision.ParentGuids = commit.Parents.Select(p => p.Sha).ToArray();

                RevisionCount++;

                OnUpdated(revision);
                if (taskState.IsCancellationRequested)
                    return;
            }
        }
    }
}
