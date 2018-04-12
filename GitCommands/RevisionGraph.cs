using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
                ([^\n]+)\n   # 1 authorname
                ([^\n]+)\n   # 2 authoremail
                ([^\n]+)\n   # 3 committername
                ([^\n]+)\n   # 4 committeremail
                ([^\n]*)\n   # 5 subject
                ((.|\n)*)    # 6 body
                $
            ",
            RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

        private const int grpAuthorName = 1;
        private const int grpAuthorEmail = 2;
        private const int grpCommitterName = 3;
        private const int grpCommitterEmail = 4;
        private const int grpSubject = 5;
        private const int grpBody = 6;

        private const string fullFormat =

            // These header entries can all be decoded from the bytes directly.
            // Each hash is 20 bytes long. There is always a

            /* Object ID       */ "%H" +
            /* Tree ID         */ "%T" +
            /* Parent IDs      */ "%P%n" +
            /* Author date     */ "%at%n" +
            /* Commit date     */ "%ct%n" +
            /* Encoding        */ "%e%n" +

            // Items below here must be decoded as strings to support non-ASCII

            /* Author name     */ "%aN%n" +
            /* Author email    */ "%aE%n" +
            /* Committer name  */ "%cN%n" +
            /* Committer email */ "%cE%n" +
            /* Commit subject  */ "%s%n" +
            /* Commit body     */ "%b";

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

        [CanBeNull] private ILookup<string, IGitRef> _refsByObjectId;

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
        public IEnumerable<IGitRef> LatestRefs
            => _refsByObjectId?.SelectMany(p => p) ?? Enumerable.Empty<IGitRef>();

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

            token.ThrowIfCancellationRequested();

            var branchName = _module.IsValidGitWorkingDir()
                ? _module.GetSelectedBranch()
                : "";

            token.ThrowIfCancellationRequested();

            var refs = _module.GetRefs(true);
            UpdateSelectedRef(refs, branchName);
            _refsByObjectId = refs.ToLookup(head => head.Guid);

            token.ThrowIfCancellationRequested();

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

            var sw = Stopwatch.StartNew();
            var revisionCount = 0;

            // This property is relatively expensive to call for every revision, so
            // cache it for the duration of the loop.
            var logOutputEncoding = _module.LogOutputEncoding;

            using (var process = _module.RunGitCmdDetached(arguments.ToString(), GitModule.LosslessEncoding))
            {
                token.ThrowIfCancellationRequested();

                // Pool string values likely to form a small set: encoding, authorname, authoremail, committername, committeremail
                var stringPool = new StringPool();

                var buffer = new byte[4096];

                foreach (var chunk in process.StandardOutput.BaseStream.ReadNullTerminatedChunks(ref buffer))
                {
                    token.ThrowIfCancellationRequested();

                    revisionCount++;

                    ProcessLogItem(chunk, stringPool, logOutputEncoding);
                }

                Trace.WriteLine($"**** PROCESSED {revisionCount} ALL REVISIONS IN {sw.Elapsed.TotalMilliseconds:#,##0.#} ms. Pool count {stringPool.Count}");
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(token);

            if (!token.IsCancellationRequested)
            {
                Exited?.Invoke(this, EventArgs.Empty);
            }
        }

        private void UpdateSelectedRef(IReadOnlyList<IGitRef> refs, string branchName)
        {
            var selectedRef = refs.FirstOrDefault(head => head.Name == branchName);

            if (selectedRef != null)
            {
                selectedRef.Selected = true;

                var localConfigFile = _module.LocalConfigFile;
                var selectedHeadMergeSource = refs.FirstOrDefault(
                    head => head.IsRemote
                         && selectedRef.GetTrackingRemote(localConfigFile) == head.Remote
                         && selectedRef.GetMergeWith(localConfigFile) == head.LocalName);

                if (selectedHeadMergeSource != null)
                {
                    selectedHeadMergeSource.SelectedHeadMergeSource = true;
                }
            }
        }

        private void ProcessLogItem(ArraySegment<byte> chunk, StringPool stringPool, Encoding logOutputEncoding)
        {
            if (!ObjectId.TryParseAsciiHexBytes(chunk, 0, out var objectId) ||
                !ObjectId.TryParseAsciiHexBytes(chunk, ObjectId.Sha1CharCount, out var treeId))
            {
                return;
            }

            var array = chunk.Array;

            var parentIds = new List<ObjectId>(capacity: 1);
            var offset = chunk.Offset + (ObjectId.Sha1CharCount * 2);
            var lastOffset = chunk.Offset + chunk.Count;

            while (true)
            {
                if (offset >= lastOffset - 21)
                {
                    return;
                }

                var b = array[offset];

                if (b == '\n')
                {
                    offset++;
                    break;
                }

                if (b == ' ')
                {
                    offset++;
                }

                if (!ObjectId.TryParseAsciiHexBytes(array, offset, out var parentId))
                {
                    // TODO log this parse problem
                    return;
                }

                parentIds.Add(parentId);
                offset += ObjectId.Sha1CharCount;
            }

            var authorDate = ParseUnixDateTime();
            var commitDate = ParseUnixDateTime();

            DateTime ParseUnixDateTime()
            {
                long unixTime = 0;

                while (true)
                {
                    var c = array[offset++];

                    if (c == '\n')
                    {
                        return DateTimeUtils.UnixEpoch.AddTicks(unixTime * TimeSpan.TicksPerSecond).ToLocalTime();
                    }

                    unixTime = (unixTime * 10) + (c - '0');
                }
            }

            string encodingName;
            Encoding encoding;

            var encodingNameEndOffset = Array.IndexOf(array, (byte)'\n', offset);

            if (encodingNameEndOffset == -1)
            {
                // TODO log this error case
                return;
            }

            if (offset == encodingNameEndOffset)
            {
                // No encoding specified
                encoding = logOutputEncoding;
                encodingName = null;
            }
            else
            {
                encodingName = logOutputEncoding.GetString(array, offset, encodingNameEndOffset - offset);
                encoding = _module.GetEncodingByGitName(encodingName);
            }

            offset = encodingNameEndOffset + 1;

            // Finally, decode the names, email, subject and body strings using the required text encoding
            var s = encoding.GetString(array, offset, lastOffset - offset);

            var match = _commitRegex.Match(s);

            if (!match.Success || match.Index != 0)
            {
                // TODO log this parse problem
                Debug.Fail("Commit regex did not match");
                return;
            }

            var revision = new GitRevision(null)
            {
                // TODO are we really sure we can't make Revision.Guid an ObjectId?
                Guid = objectId.ToString(),

                // TODO take IReadOnlyList<ObjectId> instead
                ParentGuids = parentIds.Select(p => p.ToString()).ToArray(),

                TreeGuid = treeId,

                Author = stringPool.Intern(s, match.Groups[grpAuthorName]),
                AuthorEmail = stringPool.Intern(s, match.Groups[grpAuthorEmail]),
                AuthorDate = authorDate,
                Committer = stringPool.Intern(s, match.Groups[grpCommitterName]),
                CommitterEmail = stringPool.Intern(s, match.Groups[grpCommitterEmail]),
                CommitDate = commitDate,
                MessageEncoding = encodingName,
                Subject = match.Groups[grpSubject].Value,
                Body = match.Groups[grpBody].Value
            };

            revision.HasMultiLineMessage = !string.IsNullOrWhiteSpace(revision.Body);
            revision.Refs = _refsByObjectId[revision.Guid].AsReadOnlyList();

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
