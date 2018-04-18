using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using GitCommands.Settings;
using JetBrains.Annotations;

namespace GitCommands.Patches
{
    public static class PatchProcessor
    {
        private enum PatchProcessorState
        {
            InHeader,
            InBody,
            OutsidePatch
        }

        private static readonly Regex _patchHeaderRegex = new Regex("^diff --(?<type>git|cc|combined)\\s", RegexOptions.Compiled);

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
        [NotNull, ItemNotNull, Pure]
        public static IEnumerable<Patch> CreatePatchesFromString([NotNull] string patchText, [NotNull] Encoding filesContentEncoding)
        {
            // TODO encoding for each file in patch should be obtained separately from .gitattributes

            string[] lines = patchText.Split('\n');
            int i = 0;

            // skip email header
            for (; i < lines.Length; i++)
            {
                if (IsStartOfANewPatch(lines[i]))
                {
                    break;
                }
            }

            for (; i < lines.Length; i++)
            {
                Patch patch = CreatePatchFromString(lines, filesContentEncoding, ref i);
                if (patch != null)
                {
                    yield return patch;
                }
            }
        }

        [CanBeNull]
        private static Patch CreatePatchFromString([ItemNotNull, NotNull] string[] lines, [NotNull] Encoding filesContentEncoding, ref int lineIndex)
        {
            if (lineIndex >= lines.Length)
            {
                return null;
            }

            string header = lines[lineIndex];

            var headerMatch = _patchHeaderRegex.Match(header);

            if (!headerMatch.Success)
            {
                return null;
            }

            header = GitModule.ReEncodeFileNameFromLossless(header);

            var state = PatchProcessorState.InHeader;

            string fileNameA, fileNameB;

            var isCombinedDiff = headerMatch.Groups["type"].Value != "git";

            if (!isCombinedDiff)
            {
                // diff --git a/GitCommands/CommitInformationTest.cs b/GitCommands/CommitInformationTest.cs
                // diff --git b/Benchmarks/App.config a/Benchmarks/App.config
                Match match = Regex.Match(header, "^diff --git [\\\"]?[abiwco12]/(.*)[\\\"]? [\\\"]?[abiwco12]/(.*)[\\\"]?$");

                if (!match.Success)
                {
                    throw new FormatException("Invalid patch header: " + header);
                }

                fileNameA = match.Groups[1].Value.Trim();
                fileNameB = match.Groups[2].Value.Trim();
            }
            else
            {
                Match match = Regex.Match(header, "^diff --(cc|combined) [\\\"]?(?<filenamea>.*)[\\\"]?$");

                if (!match.Success)
                {
                    throw new FormatException("Invalid patch header: " + header);
                }

                fileNameA = match.Groups["filenamea"].Value.Trim();
                fileNameB = null;
            }

            string index = null;
            var changeType = PatchChangeType.ChangeFile;
            var fileType = PatchFileType.Text;
            var patchText = new StringBuilder();

            patchText.Append(header);
            if (lineIndex < lines.Length - 1)
            {
                patchText.Append("\n");
            }

            var done = false;
            var i = lineIndex + 1;

            for (; i < lines.Length; i++)
            {
                var line = lines[i];

                if (IsStartOfANewPatch(line))
                {
                    lineIndex = i - 1;
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
                    patchText.Append(line);
                    if (i < lines.Length - 1)
                    {
                        patchText.Append("\n");
                    }

                    continue;
                }

                if (line.StartsWith("new file mode "))
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
                        throw new FormatException("Change not parsed correctly: " + line);
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
                        throw new FormatException("Change not parsed correctly: " + line);
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
                        throw new FormatException("Change not parsed correctly: " + line);
                    }
                }
                else if (line.StartsWith("--- "))
                {
                    // old file name
                    line = GitModule.UnescapeOctalCodePoints(line);
                    Match regexMatch = Regex.Match(line, "[-]{3} [\\\"]?[abiwco12]/(.*)[\\\"]?");

                    if (regexMatch.Success)
                    {
                        fileNameA = regexMatch.Groups[1].Value.Trim();
                    }
                    else
                    {
                        throw new FormatException("Old filename not parsed correctly: " + line);
                    }
                }
                else if (line.StartsWith("+++ /dev/null"))
                {
                    // there is no new file, so this should be a deleted file
                    if (changeType != PatchChangeType.DeleteFile)
                    {
                        throw new FormatException("Change not parsed correctly: " + line);
                    }
                }
                else if (line.StartsWith("+++ "))
                {
                    // new file name
                    line = GitModule.UnescapeOctalCodePoints(line);
                    Match regexMatch = Regex.Match(line, "[+]{3} [\\\"]?[abiwco12]/(.*)[\\\"]?");

                    if (regexMatch.Success)
                    {
                        fileNameB = regexMatch.Groups[1].Value.Trim();
                    }
                    else
                    {
                        throw new FormatException("New filename not parsed correctly: " + line);
                    }
                }

                patchText.Append(line);

                if (i < lines.Length - 1)
                {
                    patchText.Append("\n");
                }
            }

            // process patch body
            for (; !done && i < lines.Length; i++)
            {
                var line = lines[i];

                if (IsStartOfANewPatch(line))
                {
                    lineIndex = i - 1;
                    break;
                }

                if (state == PatchProcessorState.InBody && line.StartsWithAny(new[] { " ", "-", "+", "@" }))
                {
                    // diff content
                    line = GitModule.ReEncodeStringFromLossless(line, filesContentEncoding);
                }
                else
                {
                    // warnings, messages ...
                    line = GitModule.ReEncodeStringFromLossless(line, GitModule.SystemEncoding);
                }

                if (i < lines.Length - 1)
                {
                    line += "\n";
                }

                patchText.Append(line);
            }

            lineIndex = i - 1;

            return new Patch(header, index, fileType, fileNameA, fileNameB, isCombinedDiff, changeType, patchText.ToString());
        }

        [ContractAnnotation("diff:null=>false")]
        public static bool IsCombinedDiff([CanBeNull] string diff)
        {
            // diff --combined describe.c
            // diff --cc describe.c
            return !string.IsNullOrWhiteSpace(diff) &&
                   (diff.StartsWith("diff --cc") || diff.StartsWith("diff --combined"));
        }

        [Pure]
        private static bool IsStartOfANewPatch([NotNull] string input)
        {
            return _patchHeaderRegex.IsMatch(input);
        }
    }
}