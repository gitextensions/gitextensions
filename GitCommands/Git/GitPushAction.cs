namespace GitCommands
{
    /// <summary>Push a local branch to a remote branch.
    /// Update remote refs along with associated objects.</summary>
    public class GitPushAction
    {
        string remote;
        string _localBranch;
        string _remoteBranch;
        bool _force;
        bool _delete;

        /// <summary>Push a local branch to a remote one.</summary>
        /// <param name="remote">Name or URL of the remote repository.</param>
        /// <param name="source">Name of the branch to push.</param>
        /// <param name="destination">Ref on the remote side to be updated.</param>
        public GitPushAction(string remote, string source, string destination)
            : this(source, destination)
        {
            this.remote = remote;
        }

        /// <summary>
        /// Push a local branch to a remote one, optionally forcing a non-fast-forward commit.
        /// </summary>
        /// <param name="source">Name of the branch to push.</param>
        /// <param name="destination">Ref on the remote side to be updated.</param>
        /// <param name="force">Indicates whether to update the <paramref name="destination"/> 
        /// ref even when the update is not a fast-forward.</param>
        public GitPushAction(string source, string destination, bool force = false)
        {
            _localBranch = source;
            _remoteBranch = destination;
            _force = force;
        }

        /// <summary>Push a delete of a remote branch.</summary>
        /// <param name="branch">Remote branch to delete.</param>
        public static GitPushAction DeleteRemoteBranch(string branch)
        {
            return new GitPushAction(branch);
        }

        /// <summary>Push a delete of a remote branch.</summary>
        /// <param name="deleteBranch">Remote branch to delete.</param>
        public GitPushAction(string deleteBranch)
        {
            _remoteBranch = deleteBranch;
            _delete = true;
        }

        public string Format()
        {
            if (_delete)
            {
                return string.Format(":{0}", _remoteBranch);
            }

            return string.Format("{0}{1}:{2}",
                (_force ? "+" : ""),
                _localBranch,
                _remoteBranch);
        }

        public override string ToString()
        {// git push {remote} {source}:{destination}
            return string.Format("{0} {1}:{2}", remote, _localBranch, _remoteBranch);
        }
    }
}
