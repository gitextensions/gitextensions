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
            _controlBinding = new ComboBoxBinding(this);
        }

        public string Name { get; private set; }
        public string Caption { get; private set; }
        public string DefaultValue { get; set; }
        public List<string> Values { get; set; }

        private ISettingControlBinding _controlBinding;
        public ISettingControlBinding ControlBinding
        {
            get { return _controlBinding; }
        }

        private class ComboBoxBinding : SettingControlBinding<ComboBox>
        {

            ChoiceSetting Setting;

            public ComboBoxBinding(ChoiceSetting aSetting)
            {
                Setting = aSetting;
            }

            public override ComboBox CreateControl()
            {
                var comboBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList};
                comboBox.Items.AddRange(Setting.Values.ToArray());
                return comboBox;
            }

            public override void LoadSetting(ISettingsSource settings, ComboBox control)
            {
                control.SelectedIndex = Setting.Values.IndexOf(Setting[settings]);
            }

            public override void SaveSetting(ISettingsSource settings, ComboBox control)
            {
                Setting[settings] = control.SelectedItem.ToString();
            }
        }

        public string this[ISettingsSource settings]
        {
            get
            {
                return settings.GetValue(Name, DefaultValue, s =>
                {
                    if (string.IsNullOrEmpty(s))
                        return DefaultValue;
                    return s;
                });
            }

            set
            {
                settings.SetValue(Name, value, s => { return s; });
            }
        }
    }
}
