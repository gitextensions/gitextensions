using Avalonia.Controls;
using Avalonia.Media;
using GitUI.Properties;

namespace GitUI.UserControls.RevisionGrid;

public sealed class ErrorControl : UserControl
{
    private Image _image = null!;

    public ErrorControl()
    {
        InitializeComponent();

        // Avalonia stretches UserControl content in its ContentPresenter instead of using WinForms DockStyle.Fill.
        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
        HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
        VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
    }

    private void InitializeComponent()
    {
        //
        // _image
        //
        _image = new Image
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            Source = Images.StatusBadgeError,
            Stretch = Stretch.None,
        };

        //
        // ErrorControl
        //
        Content = _image;
    }
}
