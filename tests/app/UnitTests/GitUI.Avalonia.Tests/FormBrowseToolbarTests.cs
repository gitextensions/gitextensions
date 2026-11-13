using System.Globalization;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.Compat;
using GitUI.Hotkey;
using GitUI.UserControls;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using ResourceManager;
using ResourceManager.Hotkey;
using Image = Avalonia.Controls.Image;
using Keys = GitExtensions.Shims.WinForms.Keys;
using Point = Avalonia.Point;

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

    [AvaloniaTest]
    [Category("P8.6i.126")]
    [TestCase(GitPullAction.Merge)]
    [TestCase(GitPullAction.Rebase)]
    [TestCase(GitPullAction.Fetch)]
    [TestCase(GitPullAction.FetchAll)]
    [TestCase(GitPullAction.FetchPruneAll)]
    public void Pull_submenu_should_keep_each_source_Designer_icon(GitPullAction action)
    {
        using FormBrowse form = new();
        MenuItem source = PullSource(form, action);

        source.Icon.Should().BeOfType<Image>().Which.Source.Should().BeSameAs(PullIcon(action));
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    [TestCase(GitPullAction.None)]
    [TestCase(GitPullAction.Merge)]
    [TestCase(GitPullAction.Rebase)]
    [TestCase(GitPullAction.Fetch)]
    [TestCase(GitPullAction.FetchAll)]
    [TestCase(GitPullAction.FetchPruneAll)]
    public void Default_Pull_action_clone_should_keep_its_source_image_at_actual_menu_icon_size_and_allow_repeated_selection(
        GitPullAction action)
    {
        using PullMenuFixture fixture = new();
        fixture.Open();
        MenuItem source = PullSource(fixture.Form, action);
        MenuItem clone = fixture.Clone(action);
        Image sourceIcon = source.Icon.Should().BeOfType<Image>().Subject;
        Image cloneIcon = clone.Icon.Should().BeOfType<Image>().Subject;

        clone.Name.Should().Be($"{source.Name}SetDefault");
        clone.Header.Should().Be(source.Header);
        clone.Tag.Should().Be(action);
        clone.ToggleType.Should().Be(MenuItemToggleType.CheckBox,
            "the source CheckOnClick menu item renders a check mark rather than a radio dot");
        clone.StaysOpenOnClick.Should().BeTrue(
            "the source cancels ItemClicked closing on its default-action submenu");
        cloneIcon.Should().NotBeSameAs(sourceIcon, "Avalonia image controls cannot share visual ownership");
        cloneIcon.Source.Should().BeSameAs(PullIcon(action));
        cloneIcon.Source.Should().BeSameAs(sourceIcon.Source);
        cloneIcon.Bounds.Size.Should().Be(new Avalonia.Size(16, 16),
            "the source ToolStrip menu image allocation is sixteen pixels at its 96-DPI default");
        TopLevel.GetTopLevel(clone).Should().NotBeNull();
        fixture.Parent.IsSubMenuOpen.Should().BeTrue();
        fixture.Flyout.IsOpen.Should().BeTrue();
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Opened_default_Pull_action_submenu_should_apply_repeated_actual_clicks_without_closing_either_menu()
    {
        using PullMenuFixture fixture = new();
        fixture.Open();
        foreach (GitPullAction action in new[]
        {
            GitPullAction.None, GitPullAction.Merge, GitPullAction.Rebase,
            GitPullAction.Fetch, GitPullAction.FetchAll, GitPullAction.FetchPruneAll,
        })
        {
            MenuItem clone = fixture.Clone(action);
            for (int selection = 0; selection < 2; selection++)
            {
                fixture.Click(clone);

                AppSettings.DefaultPullAction.Should().Be(action);
                clone.IsChecked.Should().BeTrue();
                fixture.Parent.Items.OfType<MenuItem>().Where(item => !ReferenceEquals(item, clone))
                    .Should().OnlyContain(item => !item.IsChecked);
                fixture.Form.toolStripButtonPull.Icon.Should().BeSameAs(PullIcon(action));
                fixture.Parent.IsSubMenuOpen.Should().BeTrue();
                fixture.Flyout.IsOpen.Should().BeTrue();
            }
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Default_Pull_action_submenu_should_still_close_with_its_owner_after_actual_outside_input()
    {
        using PullMenuFixture fixture = new();
        fixture.Open();
        fixture.Click(fixture.Clone(GitPullAction.Fetch));
        fixture.ClickOutside();

        fixture.Parent.IsSubMenuOpen.Should().BeFalse();
        fixture.Flyout.IsOpen.Should().BeFalse(
            "the source cancels ItemClicked closing, not outside-menu closing");
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Default_Pull_action_submenu_should_still_close_on_actual_Escape_input()
    {
        using PullMenuFixture fixture = new();
        fixture.Open();
        MenuItem clone = fixture.Clone(GitPullAction.Fetch);
        fixture.Click(clone);
        clone.Focus().Should().BeTrue();
        TopLevel popup = TopLevel.GetTopLevel(clone)
            ?? throw new InvalidOperationException("The opened default-action submenu has no input root.");

        popup.KeyPress(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, null);
        fixture.Settle();

        fixture.Parent.IsSubMenuOpen.Should().BeFalse(
            "the source cancels ItemClicked closing but permits keyboard closing of the active submenu");
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Opened_Pull_popup_should_display_the_source_default_shortcut_without_adding_quick_action_or_clone_shortcuts()
    {
        using PullMenuFixture fixture = new();
        fixture.RefreshShortcuts();
        fixture.Open();
        MenuItem source = fixture.Form.pullToolStripMenuItem1;

        source.InputGesture.Should().Be(new KeyGesture(Key.Down, KeyModifiers.Control));
        WinFormsToolStripMenuSizer.GetShortcutDisplayString(source).Should().Be("Ctrl+Down");
        PullShortcutText(source).Text.Should().Be("Ctrl+Down",
            "the native hotkey display says Down, not Avalonia's automatic Down Arrow");
        AssertPullCaptionAndShortcutDoNotOverlap(source);
        foreach (GitPullAction action in new[]
        {
            GitPullAction.None, GitPullAction.Merge, GitPullAction.Rebase,
            GitPullAction.Fetch, GitPullAction.FetchAll, GitPullAction.FetchPruneAll,
        })
        {
            if (action != GitPullAction.None)
            {
                MenuItem quickAction = PullSource(fixture.Form, action);
                quickAction.InputGesture.Should().BeNull();
                WinFormsToolStripMenuSizer.GetShortcutDisplayString(quickAction).Should().BeNull();
                PullShortcutText(quickAction).Text.Should().BeNullOrEmpty();
            }

            MenuItem clone = fixture.Clone(action);
            clone.InputGesture.Should().BeNull();
            WinFormsToolStripMenuSizer.GetShortcutDisplayString(clone).Should().BeNull();
            PullShortcutText(clone).Text.Should().BeNullOrEmpty(
                "the source default-action clones copy Text and Image, not ShortcutKeyDisplayString");
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    [TestCase(Keys.Control | Keys.Shift | Keys.Up, "Ctrl+Shift+Up")]
    [TestCase(Keys.Control | Keys.Oemcomma, "Ctrl+,")]
    [TestCase(Keys.Shift | Keys.Home, "Shift+Home")]
    [TestCase(Keys.None, "")]
    public void Reopened_Pull_popup_should_refresh_source_shortcut_display_and_keep_customized_gestures_without_rebuilding(
        Keys customizedKeys, string expectedDisplay)
    {
        using PullMenuFixture fixture = new();
        fixture.RefreshShortcuts();
        fixture.Open();
        object?[] originalItems = fixture.Flyout.Items.ToArray();
        object?[] originalClones = fixture.Parent.Items.ToArray();
        fixture.Flyout.Hide();
        fixture.Settle();

        fixture.RefreshShortcuts(customizedKeys);
        fixture.Open();
        MenuItem source = fixture.Form.pullToolStripMenuItem1;
        KeyGesture? expectedGesture = KeysMapper.ToKeyGesture(customizedKeys);

        fixture.Flyout.Items.Should().Equal(originalItems);
        fixture.Parent.Items.Should().Equal(originalClones);
        source.InputGesture.Should().Be(expectedGesture);
        fixture.Form.pullToolStripMenuItem.InputGesture.Should().Be(expectedGesture,
            "both source Pull dialog presentations keep the same executable command gesture");
        WinFormsToolStripMenuSizer.GetShortcutDisplayString(source).Should().Be(
            string.IsNullOrEmpty(expectedDisplay) ? null : expectedDisplay);
        if (string.IsNullOrEmpty(expectedDisplay))
        {
            PullShortcutText(source).Text.Should().BeNullOrEmpty();
        }
        else
        {
            PullShortcutText(source).Text.Should().Be(expectedDisplay);
            AssertPullCaptionAndShortcutDoNotOverlap(source);
        }
    }

    private static TextBlock PullShortcutText(MenuItem item)
        => item.GetVisualDescendants().OfType<TextBlock>()
            .Single(text => text.Name == "PART_InputGestureText");

    private static void AssertPullCaptionAndShortcutDoNotOverlap(MenuItem item)
    {
        ContentPresenter header = item.GetVisualDescendants().OfType<ContentPresenter>()
            .Single(presenter => presenter.Name == "PART_HeaderPresenter");
        TextBlock shortcut = PullShortcutText(item);
        Point captionOrigin = header.TranslatePoint(default, item)
            ?? throw new InvalidOperationException("The opened Pull caption has no row coordinate.");
        Point shortcutOrigin = shortcut.TranslatePoint(default, item)
            ?? throw new InvalidOperationException("The opened Pull shortcut has no row coordinate.");
        FormattedText captionText = new(
            AvaloniaTranslationUtils.RemoveAvaloniaMnemonics(item.Header?.ToString() ?? string.Empty),
            CultureInfo.CurrentCulture,
            item.FlowDirection,
            new Typeface(item.FontFamily, item.FontStyle, item.FontWeight),
            item.FontSize,
            Brushes.Black);
        FormattedText shortcutText = new(
            shortcut.Text ?? string.Empty,
            CultureInfo.CurrentCulture,
            shortcut.FlowDirection,
            new Typeface(shortcut.FontFamily, shortcut.FontStyle, shortcut.FontWeight),
            shortcut.FontSize,
            Brushes.Black);

        // Compare live translated paint positions with the actual portable typography;
        // this is not a native popup-width or glyph-raster equivalence assertion.
        (captionOrigin.X + captionText.WidthIncludingTrailingWhitespace).Should().BeLessThanOrEqualTo(shortcutOrigin.X);
        (shortcutOrigin.X + shortcutText.WidthIncludingTrailingWhitespace).Should().BeLessThanOrEqualTo(item.Bounds.Width);
    }

    private static IImage PullIcon(GitPullAction action)
        => action switch
        {
            GitPullAction.None => GitUI.Properties.Images.Pull,
            GitPullAction.Merge => GitUI.Properties.Images.PullMerge,
            GitPullAction.Rebase => GitUI.Properties.Images.PullRebase,
            GitPullAction.Fetch => GitUI.Properties.Images.PullFetch,
            GitPullAction.FetchAll => GitUI.Properties.Images.PullFetchAll,
            GitPullAction.FetchPruneAll => GitUI.Properties.Images.PullFetchPruneAll,
            _ => throw new ArgumentOutOfRangeException(nameof(action)),
        };

    private static MenuItem PullSource(FormBrowse form, GitPullAction action)
        => action switch
        {
            GitPullAction.None => form.pullToolStripMenuItem1,
            GitPullAction.Merge => form.mergeToolStripMenuItem,
            GitPullAction.Rebase => form.rebaseToolStripMenuItem1,
            GitPullAction.Fetch => form.fetchToolStripMenuItem,
            GitPullAction.FetchAll => form.fetchAllToolStripMenuItem,
            GitPullAction.FetchPruneAll => form.fetchPruneAllToolStripMenuItem,
            _ => throw new ArgumentOutOfRangeException(nameof(action)),
        };

    private sealed class PullMenuFixture : IDisposable
    {
        private readonly GitPullAction _originalAction = AppSettings.DefaultPullAction;
        private readonly IGitUICommands _commands;

        public PullMenuFixture()
        {
            Form = new FormBrowse();

            // Supply Module through the real protected setter without invoking the
            // runtime constructor or showing Browse and starting repository/plugin loaders.
            _commands = Substitute.For<IGitUICommands>();
            _commands.Module.Returns(Substitute.For<IGitModule>());
            (typeof(GitModuleForm).GetProperty(nameof(GitModuleForm.UICommands), BindingFlags.Instance | BindingFlags.Public)
                ?? throw new InvalidOperationException("The original-shaped UICommands boundary was not found."))
                .SetValue(Form, _commands);
            Form.ToolStripMain.Items.Remove(Form.toolStripButtonPull);

            // Keep the real Browse button, flyout, clones and handlers without loading
            // a repository or starting the Browse window's plugin/background lifecycle.
            Window = new Window
            {
                Width = 1024,
                Height = 600,
                Content = new Border
                {
                    Padding = new Avalonia.Thickness(8),
                    Child = Form.toolStripButtonPull,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                },
            };
            Window.Show();
            Settle();
        }

        public FormBrowse Form { get; }

        public Window Window { get; }

        public MenuFlyout Flyout => (MenuFlyout)Form.toolStripButtonPull.Flyout!;

        public MenuItem Parent => Form.setDefaultPullButtonActionToolStripMenuItem;

        public MenuItem Clone(GitPullAction action)
            => Parent.Items.OfType<MenuItem>().Single(item => item.Tag is GitPullAction itemAction && itemAction == action);

        public void RefreshShortcuts(Keys? customizedPullKeys = null)
        {
            HotkeySettings browse = HotkeySettingsManager.CreateDefaultSettingsCore(scriptsManager: null)
                .Single(settings => settings.Name == FormBrowse.HotkeySettingsName);
            HotkeyCommand[] hotkeys = browse.Commands
                ?? throw new InvalidOperationException("The source Browse hotkey defaults have no commands.");
            if (customizedPullKeys is Keys keyData)
            {
                hotkeys.Single(command => command.CommandCode == (int)FormBrowse.Command.PullOrFetch).KeyData = keyData;
            }

            IHotkeySettingsLoader loader = Substitute.For<IHotkeySettingsLoader>();
            loader.LoadHotkeys(FormBrowse.HotkeySettingsName).Returns(hotkeys);
            _commands.GetService(typeof(IHotkeySettingsLoader)).Returns(loader);

            // Exercise the same protected loading and private mapping boundaries as
            // the runtime constructor, without starting its repository lifecycle.
            (typeof(GitExtensionsFormBase).GetProperty("HotkeysEnabled", BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException("The form hotkey-enable boundary was not found."))
                .SetValue(Form, true);
            (typeof(GitExtensionsFormBase).GetMethod("LoadHotkeys", BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException("The form hotkey-loading boundary was not found."))
                .Invoke(Form, [FormBrowse.HotkeySettingsName]);
            (typeof(FormBrowse).GetMethod("SetShortcutKeyDisplayStringsFromHotkeySettings", BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException("The source menu-shortcut mapping was not found."))
                .Invoke(Form, []);
        }

        public void Open()
        {
            Form.toolStripButtonPull.ShowDropDown();
            Settle();

            // The no-repository fixture does not claim remote-count visibility parity.
            // Exercise the optional source action's retained menu layout explicitly.
            Form.fetchAllToolStripMenuItem.IsVisible = true;
            Clone(GitPullAction.FetchAll).IsVisible = true;
            Parent.IsSubMenuOpen = true;
            Settle();
            Flyout.IsOpen.Should().BeTrue();
            Parent.IsSubMenuOpen.Should().BeTrue();
        }

        public void Click(Control control)
        {
            TopLevel topLevel = TopLevel.GetTopLevel(control)
                ?? throw new InvalidOperationException("The actual menu control has no input root.");
            using WriteableBitmap? frame = topLevel.CaptureRenderedFrame();
            Point point = control.TranslatePoint(new Point(control.Bounds.Width / 2, control.Bounds.Height / 2), topLevel)
                ?? throw new InvalidOperationException("The actual menu control has no input coordinate.");
            topLevel.MouseMove(point);
            topLevel.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
            topLevel.MouseUp(point, MouseButton.Left, RawInputModifiers.None);
            Settle();
        }

        public void ClickOutside()
        {
            Point point = new(Window.Bounds.Width - 8, Window.Bounds.Height - 8);
            Window.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
            Window.MouseUp(point, MouseButton.Left, RawInputModifiers.None);
            Settle();
        }

        public void Settle()
        {
            Dispatcher.UIThread.RunJobs();
            Window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            using WriteableBitmap? ownerFrame = Window.CaptureRenderedFrame();
            if (TopLevel.GetTopLevel(Parent) is { } parentPopup)
            {
                using WriteableBitmap? parentFrame = parentPopup.CaptureRenderedFrame();
            }

            if (TopLevel.GetTopLevel(Clone(GitPullAction.None)) is { } submenuPopup)
            {
                using WriteableBitmap? submenuFrame = submenuPopup.CaptureRenderedFrame();
            }
        }

        public void Dispose()
        {
            Flyout.Hide();
            Window.Close();
            AppSettings.DefaultPullAction = _originalAction;
            ((IDisposable)Form).Dispose();
        }
    }
}
