using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using JetBrains.Annotations;

namespace GitCommands.Patches
{
    public static class PatchManager
    {
        public static byte[]? GetResetWorkTreeLinesAsPatch(string text, int selectionPosition, int selectionLength, Encoding fileContentEncoding)
        {
            IReadOnlyList<Chunk> selectedChunks = GetSelectedChunks(text, selectionPosition, selectionLength, out string? header);

            if (selectedChunks is null)
            {
                return null;
            }

            string? body = ToResetWorkTreeLinesPatch(selectedChunks);

            if (header is null || body is null)
            {
                return null;
            }
            else
            {
                return GetPatchBytes(header, body, fileContentEncoding);
            }
        }

        public static byte[]? GetSelectedLinesAsPatch(string text, int selectionPosition, int selectionLength, bool isIndex, Encoding fileContentEncoding, bool reset, bool isNewFile, bool isRenamed)
        {
            IReadOnlyList<Chunk>? selectedChunks = GetSelectedChunks(text, selectionPosition, selectionLength, out string header);

            if (selectedChunks is null || header is null)
            {
                return null;
            }

            // if file is new, --- /dev/null has to be replaced by --- a/fileName
            if (isNewFile)
            {
                header = CorrectHeaderForNewFile(header);
            }

            // if file is renamed and selected lines are being reset from index then the patch undoes the rename too
            if (isIndex && isRenamed && reset)
            {
                header = CorrectHeaderForRenamedFile(header);
            }

            string? body = ToIndexPatch(selectedChunks, isIndex, isWholeFile: false);

            if (body is null)
            {
                return null;
            }

            return GetPatchBytes(header, body, fileContentEncoding);
        }

        private static string CorrectHeaderForNewFile(string header)
        {
            string[] headerLines = header.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            string? pppLine = null;
            foreach (string line in headerLines)
            {
                if (line.StartsWith("+++"))
                {
                    pppLine = "---" + line[3..];
                }
            }

            StringBuilder sb = new();

            foreach (string line in headerLines)
            {
                if (line.StartsWith("---"))
                {
                    sb.Append(pppLine).Append('\n');
                }
                else if (!line.StartsWith("new file mode"))
                {
                    sb.Append(line).Append('\n');
                }
            }

            return sb.ToString();
        }

        private static string CorrectHeaderForRenamedFile(string header)
        {
            // Expected input:
            //
            // diff --git a/original.txt b/original2.txt
            // similarity index 88%
            // rename from original.txt
            // rename to original2.txt
            // index 0e05069..d4029ea 100644
            // --- a/original.txt
            // +++ b/original2.txt

            // Expected output:
            //
            // diff --git a/original2.txt b/original2.txt
            // index 0e05069..d4029ea 100644
            // --- a/original2.txt
            // +++ b/original2.txt

            string[] headerLines = header.Split('\n');
            string? oldNameWithPrefix = null;
            string? newName = null;
            foreach (string line in headerLines)
            {
                if (line.StartsWith("+++"))
                {
                    // Takes the "original2.txt" part from patch file line: +++ b/original2.txt
                    newName = line[6..];
                }
                else if (line.StartsWith("---"))
                {
                    // Takes the "a/original.txt" part from patch file line: --- a/original.txt
                    oldNameWithPrefix = line[4..];
                }
            }

            StringBuilder sb = new();

            for (int i = 0; i < headerLines.Length; i++)
            {
                string line = headerLines[i];
                if (line.StartsWith("--- "))
                {
                    line = $"--- a/{newName}";
                }
                else if (line.Contains($" {oldNameWithPrefix} "))
                {
                    line = line.Replace($" {oldNameWithPrefix} ", $" a/{newName} ");
                }
                else if (line.StartsWith("rename from ") || line.StartsWith("rename to ") || line.StartsWith("similarity index "))
                {
                    // Note: this logic depends on git not localizing patch file format
                    continue;
                }

                if (i != 0)
                {
                    sb.Append('\n');
                }

                sb.Append(line);
            }

            return sb.ToString();
        }

        public static byte[]? GetSelectedLinesAsNewPatch(IGitModule module, string newFileName, string text, int selectionPosition, int selectionLength, Encoding fileContentEncoding, bool reset, byte[] filePreamble, string? treeGuid)
        {
            IReadOnlyList<Chunk> selectedChunks = FromNewFile(module, text, selectionPosition, selectionLength, reset, filePreamble, fileContentEncoding);
            bool isTracked = treeGuid is not null;
            string? body = ToIndexPatch(selectedChunks, isIndex: isTracked, isWholeFile: true);

            if (body is null)
            {
                return null;
            }

            const string fileMode = "100644"; // given fake mode to satisfy patch format, git will override this
            StringBuilder header = new();

            header.Append("diff --git a/").Append(newFileName).Append(" b/").Append(newFileName).Append('\n');

            if (!reset && !isTracked)
            {
                header.Append("new file mode ").Append(fileMode).Append('\n');
            }

            header.Append($"index 0000000..{treeGuid ?? "0000000"}\n");

            if (reset || isTracked)
            {
                header.Append("--- a/").Append(newFileName).Append('\n');
            }
            else
            {
                header.Append("--- /dev/null").Append('\n');
            }

            header.Append("+++ b/").Append(newFileName).Append('\n');

            return GetPatchBytes(header.ToString(), body, fileContentEncoding);
        }

        private static byte[] GetPatchBytes(string header, string body, Encoding fileContentEncoding)
        {
            byte[] hb = EncodingHelper.ConvertTo(GitModule.SystemEncoding, header);
            byte[] bb = EncodingHelper.ConvertTo(fileContentEncoding, body);
            byte[] result = new byte[hb.Length + bb.Length];
            hb.CopyTo(result, 0);
            bb.CopyTo(result, hb.Length);
            return result;
        }

        private static IReadOnlyList<Chunk>? GetSelectedChunks(string text, int selectionPosition, int selectionLength, out string? header)
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

            header = text[..patchPos];
            string diff = text[(patchPos - 1)..];

            string[] chunks = diff.Split(new[] { "\n@@" }, StringSplitOptions.RemoveEmptyEntries);
            List<Chunk> selectedChunks = [];
            int i = 0;
            int currentPos = patchPos - 1;

            while (i < chunks.Length && currentPos <= selectionPosition + selectionLength)
            {
                string chunkStr = chunks[i];
                currentPos += 3;

                // if selection intersects with chunks
                if (currentPos + chunkStr.Length >= selectionPosition)
                {
                    Chunk? chunk = Chunk.ParseChunk(chunkStr, currentPos, selectionPosition, selectionLength);
                    if (chunk is not null)
                    {
                        selectedChunks.Add(chunk);
                    }
                }

                currentPos += chunkStr.Length;
                i++;
            }

            return selectedChunks;
        }

        private static IReadOnlyList<Chunk> FromNewFile(IGitModule module, string text, int selectionPosition, int selectionLength, bool reset, byte[] filePreamble, Encoding fileContentEncoding)
        {
            return new[] { Chunk.FromNewFile(module, text, selectionPosition, selectionLength, reset, filePreamble, fileContentEncoding) };
        }

        private static string? ToResetWorkTreeLinesPatch(IEnumerable<Chunk> chunks)
        {
            static string? SubChunkToPatch(SubChunk subChunk, ref int addedCount, ref int removedCount, ref bool wereSelectedLines)
            {
                return subChunk.ToResetWorkTreeLinesPatch(ref addedCount, ref removedCount, ref wereSelectedLines);
            }

            return ToPatch(chunks, SubChunkToPatch);
        }

        private static string? ToIndexPatch(IEnumerable<Chunk> chunks, bool isIndex, bool isWholeFile)
        {
            string? SubChunkToPatch(SubChunk subChunk, ref int addedCount, ref int removedCount, ref bool wereSelectedLines)
            {
                return subChunk.ToIndexPatch(ref addedCount, ref removedCount, ref wereSelectedLines, isIndex, isWholeFile);
            }

            return ToPatch(chunks, SubChunkToPatch);
        }

        private static string? ToPatch(IEnumerable<Chunk> chunks, [InstantHandle] SubChunkToPatchFnc subChunkToPatch)
        {
            StringBuilder result = new();

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

    [DebuggerDisplay("{" + nameof(Text) + "}")]
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
            Text = operationMark + Text[1..];
        }
    }

    internal sealed class SubChunk
    {
        public List<PatchLine> PreContext { get; } = [];
        public List<PatchLine> RemovedLines { get; } = [];
        public List<PatchLine> AddedLines { get; } = [];
        public List<PatchLine> PostContext { get; } = [];
        public string? WasNoNewLineAtTheEnd { get; set; }
        public string? IsNoNewLineAtTheEnd { get; set; }

        public string? ToIndexPatch(ref int addedCount, ref int removedCount, ref bool wereSelectedLines, bool isIndex, bool isWholeFile)
        {
            string? diff = null;
            string? removePart = null;
            string? addPart = null;
            string? prePart = null;
            string? postPart = null;
            bool inPostPart = false;
            bool selectedLastRemovedLine = false;
            bool selectedLastAddedLine = false;
            addedCount += PreContext.Count + PostContext.Count;
            removedCount += PreContext.Count + PostContext.Count;

            foreach (PatchLine line in PreContext)
            {
                diff = diff.Combine("\n", line.Text);
            }

            foreach (PatchLine removedLine in RemovedLines)
            {
                selectedLastAddedLine = removedLine.Selected;
                if (removedLine.Selected)
                {
                    wereSelectedLines = true;
                    inPostPart = true;
                    removePart = removePart.Combine("\n", removedLine.Text);
                    removedCount++;
                }
                else if (!isIndex)
                {
                    if (inPostPart)
                    {
                        removePart = removePart.Combine("\n", " " + removedLine.Text[1..]);
                    }
                    else
                    {
                        prePart = prePart.Combine("\n", " " + removedLine.Text[1..]);
                    }

                    addedCount++;
                    removedCount++;
                }
            }

            foreach (PatchLine addedLine in AddedLines)
            {
                selectedLastRemovedLine = addedLine.Selected;
                if (addedLine.Selected)
                {
                    wereSelectedLines = true;
                    inPostPart = true;
                    addPart = addPart.Combine("\n", addedLine.Text);
                    addedCount++;
                }
                else if (isIndex)
                {
                    if (inPostPart)
                    {
                        postPart = postPart.Combine("\n", " " + addedLine.Text[1..]);
                    }
                    else
                    {
                        prePart = prePart.Combine("\n", " " + addedLine.Text[1..]);
                    }

                    addedCount++;
                    removedCount++;
                }
            }

            diff = diff.Combine("\n", prePart);
            diff = diff.Combine("\n", removePart);
            if (PostContext.Count == 0 && (selectedLastRemovedLine || !isIndex))
            {
                diff = diff.Combine("\n", WasNoNewLineAtTheEnd);
            }

            diff = diff.Combine("\n", addPart);
            diff = diff.Combine("\n", postPart);
            foreach (PatchLine line in PostContext)
            {
                diff = diff.Combine("\n", line.Text);
            }

            diff = PostContext.Count switch
            {
                // stage no new line at the end only if last +- line is selected
                0 when selectedLastAddedLine || isIndex || isWholeFile => diff.Combine("\n", IsNoNewLineAtTheEnd),
                > 0 => diff.Combine("\n", WasNoNewLineAtTheEnd),
                _ => diff
            };

            return diff;
        }

        // patch base is changed file
        public string? ToResetWorkTreeLinesPatch(ref int addedCount, ref int removedCount, ref bool wereSelectedLines)
        {
            string? diff = null;
            string? removePart = null;
            string? addPart = null;
            string? prePart = null;
            string? postPart = null;
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
                    addPart = addPart.Combine("\n", "+" + removedLine.Text[1..]);
                    addedCount++;
                }
            }

            foreach (PatchLine addedLine in AddedLines)
            {
                if (addedLine.Selected)
                {
                    wereSelectedLines = true;
                    inPostPart = true;
                    removePart = removePart.Combine("\n", "-" + addedLine.Text[1..]);
                    removedCount++;
                }
                else
                {
                    if (inPostPart)
                    {
                        postPart = postPart.Combine("\n", " " + addedLine.Text[1..]);
                    }
                    else
                    {
                        prePart = prePart.Combine("\n", " " + addedLine.Text[1..]);
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

    internal delegate string? SubChunkToPatchFnc(SubChunk subChunk, ref int addedCount, ref int removedCount, ref bool wereSelectedLines);

    internal sealed partial class Chunk
    {
        private int _startLine;
        private readonly List<SubChunk> _subChunks = [];
        private SubChunk? _currentSubChunk;

        [GeneratedRegex(@".*-(?<startline>\d+),", RegexOptions.ExplicitCapture)]
        private static partial Regex HeaderRegex();

        private SubChunk CurrentSubChunk
        {
            get
            {
                if (_currentSubChunk is null)
                {
                    _currentSubChunk = new SubChunk();
                    _subChunks.Add(_currentSubChunk);
                }

                return _currentSubChunk;
            }
        }

        private void AddContextLine(PatchLine line, bool preContext)
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

        private void AddDiffLine(PatchLine line, bool removed)
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
        private void ParseHeader(string header)
        {
            Match match = HeaderRegex().Match(header);

            if (match.Success)
            {
                _startLine = int.Parse(match.Groups["startline"].Value);
            }
        }

        public static Chunk? ParseChunk(string chunkStr, int currentPos, int selectionPosition, int selectionLength)
        {
            string[] lines = chunkStr.Split(Delimiters.LineFeed);
            if (lines.Length < 2)
            {
                return null;
            }

            bool inPatch = true;
            bool inPreContext = true;
            int i = 1;

            Chunk result = new();
            result.ParseHeader(lines[0]);
            currentPos += lines[0].Length + 1;

            while (i < lines.Length)
            {
                string line = lines[i];
                if (inPatch)
                {
                    PatchLine patchLine = new(line);

                    // do not refactor, there are no break points condition in VS Express
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

        public static Chunk FromNewFile(IGitModule module, string fileText, int selectionPosition, int selectionLength, bool reset, byte[] filePreamble, Encoding fileContentEncoding)
        {
            Chunk result = new() { _startLine = 0 };

            int currentPos = 0;
            string gitEol = module.GetEffectiveSetting("core.eol");
            string eol = gitEol switch
            {
                "crlf" => "\r\n",
                "native" => Environment.NewLine,
                _ => "\n"
            };

            int eolLength = eol.Length;

            string[] lines = fileText.Split(new[] { eol }, StringSplitOptions.None);
            int i = 0;

            while (i < lines.Length)
            {
                string line = lines[i];
                string preamble = i == 0 ? new string(fileContentEncoding.GetChars(filePreamble)) : string.Empty;
                PatchLine patchLine = new((reset ? "-" : "+") + preamble + line);

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
                            PatchLine? lastNotSelectedLine = result.CurrentSubChunk.RemovedLines.LastOrDefault(l => !l.Selected);
                            if (lastNotSelectedLine is not null)
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

            StringBuilder diff = new();
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
