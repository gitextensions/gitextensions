using FluentAssertions;
using GitCommands.Git;
using GitUI;

namespace GitUITests;

[TestFixture]
public sealed class UserEnvironmentInformationTests
{
    [Test]
    public void GitVersion_is_good()
    {
        string gitString = UserEnvironmentInformation.GetGitVersionInfo("2.21.0.windows.1", new GitVersion("2.18.0"),
            new GitVersion("2.21.0"));
        ClassicAssert.AreEqual("2.21.0.windows.1", gitString);
    }

    [Test]
    public void GitVersion_is_old_but_supported()
    {
        string gitString = UserEnvironmentInformation.GetGitVersionInfo("2.20.1.windows.1", new GitVersion("2.18.0"),
            new GitVersion("2.21.0"));
        ClassicAssert.AreEqual("2.20.1.windows.1 (recommended: 2.21.0 or later)", gitString);
    }

    [Test]
    public void GitVersion_is_not_supported()
    {
        string gitString = UserEnvironmentInformation.GetGitVersionInfo("1.6.5.windows.1", new GitVersion("2.18.0"),
            new GitVersion("2.21.0"));
        ClassicAssert.AreEqual("1.6.5.windows.1 (minimum: 2.18.0, please update!)", gitString);
    }

    [Test]
    public void GitVersion_is_unknown_then_return_all_data()
    {
        string gitString = UserEnvironmentInformation.GetGitVersionInfo(null, new GitVersion("2.18.0"),
            new GitVersion("2.21.0"));
        ClassicAssert.AreEqual("- (minimum: 2.18.0, recommended: 2.21.0)", gitString);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("Microsoft.WindowsDesktop.Other 1.2.3 ")]
    [TestCase("Microsoft.WindowsDesktop.App 1.2.3")] // not followed by whitespace
    [TestCase("Microsoft.WindowsDesktop.App \n")]
    [TestCase("Microsoft.WindowsDesktop.App x.y.z ")]
    [TestCase("Microsoft.WindowsDesktop.App 1.2.z ")]
    public void GetDotnetDesktopRuntimeVersions_shall_return_empty(string? versions)
    {
        UserEnvironmentInformation.GetDotnetDesktopRuntimeVersions(versions)
            .Should().BeEmpty();
    }

    [TestCase("1.2")]
    [TestCase("1.2.3")]
    [TestCase("1.2.3.4")]
    public void GetDotnetDesktopRuntimeVersions_shall_parse_version(string version)
    {
        UserEnvironmentInformation.GetDotnetDesktopRuntimeVersions($"Microsoft.WindowsDesktop.App {version} whatever")
            .Select(v => v.ToString()).Should().BeEquivalentTo([version]);
    }

    [TestCase("1.2-rc3.42.17", "1.2")]
    [TestCase("1.2.3-", "1.2.3")]
    [TestCase("1.2.3.4-suffix", "1.2.3.4")]
    public void GetDotnetDesktopRuntimeVersions_shall_ignore_suffix(string version, string expected)
    {
        UserEnvironmentInformation.GetDotnetDesktopRuntimeVersions($"Microsoft.WindowsDesktop.App {version} whatever")
            .Select(v => v.ToString()).Should().BeEquivalentTo([expected]);
    }
}
