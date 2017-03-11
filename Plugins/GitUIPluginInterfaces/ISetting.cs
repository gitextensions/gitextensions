using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GitUIPluginInterfaces
{
    public interface ISetting
    {
        /// <summary>
        /// Name of the setting
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Caption of the setting
        /// </summary>
        string Caption { get; }

        ISettingControlBinding CreateControlBinding();
    }

    public interface ISettingControlBinding
    {
        /// <summary>
        /// Creates a control to be placed on FormSettings to edit this setting value
        /// Control should take care of scalability and resizability of its subcontrols
        /// </summary>
        /// <returns></returns>
        Control GetControl();

        /// <summary>
        /// Loads setting value from settings to Control
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="areSettingsEffective"></param>
        void LoadSetting(ISettingsSource settings, bool areSettingsEffective);

        /// <summary>
        /// Saves value from Control to settings
        /// </summary>
        /// <param name="settings"></param>
        void SaveSetting(ISettingsSource settings);

        /// <summary>
        /// returns caption assotiated with this control or null if the control layouts
        /// the caption by itself
        /// </summary>
        string Caption();

        ISetting GetSetting();        
    }

    public abstract class SettingControlBinding<S, T> : ISettingControlBinding where T : Control where S : ISetting
    {
        private T _control;
        protected readonly S Setting;

        protected SettingControlBinding(S aSetting)
        {
            Setting = aSetting;
        }

        private T Control
        {
            get
            {
                if (_control == null)
                    _control = CreateControl();

                return _control;
            }
        }

        public Control GetControl()
        {
            return Control;
        }

        public void LoadSetting(ISettingsSource settings, bool areSettingsEffective)
        {
            LoadSetting(settings, areSettingsEffective, Control);
        }

        /// <summary>
        /// Saves value from Control to settings
        /// </summary>
        /// <param name="settings"></param>
        public void SaveSetting(ISettingsSource settings)
        {
            SaveSetting(settings, Control);
        }

        public virtual string Caption()
        {
            return Setting.Caption;
        }

        public ISetting GetSetting()
        {
            return Setting;
        }
        /// <summary>
        /// Creates a control to be placed on FormSettings to edit this setting value
        /// Control should take care of scalability and resizability of its subcontrols
        /// </summary>
        /// <returns></returns>
        public abstract T CreateControl();

        /// <summary>
        /// Loads setting value from settings to Control
        /// </summary>
        public abstract void LoadSetting(ISettingsSource settings, bool areSettingsEffective, T control);

        /// <summary>
        /// Saves value from Control to settings
        /// </summary>
        public abstract void SaveSetting(ISettingsSource settings, T control);
    }

}
