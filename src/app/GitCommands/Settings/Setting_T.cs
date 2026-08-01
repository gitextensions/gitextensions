using System.Globalization;

namespace GitCommands.Settings;

/// <summary>
///  Concrete implementation of an <see cref="ISetting{T}"/> instance for <see langword="string"/>.
/// </summary>
public sealed class TextSetting : Setting<string>
{
    public TextSetting(SettingsPath settingsSource, string name, string defaultValue)
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
public sealed class NullableTextSetting : Setting<string?>
{
    public NullableTextSetting(SettingsPath settingsSource, string name)
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
public sealed class BooleanSetting : Setting<bool>
{
    public BooleanSetting(SettingsPath settingsSource, string name, bool defaultValue)
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
public sealed class InvertedBooleanSetting : Setting<bool>
{
    public InvertedBooleanSetting(SettingsPath settingsSource, string name, bool defaultValue)
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
public sealed class NullableBooleanSetting : Setting<bool?>
{
    public NullableBooleanSetting(SettingsPath settingsSource, string name)
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
public sealed class IntegerSetting : Setting<int>
{
    public IntegerSetting(SettingsPath settingsSource, string name, int defaultValue)
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
public sealed class FloatSetting : Setting<float>
{
    public FloatSetting(SettingsPath settingsSource, string name, float defaultValue)
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
public sealed class EnumSetting<TEnum> : Setting<TEnum>
    where TEnum : struct, Enum
{
    public EnumSetting(SettingsPath settingsSource, string name, TEnum defaultValue)
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
