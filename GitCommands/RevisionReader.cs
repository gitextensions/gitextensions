using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using GitExtUtils;
using GitUIPluginInterfaces;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace GitCommands
{
    public sealed class RevisionReader
    {
        private const string _fullFormat =

            // These header entries can all be decoded from the bytes (ASCII compatible) directly.

            /* Object ID       */ "%H" +
            /* Tree ID         */ "%T" +
            /* Parent IDs      */ "%P%n" +
            /* Author date     */ "%at%n" +
            /* Commit date     */ "%ct%n" +

            // Items below here must be decoded
            /* Reflog selector placeholder */ "{0}" +
            /* Author name     */ "%aN%n" +
            /* Author email    */ "%aE%n" +
            /* Committer name  */ "%cN%n" +
            /* Committer email */ "%cE%n" +
            /* Commit raw body */ "%B";

        private const string _reflogSelectorFormat = "%gD%n";

        // Trace info for parse errors
        private int _noOfParseError = 0;

        private readonly GitModule _module;
        private readonly Encoding _logOutputEncoding;
        private readonly long _oldestBody;
        private const int _offsetDaysForOldestBody = 6 * 30; // about 6 months

        // reflog selector to identify stashes
        private bool _hasReflogSelector;

        public RevisionReader(GitModule module, bool allBodies = false)
            : this(module, module.LogOutputEncoding, allBodies ? 0 : GetUnixTimeForOffset(_offsetDaysForOldestBody))
        {
        }

        private RevisionReader(GitModule module, Encoding logOutputEncoding, long oldestBody)
        {
            _module = module;
            _logOutputEncoding = logOutputEncoding;
            _oldestBody = oldestBody;
        }

        /// <summary>
        /// Return the git-log format and set the reflogSelector flag
        /// (used when parsing the buffer).
        /// </summary>
        private string LogFormat(bool hasReflogSelector = false)
        {
            _hasReflogSelector = hasReflogSelector;
            return string.Format(_fullFormat, hasReflogSelector ? _reflogSelectorFormat : "");
        }

        private static long GetUnixTimeForOffset(int days)
            => new DateTimeOffset(DateTime.Now.ToUniversalTime() - TimeSpan.FromDays(days)).ToUnixTimeSeconds();

        /// <summary>
        /// Get the git-stash GitRevisions.
        /// </summary>
        /// <param name="cancellationToken">Cancellation cancellationToken.</param>
        /// <returns>List with GitRevisions.</returns>
        public IReadOnlyCollection<GitRevision> GetStashes(CancellationToken cancellationToken)
        {
            GitArgumentBuilder arguments = new("stash")
            {
                "list",
                "-z",
                $"--pretty=format:\"{LogFormat(true)}\""
            };

            return GetRevisionsFromArguments(arguments, cancellationToken);
        }

        /// <summary>
        /// Get the GitRevisions for the listed files, excluding commits without changes.
        /// </summary>
        /// <param name="untracked">commit list.</param>
        /// <param name="cancellationToken">Cancellation cancellationToken.</param>
        /// <returns>List with GitRevisions.</returns>
        public IReadOnlyCollection<GitRevision> GetRevisionsFromList(IList<ObjectId> untracked, CancellationToken cancellationToken)
        {
            if (untracked.Count == 0)
            {
                return Array.Empty<GitRevision>();
            }

            GitArgumentBuilder arguments = new("log")
            {
                "-z",
                $"--pretty=format:\"{LogFormat()}\"",
                "--dirstat=files",
                string.Join(" ", untracked),
                ".",
            };

            return GetRevisionsFromArguments(arguments, cancellationToken);
        }

        /// <summary>
        /// Retrieves the git history in the requested range (boundaries included).
        /// </summary>
        /// <param name="olderCommitHash">The first (older) commit hash.</param>
        /// <param name="newerCommitHash">The last (newer) commit hash.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The retrieved git history.</returns>
        public IReadOnlyCollection<GitRevision> GetRevisionsFromRange(string olderCommitHash, string newerCommitHash, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(olderCommitHash) || string.IsNullOrWhiteSpace(newerCommitHash))
            {
                return Array.Empty<GitRevision>();
            }

            GitArgumentBuilder arguments = new("log")
                {
                    "-z",
                    $"--pretty=format:\"{LogFormat()}\"",
                    $"{olderCommitHash}~..{newerCommitHash}"
                };

            return GetRevisionsFromArguments(arguments, cancellationToken);
        }

        /// <summary>
        /// Get the GitRevisions for Git argument.
        /// </summary>
        /// <param name="arguments">Git command arguments.</param>
        /// <param name="cancellationToken">Cancellation cancellationToken.</param>
        /// <returns>List with GitRevisions.</returns>
        private IReadOnlyCollection<GitRevision> GetRevisionsFromArguments(GitArgumentBuilder arguments, CancellationToken cancellationToken)
        {
            List<GitRevision> revisions = new();

            using IProcess process = _module.GitCommandRunner.RunDetached(cancellationToken, arguments, redirectOutput: true, outputEncoding: GitModule.LosslessEncoding);
            byte[] buffer = new byte[4096];

            foreach (ArraySegment<byte> chunk in process.StandardOutput.BaseStream.ReadNullTerminatedChunks(ref buffer))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (TryParseRevision(chunk, out GitRevision? revision))
                {
                    revisions.Add(revision);
                }
            }

            return revisions;
        }

        /// <summary>
        /// Get the git-log revisions for the revision grid.
        /// </summary>
        /// <param name="subject">Observer to update the revision grid when the revisions are available.</param>
        /// <param name="revisionFilter">Revision filter, including branch filter.</param>
        /// <param name="pathFilter">Pathfilter.</param>
        /// <param name="cancellationToken">Cancellation cancellationToken.</param>
        public void GetLog(
            IObserver<GitRevision> subject,
            string revisionFilter,
            string pathFilter,
            CancellationToken cancellationToken)
        {
#if TRACE_REVISIONREADER
            int revisionCount = 0;
            Stopwatch sw = Stopwatch.StartNew();
#endif
            ArgumentBuilder arguments = BuildArguments(revisionFilter, pathFilter);
            using IProcess process = _module.GitCommandRunner.RunDetached(cancellationToken, arguments, redirectOutput: true, outputEncoding: GitModule.LosslessEncoding);
#if DEBUG
            Debug.WriteLine($"git {arguments}");
#endif
            cancellationToken.ThrowIfCancellationRequested();

            byte[] buffer = new byte[4096];

            foreach (ArraySegment<byte> chunk in process.StandardOutput.BaseStream.ReadNullTerminatedChunks(ref buffer))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (TryParseRevision(chunk, out GitRevision? revision))
                {
#if TRACE_REVISIONREADER
                    revisionCount++;
#endif
                    subject.OnNext(revision);
                }
            }

#if TRACE_REVISIONREADER
            // TODO Make it possible to explicitly activate Trace printouts like this
            Trace.WriteLine($"**** [{nameof(RevisionReader)}] Emitted {revisionCount} revisions in {sw.Elapsed.TotalMilliseconds:#,##0.#} ms. bufferSize={buffer.Length} parseErrors={_noOfParseError}");
#endif

            if (!cancellationToken.IsCancellationRequested)
            {
                subject.OnCompleted();
            }
        }

        private ArgumentBuilder BuildArguments(string revisionFilter, string pathFilter)
        {
            return new GitArgumentBuilder("log")
            {
                "-z",
                $"--pretty=format:\"{LogFormat()}\"",

                // sorting
                { AppSettings.RevisionSortOrder == RevisionSortOrder.AuthorDate, "--author-date-order" },
                { AppSettings.RevisionSortOrder == RevisionSortOrder.Topology, "--topo-order" },

                revisionFilter,
                "--",
                { !string.IsNullOrWhiteSpace(pathFilter), pathFilter }
            };
        }

        [SuppressMessage("Style", "IDE0057:Use range operator", Justification = "Performance")]
        private bool TryParseRevision(in ArraySegment<byte> chunk, [NotNullWhen(returnValue: true)] out GitRevision? revision)
        {
            // The 'chunk' of data contains a complete git log item, encoded.
            // This method decodes that chunk and produces a revision object.

            // All values which can be read directly from the byte array are arranged
            // at the beginning of the chunk. The latter part of the chunk will require
            // decoding as a string.

            if (chunk.Count < ObjectId.Sha1CharCount * 2)
            {
                ParseAssert($"Log parse error, not enough data: {chunk.Count}");
                revision = default;
                return false;
            }

            #region Object ID, Tree ID, Parent IDs

            ReadOnlySpan<byte> array = chunk.AsSpan();

            // The first 40 bytes are the revision ID and the tree ID back to back
            if (!ObjectId.TryParse(array.Slice(0, ObjectId.Sha1CharCount), out ObjectId? objectId) ||
                !ObjectId.TryParse(array.Slice(ObjectId.Sha1CharCount, ObjectId.Sha1CharCount), out ObjectId? treeId))
            {
                ParseAssert($"Log parse error, object id: {chunk.Count}({array.Slice(0, ObjectId.Sha1CharCount).ToString()}");
                revision = default;
                return false;
            }

            int offset = ObjectId.Sha1CharCount * 2;

            // Next we have zero or more parent IDs separated by ' ' and terminated by '\n'
            int noParents = CountParents(in array, offset);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            int CountParents(in ReadOnlySpan<byte> array, int baseOffset)
            {
                int count = 0;
                while (baseOffset < array.Length && array[baseOffset] != '\n')
                {
                    if (count > 0)
                    {
                        // Except for the first parent, advance after the space
                        baseOffset++;
                    }

                    baseOffset += ObjectId.Sha1CharCount;
                    count++;

                    if (baseOffset >= array.Length || !(array[baseOffset] is (byte)'\n' or (byte)' '))
                    {
                        // Parse error, not using ParseAssert (or increasing _noOfParseError)
                        ParseAssert($"Log parse error, unexpected contents in the parent array: {baseOffset - offset} for {objectId}");
                        return -1;
                    }
                }

                return count;
            }

            ObjectId[] parentIds;
            if (noParents <= 0)
            {
                offset++;
                parentIds = Array.Empty<ObjectId>();
            }
            else
            {
                parentIds = new ObjectId[noParents];

                for (int parentIndex = 0; parentIndex < noParents; parentIndex++)
                {
                    if (!ObjectId.TryParse(array.Slice(offset, ObjectId.Sha1CharCount), out ObjectId parentId))
                    {
                        ParseAssert($"Log parse error, parent {parentIndex} for {objectId}");
                        revision = default;
                        return false;
                    }

                    parentIds[parentIndex] = parentId;
                    offset += ObjectId.Sha1CharCount + 1;
                }
            }

            #endregion

            #region Timestamps

            // Decimal ASCII seconds since the unix epoch, each terminated by `\n`
            long authorUnixTime = ParseUnixDateTime(in array);
            long commitUnixTime = ParseUnixDateTime(in array);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            long ParseUnixDateTime(in ReadOnlySpan<byte> array)
            {
                long unixTime = 0;

                while (offset < array.Length)
                {
                    int c = array[offset++];

                    if (c == '\n')
                    {
                        break;
                    }

                    unixTime = (unixTime * 10) + (c - '0');
                }

                return unixTime;
            }

            #endregion

            #region Encoded string values (names, emails, subject, body)

            // The remaining must be decoded (for above utf8/ascii must work)
            Span<char> decoded = stackalloc char[_logOutputEncoding.GetMaxByteCount(array.Slice(offset).Length)];
            int decodedLength = _logOutputEncoding.GetChars(array.Slice(offset), decoded);
            offset = 0;

            // reflogSelector are only used when listing stashes
            string? reflogSelector = _hasReflogSelector ? GetNextLine(decoded, useStringPool: false) : null;

            // Authors etc are limited, use a shared string pool
            string author = GetNextLine(decoded, useStringPool: true);
            string authorEmail = GetNextLine(decoded, useStringPool: true);
            string committer = GetNextLine(decoded, useStringPool: true);
            string committerEmail = GetNextLine(decoded, useStringPool: true);

            // Keep a full multiline message body within the last six months (by default).
            // Note also that if body and subject are identical (single line), the body never need to be stored
            bool keepBody = authorUnixTime >= _oldestBody;
            GetSubjectBody(decoded[offset..decodedLength].Trim(), out string? subject, out string? body, out bool hasMultiLineMessage, in keepBody);

            if (author is null || authorEmail is null || committer is null || committerEmail is null || subject is null || (keepBody && hasMultiLineMessage && body is null))
            {
                ParseAssert($"Log parse error, decoded fields ({subject}::{body}) for {objectId}");
                revision = default;
                return false;
            }

            #endregion

            revision = new GitRevision(objectId)
            {
                ParentIds = parentIds,
                TreeGuid = treeId,
                Author = author,
                AuthorEmail = authorEmail,
                AuthorUnixTime = authorUnixTime,
                Committer = committer,
                CommitterEmail = committerEmail,
                CommitUnixTime = commitUnixTime,
                Subject = subject,
                Body = body,
                HasMultiLineMessage = hasMultiLineMessage,
                HasNotes = false,
                ReflogSelector = reflogSelector,
            };

            return true;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            string? GetNextLine(ReadOnlySpan<char> s, in bool useStringPool)
            {
                if (offset >= s.Length)
                {
                    return null;
                }

                int lineLength = s.Slice(offset).IndexOf('\n');
                if (lineLength == -1)
                {
                    // A line must be terminated
                    return null;
                }

                ReadOnlySpan<char> r = s.Slice(offset, lineLength);
                offset += lineLength + 1;
                return useStringPool ? StringPool.Shared.GetOrAdd(r) : r.ToString();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void GetSubjectBody(ReadOnlySpan<char> bodySlice, out string? subject, out string? body, out bool hasMultiLineMessage, in bool keepBody)
            {
                // Subject can also be defined as the contents before empty line (%s for --pretty),
                // this uses the alternative definition of first line in body.
                // Handle '\v' (Shift-Enter) as '\n' for users that by habit avoid Enter to 'send'
                int lengthSubject = bodySlice.IndexOfAny('\n', '\v');
                hasMultiLineMessage = lengthSubject >= 0;
                subject = (hasMultiLineMessage
                    ? bodySlice.Slice(0, lengthSubject).TrimEnd()
                    : bodySlice)
                    .ToString();

                // See caller for reasoning when message body can be omitted
                // (String interning makes hasMultiLineMessage check only for clarity)
                body = keepBody && hasMultiLineMessage
                    ? bodySlice.ToString().Replace('\v', '\n')
                    : null;
            }

            void ParseAssert(string message)
            {
                _noOfParseError++;
                Debug.Assert(!Debugger.IsAttached || _noOfParseError > 1, message);
                Trace.WriteLineIf(_noOfParseError < 10, message);
            }
        }

        internal TestAccessor GetTestAccessor()
            => new(this);

        internal readonly struct TestAccessor
        {
            private readonly RevisionReader _revisionReader;

            internal TestAccessor(RevisionReader revisionReader)
            {
                _revisionReader = revisionReader;
            }

            internal static RevisionReader RevisionReader(GitModule module, bool hasReflogSelector, Encoding logOutputEncoding, long sixMonths)
            {
                RevisionReader reader = new(module, logOutputEncoding, sixMonths)
                {
                    _hasReflogSelector = hasReflogSelector
                };
                return reader;
            }

            internal ArgumentBuilder BuildArguments(string revisionFilter, string pathFilter) =>
                _revisionReader.BuildArguments(revisionFilter, pathFilter);

            internal bool TryParseRevision(ArraySegment<byte> chunk, [NotNullWhen(returnValue: true)] out GitRevision? revision) =>
                _revisionReader.TryParseRevision(chunk, out revision);

            internal int NoOfParseError
            {
                get { return _revisionReader._noOfParseError; }
                set { _revisionReader._noOfParseError = value; }
            }
        }
    }
}
