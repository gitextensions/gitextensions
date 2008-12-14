using System;
using System.Collections.Generic;
using System.Text;

namespace PatchApply
{
    public class Patch
    {
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
        public string Text { get; set; }
        public string FileNameA { get; set; }
        public string FileNameB { get; set; }
        public string FileTextB { get; set; }
        public int Rate { get; set; }
        public bool Apply { get; set; }

        public List<int> BookMarks { get; set; }

        public PatchType Type { get; set; }
    }
}