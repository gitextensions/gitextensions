using GitCommands;
using GitUI;
using NUnit.Framework;

namespace GitUITests
{
    [TestFixture]
    public sealed class UserEnvironmentInformationTests
    {
        [Test]
        public void GitVersion_is_good()
        {
            var gitString = UserEnvironmentInformation.GetGitVersionInfo("2.21.0.windows.1", new GitVersion("2.18.0"),
                new GitVersion("2.21.0"));
            Assert.AreEqual("2.21.0.windows.1", gitString);
        }

        [Test]
        public void GitVersion_is_old_but_supported()
        {
            var gitString = UserEnvironmentInformation.GetGitVersionInfo("2.20.1.windows.1", new GitVersion("2.18.0"),
                new GitVersion("2.21.0"));
            Assert.AreEqual("2.20.1.windows.1 (recommended: 2.21.0 or later)", gitString);
        }

        [Test]
        public void GitVersion_is_not_supported()
        {
            var gitString = UserEnvironmentInformation.GetGitVersionInfo("1.6.5.windows.1", new GitVersion("2.18.0"),
                new GitVersion("2.21.0"));
            Assert.AreEqual("1.6.5.windows.1 (minimum: 2.18.0, please update!)", gitString);
        }

        [Test]
        public void GitVersion_is_unknown_then_return_all_data()
        {
            var gitString = UserEnvironmentInformation.GetGitVersionInfo(null, new GitVersion("2.18.0"),
                new GitVersion("2.21.0"));
            Assert.AreEqual("- (minimum: 2.18.0, recommended: 2.21.0)", gitString);
        }
    }
}