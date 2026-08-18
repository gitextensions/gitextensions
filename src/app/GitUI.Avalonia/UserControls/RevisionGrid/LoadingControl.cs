using Avalonia.Controls;
using GitUI.UserControls;

namespace GitUI.UserControls.RevisionGrid;

public sealed class LoadingControl : UserControl
{
    private readonly WaitSpinner _waitSpinner;

    public LoadingControl()
    {
        MinWidth = 32;
        MinHeight = 32;
        _waitSpinner = new WaitSpinner
        {
            Width = 32,
            Height = 32,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
        };
        Content = _waitSpinner;
        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
        HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
        VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
    }

    public new bool IsAnimating
    {
        get => _waitSpinner.IsAnimating;
        set => _waitSpinner.IsAnimating = value;
    }
}
