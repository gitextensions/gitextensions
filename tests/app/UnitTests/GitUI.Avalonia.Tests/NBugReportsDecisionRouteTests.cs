using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Avalonia.Controls;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.Compat;
using GitUI.NBugReports;
using NSubstitute;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUITests.NBugReports;

[NonParallelizable]
public sealed class NBugReportsDecisionRouteTests
{
    private const string ChildScenarioVariable = "GITEXT_NBUG_CHILD_SCENARIO";
    private const string ChildOutputVariable = "GITEXT_NBUG_CHILD_OUTPUT";
    private const string ChildCategory = "P8.6h.2.Child";

    private static readonly string[] Scenarios =
    [
        "error-report",
        "error-ignore",
        "error-terminating-report",
        "failed-assembly-restart",
        "failed-assembly-report",
        "dubious-ownership-trust",
        "dubious-ownership-help",
        "dubious-ownership-open",
        "dubious-ownership-close",
    ];

    [Test]
    [Category("P8.6h.2")]
    public async Task Decision_routes_should_complete_in_isolated_child_processes()
    {
        string temporaryDirectory = Path.Combine(
            Path.GetTempPath(),
            $"GitExtensions.Avalonia.NBugReports-{Environment.ProcessId}-{Guid.NewGuid():N}");
        Directory.CreateDirectory(temporaryDirectory);

        try
        {
            foreach (string scenario in Scenarios)
            {
                ChildResult result = await RunChildScenarioAsync(scenario, temporaryDirectory);
                result.Scenario.Should().Be(scenario);
                if (scenario == "error-terminating-report")
                {
                    result.ExitCode.Should().NotBe(0);
                    result.Observation.ReportCount.Should().Be(1);
                    result.Observation.ReportCanIgnore.Should().BeFalse();
                    result.Observation.TerminationArmed.Should().BeTrue();
                }
                else
                {
                    result.ExitCode.Should().Be(0, result.StandardError);
                }

                AssertObservation(result.Observation);
            }
        }
        finally
        {
            Directory.Delete(temporaryDirectory, recursive: true);
        }
    }

    [Test]
    [Category(ChildCategory)]
    public void Child_process_should_execute_the_selected_decision_route()
    {
        string? scenario = Environment.GetEnvironmentVariable(ChildScenarioVariable);
        if (string.IsNullOrWhiteSpace(scenario))
        {
            Assert.Ignore("The P8.6h.2 parent probe supplies the isolated child scenario.");
        }

        string outputPath = Environment.GetEnvironmentVariable(ChildOutputVariable)
            ?? throw new InvalidOperationException($"{ChildOutputVariable} was not supplied.");
        ProbeObservation observation = ExecuteScenario(scenario, outputPath);
        WriteObservation(outputPath, observation);
    }

    private static ProbeObservation ExecuteScenario(string scenario, string outputPath)
    {
        UIReporter.TestAccessor.BugReportLauncher = new RecordingBugReportLauncher();
        UIReporter.TestAccessor.EnvironmentInformationProvider = () => "isolated environment";
        UIReporter.TestAccessor.RestartScheduler = action => action();
        UIReporter.TestAccessor.RestartApplication = () => CurrentObservation.RestartCount++;
        UIReporter.TestAccessor.ExitApplication = exitCode =>
        {
            WriteObservation(outputPath, CurrentObservation);
            Environment.Exit(exitCode);
        };
        UIReporter.TestAccessor.RepositoryPresenter = (_, workingDirectory) =>
        {
            CurrentObservation.RepositoryCount++;
            CurrentObservation.RepositoryWorkingDirectory = workingDirectory;
        };
        WinFormsShims.ShimHost.OsShell = new RecordingOsShell();
        BugReportInvoker.ExecutorProvider = CreateGitExecutorProvider();
        CurrentObservation = new ProbeObservation { Scenario = scenario };

        UIReporter reporter = new();
        UIReporter.TestAccessor.TaskDialogPresenter = (_, page) =>
        {
            TaskDialogButton button = SelectButton(scenario, page);
            if (scenario == "error-terminating-report")
            {
                CurrentObservation.TerminationArmed = true;
            }

            button.PerformClick();
            return button;
        };

        switch (scenario)
        {
            case "error-report":
            case "error-ignore":
                reporter.ReportError(
                    new InvalidOperationException("non-terminating failure"),
                    "non-terminating failure",
                    new StringBuilder("details"),
                    new OperationInfo());
                break;

            case "error-terminating-report":
                reporter.ReportError(
                    new InvalidOperationException("terminating failure"),
                    "terminating failure",
                    new StringBuilder("details"),
                    new OperationInfo { IsTerminating = true });
                break;

            case "failed-assembly-restart":
            case "failed-assembly-report":
                reporter.ReportFailedToLoadAnAssembly(
                    new FileNotFoundException("Could not load file or assembly Custom.Plugin", "Custom.Plugin"),
                    isTerminating: false).Should().BeTrue();
                break;

            case "dubious-ownership-trust":
            case "dubious-ownership-help":
            case "dubious-ownership-open":
            case "dubious-ownership-close":
                reporter.ReportDubiousOwnership(CreateDubiousOwnershipException());
                break;

            default:
                throw new InvalidOperationException($"Unknown child scenario '{scenario}'.");
        }

        return CurrentObservation;
    }

    private static ProbeObservation CurrentObservation { get; set; } = null!;

    private static TaskDialogButton SelectButton(string scenario, TaskDialogPage page)
        => scenario switch
        {
            "error-report" or "error-terminating-report" => page.Buttons[0],
            "error-ignore" => page.Buttons[1],
            "failed-assembly-restart" => page.Buttons[0],
            "failed-assembly-report" => page.Buttons[1],
            "dubious-ownership-open" => page.Buttons[0],
            "dubious-ownership-trust" => page.Buttons[1],
            "dubious-ownership-help" => page.Buttons[2],
            "dubious-ownership-close" => page.Buttons[3],
            _ => throw new InvalidOperationException($"Unknown child scenario '{scenario}'."),
        };

    private static ExternalOperationException CreateDubiousOwnershipException()
    {
        const string workingDirectory = "/tmp/parity-repository";
        const string message = "fatal: detected dubious ownership in repository at '/tmp/parity-repository'\n"
            + "To add an exception for this directory, call:\n\n"
            + "\tgit config --global --add safe.directory '/tmp/parity-repository'\n";
        return new ExternalOperationException(
            command: "git",
            workingDirectory: workingDirectory,
            innerException: new Exception(message));
    }

    private static IGitExecutorProvider CreateGitExecutorProvider()
    {
        IProcess process = Substitute.For<IProcess>();
        process.WaitForExit().Returns(0);
        IExecutable executable = Substitute.For<IExecutable>();
        executable.Start(
            Arg.Any<ArgumentString>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<Encoding?>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                CurrentObservation.GitCount++;
                CurrentObservation.GitArguments = callInfo.ArgAt<ArgumentString>(0).ToString();
                return process;
            });
        IGitExecutor executor = Substitute.For<IGitExecutor>();
        executor.GitExecutable.Returns(executable);
        IGitExecutorProvider provider = Substitute.For<IGitExecutorProvider>();
        provider.GetExecutor(Arg.Any<string>()).Returns(executor);
        return provider;
    }

    private static async Task<ChildResult> RunChildScenarioAsync(string scenario, string temporaryDirectory)
    {
        string repositoryRoot = FindRepositoryRoot();
        string outputPath = Path.Combine(temporaryDirectory, $"{scenario}.json");
        ProcessStartInfo startInfo = new("dotnet")
        {
            WorkingDirectory = repositoryRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        startInfo.ArgumentList.Add("test");
        startInfo.ArgumentList.Add("tests/app/UnitTests/GitUI.Avalonia.Tests/GitUI.Avalonia.Tests.csproj");
        startInfo.ArgumentList.Add("-p:BuildAvalonia=true");
        startInfo.ArgumentList.Add("--no-build");
        startInfo.ArgumentList.Add("--no-restore");
        startInfo.ArgumentList.Add("--filter");
        startInfo.ArgumentList.Add($"TestCategory={ChildCategory}");
        startInfo.Environment[ChildScenarioVariable] = scenario;
        startInfo.Environment[ChildOutputVariable] = outputPath;
        startInfo.Environment["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1";

        using Process child = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Could not start the isolated NBugReports child process.");
        using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(60));
        try
        {
            await child.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException)
        {
            child.Kill(entireProcessTree: true);
            throw new TimeoutException($"The isolated NBugReports scenario '{scenario}' did not exit.");
        }

        string standardOutput = await child.StandardOutput.ReadToEndAsync();
        string standardError = await child.StandardError.ReadToEndAsync();

        File.Exists(outputPath).Should().BeTrue($"scenario '{scenario}' should write its observation before returning or exiting");
        ProbeObservation observation = JsonSerializer.Deserialize<ProbeObservation>(File.ReadAllText(outputPath))
            ?? throw new InvalidOperationException($"Scenario '{scenario}' wrote an empty observation.");
        return new ChildResult(
            scenario,
            child.ExitCode,
            observation,
            standardOutput,
            standardError);
    }

    private static void WriteObservation(string outputPath, ProbeObservation observation)
    {
        File.WriteAllText(outputPath, JsonSerializer.Serialize(observation, new JsonSerializerOptions { WriteIndented = true }));
    }

    private static void AssertObservation(ProbeObservation observation)
    {
        switch (observation.Scenario)
        {
            case "error-report":
                observation.ReportCount.Should().Be(1);
                observation.ReportCanIgnore.Should().BeTrue();
                break;
            case "error-ignore":
                observation.ReportCount.Should().Be(0);
                break;
            case "failed-assembly-restart":
                observation.RestartCount.Should().Be(1);
                observation.ReportCount.Should().Be(0);
                break;
            case "failed-assembly-report":
                observation.RestartCount.Should().Be(0);
                observation.ReportCount.Should().Be(1);
                break;
            case "dubious-ownership-trust":
                observation.GitCount.Should().Be(1);
                observation.GitArguments.Should().Contain("safe.directory");
                observation.RepositoryCount.Should().Be(1);
                observation.RepositoryWorkingDirectory.Should().Be("/tmp/parity-repository");
                break;
            case "dubious-ownership-help":
                observation.ShellCount.Should().Be(1);
                observation.ShellKind.Should().Be(WinFormsShims.OsShellLaunchKind.OpenUri.ToString());
                break;
            case "dubious-ownership-open":
                observation.ShellCount.Should().Be(1);
                observation.ShellKind.Should().Be(WinFormsShims.OsShellLaunchKind.OpenDirectory.ToString());
                observation.ShellTarget.Should().Be(
                    OperatingSystem.IsWindows() ? @"\tmp\parity-repository" : "/tmp/parity-repository");
                break;
            case "dubious-ownership-close":
                observation.RepositoryCount.Should().Be(1);
                observation.RepositoryWorkingDirectory.Should().BeNull();
                break;
        }
    }

    private static string FindRepositoryRoot([System.Runtime.CompilerServices.CallerFilePath] string startPath = "")
    {
        DirectoryInfo? directory = new(Path.GetDirectoryName(startPath)!);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "GitExtensions.Avalonia.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new DirectoryNotFoundException("Could not locate the repository root.");
    }

    private sealed class RecordingBugReportLauncher : IBugReportLauncher
    {
        public WinFormsShims.DialogResult Show(
            Window? owner,
            Exception exception,
            string exceptionInfo,
            string environmentInfo,
            bool canIgnore,
            bool showIgnore,
            bool focusDetails)
        {
            CurrentObservation.ReportCount++;
            CurrentObservation.ReportCanIgnore = canIgnore;
            CurrentObservation.ReportShowIgnore = showIgnore;
            CurrentObservation.ReportFocusDetails = focusDetails;
            return WinFormsShims.DialogResult.Ignore;
        }
    }

    private sealed class RecordingOsShell : WinFormsShims.IOsShell
    {
        public bool TryLaunch(string target, WinFormsShims.OsShellLaunchKind kind)
        {
            CurrentObservation.ShellCount++;
            CurrentObservation.ShellTarget = target;
            CurrentObservation.ShellKind = kind.ToString();
            return true;
        }
    }

    private sealed class ProbeObservation
    {
        public required string Scenario { get; init; }
        public int ReportCount { get; set; }
        public bool ReportCanIgnore { get; set; }
        public bool ReportShowIgnore { get; set; }
        public bool ReportFocusDetails { get; set; }
        public bool TerminationArmed { get; set; }
        public int RestartCount { get; set; }
        public int RepositoryCount { get; set; }
        public string? RepositoryWorkingDirectory { get; set; }
        public int ShellCount { get; set; }
        public string? ShellTarget { get; set; }
        public string? ShellKind { get; set; }
        public int GitCount { get; set; }
        public string? GitArguments { get; set; }
    }

    private sealed record ChildResult(
        string Scenario,
        int ExitCode,
        ProbeObservation Observation,
        string StandardOutput,
        string StandardError);
}
