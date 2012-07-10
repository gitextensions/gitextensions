using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using GitUIPluginInterfaces;

namespace Gource
{
    public partial class GourceStart : Form
    {
        public GourceStart(string pathToGource, GitUIBaseEventArgs gitUiCommands, string gourceArguments)
        {
            InitializeComponent();
            PathToGource = pathToGource;
            GitCommands = gitUiCommands.GitCommands;
            GitWorkingDir = gitUiCommands.GitWorkingDir;
            AvatarsDir = gitUiCommands.GravatarCacheDir;
            GourceArguments = gourceArguments;

            WorkingDir.Text = GitWorkingDir;
            GourcePath.Text = pathToGource;
            Arguments.Text = GourceArguments;
        }

        private IGitCommands GitCommands { get; set; }

        public string PathToGource { get; set; }

        public string GitWorkingDir { get; set; }

        public string AvatarsDir { get; set; }

        public string GourceArguments { get; set; }

        private void RunRealCmdDetatched(string cmd, string arguments)
        {
            try
            {
                new Process
                    {
                        StartInfo =
                            {
                                UseShellExecute = true,
                                ErrorDialog = false,
                                RedirectStandardOutput = false,
                                RedirectStandardInput = false,
                                CreateNoWindow = false,
                                FileName = "\"" + cmd + "\"",
                                Arguments = arguments,
                                WorkingDirectory = WorkingDir.Text,
                                WindowStyle = ProcessWindowStyle.Normal,
                                LoadUserProfile = true
                            }
                    }.Start();
            }
            catch (Exception e)
            {
                MessageBox.Show(this, e.Message);
            }
        }

        private void Button1Click(object sender, EventArgs e)
        {
            if (!File.Exists(GourcePath.Text))
            {
                MessageBox.Show(this, "Cannot find \"gource\".\nPlease download \"gource\" and set the correct path.");
                return;
            }

            GourceArguments = Arguments.Text;
            string gourceAvatarsDir = "";
            if (GourceArguments.Contains("$(AVATARS)"))
                gourceAvatarsDir = LoadAvatars();
            string arguments = GourceArguments.Replace("$(AVATARS)", gourceAvatarsDir);
            PathToGource = GourcePath.Text;
            GitWorkingDir = WorkingDir.Text;

            RunRealCmdDetatched(GourcePath.Text, arguments);
            Close();
        }

        private string LoadAvatars()
        {
            var gourceAvatarsDir = Path.Combine(Path.GetTempPath(), "GitAvatars");
            Directory.CreateDirectory(gourceAvatarsDir);
            foreach (var file in Directory.GetFiles(gourceAvatarsDir))
                File.Delete(file);
            var lines = GitCommands.RunGit("log --pretty=format:\"%ae|%an\"").Split('\n');
            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (var line in lines)
            {
                var data = line.Split('|');
                var email = data[0];
                var author = data[1];
                if (!dict.ContainsKey(author))
                {
                    dict.Add(author, email);
                    string source = Path.Combine(AvatarsDir, email + ".png");
                    if (File.Exists(source))
                        File.Copy(source, Path.Combine(gourceAvatarsDir, author + ".png"), true);
                }
            }
            return gourceAvatarsDir;
        }

        private void GourceBrowseClick(object sender, EventArgs e)
        {
            var fileDialog =
                new OpenFileDialog
                    {
                        Filter = "Gource (gource.exe)|gource.exe",
                        FileName = GourcePath.Text
                    };
            fileDialog.ShowDialog(this);

            GourcePath.Text = fileDialog.FileName;
        }

        private void WorkingDirBrowseClick(object sender, EventArgs e)
        {
            var folderDialog = new FolderBrowserDialog {SelectedPath = WorkingDir.Text};
            folderDialog.ShowDialog(this);
            WorkingDir.Text = folderDialog.SelectedPath;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(@"http://code.google.com/p/gource/");
        }
    }
}