using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitExtUtils.GitUI.Theming;
using GitUI.Compat;
using GitUI.Theming;
using SkiaSharp;
using Color = Avalonia.Media.Color;
using ColorHelper = GitExtUtils.GitUI.Theming.ColorHelper;
using DrawingColor = System.Drawing.Color;
using KnownColor = System.Drawing.KnownColor;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class MenuPaintTests
{
    private static readonly StyledProperty<IImage?> TestSourceProperty =
        AvaloniaProperty.Register<AvaloniaObject, IImage?>("MenuPaintTestSource");

    [AvaloniaTest]
    [TestCase("menu", "light")]
    [TestCase("menu", "dark")]
    [TestCase("menu", "custom")]
    [TestCase("context", "light")]
    [TestCase("context", "dark")]
    [TestCase("context", "custom")]
    [TestCase("flyout", "light")]
    [TestCase("flyout", "dark")]
    [TestCase("flyout", "custom")]
    public void Separator_should_paint_one_native_integer_row_in_the_actual_popup(string owner, string theme)
    {
        using MenuFixture fixture = new(owner, theme, initiallyDisabled: false);
        fixture.AssertSeparator();
        fixture.Separator.Height = 7;
        fixture.Layout();
        fixture.AssertSeparator();
        fixture.Separator.Height = 6;
        fixture.ApplyTheme(theme == "dark" ? "light" : "dark");
        fixture.AssertSeparator();
        fixture.ApplyTheme("custom");
        fixture.AssertSeparator();
    }

    [AvaloniaTest]
    [TestCase("menu", "light")]
    [TestCase("menu", "dark")]
    [TestCase("menu", "custom")]
    [TestCase("context", "light")]
    [TestCase("context", "dark")]
    [TestCase("context", "custom")]
    [TestCase("flyout", "light")]
    [TestCase("flyout", "dark")]
    [TestCase("flyout", "custom")]
    public void Disabled_icon_should_transform_actual_template_pixels_and_restore_the_original_enabled_image(string owner, string theme)
    {
        using MenuFixture fixture = new(owner, theme, initiallyDisabled: true);
        NativeMenuImage.GetUseDisabledRendering(fixture.Image).Should().BeTrue();
        fixture.Image.Source.Should().NotBeSameAs(fixture.Source);
        SKColor disabled = fixture.IconPixel();
        disabled.Red.Should().Be(disabled.Green);
        disabled.Green.Should().Be(disabled.Blue);
        fixture.Item.IsEnabled = true;
        fixture.Layout();
        fixture.Image.Source.Should().BeSameAs(fixture.Source);
        fixture.IconPixel().Should().Be(new SKColor(225, 29, 29));
        fixture.Item.IsEnabled = false;
        fixture.Layout();
        fixture.IconPixel().Should().Be(disabled);
        fixture.AssertSeparator();
        fixture.Item.IsEnabled = true;
        fixture.Layout();
        fixture.Image.Source.Should().BeSameAs(fixture.Source);
        fixture.IconPixel().Should().Be(new SKColor(225, 29, 29));
        ReadPixel(fixture.Source).Should().Be(Color.FromRgb(225, 29, 29));
    }

    [AvaloniaTest]
    [TestCase("light", "dark")]
    [TestCase("dark", "light")]
    public void Disabled_matrix_should_be_chosen_on_the_first_actual_disabled_draw_and_survive_theme_changes(string first, string second)
    {
        using MenuFixture fixture = new("context", first, initiallyDisabled: false);
        fixture.IconPixel().Should().Be(new SKColor(225, 29, 29));
        fixture.DisabledMode.Should().Be(-1, "an enabled render must not initialize ToolStripRenderer's static matrix");
        fixture.ApplyTheme(second);
        fixture.DisabledMode.Should().Be(-1, "theme publication and style attachment are not disabled-image draws");
        fixture.Item.IsEnabled = false;
        fixture.Layout();
        fixture.IconPixel();
        fixture.DisabledMode.Should().Be(second == "dark" ? 1 : 0);
        Color initial = fixture.DisabledPixel();
        fixture.ApplyTheme(first);
        fixture.Item.IsEnabled = true;
        fixture.Layout();
        fixture.Item.IsEnabled = false;
        fixture.Layout();
        fixture.DisabledPixel().Should().Be(initial, "the source matrix retains its first-draw color mode across later themes");
        fixture.DisabledMode.Should().Be(second == "dark" ? 1 : 0);
    }

    [AvaloniaTest]
    public void Live_source_binding_and_reparenting_should_not_repeat_conversion_or_retain_a_detached_owner()
    {
        using MenuFixture fixture = new("context", "light", initiallyDisabled: true);
        using WriteableBitmap replacement = CreateSource(Color.FromRgb(22, 88, 210));
        AvaloniaObject source = new();
        source.SetValue(TestSourceProperty, fixture.Source);
        using IDisposable binding = fixture.Image.Bind(Image.SourceProperty, source.GetObservable(TestSourceProperty));
        fixture.Layout();
        fixture.IconPixel();
        source.SetValue(TestSourceProperty, replacement);
        fixture.Layout();
        fixture.Image.Source.Should().NotBeSameAs(replacement);
        Color converted = fixture.DisabledPixel();
        fixture.Item.IsEnabled = true;
        fixture.Layout();
        fixture.Image.Source.Should().BeSameAs(replacement);
        fixture.IconPixel().Should().Be(new SKColor(22, 88, 210));
        fixture.Item.IsEnabled = false;
        fixture.Layout();
        fixture.DisabledPixel().Should().Be(converted);
        fixture.Item.Icon = null;
        fixture.Layout();
        fixture.Image.Source.Should().BeSameAs(replacement, "detachment must release only the adapter's presentation value");
        fixture.OtherItem.Icon = fixture.Image;
        fixture.Layout();
        fixture.Image.IsEffectivelyEnabled.Should().BeTrue();
        fixture.Image.Source.Should().BeSameAs(replacement);
        fixture.OtherItem.IsEnabled = false;
        fixture.Layout();
        fixture.DisabledPixel().Should().Be(converted);
        NativeMenuImage.SetUseDisabledRendering(fixture.Image, false);
        fixture.Layout();
        fixture.Image.Source.Should().BeSameAs(replacement);
        source.SetValue(TestSourceProperty, fixture.Source);
        fixture.Layout();
        fixture.Image.Source.Should().BeSameAs(fixture.Source, "opting out must preserve the caller's still-live binding");
    }

    [AvaloniaTest]
    public void Retiring_disabled_variants_should_release_only_owned_bitmaps_and_keep_recorded_menu_frames_valid()
    {
        using MenuFixture fixture = new("context", "light", initiallyDisabled: true);
        fixture.IconPixel();
        Bitmap enabledRetirement = ConvertedBitmap(fixture.Image);
        fixture.Item.IsEnabled = true;
        fixture.Layout();
        AssertDisposed(enabledRetirement);
        fixture.IconPixel().Should().Be(new SKColor(225, 29, 29));
        fixture.Item.IsEnabled = false;
        fixture.Layout();
        fixture.IconPixel();
        Bitmap sourceRetirement = ConvertedBitmap(fixture.Image);
        using WriteableBitmap replacement = CreateSource(Color.FromRgb(22, 88, 210));
        fixture.Image.Source = replacement;
        fixture.Layout();
        fixture.IconPixel();
        AssertDisposed(sourceRetirement);
        Bitmap detachRetirement = ConvertedBitmap(fixture.Image);
        fixture.Item.Icon = null;
        fixture.Layout();
        AssertDisposed(detachRetirement);
        fixture.Image.Source.Should().BeSameAs(replacement);
        fixture.Item.Icon = fixture.Image;
        fixture.Layout();
        fixture.IconPixel();
        Bitmap optOutRetirement = ConvertedBitmap(fixture.Image);
        NativeMenuImage.SetUseDisabledRendering(fixture.Image, false);
        fixture.Layout();
        AssertDisposed(optOutRetirement);
        fixture.IconPixel().Should().Be(new SKColor(22, 88, 210));
        ReadPixel(fixture.Source).Should().Be(Color.FromRgb(225, 29, 29));
        ReadPixel(replacement).Should().Be(Color.FromRgb(22, 88, 210));
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Disabled_bitmap_should_preserve_the_original_frameworks_independent_premultiplied_alpha_vectors(bool dark)
    {
        using MenuFixture fixture = new("context", dark ? "dark" : "light", initiallyDisabled: true);
        byte[] alpha = [0, 1, 2, 32, 64, 127, 128, 192, 254, 255, 105, 185, 245];
        string[] expected = dark
            ? ["00000000", "01000000", "02000000", "1A030303", "33060606", "660C0C0C", "660C0C0C", "9A131313", "CB191919", "CC191919", "540A0A0A", "94121212", "C4181818"]
            : ["00000000", "01010101", "01010101", "160D0D0D", "2D1B1B1B", "59353535", "5A363636", "86505050", "B26B6B6B", "B26B6B6B", "492C2C2C", "814D4D4D", "AB676767"];
        for (int index = 0; index < alpha.Length; index++)
        {
            using WriteableBitmap source = CreateSource(Color.FromArgb(alpha[index], 225, 29, 29));
            fixture.Image.Source = source;
            fixture.Layout();
            fixture.IconPixel();
            WriteableBitmap converted = (WriteableBitmap)ConvertedBitmap(fixture.Image);
            converted.AlphaFormat.Should().Be(AlphaFormat.Premul);
            ReadPremultipliedPixel(converted).Should().Be(Color.Parse($"#{expected[index]}"),
                "original ToolStripRenderer owned-bitmap vectors retain GDI+ matrix rounding at source alpha {0}", alpha[index]);
            ReadPixel(source).Should().Be(Color.FromArgb(alpha[index], 225, 29, 29));
            fixture.Image.Source = fixture.Source;
            fixture.Layout();
        }
    }

    [AvaloniaTest]
    public void Disabled_gray_should_preserve_the_original_single_precision_half_boundary()
    {
        using MenuFixture fixture = new("context", "dark", initiallyDisabled: true);
        using WriteableBitmap source = CreateSource(Color.FromArgb(204, 160, 0, 0));
        fixture.Image.Source = source;
        fixture.Layout();
        fixture.IconPixel();
        ReadPremultipliedPixel((WriteableBitmap)ConvertedBitmap(fixture.Image)).Should().Be(Color.Parse("#A3050505"),
            "the native matrix computes float gray 8.5 and rounds to even before premultiplying; double arithmetic rounds it to 9");
        fixture.Image.Source = fixture.Source;
        fixture.Layout();
    }

    private static Color ReadPremultipliedPixel(WriteableBitmap bitmap)
    {
        using ILockedFramebuffer pixels = bitmap.Lock();
        return Color.FromUInt32(unchecked((uint)Marshal.ReadInt32(pixels.Address)));
    }

    private static void AssertDisposed(Bitmap bitmap)
    {
        Action read = () => ReadPixel(bitmap);
        read.Should().Throw<ObjectDisposedException>("each retired disabled presentation owns and disposes its converted bitmap");
    }

    private static Bitmap ConvertedBitmap(Image image)
    {
        IImage wrapper = image.Source ?? throw new AssertionException("The disabled image must retain its renderer wrapper.");

        // parity-scaffolding: verify the real owned native allocation is retired,
        // rather than adding an observable disposal API to the shipping adapter.
        return wrapper.GetType().GetField("_bitmap", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(wrapper) as Bitmap
            ?? throw new AssertionException("An actual disabled render must have created its owned bitmap.");
    }

    private static WriteableBitmap CreateSource(Color color)
    {
        WriteableBitmap source = new(new PixelSize(16, 16), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Unpremul);
        using ILockedFramebuffer pixels = source.Lock();
        int argb = (color.A << 24) | (color.R << 16) | (color.G << 8) | color.B;
        for (int y = 0; y < pixels.Size.Height; y++)
        {
            for (int x = 0; x < pixels.Size.Width; x++)
            {
                Marshal.WriteInt32(pixels.Address, (y * pixels.RowBytes) + (x * 4), argb);
            }
        }

        return source;
    }

    private static Color ReadPixel(Bitmap source)
    {
        using WriteableBitmap copy = new(source.PixelSize, source.Dpi, PixelFormat.Bgra8888, AlphaFormat.Unpremul);
        using ILockedFramebuffer pixels = copy.Lock();
        source.CopyPixels(pixels);
        return Color.FromUInt32(unchecked((uint)Marshal.ReadInt32(pixels.Address)));
    }

    private sealed class MenuFixture : IDisposable
    {
        private static readonly FieldInfo DisabledModeField = typeof(NativeMenuImage)
            .GetField("_disabledImageDarkMode", BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw new AssertionException("The source first-draw matrix must have one process-lifetime mode.");
        private readonly Application _application;
        private readonly ThemeSettings _originalSettings;
        private readonly Dictionary<ThemeVariant, IThemeVariantProvider?> _originalResources = [];
        private readonly int _originalMode;
        private readonly ContextMenu? _context;
        private readonly MenuFlyout? _flyout;
        private readonly MenuItem? _title;

        public MenuFixture(string owner, string theme, bool initiallyDisabled)
        {
            _application = Application.Current ?? throw new AssertionException("The Avalonia application must be initialized.");
            _originalSettings = ThemeModule.Settings;
            _originalMode = DisabledMode;

            // parity-scaffolding: isolate the original renderer's process-global first-use
            // behavior without exposing a reset in shipping code.
            DisabledModeField.SetValue(null, -1);
            foreach (ThemeVariant variant in new[] { ThemeVariant.Light, ThemeVariant.Dark })
            {
                _application.Resources.ThemeDictionaries.TryGetValue(variant, out IThemeVariantProvider? original);
                _originalResources.Add(variant, original);
                _application.Resources.ThemeDictionaries[variant] = new ResourceDictionary();
            }

            Source = CreateSource(Color.FromRgb(225, 29, 29));
            Image = new Image { Source = Source, Width = 16, Height = 16, Stretch = Stretch.None };
            Item = new MenuItem { Header = "_Delete branch", Icon = Image, IsEnabled = !initiallyDisabled };
            OtherItem = new MenuItem { Header = "_Other command" };
            Separator = new Separator();
            // Native ToolStrip owners have integer client coordinates. Centering this
            // odd-height fixture button creates an unrelated half-pixel popup origin.
            Button button = new()
            {
                Width = 100,
                Height = 25,
                Content = "Open",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
            };
            Window = new Window { Width = 600, Height = 250, Content = button };
            ApplyTheme(theme);
            switch (owner)
            {
                case "menu":
                    _title = new MenuItem { Header = "_Commands", Items = { Item, Separator, OtherItem } };
                    Window.Content = new Menu { Items = { _title }, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top };
                    break;
                case "context":
                    _context = new ContextMenu { Items = { Item, Separator, OtherItem } };
                    button.ContextMenu = _context;
                    break;
                case "flyout":
                    _flyout = new MenuFlyout { Items = { Item, Separator, OtherItem } };
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(owner));
            }

            Window.Show();
            Layout();
            if (_title is not null)
            {
                _title.IsSubMenuOpen = true;
            }

            _context?.Open(button);
            _flyout?.ShowAt(button);
            Layout();
            RetainDiagnostics();
        }

        public Window Window { get; }

        public MenuItem Item { get; }

        public MenuItem OtherItem { get; }

        public Separator Separator { get; }

        public Image Image { get; }

        public WriteableBitmap Source { get; }

        public int DisabledMode => (int)(DisabledModeField.GetValue(null)
            ?? throw new AssertionException("The disabled renderer's mode must be available."));

        public void ApplyTheme(string name)
        {
            Dictionary<AppColor, DrawingColor> colors = new()
            {
                [AppColor.PanelBackground] = name == "dark" ? DrawingColor.FromArgb(32, 32, 32) : DrawingColor.White,
            };
            Dictionary<KnownColor, DrawingColor> system = [];
            if (name == "custom")
            {
                colors[AppColor.PanelBackground] = DrawingColor.FromArgb(245, 241, 220);
                system[KnownColor.Window] = DrawingColor.FromArgb(245, 241, 220);
                system[KnownColor.WindowText] = DrawingColor.FromArgb(35, 55, 25);
                system[KnownColor.ControlDark] = DrawingColor.Magenta;
                system[KnownColor.GrayText] = DrawingColor.Blue;
            }

            Theme theme = new(colors, system, new ThemeId($"menu-paint-{name}"));
            AvaloniaThemeResources.Apply(_application, new ThemeSettings(theme, Theme.Default, ThemeVariations.None, useSystemVisualStyle: true));
            Window.RequestedThemeVariant = name == "dark" ? ThemeVariant.Dark : ThemeVariant.Light;
            Layout();
        }

        public void Layout()
        {
            Dispatcher.UIThread.RunJobs();
            Window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
        }

        public SKColor IconPixel()
        {
            TopLevel root = TopLevel.GetTopLevel(Image) ?? throw new AssertionException("The actual popup image must be attached.");
            using SKBitmap pixels = Capture(root);
            Point point = Image.TranslatePoint(new Point(8.5, 8.5), root)
                ?? throw new AssertionException("The icon pixel must have an actual popup coordinate.");
            return pixels.GetPixel((int)point.X, (int)point.Y);
        }

        public Color DisabledPixel()
        {
            IImage image = Image.Source ?? throw new AssertionException("The disabled menu must retain its actual image source.");
            Border canvas = new() { Width = 16, Height = 16, Child = new Image { Source = image, Stretch = Stretch.None } };
            canvas.Measure(new Avalonia.Size(16, 16));
            canvas.Arrange(new Rect(0, 0, 16, 16));
            using RenderTargetBitmap pixels = new(new PixelSize(16, 16));
            pixels.Render(canvas);
            return ReadPixel(pixels);
        }

        public void AssertSeparator()
        {
            NativeMenuSeparatorChrome chrome = Separator.GetVisualDescendants().OfType<NativeMenuSeparatorChrome>().Single();
            chrome.Bounds.Height.Should().Be(Separator.Height);
            TopLevel root = TopLevel.GetTopLevel(Separator) ?? throw new AssertionException("The separator must belong to the open popup.");
            SKColor line = ResourceColor("GitExtensionsToolStripSeparatorDarkBrush");
            SKColor background = ResourceColor("GitExtensionsMenuRenderedBackgroundBrush");
            using SKBitmap pixels = Capture(root);
            for (int y = 0; y < (int)chrome.Bounds.Height; y++)
            {
                Point point = chrome.TranslatePoint(new Point(40.5, y + 0.5), root)
                    ?? throw new AssertionException("The separator row must have an actual popup coordinate.");
                pixels.GetPixel((int)point.X, (int)point.Y).Should().Be(y == (int)chrome.Bounds.Height / 2 ? line : background,
                    "the original dropdown paints only its integer center row, including after live palette changes");
            }
        }

        public void Dispose()
        {
            if (_title is not null)
            {
                _title.IsSubMenuOpen = false;
            }

            _context?.Close();
            _flyout?.Hide();
            Window.Close();
            Dispatcher.UIThread.RunJobs();
            Source.Dispose();
            foreach ((ThemeVariant variant, IThemeVariantProvider? resource) in _originalResources)
            {
                if (resource is null)
                {
                    _application.Resources.ThemeDictionaries.Remove(variant);
                }
                else
                {
                    _application.Resources.ThemeDictionaries[variant] = resource;
                }
            }

            ColorHelper.ThemeSettings = _originalSettings;
            DisabledModeField.SetValue(null, _originalMode);
        }

        private SKColor ResourceColor(string key)
        {
            Window.TryFindResource(key, Window.ActualThemeVariant, out object? brush).Should().BeTrue();
            Color color = ((ISolidColorBrush)brush!).Color;
            return new SKColor(color.R, color.G, color.B, color.A);
        }

        private void RetainDiagnostics()
        {
            // parity-scaffolding: retain the actual failed integration's pixels and
            // materialized template, not a synthetic replacement for the popup.
            string? evidenceRoot = Environment.GetEnvironmentVariable("GITEXT_MENU_PAINT_EVIDENCE");
            if (string.IsNullOrWhiteSpace(evidenceRoot))
            {
                return;
            }

            string directory = Path.Combine(Path.GetFullPath(evidenceRoot), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            TopLevel root = TopLevel.GetTopLevel(Image) ?? throw new AssertionException("The diagnostic popup must be rendered.");
            using WriteableBitmap pixels = root.CaptureRenderedFrame()
                ?? throw new AssertionException("The diagnostic must retain the actual rendered menu frame.");
            pixels.Save(Path.Combine(directory, "actual-popup.png"), PngBitmapEncoderOptions.Default);
            object[] controls = root.GetVisualDescendants().OfType<Control>().Select(control => new
            {
                type = control.GetType().FullName,
                control.Name,
                classes = control.Classes.ToArray(),
                bounds = control.Bounds.ToString(),
                origin = control.TranslatePoint(default, root)?.ToString(),
                visualParent = control.GetVisualParent()?.GetType().FullName,
                logicalParent = control.Parent?.GetType().FullName,
                templatedParent = control.TemplatedParent?.GetType().FullName,
                control.IsVisible,
                control.IsEffectivelyEnabled,
                disabledImageOptIn = control is Image image ? NativeMenuImage.GetUseDisabledRendering(image) : (bool?)null,
                sourceType = (control as Image)?.Source?.GetType().FullName,
                background = BrushValue(control switch
                {
                    Border border => border.Background,
                    Panel panel => panel.Background,
                    ContentPresenter presenter => presenter.Background,
                    TemplatedControl templated => templated.Background,
                    _ => null,
                }),
            }).Cast<object>().ToArray();
            Dictionary<string, string?> resources = [];
            foreach (string key in new[]
            {
                "GitExtensionsMenuRenderedBackgroundBrush", "GitExtensionsMenuRenderedBorderBrush",
                "GitExtensionsToolStripSeparatorDarkBrush", "GitExtensionsNativeFlatButtonDarkMode",
            })
            {
                Window.TryFindResource(key, Window.ActualThemeVariant, out object? resource);
                resources.Add(key, resource is IBrush brush ? BrushValue(brush) : resource?.ToString());
            }

            File.WriteAllText(Path.Combine(directory, "actual-popup.json"), JsonSerializer.Serialize(new
            {
                test = TestContext.CurrentContext.Test.FullName,
                actualTheme = Window.ActualThemeVariant.ToString(),
                root = root.GetType().FullName,
                resources,
                controls,
            }, new JsonSerializerOptions { WriteIndented = true }));
            TestContext.Progress.WriteLine($"GITEXT_MENU_PAINT_EVIDENCE {directory}");
        }

        private static string? BrushValue(IBrush? brush)
            => brush is ISolidColorBrush solid ? $"{solid.Color} opacity={solid.Opacity}" : brush?.ToString();

        private static SKBitmap Capture(TopLevel root)
        {
            using WriteableBitmap frame = root.CaptureRenderedFrame() ?? throw new AssertionException("The popup must paint actual pixels.");
            using MemoryStream stream = new();
            frame.Save(stream, PngBitmapEncoderOptions.Default);
            stream.Position = 0;
            return SKBitmap.Decode(stream) ?? throw new AssertionException("The popup frame must decode.");
        }
    }
}
