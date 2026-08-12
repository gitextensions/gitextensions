using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Extensions;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.Properties;
using GitUI.UserControls;
using GitUI.UserControls.RevisionGrid;
using GitUI.UserControls.RevisionGrid.Columns;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using IHotkeySettingsLoader = ResourceManager.IHotkeySettingsLoader;

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
    [NonParallelizable]
    [Category("P8.6h.3a")]
    public void Quick_search_should_preserve_text_key_clipboard_wrap_error_and_timeout_routes()
    {
        int originalTimeout = AppSettings.RevisionGridQuickSearchTimeout;
        GitExtensions.Shims.WinForms.IClipboard? originalClipboard = TryGetClipboard();
        try
        {
            AppSettings.RevisionGridQuickSearchTimeout = 1;
            GitExtensions.Shims.WinForms.ShimHost.Clipboard = new RecordingClipboard { Text = "target" };
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

                provider.OnKeyPress(input);

                input.Handled.Should().BeTrue();
                revisions.SelectedIndex.Should().Be(1);
                Border status = overlay.Children.OfType<Border>().Single();
                status.IsVisible.Should().BeTrue();
                TextBlock statusText = (TextBlock)status.Child!;
                statusText.Text.Should().EndWith("target");
                statusText.Foreground.Should().BeSameAs(successBrush);

                provider.NextResult(down: true);
                revisions.SelectedIndex.Should().Be(2);
                provider.NextResult(down: false);
                revisions.SelectedIndex.Should().Be(1);
                provider.NextResult(down: true);
                revisions.SelectedIndex.Should().Be(2);

                provider.OnPreviewKeyDown(KeyArgs(Avalonia.Input.Key.Back));
                statusText.Text.Should().EndWith("targe");

                provider.OnPreviewKeyDown(KeyArgs(Avalonia.Input.Key.Escape));
                status.IsVisible.Should().BeFalse();

                provider.OnPreviewKeyDown(KeyArgs(Avalonia.Input.Key.V, KeyModifiers.Control));
                statusText.Text.Should().EndWith("target");
                revisions.SelectedIndex.Should().Be(2);

                provider.OnPreviewKeyDown(KeyArgs(Avalonia.Input.Key.Escape));
                input.Text = "missing";
                provider.OnKeyPress(input);

                revisions.SelectedIndex.Should().Be(2);
                statusText.Foreground.Should().BeSameAs(errorBrush);

                Thread.Sleep(20);
                Dispatcher.UIThread.RunJobs();
                status.IsVisible.Should().BeFalse();
            }
            finally
            {
                window.Close();
            }
        }
        finally
        {
            AppSettings.RevisionGridQuickSearchTimeout = originalTimeout;
            GitExtensions.Shims.WinForms.ShimHost.Clipboard = originalClipboard ?? new RecordingClipboard();
        }

        static KeyEventArgs KeyArgs(Key key, KeyModifiers modifiers = KeyModifiers.None) => new()
        {
            RoutedEvent = InputElement.KeyDownEvent,
            Key = key,
            KeyModifiers = modifiers,
        };
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
    [Category("P8.6h.3a")]
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
    [Category("P8.6h.3b.1")]
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
        control.MenuCommands.NavigateMenuCommands.Single(command => command.Name == "GotoCommit")
            .ExecuteAction.Should().NotBeNull();
    }

    [AvaloniaTest]
    [Category("P8.6h.3b.1")]
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
    [NonParallelizable]
    [Category("P8.6h.3b.1")]
    public void Copy_context_menu_should_filter_refs_split_dates_and_copy_multi_revision_payloads()
    {
        GitRevision first = Revision('a', "first");
        first.AuthorEmail = "first@example.com";
        first.AuthorUnixTime = 1_700_000_000;
        first.CommitUnixTime = 1_700_000_100;
        first.Refs =
        [
            new GitRef(null!, first.ObjectId, "refs/heads/visible"),
            new GitRef(null!, first.ObjectId, "refs/heads/hidden"),
        ];
        GitRevision second = Revision('b', "second");
        second.AuthorEmail = "second@example.com";
        second.AuthorUnixTime = 1_700_001_000;
        second.CommitUnixTime = 1_700_001_100;
        CopyContextMenuItem item = new();
        item.SetFilterRefsFunc(refs => refs.Where(name => name != "hidden"));
        item.SetRevisionFunc(() => [first, second]);
        RecordingClipboard clipboard = new();
        GitExtensions.Shims.WinForms.IClipboard? originalClipboard = TryGetClipboard();
        GitExtensions.Shims.WinForms.ShimHost.Clipboard = clipboard;
        try
        {
            item.RaiseEvent(new RoutedEventArgs(MenuItem.SubmenuOpenedEvent));

            string[] headers = [.. item.Items.OfType<MenuItem>().Select(menuItem => menuItem.Header?.ToString() ?? string.Empty)];
            headers.Should().Contain("_1:   visible");
            headers.Should().NotContain(header => header.Contains("hidden", StringComparison.Ordinal));
            headers.Count(header => header.Contains("date", StringComparison.OrdinalIgnoreCase)).Should().Be(2);
            MenuItem commitIds = item.Items.OfType<MenuItem>()
                .Single(menuItem => menuItem.Header?.ToString()?.Contains("Commit hash", StringComparison.Ordinal) == true);

            commitIds.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

            clipboard.Text.Should().Be(first.Guid + "\n" + second.Guid);
        }
        finally
        {
            GitExtensions.Shims.WinForms.ShimHost.Clipboard = originalClipboard ?? new RecordingClipboard();
        }
    }

    [AvaloniaTest]
    [Category("P8.6h.3b.1")]
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

    [AvaloniaTest]
    [Category("P8.6h.3b.1")]
    public void Revision_grid_message_tooltip_should_include_body_notes_and_refs_like_the_original()
    {
        bool originalTooltips = AppSettings.ShowRevisionGridTooltips.Value;
        bool originalNotesColumn = AppSettings.ShowGitNotesColumn.Value;
        try
        {
            AppSettings.ShowRevisionGridTooltips.Value = true;
            AppSettings.ShowGitNotesColumn.Value = false;
            RevisionGridControl control = new();
            MessageColumnProvider provider = (MessageColumnProvider)control.ColumnProviders
                .Single(column => column.Name == "Message");
            GitRevision revision = Revision('a', "subject", multiline: true);
            revision.Body = "subject\n\nbody detail";
            revision.Notes = "review note";
            revision.Refs = [new GitRef(null!, revision.ObjectId, "refs/heads/branch1")];

            provider.TryGetToolTip(revision, out string? toolTip).Should().BeTrue();

            toolTip.Should().Contain("body detail");
            toolTip.Should().Contain("Notes:");
            toolTip.Should().Contain("review note");
            toolTip.Should().Contain("[branch1]");
        }
        finally
        {
            AppSettings.ShowRevisionGridTooltips.Value = originalTooltips;
            AppSettings.ShowGitNotesColumn.Value = originalNotesColumn;
        }
    }

    [AvaloniaTest]
    [Category("P8.6h.3b.1")]
    public void Revision_grid_message_tooltip_should_request_missing_body_like_the_original()
    {
        bool originalCommitBody = AppSettings.ShowCommitBodyInRevisionGrid;
        try
        {
            AppSettings.ShowCommitBodyInRevisionGrid = true;
            RevisionGridControl control = new();
            ICommitDataManager commitDataManager = Substitute.For<ICommitDataManager>();
            MessageColumnProvider provider = new(control, new GitRevisionSummaryBuilder(), commitDataManager);
            provider.ApplySettings();
            GitRevision revision = Revision('a', "subject", multiline: true);

            provider.TryGetToolTip(revision, out string? toolTip).Should().BeTrue();

            toolTip.Should().Be("subject" + TranslatedStrings.BodyNotLoaded);
            commitDataManager.Received(1).InitiateDelayedLoadingOfDetails(revision);
        }
        finally
        {
            AppSettings.ShowCommitBodyInRevisionGrid = originalCommitBody;
        }
    }

    [AvaloniaTest]
    [Category("P8.6h.3b.1")]
    public void Revision_grid_message_column_should_mark_annotated_tag_messages_like_the_original()
    {
        bool originalAnnotatedTags = AppSettings.ShowAnnotatedTagsMessages;
        bool originalShowTags = AppSettings.ShowTags;
        try
        {
            AppSettings.ShowAnnotatedTagsMessages = true;
            AppSettings.ShowTags = true;
            RevisionGridControl control = new();
            MessageColumnProvider provider = (MessageColumnProvider)control.ColumnProviders
                .Single(column => column.Name == "Message");
            GitRevision revision = Revision('a', "subject");
            revision.Refs = [new GitRef(null!, revision.ObjectId, "refs/tags/v1.0^{}")];
            Control cell = provider.CreateCell();

            provider.UpdateCell(cell, revision);

            cell.GetVisualDescendants().OfType<RevisionGridRefRenderer.RefLabelControl>()
                .Should().ContainSingle()
                .Which.Label.Should().Be("v1.0 [...]");
        }
        finally
        {
            AppSettings.ShowAnnotatedTagsMessages = originalAnnotatedTags;
            AppSettings.ShowTags = originalShowTags;
        }
    }

    [AvaloniaTest]
    [Category("P8.6h.3b.2a")]
    public void Revision_grid_message_column_should_render_loaded_commit_body_like_the_original()
    {
        bool originalCommitBody = AppSettings.ShowCommitBodyInRevisionGrid;
        bool originalNotesColumn = AppSettings.ShowGitNotesColumn.Value;
        try
        {
            AppSettings.ShowCommitBodyInRevisionGrid = true;
            AppSettings.ShowGitNotesColumn.Value = false;
            RevisionGridControl control = new();
            MessageColumnProvider provider = (MessageColumnProvider)control.ColumnProviders
                .Single(column => column.Name == "Message");
            GitRevision revision = Revision('a', "subject", multiline: true);
            revision.Body = "subject\n\nbody line one\nbody line two";
            revision.Notes = "review note";
            Control cell = provider.CreateCell();

            provider.UpdateCell(cell, revision);

            cell.GetVisualDescendants().OfType<TextBlock>()
                .Single(textBlock => textBlock.Classes.Contains("revision-subject"))
                .Text.Should().Be("subject");
            cell.GetVisualDescendants().OfType<TextBlock>()
                .Single(textBlock => textBlock.Classes.Contains("revision-body"))
                .Text.Should().Be(string.Concat(
                    UIExtensions.FormatBodyAndNotes(revision.Body, revision.Notes)
                        .Split(Delimiters.LineFeed, StringSplitOptions.RemoveEmptyEntries)
                        .Skip(1)
                        .Select(line => " " + line)));
        }
        finally
        {
            AppSettings.ShowCommitBodyInRevisionGrid = originalCommitBody;
            AppSettings.ShowGitNotesColumn.Value = originalNotesColumn;
        }
    }

    [AvaloniaTest]
    [Category("P8.6h.3b.2a")]
    public void Revision_grid_message_column_should_hide_commit_body_when_setting_is_disabled()
    {
        bool originalCommitBody = AppSettings.ShowCommitBodyInRevisionGrid;
        try
        {
            AppSettings.ShowCommitBodyInRevisionGrid = false;
            RevisionGridControl control = new();
            MessageColumnProvider provider = (MessageColumnProvider)control.ColumnProviders
                .Single(column => column.Name == "Message");
            GitRevision revision = Revision('a', "subject", multiline: true);
            revision.Body = "subject\n\nbody detail";
            Control cell = provider.CreateCell();

            provider.UpdateCell(cell, revision);

            cell.GetVisualDescendants().OfType<TextBlock>()
                .Single(textBlock => textBlock.Classes.Contains("revision-subject"))
                .Text.Should().Be("subject");
            cell.GetVisualDescendants().OfType<TextBlock>()
                .Single(textBlock => textBlock.Classes.Contains("revision-body"))
                .Text.Should().BeEmpty();
        }
        finally
        {
            AppSettings.ShowCommitBodyInRevisionGrid = originalCommitBody;
        }
    }

    [AvaloniaTest]
    [Category("P8.6h.3b.2a")]
    public void Revision_grid_message_column_should_request_missing_body_like_the_original()
    {
        bool originalCommitBody = AppSettings.ShowCommitBodyInRevisionGrid;
        bool originalTooltips = AppSettings.ShowRevisionGridTooltips.Value;
        try
        {
            AppSettings.ShowCommitBodyInRevisionGrid = true;
            AppSettings.ShowRevisionGridTooltips.Value = false;
            RevisionGridControl control = new();
            ICommitDataManager commitDataManager = Substitute.For<ICommitDataManager>();
            MessageColumnProvider provider = new(control, new GitRevisionSummaryBuilder(), commitDataManager);
            provider.ApplySettings();
            GitRevision revision = Revision('a', "subject", multiline: true);

            provider.UpdateCell(provider.CreateCell(), revision);

            commitDataManager.Received(1).InitiateDelayedLoadingOfDetails(revision);
        }
        finally
        {
            AppSettings.ShowCommitBodyInRevisionGrid = originalCommitBody;
            AppSettings.ShowRevisionGridTooltips.Value = originalTooltips;
        }
    }

    [AvaloniaTest]
    [Category("P8.6h.3b.2a")]
    public void Revision_grid_message_column_should_render_stash_and_autostash_labels_like_the_original()
    {
        RevisionGridControl control = new();
        MessageColumnProvider provider = (MessageColumnProvider)control.ColumnProviders
            .Single(column => column.Name == "Message");
        Control cell = provider.CreateCell();
        GitRevision stash = Revision('a', "On main: saved work");
        stash.ReflogSelector = "refs/stash@{3}";

        provider.UpdateCell(cell, stash);

        cell.GetVisualDescendants().OfType<RevisionGridRefRenderer.RefLabelControl>()
            .Where(label => label.GitRef is null && label.Label == "stash@{3}")
            .Should().ContainSingle();
        cell.GetVisualDescendants().OfType<TextBlock>()
            .Single(textBlock => textBlock.Classes.Contains("revision-subject"))
            .Text.Should().Be("On main: saved work");

        GitRevision autostash = Revision('b', "autostash");
        autostash.IsAutostash = true;
        provider.UpdateCell(cell, autostash);

        cell.GetVisualDescendants().OfType<RevisionGridRefRenderer.RefLabelControl>()
            .Where(label => label.GitRef is null && label.Label == "autostash")
            .Should().ContainSingle();
        cell.GetVisualDescendants().OfType<TextBlock>()
            .Single(textBlock => textBlock.Classes.Contains("revision-subject"))
            .Text.Should().BeEmpty();
    }

    [AvaloniaTest]
    [Category("P8.6h.3b.2a")]
    public void Revision_grid_message_column_should_not_request_commit_details_for_autostash()
    {
        bool originalCommitBody = AppSettings.ShowCommitBodyInRevisionGrid;
        bool originalTooltips = AppSettings.ShowRevisionGridTooltips.Value;
        try
        {
            AppSettings.ShowCommitBodyInRevisionGrid = true;
            AppSettings.ShowRevisionGridTooltips.Value = false;
            RevisionGridControl control = new();
            ICommitDataManager commitDataManager = Substitute.For<ICommitDataManager>();
            MessageColumnProvider provider = new(control, new GitRevisionSummaryBuilder(), commitDataManager);
            provider.ApplySettings();
            GitRevision revision = Revision('a', "autostash", multiline: true);
            revision.IsAutostash = true;

            provider.UpdateCell(provider.CreateCell(), revision);

            commitDataManager.DidNotReceive().InitiateDelayedLoadingOfDetails(revision);
        }
        finally
        {
            AppSettings.ShowCommitBodyInRevisionGrid = originalCommitBody;
            AppSettings.ShowRevisionGridTooltips.Value = originalTooltips;
        }
    }

    [AvaloniaTest]
    [Category("P8.6h.3b.2a")]
    public void Revision_grid_message_column_should_render_fixup_squash_and_amend_markers_like_the_original()
    {
        RevisionGridControl control = new();
        MessageColumnProvider provider = (MessageColumnProvider)control.ColumnProviders
            .Single(column => column.Name == "Message");
        Control cell = provider.CreateCell();
        Image marker = cell.GetVisualDescendants().OfType<Image>()
            .Single(image => image.Classes.Contains("revision-message-marker"));

        foreach (string prefix in new[] { "fixup!", "squash!", "amend!" })
        {
            provider.UpdateCell(cell, Revision('a', $"{prefix} target"));
            marker.IsVisible.Should().BeTrue(prefix);
        }

        provider.UpdateCell(cell, Revision('b', "ordinary commit"));
        marker.IsVisible.Should().BeFalse();
    }

    [AvaloniaTest]
    [Category("P8.6h.1")]
    public void Revision_grid_should_preserve_selection_and_scroll_anchor_while_streaming_rows()
    {
        RevisionGridControl control = new() { UICommandsSource = CreateUICommandsSource() };
        RevisionGridControl.TestAccessor accessor = control.GetTestAccessor();
        GitRevision[] initial = [.. Enumerable.Range(1, 80).Select(Revision)];
        accessor.SetRevisions(initial);
        Window window = new()
        {
            Width = 900,
            Height = 160,
            Content = control,
        };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            GitRevision selected = initial[40];
            accessor.Revisions.SelectedItem = selected;
            accessor.Revisions.ScrollIntoView(selected);
            Dispatcher.UIThread.RunJobs();
            ScrollViewer scrollViewer = accessor.Revisions.GetVisualDescendants().OfType<ScrollViewer>().Single();
            double verticalOffset = scrollViewer.Offset.Y;
            verticalOffset.Should().BeGreaterThan(0);
            object? itemsSource = accessor.Revisions.ItemsSource;

            accessor.AppendRevisions(Enumerable.Range(81, 20).Select(Revision));
            Dispatcher.UIThread.RunJobs();

            accessor.Revisions.ItemsSource.Should().BeSameAs(itemsSource);
            accessor.Revisions.SelectedItem.Should().BeSameAs(selected);
            scrollViewer.Offset.Y.Should().Be(verticalOffset);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [NonParallelizable]
    [Category("P8.6h.1")]
    public void Revision_grid_should_preserve_home_end_copy_and_patch_drop_routes()
    {
        RevisionGridControl control = new() { UICommandsSource = CreateUICommandsSource() };
        RevisionGridControl.TestAccessor accessor = control.GetTestAccessor();
        GitRevision[] revisions = [Revision(1), Revision(2), Revision(3)];
        accessor.SetRevisions(revisions);
        Window window = new() { Width = 900, Height = 160, Content = control };
        RecordingClipboard clipboard = new();
        GitExtensions.Shims.WinForms.IClipboard? originalClipboard = TryGetClipboard();
        GitExtensions.Shims.WinForms.ShimHost.Clipboard = clipboard;
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            control.MultiSelect = true;
            accessor.Revisions.SelectedIndex = 1;
            accessor.Revisions.SelectedItems!.Add(revisions[0]);
            RaiseKey(accessor.Revisions, Key.End);
            accessor.Revisions.SelectedItem.Should().BeSameAs(revisions[2]);
            accessor.Revisions.SelectedItems!.Count.Should().Be(1);
            RaiseKey(accessor.Revisions, Key.Home);
            accessor.Revisions.SelectedItem.Should().BeSameAs(revisions[0]);

            accessor.Revisions.SelectedItems!.Add(revisions[2]);
            RaiseKey(accessor.Revisions, Key.C, KeyModifiers.Control);
            clipboard.Text.Should().Be(string.Join(Environment.NewLine, revisions[0].ObjectId, revisions[2].ObjectId));

            DragDrop.GetAllowDrop(accessor.Revisions).Should().BeTrue();
            RevisionGridControl.CanDropPatchFiles(["one.patch", "TWO.PATCH"]).Should().BeTrue();
            RevisionGridControl.CanDropPatchFiles([]).Should().BeFalse();
            RevisionGridControl.CanDropPatchFiles(["one.patch", "notes.txt"]).Should().BeFalse();
        }
        finally
        {
            window.Close();
            GitExtensions.Shims.WinForms.ShimHost.Clipboard = originalClipboard ?? new RecordingClipboard();
        }

        static void RaiseKey(ListBox list, Key key, KeyModifiers modifiers = KeyModifiers.None)
        {
            list.RaiseEvent(new KeyEventArgs
            {
                RoutedEvent = InputElement.KeyDownEvent,
                Key = key,
                KeyModifiers = modifiers,
            });
        }
    }

    private static GitExtensions.Shims.WinForms.IClipboard? TryGetClipboard()
    {
        try
        {
            return GitExtensions.Shims.WinForms.ShimHost.Clipboard;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private static ObjectId Id(char value)
        => ObjectId.Parse(new string(value, 40));

    private static IGitUICommandsSource CreateUICommandsSource()
    {
        IGitUICommandsSource source = Substitute.For<IGitUICommandsSource>();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(Substitute.For<IGitModule>());
        commands.GetService(typeof(IHotkeySettingsLoader)).Returns(Substitute.For<IHotkeySettingsLoader>());
        source.UICommands.Returns(commands);
        return source;
    }

    private static GitRevision Revision(char id, string subject, bool multiline = false)
        => new(Id(id))
        {
            Subject = subject,
            Author = "Author",
            HasMultiLineMessage = multiline,
        };

    private static GitRevision Revision(int index)
        => new(ObjectId.Parse(index.ToString("x40")))
        {
            Subject = $"Revision {index}",
            Author = "Author",
        };

    private sealed class RecordingClipboard : GitExtensions.Shims.WinForms.IClipboard
    {
        public string Text { get; set; } = string.Empty;

        public bool ContainsText() => Text.Length > 0;

        public string GetText() => Text;

        public void SetText(string text) => Text = text;
    }
}
