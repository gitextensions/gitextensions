using System.Buffers.Text;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using GitExtUtils;
using GitUIPluginInterfaces;
using Microsoft.Toolkit.HighPerformance;
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
            /* Author name     */ "%aN%n" +
            /* Author email    */ "%aE%n" +
            /* Committer name  */ "%cN%n" +
            /* Committer email */ "%cE%n" +
            /* Reflog selector placeholder */ "{0}" +
            /* Commit raw body */ "%B" +
            /* Notes placeholder */ "{1}";

        private const string _reflogSelectorFormat = "%gD%n";
        private const string _notesPrefix = "Notes:";
        private const string _notesMarker = $"\n{_notesPrefix}";
        private const string _notesFormat = $"%n{_notesPrefix}%n%N";

        // Trace info for parse errors
        private int _noOfParseError = 0;

        private readonly IGitModule _module;
        private readonly Encoding _logOutputEncoding;
        private readonly long _oldestBody;
        private const int _offsetDaysForOldestBody = 6 * 30; // about 6 months

        // reflog selector to identify stashes
        private bool _hasReflogSelector;

        // Include Git Notes for the commit
        private bool _hasNotes;

        // Buffer to decode subject
        private char[] _decodeBuffer = new char[4096];

        public RevisionReader(IGitModule module, bool allBodies = false)
            : this(module, module.LogOutputEncoding, allBodies ? 0 : GetUnixTimeForOffset(_offsetDaysForOldestBody))
        {
        }

        private RevisionReader(IGitModule module, Encoding logOutputEncoding, long oldestBody)
        {
            _module = module;
            _logOutputEncoding = logOutputEncoding;
            _oldestBody = oldestBody;
        }

        /// <summary>
        /// Return the git-log format and set the reflogSelector flag
        /// (used when parsing the buffer).
        /// </summary>
        private string GetLogFormat(bool hasReflogSelector = false, bool hasNotes = false)
        {
            _hasReflogSelector = hasReflogSelector;
            _hasNotes = hasNotes;

            return string.Format(_fullFormat, hasReflogSelector ? _reflogSelectorFormat : "", hasNotes ? _notesFormat : "");
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
                $"--pretty=format:\"{GetLogFormat(hasReflogSelector: true)}\""
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
                $"--pretty=format:\"{GetLogFormat()}\"",
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
                $"--pretty=format:\"{GetLogFormat()}\"",
                $"{olderCommitHash}~..{newerCommitHash}"
            };

            return GetRevisionsFromArguments(arguments, cancellationToken);
        }

        /// <summary>
        /// Retrieves the GitRevision.
        /// </summary>
        /// <param name="commitHash">The Git commit hash.</param>
        /// <param name="hasNotes">A cancellation token.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The retrieved git history.</returns>
        public GitRevision GetRevision(string commitHash, bool hasNotes, CancellationToken cancellationToken)
        {
            GitArgumentBuilder arguments = new("log")
            {
                "-z",
                "-1",
                $"--pretty=format:\"{GetLogFormat(hasNotes: hasNotes)}\"",
                commitHash
            };

            // output can be cached if Git Notes is not included
            if (!hasNotes && GitModule.GitCommandCache.TryGet(arguments.ToString(), out byte[]? commandOutput, out _) is true)
            {
                // OK
            }
            else
            {
#if DEBUG
                Debug.WriteLine($"git {arguments}");
#endif
                using IProcess process = _module.GitCommandRunner.RunDetached(cancellationToken, arguments, redirectOutput: true, outputEncoding: null);
                commandOutput = process.StandardOutput.BaseStream.SplitLogOutput().SingleOrDefault().ToArray();
            }

            if (!TryParseRevision(commandOutput, out GitRevision? revision))
            {
                return null;
            }

            if (!hasNotes)
            {
                GitModule.GitCommandCache.Add(arguments.ToString(), commandOutput, commandOutput);
            }

            return revision;
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
            foreach (ReadOnlyMemory<byte> chunk in process.StandardOutput.BaseStream.SplitLogOutput())
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (TryParseRevision(chunk.Span, out GitRevision? revision))
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
        /// <param name="hasNotes">Include Git Notes.</param>
        /// <param name="cancellationToken">Cancellation cancellationToken.</param>
        public void GetLog(
            IObserver<GitRevision> subject,
            string revisionFilter,
            string pathFilter,
            bool hasNotes,
            CancellationToken cancellationToken)
        {
#if TRACE_REVISIONREADER
            int revisionCount = 0;
            Stopwatch sw = Stopwatch.StartNew();
#endif
            ArgumentBuilder arguments = BuildArguments(revisionFilter, pathFilter, hasNotes);
#if DEBUG
            Debug.WriteLine($"git {arguments}");
#endif
            using IProcess process = _module.GitCommandRunner.RunDetached(cancellationToken, arguments, redirectOutput: true, outputEncoding: GitModule.LosslessEncoding);
            cancellationToken.ThrowIfCancellationRequested();

            foreach (ReadOnlyMemory<byte> chunk in process.StandardOutput.BaseStream.SplitLogOutput())
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (TryParseRevision(chunk.Span, out GitRevision? revision))
                {
#if TRACE_REVISIONREADER
                    revisionCount++;
#endif
                    subject.OnNext(revision);
                }
            }

#if TRACE_REVISIONREADER
            // TODO Make it possible to explicitly activate Trace printouts like this
            Trace.WriteLine($"**** [{nameof(RevisionReader)}] Emitted {revisionCount} revisions in {sw.Elapsed.TotalMilliseconds:#,##0.#} ms. parseErrors={_noOfParseError}");
#endif

            if (!cancellationToken.IsCancellationRequested)
            {
                subject.OnCompleted();
            }
        }

        private ArgumentBuilder BuildArguments(string revisionFilter, string pathFilter, bool hasNotes)
        {
            return new GitArgumentBuilder("log")
            {
                "-z",
                $"--pretty=format:\"{GetLogFormat(hasNotes: hasNotes)}\"",

                // sorting
                { AppSettings.RevisionSortOrder == RevisionSortOrder.AuthorDate, "--author-date-order" },
                { AppSettings.RevisionSortOrder == RevisionSortOrder.Topology, "--topo-order" },

                revisionFilter,
                "--",
                { !string.IsNullOrWhiteSpace(pathFilter), pathFilter }
            };
        }

        [SuppressMessage("Style", "IDE0057:Use range operator", Justification = "Performance")]
        private bool TryParseRevision(in ReadOnlySpan<byte> buffer, [NotNullWhen(returnValue: true)] out GitRevision? revision)
        {
            // The 'chunk' of data contains a complete git log item, encoded.
            // This method decodes that chunk and produces a revision object.

            // All values which can be read directly from the byte array are arranged
            // at the beginning of the chunk. The latter part of the chunk will require
            // decoding as a string.

            if (buffer.Length < ObjectId.Sha1CharCount * 2)
            {
                ParseAssert($"Log parse error, not enough data: {buffer.Length}");
                revision = default;
                return false;
            }

            #region Object ID, Tree ID, Parent IDs

            // The first 40 bytes are the revision ID and the tree ID back to back
            if (!ObjectId.TryParse(buffer.Slice(0, ObjectId.Sha1CharCount), out ObjectId? objectId) ||
                !ObjectId.TryParse(buffer.Slice(ObjectId.Sha1CharCount, ObjectId.Sha1CharCount), out ObjectId? treeId))
            {
                ParseAssert($"Log parse error, object id: {buffer.Length}({buffer.Slice(0, ObjectId.Sha1CharCount).ToString()}");
                revision = default;
                return false;
            }

            int offset = ObjectId.Sha1CharCount * 2;

            // Next we have zero or more parent IDs separated by ' ' and terminated by '\n'
            int noParents = CountParents(in buffer, offset);

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

                    if (baseOffset >= array.Length || ((char)array[baseOffset] is not ('\n' or ' ')))
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
                    if (!ObjectId.TryParse(buffer.Slice(offset, ObjectId.Sha1CharCount), out ObjectId parentId))
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

            // Decimal ASCII seconds since the unix epoch
            if (!Utf8Parser.TryParse(buffer.Slice(offset), out long authorUnixTime, out int bytesConsumed))
            {
                ParseAssert($"Log parse error, not enough data for authortime: {buffer.Length} {offset} {buffer.Slice(offset).ToString()}");
                revision = default;
                return false;
            }

            offset += bytesConsumed + 1;
            if (!Utf8Parser.TryParse(buffer.Slice(offset), out long commitUnixTime, out bytesConsumed))
            {
                ParseAssert($"Log parse error, not enough data for committime: {buffer.Length} {offset} {buffer.Slice(offset).ToString()}");
                revision = default;
                return false;
            }

            offset += bytesConsumed + 1;

            #endregion

            #region Encoded string values (names, emails, subject, body)

            // The remaining must be decoded (for above utf8/ascii must work)
            // First records are decoded on the stack
            // The attributes are decoded in the order they are defined in the format string
            revision = new GitRevision(objectId)
            {
                ParentIds = parentIds,
                TreeGuid = treeId,

                Author = GetNextLine(buffer),
                AuthorEmail = GetNextLine(buffer),
                AuthorUnixTime = authorUnixTime,
                Committer = GetNextLine(buffer),
                CommitterEmail = GetNextLine(buffer),
                CommitUnixTime = commitUnixTime
            };

            // Body is occasionally big, like linux repo has 35K bytes, the buffer is over 100K
            // Use a backing buffer on the heap
            int maxChars = _logOutputEncoding.GetMaxByteCount(buffer.Slice(offset).Length);
            if (maxChars > _decodeBuffer.Length)
            {
                // Default should be sufficient for most repos, Linux though has
                // unencoded of 36K, which results in maxChars being greater than 100K
                int newSize = _decodeBuffer.Length;
                while (newSize < maxChars)
                {
                    newSize *= 2;
                }

                _decodeBuffer = new char[newSize];
            }

            int decodedLength = _logOutputEncoding.GetChars(buffer.Slice(offset), _decodeBuffer);
            Span<char> decoded = _decodeBuffer.AsSpan(0, decodedLength).TrimEnd();

            // reflogSelector are only used when listing stashes
            if (_hasReflogSelector)
            {
                int lineLength = decoded.IndexOf('\n');
                if (lineLength == -1)
                {
                    ParseAssert($"Log parse error, parent no reflogselector for {objectId}");
                    revision = default;
                    return false;
                }

                revision.ReflogSelector = lineLength > 0 ? decoded.Slice(0, lineLength).ToString() : null;
                decoded = decoded.Slice(lineLength + 1);
            }

            // Keep a full multi-line message body within the last six months (by default).
            // Note also that if body and subject are identical (single line), the body never need to be stored
            bool keepBody = authorUnixTime >= _oldestBody;

            // Subject can also be defined as the contents before empty line (%s for --pretty),
            // this uses the alternative definition of first line in body.
            int lengthSubject = decoded.IndexOfAny(Delimiters.LineAndVerticalFeed);
            revision.HasMultiLineMessage = _hasNotes
                ? decoded.Length != lengthSubject + _notesMarker.Length + 1 // Notes must always include the notes marker
                : lengthSubject >= 0;

            revision.Subject = (lengthSubject >= 0
                ? decoded.Slice(0, lengthSubject).TrimEnd()
                : decoded)
                .ToString();

            if (keepBody && revision.HasMultiLineMessage)
            {
                // Handle '\v' (Shift-Enter) as '\n' for users that by habit avoid Enter to 'send'
                int currentOffset = lengthSubject;
                int verticalFeedIndex;
                while ((verticalFeedIndex = decoded.Slice(currentOffset).IndexOf('\v')) >= 0)
                {
                    currentOffset += verticalFeedIndex;
                    decoded[currentOffset] = '\n';
                    currentOffset++;
                }

                // Removes empty Notes markers (this is the most common case)
                bool hasNonEmptyNotes = _hasNotes;
                if (hasNonEmptyNotes)
                {
                    if (decoded.EndsWith(_notesMarker))
                    {
                        // Remove the empty marker
                        decoded = decoded[..^_notesMarker.Length].TrimEnd();
                        hasNonEmptyNotes = false;
                    }
                }

                if (hasNonEmptyNotes)
                {
                    // Format Notes, add indentation
                    int notesStartIndex = ((ReadOnlySpan<char>)decoded).IndexOf(_notesMarker, StringComparison.Ordinal);

                    StringBuilder message = new();
                    currentOffset = notesStartIndex + _notesMarker.Length + 1;
                    message.Append(decoded.Slice(0, currentOffset));
                    while (currentOffset < decoded.Length)
                    {
                        message.Append("    ");
                        int lineLength = decoded.Slice(currentOffset).IndexOf('\n');
                        if (lineLength == -1)
                        {
                            message.Append(decoded.Slice(currentOffset));
                            break;
                        }
                        else
                        {
                            message.Append(decoded.Slice(currentOffset, lineLength))
                                .Append('\n');
                        }

                        currentOffset += lineLength + 1;
                    }

                    revision.Body = message.ToString();
                }
                else
                {
                    revision.Body = decoded.ToString();
                }
            }

            if (_hasNotes)
            {
                revision.HasNotes = true;
            }
#if DEBUG
            if (revision.Author is null || revision.AuthorEmail is null || revision.Committer is null || revision.CommitterEmail is null || revision.Subject is null || (keepBody && revision.HasMultiLineMessage && revision.Body is null))
            {
                ParseAssert($"Log parse error, decoded fields ({revision.Subject}::{revision.Body}) for {objectId}");
                revision = default;
                return false;
            }
#endif

            #endregion

            return true;

            // Authors etc are limited, use a shared string pool
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            string? GetNextLine(in ReadOnlySpan<byte> s)
            {
                if (offset >= s.Length)
                {
                    return null;
                }

                int lineLength = s.Slice(offset).IndexOf((byte)'\n');
                if (lineLength == -1)
                {
                    // A line must be terminated
                    return null;
                }

                ReadOnlySpan<byte> r = s.Slice(offset, lineLength);
                offset += lineLength + 1;
                return StringPool.Shared.GetOrAdd(r, _logOutputEncoding);
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

            internal static RevisionReader RevisionReader(GitModule module, Encoding logOutputEncoding, long sixMonths)
            {
                RevisionReader reader = new(module, logOutputEncoding, sixMonths);
                return reader;
            }

            internal void SetParserAttributes(bool hasReflogSelector, bool hasNotes)
            {
                _revisionReader._hasReflogSelector = hasReflogSelector;
                _revisionReader._hasNotes = hasNotes;
            }

            internal ArgumentBuilder BuildArguments(string revisionFilter, string pathFilter) =>
                _revisionReader.BuildArguments(revisionFilter, pathFilter, hasNotes: false);

            internal bool TryParseRevision(ReadOnlySpan<byte> chunk, [NotNullWhen(returnValue: true)] out GitRevision? revision) =>
                _revisionReader.TryParseRevision(chunk, out revision);

            internal int NoOfParseError
            {
                get { return _revisionReader._noOfParseError; }
                set { _revisionReader._noOfParseError = value; }
            }
        }
    }
}
