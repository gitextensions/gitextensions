using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Threading;

namespace GitCommands
{
    public enum StagedStatus
    {
        Unknown = 0,
        None,
        WorkTree,
        Index
    }

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
        public bool IsSubmodule { get; set; }
        public string RenameCopyPercentage { get; set; }

        // Staged is three state and has no default status
        private StagedStatus _staged = StagedStatus.Unknown;
        public StagedStatus Staged
        {
            get
            {
                // Catch usage of unset accesses
                Debug.Assert(_staged != StagedStatus.Unknown, "Staged is used without being set. Continue should generally be OK.");

                return _staged;
            }
            set
            {
                _staged = value;
            }
        }

        [CanBeNull]
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
            int value = StringComparer.InvariantCulture.Compare(Name, other.Name);

            if (value == 0)
            {
                value = StringComparer.InvariantCulture.Compare(OldName, other.OldName);
            }

            return value;
        }

        public override string ToString()
        {
            var str = new StringBuilder();

            if (IsRenamed)
            {
                str.Append("Renamed\n   ").Append(OldName).Append("\nto\n   ").Append(Name);
            }
            else if (IsCopied)
            {
                str.Append("Copied\n   ").Append(OldName).Append("\nto\n   ").Append(Name);
            }
            else
            {
                str.Append(Name);
            }

            if (IsConflict)
            {
                str.Append(" (Conflict)");
            }

            if (!string.IsNullOrEmpty(RenameCopyPercentage))
            {
                str.Append("\nSimilarity ").Append(RenameCopyPercentage).Append('%');
            }

            return str.ToString();
        }
    }
}
