using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormAddToGitIgnore : GitModuleForm
    {
        private readonly TranslationString _addToLocalExcludeTitle = new TranslationString("Add file(s) to .git/info/exclude");
        private readonly TranslationString _matchingFilesString = new TranslationString("{0} file(s) matched");
        private readonly TranslationString _updateStatusString = new TranslationString("Updating ...");

        private readonly AsyncLoader _ignoredFilesLoader = new AsyncLoader();
        private readonly IFullPathResolver _fullPathResolver;
        private readonly bool _localExclude;

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormAddToGitIgnore()
        {
            InitializeComponent();
        }

        public FormAddToGitIgnore(GitUICommands commands, bool localExclude, params string[] filePatterns)
            : base(commands)
        {
            _localExclude = localExclude;

            InitializeComponent();
            InitializeComplete();

            if (localExclude)
            {
                Text = _addToLocalExcludeTitle.Text;
            }

            if (filePatterns != null)
            {
                FilePattern.Text = string.Join(Environment.NewLine, filePatterns);
            }

            _fullPathResolver = new FullPathResolver(() => Module.WorkingDir);
        }

        private string ExcludeFile
        {
            get
            {
                if (!_localExclude)
                {
                    return _fullPathResolver.Resolve(".gitignore");
                }
                else
                {
                    return Path.Combine(Module.ResolveGitInternalPath("info"), "exclude");
                }
            }
        }

        private void AddToIgnoreClick(object sender, EventArgs e)
        {
            var patterns = GetCurrentPatterns().ToArray();
            if (patterns.Length == 0)
            {
                Close();
                return;
            }

            try
            {
                var fileName = ExcludeFile;
                FileInfoExtensions.MakeFileTemporaryWritable(fileName, x =>
                {
                    var gitIgnoreFileAddition = new StringBuilder();

                    if (File.Exists(fileName) && !File.ReadAllText(fileName, GitModule.SystemEncoding).EndsWith(Environment.NewLine))
                    {
                        gitIgnoreFileAddition.Append(Environment.NewLine);
                    }

                    foreach (var pattern in patterns)
                    {
                        gitIgnoreFileAddition.Append(pattern);
                        gitIgnoreFileAddition.Append(Environment.NewLine);
                    }

                    using (TextWriter tw = new StreamWriter(x, true, GitModule.SystemEncoding))
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

        private void UpdatePreviewPanel(IReadOnlyList<string> ignoredFiles)
        {
            _NO_TRANSLATE_Preview.DataSource = ignoredFiles;
            _NO_TRANSLATE_filesWillBeIgnored.Text = string.Format(_matchingFilesString.Text, _NO_TRANSLATE_Preview.Items.Count);
            _NO_TRANSLATE_Preview.Enabled = true;
            noMatchPanel.Visible = _NO_TRANSLATE_Preview.Items.Count == 0;
        }

        private IEnumerable<string> GetCurrentPatterns()
        {
            return FilePattern.Lines.Where(line => !string.IsNullOrEmpty(line));
        }

        private void FilePattern_TextChanged(object sender, EventArgs e)
        {
            _ignoredFilesLoader.Cancel();
            if (_NO_TRANSLATE_Preview.Enabled)
            {
                _ignoredFilesLoader.Delay = TimeSpan.FromMilliseconds(300);
                _NO_TRANSLATE_filesWillBeIgnored.Text = _updateStatusString.Text;
                _NO_TRANSLATE_Preview.DataSource = new List<string> { _updateStatusString.Text };
                _NO_TRANSLATE_Preview.Enabled = false;
            }

            _ignoredFilesLoader.LoadAsync(() => Module.GetIgnoredFiles(GetCurrentPatterns()), UpdatePreviewPanel);
        }
    }
}