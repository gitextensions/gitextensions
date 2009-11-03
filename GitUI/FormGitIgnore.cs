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
                if (File.Exists(Settings.WorkingDir + ".gitignore"))
                {
                    using (StreamReader re = new StreamReader(Settings.WorkingDir + ".gitignore", Settings.Encoding))
                    {
                        GitIgnoreFile = re.ReadToEnd();
                        re.Close();
                    }
                }
                GitIgnoreEdit.Text = GitIgnoreFile;
            }
            catch
            {
            }
        }



        public string GitIgnoreFile;

        private void Save_Click(object sender, EventArgs e)
        {
            //Enter a newline to work around a wierd bug that causes the first line to include 3 extra bytes. (encoding marker??)
            GitIgnoreFile = Environment.NewLine + GitIgnoreEdit.Text.Trim();
            using (TextWriter tw = new StreamWriter(Settings.WorkingDir + ".gitignore", false, Settings.Encoding))
            {
                tw.Write(GitIgnoreFile);
                tw.Close();
            }
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
            GitIgnoreEdit.Text += Environment.NewLine + "#ignore thumbnails created by windows" + Environment.NewLine + "Thumbs.db" + Environment.NewLine + "#Ignore files build by Visual Studio" + Environment.NewLine + "*.obj" + Environment.NewLine + "*.exe" + Environment.NewLine + "*.pdb" + Environment.NewLine + "*.user" + Environment.NewLine + "*.aps" + Environment.NewLine + "*.pch" + Environment.NewLine + "*.vspscc" + Environment.NewLine + "*_i.c" + Environment.NewLine + "*_p.c" + Environment.NewLine + "*.ncb" + Environment.NewLine + "*.suo" + Environment.NewLine + "*.tlb" + Environment.NewLine + "*.tlh" + Environment.NewLine + "*.bak" + Environment.NewLine + "*.cache" + Environment.NewLine + "*.ilk" + Environment.NewLine + "*.log" + Environment.NewLine + "[Bb]in" + Environment.NewLine + "[Dd]ebug*/" + Environment.NewLine + "*.lib" + Environment.NewLine + "*.sbr" + Environment.NewLine + "obj/" + Environment.NewLine + "[Rr]elease*/" + Environment.NewLine + "_ReSharper*/" + Environment.NewLine + "[Tt]est[Rr]esult*" + Environment.NewLine + "";
        }
    }
}
