using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using GitCommands;
using GitCommands.Settings;
using JetBrains.Annotations;

namespace PatchApply
{
    public static class PatchProcessor
    {
        private enum PatchProcessorState
        {
            InHeader,
            InBody,
            OutsidePatch
        }

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

            if (!IsStartOfANewPatch(header, out var isCombinedDiff))
            {
                return null;
            }

            header = GitModule.ReEncodeFileNameFromLossless(header);

            var state = PatchProcessorState.InHeader;

            string fileNameA, fileNameB;

            if (!isCombinedDiff)
            {
                // diff --git a/GitCommands/CommitInformationTest.cs b/GitCommands/CommitInformationTest.cs
                Match match = Regex.Match(header, " [\\\"]?[aiwco12]/(.*)[\\\"]? [\\\"]?[biwco12]/(.*)[\\\"]?");

                fileNameA = match.Groups[1].Value.Trim();
                fileNameB = match.Groups[2].Value.Trim();
            }
            else
            {
                Match match = Regex.Match(header, "--cc [\\\"]?(.*)[\\\"]?");

                fileNameA = match.Groups[1].Value.Trim();
                fileNameB = null;
            }

            var patch = new Patch();
            patch.PatchHeader = header;
            patch.Type = Patch.PatchType.ChangeFile;
            patch.IsCombinedDiff = isCombinedDiff;
            patch.FileNameA = fileNameA;
            patch.FileNameB = fileNameB;

            patch.AppendText(header);
            if (lineIndex < lines.Length - 1)
            {
                patch.AppendText("\n");
            }

            int i = lineIndex + 1;
            for (; i < lines.Length; i++)
            {
                var line = lines[i];

                if (IsStartOfANewPatch(line))
                {
                    lineIndex = i - 1;
                    return patch;
                }

                if (IsChunkHeader(line))
                {
                    state = PatchProcessorState.InBody;
                    break;
                }

                // header lines are encoded in GitModule.SystemEncoding
                line = GitModule.ReEncodeStringFromLossless(line, GitModule.SystemEncoding);
                if (IsIndexLine(line))
                {
                    patch.PatchIndex = line;
                    if (i < lines.Length - 1)
                    {
                        line += "\n";
                    }

                    patch.AppendText(line);
                    continue;
                }

                if (TryGetPatchType(line, out var patchType))
                {
                    patch.Type = patchType;
                }
                else if (IsUnlistedBinaryFileDelete(line))
                {
                    if (patch.Type != Patch.PatchType.DeleteFile)
                    {
                        throw new FormatException("Change not parsed correct: " + line);
                    }

                    patch.File = Patch.FileType.Binary;
                    state = PatchProcessorState.OutsidePatch;
                    break;
                }
                else if (IsUnlistedBinaryNewFile(line))
                {
                    if (patch.Type != Patch.PatchType.NewFile)
                    {
                        throw new FormatException("Change not parsed correct: " + line);
                    }

                    patch.File = Patch.FileType.Binary;
                    state = PatchProcessorState.OutsidePatch;
                    break;
                }
                else if (IsBinaryPatch(line))
                {
                    patch.File = Patch.FileType.Binary;
                    state = PatchProcessorState.OutsidePatch;
                    break;
                }

                ValidateHeader(ref line, patch);
                if (i < lines.Length - 1)
                {
                    line += "\n";
                }

                patch.AppendText(line);
            }

            // process patch body
            for (; i < lines.Length; i++)
            {
                var line = lines[i];

                if (IsStartOfANewPatch(line, out isCombinedDiff))
                {
                    lineIndex = i - 1;
                    return patch;
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

                patch.AppendText(line);
            }

            lineIndex = i - 1;
            return patch;
        }

        private static bool IsIndexLine([NotNull] string input)
        {
            return input.StartsWith("index ");
        }

        [ContractAnnotation("diff:null=>false")]
        public static bool IsCombinedDiff([CanBeNull] string diff)
        {
            // diff --combined describe.c
            // diff --cc describe.c
            return !string.IsNullOrWhiteSpace(diff) &&
                                 (diff.StartsWith("diff --cc") || diff.StartsWith("diff --combined"));
        }

        private static void ValidateHeader(ref string input, Patch patch)
        {
            // --- /dev/null
            // means there is no old file, so this should be a new file
            if (IsOldFileMissing(input))
            {
                if (patch.Type != Patch.PatchType.NewFile)
                {
                    throw new FormatException("Change not parsed correct: " + input);
                }
            }

            // line starts with --- means, old file name
            else if (input.StartsWith("--- "))
            {
                input = GitModule.UnquoteFileName(input);
                Match regexMatch = Regex.Match(input, "[-]{3} [\\\"]?[abiwco12]/(.*)[\\\"]?");

                if (regexMatch.Success)
                {
                    patch.FileNameA = regexMatch.Groups[1].Value.Trim();
                }
                else
                {
                    throw new FormatException("Old filename not parsed correct: " + input);
                }
            }
            else if (IsNewFileMissing(input))
            {
                if (patch.Type != Patch.PatchType.DeleteFile)
                {
                    throw new FormatException("Change not parsed correct: " + input);
                }
            }

            // line starts with +++ means, new file name
            // we expect a new file now!
            else if (input.StartsWith("+++ "))
            {
                input = GitModule.UnquoteFileName(input);
                Match regexMatch = Regex.Match(input, "[+]{3} [\\\"]?[abiwco12]/(.*)[\\\"]?");

                if (regexMatch.Success)
                {
                    patch.FileNameB = regexMatch.Groups[1].Value.Trim();
                }
                else
                {
                    throw new FormatException("New filename not parsed correct: " + input);
                }
            }
        }

        private static bool IsChunkHeader(string input)
        {
            return input.StartsWith("@@");
        }

        private static bool IsNewFileMissing(string input)
        {
            return input.StartsWith("+++ /dev/null");
        }

        private static bool IsOldFileMissing(string input)
        {
            return input.StartsWith("--- /dev/null");
        }

        private static bool IsBinaryPatch(string input)
        {
            return input.StartsWith("GIT binary patch");
        }

        private static bool IsUnlistedBinaryFileDelete(string input)
        {
            return input.StartsWith("Binary files a/") && input.EndsWith(" and /dev/null differ");
        }

        private static bool IsUnlistedBinaryNewFile(string input)
        {
            return input.StartsWith("Binary files /dev/null and b/") && input.EndsWith(" differ");
        }

        private static bool IsStartOfANewPatch([NotNull] string input, out bool isCombinedDiff)
        {
            isCombinedDiff = IsCombinedDiff(input);
            return input.StartsWith("diff --git ") || isCombinedDiff;
        }

        private static bool IsStartOfANewPatch(string input)
        {
            return IsStartOfANewPatch(input, out _);
        }

        private static bool TryGetPatchType(string input, out Patch.PatchType type)
        {
            if (input.StartsWith("new file mode "))
            {
                type = Patch.PatchType.NewFile;
            }
            else if (input.StartsWith("deleted file mode "))
            {
                type = Patch.PatchType.DeleteFile;
            }
            else if (input.StartsWith("old mode "))
            {
                type = Patch.PatchType.ChangeFileMode;
            }
            else
            {
                type = 0;
                return false;
            }

            return true;
        }
    }
}