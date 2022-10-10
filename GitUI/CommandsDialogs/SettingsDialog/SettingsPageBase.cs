﻿using GitCommands;
using GitCommands.Settings;
using GitExtUtils.GitUI.Theming;
using GitUIPluginInterfaces;
using JetBrains.Annotations;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog
{
    /// <summary>
    /// set Text property in derived classes to set the title.
    /// </summary>
    public abstract partial class SettingsPageBase : GitExtensionsControl, ISettingsPage
    {
        private readonly List<ISettingControlBinding> _controlBindings = new();
        private ISettingsPageHost? _pageHost;

        protected SettingsPageBase()
        {
            InitializeComponent();
        }

        protected ISettingsPageHost PageHost
        {
            get
            {
                if (_pageHost is null)
                {
                    throw new InvalidOperationException("PageHost instance was not passed to page: " + GetType().FullName);
                }

                return _pageHost;
            }
        }

        protected CheckSettingsLogic CheckSettingsLogic => PageHost.CheckSettingsLogic;

        protected CommonLogic CommonLogic => CheckSettingsLogic.CommonLogic;

        protected GitModule? Module => CommonLogic.Module;

        protected ToolTip ToolTip => toolTip1;

        protected virtual void Init(ISettingsPageHost pageHost)
        {
            _pageHost = pageHost;
        }

        public static T Create<[MeansImplicitUse] T>(ISettingsPageHost pageHost) where T : SettingsPageBase, new()
        {
            T result = new();

            result.AdjustForDpiScaling();
            result.EnableRemoveWordHotkey();
            result.FixVisualStyle();

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
        /// e. g. after user clicked a tree item.
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

        /// <summary>
        /// Indicates that settings have been loaded to the page.
        /// </summary>
        protected bool IsSettingsLoaded { get; private set; }

        public void LoadSettings()
        {
            IsLoadingSettings = true;
            SettingsToPage();
            IsLoadingSettings = false;
            IsSettingsLoaded = true;
        }

        public void SaveSettings()
        {
            PageToSettings();
        }

        protected virtual void SettingsToPage()
        {
            foreach (var cb in _controlBindings)
            {
                cb.LoadSetting(GetCurrentSettings());
            }
        }

        protected virtual void PageToSettings()
        {
            foreach (var cb in _controlBindings)
            {
                cb.SaveSetting(GetCurrentSettings());
            }
        }

        protected abstract ISettingsSource GetCurrentSettings();

        public void AddControlBinding(ISettingControlBinding binding)
        {
            _controlBindings.Add(binding);
        }

        protected void AddSettingBinding(ISetting<bool> setting, CheckBox checkBox)
        {
            BoolCheckBoxAdapter adapter = new(setting, checkBox);
            AddControlBinding(adapter.CreateControlBinding());
        }

        protected void AddSettingBinding(ISetting<bool?> setting, CheckBox checkBox)
        {
            BoolCheckBoxAdapter adapter = new(setting, checkBox);
            AddControlBinding(adapter.CreateControlBinding());
        }

        protected void AddSettingBinding(ISetting<int> setting, TextBox control)
        {
            IntTextBoxAdapter adapter = new(setting, control);
            AddControlBinding(adapter.CreateControlBinding());
        }

        protected void AddSettingBinding(ISetting<int?> setting, TextBox control)
        {
            IntTextBoxAdapter adapter = new(setting, control);
            AddControlBinding(adapter.CreateControlBinding());
        }

        protected void AddSettingBinding(ISetting<string> setting, ComboBox comboBox)
        {
            StringComboBoxAdapter adapter = new(setting, comboBox);
            AddControlBinding(adapter.CreateControlBinding());
        }

        private IReadOnlyList<string>? _childrenText;

        /// <summary>
        /// override to provide search keywords.
        /// </summary>
        public virtual IEnumerable<string> GetSearchKeywords()
        {
            return _childrenText ??= GetChildrenText(this);
        }

        /// <summary>
        /// Gets the <see cref="Control.Text"/> values of <paramref name="control"/>
        /// and all its descendants.
        /// </summary>
        private static IReadOnlyList<string> GetChildrenText(Control control)
        {
            List<string> texts = new();

            Queue<Control> queue = new();
            queue.Enqueue(control);

            while (queue.Count != 0)
            {
                var next = queue.Dequeue();

                if (!next.Visible || next is NumericUpDown)
                {
                    // skip: invisible; some input controls
                    continue;
                }

                if (next.Enabled && !string.IsNullOrWhiteSpace(next.Text))
                {
                    // enabled AND not whitespace -> add
                    // also searches text boxes and comboboxes
                    // TODO(optional): search through the drop down list of comboboxes
                    // TODO(optional): convert numeric dropdown values to text
                    texts.Add(next.Text.Trim());
                }

                foreach (Control child in next.Controls)
                {
                    queue.Enqueue(child);
                }
            }

            return texts;
        }

        public virtual bool IsInstantSavePage => false;

        public virtual SettingsPageReference PageReference => new SettingsPageReferenceByType(GetType());
    }

    public class BoolCheckBoxAdapter : BoolSetting
    {
        public BoolCheckBoxAdapter(ISetting<bool> setting, CheckBox checkBox)
            : base(setting.FullPath, setting.Default)
        {
            CustomControl = checkBox;
        }

        public BoolCheckBoxAdapter(ISetting<bool?> setting, CheckBox checkBox)
            : base(setting.FullPath, setting.Default ?? false)
        {
            CustomControl = checkBox;
        }
    }

    public class StringComboBoxAdapter : ChoiceSetting
    {
        public StringComboBoxAdapter(ISetting<string> setting, ComboBox comboBox)
            : base(setting.FullPath, comboBox.Items.Cast<string>().ToList(), setting.Default)
        {
            CustomControl = comboBox;
        }
    }

    public class IntTextBoxAdapter : NumberSetting<int>
    {
        public IntTextBoxAdapter(ISetting<int> setting, TextBox control)
            : base(setting.FullPath, setting.Default)
        {
            CustomControl = control;
        }

        public IntTextBoxAdapter(ISetting<int?> setting, TextBox control)
            : base(setting.FullPath, setting.Default ?? 0)
        {
            CustomControl = control;
        }
    }
}
