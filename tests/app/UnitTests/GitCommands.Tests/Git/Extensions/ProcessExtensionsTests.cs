using System.Diagnostics;
using GitCommands.Git.Extensions;

namespace GitCommandsTests.Git.Extensions;

[Platform(Include = "Win")]
public class ProcessExtensionsTests
{
    [Test]
    public void TryGetCommandLine_should_return_command_line_of_other_process()
    {
        const string arguments = "-n 30 127.0.0.1";
        string ping = Path.Combine(Environment.SystemDirectory, "ping.exe");
        using Process process = Process.Start(new ProcessStartInfo(ping, arguments) { CreateNoWindow = true, UseShellExecute = false, RedirectStandardOutput = true })!;
        try
        {
            string? commandLine = process.TryGetCommandLine();

            commandLine.Should().ContainEquivalentOf("ping").And.EndWith(arguments);
        }
        finally
        {
            process.Kill();
        }
    }

    [Test]
    public void TryGetCommandLine_should_return_command_line_of_current_process()
    {
        using Process process = Process.GetCurrentProcess();

        process.TryGetCommandLine().Should().ContainEquivalentOf(Path.GetFileNameWithoutExtension(Environment.ProcessPath));
    }
}
