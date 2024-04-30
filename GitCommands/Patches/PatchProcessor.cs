using System.Diagnostics.Contracts;
using System.Text;
using System.Text.RegularExpressions;
using GitCommands.Settings;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;

namespace GitCommands.Patches
{
    public static partial class PatchProcessor
    {
        private enum PatchProcessorState
        {
            InHeader,
            InBody,
            OutsidePatch
        }

        // With Git coloring, Git adds escape sequences at the start and end of the line.
        private const string _escapeSequenceRegex = @"\u001b\[[^m]*m";
        [GeneratedRegex(@$"^({_escapeSequenceRegex})?(?<line>.*?)({_escapeSequenceRegex})?\s*$", RegexOptions.ExplicitCapture)]
        private static partial Regex StripWrappingEscapesSequenceRegex();

#if DEBUG
        // Check whether Git starts to emit escape sequences inside the patch header.
        [GeneratedRegex(@$"{_escapeSequenceRegex}", RegexOptions.ExplicitCapture)]
        private static partial Regex CheckAnyEscapeSequenceRegex();
#endif

        // diff --git a/GitCommands/CommitInformationTest.cs b/GitCommands/CommitInformationTest.cs
        // diff --git b/Benchmarks/App.config a/Benchmarks/App.config
        // diff --cc config-enumerator
        [GeneratedRegex(@$"^diff --(?<type>git|cc|combined)\s[""]?([^/\s]+/)?(?<filenamea>.*?)[""]?( [""]?[^/\s]+/(?<filenameb>.*?)[""]?)?\s*$", RegexOptions.ExplicitCapture)]
        private static partial Regex PatchHeaderFileNameRegex();

        [GeneratedRegex(@$"^({_escapeSequenceRegex})?diff --(?<type>git|cc|combined)\s", RegexOptions.ExplicitCapture)]
        private static partial Regex PatchHeaderRegex();

        [GeneratedRegex(@$"(---|\+\+\+) [""]?[^/\s]+/(?<filename>.*)[""]?", RegexOptions.ExplicitCapture)]
        private static partial Regex FileNameRegex();

        [GeneratedRegex(@$"^({_escapeSequenceRegex})?[ -+@]", RegexOptions.ExplicitCapture)]
        private static partial Regex StartOfContentsRegex();

        /// <summary>
        /// Parses a patch file into individual <see cref="Patch"/> objects.
        /// </summary>
        /// <remarks>
        /// The diff part of a patch is printed verbatim.
        /// <para />
        /// Everything else (header, warnings, ...) is printed in git encoding (<see cref="GitModule.SystemEncoding"/>).
        /// <para />
        /// Since a patch may contain the diff of more than one file, it would be nice to obtain the encoding for each file
        /// from <c>.gitattributes</c>. For now, one encoding is used for every file in the repo (<see cref="ConfigFileSettings.FilesEncoding"/>).
        /// <para />
        /// File paths can be quoted (see <c>core.quotepath</c>). They are unquoted by <see cref="GitModule.ReEncodeFileNameFromLossless"/>.
        /// </remarks>
        [Pure]
        public static IEnumerable<Patch> CreatePatchesFromString(string patchText, Lazy<Encoding> filesContentEncoding)
        {
            // TODO encoding for each file in patch should be obtained separately from .gitattributes

            string[] lines = patchText.Split(Delimiters.LineFeed);
            int i = 0;

            // skip email header
            for (; i < lines.Length; i++)
            {
                if (PatchHeaderRegex().IsMatch(lines[i]))
                {
                    break;
                }
            }

            for (; i < lines.Length; i++)
            {
                Patch? patch = CreatePatchFromString(lines, filesContentEncoding, ref i);
                if (patch is not null)
                {
                    yield return patch;
                }
            }
        }

        private static Patch? CreatePatchFromString(string[] lines, Lazy<Encoding> filesContentEncoding, ref int lineIndex)
        {
            if (lineIndex >= lines.Length)
            {
                return null;
            }

            string rawHeader = lines[lineIndex];
            Match contentMatch = StripWrappingEscapesSequenceRegex().Match(rawHeader);
            string header = contentMatch.Groups["line"].Value;
#if DEBUG
            DebugHelpers.Assert(!CheckAnyEscapeSequenceRegex().IsMatch(header), "Git unexpectedly emits escape sequences in first header other than at start/end. line {i}:{rawHeader}");
#endif
            Match headerMatch = PatchHeaderFileNameRegex().Match(header);
            if (!headerMatch.Success)
            {
                return null;
            }

            header = GitModule.ReEncodeFileNameFromLossless(header);
            bool isCombinedDiff = headerMatch.Groups["type"].Value != "git";
            if (!headerMatch.Success || (!isCombinedDiff && !headerMatch.Groups["filenameb"].Success))
            {
                throw new FormatException($"Invalid patch header: {header}");
            }

            string fileNameA = headerMatch.Groups["filenamea"].Value.Trim();
            string? fileNameB = isCombinedDiff ? null : headerMatch.Groups["filenameb"].Value.Trim();

            StringBuilder patchText = new();

            patchText.Append(ReaddEscapes(rawHeader, header, contentMatch));
            if (lineIndex < lines.Length - 1)
            {
                patchText.Append('\n');
            }

            PatchProcessorState state = PatchProcessorState.InHeader;
            PatchChangeType changeType = PatchChangeType.ChangeFile;
            PatchFileType fileType = PatchFileType.Text;
            string? index = null;

            bool done = false;
            int i = lineIndex + 1;

            // Process patch header
            for (; i < lines.Length; i++)
            {
                string rawLine = lines[i];
                Match lineMatch = StripWrappingEscapesSequenceRegex().Match(rawLine);
                string line = lineMatch.Groups["line"].Value;
#if DEBUG
                DebugHelpers.Assert(line.StartsWith("@@") || !CheckAnyEscapeSequenceRegex().IsMatch(line),
                    $"Git unexpectedly emits escape sequences in header other than at start/end. line {i}:{rawLine}");
#endif

                if (PatchHeaderRegex().IsMatch(line))
                {
                    done = true;
                    break;
                }

                if (line.StartsWith("@@"))
                {
                    // Starting a new chunk
                    state = PatchProcessorState.InBody;
                    break;
                }

                // header lines are encoded in GitModule.SystemEncoding
                line = GitModule.ReEncodeStringFromLossless(line, GitModule.SystemEncoding);

                if (line.StartsWith("index "))
                {
                    // Index line
                    index = line;
                }
                else if (line.StartsWith("new file mode "))
                {
                    changeType = PatchChangeType.NewFile;
                }
                else if (line.StartsWith("deleted file mode "))
                {
                    changeType = PatchChangeType.DeleteFile;
                }
                else if (line.StartsWith("old mode "))
                {
                    changeType = PatchChangeType.ChangeFileMode;
                }
                else if (line.StartsWith("Binary files a/") && line.EndsWith(" and /dev/null differ"))
                {
                    // Unlisted binary file deletion
                    if (changeType != PatchChangeType.DeleteFile)
                    {
                        throw new FormatException($"Invalid patch header: {line}");
                    }

                    fileType = PatchFileType.Binary;
                    state = PatchProcessorState.OutsidePatch;
                    break;
                }
                else if (line.StartsWith("Binary files /dev/null and b/") && line.EndsWith(" differ"))
                {
                    // Unlisted binary file addition
                    if (changeType != PatchChangeType.NewFile)
                    {
                        throw new FormatException($"Invalid patch header: {line}");
                    }

                    fileType = PatchFileType.Binary;
                    state = PatchProcessorState.OutsidePatch;
                    break;
                }
                else if (line.StartsWith("GIT binary patch"))
                {
                    fileType = PatchFileType.Binary;
                    state = PatchProcessorState.OutsidePatch;
                    break;
                }

                if (line.StartsWith("--- /dev/null"))
                {
                    // there is no old file, so this should be a new file
                    if (changeType != PatchChangeType.NewFile)
                    {
                        throw new FormatException($"Invalid patch header: {line}");
                    }
                }
                else if (line.StartsWith("--- "))
                {
                    // old file name
                    line = GitModule.UnescapeOctalCodePoints(line);
                    Match regexMatch = FileNameRegex().Match(line);

                    if (regexMatch.Success)
                    {
                        fileNameA = regexMatch.Groups["filename"].Value.Trim();
                    }
                    else
                    {
                        throw new FormatException($"Invalid patch header: {line}");
                    }
                }
                else if (line.StartsWith("+++ /dev/null"))
                {
                    // there is no new file, so this should be a deleted file
                    if (changeType != PatchChangeType.DeleteFile)
                    {
                        throw new FormatException($"Invalid patch header: {line}");
                    }
                }
                else if (line.StartsWith("+++ "))
                {
                    // new file name
                    line = GitModule.UnescapeOctalCodePoints(line);
                    Match regexMatch = FileNameRegex().Match(line);

                    if (regexMatch.Success)
                    {
                        fileNameB = regexMatch.Groups["filename"].Value.Trim();
                    }
                    else
                    {
                        throw new FormatException($"Invalid patch header: {line}");
                    }
                }

                patchText.Append(ReaddEscapes(rawLine, line, lineMatch));
                if (i < lines.Length - 1)
                {
                    patchText.Append('\n');
                }
            }

            // process patch body
            for (; !done && i < lines.Length; i++)
            {
                string line = lines[i];
                if (PatchHeaderRegex().IsMatch(line))
                {
                    break;
                }

                Encoding encoding = (state == PatchProcessorState.InBody && StartOfContentsRegex().IsMatch(line))
                    ? filesContentEncoding.Value // diff content
                    : GitModule.SystemEncoding; // warnings, messages ...

                line = GitModule.ReEncodeStringFromLossless(line, encoding);
                patchText.Append(line);
                if (i < lines.Length - 1)
                {
                    patchText.Append('\n');
                }
            }

            lineIndex = i - 1;

            return new Patch(header, index, fileType, fileNameA, fileNameB, changeType, patchText.ToString());

            // Add the escape sequences back to the header
            static string ReaddEscapes(string rawLine, string line, Match lineMatch)
                => $"{rawLine[..lineMatch.Index]}{line}{rawLine[(lineMatch.Index + lineMatch.Length)..]}";
        }
    }
}
