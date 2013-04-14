using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using GitCommands;

namespace PatchApply
{
    public class PatchProcessor
    {
        public Encoding FilesContentEncoding { get; private set; }

        public PatchProcessor(Encoding filesContentEncoding)
        {
            FilesContentEncoding = filesContentEncoding;
        }

        private enum PatchProcessorState
        {
            InHeader,
            InBody,
            OutsidePatch
        }

        /// <summary>
        /// Diff part of patch is printed verbatim, everything else (header, warnings, ...) is printed in git encoding (GitModule.SystemEncoding) 
        /// Since patch may contain diff for more than one file, it would be nice to obtaining encoding for each of file
        /// from .gitattributes, for now there is used one encoding, common for every file in repo (Settings.FilesEncoding)
        /// File path can be quoted see core.quotepath, it is unquoted by GitCommandHelpers.ReEncodeFileNameFromLossless
        /// </summary>
        /// <param name="lines">patch lines</param>
        /// <param name="lineIndex">start index</param>
        /// <returns></returns>
        public Patch CreatePatchFromString(string[] lines, ref int lineIndex)
        {
            if (lineIndex >= lines.Length)
                return null;

            string input = lines[lineIndex];
            if (!IsStartOfANewPatch(input))
                return null;

            PatchProcessorState state = PatchProcessorState.InHeader;
            Patch patch = new Patch();
            input = GitModule.ReEncodeFileNameFromLossless(input);
            patch.PatchHeader = input;
            patch.Type = Patch.PatchType.ChangeFile;
            ExtractPatchFilenames(patch);
            patch.AppendText(input);
            if (lineIndex < lines.Length - 1)
                patch.AppendText("\n");

            for (int i = lineIndex + 1; i < lines.Length; i++)
            {
                input = lines[i];

                if (IsStartOfANewPatch(input))
                {
                    lineIndex = i;
                    return patch;
                }

                if (i < lines.Length - 1)
                    input += "\n";

                if (state == PatchProcessorState.InBody)
                {
                    patch.AppendText(input);
                    continue;
                }

                if (IsChunkHeader(input))
                {
                    state = PatchProcessorState.InBody;
                    ValidateInput(ref input, patch, state);
                    patch.AppendText(input);
                    continue;
                }

                //header lines are encoded in GitModule.SystemEncoding
                input = GitModule.ReEncodeStringFromLossless(input, GitModule.SystemEncoding);
                if (IsIndexLine(input))
                {
                    patch.PatchIndex = input;
                    patch.AppendText(input);
                    continue;
                }

                if (SetPatchType(input, patch))
                {

                }
                else if (IsUnlistedBinaryFileDelete(input))
                {
                    if (patch.Type != Patch.PatchType.DeleteFile)
                        throw new FormatException("Change not parsed correct: " + input);

                    patch.File = Patch.FileType.Binary;
                    patch = null;
                    state = PatchProcessorState.OutsidePatch;
                }
                else if (IsUnlistedBinaryNewFile(input))
                {
                    if (patch.Type != Patch.PatchType.NewFile)
                        throw new FormatException("Change not parsed correct: " + input);

                    patch.File = Patch.FileType.Binary;
                    //TODO: NOT SUPPORTED!
                    patch.Apply = false;
                    patch = null;
                    state = PatchProcessorState.OutsidePatch;
                }
                else if (IsBinaryPatch(input))
                {
                    patch.File = Patch.FileType.Binary;

                    //TODO: NOT SUPPORTED!
                    patch.Apply = false;
                    patch = null;
                    state = PatchProcessorState.OutsidePatch;
                }
                ValidateInput(ref input, patch, state);
                patch.AppendText(input);
            }

            lineIndex = lines.Length;
            return patch;
        }

        public static Patch CreatePatchFromString(string patchText, Encoding filesContentEncoding)
        {
            var processor = new PatchProcessor(filesContentEncoding);
            string[] lines = patchText.Split('\n');
            int i = 0;
            Patch patch = processor.CreatePatchFromString(lines, ref i);
            return patch;
        }

        /// <summary>
        /// Diff part of patch is printed verbatim, everything else (header, warnings, ...) is printed in git encoding (GitModule.SystemEncoding) 
        /// Since patch may contain diff for more than one file, it would be nice to obtaining encoding for each of file
        /// from .gitattributes, for now there is used one encoding, common for every file in repo (Settings.FilesEncoding)
        /// File path can be quoted see core.quotepath, it is unquoted by GitCommandHelpers.ReEncodeFileNameFromLossless
        /// </summary>
        /// <param name="patchText"></param>
        /// <returns></returns>
        public IEnumerable<Patch> CreatePatchesFromString(String patchText)
        {
            var patches = new List<Patch>();
            PatchProcessorState state = PatchProcessorState.OutsidePatch;
            string[] lines = patchText.Split('\n');
            for(int i = 0; i < lines.Length; i++)
            {
                Patch patch = CreatePatchFromString(lines, ref i);
                if (patch != null)
                    yield return patch;
            }
        }

        private static bool IsIndexLine(string input)
        {
            return input.StartsWith("index ");
        }

        private void ValidateInput(ref string input, Patch patch, PatchProcessorState state)
        {
            if (state == PatchProcessorState.InHeader)
            {
                //--- /dev/null
                //means there is no old file, so this should be a new file
                if (IsOldFileMissing(input))
                {
                    if (patch.Type != Patch.PatchType.NewFile)
                        throw new FormatException("Change not parsed correct: " + input);
                }
                //line starts with --- means, old file name
                else if (input.StartsWith("--- "))
                {
                    input = GitModule.UnquoteFileName(input);
                    Match regexMatch = Regex.Match(input, "[-]{3}[ ][\\\"]{0,1}[aiwco12]/(.*)[\\\"]{0,1}");

                    if (!regexMatch.Success || patch.FileNameA != (regexMatch.Groups[1].Value.Trim()))
                        throw new FormatException("Old filename not parsed correct: " + input);
                }
                else if (IsNewFileMissing(input))
                {
                    if (patch.Type != Patch.PatchType.DeleteFile)
                        throw new FormatException("Change not parsed correct: " + input);
                }

                //line starts with +++ means, new file name
                //we expect a new file now!
                else if (input.StartsWith("+++ "))
                {
                    input = GitModule.UnquoteFileName(input);
                    Match regexMatch = Regex.Match(input, "[+]{3}[ ][\\\"]{0,1}[biwco12]/(.*)[\\\"]{0,1}");

                    if (!regexMatch.Success || patch.FileNameB != (regexMatch.Groups[1].Value.Trim()))
                        throw new FormatException("New filename not parsed correct: " + input);
                }
            }
            else
            {
                if (input.StartsWithAny(new string[] { " ", "-", "+", "@" }))
                    //diff content
                    input = GitModule.ReEncodeStringFromLossless(input, FilesContentEncoding);
                else
                    //warnings, messages ...
                    input = GitModule.ReEncodeStringFromLossless(input, GitModule.SystemEncoding);                    
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

        private static void ExtractPatchFilenames(Patch patch)
        {
            Match match = Regex.Match(patch.PatchHeader,
                                      "[ ][\\\"]{0,1}[aiwco12]/(.*)[\\\"]{0,1}[ ][\\\"]{0,1}[biwco12]/(.*)[\\\"]{0,1}");

            patch.FileNameA = match.Groups[1].Value.Trim();
            patch.FileNameB = match.Groups[2].Value.Trim();
        }

        private static bool IsStartOfANewPatch(string input)
        {
            return input.StartsWith("diff --git ");
        }

        private static bool SetPatchType(string input, Patch patch)
        {
            if (input.StartsWith("new file mode "))
                patch.Type = Patch.PatchType.NewFile;
            else if (input.StartsWith("deleted file mode "))
                patch.Type = Patch.PatchType.DeleteFile;
            else if (input.StartsWith("old mode "))
                patch.Type = Patch.PatchType.ChangeFileMode;
            else
                return false;

            return true;
        }
    }
}