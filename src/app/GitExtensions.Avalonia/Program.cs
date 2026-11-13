using System.ComponentModel.Design;
using Avalonia;
using GitCommands;
using GitExtensions.Compat;
using GitUI;

namespace GitExtensions;

// Framework constraint: the CLR entry point stays synchronous for STA; the Flatpak-only probe finishes before UI startup.
internal static class Program
{
    internal static readonly ServiceContainer ServiceContainer = new();
    private static readonly Lazy<bool> DocumentationBaseUrlInitialization = new(() =>
    {
        AppSettings.SetDocumentationBaseUrl(AppSettings.ProductVersion);
        return true;
    });

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static int Main(string[] args)
    {
        ServiceContainerRegistry.RegisterServices(ServiceContainer);

        InitializeDocumentationBaseUrl();
        AppTitleGenerator.Initialise(ThisAssembly.Git.Sha, ThisAssembly.Git.Branch);

        // If an error happens before we had a chance to init the environment information
        // the call to GetInformation() from UIReporter.ShowNBug() will fail.
        // There's no perf hit calling Initialise() multiple times.
        UserEnvironmentInformation.Initialise(ThisAssembly.Git.Sha, ThisAssembly.Git.IsDirty);

        if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(FlatpakConformanceProbe.ReportPathEnvironmentVariable)))
        {
#pragma warning disable VSTHRD002
            int? flatpakConformanceExitCode = FlatpakConformanceProbe.RunIfRequestedAsync().GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
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

    internal static void InitializeDocumentationBaseUrl()
        => _ = DocumentationBaseUrlInitialization.Value;

    internal static bool ShouldUseWayland(bool isLinux, string? waylandDisplay)
        => isLinux && !string.IsNullOrWhiteSpace(waylandDisplay);
}
