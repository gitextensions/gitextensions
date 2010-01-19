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
            InitializeComponent();
            FilePattern.Text = filePattern;
            Height = 100;
        }

        private void AddToIngore_Click(object sender, EventArgs e)
        {
            try
            {
                StringBuilder gitIgnoreFile = new StringBuilder();
                gitIgnoreFile.Append(Environment.NewLine);
                if (File.Exists(Settings.WorkingDir + ".gitignore"))
                {
                    using (StreamReader re = new StreamReader(Settings.WorkingDir + ".gitignore", Settings.Encoding))
                    {
                        gitIgnoreFile.Append(re.ReadToEnd());
                        re.Close();
                    }
                }

                gitIgnoreFile.Append(Environment.NewLine);
                gitIgnoreFile.Append(FilePattern.Text);

                using (TextWriter tw = new StreamWriter(Settings.WorkingDir + ".gitignore", false, Settings.Encoding))
                {
                    tw.Write(gitIgnoreFile);
                    tw.Close();
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
