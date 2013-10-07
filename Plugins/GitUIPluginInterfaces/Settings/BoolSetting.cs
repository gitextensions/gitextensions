using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GitUIPluginInterfaces
{
    public class BoolSetting: ISetting
    {
        public BoolSetting(string aName, bool? aDefaultValue)
            : this(aName, aName, aDefaultValue)
        {
        }

        public BoolSetting(string aName, string aCaption, bool? aDefaultValue)
        {
            Name = aName;
            Caption = aCaption;
            DefaultValue = aDefaultValue;
        }

        public string Name { get; private set; }
        public string Caption { get; private set; }
        public bool? DefaultValue { get; set; }

        public ISettingControlBinding CreateControlBinding()
        {
            return new CheckBoxBinding(this);
        }

        private class CheckBoxBinding : SettingControlBinding<CheckBox>
        {

            BoolSetting Setting;

            public CheckBoxBinding(BoolSetting aSetting)
            {
                Setting = aSetting;
            }

            public override CheckBox CreateControl()
            {
                return new CheckBox();
            }

            public override void LoadSetting(ISettingsSource settings, CheckBox control)
            {
                //TODO handle three states
                Setting[settings] = control.Checked;
            }

            public void SaveSetting(ISettingsSource settings, CheckBox control)
            {
                //TODO handle three states
                control.Checked = Setting[settings].Value;
            }
        }

        public bool? this[ISettingsSource settings]
        {
            get 
            {
                return settings.GetBool(Name, DefaultValue);
            }

            set 
            {
                settings.SetBool(Name, value);
            }
        }
    }
}
