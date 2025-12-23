namespace GitExtUtils.GitUI.Theming;

public readonly struct ThemeId
{
    private static readonly string DefaultDarkThemeName = new("dark");
    private static readonly string DefaultLightThemeName = new("light");
    private static readonly string WindowsAppColorModeName = new("Windows app color mode");

    /// <summary>
    /// The filename for the default invariant theme, identifying as "light".
    /// </summary>
    public static string InvariantThemeFileName { get; } = "invariant";

    // DefaultLight is the default invariant theme
    public static ThemeId DefaultDark { get; } = new(DefaultDarkThemeName, isBuiltin: true);
    public static ThemeId DefaultLight { get; } = new(DefaultLightThemeName, isBuiltin: true);
    public static ThemeId WindowsAppColorModeId { get; } = new(WindowsAppColorModeName, isBuiltin: true);

    public string Name { get; }
    public bool IsBuiltin { get; }

    /// <summary>
    /// Get the default ThemeId for the current Windows SystemColorMode
    /// </summary>
    public static ThemeId ColorModeThemeId
        => Application.SystemColorMode == SystemColorMode.Dark
            ? ThemeId.DefaultDark
            : ThemeId.DefaultLight;

    public ThemeId(string name, bool isBuiltin = false)
    {
        Name = string.IsNullOrWhiteSpace(name) || (isBuiltin && name.Equals(InvariantThemeFileName, StringComparison.OrdinalIgnoreCase))
            ? DefaultLightThemeName
            : name;
        IsBuiltin = isBuiltin;
    }

    public override bool Equals(object obj) =>
        obj is ThemeId other && Equals(other);

    public override int GetHashCode()
    {
        // Name can be null because of default struct constructor
        int nameHashCode = Name is null
            ? 0
            : StringComparer.OrdinalIgnoreCase.GetHashCode(Name);
        return nameHashCode ^ IsBuiltin.GetHashCode();
    }

    public static bool operator ==(ThemeId left, ThemeId right) =>
        left.Equals(right);

    public static bool operator !=(ThemeId left, ThemeId right) =>
        !left.Equals(right);

    private bool Equals(ThemeId other) =>
        string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase) &&
        IsBuiltin == other.IsBuiltin;

    public override string ToString() => Name;
}
