using System.Windows.Forms;

namespace GitUIPluginInterfaces.Settings
{
    public class PasswordSetting : ISetting
    {
        public PasswordSetting(string aName, string aDefaultValue)
            : this(aName, aName, aDefaultValue)
        {
        }

        public PasswordSetting(string aName, string aCaption, string aDefaultValue)
        {
            Name = aName;
            Caption = aCaption;
            DefaultValue = aDefaultValue;
            _controlBinding = new TextBoxBinding(this);
        }

        public string Name { get; private set; }
        public string Caption { get; private set; }
        public string DefaultValue { get; set; }

        private ISettingControlBinding _controlBinding;
        public ISettingControlBinding ControlBinding
        {
            get { return _controlBinding; }
        }

        private class TextBoxBinding : SettingControlBinding<TextBox>
        {
            PasswordSetting Setting;

            public TextBoxBinding(PasswordSetting aSetting)
            {
                Setting = aSetting;
            }

            public override TextBox CreateControl()
            {
                return new TextBox {PasswordChar = '\u25CF'};
            }

            public override void LoadSetting(ISettingsSource settings, TextBox control)
            {
                control.Text = Setting[settings];
            }

            public override void SaveSetting(ISettingsSource settings, TextBox control)
            {
                Setting[settings] = control.Text;
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