using System.Text;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using BugReporter.Serialization;
using GitUI;
using GitUI.Compat;
using Microsoft.VisualStudio.Threading;

namespace BugReporter;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        // This context installed to obtain UI synchronization context only
        AvaloniaSynchronizationContext.InstallIfNeeded();

        // Store the shared JoinableTaskContext
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();

        // Framework constraint: the independent Avalonia application supplies its own native host and theme.
        if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            ShimServices.Install(desktop);
        }

        AvaloniaFontSettings.InstallSystemDefaults();
        AvaloniaThemeSettings.ApplyAppSettings();
        AvaloniaFontSettings.ApplyAppSettings();

        // If an error happens before we had a chance to init the environment information
        // the call to GetInformation() from BugReporter.Program.Main() will fail.
        // There's no perf hit calling Initialise() multiple times.
        UserEnvironmentInformation.Initialise(ThisAssembly.Git.Sha, ThisAssembly.Git.IsDirty);

        SerializableException? exception = null;

        // parity-scaffolding: retain real command-line I/O while allowing a headless handoff to observe the receiver.
        string[] args = AvaloniaBugReportProcess.GetCommandLineArgs();
        foreach (string arg in args)
        {
            string xml;
            try
            {
                xml = Base64Decode(arg);
            }
            catch (Exception ex)
            {
                exception = new(new Exception($"Failed to decode the error payload\r\n{arg}", ex));
                continue;
            }

            try
            {
                exception = SerializableException.FromXmlString(xml);
            }
            catch (Exception ex)
            {
                exception = new(new Exception($"Failed to decode/parse error payload\r\n{xml}", ex));
            }
        }

        // No specific info extracted from these exceptions
        string exceptionInfo = "";
        exception ??= new(new Exception("Missing error payload"));

        new BugReportForm().ShowDialog(owner: null, exception, exceptionInfo, UserEnvironmentInformation.GetInformation(), canIgnore: true, showIgnore: false, focusDetails: false);
    }

    private static string Base64Decode(string base64EncodedData)
    {
        byte[] base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
        return Encoding.UTF8.GetString(base64EncodedBytes);
    }

    // Framework constraint: the shared apphost invokes the original private entry point through this bridge.
    internal static void Run() => Main();
}
