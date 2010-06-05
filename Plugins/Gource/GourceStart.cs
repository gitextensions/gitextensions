using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Gource
{
    public partial class GourceStart : Form
    {
        public GourceStart(string pathToGource, string gitWorkingDir)
        {
            InitializeComponent();
            PathToGource = pathToGource;
            GitWorkingDir = gitWorkingDir;

            WorkingDir.Text = GitWorkingDir;
            GourcePath.Text = pathToGource;
        }

        public string PathToGource { get; set; }

        public string GitWorkingDir { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            
        }

        private void RunRealCmdDetatched(string cmd, string arguments)
        {
            try
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.ErrorDialog = false;
                process.StartInfo.RedirectStandardOutput = false;
                process.StartInfo.RedirectStandardInput = false;

                process.StartInfo.CreateNoWindow = false;
                process.StartInfo.FileName = "\"" + cmd + "\"";
                process.StartInfo.Arguments = arguments;
                process.StartInfo.WorkingDirectory = WorkingDir.Text;
                process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
                process.StartInfo.LoadUserProfile = true;

                process.Start();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (!File.Exists(GourcePath.Text))
            {
                MessageBox.Show("Cannot find \"gource\".\nPlease download \"gource\" and set the correct path.");
                return;
            }

            RunRealCmdDetatched(GourcePath.Text, "");
        }

        private void ArgumentsLabel_Click(object sender, EventArgs e)
        {

        }

        private void GourceBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Gource (gource.exe)|gource.exe";
            fileDialog.FileName = GourcePath.Text;
            fileDialog.ShowDialog();
            GourcePath.Text = fileDialog.FileName;
        }

        private void WorkingDirBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            folderDialog.SelectedPath = WorkingDir.Text;
            folderDialog.ShowDialog();
            WorkingDir.Text = folderDialog.SelectedPath;
        }

        private void linkLabel1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(@"http://code.google.com/p/gource/");
        }
    }
}
