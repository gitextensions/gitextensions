using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;

namespace GitUI
{
    public sealed partial class FormAddToGitIgnore : GitExtensionsForm
    {
        private readonly TranslationString _matchingFilesString = new TranslationString("{0} file(s) matched");

        public FormAddToGitIgnore(params string[] filePatterns)
        {
            InitializeComponent();
            Translate();
            if (filePatterns != null)
                FilePattern.Text = string.Join(Environment.NewLine, filePatterns);
            UpdatePreviewPanel();
        }

        private void AddToIngoreClick(object sender, EventArgs e)
        {
            var patterns = GetCurrentPatterns().ToArray();
            if (patterns.Length == 0)
            {
                Close();
                return;
            }

            try
            {
                var fileName = Settings.WorkingDir + ".gitignore";
                FileInfoExtensions.MakeFileTemporaryWritable(fileName, x =>
                {
                    var gitIgnoreFileAddition = new StringBuilder();

                    if (File.Exists(fileName) && !File.ReadAllText(fileName, Settings.SystemEncoding).EndsWith(Environment.NewLine))
                        gitIgnoreFileAddition.Append(Environment.NewLine);

                    foreach (var pattern in patterns)
                    {
                        gitIgnoreFileAddition.Append(pattern);
                        gitIgnoreFileAddition.Append(Environment.NewLine);
                    }

                    using (TextWriter tw = new StreamWriter(x, true, Settings.SystemEncoding))
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
            _NO_TRANSLATE_Preview.DataSource = Settings.Module.GetFiles(GetCurrentPatterns());
            _NO_TRANSLATE_filesWillBeIgnored.Text = string.Format(_matchingFilesString.Text, _NO_TRANSLATE_Preview.Items.Count);
            noMatchPanel.Visible = _NO_TRANSLATE_Preview.Items.Count == 0;
        }

        private IEnumerable<string> GetCurrentPatterns()
        {
            return FilePattern.Lines.Where(line => !string.IsNullOrEmpty(line));
        }

        private void FilePattern_TextChanged(object sender, EventArgs e)
        {
            UpdatePreviewPanel();
        }


    }
}