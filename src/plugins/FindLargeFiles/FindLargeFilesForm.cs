using System.Text;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitExtUtils.GitUI;
using GitUI;
using ResourceManager;

namespace GitExtensions.Plugins.FindLargeFiles
{
    public sealed partial class FindLargeFilesForm : GitExtensionsFormBase
    {
        private readonly TranslationString _areYouSureToDelete = new("Are you sure to delete the selected files?");
        private readonly TranslationString _deleteCaption = new("Delete");

        private readonly float _threshold;
        private readonly GitUIEventArgs _gitUiCommands;
        private readonly IGitModule _gitCommands;
        private string[] _revList = Array.Empty<string>();
        private readonly Dictionary<string, GitObject> _list = [];
        private readonly SortableObjectsList _gitObjects = [];

        public FindLargeFilesForm(float threshold, GitUIEventArgs gitUiEventArgs)
        {
            InitializeComponent();

            sHADataGridViewTextBoxColumn.Width = DpiUtil.Scale(54);
            sizeDataGridViewTextBoxColumn.Width = DpiUtil.Scale(52);
            commitCountDataGridViewTextBoxColumn.Width = DpiUtil.Scale(88);
            lastCommitDateDataGridViewTextBoxColumn.Width = DpiUtil.Scale(103);

            InitializeComplete();

            // To accommodate the translation app
            if (gitUiEventArgs is null)
            {
                return;
            }

            sHADataGridViewTextBoxColumn.DataPropertyName = nameof(GitObject.SHA);
            pathDataGridViewTextBoxColumn.DataPropertyName = nameof(GitObject.Path);
            sizeDataGridViewTextBoxColumn.DataPropertyName = nameof(GitObject.Size);
            CompressedSize.DataPropertyName = nameof(GitObject.CompressedSize);
            commitCountDataGridViewTextBoxColumn.DataPropertyName = nameof(GitObject.CommitCount);
            lastCommitDateDataGridViewTextBoxColumn.DataPropertyName = nameof(GitObject.LastCommitDate);
            dataGridViewCheckBoxColumn1.DataPropertyName = nameof(GitObject.Delete);

            _threshold = threshold;
            _gitUiCommands = gitUiEventArgs;
            _gitCommands = gitUiEventArgs.GitModule;
        }

        private void FindLargeFilesFunction()
        {
            try
            {
                IEnumerable<GitObject> data = GetLargeFiles(_threshold);
                Dictionary<string, DateTime> revData = [];
                foreach (GitObject d in data)
                {
                    string commit = d.Commit.First();
                    DateTime date;
                    if (!revData.ContainsKey(commit))
                    {
                        GitArgumentBuilder args = new("show")
                        {
                            "-s",
                            commit,
                            "--format=\"%ci\""
                        };
                        string revDate = _gitCommands.GitExecutable.GetOutput(args);
                        DateTime.TryParse(revDate, out date);
                        revData.Add(commit, date);
                    }
                    else
                    {
                        date = revData[commit];
                    }

                    if (!_list.TryGetValue(d.SHA, out GitObject curGitObject))
                    {
                        d.LastCommitDate = date;
                        _list.Add(d.SHA, d);
                        ThreadHelper.JoinableTaskFactory.Run(async () =>
                        {
                            await BranchesGrid.SwitchToMainThreadAsync();
                            _gitObjects.Add(d);
                        });
                    }
                    else if (!curGitObject.Commit.Contains(commit))
                    {
                        if (curGitObject.LastCommitDate < date)
                        {
                            curGitObject.LastCommitDate = date;
                        }

                        ThreadHelper.JoinableTaskFactory.Run(async () =>
                        {
                            await BranchesGrid.SwitchToMainThreadAsync();
                            _gitObjects.ResetItem(_gitObjects.IndexOf(curGitObject));
                        });
                        curGitObject.Commit.Add(commit);
                    }
                }

                string objectsPackDirectory = _gitCommands.ResolveGitInternalPath("objects/pack/");
                if (Directory.Exists(objectsPackDirectory))
                {
                    string[] packFiles = Directory.GetFiles(objectsPackDirectory, "pack-*.idx");
                    foreach (string pack in packFiles)
                    {
                        GitArgumentBuilder args = new("verify-pack")
                        {
                            "-v",
                            pack
                        };

                        string[] objects = _gitCommands.GitExecutable.GetOutput(args).Split('\n');
                        ThreadHelper.JoinableTaskFactory.Run(async () =>
                        {
                            await pbRevisions.SwitchToMainThreadAsync();
                            pbRevisions.Value = pbRevisions.Value + (int)((_revList.Length * 0.1f) / packFiles.Length);
                        });
                        foreach (string gitObject in objects.Where(x => x.Contains(" blob ")))
                        {
                            string[] dataFields = gitObject.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            if (_list.TryGetValue(dataFields[0], out GitObject curGitObject))
                            {
                                if (int.TryParse(dataFields[3], out int compressedSize))
                                {
                                    curGitObject.CompressedSizeInBytes = compressedSize;
                                    ThreadHelper.JoinableTaskFactory.Run(async () =>
                                    {
                                        await BranchesGrid.SwitchToMainThreadAsync();
                                        _gitObjects.ResetItem(_gitObjects.IndexOf(curGitObject));
                                    });
                                }
                            }
                        }
                    }
                }

                ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    await pbRevisions.SwitchToMainThreadAsync();
                    pbRevisions.Hide();
                });
                ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    await BranchesGrid.SwitchToMainThreadAsync();
                    BranchesGrid.ReadOnly = false;
                });
            }
            catch
            {
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            GitArgumentBuilder args = new("rev-list") { "HEAD" };
            _revList = _gitCommands.GitExecutable.GetOutput(args).Split('\n', StringSplitOptions.RemoveEmptyEntries);
            pbRevisions.Maximum = (int)(_revList.Length * 1.1f);
            BranchesGrid.DataSource = _gitObjects;
            Thread thread = new(FindLargeFilesFunction);
            thread.Start();
        }

        private IEnumerable<GitObject> GetLargeFiles(float threshold)
        {
            int thresholdSize = (int)(threshold * 1024 * 1024);
            for (int i = 0; i < _revList.Length; i++)
            {
                ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    await pbRevisions.SwitchToMainThreadAsync();
                    pbRevisions.Value = i;
                });
                string rev = _revList[i];
                GitArgumentBuilder args = new("ls-tree")
                {
                    "-zrl",
                    rev.Quote()
                };
                string[] objects = _gitCommands.GitExecutable.GetOutput(args).Split(new[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string objData in objects)
                {
                    // "100644 blob b17a497cdc6140aa3b9a681344522f44768165ac 2120195\tBin/Dictionaries/de-DE.dic"
                    string[] dataPack = objData.Split('\t');
                    string[] data = dataPack[0].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (data[1] == "blob")
                    {
                        int.TryParse(data[3], out int size);
                        if (size >= thresholdSize)
                        {
                            yield return new GitObject(data[2], dataPack[1], size, rev);
                        }
                    }
                }
            }
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, _areYouSureToDelete.Text, _deleteCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                StringBuilder sb = new();
                foreach (GitObject gitObject in _gitObjects.Where(gitObject => gitObject.Delete))
                {
                    sb.AppendLine(string.Format("\"{0}\" filter-branch --index-filter \"git rm -r -f --cached --ignore-unmatch {1}\" --prune-empty -- --all",
                        AppSettings.GitCommand, gitObject.Path));
                }

                sb.AppendLine(string.Format(@"for /f %%a IN ('""{0}"" for-each-ref --format=""%%^(refname^)"" refs/original/') DO ""{0}"" update-ref -d %%a",
                    AppSettings.GitCommand));
                sb.AppendLine(string.Format("\"{0}\" reflog expire --expire=now --all",
                    AppSettings.GitCommand));
                sb.AppendLine(string.Format("\"{0}\" gc --aggressive --prune=now",
                    AppSettings.GitCommand));
                _gitUiCommands.GitUICommands.StartBatchFileProcessDialog(sb.ToString());
            }

            Close();
        }
    }
}
