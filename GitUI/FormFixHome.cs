using System;
using System.IO;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public partial class FormFixHome : GitExtensionsForm
    {
        private static bool IsFixHome()
        {
            try
            {
                string home = Environment.GetEnvironmentVariable("HOME");
                if (string.IsNullOrEmpty(home) || !Directory.Exists(home))
                    return true;

                if (File.Exists(Path.Combine(home, ".gitconfig")))
                    return false;

                string[] candidates = {
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
                            return true;
                    }
                    catch
                    {
                        //ignore
                    }
                }
            }
            catch
            {
                //Exception occured while checking for home dir. 
                //Could be a security issue. Just return true to let the user fix
                //this manually.
                return true;
            }
            return false;
        }

        public static void CheckHomePath()
        {
            GitCommandHelpers.SetEnvironmentVariable();

            if (IsFixHome())
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

            defaultHome.Text = string.Format(defaultHome.Text + " ({0})", GitCommandHelpers.GetDefaultHomeDir());
            userprofileHome.Text = string.Format(userprofileHome.Text + " ({0})", Environment.GetEnvironmentVariable("USERPROFILE"));
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

            try
            {
                if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("HOME", EnvironmentVariableTarget.User)) && File.Exists(Environment.GetEnvironmentVariable("HOME", EnvironmentVariableTarget.User) + Settings.PathSeparator + ".gitconfig"))
                {
                    MessageBox.Show("Located .gitconfig in %HOME% (" + Environment.GetEnvironmentVariable("HOME", EnvironmentVariableTarget.User) + "). This setting has been chosen automatically.");
                    defaultHome.Checked = true;
                    return;
                }
            }
            catch
            {
                //Exception occured while checking for home dir. 
                //Could be a security issue. Just ignore and let the user choose
                //manually.
            }
            try
            {
                if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("HOMEDRIVE") + Environment.GetEnvironmentVariable("HOMEPATH")) && File.Exists(Environment.GetEnvironmentVariable("HOMEDRIVE") + Environment.GetEnvironmentVariable("HOMEPATH") + Settings.PathSeparator + ".gitconfig"))
                {
                    MessageBox.Show("Located .gitconfig in %HOMEDRIVE%%HOMEPATH% (" + Environment.GetEnvironmentVariable("HOMEDRIVE") + Environment.GetEnvironmentVariable("HOMEPATH") + "). This setting has been chosen automatically.");
                    defaultHome.Checked = true;
                    return;
                }
            }
            catch
            {
                //Exception occured while checking for home dir. 
                //Could be a security issue. Just ignore and let the user choose
                //manually.
            }
            try
            {
                if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("USERPROFILE")) && File.Exists(Environment.GetEnvironmentVariable("USERPROFILE") + Settings.PathSeparator + ".gitconfig"))
                {
                    MessageBox.Show("Located .gitconfig in %USERPROFILE% (" + Environment.GetEnvironmentVariable("USERPROFILE") + "). This setting has been chosen automatically.");
                    userprofileHome.Checked = true;
                    return;
                }
            }
            catch
            {
                //Exception occured while checking for home dir. 
                //Could be a security issue. Just ignore and let the user choose
                //manually.
            }
            try
            {
                if (!string.IsNullOrEmpty(Environment.GetFolderPath(Environment.SpecialFolder.Personal)) && File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + Settings.PathSeparator + ".gitconfig"))
                {
                    MessageBox.Show("Located .gitconfig in personal folder (" + Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "). This setting has been chosen automatically.");
                    otherHome.Checked = true;
                    otherHomeDir.Text = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                    return;
                }
                            }
            catch
            {
                //Exception occured while checking for home dir. 
                //Could be a security issue. Just ignore and let the user choose
                //manually.
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

            GitCommandHelpers.SetEnvironmentVariable(true);
            if (!Directory.Exists(Environment.GetEnvironmentVariable("HOME")) || string.IsNullOrEmpty(Environment.GetEnvironmentVariable("HOME")))
            {
                MessageBox.Show("The environment variable HOME points to a directory that is not accessible:" + Environment.NewLine +
                                "\"" + Environment.GetEnvironmentVariable("HOME") + "\"");

                return;
            }

            Close();
        }

        private void otherHomeBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog browseDialog = new FolderBrowserDialog();
            browseDialog.SelectedPath = Environment.GetEnvironmentVariable("USERPROFILE");

            if (browseDialog.ShowDialog() == DialogResult.OK)
            {
                otherHomeDir.Text = browseDialog.SelectedPath;
            }
        }
    }
}
