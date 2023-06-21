using System.Reflection;
using CommonTestUtils;
using FluentAssertions;
using GitCommands.Settings;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;

namespace GitCommandsTests.Settings
{
    [TestFixture]
    internal sealed class BuildServerSettingsTests
    {
        private GitModuleTestHelper _testHelper;
        private DistributedSettings _userRoaming;
        private DistributedSettings _repoDistributed;
        private DistributedSettings _repoLocal;
        private DistributedSettings _effective;
        private string _userRoamingConfigFilePath;
        private string _repoDistributedConfigFilePath;
        private string _repoLocalConfigFilePath;

        [SetUp]
        public void Setup()
        {
            _testHelper = new GitModuleTestHelper();

            var content = EmbeddedResourceLoader.Load(Assembly.GetExecutingAssembly(), $"{GetType().Namespace}.MockData.level3_roaming_GitExtensions.settings.xml");
            _userRoamingConfigFilePath = _testHelper.CreateFile(_testHelper.TemporaryPath + "/RoamingProfile", "GitExtensions.settings", content);
            content = EmbeddedResourceLoader.Load(Assembly.GetExecutingAssembly(), $"{GetType().Namespace}.MockData.level2_repodist_GitExtensions.settings.xml");
            _repoDistributedConfigFilePath = _testHelper.CreateRepoFile("GitExtensions.settings", content);
            content = EmbeddedResourceLoader.Load(Assembly.GetExecutingAssembly(), $"{GetType().Namespace}.MockData.level1_repogit_GitExtensions.settings.xml");
            _repoLocalConfigFilePath = _testHelper.CreateRepoFile(".git", "GitExtensions.settings", content);

            _userRoaming = new DistributedSettings(lowerPriority: null, new GitExtSettingsCache(_userRoamingConfigFilePath), SettingLevel.Global);
            _repoDistributed = new DistributedSettings(lowerPriority: _userRoaming, new GitExtSettingsCache(_repoDistributedConfigFilePath), SettingLevel.Distributed);
            _repoLocal = new DistributedSettings(lowerPriority: _repoDistributed, new GitExtSettingsCache(_repoLocalConfigFilePath), SettingLevel.Local);
            _effective = new DistributedSettings(lowerPriority: _repoLocal, new GitExtSettingsCache(settingsFilePath: null), SettingLevel.Effective);
        }

        [TearDown]
        public void TearDown()
        {
            _userRoaming.SettingsCache.Dispose();
            _repoDistributed.SettingsCache.Dispose();
            _repoLocal.SettingsCache.Dispose();

            _testHelper.Dispose();
        }

        [Test]
        public void UserRoaming_settings_should_return_expected()
        {
            IBuildServerSettings buildServerSettings = _userRoaming.GetBuildServerSettings();

            // Explicitly set
            buildServerSettings.ServerName.Should().Be("Azure DevOps and Team Foundation Server (since TFS2015)");
            buildServerSettings.IntegrationEnabled.Should().BeTrue();
            buildServerSettings.ShowBuildResultPage.Should().BeNull();

            // Explicitly set
            ISettingsSource settingsSource = buildServerSettings.SettingsSource;
            settingsSource.Should().BeOfType<SettingsPath>();
            ((SettingsPath)settingsSource).PathFor("").Should().Be("BuildServer.Azure DevOps and Team Foundation Server (since TFS2015).");
        }

        [Test]
        public void RepoDistributed_settings_should_return_expected()
        {
            IBuildServerSettings buildServerSettings = _repoLocal.GetBuildServerSettings();

            // No explicit settings, inheriting from the repo local
            buildServerSettings.ServerName.Should().Be("AppVeyor");
            buildServerSettings.IntegrationEnabled.Should().BeTrue();

            // Explicitly set
            buildServerSettings.ShowBuildResultPage.Should().BeFalse();

            // No explicit settings, inheriting from the repo local
            ISettingsSource settingsSource = buildServerSettings.SettingsSource;
            settingsSource.Should().BeOfType<SettingsPath>();
            ((SettingsPath)settingsSource).PathFor("").Should().Be("BuildServer.AppVeyor.");
        }

        [TestCase(true)]
        [TestCase(false)]
        public void RepoLocalAndEffective_settings_should_return_expected(bool isLocal)
        {
            IBuildServerSettings buildServerSettings = (isLocal ? _repoLocal : _effective).GetBuildServerSettings();

            buildServerSettings.ServerName.Should().Be("AppVeyor");
            buildServerSettings.IntegrationEnabled.Should().BeTrue();

            // No explicit settings, inheriting from the repo distributed
            buildServerSettings.ShowBuildResultPage.Should().BeFalse();

            ISettingsSource settingsSource = buildServerSettings.SettingsSource;
            settingsSource.Should().BeOfType<SettingsPath>();
            ((SettingsPath)settingsSource).PathFor("").Should().Be("BuildServer.AppVeyor.");
        }
    }
}
