using System.Diagnostics;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using GitUI;
using GitUI.ConsoleEmulation.PlainText;
using Microsoft.VisualStudio.Threading;

namespace GitExtensionsTests;

[TestFixture]
[Explicit("diagnostic probe")]
public sealed class RunnerProbeTests
{
    [AvaloniaTest]
    public void Probe_runner_exit_path()
    {
        TestContext.Out.WriteLine($"UIThread={Dispatcher.UIThread.CheckAccess()} SyncCtx={SynchronizationContext.Current?.GetType().Name}");
        GitExtensions.Shims.WinForms.Application.ThreadException += (_, e) => TestContext.Out.WriteLine($"ThreadException: {e.Exception}");
        if (!ThreadHelper.HasJoinableTaskContext)
        {
            AvaloniaSynchronizationContext.InstallIfNeeded();
            ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        }

        TestContext.Out.WriteLine($"JTC.IsOnMainThread={ThreadHelper.JoinableTaskContext.IsOnMainThread}");

        PlainTextConsoleCommandRunner runner = new();
        int exitCode = int.MinValue;
        string output = "";
        runner.CommandOutputReceived += (_, e) => output += e.Text;
        runner.CommandProcessExited += (_, e) => exitCode = e.ExitCode;

        runner.StartCommand("git", "version", Path.GetTempPath(), []);

        Stopwatch stopwatch = Stopwatch.StartNew();
        while (exitCode == int.MinValue && stopwatch.ElapsedMilliseconds < 15_000)
        {
            Dispatcher.UIThread.RunJobs();
            Thread.Sleep(25);
        }

        TestContext.Out.WriteLine($"exitCode={exitCode} output=<{output.Trim()}>");
        exitCode.Should().Be(0);
        output.Should().Contain("git version");
    }
}
