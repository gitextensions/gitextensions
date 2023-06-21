using System.Reflection;
using CommonTestUtils;
using FluentAssertions;
using GitCommands.ExternalLinks;
using GitCommands.Settings;
using GitUIPluginInterfaces;
using NSubstitute;

namespace GitCommandsTests.ExternalLinks
{
    [TestFixture]
    public class ConfiguredLinkDefinitionsProviderTests
    {
        private GitModuleTestHelper _testHelper;
        private string _repoLocalConfigFilePath;
        private string _repoDistributedConfigFilePath;
        private string _userRoamingConfigFilePath;
        private DistributedSettings _userRoaming;
        private DistributedSettings _repoDistributed;
        private DistributedSettings _repoLocal;
        private IExternalLinksStorage _externalLinksStorage;
        private ConfiguredLinkDefinitionsProvider _provider;

        [SetUp]
        public void Setup()
        {
            _testHelper = new GitModuleTestHelper();

            var content = EmbeddedResourceLoader.Load(Assembly.GetExecutingAssembly(), $"{GetType().Namespace}.MockData.level1_repogit_GitExtensions.settings.xml");
            _repoLocalConfigFilePath = _testHelper.CreateRepoFile(".git", "GitExtensions.settings", content);
            content = EmbeddedResourceLoader.Load(Assembly.GetExecutingAssembly(), $"{GetType().Namespace}.MockData.level2_repodist_GitExtensions.settings.xml");
            _repoDistributedConfigFilePath = _testHelper.CreateFile(_testHelper.TemporaryPath + "/RoamingProfile", "GitExtensions.settings", content);
            content = EmbeddedResourceLoader.Load(Assembly.GetExecutingAssembly(), $"{GetType().Namespace}.MockData.level3_roaming_GitExtensions.settings.xml");
            _userRoamingConfigFilePath = _testHelper.CreateRepoFile("GitExtensions.settings", content);

            _userRoaming = new DistributedSettings(lowerPriority: null, new GitExtSettingsCache(_userRoamingConfigFilePath), SettingLevel.Global);
            _repoDistributed = new DistributedSettings(lowerPriority: _userRoaming, new GitExtSettingsCache(_repoDistributedConfigFilePath), SettingLevel.Distributed);
            _repoLocal = new DistributedSettings(lowerPriority: _repoDistributed, new GitExtSettingsCache(_repoLocalConfigFilePath), SettingLevel.Local);

            _externalLinksStorage = Substitute.For<IExternalLinksStorage>();

            _provider = new ConfiguredLinkDefinitionsProvider(_externalLinksStorage);
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
        public void Get_should_throw_if_data_null()
        {
            ((Action)(() => _provider.Get(null))).Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Can_load_1_layers_of_settings()
        {
            _externalLinksStorage.Load(Arg.Any<DistributedSettings>()).Returns(new List<ExternalLinkDefinition>
            {
                new ExternalLinkDefinition { Name = "user definition 1" },
            });

            var effectiveSettings = _provider.Get(_userRoaming);

            effectiveSettings.Count.Should().Be(1);
        }

        [Test]
        public void Can_load_2_layers_of_settings()
        {
            _externalLinksStorage.Load(Arg.Any<DistributedSettings>()).Returns(
                new List<ExternalLinkDefinition>
                {
                    new ExternalLinkDefinition { Name = "local definition 1" },
                },
                new List<ExternalLinkDefinition>
                {
                    new ExternalLinkDefinition { Name = "distributed definition 1" },
                    new ExternalLinkDefinition { Name = "distributed definition 2" },
                    new ExternalLinkDefinition { Name = "distributed definition 3" },
                });

            var effectiveSettings = _provider.Get(_repoDistributed);

            // 1 comes from the user roaming settings
            // 3 come from the distributed
            effectiveSettings.Count.Should().Be(4);
        }

        [Test]
        public void Can_load_3_layers_of_settings()
        {
            _externalLinksStorage.Load(Arg.Any<DistributedSettings>()).Returns(
                new List<ExternalLinkDefinition>
                {
                    new ExternalLinkDefinition { Name = "local definition 1" },
                },
                new List<ExternalLinkDefinition>
                {
                    new ExternalLinkDefinition { Name = "distributed definition 1" },
                    new ExternalLinkDefinition { Name = "distributed definition 2" },
                    new ExternalLinkDefinition { Name = "distributed definition 3" },
                },
                new List<ExternalLinkDefinition>
                {
                    new ExternalLinkDefinition { Name = "user definition 1" },
                });

            var effectiveSettings = _provider.Get(_repoLocal);

            // 1 comes from the user roaming settings
            // 3 come from the distributed
            // 1 comes from the local
            effectiveSettings.Count.Should().Be(5);
        }
    }
}
