using System;
using System.Collections.Generic;
using System.Linq;
using GitCommands;
using JetBrains.Annotations;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    public class GitWorkingDirectoryStatusEventArgs : EventArgs
    {
        public GitWorkingDirectoryStatusEventArgs(IEnumerable<GitItemStatus> itemStatuses)
        {
            ItemStatuses = itemStatuses;
        }

        public GitWorkingDirectoryStatusEventArgs()
        {
        }

        /// <summary>
        /// the status of all modified files/submodules or null if the previous information is invalidated
        /// </summary>
        [CanBeNull]
        public IEnumerable<GitItemStatus> ItemStatuses { get; }
    }
}