using System.Runtime.InteropServices;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitExtUtils.GitUI.Theming;
using GitUI.Compat;
using GitUI.Theming;
using ColorHelper = GitExtUtils.GitUI.Theming.ColorHelper;
using DrawingColor = System.Drawing.Color;
using KnownColor = System.Drawing.KnownColor;

namespace GitExtensionsTests;

/// <summary>
///  Verifies ordinary toolbar backgrounds against original-renderer raster rows and actual control templates.
/// </summary>
[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class NativeToolStripButtonPaintTests
{
    private const string SystemLight = "system-light";
    private const string ProfessionalLight = "professional-light";
    private const string ProfessionalDark = "professional-dark";
    private const string ProfessionalCustom = "professional-custom";
    private const string ButtonClass = "gitextensions-toolbar-button";
    private const string CheckedResource = "GitExtensionsToolStripCheckedBackgroundBrush";
    private const string SelectedResource = "GitExtensionsNativeToolStripButtonSelectedBrush";
    private const string PressedResource = "GitExtensionsNativeToolStripButtonPressedBrush";
    private const string BorderResource = "GitExtensionsNativeToolStripButtonBorderBrush";
    private const string SystemResource = "GitExtensionsNativeToolStripButtonUseSystemVisualStyle";
    private const string LiveBrushResource = "ordinary-toolbar-live-brush";
    private static readonly string[] Contexts = [SystemLight, ProfessionalLight, ProfessionalDark, ProfessionalCustom];

    private static IEnumerable<TestCaseData> NativeCases()
    {
        foreach (string context in Contexts)
        {
            foreach (string state in new[] { "normal", "selected", "pressed", "disabled", "checked", "checked-selected", "checked-pressed", "checked-disabled", "indeterminate" })
            {
                foreach (Size size in new[] { new Size(23, 22), new Size(23, 23), new Size(32, 24) })
                {
                    yield return new TestCaseData(context, state, (int)size.Width, (int)size.Height);
                }
            }
        }
    }

    private static IEnumerable<TestCaseData> TemplateCases()
        => Contexts.SelectMany(context => new[] { new TestCaseData(context, false), new TestCaseData(context, true) });

    private static IEnumerable<TestCaseData> PointerSelectionCases()
        => Contexts.SelectMany(context => new[] { false, true }.SelectMany(toggle =>
            new[] { new TestCaseData(context, toggle, false), new TestCaseData(context, toggle, true) }));

    private static IEnumerable<TestCaseData> KeyboardSelectionCases()
        => Contexts.SelectMany(context => new[] { false, true }.SelectMany(toggle =>
            new[] { new TestCaseData(context, toggle, NavigationMethod.Tab), new TestCaseData(context, toggle, NavigationMethod.Directional) }));

    private static IEnumerable<TestCaseData> BrushCases()
    {
        foreach (string brush in new[] { "Checked", "Selected", "Pressed", "Border", "SelectedCorner", "SelectedAdjacent", "PressedCorner", "PressedAdjacent" })
        {
            yield return new TestCaseData(brush, false);
            yield return new TestCaseData(brush, true);
        }
    }

    /// <summary>
    ///  Covers the same four renderer contexts, nine state branches and three dimensions as the original bitmap consumer.
    /// </summary>
    [AvaloniaTest]
    [TestCaseSource(nameof(NativeCases))]
    public void Render_should_match_every_original_native_row_over_three_independent_backdrops(string context, string state, int width, int height)
    {
        using PaintFixture fixture = new(context, width, height);
        ApplyState(fixture.Chrome, state);
        foreach (Color backdrop in new[] { Colors.Black, Colors.White, Color.Parse("#2B2D3A") })
        {
            fixture.Canvas.Background = new SolidColorBrush(backdrop);
            fixture.Layout();
            using WriteableBitmap frame = Capture(fixture.Window);
            AssertRaster(frame, default, width, height, NativeRows(context, state, width, height, backdrop));
        }

        fixture.Chrome.Bounds.Should().Be(new Rect(0, 0, width, height));
        fixture.Chrome.IsHitTestVisible.Should().BeFalse();
        fixture.Chrome.Focusable.Should().BeFalse();
    }

    /// <summary>
    ///  Exercises the real Button and ToggleButton selectors, pointer route, keyboard focus and disabled precedence.
    /// </summary>
    [AvaloniaTest]
    [TestCaseSource(nameof(TemplateCases))]
    public void Actual_template_should_follow_checked_hover_pressed_focus_and_disabled_states_without_intercepting_clicks(string context, bool toggle)
    {
        using PaintFixture fixture = new(context, toggle: toggle);
        Button button = fixture.Button ?? throw new AssertionException("A real toolbar button is required.");
        AssertNoFluentMotion(button);
        int clicks = 0;
        button.Click += (_, _) => clicks++;
        Rect allocation = button.Bounds;
        ContentPresenter presenter = button.GetVisualDescendants().OfType<ContentPresenter>().Single(part => part.Name == "PART_ContentPresenter");
        Rect contentBounds = presenter.Bounds;
        AssertTemplate(fixture, context, "normal");
        fixture.Window.MouseMove(new Point(11, 11));
        fixture.Layout();
        button.IsPointerOver.Should().BeTrue();
        fixture.Chrome.IsSelected.Should().BeTrue();
        AssertTemplate(fixture, context, "selected");
        fixture.Window.MouseDown(new Point(11, 11), MouseButton.Left);
        AssertNoFluentMotion(button);
        fixture.Layout();
        fixture.Chrome.IsPressed.Should().BeTrue();
        AssertTemplate(fixture, context, "pressed");
        fixture.Window.MouseUp(new Point(11, 11), MouseButton.Left);
        AssertNoFluentMotion(button);
        fixture.Layout();
        clicks.Should().Be(1, "the noninteractive chrome must leave the original button click route intact");
        SetChecked(button, true);
        fixture.Layout();
        AssertTemplate(fixture, context, "checked-selected");
        fixture.Window.MouseDown(new Point(11, 11), MouseButton.Left);
        AssertNoFluentMotion(button);
        fixture.Layout();
        AssertTemplate(fixture, context, "checked-pressed");
        fixture.Window.MouseUp(new Point(11, 11), MouseButton.Left);
        AssertNoFluentMotion(button);
        fixture.Layout();
        clicks.Should().Be(2);
        SetChecked(button, true);
        fixture.FocusSink.Focus();
        fixture.Window.MouseMove(new Point(150, 75));
        fixture.Layout();
        AssertTemplate(fixture, context, "checked");
        button.Focus(NavigationMethod.Tab).Should().BeTrue();
        fixture.Layout();
        button.IsFocused.Should().BeTrue();
        button.FocusAdorner.Should().BeNull();
        AssertTemplate(fixture, context, "checked-selected");
        button.IsEnabled = false;
        fixture.Layout();
        fixture.Chrome.IsEffectivelyEnabled.Should().BeFalse();
        AssertTemplate(fixture, context, "checked-disabled");
        SetChecked(button, false);
        fixture.Layout();
        AssertTemplate(fixture, context, "disabled");
        button.IsEnabled = true;
        fixture.FocusSink.Focus();
        fixture.Window.MouseMove(new Point(150, 75));
        fixture.Layout();
        AssertTemplate(fixture, context, "normal");
        if (button is ToggleButton threeState)
        {
            threeState.IsThreeState = true;
            threeState.IsChecked = null;
            fixture.Layout();
            AssertTemplate(fixture, context, "indeterminate");
        }

        button.Bounds.Should().Be(allocation);
        presenter.Bounds.Should().Be(contentBounds);
        fixture.Chrome.Bounds.Size.Should().Be(allocation.Size);
        fixture.Chrome.IsHitTestVisible.Should().BeFalse();
        fixture.Chrome.Focusable.Should().BeFalse();
        button.GetVisualDescendants().OfType<NativeToolStripButtonChrome>().Should().ContainSingle();
    }

    /// <summary>
    ///  Separates the source selected bit from Avalonia's focus retained after an ordinary pointer click.
    /// </summary>
    [AvaloniaTest]
    [TestCaseSource(nameof(PointerSelectionCases))]
    public void Pointer_click_then_leave_should_clear_selected_paint_without_moving_retained_focus(string context, bool toggle, bool check)
    {
        using PaintFixture fixture = new(context, toggle: toggle);
        Button button = fixture.Button ?? throw new AssertionException("A toolbar owner is required.");
        int clicks = 0;
        button.Click += (_, _) => clicks++;
        fixture.Window.MouseMove(new Point(11, 11));
        fixture.Window.MouseDown(new Point(11, 11), MouseButton.Left);
        fixture.Window.MouseUp(new Point(11, 11), MouseButton.Left);
        SetChecked(button, check);
        fixture.Layout();
        button.IsFocused.Should().BeTrue();
        fixture.Chrome.IsSelected.Should().BeTrue();
        clicks.Should().Be(1);
        fixture.Window.MouseMove(new Point(150, 75));
        fixture.Layout();
        button.IsFocused.Should().BeTrue("mouse leave does not move Avalonia focus, but original ToolStripItem.HandleLeave clears selection anyway");
        fixture.Chrome.IsSelected.Should().BeFalse();
        fixture.Chrome.IsPressed.Should().BeFalse();
        AssertTemplate(fixture, context, check ? "checked" : "normal");
        clicks.Should().Be(1);
    }

    /// <summary>
    ///  Covers keyboard-only selection and the source's unconditional mouse-leave clearing even while keyboard focus remains.
    /// </summary>
    [AvaloniaTest]
    [TestCaseSource(nameof(KeyboardSelectionCases))]
    public void Keyboard_selection_should_paint_outside_the_pointer_then_clear_after_pointer_enter_and_leave(string context, bool toggle, NavigationMethod method)
    {
        using PaintFixture fixture = new(context, toggle: toggle);
        Button button = fixture.Button ?? throw new AssertionException("A toolbar owner is required.");
        button.Focus(method).Should().BeTrue();
        fixture.Layout();
        button.IsPointerOver.Should().BeFalse();
        fixture.Chrome.IsSelected.Should().BeTrue();
        AssertTemplate(fixture, context, "selected");
        fixture.Window.MouseMove(new Point(11, 11));
        fixture.Window.MouseMove(new Point(150, 75));
        fixture.Layout();
        button.IsFocused.Should().BeTrue();
        fixture.Chrome.IsSelected.Should().BeFalse("source MouseLeave clears the selected bit without consulting KeyboardActive or focus-visible");
        AssertTemplate(fixture, context, "normal");
        fixture.FocusSink.Focus();
        button.Focus(method).Should().BeTrue();
        fixture.Layout();
        fixture.Chrome.IsSelected.Should().BeTrue();
        fixture.FocusSink.Focus();
        fixture.Layout();
        fixture.Chrome.IsSelected.Should().BeFalse();
    }

    /// <summary>
    ///  Leaves input capture and command cancellation with the original Button while retiring source selected/pressed paint.
    /// </summary>
    [AvaloniaTest]
    [TestCaseSource(nameof(TemplateCases))]
    public void Pointer_leave_during_a_press_should_clear_state_without_executing_the_button_command(string context, bool toggle)
    {
        using PaintFixture fixture = new(context, toggle: toggle);
        Button button = fixture.Button ?? throw new AssertionException("A toolbar owner is required.");
        int clicks = 0;
        button.Click += (_, _) => clicks++;
        fixture.Window.MouseMove(new Point(11, 11));
        fixture.Window.MouseDown(new Point(11, 11), MouseButton.Left);
        fixture.Layout();
        fixture.Chrome.IsPressed.Should().BeTrue();
        fixture.Window.MouseMove(new Point(150, 75));
        fixture.Layout();
        fixture.Chrome.IsSelected.Should().BeFalse();
        fixture.Chrome.IsPressed.Should().BeFalse();
        AssertTemplate(fixture, context, "normal");
        fixture.Window.MouseUp(new Point(150, 75), MouseButton.Left);
        clicks.Should().Be(0);
    }

    /// <summary>
    ///  Preserves a real keyboard press outside the pointer while the native selected bit remains set.
    /// </summary>
    [AvaloniaTest]
    [TestCaseSource(nameof(TemplateCases))]
    public void Keyboard_selected_owner_should_keep_pressed_paint_outside_the_pointer_without_changing_the_command_route(string context, bool toggle)
    {
        using PaintFixture fixture = new(context, toggle: toggle);
        Button button = fixture.Button ?? throw new AssertionException("A toolbar owner is required.");
        int clicks = 0;
        button.Click += (_, _) => clicks++;
        button.Focus(NavigationMethod.Tab).Should().BeTrue();
        fixture.Window.KeyPress(Key.Space, RawInputModifiers.None, PhysicalKey.Space, keySymbol: " ");
        fixture.Layout();
        button.IsPointerOver.Should().BeFalse();
        fixture.Chrome.IsSelected.Should().BeTrue();
        fixture.Chrome.IsPressed.Should().BeTrue();
        AssertTemplate(fixture, context, "pressed");
        fixture.Window.KeyRelease(Key.Space, RawInputModifiers.None, PhysicalKey.Space, keySymbol: " ");
        fixture.Layout();
        fixture.Chrome.IsPressed.Should().BeFalse();
        fixture.Chrome.IsSelected.Should().BeTrue();
        clicks.Should().Be(1);
    }

    /// <summary>
    ///  Retains only the owner's source bit across template replacement and releases every retired chrome subscription.
    /// </summary>
    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Retemplating_should_preserve_cleared_or_selected_bits_without_resurrecting_focus_or_retaining_retired_chrome(bool toggle)
    {
        using PaintFixture fixture = new(SystemLight, toggle: toggle);
        Button button = fixture.Button ?? throw new AssertionException("A toolbar owner is required.");
        button.Focus(NavigationMethod.Tab).Should().BeTrue();
        fixture.Layout();
        NativeToolStripButtonChrome retired = fixture.Chrome;
        retired.IsSelected.Should().BeTrue();
        NativeToolStripButtonChrome replacement = ReapplyTemplate(fixture, button);
        replacement.Should().NotBeSameAs(retired);
        replacement.IsSelected.Should().BeTrue("retemplating preserves the owner's bit rather than treating pointer focus as a fresh selection");
        retired.IsSelected.Should().BeFalse();
        TopLevel.GetTopLevel(retired).Should().BeNull();
        fixture.Window.MouseMove(new Point(11, 11));
        fixture.Window.MouseMove(new Point(150, 75));
        fixture.Layout();
        button.IsFocused.Should().BeTrue();
        replacement.IsSelected.Should().BeFalse();
        NativeToolStripButtonChrome current = ReapplyTemplate(fixture, button);
        current.IsSelected.Should().BeFalse("a retained focus-visible cue cannot resurrect the bit cleared by source MouseLeave");
        fixture.FocusSink.Focus();
        button.Focus(NavigationMethod.Directional).Should().BeTrue();
        fixture.Layout();
        current.IsSelected.Should().BeTrue();
        retired.IsSelected.Should().BeFalse();
        replacement.IsSelected.Should().BeFalse();
        fixture.Canvas.Children.Remove(button);
        fixture.Layout();
        current.IsSelected.Should().BeFalse();
        TopLevel.GetTopLevel(current).Should().BeNull();
        fixture.Canvas.Children.Add(button);
        fixture.Layout();
        fixture.FocusSink.Focus();
        button.Focus(NavigationMethod.Tab).Should().BeTrue();
        fixture.Layout();
        button.GetVisualDescendants().OfType<NativeToolStripButtonChrome>().Should().ContainSingle();
        current.IsSelected.Should().BeTrue();
        retired.IsSelected.Should().BeFalse();
        replacement.IsSelected.Should().BeFalse();
    }

    /// <summary>
    ///  Keeps the existing image/caption allocation and live resource lookup when the actual owner is detached and reused.
    /// </summary>
    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Existing_template_should_preserve_content_geometry_and_rejoin_theme_resources_after_reparenting(bool toggle)
    {
        using PaintFixture fixture = new(SystemLight, 83, 23, toggle, includeContent: true);
        Button button = fixture.Button ?? throw new AssertionException("A toolbar owner is required.");
        Image image = button.GetVisualDescendants().OfType<Image>().Single();
        TextBlock caption = button.GetVisualDescendants().OfType<TextBlock>().Single(text => text.Text == "Run");
        Rect allocation = button.Bounds;
        Rect imageBounds = image.Bounds;
        Rect captionBounds = caption.Bounds;
        Point imageOrigin = image.TranslatePoint(default, button) ?? throw new AssertionException("The image must belong to the toolbar owner.");
        Point captionOrigin = caption.TranslatePoint(default, button) ?? throw new AssertionException("The caption must belong to the toolbar owner.");
        NativeToolStripButtonChrome chrome = fixture.Chrome;
        image.Bounds.Size.Should().Be(new Size(16, 16));
        foreach (string context in Contexts)
        {
            fixture.ApplyTheme(context);
            SetChecked(button, true);
            fixture.Window.MouseMove(new Point(11, 11));
            fixture.Layout();
            button.Bounds.Should().Be(allocation);
            image.Bounds.Should().Be(imageBounds);
            caption.Bounds.Should().Be(captionBounds);
            image.TranslatePoint(default, button).Should().Be(imageOrigin);
            caption.TranslatePoint(default, button).Should().Be(captionOrigin);
            chrome.UseSystemVisualStyle.Should().Be(context == SystemLight);
        }

        fixture.Canvas.Children.Remove(button);
        fixture.Layout();
        TopLevel.GetTopLevel(chrome).Should().BeNull();
        Canvas replacement = new() { Background = Brushes.White };
        replacement.Children.Add(button);
        fixture.Window.Content = replacement;
        fixture.ApplyTheme(SystemLight);
        fixture.Layout();
        button.GetVisualDescendants().OfType<NativeToolStripButtonChrome>().Should().ContainSingle().Which.Should().BeSameAs(chrome);
        TopLevel.GetTopLevel(chrome).Should().BeSameAs(fixture.Window);
        button.Bounds.Should().Be(allocation);
        image.TranslatePoint(default, button).Should().Be(imageOrigin);
        caption.TranslatePoint(default, button).Should().Be(captionOrigin);
        int clicks = 0;
        button.Click += (_, _) => clicks++;
        fixture.Window.MouseDown(new Point(11, 11), MouseButton.Left);
        fixture.Window.MouseUp(new Point(11, 11), MouseButton.Left);
        clicks.Should().Be(1);
    }

    /// <summary>
    ///  Checks all live palette properties both through replacement resources and a mutable existing brush.
    /// </summary>
    [AvaloniaTest]
    [TestCaseSource(nameof(BrushCases))]
    public void DynamicPaletteAffectsRender_should_repaint_an_existing_chrome_without_layout_or_manual_invalidation(string property, bool mutateBrush)
    {
        using PaintFixture fixture = new(SystemLight);
        (StyledProperty<IBrush?> target, PixelPoint pixel, string state) = BrushTarget(property);
        SolidColorBrush brush = new(Colors.Magenta);
        fixture.Window.Resources[LiveBrushResource] = brush;
        fixture.Chrome[!target] = new DynamicResourceExtension(LiveBrushResource);
        ApplyState(fixture.Chrome, state);
        fixture.Layout();
        Rect allocation = fixture.Chrome.Bounds;
        using (WriteableBitmap before = Capture(fixture.Window))
        {
            ReadPixel(before, pixel).Should().Be(Colors.Magenta);
        }

        if (mutateBrush)
        {
            brush.Color = Colors.Orange;
        }
        else
        {
            fixture.Window.Resources[LiveBrushResource] = Brushes.Orange;
        }

        Dispatcher.UIThread.RunJobs();
        using WriteableBitmap after = Capture(fixture.Window);
        ReadPixel(after, pixel).Should().Be(Colors.Orange, "AffectsRender must observe both changed resources and changed brush contents");
        fixture.Chrome.Bounds.Should().Be(allocation);
    }

    /// <summary>
    ///  Distinguishes live renderer selection from color mode and checks the final source frame row after a resource change.
    /// </summary>
    [AvaloniaTest]
    public void DynamicPaletteAffectsRender_should_change_system_frame_height_without_recreating_the_owner()
    {
        using PaintFixture fixture = new(SystemLight, toggle: false);
        Button button = fixture.Button ?? throw new AssertionException("A toolbar owner is required.");
        SetChecked(button, true);
        fixture.Canvas.Background = Brushes.Black;
        fixture.Layout();
        Rect allocation = button.Bounds;
        using (WriteableBitmap before = Capture(fixture.Window))
        {
            ReadPixel(before, new PixelPoint(11, 21)).Should().Be(Colors.Black);
        }

        fixture.Window.Resources[SystemResource] = false;
        Dispatcher.UIThread.RunJobs();
        fixture.Chrome.UseSystemVisualStyle.Should().BeFalse();
        using WriteableBitmap after = Capture(fixture.Window);
        ReadPixel(after, new PixelPoint(11, 21)).Should().Be(Color.Parse("#0078D4"));
        button.Bounds.Should().Be(allocation);
    }

    /// <summary>
    ///  Preserves the raw native color-table boundary while independently configurable theme resources still honor CSS colors.
    /// </summary>
    [AvaloniaTest]
    [TestCase(SystemLight)]
    [TestCase(ProfessionalLight)]
    [TestCase(ProfessionalDark)]
    [TestCase(ProfessionalCustom)]
    public void Projected_resources_should_use_original_native_palette_not_configured_window_or_highlight_colors(string context)
    {
        using PaintFixture fixture = new(context);
        bool system = context == SystemLight;
        bool dark = IsDark(context);
        fixture.ResourceColor("GitExtensionsHighlightBackgroundBrush").Should().Be(Colors.Magenta);
        fixture.ResourceColor("GitExtensionsWindowBackgroundBrush").Should().Be(Colors.Lime);
        fixture.ResourceColor(BorderResource).Should().Be(Color.Parse(dark ? "#2864B4" : "#0078D4"));
        fixture.ResourceColor(CheckedResource).Should().Be(Color.Parse(system ? "#CCE8FF" : "#00000000"));
        fixture.ResourceColor(SelectedResource).Should().Be(Color.Parse(system ? "#E5F3FF" : dark ? "#2F4159" : "#B3D7F2"));
        fixture.ResourceColor(PressedResource).Should().Be(Color.Parse(system ? "#CCE8FF" : dark ? "#2D4B73" : "#80BCEA"));
        fixture.Chrome.UseSystemVisualStyle.Should().Be(system);
    }

    /// <summary>
    ///  Checks the shipped style dictionaries before runtime theme publication supplies any overrides.
    /// </summary>
    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Fallback_resources_should_supply_original_system_light_and_professional_dark_pixels(bool dark)
    {
        string context = dark ? ProfessionalDark : SystemLight;
        using PaintFixture fixture = new(context, publishTheme: false);
        ApplyState(fixture.Chrome, "checked");
        fixture.Canvas.Background = Brushes.Black;
        fixture.Layout();
        fixture.Chrome.UseSystemVisualStyle.Should().Be(!dark);
        fixture.ResourceColor(CheckedResource).Should().Be(Color.Parse(dark ? "#00000000" : "#CCE8FF"));
        using WriteableBitmap frame = Capture(fixture.Window);
        AssertRaster(frame, default, 23, 22, NativeRows(context, "checked", 23, 22, Colors.Black));
    }

    private static void AssertTemplate(PaintFixture fixture, string context, string state)
    {
        Button button = fixture.Button ?? throw new AssertionException("A toolbar owner is required.");
        fixture.Chrome.IsChecked.Should().Be(state.StartsWith("checked", StringComparison.Ordinal) || state == "indeterminate");
        ContentPresenter presenter = button.GetVisualDescendants().OfType<ContentPresenter>().Single(part => part.Name == "PART_ContentPresenter");
        ((ISolidColorBrush)button.Background!).Color.Should().Be(Colors.Transparent, "the old hover fill must not leak through native transparent or final-row pixels");
        ((ISolidColorBrush)presenter.Background!).Color.Should().Be(Colors.Transparent);
        using WriteableBitmap frame = Capture(fixture.Window);
        AssertRaster(frame, default, 23, 22, NativeRows(context, state, 23, 22, Color.Parse("#2B2D3A")));
    }

    private static void AssertNoFluentMotion(Button button)
    {
        button.Transitions.Should().BeNull("native ToolStrip state changes do not animate the item transform");
        (button.RenderTransform?.Value ?? Matrix.Identity).Should().Be(Matrix.Identity,
            "pressing or releasing the source item changes paint without scaling its chrome or content");
    }

    private static void SetChecked(Button button, bool value)
    {
        if (button is ToggleButton toggle)
        {
            toggle.IsChecked = value;
        }
        else
        {
            button.Classes.Set("checked", value);
        }
    }

    private static bool IsDark(string context) => context is ProfessionalDark or ProfessionalCustom;

    private static NativeToolStripButtonChrome ReapplyTemplate(PaintFixture fixture, Button button)
    {
        IControlTemplate template = button.Template ?? throw new AssertionException("The actual toolbar style template must exist.");
        button.Template = null;
        button.ApplyTemplate();
        fixture.Layout();
        button.Template = template;
        button.ApplyTemplate();
        fixture.Layout();
        return button.GetVisualDescendants().OfType<NativeToolStripButtonChrome>().Single();
    }

    private static void ApplyState(NativeToolStripButtonChrome chrome, string state)
    {
        chrome.IsChecked = state.StartsWith("checked", StringComparison.Ordinal) || state == "indeterminate";
        chrome.IsSelected = state is "selected" or "pressed" or "checked-selected" or "checked-pressed";
        chrome.IsPressed = state is "pressed" or "checked-pressed";
        chrome.IsEnabled = state is not "disabled" and not "checked-disabled";
    }

    private static Color[][] NativeRows(string context, string state, int width, int height, Color backdrop)
    {
        bool system = context == SystemLight;
        bool check = state.StartsWith("checked", StringComparison.Ordinal) || state == "indeterminate";
        bool selected = state is "selected" or "pressed" or "checked-selected" or "checked-pressed";
        bool pressed = state is "pressed" or "checked-pressed";
        bool paint = check || selected || pressed;
        bool hot = system && (check ? selected : selected && !pressed);
        Color border = Color.Parse(IsDark(context) ? "#2864B4" : "#0078D4");
        Color fill = !system && check && !selected ? backdrop
            : Color.Parse(system ? hot ? "#E5F3FF" : "#CCE8FF"
                : IsDark(context) ? pressed || (check && selected) ? "#2D4B73" : "#2F4159"
                : pressed || (check && selected) ? "#80BCEA" : "#B3D7F2");
        Color corner = Color.Parse(hot ? "#D5ECFF" : "#AAD9FF");
        Color adjacent = Color.Parse(hot ? "#E3F2FF" : "#C8E6FF");
        int frameRows = height - (system ? 1 : 0);
        Color[][] rows = new Color[height][];
        for (int y = 0; y < height; y++)
        {
            // These run classes are retained original renderer output over black,
            // white and custom backgrounds, not calls back into the portable painter.
            rows[y] = !paint || y >= frameRows ? Run((width, backdrop))
                : y == 0 || y == frameRows - 1 ? Run((width, border))
                : system && (y == 1 || y == frameRows - 2)
                    ? Run((1, border), (1, corner), (1, adjacent), (width - 6, fill), (1, adjacent), (1, corner), (1, border))
                : system && (y == 2 || y == frameRows - 3)
                    ? Run((1, border), (1, adjacent), (width - 4, fill), (1, adjacent), (1, border))
                : Run((1, border), (width - 2, fill), (1, border));
        }

        return rows;
    }

    private static Color[] Run(params (int Count, Color Color)[] runs)
        => runs.SelectMany(run => Enumerable.Repeat(run.Color, run.Count)).ToArray();

    private static (StyledProperty<IBrush?> Property, PixelPoint Pixel, string State) BrushTarget(string name)
        => name switch
        {
            "Checked" => (NativeToolStripButtonChrome.CheckedBrushProperty, new PixelPoint(11, 11), "checked"),
            "Selected" => (NativeToolStripButtonChrome.SelectedBrushProperty, new PixelPoint(11, 11), "selected"),
            "Pressed" => (NativeToolStripButtonChrome.PressedBrushProperty, new PixelPoint(11, 11), "pressed"),
            "Border" => (NativeToolStripButtonChrome.BorderBrushProperty, new PixelPoint(0, 0), "checked"),
            "SelectedCorner" => (NativeToolStripButtonChrome.SelectedCornerBrushProperty, new PixelPoint(1, 1), "selected"),
            "SelectedAdjacent" => (NativeToolStripButtonChrome.SelectedAdjacentBrushProperty, new PixelPoint(2, 1), "selected"),
            "PressedCorner" => (NativeToolStripButtonChrome.PressedCornerBrushProperty, new PixelPoint(1, 1), "checked"),
            "PressedAdjacent" => (NativeToolStripButtonChrome.PressedAdjacentBrushProperty, new PixelPoint(2, 1), "checked"),
            _ => throw new ArgumentOutOfRangeException(nameof(name)),
        };

    private static WriteableBitmap Capture(Window window)
        => window.CaptureRenderedFrame() ?? throw new AssertionException("The actual toolbar frame must be available.");

    private static Color ReadPixel(WriteableBitmap frame, PixelPoint point)
    {
        using ILockedFramebuffer buffer = frame.Lock();
        return ReadPixel(buffer, point.X, point.Y);
    }

    private static Color ReadPixel(ILockedFramebuffer buffer, int x, int y)
    {
        uint pixel = unchecked((uint)Marshal.ReadInt32(buffer.Address, (y * buffer.RowBytes) + (x * 4)));
        return buffer.Format == PixelFormat.Bgra8888 ? Color.FromUInt32(pixel)
            : Color.FromArgb((byte)(pixel >> 24), (byte)pixel, (byte)(pixel >> 8), (byte)(pixel >> 16));
    }

    private static void AssertRaster(WriteableBitmap frame, PixelPoint origin, int width, int height, Color[][] expected)
    {
        using ILockedFramebuffer buffer = frame.Lock();
        buffer.Format.BitsPerPixel.Should().Be(32);
        for (int y = 0; y < height; y++)
        {
            Color[] actual = Enumerable.Range(0, width).Select(x => ReadPixel(buffer, origin.X + x, origin.Y + y)).ToArray();
            actual.Should().Equal(expected[y], "every original native RLE pixel must match at row {0}", y);
        }
    }

    private static void BindPalette(NativeToolStripButtonChrome chrome)
    {
        chrome[!NativeToolStripButtonChrome.UseSystemVisualStyleProperty] = new DynamicResourceExtension(SystemResource);
        chrome[!NativeToolStripButtonChrome.CheckedBrushProperty] = new DynamicResourceExtension(CheckedResource);
        chrome[!NativeToolStripButtonChrome.SelectedBrushProperty] = new DynamicResourceExtension(SelectedResource);
        chrome[!NativeToolStripButtonChrome.PressedBrushProperty] = new DynamicResourceExtension(PressedResource);
        chrome[!NativeToolStripButtonChrome.BorderBrushProperty] = new DynamicResourceExtension(BorderResource);
        chrome[!NativeToolStripButtonChrome.SelectedCornerBrushProperty] = new DynamicResourceExtension("GitExtensionsNativeToolStripButtonSelectedCornerBrush");
        chrome[!NativeToolStripButtonChrome.SelectedAdjacentBrushProperty] = new DynamicResourceExtension("GitExtensionsNativeToolStripButtonSelectedAdjacentBrush");
        chrome[!NativeToolStripButtonChrome.PressedCornerBrushProperty] = new DynamicResourceExtension("GitExtensionsNativeToolStripButtonPressedCornerBrush");
        chrome[!NativeToolStripButtonChrome.PressedAdjacentBrushProperty] = new DynamicResourceExtension("GitExtensionsNativeToolStripButtonPressedAdjacentBrush");
    }

    private sealed class PaintFixture : IDisposable
    {
        private readonly Application _application;
        private readonly ThemeSettings _originalSettings;
        private readonly Dictionary<ThemeVariant, IThemeVariantProvider?> _originalResources = [];

        /// <summary>
        ///  Isolates native palette publication and creates either bitmap-only chrome or an actual opted-in control template.
        /// </summary>
        public PaintFixture(string context, int width = 23, int height = 22, bool? toggle = null, bool includeContent = false, bool publishTheme = true)
        {
            _application = Application.Current ?? throw new AssertionException("The headless application must be initialized.");
            _originalSettings = ThemeModule.Settings;
            foreach (ThemeVariant variant in new[] { ThemeVariant.Light, ThemeVariant.Dark })
            {
                _application.Resources.ThemeDictionaries.TryGetValue(variant, out IThemeVariantProvider? previous);
                _originalResources.Add(variant, previous);
                _application.Resources.ThemeDictionaries[variant] = new ResourceDictionary();
            }

            Canvas = new Canvas { Background = Brush.Parse("#2B2D3A") };
            FocusSink = new Button { Width = 20, Height = 20 };
            Avalonia.Controls.Canvas.SetLeft(FocusSink, 110);
            Avalonia.Controls.Canvas.SetTop(FocusSink, 50);
            Canvas.Children.Add(FocusSink);
            NativeToolStripButtonChrome? standalone = null;
            if (toggle is null)
            {
                standalone = new NativeToolStripButtonChrome { Width = width, Height = height };
                BindPalette(standalone);
                Canvas.Children.Add(standalone);
            }
            else
            {
                Button = toggle.Value ? new ToggleButton() : new Button();
                Button.Classes.Add(ButtonClass);
                Button.Width = width;
                Button.Height = height;
                Button.MinWidth = 0;
                Button.MinHeight = 0;
                Button.HorizontalAlignment = HorizontalAlignment.Left;
                Button.VerticalAlignment = VerticalAlignment.Top;
                if (includeContent)
                {
                    Image image = new()
                    {
                        Width = 16,
                        Height = 16,
                        Source = new DrawingImage { Drawing = new GeometryDrawing { Brush = Brushes.Magenta, Geometry = new RectangleGeometry(new Rect(0, 0, 16, 16)) } },
                    };
                    Button.Content = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 3, Children = { image, new TextBlock { Text = "Run" } } };
                }

                Canvas.Children.Add(Button);
            }

            Window = new Window { Width = 160, Height = 80, Content = Canvas, RequestedThemeVariant = IsDark(context) ? ThemeVariant.Dark : ThemeVariant.Light };
            if (publishTheme)
            {
                ApplyTheme(context);
            }

            Window.Show();
            Layout();
            FocusSink.Focus();
            Window.MouseMove(new Point(150, 75));
            Layout();
            Chrome = standalone ?? (Button ?? throw new AssertionException("The fixture must own either chrome or a toolbar button."))
                .GetVisualDescendants().OfType<NativeToolStripButtonChrome>().Single();
        }

        /// <summary>
        ///  Gets the actual headless window used for framework rendering and input routing.
        /// </summary>
        public Window Window { get; }

        /// <summary>
        ///  Gets the real background owner rather than a precomposed expected image.
        /// </summary>
        public Canvas Canvas { get; }

        /// <summary>
        ///  Gets the existing renderer, either standalone or materialized by the real toolbar template.
        /// </summary>
        public NativeToolStripButtonChrome Chrome { get; }

        /// <summary>
        ///  Gets the optional real Button or ToggleButton owner.
        /// </summary>
        public Button? Button { get; }

        /// <summary>
        ///  Gets a separate focus target so pointer-only and keyboard-selected states remain distinguishable.
        /// </summary>
        public Button FocusSink { get; }

        /// <summary>
        ///  Publishes a consumer-owned theme while leaving the native renderer palette independent of configurable CSS colors.
        /// </summary>
        public void ApplyTheme(string context)
        {
            bool dark = IsDark(context);
            Theme theme = new(
                new Dictionary<AppColor, DrawingColor> { [AppColor.PanelBackground] = dark ? DrawingColor.FromArgb(43, 45, 58) : DrawingColor.White },
                new Dictionary<KnownColor, DrawingColor>
                {
                    [KnownColor.Window] = DrawingColor.Lime,
                    [KnownColor.Highlight] = DrawingColor.Magenta,
                    [KnownColor.ControlDark] = DrawingColor.Cyan,
                },
                new ThemeId($"ordinary-toolbar-{context}"));
            AvaloniaThemeResources.Apply(_application, new ThemeSettings(theme, Theme.Default, ThemeVariations.None, useSystemVisualStyle: context == SystemLight));
            Window.RequestedThemeVariant = dark ? ThemeVariant.Dark : ThemeVariant.Light;
            Layout();
        }

        /// <summary>
        ///  Flushes normal layout and style work without manually invalidating any render property.
        /// </summary>
        public void Layout()
        {
            Dispatcher.UIThread.RunJobs();
            Window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
        }

        /// <summary>
        ///  Resolves the live resource consumed by the actual toolbar template.
        /// </summary>
        public Color ResourceColor(string key)
        {
            Window.TryFindResource(key, Window.ActualThemeVariant, out object? value).Should().BeTrue();
            return ((ISolidColorBrush)(value ?? throw new AssertionException("The native brush resource must exist."))).Color;
        }

        /// <summary>
        ///  Restores the application's prior dictionaries and theme settings after retiring the owned window.
        /// </summary>
        public void Dispose()
        {
            try
            {
                RetainDiagnostics();
            }
            finally
            {
                Window.Close();
                Dispatcher.UIThread.RunJobs();
                foreach ((ThemeVariant variant, IThemeVariantProvider? previous) in _originalResources)
                {
                    if (previous is null)
                    {
                        _application.Resources.ThemeDictionaries.Remove(variant);
                    }
                    else
                    {
                        _application.Resources.ThemeDictionaries[variant] = previous;
                    }
                }

                ColorHelper.ThemeSettings = _originalSettings;
            }
        }

        private static object? DescribeBrush(IBrush? brush)
            => brush is ISolidColorBrush solid
                ? new { argb = $"#{solid.Color.ToUInt32():X8}", solid.Opacity }
                : brush?.ToString();

        private void RetainDiagnostics()
        {
            string? evidenceRoot = Environment.GetEnvironmentVariable("GITEXT_MENU_PAINT_EVIDENCE");
            if (Button is null || string.IsNullOrWhiteSpace(evidenceRoot))
            {
                return;
            }

            // parity-scaffolding: capture before disposal so an assertion before
            // the pixel comparison still retains the failing actual template.
            string directory = Path.Combine(Path.GetFullPath(evidenceRoot), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            using WriteableBitmap frame = Capture(Window);
            frame.Save(Path.Combine(directory, "actual-toolbar.png"), PngBitmapEncoderOptions.Default);
            NativeToolStripButtonChrome? chrome = Button.GetVisualDescendants().OfType<NativeToolStripButtonChrome>().FirstOrDefault();
            object[] controls = Window.GetVisualDescendants().OfType<Control>().Select(control => new
            {
                type = control.GetType().FullName,
                control.Name,
                classes = control.Classes.ToArray(),
                bounds = control.Bounds.ToString(),
                origin = control.TranslatePoint(default, Window)?.ToString(),
                templatedParent = control.TemplatedParent?.GetType().FullName,
                control.IsVisible,
                control.IsEffectivelyEnabled,
                control.Opacity,
                background = DescribeBrush(control switch
                {
                    Border border => border.Background,
                    Panel panel => panel.Background,
                    ContentPresenter presenter => presenter.Background,
                    TemplatedControl templated => templated.Background,
                    _ => null,
                }),
            }).Cast<object>().ToArray();
            Dictionary<string, object?> resources = [];
            foreach (string key in new[]
            {
                SystemResource, CheckedResource, SelectedResource, PressedResource, BorderResource,
                "GitExtensionsNativeToolStripButtonSelectedCornerBrush", "GitExtensionsNativeToolStripButtonSelectedAdjacentBrush",
                "GitExtensionsNativeToolStripButtonPressedCornerBrush", "GitExtensionsNativeToolStripButtonPressedAdjacentBrush",
            })
            {
                Window.TryFindResource(key, Window.ActualThemeVariant, out object? value);
                resources.Add(key, value is IBrush brush ? DescribeBrush(brush) : value);
            }

            File.WriteAllText(Path.Combine(directory, "actual-toolbar.json"), JsonSerializer.Serialize(new
            {
                test = TestContext.CurrentContext.Test.FullName,
                phase = "actualTemplateBeforeFixtureDisposal",
                actualTheme = Window.ActualThemeVariant.ToString(),
                Window.RenderScaling,
                owner = new
                {
                    type = Button.GetType().FullName,
                    Button.IsPointerOver,
                    Button.IsFocused,
                    Button.IsPressed,
                    Button.IsEffectivelyEnabled,
                    keyboardFocusVisible = Button.Classes.Contains(":focus-visible"),
                },
                chrome = chrome is null ? null : new
                {
                    chrome.IsChecked,
                    chrome.IsSelected,
                    projectedPressed = chrome.IsPressed,
                    rawFrameworkPressed = chrome.GetValue(NativeToolStripButtonChrome.IsPressedProperty),
                    chrome.UseSystemVisualStyle,
                },
                resources,
                controls,
                captureMethod = "actualHeadlessToolbarTemplateFrame",
                noDesktopCapture = true,
                fullParityExitPassed = false,
            }, new JsonSerializerOptions { WriteIndented = true }));
            TestContext.Progress.WriteLine($"GITEXT_MENU_PAINT_EVIDENCE {directory}");
        }
    }
}
