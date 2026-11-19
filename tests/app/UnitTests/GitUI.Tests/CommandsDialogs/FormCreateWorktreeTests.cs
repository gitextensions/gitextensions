using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI.CommandsDialogs.WorktreeDialog;
using NSubstitute;

namespace GitUITests.CommandsDialogs;

public sealed class FormCreateWorktreeTests
{
    private const string UseRelativePathsSetting = "worktree.useRelativePaths";

    [Test]
    public void CreateWorktreeCommand_should_build_worktree_add_with_remote_branch_start_point()
    {
        IGitModule module = NewModuleWithRelativePathsSet();

        GitArgumentBuilder args = FormCreateWorktree.CreateWorktreeCommand(module, "feature-dir", "origin/feature");

        args.ToString().Should().Be("worktree add feature-dir origin/feature");
    }

    [Test]
    public void CreateWorktreeCommand_should_build_worktree_add_with_local_branch_start_point()
    {
        IGitModule module = NewModuleWithRelativePathsSet();

        GitArgumentBuilder args = FormCreateWorktree.CreateWorktreeCommand(module, "feature-dir", "feature");

        args.ToString().Should().Be("worktree add feature-dir feature");
    }

    [Test]
    public void CreateWorktreeCommand_should_build_new_branch_start_point()
    {
        IGitModule module = NewModuleWithRelativePathsSet();

        GitArgumentBuilder args = FormCreateWorktree.CreateWorktreeCommand(module, "feature-dir", "-b feature");

        args.ToString().Should().Be("worktree add feature-dir -b feature");
    }

    [Test]
    public void CreateWorktreeCommand_should_add_useRelativePaths_config_item_when_setting_is_unset()
    {
        IGitModule module = Substitute.For<IGitModule>();

        GitArgumentBuilder args = FormCreateWorktree.CreateWorktreeCommand(module, "feature-dir", "origin/feature");

        args.ToString().Should().Be("-c worktree.useRelativePaths=true worktree add feature-dir origin/feature");
    }

    [Test]
    public void GetCheckoutableBranches_should_return_local_branches_followed_by_remote_branches()
    {
        IReadOnlyList<IGitRef> result = FormCreateWorktree.GetCheckoutableBranches(
            [CreateBranch("master"), CreateBranch("feature")],
            [CreateBranch("origin/feature"), CreateBranch("origin/master")],
            "master");

        result.Select(b => b.Name).Should().ContainInOrder("feature", "origin/feature", "origin/master");
    }

    [Test]
    public void GetCheckoutableBranches_should_exclude_selected_branch()
    {
        IReadOnlyList<IGitRef> result = FormCreateWorktree.GetCheckoutableBranches(
            [CreateBranch("master"), CreateBranch("feature")],
            [],
            "master");

        result.Select(b => b.Name).Should().ContainInOrder("feature");
    }

    private static IGitModule NewModuleWithRelativePathsSet()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.GetEffectiveSetting(UseRelativePathsSetting, "").Returns("true");
        return module;
    }

    private static IGitRef CreateBranch(string name)
    {
        IGitRef gitRef = Substitute.For<IGitRef>();
        gitRef.Name.Returns(name);
        gitRef.LocalName.Returns(name);
        return gitRef;
    }
}
