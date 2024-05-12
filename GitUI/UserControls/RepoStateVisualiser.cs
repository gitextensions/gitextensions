using GitExtensions.Extensibility.Git;
using GitUI.Properties;

namespace GitUI.UserControls
{
    internal interface IRepoStateVisualiser
    {
        (Image image, Brush brush) Invoke(IReadOnlyList<GitItemStatus>? allChangedFiles);
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

        public (Image image, Brush brush) Invoke(IReadOnlyList<GitItemStatus>? allChangedFiles)
        {
            if (allChangedFiles is null)
            {
                return Unknown;
            }

            int indexCount = 0;
            int workTreeSubmodulesCount = 0;
            int notTrackedCount = 0;

            foreach (GitItemStatus status in allChangedFiles)
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

            int workTreeCount = allChangedFiles.Count - indexCount;

            return (indexCount, workTreeCount) switch
            {
                (0, 0) => Clean,
                (0, _) when workTreeCount == notTrackedCount => UntrackedOnly,
                (0, _) when workTreeCount != workTreeSubmodulesCount => Dirty,
                (0, _) => DirtySubmodules,
                (_, 0) => Staged,
                (_, _) => Mixed
            };
        }
    }
}
