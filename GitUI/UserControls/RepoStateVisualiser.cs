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

        internal static readonly (Bitmap, Brush) Clean = (Images.RepoStateClean, Brushes.Lime);
        internal static readonly (Bitmap, Brush) Dirty = (Images.RepoStateDirty, Brushes.LightSalmon);
        internal static readonly (Bitmap, Brush) DirtySubmodules = (Images.RepoStateDirtySubmodules, Brushes.Orange);
        internal static readonly (Bitmap, Brush) Mixed = (Images.RepoStateMixed, Brushes.Yellow);
        internal static readonly (Bitmap, Brush) Staged = (Images.RepoStateStaged, Brushes.LightSkyBlue);
        internal static readonly (Bitmap, Brush) Unknown = (Images.RepoStateUnknown, Brushes.Gray);
        internal static readonly (Bitmap, Brush) UntrackedOnly = (Images.RepoStateUntrackedOnly, Brushes.BlueViolet);

        public (Image, Brush) Invoke(IReadOnlyList<GitItemStatus> allChangedFiles)
        {
            if (allChangedFiles == null)
            {
                return Unknown;
            }

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
                return Clean;
            }

            if (indexCount == 0)
            {
                if (notTrackedCount == workTreeCount)
                {
                    return UntrackedOnly;
                }

                return workTreeCount != workTreeSubmodulesCount ? Dirty : DirtySubmodules;
            }

            return workTreeCount == 0 ? Staged : Mixed;
        }
    }
}