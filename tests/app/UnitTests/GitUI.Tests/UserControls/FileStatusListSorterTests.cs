using System.Text.Json.Nodes;
using FluentAssertions;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.UserControls;
using GitUIPluginInterfaces;

namespace GitUITests.UserControls;

public class FileStatusListSorterTests
{
    [SetUp]
    public void SetUp()
    {
    }

    [TearDown]
    public void TearDown()
    {
    }

    [Test]
    public async Task Sort_should_first_add_folders_then_files([Values(false, true)] bool flat)
    {
        GitItemStatus[] statuses =
        [
            new("root_file"),
            new(".hidden_root_file"),
            new("1/file1"),
            new("1/2/file12"),
            new("1/3/file13"),
            new("1/3/4/file134"),
            new("5/file1"),
            new("5/6/file56b"),
            new("5/6/file56a"),
            new("5/7/file57b"),
            new("5/7/file57a"),
            new("5/7/8/file578"),
        ];

        FileStatusList.StatusSorter statusSorter = new();
        TreeNode rootNode = statusSorter.CreateTreeSortedByPath(statuses, flat, createNode: status => new TreeNode(status.ToString()) { Tag = new FileStatusItem(firstRev: null, secondRev: new GitRevision(ObjectId.WorkTreeId), status) });
        await Verify(Serialize(rootNode).ToString());
    }

    [Test]
    public async Task Sort_should_not_create_subfolder_nodes_for_single_files()
    {
        GitItemStatus[] statuses =
        [
            new("1/2/file12"),
            new("1/3/file13"),
            new("1/4/file14"),
        ];

        FileStatusList.StatusSorter statusSorter = new();
        TreeNode rootNode = statusSorter.CreateTreeSortedByPath(statuses, flat: false, createNode: status => new TreeNode(status.ToString()) { Tag = new FileStatusItem(firstRev: null, secondRev: new GitRevision(ObjectId.WorkTreeId), status) });
        await Verify(Serialize(rootNode).ToString());
    }

    [TestCase("", "", "")]
    [TestCase("a", "a", "a")]
    [TestCase("a", "b", "")]
    [TestCase("a", "ab", "")]
    [TestCase("a", "a/b", "a")]
    [TestCase("a", "a/b/c", "a")]
    [TestCase("a/b", "a/bc", "a")]
    [TestCase("a/b", "a/b", "a/b")]
    [TestCase("a/b", "a/b/c", "a/b")]
    [TestCase("a/b/cc", "a/b/ccd", "a/b")]
    [TestCase("a/b/cc", "a/b/cc/de", "a/b/cc")]
    public void GetCommonPath(string a, string b, string expected)
    {
        FileStatusList.StatusSorter.TestAccessor.GetCommonPath(a, b).Should().Be(expected);
        FileStatusList.StatusSorter.TestAccessor.GetCommonPath(b, a).Should().Be(expected);
    }

    private static JsonObject Serialize(TreeNode node)
    {
        JsonObject json = [];

        JsonArray nodes = [];
        foreach (TreeNode subnode in node.Nodes)
        {
            nodes.Add(Serialize(subnode));
        }

        json.Add($"{nameof(node.Text)}", node.Text);
        json.Add($"{nameof(node.Tag)}", $"{node.Tag?.GetType().Name}: {node.Tag}");
        json.Add($"{nameof(node.Nodes)}", nodes);

        return json;
    }
}
