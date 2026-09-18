using GitCommands;
using GitExtensions.Extensibility.Git;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;
using NSubstitute;

namespace GitUITests.UserControls.RevisionGrid;
public class GitRefListsForRevisionTests
{
    private GitRevision _revision = null!;
    private IGitModule _module = null!;
    private IGitRef[] _refs = null!;

    [SetUp]
    public void Setup()
    {
        _module = Substitute.For<IGitModule>();

        _refs =
        [
            new GitRef(_module, ObjectId.Random(), $"{GitRefName.RefsTagsPrefix}tag1"),
            new GitRef(_module, ObjectId.Random(), $"{GitRefName.RefsHeadsPrefix}branch1"),
            new GitRef(_module, ObjectId.Random(), $"{GitRefName.RefsRemotesPrefix}branch1"),
        ];
        _revision = new GitRevision(ObjectId.Random())
        {
            Refs = _refs
        };
    }

    [Test]
    public void ctor_must_throw_if_revision_null()
    {
        ((Action)(() => new GitRefListsForRevision(null!))).Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void ctor_must_throw_if_revision_ref_null()
    {
        ((Action)(() => new GitRefListsForRevision(new GitRevision(ObjectId.Random()) { Refs = null! }))).Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void AllBranches_must_return_all_revision_branches()
    {
        GitRefListsForRevision grl = new(_revision);
        grl.AllBranches.Should().HaveCount(2);
    }

    [Test]
    public void AllTags_must_return_all_revision_tags()
    {
        GitRefListsForRevision grl = new(_revision);
        grl.AllTags.Should().ContainSingle();
    }

    [Test]
    public void GetAllBranchNames_must_return_branches_names()
    {
        GitRefListsForRevision grl = new(_revision);
        grl.GetAllBranchNames().Should().BeEquivalentTo("branch1", "branch1");
    }

    [Test]
    public void GetAllTagNames_must_return_branches_names()
    {
        GitRefListsForRevision grl = new(_revision);
        grl.GetAllTagNames().Should().BeEquivalentTo("tag1");
    }

    [Test]
    public void GetDeletableRefs_must_return_branches_names()
    {
        GitRefListsForRevision grl = new(_revision);
        grl.GetDeletableRefs("branch1").Should().BeEquivalentTo([_refs[2], _refs[0]]);
    }

    [Test]
    public void GetRenameableLocalBranches_must_return_branches_names()
    {
        GitRefListsForRevision grl = new(_revision);
        grl.GetRenameableLocalBranches().Should().BeEquivalentTo([_refs[1]]);
    }

    [Test]
    public void GetTrackingLocalBranch_must_return_the_branch_tracking_the_remote()
    {
        IGitRef localBranch = CreateLocalBranchTracking("branch1", "origin");
        IGitRef remoteBranch = CreateRemoteBranch("branch1", "origin");
        GitRefListsForRevision grl = new(CreateRevision(localBranch, remoteBranch));

        grl.GetTrackingLocalBranch(remoteBranch).Should().BeSameAs(localBranch);
    }

    [Test]
    public void GetTrackingLocalBranch_must_return_null_for_an_untracked_remote()
    {
        IGitRef localBranch = CreateLocalBranchTracking("branch1", "origin");
        IGitRef remoteBranch = CreateRemoteBranch("other", "origin");
        GitRefListsForRevision grl = new(CreateRevision(localBranch, remoteBranch));

        grl.GetTrackingLocalBranch(remoteBranch).Should().BeNull();
    }

    [Test]
    public void GetTrackingLocalBranch_must_return_null_for_a_local_branch()
    {
        IGitRef localBranch = CreateLocalBranchTracking("branch1", "origin");
        GitRefListsForRevision grl = new(CreateRevision(localBranch, CreateRemoteBranch("branch1", "origin")));

        grl.GetTrackingLocalBranch(localBranch).Should().BeNull();
    }

    [Test]
    public void GetTrackingLocalBranch_must_return_null_if_no_ref_is_given()
    {
        GitRefListsForRevision grl = new(_revision);

        grl.GetTrackingLocalBranch(null).Should().BeNull();
    }

    [Test]
    public void BranchesWithNoIdenticalRemotes_must_not_contain_a_tracked_remote()
    {
        IGitRef localBranch = CreateLocalBranchTracking("branch1", "origin");
        IGitRef remoteBranch = CreateRemoteBranch("branch1", "origin");
        GitRefListsForRevision grl = new(CreateRevision(localBranch, remoteBranch));

        grl.BranchesWithNoIdenticalRemotes.Should().BeEquivalentTo([localBranch]);
    }

    private static GitRevision CreateRevision(params IGitRef[] refs)
        => new(ObjectId.Random()) { Refs = refs };

    private static IGitRef CreateLocalBranchTracking(string remoteBranchName, string remoteName)
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.GetEffectiveSetting("branch.local_branch.merge").Returns($"{GitRefName.RefsHeadsPrefix}{remoteBranchName}");
        module.GetEffectiveSetting("branch.local_branch.remote").Returns(remoteName);
        return new GitRef(module, ObjectId.Random(), $"{GitRefName.RefsHeadsPrefix}local_branch");
    }

    private static IGitRef CreateRemoteBranch(string branchName, string remoteName)
        => new GitRef(Substitute.For<IGitModule>(), ObjectId.Random(), $"{GitRefName.RefsRemotesPrefix}{remoteName}/{branchName}", remoteName);
}
