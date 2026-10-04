using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.CommandsDialogs.Menus;
using GitUI.Compat;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using Size = Avalonia.Size;
using Point = Avalonia.Point;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class NativeToolStripOverflowTests
{
    [SetUp]
    public void SetUp() => ThreadHelper.JoinableTaskContext = new JoinableTaskContext();

    [AvaloniaTest]
    [TestCase(false, 25)]
    [TestCase(true, 25)]
    [TestCase(false, 48)]
    [TestCase(true, 48)]
    public void Main_owner_should_reserve_the_disabled_visible_grip_and_stretch_only_autosized_items(bool rightToLeft, int height)
    {
        using NativeToolStrip strip = NewStrip(rightToLeft);
        SourceSizedControl refresh = NewItem("RefreshButton", 23, 20);
        SourceSizedControl separator = NewItem("toolStripSeparator0", 6, 23, margin: new Thickness(0));
        SourceSizedControl split = NewItem("menuCommitInfoPosition", 32, 20);
        SourceSizedControl fixedItem = NewItem("sourceFixedHeightConsumer", 23, 20);
        NativeToolStrip.SetItemAutoSize(fixedItem, false);
        strip.Items.AddRange([refresh, separator, split, fixedItem]);
        strip.Height = height;
        Window window = NewWindow(strip, 400);
        try
        {
            window.Show();
            Settle(window);
            strip.PreferredSize.Should().Be(new Size(91, 25));
            strip.HasOverflow.Should().BeFalse();
            refresh.Bounds.Should().Be(new Rect(rightToLeft ? 372 : 5, 1, 23, height - 3));
            separator.Bounds.Should().Be(new Rect(rightToLeft ? 366 : 28, 0, 6, height));
            split.Bounds.Should().Be(new Rect(rightToLeft ? 334 : 34, 1, 32, height - 3));
            fixedItem.Bounds.Should().Be(new Rect(rightToLeft ? 311 : 66, (height - 22) / 2, 23, 22));
            strip.Items.Should().OnlyContain(item => item.Parent == strip);
            RecordEvidence(window, strip, "grip-and-item-height");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(false, 101, 3)]
    [TestCase(true, 101, 3)]
    [TestCase(false, 100, 2)]
    [TestCase(true, 100, 2)]
    [TestCase(false, 85, 1)]
    [TestCase(true, 85, 1)]
    [TestCase(false, 80, 1)]
    [TestCase(true, 80, 1)]
    public void AsNeeded_should_walk_backwards_with_the_source_strict_recovered_space_boundary(bool rightToLeft, int width, int mainCount)
    {
        using NativeToolStrip strip = NewStrip(rightToLeft);
        SourceSizedControl[] items = [NewItem("first", 32, 20), NewItem("second", 32, 20), NewItem("third", 32, 20)];
        strip.Items.AddRange(items);
        Window window = NewWindow(strip, width);
        try
        {
            window.Show();
            Settle(window);
            strip.Items.Should().Equal(items);
            strip.Items.Count(item => strip.GetItemPlacement(item) == NativeToolStripItemPlacement.Main).Should().Be(mainCount);
            strip.OverflowItems.Should().Equal(items.Skip(mainCount));
            if (mainCount < items.Length)
            {
                strip.OverflowButton.Bounds.Should().Be(new Rect(rightToLeft ? 0 : width - 16, 0, 16, 25));
                strip.OverflowItems.Should().OnlyContain(item => item.IsVisible && item.Opacity == 1 && item.IsHitTestVisible);
                strip.OverflowItems.Should().OnlyContain(item => item.GetVisualParent() == null);
            }

            // At96px display width all three items fit. At80px one moved32px item
            // exactly recovers the missing space plus overflow, so the strict source
            // boundary moves another item instead of treating equality as sufficient.
            strip.Width = 200;
            Settle(window);
            strip.HasOverflow.Should().BeFalse();
            strip.Items.Should().Equal(items);
            strip.Items.Should().OnlyContain(item => strip.GetCurrentParent(item) == strip);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(false, 9)]
    [TestCase(true, 9)]
    [TestCase(false, 22)]
    [TestCase(true, 22)]
    public void Overflow_should_use_source_two_pass_wrapping_and_keep_oversized_items_unsqueezed(bool rightToLeft, int points)
    {
        using NativeToolStrip strip = NewStrip(rightToLeft);
        int lineHeight = points == 9 ? 20 : 45;
        int workingWidth = points == 9 ? 435 : 1041;
        int branchWidth = points == 9 ? 76 : 140;
        int commitWidth = points == 9 ? 94 : 202;
        SourceSizedControl refresh = NewItem("RefreshButton", 23, 20);
        SourceSizedControl separator = NewItem("toolStripSeparator0", 6, 23, margin: new Thickness(0));
        SourceSizedControl layout = NewItem("menuCommitInfoPosition", 32, 20);
        SourceSizedControl levelUp = NewItem("toolStripButtonLevelUp", 32, 20);
        SourceSizedControl working = NewItem("_NO_TRANSLATE_WorkingDir", workingWidth, lineHeight);
        SourceSizedControl branch = NewItem("branchSelect", branchWidth, lineHeight);
        SourceSizedControl commit = NewItem("toolStripButtonCommit", commitWidth, lineHeight);
        SourceSizedControl settings = NewItem("EditSettings", 23, 20);
        SourceSizedControl fixedItem = NewItem("sourceFixedHeightConsumer", 23, 20);
        NativeToolStrip.SetItemAutoSize(fixedItem, false);
        Control[] original = [refresh, separator, layout, levelUp, working, branch, commit, settings, fixedItem];
        strip.Items.AddRange(original);
        Window window = NewWindow(strip, 180);
        try
        {
            window.Show();
            Settle(window);
            strip.OverflowItems.Should().Equal(working, branch, commit, settings, fixedItem);
            strip.ShowOverflow();
            Settle(window);
            strip.IsOverflowOpen.Should().BeTrue();
            strip.Items.Should().Equal(original);
            strip.OverflowContent.Bounds.Size.Should().Be(new Size(workingWidth + 2, points == 9 ? 52 : 100));
            working.Bounds.Should().Be(new Rect(rightToLeft ? 0 : 1, 3, workingWidth, lineHeight));
            int rowY = lineHeight + 6;
            int branchX = rightToLeft ? workingWidth - branchWidth : 1;
            branch.Bounds.Should().Be(new Rect(branchX, rowY, branchWidth, lineHeight));
            int commitX = rightToLeft ? workingWidth - branchWidth - commitWidth : branchWidth + 1;
            commit.Bounds.Should().Be(new Rect(commitX, rowY, commitWidth, lineHeight));
            fixedItem.Bounds.Height.Should().Be(22);
            strip.OverflowItems.Should().OnlyContain(item => item.Parent == strip && item.GetVisualParent() == strip.OverflowContent);
            working.MaxWidth.Should().Be(double.PositiveInfinity);
            RecordEvidence(window, strip, "wrapped-native-preferred-items");
            strip.CloseOverflow();
            Settle(window);
            strip.Items.Should().Equal(original);
            strip.Width = strip.PreferredSize.Width + 20;
            Settle(window);
            strip.HasOverflow.Should().BeFalse();
            strip.Items.Should().OnlyContain(item => strip.GetItemPlacement(item) == NativeToolStripItemPlacement.Main);
            strip.Items.Should().OnlyContain(item => item.Parent == strip && item.GetVisualParent() != null);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(false, MouseButton.Left)]
    [TestCase(true, MouseButton.Left)]
    [TestCase(false, MouseButton.Right)]
    [TestCase(true, MouseButton.Right)]
    [TestCase(false, MouseButton.Middle)]
    [TestCase(true, MouseButton.Middle)]
    public void Overflow_pointer_should_open_on_left_down_keep_first_release_and_close_the_next_down(bool rightToLeft, MouseButton button)
    {
        using NativeToolStrip strip = NewStrip(rightToLeft);
        Button command = new() { Name = "btnOriginalCommand", Width = 140, Content = "same command", Margin = new Thickness(0, 1, 0, 2) };
        ComboBox combo = new() { Name = "cbxRetainedFilter", Width = 100, ItemsSource = new[] { "retained filter" }, SelectedIndex = 0 };
        Button overflowCommand = new() { Name = "btnOverflowCommand", Width = 90, Content = "overflow command", Margin = new Thickness(0, 1, 0, 2) };
        Control[] original = [command, combo, overflowCommand];
        int commands = 0;
        command.Click += (_, _) => commands++;
        overflowCommand.Click += (_, _) => commands++;
        strip.Items.AddRange(original);
        Window window = NewWindow(strip, 180);
        try
        {
            window.Show();
            Settle(window);
            strip.HasOverflow.Should().BeTrue();
            Point point = strip.OverflowButton.TranslatePoint(new Rect(strip.OverflowButton.Bounds.Size).Center, window)
                ?? throw new InvalidOperationException("The actual chevron must be connected to its source owner window.");
            window.MouseMove(point);
            window.MouseDown(point, button);
            Settle(window);
            bool expectedOpen = button == MouseButton.Left;
            strip.IsOverflowOpen.Should().Be(expectedOpen, "native ToolStripOverflowButton opens on left mouse-down, not release");
            if (expectedOpen)
            {
                strip.Items.Should().Equal(original);
                combo.Parent.Should().BeSameAs(strip);
                combo.GetVisualParent().Should().BeSameAs(strip.OverflowContent);
                combo.SelectedItem.Should().Be("retained filter");
                RecordEvidence(window, strip, "source-left-down");
            }

            window.MouseUp(point, button);
            Settle(window);
            strip.IsOverflowOpen.Should().Be(expectedOpen, "the first release must not toggle the opening mouse-down again");
            commands.Should().Be(0);
            if (expectedOpen)
            {
                window.MouseDown(point, button);
                Settle(window);
                strip.IsOverflowOpen.Should().BeFalse("the same chevron remains a real hit target underneath its popup overlay");
                window.MouseUp(point, button);
                Settle(window);
                strip.IsOverflowOpen.Should().BeFalse("the second release must not reopen the already-closed dropdown");
            }

            strip.Items.Should().Equal(original);
            original.Should().OnlyContain(item => item.Parent == strip);
            commands.Should().Be(0);
            RecordEvidence(window, strip, $"source-overflow-input-{button}");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Overflow_Escape_should_close_after_hosted_combo_focus_without_replacing_or_resetting_the_control(bool rightToLeft)
    {
        using NativeToolStrip strip = NewStrip(rightToLeft);
        ComboBox combo = new() { Name = "cbxRetainedFilter", Width = 100, ItemsSource = new[] { "retained filter", "other filter" }, SelectedIndex = 0 };
        strip.Items.Add(combo);
        Window window = NewWindow(strip, 50);
        try
        {
            window.Show();
            Settle(window);
            Point point = strip.OverflowButton.TranslatePoint(new Rect(strip.OverflowButton.Bounds.Size).Center, window)
                ?? throw new InvalidOperationException("The actual chevron must have a window input coordinate.");
            window.MouseDown(point, MouseButton.Left);
            window.MouseUp(point, MouseButton.Left);
            Settle(window);
            strip.IsOverflowOpen.Should().BeTrue();
            strip.Items.Should().Equal(combo);
            combo.Parent.Should().BeSameAs(strip);
            combo.GetVisualParent().Should().BeSameAs(strip.OverflowContent);
            combo.Focus().Should().BeTrue();
            combo.IsFocused.Should().BeTrue();
            TopLevel popup = TopLevel.GetTopLevel(combo)
                ?? throw new InvalidOperationException("The real hosted control must have an input root in its open popup.");
            RecordEvidence(window, strip, "source-hosted-combo-before-escape");
            popup.KeyPress(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, null);
            Dispatcher.UIThread.RunJobs();
            strip.IsOverflowOpen.Should().BeFalse();
            strip.Items.Should().Equal(combo);
            combo.Parent.Should().BeSameAs(strip);
            combo.GetVisualParent().Should().BeNull();
            combo.SelectedItem.Should().Be("retained filter");
            combo.SelectedIndex.Should().Be(0);
            strip.ShowOverflow();
            Settle(window);
            combo.GetVisualParent().Should().BeSameAs(strip.OverflowContent);
            combo.SelectedItem.Should().Be("retained filter");
            RecordEvidence(window, strip, "source-hosted-combo-reopened");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Overflow_keyboard_Enter_should_keep_the_existing_named_button_Click_route_without_double_toggle(bool rightToLeft)
    {
        using NativeToolStrip strip = NewStrip(rightToLeft);
        strip.Items.Add(NewItem("overflowCommand", 100, 20));
        Button sameChevron = strip.OverflowButton;
        int clicks = 0;
        sameChevron.Click += (_, _) => clicks++;
        Window window = NewWindow(strip, 50);
        try
        {
            window.Show();
            Settle(window);
            sameChevron.Focus().Should().BeTrue();
            window.KeyPress(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, null);
            Settle(window);
            clicks.Should().Be(1);
            strip.IsOverflowOpen.Should().BeTrue();
            strip.OverflowButton.Should().BeSameAs(sameChevron);
            strip.CloseOverflow();
            Settle(window);
            sameChevron.Focus().Should().BeTrue();
            window.KeyPress(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, null);
            Settle(window);
            clicks.Should().Be(2);
            strip.IsOverflowOpen.Should().BeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Overflow_named_button_replacement_and_disposal_should_release_the_temporary_press_scope()
    {
        using NativeToolStrip strip = NewStrip(false);
        Button original = new() { Name = "originalChevron" };
        Button replacement = new() { Name = "replacementChevron" };
        original.ClickMode.Should().Be(ClickMode.Release);
        replacement.ClickMode.Should().Be(ClickMode.Release);
        strip.OverflowButton = original;
        original.ClickMode.Should().Be(ClickMode.Press);
        strip.OverflowButton = replacement;
        original.Parent.Should().BeNull();
        original.ClickMode.Should().Be(ClickMode.Release);
        replacement.ClickMode.Should().Be(ClickMode.Press);
        strip.Dispose();
        replacement.Parent.Should().BeNull();
        replacement.ClickMode.Should().Be(ClickMode.Release);
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Overflow_commands_should_preserve_the_original_click_handler_content_and_flyout(bool rightToLeft)
    {
        using NativeToolStrip strip = NewStrip(rightToLeft);
        SourceSizedControl first = NewItem("first", 40, 20);
        Button command = new() { Name = "command", Width = 40, Content = "same command", Margin = new Thickness(0, 1, 0, 2) };
        IconSplitButton split = new()
        {
            Name = "split",
            UseNativeToolStripLayout = true,
            Width = 76,
            Content = "Branch",
            Margin = new Thickness(0, 1, 0, 2),
            Flyout = new MenuFlyout { Items = { new MenuItem { Header = "same menu" } } },
        };
        FlyoutBase originalFlyout = split.Flyout
            ?? throw new InvalidOperationException("The source split command must retain its assigned drop-down.");
        int clicks = 0;
        command.Click += (_, _) => clicks++;
        strip.Items.AddRange([first, command, split]);
        Window window = NewWindow(strip, 70);
        try
        {
            window.Show();
            Settle(window);
            strip.ShowOverflow();
            Settle(window);
            command.GetVisualParent().Should().BeSameAs(strip.OverflowContent);
            command.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            clicks.Should().Be(1);
            strip.IsOverflowOpen.Should().BeFalse();
            command.Content.Should().Be("same command");
            strip.ShowOverflow();
            Settle(window);
            split.ShowDropDown();
            Settle(window);
            split.Flyout.Should().BeSameAs(originalFlyout);
            split.Flyout.IsOpen.Should().BeTrue();
            split.RaiseEvent(new RoutedEventArgs(SplitButton.ClickEvent));
            strip.IsOverflowOpen.Should().BeTrue("an open split drop-down does not dismiss its overflow parent");
            split.Flyout.Hide();
            split.RaiseEvent(new RoutedEventArgs(SplitButton.ClickEvent));
            strip.IsOverflowOpen.Should().BeFalse("the actual split-primary command dismisses overflow once its own menu is closed");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Dynamic_items_should_keep_source_order_and_release_removed_owners_and_layout_scopes(bool rightToLeft)
    {
        using NativeToolStrip strip = NewStrip(rightToLeft);
        SourceSizedControl first = NewItem("first", 40, 20);
        SourceSizedControl second = NewItem("second", 40, 20);
        SourceSizedControl added = NewItem("added", 40, 20);
        first.MinHeight = 7;
        strip.Items.AddRange([first, second]);
        Window window = NewWindow(strip, 50);
        try
        {
            window.Show();
            Settle(window);
            strip.ShowOverflow();
            Settle(window);
            strip.Items.Insert(1, added);
            strip.Items.Move(2, 0);
            Settle(window);
            strip.Items.Should().Equal(second, first, added);
            strip.OverflowItems.Should().Equal(second, first, added);
            strip.OverflowContent.GetVisualChildren().Should().Equal(second, first, added);
            strip.Items.Remove(first);
            Settle(window);
            first.Parent.Should().BeNull();
            first.GetVisualParent().Should().BeNull();
            first.MinHeight.Should().Be(7, "the explicit local value survives the owner's temporary source autosize scope");
            strip.Items.Clear();
            Settle(window);
            second.Parent.Should().BeNull();
            added.Parent.Should().BeNull();
            second.GetVisualParent().Should().BeNull();
            added.GetVisualParent().Should().BeNull();
            strip.IsOverflowOpen.Should().BeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(false, false)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(true, true)]
    public void Actual_split_primary_input_should_dispatch_the_same_overflow_command_once(bool rightToLeft, bool workingDirectory)
    {
        using NativeToolStrip strip = NewStrip(rightToLeft);
        IconSplitButton command = workingDirectory ? new WorkingDirectoryToolStripSplitButton() : new IconSplitButton();
        command.UseNativeToolStripLayout = true;
        command.Name = workingDirectory ? "_NO_TRANSLATE_WorkingDir" : "branchSelect";
        command.Content = workingDirectory ? "WorkingDir" : "Branch";
        command.Width = 100;
        command.Margin = new Thickness(0, 1, 0, 2);
        if (command is WorkingDirectoryToolStripSplitButton selector)
        {
            // Seed the actual original menu without loading real repository history.
            selector.GetTestAccessor().PrepareDropDown([], []);
        }
        else
        {
            command.Flyout = new MenuFlyout { Items = { new MenuItem { Header = "same source menu" } } };
        }

        FlyoutBase sourceFlyout = command.Flyout
            ?? throw new InvalidOperationException("The command must retain its original menu.");
        int clicks = 0;
        command.Click += (_, _) => clicks++;
        strip.Items.Add(command);
        Window window = NewWindow(strip, 50);
        try
        {
            window.Show();
            Settle(window);
            strip.ShowOverflow();
            Settle(window);
            TopLevel popup = TopLevel.GetTopLevel(command)
                ?? throw new InvalidOperationException("The actual command must be connected to the open popup.");
            Point point = command.TranslatePoint(command.ButtonBounds.Center, popup)
                ?? throw new InvalidOperationException("The actual native-shaped primary region must have a popup input coordinate.");
            popup.MouseMove(point);
            popup.MouseDown(point, MouseButton.Left);
            popup.MouseUp(point, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            clicks.Should().Be(1);
            strip.IsOverflowOpen.Should().Be(workingDirectory, "the source WorkingDir primary opens its original menu, while a plain primary command dismisses overflow");
            sourceFlyout.IsOpen.Should().Be(workingDirectory);
            command.Flyout.Should().BeSameAs(sourceFlyout);
            strip.Items.Should().Equal(command);
            command.Parent.Should().BeSameAs(strip);
            sourceFlyout.Hide();
            strip.CloseOverflow();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(9)]
    [TestCase(11)]
    [TestCase(18)]
    [TestCase(22)]
    public void Unassigned_owner_should_keep_native_menu_font_independently_of_the_form_font(int formPoints)
    {
        using NativeToolStrip strip = NewStrip(false);
        IconSplitButton split = new()
        {
            UseNativeToolStripLayout = true,
            Content = "Branch",
            Margin = new Thickness(0, 1, 0, 2),
            Icon = new DrawingImage { Drawing = new GeometryDrawing { Brush = Brushes.Magenta, Geometry = new RectangleGeometry(new Rect(0, 0, 16, 16)) } },
        };
        strip.Items.Add(split);
        Window window = NewWindow(strip, 400);
        window.FontSize = formPoints * 96d / 72;
        try
        {
            window.Show();
            Settle(window);
            strip.FontSize.Should().Be(12);
            split.FontSize.Should().Be(12);
            strip.PreferredSize.Height.Should().Be(25);
            split.Bounds.Height.Should().Be(22);
            strip.FontSize = 22 * 96d / 72;
            Settle(window);
            split.FontSize.Should().Be(strip.FontSize);
            Size actualCaption = WinFormsTextMeasurer.MeasureTextRenderer(split, "Branch");
            strip.PreferredSize.Height.Should().Be(Math.Max(25, Math.Max(16, Math.Ceiling(actualCaption.Height)) + 7));
            if (OperatingSystem.IsWindows())
            {
                strip.PreferredSize.Height.Should().Be(48, "the genuine explicitly assigned native owner probe returns48 at22pt");
            }

            split.FontSize = 12;
            strip.FontSize = 24;
            Settle(window);
            split.FontSize.Should().Be(12, "a deliberately assigned item font has source precedence over its owner's ambient font");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Always_and_Never_overflow_policies_should_preserve_source_participation_without_changing_visibility(bool rightToLeft)
    {
        using NativeToolStrip strip = NewStrip(rightToLeft);
        SourceSizedControl hidden = NewItem("hidden", 400, 20);
        hidden.IsVisible = false;
        SourceSizedControl never = NewItem("never", 32, 20);
        NativeToolStrip.SetItemOverflow(never, NativeToolStripItemOverflow.Never);
        SourceSizedControl always = NewItem("always", 32, 20);
        NativeToolStrip.SetItemOverflow(always, NativeToolStripItemOverflow.Always);
        strip.Items.AddRange([hidden, never, always]);
        Window window = NewWindow(strip, 100);
        try
        {
            window.Show();
            Settle(window);
            strip.PreferredSize.Should().Be(new Size(53, 25));
            strip.GetItemPlacement(hidden).Should().Be(NativeToolStripItemPlacement.None);
            strip.GetItemPlacement(never).Should().Be(NativeToolStripItemPlacement.Main);
            strip.OverflowItems.Should().Equal(always);
            always.IsVisible.Should().BeTrue();
            always.Opacity.Should().Be(1);
            strip.ShowOverflow();
            Settle(window);
            always.Bounds.Size.Should().Be(new Size(32, 20));
            strip.CloseOverflow();
            hidden.IsVisible = true;
            Settle(window);
            strip.Items.Should().Equal(hidden, never, always);
            hidden.Opacity.Should().Be(1);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(9)]
    [TestCase(11)]
    [TestCase(18)]
    [TestCase(22)]
    public void Source_container_should_grow_the_actual_main_row_only_for_an_explicit_owner_font(int points)
    {
        using NativeToolStrip strip = NewStrip(false);
        strip.Name = "ToolStripMain";
        strip.Items.Add(new IconSplitButton
        {
            UseNativeToolStripLayout = true,
            Content = "Branch",
            Margin = new Thickness(0, 1, 0, 2),
            Icon = new DrawingImage { Drawing = new GeometryDrawing { Brush = Brushes.Magenta, Geometry = new RectangleGeometry(new Rect(0, 0, 16, 16)) } },
        });
        Grid host = new() { Name = "toolStripMainHost", Children = { strip } };
        GitUI.Compat.WinFormsControls.ToolStripPanel top = new()
        {
            Name = "_topPanel",
            Children =
            {
                host,
                new Border { Name = "toolStripFiltersHost", Height = 27 },
                new Border { Name = "ToolStripScripts", Height = 25 },
            },
        };
        strip.PreferredSizeChanged += (_, _) =>
        {
            top.MainPreferredWidth = strip.PreferredSize.Width;
            top.MainPreferredHeight = Math.Max(25, strip.PreferredSize.Height);
            top.InvalidateMeasure();
        };
        Border body = new() { Name = "_contentPanel" };
        GitUI.Compat.WinFormsControls.ToolStripContainer container = new() { Children = { top, body } };
        Window window = new() { Width = 500, Height = 200, FontSize = points * 96d / 72, Content = container };
        try
        {
            window.Show();
            Settle(window);
            strip.Bounds.Height.Should().Be(25);
            top.Bounds.Height.Should().Be(27);
            body.Bounds.Y.Should().Be(27);
            strip.FontSize = points * 96d / 72;
            Settle(window);
            IconSplitButton item = (IconSplitButton)strip.Items.Single();
            double lineHeight = Math.Ceiling(WinFormsTextMeasurer.MeasureTextRenderer(item, "Branch").Height);
            double mainHeight = Math.Max(25, Math.Max(16, lineHeight) + 7);
            double rowHeight = Math.Max(27, mainHeight);
            strip.Bounds.Height.Should().Be(mainHeight);
            host.Bounds.Height.Should().Be(mainHeight);
            top.Bounds.Height.Should().Be(rowHeight);
            body.Bounds.Y.Should().Be(rowHeight);
            body.Bounds.Height.Should().Be(container.Bounds.Height - rowHeight);
            item.Bounds.Height.Should().Be(mainHeight - 3);
            strip.FontSize = 12;
            Settle(window);
            top.Bounds.Height.Should().Be(27);
            body.Bounds.Y.Should().Be(27);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Working_directory_refresh_should_use_its_open_logical_owner_in_closed_and_open_overflow(bool rightToLeft)
    {
        int originalMinimum = AppSettings.RecentReposComboMinWidth;
        using NativeToolStrip strip = NewStrip(rightToLeft);
        WorkingDirectoryToolStripSplitButton selector = new()
        {
            UseNativeToolStripLayout = true,
            Name = "_NO_TRANSLATE_WorkingDir",
            Margin = new Thickness(0, 1, 0, 2),
        };
        IGitModule module = Substitute.For<IGitModule>();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        IRepositoryHistoryUIService history = Substitute.For<IRepositoryHistoryUIService>();
        history.AddAsMostRecent(Arg.Any<string>()).Returns(_ => new List<Repository>());
        history.LoadSnapshot().Returns(new RepositoryHistorySnapshot([], []));
        string firstPath = Path.Combine(Path.GetTempPath(), "GitExtensions.OverflowOwner", "first-repository");
        string secondPath = Path.Combine(Path.GetTempPath(), "GitExtensions.OverflowOwner", "second-longer-repository");
        string thirdPath = Path.Combine(Path.GetTempPath(), "GitExtensions.OverflowOwner", "third-repository-with-longer-caption");
        module.WorkingDir.Returns(firstPath);
        selector.Initialize(() => commands, history, _ => { }, _ => { }, () => { }, () => { }, () => { });
        strip.Items.Add(selector);
        Window window = NewWindow(strip, 50);
        try
        {
            AppSettings.RecentReposComboMinWidth = 140;
            window.Show();
            Settle(window);
            strip.GetItemPlacement(selector).Should().Be(NativeToolStripItemPlacement.Overflow);
            selector.GetVisualParent().Should().BeNull();
            history.ClearReceivedCalls();
            module.WorkingDir.Returns(secondPath);
            selector.RefreshContent();
            Settle(window);
            history.Received(1).AddAsMostRecent(secondPath);
            selector.Content.Should().Be(PathUtil.GetDisplayPath(secondPath));
            float measured = (float)WinFormsGraphicsTextMeasurer.MeasureSize(selector, selector.Content as string ?? string.Empty).Width;
            measured = measured + 11 + 5;
            selector.Width.Should().Be(Math.Max(140, (int)measured));
            history.ClearReceivedCalls();
            strip.ShowOverflow();
            Settle(window);
            history.DidNotReceive().AddAsMostRecent(Arg.Any<string>());
            selector.Content.Should().Be(PathUtil.GetDisplayPath(secondPath), "visual reparenting is not a source content/history refresh route");
            TopLevel.GetTopLevel(selector).Should().NotBeNull();
            strip.IsOverflowOpen.Should().BeTrue();
            selector.Parent.Should().BeSameAs(strip);
            selector.GetVisualParent().Should().BeSameAs(strip.OverflowContent);
            strip.GetLogicalAncestors().Should().Contain(window);
            strip.CloseOverflow();
            strip.Width = strip.PreferredSize.Width + 20;
            Settle(window);
            history.DidNotReceive().AddAsMostRecent(Arg.Any<string>());
            strip.Width = 50;
            Settle(window);
            strip.ShowOverflow();
            Settle(window);
            history.ClearReceivedCalls();
            module.WorkingDir.Returns(thirdPath);
            selector.RefreshContent();
            Settle(window);
            history.Received(1).AddAsMostRecent(thirdPath);
            selector.Content.Should().Be(PathUtil.GetDisplayPath(thirdPath));
            selector.GetVisualParent().Should().BeSameAs(strip.OverflowContent);
            strip.CloseOverflow();
            strip.Items.Remove(selector);
            history.ClearReceivedCalls();
            selector.RefreshContent();
            history.DidNotReceive().AddAsMostRecent(Arg.Any<string>());
        }
        finally
        {
            window.Close();
            AppSettings.RecentReposComboMinWidth = originalMinimum;
        }
    }

    private static NativeToolStrip NewStrip(bool rightToLeft)
        => new() { FlowDirection = rightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight };

    private static SourceSizedControl NewItem(string name, int width, int height, Thickness? margin = null)
        => new(new Size(width, height)) { Name = name, Margin = margin ?? new Thickness(0, 1, 0, 2) };

    private static Window NewWindow(NativeToolStrip strip, int width)
    {
        strip.Width = width;
        return new Window { Width = 1800, Height = 180, Content = new Canvas { Background = Brushes.White, Children = { strip } } };
    }

    private static void Settle(Window window)
    {
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
    }

    private static void RecordEvidence(Window window, NativeToolStrip strip, string state)
    {
        string? directory = Environment.GetEnvironmentVariable("GITEXT_TOOLSTRIP_OVERFLOW_EVIDENCE");
        if (string.IsNullOrWhiteSpace(directory))
        {
            return;
        }

        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, $"{state}-{Guid.NewGuid():N}");
        using Avalonia.Media.Imaging.WriteableBitmap frame = window.CaptureRenderedFrame()
            ?? throw new InvalidOperationException("The actual overflow scene must render before recording evidence.");
        frame.Save(path + ".png", Avalonia.Media.Imaging.PngBitmapEncoderOptions.Default);
        string? popupPath = null;
        if (strip.IsOverflowOpen && strip.OverflowContent.Bounds.Width > 0 && strip.OverflowContent.Bounds.Height > 0)
        {
            popupPath = path + "-popup.png";
            using Avalonia.Media.Imaging.RenderTargetBitmap popupFrame = new(
                new PixelSize((int)Math.Ceiling(strip.OverflowContent.Bounds.Width), (int)Math.Ceiling(strip.OverflowContent.Bounds.Height)),
                new Vector(96, 96));
            popupFrame.Render(strip.OverflowContent);
            popupFrame.Save(popupPath, Avalonia.Media.Imaging.PngBitmapEncoderOptions.Default);
        }

        File.WriteAllText(path + ".json", JsonSerializer.Serialize(new
        {
            bounds = DescribeBounds(strip.Bounds),
            preferredSize = new { strip.PreferredSize.Width, strip.PreferredSize.Height },
            strip.HasOverflow,
            strip.IsOverflowOpen,
            popupBounds = DescribeBounds(strip.OverflowContent.Bounds),
            popupPath,
            captureMethod = "actualHeadlessWindowFrame; openPopupRenderTargetBitmap",
            items = strip.Items.Select(item => new { item.Name, bounds = DescribeBounds(item.Bounds), placement = strip.GetItemPlacement(item).ToString(), item.Opacity, item.IsHitTestVisible }).ToArray(),
            scope = "source integer layout and same-instance popup proof; not native chevron, popup raster, physical input or complete keyboard identity",
        }, new JsonSerializerOptions { WriteIndented = true }));

        // Serialize measured coordinates only. Avalonia Rect/Size convenience
        // properties include an undefined aspect ratio for a closed zero-size popup.
        static object DescribeBounds(Rect bounds) => new { bounds.X, bounds.Y, bounds.Width, bounds.Height };
    }

    private sealed class SourceSizedControl(Size preferred) : Control
    {
        protected override bool BypassFlowDirectionPolicies => true;

        protected override Size MeasureOverride(Size availableSize) => preferred;
    }
}
