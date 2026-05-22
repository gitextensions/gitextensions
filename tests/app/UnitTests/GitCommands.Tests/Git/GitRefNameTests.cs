using GitCommands;

namespace GitCommandsTests.Git;

public sealed class GitRefNameTests
{
    [Test]
    public void GetFullBranchNameTest()
    {
        GitRefName.GetFullBranchName(null).Should().Be(null);
        GitRefName.GetFullBranchName("").Should().Be("");
        GitRefName.GetFullBranchName("    ").Should().Be("");
        GitRefName.GetFullBranchName("4e0f0fe3f6add43557913c354de02560b8faec32").Should().Be("4e0f0fe3f6add43557913c354de02560b8faec32");
        GitRefName.GetFullBranchName("master").Should().Be("refs/heads/master");
        GitRefName.GetFullBranchName(" master ").Should().Be("refs/heads/master");
        GitRefName.GetFullBranchName("refs/heads/master").Should().Be("refs/heads/master");
        GitRefName.GetFullBranchName("refs/heads/release/2.48").Should().Be("refs/heads/release/2.48");
        GitRefName.GetFullBranchName("refs/tags/my-tag").Should().Be("refs/tags/my-tag");
    }

    [Test]
    public void GetRemoteName()
    {
        GitRefName.GetRemoteName("refs/remotes/foo/master").Should().Be("foo");
        GitRefName.GetRemoteName("refs/tags/1.0.0").Should().Be("");

        string[] remotes = ["foo", "bar"];

        GitRefName.GetRemoteName("foo/master", remotes).Should().Be("foo");
        GitRefName.GetRemoteName("food/master", remotes).Should().Be("");
        GitRefName.GetRemoteName("refs/tags/1.0.0", remotes).Should().Be("");
    }

    [Test]
    public void GetRemoteBranch()
    {
        GitRefName.GetRemoteBranch("refs/remotes/foo/master").Should().Be("master");
        GitRefName.GetRemoteBranch("refs/remotes/foo/tmp/master").Should().Be("tmp/master");

        GitRefName.GetRemoteBranch("refs/remotes/foo").Should().Be("");
        GitRefName.GetRemoteBranch("short").Should().Be("");
    }

    [Test]
    public void GetFullBranchName()
    {
        GitRefName.GetFullBranchName("foo").Should().Be("refs/heads/foo");

        GitRefName.GetFullBranchName("refs/foo").Should().Be("refs/foo");

        GitRefName.GetFullBranchName("4e0f0fe3f6add43557913c354de02560b8faec32").Should().Be("4e0f0fe3f6add43557913c354de02560b8faec32");

        GitRefName.GetFullBranchName("").Should().Be("");
        GitRefName.GetFullBranchName("    ").Should().Be("");

        GitRefName.GetFullBranchName(null).Should().BeNull();
    }

    [Test]
    public void IsRemoteHead()
    {
        GitRefName.IsRemoteHead("refs/remotes/origin/HEAD").Should().BeTrue();
        GitRefName.IsRemoteHead("refs/remotes/upstream/HEAD").Should().BeTrue();

        GitRefName.IsRemoteHead("refs/remotes/ori/gin/HEAD").Should().BeFalse();
        GitRefName.IsRemoteHead("refs/remotes//HEAD").Should().BeFalse();
        GitRefName.IsRemoteHead("refs/remotes/HEAD").Should().BeFalse();
        GitRefName.IsRemoteHead("refs/origin/HEAD").Should().BeFalse();
        GitRefName.IsRemoteHead("ref/remotes/origin/HEAD").Should().BeFalse();
        GitRefName.IsRemoteHead("remotes/origin/HEAD").Should().BeFalse();
        GitRefName.IsRemoteHead("wat/refs/remotes/origin/HEAD").Should().BeFalse();
        GitRefName.IsRemoteHead("refs/remotes/origin/HEADZ").Should().BeFalse();
        GitRefName.IsRemoteHead("refs/remotes/origin/HEAD/wat").Should().BeFalse();
        GitRefName.IsRemoteHead("refs/remotes/origin/HEAD  ").Should().BeFalse();
        GitRefName.IsRemoteHead("  refs/remotes/origin/HEAD").Should().BeFalse();
    }
}
