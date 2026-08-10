using Avalonia.Platform.Storage;
using GitExtensions.Shims.WinForms;

namespace GitUI.Compat;

/// <summary>
///  Ensures Linux file pickers use the XDG desktop portal instead of Avalonia's managed fallback.
/// </summary>
public static class PortalPickerGuard
{
    private const string ErrorCaption = "Git Extensions";

    public static bool IsAvailable()
        => DispatcherPump.Wait(IsAvailableAsync);

    public static Task<bool> IsAvailableAsync()
        => IsAvailableAsync(new XdgDesktopPortal(), OperatingSystem.IsLinux, ReportUnavailable);

    // Framework constraint: Avalonia 12.1's portal provider assumes a cancelled response contains "uris" and can hang.
    public static Task<IReadOnlyList<IStorageFile>> OpenFilePickerAsync(
        IStorageProvider storageProvider,
        FilePickerOpenOptions options)
        => OpenFilePickerAsync(storageProvider, options, new XdgDesktopPortal(), OperatingSystem.IsLinux);

    public static Task<IReadOnlyList<IStorageFolder>> OpenFolderPickerAsync(
        IStorageProvider storageProvider,
        FolderPickerOpenOptions options)
        => OpenFolderPickerAsync(storageProvider, options, new XdgDesktopPortal(), OperatingSystem.IsLinux);

    public static Task<IStorageFile?> SaveFilePickerAsync(
        IStorageProvider storageProvider,
        FilePickerSaveOptions options)
        => SaveFilePickerAsync(storageProvider, options, new XdgDesktopPortal(), OperatingSystem.IsLinux);

    internal static async Task<bool> IsAvailableAsync(
        IXdgDesktopPortal portal,
        Func<bool> isLinux,
        Action reportUnavailable)
    {
        if (!isLinux() || await portal.IsInterfaceAvailableAsync(XdgDesktopPortal.FileChooserInterface))
        {
            return true;
        }

        reportUnavailable();
        return false;
    }

    internal static async Task<IReadOnlyList<IStorageFile>> OpenFilePickerAsync(
        IStorageProvider storageProvider,
        FilePickerOpenOptions options,
        IXdgDesktopPortal portal,
        Func<bool> isLinux)
    {
        if (!isLinux())
        {
            return await storageProvider.OpenFilePickerAsync(options);
        }

        XdgFileChooserResult result = await portal.ShowFileChooserAsync(CreateRequest(options));
        if (!result.Accepted)
        {
            return [];
        }

        List<IStorageFile> files = [];
        foreach (Uri uri in result.Uris.Where(uri => uri.IsFile))
        {
            IStorageFile? file = await storageProvider.TryGetFileFromPathAsync(uri.LocalPath);
            if (file is not null)
            {
                files.Add(file);
            }
        }

        return files;
    }

    internal static async Task<IReadOnlyList<IStorageFolder>> OpenFolderPickerAsync(
        IStorageProvider storageProvider,
        FolderPickerOpenOptions options,
        IXdgDesktopPortal portal,
        Func<bool> isLinux)
    {
        if (!isLinux())
        {
            return await storageProvider.OpenFolderPickerAsync(options);
        }

        XdgFileChooserResult result = await portal.ShowFileChooserAsync(new XdgFileChooserRequest(
            options.Title ?? string.Empty,
            Directory: true,
            options.AllowMultiple,
            Save: false,
            options.SuggestedStartLocation?.TryGetLocalPath(),
            SuggestedFileName: null,
            Filters: []));
        if (!result.Accepted)
        {
            return [];
        }

        List<IStorageFolder> folders = [];
        foreach (Uri uri in result.Uris.Where(uri => uri.IsFile))
        {
            IStorageFolder? folder = await storageProvider.TryGetFolderFromPathAsync(uri.LocalPath);
            if (folder is not null)
            {
                folders.Add(folder);
            }
        }

        return folders;
    }

    internal static async Task<IStorageFile?> SaveFilePickerAsync(
        IStorageProvider storageProvider,
        FilePickerSaveOptions options,
        IXdgDesktopPortal portal,
        Func<bool> isLinux)
    {
        if (!isLinux())
        {
            return await storageProvider.SaveFilePickerAsync(options);
        }

        XdgFileChooserResult result = await portal.ShowFileChooserAsync(new XdgFileChooserRequest(
            options.Title ?? string.Empty,
            Directory: false,
            Multiple: false,
            Save: true,
            options.SuggestedStartLocation?.TryGetLocalPath(),
            options.SuggestedFileName,
            CreateFilters(options.FileTypeChoices)));
        if (!result.Accepted || result.Uris.FirstOrDefault(uri => uri.IsFile) is not { } selectedUri)
        {
            return null;
        }

        string selectedPath = selectedUri.LocalPath;
        if (!File.Exists(selectedPath))
        {
            // Avalonia cannot materialize a non-existent portal result as IStorageFile.
            using FileStream stream = File.Open(selectedPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
        }

        return await storageProvider.TryGetFileFromPathAsync(selectedPath);
    }

    private static XdgFileChooserRequest CreateRequest(FilePickerOpenOptions options)
        => new(
            options.Title ?? string.Empty,
            Directory: false,
            options.AllowMultiple,
            Save: false,
            options.SuggestedStartLocation?.TryGetLocalPath(),
            SuggestedFileName: null,
            CreateFilters(options.FileTypeFilter));

    private static IReadOnlyList<XdgFileChooserFilter> CreateFilters(IReadOnlyList<FilePickerFileType>? fileTypes)
        => fileTypes is null
            ? []
            : [.. fileTypes.Select(fileType => new XdgFileChooserFilter(
                fileType.Name,
                fileType.Patterns ?? [],
                fileType.MimeTypes ?? []))];

    private static void ReportUnavailable()
    {
        _ = ShimHost.MessageBoxHost.Show(
            owner: null,
            "A file dialog could not be opened because no XDG desktop portal backend is available.",
            ErrorCaption,
            MessageBoxButtons.OK,
            MessageBoxIcon.Error,
            MessageBoxDefaultButton.Button1);
    }
}
