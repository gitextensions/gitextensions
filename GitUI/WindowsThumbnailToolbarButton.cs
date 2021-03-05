using System;
using System.Drawing;
using Microsoft.WindowsAPICodePack.Taskbar;

namespace GitUI
{
    public sealed class WindowsThumbnailToolbarButton
    {
        public WindowsThumbnailToolbarButton(string text, Image image, EventHandler<ThumbnailButtonClickedEventArgs> click)
        {
            Text = text;
            Image = image;
            Click = click;
        }

        public EventHandler<ThumbnailButtonClickedEventArgs> Click { get; }
        public Image Image { get; }
        public string Text { get; }
    }
}
