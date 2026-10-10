namespace GitExtensions.Shims.WinForms;

/// <summary>
///  Stand-in for <c>System.Drawing.FontStyle</c>; values match GDI+.
/// </summary>
/// <remarks>
///  A value type rather than an enum keeps the WinForms-compatible <see cref="Regular"/>
///  member name while exposing the same flag-combination behavior to portable shared code.
/// </remarks>
public readonly record struct FontStyle
{
    private readonly int _value;

    private FontStyle(int value)
    {
        _value = value;
    }

    public static FontStyle Regular { get; } = new(0);

    public static FontStyle Bold { get; } = new(1);

    public static FontStyle Italic { get; } = new(2);

    public static FontStyle Underline { get; } = new(4);

    public static FontStyle Strikeout { get; } = new(8);

    public static FontStyle operator |(FontStyle left, FontStyle right)
        => new(left._value | right._value);

    public bool HasFlag(FontStyle flag)
        => (_value & flag._value) == flag._value;
}
