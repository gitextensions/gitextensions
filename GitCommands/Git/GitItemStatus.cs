using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using GitUIPluginInterfaces;
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

    public sealed class GitItemStatus : IComparable<GitItemStatus>
    {
        [Flags]
        private enum Flags
        {
            IsTracked = 1 << 1,
            IsDeleted = 1 << 2,
            IsChanged = 1 << 3,
            IsNew = 1 << 4,
            IsIgnored = 1 << 5,
            IsRenamed = 1 << 6,
            IsCopied = 1 << 7,
            IsConflict = 1 << 8,
            IsAssumeUnchanged = 1 << 9,
            IsSkipWorktree = 1 << 10,
            IsSubmodule = 1 << 11,
        }

        private JoinableTask<GitSubmoduleStatus> _submoduleStatus;

        private Flags _flags;

        public string Name { get; set; }
        public string OldName { get; set; }
        [CanBeNull]
        public ObjectId TreeGuid { get; set; }
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
            set { _staged = value; }
        }

        #region Flags

        public bool IsTracked
        {
            get => _flags.HasFlag(Flags.IsTracked);
            set => SetFlag(value, Flags.IsTracked);
        }

        public bool IsDeleted
        {
            get => _flags.HasFlag(Flags.IsDeleted);
            set => SetFlag(value, Flags.IsDeleted);
        }

        public bool IsChanged
        {
            get => _flags.HasFlag(Flags.IsChanged);
            set => SetFlag(value, Flags.IsChanged);
        }

        public bool IsNew
        {
            get => _flags.HasFlag(Flags.IsNew);
            set => SetFlag(value, Flags.IsNew);
        }

        public bool IsIgnored
        {
            get => _flags.HasFlag(Flags.IsIgnored);
            set => SetFlag(value, Flags.IsIgnored);
        }

        public bool IsRenamed
        {
            get => _flags.HasFlag(Flags.IsRenamed);
            set => SetFlag(value, Flags.IsRenamed);
        }

        public bool IsCopied
        {
            get => _flags.HasFlag(Flags.IsCopied);
            set => SetFlag(value, Flags.IsCopied);
        }

        public bool IsConflict
        {
            get => _flags.HasFlag(Flags.IsConflict);
            set => SetFlag(value, Flags.IsConflict);
        }

        public bool IsAssumeUnchanged
        {
            get => _flags.HasFlag(Flags.IsAssumeUnchanged);
            set => SetFlag(value, Flags.IsAssumeUnchanged);
        }

        public bool IsSkipWorktree
        {
            get => _flags.HasFlag(Flags.IsSkipWorktree);
            set => SetFlag(value, Flags.IsSkipWorktree);
        }

        public bool IsSubmodule
        {
            get => _flags.HasFlag(Flags.IsSubmodule);
            set => SetFlag(value, Flags.IsSubmodule);
        }

        private void SetFlag(bool isSet, Flags flag)
        {
            if (isSet)
            {
                _flags |= flag;
            }
            else
            {
                _flags &= ~flag;
            }
        }

        #endregion

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
