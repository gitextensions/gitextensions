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

        private void AddDefault_Click(object sender, EventArgs e)
        {
            GitIgnoreEdit.Text += "\n#ignore thumbnails created by windows\nThumbs.db#Ignore files build by Visual Studio\n*.obj\n*.exe\n*.pdb\n*.user\n*.aps\n*.pch\n*.vspscc\n*_i.c\n*_p.c\n*.ncb\n*.suo\n*.tlb\n*.tlh\n*.bak\n*.cache\n*.ilk\n*.log\n[Bb]in\n[Db]ebug*/\n*.lib\n*.sbr\nobj/\n[Rr]elease*/\n_ReSharper*/\n[Tt]est[Rr]esult*\n";
        }
    }
}
