using FluentAssertions;
using GitCommands.Config;
using GitCommands.Remotes;
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
        [TestCase("origin1", "file://github/project/repo.git", "project", "repo")]
        [TestCase("originx", "https://github.com/extra/extra/project/repo.git", "project", "repo")]
        [TestCase("", "https://github.com/project/repo.git", "project", "repo")]
        [TestCase("", null, null, null)]
        [TestCase(null, null, null, null)]
        [TestCase("remote", "https://github.com/project/", "project", "")]
        [TestCase("origin", "git@github.com/project/repo.git", "project", "repo")]
        public void RepoNameExtractorTest_ValidCurrentRemote(string remote, string url, string expProject, string expRepo)
        {
            _module.GetCurrentRemote().Returns(x => remote);
            _module.GetRemoteNames().Returns(x => new[] { remote, "    ", "\t" });
            _module.GetSetting(string.Format(SettingKeyString.RemoteUrl, remote)).Returns(x => url);

            var (project, repo) = _repoNameExtractor.Get();

            project.Should().Be(expProject);
            repo.Should().Be(expRepo);
        }

        [TestCase("origin", "https://github.com/project/repo.git", "project", "repo")]
        [TestCase("origin", "git@github.com/project/repo.git", "project", "repo")]
        public void RepoNameExtractorTest_NoValidCurrentRemote(string remote, string url, string expProject, string expRepo)
        {
            _module.GetCurrentRemote().Returns("");
            _module.GetRemoteNames().Returns(x => new[] { remote, "    ", "\t" });
            _module.GetSetting(string.Format(SettingKeyString.RemoteUrl, remote)).Returns(x => url);

            var (project, repo) = _repoNameExtractor.Get();

            project.Should().Be(expProject);
            repo.Should().Be(expRepo);
        }
    }
}