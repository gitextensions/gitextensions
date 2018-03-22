using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Threading;

namespace GitCommands
{
    public class GitItemStatus : IComparable<GitItemStatus>
    {
        private JoinableTask<GitSubmoduleStatus> _submoduleStatus;

        public string Name { get; set; }
        public string OldName { get; set; }
        public string TreeGuid { get; set; }
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
        public bool IsStaged { get; set; } = true;
        public bool IsSubmodule { get; set; }
        public string RenameCopyPercentage { get; set; }

        public Task<GitSubmoduleStatus> GetSubmoduleStatusAsync()
        {
            return _submoduleStatus?.JoinAsync();
        }

        internal void SetSubmoduleStatus(JoinableTask<GitSubmoduleStatus> status)
        {
            _submoduleStatus = status;
        }

        public int CompareTo(GitItemStatus other)
        {
            int value = (Name ?? "").CompareTo(other.Name ?? "");
            if (value == 0)
            {
                value = (OldName ?? "").CompareTo(other.OldName ?? "");
            }

            return value;
        }

        public override string ToString()
        {
            string toString;
            if (IsRenamed)
            {
                toString = string.Concat("Renamed\n   ", OldName, "\nto\n   ", Name, "");
            }
            else if (IsCopied)
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
            {
                toString += string.Concat("\nSimilarity ", RenameCopyPercentage, "%");
            }

            return toString;
        }
    }
}
