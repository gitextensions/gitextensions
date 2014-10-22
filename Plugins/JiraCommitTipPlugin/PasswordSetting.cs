using System.Windows.Forms;
using GitUIPluginInterfaces;

namespace JiraCommitPlugin
{
    public class PasswordSetting : ISetting
    {
        private readonly string name;
        private readonly string caption;
        private readonly ISettingControlBinding controlBinding;

        public PasswordSetting(string name)
        {
            this.name = name;
            caption = name;
            controlBinding = new PasswordSettingControlBinding(this);
        }

        public string Name
        {
            get { return name; }
        }

        public string Caption
        {
            get { return caption; }
        }

        public ISettingControlBinding ControlBinding
        {
            get { return controlBinding; }
        }

        private class PasswordSettingControlBinding : SettingControlBinding<TextBox>
        {
            private readonly PasswordSetting setting;

            public PasswordSettingControlBinding(PasswordSetting aSetting)
            {
                setting = aSetting;
            }

            public override TextBox CreateControl()
            {
                return new TextBox { PasswordChar = '*' };
            }

            public override void LoadSetting(ISettingsSource settings, TextBox control)
            {
                control.Text = setting[settings];

            }

            public override void SaveSetting(ISettingsSource settings, TextBox control)
            {
                setting[settings] = control.Text;
            }
        }

        public string this[ISettingsSource settings]
        {
            get
            {
                return settings.GetValue(Name, string.Empty, s =>
                {
                    if (string.IsNullOrEmpty(s))
                        return string.Empty;
                    return s;
                });
            }

            private set
            {
                settings.SetValue(Name, value, s => s);
            }
        }
    }
}