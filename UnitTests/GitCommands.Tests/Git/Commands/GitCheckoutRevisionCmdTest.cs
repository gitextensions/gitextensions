using GitCommands;
using GitCommands.Git.Commands;
using GitUIPluginInterfaces;

namespace GitCommandsTests.Git.Commands
{
    [TestFixture]
    public sealed class GitCheckoutRevisionCmdTest
    {
        [Test]
        public void TestConstructor()
        {
            GitCheckoutRevisionCmd cmd = new(ObjectId.IndexId);

            Assert.IsNotNull(cmd);
            Assert.AreEqual(LocalChangesAction.DontChange, cmd.LocalChanges);
            Assert.IsFalse(cmd.AccessesRemote);
            Assert.IsTrue(cmd.ChangesRepoState);
        }

        [TestCase(LocalChangesAction.DontChange, "checkout \"2222222222222222222222222222222222222222\"")]
        [TestCase(LocalChangesAction.Merge, "checkout --merge \"2222222222222222222222222222222222222222\"")]
        [TestCase(LocalChangesAction.Reset, "checkout --force \"2222222222222222222222222222222222222222\"")]
        [TestCase(LocalChangesAction.Stash, "checkout \"2222222222222222222222222222222222222222\"")]
        public void Assert_arguments(LocalChangesAction action, string expected)
        {
            Assert.AreEqual(expected, new GitCheckoutRevisionCmd(ObjectId.IndexId, action).Arguments);
        }
    }
}
