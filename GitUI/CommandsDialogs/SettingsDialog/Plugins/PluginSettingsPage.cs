using GitExtensions.Extensibility.Plugins;
using GitExtensions.Extensibility.Settings;
using Microsoft;

namespace GitUI.CommandsDialogs.SettingsDialog.Plugins
{
    public partial class PluginSettingsPage : AutoLayoutSettingsPage
    {
        private IGitPlugin? _gitPlugin;
        private GitPluginSettingsContainer? _settingsContainer;

        public PluginSettingsPage(IServiceProvider serviceProvider)
           : base(serviceProvider)
        {
            InitializeComponent();
        }

        private void CreateSettingsControls()
        {
            IEnumerable<ISetting> settings = GetSettings();

            foreach (ISetting setting in settings)
            {
                AddSettingControl(setting.CreateControlBinding());
            }
        }

        private void Init(IGitPlugin gitPlugin)
        {
            Validates.NotNull(gitPlugin.Description);

            _gitPlugin = gitPlugin;

            // Description for old plugin setting processing as key
            _settingsContainer = new GitPluginSettingsContainer(gitPlugin.Id, gitPlugin.Description);
            CreateSettingsControls();
            InitializeComplete();
        }

        public static PluginSettingsPage CreateSettingsPageFromPlugin(ISettingsPageHost pageHost, IGitPlugin gitPlugin, IServiceProvider serviceProvider)
        {
            PluginSettingsPage result = Create<PluginSettingsPage>(pageHost, serviceProvider);
            result.Init(gitPlugin);
            return result;
        }

        protected override SettingsSource GetCurrentSettings()
        {
            Validates.NotNull(_settingsContainer);

            _settingsContainer.SetSettingsSource(base.GetCurrentSettings());
            return _settingsContainer;
        }

        public override string GetTitle()
        {
            return _gitPlugin?.Name ?? string.Empty;
        }

        private IEnumerable<ISetting> GetSettings()
        {
            if (_gitPlugin is null)
            {
                throw new ApplicationException();
            }

            return _gitPlugin.HasSettings ? _gitPlugin.GetSettings() : Array.Empty<ISetting>();
        }

        public override SettingsPageReference PageReference
        {
            get
            {
                Validates.NotNull(_gitPlugin);

                return new SettingsPageReferenceByType(_gitPlugin.GetType());
            }
        }

        protected override ISettingsLayout CreateSettingsLayout()
        {
            Validates.NotNull(_gitPlugin);

            labelNoSettings.Visible = !_gitPlugin.HasSettings;

            ISettingsLayout layout = base.CreateSettingsLayout();

            tableLayoutPanel1.Controls.Add(layout.GetControl(), 0, 1);

            return layout;
        }
    }
}
