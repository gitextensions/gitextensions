using System.Globalization;

namespace GitCommands.Settings;

/// <summary>
///  Concrete implementation of an <see cref="ISetting{T}"/> instance for <see langword="string"/>.
/// </summary>
public sealed class Setting_String : Setting<string>
{
    public Setting_String(SettingsPath settingsSource, string name, string defaultValue)
        : base(
            settingsSource,
            name,
            defaultValue,
            read: static s => (true, s),
            store: static v => v)
    {
    }
}

/// <summary>
///  Concrete implementation of an <see cref="ISetting{T}"/> instance for nullable <see langword="string"/>.
/// </summary>
public sealed class Setting_NullableString : Setting<string?>
{
    public Setting_NullableString(SettingsPath settingsSource, string name)
        : base(
            settingsSource,
            name,
            defaultValue: null,
            read: static s => (true, s),
            store: static v => v)
    {
    }
}

/// <summary>
///  Concrete implementation of an <see cref="ISetting{T}"/> instance for <see langword="bool"/>.
/// </summary>
public sealed class Setting_Bool : Setting<bool>
{
    public Setting_Bool(SettingsPath settingsSource, string name, bool defaultValue)
        : base(
            settingsSource,
            name,
            defaultValue,
            read: static s => s switch { "true" or "True" => (true, true), "false" or "False" => (true, false), _ => (false, default) },
            store: static v => v ? "true" : "false")
    {
    }
}

/// <summary>
///  Concrete implementation of an <see cref="ISetting{T}"/> instance for <see langword="bool"/>
///  which is stored inverted for backwards compatibility with historical settings files.
/// </summary>
public sealed class Setting_InvertedBool : Setting<bool>
{
    public Setting_InvertedBool(SettingsPath settingsSource, string name, bool defaultValue)
        : base(
            settingsSource,
            name,
            defaultValue,
            read: static s => s switch { "true" or "True" => (true, false), "false" or "False" => (true, true), _ => (false, default) },
            store: static v => v ? "false" : "true")
    {
    }
}

/// <summary>
///  Concrete implementation of an <see cref="ISetting{T}"/> instance for <see langword="bool?"/>.
/// </summary>
public sealed class Setting_NullableBool : Setting<bool?>
{
    public Setting_NullableBool(SettingsPath settingsSource, string name)
        : base(
            settingsSource,
            name,
            defaultValue: null,
            read: static s => s switch { "true" or "True" => (true, true), "false" or "False" => (true, false), _ => (false, default) },
            store: static v => v switch { true => "true", false => "false", _ => null })
    {
    }
}

/// <summary>
///  Concrete implementation of an <see cref="ISetting{T}"/> instance for <see langword="int"/>.
/// </summary>
public sealed class Setting_Int : Setting<int>
{
    public Setting_Int(SettingsPath settingsSource, string name, int defaultValue)
        : base(
            settingsSource,
            name,
            defaultValue,
            read: static s => int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) ? (true, v) : (false, default),
            store: static v => v.ToString(CultureInfo.InvariantCulture))
    {
    }
}

/// <summary>
///  Concrete implementation of an <see cref="ISetting{T}"/> instance for <see langword="float"/>.
/// </summary>
public sealed class Setting_Float : Setting<float>
{
    public Setting_Float(SettingsPath settingsSource, string name, float defaultValue)
        : base(
            settingsSource,
            name,
            defaultValue,
            read: static s => float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float v) ? (true, v) : (false, default),
            store: static v => v.ToString(CultureInfo.InvariantCulture))
    {
    }
}

/// <summary>
///  Concrete implementation of an <see cref="ISetting{T}"/> instance for <see langword="enum"/> types.
/// </summary>
/// <typeparam name="TEnum">The enum type.</typeparam>
public sealed class Setting_Enum<TEnum> : Setting<TEnum>
    where TEnum : struct, Enum
{
    public Setting_Enum(SettingsPath settingsSource, string name, TEnum defaultValue)
        : base(
            settingsSource,
            name,
            defaultValue,
            read: static s => Enum.TryParse(s, out TEnum v) ? (true, v) : (false, default),
            store: static v => v.ToString())
    {
    }
}

/// <summary>
///  Concrete implementation of an <see cref="ISetting{T}"/> instance for nullable <see langword="enum"/> types.
/// </summary>
/// <typeparam name="TEnum">The enum type.</typeparam>
public sealed class Setting_NullableEnum<TEnum> : Setting<TEnum?>
    where TEnum : struct, Enum
{
    public Setting_NullableEnum(SettingsPath settingsSource, string name)
        : base(
            settingsSource,
            name,
            defaultValue: null,
            read: static s => Enum.TryParse(s, out TEnum v) ? (true, v) : (false, default),
            store: static v => v?.ToString())
    {
    }
}
