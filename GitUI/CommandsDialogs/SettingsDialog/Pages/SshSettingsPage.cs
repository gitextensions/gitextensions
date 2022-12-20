using GitCommands;
using GitCommands.Utils;
using GitExtUtils.GitUI.Theming;
using Microsoft.Win32;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class SshSettingsPage : SettingsPageWithHeader
    {
        public SshSettingsPage()
        {
            InitializeComponent();
            Text = "SSH";
            InitializeComplete();

            label18.SetForeColorForBackColor();
        }

        public static SettingsPageReference GetPageReference()
        {
            return new SettingsPageReferenceByType(typeof(SshSettingsPage));
        }

        protected override void SettingsToPage()
        {
            PlinkPath.Text = AppSettings.Plink;
            PuttygenPath.Text = AppSettings.Puttygen;
            PageantPath.Text = AppSettings.Pageant;
            AutostartPageant.Checked = AppSettings.AutoStartPageant;

            var sshPath = AppSettings.SshPath;
            if (string.IsNullOrEmpty(sshPath))
            {
                OpenSSH.Checked = true;
            }
            else if (GitSshHelpers.IsPlink)
            {
                Putty.Checked = true;
            }
            else
            {
                OtherSsh.Text = sshPath;
                Other.Checked = true;
            }

            EnableSshOptions();

            base.SettingsToPage();
        }

        protected override void PageToSettings()
        {
            AppSettings.Plink = PlinkPath.Text;
            AppSettings.Puttygen = PuttygenPath.Text;
            AppSettings.Pageant = PageantPath.Text;
            AppSettings.AutoStartPageant = AutostartPageant.Checked;

            string path;
            if (OpenSSH.Checked)
            {
                path = "";
            }
            else if (Putty.Checked)
            {
                path = PlinkPath.Text;
            }
            else
            {
                // Other.Checked
                path = OtherSsh.Text;
            }

            // Set persistent settings as well as the env var used by Git
            GitSshHelpers.SetGitSshEnvironmentVariable(path);
            AppSettings.SshPath = path;

            base.PageToSettings();
        }

        private void OpenSSH_CheckedChanged(object sender, EventArgs e)
        {
            EnableSshOptions();
        }

        private void Putty_CheckedChanged(object sender, EventArgs e)
        {
            groupBox2.Visible = Putty.Checked;
            if (Putty.Checked)
            {
                AutoFindPuttyPaths();
            }

            EnableSshOptions();
        }

        private IEnumerable<string> GetPuttyLocations()
        {
            string envVariable = Environment.GetEnvironmentVariable("GITEXT_PUTTY");
            if (!string.IsNullOrEmpty(envVariable))
            {
                yield return envVariable;
            }

            string programFiles = Environment.GetEnvironmentVariable("ProgramFiles");
            string? programFilesX86 = (IntPtr.Size == 8
                || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432")))
                ? Environment.GetEnvironmentVariable("ProgramFiles(x86)")
                : null;

            yield return programFiles + @"\PuTTY\";
            if (programFilesX86 is not null)
            {
                yield return programFilesX86 + @"\PuTTY\";
            }

            yield return programFiles + @"\TortoiseGit\bin\";
            if (programFilesX86 is not null)
            {
                yield return programFilesX86 + @"\TortoiseGit\bin\";
            }

            yield return programFiles + @"\TortoiseSvn\bin\";
            if (programFilesX86 is not null)
            {
                yield return programFilesX86 + @"\TortoiseSvn\bin\";
            }

            // Old(?) uninstaller
            yield return CommonLogic.GetRegistryValue(Registry.LocalMachine,
                                                        "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\PuTTY_is1",
                                                        "InstallLocation");
        }

        public bool AutoFindPuttyPaths()
        {
            if (!EnvUtils.RunningOnWindows())
            {
                return false;
            }

            return GetPuttyLocations().Any(AutoFindPuttyPathsInDir);
        }

        private bool AutoFindPuttyPathsInDir(string installdir)
        {
            if (!installdir.EndsWith("\\"))
            {
                installdir += "\\";
            }

            if (!File.Exists(PlinkPath.Text))
            {
                if (File.Exists(installdir + "plink.exe"))
                {
                    PlinkPath.Text = installdir + "plink.exe";
                }
                else if (File.Exists(installdir + "TortoisePlink.exe"))
                {
                    PlinkPath.Text = installdir + "TortoisePlink.exe";
                }
            }

            if (!File.Exists(PuttygenPath.Text))
            {
                if (File.Exists(installdir + "puttygen.exe"))
                {
                    PuttygenPath.Text = installdir + "puttygen.exe";
                }
            }

            if (!File.Exists(PageantPath.Text))
            {
                if (File.Exists(installdir + "pageant.exe"))
                {
                    PageantPath.Text = installdir + "pageant.exe";
                }
            }

            if (File.Exists(PlinkPath.Text) && File.Exists(PuttygenPath.Text) && File.Exists(PageantPath.Text))
            {
                return true;
            }

            return false;
        }

        private void Other_CheckedChanged(object sender, EventArgs e)
        {
            EnableSshOptions();
        }

        private void EnableSshOptions()
        {
            OtherSsh.Enabled = Other.Checked;
            OtherSshBrowse.Enabled = Other.Checked;

            PlinkPath.Enabled = Putty.Checked;
            PuttygenPath.Enabled = Putty.Checked;
            PageantPath.Enabled = Putty.Checked;
            PlinkBrowse.Enabled = Putty.Checked;
            PuttygenBrowse.Enabled = Putty.Checked;
            PageantBrowse.Enabled = Putty.Checked;
            AutostartPageant.Enabled = Putty.Checked;
        }

        private void OtherSshBrowse_Click(object sender, EventArgs e)
        {
            OtherSsh.Text = CommonLogic.SelectFile(".", "Executable file (*.exe)|*.exe", OtherSsh.Text);
        }

        private void PuttyBrowse_Click(object sender, EventArgs e)
        {
            PlinkPath.Text = CommonLogic.SelectFile(".",
                                        "Plink (plink.exe)|plink.exe|TortoiseGitPLink (tortoisegitplink.exe)|tortoisegitplink.exe|TortoisePlink.exe (tortoiseplink.exe)|tortoiseplink.exe",
                                        PlinkPath.Text);
        }

        private void PuttygenBrowse_Click(object sender, EventArgs e)
        {
            PuttygenPath.Text = CommonLogic.SelectFile(".", "PuttyGen (puttygen.exe)|puttygen.exe", PuttygenPath.Text);
        }

        private void PageantBrowse_Click(object sender, EventArgs e)
        {
            PageantPath.Text = CommonLogic.SelectFile(".", "PAgeant (pageant.exe)|pageant.exe", PageantPath.Text);
        }
    }
}
