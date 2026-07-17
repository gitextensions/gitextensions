using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace GitUI.Compat;

/// <summary>
/// An Avalonia button whose content remains the original translatable text while an image
/// is presented beside it by the shared visual-parity style.
/// </summary>
public class IconButton : Button
{
    public static readonly StyledProperty<IImage?> IconProperty =
        AvaloniaProperty.Register<IconButton, IImage?>(nameof(Icon));

    public IImage? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
}

/// <summary>
/// An Avalonia split button whose string content keeps the original translation key while
/// the shared template presents the corresponding WinForms toolbar image.
/// </summary>
public class IconSplitButton : SplitButton
{
    public static readonly StyledProperty<IImage?> IconProperty =
        AvaloniaProperty.Register<IconSplitButton, IImage?>(nameof(Icon));

    public IImage? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
}

/// <summary>
/// A drop-down button retaining its original string content for translation while the
/// toolbar presents only the corresponding image.
/// </summary>
public class IconDropDownButton : DropDownButton
{
    public static readonly StyledProperty<IImage?> IconProperty =
        AvaloniaProperty.Register<IconDropDownButton, IImage?>(nameof(Icon));

    public IImage? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
}

/// <summary>
/// A radio button variant retaining a string Content value for the existing translation
/// adapter while presenting the original WinForms image beside it.
/// </summary>
public class IconRadioButton : RadioButton
{
    public static readonly StyledProperty<IImage?> IconProperty =
        AvaloniaProperty.Register<IconRadioButton, IImage?>(nameof(Icon));

    public IImage? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
}
