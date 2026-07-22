using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.Media;
using GitCommands;
using GitCommands.Remotes;
using GitCommands.Submodules;
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
            roots.Should().HaveCount(6);
            HeaderText(roots[0]).Should().Be("Branches (2)");
            HeaderText(roots[1]).Should().Be("Remotes (2)");
            HeaderText(roots[2]).Should().Be("Worktrees (0)");
            HeaderText(roots[3]).Should().Be("Tags (1)");
            HeaderText(roots[4]).Should().Be("Submodules (0)");
            HeaderText(roots[5]).Should().Be("Stashes (0)");

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
            HeaderText(roots[3].Items.Cast<TreeViewItem>().Single()).Should().Be("release");
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
            int previousWorktreeIndex = AppSettings.RepoObjectsTreeWorktreesIndex;
            Click(moveUp);
            AppSettings.RepoObjectsTreeTagsIndex.Should().Be(previousWorktreeIndex);
            AppSettings.RepoObjectsTreeWorktreesIndex.Should().Be(previousTagIndex);

            accessor.ShowSubmodulesButton.IsChecked = false;
            Click(accessor.ShowSubmodulesButton);
            AppSettings.RepoObjectsTreeShowSubmodules.Should().BeFalse();
            accessor.Tree.Items.Cast<TreeViewItem>().Select(HeaderText).Should().NotContain(text => text.StartsWith("Submodules", StringComparison.Ordinal));

            accessor.ShowWorktreesButton.IsChecked = false;
            Click(accessor.ShowWorktreesButton);
            AppSettings.RepoObjectsTreeShowWorktrees.Should().BeFalse();
            accessor.Tree.Items.Cast<TreeViewItem>().Select(HeaderText).Should().NotContain(text => text.StartsWith("Worktrees", StringComparison.Ordinal));
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
            MenuItem reset = accessor.GetActionMenuItem("Reset");
            MenuItem rename = accessor.GetActionMenuItem("RenameBranch");
            MenuItem delete = accessor.GetActionMenuItem("DeleteBranch");
            reset.IsVisible.Should().BeTrue();
            reset.Header.Should().Be("Re_set current branch to here...");
            rename.IsVisible.Should().BeTrue();
            delete.IsVisible.Should().BeTrue();
            Click(reset);
            Click(rename);
            Click(delete);

            commands.Received(1).StartResetCurrentBranchDialog(control, "feature");
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
            accessor.Tree.Items.Cast<TreeViewItem>().Should().HaveCount(5);
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
        translation.Received(1).AddTranslationItem(nameof(RepoObjectsTree), "tsbShowSubmodules", "Text", "&Submodules");
        translation.Received(1).AddTranslationItem(nameof(RepoObjectsTree), "tsbShowStashes", "Text", "St&ashes");
    }

    [AvaloniaTest]
    public void Worktrees_should_preserve_display_state_and_select_their_revision()
    {
        SettingsSnapshot settings = SettingsSnapshot.Capture();
        try
        {
            settings.EnableAllTrees();
            string parentPath = Path.Combine(Path.GetTempPath(), $"GitExtensions-worktree-tree-{Guid.NewGuid():N}");
            string mainPath = Path.Combine(parentPath, "repo");
            string linkedRoot = Path.Combine(parentPath, "repo.worktrees");
            GitWorktree main = new(mainPath, GitWorktreeHeadType.Branch, "1111111111111111111111111111111111111111", "main", IsDeleted: false);
            GitWorktree feature = new(Path.Combine(linkedRoot, "feature-a"), GitWorktreeHeadType.Branch, "2222222222222222222222222222222222222222", "feature-a", IsDeleted: false);
            GitWorktree deleted = new(Path.Combine(linkedRoot, "feature-b"), GitWorktreeHeadType.Detached, "3333333333333333333333333333333333333333", null, IsDeleted: true);
            RepoObjectsTree control = new();
            control.SetRefs([], []);

            control.GetTestAccessor().SetWorktrees([main, feature, deleted], mainPath);

            RepoObjectsTree.TestAccessor accessor = control.GetTestAccessor();
            TreeViewItem root = accessor.Tree.Items.Cast<TreeViewItem>()
                .Single(item => HeaderText(item).StartsWith("Worktrees", StringComparison.Ordinal));
            HeaderText(root).Should().Be("Worktrees (3)");
            root.IsExpanded.Should().BeTrue();
            TreeViewItem[] items = root.Items.Cast<TreeViewItem>().ToArray();
            HeaderText(items[0]).Should().Be("repo (main)");
            HeaderLabel(items[0]).FontWeight.Should().Be(FontWeight.Bold);
            HeaderText(items[1]).Should().Be("feature-a (feature-a)");
            HeaderText(items[2]).Should().Be("feature-b (detached at 3333333)");
            items[2].Classes.Should().Contain("worktree-deleted");
            ToolTip.GetTip(items[0])!.ToString().Should().Contain("(current)").And.Contain("HEAD: 1111111");

            accessor.Tree.SelectedItem = items[1];

            control.SelectedRevisionObjectId.Should().Be(ObjectId.Parse(feature.Sha1!));
            control.SelectedRef.Should().BeNull();
        }
        finally
        {
            settings.Restore();
        }
    }

    [AvaloniaTest]
    public void Worktree_context_actions_should_match_the_WinForms_enablement_and_commands()
    {
        SettingsSnapshot settings = SettingsSnapshot.Capture();
        string parentPath = Path.Combine(Path.GetTempPath(), $"GitExtensions-worktree-actions-{Guid.NewGuid():N}");
        string mainPath = Path.Combine(parentPath, "repo");
        string linkedPath = Path.Combine(parentPath, "feature");
        Directory.CreateDirectory(mainPath);
        Directory.CreateDirectory(linkedPath);
        try
        {
            settings.EnableAllTrees();
            GitWorktree main = new(mainPath, GitWorktreeHeadType.Branch, "1111111111111111111111111111111111111111", "main", IsDeleted: false);
            GitWorktree linked = new(linkedPath, GitWorktreeHeadType.Branch, "2222222222222222222222222222222222222222", "feature", IsDeleted: false);
            IGitModule module = Substitute.For<IGitModule>();
            module.IsBareRepository().Returns(false);
            module.WorkingDir.Returns(mainPath);
            IGitUICommands commands = Substitute.For<IGitUICommands>();
            commands.Module.Returns(module);
            IGitUICommandsSource source = Substitute.For<IGitUICommandsSource>();
            source.UICommands.Returns(commands);
            RepoObjectsTree control = new() { UICommandsSource = source };
            control.SetRefs([], []);
            RepoObjectsTree.TestAccessor accessor = control.GetTestAccessor();
            accessor.SetWorktrees([main, linked], mainPath);
            TreeViewItem root = accessor.Tree.Items.Cast<TreeViewItem>()
                .Single(item => HeaderText(item).StartsWith("Worktrees", StringComparison.Ordinal));
            TreeViewItem current = root.Items.Cast<TreeViewItem>().First();
            TreeViewItem other = root.Items.Cast<TreeViewItem>().Last();

            accessor.Tree.SelectedItem = root;
            accessor.UpdateContextMenu().Should().BeTrue();
            accessor.CreateWorktreeMenuItem.IsVisible.Should().BeTrue();
            accessor.PruneWorktreesMenuItem.IsVisible.Should().BeTrue();
            accessor.ManageWorktreesMenuItem.IsVisible.Should().BeTrue();
            Click(accessor.CreateWorktreeMenuItem);
            commands.Received(1).WorktreeCreate(control, mainPath);

            accessor.Tree.SelectedItem = current;
            accessor.UpdateContextMenu().Should().BeTrue();
            accessor.OpenWorktreeMenuItem.IsVisible.Should().BeTrue();
            accessor.OpenWorktreeMenuItem.IsEnabled.Should().BeFalse();
            accessor.DeleteWorktreeMenuItem.IsEnabled.Should().BeFalse();

            accessor.Tree.SelectedItem = other;
            accessor.UpdateContextMenu().Should().BeTrue();
            accessor.OpenWorktreeMenuItem.IsEnabled.Should().BeTrue();
            accessor.DeleteWorktreeMenuItem.IsEnabled.Should().BeTrue();
            accessor.CopyWorktreePathMenuItem.IsEnabled.Should().BeTrue();
            accessor.ShowWorktreeInFolderMenuItem.IsEnabled.Should().BeTrue();
            Click(accessor.OpenWorktreeMenuItem);
            Click(accessor.DeleteWorktreeMenuItem);

            commands.Received(1).WorktreeSwitch(control, linkedPath);
            commands.Received(1).WorktreeDelete(control, linkedPath);
        }
        finally
        {
            settings.Restore();
            Directory.Delete(parentPath, recursive: true);
        }
    }

    [AvaloniaTest]
    public void Worktree_context_actions_should_reuse_the_existing_translation_keys()
    {
        RepoObjectsTree control = new();
        ITranslation translation = Substitute.For<ITranslation>();

        control.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(nameof(RepoObjectsTree), "mnubtnCreateWorktreeFromRootNode", "Text", "&Create worktree...");
        translation.Received(1).AddTranslationItem(nameof(RepoObjectsTree), "mnubtnPruneWorktreesFromRootNode", "Text", "&Prune worktrees");
        translation.Received(1).AddTranslationItem(nameof(RepoObjectsTree), "mnubtnManageWorktreesFromRootNode", "Text", "&Manage worktrees...");
        translation.Received(1).AddTranslationItem(nameof(RepoObjectsTree), "mnubtnOpenWorktree", "Text", "&Open worktree");
        translation.Received(1).AddTranslationItem(nameof(RepoObjectsTree), "mnubtnDeleteWorktree", "Text", "&Delete worktree...");
        translation.Received(1).AddTranslationItem(nameof(RepoObjectsTree), "mnubtnCopyWorktreePath", "Text", "Copy &path");
        translation.Received(1).AddTranslationItem(nameof(RepoObjectsTree), "mnubtnShowWorktreeInFolder", "Text", "Show &in folder");
        translation.Received(1).AddTranslationItem(nameof(RepoObjectsTree), "mnubtnOpenWorktree", "ToolTipText", "Open this worktree");
        translation.Received(1).AddTranslationItem(nameof(RepoObjectsTree), "mnubtnDeleteWorktree", "ToolTipText", "Delete this worktree");
        translation.Received(1).AddTranslationItem(nameof(RepoObjectsTree), "mnubtnCopyWorktreePath", "ToolTipText", "Copy the worktree path to clipboard");
        translation.Received(1).AddTranslationItem(nameof(RepoObjectsTree), "mnubtnShowWorktreeInFolder", "ToolTipText", "Show the worktree in File Explorer");
        translation.Received(1).AddTranslationItem(nameof(RepoObjectsTree), "tsbShowWorktrees", "Text", "&Worktrees");
    }

    [AvaloniaTest]
    public void Submodules_should_keep_the_top_project_and_nested_folder_hierarchy()
    {
        SettingsSnapshot settings = SettingsSnapshot.Capture();
        try
        {
            settings.EnableAllTrees();
            string topPath = Path.Combine(Path.GetTempPath(), "repo");
            SubmoduleInfoResult result = CreateSubmoduleResult(
                topPath,
                new SubmoduleInfo("Externals/alpha (main)", Path.Combine(topPath, "Externals", "alpha"), bold: false),
                new SubmoduleInfo("Externals/beta (release)", Path.Combine(topPath, "Externals", "beta"), bold: false),
                new SubmoduleInfo("nested/deep (topic)", Path.Combine(topPath, "Externals", "alpha", "nested", "deep"), bold: false));
            RepoObjectsTree control = new();
            control.SetRefs([], []);
            control.GetTestAccessor().SetSubmodules(result);

            TreeViewItem submodules = control.GetTestAccessor().Tree.Items.Cast<TreeViewItem>()
                .Single(item => HeaderText(item).StartsWith("Submodules", StringComparison.Ordinal));
            HeaderText(submodules).Should().Be("Submodules (3)");
            TreeViewItem top = submodules.Items.Cast<TreeViewItem>().Single();
            HeaderText(top).Should().Be("repo (main)");
            HeaderLabel(top).FontWeight.Should().Be(FontWeight.Bold);
            TreeViewItem externals = top.Items.Cast<TreeViewItem>().Single();
            HeaderText(externals).Should().Be("Externals");
            TreeViewItem alpha = externals.Items.Cast<TreeViewItem>().Single(item => HeaderText(item).StartsWith("alpha", StringComparison.Ordinal));
            externals.Items.Cast<TreeViewItem>().Should().Contain(item => HeaderText(item).StartsWith("beta", StringComparison.Ordinal));
            HeaderText(alpha.Items.Cast<TreeViewItem>().Single()).Should().Be("nested");
            HeaderText(alpha.Items.Cast<TreeViewItem>().Single().Items.Cast<TreeViewItem>().Single()).Should().Be("deep (topic)");

            control.GetTestAccessor().Tree.SelectedItem = alpha;
            control.GetTestAccessor().SetSubmodules(result);
            HeaderText((TreeViewItem)control.GetTestAccessor().Tree.SelectedItem!).Should().Be("alpha (main)");
        }
        finally
        {
            settings.Restore();
        }
    }

    [AvaloniaTest]
    public void Submodule_folder_nodes_should_compact_single_child_chains()
    {
        SettingsSnapshot settings = SettingsSnapshot.Capture();
        try
        {
            settings.EnableAllTrees();
            string topPath = Path.Combine(Path.GetTempPath(), "repo");
            RepoObjectsTree control = new();
            control.SetRefs([], []);
            control.GetTestAccessor().SetSubmodules(CreateSubmoduleResult(
                topPath,
                new SubmoduleInfo("group/one/two/leaf", Path.Combine(topPath, "group", "one", "two", "leaf"), bold: false)));

            TreeViewItem submodules = control.GetTestAccessor().Tree.Items.Cast<TreeViewItem>()
                .Single(item => HeaderText(item).StartsWith("Submodules", StringComparison.Ordinal));
            TreeViewItem top = submodules.Items.Cast<TreeViewItem>().Single();
            TreeViewItem compactFolder = top.Items.Cast<TreeViewItem>().Single();
            HeaderText(compactFolder).Should().Be("group/one/two");
            HeaderLabel(compactFolder).FontStyle.Should().Be(FontStyle.Italic);
            HeaderText(compactFolder.Items.Cast<TreeViewItem>().Single()).Should().Be("leaf");
        }
        finally
        {
            settings.Restore();
        }
    }

    [AvaloniaTest]
    public void Submodule_status_provider_should_refresh_the_existing_tree_root()
    {
        SettingsSnapshot settings = SettingsSnapshot.Capture();
        try
        {
            settings.EnableAllTrees();
            ISubmoduleStatusProvider provider = Substitute.For<ISubmoduleStatusProvider>();
            IGitUICommands commands = Substitute.For<IGitUICommands>();
            commands.GetService(typeof(ISubmoduleStatusProvider)).Returns(provider);
            IGitUICommandsSource source = Substitute.For<IGitUICommandsSource>();
            source.UICommands.Returns(commands);
            RepoObjectsTree control = new() { UICommandsSource = source };
            control.SetRefs([], []);
            SubmoduleInfoResult result = CreateSubmoduleResult(
                Path.Combine(Path.GetTempPath(), "repo"),
                new SubmoduleInfo("child", Path.Combine(Path.GetTempPath(), "repo", "child"), bold: false));

            provider.StatusUpdated += Raise.Event<EventHandler<SubmoduleStatusEventArgs>>(
                provider,
                new SubmoduleStatusEventArgs(result, structureUpdated: true, CancellationToken.None));

            TreeViewItem submodules = control.GetTestAccessor().Tree.Items.Cast<TreeViewItem>()
                .Single(item => HeaderText(item).StartsWith("Submodules", StringComparison.Ordinal));
            HeaderText(submodules).Should().Be("Submodules (1)");
            HeaderText(submodules.Items.Cast<TreeViewItem>().Single().Items.Cast<TreeViewItem>().Single()).Should().Be("child");
        }
        finally
        {
            settings.Restore();
        }
    }

    [AvaloniaTest]
    public void Submodule_context_actions_should_route_to_the_existing_workflows()
    {
        SettingsSnapshot settings = SettingsSnapshot.Capture();
        string topPath = Path.Combine(Path.GetTempPath(), $"GitExtensions-submodule-tree-{Guid.NewGuid():N}");
        string childPath = Path.Combine(topPath, "libs", "child");
        Directory.CreateDirectory(childPath);
        try
        {
            settings.EnableAllTrees();
            IGitModule module = Substitute.For<IGitModule>();
            module.IsBareRepository().Returns(false);
            IGitUICommands commands = Substitute.For<IGitUICommands>();
            commands.Module.Returns(module);
            IGitUICommandsSource source = Substitute.For<IGitUICommandsSource>();
            source.UICommands.Returns(commands);
            string? openedPath = null;
            RepoObjectsTree control = new() { UICommandsSource = source };
            control.Initialize(_ => { }, (path, _, _) => openedPath = path);
            control.SetRefs([], []);
            control.GetTestAccessor().SetSubmodules(CreateSubmoduleResult(
                topPath,
                new SubmoduleInfo("libs/child (main)", childPath, bold: false)));
            RepoObjectsTree.TestAccessor accessor = control.GetTestAccessor();
            TreeViewItem submodules = accessor.Tree.Items.Cast<TreeViewItem>()
                .Single(item => HeaderText(item).StartsWith("Submodules", StringComparison.Ordinal));
            TreeViewItem top = submodules.Items.Cast<TreeViewItem>().Single();
            TreeViewItem child = top.Items.Cast<TreeViewItem>().Single().Items.Cast<TreeViewItem>().Single();

            accessor.Tree.SelectedItem = child;
            accessor.UpdateContextMenu().Should().BeTrue();
            accessor.GetActionMenuItem("Copy").IsVisible.Should().BeFalse();
            accessor.GetActionMenuItem("Filter").IsVisible.Should().BeFalse();
            accessor.GetActionMenuItem("OpenSubmodule").IsVisible.Should().BeTrue();
            accessor.GetActionMenuItem("ManageSubmodules").IsVisible.Should().BeFalse();
            Click(accessor.GetActionMenuItem("OpenSubmodule"));
            Click(accessor.GetActionMenuItem("UpdateSubmodule"));

            openedPath.Should().Be(childPath);
            commands.Received(1).StartUpdateSubmoduleDialog(control, "libs/child", topPath);

            accessor.Tree.SelectedItem = top;
            accessor.UpdateContextMenu().Should().BeTrue();
            accessor.GetActionMenuItem("OpenSubmodule").IsVisible.Should().BeFalse();
            accessor.GetActionMenuItem("ManageSubmodules").IsVisible.Should().BeTrue();
            Click(accessor.GetActionMenuItem("ManageSubmodules"));
            Click(accessor.GetActionMenuItem("SynchronizeSubmodules"));
            commands.Received(1).StartSubmodulesDialog(control);
            commands.Received(1).StartSyncSubmodulesDialog(control);
        }
        finally
        {
            settings.Restore();
            Directory.Delete(topPath, recursive: true);
        }
    }

    private static SubmoduleInfoResult CreateSubmoduleResult(string topPath, params SubmoduleInfo[] submodules)
    {
        SubmoduleInfoResult result = new()
        {
            TopProject = new SubmoduleInfo($"{Path.GetFileName(topPath)} (main)", topPath, bold: true),
        };
        foreach (SubmoduleInfo submodule in submodules)
        {
            result.AllSubmodules.Add(submodule);
        }

        return result;
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
        bool ShowWorktrees,
        bool ShowTags,
        bool ShowSubmodules,
        bool ShowStashes,
        int BranchesIndex,
        int RemotesIndex,
        int WorktreesIndex,
        int TagsIndex,
        int SubmodulesIndex,
        int StashesIndex)
    {
        public static SettingsSnapshot Capture()
            => new(
                AppSettings.RepoObjectsTreeShowBranches,
                AppSettings.RepoObjectsTreeShowRemotes,
                AppSettings.RepoObjectsTreeShowWorktrees,
                AppSettings.RepoObjectsTreeShowTags,
                AppSettings.RepoObjectsTreeShowSubmodules,
                AppSettings.RepoObjectsTreeShowStashes,
                AppSettings.RepoObjectsTreeBranchesIndex,
                AppSettings.RepoObjectsTreeRemotesIndex,
                AppSettings.RepoObjectsTreeWorktreesIndex,
                AppSettings.RepoObjectsTreeTagsIndex,
                AppSettings.RepoObjectsTreeSubmodulesIndex,
                AppSettings.RepoObjectsTreeStashesIndex);

        public void EnableAllTrees()
        {
            AppSettings.RepoObjectsTreeShowBranches = true;
            AppSettings.RepoObjectsTreeShowRemotes = true;
            AppSettings.RepoObjectsTreeShowWorktrees = true;
            AppSettings.RepoObjectsTreeShowTags = true;
            AppSettings.RepoObjectsTreeShowSubmodules = true;
            AppSettings.RepoObjectsTreeShowStashes = true;
            AppSettings.RepoObjectsTreeBranchesIndex = 0;
            AppSettings.RepoObjectsTreeRemotesIndex = 1;
            AppSettings.RepoObjectsTreeWorktreesIndex = 2;
            AppSettings.RepoObjectsTreeTagsIndex = 3;
            AppSettings.RepoObjectsTreeSubmodulesIndex = 4;
            AppSettings.RepoObjectsTreeStashesIndex = 5;
        }

        public void Restore()
        {
            AppSettings.RepoObjectsTreeShowBranches = ShowBranches;
            AppSettings.RepoObjectsTreeShowRemotes = ShowRemotes;
            AppSettings.RepoObjectsTreeShowWorktrees = ShowWorktrees;
            AppSettings.RepoObjectsTreeShowTags = ShowTags;
            AppSettings.RepoObjectsTreeShowSubmodules = ShowSubmodules;
            AppSettings.RepoObjectsTreeShowStashes = ShowStashes;
            AppSettings.RepoObjectsTreeBranchesIndex = BranchesIndex;
            AppSettings.RepoObjectsTreeRemotesIndex = RemotesIndex;
            AppSettings.RepoObjectsTreeWorktreesIndex = WorktreesIndex;
            AppSettings.RepoObjectsTreeTagsIndex = TagsIndex;
            AppSettings.RepoObjectsTreeSubmodulesIndex = SubmodulesIndex;
            AppSettings.RepoObjectsTreeStashesIndex = StashesIndex;
        }
    }
}
