using System;

namespace GitCommands.Settings
{
    public class StringSetting : Setting<string>
    {
        public StringSetting(string name, SettingsPath settingsSource, string defaultValue)
            : base(name, settingsSource, defaultValue)
        {
        }

        public override string Value
        {
            get => SettingsSource.GetString(Name, null);
            set => SettingsSource.SetString(Name, value);
        }
    }

    public class BoolNullableSetting : Setting<bool?>
    {
        public BoolNullableSetting(string name, SettingsPath settingsSource, bool defaultValue)
            : base(name, settingsSource, defaultValue)
        {
        }

        public override bool? Value
        {
            get => SettingsSource.GetBool(Name);
            set => SettingsSource.SetBool(Name, value);
        }

        public new bool ValueOrDefault => base.ValueOrDefault.Value;
    }

    public class IntNullableSetting : Setting<int?>
    {
        public IntNullableSetting(string name, SettingsPath settingsSource, int defaultValue)
            : base(name, settingsSource, defaultValue)
        {
        }

        public override int? Value
        {
            get => SettingsSource.GetInt(Name);
            set => SettingsSource.SetInt(Name, value);
        }

        public new int ValueOrDefault => base.ValueOrDefault.Value;
    }

    public class BoolSetting : Setting<bool>
    {
        public BoolSetting(string name, SettingsPath settingsSource, bool defaultValue)
            : base(name, settingsSource, defaultValue)
        {
        }

        public override bool Value
        {
            get => SettingsSource.GetBool(Name, DefaultValue);
            set => SettingsSource.SetBool(Name, value);
        }
    }

    public class EnumSetting<T> : Setting<T> where T : struct, Enum
    {
        public EnumSetting(string name, SettingsPath settingsSource, T defaultValue)
            : base(name, settingsSource, defaultValue)
        {
        }

        public override T Value
        {
            get => SettingsSource.GetEnum(Name, DefaultValue);
            set => SettingsSource.SetEnum(Name, value);
        }
    }

    public class EnumNullableSetting<T> : Setting<T?> where T : struct, Enum
    {
        public EnumNullableSetting(string name, SettingsPath settingsSource)
            : base(name, settingsSource, null)
        {
        }

        public override T? Value
        {
            get => SettingsSource.GetNullableEnum<T>(Name);
            set => SettingsSource.SetNullableEnum(Name, value);
        }
    }
}
