using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitExtensions.ParityCapture;
using GitUI.Compat;
using AvaloniaEllipse = Avalonia.Controls.Shapes.Ellipse;
using AvaloniaPath = Avalonia.Controls.Shapes.Path;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
[Category("P1_7")]
public sealed class FrameworkControlParityTests
{
    [AvaloniaTest]
    public void CheckBox_and_RadioButton_should_use_compact_WinForms_glyphs_and_state_colors()
    {
        foreach (ThemeVariant themeVariant in new[] { ThemeVariant.Light, ThemeVariant.Dark })
        {
            CheckBox checkBox = new()
            {
                Content = "Check",
                IsThreeState = true,
            };
            RadioButton radioButton = new()
            {
                Content = "Radio",
            };
            Window window = Show(themeVariant, new StackPanel { Children = { checkBox, radioButton } });
            try
            {
                Border checkIndicator = Find<Border>(checkBox, "PART_Indicator");
                AvaloniaPath checkGlyph = Find<AvaloniaPath>(checkBox, "PART_CheckGlyph");
                Border indeterminateGlyph = Find<Border>(checkBox, "PART_IndeterminateGlyph");
                ContentPresenter checkContent = checkBox.GetVisualDescendants().OfType<ContentPresenter>().Single();
                Border radioIndicator = Find<Border>(radioButton, "PART_Indicator");
                AvaloniaEllipse radioGlyph = Find<AvaloniaEllipse>(radioButton, "PART_CheckedGlyph");

                checkBox.Bounds.Height.Should().Be(19);
                radioButton.Bounds.Height.Should().Be(19);
                checkIndicator.Bounds.Size.Should().Be(new Size(13, 13));
                radioIndicator.Bounds.Size.Should().Be(new Size(13, 13));
                checkContent.Margin.Should().Be(new Thickness(3, 0, 0, 0));
                radioIndicator.CornerRadius.Should().Be(new CornerRadius(7));
                checkGlyph.IsVisible.Should().BeFalse();
                indeterminateGlyph.IsVisible.Should().BeFalse();
                radioGlyph.IsVisible.Should().BeFalse();

                checkBox.IsChecked = true;
                radioButton.IsChecked = true;
                Dispatcher.UIThread.RunJobs();
                checkGlyph.IsVisible.Should().BeTrue();
                radioGlyph.IsVisible.Should().BeTrue();
                GetColor(checkIndicator.Background).Should().Be(GetResourceColor("GitExtensionsHighlightBackgroundBrush", themeVariant));
                GetColor(radioIndicator.Background).Should().Be(GetResourceColor("GitExtensionsHighlightBackgroundBrush", themeVariant));

                checkBox.IsChecked = null;
                Dispatcher.UIThread.RunJobs();
                checkGlyph.IsVisible.Should().BeFalse();
                indeterminateGlyph.IsVisible.Should().BeTrue();

                checkBox.IsEnabled = false;
                radioButton.IsEnabled = false;
                Dispatcher.UIThread.RunJobs();
                GetColor(checkBox.Foreground).Should().Be(GetResourceColor("GitExtensionsDisabledForegroundBrush", themeVariant));
                GetColor(radioButton.Foreground).Should().Be(GetResourceColor("GitExtensionsDisabledForegroundBrush", themeVariant));
            }
            finally
            {
                window.Close();
            }
        }
    }

    [AvaloniaTest]
    public void Button_should_resolve_normal_hover_pressed_default_focus_and_disabled_states()
    {
        Button button = new()
        {
            Name = "btnState",
            Content = "Action",
            Width = 100,
            Height = 30,
            IsDefault = true,
        };
        Window window = Show(ThemeVariant.Light, button);
        try
        {
            GetColor(button.Background).Should().Be(GetResourceColor("GitExtensionsControlBackgroundBrush", ThemeVariant.Light));
            button.BorderThickness.Should().Be(new Thickness(2));
            button.FocusAdorner.Should().NotBeNull();

            using (AvaloniaControlStateDriver.Apply(
                       window,
                       new CaptureStatePlan { Id = "hover", Kind = CaptureStateKind.Hover, TargetField = "btnState" }))
            {
                GetColor(button.Background).Should().Be(GetResourceColor("GitExtensionsControlPointerOverBackgroundBrush", ThemeVariant.Light));
            }

            using (AvaloniaControlStateDriver.Apply(
                       window,
                       new CaptureStatePlan { Id = "pressed", Kind = CaptureStateKind.Pressed, TargetField = "btnState" }))
            {
                GetColor(button.Background).Should().Be(GetResourceColor("GitExtensionsControlPressedBackgroundBrush", ThemeVariant.Light));
            }

            button.IsEnabled = false;
            Dispatcher.UIThread.RunJobs();
            GetColor(button.Foreground).Should().Be(GetResourceColor("GitExtensionsDisabledForegroundBrush", ThemeVariant.Light));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Pressed_state_cleanup_should_release_outside_without_invoking_the_button()
    {
        int clickCount = 0;
        Button button = new()
        {
            Name = "btnClose",
            Content = "Close",
        };
        Window window = Show(ThemeVariant.Light, button);
        try
        {
            button.Click += (_, _) => clickCount++;
            using AvaloniaControlStateDriver driver = AvaloniaControlStateDriver.Apply(
                window,
                new CaptureStatePlan { Id = "pressed", Kind = CaptureStateKind.Pressed, TargetField = "btnClose" });

            clickCount.Should().Be(0);
            window.IsVisible.Should().BeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Scrollbars_splitters_and_tooltips_should_use_desktop_chrome_metrics()
    {
        ScrollBar vertical = new() { Orientation = Avalonia.Layout.Orientation.Vertical };
        ScrollBar horizontal = new() { Orientation = Avalonia.Layout.Orientation.Horizontal };
        GridSplitter splitter = new();
        Button target = new() { Content = "Hover" };
        ToolTip toolTip = new() { Content = "Line one\nLine two" };
        Window window = Show(
            ThemeVariant.Light,
            new StackPanel { Children = { vertical, horizontal, splitter, target, toolTip } });
        try
        {
            vertical.Width.Should().Be(17);
            horizontal.Height.Should().Be(17);
            vertical.AllowAutoHide.Should().BeFalse();
            horizontal.AllowAutoHide.Should().BeFalse();
            GetColor(splitter.Background).Should().Be(Colors.Transparent);
            target.GetValue(ToolTip.ShowDelayProperty).Should().Be(500);
            target.GetValue(ToolTip.BetweenShowDelayProperty).Should().Be(100);
            target.GetValue(ToolTip.PlacementProperty).Should().Be(PlacementMode.Pointer);
            toolTip.MaxWidth.Should().Be(500);
            toolTip.Padding.Should().Be(new Thickness(4, 2));
            toolTip.CornerRadius.Should().Be(new CornerRadius(0));
            GetColor(toolTip.Background).Should().Be(GetResourceColor("GitExtensionsToolTipBackgroundBrush", ThemeVariant.Light));
            GetColor(toolTip.Foreground).Should().Be(GetResourceColor("GitExtensionsToolTipForegroundBrush", ThemeVariant.Light));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Capture_reader_should_emit_framework_neutral_resolved_color_roles()
    {
        Window window = Show(ThemeVariant.Light, new TextBlock { Text = "capture" });
        try
        {
            CaptureSurface surface = new AvaloniaControlTreeReader(window, renderScale: 1)
                .ReadPrimary(window, new PixelSize(300, 200));
            IReadOnlyDictionary<string, string> roles = surface.Root.Colors.Additional
                .Where(pair => pair.Key.StartsWith("semantic.", StringComparison.Ordinal))
                .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);

            roles.Should().HaveCount(18);
            roles.Keys.Should().Contain(
                "semantic.app.panel.background",
                "semantic.app.selection.background",
                "semantic.system.control.background",
                "semantic.system.highlight.background",
                "semantic.system.inactiveSelection.background",
                "semantic.system.tooltip.background",
                "semantic.app.reset.hard.background");
            roles.Values.Should().OnlyContain(color => System.Text.RegularExpressions.Regex.IsMatch(color, "^#[0-9A-F]{8}$"));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8_6h")]
    public void Capture_reader_should_keep_absolute_screen_bounds_and_emit_owner_relative_popup_bounds()
    {
        ContextMenu popup = new();
        Window window = Show(ThemeVariant.Light, new TextBlock { Text = "owner" });
        try
        {
            CaptureSurface surface = new AvaloniaControlTreeReader(
                window,
                renderScale: 1.25,
                primaryScreenOrigin: new PixelPoint(100, 50))
                .ReadSurface(popup, "popup:0", new PixelRect(140, 80, 100, 50));

            surface.ScreenBoundsPx.X.Should().Be(140);
            surface.ScreenBoundsPx.Y.Should().Be(80);
            surface.Root.BoundsPx.X.Should().Be(40);
            surface.Root.BoundsPx.Y.Should().Be(30);
            surface.Root.BoundsDip.X.Should().Be(32);
            surface.Root.BoundsDip.Y.Should().Be(24);
        }
        finally
        {
            window.Close();
        }
    }

    [Test]
    public void TaskDialog_model_should_preserve_command_descriptions_and_expanders()
    {
        TaskDialogCommandLinkButton command = new("Report", "Open the issue form");
        TaskDialogCommandLinkButton persistentCommand = new("Open folder", allowCloseDialog: false);
        TaskDialogPage page = new()
        {
            Buttons = { command },
            Expander = new TaskDialogExpander
            {
                CollapsedButtonText = "See details",
                ExpandedButtonText = "Hide details",
                Position = TaskDialogExpanderPosition.AfterFootnote,
                Text = "details",
            },
        };

        command.DescriptionText.Should().Be("Open the issue form");
        persistentCommand.AllowCloseDialog.Should().BeFalse();
        page.Expander.Should().NotBeNull();
        page.Expander!.ExpandedButtonText.Should().Be("Hide details");
        page.Expander.Text.Should().Be("details");
    }

    [Test]
    public void Bug_report_boundary_should_build_the_native_GitHub_issue_payload()
    {
        InvalidOperationException exception = new("failure details");

        string url = AvaloniaBugReportLauncher.BuildIssueUrl(exception, "Command: git", "OS: test");

        url.Should().StartWith("https://github.com/gitextensions/gitextensions/issues/new?");
        url.Should().Contain("template=bug_report.yml");
        url.Should().Contain(Uri.EscapeDataString("[NBug] failure details"));
        url.Should().Contain(Uri.EscapeDataString("Command: git"));
        url.Should().Contain(Uri.EscapeDataString("OS: test"));
    }

    [Test]
    public void Message_box_should_preserve_button_order_default_and_close_results()
    {
        AvaloniaMessageBoxHost.TestAccessor.GetChoices(WinFormsShims.MessageBoxButtons.AbortRetryIgnore)
            .Should().Equal(
                WinFormsShims.DialogResult.Abort,
                WinFormsShims.DialogResult.Retry,
                WinFormsShims.DialogResult.Ignore);
        AvaloniaMessageBoxHost.TestAccessor.GetChoices(WinFormsShims.MessageBoxButtons.YesNoCancel)
            .Should().Equal(
                WinFormsShims.DialogResult.Yes,
                WinFormsShims.DialogResult.No,
                WinFormsShims.DialogResult.Cancel);
        AvaloniaMessageBoxHost.TestAccessor.GetDefaultIndex(WinFormsShims.MessageBoxDefaultButton.Button3, buttonCount: 3)
            .Should().Be(2);
        AvaloniaMessageBoxHost.TestAccessor.GetCancelResult(WinFormsShims.MessageBoxButtons.YesNo)
            .Should().Be(WinFormsShims.DialogResult.No);
        AvaloniaMessageBoxHost.TestAccessor.GetCancelResult(WinFormsShims.MessageBoxButtons.OKCancel)
            .Should().Be(WinFormsShims.DialogResult.Cancel);
    }

    private static T Find<T>(Control root, string name)
        where T : Control
        => root.GetVisualDescendants().OfType<T>().Single(control => control.Name == name);

    private static Color GetColor(IBrush? brush)
        => brush.Should().BeAssignableTo<ISolidColorBrush>().Which.Color;

    private static Color GetResourceColor(string key, ThemeVariant themeVariant)
    {
        Application application = Application.Current
            ?? throw new InvalidOperationException("The Avalonia application was not created.");
        application.TryGetResource(key, themeVariant, out object? resource).Should().BeTrue();
        return resource.Should().BeOfType<SolidColorBrush>().Which.Color;
    }

    private static Window Show(ThemeVariant themeVariant, Control content)
    {
        Window window = new()
        {
            Width = 400,
            Height = 300,
            RequestedThemeVariant = themeVariant,
            Content = content,
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }
}
