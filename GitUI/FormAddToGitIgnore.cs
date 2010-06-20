using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using GitCommands;

namespace GitUI
{
    public partial class FormAddToGitIgnore : GitExtensionsForm
    {
        public FormAddToGitIgnore(string filePattern)
        {
            InitializeComponent(); Translate();
            FilePattern.Text = filePattern;
            Height = 100;
        }

        private void AddToIngore_Click(object sender, EventArgs e)
        {
            try
            {
                using (TempRemoveFileAttributes tempRemoveFileAttributes = new TempRemoveFileAttributes(Settings.WorkingDir + ".gitignore"))
                {
                    StringBuilder gitIgnoreFile = new StringBuilder();
                    gitIgnoreFile.Append(Environment.NewLine);
                    gitIgnoreFile.Append(FilePattern.Text);

                    using (TextWriter tw = new StreamWriter(Settings.WorkingDir + ".gitignore", true, Settings.Encoding))
                    {
                        tw.Write(gitIgnoreFile);
                        tw.Close();
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            Close();
        }

        private void ShowPreview_Click(object sender, EventArgs e)
        {
            Preview.DataSource = GitCommands.GitCommands.GetFiles(FilePattern.Text);
            
            if (Height < 110)
                Height = 300;
        }
    }
}
