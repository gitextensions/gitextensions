using GitCommands;
using GitExtensions.ParityCapture;
using GitUI;

namespace WinFormsParityCapture;

/// <summary>
///  Observes native capture inputs without replacing a source constructor's clock or sizing.
/// </summary>
internal static class NativeCaptureConstructionObservation
{
    /// <summary>
    ///  Identifies the original provider retained in each live native date column's tag.
    /// </summary>
    internal const string DateColumnProviderName = "GitUI.UserControls.RevisionGrid.Columns.DateColumnProvider";

    /// <summary>
    ///  Samples the configured native sizing inputs at the live root-factory boundary.
    /// </summary>
    /// <returns>The coherent live endpoint and actual settings context.</returns>
    internal static CaptureConstructionClockSample Sample()
    {
        Font font = AppSettings.Font;
        return CaptureConstructionClock.Sample(new CaptureConstructionFont
        {
            Family = font.Name,
            SizePoints = font.SizeInPoints,
            Bold = font.Bold,
            Italic = font.Italic,
            Underline = font.Underline,
            Strikeout = font.Strikeout,
        }, AppSettings.RelativeDate);
    }

    /// <summary>
    ///  Reads actual tagged date columns under their logical revision-grid owners.
    /// </summary>
    /// <param name="root">The already created capture root; observing it does not request a handle.</param>
    /// <returns>The actual date-column widths at this phase, in physical pixels.</returns>
    internal static IReadOnlyList<CaptureConstructionDateColumn> ReadDateColumns(Control root)
    {
        List<CaptureConstructionDateColumn> columns = [];
        foreach (DataGridView grid in Descendants(root).OfType<DataGridView>())
        {
            RevisionGridControl? owner = null;
            for (Control? ancestor = grid.Parent; ancestor is not null; ancestor = ancestor.Parent)
            {
                if (ancestor is RevisionGridControl revisionGrid)
                {
                    owner = revisionGrid;
                    break;
                }
            }

            if (owner is null)
            {
                continue;
            }

            string ownerName = ReferenceEquals(owner, root) ? "$"
                : !string.IsNullOrWhiteSpace(owner.Name) ? owner.Name
                : owner.GetType().FullName ?? owner.GetType().Name;
            columns.AddRange(ProjectDateColumns(ownerName, grid.Columns.Cast<DataGridViewColumn>(), grid.DeviceDpi,
                column => column.Tag?.GetType().FullName));
        }

        return columns;
    }

    /// <summary>
    ///  Projects already observed native column values; the resolver keeps no-window tests independent of product construction.
    /// </summary>
    /// <param name="owner">The logical owning revision-grid name.</param>
    /// <param name="columns">The actual columns, or standalone test-owned columns.</param>
    /// <param name="dpi">The native DPI observed at this phase.</param>
    /// <param name="providerName">The provider-tag lookup, using actual runtime type names in the capture consumer.</param>
    /// <returns>The date-column projections without renormalizing their widths.</returns>
    internal static IReadOnlyList<CaptureConstructionDateColumn> ProjectDateColumns(
        string owner, IEnumerable<DataGridViewColumn> columns, int dpi, Func<DataGridViewColumn, string?> providerName)
        => columns.Where(column => providerName(column) == DateColumnProviderName)
            .Select(column => new CaptureConstructionDateColumn
            {
                Owner = owner,
                Provider = DateColumnProviderName,
                ColumnIndex = column.Index,
                Width = column.Width,
                Unit = CaptureConstructionWidthUnit.PhysicalPixel,
                Dpi = new CaptureDpi { X = dpi, Y = dpi },
                RenderScale = dpi / 96d,
            }).ToArray();

    private static IEnumerable<Control> Descendants(Control root)
    {
        yield return root;
        foreach (Control child in root.Controls)
        {
            foreach (Control descendant in Descendants(child))
            {
                yield return descendant;
            }
        }
    }
}
