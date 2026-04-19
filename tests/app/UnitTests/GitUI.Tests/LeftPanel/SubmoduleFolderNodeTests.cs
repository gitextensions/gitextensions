using AwesomeAssertions;
using GitUI.LeftPanel;

namespace GitUITests.LeftPanel;

[TestFixture]
public sealed class SubmoduleFolderNodeTests
{
    [Test]
    public void CompactSingleChildFolders_should_merge_chain_of_single_child_folders()
    {
        SubmoduleFolderNode root = CreateFolder("extension");
        SubmoduleFolderNode child1 = CreateFolder("src");
        SubmoduleFolderNode child2 = CreateFolder("test");
        SubmoduleFolderNode child3 = CreateFolder("assets");

        root.Nodes.AddNode(child1);
        child1.Nodes.AddNode(child2);
        child2.Nodes.AddNode(child3);

        root.CompactSingleChildFolders();

        root.GetTestAccessor().DisplayText().Should().Be("extension/src/test/assets");
        root.Nodes.Count.Should().Be(0);
    }

    [Test]
    public void CompactSingleChildFolders_should_stop_at_non_folder_child()
    {
        SubmoduleFolderNode root = CreateFolder("a");
        SubmoduleFolderNode child = CreateFolder("b");
        DummyNode leaf = new();

        root.Nodes.AddNode(child);
        child.Nodes.AddNode(leaf);

        root.CompactSingleChildFolders();

        root.GetTestAccessor().DisplayText().Should().Be("a/b");
        root.Nodes.Count.Should().Be(1);
    }

    [Test]
    public void CompactSingleChildFolders_should_not_compact_when_multiple_children()
    {
        SubmoduleFolderNode root = CreateFolder("Externals");
        SubmoduleFolderNode child1 = CreateFolder("a");
        SubmoduleFolderNode child2 = CreateFolder("b");

        root.Nodes.AddNode(child1);
        root.Nodes.AddNode(child2);

        root.CompactSingleChildFolders();

        root.GetTestAccessor().DisplayText().Should().Be("Externals");
        root.Nodes.Count.Should().Be(2);
    }

    [Test]
    public void CompactSingleChildFolders_should_not_compact_when_no_children()
    {
        SubmoduleFolderNode root = CreateFolder("empty");

        root.CompactSingleChildFolders();

        root.GetTestAccessor().DisplayText().Should().Be("empty");
        root.Nodes.Count.Should().Be(0);
    }

    [Test]
    public void CompactSingleChildFolders_should_preserve_grandchildren_of_multi_child_node()
    {
        SubmoduleFolderNode root = CreateFolder("a");
        SubmoduleFolderNode child = CreateFolder("b");
        SubmoduleFolderNode grandchild = CreateFolder("c");
        DummyNode leaf1 = new();
        DummyNode leaf2 = new();

        root.Nodes.AddNode(child);
        child.Nodes.AddNode(grandchild);
        grandchild.Nodes.AddNode(leaf1);
        grandchild.Nodes.AddNode(leaf2);

        root.CompactSingleChildFolders();

        root.GetTestAccessor().DisplayText().Should().Be("a/b/c");
        root.Nodes.Count.Should().Be(2);
    }

    private static SubmoduleFolderNode CreateFolder(string name)
    {
        return new SubmoduleFolderNode(tree: null!, name);
    }
}
