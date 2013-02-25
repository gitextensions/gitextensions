namespace GitUI.CommandsDialogs.SettingsDialog
{
    public class GitExtensionsSettingsGroup : GroupSettingsPage
    {
        public GitExtensionsSettingsGroup()
            : base("Git Extensions")
        {
        }

        public static SettingsPageReference GetPageReference()
        {
            return new SettingsPageReferenceByType(typeof(GitExtensionsSettingsGroup));
        }
    }

    public class PluginsSettingsGroup : GroupSettingsPage
    {
        public PluginsSettingsGroup()
            : base("Plugins")
        {
        }

        public static SettingsPageReference GetPageReference()
        {
            return new SettingsPageReferenceByType(typeof(PluginsSettingsGroup));
        }
    }
}
