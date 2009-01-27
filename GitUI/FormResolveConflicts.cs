using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace GitUI
{
    public partial class FormResolveConflicts : Form
    {
        public FormResolveConflicts()
        {
            InitializeComponent();
        }

        private void Mergetool_Click(object sender, EventArgs e)
        {
            Directory.SetCurrentDirectory(GitCommands.Settings.WorkingDir);
            GitCommands.GitCommands.RunRealCmd(GitCommands.Settings.GitDir + "git.cmd", "mergetool");
            Initialize();
        }

        private void FormResolveConflicts_Load(object sender, EventArgs e)
        {
            Initialize();
        }

        private void Initialize()
        {
            ConflictedFiles.DataSource = GitCommands.GitCommands.GetConflictedFiles();
        }

        private void Rescan_Click(object sender, EventArgs e)
        {
            Initialize();
        }
    }
}
