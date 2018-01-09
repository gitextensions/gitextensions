using System.Linq;
using System.Reflection;
using CommonTestUtils;
using FluentAssertions;
using GitCommands.GitExtLinks;
using GitCommands.Settings;
using NUnit.Framework;

namespace GitCommandsTests.GitExtLinks
{
    [TestFixture]
    public class ExternalLinksParserTests
    {
        private GitModuleTestHelper _testHelper;
        private string _level1, _level2, _level3;
        private RepoDistSettings _userRoaming;
        private RepoDistSettings _repoDistributed;
        private RepoDistSettings _repoLocal;
        private GitExtLinksParser _parser;


        [SetUp]
        public void Setup()
        {
            _testHelper = new GitModuleTestHelper();

            var content = EmbeddedResourceLoader.Load(Assembly.GetExecutingAssembly(), $"{GetType().Namespace}.MockData.level1_GitExtensions.settings.xml");
            _level1 = _testHelper.CreateRepoFile(".git", "GitExtensions.settings", content);
            content = EmbeddedResourceLoader.Load(Assembly.GetExecutingAssembly(), $"{GetType().Namespace}.MockData.level2_GitExtensions.settings.xml");
            _level2 = _testHelper.CreateFile(_testHelper.TemporaryPath + "/RoamingProfile", "GitExtensions.settings", content);
            content = EmbeddedResourceLoader.Load(Assembly.GetExecutingAssembly(), $"{GetType().Namespace}.MockData.level3_GitExtensions.settings.xml");
            _level3 = _testHelper.CreateRepoFile("GitExtensions.settings", content);

            _userRoaming = new RepoDistSettings(null, new GitExtSettingsCache(_level3));
            _repoDistributed = new RepoDistSettings(_userRoaming, new GitExtSettingsCache(_level2));
            _repoLocal = new RepoDistSettings(_repoDistributed, new GitExtSettingsCache(_level1));

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
        public void Can_load_1_layers_of_settings()
        {
            _parser = new GitExtLinksParser(_userRoaming);

            var effectiveSettings = _parser.EffectiveLinkDefs;

            effectiveSettings.Count.Should().Be(1);
        }

        [Test]
        public void Can_load_2_layers_of_settings()
        {
            _parser = new GitExtLinksParser(_repoDistributed);

            var effectiveSettings = _parser.EffectiveLinkDefs;

            // 1 comes from the user roaming settings
            // 3 come from the distributed
            effectiveSettings.Count.Should().Be(4);
        }

        [Test]
        public void Can_load_3_layers_of_settings()
        {
            _parser = new GitExtLinksParser(_repoLocal);

            var effectiveSettings = _parser.EffectiveLinkDefs;

            // 1 comes from the user roaming settings
            // 3 come from the distributed
            // 1 comes from the local
            effectiveSettings.Count.Should().Be(5);
        }

        [Test]
        public void Remove_should_add_definition_to_collection()
        {
            _parser = new GitExtLinksParser(_repoLocal);
            var definition = new GitExtLinkDef
            {
                Name = "test",
                SearchPattern = "pattern"
            };
            _parser.AddLinkDef(definition);

            var effectiveSettings = _parser.EffectiveLinkDefs;
            // 1 comes from the user roaming settings
            // 3 come from the distributed
            // 2 comes from the local
            effectiveSettings.Count.Should().Be(6);
        }

        [Test]
        public void Remove_should_remove_definition_from_collection()
        {
            _parser = new GitExtLinksParser(_repoLocal);
            var effectiveSettings = _parser.EffectiveLinkDefs;

            var definition = effectiveSettings[0]; // comes from the local
            _parser.RemoveLinkDef(definition);

            effectiveSettings = _parser.EffectiveLinkDefs;
            // 1 comes from the user roaming settings
            // 3 come from the distributed
            // 0 comes from the local
            effectiveSettings.Count.Should().Be(4);
        }

        [Test]
        public void Remove_should_save_definition_to_lowest_settings()
        {
            _parser = new GitExtLinksParser(_repoLocal);
            var definition = new GitExtLinkDef
            {
                Name = "test",
                SearchPattern = "pattern"
            };
            _parser.AddLinkDef(definition);

            _parser.SaveToSettings();
            _repoLocal.SettingsCache.Save();
            _repoDistributed.SettingsCache.Save();
            _userRoaming.SettingsCache.Save();

            var settings = new RepoDistSettings(null, new GitExtSettingsCache(_level3));
            var parser = new GitExtLinksParser(settings);
            parser.EffectiveLinkDefs.Count.Should().Be(2);
            var item = parser.EffectiveLinkDefs.Last();
            item.Name.Should().Be(definition.Name);
            item.SearchPattern.Should().Be(definition.SearchPattern);
        }
    }
}