using System;
using System.Collections.Generic;
using System.Linq;

namespace GitCommands
{
    /// <summary>Creates a 'git push' command.
    /// Push a local branch to a remote branch.
    /// Update remote refs along with associated objects.</summary>
    public class GitPush
    {
        /// <summary>Gets the name or URL of the remote repo to push to.</summary>
        public string Remote { get; }

        /// <summary>Gets the set of LocalBranch:RemoteBranch actions.</summary>
        public IEnumerable<GitPushAction> PushActions { get; }

        /// <summary>Indicates whether to report progress during the push operation.</summary>
        public bool ReportProgress { get; set; }

        /// <summary>Works like 'git push {remote}', where {remote} is the current branch’s remote.
        ///  (or 'origin', if no remote is configured for the current branch).</summary>
        public GitPush()
            : this(null)
        {
        }

        /// <summary>Works like 'git push {remote} :', where branches matching the refspec are pushed.</summary>
        public GitPush(string remote)
            : this(remote, (string)null)
        {
        }

        /// <summary>'git push {remote} {source}' : Push to a matching ref in the remote,
        ///  or if non-existing, create one with the same name.</summary>
        public GitPush(string remote, string source)
            : this(remote, source, null)
        {
        }

        /// <summary>Push a local branch to a remote branch.</summary>
        /// <param name="remote">Name or URL of the remote repository.</param>
        /// <param name="source">Name of the branch to push.</param>
        /// <param name="destination">Ref on the remote side to be updated.</param>
        /// <param name="force">Indicates whether to update the <paramref name="destination"/>
        /// ref even when the update is not a fast-forward.</param>
        public GitPush(string remote, string source, string destination, bool force = false)
            : this(remote, new GitPushAction(source, destination, force))
        {
        }

        /// <summary>Push sets of local branches to a remote branches.</summary>
        /// <param name="remote">Name or URL of the remote repository.</param>
        /// <param name="pushActions">Sets of LocalBranch:RemoteBranch.</param>
        public GitPush(string remote, params GitPushAction[] pushActions)
            : this(remote, pushActions.AsEnumerable())
        {
        }

        /// <summary>Push sets of local branches to remote branches.</summary>
        /// <param name="remote">Name or URL of the remote repository.</param>
        /// <param name="pushActions">Sets of LocalBranch:RemoteBranch.</param>
        public GitPush(string remote, IEnumerable<GitPushAction> pushActions)
        {
            Remote = remote;
            PushActions = pushActions;
        }

        /// <summary>Creates the 'push' command string. <example>"push --progress origin master:master"</example></summary>
        public override string ToString()
        {
            var args = new GitArgumentBuilder("push")
            {
                { ReportProgress, "--progress" },
                Remote.Quote(),
                PushActions.Select(action => action.ToString())
            };

            return args.ToString();
        }
    }

    /// <summary>Part of a 'git push', which specifies the local and/or remote branch.</summary>
    public class GitPushAction
    {
        private readonly string _localBranch;
        private readonly string _remoteBranch;
        private readonly bool _force;

        /// <summary>
        /// Push a local branch to a remote one, optionally forcing a non-fast-forward commit.
        /// </summary>
        /// <param name="source">Name of the branch to push.</param>
        /// <param name="destination">Ref on the remote side to be updated.</param>
        /// <param name="force">Indicates whether to update the <paramref name="destination"/>
        /// ref even when the update is not a fast-forward.</param>
        public GitPushAction(string source, string destination, bool force = false)
        {
            _localBranch = GitRefName.GetFullBranchName(source);
            _remoteBranch = GitRefName.GetFullBranchName(destination);
            _force = force;
        }

        /// <summary>Delete a remote branch.</summary>
        /// <param name="branch">Remote branch to delete.</param>
        public static GitPushAction DeleteRemoteBranch(string branch)
        {
            return new GitPushAction(
                source: null,
                destination: GitRefName.GetFullBranchName(branch));
        }

        /// <summary>Creates the push action command part.</summary>
        public override string ToString()
        {
            if (_localBranch.IsNullOrWhiteSpace())
            {
                return $":{_remoteBranch}";
            }

            return $"{(_force ? "+" : "")}{_localBranch}:{_remoteBranch}";
        }
    }
}
