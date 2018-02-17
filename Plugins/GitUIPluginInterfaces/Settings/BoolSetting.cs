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
            public CheckBoxBinding(BoolSetting aSetting, CheckBox aCustomControl)
                : base(aSetting, aCustomControl)
            { }

            public override CheckBox CreateControl()
            {
                return new CheckBox {ThreeState = true};
            }

            public override void LoadSetting(ISettingsSource settings, bool areSettingsEffective, CheckBox control)
            {
                control.SetNullableChecked(areSettingsEffective ? Setting.ValueOrDefault(settings) : Setting[settings]);
            }

            public override void SaveSetting(ISettingsSource settings, bool areSettingsEffective, CheckBox control)
            {
                var controlValue = control.GetNullableChecked();
                if (areSettingsEffective && Setting.ValueOrDefault(settings) == controlValue)
                    return;

                Setting[settings] = controlValue;
            }
        }
    }
}
