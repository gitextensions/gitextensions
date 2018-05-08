using System.Linq;
using System.Reflection;
using CommonTestUtils;
using FluentAssertions;
using GitCommands.ExternalLinks;
using GitCommands.Settings;
using NUnit.Framework;

namespace GitCommandsTests.ExternalLinks
{
    [TestFixture]
    public class ExternalLinksManagerIntegrationTests
    {
        private GitModuleTestHelper _testHelper;
        private string _level1;
        private string _level2;
        private string _level3;
        private RepoDistSettings _userRoaming;
        private RepoDistSettings _repoDistributed;
        private RepoDistSettings _repoLocal;
        private ExternalLinksStorage _externalLinksStorage;

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

            _externalLinksStorage = new ExternalLinksStorage();
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
        public void Add_should_add_new_definition_to_the_lowest_level_level2()
        {
            _externalLinksStorage.Load(_userRoaming).Count.Should().Be(1);

            var manager = new ExternalLinksManager(_repoDistributed);

            // 1 comes from the user roaming settings
            // 3 come from the distributed
            manager.GetEffectiveSettings().Count.Should().Be(4);

            var definition = new ExternalLinkDefinition
            {
                Name = "test",
                SearchPattern = "pattern"
            };
            manager.Add(definition);

            var effectiveSettings = manager.GetEffectiveSettings();

            // 2 comes from the user roaming settings
            // 3 come from the distributed
            effectiveSettings.Count.Should().Be(5);

            manager.Save();

            _externalLinksStorage.Load(_userRoaming).Count.Should().Be(2);
        }

        [Test]
        public void Add_should_add_new_definition_to_the_lowest_level_level3()
        {
            _externalLinksStorage.Load(_userRoaming).Count.Should().Be(1);

            var manager = new ExternalLinksManager(_repoLocal);

            // 1 comes from the user roaming settings
            // 3 come from the distributed
            // 1 comes from the local
            manager.GetEffectiveSettings().Count.Should().Be(5);

            var definition = new ExternalLinkDefinition
            {
                Name = "test",
                SearchPattern = "pattern"
            };
            manager.Add(definition);

            var effectiveSettings = manager.GetEffectiveSettings();

            // 1 comes from the local
            // 3 come from the distributed
            // 2 comes from the local
            effectiveSettings.Count.Should().Be(6);

            manager.Save();

            _externalLinksStorage.Load(_userRoaming).Count.Should().Be(2);
        }

        [Test]
        public void Add_should_add_definition_present_in_lower_level_to_higher_level()
        {
            _externalLinksStorage.Load(_userRoaming).Count.Should().Be(1);
            _externalLinksStorage.Load(_repoDistributed).Count.Should().Be(3);

            var manager = new ExternalLinksManager(_repoLocal);

            // 1 comes from the user roaming settings
            // 3 come from the distributed
            // 1 comes from the local
            manager.GetEffectiveSettings().Count.Should().Be(5);

            var definition = new ExternalLinkDefinition
            {
                Name = "Stash",
                SearchPattern = "pattern"
            };
            manager.Add(definition);

            var effectiveSettings = manager.GetEffectiveSettings();

            // 1 comes from the user roaming settings
            // 4 come from the distributed
            // 1 comes from the local
            effectiveSettings.Count.Should().Be(6);

            manager.Save();

            _externalLinksStorage.Load(_userRoaming).Count.Should().Be(1);
            _externalLinksStorage.Load(_repoDistributed).Count.Should().Be(4);
        }

        [Test]
        public void Remove_should_remove_definition_from_collection()
        {
            _externalLinksStorage.Load(_userRoaming).Count.Should().Be(1);
            _externalLinksStorage.Load(_repoDistributed).Count.Should().Be(3);

            var manager = new ExternalLinksManager(_repoLocal);

            var effective = manager.GetEffectiveSettings();

            // 1 comes from the user roaming settings
            // 3 come from the distributed
            // 1 comes from the local
            effective.Count.Should().Be(5);

            manager.Remove(effective.Last());

            manager.Save();

            var effectiveSettings = manager.GetEffectiveSettings();

            // 0 comes from the user roaming settings
            // 3 come from the distributed
            // 1 comes from the local
            effectiveSettings.Count.Should().Be(4);

            _externalLinksStorage.Load(_userRoaming).Count.Should().Be(0);
            _externalLinksStorage.Load(_repoDistributed).Count.Should().Be(3);
        }
    }
}