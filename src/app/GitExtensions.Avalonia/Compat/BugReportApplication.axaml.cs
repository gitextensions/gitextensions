using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GitUI.Compat;
using ShutdownMode = Avalonia.Controls.ShutdownMode;

namespace GitExtensions.Compat;

/// <summary>
///  Hosts the independent error reporter without initializing the normal application or its plugins.
/// </summary>
public partial class BugReportApplication : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        AvaloniaFontSettings.InstallPlatformDefaults(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            Dispatcher.UIThread.Post(() =>
            {
                try
                {
                    BugReporter.Program.Run();
                }
                finally
                {
                    desktop.Shutdown();
                }
            });
        }

        base.OnFrameworkInitializationCompleted();
    }
}
