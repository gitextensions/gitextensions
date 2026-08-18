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
// before it is saved, so it is snapped to the nearest supported size; an item that names nothing
// resolvable, or whose toolbar no longer exists, is unreachable either way, so it is dropped.
internal static class ToolbarLayoutValidator
{
    public const int MinIconSize = 16;
    public const int MaxIconSize = 72;

    public const int MaxRow = 32;
    public const int MaxOrderInRow = 256;

    // Index is a position among the toolbars, so MaxToolbars would do; the looser bound leaves
    // room for a layout whose indices are sparse without making it any less unique.
    public const int MaxIndex = 65_536;

    public const int MaxToolbars = 64;
    public const int MaxToolbarNameLength = 128;

    // Generous enough that filling every toolbar cannot reach it, since an action may be cloned
    // onto several toolbars at once.
    public const int MaxItems = 16_384;

    private const int MaxOrder = 4096;

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

        // Index orders the custom toolbars on load, so two of them claiming the same value leaves
        // it undecided which comes first. Every writer derives it from a position - in the settings
        // page's toolbar list, or one past the highest already in use - so a duplicate can only
        // come from outside this application.
        HashSet<string> customNames = new(StringComparer.OrdinalIgnoreCase);
        HashSet<int> customIndices = [];
        foreach (ToolbarCustomMetadata? meta in config.CustomToolbars)
        {
            if (meta is null
                || !IsValidToolbarName(meta.Name)
                || !IsInRange(meta.Row, MaxRow)
                || !IsInRange(meta.OrderInRow, MaxOrderInRow)
                || !IsInRange(meta.Index, MaxIndex)
                || !customNames.Add(meta.Name)
                || !customIndices.Add(meta.Index))
            {
                return false;
            }

            meta.IconSize = NormalizeIconSize(meta.IconSize);
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
                || !IsValidToolbarName(item.ToolbarName)
                || !IsInRange(item.Order, MaxOrder))
            {
                return false;
            }
        }

        // An item is dropped rather than taken as a reason to discard the layout around it, in the
        // two cases where it is merely unreachable instead of ambiguous, both of which are easy to
        // end up with:
        //
        // - its toolbar is gone, so nothing could place it;
        // - its name is not one this application could have written, so nothing could resolve it.
        //   The loader already skips a name it cannot resolve; checking the name here is what keeps
        //   a malformed one - an unbounded label, a label whose text hides control characters
        //   behind percent-escapes - from reaching the ToolStrip in the first place.
        //
        // Callers therefore only ever see items that name something and sit on a toolbar that exists.
        config.Items.RemoveAll(item => !ToolbarItemNames.IsValid(item.ItemName)
                                    || !knownToolbars.Contains(item.ToolbarName));

        return true;
    }

    private static bool IsValidToolbarName([NotNullWhen(true)] string? name)
        => !string.IsNullOrWhiteSpace(name)
        && name.Length <= MaxToolbarNameLength
        && !name.Any(char.IsControl);

    private static bool IsInRange(int value, int max)
        => value >= 0 && value <= max;
}
