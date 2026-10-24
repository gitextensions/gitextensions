using System.Diagnostics;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Diagnostics;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitUI.Compat;
using ToolTip = Avalonia.Controls.ToolTip;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class NativeToolbarOverflowTooltipTests
{
    [AvaloniaTest]
    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public async Task Automatic_hint_service_should_follow_the_same_owners_actual_main_and_overflow_parent(bool split, bool rightToLeft)
    {
        using OverflowFixture fixture = new(split, rightToLeft);
        Control[] original = fixture.Strip.Items.ToArray();
        PlacementMode configuredPlacement = fixture.Menu.Placement;
        fixture.Strip.GetCurrentParent(fixture.Owner).Should().BeSameAs(fixture.Strip);
        await fixture.HoverOwnerAsync();
        fixture.Menu.ShowAt(fixture.Owner);
        fixture.Settle();
        ToolTip.GetServiceEnabled(fixture.Owner).Should().BeFalse();
        ToolTip.GetIsOpen(fixture.Owner).Should().BeFalse();
        fixture.Menu.Hide();
        fixture.MoveAway();
        fixture.NarrowAndOpenOverflow();

        for (int cycle = 0; cycle < 2; cycle++)
        {
            fixture.Owner.Parent.Should().BeSameAs(fixture.Strip);
            fixture.Owner.GetVisualParent().Should().BeSameAs(fixture.Strip.OverflowContent);
            fixture.Strip.GetCurrentParent(fixture.Owner).Should().BeSameAs(fixture.Strip.OverflowContent);
            await fixture.HoverOwnerAsync();
            fixture.Menu.ShowAt(fixture.Owner);
            fixture.Settle();
            fixture.Menu.IsOpen.Should().BeTrue();
            fixture.Strip.IsOverflowOpen.Should().BeTrue();
            fixture.Menu.Placement.Should().Be(configuredPlacement);
            fixture.Menu.Popup.Placement.Should().Be(rightToLeft ? PlacementMode.LeftEdgeAlignedTop : PlacementMode.RightEdgeAlignedTop);
            ToolTip.GetServiceEnabled(fixture.Owner).Should().BeTrue("the source freezes its logical main owner, not the still-visible overflow parent hosting this hint");
            ToolTip.GetIsOpen(fixture.Owner).Should().BeTrue();
            fixture.Menu.Hide();
            fixture.Menu.Popup.Placement.Should().Be(configuredPlacement);
            fixture.Strip.CloseOverflow();
            fixture.MoveAway();
            if (cycle == 0)
            {
                fixture.OpenOverflow();
            }
        }

        fixture.Strip.Width = 450;
        fixture.Settle();
        fixture.Strip.GetCurrentParent(fixture.Owner).Should().BeSameAs(fixture.Strip);
        await fixture.HoverOwnerAsync();
        fixture.Menu.ShowAt(fixture.Owner);
        fixture.Settle();
        fixture.Menu.Popup.Placement.Should().Be(configuredPlacement, "regrowth must retire the overflow-only cascade direction");
        ToolTip.GetServiceEnabled(fixture.Owner).Should().BeFalse("a regrown main parent must regain its source-only automatic-hint suspension");
        ToolTip.GetIsOpen(fixture.Owner).Should().BeFalse();
        fixture.Menu.Hide();
        fixture.Settle();
        ToolTip.GetServiceEnabled(fixture.Owner).Should().BeTrue();
        fixture.Strip.Items.Should().Equal(original);
        ToolTip.GetTip(fixture.Owner).Should().BeSameAs(fixture.Tip);
        ToolTip.GetShowDelay(fixture.Owner).Should().Be(40);
    }

    [AvaloniaTest]
    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public async Task Replacing_a_genuinely_overflowed_owners_menu_should_not_borrow_the_hidden_logical_parents_tooltip_service(bool split, bool rightToLeft)
    {
        using OverflowFixture fixture = new(split, rightToLeft);
        fixture.NarrowAndOpenOverflow();
        await fixture.HoverOwnerAsync();
        fixture.Menu.ShowAt(fixture.Owner);
        fixture.Settle();
        fixture.Menu.Hide();
        MenuFlyout replacement = new() { Items = { new MenuItem { Header = "Replacement source option" } } };
        fixture.SetFlyout(replacement);
        await fixture.HoverOwnerAsync();
        try
        {
            replacement.ShowAt(fixture.Owner);
            fixture.Settle();
            replacement.IsOpen.Should().BeTrue();
            fixture.Strip.IsOverflowOpen.Should().BeTrue();
            fixture.Strip.GetCurrentParent(fixture.Owner).Should().BeSameAs(fixture.Strip.OverflowContent);
            ToolTip.GetServiceEnabled(fixture.Owner).Should().BeTrue();
            ToolTip.GetIsOpen(fixture.Owner).Should().BeTrue();
        }
        finally
        {
            replacement.Hide();
        }

        fixture.Strip.CloseOverflow();
        fixture.Strip.Width = 450;
        fixture.Settle();
        await fixture.HoverOwnerAsync();
        replacement.ShowAt(fixture.Owner);
        fixture.Settle();
        ToolTip.GetServiceEnabled(fixture.Owner).Should().BeFalse();
        ToolTip.GetIsOpen(fixture.Owner).Should().BeFalse();
        replacement.Hide();
        fixture.Settle();
        ToolTip.GetServiceEnabled(fixture.Owner).Should().BeTrue();
    }

    [AvaloniaTest]
    [TestCase(false, false, false)]
    [TestCase(false, true, false)]
    [TestCase(true, false, false)]
    [TestCase(true, true, false)]
    [TestCase(false, false, true)]
    [TestCase(false, true, true)]
    [TestCase(true, false, true)]
    [TestCase(true, true, true)]
    public async Task Real_overflow_owner_mouse_down_should_keep_source_pending_or_shown_hover_hints(bool split, bool rightToLeft, bool pending)
    {
        using OverflowFixture fixture = new(split, rightToLeft);
        fixture.NarrowAndOpenOverflow();
        ToolTip.SetShowDelay(fixture.Owner, 150);
        if (pending)
        {
            fixture.MoveOwner();
            ToolTip.GetIsOpen(fixture.Owner).Should().BeFalse();
        }
        else
        {
            await fixture.HoverOwnerAsync();
        }

        fixture.OpenOwnerByPointer();
        await fixture.WaitForHintAsync();
        fixture.Menu.IsOpen.Should().BeTrue();
        fixture.Strip.IsOverflowOpen.Should().BeTrue();
        fixture.Owner.IsPointerOver.Should().BeTrue();
        ToolTip.GetServiceEnabled(fixture.Owner).Should().BeTrue();
        ToolTip.GetShowDelay(fixture.Owner).Should().Be(150);
        ToolTip.GetTip(fixture.Owner).Should().BeSameAs(fixture.Tip);
    }

    [AvaloniaTest]
    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public async Task Closing_only_the_hint_window_should_not_erase_the_source_active_hover_item_used_by_menu_refresh(bool split, bool rightToLeft)
    {
        using OverflowFixture fixture = new(split, rightToLeft);
        fixture.NarrowAndOpenOverflow();
        await fixture.HoverOwnerAsync();
        long epoch = fixture.Strip.GetToolTipHoverEpoch(fixture.Owner);
        ToolTip.SetIsOpen(fixture.Owner, false);
        fixture.Settle();
        ToolTip.GetIsOpen(fixture.Owner).Should().BeFalse();
        fixture.Strip.GetToolTipHoverEpoch(fixture.Owner).Should().Be(epoch);
        fixture.Menu.ShowAt(fixture.Owner);
        fixture.Settle();
        ToolTip.GetIsOpen(fixture.Owner).Should().BeTrue("the original parent retains its active hover item after Hide/auto-pop and refreshes it when the dropdown opens");
        fixture.Menu.IsOpen.Should().BeTrue();
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public async Task An_old_pending_overflow_hint_must_not_shorten_the_delay_of_a_later_hover_epoch(bool split)
    {
        using OverflowFixture fixture = new(split, false);
        fixture.NarrowAndOpenOverflow();
        ToolTip.SetShowDelay(fixture.Owner, 500);
        fixture.MoveOwner();
        await Task.Delay(280);
        fixture.Settle();
        long previous = fixture.Strip.GetToolTipHoverEpoch(fixture.Owner);
        fixture.OpenOwnerByPointer();
        fixture.MoveAway();
        fixture.MoveOwner();
        fixture.Strip.GetToolTipHoverEpoch(fixture.Owner).Should().NotBe(previous);
        await Task.Delay(320);
        fixture.Settle();
        ToolTip.GetIsOpen(fixture.Owner).Should().BeFalse("the previous hover's remaining deadline must not reveal a hint during the new hover's fresh500ms delay");
        await fixture.WaitForHintAsync();
        fixture.Menu.IsOpen.Should().BeTrue();
    }

    [AvaloniaTest]
    [TestCase(false, "cancel")]
    [TestCase(true, "cancel")]
    [TestCase(false, "disable")]
    [TestCase(true, "disable")]
    [TestCase(false, "clear")]
    [TestCase(true, "clear")]
    [TestCase(false, "remove")]
    [TestCase(true, "remove")]
    public async Task Pending_overflow_refresh_must_honor_current_caller_state_and_owner_lifetime(bool split, string change)
    {
        using OverflowFixture fixture = new(split, false);
        fixture.NarrowAndOpenOverflow();
        ToolTip.SetShowDelay(fixture.Owner, 150);
        EventHandler<CancelRoutedEventArgs> cancellation = (_, args) => args.Cancel = true;
        if (change == "cancel")
        {
            ToolTip.AddToolTipOpeningHandler(fixture.Owner, cancellation);
        }

        try
        {
            fixture.MoveOwner();
            fixture.OpenOwnerByPointer();
            switch (change)
            {
                case "disable":
                    ToolTip.SetServiceEnabled(fixture.Owner, false);
                    break;
                case "clear":
                    ToolTip.SetTip(fixture.Owner, null);
                    break;
                case "remove":
                    fixture.Strip.Items.Remove(fixture.Owner);
                    break;
            }

            await Task.Delay(250);
            fixture.Settle();
            ToolTip.GetIsOpen(fixture.Owner).Should().BeFalse();
            if (change == "remove")
            {
                fixture.Owner.Parent.Should().BeNull();
            }
            else
            {
                fixture.Menu.IsOpen.Should().BeTrue();
                ToolTip.GetServiceEnabled(fixture.Owner).Should().Be(change != "disable");
            }
        }
        finally
        {
            ToolTip.RemoveToolTipOpeningHandler(fixture.Owner, cancellation);
        }
    }

    [AvaloniaTest]
    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public async Task The_actual_overflow_parent_should_own_reclick_escape_and_outside_dismissal(bool split, bool rightToLeft)
    {
        using OverflowFixture fixture = new(split, rightToLeft);
        fixture.NarrowAndOpenOverflow();
        await fixture.HoverOwnerAsync();
        fixture.OpenOwnerByPointer();
        await fixture.WaitForHintAsync();
        fixture.Menu.Popup.IsLightDismissEnabled.Should().BeFalse("the visible overflow owns this native-like submenu's outside dismissal");
        fixture.OpenOwnerByPointer(expectOpen: false);
        fixture.Strip.IsOverflowOpen.Should().BeTrue();
        fixture.Menu.Popup.IsLightDismissEnabled.Should().BeTrue("the scoped value must retire with this menu, not overwrite its configuration");

        fixture.MoveOwner();
        fixture.OpenOwnerByPointer();
        TopLevel menuRoot = TopLevel.GetTopLevel(fixture.Menu.Popup.Child)
            ?? throw new AssertionException("The actual child menu must have an input root.");
        menuRoot.KeyPress(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, keySymbol: null);
        menuRoot.KeyRelease(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, keySymbol: null);
        fixture.Settle();
        fixture.Menu.IsOpen.Should().BeFalse();
        fixture.Strip.IsOverflowOpen.Should().BeTrue();

        fixture.MoveOwner();
        fixture.OpenOwnerByPointer();
        fixture.Click(fixture.Outside);
        fixture.Menu.IsOpen.Should().BeFalse();
        fixture.Strip.IsOverflowOpen.Should().BeFalse();
        fixture.OutsideClicks.Should().Be(1, "the original closes both menus and executes the outside control's click once");
        fixture.Menu.Popup.IsLightDismissEnabled.Should().BeTrue();
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void A_live_caller_popup_dismiss_binding_should_survive_the_overflow_scope(bool split)
    {
        using OverflowFixture fixture = new(split, false);
        fixture.NarrowAndOpenOverflow();
        fixture.Menu.ShowAt(fixture.Owner);
        fixture.Settle();
        Popup source = new() { IsLightDismissEnabled = true, Placement = PlacementMode.Top };
        using IDisposable binding = fixture.Menu.Popup.Bind(Popup.IsLightDismissEnabledProperty, source.GetObservable(Popup.IsLightDismissEnabledProperty));
        using IDisposable placement = fixture.Menu.Popup.Bind(Popup.PlacementProperty, source.GetObservable(Popup.PlacementProperty));
        fixture.Menu.Popup.IsLightDismissEnabled.Should().BeFalse();
        fixture.Menu.Popup.Placement.Should().Be(PlacementMode.RightEdgeAlignedTop);
        source.IsLightDismissEnabled = false;
        source.Placement = PlacementMode.BottomEdgeAlignedLeft;
        fixture.Menu.Popup.GetBaseValue(Popup.IsLightDismissEnabledProperty).Value.Should().BeFalse();
        fixture.Menu.Popup.GetBaseValue(Popup.PlacementProperty).Value.Should().Be(PlacementMode.BottomEdgeAlignedLeft);
        fixture.Menu.Hide();
        fixture.Settle();
        fixture.Menu.Popup.IsLightDismissEnabled.Should().BeFalse();
        fixture.Menu.Popup.Placement.Should().Be(PlacementMode.BottomEdgeAlignedLeft);
        source.IsLightDismissEnabled = true;
        source.Placement = PlacementMode.RightEdgeAlignedBottom;
        fixture.Menu.Popup.IsLightDismissEnabled.Should().BeTrue("the caller's binding must remain live after the owned value is released");
        fixture.Menu.Popup.Placement.Should().Be(PlacementMode.RightEdgeAlignedBottom);
    }

    [AvaloniaTest]
    [TestCase(false, "freeze")]
    [TestCase(true, "freeze")]
    [TestCase(false, "native")]
    [TestCase(true, "native")]
    [TestCase(false, "detach")]
    [TestCase(true, "detach")]
    public void Releasing_an_overflow_owner_scope_should_restore_its_current_popup_policy(bool split, string change)
    {
        using OverflowFixture fixture = new(split, false);
        fixture.NarrowAndOpenOverflow();
        fixture.Menu.ShowAt(fixture.Owner);
        fixture.Settle();
        fixture.Menu.Popup.IsLightDismissEnabled.Should().BeFalse();
        switch (change)
        {
            case "freeze":
                NativeToolStrip.SetFreezeDropDownOwnerPaint(fixture.Owner, false);
                break;
            case "native" when fixture.Owner is NativeToolStripSplitButton splitOwner:
                splitOwner.UseNativeToolStripLayout = false;
                break;
            case "native":
                ((IconDropDownButton)fixture.Owner).UseNativeToolStripLayout = false;
                break;
            case "detach":
                fixture.Strip.CloseOverflow();
                break;
        }

        fixture.Settle();
        fixture.Menu.Popup.IsLightDismissEnabled.Should().BeTrue();
        if (change == "detach")
        {
            fixture.Menu.IsOpen.Should().BeFalse();
        }
        else
        {
            fixture.Menu.IsOpen.Should().BeTrue();
            fixture.Click(fixture.Outside);
            fixture.Menu.IsOpen.Should().BeFalse("restoring the popup property while open must leave the real overflow parent's outside dismissal working");
            fixture.Strip.IsOverflowOpen.Should().BeFalse();
            fixture.OutsideClicks.Should().Be(1);
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public async Task A_replaced_open_overflow_menu_and_its_reopened_successor_must_keep_real_parent_dismissal(bool split)
    {
        using OverflowFixture fixture = new(split, false);
        fixture.NarrowAndOpenOverflow();
        await fixture.HoverOwnerAsync();
        fixture.OpenOwnerByPointer();
        await fixture.WaitForHintAsync();
        MenuFlyout replacement = new() { Items = { new MenuItem { Header = "Replacement source option" } } };
        fixture.SetFlyout(replacement);
        fixture.Settle();
        fixture.Menu.Popup.IsLightDismissEnabled.Should().BeTrue();
        fixture.Click(fixture.Outside);
        fixture.Menu.IsOpen.Should().BeFalse();
        fixture.Strip.IsOverflowOpen.Should().BeFalse();
        fixture.OutsideClicks.Should().Be(1);

        fixture.OpenOverflow();
        fixture.MoveOwner();
        fixture.OpenOwnerByPointer();
        await fixture.WaitForHintAsync();
        replacement.Popup.IsLightDismissEnabled.Should().BeFalse();
        fixture.Click(fixture.Outside);
        replacement.IsOpen.Should().BeFalse();
        fixture.Strip.IsOverflowOpen.Should().BeFalse();
        fixture.OutsideClicks.Should().Be(2);
        replacement.Popup.IsLightDismissEnabled.Should().BeTrue();
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public async Task Reopening_an_overflow_menu_must_use_the_latest_live_caller_hint_setting(bool split)
    {
        using OverflowFixture fixture = new(split, false);
        Border source = new();
        ToolTip.SetServiceEnabled(source, true);
        using IDisposable binding = fixture.Owner.Bind(ToolTip.ServiceEnabledProperty, source.GetObservable(ToolTip.ServiceEnabledProperty));
        fixture.NarrowAndOpenOverflow();
        await fixture.HoverOwnerAsync();
        fixture.OpenOwnerByPointer();
        await fixture.WaitForHintAsync();
        fixture.Menu.Hide();
        ToolTip.SetIsOpen(fixture.Owner, false);
        fixture.MoveAway();
        ToolTip.SetServiceEnabled(source, false);
        fixture.MoveOwner();
        fixture.OpenOwnerByPointer();
        await Task.Delay(100);
        fixture.Settle();
        ToolTip.GetServiceEnabled(fixture.Owner).Should().BeFalse();
        ToolTip.GetIsOpen(fixture.Owner).Should().BeFalse();
        fixture.Menu.IsOpen.Should().BeTrue();
        fixture.Menu.Hide();
        fixture.MoveAway();
        ToolTip.SetServiceEnabled(source, true);
        await fixture.HoverOwnerAsync();
        fixture.OpenOwnerByPointer();
        await fixture.WaitForHintAsync();
        ToolTip.GetServiceEnabled(fixture.Owner).Should().BeTrue();
        ToolTip.GetShowDelay(fixture.Owner).Should().Be(40);
        ToolTip.GetTip(fixture.Owner).Should().BeSameAs(fixture.Tip);
    }

    [AvaloniaTest]
    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public async Task Blank_parent_padding_and_sibling_commands_should_follow_the_real_source_submenu_routes(bool split, bool rightToLeft)
    {
        using OverflowFixture fixture = new(split, rightToLeft);
        Control[] original = fixture.Strip.Items.ToArray();
        fixture.NarrowAndOpenOverflow();
        await fixture.HoverOwnerAsync();
        fixture.OpenOwnerByPointer();
        await fixture.WaitForHintAsync();
        fixture.ClickAt(fixture.Strip.OverflowContent, default);
        if (fixture.Menu.IsOpen)
        {
            await fixture.CaptureFailureAsync("blank-parent-child-still-open");
        }

        fixture.Menu.IsOpen.Should().BeFalse("the source blank parent pixel (0,0) closes only its child menu");
        fixture.Strip.IsOverflowOpen.Should().BeTrue();
        fixture.SiblingClicks.Should().Be(0);
        fixture.Menu.Popup.IsLightDismissEnabled.Should().BeTrue();

        fixture.MoveOwner();
        fixture.OpenOwnerByPointer();
        await fixture.WaitForHintAsync();
        bool hintHit = fixture.Click(fixture.Sibling);
        int expectedClicks = hintHit ? 0 : 1;
        if (fixture.Menu.IsOpen || fixture.Strip.IsOverflowOpen || fixture.SiblingClicks != expectedClicks)
        {
            await fixture.CaptureFailureAsync("sibling-closure-after-click");
        }

        fixture.Menu.IsOpen.Should().BeFalse();
        fixture.Strip.IsOverflowOpen.Should().BeFalse();
        fixture.SiblingClicks.Should().Be(expectedClicks, "the original dismisses a hint-covered press without invoking, while an actual uncovered sibling hit invokes once");
        fixture.OutsideClicks.Should().Be(0);
        fixture.Strip.Items.Should().Equal(original);
    }

    [AvaloniaTest]
    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public async Task A_shown_owner_hint_hit_should_dismiss_both_menus_without_invoking_the_obscured_sibling(bool stringTip, bool split)
    {
        using OverflowFixture fixture = new(split, rightToLeft: true);
        object configured = stringTip ? fixture.Tip.Content! : fixture.Tip;
        ToolTip.SetTip(fixture.Owner, configured);
        fixture.NarrowAndOpenOverflow();
        await fixture.HoverOwnerAsync();
        fixture.OpenOwnerByPointer();
        await fixture.WaitForHintAsync();
        try
        {
            fixture.ActiveTip.Content.Should().Be(fixture.Tip.Content);
            fixture.ActiveTip.IsEffectivelyVisible.Should().BeTrue();
            fixture.ActiveTip.IsHitTestVisible.Should().BeTrue("the native hint is the real input target, not a click-through command surface");
            ToolTip.GetTip(fixture.Owner).Should().BeSameAs(configured);
            Point target = fixture.GetShownHintSiblingPoint(covered: true);
            fixture.ClickAt(fixture.Sibling, target, expectedHintHit: true);
            fixture.Menu.IsOpen.Should().BeFalse();
            fixture.Strip.IsOverflowOpen.Should().BeFalse();
            fixture.SiblingClicks.Should().Be(0, "the same real source hint-covered route invokes no obscured command");
            fixture.OutsideClicks.Should().Be(0);
            fixture.Menu.Popup.IsLightDismissEnabled.Should().BeTrue("closing the real parent must retire its child scope, not leave an open ghost menu");
            ToolTip.GetTip(fixture.Owner).Should().BeSameAs(configured);
        }
        catch
        {
            await fixture.CaptureFailureAsync("shown-hint-covered-route");
            throw;
        }
    }

    [AvaloniaTest]
    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public async Task An_uncovered_sibling_hit_should_invoke_once_through_the_same_shown_hint_input_route(bool split, bool rightToLeft)
    {
        using OverflowFixture fixture = new(split, rightToLeft);
        fixture.NarrowAndOpenOverflow();
        await fixture.HoverOwnerAsync();
        fixture.OpenOwnerByPointer();
        await fixture.WaitForHintAsync();
        try
        {
            Point target = fixture.GetShownHintSiblingPoint(covered: false);
            fixture.ClickAt(fixture.Sibling, target, expectedHintHit: false);
            fixture.Menu.IsOpen.Should().BeFalse();
            fixture.Strip.IsOverflowOpen.Should().BeFalse();
            fixture.SiblingClicks.Should().Be(1, "the positive control must hit the genuine sibling, using the identical mouse down/up actuator");
            fixture.OutsideClicks.Should().Be(0);
        }
        catch
        {
            await fixture.CaptureFailureAsync("shown-hint-uncovered-route");
            throw;
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public async Task Hint_identity_and_input_should_follow_the_same_owners_actual_detach_and_reparent(bool stringTip)
    {
        using OverflowFixture fixture = new(split: false, rightToLeft: false);
        object configured = stringTip ? fixture.Tip.Content! : fixture.Tip;
        ToolTip.SetTip(fixture.Owner, configured);
        fixture.NarrowAndOpenOverflow();
        await fixture.HoverOwnerAsync();
        ToolTip original = fixture.ActiveTip;
        original.IsHitTestVisible.Should().BeTrue();
        ToolTip.SetIsOpen(fixture.Owner, false);
        fixture.Strip.CloseOverflow();
        fixture.Strip.Items.Remove(fixture.Owner);
        fixture.Settle();
        Canvas content = (Canvas)fixture.Window.Content!;
        Canvas.SetLeft(fixture.Owner, 300);
        Canvas.SetTop(fixture.Owner, 100);
        content.Children.Add(fixture.Owner);
        fixture.Settle();
        ToolTip.SetIsOpen(fixture.Owner, true);
        fixture.Settle();
        fixture.ActiveTip.Should().BeSameAs(original);
        fixture.ActiveTip.IsEffectivelyVisible.Should().BeTrue();
        fixture.ActiveTip.IsHitTestVisible.Should().BeTrue("the source adapter must not change unrelated hint input after detachment");
        ToolTip.GetTip(fixture.Owner).Should().BeSameAs(configured);

        ToolTip.SetIsOpen(fixture.Owner, false);
        content.Children.Remove(fixture.Owner);
        fixture.Strip.Items.Add(fixture.Owner);
        fixture.Settle();
        fixture.OpenOverflow();
        ToolTip.SetIsOpen(fixture.Owner, true);
        fixture.Settle();
        fixture.ActiveTip.Should().BeSameAs(original);
        ToolTip.GetIsOpen(fixture.Owner).Should().BeTrue();
        fixture.ActiveTip.IsEffectivelyVisible.Should().BeTrue();
        fixture.ActiveTip.IsHitTestVisible.Should().BeTrue();
        fixture.ClickAt(fixture.ActiveTip, new Rect(fixture.ActiveTip.Bounds.Size).Center, expectedHintHit: true);
        fixture.Strip.IsOverflowOpen.Should().BeFalse("the actual rejoined hint must resolve its current strip rather than retain a detached owner scope");
        fixture.SiblingClicks.Should().Be(0);
        ToolTip.GetTip(fixture.Owner).Should().BeSameAs(configured);
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public async Task Hint_input_dismissal_must_not_replace_a_callers_local_value_or_live_input_binding(bool binding)
    {
        using OverflowFixture fixture = new(split: true, rightToLeft: false);
        Border source = new() { IsHitTestVisible = true };
        using IDisposable? value = binding ? fixture.Tip.Bind(InputElement.IsHitTestVisibleProperty, source.GetObservable(InputElement.IsHitTestVisibleProperty)) : null;
        if (!binding)
        {
            fixture.Tip.IsHitTestVisible = true;
        }

        fixture.NarrowAndOpenOverflow();
        await fixture.HoverOwnerAsync();
        fixture.ActiveTip.Should().BeSameAs(fixture.Tip);
        fixture.ActiveTip.IsHitTestVisible.Should().BeTrue("the source dismissal handler must not overwrite caller-local tooltip intent");
        if (binding)
        {
            source.IsHitTestVisible = false;
            fixture.Tip.IsHitTestVisible.Should().BeFalse();
            source.IsHitTestVisible = true;
            fixture.Tip.IsHitTestVisible.Should().BeTrue();
        }

        ToolTip.SetIsOpen(fixture.Owner, false);
        fixture.Strip.CloseOverflow();
        fixture.Strip.Items.Remove(fixture.Owner);
        fixture.Settle();
        ((Canvas)fixture.Window.Content!).Children.Add(fixture.Owner);
        fixture.Settle();
        ToolTip.SetIsOpen(fixture.Owner, true);
        fixture.Settle();
        fixture.ActiveTip.IsHitTestVisible.Should().BeTrue();
        if (binding)
        {
            source.IsHitTestVisible = false;
            fixture.Tip.IsHitTestVisible.Should().BeFalse("detachment and reopening must preserve the caller's live binding");
        }
    }

    private sealed class OverflowFixture : IDisposable
    {
        private readonly StringBuilder _trace = new();
        private readonly Stopwatch _elapsed = Stopwatch.StartNew();
        private IPointer? _pointer;

        internal OverflowFixture(bool split, bool rightToLeft)
        {
            Strip = new NativeToolStrip { Width = 450, Height = 25, FlowDirection = rightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight };
            Button command = new() { Width = 140, Content = "Main command", Margin = new Thickness(0, 1, 0, 2) };
            NativeToolStrip.SetItemAutoSize(command, false);
            NativeToolStrip.SetItemHeight(command, 22);
            Strip.Items.Add(command);
            Owner = split
                ? new NativeToolStripSplitButton { Width = 120, UseNativeToolStripLayout = true, Content = "Source selector" }
                : new IconDropDownButton { Width = 120, UseNativeToolStripLayout = true, Content = "Source selector" };
            Owner.Margin = new Thickness(0, 1, 0, 2);
            // The native consumer fixes AutoSize=false and Size=120x22 on this item.
            // Port that source input, not an incidental empty-template desired height.
            NativeToolStrip.SetItemAutoSize(Owner, false);
            NativeToolStrip.SetItemHeight(Owner, 22);
            Menu = new MenuFlyout { Items = { new MenuItem { Header = "Source option" } } };
            SetFlyout(Menu);
            Tip = new ToolTip { Content = "Same retained source owner hint" };
            ToolTip.SetTip(Owner, Tip);
            ToolTip.SetShowDelay(Owner, 40);
            ToolTip.SetServiceEnabled(Owner, true);
            Strip.Items.Add(Owner);
            Sibling = new Button { Width = 120, Content = "Sibling source command", Margin = new Thickness(0, 1, 0, 2) };
            NativeToolStrip.SetItemAutoSize(Sibling, false);
            NativeToolStrip.SetItemHeight(Sibling, 22);
            Sibling.Click += (_, _) => SiblingClicks++;
            Strip.Items.Add(Sibling);
            Outside = new Button { Width = 100, Height = 23, Content = "Outside command" };
            Outside.Click += (_, _) => OutsideClicks++;
            Canvas.SetLeft(Outside, 600);
            Canvas.SetTop(Outside, 300);
            Window = new Window { Width = 850, Height = 450, Content = new Canvas { Background = Brushes.White, Children = { Strip, Outside } } };
            Window.Show();
            Settle();
            Owner.PropertyChanged += (_, args) =>
            {
                if (args.Property == InputElement.IsPointerOverProperty || args.Property == ToolTip.IsOpenProperty
                    || args.Property == ToolTip.ServiceEnabledProperty || args.Property == ToolTip.TipProperty)
                {
                    Trace(args.Property.Name);
                }
            };
            Menu.Opening += (_, _) => Trace("Menu.Opening");
            Menu.Opened += (_, _) => Trace("Menu.Opened");
            Menu.Closed += (_, _) => Trace("Menu.Closed");
            Owner.AddHandler(InputElement.PointerPressedEvent, (_, args) => _pointer = args.Pointer, RoutingStrategies.Tunnel, handledEventsToo: true);
            NativeToolStrip.GetFreezeDropDownOwnerPaint(Owner).Should().BeTrue("the real source strip must opt in its actual retained owner");
        }

        internal NativeToolStrip Strip { get; }

        internal Control Owner { get; }

        internal MenuFlyout Menu { get; }

        internal MenuFlyout CurrentMenu => (Owner is NativeToolStripSplitButton split ? split.Flyout : ((IconDropDownButton)Owner).Flyout) as MenuFlyout
            ?? throw new AssertionException("The retained owner must have its actual current source menu.");

        internal ToolTip Tip { get; }

        internal ToolTip ActiveTip => Owner.GetValue(ToolTipDiagnostics.ToolTipProperty) as ToolTip
            ?? throw new AssertionException("The actual source owner must have its generated or configured open hint.");

        internal Button Sibling { get; }

        internal Button Outside { get; }

        internal int SiblingClicks { get; private set; }

        internal int OutsideClicks { get; private set; }

        internal Window Window { get; }

        public void Dispose()
        {
            (Owner is NativeToolStripSplitButton split ? split.Flyout : ((IconDropDownButton)Owner).Flyout)?.Hide();
            Menu.Hide();
            Strip.CloseOverflow();
            ToolTip.SetIsOpen(Owner, false);
            Window.Close();
            Strip.Dispose();
            Dispatcher.UIThread.RunJobs();
        }

        internal void SetFlyout(MenuFlyout menu)
        {
            if (Owner is NativeToolStripSplitButton split)
            {
                split.Flyout = menu;
            }
            else
            {
                ((IconDropDownButton)Owner).Flyout = menu;
            }
        }

        internal void Settle()
        {
            Dispatcher.UIThread.RunJobs();
            Window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
        }

        internal void MoveAway()
        {
            Trace("MoveAway.before");
            Window.MouseMove(new Point(800, 400));
            if (Strip.IsOverflowOpen && TopLevel.GetTopLevel(Owner) is TopLevel popup && !ReferenceEquals(popup, Window))
            {
                popup.MouseMove(new Point(popup.ClientSize.Width - 1, popup.ClientSize.Height - 1));
            }

            Settle();
            Trace("MoveAway.after");
        }

        internal async Task HoverOwnerAsync()
        {
            MoveAway();
            MoveOwner();
            await WaitForHintAsync();
        }

        internal void MoveOwner()
        {
            Trace("MoveOwner.before");
            TopLevel parent = TopLevel.GetTopLevel(Owner) ?? throw new AssertionException("The retained owner must be attached to its actual main or overflow surface.");
            parent.MouseMove(Owner.TranslatePoint(new Rect(Owner.Bounds.Size).Center, parent)!.Value);
            Settle();
            Trace("MoveOwner.after");
        }

        internal void OpenOwnerByPointer(bool expectOpen = true)
        {
            TopLevel parent = TopLevel.GetTopLevel(Owner) ?? throw new AssertionException("The owner's real pointer route must enter its current native-like parent.");
            Rect target = Owner is NativeToolStripSplitButton split ? split.DropDownButtonBounds : new Rect(Owner.Bounds.Size);
            Point point = Owner.TranslatePoint(target.Center, parent)!.Value;
            parent.MouseMove(point);
            Trace("OpenOwner.beforeDown");
            TraceHit(parent, point);
            parent.MouseDown(point, MouseButton.Left);
            Trace("OpenOwner.afterDown");
            TraceHit(parent, point);
            parent.MouseUp(point, MouseButton.Left);
            Trace("OpenOwner.afterUp");
            TraceHit(parent, point);
            Settle();
            Trace("OpenOwner.afterSettle");
            CurrentMenu.IsOpen.Should().Be(expectOpen);
        }

        internal bool Click(Control control)
            => ClickAt(control, new Rect(control.Bounds.Size).Center);

        internal bool ClickAt(Control control, Point local, bool? expectedHintHit = null)
        {
            TopLevel parent = TopLevel.GetTopLevel(control) ?? throw new AssertionException("The actual source command must have an input root.");
            Point point = control.TranslatePoint(local, parent)!.Value;
            TraceHintGeometry("ClickAt.beforeMove", parent, point);
            TraceHit(parent, point);
            parent.MouseMove(point);
            Trace("ClickAt." + control.GetType().Name);
            TraceHintGeometry("ClickAt.afterMove", parent, point);
            TraceHit(parent, point);
            IInputElement? hit = parent.InputHitTest(point);
            bool hintHit = hit is Visual source && Owner.GetValue(ToolTipDiagnostics.ToolTipProperty) is ToolTip tip
                && (ReferenceEquals(source, tip) || tip.IsVisualAncestorOf(source));
            if (expectedHintHit is bool expected)
            {
                hintHit.Should().Be(expected, "the same real mouse route must hit the actually acquired hint or uncovered sibling before input");
            }

            if (ReferenceEquals(control, Sibling) && !hintHit)
            {
                (ReferenceEquals(hit, Sibling) || Sibling.IsVisualAncestorOf(hit as Visual)).Should().BeTrue("an uncovered positive route must really hit the sibling, not a child menu or dismissal overlay");
            }

            parent.MouseDown(point, MouseButton.Left);
            parent.MouseUp(point, MouseButton.Left);
            Settle();
            return hintHit;
        }

        internal Point GetShownHintSiblingPoint(bool covered)
        {
            ToolTip tip = ActiveTip;
            ToolTip.GetIsOpen(Owner).Should().BeTrue();
            tip.IsEffectivelyVisible.Should().BeTrue();
            TopLevel parent = TopLevel.GetTopLevel(Sibling)
                ?? throw new AssertionException("The source sibling must have its actual current parent.");
            Matrix siblingTransform = Sibling.TransformToVisual(parent)
                ?? throw new AssertionException("The genuine sibling must be attached to its current input root.");
            Matrix hintTransform = tip.TransformToVisual(parent)
                ?? throw new AssertionException("The shown hint must share the actual sibling input root.");

            // RTL local origins are right edges. Transform the complete rectangles,
            // then invert the acquired root point through the actual visual tree.
            Rect siblingBounds = new Rect(Sibling.Bounds.Size).TransformToAABB(siblingTransform);
            Rect hintBounds = new Rect(tip.Bounds.Size).TransformToAABB(hintTransform);
            Rect intersection = siblingBounds.Intersect(hintBounds);
            TraceHintGeometry("GetShownHintSiblingPoint", parent, null);
            _trace.AppendLine($"  covered={covered}; siblingBounds={siblingBounds}; hintBounds={hintBounds}; intersection={intersection}");
            Rect target;
            if (covered)
            {
                intersection.Width.Should().BeGreaterThan(2, "the naturally shown hint must actually cover the sibling; no synthetic placement or hit override is allowed");
                intersection.Height.Should().BeGreaterThan(2);
                target = intersection;
            }
            else
            {
                Rect[] uncovered = intersection.Width <= 0 || intersection.Height <= 0
                    ? [siblingBounds]
                    :
                    [
                        new(siblingBounds.X, siblingBounds.Y, intersection.X - siblingBounds.X, siblingBounds.Height),
                        new(intersection.Right, siblingBounds.Y, siblingBounds.Right - intersection.Right, siblingBounds.Height),
                        new(siblingBounds.X, siblingBounds.Y, siblingBounds.Width, intersection.Y - siblingBounds.Y),
                        new(siblingBounds.X, intersection.Bottom, siblingBounds.Width, siblingBounds.Bottom - intersection.Bottom),
                    ];
                target = uncovered.Where(bounds => bounds.Width > 2 && bounds.Height > 2)
                    .OrderByDescending(bounds => bounds.Width * bounds.Height).FirstOrDefault();
                target.Width.Should().BeGreaterThan(2, "a positive control requires an actual uncovered part of the sibling while its owner hint is shown");
                target.Height.Should().BeGreaterThan(2);
                hintBounds.Contains(target.Center).Should().BeFalse();
            }

            _trace.AppendLine($"  acquiredRect={target}; acquiredPoint={target.Center}");
            TraceHit(parent, target.Center);
            return parent.TranslatePoint(target.Center, Sibling)
                ?? throw new AssertionException("The acquired input-root point must transform back to the real sibling.");
        }

        private void TraceHintGeometry(string stage, TopLevel parent, Point? point)
        {
            ToolTip? tip = Owner.GetValue(ToolTipDiagnostics.ToolTipProperty) as ToolTip;
            TopLevel? tipRoot = TopLevel.GetTopLevel(tip);
            Point? tipTranslateOrigin = tipRoot is not null ? tip?.TranslatePoint(default, parent) : null;
            PixelPoint? tipScreenOrigin = tipRoot is not null ? tip?.PointToScreen(default) : null;
            _trace.AppendLine($"{stage}: target={point}; inputRoot={parent.GetType().Name}; inputScreenOrigin={parent.PointToScreen(default)}; siblingOrigin={Sibling.TranslatePoint(default, parent)}; siblingBounds={Sibling.Bounds}; tipOpen={ToolTip.GetIsOpen(Owner)}; tipBounds={tip?.Bounds}; tipRoot={tipRoot?.GetType().Name}; tipTranslateOrigin={tipTranslateOrigin}; tipScreenOrigin={tipScreenOrigin}; tipVisible={tip?.IsEffectivelyVisible}; tipHitTestVisible={tip?.IsHitTestVisible}");
        }

        internal async Task WaitForHintAsync()
        {
            Stopwatch elapsed = Stopwatch.StartNew();
            while (elapsed.Elapsed < TimeSpan.FromSeconds(2))
            {
                Settle();
                if (ToolTip.GetIsOpen(Owner))
                {
                    TopLevel.GetTopLevel(ActiveTip).Should().NotBeNull();
                    return;
                }

                await Task.Delay(10);
            }

            await CaptureFailureAsync("WaitForHint.timeout");
            throw new TimeoutException("The retained source owner's automatic hint did not open on its actual current parent. See attached lifetime trace and actual surface frames.");
        }

        internal async Task CaptureFailureAsync(string stage)
        {
            Trace(stage);
            string evidence = Path.GetFullPath(Environment.GetEnvironmentVariable("GITEXTENSIONS_TEST_EVIDENCE_DIRECTORY")
                ?? Path.Combine(Path.GetTempPath(), "GitExtensions.TooltipFailures", Guid.NewGuid().ToString("N")));
            string directory = Path.Combine(evidence, TestContext.CurrentContext.Test.ID, stage);
            Directory.CreateDirectory(directory);
            string trace = Path.Combine(directory, "lifetime.txt");
            File.WriteAllText(trace, _trace.ToString());
            TestContext.AddTestAttachment(trace, "Actual current-parent tooltip lifetime");
            Capture(Window, "main");
            Capture(TopLevel.GetTopLevel(Owner), "owner-parent");
            Capture(TopLevel.GetTopLevel(Owner.GetValue(ToolTipDiagnostics.ToolTipProperty) as ToolTip), "owner-tooltip");
            Capture(TopLevel.GetTopLevel(CurrentMenu.Popup.Child), "child-menu");
            await TestContext.Out.WriteLineAsync(_trace.ToString());

            void Capture(TopLevel? surface, string name)
            {
                using WriteableBitmap? frame = surface?.CaptureRenderedFrame();
                if (frame is not null)
                {
                    string path = Path.Combine(directory, name + ".png");
                    frame.Save(path, PngBitmapEncoderOptions.Default);
                    TestContext.AddTestAttachment(path, "Actual " + name + " at tooltip failure");
                }
            }
        }

        private void Trace(string stage)
            => _trace.AppendLine($"{_elapsed.Elapsed.TotalMilliseconds:F1} {stage}: pointer={Owner.IsPointerOver}; hint={ToolTip.GetIsOpen(Owner)}; tipRoot={TopLevel.GetTopLevel(Tip)?.GetType().Name}; service={ToolTip.GetServiceEnabled(Owner)}; epoch={Strip.GetToolTipHoverEpoch(Owner)}; logical={Owner.Parent?.GetType().Name}; current={Strip.GetCurrentParent(Owner)?.GetType().Name}; visual={Owner.GetVisualParent()?.GetType().Name}; input={TopLevel.GetTopLevel(Owner)?.GetType().Name}; bounds={Owner.Bounds}; menu={Menu.IsOpen}; overflow={Strip.IsOverflowOpen}; visible={Owner.IsEffectivelyVisible}; enabled={Owner.IsEffectivelyEnabled}");

        private void TraceHit(TopLevel parent, Point point)
        {
            IInputElement? actual = parent.InputHitTest(point);
            IInputElement? underneath = parent.InputHitTest(point, visual => visual.GetType().Name != "LightDismissOverlayLayer");
            IInputElement? passThrough = Menu.GetValue(PopupFlyoutBase.OverlayInputPassThroughElementProperty);
            _trace.AppendLine($"  hit={actual?.GetType().Name}; underneath={underneath?.GetType().Name}; underneathIsOwner={ReferenceEquals(underneath, Owner)}; descendant={Owner.IsVisualAncestorOf(underneath as Visual)}; captured={_pointer?.Captured?.GetType().Name}; capturedIsHit={ReferenceEquals(_pointer?.Captured, actual)}; flyoutPassThrough={passThrough?.GetType().Name}; popupPassThrough={Menu.Popup.OverlayInputPassThroughElement?.GetType().Name}; point={point}");
            foreach (Control layer in parent.GetVisualDescendants().OfType<Control>().Where(control => control.GetType().Name == "LightDismissOverlayLayer"))
            {
                // The framework's layer is internal. Reflection is confined to this
                // diagnostic consumer, never the shipping popup or tooltip adapter.
                object? layerPassThrough = layer.GetType().GetProperty("InputPassThroughElement")?.GetValue(layer);
                Point? local = parent.TranslatePoint(point, layer);
                Point? origin = layer.TranslatePoint(default, parent);
                InputElement root = layer.GetPresentationSource()?.RootVisual as InputElement ?? parent;
                IInputElement? localHit = local is Point localPoint
                    ? root.InputHitTest(localPoint, visual => !ReferenceEquals(visual, layer)) : null;
                _trace.AppendLine($"  layer visible={layer.IsVisible}/{layer.IsEffectivelyVisible}; bounds={layer.Bounds}; origin={origin}; localPoint={local}; root={root.GetType().Name}; passThrough={layerPassThrough?.GetType().Name}; passIsOwner={ReferenceEquals(layerPassThrough, Owner)}; passAncestorOfUnderlying={(layerPassThrough as Visual).IsVisualAncestorOf(underneath as Visual)}; localRehit={localHit?.GetType().Name}; passAncestorOfLocalRehit={(layerPassThrough as Visual).IsVisualAncestorOf(localHit as Visual)}");
            }
        }

        internal void NarrowAndOpenOverflow()
        {
            Strip.Width = 180;
            Settle();
            Strip.GetItemPlacement(Owner).Should().Be(NativeToolStripItemPlacement.Overflow);
            OpenOverflow();
            Owner.Bounds.Size.Should().Be(new Size(120, 22), "the real source probe gives this retained owner an explicit 120x22 item size");
        }

        internal void OpenOverflow()
        {
            Point target = Strip.OverflowButton.TranslatePoint(new Rect(Strip.OverflowButton.Bounds.Size).Center, Window)!.Value;
            Window.MouseMove(target);
            Window.MouseDown(target, MouseButton.Left);
            Window.MouseUp(target, MouseButton.Left);
            Settle();
            Strip.IsOverflowOpen.Should().BeTrue();
            Owner.GetVisualParent().Should().BeSameAs(Strip.OverflowContent);
        }
    }
}
