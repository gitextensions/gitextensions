using System;
using System.Windows.Forms;
using GitUI;

namespace GitUIPluginInterfaces
{
    public class BoolSetting: ISetting
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

        public ISettingControlBinding CreateControlBinding()
        {
            return new CheckBoxBinding(this);
        }

        public bool? this[ISettingsSource settings]
        {
            get
            {
                return settings.GetBool(Name);
            }

            set
            {
                settings.SetBool(Name, value);
            }
        }

        public bool ValueOrDefault(ISettingsSource settings)
        {
            return this[settings] ?? DefaultValue;
        }

        private class CheckBoxBinding : SettingControlBinding<BoolSetting, CheckBox>
        {
            public CheckBoxBinding(BoolSetting aSetting)
                : base(aSetting)
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

            public override void SaveSetting(ISettingsSource settings, CheckBox control)
            {
                Setting[settings] = control.GetNullableChecked();
            }
        }
    }
}
