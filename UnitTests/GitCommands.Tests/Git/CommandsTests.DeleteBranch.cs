using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility.Configurations;
using GitExtensions.Extensibility.Git;
using NSubstitute;

namespace GitCommandsTests_Git
{
    partial class CommandsTests
    {
        [Test]
        public void ctor_should_throw_if_branches_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => Commands.DeleteBranch(branches: null, force: false));
        }

        [Test]
        public void ctor_should_throw_if_branches_is_empty()
        {
            Assert.Throws<ArgumentException>(() => Commands.DeleteBranch(Array.Empty<IGitRef>(), force: false));
        }

        [Test]
        public void ctor_should_have_expected_values()
        {
            const string remoteName = "origin";
            string completeName = $"refs/remotes/{remoteName}/branch_name";
            GitRef remoteBranchRef = SetupRawRemoteRef(remoteName, completeName);

            IGitCommand cmd = Commands.DeleteBranch(new IGitRef[] { remoteBranchRef }, force: false);

            Assert.IsFalse(cmd.AccessesRemote);
            Assert.IsTrue(cmd.ChangesRepoState);
        }

        private static IEnumerable<TestCaseData> DeleteBranchTestData
        {
            get
            {
                const string name = "local_branch";

                // Test local branches only
                List<IGitRef> localRefs = [];
                IGitModule localGitModule = Substitute.For<IGitModule>();
                IConfigFileSettings localConfigFileSettings = Substitute.For<IConfigFileSettings>();
                localConfigFileSettings.GetValue($"branch.local_branch.merge").Returns(string.Empty);
                localConfigFileSettings.GetValue($"branch.local_branch.remote").Returns(string.Empty);
                localGitModule.LocalConfigFile.Returns(localConfigFileSettings);
                GitRef localBranchRef = new(localGitModule, ObjectId.Random(), $"refs/heads/{name}");

                localRefs.Add(localBranchRef);

                yield return new TestCaseData(localRefs, /* force */ false, /* expected */ $"branch --delete \"{name}\"");
                yield return new TestCaseData(localRefs, /* force */ true, /* expected */ $"branch --delete --force \"{name}\"");

                // Test local and remote branches
                const string remoteName = "origin";
                string completeName = $"refs/remotes/{remoteName}/{name}";
                GitRef remoteBranchRef = SetupRawRemoteRef(remoteName, completeName);

                List<IGitRef> mixedRefs = [localBranchRef, remoteBranchRef];

                yield return new TestCaseData(mixedRefs, /* force */ false, /* expected */ $"branch --delete --all \"{name}\" \"origin/{name}\"");
                yield return new TestCaseData(mixedRefs, /* force */ true, /* expected */ $"branch --delete --force --all \"{name}\" \"origin/{name}\"");

                // Test remote branches only
                List<IGitRef> remoteRefs = [remoteBranchRef];

                yield return new TestCaseData(remoteRefs, /* force */ false, /* expected */ $"branch --delete --remotes \"origin/{name}\"");
                yield return new TestCaseData(remoteRefs, /* force */ true, /* expected */ $"branch --delete --force --remotes \"origin/{name}\"");
            }
        }

        [TestCaseSource(nameof(DeleteBranchTestData))]
        public void Arguments_are_Expected(IReadOnlyCollection<IGitRef> branches, bool force, string expected)
        {
            IGitCommand cmd = Commands.DeleteBranch(branches, force);
            Assert.AreEqual(expected, cmd.Arguments);
        }

        private static GitRef SetupRawRemoteRef(string remoteName, string completeName)
        {
            IGitModule localGitModule = Substitute.For<IGitModule>();
            IConfigFileSettings localConfigFileSettings = Substitute.For<IConfigFileSettings>();
            localConfigFileSettings.GetValue($"branch.local_branch.merge").Returns(completeName);
            localConfigFileSettings.GetValue($"branch.local_branch.remote").Returns(remoteName);
            localGitModule.LocalConfigFile.Returns(localConfigFileSettings);
            GitRef remoteBranchRef = new(localGitModule, ObjectId.Random(), completeName, remoteName);
            return remoteBranchRef;
        }
    }
}
