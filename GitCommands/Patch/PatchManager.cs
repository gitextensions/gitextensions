using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using GitCommands;

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
            //When there is no patch, return nothing
            if (string.IsNullOrEmpty(text))
                return null;

            // Divide diff into header and patch
            int patchPos = text.IndexOf("@@");
            if (patchPos < 1)
                return null;
            string header = text.Substring(0, patchPos);
            string diff = text.Substring(patchPos - 1);

            string[] chunks = diff.Split(new string[] {"\n@@"}, StringSplitOptions.RemoveEmptyEntries);
            ChunkList selectedChunks = new ChunkList();
            int i = 0;
            int currentPos = patchPos-1;

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

            string body = selectedChunks.ToResetUnstagedLinesPatch();

            //git apply has problem with dealing with autocrlf
            //I noticed that patch applies when '\r' chars are removed from patch if autocrlf is set to true
            if ("true".Equals(module.GetEffectiveSetting("core.autocrlf"), StringComparison.InvariantCultureIgnoreCase))
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


        public static byte[] GetSelectedLinesAsPatch(string text, int selectionPosition, int selectionLength, bool staged, Encoding fileContentEncoding)
        {
            //When there is no patch, return nothing
            if (string.IsNullOrEmpty(text))
                return null;

            // Ported from the git-gui tcl code to C#
            // see lib/diff.tcl

            // Divide diff into header and patch
            int patchPos = text.IndexOf("@@");
            string header = text.Substring(0, patchPos);
            string diff = text.Substring(patchPos);

            // Get selection position and length
            int first = selectionPosition - patchPos;
            int last = first + selectionLength;

            // Make sure the header is not in the selection
            if (first < 0)
            {
                first = 0;
            }
            // Selection was only on the header section, so cannot proceed
            if (last < 0) return null;

            // Round selection to previous and next line breaks to select the whole lines
            int firstLine = diff.LastIndexOf("\n", first, first) + 1;
            int lastLine = diff.IndexOf("\n", last);
            if (lastLine == -1)
                lastLine = diff.Length - 1;

            // Are we looking at a diff from the working dir or the staging area
            char toContext = staged ? '+' : '-';

            // this will hold the entire patch at the end
            string wholepatch = "";

            // loop until $first_l has reached $last_l
            // ($first_l is modified inside the loop!)
            while (firstLine < lastLine)
            {
                // search from $first_l backwards for lines starting with @@
                int iLine = diff.LastIndexOf("\n@@", firstLine, firstLine);
                if (iLine == -1 && diff.Substring(0, 2) != "@@")
                {
                    // if there's not a @@ above, then the selected range
                    // must have come before the first @@
                    iLine = diff.IndexOf("\n@@", firstLine, lastLine - firstLine);
                    if (iLine == -1)
                    {
                        // if the @@ is not even in the selected range then
                        // any further action is useless because there is no
                        // selected data that can be applied
                        return null;
                    }
                }
                iLine++;
                // $i_l is now at the beginning of the first @@ line in 
                // front of first_l

                // pick start line number from hunk header
                // example: hh = "@@ -604,58 +604,105 @@ foo bar"
                string hh = diff.Substring(iLine, diff.IndexOf("\n", iLine) - iLine);
                // example: hh = "@@ -604"
                hh = hh.Split(',')[0];
                // example: hlh = "604"
                string hln = hh.Split('-')[1];

                // There is a special situation to take care of. Consider this
                // hunk:
                //
                //    @@ -10,4 +10,4 @@
                //     context before
                //    -old 1
                //    -old 2
                //    +new 1
                //    +new 2
                //     context after
                //
                // We used to keep the context lines in the order they appear in
                // the hunk. But then it is not possible to correctly stage only
                // "-old 1" and "+new 1" - it would result in this staged text:
                //
                //    context before
                //    old 2
                //    new 1
                //    context after
                //
                // (By symmetry it is not possible to *un*stage "old 2" and "new
                // 2".)
                //
                // We resolve the problem by introducing an asymmetry, namely,
                // when a "+" line is *staged*, it is moved in front of the
                // context lines that are generated from the "-" lines that are
                // immediately before the "+" block. That is, we construct this
                // patch:
                //
                //    @@ -10,4 +10,5 @@
                //     context before
                //    +new 1
                //     old 1
                //     old 2
                //     context after
                //
                // But we do *not* treat "-" lines that are *un*staged in a
                // special way.
                //
                // With this asymmetry it is possible to stage the change "old
                // 1" -> "new 1" directly, and to stage the change "old 2" ->
                // "new 2" by first staging the entire hunk and then unstaging
                // the change "old 1" -> "new 1".
                //
                // Applying multiple lines adds complexity to the special
                // situation.  The pre_context must be moved after the entire
                // first block of consecutive staged "+" lines, so that
                // staging both additions gives the following patch:
                //
                //    @@ -10,4 +10,6 @@
                //     context before
                //    +new 1
                //    +new 2
                //     old 1
                //     old 2
                //     context after

                // This is non-empty if and only if we are _staging_ changes;
                // then it accumulates the consecutive "-" lines (after
                // converting them to context lines) in order to be moved after
                // "+" change lines.
                string preContext = "";

                int n = 0;
                int m = 0;
                // move $i_l to the first line after the @@ line $i_l pointed at
                iLine = diff.IndexOf("\n", iLine) + 1;
                string patch = "";

                // while $i_l is not at the end of the file and not 
                // at the next @@ line
                while (iLine < diff.Length - 1 && diff.Substring(iLine, 2) != "@@")
                {
                    // set $next_l to the beginning of the next 
                    // line after $i_l
                    int nextLine = diff.IndexOf("\n", iLine) + 1;
                    if (nextLine == 0)
                    {
                        nextLine = diff.Length;
                        m--;
                        n--;
                    }

                    // get character at $i_l 
                    char c1 = diff[iLine];

                    // if $i_l is in selected range and the line starts 
                    // with either - or + this is a line to stage/unstage
                    if (firstLine <= iLine && iLine < lastLine && (c1 == '-' || c1 == '+'))
                    {
                        // set $ln to the content of the line at $i_l
                        string ln = diff.Substring(iLine, nextLine - iLine);
                        // if line starts with -
                        if (c1 == '-')
                        {
                            // increase n counter by one
                            n++;
                            // update $patch
                            patch += preContext + ln;
                            // reset $pre_context
                            preContext = "";

                            // if line starts with +
                        }
                        else
                        {
                            // increase m counter by one
                            m++;
                            // update $patch
                            patch += ln;
                        }

                        // if the line doesn't start with either - or +
                        // this is a context line 
                    }
                    else if (c1 != '-' && c1 != '+')
                    {
                        // set $ln to the content of the line at $i_l
                        string ln = diff.Substring(iLine, nextLine - iLine);
                        // update $patch
                        patch += preContext + ln;
                        // increase counters by one each
                        if (c1 != '\\')
                        {
                            n++;
                            m++;
                        }
                        // reset $pre_context
                        preContext = "";

                        // if the line starts with $to_context (see earlier)
                        // the sign at the beginning should be stripped
                    }
                    else if (c1 == toContext)
                    {
                        // turn change line into context line
                        string ln = diff.Substring(iLine + 1, nextLine - iLine - 1);
                        if (c1 == '-')
                            preContext += " " + ln;
                        else
                            patch += " " + ln;
                        // increase counters by one each
                        n++;
                        m++;

                        // if the line starts with the opposite sign of
                        // $to_context this line should be removed
                    }
                    else
                    {
                        // a change in the opposite direction of
                        // to_context which is outside the range of
                        // lines to apply.
                        patch += preContext;
                        preContext = "";
                    }
                    // set $i_l to the next line
                    iLine = nextLine;
                }
                // finished current hunk (reached @@ or file/diff end)

                // update $patch (makes sure $pre_context gets appended)
                patch += preContext;
                // update $wholepatch with the current hunk
                wholepatch += "@@ -" + hln + "," + n.ToString() + " +" + hln + "," + m.ToString() + " @@\n" + patch;

                // set $first_l to first line after the next @@ line
                firstLine = diff.IndexOf("\n", iLine) + 1;
                if (firstLine == 0)
                    break;
            }
            // we are almost done, $wholepatch should no contain all the 
            // (modified) hunks
            byte[] hb = EncodingHelper.ConvertTo(GitModule.SystemEncoding, header);
            byte[] bb = EncodingHelper.ConvertTo(fileContentEncoding, wholepatch);
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

        //TODO encoding for each file in patch should be obtained separatly from .gitattributes
        public void LoadPatch(string text, bool applyPatch, Encoding filesContentEncoding)
        {

            PatchProcessor _patchProcessor = new PatchProcessor(filesContentEncoding);

            _patches = _patchProcessor.CreatePatchesFromString(text);

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
    }

    internal class SubChunk
    {
        public List<PatchLine> PreContext = new List<PatchLine>();
        public List<PatchLine> RemovedLines = new List<PatchLine>();
        public List<PatchLine> AddedLines = new List<PatchLine>();
        public List<PatchLine> PostContext = new List<PatchLine>();
        public string WasNoNewLineAtTheEnd = null;
        public string IsNoNewLineAtTheEnd = null;

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
                    removePart = removePart.Combine("\n", "+" + removedLine.Text.Substring(1));
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
            diff = diff.Combine("\n", WasNoNewLineAtTheEnd);
            diff = diff.Combine("\n", addPart);
            diff = diff.Combine("\n", postPart);
            foreach (PatchLine line in PostContext)
                diff = diff.Combine("\n", line.Text);
            diff = diff.Combine("\n", IsNoNewLineAtTheEnd);

            return diff;
        }
    }

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
                    else if (line.StartsWith("\\"))
                    {
                        if (line.Contains("No newline at end of file"))
                            if (result.CurrentSubChunk.AddedLines.Count > 0)
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

        //patch base is changed file
        public string ToResetUnstagedLinesPatch()
        {
            bool wereSelectedLines = false;
            string diff = null;
            int addedCount = 0;
            int removedCount = 0;

            foreach (SubChunk subChunk in SubChunks)
            {
                string subDiff = subChunk.ToResetUnstagedLinesPatch(ref addedCount, ref removedCount, ref wereSelectedLines);
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

        public string ToResetUnstagedLinesPatch()
        {
            string result = null;

            foreach (Chunk chunk in this)
                result = result.Combine("\n", chunk.ToResetUnstagedLinesPatch());


            if (result != null)
            {
                result = result.Combine("\n", "--");
                result = result.Combine("\n", Application.ProductName + " " + Settings.GitExtensionsVersionString);                
            }
            return result;
        }
    }


}