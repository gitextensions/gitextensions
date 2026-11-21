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

        GitArgumentBuilder args = FormCreateWorktree.GetTestAccessor().CreateWorktreeCommand(module, "feature-dir", "origin/feature");

        args.ToString().Should().Be("worktree add feature-dir origin/feature");
    }

    [Test]
    public void CreateWorktreeCommand_should_build_worktree_add_with_local_branch_start_point()
    {
        IGitModule module = NewModuleWithRelativePathsSet();

        GitArgumentBuilder args = FormCreateWorktree.GetTestAccessor().CreateWorktreeCommand(module, "feature-dir", "feature");

        args.ToString().Should().Be("worktree add feature-dir feature");
    }

    [Test]
    public void CreateWorktreeCommand_should_build_new_branch_start_point()
    {
        IGitModule module = NewModuleWithRelativePathsSet();

        GitArgumentBuilder args = FormCreateWorktree.GetTestAccessor().CreateWorktreeCommand(module, "feature-dir", "-b feature");

        args.ToString().Should().Be("worktree add feature-dir -b feature");
    }

    [Test]
    public void CreateWorktreeCommand_should_add_useRelativePaths_config_item_when_setting_is_unset()
    {
        IGitModule module = Substitute.For<IGitModule>();

        GitArgumentBuilder args = FormCreateWorktree.GetTestAccessor().CreateWorktreeCommand(module, "feature-dir", "origin/feature");

        args.ToString().Should().Be("-c worktree.useRelativePaths=true worktree add feature-dir origin/feature");
    }

    [Test]
    public void GetCheckoutableBranches_should_return_local_branches_followed_by_remote_branches()
    {
        IReadOnlyList<IGitRef> result = FormCreateWorktree.GetTestAccessor().GetCheckoutableBranches(
            [CreateBranch("master"), CreateBranch("feature")],
            [CreateRemoteBranch("origin/remote-only"), CreateRemoteBranch("origin/another-remote")],
            "master");

        result.Select(b => b.Name).Should().ContainInOrder("feature", "origin/remote-only", "origin/another-remote");
    }

    [Test]
    public void GetCheckoutableBranches_should_exclude_selected_branch()
    {
        IReadOnlyList<IGitRef> result = FormCreateWorktree.GetTestAccessor().GetCheckoutableBranches(
            [CreateBranch("master"), CreateBranch("feature")],
            [],
            "master");

        result.Select(b => b.Name).Should().ContainInOrder("feature");
    }

    [Test]
    public void GetCheckoutableBranches_should_exclude_remote_branches_whose_local_name_conflicts_with_local_branch()
    {
        IReadOnlyList<IGitRef> result = FormCreateWorktree.GetTestAccessor().GetCheckoutableBranches(
            [CreateBranch("master"), CreateBranch("feature")],
            [CreateRemoteBranch("origin/feature"), CreateRemoteBranch("origin/master"), CreateRemoteBranch("origin/remote-only")],
            "master");

        result.Select(b => b.Name).Should().ContainInOrder("feature", "origin/remote-only");
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

    private static IGitRef CreateRemoteBranch(string name)
    {
        // name is like "origin/feature"; LocalName is the tracking branch name ("feature").
        string localName = name[(name.IndexOf('/') + 1)..];
        IGitRef gitRef = Substitute.For<IGitRef>();
        gitRef.Name.Returns(name);
        gitRef.LocalName.Returns(localName);
        return gitRef;
    }
}
