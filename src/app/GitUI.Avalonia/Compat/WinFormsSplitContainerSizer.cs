using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace GitUI.Compat;

/// <summary>
///  Preserves the proportional, floor-rounded resize behavior of a horizontal WinForms SplitContainer.
/// </summary>
internal static class WinFormsSplitContainerSizer
{
    public static Action Attach(Grid grid, int sourceHeight, int sourceSplitterDistance, Func<bool>? isExpanded = null)
    {
        State state = new(grid, sourceHeight, sourceSplitterDistance, isExpanded);
        grid.SizeChanged += state.Resize;
        grid.Children.OfType<GridSplitter>().Single().DragCompleted += state.SplitterDragCompleted;
        return state.Refresh;
    }

    private sealed class State(Grid grid, int sourceHeight, int sourceSplitterDistance, Func<bool>? isExpanded)
    {
        private double _referenceHeight = sourceHeight;
        private double _referenceDistance = sourceSplitterDistance;

        public void Resize(object? sender, SizeChangedEventArgs e)
        {
            Apply(e.NewSize.Height);
        }

        public void Refresh() => Apply(grid.Bounds.Height);

        public void SplitterDragCompleted(object? sender, VectorEventArgs e)
        {
            if (grid.Bounds.Height <= 0)
            {
                return;
            }

            _referenceHeight = grid.Bounds.Height;
            _referenceDistance = grid.RowDefinitions[0].ActualHeight;
        }

        private void Apply(double height)
        {
            if (grid.RowDefinitions.Count < 3 || height <= 0 || isExpanded?.Invoke() == false)
            {
                return;
            }

            double availableHeight = Math.Max(0, height - grid.RowDefinitions[1].ActualHeight);
            double distance = Math.Clamp(
                Math.Floor(_referenceDistance * height / _referenceHeight),
                0,
                availableHeight);
            grid.RowDefinitions[0].Height = new GridLength(distance);
            grid.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Star);
        }
    }
}
