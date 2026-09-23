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
