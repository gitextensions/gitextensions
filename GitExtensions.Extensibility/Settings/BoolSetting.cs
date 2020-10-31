using System.Windows.Forms;
using GitExtensions.Core.Settings;
using GitExtensions.Extensibility.Settings.UserControls;

namespace GitExtensions.Extensibility.Settings
{
    public class BoolSetting : ISetting
    {
        public BoolSetting(string name, bool defaultValue)
            : this(name, name, defaultValue)
        {
        }

        public BoolSetting(string name, string caption, bool defaultValue)
        {
            Name = name;
            Caption = caption;
            DefaultValue = defaultValue;
        }

        public string Name { get; }
        public string Caption { get; }
        public bool DefaultValue { get; set; }
        public CheckBox CustomControl { get; set; }

        public ISettingControlBinding CreateControlBinding()
        {
            return new CheckBoxBinding(this, CustomControl);
        }

        public bool? this[ISettingsSource settings]
        {
            get => settings.GetBool(Name);

            set => settings.SetBool(Name, value);
        }

        public bool ValueOrDefault(ISettingsSource settings)
        {
            return this[settings] ?? DefaultValue;
        }

        private class CheckBoxBinding : SettingControlBinding<BoolSetting, CheckBox>
        {
            public CheckBoxBinding(BoolSetting setting, CheckBox customControl)
                : base(setting, customControl)
            {
            }

            public override CheckBox CreateControl()
            {
                Setting.CustomControl = new CheckBox { ThreeState = true };
                return Setting.CustomControl;
            }

            public override void LoadSetting(ISettingsSource settings, CheckBox control)
            {
                bool? settingVal = settings.SettingLevel == SettingLevel.Effective
                    ? Setting.ValueOrDefault(settings)
                    : Setting[settings];

                control.SetNullableChecked(settingVal);
            }

            public override void SaveSetting(ISettingsSource settings, CheckBox control)
            {
                var controlValue = control.GetNullableChecked();
                if (settings.SettingLevel == SettingLevel.Effective)
                {
                    if (Setting.ValueOrDefault(settings) == controlValue)
                    {
                        return;
                    }
                }

                Setting[settings] = controlValue;
            }
        }
    }
}
