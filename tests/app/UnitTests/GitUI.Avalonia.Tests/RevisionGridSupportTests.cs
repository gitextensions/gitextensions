using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.Properties;
using GitUI.UserControls;
using GitUI.UserControls.RevisionGrid;
using GitUI.UserControls.RevisionGrid.Columns;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
public sealed class RevisionGridSupportTests
{
    [SetUp]
    public void SetUp()
    {
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
    }

    [AvaloniaTest]
    public void Empty_repository_control_should_preserve_actions_and_bare_repository_state()
    {
        IGitUICommandsSource source = Substitute.For<IGitUICommandsSource>();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        source.UICommands.Returns(commands);
        EmptyRepoControl control = new() { UICommandsSource = source };

        Button editGitIgnore = control.FindControl<Button>("btnEditGitIgnore")!;
        Button openCommit = control.FindControl<Button>("btnOpenCommitForm")!;
        editGitIgnore.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
        openCommit.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));

        commands.Received(1).StartEditGitIgnoreDialog(control, localExcludes: false);
        commands.Received(1).StartCommitDialog(control);
        control.FindControl<Label>("lblEmptyRepository")!.Content.Should()
            .Be("This repository does not yet contain any commits.");
        KeyboardNavigation.GetTabIndex(editGitIgnore).Should().Be(0);
        KeyboardNavigation.GetTabIndex(openCommit).Should().Be(1);

        EmptyRepoControl bareControl = new(isBareRepository: true);
        bareControl.FindControl<Button>("btnEditGitIgnore")!.IsVisible.Should().BeFalse();
        bareControl.FindControl<Button>("btnOpenCommitForm")!.IsVisible.Should().BeFalse();
    }

    [AvaloniaTest]
    public void Error_and_loading_controls_should_preserve_their_owned_visual_state()
    {
        ErrorControl error = new();

        error.Content.Should().BeOfType<Image>().Which.Source.Should().BeSameAs(Images.StatusBadgeError);

        LoadingControl loading = new();
        loading.Content.Should().BeOfType<WaitSpinner>();
        loading.IsAnimating.Should().BeTrue();

        loading.IsAnimating = false;

        loading.IsAnimating.Should().BeFalse();
    }

    [Test]
    public void Navigation_history_should_walk_backward_forward_and_clear_forward_on_push()
    {
        ObjectId first = Id('1');
        ObjectId second = Id('2');
        ObjectId third = Id('3');
        NavigationHistory history = new();

        history.Push(first);
        history.Push(second);
        history.Push(third);

        history.NavigateBackward().Should().Be(second);
        history.NavigateBackward().Should().Be(first);
        history.NavigateForward().Should().Be(second);

        history.Push(third);

        history.CanNavigateForward.Should().BeFalse();
        history.NavigateBackward().Should().Be(second);
    }

    [Test]
    public void Parent_child_navigation_history_should_reverse_the_last_direction()
    {
        ObjectId child = Id('1');
        ObjectId parent = Id('2');
        ObjectId selected = default;
        ParentChildNavigationHistory? history = null;
        history = new ParentChildNavigationHistory(objectId =>
        {
            selected = objectId;
            history!.RevisionsSelectionChanged();
        });

        history.NavigateToParent(child, parent);

        selected.Should().Be(parent);
        history.HasPreviousChild.Should().BeTrue();

        history.NavigateToPreviousChild(parent);

        selected.Should().Be(child);
        history.HasPreviousParent.Should().BeTrue();
    }

    [Test]
    public void Visible_row_range_should_enumerate_and_compare_like_the_original()
    {
        VisibleRowRange range = new(fromIndex: 3, count: 4);

        range.Should().Equal(3, 4, 5, 6);
        range.Contains(2).Should().BeFalse();
        range.Contains(3).Should().BeTrue();
        range.Contains(6).Should().BeTrue();
        range.Contains(7).Should().BeFalse();
        range.Equals(new VisibleRowRange(3, 4)).Should().BeTrue();
        range.ToString().Should().Be("[3, 6] 4 rows");
    }

    [AvaloniaTest]
    public void Quick_search_should_select_matching_revisions_wrap_and_show_status()
    {
        int originalTimeout = AppSettings.RevisionGridQuickSearchTimeout;
        try
        {
            AppSettings.RevisionGridQuickSearchTimeout = 60_000;
            ListBox revisions = new()
            {
                ItemsSource = new[]
                {
                    Revision('1', "first"),
                    Revision('2', "target one"),
                    Revision('3', "target two"),
                },
                SelectedIndex = 0,
            };
            SolidColorBrush successBrush = new(Colors.Green);
            SolidColorBrush errorBrush = new(Colors.Red);
            revisions.Resources.Add("GitExtensionsToolTipForegroundBrush", successBrush);
            revisions.Resources.Add("GitExtensionsErrorForegroundBrush", errorBrush);
            Grid overlay = new();
            overlay.Children.Add(revisions);
            QuickSearchProvider provider = new(revisions, overlay, () => string.Empty);
            Window window = new() { Width = 500, Height = 200, Content = overlay };
            window.Show();
            try
            {
                Dispatcher.UIThread.RunJobs();
                TextInputEventArgs input = new()
                {
                    RoutedEvent = InputElement.TextInputEvent,
                    Text = "target",
                };

                provider.OnTextInput(input);

                input.Handled.Should().BeTrue();
                revisions.SelectedIndex.Should().Be(1);
                Border status = overlay.Children.OfType<Border>().Single();
                status.IsVisible.Should().BeTrue();
                TextBlock statusText = (TextBlock)status.Child!;
                statusText.Text.Should().EndWith("target");
                statusText.Foreground.Should().BeSameAs(successBrush);

                provider.NextResult(down: true);
                revisions.SelectedIndex.Should().Be(2);
                provider.NextResult(down: true);
                revisions.SelectedIndex.Should().Be(1);

                input.Text = "missing";
                provider.OnTextInput(input);

                revisions.SelectedIndex.Should().Be(1);
                statusText.Foreground.Should().BeSameAs(errorBrush);
            }
            finally
            {
                window.Close();
            }
        }
        finally
        {
            AppSettings.RevisionGridQuickSearchTimeout = originalTimeout;
        }
    }

    [Test]
    public void Index_watcher_should_report_changed_when_invalid_repository_paths_disable_watching()
    {
        IGitUICommandsSource source = Substitute.For<IGitUICommandsSource>();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        IGitModule module = Substitute.For<IGitModule>();
        source.UICommands.Returns(commands);
        commands.Module.Returns(module);
        module.WorkingDirGitDir.Returns(Path.Combine(Path.GetTempPath(), "gitextensions-missing-index-watcher"));
        module.IsValidGitWorkingDir().Returns(false);
        using IndexWatcher watcher = new(source);
        List<bool> changes = [];
        watcher.Changed += (_, e) => changes.Add(e.IsIndexChanged);

        watcher.Reset();

        changes.Should().Equal(true);
    }

    [AvaloniaTest]
    public void Menu_caption_and_multiline_indicator_should_use_native_noninteractive_controls()
    {
        MenuItem caption = new();
        MenuItem owner = new();

        MenuUtil.SetAsCaptionMenuItem(caption, owner);

        caption.IsEnabled.Should().BeFalse();
        caption.Focusable.Should().BeFalse();
        caption.IsHitTestVisible.Should().BeFalse();
        caption.Classes.Should().Contain("gitextensions-menu-caption");

        MultilineIndicator indicator = new();
        indicator.Update(Revision('1', "subject", multiline: true));
        indicator.IsVisible.Should().BeTrue();
        indicator.Width.Should().Be(26);
        indicator.Height.Should().Be(11);

        indicator.Update(Revision('2', "subject", multiline: false));
        indicator.IsVisible.Should().BeFalse();
    }

    [AvaloniaTest]
    public void Revision_grid_context_menu_should_preserve_the_original_item_order()
    {
        RevisionGridControl control = new();
        ContextMenu contextMenu = control.FindControl<ContextMenu>("mainContextMenu")
            ?? throw new InvalidOperationException("The revision context menu was not created.");

        contextMenu.Items.OfType<Control>().Select(item => item.Name).Should().Equal(
            "markRevisionAsBadToolStripMenuItem",
            "markRevisionAsGoodToolStripMenuItem",
            "bisectSkipRevisionToolStripMenuItem",
            "stopBisectToolStripMenuItem",
            "sepBisect",
            "copyToClipboardToolStripMenuItem",
            "sepCopy",
            "applyStashToolStripMenuItem",
            "popStashToolStripMenuItem",
            "dropStashToolStripMenuItem",
            "sepStash",
            "checkoutBranchToolStripMenuItem",
            "tsmiPushBranch",
            "mergeBranchToolStripMenuItem",
            "rebaseOnToolStripMenuItem",
            "resetCurrentBranchToHereToolStripMenuItem",
            "sepBranch",
            "resetChangesToolStripMenuItem",
            "commitToolStripMenuItem",
            "createNewBranchToolStripMenuItem",
            "resetAnotherBranchToHereToolStripMenuItem",
            "renameBranchToolStripMenuItem",
            "deleteBranchToolStripMenuItem",
            "sepBranchModification",
            "createTagToolStripMenuItem",
            "deleteTagToolStripMenuItem",
            "sepCommit",
            "checkoutRevisionToolStripMenuItem",
            "revertCommitToolStripMenuItem",
            "cherryPickCommitToolStripMenuItem",
            "archiveRevisionToolStripMenuItem",
            "manipulateCommitToolStripMenuItem",
            "sepCompare",
            "compareToolStripMenuItem",
            "sepNavigate",
            "navigateToolStripMenuItem",
            "tsmiSelectInLeftPanel",
            "viewToolStripMenuItem",
            "runScriptToolStripMenuItem",
            "openBuildReportToolStripMenuItem",
            "openPullRequestPageStripMenuItem",
            "tsmiOtherActions");

        control.FindControl<MenuItem>("rebaseOnToolStripMenuItem")!.Items.OfType<Control>().Select(item => item.Name).Should().Equal(
            "rebaseToolStripMenuItem",
            "rebaseInteractivelyToolStripMenuItem",
            "sepRebase",
            "rebaseWithAdvOptionsToolStripMenuItem");
        control.FindControl<MenuItem>("manipulateCommitToolStripMenuItem")!.Items.OfType<Control>().Select(item => item.Name).Should().Equal(
            "editCommitToolStripMenuItem",
            "rewordCommitToolStripMenuItem",
            "fixupCommitToolStripMenuItem",
            "squashCommitToolStripMenuItem",
            "amendCommitToolStripMenuItem",
            "getHelpOnHowToUseTheseFeaturesToolStripMenuItem");
        control.FindControl<MenuItem>("compareToolStripMenuItem")!.Items.OfType<Control>().Select(item => item.Name).Should().Equal(
            "openCommitsWithDiffToolMenuItem",
            "sepCompareDropdown",
            "compareToBranchToolStripMenuItem",
            "compareWithCurrentBranchToolStripMenuItem",
            "selectAsBaseToolStripMenuItem",
            "compareToBaseToolStripMenuItem",
            "compareToWorkingDirectoryMenuItem",
            "compareSelectedCommitsMenuItem");
    }

    [AvaloniaTest]
    public void Revision_grid_menu_commands_should_preserve_the_original_navigate_and_view_inventory()
    {
        RevisionGridControl control = new();

        control.MenuCommands.NavigateMenuCommands.Where(command => !command.IsSeparator).Select(command => command.Name).Should().Equal(
            "ToggleBetweenArtificialAndHeadCommits",
            "GotoCurrentRevision",
            "GotoCommit",
            "GotoChildCommit",
            "GotoParentCommit",
            "GotoFirstParentCommit",
            "GotoLastParentCommit",
            "GotoMergeBaseCommit",
            "NavigateBackward",
            "NavigateForward",
            "QuickSearch",
            "PrevQuickSearch",
            "NextQuickSearch");
        control.MenuCommands.ViewMenuCommands.Where(command => !command.IsSeparator).Select(command => command.Name).Should().Equal(
            "BranchesToolStripMenuItem",
            "ShowAllBranches",
            "ShowCurrentBranchOnly",
            "ShowFilteredBranches",
            "ShowReflogReferences",
            "filterToolStripMenuItem",
            "drawNonrelativesGrayToolStripMenuItem",
            "HighlightSelectedBranch",
            "CommitsToolStripMenuItem",
            "ShowArtificialCommits",
            "ShowStashes",
            "showGitNotesToolStripMenuItem",
            "ShowSessionCheckpoints",
            "Grid_labelsToolStripMenuItem",
            "ShowRemoteBranches",
            "showTagsToolStripMenuItem",
            "ShowSuperprojectTags",
            "ShowSuperprojectRemoteBranches",
            "ShowSuperprojectBranches",
            "Grid_infoToolStripMenuItem",
            "showBuildStatusIconToolStripMenuItem",
            "showBuildStatusTextToolStripMenuItem",
            "showCommitMessageBodyToolStripMenuItem",
            "showAuthorDateToolStripMenuItem",
            "showRelativeDateToolStripMenuItem",
            "ColumnsToolStripMenuItem",
            "showRevisionGraphColumnToolStripMenuItem",
            "showGitNotesColumnToolStripMenuItem",
            "showAuthorAvatarColumnToolStripMenuItem",
            "showAuthorNameColumnToolStripMenuItem",
            "showDateColumnToolStripMenuItem",
            "showIdColumnToolStripMenuItem",
            "SortingToolStripMenuItem",
            "AuthorDateSort",
            "TopoOrder",
            "Settings_persistenceToolStripMenuItem",
            "SaveAsDefault");
    }

    [AvaloniaTest]
    public void Copy_context_menu_should_rebuild_the_original_branch_tag_and_revision_groups()
    {
        GitRevision revision = Revision('1', "subject");
        revision.AuthorEmail = "author@example.com";
        revision.Refs =
        [
            new GitRef(null!, revision.ObjectId, "refs/heads/branch1"),
            new GitRef(null!, revision.ObjectId, "refs/tags/tag1"),
        ];
        CopyContextMenuItem item = new();
        item.SetRevisionFunc(() => [revision]);

        item.RaiseEvent(new RoutedEventArgs(MenuItem.SubmenuOpenedEvent));

        item.Items.Should().HaveCount(10);
        item.Items[0].Should().BeOfType<MenuItem>().Which.Header.Should().Be(TranslatedStrings.Branches);
        item.Items[1].Should().BeOfType<MenuItem>().Which.Header.Should().Be("_1:   branch1");
        item.Items[3].Should().BeOfType<MenuItem>().Which.Header.Should().Be(TranslatedStrings.Tags);
        item.Items[4].Should().BeOfType<MenuItem>().Which.Header.Should().Be("_2:   tag1");
        item.Items[6].Should().BeOfType<MenuItem>().Which.Header!.ToString().Should().StartWith("_Commit hash");
    }

    [AvaloniaTest]
    public void Revision_grid_tooltip_provider_should_be_installed_with_the_original_setting()
    {
        RevisionGridControl control = new();
        System.Reflection.FieldInfo field = typeof(RevisionGridControl).GetField(
            "_toolTipProvider",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("The revision tooltip provider field was not created.");
        RevisionGridToolTipProvider provider = (RevisionGridToolTipProvider)field.GetValue(control)!;

        provider.ShowRevisionGridTooltips.Should().Be(AppSettings.ShowRevisionGridTooltips.Value);
        provider.SetTruncation(columnIndex: 1, rowIndex: 2, truncated: true);
        provider.Clear();
        provider.Hide().Should().BeFalse();
    }

    private static ObjectId Id(char value)
        => ObjectId.Parse(new string(value, 40));

    private static GitRevision Revision(char id, string subject, bool multiline = false)
        => new(Id(id))
        {
            Subject = subject,
            Author = "Author",
            HasMultiLineMessage = multiline,
        };
}
