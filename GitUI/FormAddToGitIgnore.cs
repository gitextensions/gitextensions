using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;

namespace GitUI
{
    public partial class FormAddToGitIgnore : GitExtensionsForm
    {
        private readonly TranslationString _matchingFilesString =
            new TranslationString("{0} file(s) matched");
            
        public FormAddToGitIgnore(string filePattern)
        {
            InitializeComponent();
            Translate();
            FilePattern.Text = filePattern;
            UpdatePreviewPanel();
        }

        private void AddToIngoreClick(object sender, EventArgs e)
        {
            try
            {
                FileInfoExtensions
                    .MakeFileTemporaryWritable(Settings.WorkingDir + ".gitignore",
                                       x =>
                                       {
                                           var gitIgnoreFileAddition = new StringBuilder();
                                           gitIgnoreFileAddition.Append(FilePattern.Text);
                                           gitIgnoreFileAddition.Append(Environment.NewLine);

                                           if (File.Exists(Settings.WorkingDir + ".gitignore"))
                                               if (!File.ReadAllText(Settings.WorkingDir + ".gitignore", Settings.Encoding).EndsWith(Environment.NewLine))
                                                   gitIgnoreFileAddition.Insert(0, Environment.NewLine);

                                           using (TextWriter tw = new StreamWriter(x, true, Settings.Encoding))
                                           {
                                               tw.Write(gitIgnoreFileAddition);
                                           }
                                       });
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.ToString());
            }

            Close();
        }

        private void UpdatePreviewPanel()
        {
            Preview.DataSource = Settings.Module.GetFiles(FilePattern.Text);
            filesWillBeIgnored.Text = string.Format(_matchingFilesString.Text,Preview.Items.Count.ToString());
            noMatchPanel.Visible = (Preview.Items.Count == 0);
        }

        private void FilePattern_TextChanged(object sender, EventArgs e)
        {
            UpdatePreviewPanel();
        }
    }
}