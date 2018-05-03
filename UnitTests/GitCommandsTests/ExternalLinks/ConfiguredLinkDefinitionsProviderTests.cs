using System;
using System.Collections.Generic;
using System.Reflection;
using CommonTestUtils;
using FluentAssertions;
using GitCommands.ExternalLinks;
using GitCommands.Settings;
using NSubstitute;
using NUnit.Framework;

namespace GitCommandsTests.ExternalLinks
{
    [TestFixture]
    public class ConfiguredLinkDefinitionsProviderTests
    {
        private GitModuleTestHelper _testHelper;
        private string _level1;
        private string _level2;
        private string _level3;
        private RepoDistSettings _userRoaming;
        private RepoDistSettings _repoDistributed;
        private RepoDistSettings _repoLocal;
        private IExternalLinksStorage _externalLinksStorage;
        private ConfiguredLinkDefinitionsProvider _provider;

        [SetUp]
        public void Setup()
        {
            _testHelper = new GitModuleTestHelper();

            var content = EmbeddedResourceLoader.Load(Assembly.GetExecutingAssembly(), $"{GetType().Namespace}.MockData.level1_repogit_GitExtensions.settings.xml");
            _level1 = _testHelper.CreateRepoFile(".git", "GitExtensions.settings", content);
            content = EmbeddedResourceLoader.Load(Assembly.GetExecutingAssembly(), $"{GetType().Namespace}.MockData.level2_repodist_GitExtensions.settings.xml");
            _level2 = _testHelper.CreateFile(_testHelper.TemporaryPath + "/RoamingProfile", "GitExtensions.settings", content);
            content = EmbeddedResourceLoader.Load(Assembly.GetExecutingAssembly(), $"{GetType().Namespace}.MockData.level3_roaming_GitExtensions.settings.xml");
            _level3 = _testHelper.CreateRepoFile("GitExtensions.settings", content);

            _userRoaming = new RepoDistSettings(null, new GitExtSettingsCache(_level3));
            _repoDistributed = new RepoDistSettings(_userRoaming, new GitExtSettingsCache(_level2));
            _repoLocal = new RepoDistSettings(_repoDistributed, new GitExtSettingsCache(_level1));

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
            _testHelper = null;
        }

        [Test]
        public void Get_should_throw_if_data_null()
        {
            ((Action)(() => _provider.Get(null))).Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Can_load_1_layers_of_settings()
        {
            _externalLinksStorage.Load(Arg.Any<RepoDistSettings>()).Returns(new List<ExternalLinkDefinition>
            {
                new ExternalLinkDefinition { Name = "user definition 1" },
            });

            var effectiveSettings = _provider.Get(_userRoaming);

            effectiveSettings.Count.Should().Be(1);
        }

        [Test]
        public void Can_load_2_layers_of_settings()
        {
            _externalLinksStorage.Load(Arg.Any<RepoDistSettings>()).Returns(
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
            _externalLinksStorage.Load(Arg.Any<RepoDistSettings>()).Returns(
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