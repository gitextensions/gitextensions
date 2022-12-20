using GitCommands;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class GitSettingsPage : SettingsPageWithHeader
    {
        private readonly TranslationString _homeIsSetToString = new("HOME is set to:");

        public GitSettingsPage()
        {
            InitializeComponent();
            Text = "Paths";
            InitializeComplete();
        }

        public static SettingsPageReference GetPageReference()
        {
            return new SettingsPageReferenceByType(typeof(GitSettingsPage));
        }

        public override void OnPageShown()
        {
            GitPath.Text = AppSettings.GitCommandValue;
            GitBinPath.Text = AppSettings.GitBinDir;
        }

        protected override void SettingsToPage()
        {
            EnvironmentConfiguration.SetEnvironmentVariables();
            homeIsSetToLabel.Text = string.Concat(_homeIsSetToString.Text, " ", EnvironmentConfiguration.GetHomeDir());

            GitPath.Text = AppSettings.GitCommandValue;
            GitBinPath.Text = AppSettings.GitBinDir;

            base.SettingsToPage();
        }

        protected override void PageToSettings()
        {
            AppSettings.GitCommandValue = GitPath.Text;
            AppSettings.GitBinDir = GitBinPath.Text;

            base.PageToSettings();
        }

        private void BrowseGitPath_Click(object sender, EventArgs e)
        {
            CheckSettingsLogic.SolveGitCommand(GitPath.Text.Trim());

            using OpenFileDialog browseDialog = new()
            {
                FileName = AppSettings.GitCommandValue,
                Filter = "Git.cmd (git.cmd)|git.cmd|Git.exe (git.exe)|git.exe|Git (git)|git"
            };
            if (browseDialog.ShowDialog(this) == DialogResult.OK)
            {
                GitPath.Text = browseDialog.FileName;
            }
        }

        private void BrowseGitBinPath_Click(object sender, EventArgs e)
        {
            CheckSettingsLogic.SolveLinuxToolsDir(GitBinPath.Text.Trim());

            var userSelectedPath = OsShellUtil.PickFolder(this, AppSettings.GitBinDir);

            if (userSelectedPath is not null)
            {
                GitBinPath.Text = userSelectedPath;
            }
        }

        private void GitPath_TextChanged(object sender, EventArgs e)
        {
            // If user pastes text or types in the box be sure to validate and save in the settings.
            CheckSettingsLogic.SolveGitCommand(GitPath.Text.Trim());
        }

        private void GitBinPath_TextChanged(object sender, EventArgs e)
        {
            // If user pastes text or types in the box be sure to validate and save in the settings.
            CheckSettingsLogic.SolveLinuxToolsDir(GitBinPath.Text.Trim());
        }

        private void downloadGitForWindows_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OsShellUtil.OpenUrlInDefaultBrowser(@"https://github.com/gitextensions/gitextensions/wiki/Application-Dependencies#git");
        }

        private void ChangeHomeButton_Click(object sender, EventArgs e)
        {
            PageHost.SaveAll();
            using (FormFixHome frm = new())
            {
                frm.ShowDialog(this);
            }

            PageHost.LoadAll();

            // TODO?: rescan

            // original:
            ////            throw new NotImplementedException(@"
            ////            Save();
            ////            using (FormFixHome frm = new()) frm.ShowDialog(this);
            ////            LoadSettings();
            ////            Rescan_Click(null, null);
            ////            ");
        }
    }
}
