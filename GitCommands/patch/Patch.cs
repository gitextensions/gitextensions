using System;
using System.Collections.Generic;
using System.Text;

namespace PatchApply
{
    public class Patch
    {
        private StringBuilder textBuilder;

        public Patch()
        {
            Apply = true;
            BookMarks = new List<int>();
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
        public int Rate { get; set; }
        public bool Apply { get; set; }

        public List<int> BookMarks { get; set; }

        public PatchType Type { get; set; }

        public string Text
        { 
            get { return textBuilder == null ? null : textBuilder.ToString(); }
        }

        public void AppendText(string text)
        {
            GetTextBuilder().Append(text);
        }

        public void AppendTextLine(string line)
        {
            GetTextBuilder().Append(line).Append('\n');
        }

        private StringBuilder GetTextBuilder()
        {
            return textBuilder ?? (textBuilder = new StringBuilder());
        }
    }
}