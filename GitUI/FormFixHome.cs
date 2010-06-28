using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using System.IO;

namespace GitUI
{
    public partial class FormFixHome : GitExtensionsForm
    {
        public static void CheckHomePath()
        {
            GitCommands.GitCommands.SetEnvironmentVariable();
            if (!Directory.Exists(Environment.GetEnvironmentVariable("HOME")) ||
                string.IsNullOrEmpty(Environment.GetEnvironmentVariable("HOME")) ||
                !File.Exists(Environment.GetEnvironmentVariable("HOME") + "\\.gitconfig"))
            {
                if (MessageBox.Show("The environment variable HOME does not point to a directory that contains the global git config file:" + Environment.NewLine +
                                "\"" + Environment.GetEnvironmentVariable("HOME") + "\"" + Environment.NewLine + Environment.NewLine +
                                "Do you want Git Extensions to help locate the correct folder?", "Global config", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    new FormFixHome().ShowDialog();
            }
        }


        public FormFixHome()
        {
            InitializeComponent(); Translate();

        }

        protected override void OnLoad(EventArgs e)
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            if (!string.IsNullOrEmpty(Settings.CustomHomeDir))
            {
                defaultHome.Checked = userprofileHome.Checked = false;
                otherHome.Checked = true;
                otherHomeDir.Text = Settings.CustomHomeDir;
            }
            else if (Settings.UserProfileHomeDir)
            {
                defaultHome.Checked = otherHome.Checked = false;
                userprofileHome.Checked = true;
            }
            else
            {
                userprofileHome.Checked = otherHome.Checked = false;
                defaultHome.Checked = true;
            }

            if (File.Exists(Environment.GetEnvironmentVariable("HOME", EnvironmentVariableTarget.User) + "\\.gitconfig"))
            {
                MessageBox.Show("Located .gitconfig in %HOME% (" + Environment.GetEnvironmentVariable("HOME", EnvironmentVariableTarget.User) + "). This setting has been chosen automatically.");
                defaultHome.Checked = true;
            }
            else
                if (File.Exists(Environment.GetEnvironmentVariable("HOMEDRIVE") + Environment.GetEnvironmentVariable("HOMEPATH") + "\\.gitconfig"))
                {
                    MessageBox.Show("Located .gitconfig in %HOMEDRIVE%%HOMEPATH% (" + Environment.GetEnvironmentVariable("HOMEDRIVE") + Environment.GetEnvironmentVariable("HOMEPATH") + "). This setting has been chosen automatically.");
                    defaultHome.Checked = true;
                }
                else
                    if (File.Exists(Environment.GetEnvironmentVariable("USERPROFILE") + "\\.gitconfig"))
                    {
                        MessageBox.Show("Located .gitconfig in %USERPROFILE% (" + Environment.GetEnvironmentVariable("USERPROFILE") + "). This setting has been chosen automatically.");
                        userprofileHome.Checked = true;
                    }
        }

        private void ok_Click(object sender, EventArgs e)
        {

            if (otherHome.Checked)
            {
                if (string.IsNullOrEmpty(otherHomeDir.Text))
                {
                    MessageBox.Show("Please enter a HOME directory.");
                    return;
                }
                Settings.CustomHomeDir = otherHomeDir.Text;
            }
            else
                Settings.CustomHomeDir = "";

            Settings.UserProfileHomeDir = userprofileHome.Checked;

            GitCommands.GitCommands.SetEnvironmentVariable(true);
            if (!Directory.Exists(Environment.GetEnvironmentVariable("HOME")) || string.IsNullOrEmpty(Environment.GetEnvironmentVariable("HOME")))
            {
                MessageBox.Show("The environment variable HOME point to a directory that is not accessible:" + Environment.NewLine +
                                "\"" + Environment.GetEnvironmentVariable("HOME") + "\"");

                return;
            }

            Close();
        }
    }
}
