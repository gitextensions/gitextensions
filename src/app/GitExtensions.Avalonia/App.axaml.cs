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
using GitUI.CommandsDialogs;
using GitUI.Compat;
using Microsoft.VisualStudio.Threading;

namespace GitExtensions;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
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
            Shims.WinForms.Application.ThreadException += (_, e)
                => GitUI.MessageBoxes.ShowError(owner: null, e.Exception.ToString(), "Unhandled exception");

            AppSettings.LoadSettings();

            string[] args = Environment.GetCommandLineArgs();
            GitModule module = new(Program.ServiceContainer.GetRequiredService<IGitExecutorProvider>(), GetWorkingDir(args));
            desktop.MainWindow = new FormBrowse(Program.ServiceContainer, module);
        }

        base.OnFrameworkInitializationCompleted();
    }

    // Twin of GitExtensions/Program.cs GetWorkingDir (keep in sync on upstream drift).
    private static string? GetWorkingDir(string[] args)
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
