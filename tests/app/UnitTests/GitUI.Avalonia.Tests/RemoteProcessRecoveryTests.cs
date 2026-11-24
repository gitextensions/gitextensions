using System.ComponentModel.Design;
using System.Diagnostics;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Git;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.ConsoleEmulation;
using GitUI.ConsoleEmulation.PlainText;
using GitUI.HelperDialogs;
using GitUI.Infrastructure;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class RemoteProcessRecoveryTests
{
    private ServiceContainer _services = null!;
    private string _directory = null!;
    private string? _globalConfig;
    private string _sshPath = null!;
    private bool _closeProcess;
    private bool _confirmBranch;
    private bool _confirmTracking;
    private WinFormsShims.IMessageBoxHost? _messageBoxes;
    private Func<string, bool> _connectHost = null!;
    private Func<string, IExecutable> _createPageant = null!;
    private string _pageantPath = null!;
    private readonly List<FailureOnceRunner> _runners = [];
    private FormRemoteProcess? _process;
    private string? _remote;
    private bool _timedOut;

    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        _directory = Path.Combine(Path.GetTempPath(), $"GitExtensions.RemoteRecovery-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_directory);
        _globalConfig = Environment.GetEnvironmentVariable("GIT_CONFIG_GLOBAL");
        string config = Path.Combine(_directory, "global.config");
        File.WriteAllText(config, string.Empty);
        Environment.SetEnvironmentVariable("GIT_CONFIG_GLOBAL", config);
        _sshPath = AppSettings.SshPath;
        _closeProcess = AppSettings.CloseProcessDialog;
        _confirmBranch = AppSettings.DontConfirmPushNewBranch.Value;
        _confirmTracking = AppSettings.DontConfirmAddTrackingRef.Value;
        try
        {
            _messageBoxes = WinFormsShims.ShimHost.MessageBoxHost;
        }
        catch (InvalidOperationException)
        {
            _messageBoxes = null;
        }

        _connectHost = FormRemoteProcess.TestAccessor.ConnectHost;
        _createPageant = PuttyHelpers.TestAccessor.CreatePageantExecutable;
        _pageantPath = AppSettings.Pageant;
        AppSettings.SshPath = "plink.exe";
        AppSettings.CloseProcessDialog = true;
        AppSettings.DontConfirmPushNewBranch.Value = true;
        AppSettings.DontConfirmAddTrackingRef.Value = true;
        _services = new ServiceContainer();
        GitExtUtils.ServiceContainerRegistry.RegisterServices(_services);
        System.IO.Abstractions.FileSystem fileSystem = new();
        GitDirectoryResolver resolver = new(fileSystem);
        _services.AddService<System.IO.Abstractions.IFileSystem>(fileSystem);
        _services.AddService<IGitDirectoryResolver>(resolver);
        _services.AddService<IRepositoryDescriptionProvider>(new RepositoryDescriptionProvider(resolver));
        GitCommands.ServiceContainerRegistry.RegisterServices(_services);
        GitUI.ServiceContainerRegistry.RegisterServices(_services);
        _runners.Clear();
        _process = null;
        _remote = null;
        _timedOut = false;
    }

    [TearDown]
    public void TearDown()
    {
        _process?.Close();
        foreach (FailureOnceRunner runner in _runners)
        {
            runner.Dispose();
        }

        FormRemoteProcess.TestAccessor.ConnectHost = _connectHost;
        PuttyHelpers.TestAccessor.CreatePageantExecutable = _createPageant;
        AppSettings.Pageant = _pageantPath;
        WinFormsShims.ShimHost.MessageBoxHost = _messageBoxes!;
        AppSettings.SshPath = _sshPath;
        AppSettings.CloseProcessDialog = _closeProcess;
        AppSettings.DontConfirmPushNewBranch.Value = _confirmBranch;
        AppSettings.DontConfirmAddTrackingRef.Value = _confirmTracking;
        Environment.SetEnvironmentVariable("GIT_CONFIG_GLOBAL", _globalConfig);
        _services.Dispose();
        TestDirectory.Delete(_directory);
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Push_should_load_and_persist_the_chosen_key_before_retrying(bool loadKey)
    {
        GitModule module = CreateRepository();
        string bare = Path.Combine(_directory, "remote.git");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet", "--bare", bare }).Should().BeTrue();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("remote") { "add", "origin", bare }).Should().BeTrue();
        InstallRunner("FATAL ERROR: authentication failed\n", livePrompt: false);
        WinFormsShims.ShimHost.MessageBoxHost = new ConfirmationHost(confirm: true);
        string keyPath = Path.Combine(_directory, "chosen.ppk");
        File.WriteAllText(keyPath, "external test key");
        int loads = 0;
        string pageantPath = Path.Combine(_directory, "test-pageant.exe");
        File.WriteAllText(pageantPath, string.Empty);
        AppSettings.Pageant = pageantPath;
        IExecutable executable = Substitute.For<IExecutable>();
        IProcess process = Substitute.For<IProcess>();
        process.WaitForExitAsync().Returns(Task.FromResult(loadKey ? 0 : 1));
        executable.Start(Arg.Any<ArgumentString>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<bool>(),
            Arg.Any<Encoding?>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                ArgumentString arguments = call.Arg<ArgumentString>();
                if (!string.IsNullOrEmpty(arguments.ToString()))
                {
                    arguments.ToString().Should().Be(keyPath.Quote());
                    loads++;
                }

                return process;
            });
        PuttyHelpers.TestAccessor.CreatePageantExecutable = path =>
        {
            path.Should().Be(pageantPath);
            return executable;
        };
        IStorageProvider storage = Substitute.For<IStorageProvider>();
        IStorageFile file = Substitute.For<IStorageFile>();
        file.Path.Returns(new Uri(keyPath));
        storage.OpenFilePickerAsync(Arg.Any<FilePickerOpenOptions>()).Returns(Task.FromResult<IReadOnlyList<IStorageFile>>([file]));
        IStorageProviderFactory factory = Substitute.For<IStorageProviderFactory>();
        factory.CreateProvider(Arg.Any<TopLevel>()).Returns(storage);
        using IDisposable locator = InstallStorageFactory(factory);
        int prompts = 0;
        using IDisposable opened = ObserveWindows(window =>
        {
            if (window is FormPuttyError)
            {
                prompts++;
                Dispatcher.UIThread.Post(() =>
                {
                    window.FindControl<Button>("LoadSSHKey")!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    if (!loadKey)
                    {
                        window.FindControl<Button>("Cancel")!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    }
                });
            }
        });
        using IDisposable closeErrors = CloseFinishedErrors();
        using FormPush form = new(new GitUICommands(_services, module));
        form.Show();
        try
        {
            form.FindControl<Button>("Push")!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            PumpUntil(() => _process is not null && !_process.IsVisible);
            _timedOut.Should().BeFalse("the injected external command must terminate or accept cancellation");
            bool recovered = OperatingSystem.IsWindows() && loadKey;
            _remote.Should().Be("origin");
            prompts.Should().Be(OperatingSystem.IsWindows() ? 1 : 0);
            loads.Should().Be(OperatingSystem.IsWindows() ? 1 : 0);
            _runners.Should().ContainSingle().Which.Starts.Should().Be(recovered ? 2 : 1);
            module.GetSetting("remote.origin.puttykeyfile").Should().Be(recovered ? keyPath.Replace('\\', '/') : string.Empty);
            GitModule remoteModule = new(_services.GetRequiredService<IGitExecutorProvider>(), bare);
            remoteModule.RevParse($"refs/heads/{module.GetSelectedBranch()}").Should().Be(recovered ? module.GetCurrentCheckout() : default(ObjectId));
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(false, false)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(true, true)]
    public void Clone_should_confirm_the_source_host_and_retry_only_when_accepted(bool confirm, bool livePrompt)
    {
        GitModule source = CreateRepository();
        string destination = Path.Combine(_directory, "destination");
        Directory.CreateDirectory(destination);
        string failure = !OperatingSystem.IsWindows()
            ? "Permission denied (publickey).\n"
            : livePrompt
                ? "If you trust this host, enter \"y\" to add the key to"
                : "The server's host key is not cached in the registry\n";
        InstallRunner(failure, livePrompt && OperatingSystem.IsWindows());
        ConfirmationHost messages = new(confirm);
        WinFormsShims.ShimHost.MessageBoxHost = messages;
        List<string> connections = [];
        FormRemoteProcess.TestAccessor.ConnectHost = url =>
        {
            connections.Add(url);
            return true;
        };
        using IDisposable opened = ObserveWindows(_ => { });
        using IDisposable closeErrors = CloseFinishedErrors();
        using FormClone form = new(new GitUICommands(_services, source), url: null, openedFromProtocolHandler: false, gitModuleChanged: null);
        form.Show();
        try
        {
            form.FindControl<ComboBox>("_NO_TRANSLATE_From")!.Text = source.WorkingDir;
            form.FindControl<ComboBox>("_NO_TRANSLATE_To")!.Text = destination;
            form.FindControl<TextBox>("_NO_TRANSLATE_NewDirectory")!.Text = "cloned";
            form.FindControl<Button>("Ok")!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            PumpUntil(() => _process is not null && !_process.IsVisible);
            _timedOut.Should().BeFalse("the injected external command must terminate or accept cancellation");
            bool recovered = OperatingSystem.IsWindows() && confirm;
            messages.Confirmations.Should().Be(OperatingSystem.IsWindows() ? 1 : 0);
            connections.Should().Equal(recovered ? [source.GetPathForGitExecution(source.WorkingDir)] : []);
            _runners.Should().ContainSingle().Which.Starts.Should().Be(recovered ? 2 : 1);
            File.Exists(Path.Combine(destination, "cloned", "tracked.txt")).Should().Be(recovered);
        }
        finally
        {
            form.Close();
        }
    }

    private GitModule CreateRepository()
    {
        string path = Path.Combine(_directory, "source");
        Directory.CreateDirectory(path);
        GitModule module = new(_services.GetRequiredService<IGitExecutorProvider>(), path);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet", "-b", "main" }).Should().BeTrue();
        module.SetSetting("user.name", "Avalonia Test");
        module.SetSetting("user.email", "avalonia@example.com");
        File.WriteAllText(Path.Combine(path, "tracked.txt"), "initial\n");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "tracked.txt" }).Should().BeTrue();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" }).Should().BeTrue();
        return module;
    }

    private void InstallRunner(string failure, bool livePrompt)
    {
        IConsoleEmulatorsRegistry registry = Substitute.For<IConsoleEmulatorsRegistry>();
        registry.CreateCommandController().Returns(_ =>
        {
            FailureOnceRunner runner = new(failure, livePrompt);
            _runners.Add(runner);
            return runner;
        });
        _services.RemoveService(typeof(IConsoleEmulatorsRegistry));
        _services.AddService(registry);
    }

    private static IDisposable InstallStorageFactory(IStorageProviderFactory factory)
    {
        // Avalonia hides the locator registration API from its reference assembly.
        // Replace the external picker factory through its runtime scope, leaving window initialization intact.
        Type locator = typeof(AvaloniaLocator);
        IDisposable scope = (IDisposable)locator.GetMethod("EnterScope")!.Invoke(null, null)!;
        try
        {
            object current = locator.GetProperty("CurrentMutable")!.GetValue(null)!;
            object binding = locator.GetMethod("Bind")!.MakeGenericMethod(typeof(IStorageProviderFactory)).Invoke(current, null)!;
            binding.GetType().GetMethod("ToConstant")!.MakeGenericMethod(typeof(IStorageProviderFactory)).Invoke(binding, [factory]);
            return scope;
        }
        catch
        {
            scope.Dispose();
            throw;
        }
    }

    private IDisposable ObserveWindows(Action<Window> observe)
        => Window.WindowOpenedEvent.AddClassHandler<Window>((window, _) =>
        {
            if (window is FormRemoteProcess process)
            {
                _process = process;
                _remote = process.Remote;
            }

            observe(window);
        });

    private IDisposable CloseFinishedErrors()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        return DispatcherTimer.Run(() =>
        {
            if (_process is { IsVisible: true } process)
            {
                if (!process.Ok.IsEnabled && stopwatch.Elapsed > TimeSpan.FromSeconds(30))
                {
                    _timedOut = true;
                    process.Abort.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                }

                if (process.Ok.IsEnabled && process.ErrorOccurred())
                {
                    process.Ok.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                }
            }

            return true;
        }, TimeSpan.FromMilliseconds(25));
    }

    private static void PumpUntil(Func<bool> done)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (!done() && stopwatch.Elapsed < TimeSpan.FromSeconds(30))
        {
            Dispatcher.UIThread.RunJobs();
            Thread.Sleep(10);
        }

        done().Should().BeTrue("the production dialog should finish its command");
    }

    private sealed class ConfirmationHost(bool confirm) : WinFormsShims.IMessageBoxHost
    {
        public int Confirmations { get; private set; }

        public WinFormsShims.DialogResult Show(WinFormsShims.IWin32Window? owner, string? text, string? caption,
            WinFormsShims.MessageBoxButtons buttons, WinFormsShims.MessageBoxIcon icon, WinFormsShims.MessageBoxDefaultButton defaultButton)
        {
            if (owner is FormRemoteProcess && buttons == WinFormsShims.MessageBoxButtons.YesNo)
            {
                Confirmations++;
                return confirm ? WinFormsShims.DialogResult.Yes : WinFormsShims.DialogResult.No;
            }

            return WinFormsShims.DialogResult.OK;
        }
    }

    private sealed class FailureOnceRunner : IPlainTextConsoleCommandRunner, IDisposable
    {
        private readonly PlainTextConsoleCommandRunner _inner = new();
        private readonly string _failure;
        private readonly bool _livePrompt;
        private bool _fakeActive;

        public FailureOnceRunner(string failure, bool livePrompt)
        {
            _failure = failure;
            _livePrompt = livePrompt;
            _inner.CommandOutputReceived += (sender, args) => CommandOutputReceived?.Invoke(sender, args);
            _inner.CommandProcessExited += (sender, args) => CommandProcessExited?.Invoke(sender, args);
            _inner.ConsoleHostTerminated += (sender, args) => ConsoleHostTerminated?.Invoke(sender, args);
        }

        public event EventHandler<ConsoleOutputEventArgs>? CommandOutputReceived;
        public event EventHandler<ConsoleProcessExitEventArgs>? CommandProcessExited;
        public event EventHandler? ConsoleHostTerminated;
        public Control Control => _inner.Control;
        public int Starts { get; private set; }

        public void StartCommand(string command, string arguments, string workDir, Dictionary<string, string> envVariables)
        {
            Starts++;
            if (Starts > 1)
            {
                _inner.StartCommand(command, arguments, workDir, envVariables);
                return;
            }

            _fakeActive = true;
            Dispatcher.UIThread.Post(() =>
            {
                CommandOutputReceived?.Invoke(this, new ConsoleOutputEventArgs(_failure));
                if (!_livePrompt)
                {
                    _fakeActive = false;
                    CommandProcessExited?.Invoke(this, new ConsoleProcessExitEventArgs(1));
                }
            });
        }

        public void KillCommandProcess()
        {
            _inner.KillCommandProcess();
            if (_fakeActive)
            {
                _fakeActive = false;
                CommandProcessExited?.Invoke(this, new ConsoleProcessExitEventArgs(-1));
            }
        }

        public void ResetConsole()
        {
            _inner.ResetConsole();
            KillCommandProcess();
        }

        public void WriteCommandProcessInput(string text) => _inner.WriteCommandProcessInput(text);
        public void WriteOutputText(string text) => _inner.WriteOutputText(text);
        public void Dispose() => _inner.Dispose();
    }
}
