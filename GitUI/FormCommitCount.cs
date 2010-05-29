using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace GitUI
{
    public partial class FormCommitCount : GitExtensionsForm
    {
        private readonly SynchronizationContext syncContext;

        public FormCommitCount()
        {
            syncContext = SynchronizationContext.Current;

            InitializeComponent();
        }

        ~FormCommitCount()
        {
            cmd.Kill();
        }

        private void FormCommitCount_FormClosing(object sender, FormClosingEventArgs e)
        {
            SavePosition("commit-count");
        }

        private GitCommands.GitCommands cmd = new GitCommands.GitCommands();
        private void FormCommitCount_Load(object sender, EventArgs e)
        {
            RestorePosition("commit-count");
            Loading.Visible = true;
            cmd.Exited += new EventHandler(cmd_Exited);
            //cmd.CmdStartProcess(GitCommands.Settings.GitCommand, "shortlog -s -n");
            cmd.CmdStartProcess("cmd.exe", "/c \"\"" + GitCommands.Settings.GitCommand + "\" log --all --pretty=short | \"" + GitCommands.Settings.GitCommand + "\" shortlog --all -s -n\"");
            
           
        }

        void SetCount()
        {
            CommitCount.Text = cmd.Output.ToString();
            Loading.Visible = false;
        }

        void cmd_Exited(object sender, EventArgs e)
        {
            syncContext.Post(_ => SetCount(), null);
        }
    }
}
