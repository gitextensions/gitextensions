using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
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
public sealed class MenuArrowPaintTests
{
    private const string NormalBrushKey = "GitExtensionsNativeToolStripArrowBrush";
    private static readonly int[][] NativeRightRows = [[-2], [-2, -1], [-2, -1, 0], [-2, -1, 0, 1], [-2, -1, 0], [-2, -1], [-2]];
    private static readonly int[][] NativeLeftRows = [[1], [0, 1], [-1, 0, 1], [-2, -1, 0, 1], [-1, 0, 1], [0, 1], [1]];

    [AvaloniaTest]
    [TestCase("even", false)]
    [TestCase("even", true)]
    [TestCase("odd-offset", false)]
    [TestCase("odd-offset", true)]
    [TestCase("source-context-row", false)]
    [TestCase("source-context-row", true)]
    [TestCase("short", false)]
    [TestCase("short", true)]
    [TestCase("clipped", false)]
    [TestCase("clipped", true)]
    public void Render_should_match_actual_GDI_row_runs_for_integer_odd_offset_and_clipped_rectangles(string shape, bool rightToLeft)
    {
        (Rect rectangle, PixelSize size) = shape switch
        {
            "even" => (new Rect(0, 0, 10, 20), new PixelSize(18, 28)),
            "odd-offset" => (new Rect(3, 5, 11, 21), new PixelSize(22, 34)),
            "source-context-row" => (new Rect(rightToLeft ? 8 : 247, 1, 10, 20), new PixelSize(265, 22)),
            "short" => (new Rect(2, 1, 10, 11), new PixelSize(18, 13)),
            "clipped" => (new Rect(-5, -8, 10, 20), new PixelSize(10, 14)),
            _ => throw new ArgumentOutOfRangeException(nameof(shape)),
        };
        NativeToolStripMenuArrow arrow = new()
        {
            Width = rectangle.Width,
            Height = rectangle.Height,
            FlowDirection = rightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
        };
        Canvas canvas = new() { Width = size.Width, Height = size.Height, Background = Brushes.White, Children = { arrow } };
        Canvas.SetLeft(arrow, rectangle.X);
        Canvas.SetTop(arrow, rectangle.Y);
        canvas.Measure(new Size(size.Width, size.Height));
        canvas.Arrange(new Rect(0, 0, size.Width, size.Height));
        foreach (Color color in new[] { Colors.Black, Colors.White, Color.Parse("#FFA0A0A0"), Color.Parse("#FF4A4A4A") })
        {
            arrow.Foreground = new SolidColorBrush(color);
            using RenderTargetBitmap frame = new(size);
            frame.Render(canvas);
            using SKBitmap pixels = Decode(frame);
            int middleX = (int)rectangle.X + ((int)rectangle.Width / 2);
            int middleY = (int)rectangle.Y + ((int)rectangle.Height / 2);
            for (int y = 0; y < size.Height; y++)
            {
                for (int x = 0; x < size.Width; x++)
                {
                    pixels.GetPixel(x, y).Should().Be(IsNativeInk(x - middleX, y - middleY, rightToLeft)
                        ? Pixel(color) : SKColors.White, "original bitmap coverage at ({0},{1}) for {2}, RTL={3}", x, y, shape, rightToLeft);
                }
            }
        }

        arrow.IsHitTestVisible.Should().BeFalse();
        arrow.Focusable.Should().BeFalse();
    }

    [AvaloniaTest]
    [TestCase("menu", "light", false)]
    [TestCase("menu", "light", true)]
    [TestCase("menu", "dark", false)]
    [TestCase("menu", "dark", true)]
    [TestCase("menu", "custom", false)]
    [TestCase("menu", "custom", true)]
    [TestCase("context", "light", false)]
    [TestCase("context", "light", true)]
    [TestCase("context", "dark", false)]
    [TestCase("context", "dark", true)]
    [TestCase("context", "custom", false)]
    [TestCase("context", "custom", true)]
    [TestCase("flyout", "light", false)]
    [TestCase("flyout", "light", true)]
    [TestCase("flyout", "dark", false)]
    [TestCase("flyout", "dark", true)]
    [TestCase("flyout", "custom", false)]
    [TestCase("flyout", "custom", true)]
    public void Actual_popup_should_use_the_source_arrow_slot_and_Professional_palette_across_live_states_and_themes(
        string owner, string theme, bool rightToLeft)
    {
        using MenuFixture fixture = new(owner, theme, rightToLeft, hasChildren: true);
        fixture.Item.Foreground = Brushes.Magenta;
        fixture.AssertArrow();
        fixture.Item.IsSelected = true;
        fixture.Layout();
        fixture.AssertArrow();
        fixture.Item.IsEnabled = false;
        fixture.Layout();
        fixture.AssertArrow();
        fixture.Item.IsEnabled = true;
        fixture.Layout();
        fixture.AssertArrow();
        fixture.Item.IsSubMenuOpen = true;
        fixture.Layout();
        fixture.AssertArrow();
        fixture.Item.IsSubMenuOpen = false;
        fixture.ApplyTheme(theme == "light" ? "dark" : "light");
        fixture.AssertArrow();
        fixture.ApplyTheme("custom");
        fixture.AssertArrow();
        fixture.Item.IsEnabled = false;
        fixture.Layout();
        fixture.AssertArrow();
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void System_filter_selection_should_keep_native_HighlightText_instead_of_the_Professional_override(bool rightToLeft)
    {
        using MenuFixture fixture = new("context", "light", rightToLeft, hasChildren: true);
        fixture.Item.Classes.Add("gitextensions-filter-menu");
        fixture.Item.Classes.Add("gitextensions-filter-menu-system-renderer");
        fixture.Item.IsSelected = true;
        fixture.Layout();
        fixture.AssertArrow(systemSelected: true);
        fixture.Item.IsEnabled = false;
        fixture.Layout();
        fixture.AssertArrow();
        fixture.Item.IsEnabled = true;
        fixture.Item.IsSelected = false;
        fixture.Layout();
        fixture.AssertArrow();
        fixture.Item.IsSubMenuOpen = true;
        fixture.Item.IsSelected = false;
        fixture.Layout();
        fixture.AssertArrow(systemSelected: true);
        fixture.Item.IsSubMenuOpen = false;
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Initially_disabled_popup_should_paint_ControlDark_without_caption_opacity_and_restore_ControlText(bool rightToLeft)
    {
        using MenuFixture fixture = new("context", "light", rightToLeft, hasChildren: true, initiallyDisabled: true);
        fixture.AssertArrow();
        fixture.Item.IsEnabled = true;
        fixture.Layout();
        fixture.AssertArrow();
        fixture.ApplyTheme("dark");
        fixture.Item.IsEnabled = false;
        fixture.Layout();
        fixture.AssertArrow();
    }

    [AvaloniaTest]
    [TestCase("menu", false)]
    [TestCase("menu", true)]
    [TestCase("context", false)]
    [TestCase("context", true)]
    [TestCase("flyout", false)]
    [TestCase("flyout", true)]
    public void Empty_rows_should_reserve_the_same_shared_column_and_react_to_live_children_and_width_changes(string owner, bool rightToLeft)
    {
        using MenuFixture fixture = new(owner, "light", rightToLeft, hasChildren: false);
        Rect header = Rectangle(Part<ContentPresenter>(fixture.Item, "PART_HeaderPresenter"), fixture.Item);
        Rect gesture = Rectangle(Part<TextBlock>(fixture.Item, "PART_InputGestureText"), fixture.Item);
        Border slot = Part<Border>(fixture.Item, "PART_ChevronSlot");
        double reserved = slot.DesiredSize.Width;
        fixture.Item.TryFindResource("MenuFlyoutItemChevronMargin", fixture.Item.ActualThemeVariant, out object? margin).Should().BeTrue();
        Thickness chevronMargin = margin.Should().BeOfType<Thickness>().Subject;
        reserved.Should().Be(4 + chevronMargin.Left + chevronMargin.Right);
        fixture.AssertArrow();
        MenuItem child = new() { Header = "Child" };
        fixture.Item.Items.Add(child);
        fixture.Layout();
        fixture.Item.HasSubMenu.Should().BeTrue();
        fixture.AssertArrow();
        Rectangle(Part<ContentPresenter>(fixture.Item, "PART_HeaderPresenter"), fixture.Item).Should().Be(header);
        Rectangle(Part<TextBlock>(fixture.Item, "PART_InputGestureText"), fixture.Item).Should().Be(gesture);
        slot.DesiredSize.Width.Should().Be(reserved);
        fixture.Item.Items.Remove(child);
        fixture.Layout();
        fixture.Item.HasSubMenu.Should().BeFalse();
        fixture.AssertArrow();
        Rectangle(Part<ContentPresenter>(fixture.Item, "PART_HeaderPresenter"), fixture.Item).Should().Be(header);
        Rectangle(Part<TextBlock>(fixture.Item, "PART_InputGestureText"), fixture.Item).Should().Be(gesture);
        slot.DesiredSize.Width.Should().Be(reserved);
        double previousWidth = fixture.Item.Bounds.Width;
        fixture.OtherItem.Header = "A substantially longer literal command caption that changes the shared source width";
        fixture.SizeMenu();
        fixture.Item.Bounds.Width.Should().BeGreaterThan(previousWidth);
        fixture.Item.Bounds.Width.Should().Be(fixture.OtherItem.Bounds.Width);
        slot.DesiredSize.Width.Should().Be(reserved);
        fixture.Item.Items.Add(child);
        fixture.Layout();
        fixture.AssertArrow();
        fixture.Title?.GetVisualDescendants().OfType<NativeToolStripMenuArrow>().Should().BeEmpty("the main-menu header has no submenu glyph");
        fixture.AssertExplicitMaximumIsPreserved();
    }

    [AvaloniaTest]
    public void Actual_template_should_repaint_mutable_and_replaced_resources_after_reopening_and_direction_changes()
    {
        using MenuFixture fixture = new("context", "light", rightToLeft: false, hasChildren: true);
        SolidColorBrush brush = new(Colors.Red);
        fixture.Item.Resources[NormalBrushKey] = brush;
        fixture.Layout();
        fixture.AssertArrow(overrideColor: Colors.Red);
        brush.Color = Colors.Blue;
        fixture.Layout();
        fixture.AssertArrow(overrideColor: Colors.Blue);
        fixture.Item.Resources[NormalBrushKey] = new SolidColorBrush(Colors.Green);
        fixture.Layout();
        fixture.AssertArrow(overrideColor: Colors.Green);
        fixture.ReopenRightToLeft();
        fixture.AssertArrow(overrideColor: Colors.Green);
        fixture.ReparentToNewContext();
        fixture.AssertArrow(overrideColor: Colors.Green);
        fixture.Item.Resources.Remove(NormalBrushKey);
        fixture.Layout();
        fixture.AssertArrow();
        fixture.AssertNonMenuFlyoutMaximumIsUnchanged();
    }

    private static bool IsNativeInk(int x, int y, bool rightToLeft)
    {
        // Retained original DrawArrow bitmaps independently establish these seven
        // row runs; the source vertex rows at +/-4 contain no pixels.
        int[][] rows = rightToLeft ? NativeLeftRows : NativeRightRows;
        return y is >= -3 and <= 3 && rows[y + 3].Contains(x);
    }

    private static T Part<T>(MenuItem item, string name)
        where T : Control
        => item.GetVisualDescendants().OfType<T>().Single(control => control.Name == name);

    private static Rect Rectangle(Control control, Visual relativeTo)
    {
        Point first = control.TranslatePoint(default, relativeTo)
            ?? throw new AssertionException("The actual template part must have a visual coordinate.");
        Point second = control.TranslatePoint(new Point(control.Bounds.Width, control.Bounds.Height), relativeTo)
            ?? throw new AssertionException("The complete transformed rectangle must belong to the same visual.");
        return new Rect(Math.Min(first.X, second.X), Math.Min(first.Y, second.Y), Math.Abs(second.X - first.X), Math.Abs(second.Y - first.Y));
    }

    private static SKColor Pixel(Color color) => new(color.R, color.G, color.B, color.A);

    private static SKBitmap Decode(Bitmap frame)
    {
        using MemoryStream stream = new();
        frame.Save(stream, PngBitmapEncoderOptions.Default);
        stream.Position = 0;
        return SKBitmap.Decode(stream) ?? throw new AssertionException("The actual painted frame must decode.");
    }

    private sealed class MenuFixture : IDisposable
    {
        private readonly Application _application;
        private readonly ThemeSettings _originalSettings;
        private readonly Dictionary<ThemeVariant, IThemeVariantProvider?> _originalResources = [];
        private readonly string _owner;
        private readonly Button _button;
        private readonly ContextMenu? _context;
        private readonly MenuFlyout? _flyout;
        private ContextMenu? _reparented;
        private bool _rightToLeft;
        private string _theme = "light";

        public MenuFixture(string owner, string theme, bool rightToLeft, bool hasChildren, bool initiallyDisabled = false)
        {
            _application = Application.Current ?? throw new AssertionException("The Avalonia application must be initialized.");
            _originalSettings = ThemeModule.Settings;
            _owner = owner;
            _rightToLeft = rightToLeft;
            foreach (ThemeVariant variant in new[] { ThemeVariant.Light, ThemeVariant.Dark })
            {
                _application.Resources.ThemeDictionaries.TryGetValue(variant, out IThemeVariantProvider? original);
                _originalResources.Add(variant, original);
                _application.Resources.ThemeDictionaries[variant] = new ResourceDictionary();
            }

            Item = new MenuItem
            {
                Header = "_Parent command", InputGesture = new KeyGesture(Key.G, KeyModifiers.Control), IsEnabled = !initiallyDisabled,
            };
            if (hasChildren)
            {
                Item.Items.Add(new MenuItem { Header = "Child" });
            }

            OtherItem = new MenuItem { Header = "_Other command", Items = { new MenuItem { Header = "Other child" } } };
            _button = new Button
            {
                Content = "Open", Width = 100, Height = 25,
                HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top,
            };
            Window = new Window
            {
                Width = 900, Height = 250, Content = _button,
                FlowDirection = rightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
            };
            ApplyTheme(theme);
            switch (owner)
            {
                case "menu":
                    Title = new MenuItem { Header = "_Commands", Items = { Item, OtherItem } };
                    Window.Content = new Menu { Items = { Title }, VerticalAlignment = VerticalAlignment.Top };
                    break;
                case "context":
                    _context = new ContextMenu { Items = { Item, OtherItem } };
                    _button.ContextMenu = _context;
                    break;
                case "flyout":
                    _flyout = new MenuFlyout { Items = { Item, OtherItem } };
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(owner));
            }

            Window.Show();
            Layout();
            Open();
            SizeMenu();
        }

        public Window Window { get; }

        public MenuItem Item { get; }

        public MenuItem OtherItem { get; }

        public MenuItem? Title { get; }

        public void ApplyTheme(string theme)
        {
            _theme = theme;
            bool dark = theme != "light";
            Dictionary<AppColor, DrawingColor> colors = new()
            {
                [AppColor.PanelBackground] = dark ? DrawingColor.FromArgb(32, 32, 32) : DrawingColor.White,
            };
            Dictionary<KnownColor, DrawingColor> system = [];
            if (theme == "custom")
            {
                colors[AppColor.PanelBackground] = DrawingColor.FromArgb(43, 45, 58);
                system[KnownColor.ControlText] = DrawingColor.Orange;
                system[KnownColor.MenuText] = DrawingColor.Lime;
                system[KnownColor.ControlDark] = DrawingColor.Magenta;
                system[KnownColor.HighlightText] = DrawingColor.Cyan;
            }

            Theme sourceTheme = new(colors, system, new ThemeId($"menu-arrow-{theme}"));
            AvaloniaThemeResources.Apply(_application, new ThemeSettings(sourceTheme, Theme.Default, ThemeVariations.None, useSystemVisualStyle: !dark));
            Window.RequestedThemeVariant = dark ? ThemeVariant.Dark : ThemeVariant.Light;
            Layout();
        }

        public void AssertArrow(bool systemSelected = false, Color? overrideColor = null)
        {
            RetainDiagnostics();
            NativeToolStripMenuArrow arrow = Part<NativeToolStripMenuArrow>(Item, "PART_ChevronPath");
            Item.FlowDirection.Should().Be(_rightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight);
            bool rightToLeft = Item.FlowDirection == FlowDirection.RightToLeft;
            Rect expected = new(rightToLeft ? 8 : Item.Bounds.Width - 18, 1, 10, 20);
            TopLevel root = TopLevel.GetTopLevel(Item) ?? throw new AssertionException("The row must belong to the actual open popup.");
            Rect row = Rectangle(Item, root);
            if (Item.HasSubMenu)
            {
                // Native ClientBounds are unmirrored. Avalonia's RTL Item visual
                // coordinates are mirrored, so compare both rectangles in the
                // actual root's physical space, just like the retained bitmap.
                Rectangle(arrow, root).Should().Be(new Rect(row.X + expected.X, row.Y + expected.Y, expected.Width, expected.Height),
                    "the original item centers its source ArrowRectangle, not the glyph's ink bounds");
            }

            arrow.IsVisible.Should().Be(Item.HasSubMenu);
            arrow.IsHitTestVisible.Should().BeFalse();
            arrow.Focusable.Should().BeFalse();
            arrow.Opacity.Should().Be(1);
            Item.Opacity.Should().Be(1);
            Color color = overrideColor ?? (!Item.IsEnabled ? Color.Parse(_theme == "light" ? "#FFA0A0A0" : "#FF4A4A4A")
                : systemSelected || _theme != "light" ? Colors.White : Colors.Black);
            if (Item.HasSubMenu)
            {
                arrow.Foreground.Should().BeAssignableTo<ISolidColorBrush>().Which.Color.Should().Be(color);
            }

            using WriteableBitmap frame = root.CaptureRenderedFrame()
                ?? throw new AssertionException("The actual open popup must render.");
            using SKBitmap pixels = Decode(frame);
            (row.X + expected.Left).Should().BeGreaterThanOrEqualTo(0);
            (row.Y + expected.Top).Should().BeGreaterThanOrEqualTo(0);
            (row.X + expected.Right).Should().BeLessThanOrEqualTo(pixels.Width);
            (row.Y + expected.Bottom).Should().BeLessThanOrEqualTo(pixels.Height);
            bool selected = Item.IsSelected || Item.IsPointerOver;
            bool system = Item.Classes.Contains("gitextensions-filter-menu-system-renderer");
            string backgroundKey = system ? selected && Item.IsEnabled ? "GitExtensionsHighlightBackgroundBrush" : "GitExtensionsMenuBackgroundBrush"
                : selected && Item.IsEnabled ? "GitExtensionsNativeMenuSelectedBackgroundBrush" : "GitExtensionsMenuRenderedBackgroundBrush";
            Item.TryFindResource(backgroundKey, Item.ActualThemeVariant, out object? backdrop).Should().BeTrue();
            SKColor background = Pixel(backdrop.Should().BeAssignableTo<ISolidColorBrush>().Subject.Color);
            for (int y = 0; y < (int)expected.Height; y++)
            {
                for (int x = 0; x < (int)expected.Width; x++)
                {
                    bool ink = Item.HasSubMenu && IsNativeInk(x - 5, y - 10, rightToLeft);
                    int pixelX = (int)(row.X + expected.X + x);
                    int pixelY = (int)(row.Y + expected.Y + y);
                    pixels.GetPixel(pixelX, pixelY).Should().Be(ink ? Pixel(color) : background,
                        "actual {0} popup source arrow pixel ({1},{2}), RTL={3}, enabled={4}, selected={5}",
                        _owner, x, y, rightToLeft, Item.IsEnabled, Item.IsSelected);
                }
            }
        }

        public void Layout()
        {
            Dispatcher.UIThread.RunJobs();
            Window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
        }

        public void SizeMenu()
        {
            if (_flyout is not null)
            {
                WinFormsToolStripMenuSizer.Apply(_flyout, _button);
            }
            else
            {
                WinFormsToolStripMenuSizer.Apply((ItemsControl?)Title ?? _context
                    ?? throw new AssertionException("The actual popup must have its retained source owner."));
            }

            Layout();
        }

        public void ReopenRightToLeft()
        {
            _context?.Close();
            Layout();
            _rightToLeft = true;
            Window.FlowDirection = FlowDirection.RightToLeft;
            Open();
        }

        public void ReparentToNewContext()
        {
            ContextMenu original = _context ?? throw new AssertionException("This lifetime case must start in an actual context menu.");
            original.Close();
            original.Items.Remove(Item);
            Layout();
            Item.Parent.Should().BeNull();
            _reparented = new ContextMenu { Items = { Item } };
            _button.ContextMenu = _reparented;
            _reparented.Open(_button);
            Layout();
        }

        public void AssertExplicitMaximumIsPreserved()
        {
            Control surface = (Control?)_context
                ?? Item.GetVisualAncestors().OfType<NativeToolStripMenuPopupBorder>().FirstOrDefault()
                ?? (Control?)Item.GetVisualAncestors().OfType<MenuFlyoutPresenter>().FirstOrDefault()
                ?? throw new AssertionException("The source-sized row must retain its actual popup surface.");
            double sourceWidth = Item.Bounds.Width;
            double maximum = sourceWidth / 2;
            surface.MaxWidth = maximum;
            Layout();
            surface.MaxWidth.Should().Be(maximum, "a source-authored maximum remains an explicit local owner value");
            surface.Bounds.Width.Should().BeLessThanOrEqualTo(maximum);
            Item.Width.Should().Be(sourceWidth, "an explicit viewport limit must not rewrite the source item's requested allocation");
        }

        public void AssertNonMenuFlyoutMaximumIsUnchanged()
        {
            Flyout flyout = new() { Content = new Border { Width = Window.Width, Height = 20 } };
            try
            {
                flyout.ShowAt(_button);
                Layout();
                TopLevel root = TopLevel.GetTopLevel(_button) ?? throw new AssertionException("The non-menu target must remain attached.");
                FlyoutPresenter presenter = root.GetVisualDescendants().OfType<FlyoutPresenter>().Single();
                Window.TryFindResource("FlyoutThemeMaxWidth", Window.ActualThemeVariant, out object? maximum).Should().BeTrue();
                presenter.MaxWidth.Should().Be(maximum.Should().BeOfType<double>().Subject,
                    "only source menus opt out of Fluent's generic flyout limit");
            }
            finally
            {
                flyout.Hide();
            }
        }

        public void Dispose()
        {
            Item.IsSubMenuOpen = false;
            if (Title is not null)
            {
                Title.IsSubMenuOpen = false;
            }

            _context?.Close();
            _reparented?.Close();
            _flyout?.Hide();
            Window.Close();
            Dispatcher.UIThread.RunJobs();
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
        }

        private void Open()
        {
            if (Title is not null)
            {
                Title.IsSubMenuOpen = true;
            }

            _context?.Open(_button);
            _flyout?.ShowAt(_button);
            Layout();
        }

        private void RetainDiagnostics()
        {
            // parity-scaffolding: retain the live popup before any rectangle or
            // pixel assertion, so a failing layout cannot hide its own evidence.
            string? evidenceRoot = Environment.GetEnvironmentVariable("GITEXT_MENU_PAINT_EVIDENCE");
            if (string.IsNullOrWhiteSpace(evidenceRoot))
            {
                return;
            }

            TopLevel root = TopLevel.GetTopLevel(Item) ?? throw new AssertionException("The diagnostic row must be attached to its actual popup.");
            using WriteableBitmap frame = root.CaptureRenderedFrame()
                ?? throw new AssertionException("The diagnostic must contain the actual popup pixels.");
            string directory = Path.Combine(Path.GetFullPath(evidenceRoot), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            frame.Save(Path.Combine(directory, "actual-popup.png"), PngBitmapEncoderOptions.Default);
            object[] controls = root.GetVisualDescendants().OfType<Control>().Select(control => new
            {
                type = control.GetType().FullName,
                control.Name,
                classes = control.Classes.ToArray(),
                control.FlowDirection,
                bounds = control.Bounds.ToString(),
                rectangle = Rectangle(control, root).ToString(),
                control.IsVisible,
                control.IsEffectivelyEnabled,
                control.Opacity,
                renderTransform = control.RenderTransform?.Value.ToString(),
                visualParent = control.GetVisualParent()?.GetType().FullName,
                templatedParent = control.TemplatedParent?.GetType().FullName,
                foreground = Brush(control switch
                {
                    NativeToolStripMenuArrow arrow => arrow.Foreground,
                    TemplatedControl templated => templated.Foreground,
                    TextBlock text => text.Foreground,
                    _ => null,
                }),
                background = Brush(control switch
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
                NormalBrushKey, "GitExtensionsNativeMenuDisabledArrowBrush", "GitExtensionsNativeMenuSelectedArrowBrush",
                "GitExtensionsMenuRenderedBackgroundBrush", "GitExtensionsMenuBackgroundBrush", "GitExtensionsNativeMenuSelectedBackgroundBrush",
                "GitExtensionsHighlightBackgroundBrush", "GitExtensionsNativeFlatButtonDarkMode",
            })
            {
                Item.TryFindResource(key, Item.ActualThemeVariant, out object? resource);
                resources[key] = resource is IBrush brush ? Brush(brush) : resource?.ToString();
            }

            File.WriteAllText(Path.Combine(directory, "actual-popup.json"), JsonSerializer.Serialize(new
            {
                test = TestContext.CurrentContext.Test.FullName,
                theme = _theme,
                owner = _owner,
                actualTheme = Item.ActualThemeVariant.ToString(),
                requestedRightToLeft = _rightToLeft,
                root = root.GetType().FullName,
                rootBounds = root.Bounds.ToString(),
                frame = frame.PixelSize.ToString(),
                itemBounds = Item.Bounds.ToString(),
                itemRectangle = Rectangle(Item, root).ToString(),
                Item.IsSelected,
                Item.IsPointerOver,
                Item.IsEnabled,
                Item.HasSubMenu,
                useSystemVisualStyle = NativeToolStripDropDownMenuItem.GetUseSystemVisualStyle(Item),
                resources,
                controls,
            }, new JsonSerializerOptions { WriteIndented = true }));
            TestContext.Progress.WriteLine($"GITEXT_MENU_PAINT_EVIDENCE {directory}");
        }

        private static string? Brush(IBrush? brush)
            => brush is ISolidColorBrush solid ? $"{solid.Color} opacity={solid.Opacity}" : brush?.ToString();
    }
}
