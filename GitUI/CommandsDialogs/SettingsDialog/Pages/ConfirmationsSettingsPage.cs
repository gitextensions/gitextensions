using GitCommands;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class ConfirmationsSettingsPage : SettingsPageBase
    {
        public ConfirmationsSettingsPage()
        {
            InitializeComponent();
            Text = "Confirmations";
            Translate();
        }

        protected override void OnLoadSettings()
        {
            chkAmmend.Checked = Settings.DontConfirmAmmend;
            chkAutoPopStashAfterPull.Checked = Settings.AutoPopStashAfterPull;
            chkPushNewBranch.Checked = Settings.DontConfirmPushNewBranch;
            chkAddTrackingRef.Checked = Settings.DontConfirmAddTrackingRef;
        }

        public override void SaveSettings()
        {
            Settings.DontConfirmAmmend = chkAmmend.Checked;
            Settings.AutoPopStashAfterPull = chkAutoPopStashAfterPull.Checked;
            Settings.DontConfirmPushNewBranch = chkPushNewBranch.Checked;
            Settings.DontConfirmAddTrackingRef = chkAddTrackingRef.Checked;
        }

        public static SettingsPageReference GetPageReference()
        {
            return new SettingsPageReferenceByType(typeof(ConfirmationsSettingsPage));
        }        

    }
}
