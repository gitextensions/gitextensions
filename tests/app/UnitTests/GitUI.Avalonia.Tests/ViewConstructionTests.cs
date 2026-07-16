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
using NSubstitute;

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
            "pullToolStripMenuItem",
            "Text",
            "Pull&/Fetch...");
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

        FormBrowse form = new(commands);
        MenuItem stash = form.FindControl<MenuItem>("stashToolStripMenuItem")
            ?? throw new InvalidOperationException("Manage-stashes menu item was not created.");
        stash.IsEnabled = true;

        stash.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

        commands.Received(1).StartStashDialog(form);
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

        FormBrowse form = new(commands);
        MenuItem pull = form.FindControl<MenuItem>("pullToolStripMenuItem")
            ?? throw new InvalidOperationException("Pull menu item was not created.");
        pull.IsEnabled = true;

        pull.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

        commands.Received(1).StartPullDialog(form);
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

        FormBrowse form = new(commands);
        MenuItem createBranch = form.FindControl<MenuItem>("branchToolStripMenuItem")
            ?? throw new InvalidOperationException("Create-branch menu item was not created.");
        createBranch.IsEnabled = true;

        createBranch.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

        commands.Received(1).StartCreateBranchDialog(form, default(ObjectId));
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
            ListBox revisions = control.FindControl<ListBox>("lstRevisions")
                ?? throw new InvalidOperationException("Revision list was not created.");

            contextMenu.Open(revisions);
            Dispatcher.UIThread.RunJobs();

            checkoutBranch.IsEnabled.Should().BeFalse();
            createBranch.IsEnabled.Should().BeFalse();
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
