using GitCommands.Settings;

namespace GitCommands;

public static partial class AppSettings
{
    public static BooleanSetting MergeGraphLanesHavingCommonParent { get; } = new(RevisionGraphSettingsPath, nameof(MergeGraphLanesHavingCommonParent), defaultValue: true);

    public static BooleanSetting RenderGraphWithDiagonals { get; } = new(RevisionGraphSettingsPath, nameof(RenderGraphWithDiagonals), defaultValue: true);

    public static BooleanSetting StraightenGraphDiagonals { get; } = new(RevisionGraphSettingsPath, nameof(StraightenGraphDiagonals), defaultValue: true);

    /// <summary>
    ///  The limit when to skip the straightening of revision graph segments.
    /// </summary>
    /// <remarks>
    ///  Straightening needs to call the expensive RevisionGraphRow.BuildSegmentLanes function.<br></br>
    ///  Straightening inserts gaps making the graph wider. If it already has to display many segments, i.e. parallel branches, there would be a low benefit of straightening.<br></br>
    ///  So rather skip the - in this case particularly expensive - RevisionGraphRow.BuildSegmentLanes function and call it only if the row is visible.
    /// </remarks>
    public static IntegerSetting StraightenGraphSegmentsLimit { get; } = new(RevisionGraphSettingsPath, nameof(StraightenGraphSegmentsLimit), defaultValue: 80);
}
