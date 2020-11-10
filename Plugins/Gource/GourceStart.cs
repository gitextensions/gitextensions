using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitExtensions.Core.Avatars;
using GitExtensions.Core.Commands;
using GitExtensions.Core.Commands.Events;
using GitExtensions.Core.Utils.UI;
using Gource.Properties;

namespace Gource
{
    public partial class GourceStart : Form
    {
        public GourceStart(string pathToGource, GitUIEventArgs gitUIArgs, string gourceArguments, IAvatarProvider avatarProvider)
        {
            InitializeComponent();
            PathToGource = pathToGource;
            GitUIArgs = gitUIArgs;
            GitWorkingDir = gitUIArgs?.GitModule.WorkingDir;
            GourceArguments = gourceArguments;
            AvatarProvider = avatarProvider;

            WorkingDir.Text = GitWorkingDir;
            GourcePath.Text = pathToGource;
            Arguments.Text = GourceArguments;

            Text = Strings.FormText;
            ArgumentsLabel.Text = Strings.ArgumentsLabel;
            GourceBrowse.Text = Strings.GourceBrowse;
            WorkingDirBrowse.Text = Strings.WorkingDirBrowse;
            button1.Text = Strings.Button1;
            label1.Text = Strings.Label1;
            label2.Text = Strings.Label2;
            linkLabel1.Text = Strings.LinkLabel1;
            linkLabel2.Text = Strings.LinkLabel2;
        }

        private GitUIEventArgs GitUIArgs { get; }

        public string PathToGource { get; set; }

        public string GitWorkingDir { get; set; }

        public string GourceArguments { get; set; }

        private IAvatarProvider AvatarProvider { get; }

        private void RunRealCmdDetached(string cmd, string arguments)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "\"" + cmd + "\"",
                    Arguments = arguments,
                    WorkingDirectory = WorkingDir.Text
                });
            }
            catch (Exception e)
            {
                MessageBox.Show(this, e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Button1Click(object sender, EventArgs e)
        {
            if (!File.Exists(GourcePath.Text))
            {
                MessageBox.Show(this, "Cannot find Gource.\nPlease download Gource and set the correct path.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ThreadHelper.JoinableTaskFactory.Run(
                async () =>
                {
                    GourceArguments = Arguments.Text;
                    var gourceAvatarsDir = GourceArguments.Contains("$(AVATARS)")
                        ? await LoadAvatarsAsync()
                        : "";
                    var arguments = GourceArguments.Replace("$(AVATARS)", gourceAvatarsDir);
                    PathToGource = GourcePath.Text;
                    GitWorkingDir = WorkingDir.Text;

                    RunRealCmdDetached(GourcePath.Text, arguments);

                    await this.SwitchToMainThreadAsync();

                    Close();
                });
        }

        private async Task<string> LoadAvatarsAsync()
        {
            var gourceAvatarsDir = Path.Combine(Path.GetTempPath(), "GitAvatars");

            Directory.CreateDirectory(gourceAvatarsDir);

            foreach (var file in Directory.GetFiles(gourceAvatarsDir))
            {
                File.Delete(file);
            }

            var args = new GitArgumentBuilder("log") { "--pretty=format:\"%aE|%aN\"" };
            var lines = GitUIArgs.GitModule.GitExecutable.GetOutput(args).Split('\n');

            var authors = lines.Select(
                line =>
                {
                    var bits = line.Split('|');
                    return (email: bits[0], name: bits[1]);
                })
                .Where(t => !string.IsNullOrWhiteSpace(t.email) && !string.IsNullOrWhiteSpace(t.name))
                .GroupBy(t => t.name)
                .Select(g => (g.First().email, name: g.Key));

            await Task.WhenAll(authors.Select(DownloadImage));

            return gourceAvatarsDir;

            async Task DownloadImage((string email, string name) author)
            {
                try
                {
                    var image = await AvatarProvider.GetAvatarAsync(author.email, author.name, imageSize: 90);
                    var filename = author.name + ".png";

                    if (filename.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                    {
                        return;
                    }

                    var filePath = Path.Combine(gourceAvatarsDir, filename);
                    image.Save(filePath, ImageFormat.Png);
                }
                catch
                {
                    // Do nothing
                }
            }
        }

        private void GourceBrowseClick(object sender, EventArgs e)
        {
            using (var fileDialog =
                new OpenFileDialog
                {
                    Filter = "Gource (gource.exe)|gource.exe",
                    FileName = GourcePath.Text
                })
            {
                fileDialog.ShowDialog(this);

                GourcePath.Text = fileDialog.FileName;
            }
        }

        private void WorkingDirBrowseClick(object sender, EventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog { SelectedPath = WorkingDir.Text })
            {
                folderDialog.ShowDialog(this);
                WorkingDir.Text = folderDialog.SelectedPath;
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(@"https://github.com/acaudwell/Gource/");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(@"https://github.com/acaudwell/Gource/blob/master/README");
        }
    }
}
