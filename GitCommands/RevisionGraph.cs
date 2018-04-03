using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GitUI;
using GitUIPluginInterfaces;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Threading;

namespace GitCommands
{
    [Flags]
    public enum RefFilterOptions
    {
        Branches = 1,              // --branches
        Remotes = 2,               // --remotes
        Tags = 4,                  // --tags
        Stashes = 8,               //
        All = 15,                  // --all
        Boundary = 16,             // --boundary
        ShowGitNotes = 32,         // --not --glob=notes --not
        NoMerges = 64,             // --no-merges
        FirstParent = 128,         // --first-parent
        SimplifyByDecoration = 256 // --simplify-by-decoration
    }

    public sealed class RevisionGraph : IDisposable
    {
        private static readonly char[] ShellGlobCharacters = { '?', '*', '[' };

        public event EventHandler Exited;
        public event EventHandler<RevisionGraphUpdatedEventArgs> Updated;
        public event EventHandler<AsyncErrorEventArgs> Error;

        private readonly CancellationTokenSequence _cancellationTokenSequence = new CancellationTokenSequence();
        private readonly GitModule _module;

        [CanBeNull] private Dictionary<string, List<IGitRef>> _refs;
        private GitRevision _revision;
        public RefFilterOptions RefsOptions = RefFilterOptions.All | RefFilterOptions.Boundary;
        private string _selectedBranchName;

        public string RevisionFilter { get; set; } = string.Empty;
        public string PathFilter { get; set; } = string.Empty;
        public string BranchFilter { get; set; } = string.Empty;

        [CanBeNull]
        public Func<GitRevision, bool> RevisionPredicate { get; set; }

        public int RevisionCount { get; private set; }

        public RevisionGraph(GitModule module)
        {
            _module = module;
        }

        /// <value>Refs loaded during the last call to <see cref="Execute"/>.</value>
        public IEnumerable<IGitRef> LatestRefs => _refs?.SelectMany(p => p.Value) ?? Enumerable.Empty<IGitRef>();

        public void Execute()
        {
            ThreadHelper.JoinableTaskFactory
                .RunAsync(ExecuteAsync)
                .FileAndForget(
                    ex =>
                    {
                        var args = new AsyncErrorEventArgs(ex);
                        Error?.Invoke(this, args);
                        return !args.Handled;
                    });
        }

        private static readonly Regex _commitRegex = new Regex(@"
                ^
                (?<objectid>[0-9a-f]{40})\n
                ((?<parent>[0-9a-f]{40})\ ?)*\n # note root commits have no parent
                (?<tree>[0-9a-f]{40})\n
                (?<authorname>[^\n]+)\n
                (?<authoremail>[^\n]+)\n
                (?<authordate>\d+)\n
                (?<committername>[^\n]+)\n
                (?<committeremail>[^\n]+)\n
                (?<commitdate>\d+)\n
                (?<encoding>[^\n]*)\n
                (?<subject>.+)
                (\n+(?<body>(.|\n)*))?
                $
            ",
            RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

        private async Task ExecuteAsync()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var token = _cancellationTokenSequence.Next();

            RevisionCount = 0;
            Updated?.Invoke(this, new RevisionGraphUpdatedEventArgs(null));

            await TaskScheduler.Default;

            _refs = GetRefs().ToDictionaryOfList(head => head.Guid);

            const string fullFormat =
                /* Hash                    */ "%H%n" +
                /* Parents                 */ "%P%n" +
                /* Tree                    */ "%T%n" +
                /* Author Name             */ "%aN%n" +
                /* Author Email            */ "%aE%n" +
                /* Author Date             */ "%at%n" +
                /* Committer Name          */ "%cN%n" +
                /* Committer Email         */ "%cE%n" +
                /* Commit Date             */ "%ct%n" +
                /* Commit message encoding */ "%e%n" + // there is a bug: git does not recode commit message when format is given
                /* Commit Body             */ "%B";

            var arguments = new ArgumentBuilder
            {
                "log",
                "-z",
                $"--pretty=format:\"{fullFormat}\"",
                { AppSettings.OrderRevisionByDate, "--date-order", "--topo-order" },
                { AppSettings.ShowReflogReferences, "--reflog" },
                {
                    RefsOptions.HasFlag(RefFilterOptions.All),
                    "--all",
                    new ArgumentBuilder
                    {
                        {
                            RefsOptions.HasFlag(RefFilterOptions.Branches) && !string.IsNullOrWhiteSpace(BranchFilter) && BranchFilter.IndexOfAny(ShellGlobCharacters) != -1,
                            "--branches=" + BranchFilter
                        },
                        { RefsOptions.HasFlag(RefFilterOptions.Remotes), "--remotes" },
                        { RefsOptions.HasFlag(RefFilterOptions.Tags), "--tags" },
                    }.ToString()
                },
                { RefsOptions.HasFlag(RefFilterOptions.Boundary), "--boundary" },
                { RefsOptions.HasFlag(RefFilterOptions.ShowGitNotes), "--not --glob=notes --not" },
                { RefsOptions.HasFlag(RefFilterOptions.NoMerges), "--no-merges" },
                { RefsOptions.HasFlag(RefFilterOptions.FirstParent), "--first-parent" },
                { RefsOptions.HasFlag(RefFilterOptions.SimplifyByDecoration), "--simplify-by-decoration" },
                RevisionFilter,
                "--",
                PathFilter
            };

            Process p = _module.RunGitCmdDetached(arguments.ToString(), GitModule.LosslessEncoding);

            if (token.IsCancellationRequested)
            {
                return;
            }

            foreach (var logItem in p.StandardOutput.ReadNullTerminatedLines())
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                ProcessLogItem(logItem);
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(token);

            if (!token.IsCancellationRequested)
            {
                Exited?.Invoke(this, EventArgs.Empty);
            }
        }

        private IReadOnlyList<IGitRef> GetRefs()
        {
            var result = _module.GetRefs(true);
            bool validWorkingDir = _module.IsValidGitWorkingDir();
            _selectedBranchName = validWorkingDir ? _module.GetSelectedBranch() : string.Empty;
            var selectedRef = result.FirstOrDefault(head => head.Name == _selectedBranchName);

            if (selectedRef != null)
            {
                selectedRef.Selected = true;

                var localConfigFile = _module.LocalConfigFile;

                var selectedHeadMergeSource =
                    result.FirstOrDefault(head => head.IsRemote
                                        && selectedRef.GetTrackingRemote(localConfigFile) == head.Remote
                                        && selectedRef.GetMergeWith(localConfigFile) == head.LocalName);

                if (selectedHeadMergeSource != null)
                {
                    selectedHeadMergeSource.SelectedHeadMergeSource = true;
                }
            }

            return result;
        }

        private void ProcessLogItem(string s)
        {
            s = GitModule.ReEncodeString(s, GitModule.LosslessEncoding, _module.LogOutputEncoding);

            var match = _commitRegex.Match(s);

            if (!match.Success || match.Index != 0)
            {
                Debug.Fail("Commit regex did not match");
                return;
            }

            var encoding = match.Groups["encoding"].Value;

            _revision = new GitRevision(null)
            {
                // TODO use ObjectId (when merged) and parse directly from underlying string, avoiding copy
                Guid = match.Groups["objectid"].Value,
                ParentGuids = match.Groups["parent"].Captures.OfType<Capture>().Select(c => c.Value).ToArray(),
                TreeGuid = match.Groups["tree"].Value,
                Author = match.Groups["authorname"].Value,
                AuthorEmail = match.Groups["authoremail"].Value,
                AuthorDate = DateTimeUtils.ParseUnixTime(match.Groups["authordate"].Value),
                Committer = match.Groups["committername"].Value,
                CommitterEmail = match.Groups["committeremail"].Value,
                CommitDate = DateTimeUtils.ParseUnixTime(match.Groups["commitdate"].Value),
                MessageEncoding = encoding,
                Subject = _module.ReEncodeCommitMessage(match.Groups["subject"].Value, encoding),
                Body = _module.ReEncodeCommitMessage(match.Groups["body"].Value, encoding)
            };

            if (_refs.TryGetValue(_revision.Guid, out var gitRefs))
            {
                _revision.Refs.AddRange(gitRefs);
            }

            if (RevisionPredicate == null || RevisionPredicate(_revision))
            {
                // Remove full commit message to reduce memory consumption (28% for a repo with 69K commits)
                // Full commit message is used in InMemFilter but later it's not needed
                _revision.Body = null;

                RevisionCount++;
                Updated?.Invoke(this, new RevisionGraphUpdatedEventArgs(_revision));
            }
        }

        public void Dispose()
        {
            _cancellationTokenSequence.Dispose();
        }
    }

    public sealed class RevisionGraphUpdatedEventArgs : EventArgs
    {
        public RevisionGraphUpdatedEventArgs(GitRevision revision)
        {
            Revision = revision;
        }

        public GitRevision Revision { get; }
    }
}
