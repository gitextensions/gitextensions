﻿using FluentAssertions;
using GitCommands.Config;
using GitUI.CommandsDialogs.BrowseDialog;

namespace GitUITests.CommandsDialogs.BrowseDialog
{
    [TestFixture]
    public class ReleaseVersionTests
    {
        private static string GetReleasesConfigFileText()
        {
            ConfigFile configFile = new(fileName: "");
            configFile.SetValue("Version \"2.47\".ReleaseType", "Major");
            configFile.SetValue("Version \"2.48\".ReleaseType", "Major");
            configFile.SetValue("Version \"2.49\".ReleaseType", "ReleaseCandidate");
            configFile.SetValue("RCVersion \"2.50\".ReleaseType", "ReleaseCandidate");

            return configFile.GetAsString();
        }

        [Test]
        public void CheckForReleaseCandidatesTest()
        {
            Version currentVersion = new(2, 47);
            IEnumerable<ReleaseVersion> availableVersions = ReleaseVersion.Parse(GetReleasesConfigFileText());

            IEnumerable<ReleaseVersion> updates = ReleaseVersion.GetNewerVersions(currentVersion, true, availableVersions);
            Version[] expectedVersions = new[]
            {
                new Version(2, 48),
                new Version(2, 49),
                new Version(2, 50)
            };
            updates.Select(rv => rv.ApplicationVersion).Should().Equal(expectedVersions);
        }

        [Test]
        public void CheckForMajorReleasesTest()
        {
            Version currentVersion = new(2, 47);
            IEnumerable<ReleaseVersion> availableVersions = ReleaseVersion.Parse(GetReleasesConfigFileText());

            IEnumerable<ReleaseVersion> updates = ReleaseVersion.GetNewerVersions(currentVersion, false, availableVersions);
            Version[] expectedVersions = new[]
            {
                new Version(2, 48)
            };
            updates.Select(rv => rv.ApplicationVersion).Should().Equal(expectedVersions);
        }

        [Test]
        public void CheckForNoMajorReleasesTest()
        {
            Version currentVersion = new(2, 48);
            IEnumerable<ReleaseVersion> availableVersions = ReleaseVersion.Parse(GetReleasesConfigFileText());

            IEnumerable<ReleaseVersion> updates = ReleaseVersion.GetNewerVersions(currentVersion, false, availableVersions);
            updates.Select(rv => rv.ApplicationVersion).Should().BeEmpty();
        }
    }
}
