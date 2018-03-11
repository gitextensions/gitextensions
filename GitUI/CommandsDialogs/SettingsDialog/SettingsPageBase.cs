using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Settings;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog
{
    /// <summary>
    /// set Text property in derived classes to set the title
    /// </summary>
    public abstract class SettingsPageBase : GitExtensionsControl, ISettingsPage
    {
        private List<ISettingControlBinding> _controlBindings = new List<ISettingControlBinding>();
        private ISettingsPageHost _pageHost;
        protected ISettingsPageHost PageHost
        {
            get
            {
                if (_pageHost == null)
                {
                    throw new InvalidOperationException("PageHost instance was not passed to page: " + GetType().FullName);
                }

                return _pageHost;
            }
        }

        protected CheckSettingsLogic CheckSettingsLogic => PageHost.CheckSettingsLogic;
        protected CommonLogic CommonLogic => CheckSettingsLogic.CommonLogic;

        protected GitModule Module => CommonLogic.Module;

        protected virtual void Init(ISettingsPageHost pageHost)
        {
            _pageHost = pageHost;
        }

        public static T Create<T>(ISettingsPageHost pageHost) where T : SettingsPageBase, new()
        {
            T result = new T();

            result.Init(pageHost);

            return result;
        }

        public virtual string GetTitle()
        {
            return Text;
        }

        public virtual Control GuiControl => this;

        /// <summary>
        /// Called when SettingsPage is shown (again);
        /// e. g. after user clicked a tree item
        /// </summary>
        public virtual void OnPageShown()
        {
            // to be overridden
        }

        /// <summary>
        /// True during execution of LoadSettings(). Usually derived classes
        /// apply settings to GUI controls. Some of controls trigger events -
        /// IsLoadingSettings can be used for example to not execute the event action.
        /// </summary>
        protected bool IsLoadingSettings { get; private set; }

        public void LoadSettings()
        {
            IsLoadingSettings = true;
            SettingsToPage();
            IsLoadingSettings = false;
        }

        public void SaveSettings()
        {
            PageToSettings();
        }

        protected virtual void SettingsToPage()
        {
            foreach (var cb in _controlBindings)
            {
                cb.LoadSetting(GetCurrentSettings(), AreEffectiveSettings);
            }
        }

        protected virtual void PageToSettings()
        {
            foreach (var cb in _controlBindings)
            {
                cb.SaveSetting(GetCurrentSettings(), AreEffectiveSettings);
            }
        }

        protected abstract ISettingsSource GetCurrentSettings();
        protected abstract bool AreEffectiveSettings { get; }

        public void AddControlBinding(ISettingControlBinding binding)
        {
            _controlBindings.Add(binding);
        }

        protected void AddSettingBinding(BoolNullableSetting setting, CheckBox checkBox)
        {
            var adapter = new BoolCheckBoxAdapter(setting, checkBox);
            AddControlBinding(adapter.CreateControlBinding());
        }

        protected void AddSettingBinding(IntNullableSetting setting, TextBox control)
        {
            var adapter = new IntTextBoxAdapter(setting, control);
            AddControlBinding(adapter.CreateControlBinding());
        }

        protected void AddSettingBinding(GitCommands.Settings.StringSetting setting, ComboBox comboBox)
        {
            var adapter = new StringComboBoxAdapter(setting, comboBox);
            AddControlBinding(adapter.CreateControlBinding());
        }

        private IList<string> _childrenText;

        /// <summary>
        /// override to provide search keywords
        /// </summary>
        public virtual IEnumerable<string> GetSearchKeywords()
        {
            return _childrenText ?? (_childrenText = GetChildrenText(this));
        }

        /// <summary>Recursively gets the text from all <see cref="Control"/>s within the specified <paramref name="control"/>.</summary>
        private static IList<string> GetChildrenText(Control control)
        {
            if (control.HasChildren == false)
            {
                return new string[0];
            }

            List<string> texts = new List<string>();
            foreach (Control child in control.Controls)
            {
                if (!child.Visible || child is NumericUpDown)
                {// skip: invisible; some input controls
                    continue;
                }

                if (child.Enabled && !string.IsNullOrWhiteSpace(child.Text))
                {// enabled AND not whitespace -> add
                    // also searches text boxes and comboboxes
                    // TODO(optional): search through the drop down list of comboboxes
                    // TODO(optional): convert numeric dropdown values to text
                    texts.Add(child.Text.Trim().ToLowerInvariant());
                }

                texts.AddRange(GetChildrenText(child)); // recurse
            }

            return texts;
        }

        protected virtual string GetCommaSeparatedKeywordList()
        {
            return "";
        }

        public virtual bool IsInstantSavePage => false;

        public virtual SettingsPageReference PageReference => new SettingsPageReferenceByType(GetType());
    }

    public class BoolCheckBoxAdapter : GitUIPluginInterfaces.BoolSetting
    {
        public BoolCheckBoxAdapter(BoolNullableSetting setting, CheckBox checkBox)
            : base(setting.FullPath, setting.DefaultValue.Value)
        {
            CustomControl = checkBox;
        }
    }

    public class StringComboBoxAdapter : ChoiceSetting
    {
        public StringComboBoxAdapter(GitCommands.Settings.StringSetting setting, ComboBox comboBox)
            : base(setting.FullPath, comboBox.Items.Cast<string>().ToList(), setting.DefaultValue)
        {
            CustomControl = comboBox;
        }
    }

    public class IntTextBoxAdapter : NumberSetting<int>
    {
        public IntTextBoxAdapter(IntNullableSetting setting, TextBox control)
            : base(setting.FullPath, setting.DefaultValue.Value)
        {
            CustomControl = control;
        }
    }
}
