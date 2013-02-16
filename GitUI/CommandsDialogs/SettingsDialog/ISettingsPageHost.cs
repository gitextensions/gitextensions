namespace GitUI.CommandsDialogs.SettingsDialog
{
    public interface ISettingsPageHost
    {
        void GotoPage(SettingsPageReference settingsPageReference);

        /// <summary>
        /// needed by ChecklistSettingsPage (TODO: needed here?)
        /// </summary>
        void SaveAll();

        /// <summary>
        /// needed by ChecklistSettingsPage (TODO: needed here?)
        /// </summary>
        void LoadAll();
    }
}
