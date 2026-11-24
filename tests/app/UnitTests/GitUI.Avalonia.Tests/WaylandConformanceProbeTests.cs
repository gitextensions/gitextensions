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

    [TestCase(false, "wayland.json", "wayland-0", null, null, null)]
    [TestCase(true, null, "wayland-0", null, ":100", null)]
    [TestCase(true, null, null, "x11.json", null, null)]
    [TestCase(true, "wayland.json", "wayland-0", null, null, "wayland")]
    [TestCase(true, null, null, "x11.json", ":100", "x11")]
    [TestCase(true, "wayland.json", "wayland-0", "x11.json", ":100", "wayland")]
    public void SelectBackend_should_require_an_explicit_report_and_matching_display(
        bool isLinux,
        string? waylandReportPath,
        string? waylandDisplay,
        string? x11ReportPath,
        string? x11Display,
        string? expected)
    {
        GitExtensions.Compat.WaylandConformanceProbe.SelectBackend(
                isLinux,
                waylandReportPath,
                waylandDisplay,
                x11ReportPath,
                x11Display)
            .Should().Be(expected);
    }
}
