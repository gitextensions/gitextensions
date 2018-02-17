﻿using System;
using System.Threading.Tasks;

namespace GitCommands
{
    public class GitItemStatus : IComparable<GitItemStatus>
    {
        public GitItemStatus()
        {
            IsTracked = false;
            IsDeleted = false;
            IsChanged = false;
            IsRenamed = false;
            IsCopied = false;
            IsConflict = false;
            IsAssumeUnchanged = false;
            IsSkipWorktree = false;
            IsNew = false;
            IsIgnored = false;
            IsStaged = true;
            IsSubmodule = false;
        }

        public string Name { get; set; }
        public string OldName { get; set; }
        public string TreeGuid { get; set; }

        public string ChangeString
        {
            get
            {
                if (!IsIgnored) return "Ignored";
                if (!IsTracked) return "Untracked";
                if (IsDeleted) return "Deleted";
                if (IsChanged) return "Modified";
                if (IsNew) return "New";
                if (IsRenamed) return "Renamed";
                if (IsConflict) return "Conflict";
                if (IsCopied) return "Copied";
                if (IsAssumeUnchanged) return "Unchanged";
                if (IsSkipWorktree) return "SkipWorktree";
                return "";
            }
        }

        public bool IsTracked { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsChanged { get; set; }
        public bool IsNew { get; set; }
        public bool IsIgnored { get; set; }
        public bool IsRenamed { get; set; }
        public bool IsCopied { get; set; }
        public bool IsConflict { get; set; }
        public bool IsAssumeUnchanged { get; set; }
        public bool IsSkipWorktree { get; set; }
        public bool IsStaged { get; set; }
        public bool IsSubmodule { get; set; }
        public string RenameCopyPercentage { get; set; }

        public Task<GitSubmoduleStatus> SubmoduleStatus { get; set; }

        public int CompareTo(GitItemStatus other)
        {
            int value = (Name ?? "").CompareTo(other.Name ?? "");
            if (value == 0)
                value = (OldName ?? "").CompareTo(other.OldName ?? "");
            return value;
        }

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
