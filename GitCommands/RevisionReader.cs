using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using GitExtUtils;
using GitUI;
using GitUIPluginInterfaces;
using Microsoft.Toolkit.HighPerformance.Buffers;
using Microsoft.VisualStudio.Threading;

namespace GitCommands
{
#pragma warning disable SA1025 // Code should not contain multiple whitespace in a row
    [Flags]
    public enum RefFilterOptions
    {
        None                    = 0x000,
        Branches                = 0x001,    // --branches
        Remotes                 = 0x002,    // --remotes
        Tags                    = 0x004,    // --tags
        Stashes                 = 0x008,    //
        All                     = 0x00F,    // --all
        Boundary                = 0x010,    // --boundary
        ShowGitNotes            = 0x020,    // --not --glob=notes --not
        NoMerges                = 0x040,    // --no-merges
        FirstParent             = 0x080,    // --first-parent
        SimplifyByDecoration    = 0x100,    // --simplify-by-decoration
        Reflogs                 = 0x200,    // --reflog
    }
#pragma warning restore SA1025 // Code should not contain multiple whitespace in a row

    public sealed class RevisionReader : IDisposable
    {
        private const string FullFormat =

            // These header entries can all be decoded from the bytes directly.
            // Each hash is 20 bytes long.

            /* Object ID       */ "%H" +
            /* Tree ID         */ "%T" +
            /* Parent IDs      */ "%P%n" +
            /* Author date     */ "%at%n" +
            /* Commit date     */ "%ct%n" +
            /* Encoding        */ "%e%n" +

            // Items below here must be decoded as strings to support non-ASCII.
            /* Author name     */ "%aN%n" +
            /* Author email    */ "%aE%n" +
            /* Committer name  */ "%cN%n" +
            /* Committer email */ "%cE%n" +
            /* Commit raw body */ "%B";

        // Trace info for parse errors
        private static int _noOfParseError = 0;
        private readonly CancellationTokenSequence _cancellationTokenSequence = new();

        public void Execute(
            GitModule module,
            IReadOnlyList<IGitRef> refs,
            IObserver<GitRevision> subject,
            int maxCount,
            RefFilterOptions refFilterOptions,
            string branchFilter,
            string revisionFilter,
            string pathFilter,
            Func<GitRevision, bool>? revisionPredicate)
        {
            ThreadHelper.JoinableTaskFactory
                .RunAsync(() => ExecuteAsync(module, refs, subject, maxCount, refFilterOptions, branchFilter, revisionFilter, pathFilter, revisionPredicate))
                .FileAndForget(
                    ex =>
                    {
                        subject.OnError(ex);
                        return false;
                    });
        }

        private async Task ExecuteAsync(
            GitModule module,
            IReadOnlyList<IGitRef> refs,
            IObserver<GitRevision> subject,
            int maxCount,
            RefFilterOptions refFilterOptions,
            string branchFilter,
            string revisionFilter,
            string pathFilter,
            Func<GitRevision, bool>? revisionPredicate)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var token = _cancellationTokenSequence.Next();

            var revisionCount = 0;

            await TaskScheduler.Default;

            token.ThrowIfCancellationRequested();

            var branchName = module.IsValidGitWorkingDir()
                ? module.GetSelectedBranch()
                : "";

            token.ThrowIfCancellationRequested();

            UpdateSelectedRef(module, refs, branchName);
            var refsByObjectId = refs.ToLookup(head => head.ObjectId);

            token.ThrowIfCancellationRequested();

            var arguments = BuildArguments(maxCount, refFilterOptions, branchFilter, revisionFilter, pathFilter);

#if DEBUG
            var sw = Stopwatch.StartNew();
#endif

            var logOutputEncoding = module.LogOutputEncoding;
            long sixMonths = new DateTimeOffset(DateTime.Now.ToUniversalTime() - TimeSpan.FromDays(30 * 6)).ToUnixTimeSeconds();
            Func<string?, Encoding> getEncodingByGitName = (name) => module.GetEncodingByGitName(name);

            using (var process = module.GitCommandRunner.RunDetached(arguments, redirectOutput: true, outputEncoding: GitModule.LosslessEncoding))
            {
                token.ThrowIfCancellationRequested();

                var buffer = new byte[4096];

                foreach (var chunk in process.StandardOutput.BaseStream.ReadNullTerminatedChunks(ref buffer))
                {
                    token.ThrowIfCancellationRequested();

                    if (TryParseRevision(chunk, getEncodingByGitName, logOutputEncoding, sixMonths, out var revision)
                        && (revisionPredicate is null || revisionPredicate(revision)))
                    {
                        // Look up any refs associated with this revision
                        revision.Refs = refsByObjectId[revision.ObjectId].AsReadOnlyList();

                        revisionCount++;

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

        private ArgumentBuilder BuildArguments(int maxCount,
            RefFilterOptions refFilterOptions,
            string branchFilter,
            string revisionFilter,
            string pathFilter)
        {
            bool needParentRewrite = !string.IsNullOrWhiteSpace(pathFilter) || !string.IsNullOrWhiteSpace(revisionFilter);
            return new GitArgumentBuilder("log")
            {
                { maxCount > 0, $"--max-count={maxCount}" },
                "-z",
                {
                    !string.IsNullOrWhiteSpace(branchFilter) && IsSimpleBranchFilter(branchFilter),
                    branchFilter
                },
                $"--pretty=format:\"{FullFormat}\"",
                {
                    refFilterOptions.HasFlag(RefFilterOptions.FirstParent),
                    "--first-parent",
                    new ArgumentBuilder
                    {
                        { refFilterOptions.HasFlag(RefFilterOptions.Reflogs), "--reflog" },
                        { AppSettings.SortByAuthorDate, "--author-date-order" },
                        {
                            refFilterOptions.HasFlag(RefFilterOptions.All),
                            "--all",
                            new ArgumentBuilder
                            {
                                {
                                    refFilterOptions.HasFlag(RefFilterOptions.Branches) &&
                                    !string.IsNullOrWhiteSpace(branchFilter) && !IsSimpleBranchFilter(branchFilter),
                                    "--branches=" + branchFilter
                                },
                                { refFilterOptions.HasFlag(RefFilterOptions.Remotes), "--remotes" },
                                { refFilterOptions.HasFlag(RefFilterOptions.Tags), "--tags" },
                            }.ToString()
                        },
                        { refFilterOptions.HasFlag(RefFilterOptions.Boundary), "--boundary" },
                        { refFilterOptions.HasFlag(RefFilterOptions.ShowGitNotes), "--not --glob=notes --not" },
                        { refFilterOptions.HasFlag(RefFilterOptions.NoMerges), "--no-merges" },
                        { refFilterOptions.HasFlag(RefFilterOptions.SimplifyByDecoration), "--simplify-by-decoration" }
                    }.ToString()
                },
                revisionFilter,
                {
                    needParentRewrite,
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
        }

        private static bool IsSimpleBranchFilter(string branchFilter) =>
            branchFilter.IndexOfAny(new[] { '?', '*', '[' }) == -1;

        private static void UpdateSelectedRef(GitModule module, IReadOnlyList<IGitRef> refs, string branchName)
        {
            var selectedRef = refs.FirstOrDefault(head => head.Name == branchName);

            if (selectedRef is not null)
            {
                selectedRef.IsSelected = true;

                var localConfigFile = module.LocalConfigFile;
                var selectedHeadMergeSource = refs.FirstOrDefault(
                    head => head.IsRemote
                         && selectedRef.GetTrackingRemote(localConfigFile) == head.Remote
                         && selectedRef.GetMergeWith(localConfigFile) == head.LocalName);

                if (selectedHeadMergeSource is not null)
                {
                    selectedHeadMergeSource.IsSelectedHeadMergeSource = true;
                }
            }
        }

        private static bool TryParseRevision(in ArraySegment<byte> chunk, Func<string?, Encoding?> getEncodingByGitName, in Encoding logOutputEncoding, long sixMonths, [NotNullWhen(returnValue: true)] out GitRevision? revision)
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

            #region Encoding

            // Line is the name of the encoding used by git, or an empty string, terminated by `\n`
            string? encodingName;
            Encoding encoding;

            var encodingNameEndLength = array[offset..].IndexOf((byte)'\n');

            if (encodingNameEndLength == -1)
            {
                ParseAssert($"Log parse error, no encoding name for {objectId}");
                revision = default;
                return false;
            }

            if (encodingNameEndLength == 0)
            {
                // No encoding specified, this is the normal case since Git 1.8.4
                encoding = logOutputEncoding;
                encodingName = null;
            }
            else
            {
                encodingName = logOutputEncoding.GetString(array.Slice(offset, encodingNameEndLength));
                encoding = getEncodingByGitName(encodingName) ?? Encoding.UTF8;
            }

            offset += encodingNameEndLength + 1;

            #endregion

            #region Encoded string values (names, emails, subject, body)

            // Finally, decode the names, email, subject and body strings using the required text encoding
            ReadOnlySpan<char> s = encoding.GetString(array[offset..]).AsSpan();
            StringLineReader reader = new(in s);

            var author = reader.ReadLine();
            var authorEmail = reader.ReadLine();
            var committer = reader.ReadLine();
            var committerEmail = reader.ReadLine();

            bool skipBody = sixMonths > authorUnixTime;
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
                MessageEncoding = encodingName,
                Subject = subject,
                Body = body,
                HasMultiLineMessage = hasMultiLineMessage,
                HasNotes = false
            };

            return true;

            static void ParseAssert(string? message)
            {
                _noOfParseError++;
                Debug.Assert(_noOfParseError > 1, message);
                Trace.WriteLineIf(_noOfParseError < 10, message);
            }
        }

        public void Dispose()
        {
            _cancellationTokenSequence.Dispose();
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

            internal ArgumentBuilder BuildArgumentsBuildArguments(int maxCount, RefFilterOptions refFilterOptions,
                string branchFilter, string revisionFilter, string pathFilter) =>
                _revisionReader.BuildArguments(maxCount, refFilterOptions, branchFilter, revisionFilter, pathFilter);

            internal static bool TryParseRevision(ArraySegment<byte> chunk, Func<string?, Encoding?> getEncodingByGitName, Encoding logOutputEncoding, long sixMonths, [NotNullWhen(returnValue: true)] out GitRevision? revision) =>
                RevisionReader.TryParseRevision(chunk, getEncodingByGitName, logOutputEncoding, sixMonths, out revision);

            internal static int NoOfParseError
            {
                get { return _noOfParseError; }
                set { _noOfParseError = value; }
            }
        }
    }
}
