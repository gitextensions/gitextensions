using System;
using System.Collections.Generic;
using GitCommands;
using Microsoft.Win32;
using System.IO;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class SshSettingsPage : SettingsPageBase
    {
        readonly CommonLogic _commonLogic;

        public SshSettingsPage(CommonLogic commonLogic)
        {
            InitializeComponent();
            Text = "SSH";
            Translate();
            _commonLogic = commonLogic;
        }

        protected override string GetCommaSeparatedKeywordList()
        {
            return "plink,putty,openssh,pageant";
        }

        public static SettingsPageReference GetPageReference()
        {
            return new SettingsPageReferenceByType(typeof(SshSettingsPage));
        }

        protected override void OnLoadSettings()
        {
            PlinkPath.Text = Settings.Plink;
            PuttygenPath.Text = Settings.Puttygen;
            PageantPath.Text = Settings.Pageant;
            AutostartPageant.Checked = Settings.AutoStartPageant;

            if (string.IsNullOrEmpty(GitCommandHelpers.GetSsh()))
                OpenSSH.Checked = true;
            else if (GitCommandHelpers.Plink())
                Putty.Checked = true;
            else
            {
                OtherSsh.Text = GitCommandHelpers.GetSsh();
                Other.Checked = true;
            }

            EnableSshOptions();
        }

        public override void SaveSettings()
        {
            Settings.Plink = PlinkPath.Text;
            Settings.Puttygen = PuttygenPath.Text;
            Settings.Pageant = PageantPath.Text;
            Settings.AutoStartPageant = AutostartPageant.Checked;

            if (OpenSSH.Checked)
                GitCommandHelpers.UnsetSsh();

            if (Putty.Checked)
                GitCommandHelpers.SetSsh(PlinkPath.Text);

            if (Other.Checked)
                GitCommandHelpers.SetSsh(OtherSsh.Text);            
        }

        private void OpenSSH_CheckedChanged(object sender, EventArgs e)
        {
            EnableSshOptions();
        }

        private void Putty_CheckedChanged(object sender, EventArgs e)
        {
            if (Putty.Checked)
            {
                AutoFindPuttyPaths();
            }
            EnableSshOptions();
        }

        private IEnumerable<string> GetPuttyLocations()
        {
            string programFiles = Environment.GetEnvironmentVariable("ProgramFiles");
            string programFilesX86 = null;
            if (8 == IntPtr.Size
                || !String.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432")))
                programFilesX86 = Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            yield return programFiles + @"\PuTTY\";
            if (programFilesX86 != null)
                yield return programFilesX86 + @"\PuTTY\";
            yield return programFiles + @"\TortoiseGit\bin\";
            if (programFilesX86 != null)
                yield return programFilesX86 + @"\TortoiseGit\bin\";
            yield return programFiles + @"\TortoiseSvn\bin\";
            if (programFilesX86 != null)
                yield return programFilesX86 + @"\TortoiseSvn\bin\";
            yield return _commonLogic.GetRegistryValue(Registry.LocalMachine,
                                                        "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\PuTTY_is1",
                                                        "InstallLocation");
            yield return Path.Combine(Settings.GetInstallDir(), @"PuTTY\");
        }

        public bool AutoFindPuttyPaths()
        {
            if (!Settings.RunningOnWindows())
                return false;

            foreach (var path in GetPuttyLocations())
            {
                if (AutoFindPuttyPathsInDir(path))
                    return true;
            }
            return false;
        }

        private bool AutoFindPuttyPathsInDir(string installdir)
        {
            if (!installdir.EndsWith("\\"))
                installdir += "\\";

            if (!File.Exists(PlinkPath.Text))
            {
                if (File.Exists(installdir + "plink.exe"))
                    PlinkPath.Text = installdir + "plink.exe";
                if (!File.Exists(PlinkPath.Text))
                {
                    if (File.Exists(installdir + "TortoisePlink.exe"))
                        PlinkPath.Text = installdir + "TortoisePlink.exe";
                }
            }

            if (!File.Exists(PuttygenPath.Text))
            {
                if (File.Exists(installdir + "puttygen.exe"))
                    PuttygenPath.Text = installdir + "puttygen.exe";
            }

            if (!File.Exists(PageantPath.Text))
            {
                if (File.Exists(installdir + "pageant.exe"))
                    PageantPath.Text = installdir + "pageant.exe";
            }

            if (File.Exists(PlinkPath.Text) && File.Exists(PuttygenPath.Text) && File.Exists(PageantPath.Text))
                return true;
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
            OtherSsh.Text = _commonLogic.SelectFile(".", "Executable file (*.exe)|*.exe", OtherSsh.Text);
        }

        private void PuttyBrowse_Click(object sender, EventArgs e)
        {
            PlinkPath.Text = _commonLogic.SelectFile(".",
                                        "Plink.exe (plink.exe)|plink.exe|TortoisePlink.exe (tortoiseplink.exe)|tortoiseplink.exe",
                                        PlinkPath.Text);
        }

        private void PuttygenBrowse_Click(object sender, EventArgs e)
        {
            PuttygenPath.Text = _commonLogic.SelectFile(".", "puttygen.exe (puttygen.exe)|puttygen.exe", PuttygenPath.Text);
        }

        private void PageantBrowse_Click(object sender, EventArgs e)
        {
            PageantPath.Text = _commonLogic.SelectFile(".", "pageant.exe (pageant.exe)|pageant.exe", PageantPath.Text);
        }
    }
}
