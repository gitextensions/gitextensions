using System;
using System.Collections.Generic;
using System.Text;

namespace FileHashShell
{
    class FileInformation
    {
        string name = string.Empty;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        string path = string.Empty;

        public string Path
        {
            get { return path; }
            set { path = value; }
        }
        string hash = string.Empty;

        public string Hash
        {
            get { return hash; }
            set { hash = value; }
        }
        private DateTime fileDate;

        public DateTime FileDate
        {
            get { return fileDate; }
            set { fileDate = value; }
        }

        Int64 fileSize = 0;
        public Int64 FileSize
        {
            get { return fileSize; }
            set { fileSize = value; }
        }
    }
}
