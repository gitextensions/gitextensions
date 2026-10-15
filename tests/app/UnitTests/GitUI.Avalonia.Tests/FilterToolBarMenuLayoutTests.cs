using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitExtensions.ParityCapture;
using GitUI.Compat;
using GitUI.UserControls;
using SkiaSharp;
using Point = Avalonia.Point;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class FilterToolBarMenuLayoutTests
{
    private static readonly string[] MenuNames =
    [
        "tsbtnAdvancedFilter", "tssbtnShowBranches", "tsddbtnBranchFilter", "tsddbtnRevisionFilter",
    ];

    private static IEnumerable<TestCaseData> LayoutCases
        => from name in MenuNames
           from rightToLeft in new[] { false, true }
           from points in new[] { 9, 14 }
           select new TestCaseData(name, rightToLeft, points);

    private static IEnumerable<TestCaseData> EndpointKeyCases
        => from name in MenuNames
           from rightToLeft in new[] { false, true }
           from systemRenderer in new[] { false, true }
           from modifiers in new[]
           {
               RawInputModifiers.None,
               RawInputModifiers.Control,
               RawInputModifiers.Alt,
               RawInputModifiers.Control | RawInputModifiers.Alt | RawInputModifiers.Shift,
           }
           select new TestCaseData(name, rightToLeft, systemRenderer, modifiers);

    private static IEnumerable<TestCaseData> ActivationCases
    {
        get
        {
            foreach (bool rightToLeft in new[] { false, true })
            {
                foreach (string name in MenuNames)
                {
                    foreach (string route in new[] { "programmatic", "pointer-arrow", "Down", "Up", "F4", "Alt+Down" })
                    {
                        yield return new TestCaseData(name, route, rightToLeft);
                    }

                    if (name != "tsbtnAdvancedFilter")
                    {
                        yield return new TestCaseData(name, "Enter", rightToLeft);
                        yield return new TestCaseData(name, "Space", rightToLeft);
                    }
                }

                yield return new TestCaseData("tssbtnShowBranches", "pointer-primary", rightToLeft);
            }
        }
    }

    [AvaloniaTest]
    [TestCaseSource(nameof(ActivationCases))]
    public void Filter_popup_should_follow_actual_source_activation_selection_and_reopen_routes(
        string name, string route, bool rightToLeft)
    {
        using MenuFixture fixture = new(rightToLeft);
        (TemplatedControl owner, MenuFlyout menu) = fixture.Menu(name);
        MenuItem[] rows = menu.Items.OfType<MenuItem>().ToArray();
        bool opens = route is not ("F4" or "Alt+Down");
        bool selectsFirst = route is "Down" or "Up"
            || (owner is DropDownButton && (route is "Enter" or "Space"));
        for (int cycle = 0; cycle < 2; cycle++)
        {
            fixture.Move(fixture.Window);
            fixture.Activate(owner, menu, route);
            menu.IsOpen.Should().Be(opens);
            MenuItem firstEnabled = rows.First(item => item.IsVisible && item.IsEffectivelyEnabled);
            rows.Where(item => item.IsSelected).Select(item => item.Name)
                .Should().Equal(selectsFirst ? new[] { firstEnabled.Name } : Array.Empty<string>());
            if (opens)
            {
                menu.ShowMode.Should().Be(selectsFirst ? FlyoutShowMode.Standard : FlyoutShowMode.Transient);
                fixture.Move(rows.Last(item => item.IsVisible && item.IsEffectivelyEnabled));
                rows.Last(item => item.IsVisible && item.IsEffectivelyEnabled).IsSelected.Should().BeTrue();
                menu.Hide();
                fixture.Settle();
                rows.Should().OnlyContain(item => !item.IsSelected,
                    "the real original dropdown closure clears rows before the next activation");
            }
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Filter_programmatic_open_should_keep_actual_focus_and_unselected_rows(bool rightToLeft)
    {
        using MenuFixture fixture = new(rightToLeft);
        (TemplatedControl owner, MenuFlyout menu) = fixture.Menu("tsddbtnBranchFilter");
        fixture.Move(fixture.Window);
        fixture.Window.FocusManager.Should().NotBeNull();
        IInputElement? before = fixture.Window.FocusManager?.GetFocusedElement();
        menu.ShowAt(owner);
        fixture.Settle();
        fixture.Window.FocusManager!.GetFocusedElement().Should().BeSameAs(before);
        menu.Items.OfType<MenuItem>().Should().OnlyContain(item => !item.IsSelected);
        menu.Hide();
        fixture.Settle();
        fixture.Window.FocusManager!.GetFocusedElement().Should().BeSameAs(before);
    }

    [AvaloniaTest]
    [TestCase(FlyoutShowMode.Standard)]
    [TestCase(FlyoutShowMode.Transient)]
    public void Filter_activation_scope_should_preserve_authored_show_mode_and_retire_only_its_own_value(FlyoutShowMode authored)
    {
        using MenuFixture fixture = new();
        (TemplatedControl owner, MenuFlyout menu) = fixture.Menu("tsddbtnBranchFilter");
        menu.ShowMode = authored;
        menu.ShowAt(owner);
        fixture.Settle();
        menu.ShowMode.Should().Be(authored);
        WinFormsToolStripMenuSizer.ClearToolbarDropDown(menu);
        fixture.Settle();
        menu.ShowMode.Should().Be(authored);
        menu.Hide();
        menu.ClearValue(PopupFlyoutBase.ShowModeProperty);
        WinFormsToolStripMenuSizer.ConfigureToolbarDropDown(menu, owner);
        menu.ShowAt(owner);
        fixture.Settle();
        menu.ShowMode.Should().Be(FlyoutShowMode.Transient);
        WinFormsToolStripMenuSizer.ClearToolbarDropDown(menu);
        menu.ShowMode.Should().Be(FlyoutShowMode.Standard);
        menu.Hide();
        owner.Focus().Should().BeTrue();
        fixture.Window.KeyPress(Key.Down, RawInputModifiers.None, PhysicalKey.ArrowDown, keySymbol: null);
        fixture.Settle();
        menu.IsOpen.Should().BeFalse("retirement must release the source-specific owner-key opener");
    }

    [AvaloniaTest]
    public void Filter_activation_scope_should_retire_during_show_mode_publication_without_leaking_its_value()
    {
        using MenuFixture fixture = new();
        (TemplatedControl owner, MenuFlyout menu) = fixture.Menu("tsddbtnBranchFilter");
        bool retired = false;
        menu.PropertyChanged += (_, change) =>
        {
            if (!retired && change.Property == PopupFlyoutBase.ShowModeProperty && menu.ShowMode == FlyoutShowMode.Transient)
            {
                retired = true;
                WinFormsToolStripMenuSizer.ClearToolbarDropDown(menu);
            }
        };
        menu.ShowAt(owner);
        fixture.Settle();
        retired.Should().BeTrue();
        menu.ShowMode.Should().Be(FlyoutShowMode.Standard);
        menu.Items.OfType<MenuItem>().Should().OnlyContain(item => !item.Classes.Contains("gitextensions-filter-menu"));
    }

    [AvaloniaTest]
    [Platform(Include = "Win")]
    [TestCase(9, 48, 15, 7, 115, 22)]
    [TestCase(11, 61, 20, 9, 130, 24)]
    [TestCase(14, 75, 25, 12, 147, 30)]
    public void Filter_source_text_and_outer_frame_should_match_original_native_menu_metrics(
        int points, int captionWidth, int textHeight, int tabWidth, int itemWidth, int itemHeight)
    {
        using MenuFixture fixture = new();
        fixture.Toolbar.FontSize = points * 96d / 72;
        (TemplatedControl owner, MenuFlyout menu) = fixture.Open("tsddbtnBranchFilter");
        MenuItem remote = menu.Items.OfType<MenuItem>().Single(item => item.Name == "tsmiBranchRemote");
        Size caption = NativeToolStripDropDownLayout.MeasureSourceText(remote, "&Remote");
        Size tab = NativeToolStripDropDownLayout.MeasureSourceText(owner, "\t");
        NativeToolStripDropDownLayout.Group metrics = new(owner, menu.Items.OfType<Control>().ToArray(), null, null);
        TestContext.Out.WriteLine($"Source filter metrics: points={points}, caption={caption}, tab={tab}, item={metrics.ItemWidth}x{metrics.ItemHeight}, popup={fixture.Presenter(menu).Bounds.Size}");
        caption.Should().Be(new Size(captionWidth, textHeight));
        tab.Should().Be(new Size(tabWidth, textHeight));
        metrics.ItemWidth.Should().Be(itemWidth);
        metrics.ItemHeight.Should().Be(itemHeight);
        remote.Bounds.Size.Should().Be(new Size(itemWidth, itemHeight));
        fixture.Presenter(menu).Bounds.Size.Should().Be(new Size(itemWidth + 1, (itemHeight * 3) + 4));
    }

    [AvaloniaTest]
    [TestCaseSource(nameof(LayoutCases))]
    public void Opened_filter_popup_should_allocate_source_padding_outside_the_retained_rows(
        string name, bool rightToLeft, int points)
    {
        using MenuFixture fixture = new(rightToLeft);
        fixture.Toolbar.FontSize = points * 96d / 72;
        (TemplatedControl owner, MenuFlyout menu) = fixture.Open(name);
        object?[] retained = menu.Items.ToArray();
        AssertSourceLayout(owner, menu);
        menu.Hide();
        fixture.Settle();
        menu.ShowAt(owner);
        fixture.Settle();
        menu.Items.Should().Equal(retained);
        AssertSourceLayout(owner, menu);
    }

    [AvaloniaTest]
    [TestCaseSource(nameof(LayoutCases))]
    public void Filter_popup_tree_should_report_actual_arranged_rows_and_preserve_canonical_capture_identity(
        string name, bool rightToLeft, int points)
    {
        using MenuFixture fixture = new(rightToLeft);
        fixture.Toolbar.FontSize = points * 96d / 72;
        fixture.SetPalette(systemRenderer: false);
        (_, MenuFlyout menu) = fixture.Open(name);
        MenuItem hovered = menu.Items.OfType<MenuItem>().Last(item => item.IsEnabled);
        fixture.Move(hovered);
        hovered.IsPointerOver.Should().BeTrue();
        Control presenter = fixture.Presenter(menu);
        AvaloniaControlTreeReader reader = new(fixture.Toolbar, renderScale: 1);
        CaptureSurface popup = reader.ReadSurface(presenter, "popup:0",
            new PixelRect(0, 0, (int)presenter.Bounds.Width, (int)presenter.Bounds.Height));
        popup.Role.Should().Be("popup:0");
        popup.Root.BoundsDip.Width.Should().Be((decimal)presenter.Bounds.Width);
        popup.Root.BoundsDip.Height.Should().Be((decimal)presenter.Bounds.Height);
        popup.Root.Children.Select(node => node.FieldName).Should().Equal(menu.Items.OfType<Control>().Select(item => item.Name));
        foreach (Control item in menu.Items.OfType<Control>())
        {
            CaptureNode node = popup.Root.Children.Single(child => child.FieldName == item.Name);
            Point origin = item.TranslatePoint(default, presenter)
                ?? throw new InvalidOperationException("The retained popup row has no actual presenter coordinate.");
            node.BoundsDip.X.Should().Be(decimal.Round((decimal)origin.X, 4));
            node.BoundsDip.Y.Should().Be(decimal.Round((decimal)origin.Y, 4));
            node.BoundsDip.Width.Should().Be(decimal.Round((decimal)item.Bounds.Width, 4));
            node.BoundsDip.Height.Should().Be(decimal.Round((decimal)item.Bounds.Height, 4));
            node.BoundsPx.Width.Should().Be((int)item.Bounds.Width);
            node.BoundsPx.Height.Should().Be((int)item.Bounds.Height);
            node.ControlKind.Should().Be("menuItem");
            node.Visible.Should().BeTrue();
            if (item is MenuItem command)
            {
                node.Font.Should().NotBeNull();
                CaptureFont font = node.Font ?? throw new InvalidOperationException("The retained menu item must preserve its actual font.");
                font.SizeDip.Should().Be(decimal.Round((decimal)command.FontSize, 4));
                node.Enabled.Should().Be(command.IsEnabled);
                node.Colors.Foreground.Should().MatchRegex("^#[0-9A-F]{8}$");
                node.CheckState.Should().Be(command.IsChecked ? "Checked" : "Unchecked");
            }
        }

        using WriteableBitmap frame = fixture.Window.CaptureRenderedFrame()
            ?? throw new InvalidOperationException("The actual open popup must render its owning frame.");
        CaptureDocument document = new()
        {
            SchemaVersion = CaptureDocument.CurrentSchemaVersion,
            Component = new CaptureComponent
            {
                TypeName = typeof(FilterToolBar).FullName ?? throw new InvalidOperationException("The filter capture type must retain its namespace."),
                AssemblyName = typeof(FilterToolBar).Assembly.GetName().Name ?? throw new InvalidOperationException("The filter capture assembly identity is unavailable."),
            },
            Capture = new CaptureMetadata
            {
                Framework = "avalonia",
                Theme = new CaptureTheme { Id = "custom", Kind = "custom", SourceSha256 = new string('A', 64) },
                ScalePercent = 100,
                Dpi = new CaptureDpi { X = 96, Y = 96 },
                DpiMode = CaptureDpiMode.HeadlessRenderScale,
                State = "filter-popup.actual-row-geometry",
                StateStatus = CaptureStateStatus.Captured,
            },
            Image = new CaptureImage { WidthPx = frame.PixelSize.Width, HeightPx = frame.PixelSize.Height, CaptureMethod = CaptureMethod.HeadlessSkia },
            Surfaces = [reader.ReadPrimary(fixture.Toolbar, frame.PixelSize), popup],
        };
        string json = CaptureJson.Serialize(document);
        CaptureDocument roundTripped = CaptureJson.Deserialize(json);
        CaptureJson.Serialize(roundTripped).Should().Be(json);
        roundTripped.Surfaces[1].Root.Should().BeEquivalentTo(popup.Root);
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Opened_filter_popup_should_remeasure_actual_owner_font_caption_shortcuts_and_hidden_rows(bool rightToLeft)
    {
        using MenuFixture fixture = new(rightToLeft);
        (TemplatedControl owner, MenuFlyout menu) = fixture.Open("tsddbtnRevisionFilter");
        MenuItem row = menu.Items.OfType<MenuItem>().First();
        row.FontWeight = FontWeight.Bold;
        double oldHeight = fixture.Presenter(menu).Bounds.Height;
        fixture.Toolbar.FontSize = 14 * 96d / 72;
        row.Header = "_Actual retained filter caption requiring a wider command allocation";
        row.InputGesture = new KeyGesture(Key.F12, KeyModifiers.Control | KeyModifiers.Shift);
        fixture.Settle();
        fixture.Presenter(menu).Bounds.Height.Should().BeGreaterThan(oldHeight);
        row.FontWeight.Should().Be(FontWeight.Bold);
        AssertSourceLayout(owner, menu);
        menu.Items.OfType<MenuItem>().Last().IsVisible = false;
        fixture.Settle();
        AssertSourceLayout(owner, menu);
    }

    [AvaloniaTest]
    [TestCase(false, false, false)]
    [TestCase(false, false, true)]
    [TestCase(false, true, false)]
    [TestCase(false, true, true)]
    [TestCase(true, false, false)]
    [TestCase(true, false, true)]
    [TestCase(true, true, false)]
    [TestCase(true, true, true)]
    public void Filter_pointer_hover_should_paint_the_source_renderer_palette_without_changing_layout(
        bool systemRenderer, bool disabled, bool checkedItem)
    {
        using MenuFixture fixture = new();
        fixture.SetPalette(systemRenderer);
        (_, MenuFlyout menu) = fixture.Open("tsddbtnBranchFilter");
        MenuItem row = menu.Items.OfType<MenuItem>().First();
        fixture.Move(menu.Items.OfType<MenuItem>().Last());
        row.IsSelected.Should().BeFalse();
        row.IsEnabled = !disabled;
        row.IsChecked = checkedItem;
        fixture.Settle();
        Rect allocation = row.Bounds;
        fixture.Move(row);
        row.IsPointerOver.Should().Be(!disabled, "disabled rows retain Avalonia's real hit-testing state");
        row.IsSelected.Should().BeTrue("the actual pointer route must reproduce native disabled hot tracking as menu selection");
        row.Bounds.Should().Be(allocation);
        row.IsChecked.Should().Be(checkedItem);
        Border chrome = Part<Border>(row, "PART_LayoutRoot");
        Border selection = Part<Border>(row, "PART_NativeSelectionBorder");
        ContentPresenter caption = Part<ContentPresenter>(row, "PART_HeaderPresenter");
        Color foreground = fixture.ResourceColor(disabled ? "GitExtensionsDisabledForegroundBrush"
            : systemRenderer ? "GitExtensionsHighlightForegroundBrush" : "GitExtensionsWindowTextBrush");
        Color normal = fixture.ResourceColor(systemRenderer ? "GitExtensionsMenuBackgroundBrush" : "GitExtensionsMenuRenderedBackgroundBrush");
        Color fill = fixture.ResourceColor(systemRenderer ? "GitExtensionsHighlightBackgroundBrush" : "GitExtensionsNativeMenuSelectedBackgroundBrush");
        Color outline = fixture.ResourceColor(systemRenderer ? "GitExtensionsHighlightBackgroundBrush" : "GitExtensionsNativeMenuSelectedBorderBrush");
        ColorOf(caption.Foreground).Should().Be(foreground);
        ColorOf(chrome.Background).Should().Be(normal);
        selection.IsVisible.Should().BeTrue();
        selection.Margin.Should().Be(new Thickness(2, 0, 1, 0));
        if (!systemRenderer)
        {
            ColorOf(selection.BorderBrush).Should().Be(outline);
        }

        fixture.AssertPixel(row, new Point(27, 3), disabled ? normal : fill);
        fixture.AssertPixel(row, new Point(systemRenderer ? 1 : 2, 0), outline);
        if (systemRenderer)
        {
            Border systemSelection = Part<Border>(row, "PART_NativeSystemSelectionBorder");
            systemSelection.IsVisible.Should().BeTrue();
            systemSelection.BorderThickness.Should().Be(new Thickness(1));
            systemSelection.Margin.Should().Be(default(Thickness));
            ColorOf(systemSelection.BorderBrush).Should().Be(outline);
            MenuFlyoutPresenter presenter = (MenuFlyoutPresenter)fixture.Presenter(menu);
            fixture.AssertPixel(row, new Point(0, 0), ColorOf(presenter.BorderBrush));
        }
    }

    [AvaloniaTest]
    public void Opened_filter_hover_should_follow_live_palette_and_renderer_replacement()
    {
        using MenuFixture fixture = new();
        fixture.SetPalette(systemRenderer: false);
        (_, MenuFlyout menu) = fixture.Open("tsddbtnBranchFilter");
        MenuItem row = menu.Items.OfType<MenuItem>().First();
        fixture.Move(row);
        SolidColorBrush replacement = new(Color.Parse("#765432"));
        fixture.Window.Resources["GitExtensionsNativeMenuSelectedBackgroundBrush"] = replacement;
        fixture.Settle();
        fixture.AssertPixel(row, new Point(27, 3), replacement.Color);
        fixture.Window.Resources["GitExtensionsToolStripSeparatorUseSystemVisualStyle"] = true;
        fixture.Settle();
        fixture.AssertPixel(row, new Point(27, 3), fixture.ResourceColor("GitExtensionsHighlightBackgroundBrush"));
        ColorOf(Part<ContentPresenter>(row, "PART_HeaderPresenter").Foreground)
            .Should().Be(fixture.ResourceColor("GitExtensionsHighlightForegroundBrush"));
        fixture.Window.Resources["GitExtensionsToolStripSeparatorUseSystemVisualStyle"] = false;
        fixture.Settle();
        fixture.AssertPixel(row, new Point(27, 3), replacement.Color);
        ColorOf(Part<ContentPresenter>(row, "PART_HeaderPresenter").Foreground)
            .Should().Be(fixture.ResourceColor("GitExtensionsWindowTextBrush"));
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Filter_keyboard_selection_should_use_the_same_source_renderer_as_pointer_hover(bool systemRenderer)
    {
        using MenuFixture fixture = new();
        fixture.SetPalette(systemRenderer);
        (_, MenuFlyout menu) = fixture.Open("tsddbtnBranchFilter");
        MenuItem[] rows = menu.Items.OfType<MenuItem>().ToArray();
        fixture.Move(fixture.Toolbar);
        rows[0].Focus(NavigationMethod.Tab).Should().BeTrue();
        fixture.Settle();
        rows[0].IsSelected.Should().BeTrue();
        TopLevel input = TopLevel.GetTopLevel(rows[0])
            ?? throw new InvalidOperationException("The real filter popup input root is unavailable.");
        input.KeyPress(Key.Down, RawInputModifiers.None, PhysicalKey.ArrowDown, keySymbol: null);
        fixture.Settle();
        rows[1].IsSelected.Should().BeTrue();
        input.KeyPress(Key.Down, RawInputModifiers.None, PhysicalKey.ArrowDown, keySymbol: null);
        fixture.Settle();
        MenuItem row = rows.Last();
        row.IsSelected.Should().BeTrue("the framework's actual menu-navigation route must select the last row");
        row.IsPointerOver.Should().BeFalse();
        fixture.AssertPixel(row, new Point(27, 3), fixture.ResourceColor(systemRenderer
            ? "GitExtensionsHighlightBackgroundBrush" : "GitExtensionsNativeMenuSelectedBackgroundBrush"));
    }

    [AvaloniaTest]
    [TestCaseSource(nameof(EndpointKeyCases))]
    public void Filter_Home_End_should_use_source_dropdown_endpoints_skip_unavailable_rows_and_ignore_modifiers(
        string name, bool rightToLeft, bool systemRenderer, RawInputModifiers modifiers)
    {
        using MenuFixture fixture = new(rightToLeft);
        fixture.SetPalette(systemRenderer);
        (_, MenuFlyout menu) = fixture.Open(name);
        MenuItem[] rows = menu.Items.OfType<MenuItem>().ToArray();
        foreach (MenuItem row in rows)
        {
            row.IsEnabled = true;
        }

        fixture.Move(fixture.Toolbar);
        rows[1].Focus(NavigationMethod.Directional).Should().BeTrue();
        fixture.Settle();
        TopLevel input = TopLevel.GetTopLevel(rows[1])
            ?? throw new InvalidOperationException("The real filter popup input root is unavailable.");
        List<bool> handled = [];
        void ObserveKey(object? sender, KeyEventArgs args) => handled.Add(args.Handled);
        ((MenuFlyoutPresenter)fixture.Presenter(menu)).AddHandler(InputElement.KeyDownEvent,
            ObserveKey, RoutingStrategies.Bubble, handledEventsToo: true);
        input.KeyPress(Key.End, modifiers, PhysicalKey.End, keySymbol: null);
        fixture.Settle();
        AssertSelected(rows[^1]);
        input.KeyPress(Key.Home, modifiers, PhysicalKey.Home, keySymbol: null);
        fixture.Settle();
        AssertSelected(rows[0]);
        handled.Should().Equal([true, true], "the source handles Home/End even with Ctrl, Alt or Shift");

        rows[0].IsEnabled = false;
        rows[^1].IsVisible = false;
        fixture.Settle();
        rows[1].Focus(NavigationMethod.Directional).Should().BeTrue();
        fixture.Settle();
        input.KeyPress(Key.End, modifiers, PhysicalKey.End, keySymbol: null);
        fixture.Settle();
        AssertSelected(rows[^2]);
        input.KeyPress(Key.Home, modifiers, PhysicalKey.Home, keySymbol: null);
        fixture.Settle();
        AssertSelected(rows[1]);
        handled.Should().HaveCount(4).And.OnlyContain(value => value);

        void AssertSelected(MenuItem selected)
        {
            rows.Where(row => row.IsSelected).Select(row => row.Name).Should().Equal([selected.Name]);
            selected.IsFocused.Should().BeTrue();
            selected.IsPointerOver.Should().BeFalse();
            fixture.AssertPixel(selected, new Point(27, 3), fixture.ResourceColor(systemRenderer
                ? "GitExtensionsHighlightBackgroundBrush" : "GitExtensionsNativeMenuSelectedBackgroundBrush"));
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Filter_framework_endpoint_keys_should_survive_close_and_source_scope_retirement(bool systemRenderer)
    {
        using MenuFixture fixture = new(rightToLeft: true);
        fixture.SetPalette(systemRenderer);
        (TemplatedControl owner, MenuFlyout menu) = fixture.Open("tsddbtnBranchFilter");
        MenuItem[] rows = menu.Items.OfType<MenuItem>().ToArray();
        fixture.Move(fixture.Toolbar);
        rows[1].Focus(NavigationMethod.Directional).Should().BeTrue();
        fixture.Settle();
        menu.Hide();
        fixture.Settle();
        bool[] closedSelection = rows.Select(row => row.IsSelected).ToArray();
        fixture.Window.KeyPress(Key.End, RawInputModifiers.None, PhysicalKey.End, keySymbol: null);
        fixture.Settle();
        rows.Select(row => row.IsSelected).Should().Equal(closedSelection);
        menu.ShowAt(owner);
        fixture.Settle();
        rows[1].Focus(NavigationMethod.Directional).Should().BeTrue();
        fixture.Settle();
        TopLevel input = TopLevel.GetTopLevel(rows[1])
            ?? throw new InvalidOperationException("The reopened popup must retain its actual input root.");
        input.KeyPress(Key.End, RawInputModifiers.None, PhysicalKey.End, keySymbol: null);
        fixture.Settle();
        rows[^1].IsSelected.Should().BeTrue("ToolStripDropDownMenu retains the last End endpoint even in RTL");
        rows[1].Focus(NavigationMethod.Directional).Should().BeTrue();
        fixture.Settle();
        WinFormsToolStripMenuSizer.ClearToolbarDropDown(menu);
        fixture.Settle();
        rows[1].Focus(NavigationMethod.Directional).Should().BeTrue();
        fixture.Settle();
        rows[1].IsSelected.Should().BeTrue();
        input.KeyPress(Key.Home, RawInputModifiers.None, PhysicalKey.Home, keySymbol: null);
        fixture.Settle();
        rows[0].IsSelected.Should().BeTrue("source scope retirement must leave the normal framework Home route intact");
        rows[^1].IsSelected.Should().BeFalse();
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Filter_framework_endpoint_selection_should_respect_popup_close_and_continue_after_paint_scope_retirement(bool retireScope)
    {
        using MenuFixture fixture = new();
        (TemplatedControl owner, MenuFlyout menu) = fixture.Open("tsddbtnBranchFilter");
        MenuItem[] rows = menu.Items.OfType<MenuItem>().ToArray();
        fixture.Move(fixture.Toolbar);
        rows[1].Focus(NavigationMethod.Directional).Should().BeTrue();
        fixture.Settle();
        MenuItem target = rows[^1];
        target.IsSelected.Should().BeFalse();
        target.IsFocused.Should().BeFalse();
        bool retired = false;
        target.PropertyChanged += (_, change) =>
        {
            if (!retired && change.Property == MenuItem.IsSelectedProperty && target.IsSelected)
            {
                retired = true;
                if (retireScope)
                {
                    WinFormsToolStripMenuSizer.ClearToolbarDropDown(menu);
                }
                else
                {
                    menu.Hide();
                }

                owner.Focus(NavigationMethod.Directional).Should().BeTrue();
            }
        };
        TopLevel input = TopLevel.GetTopLevel(rows[1])
            ?? throw new InvalidOperationException("The actual callback probe must retain its popup input root.");
        input.KeyPress(Key.End, RawInputModifiers.None, PhysicalKey.End, keySymbol: null);
        fixture.Settle();
        retired.Should().BeTrue();
        menu.IsOpen.Should().Be(retireScope);
        target.IsFocused.Should().Be(retireScope,
            "the normal endpoint route may focus its attached target after paint-scope retirement, but not after popup closure");
        owner.IsFocused.Should().Be(!retireScope);
        if (retireScope)
        {
            target.IsSelected.Should().BeTrue("retiring paint overrides must not cancel the still-open menu's normal End selection");
            target.GetValue(NativeFilterToolStripDropDownPanel.GroupProperty).Should().BeNull();
            target.Classes.Should().NotContain("gitextensions-filter-menu");
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Filter_disabled_hot_tracking_should_clear_on_leave_remove_close_and_scope_retirement(bool systemRenderer)
    {
        using MenuFixture fixture = new();
        fixture.SetPalette(systemRenderer);
        (TemplatedControl owner, MenuFlyout menu) = fixture.Open("tsddbtnBranchFilter");
        MenuItem row = menu.Items.OfType<MenuItem>().First();
        fixture.Move(menu.Items.OfType<MenuItem>().Last());
        row.IsSelected.Should().BeFalse();
        row.IsEnabled = false;
        fixture.Move(row);
        row.IsSelected.Should().BeTrue();
        fixture.Move(fixture.Toolbar);
        row.IsSelected.Should().BeFalse();
        fixture.Move(row);
        row.IsSelected.Should().BeTrue();
        MenuItem enabled = menu.Items.OfType<MenuItem>().Last();
        fixture.Move(enabled);
        enabled.IsSelected.Should().BeTrue();
        row.IsSelected.Should().BeFalse();
        fixture.Move(row);
        menu.Items.Remove(row);
        fixture.Settle();
        row.IsSelected.Should().BeFalse();
        menu.Items.Insert(0, row);
        fixture.Settle();
        fixture.Move(row);
        row.IsSelected.Should().BeTrue();
        menu.Hide();
        fixture.Settle();
        menu.IsOpen.Should().BeFalse();
        ((MenuFlyoutPresenter)menu.Popup.Child!).SelectedItem.Should().BeNull();
        menu.Items.Should().Contain(row);
        row.IsSelected.Should().BeFalse();
        menu.ShowAt(owner);
        fixture.Settle();
        fixture.Move(row);
        row.IsSelected.Should().BeTrue();
        WinFormsToolStripMenuSizer.ClearToolbarDropDown(menu);
        fixture.Settle();
        row.IsSelected.Should().BeFalse();
        row.IsEnabled.Should().BeFalse();
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Filter_disabled_pointer_selection_should_not_publish_after_a_synchronous_callback_closes_the_popup(bool closeDuringDeselection)
    {
        using MenuFixture fixture = new();
        (TemplatedControl owner, MenuFlyout menu) = fixture.Open("tsddbtnBranchFilter");
        MenuItem[] rows = menu.Items.OfType<MenuItem>().ToArray();
        fixture.Move(rows[^1]);
        rows[0].IsSelected.Should().BeFalse();
        rows[1].IsSelected.Should().BeFalse();
        rows[0].IsEnabled = false;
        rows[1].IsEnabled = false;
        if (closeDuringDeselection)
        {
            fixture.Move(rows[0]);
            rows[0].IsSelected.Should().BeTrue();
        }

        MenuFlyoutPresenter presenter = (MenuFlyoutPresenter)fixture.Presenter(menu);
        MenuItem observed = closeDuringDeselection ? rows[0] : rows[1];
        bool closed = false;
        observed.PropertyChanged += (_, change) =>
        {
            if (!closed && change.Property == MenuItem.IsSelectedProperty && observed.IsSelected != closeDuringDeselection)
            {
                closed = true;
                menu.Hide();
                owner.Focus(NavigationMethod.Directional).Should().BeTrue();
            }
        };
        fixture.Move(rows[1]);
        closed.Should().BeTrue();
        menu.IsOpen.Should().BeFalse();
        presenter.SelectedItem.Should().BeNull("the captured presenter must not publish a disabled selection after its route closed");
        rows[0].IsSelected.Should().BeFalse();
        rows[1].IsSelected.Should().BeFalse();
        rows[1].IsPointerOver.Should().BeFalse();
        owner.IsFocused.Should().BeTrue();
    }

    [AvaloniaTest]
    public void Filter_disabled_hot_tracking_should_release_selection_when_a_synchronous_callback_retires_the_scope()
    {
        using MenuFixture fixture = new();
        (_, MenuFlyout menu) = fixture.Open("tsddbtnBranchFilter");
        MenuItem row = menu.Items.OfType<MenuItem>().First();
        fixture.Move(menu.Items.OfType<MenuItem>().Last());
        row.IsSelected.Should().BeFalse();
        row.IsEnabled = false;
        bool retired = false;
        row.PropertyChanged += (_, change) =>
        {
            if (!retired && change.Property == MenuItem.IsSelectedProperty && row.IsSelected)
            {
                retired = true;
                WinFormsToolStripMenuSizer.ClearToolbarDropDown(menu);
            }
        };
        fixture.Move(row);
        retired.Should().BeTrue();
        row.IsSelected.Should().BeFalse();
        row.IsPointerOver.Should().BeFalse();
        row.GetValue(NativeFilterToolStripDropDownPanel.GroupProperty).Should().BeNull();
    }

    [AvaloniaTest]
    public void Filter_layout_scope_should_restore_group_values_when_rows_are_removed_or_the_owner_rebinds()
    {
        using MenuFixture fixture = new();
        (TemplatedControl owner, MenuFlyout menu) = fixture.Open("tsddbtnBranchFilter");
        MenuItem row = menu.Items.OfType<MenuItem>().First();
        row.GetValue(NativeFilterToolStripDropDownPanel.GroupProperty).Should().NotBeNull();
        menu.Items.Remove(row);
        fixture.Settle();
        row.GetValue(NativeFilterToolStripDropDownPanel.GroupProperty).Should().BeNull();
        row.IsSet(Control.WidthProperty).Should().BeFalse();
        menu.Items.Insert(0, row);
        fixture.Settle();
        row.GetValue(NativeFilterToolStripDropDownPanel.GroupProperty).Should().NotBeNull();
        (TemplatedControl replacement, _) = fixture.Menu("tsddbtnRevisionFilter");
        replacement.FontSize = 16;
        WinFormsToolStripMenuSizer.ConfigureToolbarDropDown(menu, replacement);
        menu.Hide();
        menu.ShowAt(replacement);
        fixture.Settle();
        row.FontSize.Should().Be(replacement.FontSize);
        AssertSourceLayout(replacement, menu);
        WinFormsToolStripMenuSizer.ClearToolbarDropDown(menu);
        fixture.Settle();
        menu.Items.OfType<MenuItem>().Should().OnlyContain(item =>
            ReferenceEquals(item.GetValue(NativeFilterToolStripDropDownPanel.GroupProperty), null));
        row.IsSet(Control.WidthProperty).Should().BeFalse();
        owner.FontSize = 20;
        fixture.Settle();
        row.FontSize.Should().NotBe(20);
    }

    [AvaloniaTest]
    public void Filter_layout_scope_should_retire_synchronously_and_preserve_authored_renderer_classes()
    {
        using MenuFixture fixture = new();
        (TemplatedControl owner, MenuFlyout menu) = fixture.Menu("tsddbtnBranchFilter");
        WinFormsToolStripMenuSizer.ClearToolbarDropDown(menu);
        MenuItem row = menu.Items.OfType<MenuItem>().First();
        row.Classes.Add("gitextensions-filter-menu-system-renderer");
        WinFormsToolStripMenuSizer.ConfigureToolbarDropDown(menu, owner);
        bool retired = false;
        row.PropertyChanged += (_, change) =>
        {
            if (!retired && change.Property == NativeFilterToolStripDropDownPanel.GroupProperty && change.NewValue is not null)
            {
                retired = true;
                WinFormsToolStripMenuSizer.ClearToolbarDropDown(menu);
            }
        };
        menu.ShowAt(owner);
        fixture.Settle();
        retired.Should().BeTrue();
        row.GetValue(NativeFilterToolStripDropDownPanel.GroupProperty).Should().BeNull();
        row.IsSet(Control.WidthProperty).Should().BeFalse();
        row.Classes.Should().Contain("gitextensions-filter-menu-system-renderer");
        row.Classes.Should().NotContain("gitextensions-filter-menu");
    }

    private static void AssertSourceLayout(TemplatedControl owner, MenuFlyout menu)
    {
        Control presenter = menu.Popup.Child ?? throw new InvalidOperationException("The retained filter presenter is unavailable.");
        NativeFilterToolStripDropDownPanel panel = presenter.GetVisualDescendants().OfType<NativeFilterToolStripDropDownPanel>().Single();
        NativeToolStripDropDownLayout.Group metrics = new(owner, menu.Items.OfType<Control>().ToArray(), null, null);
        double height = metrics.Padding.Top + metrics.Padding.Bottom;
        double y = metrics.Padding.Top;
        foreach (Control control in menu.Items.OfType<Control>().Where(item => item.IsVisible))
        {
            double itemHeight = control is Separator ? 6 : metrics.ItemHeight;
            Point origin = control.TranslatePoint(default, panel)
                ?? throw new InvalidOperationException("The actual retained item has no filter-panel coordinate.");
            origin.Y.Should().Be(y);
            origin.X.Should().Be(control is Separator ? 2 : 0,
                "native ToolStrip rows keep their leading item origin even for right-to-left content");
            control.Bounds.Height.Should().Be(itemHeight);
            control.Bounds.Width.Should().Be(control is Separator ? metrics.PopupWidth - 4 : metrics.ItemWidth);
            y += itemHeight;
            height += itemHeight;
        }

        panel.Bounds.Size.Should().Be(new Size(metrics.PopupWidth, height));
        presenter.Bounds.Size.Should().Be(panel.Bounds.Size, "the source border is painted over the canvas, not added to its allocation");
        (presenter.Bounds.Height - y).Should().Be(metrics.Padding.Bottom,
            "the native trailing frame must be retained below the last visible item");
    }

    private static T Part<T>(MenuItem item, string name)
        where T : Control
        => item.GetVisualDescendants().OfType<T>().Single(control => control.Name == name && ReferenceEquals(control.TemplatedParent, item));

    private static Color ColorOf(IBrush? brush)
        => brush is ISolidColorBrush solid ? solid.Color : throw new InvalidOperationException("The source palette must resolve to a solid color.");

    private sealed class MenuFixture : IDisposable
    {
        public MenuFixture(bool rightToLeft = false)
        {
            Toolbar = new() { Width = 1100, VerticalAlignment = VerticalAlignment.Top };
            Canvas canvas = new();
            Canvas.SetLeft(Toolbar, 150);
            Canvas.SetTop(Toolbar, 10);
            canvas.Children.Add(Toolbar);
            Window = new()
            {
                Width = 1600,
                Height = 650,
                FontSize = 11 * 96d / 72,
                FlowDirection = rightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
                Content = canvas,
            };
            Window.Show();
            Settle();
        }

        public FilterToolBar Toolbar { get; }

        public Window Window { get; }

        public void SetPalette(bool systemRenderer)
        {
            Window.Resources["GitExtensionsToolStripSeparatorUseSystemVisualStyle"] = systemRenderer;
            Window.Resources["GitExtensionsMenuBackgroundBrush"] = new SolidColorBrush(Color.Parse("#314253"));
            Window.Resources["GitExtensionsMenuRenderedBackgroundBrush"] = new SolidColorBrush(Color.Parse("#425364"));
            Window.Resources["GitExtensionsHighlightBackgroundBrush"] = new SolidColorBrush(Color.Parse("#123456"));
            Window.Resources["GitExtensionsHighlightForegroundBrush"] = new SolidColorBrush(Color.Parse("#EEDDCC"));
            Window.Resources["GitExtensionsNativeMenuSelectedBackgroundBrush"] = new SolidColorBrush(Color.Parse("#546372"));
            Window.Resources["GitExtensionsNativeMenuSelectedBorderBrush"] = new SolidColorBrush(Color.Parse("#ABCDEF"));
            Window.Resources["GitExtensionsWindowTextBrush"] = new SolidColorBrush(Color.Parse("#DCEAF8"));
            Window.Resources["GitExtensionsDisabledForegroundBrush"] = new SolidColorBrush(Color.Parse("#8F9EAD"));
            Settle();
        }

        public Color ResourceColor(string key)
            => Window.TryFindResource(key, Window.ActualThemeVariant, out object? value) && value is IBrush brush
                ? ColorOf(brush) : throw new InvalidOperationException($"The source palette role '{key}' is missing.");

        public (TemplatedControl Owner, MenuFlyout Menu) Menu(string name)
        {
            TemplatedControl owner = Toolbar.Strip.Items.OfType<TemplatedControl>().Single(item => item.Name == name);
            MenuFlyout? menu = owner switch
            {
                SplitButton split => split.Flyout as MenuFlyout,
                DropDownButton dropDown => dropDown.Flyout as MenuFlyout,
                _ => null,
            };
            return (owner, menu ?? throw new InvalidOperationException("The original filter menu is unavailable."));
        }

        public (TemplatedControl Owner, MenuFlyout Menu) Open(string name)
        {
            (TemplatedControl owner, MenuFlyout menu) = Menu(name);
            menu.ShowAt(owner);
            Settle();
            menu.IsOpen.Should().BeTrue();
            return (owner, menu);
        }

        public Control Presenter(MenuFlyout menu)
            => menu.Popup.Child ?? throw new InvalidOperationException("The actual retained popup presenter is unavailable.");

        public void Move(Control control)
        {
            TopLevel root = TopLevel.GetTopLevel(control)
                ?? throw new InvalidOperationException("The actual popup has no input root.");
            Point point = control.TranslatePoint(new Point(control.Bounds.Width / 2, control.Bounds.Height / 2), root)
                ?? throw new InvalidOperationException("The retained control has no actual input coordinate.");
            root.MouseMove(point);
            Settle();
        }

        public void Activate(TemplatedControl owner, MenuFlyout menu, string route)
        {
            if (route == "programmatic")
            {
                menu.ShowAt(owner);
            }
            else if (route.StartsWith("pointer-", StringComparison.Ordinal))
            {
                Rect area = owner is NativeToolStripSplitButton split
                    ? route == "pointer-primary" ? split.ButtonBounds : split.DropDownButtonBounds
                    : new Rect(owner.Bounds.Size);
                Point point = owner.TranslatePoint(new Point(area.X + (area.Width / 2), area.Y + (area.Height / 2)), Window)
                    ?? throw new InvalidOperationException("The actual source owner has no pointer coordinate.");
                Window.MouseMove(point);
                Window.MouseDown(point, MouseButton.Left);
                Window.MouseUp(point, MouseButton.Left);
            }
            else
            {
                owner.Focus(NavigationMethod.Tab).Should().BeTrue();
                Key key = route == "Alt+Down" ? Key.Down : Enum.Parse<Key>(route);
                RawInputModifiers modifiers = route == "Alt+Down" ? RawInputModifiers.Alt : RawInputModifiers.None;
                PhysicalKey physical = key switch
                {
                    Key.Down => PhysicalKey.ArrowDown,
                    Key.Up => PhysicalKey.ArrowUp,
                    Key.Enter => PhysicalKey.Enter,
                    Key.Space => PhysicalKey.Space,
                    Key.F4 => PhysicalKey.F4,
                    _ => throw new InvalidOperationException("The source activation probe only declares its bounded menu keys."),
                };
                Window.KeyPress(key, modifiers, physical, keySymbol: null);
                Window.KeyRelease(key, modifiers, physical, keySymbol: null);
            }

            Settle();
        }

        public void AssertPixel(Control control, Point point, Color expected)
        {
            TopLevel root = TopLevel.GetTopLevel(control)
                ?? throw new InvalidOperationException("The actual popup has no frame root.");
            Point actual = control.TranslatePoint(point, root)
                ?? throw new InvalidOperationException("The retained control has no actual frame coordinate.");
            using WriteableBitmap frame = root.CaptureRenderedFrame()
                ?? throw new InvalidOperationException("The real popup input route must produce a rendered frame.");
            using MemoryStream stream = new();
            frame.Save(stream, PngBitmapEncoderOptions.Default);
            stream.Position = 0;
            using SKBitmap pixels = SKBitmap.Decode(stream)
                ?? throw new InvalidOperationException("The actual popup frame cannot be decoded.");
            pixels.GetPixel((int)actual.X, (int)actual.Y).Should().Be(new SKColor(expected.R, expected.G, expected.B, expected.A));
        }

        public void Settle()
        {
            Dispatcher.UIThread.RunJobs();
            Window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
        }

        public void Dispose()
        {
            foreach (string name in MenuNames)
            {
                Menu(name).Menu.Hide();
            }

            Window.Close();
            Toolbar.Strip.Dispose();
        }
    }
}
