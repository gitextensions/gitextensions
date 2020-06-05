using System;
using System.Collections.Generic;
using System.Linq;
using GitCommands.UserRepositoryHistory;

namespace GitUI.CommandsDialogs
{
    internal class FormRemotesController
    {
        public void RemoteDelete(IList<Repository> remotes, string oldRemoteUrl)
        {
            if (string.IsNullOrWhiteSpace(oldRemoteUrl))
            {
                return;
            }

            var oldRemote = remotes.FirstOrDefault(r => r.Path == oldRemoteUrl);
            if (oldRemote != null)
            {
                remotes.Remove(oldRemote);
            }
        }

        public void RemoteUpdate(IList<Repository> remotes, string oldRemoteUrl, string newRemoteUrl)
        {
            if (string.IsNullOrWhiteSpace(newRemoteUrl))
            {
                return;
            }

            // if remote url was renamed - delete the old value
            if (!string.Equals(oldRemoteUrl, newRemoteUrl, StringComparison.OrdinalIgnoreCase))
            {
                RemoteDelete(remotes, oldRemoteUrl);
            }

            if (remotes.All(r => r.Path != newRemoteUrl))
            {
                remotes.Insert(0, new Repository(newRemoteUrl));
            }
        }
    }
}
