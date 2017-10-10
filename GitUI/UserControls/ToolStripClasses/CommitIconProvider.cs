using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using GitCommands;

namespace GitUI.UserControls.ToolStripClasses
{
    internal class CommitIconProvider : ICommitIconProvider
    {
        private static Bitmap IconClean
        {
            get { return Properties.Resources.IconClean; }
        }

        private static Bitmap IconDirty
        {
            get { return Properties.Resources.IconDirty; }
        }

        private static Bitmap IconDirtySubmodules
        {
            get { return Properties.Resources.IconDirtySubmodules; }
        }

        private static Bitmap IconStaged
        {
            get { return Properties.Resources.IconStaged; }
        }

        private static Bitmap IconMixed
        {
            get { return Properties.Resources.IconMixed; }
        }

        private static Bitmap IconUntrackedOnly
        {
            get { return Properties.Resources.IconUntrackedOnly; }
        }

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

        private static Image GetStatusIcon(int stagedCount, int unstagedCount, int unstagedSubmodulesCount,
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
}