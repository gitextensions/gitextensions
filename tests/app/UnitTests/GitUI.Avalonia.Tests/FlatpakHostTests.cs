using System.Diagnostics;
using GitUI.Compat;

namespace GitExtensionsTests;

[TestFixture]
public sealed class FlatpakHostTests
{
    [Test]
    public void ResolveExecutable_should_query_the_host_PATH_with_a_separate_argument()
    {
        ProcessStartInfo? captured = null;
        FlatpakHost host = new(request =>
        {
            captured = request;
            return (0, "/opt/tools/my terminal\n", string.Empty);
        });

        host.ResolveExecutable("my terminal; false").Should().Be("/opt/tools/my terminal");

        captured!.FileName.Should().Be("/usr/bin/flatpak-spawn");
        captured.ArgumentList.Should().Equal(
            "--host", "--watch-bus", "/bin/sh", "-c", "command -v \"$1\" || exit 44",
            "gitextensions-tool-lookup", "my terminal; false");
        captured.UseShellExecute.Should().BeFalse();
    }

    [Test]
    public void ResolveExecutable_should_return_null_only_when_the_host_cannot_find_the_tool()
    {
        FlatpakHost host = new(_ => (44, string.Empty, string.Empty));

        host.ResolveExecutable("missing-terminal").Should().BeNull();
    }

    [TestCase("org.freedesktop.Flatpak is not available")]
    [TestCase("org.freedesktop.DBus.Error.AccessDenied")]
    public void ResolveExecutable_should_report_a_missing_service_or_revoked_permission(string failure)
    {
        FlatpakHost host = new(_ => (1, string.Empty, failure));

        Action resolve = () => host.ResolveExecutable("xdg-terminal-exec");

        resolve.Should().Throw<PlatformNotSupportedException>()
            .WithMessage($"*not available in this Flatpak installation*{failure}*");
    }

    [Test]
    public void ForwardToHost_should_preserve_the_argument_vector_streams_and_working_folder()
    {
        ProcessStartInfo startInfo = new("/opt/My tool/launch")
        {
            WorkingDirectory = "/home/test/repository with spaces",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        startInfo.ArgumentList.Add("a file\"with quotes");
        startInfo.ArgumentList.Add("$(not-a-command)");

        FlatpakHost.ForwardToHost(startInfo);

        startInfo.FileName.Should().Be("/usr/bin/flatpak-spawn");
        startInfo.ArgumentList.Should().Equal(
            "--host", "--watch-bus", "--directory=/home/test/repository with spaces",
            "/opt/My tool/launch", "a file\"with quotes", "$(not-a-command)");
        startInfo.RedirectStandardInput.Should().BeTrue();
        startInfo.RedirectStandardOutput.Should().BeTrue();
        startInfo.RedirectStandardError.Should().BeTrue();
    }
}
