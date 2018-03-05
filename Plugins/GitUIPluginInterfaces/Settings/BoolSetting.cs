using System.Windows.Forms;
using GitUI;

namespace GitUIPluginInterfaces
{
    public class BoolSetting : ISetting
    {
        public BoolSetting(string aName, bool aDefaultValue)
            : this(aName, aName, aDefaultValue)
        {
        }

        public BoolSetting(string aName, string aCaption, bool aDefaultValue)
        {
            Name = aName;
            Caption = aCaption;
            DefaultValue = aDefaultValue;
        }

        public string Name { get; private set; }
        public string Caption { get; private set; }
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
            public CheckBoxBinding(BoolSetting aSetting, CheckBox aCustomControl)
                : base(aSetting, aCustomControl)
            { }

            public override CheckBox CreateControl()
            {
                CheckBox result = new CheckBox();
                result.ThreeState = true;
                return result;
            }

            public override void LoadSetting(ISettingsSource settings, bool areSettingsEffective, CheckBox control)
            {
                bool? settingVal;
                if (areSettingsEffective)
                {
                    settingVal = Setting.ValueOrDefault(settings);
                }
                else
                {
                    settingVal = Setting[settings];
                }

                control.SetNullableChecked(settingVal);
            }

            public override void SaveSetting(ISettingsSource settings, bool areSettingsEffective, CheckBox control)
            {
                var controlValue = control.GetNullableChecked();
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
    }
}
