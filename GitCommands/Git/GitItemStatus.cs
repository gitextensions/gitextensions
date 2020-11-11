﻿using System;
using System.Text;
using System.Threading.Tasks;
using GitExtensions.Core.Module;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Threading;

namespace GitCommands
{
    /// <summary>
    /// Status if the file can be staged (worktree->index), unstaged or None (normal commits)
    /// The status may not be available or unset for some commands
    /// </summary>
    public enum StagedStatus
    {
        Unset = 0,
        None,
        WorkTree,
        Index,
        Unknown
    }

    public sealed class GitItemStatus
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
            IsDirty = 1 << 12,

            // Other flags are parsed from Git status, set fake flags for special uses
            IsStatusOnly = 1 << 13,
            IsRangeDiff = 1 << 14
        }

        private JoinableTask<GitSubmoduleStatus> _submoduleStatus;

        private Flags _flags;

        public string Name { get; set; }
        public string OldName { get; set; }
        public string ErrorMessage { get; set; }
        [CanBeNull]
        public ObjectId TreeGuid { get; set; }
        public string RenameCopyPercentage { get; set; }

        public StagedStatus Staged { get; set; }

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

        /// <summary>
        /// For files, the file is modified
        /// For submodules, the commit is changed
        /// </summary>
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

        /// <summary>
        /// Submodule is dirty
        /// Info from git-status, may be available before GetSubmoduleStatusAsync is evaluated
        /// </summary>
        public bool IsDirty
        {
            get => _flags.HasFlag(Flags.IsDirty);
            set => SetFlag(value, Flags.IsDirty);
        }

        /// <remarks>
        /// This item is not a Git item, just status information
        /// If ErrorMessage is set, this is an error from Git, otherwise just a marker that nothing is changed
        /// </remarks>
        public bool IsStatusOnly
        {
            get => _flags.HasFlag(Flags.IsStatusOnly);
            set => SetFlag(value, Flags.IsStatusOnly);
        }

        /// <remarks>
        /// This item is not a native git item, but a status information
        /// calculated with git range-diff command.
        /// </remarks>
        public bool IsRangeDiff
        {
            get => _flags.HasFlag(Flags.IsRangeDiff);
            set => SetFlag(value, Flags.IsRangeDiff);
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

        public int CompareName(GitItemStatus other)
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

            if (!string.IsNullOrWhiteSpace(ErrorMessage))
            {
                str.Append(ErrorMessage);
            }

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

            if (Staged != StagedStatus.None && Staged != StagedStatus.Unset)
            {
                str.Append($" {Staged}");
            }

            if (!string.IsNullOrEmpty(RenameCopyPercentage))
            {
                str.Append("\nSimilarity ").Append(RenameCopyPercentage).Append('%');
            }

            return str.ToString();
        }
    }
}
