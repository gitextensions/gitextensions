using Avalonia.Controls;
using Avalonia.Platform.Storage;
using GitExtensions.Shims.WinForms;
using GitUI.Compat;
using GitUI.Infrastructure;

namespace GitUI;

/// <summary>
/// Shows a dialog to let the user browse for a SSH key.
/// </summary>
public static class BrowseForPrivateKey
{
    /// <summary>
    /// Prompts the user to browse for a key, and attempts to load it. Returns the path to the key, if successful.
    /// </summary>
    public static string? BrowseAndLoad(IWin32Window parent)
    {
        string? sshKeyFile = Browse(parent);
        if (!string.IsNullOrEmpty(sshKeyFile))
        {
            if (PuttyHelpers.StartPageantIfConfigured(() => sshKeyFile))
            {
                return sshKeyFile;
            }
        }

        return null;
    }

    /// <summary>
    /// Prompts the user to browse for a key. Returns the path chosen, or null.
    /// </summary>
    public static string? Browse(IWin32Window parent)
    {
        // Avalonia exposes the picker asynchronously through the owning top level.
        return DispatcherPump.Wait(async () =>
        {
            if (parent is not TopLevel owner || !await PortalPickerGuard.IsAvailableAsync())
            {
                return null;
            }

            FilePickerOpenOptions options = new()
            {
                AllowMultiple = false,
                FileTypeFilter = [new FilePickerFileType(" (*.ppk)") { Patterns = ["*.ppk"] }],
                SuggestedStartLocation = await owner.StorageProvider.TryGetFolderFromPathAsync(Path.GetFullPath(".")),
                Title = "Browse for key",
            };
            IReadOnlyList<IStorageFile> files = await PortalPickerGuard.OpenFilePickerAsync(owner.StorageProvider, options);
            return files.FirstOrDefault()?.TryGetLocalPath();
        });
    }
}
