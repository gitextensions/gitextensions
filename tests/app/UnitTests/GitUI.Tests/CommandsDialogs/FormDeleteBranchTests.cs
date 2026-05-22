using GitExtensions.Extensibility.Git;
using GitUI.CommandsDialogs;
using NSubstitute;

namespace GitUITests.CommandsDialogs;
public sealed class FormDeleteBranchTests
{
    private const string MainWorktreePath = @"C:\repos\main";
    private const string LinkedWorktreePath = @"C:\repos\feature";
    private const string SecondLinkedWorktreePath = @"C:\repos\hotfix";

    [Test]
    public void ClassifyWorktreeBranches_should_return_empty_when_no_branches_match()
    {
        IGitRef[] branches = [CreateBranch("unrelated")];
        List<GitWorktree> worktrees =
        [
            new(MainWorktreePath, GitWorktreeHeadType.Branch, "aaa", "master", IsDeleted: false),
            new(LinkedWorktreePath, GitWorktreeHeadType.Branch, "bbb", "feature", IsDeleted: false),
        ];

        FormDeleteBranch.WorktreeBranchClassification result =
            FormDeleteBranch.ClassifyWorktreeBranches(branches, worktrees, MainWorktreePath);

        result.HasDeletedWorktrees.Should().BeFalse();
        result.MainWorktreeBranches.Should().BeEmpty();
        result.LinkedWorktreeBranches.Should().BeEmpty();
    }

    [Test]
    public void ClassifyWorktreeBranches_should_classify_branch_in_main_worktree()
    {
        IGitRef masterRef = CreateBranch("master");
        IGitRef[] branches = [masterRef];
        List<GitWorktree> worktrees =
        [
            new(MainWorktreePath, GitWorktreeHeadType.Branch, "aaa", "master", IsDeleted: false),
            new(LinkedWorktreePath, GitWorktreeHeadType.Branch, "bbb", "feature", IsDeleted: false),
        ];

        FormDeleteBranch.WorktreeBranchClassification result =
            FormDeleteBranch.ClassifyWorktreeBranches(branches, worktrees, LinkedWorktreePath);

        result.MainWorktreeBranches.Should().ContainSingle()
            .Which.Branch.Name.Should().Be("master");
        result.LinkedWorktreeBranches.Should().BeEmpty();
        result.HasDeletedWorktrees.Should().BeFalse();
    }

    [Test]
    public void ClassifyWorktreeBranches_should_classify_branch_in_linked_worktree()
    {
        IGitRef featureRef = CreateBranch("feature");
        IGitRef[] branches = [featureRef];
        List<GitWorktree> worktrees =
        [
            new(MainWorktreePath, GitWorktreeHeadType.Branch, "aaa", "master", IsDeleted: false),
            new(LinkedWorktreePath, GitWorktreeHeadType.Branch, "bbb", "feature", IsDeleted: false),
        ];

        FormDeleteBranch.WorktreeBranchClassification result =
            FormDeleteBranch.ClassifyWorktreeBranches(branches, worktrees, MainWorktreePath);

        result.LinkedWorktreeBranches.Should().ContainSingle()
            .Which.Branch.Name.Should().Be("feature");
        result.MainWorktreeBranches.Should().BeEmpty();
        result.HasDeletedWorktrees.Should().BeFalse();
    }

    [Test]
    public void ClassifyWorktreeBranches_should_skip_current_worktree()
    {
        IGitRef featureRef = CreateBranch("feature");
        IGitRef[] branches = [featureRef];
        List<GitWorktree> worktrees =
        [
            new(MainWorktreePath, GitWorktreeHeadType.Branch, "aaa", "master", IsDeleted: false),
            new(LinkedWorktreePath, GitWorktreeHeadType.Branch, "bbb", "feature", IsDeleted: false),
        ];

        // Current dir matches the linked worktree — should be skipped
        FormDeleteBranch.WorktreeBranchClassification result =
            FormDeleteBranch.ClassifyWorktreeBranches(branches, worktrees, LinkedWorktreePath);

        result.LinkedWorktreeBranches.Should().BeEmpty();
        result.MainWorktreeBranches.Should().BeEmpty();
    }

    [Test]
    public void ClassifyWorktreeBranches_should_detect_deleted_worktrees()
    {
        IGitRef staleRef = CreateBranch("stale-branch");
        IGitRef[] branches = [staleRef];
        List<GitWorktree> worktrees =
        [
            new(MainWorktreePath, GitWorktreeHeadType.Branch, "aaa", "master", IsDeleted: false),
            new(LinkedWorktreePath, GitWorktreeHeadType.Branch, "bbb", "stale-branch", IsDeleted: true),
        ];

        FormDeleteBranch.WorktreeBranchClassification result =
            FormDeleteBranch.ClassifyWorktreeBranches(branches, worktrees, MainWorktreePath);

        result.HasDeletedWorktrees.Should().BeTrue();
        result.LinkedWorktreeBranches.Should().BeEmpty();
        result.MainWorktreeBranches.Should().BeEmpty();
    }

    [Test]
    public void ClassifyWorktreeBranches_should_not_flag_deleted_worktrees_for_unrelated_branches()
    {
        IGitRef[] branches = [CreateBranch("unrelated")];
        List<GitWorktree> worktrees =
        [
            new(MainWorktreePath, GitWorktreeHeadType.Branch, "aaa", "master", IsDeleted: false),
            new(LinkedWorktreePath, GitWorktreeHeadType.Branch, "bbb", "stale-branch", IsDeleted: true),
        ];

        FormDeleteBranch.WorktreeBranchClassification result =
            FormDeleteBranch.ClassifyWorktreeBranches(branches, worktrees, MainWorktreePath);

        result.HasDeletedWorktrees.Should().BeFalse();
    }

    [Test]
    public void ClassifyWorktreeBranches_should_skip_detached_and_bare_worktrees()
    {
        IGitRef[] branches = [CreateBranch("any")];
        List<GitWorktree> worktrees =
        [
            new(MainWorktreePath, GitWorktreeHeadType.Bare, null, null, IsDeleted: false),
            new(LinkedWorktreePath, GitWorktreeHeadType.Detached, "ccc", null, IsDeleted: false),
        ];

        FormDeleteBranch.WorktreeBranchClassification result =
            FormDeleteBranch.ClassifyWorktreeBranches(branches, worktrees, @"C:\repos\other");

        result.MainWorktreeBranches.Should().BeEmpty();
        result.LinkedWorktreeBranches.Should().BeEmpty();
        result.HasDeletedWorktrees.Should().BeFalse();
    }

    [Test]
    public void ClassifyWorktreeBranches_should_handle_mixed_scenario()
    {
        IGitRef masterRef = CreateBranch("master");
        IGitRef featureRef = CreateBranch("feature");
        IGitRef hotfixRef = CreateBranch("hotfix");
        IGitRef safeRef = CreateBranch("safe-to-delete");
        IGitRef[] branches = [masterRef, featureRef, hotfixRef, safeRef];
        List<GitWorktree> worktrees =
        [
            new(MainWorktreePath, GitWorktreeHeadType.Branch, "aaa", "master", IsDeleted: false),
            new(LinkedWorktreePath, GitWorktreeHeadType.Branch, "bbb", "feature", IsDeleted: false),
            new(SecondLinkedWorktreePath, GitWorktreeHeadType.Branch, "ccc", "hotfix", IsDeleted: false),
        ];

        FormDeleteBranch.WorktreeBranchClassification result =
            FormDeleteBranch.ClassifyWorktreeBranches(branches, worktrees, @"C:\repos\other");

        result.MainWorktreeBranches.Should().ContainSingle()
            .Which.Branch.Name.Should().Be("master");
        result.LinkedWorktreeBranches.Should().HaveCount(2);
        result.LinkedWorktreeBranches.Select(x => x.Branch.Name)
            .Should().BeEquivalentTo(["feature", "hotfix"]);
        result.HasDeletedWorktrees.Should().BeFalse();
    }

    [Test]
    public void ClassifyWorktreeBranches_should_be_case_insensitive_on_paths()
    {
        IGitRef featureRef = CreateBranch("feature");
        IGitRef[] branches = [featureRef];
        List<GitWorktree> worktrees =
        [
            new(MainWorktreePath, GitWorktreeHeadType.Branch, "aaa", "master", IsDeleted: false),
            new(@"C:\Repos\Feature", GitWorktreeHeadType.Branch, "bbb", "feature", IsDeleted: false),
        ];

        // Current dir is lowercase version of the linked worktree path
        FormDeleteBranch.WorktreeBranchClassification result =
            FormDeleteBranch.ClassifyWorktreeBranches(branches, worktrees, @"C:\repos\feature");

        result.LinkedWorktreeBranches.Should().BeEmpty();
    }

    private static IGitRef CreateBranch(string name)
    {
        IGitRef gitRef = Substitute.For<IGitRef>();
        gitRef.Name.Returns(name);
        gitRef.CompleteName.Returns($"refs/heads/{name}");
        gitRef.IsHead.Returns(true);
        return gitRef;
    }
}
