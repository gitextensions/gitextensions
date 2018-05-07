using System;
using GitCommands.UserRepositoryHistory;

namespace GitUI.CommandsDialogs.BrowseDialog.DashboardControl
{
    public class RepositoryEventArgs : EventArgs
    {
        public RepositoryEventArgs(Repository repository)
        {
            Repository = repository;
        }

        public Repository Repository { get; }
    }
}