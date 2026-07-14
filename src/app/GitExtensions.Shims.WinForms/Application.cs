using System.Diagnostics;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace GitExtensions.Shims.WinForms;

/// <summary>
///  Stand-in for <c>System.Windows.Forms.Application</c>: application metadata, paths, and the
///  thread-exception channel, implemented over <see cref="Environment"/> and entry-assembly
///  attributes with the same semantics WinForms uses.
/// </summary>
/// <remarks>
///  Consumed by: <c>GitCommands/Settings/AppSettings.cs</c> (<see cref="ProductVersion"/>,
///  <see cref="ExecutablePath"/>, <see cref="UserAppDataPath"/>),
///  <c>GitCommands/Patches/PatchManager.cs</c> (<see cref="ProductName"/>),
///  <c>GitExtUtils/GitUI/TaskManager.cs</c> (<see cref="OnThreadException"/>).
/// </remarks>
public static class Application
{
    /// <summary>
    ///  Occurs when an otherwise unhandled exception is reported via <see cref="OnThreadException"/>.
    ///  The application installs a handler at startup (e.g. showing a bug-report dialog).
    /// </summary>
    public static event ThreadExceptionEventHandler? ThreadException;

    /// <summary>
    ///  Gets or sets the system light/dark mode. The Avalonia application assigns this at
    ///  startup (and on theme changes) from its actual theme variant; the default is
    ///  <see cref="SystemColorMode.Classic"/> (light).
    /// </summary>
    public static SystemColorMode SystemColorMode { get; set; }

    /// <summary>
    ///  Gets the product name from the entry assembly, like WinForms.
    /// </summary>
    public static string ProductName
        => Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyProductAttribute>()?.Product
            ?? Process.GetCurrentProcess().ProcessName;

    /// <summary>
    ///  Gets the informational version of the entry assembly, like WinForms.
    /// </summary>
    public static string ProductVersion
        => Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? "1.0.0.0";

    /// <summary>
    ///  Gets the path of the executable that started the process.
    /// </summary>
    public static string ExecutablePath
        => Environment.ProcessPath ?? Assembly.GetEntryAssembly()?.Location ?? string.Empty;

    /// <summary>
    ///  Gets the per-user application data path
    ///  (<c>{ApplicationData}/{Company}/{Product}/{ProductVersion}</c>, mirroring WinForms;
    ///  <c>ApplicationData</c> maps to <c>~/.config</c> on Linux).
    /// </summary>
    public static string UserAppDataPath
    {
        get
        {
            string company = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? ProductName;
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                company,
                ProductName,
                ProductVersion);
        }
    }

    /// <summary>
    ///  Reports an exception to the installed <see cref="ThreadException"/> handler.
    ///  Without a handler the exception is rethrown with its original stack trace —
    ///  swallowing it silently would hide errors the WinForms default dialog surfaces.
    /// </summary>
    public static void OnThreadException(Exception exception)
    {
        if (ThreadException is ThreadExceptionEventHandler handler)
        {
            handler(null!, new ThreadExceptionEventArgs(exception));
            return;
        }

        ExceptionDispatchInfo.Capture(exception).Throw();
    }
}
