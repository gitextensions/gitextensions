namespace GitUI.CommandsDialogs.SettingsDialog
{
    public class GitSettingsGroup : GroupSettingsPage
    {
        public GitSettingsGroup()
            : base("Git")
        {
        }

        public static SettingsPageReference GetPageReference()
        {
            return new SettingsPageReferenceByType(typeof(GitSettingsGroup));
        }
    }

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
