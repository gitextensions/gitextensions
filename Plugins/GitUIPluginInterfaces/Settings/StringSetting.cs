using System;
using System.Windows.Forms;

namespace GitUIPluginInterfaces
{
    public class StringSetting : ISetting
    {
        public StringSetting(string name, string defaultValue)
            : this(name, name, defaultValue)
        {
        }

        public StringSetting(string name, string caption, string defaultValue)
        {
            Name = name;
            Caption = caption;
            DefaultValue = defaultValue;
        }

        public string Name { get; }
        public string Caption { get; }
        public string DefaultValue { get; set; }
        public TextBox CustomControl { get; set; }

        public ISettingControlBinding CreateControlBinding()
        {
            return new TextBoxBinding(this, CustomControl);
    }

        private class TextBoxBinding : SettingControlBinding<StringSetting, TextBox>
        {
            public TextBoxBinding(StringSetting setting, TextBox customControl)
                : base(setting, customControl)
            {
            }

            public override TextBox CreateControl()
            {
                return new TextBox();
            }

            public override void LoadSetting(ISettingsSource settings, bool areSettingsEffective, TextBox control)
            {
                string settingVal = areSettingsEffective
                    ? Setting.ValueOrDefault(settings)
                    : Setting[settings];

                control.Text = control.Multiline
                    ? settingVal?.Replace("\n", Environment.NewLine)
                    : settingVal;
            }

            public override void SaveSetting(ISettingsSource settings, bool areSettingsEffective, TextBox control)
            {
                var controlValue = control.Text;
                if (areSettingsEffective)
                {
                    if (Setting.ValueOrDefault(settings) == controlValue)
                    {
                        return;
                    }
                }

                Setting[settings] = controlValue;
            }
        }

        public string this[ISettingsSource settings]
        {
            get => settings.GetString(Name, null);

            set => settings.SetString(Name, value);
        }

        public string ValueOrDefault(ISettingsSource settings)
        {
            return this[settings] ?? DefaultValue;
        }
    }
}
