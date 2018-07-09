using System.Collections.Generic;
using System.Drawing;
using GitCommands;
using GitUI.Properties;

namespace GitUI.UserControls.ToolStripClasses
{
    internal class CommitIconProvider : ICommitIconProvider
    {
        // Images properties allocate on each call, so cache our images.

        internal static readonly Bitmap IconClean = Images.RepoStateClean;
        internal static readonly Bitmap IconDirty = Images.RepoStateDirty;
        internal static readonly Bitmap IconDirtySubmodules = Images.RepoStateDirtySubmodules;
        internal static readonly Bitmap IconStaged = Images.RepoStateStaged;
        internal static readonly Bitmap IconMixed = Images.RepoStateMixed;
        internal static readonly Bitmap IconUntrackedOnly = Images.RepoStateUntrackedOnly;

        public Image GetCommitIcon(IReadOnlyList<GitItemStatus> allChangedFiles)
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

            return GetStatusIcon();

            Image GetStatusIcon()
            {
                var unstagedCount = allChangedFiles.Count - stagedCount;

                if (stagedCount == 0 && unstagedCount == 0)
                {
                    return IconClean;
                }

                if (stagedCount == 0)
                {
                    if (notTrackedCount == unstagedCount)
                    {
                        return IconUntrackedOnly;
                    }

                    return unstagedCount != unstagedSubmodulesCount ? IconDirty : IconDirtySubmodules;
                }

                return unstagedCount == 0 ? IconStaged : IconMixed;
            }
        }
    }

    internal interface ICommitIconProvider
    {
        Image GetCommitIcon(IReadOnlyList<GitItemStatus> allChangedFiles);
    }
}