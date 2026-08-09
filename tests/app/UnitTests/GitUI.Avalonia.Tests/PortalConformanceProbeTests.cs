namespace GitUI.Avalonia.Tests;

[TestFixture]
public sealed class PortalConformanceProbeTests
{
    [TestCase("present")]
    [TestCase("absent")]
    public void Supported_request_requires_Linux_and_complete_arguments(string expectedMode)
    {
        GitExtensions.Compat.PortalConformanceProbe.IsSupportedRequest(
            "report.json",
            expectedMode,
            "fixture.txt",
            isLinux: true).Should().BeTrue();
    }

    [TestCase(null, "present", "fixture.txt", true)]
    [TestCase("report.json", null, "fixture.txt", true)]
    [TestCase("report.json", "other", "fixture.txt", true)]
    [TestCase("report.json", "present", null, true)]
    [TestCase("report.json", "present", "fixture.txt", false)]
    public void Incomplete_or_nonLinux_request_is_ignored(
        string? reportPath,
        string? expectedMode,
        string? fixturePath,
        bool isLinux)
    {
        GitExtensions.Compat.PortalConformanceProbe.IsSupportedRequest(
            reportPath,
            expectedMode,
            fixturePath,
            isLinux).Should().BeFalse();
    }
}
