using GitUI.LeftPanel;

namespace GitUITests.LeftPanel;

public sealed class WorktreeTreeTests
{
    private static readonly char Sep = Path.DirectorySeparatorChar;

    [Test]
    public void GetCommonPrefix_should_return_empty_for_no_paths()
    {
        string result = WorktreeTree.GetCommonPrefix([]);

        result.Should().BeEmpty();
    }

    [Test]
    public void GetCommonPrefix_should_return_empty_for_single_path()
    {
        // A single path has nothing to compare against — no common prefix to strip.
        string[] paths = [$"worktrees{Sep}feature-a"];

        string result = WorktreeTree.GetCommonPrefix(paths);

        result.Should().BeEmpty();
    }

    [Test]
    public void GetCommonPrefix_should_return_common_prefix_for_sibling_paths()
    {
        string[] paths =
        [
            $"my-repo.worktrees{Sep}feature-a",
            $"my-repo.worktrees{Sep}feature-b",
        ];

        string result = WorktreeTree.GetCommonPrefix(paths);

        result.Should().Be($"my-repo.worktrees{Sep}");
    }

    [Test]
    public void GetCommonPrefix_should_return_empty_when_paths_share_no_directory_prefix()
    {
        string[] paths =
        [
            $"folder-a{Sep}feature-a",
            $"folder-b{Sep}feature-b",
        ];

        string result = WorktreeTree.GetCommonPrefix(paths);

        result.Should().BeEmpty();
    }

    [Test]
    public void GetCommonPrefix_should_return_partial_prefix_for_mixed_depth()
    {
        string[] paths =
        [
            $"root{Sep}team-a{Sep}feature-1",
            $"root{Sep}team-a{Sep}feature-2",
            $"root{Sep}team-b{Sep}feature-3",
        ];

        string result = WorktreeTree.GetCommonPrefix(paths);

        result.Should().Be($"root{Sep}");
    }

    [Test]
    public void GetCommonPrefix_should_handle_nested_common_prefix()
    {
        string[] paths =
        [
            $"root{Sep}sub{Sep}feature-1",
            $"root{Sep}sub{Sep}feature-2",
        ];

        string result = WorktreeTree.GetCommonPrefix(paths);

        result.Should().Be($"root{Sep}sub{Sep}");
    }

    [Test]
    public void GetCommonPrefix_should_be_case_insensitive()
    {
        string[] paths =
        [
            $"Worktrees{Sep}feature-a",
            $"worktrees{Sep}feature-b",
        ];

        string result = WorktreeTree.GetCommonPrefix(paths);

        result.Should().Be($"Worktrees{Sep}");
    }

    [Test]
    public void GetCommonPrefix_should_strip_underscore_delimited_prefix()
    {
        string[] paths =
        [
            "gitextensions3_configdata",
            "gitextensions3_dev",
            "gitextensions3_test",
        ];

        string result = WorktreeTree.GetCommonPrefix(paths);

        result.Should().Be("gitextensions3_");
    }

    [Test]
    public void GetCommonPrefix_should_strip_hyphen_delimited_prefix()
    {
        string[] paths =
        [
            "my-repo-feature-a",
            "my-repo-feature-b",
        ];

        string result = WorktreeTree.GetCommonPrefix(paths);

        result.Should().Be("my-repo-feature-");
    }

    [Test]
    public void GetCommonPrefix_should_strip_dot_delimited_prefix()
    {
        string[] paths =
        [
            "repo.worktrees.alpha",
            "repo.worktrees.beta",
        ];

        string result = WorktreeTree.GetCommonPrefix(paths);

        result.Should().Be("repo.worktrees.");
    }

    [Test]
    public void GetCommonPrefix_should_strip_space_delimited_prefix()
    {
        string[] paths =
        [
            "my repo feature-a",
            "my repo feature-b",
        ];

        string result = WorktreeTree.GetCommonPrefix(paths);

        result.Should().Be("my repo feature-");
    }

    [Test]
    public void GetCommonPrefix_should_not_split_mid_word()
    {
        string[] paths = ["apricot", "apple"];

        string result = WorktreeTree.GetCommonPrefix(paths);

        result.Should().BeEmpty();
    }

    [Test]
    public void GetCommonPrefix_should_return_empty_for_single_path_without_boundary()
    {
        string[] paths = ["worktree"];

        string result = WorktreeTree.GetCommonPrefix(paths);

        result.Should().BeEmpty();
    }

    [Test]
    public void GetCommonPrefix_should_snap_to_directory_separator_over_word_boundary()
    {
        // When paths contain directory separators, the prefix snaps to the last
        // directory separator even when word boundaries exist beyond it.
        string[] paths =
        [
            $"shared{Sep}gitextensions3_configdata",
            $"shared{Sep}gitextensions3_dev",
        ];

        string result = WorktreeTree.GetCommonPrefix(paths);

        result.Should().Be($"shared{Sep}");
    }
}
