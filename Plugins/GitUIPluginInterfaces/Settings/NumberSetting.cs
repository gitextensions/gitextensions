using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GitUIPluginInterfaces
{
    public class NumberSetting<T>: ISetting
    {
        public NumberSetting(string aName, T aDefaultValue)
            : this(aName, aName, aDefaultValue)
        {
        }

        public NumberSetting(string aName, string aCaption, T aDefaultValue)
        {
            Name = aName;
            Caption = aCaption;
            DefaultValue = aDefaultValue;
            _controlBinding = new TextBoxBinding(this);
        }

        public string Name { get; private set; }
        public string Caption { get; private set; }
        public T DefaultValue { get; set; }

        private ISettingControlBinding _controlBinding;
        public ISettingControlBinding ControlBinding
        {
            get { return _controlBinding; }
        }

        private class TextBoxBinding : SettingControlBinding<TextBox>
        {
            NumberSetting<T> Setting;

            public TextBoxBinding(NumberSetting<T> aSetting)
            {
                Setting = aSetting;
            }

            public override TextBox CreateControl()
            {
                return new TextBox();
            }

            public override void LoadSetting(ISettingsSource settings, TextBox control)
            {
                control.Text = Setting[settings].ToString();
            }

            public override void SaveSetting(ISettingsSource settings, TextBox control)
            {
                Setting[settings] = (T)ConvertFromString(control.Text);
            }
        }

        private static object ConvertFromString(string value)
        {
            var type = typeof (T);
            if (type == typeof (int))
                return int.Parse(value);
            if (type == typeof(float))
                return float.Parse(value);
            if (type == typeof(double))
                return double.Parse(value);
            if (type == typeof(long))
                return long.Parse(value);
            return null;
        }

        public T this[ISettingsSource settings]
        {
            get 
            {
                return settings.GetValue(Name, DefaultValue, s =>
                    {
                        return (T) ConvertFromString(s);
                    });
            }

            set 
            {
                settings.SetValue(Name, value, i => { return i.ToString(); });
            }
        }
    }
}
