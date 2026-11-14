using GitCommands;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.LeftPanel;
using GitUI.UserControls.RevisionGrid;
using NSubstitute;

namespace GitUITests.LeftPanel;

[Apartment(ApartmentState.STA)]
public sealed class BisectTreeTests
{
    private static readonly ObjectId BadId = ObjectId.Parse("1111111111111111111111111111111111111111");
    private static readonly ObjectId GoodId1 = ObjectId.Parse("2222222222222222222222222222222222222222");
    private static readonly ObjectId GoodId2 = ObjectId.Parse("3333333333333333333333333333333333333333");
    private static readonly ObjectId SkipId = ObjectId.Parse("4444444444444444444444444444444444444444");

    private readonly IGitModule _module = Substitute.For<IGitModule>();

    [Test]
    public void IsBisecting_should_be_false_without_bisect_refs()
    {
        IGitRef[] refs =
        [
            CreateRef(BadId, "refs/heads/main"),
            CreateRef(GoodId1, "refs/tags/v1.0"),
            CreateRef(GoodId2, "refs/remotes/origin/main"),
            CreateRef(SkipId, "refs/stash"),
        ];

        BisectTree.IsBisecting(refs).Should().BeFalse();
        BisectTree.GetBisectRefs(refs).Should().BeEmpty();
    }

    [Test]
    public void IsBisecting_should_be_false_for_no_refs()
    {
        BisectTree.IsBisecting([]).Should().BeFalse();
    }

    [TestCase("refs/bisect/bad")]
    [TestCase("refs/bisect/good-2222222222222222222222222222222222222222")]
    [TestCase("refs/bisect/skip-2222222222222222222222222222222222222222")]
    public void IsBisecting_should_be_true_for_any_bisect_ref(string completeName)
    {
        IGitRef[] refs = [CreateRef(GoodId1, "refs/heads/main"), CreateRef(GoodId1, completeName)];

        BisectTree.IsBisecting(refs).Should().BeTrue();
    }

    [Test]
    public void GetBisectRefs_should_return_bad_then_good_then_skipped_refs_only()
    {
        IGitRef[] refs =
        [
            CreateRef(SkipId, $"refs/bisect/skip-{SkipId}"),
            CreateRef(GoodId1, $"refs/bisect/good-{GoodId1}"),
            CreateRef(BadId, "refs/heads/main"),
            CreateRef(BadId, "refs/bisect/bad"),
            CreateRef(GoodId2, $"refs/bisect/good-{GoodId2}"),
            CreateRef(GoodId2, "refs/tags/v1.0"),
        ];

        BisectTree.GetBisectRefs(refs).Select(r => r.CompleteName).Should().Equal(
            "refs/bisect/bad",
            $"refs/bisect/good-{GoodId1}",
            $"refs/bisect/good-{GoodId2}",
            $"refs/bisect/skip-{SkipId}");
    }

    [Test]
    public void BisectNode_should_show_the_ref_name_with_a_short_hash()
    {
        using TreeView treeView = new();
        using BisectTree tree = CreateTree(treeView);

        BisectNode bad = new(tree, CreateRef(BadId, "refs/bisect/bad"), visible: true);
        BisectNode good = new(tree, CreateRef(GoodId1, $"refs/bisect/good-{GoodId1}"), visible: true);
        BisectNode skip = new(tree, CreateRef(SkipId, $"refs/bisect/skip-{SkipId}"), visible: true);

        bad.FullPath.Should().Be("bad");
        bad.DisplayName.Should().Be("bad");
        bad.ObjectId.Should().Be(BadId);
        bad.IsBad.Should().BeTrue();
        bad.IsGood.Should().BeFalse();

        good.FullPath.Should().Be($"good-{GoodId1}");
        good.DisplayName.Should().Be("good-22222222");
        good.ObjectId.Should().Be(GoodId1);
        good.IsGood.Should().BeTrue();
        good.IsBad.Should().BeFalse();

        skip.DisplayName.Should().Be("skip-44444444");
        skip.ObjectId.Should().Be(SkipId);
        skip.IsGood.Should().BeFalse();
        skip.IsBad.Should().BeFalse();
    }

    [Test]
    public void GetDisplayName_should_keep_names_not_ending_with_the_hash()
    {
        BisectNode.GetDisplayName("bad", BadId).Should().Be("bad");
        BisectNode.GetDisplayName($"good-{GoodId1}", GoodId2).Should().Be($"good-{GoodId1}");
    }

    [Test]
    public void FillTree_should_build_one_node_per_bisect_ref_in_order()
    {
        using TreeView treeView = new();
        _ = treeView.Handle;
        using BisectTree tree = CreateTree(treeView);

        IGitRef[] refs =
        [
            CreateRef(GoodId1, $"refs/bisect/good-{GoodId1}"),
            CreateRef(SkipId, $"refs/bisect/skip-{SkipId}"),
            CreateRef(BadId, "refs/bisect/bad"),
            CreateRef(BadId, "refs/heads/main"),
        ];

        tree.GetTestAccessor().FillTree(refs);

        tree.TreeViewNode.Nodes.Cast<TreeNode>().Select(node => node.Text).Should().Equal("bad", "good-22222222", "skip-44444444");
        tree.TreeViewNode.Nodes.Cast<TreeNode>().Select(node => node.ImageKey).Should().Equal("BisectBad", "BisectGood", "BisectSkip");
        tree.TreeViewNode.Nodes.Cast<TreeNode>().Select(node => node.ToolTipText).Should().Equal(
            "refs/bisect/bad", $"refs/bisect/good-{GoodId1}", $"refs/bisect/skip-{SkipId}");
    }

    private static BisectTree CreateTree(TreeView treeView)
        => new(treeView.Nodes.Add("Bisect"), Substitute.For<IGitUICommandsSource>(), Substitute.For<ICheckRefs>());

    private GitRef CreateRef(ObjectId objectId, string completeName) => new(_module, objectId, completeName);
}
