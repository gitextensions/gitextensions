using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Settings;

namespace PatchApply
{
    public class PatchManager
    {
        private List<Patch> _patches = new List<Patch>();

        public string PatchFileName { get; set; }

        public string DirToPatch { get; set; }

        public List<Patch> Patches
        {
            get { return _patches; }
            set { _patches = value; }
        }

        public static byte[] GetResetUnstagedLinesAsPatch(GitModule module, string text, int selectionPosition, int selectionLength, bool staged, Encoding fileContentEncoding)
        {

            string header;

            ChunkList selectedChunks = ChunkList.GetSelectedChunks(text, selectionPosition, selectionLength, staged, out header);

            if (selectedChunks == null)
                return null;

            string body = selectedChunks.ToResetUnstagedLinesPatch();

            //git apply has problem with dealing with autocrlf
            //I noticed that patch applies when '\r' chars are removed from patch if autocrlf is set to true
            if (body != null && module.EffectiveConfigFile.core.autocrlf.ValueOrDefault == AutoCRLFType.@true)
                body = body.Replace("\r", "");

            if (header == null || body == null)
                return null;
            else
                return GetPatchBytes(header, body, fileContentEncoding);
        }

        public static byte[] GetSelectedLinesAsPatch(GitModule module, string text, int selectionPosition, int selectionLength, bool staged, Encoding fileContentEncoding, bool isNewFile)
        {

            string header;

            ChunkList selectedChunks = ChunkList.GetSelectedChunks(text, selectionPosition, selectionLength, staged, out header);

            if (selectedChunks == null)
                return null;

            //if file is new, --- /dev/null has to be replaced by --- a/fileName
            if (isNewFile)
                header = CorrectHeaderForNewFile(header);

            string body = selectedChunks.ToStagePatch(staged, false);

            if (header == null || body == null)
                return null;
            else
                return GetPatchBytes(header, body, fileContentEncoding);
        }

        private static string CorrectHeaderForNewFile(string header)
        {
            string[] headerLines = header.Split(new string[] {"\n"}, StringSplitOptions.RemoveEmptyEntries);
            string pppLine = null;
            foreach (string line in headerLines)
                if (line.StartsWith("+++"))
                    pppLine = "---" + line.Substring(3);

            StringBuilder sb = new StringBuilder();

            foreach (string line in headerLines)
            {
                if (line.StartsWith("---"))
                    sb.Append(pppLine + "\n");
                else if (!line.StartsWith("new file mode"))
                    sb.Append(line + "\n");
            }

            return sb.ToString();
        }

        public static byte[] GetSelectedLinesAsNewPatch(GitModule module, string newFileName, string text, int selectionPosition, int selectionLength, Encoding fileContentEncoding, bool reset, byte[] FilePreabmle)
        {
            StringBuilder sb = new StringBuilder();
            string fileMode = "100000";//given fake mode to satisfy patch format, git will override this
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
                sb.Append("--- a/" + newFileName);
            else
                sb.Append("--- /dev/null");
            sb.Append("\n");
            sb.Append("+++ b/" + newFileName);
            sb.Append("\n");

            string header = sb.ToString();

            ChunkList selectedChunks = ChunkList.FromNewFile(module, text, selectionPosition, selectionLength, reset, FilePreabmle, fileContentEncoding);

            if (selectedChunks == null)
                return null;

            string body = selectedChunks.ToStagePatch(false, true);
            //git apply has problem with dealing with autocrlf
            //I noticed that patch applies when '\r' chars are removed from patch if autocrlf is set to true
            if (reset && body != null && module.EffectiveConfigFile.core.autocrlf.ValueOrDefault == AutoCRLFType.@true)
                body = body.Replace("\r", "");

            if (header == null || body == null)
                return null;
            else
                return GetPatchBytes(header, body, fileContentEncoding);
        }

        public static byte[] GetPatchBytes(string header, string body, Encoding fileContentEncoding)
        {
            byte[] hb = EncodingHelper.ConvertTo(GitModule.SystemEncoding, header);
            byte[] bb = EncodingHelper.ConvertTo(fileContentEncoding, body);
            byte[] result = new byte[hb.Length + bb.Length];
            hb.CopyTo(result, 0);
            bb.CopyTo(result, hb.Length);
            return result;
        }

        public string GetMD5Hash(string input)
        {
            IEnumerable<byte> bs = GetUTF8EncodedBytes(input);
            var s = new System.Text.StringBuilder();
            foreach (byte b in bs)
            {
                s.Append(b.ToString("x2").ToLower());
            }
            return s.ToString();
        }

        private static IEnumerable<byte> GetUTF8EncodedBytes(string input)
        {
            var x = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] bs = System.Text.Encoding.UTF8.GetBytes(input);
            bs = x.ComputeHash(bs);
            return bs;
        }

        //TODO encoding for each file in patch should be obtained separately from .gitattributes
        public void LoadPatch(string text, bool applyPatch, Encoding filesContentEncoding)
        {
            PatchProcessor patchProcessor = new PatchProcessor(filesContentEncoding);

            _patches = patchProcessor.CreatePatchesFromString(text).ToList();

            if (!applyPatch)
                return;

            foreach (Patch patchApply in _patches)
            {
                if (patchApply.Apply)
                    patchApply.ApplyPatch(filesContentEncoding);
            }
        }

        public void LoadPatchFile(bool applyPatch, Encoding filesContentEncoding)
        {
            using (var re = new StreamReader(PatchFileName, GitModule.LosslessEncoding))
            {
                LoadPatchStream(re, applyPatch, filesContentEncoding);
            }
        }

        private void LoadPatchStream(TextReader reader, bool applyPatch, Encoding filesContentEncoding)
        {
            LoadPatch(reader.ReadToEnd(), applyPatch, filesContentEncoding);
        }
    }

    internal class PatchLine
    {
        public string Text { get; set; }
        public bool Selected { get; set; }

        public PatchLine Clone()
        {
            var c = new PatchLine();
            c.Text = Text;
            c.Selected = Selected;
            return c;
        }

        public void SetOperation(string operationMark)
        {
            Text = operationMark + Text.Substring(1);
        }
    }

    internal class SubChunk
    {
        public List<PatchLine> PreContext = new List<PatchLine>();
        public List<PatchLine> RemovedLines = new List<PatchLine>();
        public List<PatchLine> AddedLines = new List<PatchLine>();
        public List<PatchLine> PostContext = new List<PatchLine>();
        public string WasNoNewLineAtTheEnd = null;
        public string IsNoNewLineAtTheEnd = null;


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
                diff = diff.Combine("\n", line.Text);

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
                        removePart = removePart.Combine("\n", " " + removedLine.Text.Substring(1));
                    else
                        prePart = prePart.Combine("\n", " " + removedLine.Text.Substring(1));
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
                        postPart = postPart.Combine("\n", " " + addedLine.Text.Substring(1));
                    else
                        prePart = prePart.Combine("\n", " " + addedLine.Text.Substring(1));
                    addedCount++;
                    removedCount++;
                }

            }

            diff = diff.Combine("\n", prePart);
            diff = diff.Combine("\n", removePart);
            if (PostContext.Count == 0 && (!staged || selectedLastRemovedLine))
                diff = diff.Combine("\n", WasNoNewLineAtTheEnd);
            diff = diff.Combine("\n", addPart);
            diff = diff.Combine("\n", postPart);
            foreach (PatchLine line in PostContext)
                diff = diff.Combine("\n", line.Text);
            //stage no new line at the end only if last +- line is selected
            if (PostContext.Count == 0 && (selectedLastLine || staged || isWholeFile))
                diff = diff.Combine("\n", IsNoNewLineAtTheEnd);
            if (PostContext.Count > 0)
                diff = diff.Combine("\n", WasNoNewLineAtTheEnd);

            return diff;
        }

        //patch base is changed file
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
                diff = diff.Combine("\n", line.Text);

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
                        postPart = postPart.Combine("\n", " " + addedLine.Text.Substring(1));
                    else
                        prePart = prePart.Combine("\n", " " + addedLine.Text.Substring(1));
                    addedCount++;
                    removedCount++;
                }
            }

            diff = diff.Combine("\n", prePart);
            diff = diff.Combine("\n", removePart);
            if (PostContext.Count == 0)
                diff = diff.Combine("\n", WasNoNewLineAtTheEnd);
            diff = diff.Combine("\n", addPart);
            diff = diff.Combine("\n", postPart);
            foreach (PatchLine line in PostContext)
                diff = diff.Combine("\n", line.Text);
            if (PostContext.Count == 0)
                diff = diff.Combine("\n", IsNoNewLineAtTheEnd);
            else
                diff = diff.Combine("\n", WasNoNewLineAtTheEnd);

            return diff;
        }
    }

    internal delegate string SubChunkToPatchFnc(SubChunk subChunk, ref int addedCount, ref int removedCount, ref bool wereSelectedLines);

    internal class Chunk
    {
        private int StartLine;
        private List<SubChunk> SubChunks = new List<SubChunk>();
        private SubChunk _CurrentSubChunk = null;

        public SubChunk CurrentSubChunk
        {
            get
            {
                if (_CurrentSubChunk == null)
                {
                    _CurrentSubChunk = new SubChunk();
                    SubChunks.Add(_CurrentSubChunk);
                }
                return _CurrentSubChunk;
            }
        }

        public void AddContextLine(PatchLine line, bool preContext)
        {
            if (preContext)
                CurrentSubChunk.PreContext.Add(line);
            else
                CurrentSubChunk.PostContext.Add(line);
        }

        public void AddDiffLine(PatchLine line, bool removed)
        {
            //if postContext is not empty @line comes from next SubChunk
            if (CurrentSubChunk.PostContext.Count > 0)
                _CurrentSubChunk = null;//start new SubChunk

            if (removed)
                CurrentSubChunk.RemovedLines.Add(line);
            else
                CurrentSubChunk.AddedLines.Add(line);
        }

        public bool ParseHeader(string header)
        {
            header = header.SkipStr("-");
            header = header.TakeUntilStr(",");

            return int.TryParse(header, out StartLine);
        }

        public static Chunk ParseChunk(string chunkStr, int currentPos, int selectionPosition, int selectionLength)
        {
            string[] lines = chunkStr.Split('\n');
            if (lines.Length < 2)
                return null;

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
                    PatchLine patchLine = new PatchLine()
                    {
                        Text = line
                    };
                    //do not refactor, there are no break points condition in VS Experss
                    if (currentPos <= selectionPosition + selectionLength && currentPos + line.Length >= selectionPosition)
                        patchLine.Selected = true;

                    if (line.StartsWith(" "))
                        result.AddContextLine(patchLine, inPreContext);
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
                            result.CurrentSubChunk.IsNoNewLineAtTheEnd = line;
                        else
                            result.CurrentSubChunk.WasNoNewLineAtTheEnd = line;
                    }
                    else
                        inPatch = false;

                }

                currentPos += line.Length + 1;
                i++;
            }

            return result;
        }

        public static Chunk FromNewFile(GitModule module, string fileText, int selectionPosition, int selectionLength, bool reset, byte[] FilePreabmle, Encoding fileContentEncoding)
        {
            Chunk result = new Chunk();
            result.StartLine = 0;
            int currentPos = 0;
            string gitEol = module.GetEffectiveSetting("core.eol");
            string eol;
            if ("crlf".Equals(gitEol))
                eol = "\r\n";
            else if ("native".Equals(gitEol))
                eol = Environment.NewLine;
            else
                eol = "\n";

            int eolLength = eol.Length;

            string[] lines = fileText.Split(new string[] { eol }, StringSplitOptions.None);
            int i = 0;

            while (i < lines.Length)
            {
                string line = lines[i];
                string preamble = (i == 0 ? new string(fileContentEncoding.GetChars(FilePreabmle)) : string.Empty);
                PatchLine patchLine = new PatchLine()
                {
                    Text = (reset ? "-" : "+") + preamble + line
                };
                //do not refactor, there are no breakpoints condition in VS Experss
                if (currentPos <= selectionPosition + selectionLength && currentPos + line.Length >= selectionPosition)
                    patchLine.Selected = true;

                if (i == lines.Length - 1)
                {
                    if (!line.Equals(string.Empty))
                    {
                        result.CurrentSubChunk.IsNoNewLineAtTheEnd = GitModule.NoNewLineAtTheEnd;
                        result.AddDiffLine(patchLine, reset);
                        if (reset && patchLine.Selected)
                        {
                            //if the last line is selected to be reset and there is no new line at the end of file
                            //then we also have to remove the last not selected line in order to add it right again with the "No newline.." indicator
                            PatchLine lastNotSelectedLine = result.CurrentSubChunk.RemovedLines.LastOrDefault(aLine => !aLine.Selected);
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
                    result.AddDiffLine(patchLine, reset);

                currentPos += line.Length + eolLength;
                i++;
            }
            return result;
        }


        public string ToPatch(SubChunkToPatchFnc subChunkToPatch)
        {
            bool wereSelectedLines = false;
            string diff = null;
            int addedCount = 0;
            int removedCount = 0;

            foreach (SubChunk subChunk in SubChunks)
            {
                string subDiff = subChunkToPatch(subChunk, ref addedCount, ref removedCount, ref wereSelectedLines);
                diff = diff.Combine("\n", subDiff);
            }

            if (!wereSelectedLines)
                return null;

            diff = "@@ -" + StartLine + "," + removedCount + " +" + StartLine + "," + addedCount + " @@".Combine("\n", diff);

            return diff;
        }
    }

    internal class ChunkList : List<Chunk>
    {

        public static ChunkList GetSelectedChunks(string text, int selectionPosition, int selectionLength, bool staged, out string header)
        {
            header = null;
            //When there is no patch, return nothing
            if (string.IsNullOrEmpty(text))
                return null;

            // TODO: handling submodules
            // Divide diff into header and patch
            int patchPos = text.IndexOf("@@");
            if (patchPos < 1)
                return null;

            header = text.Substring(0, patchPos);
            string diff = text.Substring(patchPos - 1);

            string[] chunks = diff.Split(new string[] { "\n@@" }, StringSplitOptions.RemoveEmptyEntries);
            ChunkList selectedChunks = new ChunkList();
            int i = 0;
            int currentPos = patchPos - 1;

            while (i < chunks.Length && currentPos <= selectionPosition + selectionLength)
            {
                string chunkStr = chunks[i];
                currentPos += 3;
                //if selection intersects with chunsk
                if (currentPos + chunkStr.Length >= selectionPosition)
                {
                    Chunk chunk = Chunk.ParseChunk(chunkStr, currentPos, selectionPosition, selectionLength);
                    if (chunk != null)
                        selectedChunks.Add(chunk);
                }
                currentPos += chunkStr.Length;
                i++;
            }

            return selectedChunks;
        }

        public static ChunkList FromNewFile(GitModule module, string text, int selectionPosition, int selectionLength, bool reset, byte[] FilePreabmle, Encoding fileContentEncoding)
        {
            Chunk chunk = Chunk.FromNewFile(module, text, selectionPosition, selectionLength, reset, FilePreabmle, fileContentEncoding);
            ChunkList result = new ChunkList();
            result.Add(chunk);
            return result;
        }

        public string ToResetUnstagedLinesPatch()
        {
            SubChunkToPatchFnc subChunkToPatch = (SubChunk subChunk, ref int addedCount, ref int removedCount, ref bool wereSelectedLines) =>
                {
                    return subChunk.ToResetUnstagedLinesPatch(ref addedCount, ref removedCount, ref wereSelectedLines);
                };

            return ToPatch(subChunkToPatch);
        }

        public string ToStagePatch(bool staged, bool isWholeFile)
        {
            SubChunkToPatchFnc subChunkToPatch = (SubChunk subChunk, ref int addedCount, ref int removedCount, ref bool wereSelectedLines) =>
            {
                return subChunk.ToStagePatch(ref addedCount, ref removedCount, ref wereSelectedLines, staged, isWholeFile);
            };

            return ToPatch(subChunkToPatch);
        }

        protected string ToPatch(SubChunkToPatchFnc subChunkToPatch)
        {
            string result = null;

            foreach (Chunk chunk in this)
                result = result.Combine("\n", chunk.ToPatch(subChunkToPatch));


            if (result != null)
            {
                result = result.Combine("\n", "--");
                result = result.Combine("\n", Application.ProductName + " " + AppSettings.ProductVersion);
            }
            return result;

        }

    }


}