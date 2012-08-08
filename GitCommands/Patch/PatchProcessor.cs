using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
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

        /// <summary>
        /// Diff part of patch is printed verbatim, everything else (header, warnings, ...) is printed in git encoding (Settings.SystemEncoding) 
        /// Since patch may contain diff for more than one file, it would be nice to obtaining encoding for each of file
        /// from .gitattributes, for now there is used one encoding, common for every file in repo (Settings.FilesEncoding)
        /// File path can be quoted see core.quotepath, it is unquoted by GitCommandHelpers.ReEncodeFileNameFromLossless
        /// </summary>
        /// <param name="textReader"></param>
        /// <returns></returns>
        public List<Patch> CreatePatchesFromReader(TextReader textReader)
        {
            var patches = new List<Patch>();
            Patch patch = null;
            bool gitPatch = false;
            string input;
            while ((input = textReader.ReadLine()) != null)
            {
                if (IsStartOfANewPatch(input))
                {
                    gitPatch = true;
                    patch = new Patch();
                    input = GitCommandHelpers.ReEncodeFileNameFromLossless(input);
                    patch.PatchHeader = input;
                    patch.AppendTextLine(patch.PatchHeader);
                    patches.Add(patch);
                    ExtractPatchFilenames(input, patch);
                    if ((input = textReader.ReadLine()) != null)
                    {
                        if (IsStartOfANewPatch(input))
                        {
                            continue;
                        }

                        SetPatchType(input, patch);

                        if (patch.Type == Patch.PatchType.ChangeFileMode)
                        {
                            patch.AppendTextLine(input);
                        }
                        else if (!IsIndexLine(input))
                        {
                            if (textReader.ReadLine() == null)
                            {
                                break;
                            }
                        }
                        else
                        {
                            patch.PatchIndex = input;
                            patch.AppendTextLine(patch.PatchIndex);
                        }
                    }

                    if ((input = textReader.ReadLine()) != null)
                    {
                        input = GitCommandHelpers.ReEncodeFileNameFromLossless(input);
                        if (IsUnlistedBinaryFileDelete(input))
                        {
                            patch.File = Patch.FileType.Binary;

                            if (patch.Type != Patch.PatchType.DeleteFile)
                                throw new FormatException("Change not parsed correct: " + input);

                            patch = null;
                            continue;
                        }

                        if (IsUnlistedBinaryNewFile(input))
                        {
                            patch.File = Patch.FileType.Binary;

                            if (patch.Type != Patch.PatchType.NewFile)
                                throw new FormatException("Change not parsed correct: " + input);

                            //TODO: NOT SUPPORTED!
                            patch.Apply = false;

                            patch = null;

                            continue;
                        }

                        if (IsBinaryPatch(input))
                        {
                            patch.File = Patch.FileType.Binary;

                            //TODO: NOT SUPPORTED!
                            patch.Apply = false;

                            patch = null;

                            continue;
                        }
                    }
                }

                if (!gitPatch || patch != null)
                {
                    ValidateInput(ref input, patch, gitPatch);
                }

                if (patch != null)
                    patch.AppendTextLine(input);
            }

            return patches;
        }

        private static bool IsIndexLine(string input)
        {
            return input.StartsWith("index ");
        }

        private void ValidateInput(ref string input, Patch patch, bool gitPatch)
        {
            //The previous check checked only if the file was binary
            //--- /dev/null
            //means there is no old file, so this should be a new file
            if (IsOldFileMissing(input))
            {
                if (gitPatch && patch.Type != Patch.PatchType.NewFile)
                    throw new FormatException("Change not parsed correct: " + input);
            }
            //line starts with --- means, old file name
            else if (HasOldFileName(input))
            {
                input = GitCommandHelpers.ReEncodeFileNameFromLossless(input);
                if (gitPatch && patch.FileNameA != (input.Substring(6).Trim()))
                    throw new FormatException("Old filename not parsed correct: " + input);
            }
            else if (IsNewFileMissing(input))
            {
                if (gitPatch && patch.Type != Patch.PatchType.DeleteFile)
                    throw new FormatException("Change not parsed correct: " + input);
            }

            //line starts with +++ means, new file name
            //we expect a new file now!
            else if (input.StartsWith("+++ ") && !IsNewFileMissing(input))
            {
                input = GitCommandHelpers.ReEncodeFileNameFromLossless(input);
                Match regexMatch = Regex.Match(input, "[+]{3}[ ][\\\"]{0,1}[b]/(.*)[\\\"]{0,1}");

                if (gitPatch && patch.FileNameB != (regexMatch.Groups[1].Value.Trim()))
                    throw new FormatException("New filename not parsed correct: " + input);
            }
            else if (input.StartsWithAny(new string[] { " ", "-", "+", "@" }))
                input = GitCommandHelpers.ReEncodeStringFromLossless(input, FilesContentEncoding);
            else
                input = GitCommandHelpers.ReEncodeStringFromLossless(input, Settings.SystemEncoding);
        }

        private static bool IsNewFileMissing(string input)
        {
            return input.StartsWith("+++ /dev/null");
        }

        private static bool HasOldFileName(string input)
        {
            return input.StartsWith("--- a/");
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

        private static void ExtractPatchFilenames(string input, Patch patch)
        {
            Match match = Regex.Match(input,
                                      "[ ][\\\"]{0,1}[a]/(.*)[\\\"]{0,1}[ ][\\\"]{0,1}[b]/(.*)[\\\"]{0,1}");

            patch.FileNameA = match.Groups[1].Value;
            patch.FileNameB = match.Groups[2].Value;
        }

        private static bool IsStartOfANewPatch(string input)
        {
            return input.StartsWith("diff --git ");
        }

        private static void SetPatchType(string input, Patch patch)
        {
            if (input.StartsWith("new file mode "))
                patch.Type = Patch.PatchType.NewFile;
            else if (input.StartsWith("deleted file mode "))
                patch.Type = Patch.PatchType.DeleteFile;
            else if (input.StartsWith("old mode "))
                patch.Type = Patch.PatchType.ChangeFileMode;
            else
                patch.Type = Patch.PatchType.ChangeFile;
        }
    }
}