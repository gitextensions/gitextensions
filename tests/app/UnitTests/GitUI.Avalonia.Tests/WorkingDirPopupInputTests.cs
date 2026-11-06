using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Threading;
using GitCommands.UserRepositoryHistory;
using GitUI;
using GitUI.CommandsDialogs.Menus;
using GitUI.Compat;
using Microsoft.VisualStudio.Threading;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class WorkingDirPopupInputTests
{
    [SetUp]
    public void SetUp() => ThreadHelper.JoinableTaskContext = new JoinableTaskContext();

    [AvaloniaTest]
    [TestCase(RawInputModifiers.None, RawInputModifiers.None)]
    [TestCase(RawInputModifiers.Control, RawInputModifiers.Control)]
    [TestCase(RawInputModifiers.None, RawInputModifiers.Control)]
    [TestCase(RawInputModifiers.Control, RawInputModifiers.None)]
    [TestCase(RawInputModifiers.Control, RawInputModifiers.Control | RawInputModifiers.Shift)]
    [TestCase(RawInputModifiers.Control | RawInputModifiers.Shift, RawInputModifiers.Control)]
    [TestCase(RawInputModifiers.Control, RawInputModifiers.Control | RawInputModifiers.Alt)]
    [TestCase(RawInputModifiers.Control | RawInputModifiers.Alt, RawInputModifiers.Control)]
    [TestCase(RawInputModifiers.None, RawInputModifiers.Meta)]
    [TestCase(RawInputModifiers.Control, RawInputModifiers.Control | RawInputModifiers.Meta)]
    public void Opened_repository_row_should_observe_release_modifiers_before_its_real_Click(
        RawInputModifiers downModifiers, RawInputModifiers upModifiers)
    {
        using Fixture fixture = new("Jasper_worktree");
        MenuItem row = fixture.Rows[0];
        Point center = row.TranslatePoint(new Point(row.Bounds.Width / 2, row.Bounds.Height / 2), fixture.Popup)
            ?? throw new InvalidOperationException("The actual row must be inside the opened popup.");
        fixture.Popup.MouseDown(center, MouseButton.Left, downModifiers);
        fixture.Settle();
        fixture.Current.Should().BeEmpty();
        fixture.Launched.Should().BeEmpty();
        fixture.Accessor.Menu.IsOpen.Should().BeTrue();

        int actionCountAtClick = 0;
        row.Click += (_, _) => actionCountAtClick = fixture.Current.Count + fixture.Launched.Count;
        fixture.Popup.MouseUp(center, MouseButton.Left, upModifiers);
        fixture.Settle();

        KeyModifiers primaryControl = KeysMapper.ToKeyGesture(
            GitExtensions.Shims.WinForms.Keys.Control | GitExtensions.Shims.WinForms.Keys.A)!.KeyModifiers;
        bool launches = ToKeyModifiers(upModifiers) == primaryControl;
        fixture.Current.Should().Equal(launches ? Array.Empty<string>() : new[] { fixture.Paths[0] });
        fixture.Launched.Should().Equal(launches ? new[] { fixture.Paths[0] } : Array.Empty<string>());
        actionCountAtClick.Should().Be(1, "the modifier context must already exist when the framework raises Click");
    }

    [AvaloniaTest]
    [TestCase("repo&Quasar", 'q', true)]
    [TestCase("repo&Quasar", 'Q', true)]
    [TestCase("repo&&Quasar", 'q', false)]
    [TestCase("repo&&&Quasar", 'q', true)]
    [TestCase("repo_Quasar", 'q', false)]
    [TestCase("repo_Quasar", '_', false)]
    public void Opened_row_should_match_every_genuine_source_marker_and_not_literal_caption_escapes(
        string caption, char character, bool activates)
    {
        using Fixture fixture = new(caption, "sender");
        fixture.Rows[1].Focus(NavigationMethod.Tab).Should().BeTrue();
        fixture.SendSymbol(character, withAlt: true);
        fixture.Current.Should().Equal(activates ? new[] { fixture.Paths[0] } : Array.Empty<string>());
        fixture.Launched.Should().BeEmpty("Alt is not the exact source Control modifier");
        if (!activates)
        {
            fixture.Rows[1].IsFocused.Should().BeTrue("a literal underscore must not become an AccessText focus route");
            fixture.Accessor.Menu.IsOpen.Should().BeTrue();
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Repeated_duplicate_mnemonics_should_cycle_once_per_key_and_never_click(bool withAlt)
    {
        using Fixture fixture = new("first&Quasar", "second&Quasar");
        fixture.Rows[0].Focus(NavigationMethod.Tab).Should().BeTrue();
        fixture.Rows[0].IsSelected.Should().BeTrue();
        int[] expected = [1, 0, 1, 0];
        foreach (int index in expected)
        {
            fixture.SendSymbol('q', withAlt);
            fixture.Rows[index].IsSelected.Should().BeTrue();
            fixture.Rows[index].IsFocused.Should().BeTrue();
            fixture.Current.Should().BeEmpty();
            fixture.Launched.Should().BeEmpty();
            fixture.Accessor.Menu.IsOpen.Should().BeTrue();
        }
    }

    [AvaloniaTest]
    public void Eleventh_numbered_row_should_not_register_its_escaped_literal_underscore_as_an_access_key()
    {
        string[] captions = Enumerable.Range(1, 11)
            .Select(number => number == 11 ? "work_tree" : $"caption{number}").ToArray();
        using Fixture fixture = new(captions);
        fixture.Rows[0].Focus(NavigationMethod.Tab).Should().BeTrue();
        fixture.Rows[10].Header.Should().Be("11: work__tree");
        fixture.Rows[10].Bounds.Height.Should().BeGreaterThan(0);
        fixture.SendSymbol('_', withAlt: true);
        fixture.Rows[0].IsFocused.Should().BeTrue();
        fixture.Rows[10].IsFocused.Should().BeFalse();
        fixture.Current.Should().BeEmpty();
        fixture.Launched.Should().BeEmpty();
        fixture.Accessor.Menu.IsOpen.Should().BeTrue();
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Mnemonic_search_should_ignore_source_filter_hidden_or_disabled_rows(bool filterHidden)
    {
        using Fixture fixture = new("alpha&Quasar", "beta&Quasar");
        if (filterHidden)
        {
            fixture.Accessor.Filter.Text = "beta";
        }
        else
        {
            fixture.Rows[0].IsEnabled = false;
        }

        fixture.Settle();
        fixture.Rows[1].Focus(NavigationMethod.Tab).Should().BeTrue();
        fixture.SendSymbol('q', withAlt: true);
        fixture.Current.Should().Equal(fixture.Paths[1]);
        fixture.Launched.Should().BeEmpty();
    }

    [AvaloniaTest]
    [TestCase("Zed&Quasar&Jasper", true)]
    [TestCase("Zed&&Quasar", true)]
    [TestCase("Zed&Quasar", false)]
    public void Opened_category_should_follow_the_source_raw_first_character_pseudo_pass(string categoryText, bool opens)
    {
        using Fixture fixture = new(categoryText, favourite: true);
        MenuItem favourites = fixture.Accessor.Menu.Items.OfType<MenuItem>().Single(item => item.Items.Count > 0);
        favourites.Open();
        fixture.Settle();
        MenuItem category = favourites.Items.OfType<MenuItem>().Single();
        category.Focus(NavigationMethod.Tab).Should().BeTrue();
        fixture.SendSymbol(category, 'z', withAlt: true);
        category.IsSubMenuOpen.Should().Be(opens);
        fixture.Current.Should().BeEmpty();
        fixture.Launched.Should().BeEmpty();
    }

    [AvaloniaTest]
    [TestCase("work_tree", RawInputModifiers.None)]
    [TestCase("日本語", RawInputModifiers.None)]
    [TestCase("@", RawInputModifiers.Control | RawInputModifiers.Alt)]
    public void Real_retained_filter_should_keep_text_commit_and_AltGr_outside_owned_mnemonic_routing(
        string text, RawInputModifiers modifiers)
    {
        using Fixture fixture = new("repo&Quasar");
        TextBox filter = fixture.Accessor.Filter;
        filter.Focus(NavigationMethod.Tab).Should().BeTrue();
        TopLevel popup = TopLevel.GetTopLevel(filter)!;
        popup.KeyPress(Key.Q, modifiers, PhysicalKey.Q, keySymbol: "@");
        popup.KeyTextInput(text);
        popup.KeyRelease(Key.Q, RawInputModifiers.None, PhysicalKey.Q, keySymbol: "@");
        fixture.Settle();
        filter.Text.Should().Be(text);
        filter.IsFocused.Should().BeTrue();
        fixture.Accessor.Filter.Should().BeSameAs(filter);
        fixture.Current.Should().BeEmpty();
        fixture.Launched.Should().BeEmpty();
        fixture.Accessor.Menu.IsOpen.Should().BeTrue();
    }

    [AvaloniaTest]
    public void Real_selected_Enter_should_use_the_current_target_without_a_stale_press_modifier()
    {
        using Fixture fixture = new("first", "second");
        fixture.Rows[0].Focus(NavigationMethod.Tab).Should().BeTrue();
        fixture.Popup.KeyPress(Key.LeftCtrl, RawInputModifiers.Control, PhysicalKey.ControlLeft, keySymbol: null);
        fixture.Popup.KeyRelease(Key.LeftCtrl, RawInputModifiers.None, PhysicalKey.ControlLeft, keySymbol: null);
        fixture.Rows[1].Focus(NavigationMethod.Tab).Should().BeTrue();
        fixture.Popup.KeyPress(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, keySymbol: null);
        fixture.Popup.KeyRelease(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, keySymbol: null);
        fixture.Settle();
        fixture.Current.Should().Equal(fixture.Paths[1]);
        fixture.Launched.Should().BeEmpty();
        fixture.Accessor.Menu.IsOpen.Should().BeFalse();
    }

    [AvaloniaTest]
    [TestCase(RawInputModifiers.Control)]
    [TestCase(RawInputModifiers.Shift)]
    [TestCase(RawInputModifiers.Control | RawInputModifiers.Shift)]
    [TestCase(RawInputModifiers.Control | RawInputModifiers.Alt)]
    public void Native_observed_modified_leaf_Enter_should_not_click_or_close_the_owned_popup(RawInputModifiers modifiers)
    {
        using Fixture fixture = new("first", "second");
        fixture.Rows[1].Focus(NavigationMethod.Tab).Should().BeTrue();
        fixture.Popup.KeyPress(Key.Enter, modifiers, PhysicalKey.Enter, keySymbol: null);
        fixture.Popup.KeyRelease(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, keySymbol: null);
        fixture.Settle();
        fixture.Current.Should().BeEmpty();
        fixture.Launched.Should().BeEmpty();
        fixture.Rows[1].IsSelected.Should().BeTrue();
        fixture.Accessor.Menu.IsOpen.Should().BeTrue();

        fixture.Popup.KeyPress(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, keySymbol: null);
        fixture.Popup.KeyRelease(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, keySymbol: null);
        fixture.Settle();
        fixture.Current.Should().Equal(fixture.Paths[1]);
        fixture.Launched.Should().BeEmpty();
    }

    [AvaloniaTest]
    public void Owned_mnemonic_adapter_should_leave_existing_submenu_arrow_Enter_and_Escape_routes_in_charge()
    {
        using Fixture fixture = new("Team", favourite: true);
        MenuItem favourites = fixture.Accessor.Menu.Items.OfType<MenuItem>().Single(item => item.Items.Count > 0);
        favourites.Focus(NavigationMethod.Tab).Should().BeTrue();
        fixture.Popup.KeyPress(Key.Right, RawInputModifiers.None, PhysicalKey.ArrowRight, keySymbol: null);
        fixture.Settle();
        favourites.IsSubMenuOpen.Should().BeTrue();
        MenuItem category = favourites.Items.OfType<MenuItem>().Single();
        category.Focus(NavigationMethod.Tab).Should().BeTrue();
        TopLevel categoryPopup = TopLevel.GetTopLevel(category)!;
        categoryPopup.KeyPress(Key.Right, RawInputModifiers.None, PhysicalKey.ArrowRight, keySymbol: null);
        fixture.Settle();
        category.IsSubMenuOpen.Should().BeTrue();
        MenuItem leaf = category.Items.OfType<MenuItem>().Single();
        leaf.Focus(NavigationMethod.Tab).Should().BeTrue();
        TopLevel leafPopup = TopLevel.GetTopLevel(leaf)!;
        leafPopup.KeyPress(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, keySymbol: null);
        fixture.Settle();
        category.IsSubMenuOpen.Should().BeFalse();
        fixture.Accessor.Menu.IsOpen.Should().BeTrue();
        category.Focus(NavigationMethod.Tab).Should().BeTrue();
        categoryPopup.KeyPress(Key.Right, RawInputModifiers.None, PhysicalKey.ArrowRight, keySymbol: null);
        fixture.Settle();
        leaf.Focus(NavigationMethod.Tab).Should().BeTrue();
        TopLevel.GetTopLevel(leaf)!.KeyPress(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, keySymbol: null);
        fixture.Settle();
        fixture.Current.Should().Equal(fixture.Paths[0]);
        fixture.Launched.Should().BeEmpty();
    }

    [AvaloniaTest]
    public void Default_native_wrapper_should_match_regular_AccessText_outcomes_and_keep_unmodified_Enter_activation()
    {
        DefaultMenuOutcome regular = ExerciseDefaultMenu(useNativeWrapper: false);
        DefaultMenuOutcome nativeWrapper = ExerciseDefaultMenu(useNativeWrapper: true);
        TestContext.Out.WriteLine($"Regular framework AccessText: {regular}; default native wrapper AccessText: {nativeWrapper}");
        nativeWrapper.AccessKeyClicks.Should().Be(regular.AccessKeyClicks);
        nativeWrapper.AccessKeySelected.Should().Be(regular.AccessKeySelected);
        nativeWrapper.AccessKeyFocused.Should().Be(regular.AccessKeyFocused);
        nativeWrapper.AccessKeyPopupOpen.Should().Be(regular.AccessKeyPopupOpen);
        regular.EnterClicks.Should().Be(1);
        nativeWrapper.EnterClicks.Should().Be(1);
    }

    private static DefaultMenuOutcome ExerciseDefaultMenu(bool useNativeWrapper)
    {
        int clicks = 0;
        MenuItem choice = useNativeWrapper
            ? new NativeToolStripDropDownMenuItem { Header = "_Choice" }
            : new MenuItem { Header = "_Choice" };
        if (choice is NativeToolStripDropDownMenuItem nativeItem)
        {
            nativeItem.UseSourceMnemonicRouting.Should().BeFalse();
        }

        choice.Click += (_, _) => clicks++;
        MenuFlyout menu = useNativeWrapper ? new NativeToolStripDropDownMenuFlyout() : new MenuFlyout();
        menu.Items.Add(choice);
        Button owner = new() { Content = "Generic", Flyout = menu };
        Window window = new() { Width = 320, Height = 100, Content = owner };
        try
        {
            window.Show();
            Settle(window);
            menu.ShowAt(owner);
            Settle(window);
            choice.Focus(NavigationMethod.Tab).Should().BeTrue();
            TopLevel popup = TopLevel.GetTopLevel(choice)!;

            // Compare the same full headless input on an independent framework
            // baseline; equality alone is not a positive native Alt parity claim.
            popup.KeyPress(Key.C, RawInputModifiers.Alt, PhysicalKey.C, keySymbol: "c");
            popup.KeyTextInput("c");
            popup.KeyRelease(Key.C, RawInputModifiers.None, PhysicalKey.C, keySymbol: "c");
            Settle(window);
            int accessKeyClicks = clicks;
            bool accessKeySelected = choice.IsSelected;
            bool accessKeyFocused = choice.IsFocused;
            bool accessKeyPopupOpen = menu.IsOpen;
            if (!menu.IsOpen)
            {
                menu.ShowAt(owner);
                Settle(window);
            }

            choice.Focus(NavigationMethod.Tab).Should().BeTrue();
            popup = TopLevel.GetTopLevel(choice)!;
            int beforeEnter = clicks;
            popup.KeyPress(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, keySymbol: null);
            popup.KeyRelease(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, keySymbol: null);
            Settle(window);
            return new DefaultMenuOutcome(accessKeyClicks, accessKeySelected, accessKeyFocused, accessKeyPopupOpen, clicks - beforeEnter);
        }
        finally
        {
            menu.Hide();
            window.Close();
        }
    }

    private static KeyModifiers ToKeyModifiers(RawInputModifiers modifiers)
    {
        KeyModifiers result = KeyModifiers.None;
        if (modifiers.HasFlag(RawInputModifiers.Control))
        {
            result |= KeyModifiers.Control;
        }

        if (modifiers.HasFlag(RawInputModifiers.Shift))
        {
            result |= KeyModifiers.Shift;
        }

        if (modifiers.HasFlag(RawInputModifiers.Alt))
        {
            result |= KeyModifiers.Alt;
        }

        if (modifiers.HasFlag(RawInputModifiers.Meta))
        {
            result |= KeyModifiers.Meta;
        }

        return result;
    }

    private static void Settle(Window window)
    {
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
    }

    private sealed record DefaultMenuOutcome(
        int AccessKeyClicks,
        bool AccessKeySelected,
        bool AccessKeyFocused,
        bool AccessKeyPopupOpen,
        int EnterClicks);

    private sealed class Fixture : IDisposable
    {
        private readonly Window _window;
        private readonly WorkingDirectoryToolStripSplitButton _selector = new();

        public Fixture(params string[] captions)
            : this(captions, favourite: false)
        {
        }

        public Fixture(string category, bool favourite)
            : this([category], favourite)
        {
        }

        private Fixture(string[] captions, bool favourite)
        {
            Accessor = _selector.GetTestAccessor();
            Accessor.SetRepositoryActions(Current.Add, Launched.Add);
            Paths = Enumerable.Range(0, captions.Length).Select(index => $"/repos/input-{index}").ToArray();
            RepositoryHistoryEntry[] entries = captions.Select((caption, index) => new RepositoryHistoryEntry(
                new Repository(Paths[index]) { Category = favourite ? caption : null },
                favourite ? "repo&Quasar" : caption, null, favourite, IsAnchored: false)).ToArray();
            _window = new Window { Width = 640, Height = 600, Content = _selector };
            _window.Show();
            Settle();
            Accessor.PrepareDropDown(new RepositoryHistorySnapshot(favourite ? [] : entries, favourite ? entries : []));
            _selector.ShowDropDown();
            Settle();
            Accessor.Menu.IsOpen.Should().BeTrue();
            Rows = Accessor.Menu.Items.OfType<MenuItem>().Where(item => item.Tag is RepositoryHistoryEntry).ToArray();
            Popup = TopLevel.GetTopLevel(Accessor.Filter)
                ?? throw new InvalidOperationException("The retained filter must be in the real opened popup.");
        }

        public WorkingDirectoryToolStripSplitButton.TestAccessor Accessor { get; }

        public TopLevel Popup { get; }

        public MenuItem[] Rows { get; }

        public string[] Paths { get; }

        public List<string> Current { get; } = [];

        public List<string> Launched { get; } = [];

        public void SendSymbol(char character, bool withAlt) => SendSymbol(Rows[0], character, withAlt);

        public void SendSymbol(MenuItem levelItem, char character, bool withAlt)
        {
            (Key key, PhysicalKey physicalKey) = char.ToLowerInvariant(character) switch
            {
                'q' => (Key.Q, PhysicalKey.Q),
                'z' => (Key.Z, PhysicalKey.Z),
                '_' => (Key.OemMinus, PhysicalKey.Minus),
                _ => throw new ArgumentOutOfRangeException(nameof(character)),
            };
            TopLevel popup = TopLevel.GetTopLevel(levelItem)!;
            RawInputModifiers modifiers = withAlt ? RawInputModifiers.Alt : RawInputModifiers.None;
            string symbol = character.ToString();
            popup.KeyPress(key, modifiers, physicalKey, symbol);
            popup.KeyTextInput(symbol);
            popup.KeyRelease(key, RawInputModifiers.None, physicalKey, symbol);
            Settle();
        }

        public void Settle() => WorkingDirPopupInputTests.Settle(_window);

        public void Dispose()
        {
            ((IDisposable)_selector).Dispose();
            _window.Close();
        }
    }
}
