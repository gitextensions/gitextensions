using System.Collections.Generic;
using System.Drawing;
using GitUIPluginInterfaces;

namespace GitUI.Script
{
    public interface IScriptHostControl
    {
        GitRevision GetCurrentRevision();
        GitRevision? GetLatestSelectedRevision();
        IReadOnlyList<GitRevision> GetSelectedRevisions();
        Point GetQuickItemSelectorLocation();
    }
}
