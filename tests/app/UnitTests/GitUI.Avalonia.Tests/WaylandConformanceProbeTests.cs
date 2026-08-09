namespace GitExtensionsTests;

[TestFixture]
public sealed class WaylandConformanceProbeTests
{
    [TestCase(null, true, "wayland-0")]
    [TestCase("", true, "wayland-0")]
    [TestCase("report.json", false, "wayland-0")]
    [TestCase("report.json", true, null)]
    [TestCase("report.json", true, "")]
    public void IsSupportedRequest_should_reject_incomplete_or_non_Linux_requests(
        string? reportPath,
        bool isLinux,
        string? waylandDisplay)
    {
        GitExtensions.Compat.WaylandConformanceProbe.IsSupportedRequest(reportPath, isLinux, waylandDisplay)
            .Should().BeFalse();
    }

    [Test]
    public void IsSupportedRequest_should_accept_an_explicit_Linux_Wayland_report()
    {
        GitExtensions.Compat.WaylandConformanceProbe.IsSupportedRequest(
                "report.json",
                isLinux: true,
                waylandDisplay: "wayland-0")
            .Should().BeTrue();
    }
}
