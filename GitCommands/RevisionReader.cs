using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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

    public sealed class RevisionReader : IDisposable
    {
        private readonly CancellationTokenSequence _cancellationTokenSequence = new CancellationTokenSequence();

        public int RevisionCount { get; private set; }

        /// <value>Refs loaded during the last call to <see cref="Execute"/>.</value>
        public IReadOnlyList<IGitRef> LatestRefs { get; private set; } = Array.Empty<IGitRef>();

        public void Execute(
            GitModule module,
            IObserver<GitRevision> subject,
            RefFilterOptions refFilterOptions,
            string branchFilter,
            string revisionFilter,
            string pathFilter,
            [CanBeNull] Func<GitRevision, bool> revisionPredicate)
        {
            ThreadHelper.JoinableTaskFactory
                .RunAsync(() => ExecuteAsync(module, subject, refFilterOptions, branchFilter, revisionFilter, pathFilter, revisionPredicate))
                .FileAndForget(
                    ex =>
                    {
                        subject.OnError(ex);
                        return false;
                    });
        }

        private async Task ExecuteAsync(
            GitModule module,
            IObserver<GitRevision> subject,
            RefFilterOptions refFilterOptions,
            string branchFilter,
            string revisionFilter,
            string pathFilter,
            [CanBeNull] Func<GitRevision, bool> revisionPredicate)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var token = _cancellationTokenSequence.Next();

            RevisionCount = 0;

            await TaskScheduler.Default;

            subject.OnNext(null);

            token.ThrowIfCancellationRequested();

            var branchName = module.IsValidGitWorkingDir()
                ? module.GetSelectedBranch()
                : "";

            token.ThrowIfCancellationRequested();

            LatestRefs = module.GetRefs();
            UpdateSelectedRef(module, LatestRefs, branchName);
            var refsByObjectId = LatestRefs.ToLookup(head => head.Guid);

            token.ThrowIfCancellationRequested();

            const string fullFormat =

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
                /* Commit subject  */ "%s%n%n" +
                /* Commit body     */ "%b";

            // TODO add AppBuilderExtensions support for flags enums, starting with RefFilterOptions, then use it in the below construction

            var arguments = BuildArguments();

            var sw = Stopwatch.StartNew();

            // This property is relatively expensive to call for every revision, so
            // cache it for the duration of the loop.
            var logOutputEncoding = module.LogOutputEncoding;

            using (var process = module.RunGitCmdDetached(arguments.ToString(), GitModule.LosslessEncoding))
            {
                token.ThrowIfCancellationRequested();

                // Pool string values likely to form a small set: encoding, authorname, authoremail, committername, committeremail
                var stringPool = new StringPool();

                var buffer = new byte[4096];

                foreach (var chunk in process.StandardOutput.BaseStream.ReadNullTerminatedChunks(ref buffer))
                {
                    token.ThrowIfCancellationRequested();

                    if (TryParseRevision(module, chunk, stringPool, logOutputEncoding, out var revision))
                    {
                        if (revisionPredicate == null || revisionPredicate(revision))
                        {
                            // Remove full commit message to reduce memory consumption (28% for a repo with 69K commits)
                            // Full commit message is used in InMemFilter but later it's not needed
                            revision.Body = null;

                            // Look up any refs associate with this revision
                            revision.Refs = refsByObjectId[revision.Guid].AsReadOnlyList();

                            RevisionCount++;

                            subject.OnNext(revision);
                        }
                    }
                }

                Trace.WriteLine($"**** [{nameof(RevisionReader)}] Emitted {RevisionCount} revisions in {sw.Elapsed.TotalMilliseconds:#,##0.#} ms. bufferSize={buffer.Length} poolCount={stringPool.Count}");
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(token);

            if (!token.IsCancellationRequested)
            {
                subject.OnCompleted();
            }

            ArgumentBuilder BuildArguments()
            {
                return new ArgumentBuilder
                {
                    "log",
                    "-z",
                    $"--pretty=format:\"{fullFormat}\"",
                    { AppSettings.OrderRevisionByDate, "--date-order", "--topo-order" },
                    { AppSettings.ShowReflogReferences, "--reflog" },
                    {
                        refFilterOptions.HasFlag(RefFilterOptions.All),
                        "--all",
                        new ArgumentBuilder
                        {
                            {
                                refFilterOptions.HasFlag(RefFilterOptions.Branches) &&
                                !branchFilter.IsNullOrWhiteSpace() &&
                                branchFilter.IndexOfAny(new[] { '?', '*', '[' }) != -1,
                                "--branches=" + branchFilter
                            },
                            { refFilterOptions.HasFlag(RefFilterOptions.Remotes), "--remotes" },
                            { refFilterOptions.HasFlag(RefFilterOptions.Tags), "--tags" },
                        }.ToString()
                    },
                    { refFilterOptions.HasFlag(RefFilterOptions.Boundary), "--boundary" },
                    { refFilterOptions.HasFlag(RefFilterOptions.ShowGitNotes), "--not --glob=notes --not" },
                    { refFilterOptions.HasFlag(RefFilterOptions.NoMerges), "--no-merges" },
                    { refFilterOptions.HasFlag(RefFilterOptions.FirstParent), "--first-parent" },
                    { refFilterOptions.HasFlag(RefFilterOptions.SimplifyByDecoration), "--simplify-by-decoration" },
                    revisionFilter,
                    "--",
                    pathFilter
                };
            }
        }

        private static void UpdateSelectedRef(GitModule module, IReadOnlyList<IGitRef> refs, string branchName)
        {
            var selectedRef = refs.FirstOrDefault(head => head.Name == branchName);

            if (selectedRef != null)
            {
                selectedRef.Selected = true;

                var localConfigFile = module.LocalConfigFile;
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

        private static bool TryParseRevision(GitModule module, ArraySegment<byte> chunk, StringPool stringPool, Encoding logOutputEncoding, [CanBeNull] out GitRevision revision)
        {
            // The 'chunk' of data contains a complete git log item, encoded.
            // This method decodes that chunk and produces a revision object.

            // All values which can be read directly from the byte array are arranged
            // at the beginning of the chunk. The latter part of the chunk will require
            // decoding as a string.

            #region Object ID, Tree ID, Parent IDs

            // The first 40 bytes are the revision ID and the tree ID back to back
            if (!ObjectId.TryParseAsciiHexBytes(chunk, 0, out var objectId) ||
                !ObjectId.TryParseAsciiHexBytes(chunk, ObjectId.Sha1CharCount, out var treeId))
            {
                revision = default;
                return false;
            }

            var objectIdStr = objectId.ToString();

            var array = chunk.Array;
            var offset = chunk.Offset + (ObjectId.Sha1CharCount * 2);
            var lastOffset = chunk.Offset + chunk.Count;

            // Next we have zero or more parent IDs separated by ' ' and terminated by '\n'
            var parentIds = new List<ObjectId>(capacity: 1);

            while (true)
            {
                if (offset >= lastOffset - 21)
                {
                    revision = default;
                    return false;
                }

                var b = array[offset];

                if (b == '\n')
                {
                    // There are no more parent IDs
                    offset++;
                    break;
                }

                if (b == ' ')
                {
                    // We are starting a new parent ID
                    offset++;
                }

                if (!ObjectId.TryParseAsciiHexBytes(array, offset, out var parentId))
                {
                    // TODO log this parse problem
                    revision = default;
                    return false;
                }

                parentIds.Add(parentId);
                offset += ObjectId.Sha1CharCount;
            }

            #endregion

            #region Timestamps

            // Lines 2 and 3 are timestamps, as decimal ASCII seconds since the unix epoch, each terminated by `\n`
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

            #endregion

            #region Encoding

            // Line is the name of the encoding used by git, or an empty string, terminated by `\n`
            string encodingName;
            Encoding encoding;

            var encodingNameEndOffset = Array.IndexOf(array, (byte)'\n', offset);

            if (encodingNameEndOffset == -1)
            {
                // TODO log this error case
                revision = default;
                return false;
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
                encoding = module.GetEncodingByGitName(encodingName);
            }

            offset = encodingNameEndOffset + 1;

            #endregion

            #region Encoded string valies (names, emails, subject, body)

            // Finally, decode the names, email, subject and body strings using the required text encoding
            var s = encoding.GetString(array, offset, lastOffset - offset);

            var reader = new StringLineReader(s);

            var author = reader.ReadLine(stringPool);
            var authorEmail = reader.ReadLine(stringPool);
            var committer = reader.ReadLine(stringPool);
            var committerEmail = reader.ReadLine(stringPool);

            var subject = reader.ReadLine(advance: false);
            Debug.Assert(subject != null, "subject != null");

            // NOTE the convention is that the Subject string is duplicated at the start of the Body string
            // Therefore we read the subject twice.
            // If there are not enough characters remaining for a body, then just assign the subject string directly.
            var body = reader.Remaining - subject.Length == 2 ? subject : reader.ReadToEnd();
            Debug.Assert(body != null, "body != null");

            if (author == null || authorEmail == null || committer == null || committerEmail == null || subject == null || body == null)
            {
                // TODO log this parse error
                Debug.Fail("Unable to read an entry from the log -- this should not happen");
                revision = default;
                return false;
            }

            #endregion

            revision = new GitRevision(null)
            {
                // TODO are we really sure we can't make Revision.Guid an ObjectId?
                Guid = objectIdStr,

                // TODO take IReadOnlyList<ObjectId> instead
                ParentGuids = parentIds.ToArray(p => p.ToString()),

                TreeGuid = treeId,
                Author = author,
                AuthorEmail = authorEmail,
                AuthorDate = authorDate,
                Committer = committer,
                CommitterEmail = committerEmail,
                CommitDate = commitDate,
                MessageEncoding = encodingName,
                Subject = subject,
                Body = body,
                HasMultiLineMessage = !ReferenceEquals(subject, body)
            };

            return true;
        }

        public void Dispose()
        {
            _cancellationTokenSequence.Dispose();
        }

        #region Nested type: StringLineReader

        /// <summary>
        /// Simple type to walk along a string, line by line, without redundant allocations.
        /// </summary>
        private struct StringLineReader
        {
            private readonly string _s;
            private int _index;

            public StringLineReader(string s)
            {
                _s = s;
                _index = 0;
            }

            public int Remaining => _s.Length - _index;

            [CanBeNull]
            public string ReadLine([CanBeNull] StringPool pool = null, bool advance = true)
            {
                if (_index == _s.Length)
                {
                    return null;
                }

                var startIndex = _index;
                var endIndex = _s.IndexOf('\n', startIndex);

                if (endIndex == -1)
                {
                    return ReadToEnd(advance);
                }

                if (advance)
                {
                    _index = endIndex + 1;
                }

                return pool != null
                    ? pool.Intern(_s, startIndex, endIndex - startIndex)
                    : _s.Substring(startIndex, endIndex - startIndex);
            }

            [CanBeNull]
            public string ReadToEnd(bool advance = true)
            {
                if (_index == _s.Length)
                {
                    return null;
                }

                var s = _s.Substring(_index);

                if (advance)
                {
                    _index = _s.Length;
                }

                return s;
            }
        }

        #endregion
    }
}
