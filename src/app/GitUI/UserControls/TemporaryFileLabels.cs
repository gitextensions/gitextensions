using System.Drawing;
using System.Windows.Forms;

namespace GitUI.UserControls;

/// <summary>
///  Temporary per-file labels for a diff review session, as requested in
///  https://github.com/gitextensions/gitextensions/issues/13205.
/// </summary>
/// <remarks>
///  Labels live only as long as the owning <see cref="FileStatusList"/> instance
///  (for example a <c>FormDiff</c> window) and are intentionally never persisted:
///  they mark which files of the current diff have been reviewed, in which order
///  or with which outcome. A label is identified by the file path, so it survives
///  list rebuilds (refresh, filter, regrouping) within the same session.
/// </remarks>
internal sealed class TemporaryFileLabels
{
    /// <summary>
    ///  No label; also the hotkey that clears the label of the selected files.
    /// </summary>
    public const int NoLabel = 0;

    /// <summary>
    ///  The highest assignable label, following the hotkeys 1-9 from the issue.
    /// </summary>
    public const int MaxLabel = 9;

    // Fixed palette, readable on both light and dark backgrounds.
    private static readonly Color[] _labelColors =
    [
        Color.FromArgb(0xD3, 0x2F, 0x2F),
        Color.FromArgb(0xF5, 0x7C, 0x00),
        Color.FromArgb(0x38, 0x8E, 0x3C),
        Color.FromArgb(0x00, 0x89, 0x7B),
        Color.FromArgb(0x19, 0x76, 0xD2),
        Color.FromArgb(0x7B, 0x1F, 0xA2),
        Color.FromArgb(0x6D, 0x4C, 0x41),
        Color.FromArgb(0xC2, 0x18, 0x5B),
        Color.FromArgb(0x54, 0x6E, 0x7A),
    ];

    private readonly Dictionary<string, int> _labels = new(StringComparer.Ordinal);

    /// <summary>
    ///  Gets the number of labelled files in this session.
    /// </summary>
    public int Count => _labels.Count;

    /// <summary>
    ///  Gets the display color of the given label.
    /// </summary>
    /// <param name="label">The label, 1 to <see cref="MaxLabel"/>.</param>
    public static Color GetColor(int label)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(label, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(label, MaxLabel);

        return _labelColors[label - 1];
    }

    /// <summary>
    ///  Formats the marker shown in front of a labelled file, for example "[1] ".
    /// </summary>
    /// <param name="label">The label, 1 to <see cref="MaxLabel"/>.</param>
    public static string Format(int label)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(label, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(label, MaxLabel);

        return $"[{label}] ";
    }

    /// <summary>
    ///  Maps a plain digit key press to a label: 1-9 assign that label, 0 clears it.
    ///  Keys pressed with Ctrl, Alt or Shift are not hotkeys, so typing into the
    ///  filter boxes keeps working.
    /// </summary>
    /// <param name="keyData">The pressed key, including modifiers.</param>
    /// <param name="label">The label, 0 to <see cref="MaxLabel"/>.</param>
    /// <returns><c>true</c> if <paramref name="keyData"/> is a label hotkey.</returns>
    public static bool TryParseHotkey(Keys keyData, out int label)
    {
        label = keyData switch
        {
            Keys.D0 or Keys.NumPad0 => NoLabel,
            Keys.D1 or Keys.NumPad1 => 1,
            Keys.D2 or Keys.NumPad2 => 2,
            Keys.D3 or Keys.NumPad3 => 3,
            Keys.D4 or Keys.NumPad4 => 4,
            Keys.D5 or Keys.NumPad5 => 5,
            Keys.D6 or Keys.NumPad6 => 6,
            Keys.D7 or Keys.NumPad7 => 7,
            Keys.D8 or Keys.NumPad8 => 8,
            Keys.D9 or Keys.NumPad9 => 9,
            _ => -1,
        };

        return label >= NoLabel;
    }

    /// <summary>
    ///  Assigns <paramref name="label"/> to the file at <paramref name="filePath"/>.
    /// </summary>
    /// <param name="filePath">The path of the file, as in <c>GitItemStatus.Name</c>.</param>
    /// <param name="label">The label, 1 to <see cref="MaxLabel"/>.</param>
    /// <returns><c>true</c> if the stored label changed.</returns>
    public bool TryAssign(string filePath, int label)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(label, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(label, MaxLabel);

        if (_labels.TryGetValue(filePath, out int existing) && existing == label)
        {
            return false;
        }

        _labels[filePath] = label;
        return true;
    }

    /// <summary>
    ///  Removes the label of the file at <paramref name="filePath"/>, if any.
    /// </summary>
    /// <param name="filePath">The path of the file, as in <c>GitItemStatus.Name</c>.</param>
    /// <returns><c>true</c> if a label was removed.</returns>
    public bool Clear(string filePath)
    {
        return _labels.Remove(filePath);
    }

    /// <summary>
    ///  Removes all labels of this session.
    /// </summary>
    public void ClearAll()
    {
        _labels.Clear();
    }

    /// <summary>
    ///  Gets the label of the file at <paramref name="filePath"/>, if any.
    /// </summary>
    /// <param name="filePath">The path of the file, as in <c>GitItemStatus.Name</c>.</param>
    /// <returns>The label, 1 to <see cref="MaxLabel"/>, or <c>null</c> when the file has no label.</returns>
    public int? GetLabel(string filePath)
    {
        return _labels.TryGetValue(filePath, out int label) ? label : null;
    }
}
