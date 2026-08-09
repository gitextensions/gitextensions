using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using GitExtensions.Shims.WinForms;
using GitUI.Compat;

namespace GitExtensions.Compat;

// parity-scaffolding: records real XDG portal transport evidence until the platform gate closes.
internal sealed class PortalConformanceProbe
{
    internal const string ReportPathEnvironmentVariable = "GITEXTENSIONS_PORTAL_CONFORMANCE_REPORT";
    internal const string ExpectedModeEnvironmentVariable = "GITEXTENSIONS_PORTAL_CONFORMANCE_EXPECTED";
    internal const string FixturePathEnvironmentVariable = "GITEXTENSIONS_PORTAL_CONFORMANCE_FIXTURE";

    private readonly string _reportPath;
    private readonly string _expectedMode;
    private readonly string _fixturePath;
    private readonly Window _mainWindow;
    private readonly XdgDesktopPortal _portal = new();
    private string _stage = "starting";
    private bool? _fileChooserAvailable;
    private bool? _openUriAvailable;
    private bool? _openFileAccepted;
    private bool? _openDirectoryAccepted;
    private bool? _showInDirectoryAccepted;
    private bool? _openUriAccepted;
    private bool? _openFilePickerCompleted;
    private bool? _openFolderPickerCompleted;
    private bool? _saveFilePickerCompleted;
    private string? _error;

    private PortalConformanceProbe(string reportPath, string expectedMode, string fixturePath, Window mainWindow)
    {
        _reportPath = reportPath;
        _expectedMode = expectedMode;
        _fixturePath = fixturePath;
        _mainWindow = mainWindow;
    }

    internal static bool IsSupportedRequest(string? reportPath, string? expectedMode, string? fixturePath, bool isLinux)
        => !string.IsNullOrWhiteSpace(reportPath)
            && (expectedMode is "present" or "absent")
            && !string.IsNullOrWhiteSpace(fixturePath)
            && isLinux;

    internal static void StartIfRequested(IClassicDesktopStyleApplicationLifetime desktop)
    {
        string? reportPath = Environment.GetEnvironmentVariable(ReportPathEnvironmentVariable);
        string? expectedMode = Environment.GetEnvironmentVariable(ExpectedModeEnvironmentVariable);
        string? fixturePath = Environment.GetEnvironmentVariable(FixturePathEnvironmentVariable);
        if (!IsSupportedRequest(reportPath, expectedMode, fixturePath, OperatingSystem.IsLinux())
            || desktop.MainWindow is not { } mainWindow)
        {
            return;
        }

        PortalConformanceProbe probe = new(
            Path.GetFullPath(reportPath!),
            expectedMode!,
            Path.GetFullPath(fixturePath!),
            mainWindow);
        Dispatcher.UIThread.Post(() => _ = probe.RunAsync(), DispatcherPriority.Background);
    }

    private async Task RunAsync()
    {
        try
        {
            WriteReport();
            _fileChooserAvailable = await _portal.IsInterfaceAvailableAsync(XdgDesktopPortal.FileChooserInterface);
            _openUriAvailable = await _portal.IsInterfaceAvailableAsync(XdgDesktopPortal.OpenUriInterface);
            WriteReport();

            if (_expectedMode == "present")
            {
                await RunPickersAsync();
            }

            string directory = Path.GetDirectoryName(_fixturePath)!;
            _stage = "openFileAction";
            WriteReport();
            _openFileAccepted = await _portal.TryLaunchAsync(_fixturePath, OsShellLaunchKind.Open);
            _stage = "openDirectoryAction";
            WriteReport();
            _openDirectoryAccepted = await _portal.TryLaunchAsync(directory, OsShellLaunchKind.OpenDirectory);
            _stage = "showInDirectoryAction";
            WriteReport();
            _showInDirectoryAccepted = await _portal.TryLaunchAsync(_fixturePath, OsShellLaunchKind.ShowInDirectory);
            _stage = "openUriAction";
            WriteReport();
            _openUriAccepted = await _portal.TryLaunchAsync("https://portal-p82.example.invalid/", OsShellLaunchKind.OpenUri);
            _stage = "completed";
        }
        catch (Exception exception)
        {
            _error = exception.ToString();
            _stage = "failed";
        }

        WriteReport();
    }

    private async Task RunPickersAsync()
    {
        IStorageProvider storageProvider = _mainWindow.StorageProvider;
        _stage = "openFilePicker";
        WriteReport();
        IReadOnlyList<IStorageFile> files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = false,
            Title = "P8.2 Open file",
        });
        _openFilePickerCompleted = files.Count == 1;

        _stage = "openFolderPicker";
        WriteReport();
        IReadOnlyList<IStorageFolder> folders = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            AllowMultiple = false,
            Title = "P8.2 Open folder",
        });
        _openFolderPickerCompleted = folders.Count == 1;

        _stage = "saveFilePicker";
        WriteReport();
        IStorageFile? file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            SuggestedFileName = "p82-portal.txt",
            Title = "P8.2 Save file",
        });
        _saveFilePickerCompleted = file is not null;
    }

    private void WriteReport()
    {
        object report = new
        {
            schemaVersion = 1,
            expectedMode = _expectedMode,
            stage = _stage,
            backend = "xdgDesktopPortal",
            interfaces = new
            {
                fileChooser = _fileChooserAvailable,
                openUri = _openUriAvailable,
            },
            pickers = new
            {
                openFileCompleted = _openFilePickerCompleted,
                openFolderCompleted = _openFolderPickerCompleted,
                saveFileCompleted = _saveFilePickerCompleted,
            },
            shellActions = new
            {
                openFileAccepted = _openFileAccepted,
                openDirectoryAccepted = _openDirectoryAccepted,
                showInDirectoryAccepted = _showInDirectoryAccepted,
                openUriAccepted = _openUriAccepted,
            },
            error = _error,
        };

        string? parentDirectory = Path.GetDirectoryName(_reportPath);
        if (!string.IsNullOrEmpty(parentDirectory))
        {
            Directory.CreateDirectory(parentDirectory);
        }

        string temporaryPath = $"{_reportPath}.tmp";
        File.WriteAllText(
            temporaryPath,
            JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine);
        File.Move(temporaryPath, _reportPath, overwrite: true);
    }
}
