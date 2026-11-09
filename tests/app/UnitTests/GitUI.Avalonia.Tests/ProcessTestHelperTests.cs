using System.Diagnostics;

namespace GitExtensionsTests;

[TestFixture]
public sealed class ProcessTestHelperTests
{
    [Test]
    public async Task HasExited_should_check_the_actual_lifetime_when_procfs_read_fails([Values] bool exited)
    {
        ProcessStartInfo start = new(OperatingSystem.IsWindows() ? Path.Combine(Environment.SystemDirectory, "cmd.exe") : "/bin/sh")
        {
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        if (OperatingSystem.IsWindows())
        {
            start.ArgumentList.Add("/d");
            start.ArgumentList.Add("/c");
        }
        else
        {
            start.ArgumentList.Add("-c");
        }

        start.ArgumentList.Add("exit 0");
        using Process process = exited ? new Process { StartInfo = start } : Process.GetCurrentProcess();
        bool started = false;
        try
        {
            if (exited)
            {
                process.Start().Should().BeTrue();
                started = true;
                await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(5));
                process.ExitCode.Should().Be(0);
            }

            int reads = 0;
            bool actual = ProcessTestHelper.HasExited(process.Id, _ =>
            {
                reads++;
                throw new IOException("The process disappeared while its procfs stat file was open.");
            });
            actual.Should().Be(exited);
            reads.Should().Be(OperatingSystem.IsLinux() ? 1 : 0);
        }
        finally
        {
            if (started && !process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync();
            }
        }
    }
}
