using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace PatchApply
{
    public class PatchFile
    {
        private string fullName;
        public string FullName
        {
            get
            {
                return fullName;
            }
            set
            {
                fullName = value;
            }
        }
        private string name;
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }
        private string author;
        public string Author
        {
            get
            {
                return author;
            }
            set
            {
                author = value;
            }
        }
        private string subject;
        public string Subject
        {
            get
            {
                return subject;
            }
            set
            {
                subject = value;
            }
        }
        private string date;
        public string Date
        {
            get
            {
                return date;
            }
            set
            {
                date = value;
            }
        }

        public string Status
        {
            get
            {
                if (IsNext)
                {
                    return "Next to apply";
                }
                if (IsSkipped)
                {
                    return "Skipped";
                }
                if (!File.Exists(FullName))
                {
                    return "Applied";
                }
                return "";
            }
        }

        private bool isNext;
        public bool IsNext
        {
            get
            {
                return isNext;
            }
            set
            {
                isNext = value;
            }
        }
        private bool isSkipped;
        public bool IsSkipped
        {
            get
            {
                return isSkipped;
            }
            set
            {
                isSkipped = value;
            }
        }  
    }
}
