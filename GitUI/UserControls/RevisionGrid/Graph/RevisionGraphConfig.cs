using GitCommands;

namespace GitUI.UserControls.RevisionGrid.Graph;

internal readonly struct RevisionGraphConfig
{
    public bool MergeGraphLanesHavingCommonParent { get; }

    public bool ReduceGraphCrossings => !MergeGraphLanesHavingCommonParent;

    public bool RenderGraphWithDiagonals { get; }

    /// <summary>
    ///  <see cref="AppSettings.StraightenGraphSegmentsLimit"/>
    /// </summary>
    public int StraightenGraphSegmentsLimit { get; }

    public RevisionGraphConfig()
    {
        MergeGraphLanesHavingCommonParent = AppSettings.MergeGraphLanesHavingCommonParent.Value;
        RenderGraphWithDiagonals = AppSettings.RenderGraphWithDiagonals.Value;
        StraightenGraphSegmentsLimit = AppSettings.StraightenGraphSegmentsLimit.Value;
    }
}
