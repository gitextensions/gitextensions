using GitUIPluginInterfaces;

namespace GitCommands.Settings
{

    public class StringSetting : Setting<string>
    {
        public StringSetting(string aName, SettingsPath settingsSource, string aDefaultValue)
            : base(aName, settingsSource, aDefaultValue)
        {
        }

        public override string Value
        {
            get
            {
                return SettingsSource.GetString(Name, DefaultValue);
            }

            set
            {
                SettingsSource.SetString(Name, value);
            }
        }
    }

    public class BoolNullableSetting : Setting<bool?>
    {
        public BoolNullableSetting(string aName, SettingsPath settingsSource, bool aDefaultValue)
            : base(aName, settingsSource, aDefaultValue)
        { }

        public override bool? Value
        {
            get
            {
                return SettingsSource.GetBool(Name);
            }

            set
            {
                SettingsSource.SetBool(Name, value);
            }
        }

        public bool ValueOrDefault
        {
            get
            {
                if (Value.HasValue)
                    return Value.Value;

                return DefaultValue.Value;
            }
        }

    }

    public class BoolSetting : Setting<bool>
    {
        public BoolSetting(string aName, SettingsPath settingsSource, bool aDefaultValue)
            : base(aName, settingsSource, aDefaultValue)
        { }

        public override bool Value
        {
            get
            {
                return SettingsSource.GetBool(Name, DefaultValue);
            }

            set
            {
                SettingsSource.SetBool(Name, value);
            }
        }
    }

    public class EnumSetting<T> : Setting<T> where T : struct
    {
        public EnumSetting(string aName, SettingsPath settingsSource, T aDefaultValue)
            : base(aName, settingsSource, aDefaultValue)
        { }

        public override T Value
        {
            get
            {
                return SettingsSource.GetEnum(Name, DefaultValue);
            }

            set
            {
                SettingsSource.SetEnum(Name, value);
            }
        }
    }

    public class EnumNullableSetting<T> : Setting<T?> where T : struct
    {
        public EnumNullableSetting(string aName, SettingsPath settingsSource)
            : base(aName, settingsSource, null)
        { }

        public override T? Value
        {
            get
            {
                return SettingsSource.GetNullableEnum<T>(Name);
            }

            set
            {
                SettingsSource.SetNullableEnum(Name, value);
            }
        }
    }
}
