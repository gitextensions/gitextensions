﻿namespace GitUI
{
    public sealed partial class RevisionGridControl
    {
        internal enum Command
        {
            ToggleRevisionGraph = 0,
            RevisionFilter = 1,
            ToggleAuthorDateCommitDate = 2,
            ToggleOrderRevisionsByDate = 3,
            ToggleShowRelativeDate = 4,
            ToggleDrawNonRelativesGray = 5,
            ToggleShowGitNotes = 6,
            //// <snip>
            ToggleShowMergeCommits = 8,
            ShowAllBranches = 9,
            ShowCurrentBranchOnly = 10,
            ShowFilteredBranches = 11,
            ShowRemoteBranches = 12,
            ShowFirstParent = 13,
            GoToParent = 14,
            GoToChild = 15,
            ToggleHighlightSelectedBranch = 16,
            NextQuickSearch = 17,
            PrevQuickSearch = 18,
            SelectCurrentRevision = 19,
            GoToCommit = 20,
            NavigateBackward = 21,
            NavigateForward = 22,
            SelectAsBaseToCompare = 23,
            CompareToBase = 24,
            CreateFixupCommit = 25,
            ToggleShowTags = 26,
            CompareToWorkingDirectory = 27,
            CompareToCurrentBranch = 28,
            CompareToBranch = 29,
            CompareSelectedCommits = 30,
            GoToMergeBase = 31,
            OpenCommitsWithDifftool = 32,
            ToggleBetweenArtificialAndHeadCommits = 33,
            ShowReflogReferences = 34,
            ShowStashes = 35,
            ResetRevisionFilter = 36,
            ResetRevisionPathFilter = 37,
            SelectNextForkPointAsDiffBase = 38,
            NavigateBackward_AlternativeHotkey = 39,
            NavigateForward_AlternativeHotkey = 40,
            DeleteRef = 41,
            RenameRef = 42,
            CreateSquashCommit = 43,
            CreateAmendCommit = 44,
        }
    }
}
