using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Compat;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs.SettingsDialog.Pages;
using GitUI.Compat;
using GitUI.NBugReports;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using ShutdownMode = Avalonia.Controls.ShutdownMode;

namespace GitExtensions;

public partial class App : Application
{
    public override void Initialize()
    {
        Program.InitializeDocumentationBaseUrl();
        AvaloniaXamlLoader.Load(this);
        AvaloniaFontSettings.InstallPlatformDefaults(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Avalonia reports recoverable UI failures through its dispatcher instead of WinForms ThreadException.
        Dispatcher.UIThread.UnhandledException += (_, e) =>
        {
            ReportUnhandledException(e.Exception, isTerminating: false);
            e.Handled = true;
        };

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // The JoinableTaskContext must capture Avalonia's UI SynchronizationContext,
            // which is established here by installing the context before constructing it.
            // This context installed to obtain UI synchronization context only
            AvaloniaSynchronizationContext.InstallIfNeeded();

            // Store the shared JoinableTaskContext
            ThreadHelper.JoinableTaskContext = new JoinableTaskContext();

            ShimServices.Install(desktop);

            // Cross-platform constraint: external tools need host-visible files, while their launch proxies stay private.
            DispatcherPump.Wait(FlatpakToolEnvironment.InitializeAsync);

            // If you want to suppress the BugReportInvoker when debugging and exit quickly, uncomment the condition:
            ////if (!Debugger.IsAttached)
            AppDomain.CurrentDomain.UnhandledException += (_, e) => ReportUnhandledException(
                e.ExceptionObject as Exception ?? new InvalidOperationException($"Unhandled object: {e.ExceptionObject}"),
                e.IsTerminating);
            Shims.WinForms.Application.ThreadException += (_, e) => ReportUnhandledException(e.Exception, isTerminating: false);

            AvaloniaFontSettings.InstallSystemDefaults();

            // Cross-platform constraint: portable plugins are isolated from WinForms-only user plugins.
            string? userPluginsPath = UserPluginsDirectory.GetPath(AppSettings.LocalApplicationDataPath.Value);
            ManagedExtensibility.Initialise(userPluginsPath: userPluginsPath);
            AppSettings.LoadSettings();
            FlatpakHostGit.InitializeEnvironment();
            AvaloniaThemeSettings.ApplyAppSettings();
            AvaloniaFontSettings.ApplyAppSettings();

            string[] args = Environment.GetCommandLineArgs();
            GitModule module = new(Program.ServiceContainer.GetRequiredService<IGitExecutorProvider>(), GetWorkingDir(args));
            BugReportInvoker.ExecutorProvider = Program.ServiceContainer.GetRequiredService<IGitExecutorProvider>();
            GitUICommands commands = new(Program.ServiceContainer, module);
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            desktop.Exit += (_, _) =>
            {
                BugReportInvoker.IgnoreFailedToLoadAnAssembly = true;
                AppSettings.SaveSettings();
            };

            // NOTE we perform the rest of the application's startup in another method to defer
            // the JIT processing more types than required to configure native bug reporting.
            // In this way, there's more chance we can handle startup exceptions correctly.
            Dispatcher.UIThread.Post(() => RunStartup(desktop, commands, args));
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void RunStartup(
        IClassicDesktopStyleApplicationLifetime desktop,
        GitUICommands commands,
        string[] args)
    {
        try
        {
            if (string.IsNullOrEmpty(AppSettings.Translation))
            {
                using FormChooseTranslation formChoose = new();
                formChoose.ShowDialog();
            }

            FlatpakHostGit.InitializeEnvironment();
            DispatcherPump.Wait(async () =>
            {
                await FlatpakHostGit.NotifyIfFallingBackAsync();
                return true;
            });
            FileEditorConformanceProbe.InstallIfRequested(desktop);

            StartupCoordinator coordinator = new();
            if (!coordinator.EnsurePrerequisites(commands, args))
            {
                desktop.Shutdown(-1);
                return;
            }

            if (args.Length <= 1)
            {
                commands.StartBrowseDialog(owner: null);
            }
            else
            {
                // if we are here args.Length > 1
                RunCommand(desktop, commands, args);
            }

            // parity-scaffolding: opt-in native Linux desktop evidence until the platform gate closes.
            WaylandConformanceProbe.StartIfRequested(desktop);

            // parity-scaffolding: opt-in XDG portal evidence until the platform gate closes.
            PortalConformanceProbe.StartIfRequested(desktop);

            // parity-scaffolding: font evidence is requested only after the runtime screenshot.
            RuntimeFontProbe.StartIfRequested(desktop);

            HostGitConformanceProbe.StartIfRequested(desktop, commands);
        }
        catch (Exception exception)
        {
            // TODO: remove catch-all
            ReportUnhandledException(exception, isTerminating: false);
            desktop.Shutdown(-1);
        }
    }

    private static void RunCommand(IClassicDesktopStyleApplicationLifetime desktop, GitUICommands commands, string[] args)
    {
        // Avoid replacing the ExitCode eventually set while parsing arguments,
        // i.e. assume -1 and afterwards, only set it to 0 if no error is indicated.
        int exitCode = -1;
        try
        {
            if (commands.RunCommand(args))
            {
                exitCode = 0;
            }
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            GitUI.MessageBoxes.ShowError(
                owner: null,
                $"Invalid Git Extensions command line:{Environment.NewLine}{Environment.NewLine}{exception}",
                "Command-line error");
        }

        if (desktop.MainWindow is null)
        {
            desktop.Shutdown(exitCode);
        }
    }

    private static void ReportUnhandledException(Exception exception, bool isTerminating)
    {
        Console.Error.WriteLine(exception);
        if (Dispatcher.UIThread.CheckAccess())
        {
            BugReportInvoker.Report(exception, isTerminating);
        }
        else
        {
            Dispatcher.UIThread.Invoke(() => BugReportInvoker.Report(exception, isTerminating));
        }
    }

    // Keep startup working-directory resolution aligned with Program.GetWorkingDir.
    internal static string? GetWorkingDir(string[] args)
    {
        string? workingDir = null;

        if (args.Length >= 3)
        {
            // there is bug in .net
            // while parsing command line arguments, it unescapes " incorrectly
            // https://github.com/gitextensions/gitextensions/issues/3489
            string dirArg = args[2].TrimEnd('"');
            if (!string.IsNullOrWhiteSpace(dirArg))
            {
                if (!Directory.Exists(dirArg))
                {
                    dirArg = Path.GetDirectoryName(dirArg)!;
                }

                workingDir = GitModule.TryFindGitWorkingDir(dirArg);

                if (Directory.Exists(workingDir))
                {
                    workingDir = Path.GetFullPath(workingDir);
                }

                // Do not add this working directory to the recent repositories. It is a nice feature, but it
                // also increases the startup time
                ////if (Module.ValidWorkingDir())
                ////   Repositories.RepositoryHistory.AddMostRecentRepository(Module.WorkingDir);
            }
        }

        if (args.Length <= 1 && workingDir is null && AppSettings.StartWithRecentWorkingDir)
        {
            if (GitModule.IsValidGitWorkingDir(AppSettings.RecentWorkingDir))
            {
                workingDir = AppSettings.RecentWorkingDir;
            }
        }

        if (args.Length > 1 && workingDir is null)
        {
            // If no working dir is yet found, try to find one relative to the current working directory.
            // This allows the `fileeditor` command to discover repository configuration which is
            // required for core.commentchar support.
            workingDir = GitModule.TryFindGitWorkingDir(Environment.CurrentDirectory);
        }

        return workingDir;
    }
}
