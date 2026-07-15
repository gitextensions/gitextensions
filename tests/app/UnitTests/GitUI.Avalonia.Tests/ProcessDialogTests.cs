using System.ComponentModel.Design;
using System.Diagnostics;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Threading;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI;
using GitUI.HelperDialogs;
using GitUI.UserControls;
using Microsoft.VisualStudio.Threading;

namespace GitExtensionsTests;

/// <summary>
///  Tests for the ported process dialogs: construction, and an end-to-end run of a real
///  git command through <see cref="FormProcess"/> with the plain text console emulation.
/// </summary>
[TestFixture]
public sealed class ProcessDialogTests
{
    private ServiceContainer _serviceContainer = null!;

    [SetUp]
    public void SetUp()
    {
        // ThreadHelper is static, while Avalonia's test context can change between cases.
        // Capture the current headless UI thread for every process-dialog test.
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();

        // Like GitExtensions.Avalonia's ServiceContainerRegistry, reduced to what the
        // process dialogs consume.
        _serviceContainer = new ServiceContainer();
        GitExtUtils.ServiceContainerRegistry.RegisterServices(_serviceContainer);
        System.IO.Abstractions.FileSystem fileSystem = new();
        GitCommands.Git.GitDirectoryResolver gitDirectoryResolver = new(fileSystem);
        _serviceContainer.AddService<System.IO.Abstractions.IFileSystem>(fileSystem);
        _serviceContainer.AddService<GitCommands.Git.IGitDirectoryResolver>(gitDirectoryResolver);
        GitCommands.ServiceContainerRegistry.RegisterServices(_serviceContainer);
        GitUI.ServiceContainerRegistry.RegisterServices(_serviceContainer);
    }

    [TearDown]
    public void TearDown()
    {
        _serviceContainer.Dispose();
    }

    private GitUICommands CreateCommands(string workingDir)
        => new(_serviceContainer, new GitModule(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), workingDir));

    [AvaloniaTest]
    public void PasswordInput_should_construct()
    {
        PasswordInput control = new();
        control.Password.Should().NotBeNull();
    }

    [AvaloniaTest]
    public void FormStatus_should_construct()
    {
        using FormStatus form = new(CreateCommands(Path.GetTempPath()), GitUI.ConsoleEmulation.PlainText.PlainTextConsoleEmulatorsRegistry.Instance, useDialogSettings: false);
        form.ErrorOccurred().Should().BeFalse();
        form.KeepDialogOpen.IsVisible.Should().BeFalse();
    }

    [AvaloniaTest]
    public void FormProcess_should_construct()
    {
        using FormProcess form = new(CreateCommands(Path.GetTempPath()), arguments: "version", Path.GetTempPath(), input: null, useDialogSettings: true);
        form.Title.Should().StartWith("Process");
        form.ProcessString.Should().NotBeNullOrEmpty();
    }

    [AvaloniaTest]
    public void FormProcess_Escape_should_close_through_the_shared_dialog_handler()
    {
        using FormProcess form = new(CreateCommands(Path.GetTempPath()), arguments: "version", Path.GetTempPath(), input: null, useDialogSettings: true);
        form.Show();

        form.KeyPress(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, keySymbol: null);

        form.IsVisible.Should().BeFalse();
    }

    [AvaloniaTest]
    public void FormProcess_Escape_should_terminate_a_running_linux_command()
    {
        if (!OperatingSystem.IsLinux())
        {
            Assert.Ignore("Linux process groups are required for this test.");
        }

        using FormProcess form = new(
            CreateCommands(Path.GetTempPath()),
            arguments: "-c \"sleep 100 & echo $!; wait\"",
            Path.GetTempPath(),
            input: null,
            useDialogSettings: true,
            process: "/bin/bash");
        try
        {
            form.Show();
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (string.IsNullOrWhiteSpace(form.GetOutputString()) && stopwatch.ElapsedMilliseconds < 5_000)
            {
                Dispatcher.UIThread.RunJobs();
                Thread.Sleep(25);
            }

            int childProcessId = int.Parse(form.GetOutputString().Trim());

            form.KeyPress(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, keySymbol: null);

            form.IsVisible.Should().BeFalse();
            SpinWait.SpinUntil(() => HasExited(childProcessId), TimeSpan.FromSeconds(5)).Should().BeTrue();
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public void FormProcess_should_run_a_git_command_and_report_success()
    {
        FormProcess form = new(CreateCommands(Path.GetTempPath()), arguments: "version", Path.GetTempPath(), input: null, useDialogSettings: false);
        try
        {
            form.Show();

            // The command runs in the background; pump the dispatcher until Done enables OK.
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (!form.Ok.IsEnabled && stopwatch.ElapsedMilliseconds < 30_000)
            {
                Dispatcher.UIThread.RunJobs();
                Thread.Sleep(25);
            }

            form.Ok.IsEnabled.Should().BeTrue("the git process should finish and enable OK");
            form.ErrorOccurred().Should().BeFalse();
            form.GetOutputString().Should().Contain("git version");
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public void FormProcess_should_report_an_error_for_a_failing_command()
    {
        FormProcess form = new(CreateCommands(Path.GetTempPath()), arguments: "definitely-not-a-git-command", Path.GetTempPath(), input: null, useDialogSettings: false);
        try
        {
            form.Show();

            Stopwatch stopwatch = Stopwatch.StartNew();
            while (!form.Ok.IsEnabled && stopwatch.ElapsedMilliseconds < 30_000)
            {
                Dispatcher.UIThread.RunJobs();
                Thread.Sleep(25);
            }

            form.Ok.IsEnabled.Should().BeTrue("the git process should finish and enable OK");
            form.ErrorOccurred().Should().BeTrue();
        }
        finally
        {
            form.Close();
        }
    }

    private static bool HasExited(int processId)
    {
        try
        {
            using Process process = Process.GetProcessById(processId);
            return process.HasExited;
        }
        catch (ArgumentException)
        {
            return true;
        }
    }
}
