using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AwesomeAssertions;
using GitExtensions.ParityCapture;
using NUnit.Framework;
using Point = Avalonia.Point;

namespace GitExtensionsTests;

[TestFixture]
public sealed class PopupRowHoverCaptureTests
{
    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Apply_should_open_the_real_popup_before_hovering_its_declared_row(bool context)
    {
        MenuItem row = new() { Header = "Action" };
        MenuItem owner = new() { Header = "Owner", ItemsSource = new[] { row } };
        ContextMenu contextMenu = new();
        Border content = new();
        if (context)
        {
            owner.ItemsSource = null;
            contextMenu.Items.Add(row);
            content.ContextMenu = contextMenu;
        }

        HoverRowHost window = new(row)
        {
            Width = 420,
            Height = 300,
            Content = context ? content : new Menu { ItemsSource = new[] { owner } },
        };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            using (AvaloniaControlStateDriver driver = AvaloniaControlStateDriver.Apply(window,
                       new CaptureStatePlan { Id = "row.hover", Kind = CaptureStateKind.Hover, TargetField = "_hoverRow" }))
            {
                if (context)
                {
                    contextMenu.IsOpen.Should().BeTrue();
                }
                else
                {
                    owner.IsSubMenuOpen.Should().BeTrue();
                }

                row.IsPointerOver.Should().BeTrue();
                driver.PopupSurfaceRoots.Should().NotBeEmpty();
                window.CaptureRenderedFrame().Should().NotBeNull();
            }

            contextMenu.IsOpen.Should().BeFalse();
            owner.IsSubMenuOpen.Should().BeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Apply_should_close_opened_popup_ancestors_and_restore_size_when_the_hover_row_is_hidden(bool context)
    {
        MenuItem row = new() { Header = "Hidden action", IsVisible = false };
        MenuItem visibleRow = new() { Header = "Visible action" };
        MenuItem owner = new() { Header = "Owner", ItemsSource = new[] { visibleRow, row } };
        ContextMenu contextMenu = new();
        Border content = new();
        if (context)
        {
            owner.ItemsSource = null;
            contextMenu.Items.Add(visibleRow);
            contextMenu.Items.Add(row);
            content.ContextMenu = contextMenu;
        }

        int openings = 0;
        contextMenu.Opened += (_, _) => openings++;
        owner.SubmenuOpened += (_, _) => openings++;
        HoverRowHost window = new(row)
        {
            Width = 420,
            Height = 300,
            Content = context ? content : new Menu { ItemsSource = new[] { owner } },
        };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            Action capture = () => AvaloniaControlStateDriver.Apply(window, new CaptureStatePlan
            {
                Id = "hidden-row.hover",
                Kind = CaptureStateKind.Hover,
                TargetField = "_hoverRow",
                WidthDip = 640,
                HeightDip = 480,
            });

            capture.Should().Throw<AvaloniaCaptureStateUnsupportedException>().WithMessage("*hover state requires a visible Control*");

            openings.Should().BeGreaterThan(0, "the target must fail after its actual popup owner opened");
            contextMenu.IsOpen.Should().BeFalse();
            owner.IsSubMenuOpen.Should().BeFalse();
            window.Width.Should().Be(420);
            window.Height.Should().Be(300);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(CaptureStateKind.MenuOpen)]
    [TestCase(CaptureStateKind.MenuOpenHoveredOwner)]
    public void Apply_should_preserve_the_declared_menu_owner_pointer_route(CaptureStateKind kind)
    {
        MenuItem owner = new() { Header = "Owner", ItemsSource = new[] { new MenuItem { Header = "Action" } } };
        HoverRowHost window = new(owner)
        {
            Width = 420,
            Height = 300,
            Content = new Menu { ItemsSource = new[] { owner } },
        };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            Point pointer = owner.TranslatePoint(new Point(owner.Bounds.Width / 2, owner.Bounds.Height / 2), window)
                ?? throw new AssertionException("The actual menu title must have a pointer position in its owner.");
            window.MouseMove(pointer, RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();
            owner.IsPointerOver.Should().BeTrue();

            using (AvaloniaControlStateDriver driver = AvaloniaControlStateDriver.Apply(window,
                       new CaptureStatePlan { Id = "owner.open", Kind = kind, TargetField = "_hoverRow" }))
            {
                owner.IsSubMenuOpen.Should().BeTrue();
                owner.IsPointerOver.Should().Be(kind == CaptureStateKind.MenuOpenHoveredOwner);
                driver.PopupSurfaceRoots.Should().NotBeEmpty();
            }

            owner.IsSubMenuOpen.Should().BeFalse();
            owner.IsPointerOver.Should().BeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Apply_should_release_the_pointer_when_hover_succeeds_but_the_requested_menu_is_unsupported()
    {
        Window window = new() { Width = 420, Height = 300, Content = new Border() };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            Action capture = () => AvaloniaControlStateDriver.Apply(window, new CaptureStatePlan
            {
                Id = "unsupported-owner.open",
                Kind = CaptureStateKind.MenuOpenHoveredOwner,
                WidthDip = 640,
                HeightDip = 480,
            });

            capture.Should().Throw<AvaloniaCaptureStateUnsupportedException>().WithMessage("*open-menu state requires*");

            window.IsPointerOver.Should().BeFalse();
            window.Width.Should().Be(420);
            window.Height.Should().Be(300);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Apply_should_restore_the_menu_open_flag_when_the_owner_is_not_realized()
    {
        MenuItem owner = new()
        {
            Header = "Hidden owner",
            IsVisible = false,
            ItemsSource = new[] { new MenuItem { Header = "Action" } },
        };
        HoverRowHost window = new(owner)
        {
            Width = 420,
            Height = 300,
            Content = new Menu { ItemsSource = new[] { new MenuItem { Header = "Visible owner" }, owner } },
        };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            Action capture = () => AvaloniaControlStateDriver.Apply(window,
                new CaptureStatePlan { Id = "hidden-owner.open", Kind = CaptureStateKind.MenuOpen, TargetField = "_hoverRow" });

            capture.Should().Throw<AvaloniaCaptureStateUnsupportedException>().WithMessage("*not realized*");

            owner.IsSubMenuOpen.Should().BeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Apply_should_open_and_restore_the_actual_outer_to_inner_menu_owner_path_before_hovering_a_nested_row()
    {
        MenuItem row = new() { Header = "Nested action" };
        MenuItem inner = new() { Header = "Inner owner", ItemsSource = new[] { row } };
        MenuItem outer = new() { Header = "Outer owner", ItemsSource = new[] { inner } };
        HoverRowHost window = new(row)
        {
            Width = 640,
            Height = 480,
            Content = new Menu { ItemsSource = new[] { outer } },
        };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            using (AvaloniaControlStateDriver driver = AvaloniaControlStateDriver.Apply(window,
                       new CaptureStatePlan { Id = "nested-row.hover", Kind = CaptureStateKind.Hover, TargetField = "_hoverRow" }))
            {
                outer.IsSubMenuOpen.Should().BeTrue();
                inner.IsSubMenuOpen.Should().BeTrue();
                row.IsPointerOver.Should().BeTrue();
                driver.PopupSurfaceRoots.Should().HaveCount(2);
            }

            outer.IsSubMenuOpen.Should().BeFalse();
            inner.IsSubMenuOpen.Should().BeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    private sealed class HoverRowHost(MenuItem row) : Window
    {
        private readonly MenuItem _hoverRow = row;
    }
}
