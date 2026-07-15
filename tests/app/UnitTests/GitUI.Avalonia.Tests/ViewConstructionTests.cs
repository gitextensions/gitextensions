using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
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
        MenuItem fetchAll = form.FindControl<MenuItem>("fetchAllToolStripMenuItem")
            ?? throw new InvalidOperationException("Fetch-all menu item was not created.");
        fetchAll.Header.Should().Be("_Fetch translated");
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
    public void FormCheckoutBranch_should_use_existing_translation_keys()
    {
        FormCheckoutBranch form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormCheckoutBranch), "Ok", "Text", "&Checkout");
        translation.Received(1).AddTranslationItem(nameof(FormCheckoutBranch), "LocalBranch", "Text", "Local &branch");
        translation.Received(1).AddTranslationItem(nameof(FormCheckoutBranch), "label1", "Text", "&Select branch");
        translation.Received(1).AddTranslationItem(nameof(FormCheckoutBranch), "localChangesGB", "Text", "Local changes");
        translation.Received(1).AddTranslationItem(nameof(FormCheckoutBranch), "rbDontChange", "Text", "Do&n't change");
    }

    [AvaloniaTest]
    public void RevisionGridControl_should_construct()
    {
        RevisionGridControl control = new();
        control.Should().NotBeNull();
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
