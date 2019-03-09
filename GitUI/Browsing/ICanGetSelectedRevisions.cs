using System.Collections.Generic;
using System.DirectoryServices;
using GitCommands;

namespace GitUI.Browsing
{
    internal interface ICanGetSelectedRevisions
    {
        IReadOnlyList<GitRevision> GetSelectedRevisions(SortDirection? direction = null);
    }
}
