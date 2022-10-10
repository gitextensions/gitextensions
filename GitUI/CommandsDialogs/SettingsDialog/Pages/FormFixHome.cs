﻿using GitCommands;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class FormFixHome : GitExtensionsForm
    {
        private readonly TranslationString _gitGlobalConfigNotFound =
            new("The environment variable HOME does not point to a directory that contains the global git config file:" + Environment.NewLine +
                "\" {0} \"" + Environment.NewLine + Environment.NewLine + "Do you want Git Extensions to help locate the correct folder?");
        private readonly TranslationString _gitGlobalConfigNotFoundCaption =
            new("Global config");

        private readonly TranslationString _gitconfigFoundHome =
            new("Located .gitconfig in %HOME% ({0}). This setting has been chosen automatically.");
        private readonly TranslationString _gitconfigFoundHomedrive =
            new("Located .gitconfig in %HOMEDRIVE%%HOMEPATH% ({0}). This setting has been chosen automatically.");
        private readonly TranslationString _gitconfigFoundUserprofile =
            new("Located .gitconfig in %USERPROFILE% ({0}). This setting has been chosen automatically.");
        private readonly TranslationString _gitconfigFoundPersonalFolder =
            new("Located .gitconfig in personal folder ({0}). This setting has been chosen automatically.");

        private readonly TranslationString _noHomeDirectorySpecified =
            new("Please enter a HOME directory.");
        private readonly TranslationString _homeNotAccessible =
            new("The environment variable HOME points to a directory that is not accessible:" + Environment.NewLine +
                                "\"{0}\"");

        public FormFixHome()
        {
            InitializeComponent();
            Text = "Home";
            InitializeComplete();
        }

        private static bool IsFixHome()
        {
            try
            {
                string home = Environment.GetEnvironmentVariable("HOME");
                if (string.IsNullOrEmpty(home) || !Directory.Exists(home))
                {
                    return true;
                }

                try
                {
                    using FileStream fs = File.Open(Path.Combine(home, ".gitconfig"), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                    // file is readable, no further checks
                    return false;
                }
                catch (IOException)
                {
                }

                string[] candidates =
                {
                            Environment.GetEnvironmentVariable("HOME", EnvironmentVariableTarget.User),
                            Environment.GetEnvironmentVariable("HOMEDRIVE") + Environment.GetEnvironmentVariable("HOMEPATH"),
                            Environment.GetEnvironmentVariable("USERPROFILE"),
                            Environment.GetFolderPath(Environment.SpecialFolder.Personal)
                };

                foreach (string candidate in candidates)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(candidate) &&
                            File.Exists(Path.Combine(candidate, ".gitconfig")))
                        {
                            return true;
                        }
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }
            catch
            {
                // Exception occurred while checking for home dir.
                // Could be a security issue. Just return true to let the user fix
                // this manually.
                return true;
            }

            return false;
        }

        public void ShowIfUserWant()
        {
            if (MessageBox.Show(string.Format(_gitGlobalConfigNotFound.Text, Environment.GetEnvironmentVariable("HOME")),
                     _gitGlobalConfigNotFoundCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
            {
                ShowDialog();
            }
        }

        public static void CheckHomePath()
        {
            EnvironmentConfiguration.SetEnvironmentVariables();

            if (IsFixHome())
            {
                using FormFixHome frm = new();
                frm.ShowIfUserWant();
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            LoadSettings();

            defaultHome.Text = string.Format(defaultHome.Text + " ({0})", EnvironmentConfiguration.GetDefaultHomeDir());
            userprofileHome.Text = string.Format(userprofileHome.Text + " ({0})", Environment.GetEnvironmentVariable("USERPROFILE"));
        }

        private void LoadSettings()
        {
            if (!string.IsNullOrEmpty(AppSettings.CustomHomeDir))
            {
                defaultHome.Checked = userprofileHome.Checked = false;
                otherHome.Checked = true;
                otherHomeDir.Text = AppSettings.CustomHomeDir;
            }
            else if (AppSettings.UserProfileHomeDir)
            {
                defaultHome.Checked = otherHome.Checked = false;
                userprofileHome.Checked = true;
            }
            else
            {
                userprofileHome.Checked = otherHome.Checked = false;
                defaultHome.Checked = true;
            }

            try
            {
                string userHomeDir = Environment.GetEnvironmentVariable("HOME", EnvironmentVariableTarget.User);
                if (!string.IsNullOrEmpty(userHomeDir) && File.Exists(Path.Combine(userHomeDir, ".gitconfig")))
                {
                    MessageBox.Show(this, string.Format(_gitconfigFoundHome.Text, userHomeDir), "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    defaultHome.Checked = true;
                    return;
                }
            }
            catch
            {
                // Exception occurred while checking for home dir.
                // Could be a security issue. Just ignore and let the user choose
                // manually.
            }

            try
            {
                var path = Environment.GetEnvironmentVariable("HOMEDRIVE") +
                           Environment.GetEnvironmentVariable("HOMEPATH");
                if (!string.IsNullOrEmpty(path) && File.Exists(Path.Combine(path, ".gitconfig")))
                {
                    MessageBox.Show(this, string.Format(_gitconfigFoundHomedrive.Text, path), "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    defaultHome.Checked = true;
                    return;
                }
            }
            catch
            {
                // Exception occurred while checking for home dir.
                // Could be a security issue. Just ignore and let the user choose
                // manually.
            }

            try
            {
                var path = Environment.GetEnvironmentVariable("USERPROFILE");
                if (!string.IsNullOrEmpty(path) && File.Exists(Path.Combine(path, ".gitconfig")))
                {
                    MessageBox.Show(this, string.Format(_gitconfigFoundUserprofile.Text, path), "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    userprofileHome.Checked = true;
                    return;
                }
            }
            catch
            {
                // Exception occurred while checking for home dir.
                // Could be a security issue. Just ignore and let the user choose
                // manually.
            }

            try
            {
                var path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                if (!string.IsNullOrEmpty(path) && File.Exists(Path.Combine(path, ".gitconfig")))
                {
                    MessageBox.Show(this, string.Format(_gitconfigFoundPersonalFolder.Text, Environment.GetFolderPath(Environment.SpecialFolder.Personal)),
                        "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    otherHome.Checked = true;
                    otherHomeDir.Text = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                }
            }
            catch
            {
                // Exception occurred while checking for home dir.
                // Could be a security issue. Just ignore and let the user choose
                // manually.
            }
        }

        private void ok_Click(object sender, EventArgs e)
        {
            if (otherHome.Checked)
            {
                if (string.IsNullOrEmpty(otherHomeDir.Text))
                {
                    MessageBox.Show(this, _noHomeDirectorySpecified.Text, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                AppSettings.CustomHomeDir = otherHomeDir.Text;
            }
            else
            {
                AppSettings.CustomHomeDir = "";
            }

            AppSettings.UserProfileHomeDir = userprofileHome.Checked;

            EnvironmentConfiguration.SetEnvironmentVariables();
            string path = Environment.GetEnvironmentVariable("HOME");
            if (!Directory.Exists(path) || string.IsNullOrEmpty(path))
            {
                MessageBox.Show(this, string.Format(_homeNotAccessible.Text, path), TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }

            Close();
        }

        private void otherHomeBrowse_Click(object sender, EventArgs e)
        {
            var userSelectedPath = OsShellUtil.PickFolder(this, Environment.GetEnvironmentVariable("USERPROFILE"));

            if (userSelectedPath is not null)
            {
                otherHomeDir.Text = userSelectedPath;
            }
        }
    }
}
