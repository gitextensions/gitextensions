using System.ComponentModel.Design;
using Avalonia;
using GitCommands;

namespace GitExtensions;

internal static class Program
{
    internal static readonly ServiceContainer ServiceContainer = new();

    [STAThread]
    private static int Main(string[] args)
    {
        ServiceContainerRegistry.RegisterServices(ServiceContainer);

        AppSettings.SetDocumentationBaseUrl(AppSettings.ProductVersion);
        AppTitleGenerator.Initialise(ThisAssembly.Git.Sha, ThisAssembly.Git.Branch);

        int exitCode = BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

        AppSettings.SaveSettings();
        return exitCode;
    }

    /// <summary>
    ///  Also used by the Avalonia designer/previewer tooling, which discovers it by name.
    /// </summary>
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}
