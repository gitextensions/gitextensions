using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace GitUI.Compat;

/// <summary>
///  Preserves the proportional, floor-rounded resize behavior of a horizontal WinForms SplitContainer.
/// </summary>
internal static class WinFormsSplitContainerSizer
{
    public static void Attach(Grid grid, int sourceHeight, int sourceSplitterDistance)
    {
        State state = new(grid, sourceHeight, sourceSplitterDistance);
        grid.SizeChanged += state.Resize;
        grid.Children.OfType<GridSplitter>().Single().DragCompleted += state.SplitterDragCompleted;
    }

    private sealed class State(Grid grid, int sourceHeight, int sourceSplitterDistance)
    {
        private double _referenceHeight = sourceHeight;
        private double _referenceDistance = sourceSplitterDistance;

        public void Resize(object? sender, SizeChangedEventArgs e)
        {
            if (grid.RowDefinitions.Count < 3 || e.NewSize.Height <= 0)
            {
                return;
            }

            double availableHeight = Math.Max(0, e.NewSize.Height - grid.RowDefinitions[1].ActualHeight);
            double distance = Math.Clamp(
                Math.Floor(_referenceDistance * e.NewSize.Height / _referenceHeight),
                0,
                availableHeight);
            grid.RowDefinitions[0].Height = new GridLength(distance);
            grid.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Star);
        }

        public void SplitterDragCompleted(object? sender, VectorEventArgs e)
        {
            if (grid.Bounds.Height <= 0)
            {
                return;
            }

            _referenceHeight = grid.Bounds.Height;
            _referenceDistance = grid.RowDefinitions[0].ActualHeight;
        }
    }
}
