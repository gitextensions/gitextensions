using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace GitCommands.Git
{
    // https://github.com/git/git/blob/master/builtin/remote.c

    // $ git remote show {remote}
    // * remote jberger
    //   Fetch URL: https://github.com/bergerjac/gitextensions.git
    //   Push  URL: https://github.com/bergerjac/gitextensions.git
    //   HEAD branch: left-panel/-main
    //   Remote branches:
    //     LibGit2Sharp                 tracked
    //     LibGit2SharpRevisionGridTest tracked
    //     translationApp               tracked
    //   Local branch configured for 'git pull':
    //     left-panel/-main merges with remote left-panel/-main
    //   Local refs configured for 'git push':
    //     dir-plus-path-replace pushes to dir-plus-path-replace (up to date)
    //     iss538-gui-layout     pushes to iss538-gui-layout     (up to date)
    //     issues/iss1344        pushes to issues/iss1344        (up to date)
    //     left-panel/-main      pushes to left-panel/-main      (up to date)
    //     master                pushes to master                (fast-forwardable)

    /// <summary>Information for a specific remote. <code>'git remote show {remote}'</code></summary>
    public class RemoteInfo
    {
        /// <summary>Gets the configured name of the remote.</summary>
        public string Name { get; private set; }
        /// <summary>Gets the URL which this remote fetches from. <remarks>May be null.</remarks></summary>
        public string FetchUrl { get; private set; }
        /// <summary>Gets the URL(s) which this remote may be configured to push to.</summary>
        public IEnumerable<string> PushUrls { get; private set; }
        /// <summary>(internal) Gets or sets the HEAD branch name (or null if indeterminate).</summary>
        internal string HeadBranchName { get; set; }
        /// <summary>Gets the HEAD branch, which is the default branch when the remote is cloned.</summary>
        public RemoteBranch HeadBranch { get; internal set; }
        /// <summary>Gets the existing branches on the remote repo.</summary>
        public IEnumerable<RemoteBranch> RemoteBranches { get { return _RemoteBranches; } }
        ObservableCollection<RemoteBranch> _RemoteBranches;
        /// <summary>Gets the local remote-tracking branches for the remote repo.</summary>
        public IEnumerable<RemoteTrackingBranch> RemoteTrackingBranches { get { return _RemoteTrackingBranches; } }
        private ObservableCollection<RemoteTrackingBranch> _RemoteTrackingBranches;
        /// <summary>Gets the configured pull branches.</summary>
        public IEnumerable<PullConfig> PullConfigs { get; private set; }
        /// <summary>Gets the configured push branches.</summary>
        public IEnumerable<PushConfig> PushConfigs { get; private set; }
        /// <summary>Indicates whether push is configured for mirroring. 
        /// <para><remarks>Newly created local refs will be pushed to the remote end,
        ///  locally updated refs will be force updated on the remote end,
        ///  and deleted refs will be removed from the remote end.</remarks></para></summary>
        public bool IsMirror { get; private set; }

        /// <summary>Gets a key/value list of the remote-tracking branches.
        /// RemoteTrackingBranch.Name -> RemoteTrackingBranch</summary>
        public IDictionary<string, RemoteTrackingBranch> NameToRemoteTrackingBranch { get; private set; }
        /// <summary>Gets a key/value list of the remote branches.
        /// RemoteBranch.Name -> RemoteBranch</summary>
        public IDictionary<string, RemoteBranch> NameToRemoteBranch { get; private set; }

        /// <summary>Creates a new <see cref="RemoteInfo"/> by parsing the <code>git remote show {remote}</code> output.</summary>
        /// <param name="remoteShowOutput">output from 'git remote show {remote}'</param>
        /// <param name="lsRemoteOutput">output from 'git ls-remote --heads {remote}'</param>
        internal RemoteInfo(string remoteShowOutput, string lsRemoteOutput)
        {
            // $ git ls-remote --heads {remote}
            // {SHA-1}   refs/heads/{branch}
            // ...

            // https://github.com/git/git/blob/d1ede/builtin/remote.c#L1087
            // $ git remote show {remote}
            //   * remote jberger
            //     Fetch URL: https://github.com/bergerjac/gitextensions.git
            //     Push  URL: https://github.com/bergerjac/gitextensions.git
            //     HEAD branch: left-panel/-main
            //     ...

            #region Header
            var lines = remoteShowOutput.SplitLinesThenTrim().ToList();

            int i = 0;
            Name = lines[i].Substring("* remote ".Length);

            _RemoteTrackingBranches = new ObservableCollection<RemoteTrackingBranch>();
            PushUrls = new string[0];
            if (lines.Count == 1)
            {                
                return;
            }
            i += 1;// 1
            string fetchURL = lines[i];
            if (!fetchURL.Contains("(no URL)"))
            {// (has fetch URL)
                FetchUrl = GetURL(fetchURL);
            }

            i += 1;// 2
            PushUrls = (from line in lines
                            .Skip(i) // skip remote name and fetch url lines
                            .TakeWhile(line => line.StartsWith("Push")) // take all Push lines
                            .Where(line => !line.Contains("(no URL)")) // but NOT ones with NO url
                        select GetURL(line)).ToArray();

            int nPushUrls = PushUrls.Count();

            i += nPushUrls;// 2 + PushURLs
            string headLine = lines[i];
            if (headLine.Contains("("))
            {// (not queried), (unknown), (remote HEAD is ambiguous...)
                i += 1; // skip HEAD line
            }
            else
            {// HEAD branch: left-panel/-main
                HeadBranchName = headLine.Substring(headLine.IndexOf(":") + 1).Trim();
            }

            foreach (string line in lines.Skip(i))
            {
                if (line.EndsWith(":", StringComparison.InvariantCulture))
                {// ":" marks a headline
                    break;
                }
                i += 1;
            }

            if (i >= lines.Count) { return; }// [8] >= 8 items (IndexOutOfRangeEx)

            #endregion Header

            #region Remote-Tracking Branches
            //   Remote branches:
            //     LibGit2Sharp                 tracked
            //     ...
            //     translationApp               tracked
            //   Local branch configured for 'git pull':

            if (lines[i].StartsWith("Remote branch"))
            {
                i += 1;// increment
                _RemoteTrackingBranches = new ObservableCollection<RemoteTrackingBranch>(
                    lines
                        .Skip(i)
                        .TakeWhile(line => !line.StartsWith("Local"))
                        .Select(line => new RemoteTrackingBranch(line, this)));
                NameToRemoteTrackingBranch = RemoteTrackingBranches.ToDictionary(branch => branch.Name);
                _RemoteTrackingBranches.CollectionChanged += (o, e) =>
                {
                    if (e.Action == NotifyCollectionChangedAction.Remove)
                    {
                        NameToRemoteTrackingBranch.Remove(((RemoteTrackingBranch)e.OldItems[0]).Name);
                    }
                };
            }
            else
            {
                _RemoteTrackingBranches = new ObservableCollection<RemoteTrackingBranch>();
                NameToRemoteTrackingBranch = new Dictionary<string, RemoteTrackingBranch>(0);
            }

            i += RemoteTrackingBranches.Count();
            if (i >= lines.Count) { return; }// [8] >= 8 items (IndexOutOfRangeEx)

            #endregion Remote-Tracking Branches

            #region Pull
            //   Local branch configured for 'git pull':
            //     left-panel/-main merges with remote left-panel/-main
            //   Local refs configured for 'git push':
            if (lines[i].Contains("configured for 'git pull':"))
            {
                i += 1;

                var dict = new Dictionary<string, List<string>>();
                string branch = null;
                var pulls = new List<PullConfig>();

                foreach (var line in lines.Skip(i).TakeWhile(line => !line.Contains("Local")))
                {
                    // {local}  merges with remote {remote}
                    //             and with remote {remote}
                    //             ...

                    // {local} rebases onto remote {remote}

                    if (line.Contains(PullConfig.RebasesOntoRemote))
                    {// rebase (only one per local)
                        pulls.Add(new PullConfig(this, line));
                        branch = null;
                    }
                    else if (line.Contains("merges with remote"))
                    {// merge ("merges with" marks the beginning of a sequence)
                        branch = line;
                        dict[branch] = new List<string>();
                    }
                    else if (line.Contains("and with remote"))
                    {// "and with" marks an additional remote branch which the local pulls from
                        dict[branch].Add(line);
                    }
                    else
                    {
                        throw new FormatException("RemoteInfo could NOT parse 'git pull' branches.");
                    }
                }

                pulls.AddRange(
                    dict.Select(
                        firstToFollowers =>
                            new PullConfig(this, firstToFollowers.Key, firstToFollowers.Value)
                    )
                );
                PullConfigs = pulls;
                foreach (PullConfig pullConfig in pulls)
                {
                    foreach (string remoteBranch in pullConfig.RemoteBranches)
                    {
                        RemoteTrackingBranch remoteTrackingBranch;
                        if (NameToRemoteTrackingBranch.TryGetValue(remoteBranch, out remoteTrackingBranch))
                        {
                            remoteTrackingBranch.InternalPullConfigs.Add(pullConfig);
                        }
                    }
                }
            }
            else
            {
                PullConfigs = Enumerable.Empty<PullConfig>();
            }

            i += PullConfigs.Count();
            if (i >= lines.Count) { return; }// [8] >= 8 items (IndexOutOfRangeEx)
            #endregion Pull

            #region Push
            if (lines[i].Contains("Local refs will be mirrored by 'git push'"))
            {// mirror
                IsMirror = true;
                return; // no push configs are processed (in git) w/ mirror
            }

            //   Local refs configured for 'git push':
            //     left-panel/-main      pushes to left-panel/-main      (up to date)
            //     master                pushes to master                (fast-forwardable)
            if (lines[i].Contains("configured for 'git push':"))
            {
                i += 1;
                PushConfigs = lines
                    .Skip(i)// skip previous lines
                    .Select(line => new PushConfig(line))
                    .ToArray();
                foreach (PushConfig pushConfig in PushConfigs)
                {
                    RemoteTrackingBranch remoteTrackingBranch;
                    if (NameToRemoteTrackingBranch.TryGetValue(pushConfig.RemoteBranch, out remoteTrackingBranch))
                    {
                        remoteTrackingBranch.PushConfig = pushConfig;
                    }
                }
            }
            else
            {
                PushConfigs = Enumerable.Empty<PushConfig>();
            }
            #endregion Push

            // HeadBranchName must be set BEFORE this line
            #region Remote Branches

            _RemoteBranches = new ObservableCollection<RemoteBranch>(
                    from branch in lsRemoteOutput.SplitLinesThenTrim()
                    select new RemoteBranch(this, branch, HeadBranchName));
            NameToRemoteBranch = _RemoteBranches.ToDictionary(branch => branch.Name);

            #endregion Remote Branches

            #region Match {remote branch} to {remote-tracking branch}
            foreach (RemoteTrackingBranch trackingBranch in
                   RemoteTrackingBranches.Where(branch => branch.Status != RemoteTrackingBranch.State.Stale))
            {// skip stale tracking branches (stale == deleted on the remote repo)
                RemoteBranch match;
                if (NameToRemoteBranch.TryGetValue(trackingBranch.Name, out match))
                {// found match -> 
                    match.TrackingBranch = trackingBranch;
                    trackingBranch.RemoteBranch = match;
                }
            }
            #endregion Match {remote branch} to {remote-tracking branch}
        }

        /// <summary>Gets a URL from a line.</summary>
        static string GetURL(string line)
        {
            return line.Substring(line.IndexOf(":") + 1).Trim();
        }

        public override string ToString() { return Name; }

        /// <summary>Creates a <see cref="GitPush"/> to push from a local branch to a remote branch.</summary>
        public GitPush CreatePush(RemoteBranch remoteBranch, string localBranch)
        {
            return new GitPush(Name, localBranch, remoteBranch.FullRefName);
        }

        /// <summary>Removes the specified remote-tracking branches.</summary>
        public void UnTrack(params RemoteTrackingBranch[] remoteTrackingBranches)
        {
            foreach (RemoteTrackingBranch remoteBranch in remoteTrackingBranches)
            {
                _RemoteTrackingBranches.Remove(remoteBranch);
            }
            // 'git remote' command should be executed and successful before removing
            //return GitRemote.UnTrack(this, remoteBranches);
        }

        /// <summary>Branch existing on a remote repository.</summary>
        public class RemoteBranch
        {
            /// <summary>40</summary>
            static readonly int Sha1Length = GitRevision.IndexGuid.Length;

            /// <summary>Gets the remote repository that the branch exists on.</summary>
            public RemoteInfo Remote { get; private set; }
            /// <summary>Full name of the ref. <example>"refs/heads/master"</example></summary>
            public string FullRefName { get; private set; }
            /// <summary>Name of the ref. <example>"master"</example></summary>
            public string Name { get; private set; }
            /// <summary>Gets the SHA-1 of the branch.</summary>
            public string Sha1 { get; private set; }
            /// <summary>Indicates whether this is the HEAD branch, which is the default branch when the remote is cloned.</summary>
            public bool IsHead { get; private set; }
            /// <summary>Gets the local remote-tracking branch.
            /// <remarks>May be null if not tracked.</remarks></summary>
            public RemoteTrackingBranch TrackingBranch { get; internal set; }

            public RemoteBranch(RemoteInfo remote, string remoteBranchLine, string headBranchName)
            {
                Remote = remote;
                // $ git ls-remote --heads {remote}
                // {SHA-1}   refs/heads/{branch}

                Sha1 = remoteBranchLine.Substring(0, Sha1Length);
                var temp = remoteBranchLine.Substring(Sha1Length).Trim();// remove SHA-1, then trim
                FullRefName = temp;
                Name = FullRefName.Substring(GitRef.RefsHeadsPrefix.Length);// remove "refs/heads/"

                IsHead = Equals(Name, headBranchName);
                if (IsHead)
                {
                    remote.HeadBranch = this;
                }
            }

            /// <summary>Creates a <see cref="GitPush"/> to push from the specified local branch to this <see cref="RemoteBranch"/>.</summary>
            public GitPush CreatePush(string localBranch)
            {
                return Remote.CreatePush(this, localBranch);
            }

        }

        /// <summary>Remote-tracking branch. 
        /// <remarks>Read-only intermediary branch between a local branch and a remote branch.</remarks></summary>
        [DebuggerDisplay("{Name} ({Status})")]
        public class RemoteTrackingBranch
        {
            //  Remote branches:
            //    LibGit2Sharp     tracked
            //    LibGit2Sharp     new (next fetch will store in remotes/{remote})"
            //    LibGit2Sharp     stale (use 'git remote prune' to remove)"

            internal RemoteTrackingBranch(string line, RemoteInfo remote)
            {
                int gapStart = line.IndexOf(" ");
                Name = line.Substring(0, gapStart);// branch name ends right before first space  

                string status = line.Substring(gapStart).Trim().ToLower();
                foreach (var state in ValidStates)
                {
                    if (status.StartsWith(state.Key))
                    {
                        Status = state.Value;
                        if (Status == State.Stale)
                        {
                            // refs/remotes/berger/left-panel/remotes     stale (use 'git remote prune' to remove)
                            // release/2.32                               tracked
                            string staleName = string.Format("{0}{1}/", GitRef.RefsRemotesPrefix, remote.Name);
                            if (Name.StartsWith(staleName))
                            {// stale remote branch starts with "refs/remotes/{0}/"
                                Name = Name.Substring(staleName.Length);
                            }
                        }
                        break;
                    }
                }

                InternalPullConfigs = new List<PullConfig>();
                Remote = remote;
                FullPath = string.Format(
                    "{0}{1}{2}",
                    remote.Name,
                    GitModule.RefSep,
                    Name);

            }

            /// <summary>Gets the full name of the remote-tracking branch. "master"</summary>
            public string Name { get; private set; }
            /// <summary>Gets the full path of the remote-tracking branch. "origin/master"</summary>
            public string FullPath { get; private set; }
            ///// <summary>Gets the full ref name of the remote-tracking branch. "refs/remotes/origin/master"</summary>
            //public string FullRefName { get; private set; }
            /// <summary>Gets the remote of the remote-tracking branch.</summary>
            public RemoteInfo Remote { get; private set; }
            /// <summary>Gets the state of the branch.</summary>
            public State Status { get; private set; }
            /// <summary>Gets the configurations which local branch(es) may be setup to pull from this branch.</summary>
            public IEnumerable<PullConfig> PullConfigs { get { return InternalPullConfigs; } }
            internal IList<PullConfig> InternalPullConfigs { get; private set; }
            /// <summary>Gets the implied push configuration, if any, for this remote-tracking branch.</summary>
            public PushConfig PushConfig { get; internal set; }
            /// <summary>Gets the remote branch.
            /// <remarks>May be null if the remote-tracking branch is stale;
            /// may be invalid if the remote branch has since been deleted.</remarks></summary>
            public RemoteBranch RemoteBranch { get; internal set; }
            /// <summary>Indicates whether the related remote branch is the HEAD branch on the remote repo.</summary>
            public bool IsHead { get { return RemoteBranch != null && RemoteBranch.IsHead; } }

            /// <summary>Returns the <see cref="RemoteTrackingBranch"/>'s <see cref="Name"/>.</summary>
            public override string ToString() { return Name; }

            /// <summary>Creates a <see cref="GitPush"/> to push from the specified local branch to the related <see cref="RemoteBranch"/>.</summary>
            public GitPush CreatePush(string localBranch)
            {
                return Remote.CreatePush(RemoteBranch, localBranch);
            }

            static Dictionary<string, State> ValidStates =
                ((State[])Enum.GetValues(typeof(State)))
                .Skip(1)// skip Unknown
                .ToDictionary(state => state.ToString().ToLower());// e.g. "tracked" -> Tracked

            /// <summary>Specifies the state of a <see cref="RemoteTrackingBranch"/>.</summary>
            public enum State
            {
                Unknown,
                /// <summary>Branch is being tracked.</summary>
                Tracked,
                /// <summary>Has already been removed from the remote repository, but is still locally available in 'refs/remotes/{remote}/'.</summary>
                Stale,
                /// <summary>Next fetch will store in 'remotes/{remote}/'.</summary>
                New,
            }
        }

        /// <summary>Local branch configured to pull from remote branch(es).</summary>
        public class PullConfig
        {
            /// <summary>String which identifies that a local branch rebases onto a remote branch.</summary>
            internal static readonly string RebasesOntoRemote = "rebases onto remote";

            /// <summary>Creates a new <see cref="PullConfig"/> which rebases onto a remote.</summary>
            internal PullConfig(RemoteInfo remote, string line)
                : this(remote, line, null, true) { }

            /// <summary>Creates a new <see cref="PullConfig"/> which merges onto remote(s).</summary>
            internal PullConfig(RemoteInfo remote, string firstLine, IEnumerable<string> proceedingLines)
                : this(remote, firstLine, proceedingLines, false) { }

            PullConfig(RemoteInfo remote, string line, IEnumerable<string> proceedingLines, bool isRebase)
            {
                Remote = remote;
                line = line.Trim();
                int gapStart = line.IndexOf(" ");

                LocalBranch = line.Substring(0, gapStart);

                if (isRebase)
                {// {local} rebases onto remote {remoteBranch}
                    RemoteBranches = new string[] { GetRemoteBranch(line, RebasesOntoRemote) };
                    Config = MergeAction.Rebase;
                }
                else
                {
                    // "localBranch merges with remote remoteBranch"
                    //                 and with remote remoteBranch
                    //                 and with remote remoteBranch
                    //             ...
                    const string mergeMarker = "with remote";
                    var remotes = new List<string> { GetRemoteBranch(line, mergeMarker) };

                    remotes.AddRange(from branch in proceedingLines
                                     select GetRemoteBranch(branch, mergeMarker));
                    RemoteBranches = remotes;
                    Config = MergeAction.Merge;
                }
            }

            /// <summary>Gets the remote branch from a line, using the specified marker.</summary>
            string GetRemoteBranch(string line, string marker)
            {
                string remoteBranch = line.Substring(marker.Length + line.LastIndexOf(marker, StringComparison.InvariantCultureIgnoreCase)).Trim();
                if (remoteBranch.StartsWith(string.Format("{0}{1}", Remote.Name, GitModule.RefSep)) &&
                    !Remote.NameToRemoteTrackingBranch.ContainsKey(remoteBranch))
                {// anomaly "merges with remote {remote}/branch"
                    return remoteBranch.SubstringAfterLastSafe(GitModule.RefSep);
                }// (else no anomaly)
                return remoteBranch;
            }

            /// <summary>Gets the remote which contains the <see cref="PullConfig"/>.</summary>
            public RemoteInfo Remote { get; set; }
            /// <summary>Gets the local branch.</summary>
            public string LocalBranch { get; private set; }
            /// <summary>Gets the remote branch(es) which the local is configured to pull from.</summary>
            public IEnumerable<string> RemoteBranches { get; private set; }
            /// <summary>Gets the action to perform when a remote branch is pulled.</summary>
            public MergeAction Config { get; private set; }

            public override string ToString()
            {// Local: 'master' merges onto master
                return string.Format(
                    "Local '{0}' {1}s {2} '{3}'",
                    LocalBranch,
                    Config.ToString().ToLowerInvariant(),
                    Config == MergeAction.Merge ? "with" : "onto",
                    RemoteBranches.Count() == 1 ? RemoteBranches.First() : "(many)");
            }

            /// <summary>Specifies the action to perform when a remote branch is pulled.</summary>
            public enum MergeAction
            {
                // currently remote.c show_local_info_item errors out if config'd to 
                // rebase AND merge on more than one
                ///// <summary>Cannot rebase onto more than 1 branch.</summary>
                //Invalid,
                /// <summary>Merges with remote.</summary>
                Merge,
                /// <summary>Rebases onto remote.</summary>
                Rebase,
            }
        }

        /// <summary>Local ref which pushes to a remote branch;
        ///  implied via the configured push refspec for the remote.</summary>
        public class PushConfig
        {// https://github.com/git/git/blob/d1ede/remote.c#L1199
            //   Local refs configured for 'git push':
            //     left-panel/-main      pushes to left-panel/-main      (up to date)
            //     master                pushes to master                (fast-forwardable)

            internal PushConfig(string line)
            {
                var splits = line.SplitThenTrim("pushes to").ToArray();
                // "master", "master ... ({status})"

                LocalBranch = splits[0];

                string remote = splits[1];// "master ... ({status})"
                RemoteBranch = remote.Substring(0, remote.IndexOf(" "));

                // (fast-forwardable)
                string status = remote.Substring(remote.IndexOf("(") + 1).TrimEnd(')');
                // "fast-forwardable"

                var state = states.FirstOrDefault(kvp => Equals(status, kvp.Key));
                Status = state.Key.IsNullOrWhiteSpace()
                    ? State.NotQueried
                    : state.Value;
            }

            /// <summary>Gets the local branch which pushes to the <see cref="RemoteBranch"/>.</summary>
            public string LocalBranch { get; private set; }
            /// <summary>Gets the remote branch which receives pushes from <see cref="LocalBranch"/>.</summary>
            public string RemoteBranch { get; private set; }
            /// <summary>Gets the status of the <see cref="RemoteBranch"/> compared to <see cref="LocalBranch"/>.</summary>
            public State Status { get; private set; }

            public override string ToString()
            {
                return string.Format(
                    "Local '{0}' pushes to '{1}' ({2})",
                    LocalBranch,
                    RemoteBranch,
                    states.First(state => state.Value == Status).Key
                    );
            }

            static Dictionary<string, State> states = new Dictionary<string, State>
            {
                { "up to date", State.UpToDate },
                { "fast-forwardable", State.FastForwardable },
                { "local out of date", State.LocalOutOfDate },
                { "create", State.Create },
                { "delete", State.Delete },
            };

            /// <summary>Specifies the state of a remote branch relative to its local branch.</summary>
            public enum State
            {
                NotQueried,
                Create,
                Delete,
                /// <summary>Remote branch is up-to-date with the local branch.</summary>
                UpToDate,
                /// <summary>Remote branch may be fast-forward merged from the local branch.</summary>
                FastForwardable,
                /// <summary>Local branch is out-dated relative to the remote branch.</summary>
                LocalOutOfDate,
            }
        }
    }
}
