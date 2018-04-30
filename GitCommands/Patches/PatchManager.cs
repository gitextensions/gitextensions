using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using GitCommands.Settings;
using JetBrains.Annotations;

namespace GitCommands.Patches
{
    public static class PatchManager
    {
        [CanBeNull]
        public static byte[] GetResetUnstagedLinesAsPatch([NotNull] GitModule module, [NotNull] string text, int selectionPosition, int selectionLength, [NotNull] Encoding fileContentEncoding)
        {
            var selectedChunks = GetSelectedChunks(text, selectionPosition, selectionLength, out var header);

            if (selectedChunks == null)
            {
                return null;
            }

            string body = ToResetUnstagedLinesPatch(selectedChunks);

            // git apply has problem with dealing with autocrlf
            // I noticed that patch applies when '\r' chars are removed from patch if autocrlf is set to true
            if (body != null && module.EffectiveConfigFile.core.autocrlf.ValueOrDefault == AutoCRLFType.@true)
            {
                body = body.Replace("\r", "");
            }

            if (header == null || body == null)
            {
                return null;
            }
            else
            {
                return GetPatchBytes(header, body, fileContentEncoding);
            }
        }

        [CanBeNull]
        public static byte[] GetSelectedLinesAsPatch([NotNull] string text, int selectionPosition, int selectionLength, bool staged, [NotNull] Encoding fileContentEncoding, bool isNewFile)
        {
            var selectedChunks = GetSelectedChunks(text, selectionPosition, selectionLength, out var header);

            if (selectedChunks == null)
            {
                return null;
            }

            // if file is new, --- /dev/null has to be replaced by --- a/fileName
            if (isNewFile)
            {
                header = CorrectHeaderForNewFile(header);
            }

            string body = ToStagePatch(selectedChunks, staged, isWholeFile: false);

            if (header == null || body == null)
            {
                return null;
            }

            return GetPatchBytes(header, body, fileContentEncoding);
        }

        [NotNull]
        private static string CorrectHeaderForNewFile([NotNull] string header)
        {
            string[] headerLines = header.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            string pppLine = null;
            foreach (string line in headerLines)
            {
                if (line.StartsWith("+++"))
                {
                    pppLine = "---" + line.Substring(3);
                }
            }

            StringBuilder sb = new StringBuilder();

            foreach (string line in headerLines)
            {
                if (line.StartsWith("---"))
                {
                    sb.Append(pppLine + "\n");
                }
                else if (!line.StartsWith("new file mode"))
                {
                    sb.Append(line + "\n");
                }
            }

            return sb.ToString();
        }

        [CanBeNull]
        public static byte[] GetSelectedLinesAsNewPatch([NotNull] GitModule module, [NotNull] string newFileName, [NotNull] string text, int selectionPosition, int selectionLength, [NotNull] Encoding fileContentEncoding, bool reset, byte[] filePreabmle)
        {
            StringBuilder sb = new StringBuilder();
            const string fileMode = "100000"; // given fake mode to satisfy patch format, git will override this
            sb.Append(string.Format("diff --git a/{0} b/{0}", newFileName));
            sb.Append("\n");
            if (!reset)
            {
                sb.Append("new file mode " + fileMode);
                sb.Append("\n");
            }

            sb.Append("index 0000000..0000000");
            sb.Append("\n");
            if (reset)
            {
                sb.Append("--- a/" + newFileName);
            }
            else
            {
                sb.Append("--- /dev/null");
            }

            sb.Append("\n");
            sb.Append("+++ b/" + newFileName);
            sb.Append("\n");

            string header = sb.ToString();

            var selectedChunks = FromNewFile(module, text, selectionPosition, selectionLength, reset, filePreabmle, fileContentEncoding);

            string body = ToStagePatch(selectedChunks, staged: false, isWholeFile: true);

            // git apply has problem with dealing with autocrlf
            // I noticed that patch applies when '\r' chars are removed from patch if autocrlf is set to true
            if (reset && body != null && module.EffectiveConfigFile.core.autocrlf.ValueOrDefault == AutoCRLFType.@true)
            {
                body = body.Replace("\r", "");
            }

            if (header == null || body == null)
            {
                return null;
            }
            else
            {
                return GetPatchBytes(header, body, fileContentEncoding);
            }
        }

        [NotNull]
        private static byte[] GetPatchBytes([NotNull] string header, [NotNull] string body, [NotNull] Encoding fileContentEncoding)
        {
            byte[] hb = EncodingHelper.ConvertTo(GitModule.SystemEncoding, header);
            byte[] bb = EncodingHelper.ConvertTo(fileContentEncoding, body);
            byte[] result = new byte[hb.Length + bb.Length];
            hb.CopyTo(result, 0);
            bb.CopyTo(result, hb.Length);
            return result;
        }

        [CanBeNull, ItemNotNull]
        private static IReadOnlyList<Chunk> GetSelectedChunks([NotNull] string text, int selectionPosition, int selectionLength, [CanBeNull] out string header)
        {
            header = null;

            // When there is no patch, return nothing
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            // TODO: handling submodules
            // Divide diff into header and patch
            int patchPos = text.IndexOf("@@", StringComparison.Ordinal);
            if (patchPos < 1)
            {
                return null;
            }

            header = text.Substring(0, patchPos);
            string diff = text.Substring(patchPos - 1);

            string[] chunks = diff.Split(new[] { "\n@@" }, StringSplitOptions.RemoveEmptyEntries);
            var selectedChunks = new List<Chunk>();
            int i = 0;
            int currentPos = patchPos - 1;

            while (i < chunks.Length && currentPos <= selectionPosition + selectionLength)
            {
                string chunkStr = chunks[i];
                currentPos += 3;

                // if selection intersects with chunsk
                if (currentPos + chunkStr.Length >= selectionPosition)
                {
                    Chunk chunk = Chunk.ParseChunk(chunkStr, currentPos, selectionPosition, selectionLength);
                    if (chunk != null)
                    {
                        selectedChunks.Add(chunk);
                    }
                }

                currentPos += chunkStr.Length;
                i++;
            }

            return selectedChunks;
        }

        [NotNull]
        private static IReadOnlyList<Chunk> FromNewFile([NotNull] GitModule module, [NotNull] string text, int selectionPosition, int selectionLength, bool reset, [NotNull] byte[] filePreabmle, [NotNull] Encoding fileContentEncoding)
        {
            return new List<Chunk>
            {
                Chunk.FromNewFile(module, text, selectionPosition, selectionLength, reset, filePreabmle, fileContentEncoding)
            };
        }

        [CanBeNull]
        private static string ToResetUnstagedLinesPatch([NotNull, ItemNotNull] IEnumerable<Chunk> chunks)
        {
            string SubChunkToPatch(SubChunk subChunk, ref int addedCount, ref int removedCount, ref bool wereSelectedLines)
            {
                return subChunk.ToResetUnstagedLinesPatch(ref addedCount, ref removedCount, ref wereSelectedLines);
            }

            return ToPatch(chunks, SubChunkToPatch);
        }

        [CanBeNull]
        private static string ToStagePatch([NotNull, ItemNotNull] IEnumerable<Chunk> chunks, bool staged, bool isWholeFile)
        {
            string SubChunkToPatch(SubChunk subChunk, ref int addedCount, ref int removedCount, ref bool wereSelectedLines)
            {
                return subChunk.ToStagePatch(ref addedCount, ref removedCount, ref wereSelectedLines, staged, isWholeFile);
            }

            return ToPatch(chunks, SubChunkToPatch);
        }

        [CanBeNull]
        private static string ToPatch([NotNull, ItemNotNull] IEnumerable<Chunk> chunks, [NotNull, InstantHandle] SubChunkToPatchFnc subChunkToPatch)
        {
            var result = new StringBuilder();

            foreach (Chunk chunk in chunks)
            {
                if (result.Length != 0)
                {
                    result.Append('\n');
                }

                chunk.ToPatch(subChunkToPatch, result);
            }

            if (result.Length == 0)
            {
                return null;
            }

            result.Append($"\n--\n{Application.ProductName} {AppSettings.ProductVersion}");

            return result.ToString();
        }
    }

    internal sealed class PatchLine
    {
        public string Text { get; private set; }
        public bool Selected { get; set; }

        public PatchLine(string text, bool selected = false)
        {
            Text = text;
            Selected = selected;
        }

        public PatchLine Clone()
        {
            return new PatchLine(Text, Selected);
        }

        public void SetOperation(string operationMark)
        {
            Text = operationMark + Text.Substring(1);
        }
    }

    internal sealed class SubChunk
    {
        public List<PatchLine> PreContext { get; } = new List<PatchLine>();
        public List<PatchLine> RemovedLines { get; } = new List<PatchLine>();
        public List<PatchLine> AddedLines { get; } = new List<PatchLine>();
        public List<PatchLine> PostContext { get; } = new List<PatchLine>();
        public string WasNoNewLineAtTheEnd { get; set; }
        public string IsNoNewLineAtTheEnd { get; set; }

        public string ToStagePatch(ref int addedCount, ref int removedCount, ref bool wereSelectedLines, bool staged, bool isWholeFile)
        {
            string diff = null;
            string removePart = null;
            string addPart = null;
            string prePart = null;
            string postPart = null;
            bool inPostPart = false;
            bool selectedLastLine = false;
            addedCount += PreContext.Count + PostContext.Count;
            removedCount += PreContext.Count + PostContext.Count;

            foreach (PatchLine line in PreContext)
            {
                diff = diff.Combine("\n", line.Text);
            }

            for (int i = 0; i < RemovedLines.Count; i++)
            {
                PatchLine removedLine = RemovedLines[i];
                selectedLastLine = removedLine.Selected;
                if (removedLine.Selected)
                {
                    wereSelectedLines = true;
                    inPostPart = true;
                    removePart = removePart.Combine("\n", removedLine.Text);
                    removedCount++;
                }
                else if (!staged)
                {
                    if (inPostPart)
                    {
                        removePart = removePart.Combine("\n", " " + removedLine.Text.Substring(1));
                    }
                    else
                    {
                        prePart = prePart.Combine("\n", " " + removedLine.Text.Substring(1));
                    }

                    addedCount++;
                    removedCount++;
                }
            }

            bool selectedLastRemovedLine = selectedLastLine;

            for (int i = 0; i < AddedLines.Count; i++)
            {
                PatchLine addedLine = AddedLines[i];
                selectedLastLine = addedLine.Selected;
                if (addedLine.Selected)
                {
                    wereSelectedLines = true;
                    inPostPart = true;
                    addPart = addPart.Combine("\n", addedLine.Text);
                    addedCount++;
                }
                else if (staged)
                {
                    if (inPostPart)
                    {
                        postPart = postPart.Combine("\n", " " + addedLine.Text.Substring(1));
                    }
                    else
                    {
                        prePart = prePart.Combine("\n", " " + addedLine.Text.Substring(1));
                    }

                    addedCount++;
                    removedCount++;
                }
            }

            diff = diff.Combine("\n", prePart);
            diff = diff.Combine("\n", removePart);
            if (PostContext.Count == 0 && (!staged || selectedLastRemovedLine))
            {
                diff = diff.Combine("\n", WasNoNewLineAtTheEnd);
            }

            diff = diff.Combine("\n", addPart);
            diff = diff.Combine("\n", postPart);
            foreach (PatchLine line in PostContext)
            {
                diff = diff.Combine("\n", line.Text);
            }

            // stage no new line at the end only if last +- line is selected
            if (PostContext.Count == 0 && (selectedLastLine || staged || isWholeFile))
            {
                diff = diff.Combine("\n", IsNoNewLineAtTheEnd);
            }

            if (PostContext.Count > 0)
            {
                diff = diff.Combine("\n", WasNoNewLineAtTheEnd);
            }

            return diff;
        }

        // patch base is changed file
        public string ToResetUnstagedLinesPatch(ref int addedCount, ref int removedCount, ref bool wereSelectedLines)
        {
            string diff = null;
            string removePart = null;
            string addPart = null;
            string prePart = null;
            string postPart = null;
            bool inPostPart = false;
            addedCount += PreContext.Count + PostContext.Count;
            removedCount += PreContext.Count + PostContext.Count;

            foreach (PatchLine line in PreContext)
            {
                diff = diff.Combine("\n", line.Text);
            }

            foreach (PatchLine removedLine in RemovedLines)
            {
                if (removedLine.Selected)
                {
                    wereSelectedLines = true;
                    inPostPart = true;
                    addPart = addPart.Combine("\n", "+" + removedLine.Text.Substring(1));
                    addedCount++;
                }
            }

            foreach (PatchLine addedLine in AddedLines)
            {
                if (addedLine.Selected)
                {
                    wereSelectedLines = true;
                    inPostPart = true;
                    removePart = removePart.Combine("\n", "-" + addedLine.Text.Substring(1));
                    removedCount++;
                }
                else
                {
                    if (inPostPart)
                    {
                        postPart = postPart.Combine("\n", " " + addedLine.Text.Substring(1));
                    }
                    else
                    {
                        prePart = prePart.Combine("\n", " " + addedLine.Text.Substring(1));
                    }

                    addedCount++;
                    removedCount++;
                }
            }

            diff = diff.Combine("\n", prePart);
            diff = diff.Combine("\n", removePart);
            if (PostContext.Count == 0)
            {
                diff = diff.Combine("\n", WasNoNewLineAtTheEnd);
            }

            diff = diff.Combine("\n", addPart);
            diff = diff.Combine("\n", postPart);
            foreach (PatchLine line in PostContext)
            {
                diff = diff.Combine("\n", line.Text);
            }

            if (PostContext.Count == 0)
            {
                diff = diff.Combine("\n", IsNoNewLineAtTheEnd);
            }
            else
            {
                diff = diff.Combine("\n", WasNoNewLineAtTheEnd);
            }

            return diff;
        }
    }

    internal delegate string SubChunkToPatchFnc(SubChunk subChunk, ref int addedCount, ref int removedCount, ref bool wereSelectedLines);

    internal sealed class Chunk
    {
        private int _startLine;
        private readonly List<SubChunk> _subChunks = new List<SubChunk>();
        private SubChunk _currentSubChunk;

        [NotNull]
        private SubChunk CurrentSubChunk
        {
            get
            {
                if (_currentSubChunk == null)
                {
                    _currentSubChunk = new SubChunk();
                    _subChunks.Add(_currentSubChunk);
                }

                return _currentSubChunk;
            }
        }

        private void AddContextLine([NotNull] PatchLine line, bool preContext)
        {
            if (preContext)
            {
                CurrentSubChunk.PreContext.Add(line);
            }
            else
            {
                CurrentSubChunk.PostContext.Add(line);
            }
        }

        private void AddDiffLine([NotNull] PatchLine line, bool removed)
        {
            // if postContext is not empty @line comes from next SubChunk
            if (CurrentSubChunk.PostContext.Count > 0)
            {
                _currentSubChunk = null; // start new SubChunk
            }

            if (removed)
            {
                CurrentSubChunk.RemovedLines.Add(line);
            }
            else
            {
                CurrentSubChunk.AddedLines.Add(line);
            }
        }

        /// <summary>
        /// Parses a header line, setting the start index.
        /// </summary>
        /// <remarks>
        /// An example header line is:
        /// <code>
        ///  -116,12 +117,15 @@ private string LoadFile(string fileName, Encoding filesContentEncoding)
        /// </code>
        /// In which case the start line is <c>116</c>.
        /// </remarks>
        private void ParseHeader([NotNull] string header)
        {
            var match = Regex.Match(header, @".*-(\d+),");

            if (match.Success)
            {
                _startLine = int.Parse(match.Groups[1].Value);
            }
        }

        [CanBeNull]
        public static Chunk ParseChunk(string chunkStr, int currentPos, int selectionPosition, int selectionLength)
        {
            string[] lines = chunkStr.Split('\n');
            if (lines.Length < 2)
            {
                return null;
            }

            bool inPatch = true;
            bool inPreContext = true;
            int i = 1;

            Chunk result = new Chunk();
            result.ParseHeader(lines[0]);
            currentPos += lines[0].Length + 1;

            while (i < lines.Length)
            {
                string line = lines[i];
                if (inPatch)
                {
                    var patchLine = new PatchLine(line);

                    // do not refactor, there are no break points condition in VS Experss
                    if (currentPos <= selectionPosition + selectionLength && currentPos + line.Length >= selectionPosition)
                    {
                        patchLine.Selected = true;
                    }

                    if (line.StartsWith(" "))
                    {
                        result.AddContextLine(patchLine, inPreContext);
                    }
                    else if (line.StartsWith("-"))
                    {
                        inPreContext = false;
                        result.AddDiffLine(patchLine, true);
                    }
                    else if (line.StartsWith("+"))
                    {
                        inPreContext = false;
                        result.AddDiffLine(patchLine, false);
                    }
                    else if (line.StartsWith(GitModule.NoNewLineAtTheEnd))
                    {
                        if (result.CurrentSubChunk.AddedLines.Count > 0 && result.CurrentSubChunk.PostContext.Count == 0)
                        {
                            result.CurrentSubChunk.IsNoNewLineAtTheEnd = line;
                        }
                        else
                        {
                            result.CurrentSubChunk.WasNoNewLineAtTheEnd = line;
                        }
                    }
                    else
                    {
                        inPatch = false;
                    }
                }

                currentPos += line.Length + 1;
                i++;
            }

            return result;
        }

        [NotNull]
        public static Chunk FromNewFile([NotNull] GitModule module, [NotNull] string fileText, int selectionPosition, int selectionLength, bool reset, [NotNull] byte[] filePreabmle, [NotNull] Encoding fileContentEncoding)
        {
            Chunk result = new Chunk { _startLine = 0 };

            int currentPos = 0;
            string gitEol = module.GetEffectiveSetting("core.eol");
            string eol;
            if (gitEol == "crlf")
            {
                eol = "\r\n";
            }
            else if (gitEol == "native")
            {
                eol = Environment.NewLine;
            }
            else
            {
                eol = "\n";
            }

            int eolLength = eol.Length;

            string[] lines = fileText.Split(new[] { eol }, StringSplitOptions.None);
            int i = 0;

            while (i < lines.Length)
            {
                string line = lines[i];
                string preamble = i == 0 ? new string(fileContentEncoding.GetChars(filePreabmle)) : string.Empty;
                var patchLine = new PatchLine((reset ? "-" : "+") + preamble + line);

                // do not refactor, there are no breakpoints condition in VS Express
                if (currentPos <= selectionPosition + selectionLength && currentPos + line.Length >= selectionPosition)
                {
                    patchLine.Selected = true;
                }

                if (i == lines.Length - 1)
                {
                    if (line != string.Empty)
                    {
                        result.CurrentSubChunk.IsNoNewLineAtTheEnd = GitModule.NoNewLineAtTheEnd;
                        result.AddDiffLine(patchLine, reset);
                        if (reset && patchLine.Selected)
                        {
                            // if the last line is selected to be reset and there is no new line at the end of file
                            // then we also have to remove the last not selected line in order to add it right again with the "No newline.." indicator
                            PatchLine lastNotSelectedLine = result.CurrentSubChunk.RemovedLines.LastOrDefault(l => !l.Selected);
                            if (lastNotSelectedLine != null)
                            {
                                lastNotSelectedLine.Selected = true;
                                PatchLine clonedLine = lastNotSelectedLine.Clone();
                                clonedLine.SetOperation("+");
                                result.CurrentSubChunk.AddedLines.Add(clonedLine);
                            }

                            result.CurrentSubChunk.WasNoNewLineAtTheEnd = GitModule.NoNewLineAtTheEnd;
                        }
                    }
                }
                else
                {
                    result.AddDiffLine(patchLine, reset);
                }

                currentPos += line.Length + eolLength;
                i++;
            }

            return result;
        }

        public void ToPatch(SubChunkToPatchFnc subChunkToPatch, StringBuilder str)
        {
            bool wereSelectedLines = false;
            int addedCount = 0;
            int removedCount = 0;

            var diff = new StringBuilder();
            foreach (SubChunk subChunk in _subChunks)
            {
                if (diff.Length != 0)
                {
                    diff.Append('\n');
                }

                diff.Append(subChunkToPatch(subChunk, ref addedCount, ref removedCount, ref wereSelectedLines));
            }

            if (wereSelectedLines)
            {
                str.Append($"@@ -{_startLine},{removedCount} +{_startLine},{addedCount} @@\n");
                str.Append(diff);
            }
        }
    }
}