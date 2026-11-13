using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitUI.Compat;
using GitUI.Compat.WinFormsControls;

namespace GitExtensionsTests;

[TestFixture]
public sealed class MenuMnemonicTests
{
    [AvaloniaTest]
    [TestCase("context", true)]
    [TestCase("strip", true)]
    [TestCase("flyout", true)]
    [TestCase("directedFlyout", true)]
    [TestCase("context", false)]
    [TestCase("strip", false)]
    [TestCase("flyout", false)]
    [TestCase("directedFlyout", false)]
    public void Menu_mnemonics_should_open_submenus_check_and_click_once_then_close(string kind, bool marked)
    {
        int clicks = 0;
        MenuItem leaf = new() { Header = marked ? "_1: entry" : "1: entry", ToggleType = MenuItemToggleType.CheckBox };
        leaf.Click += (_, _) =>
        {
            leaf.IsChecked.Should().BeTrue("native activation checks the item before Click");
            clicks++;
        };
        MenuItem submenu = new() { Header = marked ? "_Open" : "Open", ItemsSource = new[] { leaf } };
        Button owner = new() { Content = "Open menu", Width = 140, Height = 32 };
        MenuBase? menu = null;
        MenuItem? tools = null;
        MenuFlyout? flyout = null;
        Control content = owner;
        switch (kind)
        {
            case "context":
                owner.ContextMenu = new ContextMenuStrip { ItemsSource = new[] { submenu } };
                menu = owner.ContextMenu;
                break;
            case "strip":
                tools = new MenuItem { Header = "_Tools", ItemsSource = new[] { submenu } };
                menu = new MenuStrip { ItemsSource = new[] { tools } };
                content = new StackPanel { Children = { menu, owner } };
                break;
            default:
                flyout = kind == "flyout" ? new WinFormsMenuFlyout() : new NativeToolStripDropDownMenuFlyout();
                flyout.Items.Add(submenu);
                owner.Flyout = flyout;
                break;
        }

        Window window = new() { Width = 360, Height = 240, Content = content };
        window.Show();
        try
        {
            Control trigger = tools ?? (Control)owner;
            Point point = trigger.TranslatePoint(new Point(trigger.Bounds.Width / 2, trigger.Bounds.Height / 2), window)!.Value;
            MouseButton button = kind == "context" ? MouseButton.Right : MouseButton.Left;
            window.MouseMove(point);
            window.MouseDown(point, button);
            window.MouseUp(point, button);
            Dispatcher.UIThread.RunJobs();
            MenuBase actualMenu = menu ?? submenu.GetLogicalAncestors().OfType<MenuBase>().Single();
            (flyout?.IsOpen ?? actualMenu.IsOpen).Should().BeTrue();
            if (tools is not null)
            {
                tools.IsSubMenuOpen.Should().BeTrue("the real pointer must open the menu strip's dropdown first");
            }

            Type(window, Key.O, PhysicalKey.O, "o");

            submenu.IsSubMenuOpen.Should().BeTrue("a unique mnemonic opens without Enter");
            clicks.Should().Be(0);
            Type(window, Key.D1, PhysicalKey.Digit1, "1");

            clicks.Should().Be(1);
            leaf.IsChecked.Should().BeTrue();
            (flyout?.IsOpen ?? actualMenu.IsOpen).Should().BeFalse("native leaf activation dismisses the owning menu");
        }
        finally
        {
            menu?.Close();
            flyout?.Hide();
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(true)]
    [TestCase(false)]
    public void Context_mnemonics_should_cycle_duplicates_and_ignore_escaped_hidden_and_disabled_items(bool marked)
    {
        MenuItem first = new() { Header = marked ? "_Export" : "Export" };
        MenuItem second = new() { Header = marked ? "_Edit" : "Edit" };
        MenuItem disabled = new() { Header = marked ? "_Erase" : "Erase", IsEnabled = false };
        MenuItem hidden = new() { Header = marked ? "_Empty" : "Empty", IsVisible = false };
        MenuItem escaped = new() { Header = "__External" };
        MenuItem other = new() { Header = marked ? "Extra" : "_Zebra" };
        MenuItem empty = new() { Header = string.Empty };
        MenuItem[] matches = [first, second];
        List<MenuItem> clicks = [];
        foreach (MenuItem item in new[] { first, second, disabled, hidden, escaped, other, empty })
        {
            item.Click += (_, _) => clicks.Add(item);
        }

        ContextMenuStrip menu = new() { ItemsSource = new[] { first, second, disabled, hidden, escaped, other, empty } };
        Button owner = new() { Content = "Open menu", ContextMenu = menu };
        Window window = new() { Width = 360, Height = 240, Content = owner };
        window.Show();
        try
        {
            Point point = owner.TranslatePoint(new Point(owner.Bounds.Width / 2, owner.Bounds.Height / 2), window)!.Value;
            window.MouseDown(point, MouseButton.Right);
            window.MouseUp(point, MouseButton.Right);
            menu.IsOpen.Should().BeTrue();
            int selected = Array.IndexOf(matches, menu.SelectedItem as MenuItem);
            for (int attempt = 1; attempt <= 3; attempt++)
            {
                Type(window, Key.E, PhysicalKey.E, "e");
                menu.SelectedItem.Should().BeSameAs(matches[(selected + attempt) % matches.Length]);
                clicks.Should().BeEmpty("duplicate mnemonics select until Enter confirms one");
                menu.IsOpen.Should().BeTrue();
            }

            MenuItem expected = (MenuItem)menu.SelectedItem!;
            window.KeyPress(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, keySymbol: null);
            window.KeyRelease(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, keySymbol: null);
            clicks.Should().ContainSingle().Which.Should().BeSameAs(expected);
            menu.IsOpen.Should().BeFalse();
        }
        finally
        {
            menu.Close();
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Alt_mnemonics_should_check_and_click_once_when_the_menu_stays_open()
    {
        MenuItem choice = new() { Header = "_Choice", ToggleType = MenuItemToggleType.CheckBox, StaysOpenOnClick = true };
        int clicks = 0;
        choice.Click += (_, _) => clicks++;
        ContextMenuStrip menu = new() { ItemsSource = new[] { choice } };
        Button owner = new() { Content = "Open menu", ContextMenu = menu };
        Window window = new() { Width = 360, Height = 240, Content = owner };
        window.Show();
        try
        {
            Point point = owner.TranslatePoint(new Point(owner.Bounds.Width / 2, owner.Bounds.Height / 2), window)!.Value;
            window.MouseDown(point, MouseButton.Right);
            window.MouseUp(point, MouseButton.Right);
            menu.IsOpen.Should().BeTrue();

            Type(window, Key.C, PhysicalKey.C, "c", RawInputModifiers.Alt);

            clicks.Should().Be(1, "KeyDown and TextInput describe one Alt mnemonic activation");
            choice.IsChecked.Should().BeTrue();
            menu.IsOpen.Should().BeTrue();
            Type(window, Key.C, PhysicalKey.C, "c");
            clicks.Should().Be(2);
            choice.IsChecked.Should().BeFalse();
            menu.IsOpen.Should().BeTrue();
        }
        finally
        {
            menu.Close();
            window.Close();
        }
    }

    private static void Type(Window window, Key key, PhysicalKey physicalKey, string text, RawInputModifiers modifiers = RawInputModifiers.None)
    {
        window.KeyPress(key, modifiers, physicalKey, text);
        window.KeyTextInput(text);
        window.KeyRelease(key, RawInputModifiers.None, physicalKey, text);
    }
}
