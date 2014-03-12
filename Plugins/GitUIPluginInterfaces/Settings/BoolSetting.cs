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
            _controlBinding = new CheckBoxBinding(this);
        }

        public string Name { get; private set; }
        public string Caption { get; private set; }
        public bool? DefaultValue { get; set; }
        private ISettingControlBinding _controlBinding;
        public ISettingControlBinding ControlBinding
        {
            get { return _controlBinding; }
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
                control.Checked = Setting[settings].Value;
            }

            public override void SaveSetting(ISettingsSource settings, CheckBox control)
            {
                //TODO handle three states
                Setting[settings] = control.Checked;
            }
        }

        public bool? this[ISettingsSource settings]
        {
            get 
            {
                return settings.GetValue(Name, DefaultValue, s =>
                    {
                        if (string.IsNullOrWhiteSpace(s))
                            return DefaultValue;
                        return string.Compare(s, true.ToString(), StringComparison.InvariantCultureIgnoreCase) == 0;
                    });
            }

            set 
            {
                settings.SetValue(Name, value, b =>
                    {
                        if (!b.HasValue)
                            return string.Empty;
                        return b.Value.ToString();
                    });
            }
        }
    }
}
