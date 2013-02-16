using System;
using System.Windows.Forms;
using System.Diagnostics;
using GitCommands;
using ResourceManager.Translation;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class GitSettingsPage : SettingsPageBase
    {
        private readonly TranslationString _homeIsSetToString = new TranslationString("HOME is set to:");

        readonly CheckSettingsLogic _checkSettingsLogic;
        private readonly ISettingsPageHost _settingsPageHost;

        public GitSettingsPage(CheckSettingsLogic checkSettingsLogic,
            ISettingsPageHost settingsPageHost)
        {
            InitializeComponent();
            Text = "Git";
            Translate();

            _checkSettingsLogic = checkSettingsLogic;
            _settingsPageHost = settingsPageHost;
        }

        protected override string GetCommaSeparatedKeywordList()
        {
            return "path,home,environment,variable,msys,cygwin,download,git,command,linux,tools";
        }

        public static SettingsPageReference GetPageReference()
        {
            return new SettingsPageReferenceByType(typeof(GitSettingsPage));
        }

        public override void OnPageShown()
        {
            GitPath.Text = Settings.GitCommand;
            GitBinPath.Text = Settings.GitBinDir;
        }

        protected override void OnLoadSettings()
        {
            GitCommandHelpers.SetEnvironmentVariable();
            homeIsSetToLabel.Text = string.Concat(_homeIsSetToString.Text, " ", GitCommandHelpers.GetHomeDir());

            GitPath.Text = Settings.GitCommand;
            GitBinPath.Text = Settings.GitBinDir;
        }

        public override void SaveSettings()
        {
            Settings.GitCommand = GitPath.Text;
            Settings.GitBinDir = GitBinPath.Text;
        }

        private void BrowseGitPath_Click(object sender, EventArgs e)
        {
            _checkSettingsLogic.SolveGitCommand();

            using (var browseDialog = new OpenFileDialog
            {
                FileName = Settings.GitCommand,
                Filter = "Git.cmd (git.cmd)|git.cmd|Git.exe (git.exe)|git.exe|Git (git)|git"
            })
            {

                if (browseDialog.ShowDialog(this) == DialogResult.OK)
                {
                    GitPath.Text = browseDialog.FileName;
                }
            }
        }

        private void BrowseGitBinPath_Click(object sender, EventArgs e)
        {
            _checkSettingsLogic.SolveLinuxToolsDir();

            using (var browseDialog = new FolderBrowserDialog { SelectedPath = Settings.GitBinDir })
            {

                if (browseDialog.ShowDialog(this) == DialogResult.OK)
                {
                    GitBinPath.Text = browseDialog.SelectedPath;
                }
            }
        }

        // TODO: needed anymore?
        private void GitPath_TextChanged(object sender, EventArgs e)
        {
            ////    if (loadingSettings)
            ////        return;

            ////    Settings.GitCommand = GitPath.Text;
            ////    OnLoadSettings();
        }

        private void downloadMsysgit_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(@"http://msysgit.github.com/");
        }

        private void ChangeHomeButton_Click(object sender, EventArgs e)
        {
            _settingsPageHost.SaveAll();
            using (var frm = new FormFixHome()) frm.ShowDialog(this);
            _settingsPageHost.LoadAll();
            // TODO?: rescan

            // orginal:
            ////            throw new NotImplementedException(@"
            ////            Save();
            ////            using (var frm = new FormFixHome()) frm.ShowDialog(this);
            ////            LoadSettings();
            ////            Rescan_Click(null, null);
            ////            ");
        }
    }
}
