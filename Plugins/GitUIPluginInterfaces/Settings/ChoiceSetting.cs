using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace GitUIPluginInterfaces
{
    public class ChoiceSetting : ISetting
    {
        public ChoiceSetting(string aName, IList<string> values, string aDefaultValue = null)
            : this(aName, aName, values, aDefaultValue)
        {
        }

        public ChoiceSetting(string aName, string aCaption, IList<string> values, string aDefaultValue = null)
        {
            Name = aName;
            Caption = aCaption;
            DefaultValue = aDefaultValue;
            Values = values;
            if (DefaultValue == null && values.Any())
                DefaultValue = values.First();
        }

        public string Name { get; private set; }
        public string Caption { get; private set; }
        public string DefaultValue { get; set; }
        public IList<string> Values { get; set; }
        public ComboBox CustomControl { get; set; }

        public ISettingControlBinding CreateControlBinding()
        {
            return new ComboBoxBinding(this, CustomControl);
    }

        private class ComboBoxBinding : SettingControlBinding<ChoiceSetting, ComboBox>
        {

            public ComboBoxBinding(ChoiceSetting aSetting, ComboBox aCustomControl)
                : base(aSetting, aCustomControl)
            { }

            public override ComboBox CreateControl()
            {
                var comboBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
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
                if (control.SelectedIndex == -1)
                {
                    control.Text = settingVal;
                }
            }

            public override void SaveSetting(ISettingsSource settings, bool areSettingsEffective, ComboBox control)
            {
                var controlValue = control.SelectedItem?.ToString();
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

        public string ValueOrDefault(ISettingsSource settings)
        {
            return this[settings] ?? DefaultValue;
        }

        public string this[ISettingsSource settings]
        {
            get => settings.GetString(Name, null);

            set => settings.SetString(Name, value);
        }
    }
}
