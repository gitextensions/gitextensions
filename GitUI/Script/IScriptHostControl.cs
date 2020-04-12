using System.Collections.Generic;
using System.Drawing;
using GitCommands;

namespace GitUI.Script
{
    public interface IScriptHostControl
    {
        GitRevision GetCurrentRevision();
        GitRevision GetLatestSelectedRevision();
        IReadOnlyList<GitRevision> GetSelectedRevisions();
        Point GetQuickItemSelectorLocation();
    }
}
