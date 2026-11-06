using System.ComponentModel.Design;
using Avalonia;
using GitCommands;
using GitExtensions.Compat;
using GitUI;

namespace GitExtensions;

internal static class Program
{
    internal static readonly ServiceContainer ServiceContainer = new();

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static async Task<int> Main(string[] args)
    {
        ServiceContainerRegistry.RegisterServices(ServiceContainer);

        AppSettings.SetDocumentationBaseUrl(AppSettings.ProductVersion);
        AppTitleGenerator.Initialise(ThisAssembly.Git.Sha, ThisAssembly.Git.Branch);

        // If an error happens before we had a chance to init the environment information
        // the call to GetInformation() from UIReporter.ShowNBug() will fail.
        // There's no perf hit calling Initialise() multiple times.
        UserEnvironmentInformation.Initialise(ThisAssembly.Git.Sha, ThisAssembly.Git.IsDirty);

        if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(FlatpakConformanceProbe.ReportPathEnvironmentVariable)))
        {
            int? flatpakConformanceExitCode = await FlatpakConformanceProbe.RunIfRequestedAsync();
            return flatpakConformanceExitCode!.Value;
        }

        int exitCode = BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

        AppSettings.SaveSettings();
        return exitCode;
    }

    /// <summary>
    ///  Also used by the Avalonia designer/previewer tooling, which discovers it by name.
    /// </summary>
    public static AppBuilder BuildAvaloniaApp()
    {
        AppBuilder builder = AppBuilder.Configure<App>()
            .UsePlatformDetect();

        if (ShouldUseWayland(OperatingSystem.IsLinux(), Environment.GetEnvironmentVariable("WAYLAND_DISPLAY")))
        {
            builder = builder.UseWayland();
        }

        return builder.LogToTrace();
    }

    internal static bool ShouldUseWayland(bool isLinux, string? waylandDisplay)
        => isLinux && !string.IsNullOrWhiteSpace(waylandDisplay);
}
