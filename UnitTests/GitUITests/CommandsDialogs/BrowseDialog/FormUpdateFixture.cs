using System;
using System.Linq;
using FluentAssertions;
using GitCommands.Config;
using GitUI.CommandsDialogs.BrowseDialog;
using NUnit.Framework;

namespace GitUITests.CommandsDialogs.BrowseDialog
{
    [TestFixture]
    public class FormUpdateFixture
    {
        private static string GetReleasesConfigFileText()
        {
            var configFile = new ConfigFile("", true);
            configFile.SetValue("Version \"2.47\".ReleaseType", "Major");
            configFile.SetValue("Version \"2.48\".ReleaseType", "Major");
            configFile.SetValue("Version \"2.49\".ReleaseType", "ReleaseCandidate");
            configFile.SetValue("RCVersion \"2.50\".ReleaseType", "ReleaseCandidate");

            return configFile.GetAsString();
        }

        [Test]
        public void CheckForReleaseCandidatesTest()
        {
            var currentVersion = new Version(2, 47);
            var availableVersions = ReleaseVersion.Parse(GetReleasesConfigFileText());

            var updates = ReleaseVersion.GetNewerVersions(currentVersion, true, availableVersions);
            var expectedVersions = new[]
            {
                new Version(2, 48),
                new Version(2, 49),
                new Version(2, 50)
            };
            updates.Select(rv => rv.Version).Should().Equal(expectedVersions);
        }

        [Test]
        public void CheckForMajorReleasesTest()
        {
            var currentVersion = new Version(2, 47);
            var availableVersions = ReleaseVersion.Parse(GetReleasesConfigFileText());

            var updates = ReleaseVersion.GetNewerVersions(currentVersion, false, availableVersions);
            var expectedVersions = new[]
            {
                new Version(2, 48)
            };
            updates.Select(rv => rv.Version).Should().Equal(expectedVersions);
        }

        [Test]
        public void CheckForNoMajorReleasesTest()
        {
            var currentVersion = new Version(2, 48);
            var availableVersions = ReleaseVersion.Parse(GetReleasesConfigFileText());

            var updates = ReleaseVersion.GetNewerVersions(currentVersion, false, availableVersions);
            updates.Select(rv => rv.Version).Should().BeEmpty();
        }
    }
}
