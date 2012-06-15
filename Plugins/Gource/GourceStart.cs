﻿using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Gource
{
    public partial class GourceStart : Form
    {
        public GourceStart(string pathToGource, string gitWorkingDir, string gourceArguments)
        {
            InitializeComponent();
            PathToGource = pathToGource;
            GitWorkingDir = gitWorkingDir;
            GourceArguments = gourceArguments;

            WorkingDir.Text = GitWorkingDir;
            GourcePath.Text = pathToGource;
            Arguments.Text = GourceArguments;
        }

        public string PathToGource { get; set; }

        public string GitWorkingDir { get; set; }

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
            PathToGource = GourcePath.Text;
            GitWorkingDir = WorkingDir.Text;

            RunRealCmdDetatched(GourcePath.Text, GourceArguments);
            Close();
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
            Process.Start(@"http://code.google.com/p/gource/");
        }
    }
}