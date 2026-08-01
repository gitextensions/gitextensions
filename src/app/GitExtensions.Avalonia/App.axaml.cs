using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI;
using GitUI.Compat;
using GitUI.NBugReports;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using ShutdownMode = Avalonia.Controls.ShutdownMode;

namespace GitExtensions;

public partial class App : Application
{
    private const string AvaloniaUserPluginsDirectoryName = "UserPlugins.Avalonia";

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        AvaloniaFontSettings.InstallPlatformDefaults(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // The JoinableTaskContext must capture Avalonia's UI SynchronizationContext —
            // the twin of the `using (new Form())` trick in the WinForms Program.
            AvaloniaSynchronizationContext.InstallIfNeeded();
            ThreadHelper.JoinableTaskContext = new JoinableTaskContext();

            ShimServices.Install(desktop);
            AppDomain.CurrentDomain.UnhandledException += (_, e) => ReportUnhandledException(
                e.ExceptionObject as Exception ?? new InvalidOperationException($"Unhandled object: {e.ExceptionObject}"),
                e.IsTerminating);
            Shims.WinForms.Application.ThreadException += (_, e) => ReportUnhandledException(e.Exception, isTerminating: false);

            AvaloniaFontSettings.InstallSystemDefaults();
            string userPluginsPath = Path.Join(AppSettings.LocalApplicationDataPath.Value!, AvaloniaUserPluginsDirectoryName);
            ManagedExtensibility.Initialise(userPluginsPath: userPluginsPath);
            AppSettings.LoadSettings();
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
                RunCommand(desktop, commands, args);
            }
        }
        catch (Exception exception)
        {
            ReportUnhandledException(exception, isTerminating: false);
            desktop.Shutdown(-1);
        }
    }

    private static void RunCommand(IClassicDesktopStyleApplicationLifetime desktop, GitUICommands commands, string[] args)
    {
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

    // Twin of GitExtensions/Program.cs GetWorkingDir (keep in sync on upstream drift).
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
