using System.Diagnostics;
using System.Reflection;
using System.Text;
using Avalonia.Headless.NUnit;
using BugReporter.Serialization;
using GitExtensions;
using GitUI.Compat;
using GitUI.NBugReports;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class FatalBugReportTests
{
    [AvaloniaTest]
    public async Task Fatal_callback_should_handoff_its_actual_payload_on_the_failing_workerAsync()
    {
        using AppSettingsTestScope settings = new();
        Action<ProcessStartInfo>? previousStart = AvaloniaBugReportProcess.TestAccessor.StartProcess;
        var previousPresenter = UIReporter.TestAccessor.TaskDialogPresenter;
        Action<int> previousExit = UIReporter.TestAccessor.ExitApplication;
        ProcessStartInfo? observed = null;
        int workerThread = 0;
        int handoffThread = 0;
        int presentations = 0;
        int exits = 0;
        Exception exception = new InvalidOperationException("Owned fatal report payload", new IOException("Owned inner exception"));
        try
        {
            AvaloniaBugReportProcess.TestAccessor.StartProcess = start =>
            {
                observed = start;
                handoffThread = Environment.CurrentManagedThreadId;
            };
            UIReporter.TestAccessor.TaskDialogPresenter = (_, _) =>
            {
                presentations++;
                return TaskDialogButton.Cancel;
            };
            UIReporter.TestAccessor.ExitApplication = _ => exits++;
            MethodInfo report = typeof(App).GetMethod("ReportUnhandledException", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new InvalidOperationException("The production unhandled-exception callback is missing.");
            await Task.Run(() =>
            {
                workerThread = Environment.CurrentManagedThreadId;
                report.Invoke(null, [exception, true]);
            });

            presentations.Should().Be(0, "a fatal callback must not depend on the failed UI dispatcher");
            exits.Should().Be(0, "the CLR owns termination after the fatal handoff");
            ProcessStartInfo handoff = observed.Should().BeOfType<ProcessStartInfo>().Subject;
            handoffThread.Should().Be(workerThread);
            handoff.FileName.Should().Be(Environment.ProcessPath);
            handoff.UseShellExecute.Should().BeFalse();
            handoff.WorkingDirectory.Should().BeEmpty("the original reporter inherits the repository context");
            handoff.ArgumentList[^2].Should().Be("--bugreport");
            SerializableException payload = SerializableException.FromXmlString(
                Encoding.UTF8.GetString(Convert.FromBase64String(handoff.ArgumentList[^1])));
            payload.Type.Should().Be(typeof(InvalidOperationException).FullName);
            payload.Message.Should().Be(exception.Message);
            payload.InnerException.Should().NotBeNull();
            payload.InnerException!.Type.Should().Be(typeof(IOException).FullName);
            payload.InnerException.Message.Should().Be(exception.InnerException!.Message);
            if (string.Equals(Path.GetFileNameWithoutExtension(Environment.ProcessPath), "dotnet", StringComparison.OrdinalIgnoreCase))
            {
                handoff.ArgumentList[0].Should().Be(Assembly.GetEntryAssembly()!.Location);
                handoff.ArgumentList.Should().HaveCount(3);
            }
            else
            {
                handoff.ArgumentList.Should().HaveCount(2);
            }
        }
        finally
        {
            AvaloniaBugReportProcess.TestAccessor.StartProcess = previousStart;
            UIReporter.TestAccessor.TaskDialogPresenter = previousPresenter;
            UIReporter.TestAccessor.ExitApplication = previousExit;
        }
    }
}
