using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Xml.Linq;
using Avalonia;
using Avalonia.Headless;

namespace GitExtensionsTests;

// parity-scaffolding: threading checks need a non-pool UI thread, as the desktop entry point does.
internal static class DedicatedHeadlessTestSession
{
    private const string ChildTestVariable = "GITEXTENSIONS_DEDICATED_HEADLESS_CHILD";

    public static async Task RunAsync(Func<Task> action)
    {
        string testName = TestContext.CurrentContext.Test.FullName;
        if (Environment.GetEnvironmentVariable(ChildTestVariable) == testName)
        {
            await RunDedicatedAsync(action);
            return;
        }

        // Avalonia rendering resources can retain their creating dispatcher across application scopes.
        string directory = Path.Combine(Path.GetTempPath(), $"GitExtensions.ThreadingEvidence-{Environment.ProcessId}-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        string assembly = typeof(DedicatedHeadlessTestSession).Assembly.Location;
        ProcessStartInfo start = new(Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") ?? "dotnet")
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        start.ArgumentList.Add("vstest");
        start.ArgumentList.Add(assembly);
        start.ArgumentList.Add("--TestCaseFilter:FullyQualifiedName=" + testName);
        start.ArgumentList.Add("--TestAdapterPath:" + Path.GetDirectoryName(assembly));
        start.ArgumentList.Add("--ResultsDirectory:" + directory);
        start.ArgumentList.Add("--Logger:trx;LogFileName=threading.trx");
        start.Environment[ChildTestVariable] = testName;
        using Process process = Process.Start(start) ?? throw new InvalidOperationException("Could not start the isolated test host.");
        Task<string> output = process.StandardOutput.ReadToEndAsync();
        Task<string> errors = process.StandardError.ReadToEndAsync();
        using CancellationTokenSource deadline = new(TimeSpan.FromMinutes(2));
        try
        {
            await process.WaitForExitAsync(deadline.Token);
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync();
            }

            throw;
        }

        string log = await output + Environment.NewLine + await errors;
        string logPath = Path.Combine(directory, "threading.log");
        await File.WriteAllTextAsync(logPath, log);
        TestContext.AddTestAttachment(logPath);
        string resultPath = Path.Combine(directory, "threading.trx");
        if (File.Exists(resultPath))
        {
            TestContext.AddTestAttachment(resultPath);
        }

        process.ExitCode.Should().Be(0, log);
        XDocument results = XDocument.Load(resultPath);
        XNamespace ns = results.Root!.Name.Namespace;
        XElement result = results.Descendants(ns + "UnitTestResult").Should().ContainSingle().Which;
        result.Attribute("outcome")!.Value.Should().Be("Passed");
        await TestContext.Out.WriteLineAsync($"Isolated {testName}: 1 passed, 0 skipped; child {process.Id}");
        await TestContext.Out.WriteLineAsync(result.Element(ns + "Output")?.Element(ns + "StdOut")?.Value);
    }

    private static async Task RunDedicatedAsync(Func<Task> action)
    {
        using CancellationTokenSource cancellation = new();
        using BlockingCollection<(Action Action, ExecutionContext? Context)> queue = new();
        Task dispatch = new(() =>
        {
            while (!cancellation.IsCancellationRequested)
            {
                try
                {
                    (Action callback, ExecutionContext? context) = queue.Take(cancellation.Token);
                    if (context is null)
                    {
                        callback();
                    }
                    else
                    {
                        ExecutionContext.Run(context, state => ((Action)state!).Invoke(), callback);
                    }
                }
                catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
                {
                }
            }
        }, TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach);

        // The SDK factory fixes its loop to the pool; retain its actual application/dispatcher isolation with a dedicated loop.
        ConstructorInfo constructor = typeof(HeadlessUnitTestSession).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            [typeof(AppBuilder), typeof(CancellationTokenSource), typeof(BlockingCollection<(Action, ExecutionContext?)>), typeof(Task), typeof(bool)],
            modifiers: null) ?? throw new MissingMethodException(typeof(HeadlessUnitTestSession).FullName, ".ctor");
        HeadlessUnitTestSession session = (HeadlessUnitTestSession)constructor.Invoke(
            [TestAppBuilder.BuildAvaloniaApp(), cancellation, queue, dispatch, true]);
        dispatch.Start(TaskScheduler.Default);
        try
        {
            await session.Dispatch(async () =>
            {
                await action();
                return true;
            }, CancellationToken.None);
        }
        finally
        {
            await session.DisposeAsync();
        }
    }
}
