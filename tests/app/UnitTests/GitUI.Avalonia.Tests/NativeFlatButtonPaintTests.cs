using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitExtUtils.GitUI.Theming;
using GitUI.Compat;
using GitUI.LeftPanel;
using GitUI.Theming;
using DrawingColor = System.Drawing.Color;
using KnownColor = System.Drawing.KnownColor;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class NativeFlatButtonPaintTests
{
    [AvaloniaTest]
    [Category("P8.6i.126")]
    [TestCase("#FFFE0000", "#FFFF0000")]
    [TestCase("#FF00FE00", "#FF00FF00")]
    [TestCase("#FF0000FE", "#FF0000FF")]
    [TestCase("#40FE0000", "#FFFF0000")]
    [TestCase("#407F7F7F", "#FF989898")]
    [TestCase("#40808080", "#FF737373")]
    [TestCase("#40FFFFFF", "#FFE5E5E5")]
    public void Flat_palette_should_clamp_channels_and_make_source_low_colors_opaque(string input, string expected)
    {
        Application application = Application.Current!;
        bool hadLight = application.Resources.ThemeDictionaries.TryGetValue(ThemeVariant.Light, out IThemeVariantProvider? originalLight);
        ThemeSettings originalSettings = ThemeModule.Settings;
        ResourceDictionary resources = new();
        application.Resources.ThemeDictionaries[ThemeVariant.Light] = resources;
        Color color = Color.Parse(input);
        DrawingColor drawingColor = DrawingColor.FromArgb(color.A, color.R, color.G, color.B);
        Theme theme = new(
            new Dictionary<AppColor, DrawingColor> { [AppColor.PanelBackground] = DrawingColor.White },
            new Dictionary<KnownColor, DrawingColor>
            {
                [KnownColor.Control] = drawingColor,
                [KnownColor.ControlLightLight] = drawingColor,
            },
            new ThemeId("native-flat-button-saturated-light"));
        try
        {
            // ColorOptions.Adjust255 truncates, clamps to 255, and creates opaque RGB colors.
            AvaloniaThemeResources.Apply(application, new ThemeSettings(theme, Theme.Default, ThemeVariations.None, useSystemVisualStyle: true));
            GetColor((IBrush)resources["GitExtensionsNativeFlatButtonHoverBackgroundBrush"]!).Should().Be(Color.Parse(expected));
            GetColor((IBrush)resources["GitExtensionsNativeFlatButtonPressedBackgroundBrush"]!).Should().Be(Color.Parse(expected));
        }
        finally
        {
            if (hadLight)
            {
                application.Resources.ThemeDictionaries[ThemeVariant.Light] = originalLight!;
            }
            else
            {
                application.Resources.ThemeDictionaries.Remove(ThemeVariant.Light);
            }

            ColorHelper.ThemeSettings = originalSettings;
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    [TestCase(false, "#F0F0F0", "#F0F0F0", "#F0F0F0")]
    [TestCase(false, "#D8D8D8", "#D8D8D8", "#F0F0F0")]
    [TestCase(false, "#E5E5E5", "#E5E5E5", "#F0F0F0")]
    [TestCase(true, "#333333", "#9B9B9B", "#F0F0F0")]
    [TestCase(true, "#454545", "#9B9B9B", "#F0F0F0")]
    [TestCase(true, "#666666", "#A0A0A0", "#F0F0F0")]
    [TestCase(true, "#333333", "#373737", "#F0F0F0")]
    [TestCase(true, "#333333", "#9B9B9B", "#202020")]
    [TestCase(true, "#454545", "#9B9B9B", "#202020")]
    public void Flat_chrome_should_keep_the_source_inset_without_changing_the_button_size(bool dark, string fill, string edge, string backdrop)
    {
        NativeFlatButtonChrome chrome = new()
        {
            Width = 26,
            Height = 26,
            UseDarkMode = dark,
            Background = Brush.Parse(fill),
            BorderBrush = Brush.Parse(edge),
            BackdropBrush = Brush.Parse(backdrop),
        };
        Window window = new() { Width = 80, Height = 60, Content = new Canvas { Background = Brush.Parse(backdrop), Children = { chrome } } };
        try
        {
            window.Show();
            window.UpdateLayout();
            chrome.Bounds.Size.Should().Be(new Avalonia.Size(26, 26));
            chrome.IsHitTestVisible.Should().BeFalse();
            chrome.Focusable.Should().BeFalse();
            using WriteableBitmap frame = window.CaptureRenderedFrame()
                ?? throw new InvalidOperationException("The flat-button chrome frame is unavailable.");
            ReadPixel(frame, new PixelPoint(0, 13)).Should().Be(dark ? BlendEdge(Color.Parse(fill), Color.Parse(backdrop), 2) : Color.Parse(fill));
            ReadPixel(frame, new PixelPoint(1, 13)).Should().Be(Color.Parse(edge));
            ReadPixel(frame, new PixelPoint(2, 13)).Should().Be(Color.Parse(fill));
            ReadPixel(frame, new PixelPoint(24, 13)).Should().Be(Color.Parse(fill));
            ReadPixel(frame, new PixelPoint(25, 13)).Should().Be(Color.Parse(dark ? edge : fill));
            ReadPixel(frame, default).Should().Be(dark ? BlendEdge(Color.Parse(fill), Color.Parse(backdrop), 4) : Color.Parse(fill));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public void Flat_focus_should_use_native_solid_or_dotted_pixels_only_when_requested(bool dark, bool focused)
    {
        Color fill = Color.Parse(dark ? "#333333" : "#F0F0F0");
        Color focus = dark ? Colors.Black : Color.Parse("#A0A0A0");
        NativeFlatButtonChrome chrome = new()
        {
            Width = 26,
            Height = 26,
            UseDarkMode = dark,
            Background = new SolidColorBrush(fill),
            ShowKeyboardFocus = focused,
            FocusBrush = new SolidColorBrush(focus),
        };
        Window window = new() { Width = 80, Height = 60, Content = new Canvas { Children = { chrome } } };
        try
        {
            window.Show();
            window.UpdateLayout();
            using WriteableBitmap frame = window.CaptureRenderedFrame()
                ?? throw new InvalidOperationException("The flat-button focus frame is unavailable.");
            for (int offset = 0; offset < 20; offset++)
            {
                ReadPixel(frame, new PixelPoint(3 + offset, 3)).Should().Be(focused && (!dark || (offset & 1) == 0) ? focus : fill);
                ReadPixel(frame, new PixelPoint(3 + offset, 22)).Should().Be(focused && (!dark || (offset & 1) != 0) ? focus : fill);
                ReadPixel(frame, new PixelPoint(3, 3 + offset)).Should().Be(focused && (!dark || (offset & 1) == 0) ? focus : fill);
                ReadPixel(frame, new PixelPoint(22, 3 + offset)).Should().Be(focused && (!dark || (offset & 1) != 0) ? focus : fill);
            }
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    [TestCase(false)]
    [TestCase(true)]
    public void Search_button_should_keep_its_icon_layout_and_use_flat_state_resources(bool dark)
    {
        RepoObjectsTree control = new();
        IconButton button = control.FindControl<IconButton>("btnSearch")!;
        Window window = new() { Width = 360, Height = 220, Content = control };
        window.Resources["GitExtensionsNativeFlatButtonDarkMode"] = dark;
        window.Resources["GitExtensionsNativeFlatButtonFocusBrush"] = dark ? Brushes.Black : Brush.Parse("#A0A0A0");
        window.Resources["GitExtensionsNativeFlatButtonBackgroundBrush"] = Brush.Parse(dark ? "#333333" : "#F0F0F0");
        window.Resources["GitExtensionsNativeFlatButtonHoverBackgroundBrush"] = Brush.Parse(dark ? "#454545" : "#D8D8D8");
        window.Resources["GitExtensionsNativeFlatButtonPressedBackgroundBrush"] = Brush.Parse(dark ? "#666666" : "#E5E5E5");
        window.Resources["GitExtensionsNativeFlatButtonDisabledBackgroundBrush"] = Brush.Parse(dark ? "#333333" : "#F0F0F0");
        window.Resources["GitExtensionsNativeFlatButtonBorderBrush"] = Brush.Parse(dark ? "#9B9B9B" : "#F0F0F0");
        window.Resources["GitExtensionsNativeFlatButtonHoverBorderBrush"] = Brush.Parse(dark ? "#9B9B9B" : "#D8D8D8");
        window.Resources["GitExtensionsNativeFlatButtonPressedBorderBrush"] = Brush.Parse(dark ? "#A0A0A0" : "#E5E5E5");
        window.Resources["GitExtensionsNativeFlatButtonDisabledBorderBrush"] = Brush.Parse(dark ? "#373737" : "#F0F0F0");
        try
        {
            window.Show();
            window.UpdateLayout();
            NativeFlatButtonChrome chrome = button.GetVisualDescendants().OfType<NativeFlatButtonChrome>().Single();
            button.Bounds.Size.Should().Be(new Avalonia.Size(26, 26));
            button.Padding.Should().Be(new Thickness(2));
            chrome.UseDarkMode.Should().Be(dark);
            button.GetVisualDescendants().OfType<Image>().Single().Bounds.Size.Should().Be(new Avalonia.Size(16, 16));
            Color normal = GetColor(chrome.Background);
            normal.Should().Be(Color.Parse(dark ? "#333333" : "#F0F0F0"));
            TreeView tree = control.FindControl<TreeView>("treeMain")!;
            tree.Focus(NavigationMethod.Pointer).Should().BeTrue();
            button.Focus(NavigationMethod.Tab).Should().BeTrue();
            Dispatcher.UIThread.RunJobs();
            chrome.ShowKeyboardFocus.Should().BeTrue();
            GetColor(chrome.FocusBrush).Should().Be(dark ? Colors.Black : Color.Parse("#A0A0A0"));
            window.GetVisualDescendants().OfType<WinFormsFocusAdorner>().Should().BeEmpty("the scoped native focus painter replaces the generic adorner");
            tree.Focus(NavigationMethod.Pointer).Should().BeTrue();
            button.Focus(NavigationMethod.Pointer).Should().BeTrue();
            Dispatcher.UIThread.RunJobs();
            chrome.ShowKeyboardFocus.Should().BeFalse();
            Point position = button.TranslatePoint(new Point(13, 13), window)
                ?? throw new InvalidOperationException("The search-button input position is unavailable.");
            window.MouseMove(position);
            Dispatcher.UIThread.RunJobs();
            GetColor(chrome.Background).Should().Be(Color.Parse(dark ? "#454545" : "#D8D8D8"));
            window.MouseDown(position, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            GetColor(chrome.Background).Should().Be(Color.Parse(dark ? "#666666" : "#E5E5E5"));
            button.IsEnabled = false;
            Dispatcher.UIThread.RunJobs();
            chrome.ShowKeyboardFocus.Should().BeFalse();
            GetColor(chrome.Background).Should().Be(normal, "disabled state wins over an existing press or hover");
            GetColor(chrome.BorderBrush).Should().Be(Color.Parse(dark ? "#373737" : "#F0F0F0"));
            window.MouseUp(position, MouseButton.Left);
            button.IsEnabled = true;
            window.MouseMove(new Point(350, 210));
            Dispatcher.UIThread.RunJobs();
            GetColor(chrome.Background).Should().Be(normal);
            window.Resources["GitExtensionsNativeFlatButtonBackgroundBrush"] = Brushes.Green;
            Dispatcher.UIThread.RunJobs();
            GetColor(chrome.Background).Should().Be(Colors.Green, "the live source-theme resource boundary must remain attached");
            button.Bounds.Size.Should().Be(new Avalonia.Size(26, 26));
        }
        finally
        {
            window.Close();
        }
    }

    private static Color GetColor(IBrush? brush) => ((ISolidColorBrush)brush!).Color;

    private static Color BlendEdge(Color fill, Color backdrop, int divisor) => Color.FromRgb(
        (byte)((fill.R + (backdrop.R * (divisor - 1)) + (divisor / 2)) / divisor),
        (byte)((fill.G + (backdrop.G * (divisor - 1)) + (divisor / 2)) / divisor),
        (byte)((fill.B + (backdrop.B * (divisor - 1)) + (divisor / 2)) / divisor));

    private static Color ReadPixel(WriteableBitmap frame, PixelPoint point)
    {
        using ILockedFramebuffer buffer = frame.Lock();
        buffer.Format.BitsPerPixel.Should().Be(32);
        byte[] pixel = new byte[4];
        Marshal.Copy(IntPtr.Add(buffer.Address, (point.Y * buffer.RowBytes) + (point.X * 4)), pixel, 0, pixel.Length);
        return buffer.Format == PixelFormat.Bgra8888
            ? Color.FromArgb(pixel[3], pixel[2], pixel[1], pixel[0])
            : Color.FromArgb(pixel[3], pixel[0], pixel[1], pixel[2]);
    }
}
