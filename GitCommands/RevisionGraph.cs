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
        private static readonly char[] _shellGlobCharacters = { '?', '*', '[' };

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

        public event EventHandler Exited;
        public event Action<GitRevision> Updated;
        public event EventHandler<AsyncErrorEventArgs> Error;

        private readonly CancellationTokenSequence _cancellationTokenSequence = new CancellationTokenSequence();
        private readonly GitModule _module;
        private readonly RefFilterOptions _refFilterOptions;
        private readonly string _branchFilter;
        private readonly string _revisionFilter;
        private readonly string _pathFilter;
        [CanBeNull] private readonly Func<GitRevision, bool> _revisionPredicate;

        [CanBeNull] private Dictionary<string, List<IGitRef>> _refs;
        private string _selectedBranchName;

        public int RevisionCount { get; private set; }

        public RevisionGraph(
            GitModule module,
            RefFilterOptions refFilterOptions,
            string branchFilter,
            string revisionFilter,
            string pathFilter,
            [CanBeNull] Func<GitRevision, bool> revisionPredicate)
        {
            _module = module;
            _refFilterOptions = refFilterOptions;
            _branchFilter = branchFilter;
            _revisionFilter = revisionFilter;
            _pathFilter = pathFilter;
            _revisionPredicate = revisionPredicate;
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

        private async Task ExecuteAsync()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var token = _cancellationTokenSequence.Next();

            RevisionCount = 0;
            Updated?.Invoke(null);

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
                    _refFilterOptions.HasFlag(RefFilterOptions.All),
                    "--all",
                    new ArgumentBuilder
                    {
                        {
                            _refFilterOptions.HasFlag(RefFilterOptions.Branches) && !string.IsNullOrWhiteSpace(_branchFilter) && _branchFilter.IndexOfAny(_shellGlobCharacters) != -1,
                            "--branches=" + _branchFilter
                        },
                        { _refFilterOptions.HasFlag(RefFilterOptions.Remotes), "--remotes" },
                        { _refFilterOptions.HasFlag(RefFilterOptions.Tags), "--tags" },
                    }.ToString()
                },
                { _refFilterOptions.HasFlag(RefFilterOptions.Boundary), "--boundary" },
                { _refFilterOptions.HasFlag(RefFilterOptions.ShowGitNotes), "--not --glob=notes --not" },
                { _refFilterOptions.HasFlag(RefFilterOptions.NoMerges), "--no-merges" },
                { _refFilterOptions.HasFlag(RefFilterOptions.FirstParent), "--first-parent" },
                { _refFilterOptions.HasFlag(RefFilterOptions.SimplifyByDecoration), "--simplify-by-decoration" },
                _revisionFilter,
                "--",
                _pathFilter
            };

            Process p = _module.RunGitCmdDetached(arguments.ToString(), GitModule.LosslessEncoding);

            if (token.IsCancellationRequested)
            {
                return;
            }

            // Pool string values likely to form a small set: encoding, authorname, authoremail, committername, committeremail
            var stringPool = new ObjectPool<string>(StringComparer.Ordinal, capacity: 256);

            foreach (var logItem in p.StandardOutput.ReadNullTerminatedLines())
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                ProcessLogItem(logItem, stringPool);
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

        private void ProcessLogItem(string s, ObjectPool<string> stringPool)
        {
            s = GitModule.ReEncodeString(s, GitModule.LosslessEncoding, _module.LogOutputEncoding);

            var match = _commitRegex.Match(s);

            if (!match.Success || match.Index != 0)
            {
                Debug.Fail("Commit regex did not match");
                return;
            }

            var encoding = stringPool.Intern(match.Groups["encoding"].Value);

            var revision = new GitRevision(null)
            {
                // TODO use ObjectId (when merged) and parse directly from underlying string, avoiding copy
                Guid = match.Groups["objectid"].Value,
                ParentGuids = match.Groups["parent"].Captures.OfType<Capture>().Select(c => c.Value).ToArray(),
                TreeGuid = match.Groups["tree"].Value,
                Author = stringPool.Intern(match.Groups["authorname"].Value),
                AuthorEmail = stringPool.Intern(match.Groups["authoremail"].Value),
                AuthorDate = DateTimeUtils.ParseUnixTime(match.Groups["authordate"].Value),
                Committer = stringPool.Intern(match.Groups["committername"].Value),
                CommitterEmail = stringPool.Intern(match.Groups["committeremail"].Value),
                CommitDate = DateTimeUtils.ParseUnixTime(match.Groups["commitdate"].Value),
                MessageEncoding = encoding,
                Subject = _module.ReEncodeCommitMessage(match.Groups["subject"].Value, encoding),
                Body = _module.ReEncodeCommitMessage(match.Groups["body"].Value, encoding)
            };

            revision.HasMultiLineMessage = !string.IsNullOrWhiteSpace(revision.Body);

            if (_refs.TryGetValue(revision.Guid, out var gitRefs))
            {
                revision.Refs.AddRange(gitRefs);
            }

            if (_revisionPredicate == null || _revisionPredicate(revision))
            {
                // Remove full commit message to reduce memory consumption (28% for a repo with 69K commits)
                // Full commit message is used in InMemFilter but later it's not needed
                revision.Body = null;

                RevisionCount++;
                Updated?.Invoke(revision);
            }
        }

        public void Dispose()
        {
            _cancellationTokenSequence.Dispose();
        }
    }
}
