using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
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

    [AvaloniaTest]
    public void FormManageWorktree_should_enable_actions_only_for_live_noncurrent_worktrees()
    {
        string root = Path.Combine(Path.GetTempPath(), $"gitextensions-worktrees-{Guid.NewGuid():N}");
        string mainPath = Path.Combine(root, "main");
        string linkedPath = Path.Combine(root, "feature");
        string deletedPath = Path.Combine(root, "deleted");
        IReadOnlyList<GitWorktree> worktrees =
        [
            new(mainPath, GitWorktreeHeadType.Branch, "1111111", "main", IsDeleted: false),
            new(linkedPath, GitWorktreeHeadType.Branch, "2222222", "feature", IsDeleted: false),
            new(deletedPath, GitWorktreeHeadType.Branch, "3333333", "old", IsDeleted: true),
        ];
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(mainPath);
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        FormManageWorktree form = new(commands);
        FormManageWorktree.TestAccessor accessor = form.GetTestAccessor();

        accessor.SetWorktrees(worktrees);

        accessor.Open.IsEnabled.Should().BeFalse();
        accessor.Delete.IsEnabled.Should().BeFalse();
        accessor.Prune.IsEnabled.Should().BeTrue();

        accessor.Worktrees.SelectedItem = worktrees[1];
        accessor.Open.IsEnabled.Should().BeTrue();
        accessor.Delete.IsEnabled.Should().BeTrue();

        accessor.Worktrees.SelectedItem = worktrees[2];
        accessor.Open.IsEnabled.Should().BeFalse();
        accessor.Delete.IsEnabled.Should().BeFalse();
    }

    [AvaloniaTest]
    public void FormManageWorktree_should_forward_selected_and_create_actions()
    {
        string root = Path.Combine(Path.GetTempPath(), $"gitextensions-worktrees-{Guid.NewGuid():N}");
        GitWorktree main = new(Path.Combine(root, "main"), GitWorktreeHeadType.Branch, "1111111", "main", IsDeleted: false);
        GitWorktree linked = new(Path.Combine(root, "feature"), GitWorktreeHeadType.Branch, "2222222", "feature", IsDeleted: false);
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(main.Path);
        module.GetWorktrees().Returns([main, linked]);
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.WorktreeCreate(Arg.Any<GitExtensions.Shims.WinForms.IWin32Window>(), main.Path).Returns(true);
        FormManageWorktree form = new(commands);
        FormManageWorktree.TestAccessor accessor = form.GetTestAccessor();
        accessor.SetWorktrees([main, linked], linked.Path);

        accessor.Open.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
        accessor.Delete.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
        accessor.Create.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));

        commands.Received(1).WorktreeSwitch(form, linked.Path);
        commands.Received(1).WorktreeDelete(form, linked.Path);
        commands.Received(1).WorktreeCreate(form, main.Path);
        form.ShouldRefreshRevisionGrid.Should().BeTrue();
    }

    [AvaloniaTest]
    public void FormManageWorktree_should_compare_paths_with_host_semantics()
    {
        string path = Path.Combine(Path.GetTempPath(), "GitExtensions", "Worktree");
        string pathWithTrailingSeparator = path + Path.DirectorySeparatorChar;

        FormManageWorktree.TestAccessor.PathsEqual(path, pathWithTrailingSeparator).Should().BeTrue();
        FormManageWorktree.TestAccessor.PathsEqual(path, path.ToUpperInvariant()).Should().Be(OperatingSystem.IsWindows());
    }

    [AvaloniaTest]
    public void WorktreeSwitch_should_accept_an_existing_directory_without_a_browse_owner()
    {
        string worktreePath = Path.Combine(Path.GetTempPath(), $"gitextensions-switch-{Guid.NewGuid():N}");
        Directory.CreateDirectory(worktreePath);
        bool oldConfirmation = AppSettings.DontConfirmSwitchWorktree;
        try
        {
            AppSettings.DontConfirmSwitchWorktree = true;
            GitUICommands commands = new(Substitute.For<IServiceProvider>(), Substitute.For<IGitModule>());

            commands.WorktreeSwitch(owner: null, worktreePath).Should().BeTrue();
            commands.WorktreeSwitch(owner: null, Path.Combine(worktreePath, "missing")).Should().BeFalse();
        }
        finally
        {
            AppSettings.DontConfirmSwitchWorktree = oldConfirmation;
            Directory.Delete(worktreePath);
        }
    }

    [AvaloniaTest]
    public void FormManageWorktree_should_use_the_existing_translation_keys_once()
    {
        FormManageWorktree form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);
        form.TranslateItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormManageWorktree), "$this", "Text", "Existing worktrees");
        translation.Received(1).AddTranslationItem(nameof(FormManageWorktree), "Path", "HeaderText", "Path");
        translation.Received(1).AddTranslationItem(nameof(FormManageWorktree), "Type", "HeaderText", "Type");
        translation.Received(1).AddTranslationItem(nameof(FormManageWorktree), "Branch", "HeaderText", "Branch");
        translation.Received(1).AddTranslationItem(nameof(FormManageWorktree), "Sha1", "HeaderText", "SHA-1");
        translation.Received(1).AddTranslationItem(nameof(FormManageWorktree), "buttonCreateNewWorktree", "Text", "&Create...");
        translation.Received(1).AddTranslationItem(nameof(FormManageWorktree), "buttonDeleteSelectedWorktree", "Text", "&Delete selected");
        translation.Received(1).AddTranslationItem(nameof(FormManageWorktree), "buttonOpenSelectedWorktree", "Text", "&Open selected");
        translation.Received(1).AddTranslationItem(nameof(FormManageWorktree), "buttonPruneWorktrees", "Text", "&Prune deleted worktrees");
    }

    private static IGitRef CreateBranch(string name)
    {
        IGitRef branch = Substitute.For<IGitRef>();
        branch.Name.Returns(name);
        branch.LocalName.Returns(name);
        return branch;
    }
}
