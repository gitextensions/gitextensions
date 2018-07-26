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
            var indexCount = 0;
            var workTreeSubmodulesCount = 0;
            var notTrackedCount = 0;

            foreach (var status in allChangedFiles)
            {
                if (status.Staged == StagedStatus.Index)
                {
                    indexCount++;
                }

                if (status.Staged == StagedStatus.WorkTree && status.IsSubmodule)
                {
                    workTreeSubmodulesCount++;
                }

                if (!status.IsTracked)
                {
                    notTrackedCount++;
                }
            }

            var workTreeCount = allChangedFiles.Count - indexCount;

            if (indexCount == 0 && workTreeCount == 0)
            {
                return (IconClean, BrushClean);
            }

            if (indexCount == 0)
            {
                if (notTrackedCount == workTreeCount)
                {
                    return (IconUntrackedOnly, BrushUntrackedOnly);
                }

                return workTreeCount != workTreeSubmodulesCount
                    ? (IconDirty, BrushDirty)
                    : (IconDirtySubmodules, BrushDirtySubmodules);
            }

            return workTreeCount == 0
                ? (IconStaged, BrushStaged)
                : (IconMixed, BrushMixed);
        }
    }
}