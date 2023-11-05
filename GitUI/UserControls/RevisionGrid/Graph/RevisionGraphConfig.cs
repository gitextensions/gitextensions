using GitCommands;

namespace GitUI.UserControls.RevisionGrid.Graph;

internal readonly struct RevisionGraphConfig
{
    public bool MergeGraphLanesHavingCommonParent { get; }

    public RevisionGraphConfig()
    {
        MergeGraphLanesHavingCommonParent = AppSettings.MergeGraphLanesHavingCommonParent.Value;
    }
}
