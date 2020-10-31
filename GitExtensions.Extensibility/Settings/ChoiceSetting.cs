using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GitExtensions.Core.Settings;

namespace GitExtensions.Extensibility.Settings
{
    public class ChoiceSetting : ISetting
    {
        public ChoiceSetting(string name, IList<string> values, string defaultValue = null)
            : this(name, name, values, defaultValue)
        {
        }

        public ChoiceSetting(string name, string caption, IList<string> values, string defaultValue = null)
        {
            Name = name;
            Caption = caption;
            DefaultValue = defaultValue;
            Values = values;
            if (DefaultValue == null && values.Any())
            {
                DefaultValue = values.First();
            }
        }

        public string Name { get; }
        public string Caption { get; }
        public string DefaultValue { get; set; }
        public IList<string> Values { get; set; }
        public ComboBox CustomControl { get; set; }

        public ISettingControlBinding CreateControlBinding()
        {
            return new ComboBoxBinding(this, CustomControl);
        }

        private class ComboBoxBinding : SettingControlBinding<ChoiceSetting, ComboBox>
        {
            public ComboBoxBinding(ChoiceSetting setting, ComboBox customControl)
                : base(setting, customControl)
            {
            }

            public override ComboBox CreateControl()
            {
                Setting.CustomControl = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
                Setting.CustomControl.Items.AddRange(Setting.Values.ToArray());
                return Setting.CustomControl;
            }

            public override void LoadSetting(ISettingsSource settings, ComboBox control)
            {
                string settingVal = settings.SettingLevel == SettingLevel.Effective
                    ? Setting.ValueOrDefault(settings)
                    : Setting[settings];

                control.SelectedIndex = Setting.Values.IndexOf(settingVal);

                if (control.SelectedIndex == -1)
                {
                    control.Text = settingVal;
                }
            }

            public override void SaveSetting(ISettingsSource settings, ComboBox control)
            {
                var controlValue = control.SelectedItem?.ToString();
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
