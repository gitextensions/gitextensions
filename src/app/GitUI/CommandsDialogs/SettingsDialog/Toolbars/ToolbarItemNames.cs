using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace GitUI.CommandsDialogs.SettingsDialog.Toolbars;

// The grammar of ToolbarItemConfig.ItemName, which is the only handle the loader has on an item.
//
// A name is either the Name of a live ToolStripItem, or one of three placeholders standing for an
// item that has no live counterpart to point at:
//
//   _SEPARATOR_{order}          a separator
//   _SPACER_{order}             an expanding spacer
//   _LABEL_{escapedText}_{order} a free-text label, its text URI-escaped so that the name stays
//                               a single token whatever the user typed
//
// Both writers (FormBrowse when it snapshots the live toolbars, and the Toolbars settings page)
// produce names through this type, the loader takes them apart through it, and
// ToolbarLayoutValidator checks a restored layout against it, so the three cannot drift apart.
internal static partial class ToolbarItemNames
{
    public const string SeparatorPrefix = "_SEPARATOR_";
    public const string SpacerPrefix = "_SPACER_";
    public const string LabelPrefix = "_LABEL_";

    // A label is free text typed by the user, so it is the one part of a name that is not derived
    // from a control name. The "Add Label" dialog is bounded to the same value, so a label the user
    // can enter always survives the round trip through the settings file.
    public const int MaxLabelTextLength = 128;

    // Outer bound on a stored name. A label carries its text URI-escaped, which costs up to twelve
    // characters per non-ASCII one, so this sits well above MaxLabelTextLength.
    private const int MaxLength = 2048;

    public static string Separator(int order) => $"{SeparatorPrefix}{order}";

    public static string Spacer(int order) => $"{SpacerPrefix}{order}";

    /// <summary>
    /// Builds the stored name of a free-text label. The text is truncated to
    /// <see cref="MaxLabelTextLength"/> so that this never produces a name a restored layout
    /// would have to reject.
    /// </summary>
    public static string Label(string? text, int order)
    {
        string bounded = text ?? string.Empty;
        if (bounded.Length > MaxLabelTextLength)
        {
            bounded = bounded[..MaxLabelTextLength];
        }

        return $"{LabelPrefix}{Uri.EscapeDataString(bounded)}_{order}";
    }

    /// <summary>
    /// Whether <paramref name="name"/> stands for a separator, a spacer or a label rather than for
    /// a live item to resolve by name. Note that this is not the same as "starts with an
    /// underscore": several real menu items are named that way (e.g. _viewPullRequestsToolStripMenuItem).
    /// </summary>
    public static bool IsPlaceholder(string name)
        => IsSeparator(name) || IsSpacer(name) || IsLabel(name);

    public static bool IsSeparator(string name) => name.StartsWith(SeparatorPrefix, StringComparison.Ordinal);

    public static bool IsSpacer(string name) => name.StartsWith(SpacerPrefix, StringComparison.Ordinal);

    public static bool IsLabel(string name) => name.StartsWith(LabelPrefix, StringComparison.Ordinal);

    /// <summary>
    /// Recovers the text of a label name. Returns <see langword="false"/> when
    /// <paramref name="name"/> does not follow the label grammar, or when its text is unbounded or
    /// carries control characters - which percent-escaping would otherwise hide from a plain
    /// inspection of the name.
    /// </summary>
    public static bool TryParseLabel(string name, [NotNullWhen(true)] out string? text)
    {
        text = null;

        Match match = LabelRegex().Match(name);
        if (!match.Success)
        {
            return false;
        }

        string decoded = Uri.UnescapeDataString(match.Groups["text"].Value);
        if (decoded.Length > MaxLabelTextLength || decoded.Any(char.IsControl))
        {
            return false;
        }

        text = decoded;
        return true;
    }

    /// <summary>
    /// Whether <paramref name="name"/> is a name this application could have written. A name that
    /// starts with a placeholder prefix must follow that placeholder's grammar; anything else is
    /// read as the Name of a live item, which is always a control name.
    /// </summary>
    public static bool IsValid([NotNullWhen(true)] string? name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > MaxLength)
        {
            return false;
        }

        if (IsLabel(name))
        {
            return TryParseLabel(name, out _);
        }

        if (IsSeparator(name) || IsSpacer(name))
        {
            return IndexedPlaceholderRegex().IsMatch(name);
        }

        return ItemIdRegex().IsMatch(name);
    }

    // A control name, as the designer and the runtime item factories produce it. Leading
    // underscores are allowed: the main menu has several such items.
    [GeneratedRegex(@"^[A-Za-z_][A-Za-z0-9_.\-]*$", RegexOptions.CultureInvariant)]
    private static partial Regex ItemIdRegex();

    [GeneratedRegex(@"^(?:_SEPARATOR_|_SPACER_)[0-9]{1,9}$", RegexOptions.CultureInvariant)]
    private static partial Regex IndexedPlaceholderRegex();

    // The text is whatever Uri.EscapeDataString emits: the RFC 3986 unreserved characters, plus
    // percent-escapes for everything else. The order suffix is separated by the last underscore,
    // which is unambiguous because an unreserved underscore can only appear inside the text.
    [GeneratedRegex(@"^_LABEL_(?<text>(?:[A-Za-z0-9\-._~]|%[0-9A-Fa-f]{2})*)_[0-9]{1,9}$", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture)]
    private static partial Regex LabelRegex();
}
