using System.Diagnostics;
using GitCommands.Git.Extensions;

namespace GitExtensionsTests;

[TestFixture]
public sealed class ProcessExtensionsTests
{
    [Test]
    public async Task TerminateTree_should_kill_a_linux_process_group()
    {
        if (!OperatingSystem.IsLinux())
        {
            Assert.Ignore("Linux process groups are required for this test.");
        }

        using Process process = new()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                RedirectStandardOutput = true,
                UseShellExecute = false,
            },
        };
        process.StartInfo.ArgumentList.Add("-c");
        process.StartInfo.ArgumentList.Add(
            "orphan=$(bash -c 'sleep 100 >/dev/null 2>&1 & echo $!; disown'); "
            + "sleep 100 & child=$!; echo \"$$ $(ps -o pgid= -p $$) $orphan $child\"; wait");

        List<int> childProcessIds = [];
        bool started = false;
        try
        {
            process.StartInOwnProcessGroup().Should().BeTrue();
            started = true;
            string? processIds = await process.StandardOutput.ReadLineAsync().WaitAsync(TimeSpan.FromSeconds(5));
            int[] ids = processIds!.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
            ids.Should().HaveCount(4);
            ids[0].Should().Be(process.Id);
            ids[1].Should().Be(process.Id, "setsid should make the command its process-group leader");
            childProcessIds.AddRange(ids[2..]);

            process.TerminateTree();

            await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(5));
            SpinWait.SpinUntil(
                () => childProcessIds.All(ProcessTestHelper.HasExited),
                TimeSpan.FromSeconds(5)).Should().BeTrue();
        }
        finally
        {
            if (started && !process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }

            foreach (int processId in childProcessIds.Where(processId => !ProcessTestHelper.HasExited(processId)))
            {
                Process.GetProcessById(processId).Kill();
            }
        }
    }
}
