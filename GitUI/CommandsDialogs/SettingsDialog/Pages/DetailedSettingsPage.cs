using System;
using GitCommands;
using GitCommands.Settings;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class DetailedSettingsPage : RepoDistSettingsPage
    {
        public DetailedSettingsPage()
        {
            InitializeComponent();
            Text = "Detailed";
            InitializeComplete();
        }

        private DetailedGroup DetailedSettings => CurrentSettings.Detailed;

        public static SettingsPageReference GetPageReference()
        {
            return new SettingsPageReferenceByType(typeof(DetailedSettingsPage));
        }

        protected override void Init(ISettingsPageHost pageHost)
        {
            base.Init(pageHost);
            BindSettingsWithControls();
        }

        protected override void PageToSettings()
        {
            AppSettings.SmtpServer = SmtpServer.Text;
            if (int.TryParse(SmtpServerPort.Text, out var port))
            {
                AppSettings.SmtpPort = port;
            }

            AppSettings.SmtpUseSsl = chkUseSSL.Checked;

            base.PageToSettings();
        }

        protected override void OnRuntimeLoad()
        {
            base.OnRuntimeLoad();

            // align 1st columns across all tables
            tlpnlEmailSettings.AdjustWidthToSize(0, addLogMessages);
        }

        protected override void SettingsToPage()
        {
            SmtpServer.Text = AppSettings.SmtpServer;
            SmtpServerPort.Text = AppSettings.SmtpPort.ToString();
            chkUseSSL.Checked = AppSettings.SmtpUseSsl;

            base.SettingsToPage();
        }

        private void BindSettingsWithControls()
        {
            AddSettingBinding(DetailedSettings.GetRemoteBranchesDirectlyFromRemote, chkRemotesFromServer);
            AddSettingBinding(DetailedSettings.AddMergeLogMessages, addLogMessages);
            AddSettingBinding(DetailedSettings.MergeLogMessagesCount, nbMessages);
        }

        private void chkUseSSL_CheckedChanged(object sender, EventArgs e)
        {
            if (!chkUseSSL.Checked)
            {
                if (SmtpServerPort.Text == "587")
                {
                    SmtpServerPort.Text = "465";
                }
            }
            else
            {
                if (SmtpServerPort.Text == "465")
                {
                    SmtpServerPort.Text = "587";
                }
            }
        }
    }
}
