using FluentAssertions;
using GitCommands.Config;
using GitCommands.Remote;
using GitUIPluginInterfaces;
using NSubstitute;
using NUnit.Framework;

namespace GitCommandsTests
{
    [TestFixture]
    public class RepoNameExtractorTest
    {
        private IGitModule _module;
        private IConfigFileSettings _configFile;
        private IRepoNameExtractor _repoNameExtractor;

        [SetUp]
        public void Setup()
        {
            _configFile = Substitute.For<IConfigFileSettings>();

            _module = Substitute.For<IGitModule>();
            _module.LocalConfigFile.Returns(_configFile);
            _repoNameExtractor = new RepoNameExtractor(() => _module);
        }

        // These test cases basically verifies Path.GetFileNameWithoutExtension(remoteUrl) and Path.GetFileNameWithoutExtension()
        [TestCase("origin", "https://github.com/project/repo.git", "project", "repo")]
        [TestCase("origin", "file://github/project/repo.git", "project", "repo")]
        [TestCase("origin", "https://github.com/extra/extra/project/repo.git", "project", "repo")]
        [TestCase("", "https://github.com/project/repo.git", "project", "repo")]
        [TestCase("", null, null, null)]
        [TestCase(null, null, null, null)]
        [TestCase("remote", "https://github.com/project/", "project", "")]
        [TestCase("origin", "git@github.com/project/repo.git", "project", "repo")]
        public void RepoNameExtractorTests(string remote, string url, string expProject, string expRepo)
        {
            _module.GetCurrentRemote().Returns(x => remote);
            _module.GetRemotes().Returns(x => new[] { remote, "    ", "\t" });
            _module.GetSetting(string.Format(SettingKeyString.RemoteUrl, remote)).Returns(x => url);

            _repoNameExtractor.Get(out string project, out string repo);
            project.Should().Be(expProject);
            repo.Should().Be(expRepo);
        }
    }
}