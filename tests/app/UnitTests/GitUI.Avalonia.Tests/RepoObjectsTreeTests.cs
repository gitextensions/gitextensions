using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.Media;
using GitCommands;
using GitCommands.Remotes;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitUI;
using GitUI.LeftPanel;
using GitUIPluginInterfaces;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class RepoObjectsTreeTests
{
    private static readonly ObjectId StashId = ObjectId.Parse("3333333333333333333333333333333333333333");

    [AvaloniaTest]
    public void Refs_should_keep_the_WinForms_tree_boundaries_and_nested_paths()
    {
        SettingsSnapshot settings = SettingsSnapshot.Capture();
        try
        {
            settings.EnableAllTrees();
            RepoObjectsTree control = new();
            control.SetRefs(
            [
                CreateRef("refs/heads/main"),
                CreateRef("refs/heads/feature/ui/buttons"),
                CreateRef("refs/remotes/origin/main", "origin"),
                CreateRef("refs/remotes/origin/feature/ui"),
                CreateRef("refs/tags/release/v1"),
            ], [], "main");

            TreeViewItem[] roots = control.GetTestAccessor().Tree.Items.Cast<TreeViewItem>().ToArray();
            roots.Should().HaveCount(4);
            HeaderText(roots[0]).Should().Be("Branches (2)");
            HeaderText(roots[1]).Should().Be("Remotes (2)");
            HeaderText(roots[2]).Should().Be("Tags (1)");
            HeaderText(roots[3]).Should().Be("Stashes (0)");

            TreeViewItem main = roots[0].Items.Cast<TreeViewItem>().Single(item => HeaderText(item) == "main");
            HeaderLabel(main).FontWeight.Should().Be(FontWeight.Bold);
            TreeViewItem feature = roots[0].Items.Cast<TreeViewItem>().Single(item => HeaderText(item) == "feature");
            TreeViewItem ui = feature.Items.Cast<TreeViewItem>().Single();
            TreeViewItem buttons = ui.Items.Cast<TreeViewItem>().Single();
            HeaderText(ui).Should().Be("ui");
            HeaderText(buttons).Should().Be("buttons");

            TreeViewItem origin = roots[1].Items.Cast<TreeViewItem>().Single();
            HeaderText(origin).Should().Be("origin");
            origin.Items.Cast<TreeViewItem>().Should().Contain(item => HeaderText(item) == "main");
            HeaderText(roots[2].Items.Cast<TreeViewItem>().Single()).Should().Be("release");
        }
        finally
        {
            settings.Restore();
        }
    }

    [AvaloniaTest]
    public void Search_should_cycle_matching_nodes_and_expand_their_paths()
    {
        SettingsSnapshot settings = SettingsSnapshot.Capture();
        try
        {
            settings.EnableAllTrees();
            RepoObjectsTree control = new();
            control.SetRefs(
            [
                CreateRef("refs/heads/feature/buttons"),
                CreateRef("refs/tags/release/buttons"),
            ]);
            RepoObjectsTree.TestAccessor accessor = control.GetTestAccessor();
            accessor.SearchBox.Text = "buttons";

            accessor.Search();
            TreeViewItem first = (TreeViewItem)accessor.Tree.SelectedItem!;
            HeaderText(first).Should().Be("buttons");
            ((NodeBase)first.Tag!).Parent!.TreeViewNode.IsExpanded.Should().BeTrue();

            accessor.Search();
            TreeViewItem second = (TreeViewItem)accessor.Tree.SelectedItem!;
            second.Should().NotBeSameAs(first);
            HeaderText(second).Should().Be("buttons");
        }
        finally
        {
            settings.Restore();
        }
    }

    [AvaloniaTest]
    public void Category_visibility_and_order_should_use_the_existing_settings()
    {
        SettingsSnapshot settings = SettingsSnapshot.Capture();
        try
        {
            settings.EnableAllTrees();
            RepoObjectsTree control = new();
            control.SetRefs([], []);
            RepoObjectsTree.TestAccessor accessor = control.GetTestAccessor();

            accessor.ShowTagsButton.IsChecked = false;
            Click(accessor.ShowTagsButton);
            AppSettings.RepoObjectsTreeShowTags.Should().BeFalse();
            accessor.Tree.Items.Cast<TreeViewItem>().Select(HeaderText).Should().NotContain(text => text.StartsWith("Tags", StringComparison.Ordinal));

            accessor.ShowTagsButton.IsChecked = true;
            Click(accessor.ShowTagsButton);
            TreeViewItem tags = accessor.Tree.Items.Cast<TreeViewItem>().Single(item => HeaderText(item).StartsWith("Tags", StringComparison.Ordinal));
            accessor.Tree.SelectedItem = tags;
            accessor.UpdateContextMenu().Should().BeTrue();
            MenuItem moveUp = accessor.GetActionMenuItem("MoveUp");
            moveUp.IsVisible.Should().BeTrue();
            int previousTagIndex = AppSettings.RepoObjectsTreeTagsIndex;
            int previousRemoteIndex = AppSettings.RepoObjectsTreeRemotesIndex;
            Click(moveUp);
            AppSettings.RepoObjectsTreeTagsIndex.Should().Be(previousRemoteIndex);
            AppSettings.RepoObjectsTreeRemotesIndex.Should().Be(previousTagIndex);
        }
        finally
        {
            settings.Restore();
        }
    }

    [AvaloniaTest]
    public void Local_branch_context_actions_should_route_to_the_existing_commands()
    {
        SettingsSnapshot settings = SettingsSnapshot.Capture();
        try
        {
            settings.EnableAllTrees();
            IGitModule module = Substitute.For<IGitModule>();
            module.IsBareRepository().Returns(false);
            IGitUICommands commands = Substitute.For<IGitUICommands>();
            commands.Module.Returns(module);
            IGitUICommandsSource source = Substitute.For<IGitUICommandsSource>();
            source.UICommands.Returns(commands);
            RepoObjectsTree control = new() { UICommandsSource = source };
            control.SetRefs([CreateRef("refs/heads/feature")], [], "main");
            RepoObjectsTree.TestAccessor accessor = control.GetTestAccessor();
            TreeViewItem branch = accessor.Tree.Items.Cast<TreeViewItem>().First().Items.Cast<TreeViewItem>().Single();

            accessor.Tree.SelectedItem = branch;
            accessor.UpdateContextMenu().Should().BeTrue();
            MenuItem rename = accessor.GetActionMenuItem("RenameBranch");
            MenuItem delete = accessor.GetActionMenuItem("DeleteBranch");
            rename.IsVisible.Should().BeTrue();
            delete.IsVisible.Should().BeTrue();
            Click(rename);
            Click(delete);

            commands.Received(1).StartRenameDialog(control, "feature");
            commands.Received(1).StartDeleteBranchDialog(control, "feature");
        }
        finally
        {
            settings.Restore();
        }
    }

    [AvaloniaTest]
    public void Remotes_should_include_configured_empty_and_inactive_entries()
    {
        SettingsSnapshot settings = SettingsSnapshot.Capture();
        try
        {
            settings.EnableAllTrees();
            RepoObjectsTree control = new();
            Remote empty = new("empty", "https://example.test/empty.git", "https://example.test/empty.git");
            Remote inactive = new("inactive", "https://example.test/inactive.git", "https://example.test/inactive.git");
            control.SetRefs(
                [],
                [],
                currentBranch: string.Empty,
                enabledRemotes: [empty],
                disabledRemotes: [inactive],
                Substitute.For<IConfigFileRemoteSettingsManager>());

            TreeViewItem remotes = control.GetTestAccessor().Tree.Items.Cast<TreeViewItem>().Single(item => HeaderText(item).StartsWith("Remotes", StringComparison.Ordinal));
            TreeViewItem[] remoteItems = remotes.Items.Cast<TreeViewItem>().ToArray();
            HeaderText(remoteItems[0]).Should().Be("empty");
            HeaderText(remoteItems[1]).Should().Be("[ Inactive ]");
            HeaderText(remoteItems[1].Items.Cast<TreeViewItem>().Single()).Should().Be("inactive");
        }
        finally
        {
            settings.Restore();
        }
    }

    [AvaloniaTest]
    public void Multi_selection_should_filter_the_revision_grid_for_all_selected_refs()
    {
        SettingsSnapshot settings = SettingsSnapshot.Capture();
        try
        {
            settings.EnableAllTrees();
            string? filter = null;
            RepoObjectsTree control = new();
            control.Initialize(value => filter = value);
            control.SetRefs(
            [
                CreateRef("refs/heads/main"),
                CreateRef("refs/tags/v1"),
            ]);
            RepoObjectsTree.TestAccessor accessor = control.GetTestAccessor();
            TreeViewItem branch = accessor.Tree.Items.Cast<TreeViewItem>().First().Items.Cast<TreeViewItem>().Single();
            TreeViewItem tag = accessor.Tree.Items.Cast<TreeViewItem>().Single(item => HeaderText(item).StartsWith("Tags", StringComparison.Ordinal)).Items.Cast<TreeViewItem>().Single();
            accessor.Tree.SelectedItem = tag;
            accessor.Tree.SelectedItems!.Add(branch);
            accessor.UpdateContextMenu().Should().BeTrue();
            MenuItem filterItem = accessor.GetActionMenuItem("Filter");
            filterItem.IsVisible.Should().BeTrue();
            Click(filterItem);

            filter.Should().NotBeNull();
            filter!.Split(' ').Should().BeEquivalentTo("main", "v1");
        }
        finally
        {
            settings.Restore();
        }
    }

    [AvaloniaTest]
    public void Stashes_should_keep_the_WinForms_node_identity_and_select_the_stash_revision()
    {
        bool originalShowStashes = AppSettings.RepoObjectsTreeShowStashes;
        try
        {
            AppSettings.RepoObjectsTreeShowStashes = true;
            RepoObjectsTree control = new();
            GitRevision stash = CreateStash();

            control.SetRefs([], [stash]);

            RepoObjectsTree.TestAccessor accessor = control.GetTestAccessor();
            TreeViewItem stashRoot = accessor.Tree.Items.Cast<TreeViewItem>().Last();
            TreeViewItem stashItem = stashRoot.Items.Cast<TreeViewItem>().Single();
            ((TextBlock)((StackPanel)stashRoot.Header!).Children[1]).Text.Should().Be("Stashes (1)");
            ((TextBlock)((StackPanel)stashItem.Header!).Children[1]).Text.Should().Be("@{0}: On main: saved work");

            accessor.Tree.SelectedItem = stashItem;

            control.SelectedRef.Should().BeNull();
            control.SelectedRevisionObjectId.Should().Be(StashId);

            AppSettings.RepoObjectsTreeShowStashes = false;
            control.SetRefs([], [stash]);
            accessor.Tree.Items.Cast<TreeViewItem>().Should().HaveCount(3);
        }
        finally
        {
            AppSettings.RepoObjectsTreeShowStashes = originalShowStashes;
        }
    }

    [AvaloniaTest]
    public void Stash_context_actions_should_route_to_the_existing_commands()
    {
        bool originalShowStashes = AppSettings.RepoObjectsTreeShowStashes;
        bool originalDontConfirmDrop = AppSettings.DontConfirmStashDrop;
        try
        {
            AppSettings.RepoObjectsTreeShowStashes = true;
            AppSettings.DontConfirmStashDrop = true;
            IGitModule module = Substitute.For<IGitModule>();
            module.IsBareRepository().Returns(false);
            IGitUICommands commands = Substitute.For<IGitUICommands>();
            commands.Module.Returns(module);
            IGitUICommandsSource source = Substitute.For<IGitUICommandsSource>();
            source.UICommands.Returns(commands);
            RepoObjectsTree control = new() { UICommandsSource = source };
            control.SetRefs([], [CreateStash()]);
            RepoObjectsTree.TestAccessor accessor = control.GetTestAccessor();
            TreeViewItem stashRoot = accessor.Tree.Items.Cast<TreeViewItem>().Last();
            TreeViewItem stashItem = stashRoot.Items.Cast<TreeViewItem>().Single();

            accessor.Tree.SelectedItem = stashRoot;
            accessor.UpdateContextMenu().Should().BeTrue();
            accessor.StashAllMenuItem.IsVisible.Should().BeTrue();
            accessor.OpenStashMenuItem.IsVisible.Should().BeFalse();
            Click(accessor.StashAllMenuItem);
            Click(accessor.StashStagedMenuItem);
            Click(accessor.ManageStashesMenuItem);

            commands.Received(1).StashSave(
                control,
                AppSettings.IncludeUntrackedFilesInManualStash,
                false,
                string.Empty,
                null);
            commands.Received(1).StashStaged(control);
            commands.Received(1).StartStashDialog(control, manageStashes: true);

            accessor.Tree.SelectedItem = stashItem;
            accessor.UpdateContextMenu().Should().BeTrue();
            accessor.StashAllMenuItem.IsVisible.Should().BeFalse();
            accessor.OpenStashMenuItem.IsVisible.Should().BeTrue();
            Click(accessor.OpenStashMenuItem);
            Click(accessor.ApplyStashMenuItem);
            Click(accessor.PopStashMenuItem);
            Click(accessor.DropStashMenuItem);

            commands.Received(1).StartStashDialog(control, manageStashes: true, "refs/stash@{0}");
            commands.Received(1).StashApply(control, "refs/stash@{0}");
            commands.Received(1).StashPop(control, "refs/stash@{0}");
            commands.Received(1).StashDrop(control, "refs/stash@{0}");
        }
        finally
        {
            AppSettings.RepoObjectsTreeShowStashes = originalShowStashes;
            AppSettings.DontConfirmStashDrop = originalDontConfirmDrop;
        }
    }

    [AvaloniaTest]
    public void Stash_context_actions_should_reuse_the_existing_translation_keys()
    {
        RepoObjectsTree control = new();
        ITranslation translation = Substitute.For<ITranslation>();

        control.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(nameof(RepoObjectsTree), "mnubtnStashAllFromRootNode", "Text", "&Stash");
        translation.Received(1).AddTranslationItem(nameof(RepoObjectsTree), "mnubtnStashStagedFromRootNode", "Text", "S&tash staged");
        translation.Received(1).AddTranslationItem(nameof(RepoObjectsTree), "mnubtnManageStashFromRootNode", "Text", "&Manage stashes...");
        translation.Received(1).AddTranslationItem(nameof(RepoObjectsTree), "mnubtnOpenStash", "Text", "&Open stash");
        translation.Received(1).AddTranslationItem(nameof(RepoObjectsTree), "mnubtnApplyStash", "Text", "&Apply stash");
        translation.Received(1).AddTranslationItem(nameof(RepoObjectsTree), "mnubtnPopStash", "Text", "&Pop stash");
        translation.Received(1).AddTranslationItem(nameof(RepoObjectsTree), "mnubtnDropStash", "Text", "&Drop stash...");
        translation.Received(1).AddTranslationItem(nameof(RepoObjectsTree), "mnubtnOpenStash", "ToolTipText", "Open this stash");
        translation.Received(1).AddTranslationItem(nameof(RepoObjectsTree), "mnubtnApplyStash", "ToolTipText", "Apply this stash");
        translation.Received(1).AddTranslationItem(nameof(RepoObjectsTree), "mnubtnPopStash", "ToolTipText", "Pop this stash");
        translation.Received(1).AddTranslationItem(nameof(RepoObjectsTree), "mnubtnDropStash", "ToolTipText", "Drop this stash");
        translation.Received(1).AddTranslationItem(nameof(RepoObjectsTree), "tsbShowBranches", "Text", "&Branches");
        translation.Received(1).AddTranslationItem(nameof(RepoObjectsTree), "tsbShowRemotes", "Text", "&Remotes");
        translation.Received(1).AddTranslationItem(nameof(RepoObjectsTree), "tsbShowTags", "Text", "&Tags");
        translation.Received(1).AddTranslationItem(nameof(RepoObjectsTree), "tsbShowStashes", "Text", "St&ashes");
    }

    private static GitRevision CreateStash()
        => new(StashId)
        {
            ReflogSelector = "refs/stash@{0}",
            Subject = "On main: saved work",
        };

    private static GitRef CreateRef(string completeName, string remote = "")
        => new(Substitute.For<IGitModule>(), ObjectId.Random(), completeName, remote);

    private static string HeaderText(TreeViewItem item)
        => HeaderLabel(item).Text!;

    private static TextBlock HeaderLabel(TreeViewItem item)
        => (TextBlock)((StackPanel)item.Header!).Children[1];

    private static void Click(MenuItem item)
        => item.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

    private static void Click(Button button)
        => button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

    private readonly record struct SettingsSnapshot(
        bool ShowBranches,
        bool ShowRemotes,
        bool ShowTags,
        bool ShowStashes,
        int BranchesIndex,
        int RemotesIndex,
        int TagsIndex,
        int StashesIndex)
    {
        public static SettingsSnapshot Capture()
            => new(
                AppSettings.RepoObjectsTreeShowBranches,
                AppSettings.RepoObjectsTreeShowRemotes,
                AppSettings.RepoObjectsTreeShowTags,
                AppSettings.RepoObjectsTreeShowStashes,
                AppSettings.RepoObjectsTreeBranchesIndex,
                AppSettings.RepoObjectsTreeRemotesIndex,
                AppSettings.RepoObjectsTreeTagsIndex,
                AppSettings.RepoObjectsTreeStashesIndex);

        public void EnableAllTrees()
        {
            AppSettings.RepoObjectsTreeShowBranches = true;
            AppSettings.RepoObjectsTreeShowRemotes = true;
            AppSettings.RepoObjectsTreeShowTags = true;
            AppSettings.RepoObjectsTreeShowStashes = true;
            AppSettings.RepoObjectsTreeBranchesIndex = 0;
            AppSettings.RepoObjectsTreeRemotesIndex = 1;
            AppSettings.RepoObjectsTreeTagsIndex = 3;
            AppSettings.RepoObjectsTreeStashesIndex = 5;
        }

        public void Restore()
        {
            AppSettings.RepoObjectsTreeShowBranches = ShowBranches;
            AppSettings.RepoObjectsTreeShowRemotes = ShowRemotes;
            AppSettings.RepoObjectsTreeShowTags = ShowTags;
            AppSettings.RepoObjectsTreeShowStashes = ShowStashes;
            AppSettings.RepoObjectsTreeBranchesIndex = BranchesIndex;
            AppSettings.RepoObjectsTreeRemotesIndex = RemotesIndex;
            AppSettings.RepoObjectsTreeTagsIndex = TagsIndex;
            AppSettings.RepoObjectsTreeStashesIndex = StashesIndex;
        }
    }
}
