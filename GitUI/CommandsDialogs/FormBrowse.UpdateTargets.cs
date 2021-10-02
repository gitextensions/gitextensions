using System;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormBrowse
    {
        [Flags]
        private enum UpdateTargets
        {
            None = 1,
            DiffList = 2,
            FileTree = 4,
            CommitInfo = 8
        }
    }
}
