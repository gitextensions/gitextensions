using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommitInfo;
using GitUI.Editor;
using GitUI.LeftPanel;
using GitUIPluginInterfaces;
using NSubstitute;
using ResourceManager;

namespace GitExtensionsTests;

[TestFixture]
public sealed class ViewConstructionTests
{
    [Test]
    public void Translator_should_find_the_shared_translations()
    {
        Translator.GetAllTranslations().Should().NotBeEmpty();
    }

    [AvaloniaTest]
    public void FormBrowse_should_construct()
    {
        FormBrowse form = new();
        form.Should().NotBeNull();
    }

    [AvaloniaTest]
    public void FormBrowse_workspace_layout_should_follow_and_persist_shared_settings()
    {
        CommitInfoPosition originalPosition = AppSettings.CommitInfoPosition;
        bool originalShowSplitView = AppSettings.ShowSplitViewLayout;
        try
        {
            AppSettings.ShowSplitViewLayout = true;
            foreach (CommitInfoPosition position in Enum.GetValues<CommitInfoPosition>())
            {
                AppSettings.CommitInfoPosition = position;
                FormBrowse form = new();

                Border expectedHost = position switch
                {
                    CommitInfoPosition.BelowList => form.commitInfoBelowHost,
                    CommitInfoPosition.LeftwardFromList => form.commitInfoLeftHost,
                    CommitInfoPosition.RightwardFromList => form.commitInfoRightHost,
                    _ => throw new NotSupportedException(),
                };
                form.RevisionInfo.Parent.Should().BeSameAs(expectedHost);
                form.CommitInfoTabPage.IsVisible.Should().Be(position == CommitInfoPosition.BelowList);
                form.RevisionsSplitContainer.ColumnDefinitions[0].Width.Value
                    .Should().Be(position == CommitInfoPosition.LeftwardFromList ? 490 : 0);
                form.RevisionsSplitContainer.ColumnDefinitions[4].Width.Value
                    .Should().Be(position == CommitInfoPosition.RightwardFromList ? 490 : 0);
                form.CommitInfoTabControl.IsVisible.Should().BeTrue();
                form.RightSplitContainer.RowDefinitions[2].Height.Value.Should().BeGreaterThan(0);
                form.Close();
            }

            AppSettings.CommitInfoPosition = CommitInfoPosition.BelowList;
            FormBrowse changedForm = new();
            changedForm.CommitInfoTabControl.SelectedItem = changedForm.DiffTabPage;
            changedForm.RightSplitContainer.RowDefinitions[0].Height = new GridLength(4, GridUnitType.Star);
            changedForm.RightSplitContainer.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Star);
            changedForm.toggleSplitViewLayout.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            changedForm.toggleSplitViewLayout.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            changedForm.CommitInfoTabControl.SelectedItem.Should().BeSameAs(changedForm.DiffTabPage);
            changedForm.RightSplitContainer.RowDefinitions[0].Height.Value.Should().Be(4);
            changedForm.RightSplitContainer.RowDefinitions[2].Height.Value.Should().Be(1);

            changedForm.menuCommitInfoPosition.RaiseEvent(new RoutedEventArgs(SplitButton.ClickEvent));
            AppSettings.CommitInfoPosition.Should().Be(CommitInfoPosition.LeftwardFromList);
            changedForm.RevisionInfo.Parent.Should().BeSameAs(changedForm.commitInfoLeftHost);

            changedForm.toggleSplitViewLayout.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            AppSettings.ShowSplitViewLayout.Should().BeFalse();
            changedForm.CommitInfoTabControl.IsVisible.Should().BeFalse();
            changedForm.RightSplitContainer.RowDefinitions[2].Height.Value.Should().Be(0);
            changedForm.Close();

            FormBrowse restoredForm = new();
            restoredForm.RevisionInfo.Parent.Should().BeSameAs(restoredForm.commitInfoLeftHost);
            restoredForm.CommitInfoTabControl.IsVisible.Should().BeFalse();
            restoredForm.Close();
        }
        finally
        {
            AppSettings.CommitInfoPosition = originalPosition;
            AppSettings.ShowSplitViewLayout = originalShowSplitView;
        }
    }

    // The constructor the designer and the XAML loader use must complete initialisation like
    // the run-time one, or the dialog they build is left untranslated.
    [AvaloniaTest]
    public void Designer_constructors_should_translate_the_view()
    {
        string original = AppSettings.CurrentTranslation ?? "";
        try
        {
            AppSettings.CurrentTranslation = "German";

            FormCreateBranch form = new();

            form.cmdOk.Content.Should().Be("Branch _erstellen");
            form.chkCheckoutAfterCreate.Content.Should().Be("Nach dem Erstellen _auschecken");
        }
        finally
        {
            AppSettings.CurrentTranslation = original;
        }
    }

    [AvaloniaTest]
    public void FormBrowse_refresh_should_notify_repository_changed()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(Path.GetTempPath());
        module.IsValidGitWorkingDir().Returns(false);

        ILockableNotifier notifier = Substitute.For<ILockableNotifier>();
        IAppTitleGenerator appTitleGenerator = Substitute.For<IAppTitleGenerator>();
        appTitleGenerator.Generate(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string>()).Returns("Git Extensions");

        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.RepoChangedNotifier.Returns(notifier);
        commands.GetService(typeof(IAppTitleGenerator)).Returns(appTitleGenerator);
        commands.GetService(typeof(IHotkeySettingsLoader)).Returns(Substitute.For<IHotkeySettingsLoader>());

        FormBrowse form = new(commands);
        MenuItem refresh = form.FindControl<MenuItem>("refreshToolStripMenuItem")
            ?? throw new InvalidOperationException("Refresh menu item was not created.");

        refresh.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

        notifier.Received(1).Notify();
    }

    [AvaloniaTest]
    public void FormBrowse_should_use_existing_translation_keys_for_menu_headers()
    {
        FormBrowse form = new();
        ITranslation translation = Substitute.For<ITranslation>();
        translation.TranslateItem(
                nameof(FormBrowse),
                "fetchAllToolStripMenuItem",
                "Text",
                Arg.Any<Func<string?>>())
            .Returns("&Fetch translated");

        form.AddTranslationItems(translation);
        form.TranslateItems(translation);

        translation.Received(1).AddTranslationItem(
            nameof(FormBrowse),
            "fetchAllToolStripMenuItem",
            "Text",
            "Fetch &all");
        translation.Received(1).AddTranslationItem(
            nameof(FormBrowse),
            "commitToolStripMenuItem",
            "Text",
            "&Commit...");
        translation.Received(1).AddTranslationItem(
            nameof(FormBrowse),
            "deleteBranchToolStripMenuItem",
            "Text",
            "De&lete branch...");
        translation.Received(1).AddTranslationItem(
            nameof(FormBrowse),
            "pullToolStripMenuItem",
            "Text",
            "Pull&/Fetch...");
        translation.Received(1).AddTranslationItem(
            nameof(FormBrowse),
            "tagToolStripMenuItem",
            "Text",
            "Create &tag...");
        translation.Received(1).AddTranslationItem(
            nameof(FormBrowse),
            "deleteTagToolStripMenuItem",
            "Text",
            "&Delete tag...");
        translation.Received(1).AddTranslationItem(
            nameof(FormBrowse),
            "CommitInfoTabPage",
            "Text",
            "Commit");
        translation.Received(1).AddTranslationItem(
            nameof(FormBrowse),
            "DiffTabPage",
            "Text",
            "Diff");
        translation.Received(1).AddTranslationItem(
            nameof(FormBrowse),
            "TreeTabPage",
            "Text",
            "File tree");
        translation.Received(1).AddTranslationItem(
            nameof(FormBrowse),
            "commitInfoBelowMenuItem",
            "Text",
            "Commit info &below graph");
        translation.Received(1).AddTranslationItem(
            nameof(FormBrowse),
            "toggleSplitViewLayout",
            "ToolTipText",
            "Toggle split view layout");
        translation.Received(1).AddTranslationItem(
            nameof(FormBrowse),
            "menuCommitInfoPosition",
            "ToolTipText",
            "Commit info position");
        translation.DidNotReceive().AddTranslationItem(
            nameof(FormBrowse),
            "menuCommitInfoPosition",
            "toolTip",
            Arg.Any<string>());
        translation.Received(1).TranslateItem(
            nameof(FormBrowse),
            "fetchAllToolStripMenuItem",
            "Text",
            Arg.Is<Func<string?>>(provideDefaultValue => provideDefaultValue() == "Fetch &all"));
        MenuItem fetchAll = form.FindControl<MenuItem>("fetchAllToolStripMenuItem")
            ?? throw new InvalidOperationException("Fetch-all menu item was not created.");
        fetchAll.Header.Should().Be("_Fetch translated");
    }

    [AvaloniaTest]
    public void RevisionGrid_should_use_the_shared_menu_command_translation_keys()
    {
        RevisionGridControl revisionGrid = new();
        ITranslation translation = Substitute.For<ITranslation>();
        translation.TranslateItem(
                "RevisionGrid",
                "GotoCurrentRevision",
                "Text",
                Arg.Any<Func<string?>>())
            .Returns("Go to translated &revision");

        revisionGrid.AddTranslationItems(translation);
        revisionGrid.TranslateItems(translation);

        translation.Received(1).AddTranslationItem(
            "RevisionGrid",
            "GotoCurrentRevision",
            "Text",
            "Go to c&urrent revision");
        translation.DidNotReceive().AddTranslationItem(
            nameof(RevisionGridControl),
            "GotoCurrentRevision",
            "Text",
            Arg.Any<string>());
        MenuItem navigate = revisionGrid.FindControl<MenuItem>("navigateToolStripMenuItem")
            ?? throw new InvalidOperationException("Navigate menu was not created.");
        MenuItem translatedItem = navigate.Items.OfType<MenuItem>()
            .Single(menuItem => menuItem.Tag as string == "GotoCurrentRevision");
        translatedItem.Header.Should().Be("Go to translated _revision");
    }

    [AvaloniaTest]
    public void FormBrowse_commit_should_start_the_dialog()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(Path.GetTempPath());
        module.IsValidGitWorkingDir().Returns(false);

        ILockableNotifier notifier = Substitute.For<ILockableNotifier>();
        IAppTitleGenerator appTitleGenerator = Substitute.For<IAppTitleGenerator>();
        appTitleGenerator.Generate(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string>()).Returns("Git Extensions");

        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.RepoChangedNotifier.Returns(notifier);
        commands.GetService(typeof(IAppTitleGenerator)).Returns(appTitleGenerator);
        commands.GetService(typeof(IHotkeySettingsLoader)).Returns(Substitute.For<IHotkeySettingsLoader>());

        FormBrowse form = new(commands);
        MenuItem commit = form.FindControl<MenuItem>("commitToolStripMenuItem")
            ?? throw new InvalidOperationException("Commit menu item was not created.");
        commit.IsEnabled = true;

        commit.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

        commands.Received(1).StartCommitDialog(form);
    }

    [AvaloniaTest]
    public void FormBrowse_stash_should_start_the_dialog()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(Path.GetTempPath());
        module.IsValidGitWorkingDir().Returns(false);

        ILockableNotifier notifier = Substitute.For<ILockableNotifier>();
        IAppTitleGenerator appTitleGenerator = Substitute.For<IAppTitleGenerator>();
        appTitleGenerator.Generate(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string>()).Returns("Git Extensions");

        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.RepoChangedNotifier.Returns(notifier);
        commands.GetService(typeof(IAppTitleGenerator)).Returns(appTitleGenerator);
        commands.GetService(typeof(IHotkeySettingsLoader)).Returns(Substitute.For<IHotkeySettingsLoader>());

        FormBrowse form = new(commands);
        MenuItem stash = form.FindControl<MenuItem>("stashToolStripMenuItem")
            ?? throw new InvalidOperationException("Manage-stashes menu item was not created.");
        stash.IsEnabled = true;

        stash.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

        commands.Received(1).StartStashDialog(form);
    }

    [AvaloniaTest]
    public void FormBrowse_merge_should_start_the_dialog()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(Path.GetTempPath());
        module.IsValidGitWorkingDir().Returns(false);

        ILockableNotifier notifier = Substitute.For<ILockableNotifier>();
        IAppTitleGenerator appTitleGenerator = Substitute.For<IAppTitleGenerator>();
        appTitleGenerator.Generate(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string>()).Returns("Git Extensions");

        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.RepoChangedNotifier.Returns(notifier);
        commands.GetService(typeof(IAppTitleGenerator)).Returns(appTitleGenerator);
        commands.GetService(typeof(IHotkeySettingsLoader)).Returns(Substitute.For<IHotkeySettingsLoader>());

        FormBrowse form = new(commands);
        MenuItem merge = form.FindControl<MenuItem>("mergeBranchToolStripMenuItem")
            ?? throw new InvalidOperationException("Merge-branches menu item was not created.");
        merge.IsEnabled = true;

        merge.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

        commands.Received(1).StartMergeBranchDialog(form, null);
    }

    [AvaloniaTest]
    public void FormBrowse_rebase_should_start_the_dialog()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(Path.GetTempPath());
        module.IsValidGitWorkingDir().Returns(false);

        ILockableNotifier notifier = Substitute.For<ILockableNotifier>();
        IAppTitleGenerator appTitleGenerator = Substitute.For<IAppTitleGenerator>();
        appTitleGenerator.Generate(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string>()).Returns("Git Extensions");

        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.RepoChangedNotifier.Returns(notifier);
        commands.GetService(typeof(IAppTitleGenerator)).Returns(appTitleGenerator);
        commands.GetService(typeof(IHotkeySettingsLoader)).Returns(Substitute.For<IHotkeySettingsLoader>());

        FormBrowse form = new(commands);
        MenuItem rebase = form.FindControl<MenuItem>("rebaseToolStripMenuItem")
            ?? throw new InvalidOperationException("Rebase menu item was not created.");
        RevisionGridControl revisionGrid = form.FindControl<RevisionGridControl>("RevisionGrid")
            ?? throw new InvalidOperationException("Revision grid was not created.");
        ListBox revisions = revisionGrid.FindControl<ListBox>("lstRevisions")
            ?? throw new InvalidOperationException("Revision list was not created.");
        GitRevision revision = new(ObjectId.Random());
        revisions.ItemsSource = new[] { revision };
        revisions.SelectedItem = revision;
        rebase.IsEnabled = true;

        rebase.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

        commands.Received(1).StartRebaseDialog(form, revision.ObjectId.ToString());
    }

    [AvaloniaTest]
    public void FormBrowse_pull_should_start_the_dialog()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(Path.GetTempPath());
        module.IsValidGitWorkingDir().Returns(false);

        ILockableNotifier notifier = Substitute.For<ILockableNotifier>();
        IAppTitleGenerator appTitleGenerator = Substitute.For<IAppTitleGenerator>();
        appTitleGenerator.Generate(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string>()).Returns("Git Extensions");

        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.RepoChangedNotifier.Returns(notifier);
        commands.GetService(typeof(IAppTitleGenerator)).Returns(appTitleGenerator);
        commands.GetService(typeof(IHotkeySettingsLoader)).Returns(Substitute.For<IHotkeySettingsLoader>());

        FormBrowse form = new(commands);
        MenuItem pull = form.FindControl<MenuItem>("pullToolStripMenuItem")
            ?? throw new InvalidOperationException("Pull menu item was not created.");
        pull.IsEnabled = true;

        pull.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

        commands.Received(1).StartPullDialog(form);
    }

    [AvaloniaTest]
    public void FormBrowse_toolbar_should_route_commit_push_stash_and_default_pull_actions()
    {
        GitPullAction originalDefaultPullAction = AppSettings.DefaultPullAction;
        try
        {
            AppSettings.DefaultPullAction = GitPullAction.Merge;
            IGitModule module = Substitute.For<IGitModule>();
            module.WorkingDir.Returns(Path.GetTempPath());
            module.IsValidGitWorkingDir().Returns(false);

            IAppTitleGenerator titleGenerator = Substitute.For<IAppTitleGenerator>();
            titleGenerator.Generate(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string>()).Returns("Git Extensions");
            IGitUICommands commands = Substitute.For<IGitUICommands>();
            commands.Module.Returns(module);
            commands.RepoChangedNotifier.Returns(Substitute.For<ILockableNotifier>());
            commands.GetService(typeof(IAppTitleGenerator)).Returns(titleGenerator);
            commands.GetService(typeof(IHotkeySettingsLoader)).Returns(Substitute.For<IHotkeySettingsLoader>());

            FormBrowse form = new(commands);
            Button commit = form.FindControl<Button>("toolStripButtonCommit")!;
            Button push = form.FindControl<Button>("toolStripButtonPush")!;
            SplitButton stash = form.FindControl<SplitButton>("toolStripSplitStash")!;
            SplitButton pull = form.FindControl<SplitButton>("toolStripButtonPull")!;
            commit.IsEnabled = push.IsEnabled = stash.IsEnabled = pull.IsEnabled = true;

            commit.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            push.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            stash.RaiseEvent(new RoutedEventArgs(SplitButton.ClickEvent));
            pull.RaiseEvent(new RoutedEventArgs(SplitButton.ClickEvent));

            commands.Received(1).StartCommitDialog(form);
            commands.Received(1).StartPushDialog(form, pushOnShow: false);
            commands.Received(1).StartStashDialog(form);
            commands.Received(1).StartPullDialogAndPullImmediately(form, pullAction: GitPullAction.Merge);
        }
        finally
        {
            AppSettings.DefaultPullAction = originalDefaultPullAction;
        }
    }

    [AvaloniaTest]
    public void FormBrowse_checkout_branch_should_start_the_dialog()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(Path.GetTempPath());
        module.IsValidGitWorkingDir().Returns(false);

        ILockableNotifier notifier = Substitute.For<ILockableNotifier>();
        IAppTitleGenerator appTitleGenerator = Substitute.For<IAppTitleGenerator>();
        appTitleGenerator.Generate(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string>()).Returns("Git Extensions");

        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.RepoChangedNotifier.Returns(notifier);
        commands.GetService(typeof(IAppTitleGenerator)).Returns(appTitleGenerator);
        commands.GetService(typeof(IHotkeySettingsLoader)).Returns(Substitute.For<IHotkeySettingsLoader>());

        FormBrowse form = new(commands);
        MenuItem checkout = form.FindControl<MenuItem>("checkoutBranchToolStripMenuItem")
            ?? throw new InvalidOperationException("Checkout-branch menu item was not created.");
        checkout.IsEnabled = true;

        checkout.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

        commands.Received(1).StartCheckoutBranch(form);
    }

    [AvaloniaTest]
    public void FormBrowse_create_branch_should_start_the_dialog_at_the_selected_revision()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(Path.GetTempPath());
        module.IsValidGitWorkingDir().Returns(false);

        ILockableNotifier notifier = Substitute.For<ILockableNotifier>();
        IAppTitleGenerator appTitleGenerator = Substitute.For<IAppTitleGenerator>();
        appTitleGenerator.Generate(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string>()).Returns("Git Extensions");

        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.RepoChangedNotifier.Returns(notifier);
        commands.GetService(typeof(IAppTitleGenerator)).Returns(appTitleGenerator);
        commands.GetService(typeof(IHotkeySettingsLoader)).Returns(Substitute.For<IHotkeySettingsLoader>());

        FormBrowse form = new(commands);
        MenuItem createBranch = form.FindControl<MenuItem>("branchToolStripMenuItem")
            ?? throw new InvalidOperationException("Create-branch menu item was not created.");
        createBranch.IsEnabled = true;

        createBranch.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

        commands.Received(1).StartCreateBranchDialog(form, default(ObjectId));
    }

    [AvaloniaTest]
    public void FormBrowse_delete_branch_should_start_the_dialog()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(Path.GetTempPath());
        module.IsValidGitWorkingDir().Returns(false);

        ILockableNotifier notifier = Substitute.For<ILockableNotifier>();
        IAppTitleGenerator appTitleGenerator = Substitute.For<IAppTitleGenerator>();
        appTitleGenerator.Generate(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string>()).Returns("Git Extensions");

        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.RepoChangedNotifier.Returns(notifier);
        commands.GetService(typeof(IAppTitleGenerator)).Returns(appTitleGenerator);
        commands.GetService(typeof(IHotkeySettingsLoader)).Returns(Substitute.For<IHotkeySettingsLoader>());

        FormBrowse form = new(commands);
        MenuItem deleteBranch = form.FindControl<MenuItem>("deleteBranchToolStripMenuItem")
            ?? throw new InvalidOperationException("Delete-branch menu item was not created.");
        deleteBranch.IsEnabled = true;

        deleteBranch.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

        commands.Received(1).StartDeleteBranchDialog(form, string.Empty);
    }

    [AvaloniaTest]
    public void FormBrowse_tag_commands_should_start_the_matching_dialogs()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(Path.GetTempPath());
        module.IsValidGitWorkingDir().Returns(false);

        ILockableNotifier notifier = Substitute.For<ILockableNotifier>();
        IAppTitleGenerator appTitleGenerator = Substitute.For<IAppTitleGenerator>();
        appTitleGenerator.Generate(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string>()).Returns("Git Extensions");

        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.RepoChangedNotifier.Returns(notifier);
        commands.GetService(typeof(IAppTitleGenerator)).Returns(appTitleGenerator);
        commands.GetService(typeof(IHotkeySettingsLoader)).Returns(Substitute.For<IHotkeySettingsLoader>());

        FormBrowse form = new(commands);
        MenuItem createTag = form.FindControl<MenuItem>("tagToolStripMenuItem")
            ?? throw new InvalidOperationException("Create-tag menu item was not created.");
        MenuItem deleteTag = form.FindControl<MenuItem>("deleteTagToolStripMenuItem")
            ?? throw new InvalidOperationException("Delete-tag menu item was not created.");
        createTag.IsEnabled = true;
        deleteTag.IsEnabled = true;

        createTag.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        deleteTag.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

        commands.Received(1).StartCreateTagDialog(form, revision: null);
        commands.Received(1).StartDeleteTagDialog(form, tag: null);
    }

    [AvaloniaTest]
    public void FormCheckoutBranch_should_use_existing_translation_keys()
    {
        FormCheckoutBranch form = new();
        ITranslation translation = Substitute.For<ITranslation>();
        translation.TranslateItem(
                nameof(FormCheckoutBranch),
                "$this",
                "Text",
                Arg.Any<Func<string?>>())
            .Returns("Translated checkout title");

        form.AddTranslationItems(translation);
        form.TranslateItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormCheckoutBranch), "$this", "Text", "Checkout branch");
        translation.Received(1).AddTranslationItem(nameof(FormCheckoutBranch), "Ok", "Text", "&Checkout");
        translation.Received(1).AddTranslationItem(nameof(FormCheckoutBranch), "LocalBranch", "Text", "Local &branch");
        translation.Received(1).AddTranslationItem(nameof(FormCheckoutBranch), "label1", "Text", "&Select branch");
        translation.Received(1).AddTranslationItem(nameof(FormCheckoutBranch), "localChangesGB", "Text", "Local changes");
        translation.Received(1).AddTranslationItem(nameof(FormCheckoutBranch), "rbDontChange", "Text", "Do&n't change");
        translation.Received(1).AddTranslationItem(
            nameof(FormCheckoutBranch),
            "_invalidBranchName",
            "Text",
            "An existing branch must be selected.");
        translation.DidNotReceive().AddTranslationItem(
            nameof(FormCheckoutBranch),
            "$this",
            "Title",
            Arg.Any<string>());
        translation.DidNotReceive().TranslateItem(
            nameof(FormCheckoutBranch),
            "$this",
            "Title",
            Arg.Any<Func<string?>>());
        form.Title.Should().Be("Translated checkout title");

        string[] emittedKeys = translation.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(ITranslation.AddTranslationItem))
            .Select(call => string.Join('.', call.GetArguments().Take(3)))
            .ToArray();
        emittedKeys.Distinct(StringComparer.Ordinal).Count().Should().Be(
            emittedKeys.Length,
            "each field must be routed through exactly one translation path");
    }

    [AvaloniaTest]
    public void RevisionGridControl_should_construct()
    {
        RevisionGridControl control = new();
        control.Should().NotBeNull();
    }

    [AvaloniaTest]
    public void RevisionGridControl_context_menu_should_use_existing_translation_keys()
    {
        RevisionGridControl control = new();
        ITranslation translation = Substitute.For<ITranslation>();

        control.AddTranslationItems(translation);
        control.TranslateItems(translation);

        translation.Received(1).AddTranslationItem(
            nameof(RevisionGridControl),
            "checkoutBranchToolStripMenuItem",
            "Text",
            "Chec&kout branch...");
        translation.Received(1).AddTranslationItem(
            nameof(RevisionGridControl),
            "createNewBranchToolStripMenuItem",
            "Text",
            "Create new branch here (&x)...");
        translation.Received(1).AddTranslationItem(
            nameof(RevisionGridControl),
            "createTagToolStripMenuItem",
            "Text",
            "Create new ta&g here...");
    }

    [AvaloniaTest]
    public void RevisionGridControl_context_menu_should_be_disabled_without_a_selection()
    {
        RevisionGridControl control = new();
        Window window = new() { Content = control };
        window.Show();
        try
        {
            ContextMenu contextMenu = control.FindControl<ContextMenu>("revisionContextMenu")
                ?? throw new InvalidOperationException("Revision context menu was not created.");
            MenuItem checkoutBranch = control.FindControl<MenuItem>("checkoutBranchToolStripMenuItem")
                ?? throw new InvalidOperationException("Checkout-branch menu item was not created.");
            MenuItem createBranch = control.FindControl<MenuItem>("createNewBranchToolStripMenuItem")
                ?? throw new InvalidOperationException("Create-branch menu item was not created.");
            MenuItem createTag = control.FindControl<MenuItem>("createTagToolStripMenuItem")
                ?? throw new InvalidOperationException("Create-tag menu item was not created.");
            ListBox revisions = control.FindControl<ListBox>("lstRevisions")
                ?? throw new InvalidOperationException("Revision list was not created.");

            contextMenu.Open(revisions);
            Dispatcher.UIThread.RunJobs();

            checkoutBranch.IsEnabled.Should().BeFalse();
            createBranch.IsEnabled.Should().BeFalse();
            createTag.IsEnabled.Should().BeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void FileStatusList_should_construct()
    {
        FileStatusList control = new();
        control.Should().NotBeNull();
    }

    [AvaloniaTest]
    public void FileViewer_should_construct_and_view_a_patch()
    {
        FileViewer control = new();
        control.ViewPatch("@@ -1,1 +1,1 @@\n-old\n+new\n");
        control.Should().NotBeNull();
    }

    [AvaloniaTest]
    public void CommitInfo_should_construct()
    {
        CommitInfo control = new();
        control.Revision.Should().BeNull();
    }

    [AvaloniaTest]
    public void RepoObjectsTree_should_construct()
    {
        RepoObjectsTree control = new();
        control.SetRefs([]);
        control.SelectedRef.Should().BeNull();
    }
}
