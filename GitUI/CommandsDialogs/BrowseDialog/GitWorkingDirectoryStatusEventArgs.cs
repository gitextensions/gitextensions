using System;
using System.Collections.Generic;
using GitCommands;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    public class GitWorkingDirectoryStatusEventArgs : EventArgs
    {
        private readonly IReadOnlyList<GitItemStatus> _itemStatuses = Array.Empty<GitItemStatus>();

        public GitWorkingDirectoryStatusEventArgs(IReadOnlyList<GitItemStatus> itemStatuses)
        {
            _itemStatuses = itemStatuses;
        }

        public GitWorkingDirectoryStatusEventArgs()
        {
        }

        /// <summary>
        /// the status of all modified files/submodules or null if the previous information is invalidated.
        /// </summary>
        public IReadOnlyList<GitItemStatus>? ItemStatuses => _itemStatuses ?? Array.Empty<GitItemStatus>();
    }
}
