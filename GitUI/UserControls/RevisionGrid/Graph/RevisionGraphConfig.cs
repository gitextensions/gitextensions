using GitCommands;

namespace GitUI.UserControls.RevisionGrid.Graph;

internal readonly struct RevisionGraphConfig
{
    public bool MergeGraphLanesHavingCommonParent { get; }

    public bool ReduceGraphCrossings => !MergeGraphLanesHavingCommonParent;

    public int StraightenGraphSegmentsLimit { get; }

    public RevisionGraphConfig()
    {
        MergeGraphLanesHavingCommonParent = AppSettings.MergeGraphLanesHavingCommonParent.Value;
        StraightenGraphSegmentsLimit = AppSettings.StraightenGraphSegmentsLimit.Value;
    }
}
