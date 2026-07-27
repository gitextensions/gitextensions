using Avalonia.Media;
using GitExtensions.Extensibility.Git;
using GitUI.Properties;

namespace GitUI.UserControls;

internal interface IRepoStateVisualiser
{
    (IImage image, IBrush brush) Invoke(IReadOnlyList<GitItemStatus>? allChangedFiles);
}

internal sealed class RepoStateVisualiser : IRepoStateVisualiser
{
    internal static readonly (IImage, IBrush) Clean = (Images.RepoStateClean, Brushes.Lime);
    internal static readonly (IImage, IBrush) Dirty = (Images.RepoStateDirty, Brushes.LightSalmon);
    internal static readonly (IImage, IBrush) DirtySubmodules = (Images.RepoStateDirtySubmodules, Brushes.Orange);
    internal static readonly (IImage, IBrush) Mixed = (Images.RepoStateMixed, Brushes.Yellow);
    internal static readonly (IImage, IBrush) Staged = (Images.RepoStateStaged, Brushes.LightSkyBlue);
    internal static readonly (IImage, IBrush) Unknown = (Images.RepoStateUnknown, Brushes.Gray);
    internal static readonly (IImage, IBrush) UntrackedOnly = (Images.RepoStateUntrackedOnly, Brushes.BlueViolet);

    public (IImage image, IBrush brush) Invoke(IReadOnlyList<GitItemStatus>? allChangedFiles)
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
