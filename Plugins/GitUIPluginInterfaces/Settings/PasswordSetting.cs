using System.Windows.Forms;

namespace GitUIPluginInterfaces
{
    public class PasswordSetting : ISetting
    {
        public PasswordSetting(string name, string defaultValue)
            : this(name, name, defaultValue)
        {
        }

        public PasswordSetting(string name, string caption, string defaultValue)
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

        private class TextBoxBinding : SettingControlBinding<PasswordSetting, TextBox>
        {
            public TextBoxBinding(PasswordSetting setting, TextBox customControl)
                : base(setting, customControl)
            {
            }

            public override TextBox CreateControl()
            {
                return new TextBox { PasswordChar = '\u25CF' };
            }

            public override void LoadSetting(ISettingsSource settings, bool areSettingsEffective, TextBox control)
            {
                string settingVal = areSettingsEffective
                    ? Setting.ValueOrDefault(settings)
                    : Setting[settings];

                control.Text = settingVal;
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