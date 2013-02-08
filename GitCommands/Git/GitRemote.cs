using System.Collections.Generic;
using System.Linq;

namespace GitCommands.Git
{
    /// <summary>Helps create a 'git remote' command.</summary>
    public class GitRemote
    {
        string cmd;

        /// <summary>Specifies what to do with remote-tracking branches.</summary>
        public enum SetBranches
        {
            /// <summary>Add the specified branches to the list of tracked branches.</summary>
            Append,
            /// <summary>Un-track the specified branches.</summary>
            UnTrack,
            /// <summary>Reset tracked branches and only track the specified branches.</summary>
            Reset,
        }

        /// <summary>Creates a 'git remote' command to track/un-track specified branches.</summary>
        /// <param name="remote">Remote whose branches to set.</param>
        /// <param name="option">Indicates the add/remove/reset action to perform.</param>
        /// <param name="branches">Branches to track/un-track.</param>
        public GitRemote(RemoteInfo remote, SetBranches option, params RemoteInfo.RemoteBranch[] branches)
        {// 'git remote set-branches [--add] <name> <branch> <branch> ...'
            string add = (option == SetBranches.Append)
                    ? "--add" // add to tracked branches
                    : "";// reset then track/untrack branches

            IEnumerable<RemoteInfo.RemoteBranch> cmdBranches;
            if (option == SetBranches.UnTrack)
            {// un-track
                var keepers = new HashSet<RemoteInfo.RemoteBranch>(remote.Branches);
                keepers.ExceptWith(branches);
                cmdBranches = keepers;

            }
            else
            {
                cmdBranches = branches.AsEnumerable();
            }

            string branchesString = string.Join(" ", cmdBranches);
            // 'git remote set-branches [--add] <name> <branch> <branch> ...'
            cmd = string.Format("remote set-branches {0} {1} {2}", remote.Name, add, branchesString);
        }

        /// <summary>Add the specified remote-tracking branches.</summary>
        public static GitRemote Track(RemoteInfo remote, params RemoteInfo.RemoteBranch[] branches)
        {
            return new GitRemote(remote, SetBranches.Append, branches);
        }

        /// <summary>Un-track the specified remote-tracking branches.</summary>
        public static GitRemote UnTrack(RemoteInfo remote, params RemoteInfo.RemoteBranch[] branches)
        {
            return new GitRemote(remote, SetBranches.UnTrack, branches);
        }

        /// <summary>Reset the current remote-tracking branches and only track the specified branches.</summary>
        public static GitRemote TrackOnly(RemoteInfo remote, params RemoteInfo.RemoteBranch[] branches)
        {
            return new GitRemote(remote, SetBranches.Reset, branches);
        }


        /// <summary>Returns the created 'git remote' command (without 'git ' portion).</summary>
        public override string ToString()
        {
            return cmd;
        }
    }
}
