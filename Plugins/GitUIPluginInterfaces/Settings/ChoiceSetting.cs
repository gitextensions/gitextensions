using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace GitUIPluginInterfaces
{
    public class ChoiceSetting: ISetting
    {
        public ChoiceSetting(string aName, List<string> values, string aDefaultValue = null)
            : this(aName, aName, values, aDefaultValue)
        {
        }

        public ChoiceSetting(string aName, string aCaption, List<string> values, string aDefaultValue = null)
        {
            Name = aName;
            Caption = aCaption;
            DefaultValue = aDefaultValue;
            Values = values;
            if (DefaultValue == null && values.Count != 0)
                DefaultValue = values[0];
        }

        public string Name { get; private set; }
        public string Caption { get; private set; }
        public string DefaultValue { get; set; }
        public List<string> Values { get; set; }

        public ISettingControlBinding CreateControlBinding()
        {
            return new ComboBoxBinding(this);
    }

        private class ComboBoxBinding : SettingControlBinding<ChoiceSetting, ComboBox>
        {

            public ComboBoxBinding(ChoiceSetting aSetting)
                : base(aSetting)
            { }

            public override ComboBox CreateControl()
            {
                var comboBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList};
                comboBox.Items.AddRange(Setting.Values.ToArray());
                return comboBox;
            }

            public override void LoadSetting(ISettingsSource settings, bool areSettingsEffective, ComboBox control)
            {
                string settingVal;
                if (areSettingsEffective)
                {
                    settingVal = Setting.ValueOrDefault(settings);
                }
                else
                {
                    settingVal = Setting[settings];
                }

                control.SelectedIndex = Setting.Values.IndexOf(settingVal);
            }

            public override void SaveSetting(ISettingsSource settings, ComboBox control)
            {
                Setting[settings] = control.SelectedItem.ToString();
            }
        }

        public string ValueOrDefault(ISettingsSource settings)
        {
            return this[settings] ?? DefaultValue;
        }

        public string this[ISettingsSource settings]
        {
            get
            {
                return settings.GetString(Name, null);
            }

            set
            {
                settings.SetString(Name, value);
            }
        }
    }
}
