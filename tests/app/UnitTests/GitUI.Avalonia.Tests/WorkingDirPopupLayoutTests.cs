using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitUI;
using GitUI.CommandsDialogs.Menus;
using GitUI.Compat;
using Microsoft.VisualStudio.Threading;
using Point = Avalonia.Point;
using Size = Avalonia.Size;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class WorkingDirPopupLayoutTests
{
    [SetUp]
    public void SetUp() => ThreadHelper.JoinableTaskContext = new JoinableTaskContext();

    [AvaloniaTest]
    [TestCase(false, false, false)]
    [TestCase(true, false, false)]
    [TestCase(false, true, false)]
    [TestCase(true, true, false)]
    [TestCase(false, true, true)]
    [TestCase(true, true, true)]
    public void Opened_menu_should_use_source_owned_font_metrics_and_actual_slots(bool rightToLeft, bool ambientEleven, bool ownedEleven)
    {
        using NativeToolStrip strip = new() { Width = 500, Height = 27 };
        if (ownedEleven)
        {
            strip.FontSize = 11 * 96d / 72;
        }

        using WorkingDirectoryToolStripSplitButton selector = new() { UseNativeToolStripLayout = true, Width = 140 };
        strip.Items.Add(selector);
        Window window = CreateWindow(strip, rightToLeft);
        window.FontSize = (ambientEleven ? 11 : 9) * 96d / 72;
        WorkingDirectoryToolStripSplitButton.TestAccessor accessor = selector.GetTestAccessor();
        try
        {
            window.Show();
            Settle(window);
            accessor.PrepareDropDown(CreateSnapshot());
            selector.ShowDropDown();
            Settle(window);
            MenuFlyoutPresenter popup = GetPopup(accessor);
            NativeToolStripDropDownLayout.Group layout = accessor.Layout;
            MenuItem[] rows = accessor.Menu.Items.OfType<MenuItem>().Where(item => item != accessor.FilterHost).ToArray();

            rows.Should().OnlyContain(item => item.FontSize == (ownedEleven ? 11 : 9) * 96d / 72,
                "ToolStrip owns its default font independently of the enclosing Form");
            accessor.Filter.FontSize.Should().Be(12, "the hosted TextBox does not inherit an explicitly enlarged menu font");
            accessor.Filter.FontFamily.Name.Should().Be(OperatingSystem.IsWindows()
                ? "Segoe UI" : FontManager.Current.DefaultFontFamily.Name);
            accessor.Filter.Margin.Should().Be(new Thickness(1));
            accessor.Filter.Padding.Should().Be(default(Thickness));
            accessor.Filter.BorderThickness.Should().Be(new Thickness(2));
            double preferredFilterHeight = WinFormsGraphicsTextMeasurer.GetFontHeight(accessor.Filter) + 4 + 3;
            accessor.Filter.Bounds.Height.Should().Be(preferredFilterHeight);
            accessor.Filter.Width.Should().Be(layout.ItemWidth - 60);
            popup.Bounds.Width.Should().Be(layout.ItemWidth + 1);
            rows.Should().OnlyContain(item => item.Bounds.Width == layout.ItemWidth && item.Bounds.Height == layout.ItemHeight);
            if (OperatingSystem.IsWindows())
            {
                layout.ItemHeight.Should().Be(ownedEleven ? 24 : 22, "the native 9/11pt consumer probes retain these source preferred row heights");
                accessor.Filter.Bounds.Height.Should().Be(23);
            }

            Point filterOrigin = accessor.Filter.TranslatePoint(default, popup)!.Value;
            double expectedFilterX = rightToLeft
                ? popup.Bounds.Width - layout.Padding.Right - layout.Padding.Left - accessor.Filter.Margin.Left - accessor.Filter.Width
                : layout.Padding.Left + accessor.Filter.Margin.Left;
            filterOrigin.Should().Be(new Point(expectedFilterX, layout.Padding.Top + accessor.Filter.Margin.Top));
            double y = layout.Padding.Top + preferredFilterHeight + accessor.Filter.Margin.Top + accessor.Filter.Margin.Bottom;
            foreach (Control item in accessor.Menu.Items.OfType<Control>().Skip(1))
            {
                Point origin = item.TranslatePoint(default, popup)!.Value;
                origin.Y.Should().Be(y);
                if (item is Separator)
                {
                    origin.X.Should().Be(2);
                    item.Bounds.Size.Should().Be(new Size(popup.Bounds.Width - 4, 6));
                    y += 6;
                }
                else
                {
                    origin.X.Should().Be(0);
                    y += layout.ItemHeight;
                }
            }

            popup.Bounds.Height.Should().Be(y + layout.Padding.Bottom);
            foreach (MenuItem row in rows)
            {
                ContentPresenter caption = Part<ContentPresenter>(row, "PART_HeaderPresenter");
                TextBlock shortcut = Part<TextBlock>(row, "PART_InputGestureText");
                AssertPartRectangle(row, caption, layout.TextRectangle);
                AssertPartRectangle(row, shortcut, layout.TextRectangle);
                caption.RenderTransform.Should().BeNull();
                shortcut.RenderTransform.Should().BeNull();
                caption.Padding.Should().Be(layout.TextPadding);
                shortcut.Padding.Should().Be(layout.GetShortcutPadding(row));
                shortcut.Text.Should().Be(NativeToolStripDropDownLayout.Group.GetShortcut(row));
                shortcut.TextAlignment.Should().Be(rightToLeft ? TextAlignment.Left : TextAlignment.Right);
                AssertPartRectangle(row, Part<ContentControl>(row, "PART_IconPresenter"), layout.ImageRectangle);
            }
        }
        finally
        {
            accessor.Menu.Hide();
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Owned_filter_should_keep_identity_input_and_source_refill_width_across_font_changes(bool rightToLeft)
    {
        using NativeToolStrip strip = new() { Width = 500, Height = 27 };
        using WorkingDirectoryToolStripSplitButton selector = new() { UseNativeToolStripLayout = true, Width = 140 };
        strip.Items.Add(selector);
        Window window = CreateWindow(strip, rightToLeft);
        WorkingDirectoryToolStripSplitButton.TestAccessor accessor = selector.GetTestAccessor();
        try
        {
            window.Show();
            Settle(window);
            RepositoryHistorySnapshot snapshot = CreateSnapshot();
            accessor.PrepareDropDown(snapshot);
            selector.ShowDropDown();
            Settle(window);
            TextBox retained = accessor.Filter;
            MenuItem[] fixedItems = accessor.Menu.Items.OfType<MenuItem>().Where(item => item.Tag is null && item != accessor.FilterHost).ToArray();
            double originalFilterWidth = retained.Width;
            retained.Focus().Should().BeTrue();
            retained.Text = "beta";
            retained.CaretIndex = 2;
            Settle(window);
            strip.FontSize = 11 * 96d / 72;
            Settle(window);
            accessor.Filter.Should().BeSameAs(retained);
            retained.IsFocused.Should().BeTrue();
            retained.Text.Should().Be("beta");
            retained.CaretIndex.Should().Be(2);
            retained.FontSize.Should().Be(12);
            retained.Width.Should().Be(originalFilterWidth,
                "source FillDropDown owns the sixty-pixel assignment, not an already-open font relayout");
            accessor.Menu.Items.OfType<MenuItem>().Where(item => item.Tag is RepositoryHistoryEntry)
                .Should().OnlyContain(item => item.IsVisible == (((RepositoryHistoryEntry)item.Tag!).Caption == "beta"));

            TopLevel popup = TopLevel.GetTopLevel(retained)!;
            popup.KeyPress(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, keySymbol: null);
            Settle(window);
            accessor.Menu.IsOpen.Should().BeFalse();
            retained.Text.Should().Be("beta");
            accessor.PrepareDropDown(snapshot);
            selector.ShowDropDown();
            Settle(window);
            accessor.Filter.Should().BeSameAs(retained);
            retained.Text.Should().BeEmpty();
            retained.Width.Should().Be(accessor.Layout.ItemWidth - 60);
            accessor.Menu.Items.OfType<MenuItem>().Where(item => item.Tag is null && item != accessor.FilterHost)
                .Should().Equal(fixedItems, "source category and fixed action controls survive subsequent fills");
        }
        finally
        {
            accessor.Menu.Hide();
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Favourite_submenus_should_use_owned_layout_without_replacing_menu_interactions(bool rightToLeft)
    {
        using WorkingDirectoryToolStripSplitButton selector = new();
        WorkingDirectoryToolStripSplitButton.TestAccessor accessor = selector.GetTestAccessor();
        Window window = CreateWindow(selector, rightToLeft);
        try
        {
            window.Show();
            Settle(window);
            RepositoryHistoryEntry favourite = new(new Repository("/repos/favourite") { Category = "Team" }, "favourite", "main", true, false);
            accessor.PrepareDropDown(new RepositoryHistorySnapshot([], [favourite]));
            selector.ShowDropDown();
            Settle(window);
            MenuItem favourites = accessor.Menu.Items.OfType<MenuItem>().Single(item => item.Items.Count > 0);
            favourites.Open();
            Settle(window);
            favourites.IsSubMenuOpen.Should().BeTrue();
            Popup submenu = Part<Popup>(favourites, "PART_Popup");
            submenu.IsOpen.Should().BeTrue();
            MenuItem category = favourites.Items.OfType<MenuItem>().Single();
            category.GetVisualAncestors().Should().Contain(item => item is NativeToolStripDropDownPanel);
            NativeToolStripDropDownLayout.Group group = NativeToolStripDropDownLayout.GetGroup(category)!;
            category.Bounds.Width.Should().Be(group.ItemWidth);
            category.Bounds.Height.Should().Be(group.ItemHeight);
            AssertPartRectangle(favourites, Part<NativeToolStripMenuArrow>(favourites, "PART_ChevronPath"), accessor.Layout.ArrowRectangle);
            Part<NativeToolStripMenuArrow>(favourites, "PART_ChevronPath").IsHitTestVisible.Should().BeFalse();
            category.Open();
            Settle(window);
            category.IsSubMenuOpen.Should().BeTrue();
            category.Items.OfType<MenuItem>().Single().Icon.Should().BeNull();
        }
        finally
        {
            accessor.Menu.Hide();
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase("&1: work_tree", "1: work_tree")]
    [TestCase("repo&&main", "repo&main")]
    [TestCase("repo&main", "repomain")]
    public void Source_prefix_measurement_should_not_allocate_mnemonic_markers_as_literal_glyphs(string source, string displayed)
    {
        MenuItem owner = new() { FontFamily = FontManager.Current.DefaultFontFamily, FontSize = 12 };
        Size actual = NativeToolStripDropDownLayout.MeasureSourceText(owner, source);
        Size expected = OperatingSystem.IsWindows()
            ? WinFormsTextMeasurer.MeasureTextRenderer(owner, displayed.Replace("&", "&&", StringComparison.Ordinal))
            : WinFormsTextMeasurer.MeasureSize(owner.FontFamily, owner.FontStyle, owner.FontWeight, owner.FontSize,
                displayed, singleLine: true, useTextRendererPadding: true);
        actual.Should().Be(expected);
    }

    [AvaloniaTest]
    public void Owned_fonts_should_preserve_explicit_item_overrides_and_release_removed_controls()
    {
        using WorkingDirectoryToolStripSplitButton selector = new() { FontSize = 11 * 96d / 72 };
        WorkingDirectoryToolStripSplitButton.TestAccessor accessor = selector.GetTestAccessor();
        accessor.FillDropDown(CreateSnapshot());
        MenuItem repository = accessor.Menu.Items.OfType<MenuItem>().First(item => item.Tag is RepositoryHistoryEntry);
        repository.FontSize = 18;
        Dispatcher.UIThread.RunJobs();
        repository.FontSize.Should().Be(18);
        accessor.Menu.Items.OfType<MenuItem>().Where(item => item != repository && item != accessor.FilterHost)
            .Should().OnlyContain(item => item.FontSize == selector.FontSize);
        accessor.FillDropDown(new RepositoryHistorySnapshot([], []));
        selector.FontSize = 20;
        Dispatcher.UIThread.RunJobs();
        repository.FontSize.Should().Be(18);
        repository.ClearValue(MenuItem.FontSizeProperty);
        repository.FontSize.Should().NotBe(20, "a removed item must no longer retain the owner's font scope or subscriptions");
        repository.Width.Should().Be(double.NaN, "source-owned size scopes must also be released when a row is removed");
    }

    [AvaloniaTest]
    public void Generic_menus_should_keep_their_existing_templates_and_fonts()
    {
        MenuItem item = new() { Header = "Generic", InputGesture = new KeyGesture(Key.G, KeyModifiers.Control), FontSize = 18 };
        MenuFlyout menu = new() { Items = { item } };
        DropDownButton button = new() { Content = "Generic", Flyout = menu };
        Window window = CreateWindow(button, rightToLeft: false);
        try
        {
            window.Show();
            Settle(window);
            menu.ShowAt(button);
            Settle(window);
            MenuFlyoutPresenter popup = item.GetVisualAncestors().OfType<MenuFlyoutPresenter>().First();
            popup.GetVisualDescendants().Where(control => control is NativeToolStripDropDownPanel or NativeToolStripMenuItemPanel)
                .Should().BeEmpty();
            NativeToolStripDropDownLayout.GetGroup(item).Should().BeNull();
            item.FontSize.Should().Be(18);
            Part<ContentPresenter>(item, "PART_HeaderPresenter").RenderTransform.Should().NotBeNull();
        }
        finally
        {
            menu.Hide();
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase("repo&main", "1: repomain")]
    [TestCase("repo_main", "1: repo_main")]
    [TestCase("repo&&main", "1: repo&main")]
    [TestCase("repo&ma&in", "1: repomain")]
    [TestCase("&repo", "1: repo")]
    [TestCase("repo&", "1: repo")]
    public void Owned_caption_should_paint_all_source_prefixes_without_changing_its_header_or_first_access_key(string caption, string displayed)
    {
        using WorkingDirectoryToolStripSplitButton selector = new();
        WorkingDirectoryToolStripSplitButton.TestAccessor accessor = selector.GetTestAccessor();
        Window window = CreateWindow(selector, rightToLeft: false);
        try
        {
            window.Show();
            Settle(window);
            RepositoryHistoryEntry repository = new(new Repository("/repos/prefix"), caption, "main", false, false);
            accessor.PrepareDropDown(new RepositoryHistorySnapshot([repository], []));
            selector.ShowDropDown();
            Settle(window);
            MenuItem row = accessor.Menu.Items.OfType<MenuItem>().Single(item => item.Tag is RepositoryHistoryEntry);
            row.Header.Should().Be(AvaloniaTranslationUtils.ToAvaloniaMnemonics($"&1: {caption}"));
            AccessText text = Part<ContentPresenter>(row, "PART_HeaderPresenter").GetVisualDescendants().OfType<AccessText>().Single();
            string shapedText = string.Concat(text.TextLayout.TextLines.SelectMany(line => line.TextRuns)
                .OfType<ShapedTextRun>().Select(run => run.Text.ToString()));
            shapedText.Should().Be(displayed, "the actual paint layout must parse every native prefix marker");
            text.AccessKey.Should().Be("1");
        }
        finally
        {
            accessor.Menu.Hide();
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(false, false)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(true, true)]
    public void Actual_selected_and_disabled_rows_should_share_source_foregrounds_even_without_pointer_hover(bool dark, bool rightToLeft)
    {
        using WorkingDirectoryToolStripSplitButton selector = new();
        WorkingDirectoryToolStripSplitButton.TestAccessor accessor = selector.GetTestAccessor();
        Window window = CreateWindow(selector, rightToLeft);
        window.RequestedThemeVariant = dark ? ThemeVariant.Dark : ThemeVariant.Light;
        try
        {
            window.Show();
            Settle(window);
            accessor.PrepareDropDown(CreateSnapshot());
            selector.ShowDropDown();
            Settle(window);
            MenuItem row = accessor.Menu.Items.OfType<MenuItem>().Single(item => item.Header as string == "Close (go to Dashboard)");
            ContentPresenter caption = Part<ContentPresenter>(row, "PART_HeaderPresenter");
            TextBlock shortcut = Part<TextBlock>(row, "PART_InputGestureText");
            Border background = Part<Border>(row, "PART_LayoutRoot");
            row.IsPointerOver.Should().BeFalse();
            AssertBrush(caption.Foreground, dark ? "#F0F0F0" : "#000000");
            AssertBrush(shortcut.Foreground, dark ? "#F0F0F0" : "#000000");
            row.IsSelected = true;
            Settle(window);
            row.IsSelected.Should().BeTrue();
            row.IsPointerOver.Should().BeFalse();
            AssertBrush(caption.Foreground, "#FFFFFF");
            AssertBrush(shortcut.Foreground, "#FFFFFF");
            AssertBrush(background.Background, dark ? "#2E3F56" : "#0078D7");
            row.IsSelected = false;
            row.IsEnabled = false;
            Settle(window);
            AssertBrush(caption.Foreground, dark ? "#969696" : "#6D6D6D");
            AssertBrush(shortcut.Foreground, dark ? "#969696" : "#6D6D6D");
            AssertBrush(Part<NativeToolStripMenuArrow>(row, "PART_ChevronPath").Foreground, dark ? "#4A4A4A" : "#A0A0A0");
        }
        finally
        {
            accessor.Menu.Hide();
            window.Close();
        }
    }

    private static void AssertBrush(IBrush? actual, string expected)
        => actual.Should().BeAssignableTo<ISolidColorBrush>().Which.Color.Should().Be(Color.Parse(expected));

    private static RepositoryHistorySnapshot CreateSnapshot()
        => new(
        [
            new RepositoryHistoryEntry(new Repository("/repos/alpha"), "alpha repository with a longer source caption", "feature/first", false, false),
            new RepositoryHistoryEntry(new Repository("/repos/beta"), "beta", "main", false, true),
        ], []);

    private static Window CreateWindow(Control content, bool rightToLeft)
        => new()
        {
            Width = 1200,
            Height = 500,
            FlowDirection = rightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
            Content = new Canvas { Children = { content } },
        };

    private static MenuFlyoutPresenter GetPopup(WorkingDirectoryToolStripSplitButton.TestAccessor accessor)
        => accessor.Filter.GetVisualAncestors().OfType<MenuFlyoutPresenter>().First();

    private static T Part<T>(MenuItem item, string name)
        where T : Control
        => item.GetVisualDescendants().OfType<T>().Single(control => control.Name == name);

    private static void AssertPartRectangle(MenuItem item, Control part, Rect expected)
    {
        Point origin = part.TranslatePoint(default, item)!.Value;
        new Rect(origin, part.Bounds.Size).Should().Be(expected);
    }

    private static void Settle(Window window)
    {
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
    }
}
