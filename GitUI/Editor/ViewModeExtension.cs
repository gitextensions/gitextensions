namespace GitUI.Editor;

public static class ViewModeExtension
{
    public static bool IsNormalDiffView(this ViewMode viewMode)
        => viewMode is (ViewMode.Diff or ViewMode.FixedDiff or ViewMode.CombinedDiff);

    public static bool IsDiffView(this ViewMode viewMode)
        => viewMode.IsNormalDiffView() || viewMode is ViewMode.RangeDiff or ViewMode.Difftastic;

    /// <summary>
    /// The document is partial, the line numbers ar (normally) non-continuous.
    /// </summary>
    /// <param name="viewMode">The current view mode in the editor.</param>
    /// <returns><c>true</c> if the view is partial, <c>false</c> if the complete contents is shown.</returns>
    public static bool IsPartialTextView(this ViewMode viewMode)
        => viewMode.IsDiffView() || viewMode is ViewMode.Grep;
}
