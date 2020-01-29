using GitCommands;
using NUnit.Framework;

namespace GitCommandsTests.Git
{
    public sealed class GitRefNameTests
    {
        [Test]
        public void GetFullBranchNameTest()
        {
            Assert.AreEqual(null, GitRefName.GetFullBranchName(null));
            Assert.AreEqual("", GitRefName.GetFullBranchName(""));
            Assert.AreEqual("", GitRefName.GetFullBranchName("    "));
            Assert.AreEqual("4e0f0fe3f6add43557913c354de02560b8faec32", GitRefName.GetFullBranchName("4e0f0fe3f6add43557913c354de02560b8faec32"));
            Assert.AreEqual("refs/heads/master", GitRefName.GetFullBranchName("master"));
            Assert.AreEqual("refs/heads/master", GitRefName.GetFullBranchName(" master "));
            Assert.AreEqual("refs/heads/master", GitRefName.GetFullBranchName("refs/heads/master"));
            Assert.AreEqual("refs/heads/release/2.48", GitRefName.GetFullBranchName("refs/heads/release/2.48"));
            Assert.AreEqual("refs/tags/my-tag", GitRefName.GetFullBranchName("refs/tags/my-tag"));
        }

        [Test]
        public void GetRemoteName()
        {
            Assert.AreEqual("foo", GitRefName.GetRemoteName("refs/remotes/foo/master"));
            Assert.AreEqual("", GitRefName.GetRemoteName("refs/tags/1.0.0"));

            var remotes = new[] { "foo", "bar" };

            Assert.AreEqual("foo", GitRefName.GetRemoteName("foo/master", remotes));
            Assert.AreEqual("", GitRefName.GetRemoteName("food/master", remotes));
            Assert.AreEqual("", GitRefName.GetRemoteName("refs/tags/1.0.0", remotes));
        }

        [Test]
        public void GetFullBranchName()
        {
            Assert.AreEqual("refs/heads/foo", GitRefName.GetFullBranchName("foo"));

            Assert.AreEqual("refs/foo", GitRefName.GetFullBranchName("refs/foo"));

            Assert.AreEqual(
                "4e0f0fe3f6add43557913c354de02560b8faec32",
                GitRefName.GetFullBranchName("4e0f0fe3f6add43557913c354de02560b8faec32"));

            Assert.AreEqual("", GitRefName.GetFullBranchName(""));
            Assert.AreEqual("", GitRefName.GetFullBranchName("    "));

            Assert.IsNull(GitRefName.GetFullBranchName(null));
        }

        [Test]
        public void IsRemoteHead()
        {
            Assert.IsTrue(GitRefName.IsRemoteHead("refs/remotes/origin/HEAD"));
            Assert.IsTrue(GitRefName.IsRemoteHead("refs/remotes/upstream/HEAD"));

            Assert.IsFalse(GitRefName.IsRemoteHead("refs/remotes/ori/gin/HEAD"));
            Assert.IsFalse(GitRefName.IsRemoteHead("refs/remotes//HEAD"));
            Assert.IsFalse(GitRefName.IsRemoteHead("refs/remotes/HEAD"));
            Assert.IsFalse(GitRefName.IsRemoteHead("refs/origin/HEAD"));
            Assert.IsFalse(GitRefName.IsRemoteHead("ref/remotes/origin/HEAD"));
            Assert.IsFalse(GitRefName.IsRemoteHead("remotes/origin/HEAD"));
            Assert.IsFalse(GitRefName.IsRemoteHead("wat/refs/remotes/origin/HEAD"));
            Assert.IsFalse(GitRefName.IsRemoteHead("refs/remotes/origin/HEADZ"));
            Assert.IsFalse(GitRefName.IsRemoteHead("refs/remotes/origin/HEAD/wat"));
            Assert.IsFalse(GitRefName.IsRemoteHead("refs/remotes/origin/HEAD  "));
            Assert.IsFalse(GitRefName.IsRemoteHead("  refs/remotes/origin/HEAD"));
        }
    }
}