﻿#nullable enable

using System.Text;
using Microsoft;
using Microsoft.VisualStudio.Threading;

namespace GitExtensions.Extensibility.Git;

/// <summary>
/// Status if the file can be staged (worktree->index), unstaged or None (normal commits).
/// The status may not be available or unset for some commands.
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
        IsUnmerged = 1 << 8,
        IsAssumeUnchanged = 1 << 9,
        IsSkipWorktree = 1 << 10,
        IsSubmodule = 1 << 11,
        IsDirty = 1 << 12,

        // Other flags are parsed from Git status, set fake flags for special uses
        IsStatusOnly = 1 << 13,
        IsRangeDiff = 1 << 14
    }

    private JoinableTask<GitSubmoduleStatus?>? _submoduleStatus;

    private Flags _flags;

    public GitItemStatus(string name)
    {
        Requires.NotNull(name, nameof(name));
        Name = name;

        int pathEndIndex = GetPathEndIndex(Name);
        Path = RelativePath.From(pathEndIndex >= 1 ? Name[..pathEndIndex] : "");

        return;

        static int GetPathEndIndex(string name)
        {
            if (name.Length == 0)
            {
                return 0;
            }

            int lastIndex = name.Length - 1;
            int startIndex = name[lastIndex] == '/' ? lastIndex - 1 : lastIndex;
            return name.LastIndexOf('/', startIndex);
        }
    }

    /// <summary>
    /// Get a default object for an item unchanged in the WorkTree.
    /// </summary>
    /// <param name="name">The file name for the item.</param>
    /// <returns>The default GitItemStatus object.</returns>
    public static GitItemStatus GetDefaultStatus(string name)
    {
        return GitItemStatusConverter.FromStatusCharacter(StagedStatus.WorkTree, name, GitItemStatusConverter.UnusedCharacter);
    }

    public string Name { get; set; }
    public string? OldName { get; set; }
    public RelativePath Path { get; init; }
    public string? ErrorMessage { get; set; }
    public ObjectId? TreeGuid { get; set; }
    public string? RenameCopyPercentage { get; set; }

    public StagedStatus Staged { get; set; }
    public DiffBranchStatus DiffStatus { get; set; } = DiffBranchStatus.Unknown;

    /// <summary>
    /// The git-grep arguments used to match this file.
    /// </summary>
    public string? GrepString { get; set; }

    #region Flags

    public bool IsTracked
    {
        get => HasFlag(Flags.IsTracked);
        set => SetFlag(value, Flags.IsTracked);
    }

    public bool IsDeleted
    {
        get => HasFlag(Flags.IsDeleted);
        set => SetFlag(value, Flags.IsDeleted);
    }

    /// <summary>
    /// For files, the file is modified
    /// For submodules, the commit is changed.
    /// </summary>
    public bool IsChanged
    {
        get => HasFlag(Flags.IsChanged);
        set => SetFlag(value, Flags.IsChanged);
    }

    public bool IsNew
    {
        get => HasFlag(Flags.IsNew);
        set => SetFlag(value, Flags.IsNew);
    }

    public bool IsIgnored
    {
        get => HasFlag(Flags.IsIgnored);
        set => SetFlag(value, Flags.IsIgnored);
    }

    public bool IsRenamed
    {
        get => HasFlag(Flags.IsRenamed);
        set => SetFlag(value, Flags.IsRenamed);
    }

    public bool IsCopied
    {
        get => HasFlag(Flags.IsCopied);
        set => SetFlag(value, Flags.IsCopied);
    }

    public bool IsUnmerged
    {
        get => HasFlag(Flags.IsUnmerged);
        set => SetFlag(value, Flags.IsUnmerged);
    }

    // Flags below are not set from git-status parsing, but from other sources
    // (IsSubmodule and IsDirty can be parsed in git-status porcelain=2 mode)

    public bool IsAssumeUnchanged
    {
        get => HasFlag(Flags.IsAssumeUnchanged);
        set => SetFlag(value, Flags.IsAssumeUnchanged);
    }

    public bool IsSkipWorktree
    {
        get => HasFlag(Flags.IsSkipWorktree);
        set => SetFlag(value, Flags.IsSkipWorktree);
    }

    public bool IsSubmodule
    {
        get => HasFlag(Flags.IsSubmodule);
        set => SetFlag(value, Flags.IsSubmodule);
    }

    /// <summary>
    /// Submodule is dirty
    /// Info from git-status, may be available before GetSubmoduleStatusAsync is evaluated.
    /// </summary>
    public bool IsDirty
    {
        get => HasFlag(Flags.IsDirty);
        set => SetFlag(value, Flags.IsDirty);
    }

    /// <remarks>
    /// This item is not a Git item, just status information
    /// If ErrorMessage is set, this is an error from Git, otherwise just a marker that nothing is changed.
    /// </remarks>
    public bool IsStatusOnly
    {
        get => HasFlag(Flags.IsStatusOnly);
        set => SetFlag(value, Flags.IsStatusOnly);
    }

    /// <remarks>
    /// This item is not a native git item, but a status information
    /// calculated with git range-diff command.
    /// </remarks>
    public bool IsRangeDiff
    {
        get => HasFlag(Flags.IsRangeDiff);
        set => SetFlag(value, Flags.IsRangeDiff);
    }

    private bool HasFlag(Flags flags)
    {
        // NOTE Enum.HasFlag boxes its argument
        return (flags & _flags) == flags;
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

    #region Derived Flags

    /// <summary>
    /// Indicates whether the Git item was added in the (artificial or real) commit.
    /// </summary>
    public bool IsAdded
        => IsNew || IsCopied;

    /// <summary>
    /// Indicates whether the Git item is yet to be committed;
    /// that is it belongs to either WorkTree or Index (<see cref="Staged"/> must be set).
    /// </summary>
    public bool IsUncommitted
        => Staged is StagedStatus.WorkTree or StagedStatus.Index;

    /// <summary>
    /// Indicates whether the Git item is new or copied (<see cref="IsAdded"/>) and has not been committed yet (<see cref="IsUncommitted"/>).
    /// </summary>
    public bool IsUncommittedAdded
        => IsUncommitted && IsAdded;

    #endregion

    /// <summary>
    /// Gets a task whose result is the submodule status.
    /// </summary>
    /// <returns>
    /// A null task when <see cref="SetSubmoduleStatus"/> has not been called on this object, or
    /// a task whose result is the status. The task may also return null if the status could not be
    /// determined.
    /// </returns>
    public Task<GitSubmoduleStatus?> GetSubmoduleStatusAsync()
    {
        if (_submoduleStatus is null)
        {
            return Task.FromResult((GitSubmoduleStatus?)null);
        }

        return _submoduleStatus.JoinAsync();
    }

    public void SetSubmoduleStatus(JoinableTask<GitSubmoduleStatus?> status)
    {
        _submoduleStatus = status;
    }

    /// <summary>
    /// Return an object with the status as if the item was created
    /// with first and second commit reverse selected.
    /// </summary>
    /// <returns>An inverted copy of the status.</returns>
    public GitItemStatus InvertStatus()
    {
        GitItemStatus gitItemStatus = new(Name)
        {
            Name = IsRenamed ? (OldName ?? string.Empty) : Name,
            OldName = IsRenamed ? Name : OldName,
            ErrorMessage = ErrorMessage,
            TreeGuid = TreeGuid,
            RenameCopyPercentage = RenameCopyPercentage,
            Staged = Staged,
            DiffStatus = DiffStatus,
            _flags = _flags,
            IsNew = IsDeleted,
            IsDeleted = IsNew
        };

        return gitItemStatus;
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
        StringBuilder str = new();

        if (!string.IsNullOrWhiteSpace(ErrorMessage))
        {
            str.Append(ErrorMessage);
        }

        if (IsRenamed)
        {
            str.Append("Renamed\n   ").Append(OldName).Append("\n to\n   ").Append(Name);
        }
        else if (IsCopied)
        {
            str.Append("Copied\n   ").Append(OldName).Append("\n to\n   ").Append(Name);
        }
        else
        {
            str.Append(Name);
        }

        if (IsUnmerged)
        {
            str.Append(" (Unmerged)");
        }

        if (Staged is not (StagedStatus.None or StagedStatus.Unset))
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
