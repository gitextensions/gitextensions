using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using GitCommands;

namespace GitUI.UserControls.ToolStripClasses
{
    internal class CommitIconProvider : ICommitIconProvider
    {
        public static readonly Bitmap IconClean = Properties.Resources.IconClean;

        public static readonly Bitmap IconDirty = Properties.Resources.IconDirty;

        public static readonly Bitmap IconDirtySubmodules = Properties.Resources.IconDirtySubmodules;

        public static readonly Bitmap IconStaged = Properties.Resources.IconStaged;

        public static readonly Bitmap IconMixed = Properties.Resources.IconMixed;

        public static readonly Bitmap IconUntrackedOnly = Properties.Resources.IconUntrackedOnly;

        public Image DefaultIcon
        {
            get { return IconClean; }
        }

        public Image GetCommitIcon(IList<GitItemStatus> allChangedFiles)
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
                return IconClean;

            if (stagedCount == 0)
            {
                if (notTrackedCount == unstagedCount)
                    return IconUntrackedOnly;

                return unstagedCount != unstagedSubmodulesCount ? IconDirty : IconDirtySubmodules;
            }

            return unstagedCount == 0 ? IconStaged : IconMixed;
        }
    }

    internal interface ICommitIconProvider
    {
        Image DefaultIcon { get; }
        Image GetCommitIcon(IList<GitItemStatus> allChangedFiles);
    }
}