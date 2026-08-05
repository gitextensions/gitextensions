using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.Media;
using GitCommands;
using GitCommands.Utils;
using GitUI.LeftPanel.ContextMenu;
using GitUI.LeftPanel.Interfaces;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitExtensionsTests.LeftPanel.ContextMenu;

[TestFixture]
[NonParallelizable]
[Category("P4.4")]
public sealed class ContextMenuGeneratorTests
{
    [AvaloniaTest]
    public void Local_branch_generator_has_the_original_items_in_the_original_order()
    {
        TestBranchNode node = new();
        LocalBranchMenuItems<TestBranchNode> generator = new(new TestMenuItemFactory(node));

        AssertAllItems(generator, node);
    }

    [AvaloniaTest]
    public void Remote_branch_generator_has_the_original_items_in_the_original_order()
    {
        TestBranchNode node = new();
        RemoteBranchMenuItems<TestBranchNode> generator = new(new TestMenuItemFactory(node));

        AssertAllItems(generator, node);
    }

    [AvaloniaTest]
    public void Tag_generator_has_the_original_items_in_the_original_order()
    {
        TestBranchNode node = new();
        TagMenuItems<TestBranchNode> generator = new(new TestMenuItemFactory(node));
        generator.Strings.Tooltips[MenuItemKey.Rename] = new TranslationString("Rename");

        AssertAllItems(generator, node);
    }

    [AvaloniaTest]
    public void Current_local_branch_only_keeps_create_and_rename_enabled()
    {
        LocalBranchMenuItems<TestBranchNode> generator = new(new TestMenuItemFactory(new TestBranchNode()));

        MenuItemKey[] enabledKeys =
        [
            .. generator
                .Where(item => LocalBranchMenuItems<TestBranchNode>.CurrentBranchItemKeys.Contains(item.Key))
                .Select(item => item.Key),
        ];
        MenuItemKey[] disabledKeys =
        [
            .. generator
                .Where(item => !LocalBranchMenuItems<TestBranchNode>.CurrentBranchItemKeys.Contains(item.Key))
                .Select(item => item.Key),
        ];

        enabledKeys.Should().Equal(MenuItemKey.GitRefCreateBranch, MenuItemKey.Rename);
        disabledKeys.Should().Equal(
            MenuItemKey.GitRefCheckout,
            MenuItemKey.GitRefMerge,
            MenuItemKey.GitRefRebase,
            MenuItemKey.GitRefReset,
            MenuItemKey.GitRefActionsSeparator,
            MenuItemKey.Delete);
    }

    [AvaloniaTest]
    public void Sort_by_menu_exposes_and_requeries_every_original_option()
    {
        GitRefsSortBy original = AppSettings.RefsSortBy;
        try
        {
            GitRefsSortByContextMenuItem item = new(() => { });
            MenuItem[] options = [.. item.Items.OfType<MenuItem>()];
            options.Should().HaveCount(EnumHelper.GetValues<GitRefsSortBy>().Length);

            foreach (GitRefsSortBy value in EnumHelper.GetValues<GitRefsSortBy>())
            {
                AppSettings.RefsSortBy = value;
                item.GetTestAccessor().RaiseDropDownOpening();
                options.Single(option => Equals(option.Tag, value)).IsChecked.Should().BeTrue();
                options.Where(option => !Equals(option.Tag, value)).Should().OnlyContain(option => !option.IsChecked);
            }
        }
        finally
        {
            AppSettings.RefsSortBy = original;
        }
    }

    [AvaloniaTest]
    public void Sort_by_click_updates_the_setting_and_refreshes_refs()
    {
        GitRefsSortBy original = AppSettings.RefsSortBy;
        try
        {
            int refreshCount = 0;
            GitRefsSortByContextMenuItem item = new(() => refreshCount++);
            MenuItem[] options = [.. item.Items.OfType<MenuItem>()];

            foreach (MenuItem option in options)
            {
                option.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
                AppSettings.RefsSortBy.Should().Be((GitRefsSortBy)option.Tag!);
            }

            refreshCount.Should().Be(options.Length);
        }
        finally
        {
            AppSettings.RefsSortBy = original;
        }
    }

    [AvaloniaTest]
    public void Sort_order_menu_exposes_and_requeries_every_original_option()
    {
        GitRefsSortOrder original = AppSettings.RefsSortOrder;
        try
        {
            GitRefsSortOrderContextMenuItem item = new(() => { });
            MenuItem[] options = [.. item.Items.OfType<MenuItem>()];
            options.Should().HaveCount(EnumHelper.GetValues<GitRefsSortOrder>().Length);

            foreach (GitRefsSortOrder value in EnumHelper.GetValues<GitRefsSortOrder>())
            {
                AppSettings.RefsSortOrder = value;
                item.GetTestAccessor().RaiseDropDownOpening();
                options.Single(option => Equals(option.Tag, value)).IsChecked.Should().BeTrue();
                options.Where(option => !Equals(option.Tag, value)).Should().OnlyContain(option => !option.IsChecked);
            }
        }
        finally
        {
            AppSettings.RefsSortOrder = original;
        }
    }

    [AvaloniaTest]
    public void Sort_order_click_updates_the_setting_and_refreshes_refs()
    {
        GitRefsSortOrder original = AppSettings.RefsSortOrder;
        try
        {
            int refreshCount = 0;
            GitRefsSortOrderContextMenuItem item = new(() => refreshCount++);
            MenuItem[] options = [.. item.Items.OfType<MenuItem>()];

            foreach (MenuItem option in options)
            {
                option.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
                AppSettings.RefsSortOrder.Should().Be((GitRefsSortOrder)option.Tag!);
            }

            refreshCount.Should().Be(options.Length);
        }
        finally
        {
            AppSettings.RefsSortOrder = original;
        }
    }

    [AvaloniaTest]
    public void Insert_and_separator_normalization_match_the_original_menu_contract()
    {
        MenuItem first = new() { Header = "first" };
        MenuItem middle = new() { Header = "middle" };
        MenuItem last = new() { Header = "last" };
        Separator leading = new();
        Separator duplicate = new();
        Separator trailing = new();
        Avalonia.Controls.ContextMenu menu = new()
        {
            Items = { leading, first, duplicate, trailing },
        };

        menu.InsertItems([middle], after: first);
        menu.InsertItems([last], after: trailing);
        duplicate.IsVisible = false;
        menu.ToggleSeparators();

        menu.Items.OfType<Control>().Should().Equal(leading, first, middle, duplicate, trailing, last);
        leading.IsVisible.Should().BeFalse();
        duplicate.IsVisible.Should().BeTrue();
        trailing.IsVisible.Should().BeFalse();
    }

    private static void AssertAllItems(
        MenuItemsGenerator<TestBranchNode> generator,
        TestBranchNode node)
    {
        ToolStripItemWithKey[] items = [.. generator];
        items.Select(item => item.Key).Should().Equal(
            MenuItemKey.GitRefCheckout,
            MenuItemKey.GitRefMerge,
            MenuItemKey.GitRefRebase,
            MenuItemKey.GitRefCreateBranch,
            MenuItemKey.GitRefReset,
            MenuItemKey.GitRefActionsSeparator,
            MenuItemKey.Rename,
            MenuItemKey.Delete);

        foreach (ToolStripItemWithKey item in items.Where(item => item.Item is MenuItem))
        {
            ((MenuItem)item.Item).RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        }

        node.CallStack.Should().Equal(
            nameof(TestBranchNode.Checkout),
            nameof(TestBranchNode.Merge),
            nameof(TestBranchNode.Rebase),
            nameof(TestBranchNode.CreateBranch),
            nameof(TestBranchNode.Reset),
            nameof(TestBranchNode.Rename),
            nameof(TestBranchNode.Delete));
    }

    private sealed class TestMenuItemFactory(TestBranchNode node) : IMenuItemFactory
    {
        public TMenuItem CreateMenuItem<TMenuItem, TNode>(
            Action<TNode> onClick,
            TranslationString text,
            TranslationString toolTip,
            IImage? icon = null)
            where TMenuItem : MenuItem, new()
            where TNode : class, INode
        {
            TMenuItem item = new() { Header = text.Text };
            item.Click += (_, _) => onClick((TNode)(INode)node);
            return item;
        }
    }

    private sealed class TestBranchNode : INode, IGitRefActions, ICanDelete, ICanRename
    {
        public List<string> CallStack { get; } = [];

        public string FullPath => "refs/heads/test";

        public bool Checkout() => Trace();

        public bool CreateBranch() => Trace();

        public bool Delete() => Trace();

        public bool Merge() => Trace();

        public bool Rebase() => Trace();

        public bool Rename() => Trace();

        public bool Reset() => Trace();

        private bool Trace([CallerMemberName] string name = "")
        {
            CallStack.Add(name);
            return true;
        }
    }
}
