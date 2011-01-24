using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GitCommands;

namespace PatchApply
{
    public class Patch
    {
        private StringBuilder _textBuilder;

        public Patch()
        {
            Apply = true;
            Bookmarks = new List<int>();
            File = FileType.Text;
        }

        public enum PatchType
        {
            NewFile,
            DeleteFile,
            ChangeFile
        }

        public enum FileType
        {
            Binary,
            Text
        }

        public FileType File { get; set; }
        public string FileNameA { get; set; }
        public string FileNameB { get; set; }
        public string FileTextB { get; set; }
        public string DirToPatch { get; set; }
        public int Rate { get; set; }
        public bool Apply { get; set; }

        public List<int> Bookmarks { get; set; }

        public PatchType Type { get; set; }

        public string Text
        {
            get { return _textBuilder == null ? null : _textBuilder.ToString(); }
        }

        public void AppendText(string text)
        {
            GetTextBuilder().Append(text);
        }

        public void AppendTextLine(string line)
        {
            GetTextBuilder().Append(line).Append('\n');
        }

        public void ApplyPatch()
        {
            FileTextB = "";
            Rate = 100;

            if (Type == PatchType.DeleteFile)
            {
                HandleDeletePatchType();
                return;
            }

            if (Text == null)
                return;

            if (Type == PatchType.NewFile)
            {
                HandleNewFilePatchType();
                return;
            }

            if (Type == PatchType.ChangeFile)
            {
                HandleChangeFilePatchType();
                return;
            }
        }

        private StringBuilder GetTextBuilder()
        {
            return _textBuilder ?? (_textBuilder = new StringBuilder());
        }

        private string LoadFile(string fileName)
        {
            try
            {
                using (var streamReader = new StreamReader(DirToPatch + fileName, Settings.Encoding))
                {
                    string retval = "";
                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        retval += line + "\n";
                    }

                    if (retval.Length > 0 && retval[retval.Length - 1] == '\n')
                        retval = retval.Remove(retval.Length - 1, 1);

                    return retval;
                }
            }
            catch
            {
                return "";
            }
        }

        private void HandleChangeFilePatchType()
        {
            var fileLines = new List<string>();
            foreach (string s in LoadFile(FileNameA).Split('\n'))
            {
                fileLines.Add(s);
            }

            int lineNumber = 0;
            foreach (string line in Text.Split('\n'))
            {
                //Parse first line
                //@@ -1,4 +1,4 @@
                if (line.StartsWith("@@") && line.LastIndexOf("@@") > 0)
                {
                    string pos = line.Substring(3, line.LastIndexOf("@@") - 3).Trim();
                    string[] addrem = pos.Split('+', '-');
                    string[] oldLines = addrem[1].Split(',');

                    lineNumber = Int32.Parse(oldLines[0]) - 1;
                    continue;
                }

                if (line.StartsWith(" "))
                {
                    //Do some extra checks
                    if (line.Length > 0)
                    {
                        if (fileLines.Count > lineNumber && fileLines[lineNumber].CompareTo(line.Substring(1)) != 0)
                            Rate -= 20;
                    }
                    else
                    {
                        if (fileLines.Count > lineNumber && fileLines[lineNumber] != "")
                            Rate -= 20;
                    }

                    lineNumber++;
                }
                if (line.StartsWith("-"))
                {
                    if (line.Length > 0)
                    {
                        if (fileLines.Count > lineNumber && fileLines[lineNumber].CompareTo(line.Substring(1)) != 0)
                            Rate -= 20;
                    }
                    else
                    {
                        if (fileLines.Count > lineNumber && fileLines[lineNumber] != "")
                            Rate -= 20;
                    }

                    Bookmarks.Add(lineNumber);

                    if (fileLines.Count > lineNumber)
                        fileLines.RemoveAt(lineNumber);
                    else
                        Rate -= 20;
                }
                if (line.StartsWith("+"))
                {
                    string insertLine = "";
                    if (line.Length > 1)
                        insertLine = line.Substring(1);

                    //Is the patch allready applied?
                    if (fileLines.Count > lineNumber && fileLines[lineNumber].CompareTo(insertLine) == 0)
                    {
                        Rate -= 20;
                    }

                    fileLines.Insert(lineNumber, insertLine);
                    Bookmarks.Add(lineNumber);

                    lineNumber++;
                }
            }
            foreach (string patchedLine in fileLines)
            {
                FileTextB += patchedLine + "\n";
            }
            if (FileTextB.Length > 0 && FileTextB[FileTextB.Length - 1] == '\n')
                FileTextB = FileTextB.Remove(FileTextB.Length - 1, 1);

            if (Rate != 100)
                Apply = false;
        }

        private void HandleNewFilePatchType()
        {
            foreach (string line in Text.Split('\n'))
            {
                if (line.Length > 0 && line.StartsWith("+"))
                {
                    if (line.Length > 4 && line.StartsWith("+ï»¿"))
                        AppendText(line.Substring(4));
                    else
                        if (line.Length > 1)
                            FileTextB += line.Substring(1);

                    FileTextB += "\n";
                }
            }
            if (FileTextB.Length > 0 && FileTextB[FileTextB.Length - 1] == '\n')
                FileTextB = FileTextB.Remove(FileTextB.Length - 1, 1);
            Rate = 100;

            if (System.IO.File.Exists(DirToPatch + FileNameB))
            {
                Rate -= 40;
                Apply = false;
            }
        }

        private void HandleDeletePatchType()
        {
            FileTextB = "";
            Rate = 100;

            if (!System.IO.File.Exists(DirToPatch + FileNameA))
            {
                Rate -= 40;
                Apply = false;
            }
        }
    }
}