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
    public partial class FormClone : GitExtensionsForm
    {
        public FormClone()
        {
            InitializeComponent();

            if (Settings.ValidWorkingDir())
                From.Text = Settings.WorkingDir;
            else
                To.Text = Settings.WorkingDir;

            From_TextUpdate(null, null);
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            try
            {
                string dirTo = To.Text;
                if (!dirTo.EndsWith("\\") && !dirTo.EndsWith("/"))
                    dirTo += "\\";

                dirTo += NewDirectory.Text;

                RepositoryHistory.AddMostRecentRepository(From.Text);
                RepositoryHistory.AddMostRecentRepository(dirTo);

                //CloneDto dto = new CloneDto(From.Text, To.Text, CentralRepository.Checked);
                //GitCommands.Clone commit = new GitCommands.Clone(dto);
                //commit.Execute();

                FormProcess fromProcess;
                fromProcess = new FormProcess(Settings.GitDir + "git.cmd", GitCommands.GitCommands.CloneCmd(From.Text, dirTo, CentralRepository.Checked));

                if (!fromProcess.ErrorOccured() && !GitCommands.GitCommands.InTheMiddleOfPatch())
                {
                    if (this.ShowInTaskbar == false)
                    {
                        if (MessageBox.Show("The repository has been cloned successfully." + Environment.NewLine + "Do you want to open the new repository \"" + dirTo + "\" now?", "Open", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            GitCommands.Settings.WorkingDir = dirTo;

                            if (File.Exists(GitCommands.Settings.WorkingDir + ".gitmodules"))
                            {
                                if (MessageBox.Show("The cloned has submodules configured." + Environment.NewLine + "Do you want to initialize the submodules?" + Environment.NewLine + Environment.NewLine + "This will initialize and update all submodules recursive.", "Submodules", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                {
                                    FormProcess process = new FormProcess(GitCommands.GitCommands.SubmoduleInitCmd(""));
                                    InitializeSubmodulesRecursive();
                                }

                            }

                        }
                    }
                    Close();
                }
            }
            catch
            {
            }
        }

        private static void InitializeSubmodulesRecursive()
        {
            string oldworkingdir = Settings.WorkingDir;

            foreach (GitSubmodule submodule in (new GitCommands.GitCommands()).GetSubmodules())
            {
                if (!string.IsNullOrEmpty(submodule.LocalPath))
                {
                    Settings.WorkingDir = oldworkingdir + submodule.LocalPath;

                    if (Settings.WorkingDir != oldworkingdir && File.Exists(GitCommands.Settings.WorkingDir + ".gitmodules"))
                    {
                        FormProcess process = new FormProcess(GitCommands.GitCommands.SubmoduleInitCmd(""));

                        InitializeSubmodulesRecursive();
                    }

                    Settings.WorkingDir = oldworkingdir;
                }
            }

            Settings.WorkingDir = oldworkingdir;
        }

        private void FromBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.SelectedPath = From.Text;
            if (dialog.ShowDialog() == DialogResult.OK)
                From.Text = dialog.SelectedPath;
            
            To_TextUpdate(sender, e);
        }

        private void ToBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.SelectedPath = To.Text;
            if (dialog.ShowDialog() == DialogResult.OK)
                To.Text = dialog.SelectedPath;

            To_TextUpdate(sender, e); 
        }

        private void From_DropDown(object sender, EventArgs e)
        {
            From.DataSource = RepositoryHistory.MostRecentRepositories.ToArray();
        }

        private void To_DropDown(object sender, EventArgs e)
        {
            To.DataSource = RepositoryHistory.MostRecentRepositories.ToArray();
        }

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void LoadSSHKey_Click(object sender, EventArgs e)
        {
            new FormLoadPuttySSHKey().ShowDialog();
        }

        private void FormClone_Load(object sender, EventArgs e)
        {
            if (!GitCommands.GitCommands.Plink())
                LoadSSHKey.Visible = false;
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void From_SelectedIndexChanged(object sender, EventArgs e)
        {
            From_TextUpdate(sender, e);
        }

        private void From_TextUpdate(object sender, EventArgs e)
        {
            string path = From.Text;
            path = path.TrimEnd(new char[] { '\\', '/' });
            
            if (path.EndsWith(".git"))
                path = path.Replace(".git", "");

            if (path.Contains("\\") || path.Contains("/"))
                NewDirectory.Text = path.Substring(path.LastIndexOfAny(new char[] { '\\', '/' }) + 1);

            To_TextUpdate(sender, e);
        }

        private void To_TextUpdate(object sender, EventArgs e)
        {
            string destinationPath ="";

            Info.Text = "The repository will be cloned to a new directory located here:" + Environment.NewLine;
            if (string.IsNullOrEmpty(To.Text))
                destinationPath += "[destination]";
            else
                destinationPath += To.Text.TrimEnd(new char[] { '\\', '/' }); ;
            destinationPath += "\\";

            if (string.IsNullOrEmpty(NewDirectory.Text))
                destinationPath += "[directory]";
            else
                destinationPath += NewDirectory.Text;

            Info.Text += "     " + destinationPath;

            if (destinationPath.Contains("[") || destinationPath.Contains("]"))
            {
                Info.ForeColor = Color.Red;
            }
            else
            {
                if (Directory.Exists(destinationPath))
                {
                    Info.Text += " (directory already exists)";
                    Info.ForeColor = Color.Red;
                }
                else
                {
                    Info.ForeColor = Color.Black;
                }
            }
        }

        private void NewDirectory_TextChanged(object sender, EventArgs e)
        {
            To_TextUpdate(sender, e);
        }

        private void To_SelectedIndexChanged(object sender, EventArgs e)
        {
            To_TextUpdate(sender, e);
        }
    }
}
