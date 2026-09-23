using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GitCommands;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.Compat;
using GitUI.UserControls;
using Microsoft.VisualStudio.Threading;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class FormBrowseToolbarTests
{
    [SetUp]
    public void SetUp()
        => ThreadHelper.JoinableTaskContext = new JoinableTaskContext();

    [AvaloniaTest]
    public void Dynamic_split_button_menu_should_render_items_populated_on_each_opening()
    {
        MenuFlyout flyout = new();
        IconSplitButton button = new() { Flyout = flyout };
        Window window = new() { Content = button };
        int generation = 0;
        flyout.Opening += (_, _) =>
        {
            flyout.Items.Clear();
            flyout.Items.Add(new MenuItem { Header = $"Branch {++generation}" });
        };
        window.Show();
        try
        {
            for (int opening = 1; opening <= 2; opening++)
            {
                button.ShowDropDown();
                Dispatcher.UIThread.RunJobs();
                MenuItem item = flyout.Items.OfType<MenuItem>().Single();
                item.Header.Should().Be($"Branch {opening}");
                TopLevel.GetTopLevel(item).Should().NotBeNull();
                flyout.Hide();
            }
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Filter_toolbar_visibility_should_persist_the_original_group_and_restore_all_its_controls()
    {
        const string key = "formbrowse_toolbar_visibility_ToolBar_group:Branch filter";
        bool? previous = AppSettings.GetBool(key);
        try
        {
            AppSettings.SetBool(key, null);
            FilterToolBar toolbar = new();
            Menu mainMenu = new();
            MenuItem repositoryMenu = new();
            mainMenu.Items.Add(repositoryMenu);
            using FormBrowseMenus menus = new(mainMenu);
            menus.CreateToolbarsMenus((toolbar, "Filters"));
            MenuItem toolbarMenu = menus.ToolStripContextMenu.Items.OfType<MenuItem>().Single();
            MenuItem group = toolbarMenu.Items.OfType<MenuItem>()
                .Single(item => item.Tag is Control { Name: "toolStripLabel1" });
            Control[] controls =
            [
                toolbar.FindControl<Control>("toolStripLabel1")!,
                toolbar.FindControl<Control>("tscboBranchFilter")!,
                toolbar.FindControl<Control>("tsddbtnBranchFilter")!,
            ];

            group.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

            controls.Should().OnlyContain(control => !control.IsVisible);
            AppSettings.GetBool(key).Should().BeFalse();
            toolbarMenu.Items.OfType<MenuItem>()
                .Where(item => item.Tag is Control { Name: "tscboBranchFilter" or "tsddbtnBranchFilter" })
                .Should().BeEmpty();

            FilterToolBar reopened = new();
            menus.CreateToolbarsMenus((reopened, "Reopened"));
            reopened.FindControl<Control>("tscboBranchFilter")!.IsVisible.Should().BeFalse();
            reopened.FindControl<Control>("tsddbtnBranchFilter")!.IsVisible.Should().BeFalse();

            group.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            controls.Should().OnlyContain(control => control.IsVisible);
            AppSettings.GetBool(key).Should().BeNull();
        }
        finally
        {
            AppSettings.SetBool(key, previous);
        }
    }

    [AvaloniaTest]
    public void Browse_menu_commands_should_share_state_and_unregister_only_their_own_presentations()
    {
        bool enabled = true;
        bool selected = false;
        MenuCommand command = new()
        {
            Name = "TestCommand",
            Text = "Toggle selection",
            IsCheckedFunc = () => selected,
            IsEnabledFunc = () => enabled,
            ExecuteAction = () => selected = !selected,
        };
        MenuItem contextItem = (MenuItem)MenuCommand.CreateToolStripItem(command);
        command.RegisterMenuItem(contextItem);
        Menu mainMenu = new();
        MenuItem repositoryMenu = new();
        mainMenu.Items.Add(repositoryMenu);
        using FormBrowseMenus menus = new(mainMenu);
        menus.AddMenuCommandSet(MainMenuItem.NavigateMenu, []);
        menus.AddMenuCommandSet(MainMenuItem.ViewMenu, [command]);
        menus.InsertRevisionGridMainMenuItems(repositoryMenu);
        MenuItem mainItem = menus.ViewMenuItem.Items.OfType<MenuItem>().Single();

        mainItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

        selected.Should().BeTrue();
        mainItem.IsChecked.Should().BeTrue();
        contextItem.IsChecked.Should().BeTrue();

        enabled = false;
        menus.OnMenuCommandsPropertyChanged();
        mainItem.IsEnabled.Should().BeFalse();
        contextItem.IsEnabled.Should().BeFalse();

        menus.ResetMenuCommandSets();
        enabled = true;
        selected = false;
        command.SetCheckForRegisteredMenuItems();
        contextItem.IsEnabled.Should().BeTrue();
        contextItem.IsChecked.Should().BeFalse();
        mainItem.IsEnabled.Should().BeFalse();
        mainItem.IsChecked.Should().BeTrue();
        mainMenu.Items.Should().ContainSingle().Which.Should().BeSameAs(repositoryMenu);
    }

    [AvaloniaTest]
    public void Toolbar_separators_should_hide_at_the_edges_and_between_hidden_items()
    {
        Menu mainMenu = new();
        MenuItem repositoryMenu = new();
        mainMenu.Items.Add(repositoryMenu);
        using FormBrowseMenus menus = new(mainMenu);
        Border leading = Separator();
        Border middle = Separator();
        Border duplicate = Separator();
        Border trailing = Separator();
        Button first = new() { Name = "ParityToolbarFirst" };
        Button second = new() { Name = "ParityToolbarSecond" };
        StackPanel toolbar = new() { Children = { leading, first, middle, duplicate, second, trailing } };
        const string key = "formbrowse_toolbar_visibility_ParityToolbarFirst";
        bool? previous = AppSettings.GetBool(key);
        try
        {
            AppSettings.SetBool(key, null);
            menus.CreateToolbarsMenus((toolbar, "Test toolbar"));
            leading.IsVisible.Should().BeFalse();
            middle.IsVisible.Should().BeTrue();
            duplicate.IsVisible.Should().BeFalse();
            trailing.IsVisible.Should().BeFalse();

            MenuItem toolbarMenu = menus.ToolStripContextMenu.Items.OfType<MenuItem>().Single();
            MenuItem item = toolbarMenu.Items.OfType<MenuItem>().Single(item => ReferenceEquals(item.Tag, first));
            item.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

            middle.IsVisible.Should().BeFalse();
            duplicate.IsVisible.Should().BeFalse();
            second.IsVisible.Should().BeTrue();

            item.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            middle.IsVisible.Should().BeTrue();
        }
        finally
        {
            AppSettings.SetBool(key, previous);
        }

        static Border Separator() => new() { Classes = { "gitextensions-toolbar-separator" } };
    }
}
