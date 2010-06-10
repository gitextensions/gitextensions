using System;
using System.Collections.Generic;

using System.Text;
using System.IO;

namespace GitCommands
{
    public class ChangedFile
    {
        public ChangedFile()
        {
            DeleteFile = false;
            CreateFile = false;
            PatchText = "";
        }

        public string PatchText { get; set; }
        public bool DeleteFile { get; set; }
        public bool CreateFile { get; set; }

        public string Description
        {
            get
            {
                string description = "";
                if (DeleteFile)
                    description += "- ";
                if (CreateFile)
                    description += "+ ";
                description += FileName;
                return description;
            }
        }

        public string Old { get; set; }
        public string New { get; set; }
        public string FileName { get; set; }

        public int Reliability { get; set; }
        public string Comments { get; set; }
/*
        private List<ChangedFileChange> changes = new List<ChangedFileChange>();

        public List<ChangedFileChange> Changes
        {
            get
            {
                return changes;
            }
        }
        */
    }
}
