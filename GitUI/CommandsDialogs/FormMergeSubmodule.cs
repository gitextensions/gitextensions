using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormMergeSubmodule : GitModuleForm
    {
        string filename;
        GitModule submodule; 

        public FormMergeSubmodule(GitUICommands aCommands, string filename)
            : base(aCommands)
        {
            InitializeComponent();
            Translate();
            lbSubmodule.Text = filename;
            this.filename = filename;

            this.submodule = Module.GetSubmodule(filename);
        }

        private void FormMergeSubmodule_Load(object sender, EventArgs e)
        {
            string[] hashes = Module.GetConflictedSubmoduleHashes(filename);
            this.tbBase.Text = hashes[0];
            this.tbLocal.Text = hashes[1];
            this.tbRemote.Text = hashes[2];
            this.tbCurrent.Text = submodule.GetCurrentCheckout();
        }

        private void btRefresh_Click(object sender, EventArgs e)
        {
            this.tbCurrent.Text = submodule.GetCurrentCheckout();
        }

        private void btStageCurrent_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;

            this.Close();
        }

        private void btOpenSubmodule_Click(object sender, EventArgs e)
        {
            Process process = new Process();
            process.StartInfo.FileName = Application.ExecutablePath;
            process.StartInfo.Arguments = "browse";
            process.StartInfo.WorkingDirectory = Path.Combine(Module.WorkingDir, filename + AppSettings.PathSeparator.ToString());
            process.Start();
        }


    }
}