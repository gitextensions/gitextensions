namespace GitExtensions.Shims.WinForms;

/// <summary>
///  Stand-in for <c>System.Windows.Forms.Clipboard</c>: routes to the <see cref="IClipboard"/>
///  service installed in <see cref="ShimHost"/>.
/// </summary>
/// <remarks>
///  Consumed by: <c>GitExtUtils/ClipboardUtil.cs</c>.
/// </remarks>
public static class Clipboard
{
    /// <summary>
    ///  Places text on the clipboard.
    /// </summary>
    public static void SetText(string text) => ShimHost.Clipboard.SetText(text);

    /// <summary>
    ///  Returns the clipboard text, or an empty string when the clipboard holds no text.
    /// </summary>
    public static string GetText() => ShimHost.Clipboard.GetText();

    /// <summary>
    ///  Returns whether the clipboard currently holds text.
    /// </summary>
    public static bool ContainsText() => ShimHost.Clipboard.ContainsText();

    /// <summary>
    ///  Places string data on the clipboard. The retry parameters exist for signature
    ///  compatibility with WinForms; non-Win32 clipboards have no lock contention, so a single
    ///  attempt is made.
    /// </summary>
    /// <exception cref="NotSupportedException"><paramref name="data"/> is not a string.</exception>
    public static void SetDataObject(object data, bool copy, int retryTimes, int retryDelay)
    {
        if (data is not string text)
        {
            throw new NotSupportedException($"{nameof(Clipboard)}.{nameof(SetDataObject)} supports string data only; got {data?.GetType().Name ?? "null"}.");
        }

        ShimHost.Clipboard.SetText(text);
    }
}
