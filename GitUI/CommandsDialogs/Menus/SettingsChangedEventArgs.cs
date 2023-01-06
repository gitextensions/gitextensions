using GitCommands;

namespace GitUI.CommandsDialogs.Menus
{
    public class SettingsChangedEventArgs : EventArgs
    {
        public SettingsChangedEventArgs(string oldTranslation, CommitInfoPosition oldCommitInfoPosition)
        {
            OldTranslation = oldTranslation;
            OldCommitInfoPosition = oldCommitInfoPosition;
        }

        public CommitInfoPosition OldCommitInfoPosition { get; }
        public string OldTranslation { get; }
    }
}
