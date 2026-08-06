using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.HelperDialogs;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
public sealed class BranchOperationsTests
{
    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
    }

    [AvaloniaTest]
    public void FormDeleteRemoteBranch_should_construct_with_the_original_controls()
    {
        FormDeleteRemoteBranch form = new();
        FormDeleteRemoteBranch.TestAccessor accessor = form.GetTestAccessor();

        accessor.Delete.Should().NotBeNull();
        accessor.DeleteRemote.Should().NotBeNull();
        accessor.DeleteLocalTrackingBranch.Should().NotBeNull();
        accessor.Branches.Should().NotBeNull();

        // Delete is disabled until the remote checkbox is set, mirroring the original.
        accessor.Delete.IsEnabled.Should().BeFalse();
    }

    [AvaloniaTest]
    public void FormDeleteRemoteBranch_should_emit_its_translation_keys()
    {
        FormDeleteRemoteBranch form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormDeleteRemoteBranch), "$this", "Text", "Delete branch");
        translation.Received(1).AddTranslationItem(nameof(FormDeleteRemoteBranch), "Delete", "Text", "&Delete");
        translation.Received(1).AddTranslationItem(nameof(FormDeleteRemoteBranch), "labelSelectBranches", "Text", "Select &branches");
        translation.Received(1).AddTranslationItem(nameof(FormDeleteRemoteBranch), "DeleteRemote", "Text", "Delete branch(es) from &remote repository");
        translation.Received(1).AddTranslationItem(nameof(FormDeleteRemoteBranch), "DeleteLocalTrackingBranch", "Text", "Delete &local tracking branch (if available)");
    }

    [AvaloniaTest]
    public void FormDeleteRemoteBranch_delete_button_follows_the_remote_checkbox()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(Path.Combine(Path.GetTempPath(), "ge-delete-remote"));
        module.GetMergedRemoteBranches().Returns([]);
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);

        FormDeleteRemoteBranch form = new(commands, string.Empty);
        FormDeleteRemoteBranch.TestAccessor accessor = form.GetTestAccessor();

        accessor.Delete.IsEnabled.Should().BeFalse();

        accessor.DeleteRemote.IsChecked = true;
        accessor.Delete.IsEnabled.Should().BeTrue();

        accessor.DeleteRemote.IsChecked = false;
        accessor.Delete.IsEnabled.Should().BeFalse();

        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaTest]
    public void FormResetAnotherBranch_should_construct_with_the_original_controls()
    {
        FormResetAnotherBranch form = new();
        FormResetAnotherBranch.TestAccessor accessor = form.GetTestAccessor();

        accessor.Branches.Should().NotBeNull();
        accessor.Ok.Should().NotBeNull();
        accessor.Cancel.Should().NotBeNull();
        accessor.ForceReset.Should().NotBeNull();
        accessor.CheckoutBranch.Should().NotBeNull();
    }

    [AvaloniaTest]
    public void FormResetAnotherBranch_should_emit_its_translation_keys()
    {
        FormResetAnotherBranch form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormResetAnotherBranch), "$this", "Text", "Reset branch");
        translation.Received(1).AddTranslationItem(nameof(FormResetAnotherBranch), "BranchInfo", "Text", "Reset local &branch:");
        translation.Received(1).AddTranslationItem(nameof(FormResetAnotherBranch), "Ok", "Text", "OK");
        translation.Received(1).AddTranslationItem(nameof(FormResetAnotherBranch), "Cancel", "Text", "Cancel");
        translation.Received(1).AddTranslationItem(nameof(FormResetAnotherBranch), "ForceReset", "Text", "&Force reset for a non-fast-forward reset");
        translation.Received(1).AddTranslationItem(nameof(FormResetAnotherBranch), "cbxCheckoutBranch", "Text", "Chec&kout branch after reset");
        translation.Received(1).AddTranslationItem(nameof(FormResetAnotherBranch), "_localRefInvalid", "Text", "The entered value '{0}' is not the name of an existing local branch.");
    }

    [AvaloniaTest]
    public void FormResetAnotherBranch_should_load_the_branches_and_revision_summary()
    {
        GitRevision revision = new(ObjectId.Random());
        IGitModule module = Substitute.For<IGitModule>();
        module.GetSelectedBranch().Returns("main");
        module.GetRefs(RefsFilter.Heads).Returns([]);
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);

        FormResetAnotherBranch form = FormResetAnotherBranch.Create(commands, revision);
        FormResetAnotherBranch.TestAccessor accessor = form.GetTestAccessor();

        accessor.Load();

        accessor.SummaryRevision.Should().BeSameAs(revision);
        accessor.Ok.IsEnabled.Should().BeFalse();

        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaTest]
    public void Revision_grid_should_emit_the_reset_another_branch_translation_key()
    {
        RevisionGridControl control = new();
        ITranslation translation = Substitute.For<ITranslation>();

        control.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(
            nameof(RevisionGridControl),
            "resetAnotherBranchToHereToolStripMenuItem",
            "Text",
            "Reset an&other branch to here...");
    }
}
