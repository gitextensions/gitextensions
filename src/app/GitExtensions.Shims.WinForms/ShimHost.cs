namespace GitExtensions.Shims.WinForms;

/// <summary>
///  Registry for the platform services behind the WinForms stand-in types.
///  The Avalonia application installs implementations at startup; shared-tier code never
///  accesses this type directly.
/// </summary>
public static class ShimHost
{
    private static IMessageBoxHost? _messageBoxHost;

    /// <summary>
    ///  Gets or sets the handler that displays message boxes for <see cref="MessageBox"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">No host has been installed.</exception>
    public static IMessageBoxHost MessageBoxHost
    {
        get => _messageBoxHost ?? throw new InvalidOperationException($"No {nameof(IMessageBoxHost)} has been installed. The application must assign {nameof(ShimHost)}.{nameof(MessageBoxHost)} at startup.");
        set => _messageBoxHost = value;
    }

    private static TaskDialogs.ITaskDialogHost? _taskDialogHost;

    /// <summary>
    ///  Gets or sets the handler that displays custom-button task dialogs for
    ///  <see cref="TaskDialogs.TaskDialog"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">No host has been installed.</exception>
    public static TaskDialogs.ITaskDialogHost TaskDialogHost
    {
        get => _taskDialogHost ?? throw new InvalidOperationException($"No {nameof(TaskDialogs.ITaskDialogHost)} has been installed. The application must assign {nameof(ShimHost)}.{nameof(TaskDialogHost)} at startup.");
        set => _taskDialogHost = value;
    }

    /// <summary>
    ///  Gets or sets the provider of the currently active form, used by <see cref="Form.ActiveForm"/>.
    ///  <see langword="null"/> (the default) means there is no active form.
    /// </summary>
    public static Func<Form?>? ActiveFormProvider { get; set; }

    private static IClipboard? _clipboard;

    /// <summary>
    ///  Gets or sets the clipboard service behind <see cref="WinForms.Clipboard"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">No service has been installed.</exception>
    public static IClipboard Clipboard
    {
        get => _clipboard ?? throw new InvalidOperationException($"No {nameof(IClipboard)} has been installed. The application must assign {nameof(ShimHost)}.{nameof(Clipboard)} at startup.");
        set => _clipboard = value;
    }

    private static IFolderPicker? _folderPicker;

    /// <summary>
    ///  Gets or sets the folder-selection service behind <see cref="FolderBrowserDialog"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">No service has been installed.</exception>
    public static IFolderPicker FolderPicker
    {
        get => _folderPicker ?? throw new InvalidOperationException($"No {nameof(IFolderPicker)} has been installed. The application must assign {nameof(ShimHost)}.{nameof(FolderPicker)} at startup.");
        set => _folderPicker = value;
    }

    /// <summary>
    ///  Gets or sets the text-measurement service behind <see cref="TextRenderer"/>.
    ///  The default is a documented approximation (average character width) so that headless
    ///  code paths work without a UI; the application installs real text shaping at startup.
    /// </summary>
    public static ITextMeasurer TextMeasurer { get; set; } = new ApproximateTextMeasurer();

    private sealed class ApproximateTextMeasurer : ITextMeasurer
    {
        // Rough average glyph-width-to-em ratio for proportional UI fonts; only used until
        // the application installs a real measurer, and only for non-critical layout hints.
        private const float AverageGlyphWidthFactor = 0.6f;

        public Size MeasureText(string? text, Font? font)
        {
            float fontSize = font?.Size ?? 9F;
            int width = (int)Math.Ceiling((text?.Length ?? 0) * fontSize * AverageGlyphWidthFactor);
            int height = (int)Math.Ceiling(fontSize * 4 / 3);
            return new Size(width, height);
        }
    }
}
