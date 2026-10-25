namespace GitUI.UserControls.RevisionGrid;

/// <summary>
///  Supplies the vertical geometry a ref label icon needs to size and position itself.
/// </summary>
/// <remarks>
///  All values are relative to the top of the ref label capsule, so the metrics are constant
///  as long as the font, the DPI scaling and the row height do not change.
/// </remarks>
/// <param name="CapsuleHeight">The height of the ref label capsule, which follows the row height.</param>
/// <param name="TextHeight">The height of the ref label text.</param>
/// <param name="TextBaselineOffset">The distance from the top of the capsule to the baseline of the ref label text.</param>
internal readonly record struct RefLabelIconMetrics(int CapsuleHeight, int TextHeight, int TextBaselineOffset);
