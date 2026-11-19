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

    // Neutral id for refs that are not related to a bisect.
    private static readonly ObjectId OtherId = ObjectId.Parse("5555555555555555555555555555555555555555");

    private readonly IGitModule _module = Substitute.For<IGitModule>();

    private TreeView _treeView = null!;
    private BisectTree _tree = null!;

    [SetUp]
    public void Setup()
    {
        _treeView = new TreeView();
        _ = _treeView.Handle;
        _tree = new BisectTree(_treeView.Nodes.Add("Bisect"), Substitute.For<IGitUICommandsSource>(), Substitute.For<ICheckRefs>());
    }

    [TearDown]
    public void TearDown()
    {
        _tree.Dispose();
        _treeView.Dispose();
    }

    [Test]
    public void IsBisecting_should_be_false_without_bisect_refs()
    {
        IGitRef[] refs =
        [
            CreateRef(OtherId, "refs/heads/main"),
            CreateRef(OtherId, "refs/tags/v1.0"),
            CreateRef(OtherId, "refs/remotes/origin/main"),
            CreateRef(OtherId, "refs/stash"),
        ];

        BisectTree.IsBisecting(refs).Should().BeFalse();
        BisectTree.IsBisecting([]).Should().BeFalse();
    }

    [TestCase("refs/bisect/bad")]
    [TestCase("refs/bisect/good-2222222222222222222222222222222222222222")]
    [TestCase("refs/bisect/skip-2222222222222222222222222222222222222222")]
    public void IsBisecting_should_be_true_for_any_bisect_ref(string completeName)
    {
        IGitRef[] refs = [CreateRef(OtherId, "refs/heads/main"), CreateRef(GoodId1, completeName)];

        BisectTree.IsBisecting(refs).Should().BeTrue();
    }

    [Test]
    public void GetBisectRefs_should_return_bad_then_good_then_skipped_refs_only()
    {
        IGitRef[] refs =
        [
            CreateRef(SkipId, $"refs/bisect/skip-{SkipId}"),
            CreateRef(GoodId1, $"refs/bisect/good-{GoodId1}"),
            CreateRef(OtherId, "refs/heads/main"),
            CreateRef(BadId, "refs/bisect/bad"),
            CreateRef(GoodId2, $"refs/bisect/good-{GoodId2}"),
            CreateRef(OtherId, "refs/tags/v1.0"),
        ];

        BisectTree.TestAccessor.GetBisectRefs(refs).Select(r => r.CompleteName).Should().Equal(
            "refs/bisect/bad",
            $"refs/bisect/good-{GoodId1}",
            $"refs/bisect/good-{GoodId2}",
            $"refs/bisect/skip-{SkipId}");
    }

    [Test]
    public void BisectNode_should_flag_bad_and_good_refs()
    {
        BisectNode bad = new(_tree, CreateRef(BadId, "refs/bisect/bad"), visible: true);
        BisectNode good = new(_tree, CreateRef(GoodId1, $"refs/bisect/good-{GoodId1}"), visible: true);
        BisectNode skip = new(_tree, CreateRef(SkipId, $"refs/bisect/skip-{SkipId}"), visible: true);

        (bad.IsBad, bad.IsGood).Should().Be((true, false));
        (good.IsBad, good.IsGood).Should().Be((false, true));
        (skip.IsBad, skip.IsGood).Should().Be((false, false));
        bad.ObjectId.Should().Be(BadId);
        good.ObjectId.Should().Be(GoodId1);
    }

    [Test]
    public void BisectNode_should_show_the_ref_name_with_a_short_hash()
    {
        BisectNode bad = new(_tree, CreateRef(BadId, "refs/bisect/bad"), visible: true);
        BisectNode good = new(_tree, CreateRef(GoodId1, $"refs/bisect/good-{GoodId1}"), visible: true);
        BisectNode skip = new(_tree, CreateRef(SkipId, $"refs/bisect/skip-{SkipId}"), visible: true);

        bad.DisplayName.Should().Be("bad");
        good.DisplayName.Should().Be("good-22222222");
        skip.DisplayName.Should().Be("skip-44444444");
    }

    [Test]
    public void BisectNode_should_keep_the_name_if_it_does_not_end_with_the_ref_hash()
    {
        BisectNode node = new(_tree, CreateRef(GoodId2, $"refs/bisect/good-{GoodId1}"), visible: true);

        node.DisplayName.Should().Be($"good-{GoodId1}");
    }

    [Test]
    public void FillTree_should_build_one_node_per_bisect_ref_in_order()
    {
        IGitRef[] refs =
        [
            CreateRef(GoodId1, $"refs/bisect/good-{GoodId1}"),
            CreateRef(SkipId, $"refs/bisect/skip-{SkipId}"),
            CreateRef(BadId, "refs/bisect/bad"),
            CreateRef(OtherId, "refs/heads/main"),
        ];

        _tree.GetTestAccessor().FillTree(refs);

        TreeNode[] nodes = [.. _tree.TreeViewNode.Nodes.Cast<TreeNode>()];
        nodes.Select(node => node.Text).Should().Equal("bad", "good-22222222", "skip-44444444");
        nodes.Select(node => node.ImageKey).Should().Equal("BisectBad", "BisectGood", "BisectSkip");
        nodes.Select(node => node.ToolTipText).Should().Equal(
            "refs/bisect/bad", $"refs/bisect/good-{GoodId1}", $"refs/bisect/skip-{SkipId}");
    }

    private GitRef CreateRef(ObjectId objectId, string completeName) => new(_module, objectId, completeName);
}
