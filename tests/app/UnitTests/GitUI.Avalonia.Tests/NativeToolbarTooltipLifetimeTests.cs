using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using GitUI.Compat;
using ToolTip = Avalonia.Controls.ToolTip;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class NativeToolbarTooltipLifetimeTests
{
    [AvaloniaTest]
    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public async Task Native_menu_should_retire_visible_and_pending_owner_tooltips_without_changing_their_configuration(bool split, bool pending)
    {
        using TooltipFixture fixture = new(split);
        ToolTip.SetShowDelay(fixture.Owner, 60);
        fixture.MoveToOwner();
        if (!pending)
        {
            ToolTip.SetIsOpen(fixture.Owner, true);
            fixture.Settle();
            fixture.AssertTipOpen();
        }

        fixture.Menu.ShowAt(fixture.Owner);
        fixture.Settle();
        fixture.Menu.IsOpen.Should().BeTrue();
        await Task.Delay(180);
        fixture.Settle();
        ToolTip.GetIsOpen(fixture.Owner).Should().BeFalse("a source toolbar owner must not leave or reopen its tooltip over the open menu");
        TopLevel.GetTopLevel(fixture.Tip).Should().BeNull();
        ToolTip.GetTip(fixture.Owner).Should().BeSameAs(fixture.Tip);
        ToolTip.GetShowDelay(fixture.Owner).Should().Be(60);
        ToolTip.GetServiceEnabled(fixture.Owner).Should().BeFalse("only the automatic owner service is temporarily suspended");
        fixture.Owner.GetBaseValue(ToolTip.ServiceEnabledProperty).Value.Should().BeTrue("the configured caller value must remain intact beneath the owned lease");

        fixture.Menu.Hide();
        ToolTip.GetServiceEnabled(fixture.Owner).Should().BeTrue();
        fixture.MoveAway();
        fixture.MoveToOwner();
        await WaitUntilAsync(() => ToolTip.GetIsOpen(fixture.Owner));
        fixture.AssertTipOpen();
        fixture.Menu.ShowAt(fixture.Owner);
        fixture.Settle();
        ToolTip.GetIsOpen(fixture.Owner).Should().BeFalse("reopening the menu must retire the newly shown tooltip too");
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Frozen_native_menu_should_preserve_explicit_tooltip_requests_as_the_original_Show_API_does(bool split)
    {
        using TooltipFixture fixture = new(split);
        fixture.Menu.ShowAt(fixture.Owner);
        fixture.Settle();
        ToolTip.SetIsOpen(fixture.Owner, true);
        fixture.Settle();
        fixture.AssertTipOpen();
        fixture.Menu.IsOpen.Should().BeTrue();
        ToolTip.GetServiceEnabled(fixture.Owner).Should().BeFalse();
        fixture.Menu.Hide();
        fixture.Settle();
        ToolTip.SetIsOpen(fixture.Owner, true);
        fixture.Settle();
        fixture.AssertTipOpen();
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Native_tooltip_guard_should_leave_non_opted_in_owners_unchanged(bool split)
    {
        using TooltipFixture fixture = new(split, native: false);
        fixture.Menu.ShowAt(fixture.Owner);
        fixture.Settle();
        ToolTip.SetIsOpen(fixture.Owner, true);
        fixture.Settle();
        fixture.AssertTipOpen();
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Native_layout_without_source_redraw_suspension_should_retain_configured_open_menu_summary_tooltips(bool split)
    {
        using TooltipFixture fixture = new(split, freeze: false);
        ToolTip.SetShowDelay(fixture.Owner, 75);
        ToolTip.SetIsOpen(fixture.Owner, true);
        fixture.Settle();
        fixture.Menu.ShowAt(fixture.Owner);
        fixture.Settle();
        fixture.Menu.IsOpen.Should().BeTrue();
        fixture.AssertTipOpen();
        ToolTip.GetTip(fixture.Owner).Should().BeSameAs(fixture.Tip);
        ToolTip.GetShowDelay(fixture.Owner).Should().Be(75);
        ToolTip.GetServiceEnabled(fixture.Owner).Should().BeTrue();
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Native_tooltip_guard_should_follow_flyout_replacement_and_opt_out_without_retaining_the_old_open_menu(bool split)
    {
        using TooltipFixture fixture = new(split);
        fixture.Menu.ShowAt(fixture.Owner);
        fixture.Settle();
        MenuFlyout replacement = new() { Items = { new MenuItem { Header = "Replacement option" } } };
        fixture.SetFlyout(replacement);
        fixture.Settle();
        ToolTip.SetIsOpen(fixture.Owner, true);
        fixture.Settle();
        fixture.AssertTipOpen();
        fixture.Menu.Hide();
        replacement.ShowAt(fixture.Owner);
        fixture.Settle();
        ToolTip.GetIsOpen(fixture.Owner).Should().BeFalse();
        fixture.SetNative(false);
        ToolTip.SetIsOpen(fixture.Owner, true);
        fixture.Settle();
        fixture.AssertTipOpen();
        replacement.Hide();
    }

    [AvaloniaTest]
    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public void Retiring_a_tooltip_may_replace_or_opt_out_its_owner_without_leaving_a_stale_guard(bool split, bool optOut)
    {
        using TooltipFixture fixture = new(split);
        MenuFlyout replacement = new() { Items = { new MenuItem { Header = "Replacement option" } } };
        ToolTip.SetIsOpen(fixture.Owner, true);
        fixture.Settle();
        bool intercepted = false;
        EventHandler<AvaloniaPropertyChangedEventArgs> publication = (_, args) =>
        {
            if (!intercepted && args.Property == ToolTip.IsOpenProperty && !ToolTip.GetIsOpen(fixture.Owner))
            {
                intercepted = true;
                if (optOut)
                {
                    fixture.SetNative(false);
                }
                else
                {
                    fixture.SetFlyout(replacement);
                }
            }
        };
        fixture.Owner.PropertyChanged += publication;
        try
        {
            fixture.Menu.ShowAt(fixture.Owner);
            fixture.Settle();
            intercepted.Should().BeTrue("the actual tooltip close publication must exercise the synchronous lifetime change");
            ToolTip.SetIsOpen(fixture.Owner, true);
            fixture.Settle();
            fixture.AssertTipOpen();
            fixture.Menu.Hide();
            if (!optOut)
            {
                replacement.ShowAt(fixture.Owner);
                fixture.Settle();
                ToolTip.GetIsOpen(fixture.Owner).Should().BeFalse();
            }
        }
        finally
        {
            fixture.Owner.PropertyChanged -= publication;
            replacement.Hide();
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Source_redraw_suspension_opt_out_should_restore_the_configured_tooltip_during_the_same_open_menu(bool split)
    {
        using TooltipFixture fixture = new(split);
        fixture.Menu.ShowAt(fixture.Owner);
        fixture.Settle();
        ToolTip.GetServiceEnabled(fixture.Owner).Should().BeFalse();
        NativeToolStrip.SetFreezeDropDownOwnerPaint(fixture.Owner, false);
        ToolTip.GetServiceEnabled(fixture.Owner).Should().BeTrue();
        ToolTip.SetIsOpen(fixture.Owner, true);
        fixture.Settle();
        fixture.AssertTipOpen();
        ToolTip.GetTip(fixture.Owner).Should().BeSameAs(fixture.Tip);
        ToolTip.GetServiceEnabled(fixture.Owner).Should().BeTrue();
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Menu_tooltip_guard_must_not_clear_an_existing_caller_cancellation_after_menu_close(bool split)
    {
        using TooltipFixture fixture = new(split);
        EventHandler<CancelRoutedEventArgs> cancellation = (_, args) => args.Cancel = true;
        ToolTip.AddToolTipOpeningHandler(fixture.Owner, cancellation);
        try
        {
            fixture.Menu.ShowAt(fixture.Owner);
            fixture.Settle();
            fixture.Menu.Hide();
            fixture.Settle();
            ToolTip.SetIsOpen(fixture.Owner, true);
            ToolTip.GetIsOpen(fixture.Owner).Should().BeFalse("the owner-specific guard must never clear another tooltip opening handler's cancellation");
        }
        finally
        {
            ToolTip.RemoveToolTipOpeningHandler(fixture.Owner, cancellation);
        }

        ToolTip.SetIsOpen(fixture.Owner, true);
        fixture.Settle();
        fixture.AssertTipOpen();
    }

    [AvaloniaTest]
    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public async Task Owner_automatic_service_suspension_must_not_disable_popup_row_tooltips_or_replace_their_own_setting(bool split, bool disabledRow)
    {
        using TooltipFixture fixture = new(split);
        MenuItem row = fixture.Menu.Items.OfType<MenuItem>().Single();
        ToolTip rowTip = new() { Content = "Actual menu row hint" };
        ToolTip.SetTip(row, rowTip);
        ToolTip.SetShowDelay(row, 30);
        if (disabledRow)
        {
            ToolTip.SetServiceEnabled(row, false);
        }

        fixture.Menu.ShowAt(fixture.Owner);
        fixture.Settle();
        ToolTip.GetServiceEnabled(fixture.Owner).Should().BeFalse();
        ToolTip.GetServiceEnabled(row).Should().Be(!disabledRow, "the popup is a separate hover surface, and caller row overrides retain precedence");
        fixture.MoveAway();
        TopLevel popup = TopLevel.GetTopLevel(row) ?? throw new AssertionException("The actual menu row must be attached to its popup.");
        popup.MouseMove(row.TranslatePoint(new Rect(row.Bounds.Size).Center, popup)!.Value);
        await Task.Delay(120);
        fixture.Settle();
        ToolTip.GetIsOpen(row).Should().Be(!disabledRow);
        if (!disabledRow)
        {
            TopLevel.GetTopLevel(rowTip).Should().NotBeNull();
            rowTip.IsEffectivelyVisible.Should().BeTrue();
        }

        fixture.Menu.IsOpen.Should().BeTrue();
        fixture.Menu.Hide();
        ToolTip.SetIsOpen(row, false);
        fixture.Settle();
        ToolTip.GetServiceEnabled(fixture.Owner).Should().BeTrue();
        ToolTip.GetServiceEnabled(row).Should().Be(!disabledRow);
    }

    [AvaloniaTest]
    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public void Automatic_service_lease_must_preserve_a_live_caller_binding_and_publish_its_latest_value_after_close(bool split, bool enabled)
    {
        using TooltipFixture fixture = new(split);
        Border source = new();
        ToolTip.SetServiceEnabled(source, enabled);
        using IDisposable binding = fixture.Owner.Bind(ToolTip.ServiceEnabledProperty, source.GetObservable(ToolTip.ServiceEnabledProperty));
        ToolTip.GetServiceEnabled(fixture.Owner).Should().Be(enabled);
        fixture.Menu.ShowAt(fixture.Owner);
        fixture.Settle();
        ToolTip.GetServiceEnabled(fixture.Owner).Should().BeFalse();
        ToolTip.SetServiceEnabled(source, !enabled);
        fixture.Settle();
        ToolTip.GetServiceEnabled(fixture.Owner).Should().BeFalse();
        fixture.Owner.GetBaseValue(ToolTip.ServiceEnabledProperty).Value.Should().Be(!enabled);
        fixture.Menu.Hide();
        fixture.Settle();
        ToolTip.GetServiceEnabled(fixture.Owner).Should().Be(!enabled);
        ToolTip.SetServiceEnabled(source, enabled);
        fixture.Settle();
        ToolTip.GetServiceEnabled(fixture.Owner).Should().Be(enabled, "the caller binding must still be active, not replaced by a restored snapshot");
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Detaching_a_frozen_owner_must_release_its_automatic_service_lease(bool split)
    {
        using TooltipFixture fixture = new(split);
        fixture.Menu.ShowAt(fixture.Owner);
        fixture.Settle();
        ToolTip.GetServiceEnabled(fixture.Owner).Should().BeFalse();
        fixture.Window.Content = null;
        fixture.Settle();
        ToolTip.GetServiceEnabled(fixture.Owner).Should().BeTrue();
        fixture.Owner.GetBaseValue(ToolTip.ServiceEnabledProperty).Value.Should().BeTrue();
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        Stopwatch elapsed = Stopwatch.StartNew();
        while (elapsed.Elapsed < TimeSpan.FromSeconds(2))
        {
            Dispatcher.UIThread.RunJobs();
            if (condition())
            {
                return;
            }

            await Task.Delay(10);
        }

        throw new TimeoutException("The owner's ordinary tooltip service did not reopen after its menu closed.");
    }

    private sealed class TooltipFixture : IDisposable
    {
        internal TooltipFixture(bool split, bool native = true, bool freeze = true)
        {
            Owner = split
                ? new NativeToolStripSplitButton { UseNativeToolStripLayout = native, Content = "Source selector" }
                : new IconDropDownButton { UseNativeToolStripLayout = native, Content = "Source selector" };
            Owner.Width = 160;
            Owner.Height = 24;
            Owner.MinHeight = 0;
            Owner.Margin = default;
            NativeToolStrip.SetFreezeDropDownOwnerPaint(Owner, freeze);
            Tip = new ToolTip { Content = "Source owner tooltip" };
            ToolTip.SetTip(Owner, Tip);
            ToolTip.SetServiceEnabled(Owner, true);
            Menu = new MenuFlyout { Items = { new MenuItem { Header = "Source option" } } };
            SetFlyout(Menu);
            Window = new Window
            {
                Width = 500,
                Height = 300,
                Content = new Canvas { Background = Brushes.White, Children = { Owner } },
            };
            Window.Show();
            Settle();
        }

        internal Control Owner { get; }

        internal ToolTip Tip { get; }

        internal MenuFlyout Menu { get; }

        internal Window Window { get; }

        public void Dispose()
        {
            (Owner is NativeToolStripSplitButton split ? split.Flyout : ((IconDropDownButton)Owner).Flyout)?.Hide();
            Menu.Hide();
            ToolTip.SetIsOpen(Owner, false);
            Window.Close();
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

        internal void SetNative(bool native)
        {
            if (Owner is NativeToolStripSplitButton split)
            {
                split.UseNativeToolStripLayout = native;
            }
            else
            {
                ((IconDropDownButton)Owner).UseNativeToolStripLayout = native;
            }
        }

        internal void Settle()
        {
            Dispatcher.UIThread.RunJobs();
            Window.UpdateLayout();
        }

        internal void MoveToOwner()
        {
            Window.MouseMove(Owner.TranslatePoint(new Point(20, 10), Window)!.Value);
            Settle();
        }

        internal void MoveAway()
        {
            Window.MouseMove(new Point(450, 250));
            Settle();
        }

        internal void AssertTipOpen()
        {
            ToolTip.GetIsOpen(Owner).Should().BeTrue();
            TopLevel.GetTopLevel(Tip).Should().NotBeNull("the tooltip must have an actual popup host, not only an attached flag");
            Tip.IsEffectivelyVisible.Should().BeTrue();
        }
    }
}
