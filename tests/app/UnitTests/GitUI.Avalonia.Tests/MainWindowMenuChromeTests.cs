using System.ComponentModel.Design;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.TextFormatting;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommonTestUtils;
using GitCommands;
using GitCommands.Git;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitExtUtils.GitUI.Theming;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.Compat;
using GitUI.Theming;
using Microsoft.VisualStudio.Threading;
using ResourceManager;
using SkiaSharp;
using Color = Avalonia.Media.Color;
using ColorHelper = GitExtUtils.GitUI.Theming.ColorHelper;
using DrawingColor = System.Drawing.Color;
using KnownColor = System.Drawing.KnownColor;
using Point = Avalonia.Point;
using SourceControls = GitUI.Compat.WinFormsControls;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class MainWindowMenuChromeTests
{
    [AvaloniaTest]
    [TestCase("light")]
    [TestCase("dark")]
    [TestCase("custom")]
    public void Top_title_pointer_hover_should_use_the_source_fill_border_and_window_text(string theme)
    {
        using MenuFixture fixture = new(theme);
        Rect allocation = fixture.Title.Bounds;
        fixture.Window.MouseMove(fixture.TitleCentre);
        fixture.Layout();

        fixture.Title.IsPointerOver.Should().BeTrue();
        fixture.Title.IsSubMenuOpen.Should().BeFalse();
        Border chrome = fixture.TitleChrome;
        GetColor(chrome.Background).Should().Be(fixture.ResourceColor("GitExtensionsNativeMenuSelectedBackgroundBrush"));
        GetColor(chrome.BorderBrush).Should().Be(fixture.ResourceColor("GitExtensionsNativeMenuSelectedBorderBrush"));
        chrome.BorderThickness.Should().Be(new Thickness(1));
        fixture.AssertTitleText();
        fixture.Title.Bounds.Should().Be(allocation);

        fixture.Window.MouseMove(new Point(300, 100));
        fixture.Layout();

        fixture.Title.IsPointerOver.Should().BeFalse();
        fixture.Title.IsSubMenuOpen.Should().BeFalse();
        GetColor(chrome.Background).A.Should().Be(0);
        fixture.AssertTitleText();
        fixture.Title.Bounds.Should().Be(allocation);
    }

    [AvaloniaTest]
    [TestCase("light")]
    [TestCase("dark")]
    [TestCase("custom")]
    public void Open_top_title_should_retain_menu_chrome_and_window_text_after_pointer_moves_to_the_actual_popup(string theme)
    {
        using MenuFixture fixture = new(theme);
        Rect allocation = fixture.Title.Bounds;
        fixture.ClickTitle();
        fixture.Title.IsSubMenuOpen.Should().BeTrue();
        fixture.AssertOpenTitleChrome();
        fixture.AssertTitleText();
        fixture.Title.Bounds.Should().Be(allocation);

        fixture.MovePointerToPopup(fixture.PopupItem);

        fixture.Title.IsPointerOver.Should().BeFalse();
        fixture.Title.IsSubMenuOpen.Should().BeTrue();
        fixture.PopupItem.IsPointerOver.Should().BeTrue();
        fixture.AssertOpenTitleChrome();
        fixture.AssertTitleText();
        fixture.Title.Bounds.Should().Be(allocation);

        fixture.Title.IsSubMenuOpen = false;
        fixture.Window.MouseMove(new Point(300, 100));
        fixture.Layout();
        fixture.Title.IsSubMenuOpen.Should().BeFalse();
        fixture.AssertTitleText();
        fixture.Title.Bounds.Should().Be(allocation);
    }

    [AvaloniaTest]
    [TestCase("light")]
    [TestCase("dark")]
    [TestCase("custom")]
    public void Rapid_second_top_title_click_should_close_the_popup_without_leaving_a_white_caption_or_open_border(string theme)
    {
        using MenuFixture fixture = new(theme);
        List<int> clickCounts = [];
        fixture.Title.AddHandler(InputElement.PointerPressedEvent,
            (_, e) => clickCounts.Add(e.ClickCount), RoutingStrategies.Tunnel, handledEventsToo: true);
        fixture.ClickTitle();
        fixture.Title.IsSubMenuOpen.Should().BeTrue();

        fixture.ClickTitle();

        clickCounts.Should().HaveCount(2, "both clicks must travel through the actual top-level pointer input route");
        clickCounts[1].Should().Be(2, "the second physical click is a double-click, not a fabricated pseudo-class");
        fixture.Title.IsSubMenuOpen.Should().BeFalse();
        fixture.Title.IsPointerOver.Should().BeTrue();
        fixture.AssertTitleText();
        GetColor(fixture.TitleChrome.Background).Should().Be(fixture.ResourceColor("GitExtensionsNativeMenuSelectedBackgroundBrush"));
        GetColor(fixture.TitleChrome.BorderBrush).Should().Be(fixture.ResourceColor("GitExtensionsNativeMenuSelectedBorderBrush"));
        fixture.TitleChrome.BorderThickness.Should().Be(new Thickness(1));
    }

    [AvaloniaTest]
    [TestCase("light")]
    [TestCase("dark")]
    [TestCase("custom")]
    public void Top_title_caption_allocation_and_rendered_ink_should_not_move_between_normal_hover_and_open(string theme)
    {
        using MenuFixture fixture = new(theme);
        Rect caption = fixture.TitleCaptionBounds;
        SKRectI ink = fixture.CaptureTitleInkBounds();
        fixture.Window.MouseMove(fixture.TitleCentre);
        fixture.Layout();

        fixture.Title.IsPointerOver.Should().BeTrue();
        fixture.TitleCaptionBounds.Should().Be(caption);
        fixture.CaptureTitleInkBounds().Should().Be(ink);
        fixture.ClickTitle();

        fixture.Title.IsSubMenuOpen.Should().BeTrue();
        fixture.TitleCaptionBounds.Should().Be(caption);
        fixture.CaptureTitleInkBounds().Should().Be(ink);
        fixture.MovePointerToPopup(fixture.PopupItem);

        fixture.TitleCaptionBounds.Should().Be(caption);
        fixture.CaptureTitleInkBounds().Should().Be(ink);
    }

    [AvaloniaTest]
    [TestCase("light")]
    [TestCase("dark")]
    [TestCase("custom")]
    public void Actual_popup_should_anchor_to_the_title_edge_and_erase_only_its_connected_border(string theme)
    {
        using MenuFixture fixture = new(theme);
        fixture.ClickTitle();
        Popup popup = fixture.Title.GetVisualDescendants().OfType<Popup>().Single();
        popup.PlacementTarget.Should().BeSameAs(fixture.Title);
        popup.VerticalOffset.Should().Be(-1, "ToolStripDropDownItem BelowRight overlaps one border pixel");
        NativeToolStripMenuPopupBorder border = fixture.PopupItem.GetVisualAncestors()
            .OfType<NativeToolStripMenuPopupBorder>().Single();
        border.ConnectedTitleWidth.Should().Be(fixture.Title.Bounds.Width);

        TopLevel popupRoot = TopLevel.GetTopLevel(fixture.PopupItem)
            ?? throw new AssertionException("The connected menu must render in its actual popup root.");

        // Headless popups may render in the owner's OverlayPopupHost rather than
        // a separate TopLevel. Inspect the actual border origin in that same frame.
        Point origin = border.TranslatePoint(default, popupRoot)
            ?? throw new AssertionException("The connected border must have an actual frame coordinate.");
        int x = (int)origin.X;
        int y = (int)origin.Y;
        using SKBitmap pixels = fixture.Capture(popupRoot);
        SKColor background = ToSkColor(fixture.ResourceColor("GitExtensionsMenuRenderedBackgroundBrush"));
        SKColor outline = ToSkColor(fixture.ResourceColor("GitExtensionsMenuRenderedBorderBrush"));
        pixels.GetPixel(x, y).Should().Be(outline);
        pixels.GetPixel(x + 1, y).Should().Be(background);
        pixels.GetPixel(x + (int)fixture.Title.Bounds.Width - 2, y).Should().Be(background);
        pixels.GetPixel(x + (int)fixture.Title.Bounds.Width, y).Should().Be(outline);
    }

    [AvaloniaTest]
    [TestCase("light")]
    [TestCase("dark")]
    [TestCase("custom")]
    public void Context_popup_hover_should_use_Professional_native_selection_without_moving_or_recoloring_the_caption(string theme)
    {
        using MenuFixture fixture = new(theme);
        MenuItem command = new() { Header = "_Context command" };
        SourceControls.ContextMenuStrip menu = new() { Items = { command } };
        menu.Classes.Add("gitextensions-toolstrip-context-menu");
        fixture.Window.ContextMenu = menu;
        menu.Open(fixture.Window);
        fixture.Layout();
        ContentPresenter caption = command.GetVisualDescendants().OfType<ContentPresenter>()
            .Single(part => part.Name == "PART_HeaderPresenter");
        Rect allocation = caption.Bounds;
        Color foreground = GetColor(caption.Foreground);
        fixture.MovePointerToPopup(command);

        command.IsPointerOver.Should().BeTrue();
        Border selection = command.GetVisualDescendants().OfType<Border>()
            .Single(part => part.Name == "PART_NativeSelectionBorder");
        selection.IsVisible.Should().BeTrue();
        selection.Margin.Should().Be(new Thickness(2, 0, 1, 0));
        GetColor(selection.Background).Should().Be(fixture.ResourceColor("GitExtensionsNativeMenuSelectedBackgroundBrush"));
        GetColor(selection.BorderBrush).Should().Be(fixture.ResourceColor("GitExtensionsNativeMenuSelectedBorderBrush"));
        GetColor(caption.Foreground).Should().Be(foreground);
        caption.Bounds.Should().Be(allocation);
        Color background = fixture.ResourceColor("GitExtensionsNativeMenuSelectedBackgroundBrush");
        background.Should().Be(theme == "dark" ? Color.Parse("#2F4159") : Color.Parse("#B3D7F3"),
            "ProfessionalColorTable uses the native system Window/Highlight blend, not CSS Highlight");
        menu.Close();
    }

    [AvaloniaTest]
    [TestCase("light")]
    [TestCase("dark")]
    [TestCase("custom")]
    public void System_split_dropdown_should_keep_menu_owner_paint_when_pointer_leaves_for_the_popup(string theme)
    {
        using MenuFixture fixture = new(theme);
        NativeToolStripSplitButton button = fixture.Split;
        button.UseSystemVisualStyle = true;
        fixture.Layout();
        Rect allocation = button.Bounds;
        Point point = button.TranslatePoint(button.DropDownButtonBounds.Center, fixture.Window)
            ?? throw new AssertionException("The split button must have an actual owner input coordinate.");
        fixture.Window.MouseMove(point);
        fixture.Window.MouseDown(point, MouseButton.Left);
        fixture.Window.MouseUp(point, MouseButton.Left);
        fixture.Layout();

        fixture.SplitFlyout.IsOpen.Should().BeTrue();
        button.DropDownButtonPressed.Should().BeTrue();
        button.ButtonPressed.Should().BeFalse();
        fixture.MovePointerToPopup(fixture.SplitPopupItem);

        button.IsPointerOver.Should().BeFalse();
        fixture.SplitPopupItem.IsPointerOver.Should().BeTrue();
        button.DropDownButtonPressed.Should().BeTrue();
        button.ButtonSelected.Should().BeTrue();
        button.ButtonPressed.Should().BeFalse();
        GetColor(button.Background).Should().Be(fixture.ResourceColor("GitExtensionsMenuRenderedBackgroundBrush"));
        GetColor(button.BorderBrush).Should().Be(fixture.ResourceColor("GitExtensionsMenuRenderedBorderBrush"));
        Button primary = button.GetVisualDescendants().OfType<Button>().Single(part => part.Name == "PART_PrimaryButton");
        Button secondary = button.GetVisualDescendants().OfType<Button>().Single(part => part.Name == "PART_SecondaryButton");
        ContentPresenter primaryChrome = primary.GetVisualDescendants().OfType<ContentPresenter>()
            .Single(part => part.Name == "PART_ContentPresenter");
        ContentPresenter secondaryChrome = secondary.GetVisualDescendants().OfType<ContentPresenter>()
            .Single(part => part.Name == "PART_ContentPresenter");
        GetColor(primaryChrome.Background).Should().Be(fixture.ResourceColor("GitExtensionsMenuRenderedBackgroundBrush"));
        GetColor(secondaryChrome.Background).Should().Be(fixture.ResourceColor("GitExtensionsMenuRenderedBackgroundBrush"));
        button.Bounds.Should().Be(allocation);
        Point origin = button.TranslatePoint(default, fixture.Window)
            ?? throw new AssertionException("The open split button must remain in its original visual owner.");
        using SKBitmap pixels = fixture.CaptureOwner();
        SKColor fill = ToSkColor(fixture.ResourceColor("GitExtensionsMenuRenderedBackgroundBrush"));
        pixels.GetPixel((int)origin.X + 2, (int)origin.Y + 1).Should().Be(fill);
        pixels.GetPixel((int)origin.X + (int)button.Bounds.Width - 2, (int)origin.Y + 1).Should().Be(fill);

        fixture.SplitFlyout.Hide();
        fixture.Layout();
        button.DropDownButtonPressed.Should().BeFalse();
        button.ButtonPressed.Should().BeFalse();
        button.Bounds.Should().Be(allocation);
    }

    [AvaloniaTest]
    public void Professional_split_open_painter_should_remain_independent_of_the_new_system_owner_chrome()
    {
        using MenuFixture fixture = new("custom");
        NativeToolStripSplitButton button = fixture.Split;
        button.UseSystemVisualStyle = false;
        button.ProfessionalOpenBrush = Brushes.Green;
        button.ProfessionalOpenBorderBrush = Brushes.Yellow;
        Point point = button.TranslatePoint(button.DropDownButtonBounds.Center, fixture.Window)
            ?? throw new AssertionException("The professional split button must have an actual input coordinate.");
        fixture.Window.MouseMove(point);
        fixture.Window.MouseDown(point, MouseButton.Left);
        fixture.Window.MouseUp(point, MouseButton.Left);
        fixture.Layout();
        fixture.MovePointerToPopup(fixture.SplitPopupItem);

        fixture.SplitFlyout.IsOpen.Should().BeTrue();
        button.DropDownButtonPressed.Should().BeTrue();
        button.IsPointerOver.Should().BeFalse();
        Point origin = button.TranslatePoint(default, fixture.Window)
            ?? throw new AssertionException("The open professional button must retain its original visual owner.");
        using SKBitmap pixels = fixture.CaptureOwner();
        pixels.GetPixel((int)origin.X + 2, (int)origin.Y + 1).Should().Be(SKColors.Green);
        pixels.GetPixel((int)origin.X + (int)button.Bounds.Width - 2, (int)origin.Y + 1).Should().Be(SKColors.Green);
        pixels.GetPixel((int)origin.X + 5, (int)origin.Y).Should().Be(SKColors.Yellow);
    }

    [AvaloniaTest]
    public async Task Branch_popup_should_fit_complete_literal_branch_captions_without_reserving_an_empty_shortcut_column()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        JoinableTaskContext? previousContext = ThreadHelper.HasJoinableTaskContext ? ThreadHelper.JoinableTaskContext : null;
        using JoinableTaskContext context = new(Thread.CurrentThread, SynchronizationContext.Current);
        ThreadHelper.JoinableTaskContext = context;
        try
        {
            using MenuFixture palette = new("light");
            using ServiceContainer services = new();
            GitExtUtils.ServiceContainerRegistry.RegisterServices(services);
            System.IO.Abstractions.FileSystem fileSystem = new();
            GitDirectoryResolver resolver = new(fileSystem);
            RepositoryDescriptionProvider descriptions = new(resolver);
            services.AddService<System.IO.Abstractions.IFileSystem>(fileSystem);
            services.AddService<IGitDirectoryResolver>(resolver);
            services.AddService<IRepositoryDescriptionProvider>(descriptions);
            services.AddService<IAppTitleGenerator>(new AppTitleGenerator(descriptions));
            services.AddService<ILinkFactory>(new LinkFactory());
            GitCommands.ServiceContainerRegistry.RegisterServices(services);
            GitUI.ServiceContainerRegistry.RegisterServices(services);
            using GitModuleTestHelper repository = new(nameof(MainWindowMenuChromeTests));
            GitModule module = repository.Module;
            module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "--allow-empty", "-m", "branch-popup" }).Should().BeTrue();
            string[] names =
            [
                "backup/Avalonia_Port-pre-upstream-rebase-20260816",
                "backup/Avalonia_Port-pre-upstream-rebase-20260830",
                "backup/master-pre-Avalonia-merge-20261004",
            ];
            foreach (string name in names)
            {
                module.GitExecutable.RunCommand(new GitArgumentBuilder("branch") { name }).Should().BeTrue();
            }

            using FormBrowse form = new(new GitUICommands(services, module)) { Width = 1280, Height = 800 };
            try
            {
                form.Show();
                using CancellationTokenSource deadline = new(TimeSpan.FromSeconds(10));
                await form.JoinLoadOperationsForTestAsync(deadline.Token);
                IconSplitButton selector = form.FindControl<IconSplitButton>("branchSelect")
                    ?? throw new AssertionException("FormBrowse must expose its actual source branch selector.");
                selector.ShowDropDown();
                Dispatcher.UIThread.RunJobs();
                form.UpdateLayout();
                MenuFlyout flyout = (MenuFlyout)selector.Flyout!;
                flyout.IsOpen.Should().BeTrue();
                MenuItem checkout = flyout.Items.OfType<MenuItem>().First();
                checkout.InputGesture.Should().NotBeNull();
                checkout.Classes.Should().NotContain("gitextensions-menu-no-gesture");
                ContentPresenter checkoutCaption = checkout.GetVisualDescendants().OfType<ContentPresenter>()
                    .Single(part => part.Name == "PART_HeaderPresenter");
                Grid.GetColumnSpan(checkoutCaption).Should().Be(1);
                checkout.GetVisualDescendants().OfType<TextBlock>()
                    .Single(part => part.Name == "PART_InputGestureText").Text.Should().NotBeNullOrWhiteSpace();

                foreach (string name in names)
                {
                    string header = name.Replace("_", "__", StringComparison.Ordinal);
                    MenuItem branch = flyout.Items.OfType<MenuItem>().Single(item => item.Header as string == header);
                    TopLevel.GetTopLevel(branch).Should().NotBeNull("the caption must belong to the actual opened popup");
                    branch.InputGesture.Should().BeNull();
                    branch.Classes.Should().Contain("gitextensions-menu-no-gesture");
                    branch.Width.Should().Be(checkout.Width, "source rows retain one measured popup width");
                    ContentPresenter caption = branch.GetVisualDescendants().OfType<ContentPresenter>()
                        .Single(part => part.Name == "PART_HeaderPresenter");
                    Grid.GetColumnSpan(caption).Should().Be(2,
                        "source branch text uses the empty shortcut column instead of clipping before it");
                    AccessText text = branch.GetVisualDescendants().OfType<AccessText>().Single();
                    text.Text.Should().Be(header);
                    text.Measure(Size.Infinity);
                    string renderedCaption = string.Concat(text.TextLayout.TextLines.SelectMany(line => line.TextRuns)
                        .OfType<ShapedTextRun>().Select(run => run.Text.ToString()));
                    renderedCaption.Should().Be(name, "the source branch caption must retain its literal underscores");
                    caption.Bounds.Width.Should().BeGreaterThanOrEqualTo(text.DesiredSize.Width,
                        "the full source branch caption {0} must fit its rendered popup allocation", name);
                    branch.GetVisualDescendants().OfType<TextBlock>()
                        .Single(part => part.Name == "PART_InputGestureText").Text.Should().BeNullOrEmpty();
                }
            }
            finally
            {
                form.Close();
                using CancellationTokenSource deadline = new(TimeSpan.FromSeconds(10));
                await form.JoinLoadOperationsForTestAsync(deadline.Token);
                repository.Dispose();
                TestDirectory.Delete(repository.TemporaryPath);
            }
        }
        finally
        {
            ThreadHelper.JoinableTaskContext = previousContext!;
        }
    }

    private static Color GetColor(IBrush? brush)
        => brush is ISolidColorBrush solid
            ? solid.Color
            : throw new AssertionException("The native menu chrome must resolve to an actual solid-color brush.");

    private static SKColor ToSkColor(Color color) => new(color.R, color.G, color.B, color.A);

    private sealed class MenuFixture : IDisposable
    {
        private readonly Application _application;
        private readonly ThemeSettings _originalSettings;
        private readonly Dictionary<ThemeVariant, IThemeVariantProvider?> _originalResources = [];

        public MenuFixture(string themeName)
        {
            _application = Application.Current
                ?? throw new AssertionException("The actual Avalonia test application must be initialized.");
            _originalSettings = ThemeModule.Settings;
            foreach (ThemeVariant variant in new[] { ThemeVariant.Light, ThemeVariant.Dark })
            {
                _application.Resources.ThemeDictionaries.TryGetValue(variant, out IThemeVariantProvider? original);
                _originalResources.Add(variant, original);
                _application.Resources.ThemeDictionaries[variant] = new ResourceDictionary();
            }

            bool dark = themeName == "dark";
            Dictionary<AppColor, DrawingColor> appColors = new()
            {
                [AppColor.PanelBackground] = dark ? DrawingColor.FromArgb(32, 32, 32) : DrawingColor.White,
            };
            Dictionary<KnownColor, DrawingColor> systemColors = [];
            if (themeName == "custom")
            {
                appColors[AppColor.PanelBackground] = DrawingColor.FromArgb(246, 242, 222);
                systemColors[KnownColor.Window] = DrawingColor.FromArgb(246, 242, 222);
                systemColors[KnownColor.WindowText] = DrawingColor.FromArgb(39, 55, 24);
                systemColors[KnownColor.Highlight] = DrawingColor.FromArgb(34, 104, 55);
                systemColors[KnownColor.HighlightText] = DrawingColor.White;
            }

            Theme theme = new(appColors, systemColors, new ThemeId($"main-window-menu-{themeName}"));
            AvaloniaThemeResources.Apply(_application,
                new ThemeSettings(theme, Theme.Default, ThemeVariations.None, useSystemVisualStyle: true));
            PopupItem = new SourceControls.ToolStripMenuItem { Header = "_Child command" };
            Title = new SourceControls.ToolStripMenuItem { Header = "_Commands", Items = { PopupItem } };
            SourceControls.MenuStripEx menu = new() { Items = { Title }, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top };
            SplitPopupItem = new MenuItem { Header = "_Branch command" };
            SplitFlyout = new MenuFlyout { Items = { SplitPopupItem } };
            Split = new NativeToolStripSplitButton
            {
                UseNativeToolStripLayout = true,
                UseSystemVisualStyle = true,
                Width = 60,
                Height = 22,
                MinHeight = 0,
                Margin = default,
                Content = "Branch",
                Flyout = SplitFlyout,
            };
            Split.Classes.Add("gitextensions-toolbar-button");
            Canvas.SetTop(Split, 40);
            Window = new Window
            {
                Width = 500,
                Height = 200,
                RequestedThemeVariant = dark ? ThemeVariant.Dark : ThemeVariant.Light,
                Content = new Canvas { Background = Brushes.White, Children = { menu, Split } },
            };
            menu.Width = Window.Width;
            Window.Show();
            Layout();
        }

        public Window Window { get; }

        public SourceControls.ToolStripMenuItem Title { get; }

        public SourceControls.ToolStripMenuItem PopupItem { get; }

        public NativeToolStripSplitButton Split { get; }

        public MenuFlyout SplitFlyout { get; }

        public MenuItem SplitPopupItem { get; }

        public Point TitleCentre => Title.TranslatePoint(new Rect(Title.Bounds.Size).Center, Window)
            ?? throw new AssertionException("The main menu title must have an actual window input coordinate.");

        public Border TitleChrome => Title.GetVisualDescendants().OfType<Border>()
            .Single(border => border.Name == "PART_LayoutRoot");

        public Rect TitleCaptionBounds
        {
            get
            {
                ContentPresenter presenter = Title.GetVisualDescendants().OfType<ContentPresenter>()
                    .Single(part => part.Name == "PART_HeaderPresenter");
                Point point = presenter.TranslatePoint(default, Window)
                    ?? throw new AssertionException("The caption must retain its actual owner coordinate.");
                return new Rect(point, presenter.Bounds.Size);
            }
        }

        public Color ResourceColor(string key)
            => Window.TryFindResource(key, Window.ActualThemeVariant, out object? resource)
                ? GetColor(resource as IBrush)
                : throw new AssertionException($"The actual runtime palette must expose {key}.");

        public void AssertTitleText()
        {
            Color expected = ResourceColor("GitExtensionsWindowTextBrush");
            GetColor(Title.Foreground).Should().Be(expected);
            ContentPresenter presenter = Title.GetVisualDescendants().OfType<ContentPresenter>()
                .Single(part => part.Name == "PART_HeaderPresenter");
            GetColor(presenter.Foreground).Should().Be(expected);
        }

        public void AssertOpenTitleChrome()
        {
            Window.TryFindResource("GitExtensionsNativeMenuOpenBackgroundBrush", Window.ActualThemeVariant, out object? background)
                .Should().BeTrue();
            TitleChrome.Background.Should().BeSameAs(background);
            GetColor(TitleChrome.BorderBrush).Should().Be(ResourceColor("GitExtensionsNativeMenuOpenBorderBrush"));
            TitleChrome.BorderThickness.Should().Be(new Thickness(1, 1, 1, 0));
        }

        public void ClickTitle()
        {
            Window.MouseMove(TitleCentre);
            Window.MouseDown(TitleCentre, MouseButton.Left);
            Window.MouseUp(TitleCentre, MouseButton.Left);
            Layout();
        }

        public void MovePointerToPopup(MenuItem item)
        {
            TopLevel popup = TopLevel.GetTopLevel(item)
                ?? throw new AssertionException("The source menu item must be attached to its real open popup.");
            Window.MouseMove(new Point(300, 100));
            Point point = item.TranslatePoint(new Rect(item.Bounds.Size).Center, popup)
                ?? throw new AssertionException("The source popup item must have a real popup input coordinate.");
            popup.MouseMove(point);
            Layout();
        }

        public void Layout()
        {
            Dispatcher.UIThread.RunJobs();
            Window.UpdateLayout();
        }

        public SKRectI CaptureTitleInkBounds()
        {
            using SKBitmap pixels = CaptureOwner();
            Point origin = Title.TranslatePoint(default, Window)
                ?? throw new AssertionException("The caption must render within its actual menu title.");
            SKColor foreground = ToSkColor(ResourceColor("GitExtensionsWindowTextBrush"));
            int left = pixels.Width;
            int top = pixels.Height;
            int right = 0;
            int bottom = 0;
            for (int y = (int)origin.Y + 1; y < (int)(origin.Y + Title.Bounds.Height) - 1; y++)
            {
                for (int x = (int)origin.X + 1; x < (int)(origin.X + Title.Bounds.Width) - 1; x++)
                {
                    if (pixels.GetPixel(x, y) == foreground)
                    {
                        left = Math.Min(left, x);
                        top = Math.Min(top, y);
                        right = Math.Max(right, x + 1);
                        bottom = Math.Max(bottom, y + 1);
                    }
                }
            }

            right.Should().BeGreaterThan(left, "the actual title frame must contain caption ink");
            return new SKRectI(left, top, right, bottom);
        }

        public SKBitmap CaptureOwner() => Capture(Window);

        public SKBitmap Capture(TopLevel root)
        {
            using WriteableBitmap frame = root.CaptureRenderedFrame()
                ?? throw new AssertionException("The actual open menu owner must render a frame.");
            using MemoryStream stream = new();
            frame.Save(stream, PngBitmapEncoderOptions.Default);
            stream.Position = 0;
            return SKBitmap.Decode(stream)
                ?? throw new AssertionException("The actual menu owner frame must decode to pixels.");
        }

        public void Dispose()
        {
            Title.IsSubMenuOpen = false;
            SplitFlyout.Hide();
            Window.Close();
            foreach ((ThemeVariant variant, IThemeVariantProvider? resources) in _originalResources)
            {
                if (resources is null)
                {
                    _application.Resources.ThemeDictionaries.Remove(variant);
                }
                else
                {
                    _application.Resources.ThemeDictionaries[variant] = resources;
                }
            }

            ColorHelper.ThemeSettings = _originalSettings;
        }
    }
}
