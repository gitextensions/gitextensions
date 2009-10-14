using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using GitCommands;
using GitUIPluginInterfaces;

namespace GitUI
{
    public partial class FormResolveConflicts : GitExtensionsForm
    {
        public FormResolveConflicts()
        {
            InitializeComponent();
            ThereWhereMergeConflicts = GitCommands.GitCommands.InTheMiddleOfConflictedMerge();
        }

        private void Mergetool_Click(object sender, EventArgs e)
        {
            Directory.SetCurrentDirectory(GitCommands.Settings.WorkingDir);
            GitCommands.GitCommands.RunRealCmd(GitCommands.Settings.GitDir + "git.cmd", "mergetool");
            Initialize();
        }

        public bool ThereWhereMergeConflicts { get; set; }

        private void FormResolveConflicts_Load(object sender, EventArgs e)
        {
            Initialize();
        }

        private void Initialize()
        {
            button1.Focus();

            ConflictedFiles.DataSource = GitCommands.GitCommands.GetConflictedFiles();
            InitMergetool();

            if (!GitCommands.GitCommands.InTheMiddleOfPatch() && !GitCommands.GitCommands.InTheMiddleOfRebase() && !GitCommands.GitCommands.InTheMiddleOfConflictedMerge() && ThereWhereMergeConflicts)
            {
                if (MessageBox.Show("All mergeconflicts are resolved, you can commit." + Environment.NewLine + "Do you want to commit now?", "Commit", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    GitUICommands.Instance.StartCommitDialog();
                }

            }

            if (!GitCommands.GitCommands.InTheMiddleOfConflictedMerge() && ThereWhereMergeConflicts)
            {
                Close();
            }
        }

        private void Rescan_Click(object sender, EventArgs e)
        {
            Initialize();
        }

        private string mergetool;
        private string mergetoolCmd;
        private string mergetoolPath;

        private void ConflictedFiles_DoubleClick(object sender, EventArgs e)
        {
            if (ConflictedFiles.SelectedRows.Count != 1)
                return;

            DataGridViewRow row = ConflictedFiles.SelectedRows[0];

            string filename = GitCommands.GitCommands.GetConflictedFiles(((GitItem)row.DataBoundItem).FileName);

            if (Directory.Exists(Settings.WorkingDir + filename) && !File.Exists(Settings.WorkingDir + filename))
            {
                IList<IGitSubmodule> submodules = (new GitCommands.GitCommands()).GetSubmodules();
                foreach (IGitSubmodule submodule in submodules)
                {
                    if (submodule.LocalPath.Equals(filename))
                    {
                        if (MessageBox.Show("The selected mergeconflict is a submodule. Mark conflict as resolved?", "Merge", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            GitCommands.GitCommands.RunCmd(Settings.GitDir + "git.cmd", "add -- \"" + filename + "\"");
                            Initialize();
                        }
                        return;
                    }
                }
            }

            bool file1 = File.Exists(GitCommands.Settings.WorkingDir + filename + ".BASE");
            bool file2 = File.Exists(GitCommands.Settings.WorkingDir + filename + ".LOCAL");
            bool file3 = File.Exists(GitCommands.Settings.WorkingDir + filename + ".REMOTE");

            string arguments = mergetoolCmd;

            if (file1 && file2 && file3)
            {
                arguments = arguments.Replace("$BASE", filename + ".BASE");
                arguments = arguments.Replace("$LOCAL", filename + ".LOCAL");
                arguments = arguments.Replace("$REMOTE", filename + ".REMOTE");
                arguments = arguments.Replace("$MERGED", filename + "");

                GitCommands.GitCommands.RunCmd(mergetoolPath, "" + arguments + "");

                if (MessageBox.Show("Is the mergeconflict solved?", "Merge", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    GitCommands.GitCommands.RunCmd(Settings.GitDir + "git.cmd", "add -- \"" + filename + "\"");
                    Initialize();

                }
            }
            else
            {
                FormModifiedDeletedCreated frm = new FormModifiedDeletedCreated();
                if ((file1 && file2 && !file3) || (file1 && !file2 && file3))
                {
                    frm.Label.Text = "Use modified or deleted file?";
                    frm.Created.Text = "Modified";
                }
                else
                    if (!file1)
                    {
                        frm.Label.Text = "Use created or delete file?";
                    }
                    else
                    {
                        File.Delete(Settings.WorkingDir + filename + ".BASE");
                        File.Delete(Settings.WorkingDir + filename + ".LOCAL");
                        File.Delete(Settings.WorkingDir + filename + ".REMOTE");

                        Directory.SetCurrentDirectory(GitCommands.Settings.WorkingDir);
                        GitCommands.GitCommands.RunRealCmd(GitCommands.Settings.GitDir + "git.cmd", "mergetool \"" + filename + "\"");
                        Initialize();
                        return;
                    }

                frm.ShowDialog();

                if (frm.Aborted)
                {
                    File.Delete(Settings.WorkingDir + filename + ".BASE");
                    File.Delete(Settings.WorkingDir + filename + ".LOCAL");
                    File.Delete(Settings.WorkingDir + filename + ".REMOTE");
                    return;
                }
                else
                    if (frm.Delete)
                        GitCommands.GitCommands.RunCmd(Settings.GitDir + "git.cmd", "rm -- \"" + filename + "\"");
                    else
                        if (!frm.Delete)
                            GitCommands.GitCommands.RunCmd(Settings.GitDir + "git.cmd", "add -- \"" + filename + "\"");

                Initialize();
            }

            File.Delete(Settings.WorkingDir + filename + ".BASE");
            File.Delete(Settings.WorkingDir + filename + ".LOCAL");
            File.Delete(Settings.WorkingDir + filename + ".REMOTE");
        }

        private void InitMergetool()
        {
            GitCommands.GitCommands gitCommands = new GitCommands.GitCommands();

            mergetool = GitCommands.GitCommands.GetSetting("merge.tool");
            if (string.IsNullOrEmpty(mergetool))
                mergetool = gitCommands.GetGlobalSetting("merge.tool");

            if (string.IsNullOrEmpty(mergetool))
            {
                MessageBox.Show("There is no mergetool configured. Please go to settings and set a mergetool!");
                return;
            }

            mergetoolCmd = GitCommands.GitCommands.GetSetting("mergetool." + mergetool + ".cmd");
            if (string.IsNullOrEmpty(mergetoolCmd))
                mergetoolCmd = gitCommands.GetGlobalSetting("mergetool." + mergetool + ".cmd");

            mergetoolPath = GitCommands.GitCommands.GetSetting("mergetool." + mergetool + ".path");
            if (string.IsNullOrEmpty(mergetoolPath))
                mergetoolPath = gitCommands.GetGlobalSetting("mergetool." + mergetool + ".path");

            if (string.IsNullOrEmpty(mergetool) || mergetool == "kdiff3")
                mergetoolCmd = mergetoolPath + " \"$BASE\" \"$LOCAL\" \"$REMOTE\" -o \"$MERGED\"";

            mergetoolPath = mergetoolCmd.Substring(0, mergetoolCmd.IndexOf(".exe") + 5).Trim(new char[]{'\"', ' '});
            mergetoolCmd = mergetoolCmd.Substring(mergetoolCmd.IndexOf(".exe") + 5);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ConflictedFiles_DoubleClick(sender, e);
        }
    }
}
