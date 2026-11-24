using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using IShimClipboard = GitExtensions.Shims.WinForms.IClipboard;

namespace GitUI.Compat;

/// <summary>
///  Implements the shim <see cref="IShimClipboard"/> over the main window's Avalonia clipboard.
/// </summary>
public sealed class AvaloniaClipboard(IClassicDesktopStyleApplicationLifetime desktop) : IShimClipboard
{
    private IClipboard? PlatformClipboard => desktop.MainWindow?.Clipboard;

    public void SetText(string text)
        => DispatcherPump.Wait(async () =>
        {
            if (PlatformClipboard is { } clipboard)
            {
                await clipboard.SetTextAsync(text);
            }

            return true;
        });

    public string GetText()
        => DispatcherPump.Wait(async () =>
        {
            if (PlatformClipboard is { } clipboard)
            {
                return await clipboard.TryGetTextAsync() ?? string.Empty;
            }

            return string.Empty;
        });

    public bool ContainsText()
        => GetText().Length > 0;
}
