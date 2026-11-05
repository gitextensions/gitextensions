using System.ComponentModel.Design;
using System.Diagnostics;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI;
using GitUI.ConsoleEmulation;
using GitUI.HelperDialogs;
using GitUI.UserControls;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

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
    public void FormStatus_should_show_waiting_success_and_error_window_icons()
    {
        using FormStatus form = new(CreateCommands(Path.GetTempPath()), GitUI.ConsoleEmulation.PlainText.PlainTextConsoleEmulatorsRegistry.Instance, useDialogSettings: false);
        WindowIcon waiting = form.Icon ?? throw new InvalidOperationException("FormStatus has no waiting icon.");

        InvokeFormStatusMethod(form, "Done", false);
        WindowIcon error = form.Icon ?? throw new InvalidOperationException("FormStatus has no error icon.");
        error.Should().NotBeSameAs(waiting);

        InvokeFormStatusMethod(form, "Reset");
        form.Icon.Should().BeSameAs(waiting);

        InvokeFormStatusMethod(form, "Done", true);
        WindowIcon success = form.Icon ?? throw new InvalidOperationException("FormStatus has no success icon.");
        success.Should().NotBeSameAs(waiting).And.NotBeSameAs(error);
    }

    [AvaloniaTest]
    public void FormStatus_should_construct_with_the_designer_constructor()
    {
        // The XAML loader needs this constructor. It builds the dialog exactly like the
        // run-time one does — console included — so the designer shows the real thing.
        using FormStatus form = new();

        form.pnlOutput.Children.Should().NotBeEmpty("the console control belongs to the dialog");
        form.KeepDialogOpen.IsVisible.Should().BeFalse();

        // Only the commands are absent, and asking for them says so.
        form.Invoking(f => _ = f.UICommands).Should().Throw<InvalidOperationException>()
            .WithMessage("*constructed incorrectly*");
    }

    [AvaloniaTest]
    public void FormStatus_should_be_translated_by_the_designer_constructor()
    {
        using TranslationTestScope translationScope = new();
        string original = AppSettings.CurrentTranslation ?? "";
        try
        {
            AppSettings.CurrentTranslation = "German";
            using FormStatus form = new();

            form.Abort.Content.Should().Be("_Abbrechen", "InitializeComplete must translate the dialog the loader built");
        }
        finally
        {
            AppSettings.CurrentTranslation = original;
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void FormStatus_should_refuse_title_bar_close_without_disposing_the_command_runner(bool completed)
    {
        IConsoleCommandRunner runner = Substitute.For<IConsoleCommandRunner, IDisposable>();
        runner.Control.Returns(new UserControl());
        IConsoleEmulatorsRegistry registry = Substitute.For<IConsoleEmulatorsRegistry>();
        registry.CreateCommandController().Returns(runner);
        using FormStatus form = new(CreateCommands(Path.GetTempPath()), registry, useDialogSettings: false);
        int aborts = 0;
        SetFormStatusCallback(form, "ProcessCallback", _ => { });
        SetFormStatusCallback(form, "AbortCallback", _ => aborts++);
        try
        {
            form.Show();
            Dispatcher.UIThread.RunJobs();
            runner.Received(1).ResetConsole();
            if (completed)
            {
                InvokeFormStatusMethod(form, "Done", true);
            }

            bool cancelled = RequestNativeClose(form, WindowCloseReason.WindowClosing);

            cancelled.Should().BeTrue("the original disables the control-box close command");
            form.IsVisible.Should().BeTrue();
            aborts.Should().Be(0);
            ((IDisposable)runner).DidNotReceive().Dispose();
            runner.DidNotReceive().KillCommandProcess();
            form.GetOutputString().Should().NotContain("Aborted");

            form.Close();

            form.IsVisible.Should().BeFalse("programmatic completion must still be able to close the dialog");
            ((IDisposable)runner).Received(1).Dispose();
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public void FormStatus_should_keep_the_explicit_abort_callback_and_result()
    {
        IConsoleCommandRunner runner = Substitute.For<IConsoleCommandRunner, IDisposable>();
        runner.Control.Returns(new UserControl());
        IConsoleEmulatorsRegistry registry = Substitute.For<IConsoleEmulatorsRegistry>();
        registry.CreateCommandController().Returns(runner);
        using FormStatus form = new(CreateCommands(Path.GetTempPath()), registry, useDialogSettings: false);
        int aborts = 0;
        SetFormStatusCallback(form, "ProcessCallback", _ => { });
        SetFormStatusCallback(form, "AbortCallback", _ => aborts++);
        try
        {
            form.Show();
            Dispatcher.UIThread.RunJobs();
            form.Abort.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            aborts.Should().Be(1);
            form.ErrorOccurred().Should().BeTrue();
            form.GetOutputString().Should().Contain("Aborted");
            form.DialogResult.Should().Be(GitExtensions.Shims.WinForms.DialogResult.Abort);
            form.Ok.IsEnabled.Should().BeTrue();
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(WindowCloseReason.OwnerWindowClosing)]
    [TestCase(WindowCloseReason.ApplicationShutdown)]
    [TestCase(WindowCloseReason.OSShutdown)]
    public void FormStatus_should_allow_owner_and_application_shutdown(WindowCloseReason reason)
    {
        IConsoleCommandRunner runner = Substitute.For<IConsoleCommandRunner, IDisposable>();
        runner.Control.Returns(new UserControl());
        IConsoleEmulatorsRegistry registry = Substitute.For<IConsoleEmulatorsRegistry>();
        registry.CreateCommandController().Returns(runner);
        using FormStatus form = new(CreateCommands(Path.GetTempPath()), registry, useDialogSettings: false);
        SetFormStatusCallback(form, "ProcessCallback", _ => { });
        try
        {
            form.Show();
            Dispatcher.UIThread.RunJobs();

            RequestNativeClose(form, reason).Should().BeFalse();
            form.Close();

            form.IsVisible.Should().BeFalse();
            ((IDisposable)runner).Received(1).Dispose();
        }
        finally
        {
            form.Close();
        }
    }

    private static bool RequestNativeClose(Window form, WindowCloseReason reason)
    {
        PropertyInfo closing = typeof(Avalonia.Platform.IWindowImpl).GetProperty("Closing", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("The platform close callback could not be found.");
        Func<WindowCloseReason, bool> requestClose = (Func<WindowCloseReason, bool>?)closing.GetValue(form.PlatformImpl)
            ?? throw new InvalidOperationException("The window has no platform close callback.");
        return requestClose(reason);
    }

    private static void SetFormStatusCallback(FormStatus form, string name, Action<FormStatus> callback)
    {
        FieldInfo field = typeof(FormStatus).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"FormStatus.{name} could not be found.");
        field.SetValue(form, callback);
    }

    private static void InvokeFormStatusMethod(FormStatus form, string name, params object[] arguments)
    {
        MethodInfo method = typeof(FormStatus).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"FormStatus.{name} could not be found.");
        method.Invoke(form, arguments);
    }

    [AvaloniaTest]
    public void FormProcess_should_construct()
    {
        using FormProcess form = new(CreateCommands(Path.GetTempPath()), arguments: "version", Path.GetTempPath(), input: null, useDialogSettings: true);
        form.Title.Should().StartWith("Process");
        form.ProcessString.Should().NotBeNullOrEmpty();
    }

    [AvaloniaTest]
    public void FormProcess_should_pass_WSL_git_globs_directly_without_a_login_shell_reparse()
    {
        string settingsPath = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.WslArguments-{Guid.NewGuid():N}.settings");
        DistributedSettings settings = new(lowerPriority: null, new GitExtSettingsCache(settingsPath, autoSave: false), SettingLevel.Unknown);
        try
        {
            AppSettings.UsingContainer(settings, () =>
            {
                AppSettings.SetBool(nameof(AppSettings.WslGitEnabled), true);
                AppSettings.SetString(nameof(AppSettings.WslCommand), "wsl");
                AppSettings.SetString(nameof(AppSettings.WslGitCommand), "git");
                const string directory = @"\\wsl$\Ubuntu\home\example repo\";
                const string arguments = "log --exclude=refs/sessions/** --exclude=refs/agents/** --all";
                using FormProcess form = new(CreateCommands(Path.GetTempPath()), arguments, directory, input: null, useDialogSettings: true);

                form.ProcessString.Should().Be("wsl");
                form.ProcessArguments.Should().Be($"-d Ubuntu --cd {directory.RemoveTrailingPathSeparator().Quote()} --exec git {arguments}");
                File.Exists(settingsPath).Should().BeFalse("the argument-only fixture must not persist even its isolated settings");
            });
        }
        finally
        {
            File.Delete(settingsPath);
        }
    }

    [AvaloniaTest]
    [TestCase("version", true)]
    [TestCase("definitely-not-a-git-command", false)]
    public void FormProcess_ShowDialog_should_return_the_completed_command_output_without_changing_success(string arguments, bool expectedSuccess)
    {
        using GitExtensionsForm owner = new();
        FormProcess? openedProcess = null;
        DispatcherTimer timer = new() { Interval = TimeSpan.FromMilliseconds(20) };
        Stopwatch elapsed = Stopwatch.StartNew();
        using IDisposable subscription = Window.WindowOpenedEvent.AddClassHandler<Window>((window, _) =>
        {
            if (window is FormProcess process)
            {
                openedProcess = process;
                timer.Start();
            }
        });
        timer.Tick += (_, _) =>
        {
            if (openedProcess is { } process && (process.Ok.IsEnabled || elapsed.Elapsed > TimeSpan.FromSeconds(30)))
            {
                timer.Stop();
                process.Close();
            }
        };
        owner.Show();
        try
        {
            bool success = FormProcess.ShowDialog(owner, CreateCommands(Path.GetTempPath()), arguments, Path.GetTempPath(), input: null, useDialogSettings: false, out string output);

            success.Should().Be(expectedSuccess);
            openedProcess.Should().NotBeNull();
            openedProcess!.Ok.IsEnabled.Should().BeTrue("the process must complete rather than time out");
            output.Should().Be(openedProcess.GetOutputString()).And.NotBeNullOrWhiteSpace();
            if (expectedSuccess)
            {
                output.Should().Contain("git version");
            }
            else
            {
                output.Should().Contain("definitely-not-a-git-command");
            }
        }
        finally
        {
            timer.Stop();
            openedProcess?.Close();
            owner.Close();
        }
    }

    [AvaloniaTest]
    public void FormRemoteProcess_should_construct_with_the_existing_translation_boundary()
    {
        using FormRemoteProcess form = new(CreateCommands(Path.GetTempPath()), arguments: "version", useDialogSettings: false);
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);

        form.Should().BeAssignableTo<FormProcess>();
        translation.Received(1).AddTranslationItem(nameof(FormRemoteProcess), "$this", "Text", Arg.Any<string>());
        translation.Received(1).AddTranslationItem(nameof(FormRemoteProcess), "Abort", "Text", "&Abort");
        translation.DidNotReceive().AddTranslationItem(
            nameof(FormRemoteProcess),
            "_fingerprintNotRegistredText",
            "Text",
            Arg.Any<string>());
    }

    [AvaloniaTest]
    public void FormRemoteProcess_should_allow_a_consumer_to_retry_a_failed_remote_command()
    {
        using FormRemoteProcess form = new(
            CreateCommands(Path.GetTempPath()),
            arguments: "definitely-not-a-git-command",
            useDialogSettings: false);
        int exitCallbacks = 0;
        form.HandleOnExitCallback = (ref bool isError, FormProcess process) =>
        {
            exitCallbacks++;
            if (isError && exitCallbacks == 1)
            {
                process.ProcessArguments = "version";
                process.Retry();
                return true;
            }

            return false;
        };

        form.Show();
        try
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (!form.Ok.IsEnabled && stopwatch.ElapsedMilliseconds < 30_000)
            {
                Dispatcher.UIThread.RunJobs();
                Thread.Sleep(25);
            }

            form.Ok.IsEnabled.Should().BeTrue("the retried git command should finish");
            exitCallbacks.Should().Be(2);
            form.ErrorOccurred().Should().BeFalse();
            form.GetOutputString().Should().Contain("git version");
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public void FormProcess_Escape_should_close_through_the_shared_dialog_handler()
    {
        using FormProcess form = new(
            CreateCommands(Path.GetTempPath()),
            arguments: "version",
            Path.GetTempPath(),
            input: null,
            useDialogSettings: false);
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
            SpinWait.SpinUntil(
                () => ProcessTestHelper.HasExited(childProcessId),
                TimeSpan.FromSeconds(5)).Should().BeTrue();
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
}
