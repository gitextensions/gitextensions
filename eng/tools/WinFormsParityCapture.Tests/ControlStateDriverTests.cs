using AwesomeAssertions;
using GitExtensions.ParityCapture;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

[TestFixture]
[Category("P0_1")]
public sealed class ControlStateDriverTests
{
    [TestCase(false)]
    [TestCase(true)]
    [Apartment(ApartmentState.STA)]
    public void Apply_should_resolve_a_nested_controls_private_menu_and_honor_cancellation(bool cancel)
    {
        using ContextMenuStrip menu = new();
        menu.Items.Add("Action");
        int openingCount = 0;
        menu.Opening += (_, e) =>
        {
            openingCount++;
            e.Cancel = cancel;
        };
        using Form form = new() { ClientSize = new Size(300, 200) };
        using Panel panel = new() { Dock = DockStyle.Fill };
        using MenuOwner owner = new(menu) { Dock = DockStyle.Fill };
        panel.Controls.Add(owner);
        form.Controls.Add(panel);
        form.Show();
        CaptureStatePlan state = new() { Id = "nested-menu.open", Kind = CaptureStateKind.MenuOpen, TargetField = "_menu" };
        if (cancel)
        {
            Action capture = () => ControlStateDriver.Apply(form, state);
            capture.Should().Throw<CaptureStateUnsupportedException>().WithMessage("*declined to open*");
            menu.Visible.Should().BeFalse();
        }
        else
        {
            using ControlStateDriver driver = ControlStateDriver.Apply(form, state);
            driver.Popups.Should().ContainSingle().Which.Should().BeSameAs(menu);
            menu.Visible.Should().BeTrue();
        }

        openingCount.Should().Be(1);
    }

    [Test]
    public void Apply_should_resize_and_restore_the_client_surface()
    {
        using Form form = new() { ClientSize = new Size(240, 140) };
        form.Show();
        CaptureStatePlan state = new()
        {
            Id = "resized",
            Kind = CaptureStateKind.Normal,
            WidthDip = 340,
            HeightDip = 220
        };

        using (ControlStateDriver.Apply(form, state))
        {
            form.ClientSize.Should().Be(new Size(340, 220));
        }

        form.ClientSize.Should().Be(new Size(240, 140));
    }

    [Test]
    [Apartment(ApartmentState.STA)]
    [Category("P8_6i")]
    public void Apply_should_activate_a_tab_page_when_the_page_is_the_focus_target()
    {
        using Form form = new() { ClientSize = new Size(240, 140) };
        using TabControl tabs = new() { Dock = DockStyle.Fill };
        TabPage first = new() { Name = "FirstTab", Text = "First" };
        TabPage second = new() { Name = "SecondTab", Text = "Second" };
        tabs.TabPages.AddRange([first, second]);
        form.Controls.Add(tabs);
        form.Show();
        tabs.SelectedTab.Should().BeSameAs(first);

        using (ControlStateDriver.Apply(
                   form,
                   new CaptureStatePlan
                   {
                       Id = "second.focused",
                       Kind = CaptureStateKind.Focus,
                       TargetField = second.Name
                   }))
        {
            tabs.SelectedTab.Should().BeSameAs(second);
        }

        tabs.SelectedTab.Should().BeSameAs(first);
    }

    [Test]
    [Apartment(ApartmentState.STA)]
    [Category("P8_6i")]
    public void Apply_should_deliver_hover_to_the_visible_child_under_a_composite_target()
    {
        using Form form = new() { ClientSize = new Size(240, 140) };
        using Panel composite = new() { Name = "composite", Dock = DockStyle.Fill };
        using Panel child = new() { Dock = DockStyle.Fill };
        bool childMoved = false;
        child.MouseMove += (_, _) => childMoved = true;
        composite.Controls.Add(child);
        form.Controls.Add(composite);
        form.Show();

        using ControlStateDriver driver = ControlStateDriver.Apply(
            form,
            new CaptureStatePlan
            {
                Id = "composite.hover",
                Kind = CaptureStateKind.Hover,
                TargetField = composite.Name
            });

        childMoved.Should().BeTrue();
    }

    [Test]
    [Apartment(ApartmentState.STA)]
    [Category("P8_6i")]
    public void Apply_should_drive_toolbar_item_hover_through_its_owner_window()
    {
        using Form form = new() { ClientSize = new Size(240, 140) };
        using ToolStrip toolbar = new() { Dock = DockStyle.Top };
        ToolStripButton button = new() { Name = "button", Text = "Commit" };
        bool pointerReachedItem = false;
        button.MouseEnter += (_, _) => pointerReachedItem = true;
        toolbar.Items.Add(button);
        form.Controls.Add(toolbar);
        form.Show();

        using ControlStateDriver driver = ControlStateDriver.Apply(
            form,
            new CaptureStatePlan
            {
                Id = "toolbar.hover",
                Kind = CaptureStateKind.Hover,
                TargetField = button.Name
            });

        pointerReachedItem.Should().BeTrue();
        button.Selected.Should().BeTrue();
    }

    [TestCase(false)]
    [TestCase(true)]
    [Apartment(ApartmentState.STA)]
    [Category("P8_6i")]
    public void Apply_should_allow_actual_DropDown_population_and_restore_genuinely_empty_popups(bool populateOnOpening)
    {
        using Form form = new() { ClientSize = new Size(300, 200) };
        using ComboBox combo = new() { Name = "lazyCombo", Width = 200 };
        int openingCount = 0;
        combo.DropDown += (_, _) =>
        {
            openingCount++;
            if (populateOnOpening)
            {
                combo.Items.AddRange(["feature/filter", "main"]);
            }
        };
        form.Controls.Add(combo);
        form.Show();
        combo.IsHandleCreated.Should().BeTrue();
        combo.Items.Cast<object>().Should().BeEmpty();
        combo.DroppedDown.Should().BeFalse();
        CaptureStatePlan state = new()
        {
            Id = "lazy-combo.open",
            Kind = CaptureStateKind.MenuOpen,
            TargetField = combo.Name,
        };
        if (populateOnOpening)
        {
            using (ControlStateDriver driver = ControlStateDriver.Apply(form, state))
            {
                combo.DroppedDown.Should().BeTrue();
                combo.Items.Cast<string>().Should().Equal("feature/filter", "main");
                ComboBoxPopup popup = driver.ComboBoxPopups.Should().ContainSingle().Which;
                popup.Owner.Should().BeSameAs(combo);
                popup.Bounds.Width.Should().BeGreaterThan(0);
                popup.Bounds.Height.Should().BeGreaterThan(0);
            }

            combo.DroppedDown.Should().BeFalse();
        }
        else
        {
            Action capture = () => ControlStateDriver.Apply(form, state);
            capture.Should().Throw<CaptureStateUnsupportedException>().WithMessage("*remained unpopulated after opening*");
            combo.Items.Cast<object>().Should().BeEmpty();
            combo.DroppedDown.Should().BeFalse();
        }

        openingCount.Should().Be(1, "the driver must observe the real native DropDown event before deciding whether the lazy list is supported");
    }

    [TestCase(false)]
    [TestCase(true)]
    [Apartment(ApartmentState.STA)]
    public void Apply_should_open_the_real_popup_before_hovering_its_declared_row(bool context)
    {
        using ContextMenuStrip contextMenu = new();
        using MenuStrip strip = new();
        using ToolStripMenuItem owner = new("Owner");
        using ToolStripMenuItem row = new("Action");
        using HoverRowHost form = new(row) { ClientSize = new Size(420, 300) };
        ToolStripDropDown popup;
        if (context)
        {
            contextMenu.Items.Add(row);
            form.ContextMenuStrip = contextMenu;
            popup = contextMenu;
        }
        else
        {
            owner.DropDownItems.Add(row);
            strip.Items.Add(owner);
            form.Controls.Add(strip);
            popup = owner.DropDown;
        }

        int openings = 0;
        popup.Opening += (_, _) => openings++;
        form.Show();
        using (ControlStateDriver driver = ControlStateDriver.Apply(form,
                   new CaptureStatePlan { Id = "row.hover", Kind = CaptureStateKind.Hover, TargetField = "_hoverRow" }))
        {
            popup.Visible.Should().BeTrue();
            row.Selected.Should().BeTrue();
            driver.Popups.Should().ContainSingle().Which.Should().BeSameAs(popup);
            openings.Should().Be(1);
        }

        popup.Visible.Should().BeFalse();
    }

    [TestCase(false)]
    [TestCase(true)]
    [Apartment(ApartmentState.STA)]
    public void Apply_should_close_opened_popup_ancestors_and_restore_size_when_the_hover_row_is_hidden(bool context)
    {
        using ContextMenuStrip contextMenu = new();
        using MenuStrip strip = new();
        using ToolStripMenuItem owner = new("Owner");
        using ToolStripMenuItem row = new("Hidden action") { Available = false };
        using ToolStripMenuItem visibleRow = new("Visible action");
        using HoverRowHost form = new(row) { ClientSize = new Size(420, 300) };
        ToolStripDropDown popup;
        if (context)
        {
            contextMenu.Items.AddRange([visibleRow, row]);
            form.ContextMenuStrip = contextMenu;
            popup = contextMenu;
        }
        else
        {
            owner.DropDownItems.AddRange([visibleRow, row]);
            strip.Items.Add(owner);
            form.Controls.Add(strip);
            popup = owner.DropDown;
        }

        int openings = 0;
        popup.Opening += (_, _) => openings++;
        form.Show();
        Action capture = () => ControlStateDriver.Apply(form, new CaptureStatePlan
        {
            Id = "hidden-row.hover",
            Kind = CaptureStateKind.Hover,
            TargetField = "_hoverRow",
            WidthDip = 640,
            HeightDip = 480,
        });

        capture.Should().Throw<CaptureStateUnsupportedException>().WithMessage("*hover state requires a visible item*");

        openings.Should().Be(1, "the target must fail after its actual popup owner opened");
        popup.Visible.Should().BeFalse();
        form.ClientSize.Should().Be(new Size(420, 300));
    }

    [TestCase(CaptureStateKind.MenuOpen)]
    [TestCase(CaptureStateKind.MenuOpenHoveredOwner)]
    [Apartment(ApartmentState.STA)]
    public void Apply_should_preserve_the_declared_menu_owner_pointer_route(CaptureStateKind kind)
    {
        Point originalCursor = NativeMethods.GetCursorPosition();
        using MenuStrip strip = new();
        using ToolStripMenuItem owner = new("Owner");
        owner.DropDownItems.Add("Action");
        strip.Items.Add(owner);
        using HoverRowHost form = new(owner) { ClientSize = new Size(420, 300) };
        form.Controls.Add(strip);
        form.Show();
        try
        {
            Point neutralCursor = form.PointToScreen(new Point(form.ClientSize.Width - 10, form.ClientSize.Height - 10));
            NativeMethods.SetCursorPosition(neutralCursor);
            Point ownerCursor = strip.PointToScreen(new Point(
                owner.Bounds.Left + (owner.Bounds.Width / 2),
                owner.Bounds.Top + (owner.Bounds.Height / 2)));

            using (ControlStateDriver driver = ControlStateDriver.Apply(form,
                       new CaptureStatePlan { Id = "owner.open", Kind = kind, TargetField = "_hoverRow" }))
            {
                owner.DropDown.Visible.Should().BeTrue();
                NativeMethods.GetCursorPosition().Should().Be(
                    kind == CaptureStateKind.MenuOpenHoveredOwner ? ownerCursor : neutralCursor);
                driver.Popups.Should().ContainSingle().Which.Should().BeSameAs(owner.DropDown);
            }

            owner.DropDown.Visible.Should().BeFalse();
            NativeMethods.GetCursorPosition().Should().Be(neutralCursor);
        }
        finally
        {
            NativeMethods.SetCursorPosition(originalCursor);
        }
    }

    [Test]
    [Apartment(ApartmentState.STA)]
    public void Apply_should_restore_the_cursor_when_hover_succeeds_but_the_requested_menu_is_unsupported()
    {
        Point originalCursor = NativeMethods.GetCursorPosition();
        using Form form = new() { ClientSize = new Size(420, 300) };
        form.Show();
        Action capture = () => ControlStateDriver.Apply(form, new CaptureStatePlan
        {
            Id = "unsupported-owner.open",
            Kind = CaptureStateKind.MenuOpenHoveredOwner,
            WidthDip = 640,
            HeightDip = 480,
        });

        capture.Should().Throw<CaptureStateUnsupportedException>().WithMessage("*open-menu state requires*");

        NativeMethods.GetCursorPosition().Should().Be(originalCursor);
        form.ClientSize.Should().Be(new Size(420, 300));
    }

    private sealed class HoverRowHost(ToolStripMenuItem row) : Form
    {
        private readonly ToolStripMenuItem _hoverRow = row;
    }

    private sealed class MenuOwner : UserControl
    {
        private readonly ContextMenuStrip _menu;

        public MenuOwner(ContextMenuStrip menu)
        {
            _menu = menu;
            ContextMenuStrip = _menu;
        }
    }
}
