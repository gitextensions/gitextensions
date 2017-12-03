using System;
using System.Collections.Generic;
using System.Linq;
using GitCommands;

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
            ItemStatuses = Enumerable.Empty<GitItemStatus>();
        }

        public IEnumerable<GitItemStatus> ItemStatuses { get; }
    }
}