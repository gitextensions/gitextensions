using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;

namespace GitUI.Compat;

/// <summary>
///  Keeps an original image-bearing button's caption translatable without replacing its native chrome.
/// </summary>
internal sealed class ImageTextButtonContent : Grid
{
    private readonly Image _image = new() { Margin = new Thickness(0, 0, 4, 0), Stretch = Stretch.None, VerticalAlignment = VerticalAlignment.Center };
    private readonly AccessText _text = new() { VerticalAlignment = VerticalAlignment.Center };

    public ImageTextButtonContent()
    {
        ColumnDefinitions = new ColumnDefinitions("Auto,Auto");
        HorizontalAlignment = HorizontalAlignment.Center;
        VerticalAlignment = VerticalAlignment.Center;
        Grid.SetColumn(_text, 1);
        Children.Add(_image);
        Children.Add(_text);
    }

    public IImage? Image
    {
        get => _image.Source;
        set => _image.Source = value;
    }

    public string? Text
    {
        get => _text.Text;
        set => _text.Text = value;
    }

    public override string ToString() => AvaloniaTranslationUtils.RemoveAvaloniaMnemonics(Text ?? string.Empty);
}
