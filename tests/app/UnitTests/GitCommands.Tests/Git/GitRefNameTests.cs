using GitCommands;

namespace GitCommandsTests.Git;

public sealed class GitRefNameTests
{
    [Test]
    public void GetFullBranchNameTest()
    {
        ClassicAssert.AreEqual(null, GitRefName.GetFullBranchName(null));
        ClassicAssert.AreEqual("", GitRefName.GetFullBranchName(""));
        ClassicAssert.AreEqual("", GitRefName.GetFullBranchName("    "));
        ClassicAssert.AreEqual("4e0f0fe3f6add43557913c354de02560b8faec32", GitRefName.GetFullBranchName("4e0f0fe3f6add43557913c354de02560b8faec32"));
        ClassicAssert.AreEqual("refs/heads/master", GitRefName.GetFullBranchName("master"));
        ClassicAssert.AreEqual("refs/heads/master", GitRefName.GetFullBranchName(" master "));
        ClassicAssert.AreEqual("refs/heads/master", GitRefName.GetFullBranchName("refs/heads/master"));
        ClassicAssert.AreEqual("refs/heads/release/2.48", GitRefName.GetFullBranchName("refs/heads/release/2.48"));
        ClassicAssert.AreEqual("refs/tags/my-tag", GitRefName.GetFullBranchName("refs/tags/my-tag"));
    }

    [Test]
    public void GetRemoteName()
    {
        ClassicAssert.AreEqual("foo", GitRefName.GetRemoteName("refs/remotes/foo/master"));
        ClassicAssert.AreEqual("", GitRefName.GetRemoteName("refs/tags/1.0.0"));

        string[] remotes = new[] { "foo", "bar" };

        ClassicAssert.AreEqual("foo", GitRefName.GetRemoteName("foo/master", remotes));
        ClassicAssert.AreEqual("", GitRefName.GetRemoteName("food/master", remotes));
        ClassicAssert.AreEqual("", GitRefName.GetRemoteName("refs/tags/1.0.0", remotes));
    }

    [Test]
    public void GetRemoteBranch()
    {
        ClassicAssert.AreEqual("master", GitRefName.GetRemoteBranch("refs/remotes/foo/master"));
        ClassicAssert.AreEqual("tmp/master", GitRefName.GetRemoteBranch("refs/remotes/foo/tmp/master"));

        ClassicAssert.AreEqual("", GitRefName.GetRemoteBranch("refs/remotes/foo"));
        ClassicAssert.AreEqual("", GitRefName.GetRemoteBranch("short"));
    }

    [Test]
    public void GetFullBranchName()
    {
        ClassicAssert.AreEqual("refs/heads/foo", GitRefName.GetFullBranchName("foo"));

        ClassicAssert.AreEqual("refs/foo", GitRefName.GetFullBranchName("refs/foo"));

        ClassicAssert.AreEqual(
            "4e0f0fe3f6add43557913c354de02560b8faec32",
            GitRefName.GetFullBranchName("4e0f0fe3f6add43557913c354de02560b8faec32"));

        ClassicAssert.AreEqual("", GitRefName.GetFullBranchName(""));
        ClassicAssert.AreEqual("", GitRefName.GetFullBranchName("    "));

        ClassicAssert.IsNull(GitRefName.GetFullBranchName(null));
    }

    [Test]
    public void IsRemoteHead()
    {
        ClassicAssert.IsTrue(GitRefName.IsRemoteHead("refs/remotes/origin/HEAD"));
        ClassicAssert.IsTrue(GitRefName.IsRemoteHead("refs/remotes/upstream/HEAD"));

        ClassicAssert.IsFalse(GitRefName.IsRemoteHead("refs/remotes/ori/gin/HEAD"));
        ClassicAssert.IsFalse(GitRefName.IsRemoteHead("refs/remotes//HEAD"));
        ClassicAssert.IsFalse(GitRefName.IsRemoteHead("refs/remotes/HEAD"));
        ClassicAssert.IsFalse(GitRefName.IsRemoteHead("refs/origin/HEAD"));
        ClassicAssert.IsFalse(GitRefName.IsRemoteHead("ref/remotes/origin/HEAD"));
        ClassicAssert.IsFalse(GitRefName.IsRemoteHead("remotes/origin/HEAD"));
        ClassicAssert.IsFalse(GitRefName.IsRemoteHead("wat/refs/remotes/origin/HEAD"));
        ClassicAssert.IsFalse(GitRefName.IsRemoteHead("refs/remotes/origin/HEADZ"));
        ClassicAssert.IsFalse(GitRefName.IsRemoteHead("refs/remotes/origin/HEAD/wat"));
        ClassicAssert.IsFalse(GitRefName.IsRemoteHead("refs/remotes/origin/HEAD  "));
        ClassicAssert.IsFalse(GitRefName.IsRemoteHead("  refs/remotes/origin/HEAD"));
    }
}
