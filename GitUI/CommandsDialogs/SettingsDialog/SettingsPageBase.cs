using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GitCommands;
using ResourceManager;
using GitUIPluginInterfaces;
using GitCommands.Settings;
using System.Linq;

namespace GitUI.CommandsDialogs.SettingsDialog
{
    /// <summary>
    /// set Text property in derived classes to set the title
    /// </summary>
    public abstract class SettingsPageBase : GitExtensionsControl, ISettingsPage
    {
        private List<ISettingControlBinding> _controlBindings = new List<ISettingControlBinding>();
        private ISettingsPageHost _PageHost;
        protected ISettingsPageHost PageHost
        {
            get
            {
                if (_PageHost == null)
                    throw new InvalidOperationException("PageHost instance was not passed to page: " + GetType().FullName);

                return _PageHost;
            }
        }

        protected CheckSettingsLogic CheckSettingsLogic => PageHost.CheckSettingsLogic;
        protected CommonLogic CommonLogic => CheckSettingsLogic.CommonLogic;


        protected GitModule Module => this.CommonLogic.Module;

        protected virtual void Init(ISettingsPageHost aPageHost)
        {
            _PageHost = aPageHost;
        }

        public static T Create<T>(ISettingsPageHost aPageHost) where T : SettingsPageBase, new()
        {
            T result = new T();

            result.Init(aPageHost);

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

        public void AddControlBinding(ISettingControlBinding aBinding)
        {
            _controlBindings.Add(aBinding);
        }

        protected void AddSettingBinding(BoolNullableSetting aSetting, CheckBox aCheckBox)
        {
            var adapter = new BoolCheckBoxAdapter(aSetting, aCheckBox);
            AddControlBinding(adapter.CreateControlBinding());
        }

        protected void AddSettingBinding(IntNullableSetting aSetting, TextBox aControl)
        {
            var adapter = new IntTextBoxAdapter(aSetting, aControl);
            AddControlBinding(adapter.CreateControlBinding());
        }

        protected void AddSettingBinding(GitCommands.Settings.StringSetting aSetting, ComboBox aComboBox)
        {
            var adapter = new StringComboBoxAdapter(aSetting, aComboBox);
            AddControlBinding(adapter.CreateControlBinding());
        }

        IList<string> childrenText;

        /// <summary>
        /// override to provide search keywords
        /// </summary>
        public virtual IEnumerable<string> GetSearchKeywords()
        {
            return childrenText ?? (childrenText = GetChildrenText(this));
        }

        /// <summary>Recursively gets the text from all <see cref="Control"/>s within the specified <paramref name="control"/>.</summary>
        static IList<string> GetChildrenText(Control control)
        {
            if (control.HasChildren == false) { return new string[0]; }

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
        public BoolCheckBoxAdapter(BoolNullableSetting aSetting, CheckBox aCheckBox)
            : base(aSetting.FullPath, aSetting.DefaultValue.Value)
        {
            CustomControl = aCheckBox;
        }
    }

    public class StringComboBoxAdapter : GitUIPluginInterfaces.ChoiceSetting
    {
        public StringComboBoxAdapter(GitCommands.Settings.StringSetting aSetting, ComboBox aComboBox)
            : base(aSetting.FullPath, aComboBox.Items.Cast<string>().ToList(), aSetting.DefaultValue)
        {
            CustomControl = aComboBox;
        }
    }

    public class IntTextBoxAdapter : GitUIPluginInterfaces.NumberSetting<int>
    {
        public IntTextBoxAdapter(IntNullableSetting aSetting, TextBox aControl)
            : base(aSetting.FullPath, aSetting.DefaultValue.Value)
        {
            CustomControl = aControl;
        }
    }
}
