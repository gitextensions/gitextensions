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

        CheckSettingsLogic CheckSettingsLogic { get; }
    }

    public class SettingsPageHostMock : ISettingsPageHost
    {
        private readonly CheckSettingsLogic _CheckSettingsLogic;

        public SettingsPageHostMock(CheckSettingsLogic aCheckSettingsLogic)
        {
            _CheckSettingsLogic = aCheckSettingsLogic;
        }

        public void GotoPage(SettingsPageReference settingsPageReference)
        {
        }

        public void SaveAll()
        {
        }

        public void LoadAll()
        {
        }

        public CheckSettingsLogic CheckSettingsLogic { get { return _CheckSettingsLogic; } }
    }
}
