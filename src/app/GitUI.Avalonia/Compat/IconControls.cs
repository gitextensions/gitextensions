using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace GitUI.Compat;

/// <summary>
/// An Avalonia button whose content remains the original translatable text while an image
/// is presented beside it by the shared visual-parity style.
/// </summary>
public class IconButton : Button
{
    public IconButton()
    {
        Classes.Add("gitextensions-icon-button");
    }

    public static readonly StyledProperty<IImage?> IconProperty =
        AvaloniaProperty.Register<IconButton, IImage?>(nameof(Icon));

    public IImage? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    protected override Type StyleKeyOverride => typeof(Button);
}

/// <summary>
/// A toggle button whose string content retains the original translation key while the
/// shared toolbar template presents only its image.
/// </summary>
public class IconToggleButton : ToggleButton
{
    public IconToggleButton()
    {
        Classes.Add("gitextensions-icon-toggle-button");
    }

    public static readonly StyledProperty<IImage?> IconProperty =
        AvaloniaProperty.Register<IconToggleButton, IImage?>(nameof(Icon));

    public IImage? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    protected override Type StyleKeyOverride => typeof(ToggleButton);
}

/// <summary>
/// An Avalonia split button whose string content keeps the original translation key while
/// the shared template presents the corresponding WinForms toolbar image.
/// </summary>
public class IconSplitButton : SplitButton
{
    public IconSplitButton()
    {
        Classes.Add("gitextensions-icon-split-button");
    }

    public static readonly StyledProperty<IImage?> IconProperty =
        AvaloniaProperty.Register<IconSplitButton, IImage?>(nameof(Icon));

    public IImage? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    protected override Type StyleKeyOverride => typeof(SplitButton);
}

/// <summary>
/// A drop-down button retaining its original string content for translation while the
/// toolbar presents only the corresponding image.
/// </summary>
public class IconDropDownButton : DropDownButton
{
    public IconDropDownButton()
    {
        Classes.Add("gitextensions-icon-drop-down-button");
    }

    public static readonly StyledProperty<IImage?> IconProperty =
        AvaloniaProperty.Register<IconDropDownButton, IImage?>(nameof(Icon));

    public IImage? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    protected override Type StyleKeyOverride => typeof(DropDownButton);
}

/// <summary>
/// A radio button variant retaining a string Content value for the existing translation
/// adapter while presenting the original WinForms image beside it.
/// </summary>
public class IconRadioButton : RadioButton
{
    public IconRadioButton()
    {
        Classes.Add("gitextensions-icon-radio-button");
    }

    public static readonly StyledProperty<IImage?> IconProperty =
        AvaloniaProperty.Register<IconRadioButton, IImage?>(nameof(Icon));

    public IImage? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    protected override Type StyleKeyOverride => typeof(RadioButton);
}
