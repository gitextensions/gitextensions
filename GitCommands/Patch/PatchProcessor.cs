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

        private enum PatchProcessorState
        {
            InHeader,
            InBody,
            OutsidePatch
        }

        /// <summary>
        /// Diff part of patch is printed verbatim, everything else (header, warnings, ...) is printed in git encoding (Settings.SystemEncoding) 
        /// Since patch may contain diff for more than one file, it would be nice to obtaining encoding for each of file
        /// from .gitattributes, for now there is used one encoding, common for every file in repo (Settings.FilesEncoding)
        /// File path can be quoted see core.quotepath, it is unquoted by GitCommandHelpers.ReEncodeFileNameFromLossless
        /// </summary>
        /// <param name="textReader"></param>
        /// <returns></returns>
        public List<Patch> CreatePatchesFromString(String patchText)
        {
            var patches = new List<Patch>();
            Patch patch = null;
            bool validate;
            string input;
            PatchProcessorState state = PatchProcessorState.OutsidePatch;
            string[] lines = patchText.Split('\n');
            for(int i = 0; i < lines.Length; i++)
            {
                input = lines[i];
                validate = true;
                if (IsStartOfANewPatch(input))
                {
                    state = PatchProcessorState.InHeader;
                    validate = false;
                    patch = new Patch();
                    patches.Add(patch);
                    input = GitCommandHelpers.ReEncodeFileNameFromLossless(input);
                    patch.PatchHeader = input;
                    patch.Type = Patch.PatchType.ChangeFile;
                    ExtractPatchFilenames(patch);
                }
                else if (state == PatchProcessorState.InHeader)
                {
                    if (IsChunkHeader(input))
                        state = PatchProcessorState.InBody;
                    else
                    {
                        //header lines are encoded in Settings.SystemEncoding
                        input = GitCommandHelpers.ReEncodeStringFromLossless(input, Settings.SystemEncoding);
                        if (IsIndexLine(input))
                        {                            
                            validate = false;
                            patch.PatchIndex = input;
                        }
                        else
                        {
                            if (SetPatchType(input, patch))
                            { }
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
                        }
                    }
                }

                if (state != PatchProcessorState.OutsidePatch)
                {
                    if (validate)
                        ValidateInput(ref input, patch, state);
                    patch.AppendText(input);
                    if (i < lines.Length - 1)
                        patch.AppendText("\n");
                }
            }

            return patches;
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
                    input = GitCommandHelpers.UnquoteFileName(input);
                    Match regexMatch = Regex.Match(input, "[-]{3}[ ][\\\"]{0,1}[a]/(.*)[\\\"]{0,1}");

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
                    input = GitCommandHelpers.UnquoteFileName(input);
                    Match regexMatch = Regex.Match(input, "[+]{3}[ ][\\\"]{0,1}[b]/(.*)[\\\"]{0,1}");

                    if (!regexMatch.Success || patch.FileNameB != (regexMatch.Groups[1].Value.Trim()))
                        throw new FormatException("New filename not parsed correct: " + input);
                }
            }
            else
            {
                if (input.StartsWithAny(new string[] { " ", "-", "+", "@" }))
                    //diff content
                    input = GitCommandHelpers.ReEncodeStringFromLossless(input, FilesContentEncoding);
                else
                    //warnings, messages ...
                    input = GitCommandHelpers.ReEncodeStringFromLossless(input, Settings.SystemEncoding);                    
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
                                      "[ ][\\\"]{0,1}[a]/(.*)[\\\"]{0,1}[ ][\\\"]{0,1}[b]/(.*)[\\\"]{0,1}");

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