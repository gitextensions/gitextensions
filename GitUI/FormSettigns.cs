using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GitUI
{
    public partial class FormSettigns : Form
    {
        public FormSettigns()
        {
            InitializeComponent();

            GitCommands.GitCommands gitCommands = new GitCommands.GitCommands();

            UserName.Text = GitCommands.GitCommands.GetSetting("user.name");
            UserEmail.Text = GitCommands.GitCommands.GetSetting("user.email");
            Editor.Text = GitCommands.GitCommands.GetSetting("core.editor");
            MergeTool.Text = GitCommands.GitCommands.GetSetting("merge.tool");
            KeepMergeBackup.Checked = GitCommands.GitCommands.GetSetting("merge.keepBackup").Trim() == "true";
            

            GlobalUserName.Text = gitCommands.GetGlobalSetting("user.name");
            GlobalUserEmail.Text = gitCommands.GetGlobalSetting("user.email");
            GlobalEditor.Text = gitCommands.GetGlobalSetting("core.editor");
            GlobalMergeTool.Text = gitCommands.GetGlobalSetting("merge.tool");
            GlobalKeepMergeBackup.Checked = gitCommands.GetGlobalSetting("merge.keepBackup").Trim() == "true";
        }

        private void UserName_TextChanged(object sender, EventArgs e)
        {
        }

        private void UserEmail_TextChanged(object sender, EventArgs e)
        {
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            GitCommands.GitCommands gitCommands = new GitCommands.GitCommands();

            GitCommands.GitCommands.SetSetting("user.name", UserName.Text);
            GitCommands.GitCommands.SetSetting("user.email", UserEmail.Text);
            GitCommands.GitCommands.SetSetting("core.editor", Editor.Text);
            GitCommands.GitCommands.SetSetting("merge.tool", MergeTool.Text);
            if (KeepMergeBackup.Checked)
                GitCommands.GitCommands.SetSetting("merge.keepBackup", "true");
            else
                GitCommands.GitCommands.SetSetting("merge.keepBackup", "false");


            gitCommands.SetGlobalSetting("user.name", GlobalUserName.Text);
            gitCommands.SetGlobalSetting("user.email", GlobalUserEmail.Text);
            gitCommands.SetGlobalSetting("core.editor", GlobalEditor.Text);
            gitCommands.SetGlobalSetting("merge.tool", GlobalMergeTool.Text);

            if (GlobalKeepMergeBackup.Checked)
                gitCommands.SetGlobalSetting("merge.keepBackup", "true");
            else
                gitCommands.SetGlobalSetting("merge.keepBackup", "false");

            Close();
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
