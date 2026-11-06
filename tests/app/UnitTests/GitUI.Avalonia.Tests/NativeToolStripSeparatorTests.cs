using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Threading;
using GitExtUtils.GitUI.Theming;
using GitUI.Compat;
using GitUI.Theming;
using DrawingColor = System.Drawing.Color;
using KnownColor = System.Drawing.KnownColor;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class NativeToolStripSeparatorTests
{
    [AvaloniaTest]
    [TestCase(6, 25, false, true, "#000000", "#000000", "#4D4D4D")]
    [TestCase(6, 25, false, true, "#FFFFFF", "#8C8C8C", "#FFFFFF")]
    [TestCase(6, 25, false, true, "#F0F0F0", "#848484", "#F5F5F5")]
    [TestCase(6, 25, false, true, "#202020", "#121212", "#636363")]
    [TestCase(6, 25, true, true, "#F0F0F0", "#848484", "#F5F5F5")]
    [TestCase(6, 25, false, false, "#F0F0F0", "#848484", "#F5F5F5")]
    [TestCase(6, 25, true, false, "#F0F0F0", "#848484", "#F5F5F5")]
    [TestCase(9, 31, true, false, "#F0F0F0", "#848484", "#F5F5F5")]
    [TestCase(10, 18, false, true, "#F0F0F0", "#848484", "#F5F5F5")]
    [TestCase(6, 9, true, false, "#F0F0F0", "#848484", "#F5F5F5")]
    [TestCase(6, 5, false, true, "#F0F0F0", "#848484", "#F5F5F5")]
    [TestCase(5, 5, false, true, "#F0F0F0", "#848484", "#F5F5F5")]
    [TestCase(4, 5, false, true, "#F0F0F0", "#848484", "#F5F5F5")]
    [TestCase(6, 4, false, true, "#F0F0F0", "#848484", "#F5F5F5")]
    [TestCase(6, 1, false, true, "#F0F0F0", "#848484", "#F5F5F5")]
    public void System_separator_should_stretch_the_native_alpha_mask_over_the_actual_backdrop(
        int width, int height, bool rightToLeft, bool enabled, string background, string shadow, string highlight)
    {
        Color backdrop = Color.Parse(background);
        NativeToolStripSeparatorChrome chrome = new()
        {
            Width = width,
            Height = height,
            UseSystemVisualStyle = true,
            FlowDirection = rightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
            IsEnabled = enabled,
        };
        Window window = new()
        {
            Width = 80,
            Height = 60,
            Content = new Canvas { Background = new SolidColorBrush(backdrop), Children = { chrome } },
        };
        try
        {
            window.Show();
            window.UpdateLayout();
            using WriteableBitmap frame = Capture(window);
            int left = width == 4 ? 1 : 2;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color expected = y >= 2 && y < height - 2
                        ? x == left ? Color.Parse(shadow) : x == left + 1 ? Color.Parse(highlight) : backdrop
                        : backdrop;
                    ReadPixel(frame, new PixelPoint(x, y)).Should().Be(expected,
                        $"the native image cap and premultiplied alpha compose at {x},{y}, independent of enabled state or RTL");
                }
            }
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(6, 25, 3, 5, 19, false, true)]
    [TestCase(6, 25, 3, 5, 19, true, true)]
    [TestCase(6, 25, 3, 5, 19, false, false)]
    [TestCase(6, 25, 3, 5, 19, true, false)]
    [TestCase(5, 25, 2, 5, 19, false, true)]
    [TestCase(7, 25, 3, 5, 19, true, true)]
    [TestCase(8, 25, 4, 5, 19, false, true)]
    [TestCase(6, 9, 3, 3, 5, true, false)]
    [TestCase(6, 4, 3, 2, 3, false, true)]
    [TestCase(6, 17, 3, 5, 11, true, true)]
    [TestCase(6, 32, 3, 5, 26, false, true)]
    public void Professional_separator_should_paint_the_native_integer_etched_pair(
        int width, int height, int left, int top, int bottom, bool rightToLeft, bool enabled)
    {
        Color backdrop = Color.Parse("#323232");
        Color dark = Color.Parse("#404040");
        Color light = Color.Parse("#101010");
        NativeToolStripSeparatorChrome chrome = new()
        {
            Width = width,
            Height = height,
            DarkBrush = new SolidColorBrush(dark),
            LightBrush = new SolidColorBrush(light),
            FlowDirection = rightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
            IsEnabled = enabled,
        };
        Window window = new()
        {
            Width = 80,
            Height = 60,
            Content = new Canvas { Background = new SolidColorBrush(backdrop), Children = { chrome } },
        };
        try
        {
            window.Show();
            window.UpdateLayout();
            chrome.Bounds.Size.Should().Be(new Avalonia.Size(width, height));
            chrome.IsHitTestVisible.Should().BeFalse();
            chrome.Focusable.Should().BeFalse();
            using WriteableBitmap frame = Capture(window);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color expected = x == left && y >= top && y <= bottom
                        ? rightToLeft ? light : dark
                        : x == left + 1 && y >= top + 1 && y <= bottom + 1
                            ? rightToLeft ? dark : light
                            : backdrop;
                    ReadPixel(frame, new PixelPoint(x, y)).Should().Be(expected,
                        $"native inclusive integer endpoints must remain aliased at {x},{y}");
                }
            }
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(1)]
    [TestCase(11)]
    public void Professional_separator_should_not_invent_ink_for_clipped_or_zero_length_source_lines(int height)
    {
        Color backdrop = Color.Parse("#323232");
        NativeToolStripSeparatorChrome chrome = new()
        {
            Width = 6,
            Height = height,
            DarkBrush = Brush.Parse("#404040"),
            LightBrush = Brush.Parse("#101010"),
        };
        Window window = new()
        {
            Width = 80,
            Height = 60,
            Content = new Canvas { Background = new SolidColorBrush(backdrop), Children = { chrome } },
        };
        try
        {
            window.Show();
            window.UpdateLayout();
            using WriteableBitmap frame = Capture(window);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < 6; x++)
                {
                    ReadPixel(frame, new PixelPoint(x, y)).Should().Be(backdrop,
                        "the actual native renderer omits flat-cap zero-length lines and clips paint outside its item");
                }
            }
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(false, "#BDBDBD", "#FFFFFF")]
    [TestCase(true, "#404040", "#101010")]
    public void Separator_palette_should_keep_native_renderer_colors_independent_of_custom_css(bool dark, string expectedDark, string expectedLight)
    {
        Application application = Application.Current!;
        ThemeVariant variant = dark ? ThemeVariant.Dark : ThemeVariant.Light;
        bool hadResources = application.Resources.ThemeDictionaries.TryGetValue(variant, out IThemeVariantProvider? originalResources);
        ThemeSettings originalSettings = ThemeModule.Settings;
        ResourceDictionary resources = new();
        application.Resources.ThemeDictionaries[variant] = resources;
        Theme theme = new(
            new Dictionary<AppColor, DrawingColor> { [AppColor.PanelBackground] = dark ? DrawingColor.Black : DrawingColor.White },
            new Dictionary<KnownColor, DrawingColor>
            {
                [KnownColor.ButtonShadow] = DrawingColor.Magenta,
                [KnownColor.Window] = DrawingColor.Lime,
                [KnownColor.ButtonHighlight] = DrawingColor.Blue,
            },
            new ThemeId("native-toolbar-separator-custom"));
        try
        {
            AvaloniaThemeResources.Apply(application, new ThemeSettings(theme, Theme.Default, ThemeVariations.None, useSystemVisualStyle: false));
            GetColor((IBrush)resources["GitExtensionsToolStripSeparatorDarkBrush"]!).Should().Be(Color.Parse(expectedDark));
            GetColor((IBrush)resources["GitExtensionsToolStripSeparatorLightBrush"]!).Should().Be(Color.Parse(expectedLight));
            resources["GitExtensionsToolStripSeparatorUseSystemVisualStyle"].Should().Be(false);
            GetColor((IBrush)resources["GitExtensionsWindowBackgroundBrush"]!).Should().Be(Colors.Lime,
                "the separator renderer boundary must not disable separately configurable control resources");
        }
        finally
        {
            if (hadResources)
            {
                application.Resources.ThemeDictionaries[variant] = originalResources!;
            }
            else
            {
                application.Resources.ThemeDictionaries.Remove(variant);
            }

            ColorHelper.ThemeSettings = originalSettings;
        }
    }

    [AvaloniaTest]
    public void Existing_separator_should_repaint_live_brushes_and_RTL_without_reallocating_its_owner()
    {
        NativeToolStripSeparatorChrome chrome = new();
        chrome[!NativeToolStripSeparatorChrome.DarkBrushProperty] = new DynamicResourceExtension("separator-dark");
        chrome[!NativeToolStripSeparatorChrome.LightBrushProperty] = new DynamicResourceExtension("separator-light");
        Border owner = new() { Width = 6, Height = 25, Background = Brushes.Transparent, Child = chrome };
        Window window = new()
        {
            Width = 80,
            Height = 60,
            Content = new Canvas { Background = Brush.Parse("#323232"), Children = { owner } },
        };
        window.Resources["separator-dark"] = Brush.Parse("#404040");
        window.Resources["separator-light"] = Brush.Parse("#101010");
        try
        {
            window.Show();
            window.UpdateLayout();
            Rect allocation = owner.Bounds;
            chrome.Bounds.Size.Should().Be(allocation.Size);
            using (WriteableBitmap before = Capture(window))
            {
                ReadPixel(before, new PixelPoint(3, 5)).Should().Be(Color.Parse("#404040"));
                ReadPixel(before, new PixelPoint(4, 6)).Should().Be(Color.Parse("#101010"));
            }

            window.Resources["separator-dark"] = Brushes.Red;
            window.Resources["separator-light"] = Brushes.Green;
            owner.FlowDirection = FlowDirection.RightToLeft;
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            GetColor(chrome.DarkBrush!).Should().Be(Colors.Red);
            GetColor(chrome.LightBrush!).Should().Be(Colors.Green);
            chrome.FlowDirection.Should().Be(FlowDirection.RightToLeft);
            owner.Bounds.Should().Be(allocation);
            using WriteableBitmap after = Capture(window);
            ReadPixel(after, new PixelPoint(3, 5)).Should().Be(Colors.Green);
            ReadPixel(after, new PixelPoint(4, 6)).Should().Be(Colors.Red);
            ReadPixel(after, new PixelPoint(2, 5)).Should().Be(Color.Parse("#323232"),
                "RTL swaps native pens without mirroring their item-relative coordinates");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Existing_system_separator_should_composite_over_live_owner_paint_without_a_guessed_backdrop_resource()
    {
        NativeToolStripSeparatorChrome chrome = new() { UseSystemVisualStyle = true };
        Border owner = new() { Width = 6, Height = 25, Child = chrome };
        Canvas canvas = new() { Children = { owner } };
        Window window = new() { Width = 80, Height = 60, Content = canvas };
        try
        {
            window.Show();
            foreach ((string backdrop, string shadow, string highlight) in new[]
                     {
                         ("#000000", "#000000", "#4D4D4D"),
                         ("#FFFFFF", "#8C8C8C", "#FFFFFF"),
                         ("#F0F0F0", "#848484", "#F5F5F5"),
                         ("#202020", "#121212", "#636363"),
                     })
            {
                canvas.Background = Brush.Parse(backdrop);
                Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();
                chrome.Bounds.Size.Should().Be(new Avalonia.Size(6, 25));
                using WriteableBitmap frame = Capture(window);
                ReadPixel(frame, new PixelPoint(2, 2)).Should().Be(Color.Parse(shadow));
                ReadPixel(frame, new PixelPoint(3, 22)).Should().Be(Color.Parse(highlight));
                ReadPixel(frame, new PixelPoint(4, 2)).Should().Be(Color.Parse(backdrop));
            }
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Existing_separator_should_follow_the_real_theme_resource_projection()
    {
        Application application = Application.Current!;
        bool hadLight = application.Resources.ThemeDictionaries.TryGetValue(ThemeVariant.Light, out IThemeVariantProvider? originalLight);
        bool hadDark = application.Resources.ThemeDictionaries.TryGetValue(ThemeVariant.Dark, out IThemeVariantProvider? originalDark);
        ThemeSettings originalSettings = ThemeModule.Settings;
        application.Resources.ThemeDictionaries[ThemeVariant.Light] = new ResourceDictionary();
        application.Resources.ThemeDictionaries[ThemeVariant.Dark] = new ResourceDictionary();
        NativeToolStripSeparatorChrome chrome = new();
        chrome[!NativeToolStripSeparatorChrome.UseSystemVisualStyleProperty] = new DynamicResourceExtension("GitExtensionsToolStripSeparatorUseSystemVisualStyle");
        chrome[!NativeToolStripSeparatorChrome.DarkBrushProperty] = new DynamicResourceExtension("GitExtensionsToolStripSeparatorDarkBrush");
        chrome[!NativeToolStripSeparatorChrome.LightBrushProperty] = new DynamicResourceExtension("GitExtensionsToolStripSeparatorLightBrush");
        Border owner = new() { Width = 6, Height = 25, Child = chrome };
        Window window = new()
        {
            Width = 80,
            Height = 60,
            RequestedThemeVariant = ThemeVariant.Light,
            Content = new Canvas { Background = Brushes.White, Children = { owner } },
        };
        try
        {
            foreach ((bool dark, bool system) in new[] { (false, true), (true, false), (false, false), (true, true) })
            {
                Theme theme = new(
                    new Dictionary<AppColor, DrawingColor> { [AppColor.PanelBackground] = dark ? DrawingColor.Black : DrawingColor.White },
                    new Dictionary<KnownColor, DrawingColor>(),
                    new ThemeId(dark ? "native-separator-dark" : "native-separator-light"));
                AvaloniaThemeResources.Apply(application, new ThemeSettings(theme, Theme.Default, ThemeVariations.None, useSystemVisualStyle: system));
                window.RequestedThemeVariant = dark ? ThemeVariant.Dark : ThemeVariant.Light;
                window.Show();
                window.UpdateLayout();
                Dispatcher.UIThread.RunJobs();
                owner.Bounds.Size.Should().Be(new Avalonia.Size(6, 25));
                chrome.Bounds.Size.Should().Be(owner.Bounds.Size);
                chrome.UseSystemVisualStyle.Should().Be(system);
                using WriteableBitmap frame = Capture(window);
                if (system)
                {
                    ReadPixel(frame, new PixelPoint(2, 2)).Should().Be(Color.Parse("#8C8C8C"));
                    ReadPixel(frame, new PixelPoint(3, 22)).Should().Be(Colors.White);
                    ReadPixel(frame, new PixelPoint(4, 6)).Should().Be(Colors.White);
                }
                else
                {
                    ReadPixel(frame, new PixelPoint(3, 5)).Should().Be(Color.Parse(dark ? "#404040" : "#BDBDBD"));
                    ReadPixel(frame, new PixelPoint(4, 6)).Should().Be(Color.Parse(dark ? "#101010" : "#FFFFFF"));
                }
            }
        }
        finally
        {
            window.Close();
            if (hadLight)
            {
                application.Resources.ThemeDictionaries[ThemeVariant.Light] = originalLight!;
            }
            else
            {
                application.Resources.ThemeDictionaries.Remove(ThemeVariant.Light);
            }

            if (hadDark)
            {
                application.Resources.ThemeDictionaries[ThemeVariant.Dark] = originalDark!;
            }
            else
            {
                application.Resources.ThemeDictionaries.Remove(ThemeVariant.Dark);
            }

            ColorHelper.ThemeSettings = originalSettings;
        }
    }

    private static WriteableBitmap Capture(Window window)
        => window.CaptureRenderedFrame() ?? throw new InvalidOperationException("The toolbar separator frame is unavailable.");

    private static Color GetColor(IBrush brush) => ((ISolidColorBrush)brush).Color;

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
