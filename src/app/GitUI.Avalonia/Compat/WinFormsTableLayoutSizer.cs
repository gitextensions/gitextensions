using Avalonia.Controls;

namespace GitUI.Compat;

/// <summary>
///  Preserves the floor-rounded percentage allocation used by WinForms TableLayoutPanel.
/// </summary>
internal static class WinFormsTableLayoutSizer
{
    public static void AttachColumns(Grid grid, int firstColumnPercent, int totalPercent)
    {
        grid.SizeChanged += (_, e) =>
        {
            if (grid.ColumnDefinitions.Count < 2 || e.NewSize.Width <= 0)
            {
                return;
            }

            double availableWidth = Math.Floor(e.NewSize.Width);
            double firstColumnWidth = Math.Floor(availableWidth * firstColumnPercent / totalPercent);
            grid.ColumnDefinitions[0].Width = new GridLength(firstColumnWidth);
            grid.ColumnDefinitions[1].Width = new GridLength(availableWidth - firstColumnWidth);
        };
    }
}
