using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.BuildServerIntegration;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using GitUI;
using GitUI.BuildServerIntegration;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.SettingsDialog;
using GitUI.CommandsDialogs.SettingsDialog.Pages;
using GitUI.Compat;
using GitUIPluginInterfaces.BuildServerIntegration;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensions.Compat;

// parity-scaffolding: proves the real settings route, launcher streams/cancellation and fixed-command timings in the test Flatpak.
internal static class HostGitConformanceProbe
{
    internal static void StartIfRequested(IClassicDesktopStyleApplicationLifetime desktop, GitUICommands commands)
    {
        string? report = Environment.GetEnvironmentVariable("GITEXTENSIONS_HOST_GIT_CONFORMANCE_REPORT");
        if (!FlatpakEnvironment.IsFlatpak() || string.IsNullOrWhiteSpace(report) || desktop.MainWindow is not { } main)
        {
            return;
        }

        Dispatcher.UIThread.Post(() => _ = RunAsync(desktop, commands, main, report), DispatcherPriority.Background);
    }

    private static async Task RunAsync(IClassicDesktopStyleApplicationLifetime desktop, GitUICommands commands, Window main, string reportPath)
    {
        Dictionary<string, object?> report = new() { ["confined"] = FlatpakEnvironment.IsFlatpak(), ["passed"] = false };
        List<string> warnings = [];
        bool fallback = Environment.GetEnvironmentVariable("GITEXTENSIONS_HOST_GIT_EXPECT_FALLBACK") == "1";
        using IDisposable observer = Window.WindowOpenedEvent.AddClassHandler<Window>((window, eventArgs) =>
        {
            if (window.GetType().Name == "FormBuildServerCredentials")
            {
                report["unexpectedCredentialPrompt"] = true;
                Dispatcher.UIThread.Post(() => window.Close(), DispatcherPriority.Background);
                return;
            }

            if (window.GetType() != typeof(Window) || !window.Classes.Contains("gitextensions-message-box"))
            {
                return;
            }

            Dispatcher.UIThread.Post(() =>
            {
                string text = string.Join("\n", window.GetVisualDescendants().OfType<TextBlock>().Select(block => block.Text));
                if (text.Contains("Host Git", StringComparison.Ordinal))
                {
                    warnings.Add(text);
                    Button? button = window.GetVisualDescendants().OfType<Button>().FirstOrDefault(button => button.IsDefault);
                    button?.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                }
            }, DispatcherPriority.Background);
        });
        try
        {
            VerifyCachedCredentialBackend(main, commands, reportPath, "bundled-before", expectHost: false, report);
            await ConfigureAsync(commands, main, enabled: true, configureEditor: true, report);
            Require(commands.Module.GitExecutable.Command == FlatpakHostGit.LauncherPath, "Closing Settings did not retain the production host launcher.");
            string selectedVersion = (await ExecuteAsync(commands, "--version")).StandardOutput.Trim();
            report["selectedVersion"] = selectedVersion;
            Require(selectedVersion == Environment.GetEnvironmentVariable("GITEXTENSIONS_HOST_GIT_EXPECTED_VERSION"), "The selected backend did not run the expected Git.");
            report["hostSelectionPersisted"] = true;
            VerifyCachedCredentialBackend(main, commands, reportPath, "selected", expectHost: !fallback, report);
            if (!fallback)
            {
                await VerifyIoAsync(commands, report);
                await VerifyCancellationAsync(commands, report);
                report["hostBuiltinEditor"] = await CommitWithBuiltinEditorAsync(commands, reportPath, "host");
            }

            report["selectedBackendStatus"] = (await ExecuteAsync(commands, "status --porcelain=v1")).StandardOutput;
            await ConfigureAsync(commands, main, enabled: false, configureEditor: false, report);
            Require(commands.Module.GitExecutable.Command == Path.Combine(AppContext.BaseDirectory, "git"), "Closing Settings did not restore bundled Git.");
            report["bundledVersion"] = (await ExecuteAsync(commands, "--version")).StandardOutput.Trim();
            VerifyCachedCredentialBackend(main, commands, reportPath, "bundled-after", expectHost: false, report);
            if (!fallback)
            {
                report["bundledBuiltinEditor"] = await CommitWithBuiltinEditorAsync(commands, reportPath, "bundled");
                await ConfigureAsync(commands, main, enabled: true, configureEditor: false, report);
                List<BenchmarkMeasurement> host = await MeasureAsync(commands);
                await ConfigureAsync(commands, main, enabled: false, configureEditor: false, report);
                List<BenchmarkMeasurement> bundled = await MeasureAsync(commands);
                Require(host.Select(measurement => measurement.Output).SequenceEqual(bundled.Select(measurement => measurement.Output)),
                    "The two Git backends did not return the same fixed-fixture outputs.");
                report["hostTimings"] = host;
                report["bundledTimings"] = bundled;
                report["backendOutputsEqual"] = true;
            }

            Require(!fallback || warnings.Count > 0, "Permission fallback did not show its translated warning in the real settings flow.");
            report["warnings"] = warnings;
            report["passed"] = true;
        }
        catch (Exception exception)
        {
            report["error"] = exception.ToString();
            report["warnings"] = warnings;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(reportPath))!);
        await File.WriteAllTextAsync(reportPath, JsonSerializer.Serialize(report,
            new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase }) + "\n");
        desktop.Shutdown(report["passed"] is true ? 0 : 1);
    }

    private static async Task ConfigureAsync(GitUICommands commands, Window owner, bool enabled, bool configureEditor, Dictionary<string, object?> report)
    {
        TaskCompletionSource completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        bool opened = false;
        using IDisposable observer = Window.WindowOpenedEvent.AddClassHandler<Window>((window, eventArgs) =>
        {
            if (window is FormSettings settings && !opened)
            {
                opened = true;
                Dispatcher.UIThread.Post(() => _ = ConfigureDialogAsync(settings, enabled, configureEditor, report, completion), DispatcherPriority.Background);
            }
        });
        bool saved = commands.StartSettingsDialog(owner as WinFormsShims.IWin32Window, GitSettingsPage.GetPageReference());
        Require(opened, "The production settings command did not open its dialog.");
        await completion.Task;
        Require(saved, "The production settings command did not report a successful save.");
    }

    private static async Task ConfigureDialogAsync(FormSettings settings, bool enabled, bool configureEditor,
        Dictionary<string, object?> report, TaskCompletionSource completion)
    {
        try
        {
            await WaitUntilAsync(() => settings.GetVisualDescendants().OfType<GitSettingsPage>().Any(page => page.FindControl<TextBox>("GitPath")?.Text is not null));
            if (configureEditor)
            {
                settings.GotoPage(GitConfigSettingsPage.GetPageReference());
                await WaitUntilAsync(() => settings.GetVisualDescendants().OfType<GitConfigSettingsPage>().Any());
                SettingsPageHeader header = settings.GetVisualDescendants().OfType<SettingsPageHeader>().Single();
                header.FindControl<RadioButton>("LocalRB")!.IsChecked = true;
                GitConfigSettingsPage config = settings.GetVisualDescendants().OfType<GitConfigSettingsPage>().Single();
                ComboBox editor = config.FindControl<ComboBox>("GlobalEditor")!;
                await WaitUntilAsync(() => editor.IsEffectivelyEnabled && editor.ItemCount > 0);
                editor.SelectedIndex = 0;
                string command = editor.Text ?? throw new InvalidOperationException("The original editor selector did not supply its command.");
                Require(command.Contains("fileeditor", StringComparison.Ordinal) && !command.Contains("/app/", StringComparison.Ordinal)
                    && !command.Contains("flatpak-spawn", StringComparison.Ordinal), "The selected editor command is not host-valid.");
                report["editorCommand"] = command;
            }

            settings.GotoPage(GitSettingsPage.GetPageReference());
            await WaitUntilAsync(() => settings.GetVisualDescendants().OfType<GitSettingsPage>().Any());
            GitSettingsPage page = settings.GetVisualDescendants().OfType<GitSettingsPage>().Single();
            CheckBox option = page.GetLogicalDescendants().OfType<CheckBox>()
                .SingleOrDefault(control => control.Name == "_NO_TRANSLATE_chkUseHostGit")
                ?? throw new InvalidOperationException("The real Paths page did not provide the host-Git option.");
            Require(option.IsEffectivelyVisible, "The host-Git option is not visible on its actual page.");
            report["optionCaption"] = option.Content?.ToString();
            if (!option.IsEffectivelyEnabled)
            {
                settings.GetVisualDescendants().OfType<SettingsPageHeader>().Single().FindControl<RadioButton>("GlobalRB")!.IsChecked = true;
            }

            Require(option.IsEffectivelyEnabled, "The host-Git option is not actionable.");
            option.IsChecked = enabled;
            settings.FindControl<Button>("buttonOk")!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Require(!settings.IsVisible, "The production OK action did not save and close Settings.");
            completion.TrySetResult();
        }
        catch (Exception exception)
        {
            completion.TrySetException(exception);
            settings.Close();
        }
    }

    private sealed record BenchmarkMeasurement(string Name, string Arguments, List<double> Samples, double MeanMs, double MinMs,
        double MaxMs, string Output, string OutputSha256);

    private static void VerifyCachedCredentialBackend(Window main, GitUICommands commands, string reportPath, string phase,
        bool expectHost, Dictionary<string, object?> report)
    {
        RevisionGridControl grid = main.GetLogicalDescendants().OfType<RevisionGridControl>().Single();
        FieldInfo field = typeof(RevisionGridControl).GetField("_buildServerWatcher", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("The revision grid did not retain its production watcher.");
        BuildServerWatcher watcher = (BuildServerWatcher)field.GetValue(grid)!;
        string evidence = Path.Combine(commands.Module.WorkingDir, "credential-helper.json");
        File.Delete(evidence);
        using OwnedBuildServer adapter = new();
        IBuildServerCredentials? credential = watcher.GetBuildServerCredentials(adapter, useStoredCredentialsIfExisting: true);
        Require(credential?.Username == "Owned host Git", "The real watcher did not read its owned credential helper.");
        using JsonDocument proof = JsonDocument.Parse(File.ReadAllText(evidence));
        string? hostPid = proof.RootElement.GetProperty("hostPid").GetString();
        bool confined = proof.RootElement.GetProperty("confined").GetBoolean();
        Require(!string.IsNullOrEmpty(hostPid) == expectHost && confined != expectHost,
            "The shared credential process did not follow the selected Git backend through its cached executable.");
        Require(proof.RootElement.GetProperty("gcmInteractive").GetString() == "Never",
            "The host launcher lost the production credential prompt suppression.");
        File.Copy(evidence, Path.Combine(Path.GetDirectoryName(reportPath)!, "credential-" + phase + ".json"), overwrite: true);
        report["credential-" + phase] = new { host = expectHost, hostPid, confined };
    }

    private sealed class OwnedBuildServer : IBuildServerAdapter
    {
        public string UniqueKey => "HostGitConformance";

        public void Initialize(IBuildServerWatcher buildServerWatcher, SettingsSource config, Action openSettings,
            Func<ObjectId, bool>? isCommitInRevisionGrid = null)
            => throw new NotSupportedException("Only external server identity is used by this credential probe.");

        public IObservable<BuildInfo> GetFinishedBuildsSince(IScheduler scheduler, DateTime? sinceDate = null)
            => throw new NotSupportedException();

        public IObservable<BuildInfo> GetRunningBuilds(IScheduler scheduler)
            => throw new NotSupportedException();

        public void Dispose()
        {
        }
    }

    private static async Task<List<BenchmarkMeasurement>> MeasureAsync(GitUICommands commands)
    {
        List<BenchmarkMeasurement> measurements = [];
        foreach ((string name, string arguments) in new[]
        {
            ("status", "status --porcelain=v1"),
            ("log", "log -20 --format=%H%x00%s"),
            ("refs", "for-each-ref --format=%(refname)%00%(objectname) refs/heads refs/tags"),
        })
        {
            await ExecuteAsync(commands, arguments);
            List<double> times = [];
            string? output = null;
            for (int index = 0; index < 5; index++)
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                ExecutionResult result = await ExecuteAsync(commands, arguments);
                stopwatch.Stop();
                Require(output is null || output == result.StandardOutput, "The fixed benchmark command changed its output between samples.");
                output = result.StandardOutput;
                times.Add(stopwatch.Elapsed.TotalMilliseconds);
            }

            measurements.Add(new(name, arguments, times, times.Average(), times.Min(), times.Max(), output!,
                Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(output!)))));
        }

        return measurements;
    }

    private static async Task VerifyIoAsync(GitUICommands commands, Dictionary<string, object?> report)
    {
        ProcessStartInfo start = new(commands.Module.GitExecutable.Command)
        {
            WorkingDirectory = commands.Module.WorkingDir,
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        start.ArgumentList.Add("host-io");
        start.ArgumentList.Add("host argument");
        start.ArgumentList.Add("$(literal)");
        using Process process = Process.Start(start)!;
        Task<string> output = process.StandardOutput.ReadToEndAsync();
        Task<string> error = process.StandardError.ReadToEndAsync();
        await process.StandardInput.WriteAsync("literal stdin $(untouched)\n");
        process.StandardInput.Close();
        using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(20));
        await process.WaitForExitAsync(timeout.Token);
        string stdout = await output;
        string stderr = await error;
        Require(process.ExitCode == 23 && stdout.Contains("literal stdin $(untouched)", StringComparison.Ordinal)
            && stdout.Contains("$(literal)", StringComparison.Ordinal)
            && stdout.Contains("cwd=" + Path.TrimEndingDirectorySeparator(commands.Module.WorkingDir) + "\n", StringComparison.Ordinal)
            && stdout.Contains("term=" + Environment.GetEnvironmentVariable("TERM") + "\n", StringComparison.Ordinal)
            && stdout.Contains("askpassRequire=" + Environment.GetEnvironmentVariable("SSH_ASKPASS_REQUIRE") + "\n", StringComparison.Ordinal)
            && stderr == "host-io-error\n", "Host Git did not preserve its streams, literal arguments and exit code.");
        report["io"] = new { exitCode = process.ExitCode, stdout, stderr };
    }

    private static async Task VerifyCancellationAsync(GitUICommands commands, Dictionary<string, object?> report)
    {
        string marker = Path.Combine(commands.Module.WorkingDir, "host-cancellation.json");
        File.Delete(marker);
        using CancellationTokenSource cancellation = new();
        Task<ExecutionResult> running = commands.Module.GitExecutable.ExecuteAsync("host-wait", throwOnErrorExit: false, cancellationToken: cancellation.Token);
        bool observed = false;
        try
        {
            await WaitUntilAsync(() => File.Exists(marker));
            using JsonDocument pids = JsonDocument.Parse(await File.ReadAllTextAsync(marker));
            int[] identifiers = pids.RootElement.EnumerateObject().Select(property => property.Value.GetInt32()).ToArray();
            await cancellation.CancelAsync();
            try
            {
                await running;
            }
            catch (OperationCanceledException)
            {
                observed = true;
            }

            bool dead = false;
            for (int attempt = 0; attempt < 40 && !dead; attempt++)
            {
                ProcessStartInfo check = new("/usr/bin/flatpak-spawn");
                check.ArgumentList.Add("--host");
                check.ArgumentList.Add("/bin/sh");
                check.ArgumentList.Add("-c");
                check.ArgumentList.Add("for pid do state=$(ps -o stat= -p \"$pid\"); case \"$state\" in ''|Z*|X*) ;; *) exit 1;; esac; done");
                check.ArgumentList.Add("gitextensions-host-cancellation-check");
                foreach (int identifier in identifiers)
                {
                    check.ArgumentList.Add(identifier.ToString(System.Globalization.CultureInfo.InvariantCulture));
                }

                check.RedirectStandardOutput = true;
                check.RedirectStandardError = true;
                using Process process = Process.Start(check)!;
                await process.WaitForExitAsync();
                dead = process.ExitCode == 0;
                if (!dead)
                {
                    await Task.Delay(100);
                }
            }

            Require(observed && dead, "Cancelling the production Git executable left a host Git/hook/child alive.");
            report["cancellation"] = new { observed, hostProcessesTerminated = dead, identifiers };
        }
        finally
        {
            await cancellation.CancelAsync();
            try
            {
                await running;
            }
            catch (OperationCanceledException)
            {
            }
        }
    }

    private static async Task<string> CommitWithBuiltinEditorAsync(GitUICommands commands, string report, string backend)
    {
        string editorReport = Path.Combine(Path.GetDirectoryName(report)!, "fileeditor.json");
        File.Delete(editorReport);
        await ExecuteAsync(commands, "commit --allow-empty");
        using JsonDocument proof = JsonDocument.Parse(await File.ReadAllTextAsync(editorReport));
        Require(proof.RootElement.GetProperty("passed").GetBoolean(), "The built-in editor did not save Git's actual file.");
        string subject = (await ExecuteAsync(commands, "log -1 --format=%s")).StandardOutput.Trim();
        File.Copy(editorReport, Path.Combine(Path.GetDirectoryName(report)!, "fileeditor-" + backend + ".json"), overwrite: true);
        Require(subject == Environment.GetEnvironmentVariable("GITEXTENSIONS_FILEEDITOR_CONFORMANCE_TEXT")?.Trim(), "Git did not use the message saved by its actual editor.");
        return subject;
    }

    private static async Task<ExecutionResult> ExecuteAsync(GitUICommands commands, string arguments)
    {
        ExecutionResult result = await commands.Module.GitExecutable.ExecuteAsync(arguments, throwOnErrorExit: false);
        Require(result.ExitedSuccessfully, "Git failed: " + arguments + "\n" + result.AllOutput);
        return result;
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        DateTime deadline = DateTime.UtcNow.AddSeconds(20);
        while (!condition() && DateTime.UtcNow < deadline)
        {
            await Task.Delay(50);
        }

        Require(condition(), "Timed out waiting for the production UI/process state.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
