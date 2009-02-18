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
    public partial class FormGitIgnore : GitExtensionsForm
    {
        public FormGitIgnore()
        {
            InitializeComponent();
            GitIgnoreFile = "";

            try
            {
                StreamReader re = File.OpenText(Settings.WorkingDir + ".gitignore");
                GitIgnoreFile = re.ReadToEnd();
                re.Close();

                GitIgnoreEdit.Text = GitIgnoreFile;
            }
            catch
            {
            }
        }



        public string GitIgnoreFile;

        private void Save_Click(object sender, EventArgs e)
        {
            GitIgnoreFile = GitIgnoreEdit.Text;
            TextWriter tw = new StreamWriter(Settings.WorkingDir + ".gitignore", false);
            tw.Write(GitIgnoreFile);
            tw.Close();
            Close();

        }

        private void FormGitIgnore_Load(object sender, EventArgs e)
        {
            if (Settings.IsBareRepository())
            {
                MessageBox.Show(".gitignore is only supported when there is a working dir.");
                Close();
            }
        }
    }
}
