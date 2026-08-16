using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace GitUI.CommandsDialogs.SettingsDialog.Toolbars;

// Validates a layout restored from the settings file, which the user or any other process can edit.
//
// Nothing in a deserialized graph is trustworthy. DataContractSerializer assigns members directly,
// so it can produce null collections (through xsi:nil or a simply absent element), null entries and
// out-of-range numbers no matter what the property initializers and non-null annotations on the
// model say. Left unchecked, those values reach WinForms while the main window is being built: a
// row index selects a toolbar panel row, and an icon size becomes a Size and a scaled Font.
//
// An ambiguous payload - one where it is unclear which value was meant, such as a duplicate
// toolbar name or a number outside its range - is discarded whole, so a caller either gets a
// layout satisfying every invariant below or the default one. What is merely recoverable is
// normalized instead, because discarding a user's entire layout over it would be out of
// proportion: an icon size is cosmetic and WinForms DPI-scales a live ToolStrip.ImageScalingSize
// before it is saved, so it is snapped to the nearest supported size; an item whose toolbar no
// longer exists is unreachable either way, so it is dropped.
internal static class ToolbarLayoutValidator
{
    public const int MinIconSize = 16;
    public const int MaxIconSize = 72;

    public const int MaxRow = 32;
    public const int MaxOrderInRow = 256;

    // Index only orders the custom toolbars, and the settings page derives it from the digits in
    // a user-supplied name ("Custom 9999" yields 10001), so it is clamped rather than checked.
    public const int MaxIndex = 65_536;

    public const int MaxToolbars = 64;
    public const int MaxToolbarNameLength = 128;

    // Generous enough that filling every toolbar cannot reach it, since an action may be cloned
    // onto several toolbars at once.
    public const int MaxItems = 16_384;

    private const int MaxOrder = 4096;

    // An item name embeds the URI-escaped text of an editable label, which costs up to nine
    // characters per accented one, so it needs far more headroom than a toolbar name
    // (see FormBrowse.GetItemSerializationName). The cap is only there to stop absurd strings.
    private const int MaxItemNameLength = 4096;

    // The supported icon sizes, and the ones the layout dialog offers.
    public static readonly ImmutableArray<int> IconSizes = [16, 20, 24, 28, 32, 36, 40, 44, 48, 52, 56, 60, 64, 68, 72];

    /// <summary>
    /// Clamps <paramref name="iconSize"/> into the supported range and snaps it to the nearest
    /// supported size, so a value read from a settings file or from a DPI-scaled ToolStrip can
    /// never produce an unusable toolbar.
    /// </summary>
    public static int NormalizeIconSize(int iconSize)
    {
        int clamped = Math.Clamp(iconSize, MinIconSize, MaxIconSize);
        return IconSizes.MinBy(size => Math.Abs(size - clamped));
    }

    /// <summary>
    /// Checks every invariant of a deserialized <paramref name="config"/>, snapping its icon sizes,
    /// clamping its custom toolbar indices and dropping items left over from a deleted toolbar.
    /// Returns <see langword="false"/> when the layout must be discarded in favour of the default.
    /// </summary>
    public static bool TryNormalize([NotNullWhen(true)] ToolbarLayoutConfig? config)
    {
        // A member can be absent or explicitly nil regardless of its initializer.
        if (config?.Items is null || config.CustomToolbars is null || config.ToolbarsVisibility is null)
        {
            return false;
        }

        if (config.Items.Count > MaxItems
            || config.CustomToolbars.Count > MaxToolbars
            || config.ToolbarsVisibility.Count > MaxToolbars)
        {
            return false;
        }

        // Toolbar names are the key linking the two metadata lists and the items, so a duplicate
        // would make the layout ambiguous rather than merely odd.
        HashSet<string> knownToolbars = new(StringComparer.OrdinalIgnoreCase);
        foreach (ToolbarBuiltInMetadata? meta in config.ToolbarsVisibility)
        {
            if (meta is null
                || !IsValidToolbarName(meta.Name)
                || !IsInRange(meta.Row, MaxRow)
                || !IsInRange(meta.OrderInRow, MaxOrderInRow)
                || !knownToolbars.Add(meta.Name))
            {
                return false;
            }

            meta.IconSize = NormalizeIconSize(meta.IconSize);
        }

        // Index only orders the custom toolbars on load, so it is normalized rather than enforced:
        // two of them sharing a value is harmless (OrderBy is stable) and does happen, since the
        // settings page derives the index from the name for "Custom NN" but from the combo box
        // position otherwise. The name remains the key that must be unique.
        HashSet<string> customNames = new(StringComparer.OrdinalIgnoreCase);
        foreach (ToolbarCustomMetadata? meta in config.CustomToolbars)
        {
            if (meta is null
                || !IsValidToolbarName(meta.Name)
                || !IsInRange(meta.Row, MaxRow)
                || !IsInRange(meta.OrderInRow, MaxOrderInRow)
                || !customNames.Add(meta.Name))
            {
                return false;
            }

            meta.IconSize = NormalizeIconSize(meta.IconSize);
            meta.Index = Math.Clamp(meta.Index, 0, MaxIndex);
        }

        // A custom toolbar must not shadow a built-in one: both would then claim the same key.
        if (customNames.Overlaps(ToolbarNames.BuiltIn))
        {
            return false;
        }

        knownToolbars.UnionWith(customNames);
        knownToolbars.UnionWith(ToolbarNames.BuiltIn);

        foreach (ToolbarItemConfig? item in config.Items)
        {
            if (item is null
                || !IsValidName(item.ItemName, MaxItemNameLength)
                || !IsValidToolbarName(item.ToolbarName)
                || !IsInRange(item.Order, MaxOrder))
            {
                return false;
            }
        }

        // An item whose toolbar is gone is unreachable rather than ambiguous, and a stale one is
        // easy to end up with, so drop it instead of discarding the entire layout with it. Callers
        // still only ever see items that belong to a toolbar that exists.
        config.Items.RemoveAll(item => !knownToolbars.Contains(item.ToolbarName));

        return true;
    }

    private static bool IsValidToolbarName([NotNullWhen(true)] string? name)
        => IsValidName(name, MaxToolbarNameLength);

    private static bool IsValidName([NotNullWhen(true)] string? name, int maxLength)
        => !string.IsNullOrWhiteSpace(name)
        && name.Length <= maxLength
        && !name.Any(char.IsControl);

    private static bool IsInRange(int value, int max)
        => value >= 0 && value <= max;
}
