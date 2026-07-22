using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using GitCommands;
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
    }

    private static GitRevision CreateStash()
        => new(StashId)
        {
            ReflogSelector = "refs/stash@{0}",
            Subject = "On main: saved work",
        };

    private static void Click(MenuItem item)
        => item.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
}
