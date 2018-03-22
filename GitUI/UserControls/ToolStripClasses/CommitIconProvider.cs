using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using GitCommands;

namespace GitUI.UserControls.ToolStripClasses
{
    internal class CommitIconProvider : ICommitIconProvider
    {
        internal static readonly Bitmap IconClean = Properties.Resources.IconClean;

        internal static readonly Bitmap IconDirty = Properties.Resources.IconDirty;

        internal static readonly Bitmap IconDirtySubmodules = Properties.Resources.IconDirtySubmodules;

        internal static readonly Bitmap IconStaged = Properties.Resources.IconStaged;

        internal static readonly Bitmap IconMixed = Properties.Resources.IconMixed;

        internal static readonly Bitmap IconUntrackedOnly = Properties.Resources.IconUntrackedOnly;

        public Image GetCommitIcon(IReadOnlyList<GitItemStatus> allChangedFiles)
        {
            var stagedCount = allChangedFiles.Count(status => status.IsStaged);
            var unstagedCount = allChangedFiles.Count - stagedCount;
            var unstagedSubmodulesCount = allChangedFiles.Count(status => status.IsSubmodule && !status.IsStaged);
            var notTrackedCount = allChangedFiles.Count(status => !status.IsTracked);

            return GetStatusIcon(stagedCount, unstagedCount, unstagedSubmodulesCount, notTrackedCount);
        }

        private static Image GetStatusIcon(
            int stagedCount,
            int unstagedCount,
            int unstagedSubmodulesCount,
            int notTrackedCount)
        {
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

    internal interface ICommitIconProvider
    {
        Image GetCommitIcon(IReadOnlyList<GitItemStatus> allChangedFiles);
    }
}