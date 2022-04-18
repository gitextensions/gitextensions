using System.IO;
using System.Reflection;
using ApprovalTests;
using CommonTestUtils;
using FluentAssertions;
using GitCommands.ExternalLinks;
using GitCommands.Settings;
using GitUIPluginInterfaces;
using NUnit.Framework;

namespace GitCommandsTests.ExternalLinks
{
    [TestFixture]
    public class ExternalLinksStorageIntegrationTests
    {
        private ExternalLinksStorage _externalLinksStorage;

        [SetUp]
        public void Setup()
        {
            _externalLinksStorage = new ExternalLinksStorage();
        }

        [TestCase("level1_repogit_GitExtensions", 1)]
        [TestCase("level2_repodist_GitExtensions", 3)]
        [TestCase("level3_roaming_GitExtensions", 1)]
        public void Can_load_settings(string fileName, int expected)
        {
            var content = EmbeddedResourceLoader.Load(Assembly.GetExecutingAssembly(), $"{GetType().Namespace}.MockData.{fileName}.settings.xml");

            using GitModuleTestHelper testHelper = new();
            var settingsFile = testHelper.CreateRepoFile(".git", "GitExtensions.settings", content);
            using GitExtSettingsCache settingsCache = new(settingsFile);
            RepoDistSettings settings = new(null, settingsCache, SettingLevel.Unknown);

            var definitions = _externalLinksStorage.Load(settings);
            definitions.Count.Should().Be(expected);
        }

        [Test]
        public void Can_save_settings()
        {
            using GitModuleTestHelper testHelper = new();
            string settingsFile = testHelper.CreateRepoFile(".git", "GitExtensions.settings", "﻿<dictionary />");
            using GitExtSettingsCache settingsCache = new(settingsFile);
            RepoDistSettings settings = new(null, settingsCache, SettingLevel.Unknown);

            ExternalLinkDefinition definition = new()
            {
                Name = "<new>",
                Enabled = true,
                UseRemotesPattern = "upstream|origin",
                UseOnlyFirstRemote = true,
                SearchInParts = { ExternalLinkDefinition.RevisionPart.Message },
                RemoteSearchInParts = { ExternalLinkDefinition.RemotePart.URL }
            };

            _externalLinksStorage.Save(settings, new[] { definition });

            settings.Save();

            Approvals.VerifyXml(File.ReadAllText(settingsFile));
        }
    }
}
