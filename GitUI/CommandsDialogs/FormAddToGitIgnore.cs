using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using GitCommands;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormAddToGitIgnore : GitModuleForm
    {
        private readonly TranslationString _matchingFilesString = new TranslationString("{0} file(s) matched");

        private readonly ManualResetEvent _notifyThread;
        private volatile bool _stopUpdateThread;

        public FormAddToGitIgnore(GitUICommands aCommands, params string[] filePatterns)
            : base(aCommands)
        {
            InitializeComponent();
            Translate();

            // closed by thread
            _notifyThread = new ManualResetEvent(false);

            // when form is closed - notify thread for stop activity, but do not wait for actual stop
            // waithing can lead to deadlocks, because thread uses SyncContext.Send
            FormClosed += (sender, args) => {
                _stopUpdateThread = true;
                _notifyThread.Set();
            };

            new Thread(UpdateIgnoreItemsThreadProc).Start();

            if (filePatterns != null)
                FilePattern.Text = string.Join(Environment.NewLine, filePatterns);
        }

        void UpdateIgnoreItemsThreadProc()
        {
            while (true)
            {
                _notifyThread.WaitOne();
                _notifyThread.Reset();

                // check if requested quit
                if (_stopUpdateThread)
                    break;

                string[] patterns = null;

                // switch UI to 'refreshing' state and retrieve patterns
                GitUIExtensions.UISynchronizationContext.Send(state => {
                    _NO_TRANSLATE_filesWillBeIgnored.Text = "Updating ...";
                    _NO_TRANSLATE_Preview.DataSource = new List<string> { "Updating ..." };
                    _NO_TRANSLATE_Preview.Enabled = false;

                    patterns = GetCurrentPatterns().ToArray();
                }, null);

                // check quit condition after each long work
                if (_stopUpdateThread)
                    break;

                var ignoredFiles = Module.GetIgnoredFiles(patterns);

                if (_stopUpdateThread)
                    break;

                // if client request one more refresh - do not update UI and go one more cycle
                if (_notifyThread.WaitOne(0))
                    continue;

                // finally - update UI
                GitUIExtensions.UISynchronizationContext.Post(state => UpdatePreviewPanel((IList<string>)state), ignoredFiles);
            }

            _notifyThread.Close();
        }

        protected override void OnRuntimeLoad(EventArgs e)
        {
            base.OnRuntimeLoad(e);

            // refresh on start
            _notifyThread.Set();
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
                var fileName = Module.WorkingDir + ".gitignore";
                FileInfoExtensions.MakeFileTemporaryWritable(fileName, x =>
                {
                    var gitIgnoreFileAddition = new StringBuilder();

                    if (File.Exists(fileName) && !File.ReadAllText(fileName, GitModule.SystemEncoding).EndsWith(Environment.NewLine))
                        gitIgnoreFileAddition.Append(Environment.NewLine);

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

        private void UpdatePreviewPanel(IList<string> ignoredFiles)
        {
            // thread can call this method after from closed. prevent access to disposed controls
            if (_stopUpdateThread)
                return;

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
            _notifyThread.Set();
        }


    }
}