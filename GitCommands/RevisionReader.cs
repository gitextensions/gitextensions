﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GitExtUtils;
using GitUI;
using GitUIPluginInterfaces;
using Microsoft.Toolkit.HighPerformance.Buffers;
using Microsoft.VisualStudio.Threading;

namespace GitCommands
{
    public sealed class RevisionReader
    {
        private const string FullFormat =

            // These header entries can all be decoded from the bytes directly.
            // Each hash is 20 bytes long.

            /* Object ID       */ "%H" +
            /* Tree ID         */ "%T" +
            /* Parent IDs      */ "%P%n" +
            /* Author date     */ "%at%n" +
            /* Commit date     */ "%ct%n" +

            // Items below here must be decoded as strings to support non-ASCII.
            /* Author name     */ "%aN%n" +
            /* Author email    */ "%aE%n" +
            /* Committer name  */ "%cN%n" +
            /* Committer email */ "%cE%n" +
            /* Commit raw body */ "%B";

        // Trace info for parse errors
        private int _noOfParseError = 0;

        private readonly GitModule _module;
        private readonly Encoding _logOutputEncoding;
        private readonly long _sixMonths;

        public RevisionReader(GitModule module)
            : this(module, module.LogOutputEncoding, new DateTimeOffset(DateTime.Now.ToUniversalTime() - TimeSpan.FromDays(30 * 6)).ToUnixTimeSeconds())
        {
        }

        internal RevisionReader(GitModule module, Encoding logOutputEncoding, long sixMonths)
        {
            _module = module;
            _logOutputEncoding = logOutputEncoding;
            _sixMonths = sixMonths;
        }

        public async Task GetLogAsync(
            IObserver<GitRevision> subject,
            ArgumentBuilder arguments,
            CancellationToken token)
        {
            await TaskScheduler.Default;
            token.ThrowIfCancellationRequested();

#if DEBUG
            int revisionCount = 0;
            var sw = Stopwatch.StartNew();
#endif

            using (var process = _module.GitCommandRunner.RunDetached(arguments, redirectOutput: true, outputEncoding: GitModule.LosslessEncoding))
            {
#if DEBUG
                Debug.WriteLine($"git {arguments}");
#endif
                token.ThrowIfCancellationRequested();

                var buffer = new byte[4096];

                foreach (var chunk in process.StandardOutput.BaseStream.ReadNullTerminatedChunks(ref buffer))
                {
                    token.ThrowIfCancellationRequested();

                    if (TryParseRevision(chunk, out GitRevision? revision))
                    {
#if DEBUG
                        revisionCount++;
#endif
                        subject.OnNext(revision);
                    }
                }

#if DEBUG
                // TODO Make it possible to explicitly activate Trace printouts like this
                Debug.WriteLine($"**** [{nameof(RevisionReader)}] Emitted {revisionCount} revisions in {sw.Elapsed.TotalMilliseconds:#,##0.#} ms. bufferSize={buffer.Length} parseErrors={_noOfParseError}");
#endif
            }

            if (!token.IsCancellationRequested)
            {
                subject.OnCompleted();
            }
        }

        public ArgumentBuilder BuildArguments(int maxCount,
            RefFilterOptions refFilterOptions,
            string branchFilter,
            string revisionFilter,
            string pathFilter,
            out bool parentsAreRewritten)
        {
            parentsAreRewritten = !string.IsNullOrWhiteSpace(pathFilter) || !string.IsNullOrWhiteSpace(revisionFilter);

            return new GitArgumentBuilder("log")
            {
                { maxCount > 0, $"--max-count={maxCount}" },
                "-z",
                $"--pretty=format:\"{FullFormat}\"",
                { AppSettings.RevisionSortOrder == RevisionSortOrder.AuthorDate, "--author-date-order" },
                { AppSettings.RevisionSortOrder == RevisionSortOrder.Topology, "--topo-order" },
                { refFilterOptions.HasFlag(RefFilterOptions.Boundary), "--boundary" },
                { refFilterOptions.HasFlag(RefFilterOptions.NoMerges), "--no-merges" },
                { refFilterOptions.HasFlag(RefFilterOptions.SimplifyByDecoration), "--simplify-by-decoration" },
                { refFilterOptions.HasFlag(RefFilterOptions.FirstParent), "--first-parent" },
                {
                    refFilterOptions.HasFlag(RefFilterOptions.Reflogs),
                    "--reflog",
                    new ArgumentBuilder
                    {
                        {
                            refFilterOptions.HasFlag(RefFilterOptions.All),
                            new ArgumentBuilder
                            {
                                { refFilterOptions.HasFlag(RefFilterOptions.NoGitNotes), "--not --glob=notes --not" },
                                { refFilterOptions.HasFlag(RefFilterOptions.NoStash), "--exclude=refs/stash" },
                                "--all",
                            }.ToString(),
                            new ArgumentBuilder
                            {
                                {
                                    (refFilterOptions.HasFlag(RefFilterOptions.Branches) && !string.IsNullOrWhiteSpace(branchFilter)),
                                    new ArgumentBuilder
                                    {
                                        {
                                            IsSimpleBranchFilter(branchFilter),
                                            branchFilter,
                                            "--branches=" + branchFilter
                                        }
                                    }.ToString()
                                },
                            }.ToString()
                        }
                    }.ToString()
                },
                revisionFilter,
                {
                    parentsAreRewritten,
                    new ArgumentBuilder
                    {
                        { AppSettings.FullHistoryInFileHistory, $"--full-history" },
                        {
                            AppSettings.FullHistoryInFileHistory && AppSettings.SimplifyMergesInFileHistory,
                            $"--simplify-merges"
                        },
                        $"--parents"
                    }.ToString()
                },
                { !string.IsNullOrWhiteSpace(pathFilter), $"-- {pathFilter}" }
            };

            static bool IsSimpleBranchFilter(string branchFilter) =>
                branchFilter.IndexOfAny(new[] { '?', '*', '[' }) == -1;
        }

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
            if (!ObjectId.TryParseAsciiHexReadOnlySpan(array.Slice(0, ObjectId.Sha1CharCount), out var objectId) ||
                !ObjectId.TryParseAsciiHexReadOnlySpan(array.Slice(ObjectId.Sha1CharCount, ObjectId.Sha1CharCount), out var treeId))
            {
                ParseAssert($"Log parse error, object id: {chunk.Count}({array.Slice(0, ObjectId.Sha1CharCount).ToString()}");
                revision = default;
                return false;
            }

            var offset = ObjectId.Sha1CharCount * 2;

            // Next we have zero or more parent IDs separated by ' ' and terminated by '\n'
            int noParents = CountParents(in array, offset);
            if (noParents < 0)
            {
                ParseAssert($"Log parse error, {noParents} no of parents for {objectId}");
                revision = default;
                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            int CountParents(in ReadOnlySpan<byte> array, int baseOffset)
            {
                int count = 0;

                while (baseOffset < array.Length && array[baseOffset] != '\n')
                {
                    // Parse error, not using ParseAssert (or increasing _noOfParseError)
                    Debug.Assert(count == 0 || array[baseOffset] == ' ', $"Log parse error, unexpected contents in the parent array: {array[baseOffset]}/{count} for {objectId}");
                    baseOffset += ObjectId.Sha1CharCount;
                    if (count > 0)
                    {
                        // Except for the first parent, advance after the space
                        baseOffset++;
                    }

                    count++;
                }

                if (baseOffset >= array.Length || array[baseOffset] != '\n')
                {
                    return -1;
                }

                return count;
            }

            var parentIds = new ObjectId[noParents];

            if (noParents == 0)
            {
                offset++;
            }
            else
            {
                for (int parentIndex = 0; parentIndex < noParents; parentIndex++)
                {
                    if (!ObjectId.TryParseAsciiHexReadOnlySpan(array.Slice(offset, ObjectId.Sha1CharCount), out ObjectId parentId))
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

            // Lines 2 and 3 are timestamps, as decimal ASCII seconds since the unix epoch, each terminated by `\n`
            var authorUnixTime = ParseUnixDateTime(in array);
            var commitUnixTime = ParseUnixDateTime(in array);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            long ParseUnixDateTime(in ReadOnlySpan<byte> array)
            {
                long unixTime = 0;

                while (true)
                {
                    int c = array[offset++];

                    if (c == '\n')
                    {
                        return unixTime;
                    }

                    unixTime = (unixTime * 10) + (c - '0');
                }
            }

            #endregion

            #region Encoded string values (names, emails, subject, body)

            // Finally, decode the names, email, subject and body strings using the required text encoding
            ReadOnlySpan<char> s = _logOutputEncoding.GetString(array[offset..]).AsSpan();
            StringLineReader reader = new(in s);

            var author = reader.ReadLine();
            var authorEmail = reader.ReadLine();
            var committer = reader.ReadLine();
            var committerEmail = reader.ReadLine();

            bool skipBody = _sixMonths > authorUnixTime;
            (string? subject, string? body, bool hasMultiLineMessage) = reader.PeekSubjectBody(skipBody);

            // We keep a full multiline message body within the last six months.
            // Note also that if body and subject are identical (single line), the body never need to be stored
            skipBody = skipBody || !hasMultiLineMessage;

            if (author is null || authorEmail is null || committer is null || committerEmail is null || subject is null || (skipBody != (body is null)))
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
                HasNotes = false
            };

            return true;

            void ParseAssert(string? message)
            {
                _noOfParseError++;
                Debug.Assert(_noOfParseError > 1, message);
                Trace.WriteLineIf(_noOfParseError < 10, message);
            }
        }

        #region Nested type: StringLineReader

        /// <summary>
        /// Simple type to walk along a string, line by line, without redundant allocations.
        /// </summary>
        internal ref struct StringLineReader
        {
            private readonly ReadOnlySpan<char> _s;
            private int _index;

            public StringLineReader(in ReadOnlySpan<char> s)
            {
                _s = s;
                _index = 0;
            }

            public string? ReadLine()
            {
                if (_index >= _s.Length)
                {
                    return null;
                }

                int lineLength = _s[_index..].IndexOf('\n');
                if (lineLength == -1)
                {
                    // A line must be terminated
                    return null;
                }

                int startIndex = _index;
                _index += lineLength + 1;
                return StringPool.Shared.GetOrAdd(_s.Slice(startIndex, lineLength));
            }

            public (string? subject, string? body, bool hasMultiLineMessage) PeekSubjectBody(bool skipBody)
            {
                // Empty subject is allowed
                if (_index > _s.Length)
                {
                    return (null, null, false);
                }

                ReadOnlySpan<char> bodySlice = _s[_index..].Trim();

                // Subject can also be defined as the contents before empty line (%s for --pretty),
                // this uses the alternative definition of first line in body.
                int lengthSubject = bodySlice.IndexOf('\n');
                bool hasMultiLineMessage = lengthSubject >= 0;
                string subject = hasMultiLineMessage
                    ? bodySlice.Slice(0, lengthSubject).TrimEnd().ToString()
                    : bodySlice.ToString();

                // See caller for reasoning when message body can be omitted
                // (String interning makes hasMultiLineMessage check only for clarity)
                string? body = skipBody || !hasMultiLineMessage
                    ? null
                    : bodySlice.ToString();

                return (subject, body, hasMultiLineMessage);
            }
        }

        #endregion

        internal TestAccessor GetTestAccessor()
            => new(this);

        internal readonly struct TestAccessor
        {
            private readonly RevisionReader _revisionReader;

            internal TestAccessor(RevisionReader revisionReader)
            {
                _revisionReader = revisionReader;
            }

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
