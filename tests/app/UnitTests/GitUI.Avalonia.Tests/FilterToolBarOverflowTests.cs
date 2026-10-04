using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitExtensions.Extensibility.Git;
using GitUI.Compat;
using GitUI.UserControls;
using GitUI.UserControls.RevisionGrid;
using NSubstitute;
using Point = Avalonia.Point;
using Size = Avalonia.Size;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class FilterToolBarOverflowTests
{
    private static readonly string[] SourceItemNames =
    [
        "tsbtnAdvancedFilter",
        "tsbShowReflog",
        "tssbtnShowBranches",
        "toolStripLabel1",
        "tscboBranchFilter",
        "tsddbtnBranchFilter",
        "toolStripSeparator19",
        "tslblRevisionFilter",
        "tstxtRevisionFilter",
        "tsddbtnRevisionFilter",
        "tsmiShowOnlyFirstParent",
    ];

    [AvaloniaTest]
    [TestCase(false, 9)]
    [TestCase(true, 9)]
    [TestCase(false, 11)]
    [TestCase(true, 11)]
    public void Standalone_owner_should_overflow_individual_source_items_and_restore_the_same_controls(bool rightToLeft, int parentPoints)
    {
        FilterToolBar toolbar = NewToolbar(out _);
        NativeToolStrip strip = toolbar.Strip;
        Control[] original = strip.Items.ToArray();
        Window window = NewWindow(toolbar, rightToLeft, 800, parentPoints);
        try
        {
            window.Show();
            Settle(window);
            original.Select(item => item.Name).Should().Equal(SourceItemNames);
            original.Should().OnlyContain(item => NativeToolStrip.GetItemOverflow(item) == NativeToolStripItemOverflow.AsNeeded);
            strip.Bounds.Width.Should().Be(800);
            strip.FontSize.Should().Be(12, "the source ToolStrip menu font does not inherit its Form's configured font");
            strip.PreferredSize.Height.Should().Be(25);
            strip.HasOverflow.Should().BeFalse();
            original.Should().OnlyContain(item => strip.GetCurrentParent(item) == strip);
            original[0].Bounds.X.Should().Be(rightToLeft ? 800 - 5 - 32 : 5);
            toolbar.GetTestAccessor().BranchFilter.Bounds.Size.Should().Be(new Size(100, 23));
            toolbar.GetTestAccessor().RevisionFilter.Bounds.Size.Should().Be(new Size(100, 23));

            toolbar.Width = 339;
            Settle(window);
            strip.Bounds.Width.Should().Be(339);
            strip.HasOverflow.Should().BeTrue();
            if (OperatingSystem.IsWindows())
            {
                strip.OverflowItems.Should().Equal(original.Skip(4), "the running source keeps the first four complete items at339px");
            }

            strip.ShowOverflow();
            Settle(window);
            strip.IsOverflowOpen.Should().BeTrue();
            strip.OverflowItems.Where(item => !NativeToolStrip.GetItemIsSeparator(item))
                .Should().OnlyContain(item => item.GetVisualParent() == strip.OverflowContent);
            strip.OverflowItems.Should().OnlyContain(item => item.Parent == strip && item.IsVisible && item.Opacity == 1 && item.IsHitTestVisible);
            strip.Items.Should().Equal(original);
            strip.CloseOverflow();

            toolbar.Width = 50;
            Settle(window);
            strip.OverflowItems.Should().Equal(original);
            strip.OverflowButton.Bounds.Should().Be(new Rect(rightToLeft ? 0 : 34, 0, 16, 25));
            strip.ShowOverflow();
            Settle(window);
            strip.OverflowContent.Bounds.Width.Should().BeGreaterThan(100);
            original.Where(item => !NativeToolStrip.GetItemIsSeparator(item))
                .Should().OnlyContain(item => item.GetVisualParent() == strip.OverflowContent);
            toolbar.Width = 800;
            Settle(window);
            strip.HasOverflow.Should().BeFalse();
            strip.IsOverflowOpen.Should().BeFalse();
            strip.Items.Should().Equal(original);
            original.Should().OnlyContain(item => strip.GetCurrentParent(item) == strip && item.Parent == strip);
        }
        finally
        {
            window.Close();
            strip.Dispose();
        }
    }

    [AvaloniaTest]
    [TestCase(false, 9)]
    [TestCase(true, 9)]
    [TestCase(false, 11)]
    [TestCase(true, 11)]
    public void Explicit_owner_font_should_change_native_preferred_height_but_not_the_hosted_combo_font(bool rightToLeft, int points)
    {
        FilterToolBar toolbar = NewToolbar(out _);
        NativeToolStrip strip = toolbar.Strip;
        Window window = NewWindow(toolbar, rightToLeft, 800, 11);
        try
        {
            window.Show();
            Settle(window);
            toolbar.FontSize = points * 96d / 72;
            Settle(window);
            IconSplitButton branches = (IconSplitButton)strip.Items[2];
            strip.FontSize.Should().Be(toolbar.FontSize);
            branches.FontSize.Should().Be(toolbar.FontSize);
            double textHeight = Math.Ceiling(WinFormsTextMeasurer.MeasureTextRenderer(branches, "All branches").Height);
            strip.PreferredSize.Height.Should().Be(Math.Max(25, Math.Max(16, textHeight) + 7));
            if (OperatingSystem.IsWindows())
            {
                strip.PreferredSize.Height.Should().Be(points == 9 ? 25 : 27);
            }

            toolbar.GetTestAccessor().BranchFilter.FontSize.Should().Be(12);
            toolbar.GetTestAccessor().RevisionFilter.FontSize.Should().Be(12);
            toolbar.GetTestAccessor().BranchFilter.Bounds.Height.Should().Be(23);
            toolbar.GetTestAccessor().RevisionFilter.Bounds.Height.Should().Be(23);
            toolbar.ClearValue(TemplatedControl.FontSizeProperty);
            Settle(window);
            strip.FontSize.Should().Be(12);
            strip.PreferredSize.Height.Should().Be(25);
        }
        finally
        {
            window.Close();
            strip.Dispose();
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Authored_font_api_should_forward_equal_style_values_and_restore_each_independent_menu_default(bool rightToLeft)
    {
        FilterToolBar toolbar = NewToolbar(out _);
        NativeToolStrip strip = toolbar.Strip;
        FontFamily nativeFamily = strip.FontFamily;
        FontFamily authoredFamily = new("Consolas");
        Window window = NewWindow(toolbar, rightToLeft, 800, 11);
        window.Resources["GitExtensionsUiFontFamily"] = authoredFamily;
        window.Resources["GitExtensionsUiFontStyle"] = FontStyle.Italic;
        window.Resources["GitExtensionsUiFontWeight"] = FontWeight.Bold;
        try
        {
            window.Show();
            Settle(window);
            toolbar.FontFamily.Should().Be(authoredFamily);
            toolbar.FontStyle.Should().Be(FontStyle.Italic);
            toolbar.FontWeight.Should().Be(FontWeight.Bold);
            strip.FontFamily.Should().Be(nativeFamily);
            strip.FontStyle.Should().Be(FontStyle.Normal);
            strip.FontWeight.Should().Be(FontWeight.Normal);
            toolbar.FontFamily = authoredFamily;
            toolbar.FontSize = 11 * 96d / 72;
            toolbar.FontStyle = FontStyle.Italic;
            toolbar.FontWeight = FontWeight.Bold;
            Settle(window);
            strip.FontFamily.Should().Be(authoredFamily);
            strip.FontSize.Should().Be(11 * 96d / 72);
            strip.FontStyle.Should().Be(FontStyle.Italic);
            strip.FontWeight.Should().Be(FontWeight.Bold);
            toolbar.ClearValue(TemplatedControl.FontFamilyProperty);
            toolbar.ClearValue(TemplatedControl.FontSizeProperty);
            toolbar.ClearValue(TemplatedControl.FontStyleProperty);
            toolbar.ClearValue((AvaloniaProperty)TemplatedControl.FontWeightProperty);
            Settle(window);
            toolbar.FontFamily.Should().Be(authoredFamily);
            toolbar.FontSize.Should().Be(11 * 96d / 72);
            toolbar.FontStyle.Should().Be(FontStyle.Italic);
            toolbar.FontWeight.Should().Be(FontWeight.Bold);
            strip.FontFamily.Should().Be(nativeFamily);
            strip.FontSize.Should().Be(12);
            strip.FontStyle.Should().Be(FontStyle.Normal);
            strip.FontWeight.Should().Be(FontWeight.Normal);
        }
        finally
        {
            window.Close();
            strip.Dispose();
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Hosted_revision_editor_should_retain_text_and_input_and_record_selection_reparenting(bool rightToLeft)
    {
        FilterToolBar toolbar = NewToolbar(out IRevisionGridFilter revisionGridFilter);
        NativeToolStrip strip = toolbar.Strip;
        ToolbarComboBox revision = toolbar.GetTestAccessor().RevisionFilter;
        revision.Text = "retained revision";
        Window window = NewWindow(toolbar, rightToLeft, 800, 11);
        try
        {
            window.Show();
            Settle(window);
            TextBox editor = revision.GetVisualDescendants().OfType<TextBox>().Single(control => control.Name == "PART_EditableTextBox");
            editor.Focus();
            editor.SelectAll();
            window.KeyTextInput("typed revision");
            Settle(window);
            revision.Text.Should().Be("typed revision");
            editor.SelectionStart = 2;
            editor.SelectionEnd = 7;
            toolbar.Width = 50;
            Settle(window);
            strip.ShowOverflow();
            Settle(window);
            revision.GetVisualParent().Should().BeSameAs(strip.OverflowContent);
            revision.Text.Should().Be("typed revision");
            revision.Margin.Should().Be(new Thickness(2));
            revision.Bounds.Size.Should().Be(new Size(100, 23));
            TextBox retainedEditor = revision.GetVisualDescendants().OfType<TextBox>().Single(control => control.Name == "PART_EditableTextBox");
            retainedEditor.Should().BeSameAs(editor);
            editor.SelectionStart.Should().Be(2, "Avalonia preserves the caret start while the native overflow probe resets both endpoints to zero");
            editor.SelectionEnd.Should().Be(2, "this records the current framework reparenting result, not native selection-reset equivalence");
            editor.Focus();
            editor.SelectionStart = 2;
            editor.SelectionEnd = 7;
            TopLevel popup = TopLevel.GetTopLevel(editor) ?? throw new InvalidOperationException("The real hosted editor must have a popup input root.");
            popup.KeyTextInput("active");
            Settle(window);
            revision.Text.Should().Be("tyactiveevision");
            revision.RaiseEvent(new KeyEventArgs { RoutedEvent = InputElement.KeyUpEvent, Key = Key.Enter });
            revisionGridFilter.Received(1).SetAndApplyRevisionFilter(Arg.Is<RevisionFilter>(filter => filter.Text == "tyactiveevision"));
            strip.CloseOverflow();
            toolbar.Width = 800;
            Settle(window);
            revision.GetVisualParent().Should().NotBeSameAs(strip.OverflowContent);
            revision.Margin.Should().Be(new Thickness(1, 0, 1, 0));
            revision.Text.Should().Be("tyactiveevision");
            revision.GetVisualDescendants().OfType<TextBox>().Single(control => control.Name == "PART_EditableTextBox").Should().BeSameAs(editor);
        }
        finally
        {
            window.Close();
            strip.Dispose();
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Overflow_split_buttons_should_keep_their_original_flyouts_and_filter_command_routes(bool rightToLeft)
    {
        FilterToolBar toolbar = NewToolbar(out IRevisionGridFilter revisionGridFilter);
        FilterToolBar.TestAccessor accessor = toolbar.GetTestAccessor();
        NativeToolStrip strip = toolbar.Strip;
        IconSplitButton advanced = accessor.AdvancedFilter;
        IconSplitButton branches = (IconSplitButton)strip.Items[2];
        FlyoutBase advancedFlyout = advanced.Flyout ?? throw new InvalidOperationException("The source advanced-filter flyout must exist.");
        FlyoutBase branchesFlyout = branches.Flyout ?? throw new InvalidOperationException("The source branch-mode flyout must exist.");
        Window window = NewWindow(toolbar, rightToLeft, 50, 11);
        try
        {
            window.Show();
            Settle(window);
            strip.ShowOverflow();
            Settle(window);
            Click(advanced, advanced.DropDownButtonBounds.Center);
            Settle(window);
            advancedFlyout.IsOpen.Should().BeTrue();
            strip.IsOverflowOpen.Should().BeTrue();
            accessor.AdvancedFilterMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            revisionGridFilter.Received(1).ShowRevisionFilterDialog();
            advancedFlyout.Hide();
            Settle(window);
            accessor.ResetPathFilters.IsEnabled = true;
            accessor.ResetAllFilters.IsEnabled = true;
            accessor.ResetPathFilters.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            accessor.ResetAllFilters.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            revisionGridFilter.Received(1).SetAndApplyPathFilter(string.Empty);
            revisionGridFilter.Received(1).ResetAllFiltersAndRefresh();

            strip.ShowOverflow();
            Settle(window);
            Click(branches, branches.ButtonBounds.Center);
            Settle(window);
            branchesFlyout.IsOpen.Should().BeTrue();
            ((MenuFlyout)branchesFlyout).Items.OfType<MenuItem>().ElementAt(1).RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            revisionGridFilter.Received(1).ShowCurrentBranchOnly();
            branchesFlyout.Hide();
            Settle(window);
            accessor.ShowOnlyFirstParent.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            accessor.ShowReflog.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            revisionGridFilter.Received(1).ToggleShowOnlyFirstParent();
            revisionGridFilter.Received(1).ToggleShowReflogReferences();
            strip.CloseOverflow();
            toolbar.Width = 800;
            Settle(window);
            advanced.Flyout.Should().BeSameAs(advancedFlyout);
            branches.Flyout.Should().BeSameAs(branchesFlyout);
            strip.Items.Select(item => item.Name).Should().Equal(SourceItemNames);
        }
        finally
        {
            advancedFlyout.Hide();
            branchesFlyout.Hide();
            window.Close();
            strip.Dispose();
        }
    }

    [AvaloniaTest]
    [TestCase(false, false)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(true, true)]
    public void Retained_items_should_follow_the_live_theme_resource_and_RTL_without_replacing_their_owner(bool rightToLeft, bool dark)
    {
        FilterToolBar toolbar = NewToolbar(out _);
        NativeToolStrip strip = toolbar.Strip;
        Control[] original = strip.Items.ToArray();
        Window window = NewWindow(toolbar, rightToLeft, 50, 11);
        window.RequestedThemeVariant = dark ? ThemeVariant.Dark : ThemeVariant.Light;
        window.Resources["GitExtensionsWindowTextBrush"] = Brushes.Magenta;
        try
        {
            window.Show();
            Settle(window);
            strip.ShowOverflow();
            Settle(window);
            toolbar.GetTestAccessor().AdvancedFilter.Foreground.Should().BeSameAs(Brushes.Magenta);
            window.Resources["GitExtensionsWindowTextBrush"] = Brushes.Lime;
            Settle(window);
            toolbar.GetTestAccessor().AdvancedFilter.Foreground.Should().BeSameAs(Brushes.Lime);
            strip.FlowDirection.Should().Be(rightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight);
            strip.Items.Should().Equal(original);
            original.Should().OnlyContain(item => item.Parent == strip);
            NativeToolStrip.GetItemIsSeparator(original[6]).Should().BeTrue();
            strip.GetItemPreferredSize(original[6]).Width.Should().Be(6);
            toolbar.Width = 800;
            Settle(window);
            original[6].Bounds.Width.Should().Be(6);
            original[6].Margin.Should().Be(new Thickness(0));
        }
        finally
        {
            window.Close();
            strip.Dispose();
        }
    }

    private static void Click(NativeToolStripSplitButton control, Point point)
    {
        TopLevel popup = TopLevel.GetTopLevel(control) ?? throw new InvalidOperationException("The actual split item must be attached to its input root.");
        Point input = control.TranslatePoint(point, popup) ?? throw new InvalidOperationException("The original split item must have a real input coordinate.");
        popup.MouseMove(input);
        popup.MouseDown(input, MouseButton.Left);
        popup.MouseUp(input, MouseButton.Left);
    }

    private static FilterToolBar NewToolbar(out IRevisionGridFilter revisionGridFilter)
    {
        FilterToolBar toolbar = new() { VerticalAlignment = VerticalAlignment.Top };
        IGitModule module = Substitute.For<IGitModule>();
        module.IsValidGitWorkingDir().Returns(true);
        revisionGridFilter = Substitute.For<IRevisionGridFilter>();
        toolbar.Bind(() => module, revisionGridFilter);
        toolbar.RefreshRevisionFunction(_ => []);
        return toolbar;
    }

    private static Window NewWindow(FilterToolBar toolbar, bool rightToLeft, int width, int parentPoints)
    {
        toolbar.Width = width;
        return new Window
        {
            Width = 1000,
            Height = 240,
            FontSize = parentPoints * 96d / 72,
            FlowDirection = rightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
            Resources = { ["GitExtensionsUiFontSize"] = parentPoints * 96d / 72 },
            Content = new Canvas { Children = { toolbar } },
        };
    }

    private static void Settle(Window window)
    {
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
    }
}
