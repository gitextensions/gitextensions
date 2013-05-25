﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using GitUIPluginInterfaces;

namespace Gource
{
    public partial class GourceStart : Form
    {
        public GourceStart(string pathToGource, GitUIBaseEventArgs gitUIArgs, string gourceArguments)
        {
            InitializeComponent();
            PathToGource = pathToGource;
            GitUIArgs = gitUIArgs;
            GitWorkingDir = gitUIArgs.GitModule.GitWorkingDir;
            AvatarsDir = gitUIArgs.GitModule.GravatarCacheDir;
            GourceArguments = gourceArguments;

            WorkingDir.Text = GitWorkingDir;
            GourcePath.Text = pathToGource;
            Arguments.Text = GourceArguments;
        }

        private GitUIBaseEventArgs GitUIArgs { get; set; }

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
            var lines = GitUIArgs.GitModule.RunGit("log --pretty=format:\"%aE|%aN\"").Split('\n');
            HashSet<string> authors = new HashSet<string>();
            foreach (var line in lines)
            {
                var data = line.Split('|');
                var email = data[0];
                var author = data[1];
                if (!authors.Contains(author))
                {
                    authors.Add(author);
                    string source = Path.Combine(AvatarsDir, email + ".png");
                    GitUIArgs.GitUICommands.CacheAvatar(email);
                    if (File.Exists(source))
                    {
                        try
                        {
                            File.Copy(source, Path.Combine(gourceAvatarsDir, author + ".png"), true);
                        }
                        catch (IOException)
                        {
                        }
                    }
                }
            }
            return gourceAvatarsDir;
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