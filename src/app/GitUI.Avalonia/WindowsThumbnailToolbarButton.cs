using Avalonia.Media;
using GitUI.Compat;

namespace GitUI;

public sealed class WindowsThumbnailToolbarButton
{
    // Framework constraint: toolbar images are Avalonia images.
    public WindowsThumbnailToolbarButton(string text, IImage image, EventHandler<ThumbnailButtonClickedEventArgs> click)
    {
        Text = text;
        Image = image;
        Click = click;
    }

    public EventHandler<ThumbnailButtonClickedEventArgs> Click { get; }
    public IImage Image { get; }
    public string Text { get; }
}
