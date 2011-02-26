using System;
using System.Collections.Generic;

using System.Text;

namespace GitCommands
{
    public class GitItemStatus
    {
        public GitItemStatus()
        {
            IsTracked = false;
            IsDeleted = false;
            IsChanged = false;
            IsRenamed = false;
            IsCopied = false;
            IsConflict = false;
            IsNew = false;
            IsStaged = true;
        }

        public string Name { get; set; }
        public string OldName { get; set; }

        public string ChangeString
        {
            get
            {
                if (!IsTracked) return "Untracked";
                else if (IsDeleted) return "Deleted";
                else if (IsChanged) return "Modified";
                else if (IsNew) return "New";
                else if (IsRenamed) return "Renamed";
                else if (IsConflict) return "Conflict";
                else if (IsCopied) return "Copied";
                else return "";
            }
        }

        public bool IsTracked { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsChanged { get; set; }
        public bool IsNew { get; set; }
        public bool IsRenamed { get; set; }
        public bool IsCopied { get; set; }
        public bool IsConflict { get; set; }
        public bool IsStaged { get; set; }
        public string RenameCopyPercentage { get; set; }

        public override string ToString()
        {
            string toString = string.Empty;
            if (IsRenamed)
            {
                toString = string.Concat("Renamed\n   ", OldName, "\nto\n   ", Name, "");
            }
            else
                if (IsCopied)
                {
                    toString = string.Concat("Copied\n   ", OldName, "\nto\n   ", Name, "");
                }
                else
                {
                    toString = Name;
                }



            if (IsConflict)
            {
                toString += " (Conflict)";
            }

            if (!string.IsNullOrEmpty(RenameCopyPercentage))
                toString += string.Concat("\nSimilarity ", RenameCopyPercentage, "%");

            return toString;
        }
    }
}
