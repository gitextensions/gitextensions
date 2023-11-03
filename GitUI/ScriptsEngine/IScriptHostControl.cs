using GitUIPluginInterfaces;

namespace GitUI.ScriptsEngine
{
    public interface IScriptHostControl
    {
        GitRevision? GetLatestSelectedRevision();
        IReadOnlyList<GitRevision> GetSelectedRevisions();
        Point GetQuickItemSelectorLocation();

        void GoToRef(string? refName, bool showNoRevisionMsg, bool toggleSelection = false);

        bool IsRevisionGrid { get; }
        IWin32Window Window { get; }
        IGitUICommands UICommands { get; }
    }

    public class DefaultScriptHostControl : IScriptHostControl
    {
        public DefaultScriptHostControl(IWin32Window window, IGitUICommands uiCommands)
        {
            Window = window;
            UICommands = uiCommands;
        }

        public GitRevision? GetLatestSelectedRevision() => throw new NotImplementedException();

        public IReadOnlyList<GitRevision> GetSelectedRevisions() => throw new NotImplementedException();

        public Point GetQuickItemSelectorLocation() => new Point();

        public void GoToRef(string? refName, bool showNoRevisionMsg, bool toggleSelection = false) => throw new NotImplementedException();

        public bool IsRevisionGrid => false;

        public IWin32Window Window { get; }

        public IGitUICommands UICommands { get; }
    }
}
