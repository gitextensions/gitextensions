using System.Collections.Generic;
using System.Drawing;
using GitCommands;
using GitUI.Properties;

namespace GitUI.UserControls
{
    internal interface IRepoStateVisualiser
    {
        (Image, Brush) Invoke(IReadOnlyList<GitItemStatus> allChangedFiles);
    }

    internal sealed class RepoStateVisualiser : IRepoStateVisualiser
    {
        // Images properties allocate on each call, so cache our images.

        internal static readonly Bitmap IconClean = Images.RepoStateClean;
        internal static readonly Bitmap IconDirty = Images.RepoStateDirty;
        internal static readonly Bitmap IconDirtySubmodules = Images.RepoStateDirtySubmodules;
        internal static readonly Bitmap IconStaged = Images.RepoStateStaged;
        internal static readonly Bitmap IconMixed = Images.RepoStateMixed;
        internal static readonly Bitmap IconUntrackedOnly = Images.RepoStateUntrackedOnly;
        internal static readonly Brush BrushClean = Brushes.Green;
        internal static readonly Brush BrushDirty = Brushes.Red;
        internal static readonly Brush BrushDirtySubmodules = Brushes.Orange;
        internal static readonly Brush BrushStaged = Brushes.Blue;
        internal static readonly Brush BrushMixed = Brushes.Yellow;
        internal static readonly Brush BrushUntrackedOnly = Brushes.Purple;

        public (Image, Brush) Invoke(IReadOnlyList<GitItemStatus> allChangedFiles)
        {
            var stagedCount = 0;
            var unstagedSubmodulesCount = 0;
            var notTrackedCount = 0;

            foreach (var status in allChangedFiles)
            {
                if (status.Staged == StagedStatus.Index)
                {
                    stagedCount++;
                }

                if (status.Staged == StagedStatus.WorkTree && status.IsSubmodule)
                {
                    unstagedSubmodulesCount++;
                }

                if (!status.IsTracked)
                {
                    notTrackedCount++;
                }
            }

            var unstagedCount = allChangedFiles.Count - stagedCount;

            if (stagedCount == 0 && unstagedCount == 0)
            {
                return (IconClean, BrushClean);
            }

            if (stagedCount == 0)
            {
                if (notTrackedCount == unstagedCount)
                {
                    return (IconUntrackedOnly, BrushUntrackedOnly);
                }

                return unstagedCount != unstagedSubmodulesCount
                    ? (IconDirty, BrushDirty)
                    : (IconDirtySubmodules, BrushDirtySubmodules);
            }

            return unstagedCount == 0
                ? (IconStaged, BrushStaged)
                : (IconMixed, BrushMixed);
        }
    }
}