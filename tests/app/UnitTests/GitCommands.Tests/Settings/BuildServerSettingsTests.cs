using System.Reflection;
using CommonTestUtils;
using GitCommands.Settings;
using GitExtensions.Extensibility.Settings;

namespace GitCommandsTests.Settings;
internal sealed class BuildServerSettingsTests
{
    private GitModuleTestHelper _testHelper = null!;
    private DistributedSettings _userRoaming = null!;
    private DistributedSettings _repoDistributed = null!;
    private DistributedSettings _repoLocal = null!;
    private DistributedSettings _effective = null!;
    private string _userRoamingConfigFilePath = null!;
    private string _repoDistributedConfigFilePath = null!;
    private string _repoLocalConfigFilePath = null!;

    [SetUp]
    public void Setup()
    {
        _testHelper = new GitModuleTestHelper();

        string content = EmbeddedResourceLoader.Load(Assembly.GetExecutingAssembly(), $"{GetType().Namespace}.MockData.level3_roaming_GitExtensions.settings.xml");
        _userRoamingConfigFilePath = _testHelper.CreateFile(_testHelper.TemporaryPath + "/RoamingProfile", "GitExtensions.settings", content);
        content = EmbeddedResourceLoader.Load(Assembly.GetExecutingAssembly(), $"{GetType().Namespace}.MockData.level2_repodist_GitExtensions.settings.xml");
        _repoDistributedConfigFilePath = _testHelper.CreateRepoFile("GitExtensions.settings", content);
        content = EmbeddedResourceLoader.Load(Assembly.GetExecutingAssembly(), $"{GetType().Namespace}.MockData.level1_repogit_GitExtensions.settings.xml");
        _repoLocalConfigFilePath = _testHelper.CreateRepoFile(".git", "GitExtensions.settings", content);

        _userRoaming = new DistributedSettings(lowerPriority: null, new GitExtSettingsCache(_userRoamingConfigFilePath), SettingLevel.Global);
        _repoDistributed = new DistributedSettings(lowerPriority: _userRoaming, new GitExtSettingsCache(_repoDistributedConfigFilePath), SettingLevel.Distributed);
        _repoLocal = new DistributedSettings(lowerPriority: _repoDistributed, new GitExtSettingsCache(_repoLocalConfigFilePath), SettingLevel.Local);
        _effective = new DistributedSettings(lowerPriority: _repoLocal, new GitExtSettingsCache(settingsFilePath: null!), SettingLevel.Effective);
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
        // Explicitly set
        BuildServerSettings.ServerName[_userRoaming].Should().Be("Azure DevOps and Team Foundation Server (since TFS2015)");
        BuildServerSettings.IntegrationEnabled[_userRoaming].Should().BeTrue();
        BuildServerSettings.ShowBuildResultPage[_userRoaming].Should().BeNull();

        // Explicitly set
        SettingsSource settingsSource = BuildServerSettings.GetSettingsSource(_userRoaming);
        settingsSource.Should().BeOfType<SettingsPath>();
        ((SettingsPath)settingsSource).PathFor("").Should().Be("BuildServer.Azure DevOps and Team Foundation Server (since TFS2015).");
    }

    [Test]
    public void RepoDistributed_settings_should_return_expected()
    {
        // No explicit settings, inheriting from the repo local
        BuildServerSettings.ServerName[_repoLocal].Should().Be("AppVeyor");
        BuildServerSettings.IntegrationEnabled[_repoLocal].Should().BeTrue();

        // Explicitly set
        BuildServerSettings.ShowBuildResultPage[_repoLocal].Should().BeFalse();

        // No explicit settings, inheriting from the repo local
        SettingsSource settingsSource = BuildServerSettings.GetSettingsSource(_repoLocal);
        settingsSource.Should().BeOfType<SettingsPath>();
        ((SettingsPath)settingsSource).PathFor("").Should().Be("BuildServer.AppVeyor.");
    }

    [TestCase(true)]
    [TestCase(false)]
    public void RepoLocalAndEffective_settings_should_return_expected(bool isLocal)
    {
        SettingsSource source = isLocal ? _repoLocal : _effective;
        BuildServerSettings.ServerName[source].Should().Be("AppVeyor");
        BuildServerSettings.IntegrationEnabled[source].Should().BeTrue();

        // No explicit settings, inheriting from the repo distributed
        BuildServerSettings.ShowBuildResultPage[source].Should().BeFalse();

        SettingsSource settingsSource = BuildServerSettings.GetSettingsSource(source);
        settingsSource.Should().BeOfType<SettingsPath>();
        ((SettingsPath)settingsSource).PathFor("").Should().Be("BuildServer.AppVeyor.");
    }
}
