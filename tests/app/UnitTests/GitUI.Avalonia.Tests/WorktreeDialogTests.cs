using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs.WorktreeDialog;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
public sealed class WorktreeDialogTests
{
    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
    }

    [AvaloniaTest]
    public void FormCreateWorktree_should_update_the_target_path_and_validation_from_the_selected_mode()
    {
        string mainPath = Path.Combine(Path.GetTempPath(), $"gitextensions-worktree-{Guid.NewGuid():N}");
        IGitModule module = Substitute.For<IGitModule>();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        FormCreateWorktree form = new(commands, mainPath);
        FormCreateWorktree.TestAccessor accessor = form.GetTestAccessor();
        IGitRef feature = CreateBranch("feature");
        IGitRef topic = CreateBranch("topic/test");

        accessor.SetBranches([feature, topic]);

        accessor.Branches.SelectedItem.Should().BeSameAs(feature);
        accessor.WorktreeDirectory.Text.Should().Be($"{mainPath}_feature");
        accessor.Create.IsEnabled.Should().BeTrue();

        accessor.SelectNewBranch("topic/test");

        accessor.WorktreeDirectory.Text.Should().Be($"{mainPath}_topic_test");
        accessor.Create.IsEnabled.Should().BeFalse("an existing branch cannot be recreated");
    }

    [AvaloniaTest]
    public void FormCreateWorktree_should_fall_back_to_new_branch_mode_when_no_existing_branch_is_available()
    {
        FormCreateWorktree form = new();
        FormCreateWorktree.TestAccessor accessor = form.GetTestAccessor();

        accessor.SetBranches([]);

        accessor.CheckoutExisting.IsEnabled.Should().BeFalse();
        accessor.CreateNew.IsChecked.Should().BeTrue();
        accessor.NewBranchName.IsEnabled.Should().BeTrue();
        accessor.Create.IsEnabled.Should().BeFalse();
    }

    [AvaloniaTest]
    public void FormCreateWorktree_should_build_the_original_relative_path_command()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.GetEffectiveSetting("worktree.useRelativePaths").Returns((string?)null);
        FormCreateWorktree form = new();

        ArgumentString command = form.GetTestAccessor().CreateCommand(module, "\"../repo_feature\"", "refs/heads/feature");

        command.ToString().Should().Contain("worktree add \"../repo_feature\" refs/heads/feature");
        command.ToString().Should().Contain("worktree.useRelativePaths=true");
    }

    [AvaloniaTest]
    public void FormCreateWorktree_should_use_the_existing_translation_keys_once()
    {
        FormCreateWorktree form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);
        form.TranslateItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormCreateWorktree), "$this", "Text", "Create a new worktree");
        translation.Received(1).AddTranslationItem(nameof(FormCreateWorktree), "btnCreateWorktree", "Text", "&Create the new worktree");
        translation.Received(1).AddTranslationItem(nameof(FormCreateWorktree), "chkOpenWorktree", "Text", "&Open the new worktree after the creation");
        translation.Received(1).AddTranslationItem(nameof(FormCreateWorktree), "gbxWhatToCheckout", "Text", "What to checkout:");
        translation.Received(1).AddTranslationItem(nameof(FormCreateWorktree), "lblNewWorktreeFolder", "Text", "New worktree &directory:");
        translation.Received(1).AddTranslationItem(nameof(FormCreateWorktree), "rbCheckoutExistingBranch", "Text", "Checkout an &existing branch:");
        translation.Received(1).AddTranslationItem(nameof(FormCreateWorktree), "rbCreateNewBranch", "Text", "Create a &new branch:\n(from current commit)");
    }

    private static IGitRef CreateBranch(string name)
    {
        IGitRef branch = Substitute.For<IGitRef>();
        branch.Name.Returns(name);
        branch.LocalName.Returns(name);
        return branch;
    }
}
