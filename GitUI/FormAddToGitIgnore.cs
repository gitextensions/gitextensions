using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public partial class FormAddToGitIgnore : GitExtensionsForm
    {
        public FormAddToGitIgnore(string filePattern)
        {
            InitializeComponent();
            Translate();
            FilePattern.Text = filePattern;
            Height = 100;
        }

        private void AddToIngoreClick(object sender, EventArgs e)
        {
            try
            {
                FileInfoExtensions
                    .MakeFileTemporaryWriteable(Settings.WorkingDir + ".gitignore",
                                       x =>
                                           {
                                               var gitIgnoreFile = new StringBuilder();
                                               gitIgnoreFile.Append(Environment.NewLine);
                                               gitIgnoreFile.Append(FilePattern.Text);

                                               using (TextWriter tw = new StreamWriter(x, true, Settings.Encoding))
                                               {
                                                   tw.Write(gitIgnoreFile);
                                                   tw.Close();
                                               }
                                           });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            Close();
        }

        private void ShowPreviewClick(object sender, EventArgs e)
        {
            Preview.DataSource = GitCommands.GitCommands.GetFiles(FilePattern.Text);

            if (Height < 110)
                Height = 300;
        }
    }
}