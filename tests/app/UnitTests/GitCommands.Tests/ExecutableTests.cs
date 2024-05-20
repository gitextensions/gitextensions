using System.Text;
using FluentAssertions;
using GitCommands;
using GitCommands.Logging;
using GitExtensions.Extensibility;
using GitUI;
using Microsoft.VisualStudio.Threading;

namespace GitCommandsTests
{
    public sealed class ExecutableTests
    {
        [SetUp]
        public void SetUp()
        {
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public async Task Process_shall_be_killed_on_cancellation()
        {
            TimeSpan cancelDelay = TimeSpan.FromSeconds(1);

            await TaskScheduler.Default;

            using CancellationTokenSource cts = new();

            // start a process running for seconds
            IExecutable executable = new Executable("ping.exe");
            IProcess process = executable.Start($"-n {cancelDelay.TotalSeconds + 60} 127.0.0.1", cancellationToken: cts.Token);
            DateTime startedAt = DateTime.Now;

            // cancel after delay
            await Task.Delay(cancelDelay);
            await cts.CancelAsync();

            // the process is killed on dispose but not before
            using (CancellationTokenSource ctsWaitExit = new())
            {
                try
                {
                    ctsWaitExit.CancelAfter(cancelDelay);
                    int exitCode = await process.WaitForExitAsync(ctsWaitExit.Token);
                    Assert.Fail("should not have exited", exitCode);
                }
                catch (OperationCanceledException)
                {
                }
            }

            process.Dispose();

            TimeSpan durationWaitTimeout = DateTime.Now - startedAt;
            durationWaitTimeout.Should().BeGreaterThan(1.5 * cancelDelay).And.BeLessThan(2.5 * cancelDelay);

            CommandLogEntry? cmd = CommandLog.Commands.LastOrDefault();
            (cmd?.Exception?.Message).Should().Be("Process killed");
        }

        [Test]
        public async Task WaitForProcessExitAsync_shall_return_latest_after_timeout()
        {
            TimeSpan halfRuntime = TimeSpan.FromSeconds(3);

            await TaskScheduler.Default;

            // start a process running for seconds
            IExecutable executable = new Executable("ping.exe");
            using IProcess process = executable.Start($"-n {(halfRuntime.TotalSeconds * 2) + 1} 127.0.0.1");

            // wait for process exit, but cancel the wait while the process is still running
            using CancellationTokenSource cts = new();
            DateTime startedAt = DateTime.Now;
            cts.CancelAfter(halfRuntime);
            try
            {
                await process.WaitForExitAsync(cts.Token); // may or may not throw on cancel
            }
            catch (OperationCanceledException)
            {
                // ignore
            }

            TimeSpan durationWaitTimeout = DateTime.Now - startedAt;
            durationWaitTimeout.Should().BeGreaterThan(0.5 * halfRuntime).And.BeLessThan(1.5 * halfRuntime);

            // wait for process exit without cancellation
            await process.WaitForExitAsync(CancellationToken.None);

            TimeSpan durationExit = DateTime.Now - startedAt;
            durationExit.Should().BeGreaterThan(1.5 * halfRuntime).And.BeLessThan(2.5 * halfRuntime);
        }

        [Test]
        public async Task ExecuteAsync_shall_return_latest_after_timeout([Values("cmd.exe", "ping.exe")] string exeFile)
        {
            const int cancelDelay = 1000;
            const int exitDelay = cancelDelay;
            const int minRuntime = cancelDelay + exitDelay;
            string arguments = exeFile.Contains("ping") ? $"-n {(minRuntime / 1000) + 2} 127.0.0.1" : "";

            using CancellationTokenSource cancellationTokenSource = new();
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            IExecutable executable = new Executable(exeFile);

            Exception exception = null;
            ExecutionResult? executionResult = null;
            async Task ExecuteAsync()
            {
                try
                {
                    await TaskScheduler.Default.SwitchTo(alwaysYield: true);
                    executionResult = await executable.ExecuteAsync(arguments, outputEncoding: Encoding.Default, cancellationToken: cancellationToken);
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            }

            _ = ThreadHelper.JoinableTaskFactory.RunAsync(ExecuteAsync, JoinableTaskCreationOptions.LongRunning);

            await Task.Delay(cancelDelay);
            await cancellationTokenSource.CancelAsync();
            await Task.Delay(exitDelay);

            exception.Should().NotBeNull();
            exception.GetType().Should().Be<OperationCanceledException>();
            executionResult.Should().BeNull();
        }
    }
}
