using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitUI.Compat;
using GitUI.UserControls;
using GitUI.UserControls.RevisionGrid;
using NSubstitute;
using Point = Avalonia.Point;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class FilterToolBarInteractionTests
{
    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Branch_completion_should_survive_the_actual_opening_release_and_editor_input(bool editorClick)
    {
        using FilterFixture fixture = new();
        ToolbarComboBox combo = fixture.Toolbar.GetTestAccessor().BranchFilter;
        TextBox editor = Editor(combo);
        int opened = 0;
        int closed = 0;
        combo.DropDownOpened += (_, _) => opened++;
        combo.DropDownClosed += (_, _) => closed++;
        fixture.Click(editorClick ? editor : combo, editorClick ? null : new Point(combo.Bounds.Width - 4, combo.Bounds.Height / 2));
        combo.IsDropDownOpen.Should().BeTrue();
        opened.Should().Be(1);
        closed.Should().Be(0, "the opening click must not be toggled closed on its release");
        combo.Items.Cast<string>().Should().Equal("feature/filters", "feature/other", "main");
        editor.Focus();
        fixture.Window.KeyTextInput("fea");
        fixture.Settle();
        combo.IsDropDownOpen.Should().BeTrue();
        Editor(combo).Should().BeSameAs(editor);
        editor.IsFocused.Should().BeTrue();
        combo.Text.Should().Be("fea");
        combo.Items.Cast<string>().Should().Equal("feature/filters", "feature/other");
        closed.Should().Be(0);
        fixture.Window.KeyPress(Key.Back, RawInputModifiers.None, PhysicalKey.Backspace, keySymbol: null);
        fixture.Settle();
        combo.Text.Should().Be("fe");
        combo.Items.Cast<string>().Should().Equal("feature/filters", "feature/other");
        combo.IsDropDownOpen.Should().BeTrue();
    }

    [AvaloniaTest]
    public void Selecting_an_actual_completion_row_should_close_the_popup_without_the_owner_reopening_it()
    {
        using FilterFixture fixture = new();
        ToolbarComboBox combo = fixture.Toolbar.GetTestAccessor().BranchFilter;
        fixture.Click(combo, new Point(combo.Bounds.Width - 4, combo.Bounds.Height / 2));
        combo.IsDropDownOpen.Should().BeTrue();
        ComboBoxItem row = combo.ContainerFromIndex(0) as ComboBoxItem
            ?? throw new InvalidOperationException("The opened native completion list has no retained row.");
        fixture.Click(row);
        combo.SelectedItem.Should().Be("feature/filters");
        combo.Text.Should().Be("feature/filters");
        combo.Items.Cast<string>().Should().Equal("feature/filters", "feature/other", "main");
        combo.IsDropDownOpen.Should().BeFalse();
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Branch_completion_should_keep_framework_Escape_and_outside_pointer_dismissal(bool escape)
    {
        using FilterFixture fixture = new();
        ToolbarComboBox combo = fixture.Toolbar.GetTestAccessor().BranchFilter;
        fixture.Click(combo, new Point(combo.Bounds.Width - 4, combo.Bounds.Height / 2));
        combo.IsDropDownOpen.Should().BeTrue();
        if (escape)
        {
            Editor(combo).Focus();
            fixture.Window.KeyPress(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, keySymbol: null);
        }
        else
        {
            Point outside = new(fixture.Window.Bounds.Width - 20, fixture.Window.Bounds.Height - 20);
            fixture.Window.MouseDown(outside, MouseButton.Left);
            fixture.Window.MouseUp(outside, MouseButton.Left);
        }

        fixture.Settle();
        combo.IsDropDownOpen.Should().BeFalse();
    }

    [AvaloniaTest]
    [TestCase("tsbtnAdvancedFilter", false, false)]
    [TestCase("tsbtnAdvancedFilter", true, true)]
    [TestCase("tssbtnShowBranches", false, false)]
    [TestCase("tssbtnShowBranches", true, true)]
    [TestCase("tsddbtnBranchFilter", false, false)]
    [TestCase("tsddbtnBranchFilter", true, true)]
    [TestCase("tsddbtnRevisionFilter", false, false)]
    [TestCase("tsddbtnRevisionFilter", true, true)]
    public void Opened_filter_menus_should_use_actual_owner_font_native_item_metrics_and_directional_edges(
        string name, bool rightToLeft, bool dark)
    {
        using FilterFixture fixture = new(rightToLeft, dark);
        (TemplatedControl owner, MenuFlyout menu) = fixture.Menu(name);
        object?[] retained = menu.Items.ToArray();
        menu.ShowAt(owner);
        fixture.Settle();
        menu.IsOpen.Should().BeTrue();
        NativeToolStripDropDownLayout.Group sourceMetrics = new(owner, menu.Items.OfType<Control>().ToArray(), null, null);
        foreach (MenuItem item in menu.Items.OfType<MenuItem>())
        {
            item.Classes.Should().Contain("gitextensions-filter-menu");
            item.FontFamily.Should().Be(owner.FontFamily);
            item.FontSize.Should().Be(owner.FontSize);
            item.FontSize.Should().Be(12, "the source ToolStrip's default menu font is independent of the ambient eleven-point Form font");
            item.Bounds.Width.Should().Be(sourceMetrics.ItemWidth);
            item.Bounds.Height.Should().Be(sourceMetrics.ItemHeight);
            item.FlowDirection.Should().Be(owner.FlowDirection);
            Border chrome = item.GetVisualDescendants().OfType<Border>()
                .Single(part => part.Name == "PART_LayoutRoot" && ReferenceEquals(part.TemplatedParent, item));
            bool systemRenderer = item.GetValue(NativeToolStripDropDownMenuItem.UseSystemVisualStyleProperty);
            TestContext.Out.WriteLine($"{name}/{item.Name}: system={systemRenderer}, selected={item.IsSelected}, pointer={item.IsPointerOver}, normalBackground={chrome.Background}, foreground={item.Foreground}");
            ColorOf(chrome.Background).Should().Be(ColorOf(fixture.ResourceBrush(systemRenderer
                ? "GitExtensionsMenuBackgroundBrush" : "GitExtensionsMenuRenderedBackgroundBrush")));
            if (item.IsEnabled)
            {
                ContentPresenter caption = item.GetVisualDescendants().OfType<ContentPresenter>()
                    .Single(part => part.Name == "PART_HeaderPresenter" && ReferenceEquals(part.TemplatedParent, item));
                caption.Foreground.Should().NotBeNull("normal menu text must not rely on pointer-over paint to become visible");
            }
        }

        menu.Popup.Placement.Should().Be(PlacementMode.BottomEdgeAlignedLeft,
            "Avalonia already mirrors its logical popup anchors for the RTL owner");
        menu.Popup.VerticalOffset.Should().Be(-1);
        Control presenter = menu.Popup.Child ?? throw new InvalidOperationException("The actual filter popup presenter is missing.");
        presenter.Classes.Should().Contain("gitextensions-filter-menu");
        PixelRect menuBounds = PhysicalBounds(presenter);
        PixelRect ownerBounds = PhysicalBounds(owner);
        menuBounds.X.Should().Be(rightToLeft ? ownerBounds.Right - menuBounds.Width : ownerBounds.X);
        menuBounds.Y.Should().Be(ownerBounds.Bottom - 1);
        menu.Hide();
        fixture.Settle();
        menu.ShowAt(owner);
        fixture.Settle();
        menu.Items.Should().Equal(retained);
        menu.Items.OfType<MenuItem>().Should().OnlyContain(item => item.Bounds.Width == sourceMetrics.ItemWidth);
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Filter_menu_palette_should_follow_the_source_renderer_flag_and_live_resolved_colors(bool systemRenderer)
    {
        using FilterFixture fixture = new();
        fixture.Window.Resources["GitExtensionsToolStripSeparatorUseSystemVisualStyle"] = systemRenderer;
        SolidColorBrush systemSurface = new(Color.Parse("#37434F"));
        SolidColorBrush professionalSurface = new(Color.Parse("#52606E"));
        fixture.Window.Resources["GitExtensionsMenuBackgroundBrush"] = systemSurface;
        fixture.Window.Resources["GitExtensionsMenuRenderedBackgroundBrush"] = professionalSurface;
        fixture.Window.Resources["GitExtensionsHighlightBackgroundBrush"] = new SolidColorBrush(Color.Parse("#213141"));
        fixture.Window.Resources["GitExtensionsHighlightForegroundBrush"] = new SolidColorBrush(Color.Parse("#ECDFCA"));
        fixture.Window.Resources["GitExtensionsWindowTextBrush"] = new SolidColorBrush(Color.Parse("#DFE4E9"));
        (TemplatedControl owner, MenuFlyout menu) = fixture.Menu("tsddbtnBranchFilter");
        menu.ShowAt(owner);
        fixture.Settle();
        MenuItem row = menu.Items.OfType<MenuItem>().First();
        Border chrome = row.GetVisualDescendants().OfType<Border>()
            .Single(part => part.Name == "PART_LayoutRoot" && ReferenceEquals(part.TemplatedParent, row));
        ContentPresenter caption = row.GetVisualDescendants().OfType<ContentPresenter>()
            .Single(part => part.Name == "PART_HeaderPresenter" && ReferenceEquals(part.TemplatedParent, row));
        TestContext.Out.WriteLine($"Custom filter palette: system={row.GetValue(NativeToolStripDropDownMenuItem.UseSystemVisualStyleProperty)}, selected={row.IsSelected}, pointer={row.IsPointerOver}, background={chrome.Background}, itemForeground={row.Foreground}, captionForeground={caption.Foreground}");
        row.GetValue(NativeToolStripDropDownMenuItem.UseSystemVisualStyleProperty).Should().Be(systemRenderer);
        ColorOf(chrome.Background).Should().Be((systemRenderer ? systemSurface : professionalSurface).Color);
        ColorOf(caption.Foreground).Should().Be(ColorOf(fixture.ResourceBrush("GitExtensionsWindowTextBrush")));
        row.IsChecked.Should().BeTrue("checked is retained source state, not a reason to replace its normal menu surface");
        fixture.Move(row);
        ColorOf(chrome.Background).Should().Be(ColorOf(fixture.ResourceBrush("GitExtensionsHighlightBackgroundBrush")));
        ColorOf(caption.Foreground).Should().Be(ColorOf(fixture.ResourceBrush("GitExtensionsHighlightForegroundBrush")));
        fixture.Window.Resources["GitExtensionsHighlightBackgroundBrush"] = new SolidColorBrush(Color.Parse("#192B3D"));
        fixture.Settle();
        ColorOf(chrome.Background).Should().Be(ColorOf(fixture.ResourceBrush("GitExtensionsHighlightBackgroundBrush")));
    }

    [AvaloniaTest]
    public void Open_filter_menu_should_follow_live_owner_font_and_preserve_authored_item_typography()
    {
        using FilterFixture fixture = new();
        (TemplatedControl owner, MenuFlyout menu) = fixture.Menu("tsddbtnRevisionFilter");
        MenuItem[] rows = menu.Items.OfType<MenuItem>().ToArray();
        rows[0].FontWeight = FontWeight.Bold;
        rows[0].FontSize = 16;
        menu.ShowAt(owner);
        fixture.Settle();
        rows[0].FontWeight.Should().Be(FontWeight.Bold);
        rows[0].FontSize.Should().Be(16);
        fixture.Toolbar.FontSize = 11 * 96d / 72;
        fixture.Settle();
        menu.IsOpen.Should().BeTrue();
        menu.Items.OfType<MenuItem>().Should().Equal(rows);
        rows[0].FontSize.Should().Be(16);
        rows.Skip(1).Should().OnlyContain(item => item.FontSize == owner.FontSize);
        NativeToolStripDropDownLayout.Group sourceMetrics = new(owner, menu.Items.OfType<Control>().ToArray(), null, null);
        rows.Should().OnlyContain(item => item.Bounds.Width == sourceMetrics.ItemWidth && item.Bounds.Height == sourceMetrics.ItemHeight);
    }

    [AvaloniaTest]
    [TestCase(9)]
    [TestCase(11)]
    public void Branches_and_Filter_labels_should_center_content_in_the_native_owner_row(int ownerPoints)
    {
        using FilterFixture fixture = new();
        fixture.Toolbar.FontSize = ownerPoints * 96d / 72;
        fixture.Settle();
        foreach (Label label in fixture.Toolbar.Strip.Items.OfType<Label>())
        {
            label.VerticalContentAlignment.Should().Be(VerticalAlignment.Center);
            label.HorizontalContentAlignment.Should().Be(HorizontalAlignment.Center);
            ContentPresenter caption = label.GetVisualDescendants().OfType<ContentPresenter>().Single();
            caption.VerticalContentAlignment.Should().Be(VerticalAlignment.Center);
            caption.Bounds.Height.Should().BeGreaterThan(0);
            label.Bounds.Top.Should().Be(1, "the source label retains its one-pixel item margin; centering belongs to its content");
        }
    }

    [AvaloniaTest]
    [TestCase("")]
    [TestCase("main")]
    [TestCase("a deliberately long feature branch caption with enough characters to measure independently of its editor")]
    [TestCase("a deliberately extremely long feature branch caption repeated a deliberately extremely long feature branch caption repeated a deliberately extremely long feature branch caption repeated")]
    public void Source_combo_width_should_use_Graphics_metrics_and_the_original_bounds_without_widening_the_editor(string caption)
    {
        using FilterFixture fixture = new();
        ToolbarComboBox combo = fixture.Toolbar.GetTestAccessor().RevisionFilter;
        combo.ItemsSource = new[] { caption };
        combo.ResizeDropDownWidth();
        fixture.Settle();
        int measured = (int)WinFormsGraphicsTextMeasurer.MeasureSize(combo, caption).Width;
        combo.DropDownWidth.Should().Be(Math.Min(Math.Max(measured, 200), 600));
        combo.Bounds.Width.Should().Be(100);
        combo.IsDropDownOpen = true;
        fixture.Settle();
        Popup popup = combo.GetVisualDescendants().OfType<Popup>().Single(part => part.Name == "PART_Popup");
        popup.Width.Should().Be(combo.DropDownWidth);
        popup.MaxWidth.Should().Be(combo.DropDownWidth);
        Control list = popup.Child ?? throw new InvalidOperationException("The actual source-sized list is missing.");
        list.Bounds.Width.Should().Be(combo.DropDownWidth);
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void InitToolStripStyles_should_keep_combo_hosts_on_the_live_Window_color_not_the_strip_background(bool dark)
    {
        using FilterFixture fixture = new(dark: dark);
        fixture.Toolbar.InitToolStripStyles(Colors.Magenta, Colors.Transparent);
        fixture.Settle();
        ToolbarComboBox[] hosts =
        [
            fixture.Toolbar.GetTestAccessor().BranchFilter,
            fixture.Toolbar.GetTestAccessor().RevisionFilter,
        ];
        IBrush sourceWindow = fixture.ResourceBrush("GitExtensionsWindowBackgroundBrush");
        foreach (ToolbarComboBox host in hosts)
        {
            host.Background.Should().BeSameAs(sourceWindow);
            host.Foreground.Should().BeOfType<SolidColorBrush>().Which.Color.Should().Be(Colors.Magenta);
        }

        // A distinct resolved custom resource proves that this source-known color
        // remains live without treating the transparent toolbar as the input surface.
        SolidColorBrush customWindow = new(Color.Parse("#304050"));
        fixture.Window.Resources["GitExtensionsWindowBackgroundBrush"] = customWindow;
        fixture.Settle();
        hosts.Should().OnlyContain(host => ReferenceEquals(host.Background, customWindow));
        fixture.Menu("tsddbtnRevisionFilter").Owner.Background.Should().BeOfType<SolidColorBrush>()
            .Which.Color.Should().Be(Colors.Transparent);
    }

    [AvaloniaTest]
    public void Resized_combo_should_remeasure_after_actual_control_host_typography_changes()
    {
        using FilterFixture fixture = new();
        ToolbarComboBox combo = fixture.Toolbar.GetTestAccessor().RevisionFilter;
        const string caption = "feature/actual-source-font-allocation-for-completion";
        combo.ItemsSource = new[] { caption };
        combo.ResizeDropDownWidth();
        fixture.Settle();
        combo.FontSize.Should().Be(12);
        double originalWidth = combo.DropDownWidth;
        combo.FontSize = 16;
        fixture.Settle();
        int measured = (int)WinFormsGraphicsTextMeasurer.MeasureSize(combo, caption).Width;
        combo.DropDownWidth.Should().Be(Math.Min(Math.Max(measured, 200), 600));
        combo.DropDownWidth.Should().BeGreaterThan(originalWidth);
        combo.Bounds.Width.Should().Be(100);
    }

    [AvaloniaTest]
    public void Opened_combo_should_include_its_actual_vertical_scrollbar_in_the_source_width_calculation()
    {
        using FilterFixture fixture = new();
        ToolbarComboBox combo = fixture.Toolbar.GetTestAccessor().RevisionFilter;
        string[] captions = Enumerable.Range(0, 40).Select(index => $"feature/scrollable-source-completion-{index}").ToArray();
        combo.ItemsSource = captions;
        combo.MaxDropDownHeight = 80;
        combo.ResizeDropDownWidth();
        combo.IsDropDownOpen = true;
        fixture.Settle();
        Popup popup = combo.GetVisualDescendants().OfType<Popup>().Single(part => part.Name == "PART_Popup");
        Control list = popup.Child ?? throw new InvalidOperationException("The actual source-sized list is missing.");
        ScrollBar vertical = list.GetVisualDescendants().OfType<ScrollBar>()
            .Single(scrollBar => scrollBar.Orientation == Orientation.Vertical && scrollBar.IsVisible);
        vertical.Bounds.Width.Should().BeGreaterThan(0);
        int measured = captions.Max(caption => (int)WinFormsGraphicsTextMeasurer.MeasureSize(combo, caption).Width);
        combo.DropDownWidth.Should().Be(Math.Min(Math.Max(measured + (int)vertical.Bounds.Width, 200), 600));
        combo.Bounds.Width.Should().Be(100);
        combo.IsDropDownOpen = false;
        combo.ItemsSource = new[] { captions[0] };
        combo.ResizeDropDownWidth();
        combo.IsDropDownOpen = true;
        fixture.Settle();
        list.GetVisualDescendants().OfType<ScrollBar>()
            .Where(scrollBar => scrollBar.Orientation == Orientation.Vertical)
            .Should().OnlyContain(scrollBar => !scrollBar.IsVisible);
        int singleMeasured = (int)WinFormsGraphicsTextMeasurer.MeasureSize(combo, captions[0]).Width;
        combo.DropDownWidth.Should().Be(Math.Min(Math.Max(singleMeasured, 200), 600), "a prior long list must not retain its old scrollbar allocation");
    }

    [AvaloniaTest]
    public void Toolbar_menu_scope_should_rebind_release_removed_rows_and_preserve_local_values()
    {
        using FilterFixture fixture = new();
        (TemplatedControl owner, MenuFlyout menu) = fixture.Menu("tsddbtnBranchFilter");
        (TemplatedControl secondOwner, _) = fixture.Menu("tsddbtnRevisionFilter");
        MenuItem row = menu.Items.OfType<MenuItem>().First();
        row.FontWeight = FontWeight.Bold;
        menu.ShowAt(owner);
        fixture.Settle();
        secondOwner.FontSize = 16;
        secondOwner.FlowDirection = FlowDirection.RightToLeft;
        WinFormsToolStripMenuSizer.ConfigureToolbarDropDown(menu, secondOwner);
        menu.Hide();
        menu.ShowAt(secondOwner);
        fixture.Settle();
        row.FontSize.Should().Be(16);
        row.FontWeight.Should().Be(FontWeight.Bold);
        row.FlowDirection.Should().Be(FlowDirection.RightToLeft);
        menu.Popup.Placement.Should().Be(PlacementMode.BottomEdgeAlignedLeft);
        menu.Items.Remove(row);
        fixture.Settle();
        row.Classes.Should().NotContain("gitextensions-filter-menu");
        row.IsSet(Control.WidthProperty).Should().BeFalse();
        row.FontWeight.Should().Be(FontWeight.Bold);
        WinFormsToolStripMenuSizer.ClearToolbarDropDown(menu);
        menu.FlyoutPresenterClasses.Should().NotContain("gitextensions-filter-menu");
        menu.Items.OfType<MenuItem>().Should().OnlyContain(item => !item.Classes.Contains("gitextensions-filter-menu"));
    }

    [AvaloniaTest]
    public void Open_toolbar_menu_should_remeasure_retained_caption_gesture_and_local_font_changes()
    {
        using FilterFixture fixture = new();
        (TemplatedControl owner, MenuFlyout menu) = fixture.Menu("tsddbtnBranchFilter");
        menu.ShowAt(owner);
        fixture.Settle();
        MenuItem row = menu.Items.OfType<MenuItem>().First();
        double originalWidth = row.Bounds.Width;
        row.Header = "_A substantially longer source caption for a retained filter command";
        row.FontSize = 16;
        row.InputGesture = new KeyGesture(Key.F12, KeyModifiers.Control | KeyModifiers.Shift);
        fixture.Settle();
        row.Bounds.Width.Should().BeGreaterThan(originalWidth);
        NativeToolStripDropDownLayout.Group sourceMetrics = new(owner, menu.Items.OfType<Control>().ToArray(), null, null);
        row.Bounds.Width.Should().Be(sourceMetrics.ItemWidth);
        row.Bounds.Height.Should().Be(sourceMetrics.ItemHeight);
        row.Classes.Should().NotContain("gitextensions-menu-no-gesture");
        row.InputGesture = null;
        fixture.Settle();
        row.Classes.Should().Contain("gitextensions-menu-no-gesture");
        row.FontSize.Should().Be(16);
    }

    [AvaloniaTest]
    public void Retiring_a_toolbar_scope_during_a_value_callback_should_not_leak_the_returned_value()
    {
        using FilterFixture fixture = new();
        (TemplatedControl owner, MenuFlyout menu) = fixture.Menu("tsddbtnBranchFilter");
        MenuItem row = menu.Items.OfType<MenuItem>().First();
        owner.FontSize = 16;
        bool retired = false;
        row.PropertyChanged += (_, change) =>
        {
            if (!retired && change.Property == TemplatedControl.FontSizeProperty)
            {
                retired = true;
                WinFormsToolStripMenuSizer.ClearToolbarDropDown(menu);
            }
        };
        menu.ShowAt(owner);
        fixture.Settle();
        retired.Should().BeTrue();
        row.Classes.Should().NotContain("gitextensions-filter-menu");
        row.IsSet(Control.WidthProperty).Should().BeFalse();
        row.FontSize.Should().Be(11 * 96d / 72, "the disposed template-priority value must not survive its synchronous retirement callback");
    }

    private static TextBox Editor(ToolbarComboBox combo)
        => combo.GetVisualDescendants().OfType<TextBox>().Single(part => part.Name == "PART_EditableTextBox");

    private static Color ColorOf(IBrush? brush)
        => brush is ISolidColorBrush solid ? solid.Color
            : throw new InvalidOperationException("The actual source palette requires a resolved solid color.");

    private static PixelRect PhysicalBounds(Control control)
    {
        // Logical zero can be the right edge of an RTL visual. Compare actual screen
        // extents so this assertion proves native physical placement, not a mirror artifact.
        PixelPoint first = control.PointToScreen(default);
        PixelPoint opposite = control.PointToScreen(new Point(control.Bounds.Width, control.Bounds.Height));
        return new PixelRect(Math.Min(first.X, opposite.X), Math.Min(first.Y, opposite.Y),
            Math.Abs(opposite.X - first.X), Math.Abs(opposite.Y - first.Y));
    }

    private sealed class FilterFixture : IDisposable
    {
        public FilterFixture(bool rightToLeft = false, bool dark = false)
        {
            Toolbar = new() { Width = 1000, VerticalAlignment = VerticalAlignment.Top };
            IGitModule module = Substitute.For<IGitModule>();
            module.IsValidGitWorkingDir().Returns(true);
            Toolbar.Bind(() => module, Substitute.For<IRevisionGridFilter>());
            IReadOnlyList<IGitRef> refs =
            [
                new GitRef(module, ObjectId.Random(), "refs/heads/main"),
                new GitRef(module, ObjectId.Random(), "refs/heads/feature/filters"),
                new GitRef(module, ObjectId.Random(), "refs/heads/feature/other"),
            ];
            Toolbar.RefreshRevisionFunction(_ => refs);
            Canvas canvas = new();
            Canvas.SetLeft(Toolbar, 300);
            Canvas.SetTop(Toolbar, 12);
            canvas.Children.Add(Toolbar);
            Window = new()
            {
                Width = 1600,
                Height = 600,
                FontSize = 11 * 96d / 72,
                RequestedThemeVariant = dark ? ThemeVariant.Dark : ThemeVariant.Light,
                FlowDirection = rightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
                Resources = { ["GitExtensionsUiFontSize"] = 11 * 96d / 72 },
                Content = canvas,
            };
            Window.Show();
            Settle();
        }

        public FilterToolBar Toolbar { get; }

        public Window Window { get; }

        public IBrush ResourceBrush(string key)
            => Window.TryFindResource(key, Window.ActualThemeVariant, out object? resource) && resource is IBrush brush
                ? brush : throw new InvalidOperationException($"The source color resource '{key}' is missing.");

        public (TemplatedControl Owner, MenuFlyout Menu) Menu(string name)
        {
            TemplatedControl owner = Toolbar.Strip.Items.OfType<TemplatedControl>().Single(item => item.Name == name);
            MenuFlyout? menu = owner switch
            {
                SplitButton split => split.Flyout as MenuFlyout,
                DropDownButton dropDown => dropDown.Flyout as MenuFlyout,
                _ => null,
            };
            return (owner, menu ?? throw new InvalidOperationException("The retained source toolbar item has no menu."));
        }

        public void Click(Control control, Point? localPoint = null)
        {
            TopLevel input = TopLevel.GetTopLevel(control) ?? throw new InvalidOperationException("The actual control has no owned input root.");
            Point local = localPoint ?? new Point(control.Bounds.Width / 2, control.Bounds.Height / 2);
            Point point = control.TranslatePoint(local, input) ?? throw new InvalidOperationException("The actual control has no input coordinate.");
            input.MouseMove(point);
            input.MouseDown(point, MouseButton.Left);
            input.MouseUp(point, MouseButton.Left);
            Settle();
        }

        public void Move(Control control)
        {
            TopLevel input = TopLevel.GetTopLevel(control) ?? throw new InvalidOperationException("The actual control has no owned input root.");
            Point point = control.TranslatePoint(new Point(control.Bounds.Width / 2, control.Bounds.Height / 2), input)
                ?? throw new InvalidOperationException("The actual control has no input coordinate.");
            input.MouseMove(point);
            Settle();
        }

        public void Settle()
        {
            Dispatcher.UIThread.RunJobs();
            Window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
        }

        public void Dispose()
        {
            Toolbar.GetTestAccessor().BranchFilter.IsDropDownOpen = false;
            Toolbar.GetTestAccessor().RevisionFilter.IsDropDownOpen = false;
            foreach (TemplatedControl owner in Toolbar.Strip.Items.OfType<TemplatedControl>())
            {
                if (owner is SplitButton split)
                {
                    split.Flyout?.Hide();
                }
                else if (owner is DropDownButton dropDown)
                {
                    dropDown.Flyout?.Hide();
                }
            }

            Window.Close();
            Toolbar.Strip.Dispose();
        }
    }
}
