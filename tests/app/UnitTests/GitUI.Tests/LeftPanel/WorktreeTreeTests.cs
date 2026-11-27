using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.LeftPanel;
using NSubstitute;

namespace GitUITests.LeftPanel;

[Apartment(ApartmentState.STA)]
public sealed class WorktreeTreeTests
{
    private const string Sha = "1111111111111111111111111111111111111111";
    private const string Main = @"C:\repos\project";

    private static readonly char Sep = Path.DirectorySeparatorChar;

    private TreeView _treeView = null!;
    private WorktreeTree _tree = null!;

    [SetUp]
    public void Setup()
    {
        _treeView = new TreeView();
        _ = _treeView.Handle;
        _tree = new WorktreeTree(_treeView.Nodes.Add("Worktrees"), Substitute.For<IGitUICommandsSource>());
    }

    [TearDown]
    public void TearDown()
    {
        _tree.Dispose();
        _treeView.Dispose();
    }

    [Test]
    public void FillTree_should_keep_flat_siblings_flat()
    {
        FillTree(Main, Worktree(Main, "main"), Worktree(@"C:\repos\project_dev", "dev"), Worktree(@"C:\repos\project_test", "test"));

        DumpTree().Should().Equal(
            "project (main)",
            "dev (dev)",
            "test (test)");
        _tree.TreeViewNode.Nodes.Cast<TreeNode>().Select(node => node.ImageKey).Should().AllBe("WorkTree");
    }

    [Test]
    public void FillTree_should_group_worktrees_into_folders_by_path()
    {
        FillTree(
            Main,
            Worktree(Main, "main"),
            Worktree(@"C:\repos\project.worktrees\agent\task-1", "agent/task-1"),
            Worktree(@"C:\repos\project.worktrees\agent\task-2", "agent/task-2"),
            Worktree(@"C:\repos\project.worktrees\plain", "plain"),
            Worktree(@"C:\repos\project.worktrees\release\v1", "release/v1"));

        DumpTree().Should().Equal(
            "project (main)",
            "agent/",
            "  task-1 (agent/task-1)",
            "  task-2 (agent/task-2)",
            "plain (plain)",
            "release/",
            "  v1 (release/v1)");

        TreeNode agent = _tree.TreeViewNode.Nodes[1];
        agent.ImageKey.Should().Be("BranchFolder");
        agent.Nodes[0].ImageKey.Should().Be("WorkTree");
        agent.Nodes[0].Tag.Should().BeOfType<WorktreeNode>();
    }

    [Test]
    public void FillTree_should_compact_single_child_folder_chains()
    {
        FillTree(
            Main,
            Worktree(Main, "main"),
            Worktree(@"C:\repos\project.worktrees\a\b\c\one", "one"),
            Worktree(@"C:\repos\project.worktrees\d\two", "two"));

        DumpTree().Should().Equal(
            "project (main)",
            "a/b/c/",
            "  one (one)",
            "d/",
            "  two (two)");
    }

    [Test]
    public void FillTree_should_not_compact_a_folder_which_also_holds_a_worktree()
    {
        FillTree(
            Main,
            Worktree(Main, "main"),
            Worktree(@"C:\repos\project.worktrees\x\y", "y"),
            Worktree(@"C:\repos\project.worktrees\x\y\z\w", "w"),
            Worktree(@"C:\repos\project.worktrees\other", "other"));

        DumpTree().Should().Equal(
            "project (main)",
            "x/",
            "  y (y)",
            "  y/z/",
            "    w (w)",
            "other (other)");
    }

    [Test]
    public void FillTree_should_merge_folders_that_differ_only_in_case()
    {
        FillTree(
            Main,
            Worktree(Main, "main"),
            Worktree(@"C:\repos\project.worktrees\Team\one", "one"),
            Worktree(@"C:\repos\project.worktrees\team\two", "two"),
            Worktree(@"C:\repos\project.worktrees\other\three", "three"));

        DumpTree().Should().Equal(
            "project (main)",
            "Team/",
            "  one (one)",
            "  two (two)",
            "other/",
            "  three (three)");
    }

    [Test]
    public void FillTree_should_keep_the_main_worktree_first_and_at_the_top_level()
    {
        FillTree(
            Main,
            Worktree(Main, "main"),
            Worktree(@"C:\repos\project\.worktrees\a\one", "one"),
            Worktree(@"C:\repos\project\.worktrees\b\two", "two"));

        DumpTree().Should().Equal(
            "project (main)",
            "a/",
            "  one (one)",
            "b/",
            "  two (two)");
        _tree.TreeViewNode.Nodes[0].Tag.Should().BeOfType<WorktreeNode>().Which.Worktree.IsMain.Should().BeTrue();
    }

    [Test]
    public void FillTree_should_show_worktrees_outside_the_common_root_with_their_full_path_at_the_top_level()
    {
        FillTree(
            Main,
            Worktree(Main, "main"),
            Worktree(@"C:\repos\project.worktrees\team\one", "one"),
            Worktree(@"C:\repos\project.worktrees\team\two", "two"),
            Worktree(@"C:\elsewhere\nested\three", "three"),
            Worktree(@"D:\other-drive\four", "four"));

        DumpTree().Should().Equal(
            "project (main)",
            "project.worktrees/team/",
            "  one (one)",
            "  two (two)",
            @"C:\elsewhere\nested\three (three)",
            @"D:\other-drive\four (four)");
    }

    [Test]
    public void FillTree_should_keep_a_single_linked_worktree_outside_the_parent_folder_at_the_top_level()
    {
        // With one linked worktree there is no common prefix to strip, so its relative path starts with "..".
        FillTree(Main, Worktree(Main, "main"), Worktree(@"C:\worktrees\project\one", "one"));

        DumpTree().Should().Equal(
            "project (main)",
            @"C:\worktrees\project\one (one)");
    }

    [TestCase("TEMP")]
    [TestCase("LOCALAPPDATA")]
    [TestCase("APPDATA")]
    [TestCase("ONEDRIVE")]
    [TestCase("USERPROFILE")]
    [TestCase("PUBLIC")]
    [TestCase("PROGRAMDATA")]
    [TestCase("PROGRAMFILES")]
    [TestCase("PROGRAMFILES(X86)")]
    [TestCase("SYSTEMROOT")]
    public void FillTree_should_abbreviate_a_full_path_in_a_special_folder(string variable)
    {
        string? folder = Environment.GetEnvironmentVariable(variable);
        if (string.IsNullOrEmpty(folder))
        {
            Assert.Ignore($"%{variable}% is not set.");
        }

        string path = Path.Combine(folder, "tool", "session", "one");
        FillTree(Main, Worktree(Main, "main"), Worktree(path, "one"));

        DumpTree().Should().Equal(
            "project (main)",
            $@"%{variable}%\tool\session\one (one)");
    }

    [Test]
    public void FillTree_should_not_abbreviate_a_folder_which_only_starts_like_a_special_folder()
    {
        string path = Environment.GetEnvironmentVariable("USERPROFILE") + @"-other\one";
        FillTree(Main, Worktree(Main, "main"), Worktree(path, "one"));

        DumpTree().Should().Equal(
            "project (main)",
            $"{path} (one)");
    }

    [Test]
    public void FillTree_should_show_the_current_worktree_in_bold_and_expand_its_folders()
    {
        string current = @"C:\repos\project.worktrees\agent\task-2";
        FillTree(
            current,
            Worktree(Main, "main"),
            Worktree(@"C:\repos\project.worktrees\agent\task-1", "agent/task-1"),
            Worktree(current, "agent/task-2"),
            Worktree(@"C:\repos\project.worktrees\release\v1", "release/v1"));

        TreeNode agent = _tree.TreeViewNode.Nodes[1];
        TreeNode release = _tree.TreeViewNode.Nodes[2];

        _tree.TreeViewNode.IsExpanded.Should().BeTrue();
        agent.IsExpanded.Should().BeTrue();
        release.IsExpanded.Should().BeFalse();
        agent.Nodes[1].Tag.Should().BeOfType<WorktreeNode>().Which.IsCurrent.Should().BeTrue();
        agent.Nodes[1].NodeFont!.Bold.Should().BeTrue();
        agent.Nodes[0].NodeFont.Should().BeNull();
        agent.NodeFont.Should().BeNull();
    }

    [Test]
    public void Refresh_should_split_a_compacted_folder_when_a_worktree_is_created_beside_it()
    {
        GitWorktree[] worktrees =
        [
            Worktree(Main, "main"),
            Worktree(@"C:\repos\project.worktrees\a\b\c\one", "one"),
            Worktree(@"C:\repos\project.worktrees\d\two", "two"),
        ];
        FillTree(Main, worktrees);
        FindFolder("a/b/c").Expand();

        RefreshTree(Main, [.. worktrees, Worktree(@"C:\repos\project.worktrees\a\x\three", "three")]);

        DumpTree().Should().Equal(
            "project (main)",
            "a/ +",
            "  b/c/ +",
            "    one (one)",
            "  x/",
            "    three (three)",
            "d/",
            "  two (two)");
    }

    [Test]
    public void Refresh_should_compact_a_folder_again_when_a_worktree_is_deleted()
    {
        GitWorktree[] worktrees =
        [
            Worktree(Main, "main"),
            Worktree(@"C:\repos\project.worktrees\a\b\c\one", "one"),
            Worktree(@"C:\repos\project.worktrees\a\x\three", "three"),
            Worktree(@"C:\repos\project.worktrees\d\two", "two"),
        ];
        FillTree(Main, worktrees);
        FindFolder("a").Expand();
        FindFolder("a", "b/c").Expand();

        RefreshTree(Main, [.. worktrees.Where(worktree => worktree.Branch != "three")]);

        DumpTree().Should().Equal(
            "project (main)",
            "a/b/c/ +",
            "  one (one)",
            "d/",
            "  two (two)");
    }

    [Test]
    public void Refresh_should_remove_a_folder_when_its_last_worktree_is_deleted()
    {
        GitWorktree[] worktrees =
        [
            Worktree(Main, "main"),
            Worktree(@"C:\repos\project.worktrees\a\one", "one"),
            Worktree(@"C:\repos\project.worktrees\d\two", "two"),
            Worktree(@"C:\repos\project.worktrees\e\three", "three"),
        ];
        FillTree(Main, worktrees);
        FindFolder("a").Expand();
        FindFolder("e").Expand();

        RefreshTree(Main, [.. worktrees.Where(worktree => worktree.Branch != "one")]);

        DumpTree().Should().Equal(
            "project (main)",
            "d/",
            "  two (two)",
            "e/ +",
            "  three (three)");
    }

    private void FillTree(string workingDir, params GitWorktree[] worktrees)
        => _tree.GetTestAccessor().FillTree(worktrees, workingDir);

    private void RefreshTree(string workingDir, GitWorktree[] worktrees)
        => _tree.GetTestAccessor().FillTree(worktrees, workingDir, firstTime: false);

    private TreeNode FindFolder(params string[] texts)
    {
        TreeNode node = _tree.TreeViewNode;
        foreach (string text in texts)
        {
            node = node.Nodes.Cast<TreeNode>().Single(child => child.Text == text);
        }

        return node;
    }

    private static GitWorktree Worktree(string path, string branch)
        => new(path, GitWorktreeHeadType.Branch, Sha, branch, IsDeleted: false) { IsMain = path == Main };

    private string[] DumpTree()
    {
        List<string> lines = [];
        Dump(_tree.TreeViewNode.Nodes, "");
        return [.. lines];

        void Dump(TreeNodeCollection nodes, string indent)
        {
            foreach (TreeNode node in nodes)
            {
                // A folder is marked with "/", and with " +" when it is expanded.
                lines.Add(node.Tag is BasePathNode
                    ? $"{indent}{node.Text}/{(node.IsExpanded ? " +" : "")}"
                    : $"{indent}{node.Text}");
                Dump(node.Nodes, indent + "  ");
            }
        }
    }

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
