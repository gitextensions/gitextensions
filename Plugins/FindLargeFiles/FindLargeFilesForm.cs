using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using GitExtUtils.GitUI;
using GitUI;
using GitUIPluginInterfaces;
using ResourceManager;

namespace FindLargeFiles
{
    public sealed partial class FindLargeFilesForm : GitExtensionsFormBase
    {
        private readonly TranslationString _areYouSureToDelete = new TranslationString("Are you sure to delete the selected files?");
        private readonly TranslationString _deleteCaption = new TranslationString("Delete");

        private readonly float _threshold;
        private readonly GitUIEventArgs _gitUiCommands;
        private readonly IGitModule _gitCommands;
        private string[] _revList;
        private readonly Dictionary<string, GitObject> _list = new Dictionary<string, GitObject>();
        private readonly SortableObjectsList _gitObjects = new SortableObjectsList();

        public FindLargeFilesForm(float threshold, GitUIEventArgs gitUiEventArgs)
        {
            InitializeComponent();

            sHADataGridViewTextBoxColumn.Width = DpiUtil.Scale(54);
            sizeDataGridViewTextBoxColumn.Width = DpiUtil.Scale(52);
            commitCountDataGridViewTextBoxColumn.Width = DpiUtil.Scale(88);
            lastCommitDateDataGridViewTextBoxColumn.Width = DpiUtil.Scale(103);

            Translate();

            sHADataGridViewTextBoxColumn.DataPropertyName = nameof(GitObject.SHA);
            pathDataGridViewTextBoxColumn.DataPropertyName = nameof(GitObject.Path);
            sizeDataGridViewTextBoxColumn.DataPropertyName = nameof(GitObject.Size);
            CompressedSize.DataPropertyName = nameof(GitObject.CompressedSize);
            commitCountDataGridViewTextBoxColumn.DataPropertyName = nameof(GitObject.CommitCount);
            lastCommitDateDataGridViewTextBoxColumn.DataPropertyName = nameof(GitObject.LastCommitDate);
            dataGridViewCheckBoxColumn1.DataPropertyName = nameof(GitObject.Delete);

            _threshold = threshold;
            _gitUiCommands = gitUiEventArgs;
            _gitCommands = gitUiEventArgs?.GitModule;
        }

        private void FindLargeFilesFunction()
        {
            try
            {
                var data = GetLargeFiles(_threshold);
                Dictionary<string, DateTime> revData = new Dictionary<string, DateTime>();
                foreach (var d in data)
                {
                    string commit = d.Commit.First();
                    DateTime date;
                    if (!revData.ContainsKey(commit))
                    {
                        string revDate = _gitCommands.RunGitCmd(string.Format("show -s {0} --format=\"%ci\"", commit));
                        DateTime.TryParse(revDate, out date);
                        revData.Add(commit, date);
                    }
                    else
                    {
                        date = revData[commit];
                    }

                    if (!_list.TryGetValue(d.SHA, out var curGitObject))
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
                    var packFiles = Directory.GetFiles(objectsPackDirectory, "pack-*.idx");
                    foreach (var pack in packFiles)
                    {
                        string[] objects = _gitCommands.RunGitCmd(string.Concat("verify-pack -v ", pack)).Split('\n');
                        ThreadHelper.JoinableTaskFactory.Run(async () =>
                        {
                            await pbRevisions.SwitchToMainThreadAsync();
                            pbRevisions.Value = pbRevisions.Value + (int)((_revList.Length * 0.1f) / packFiles.Length);
                        });
                        foreach (var gitobj in objects.Where(x => x.Contains(" blob ")))
                        {
                            string[] dataFields = gitobj.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            if (_list.TryGetValue(dataFields[0], out var curGitObject))
                            {
                                if (int.TryParse(dataFields[3], out var compressedSize))
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
            _revList = _gitCommands.RunGitCmd("rev-list HEAD").Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            pbRevisions.Maximum = (int)(_revList.Length * 1.1f);
            BranchesGrid.DataSource = _gitObjects;
            Thread thread = new Thread(FindLargeFilesFunction);
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
                string[] objects = _gitCommands.RunGitCmd(string.Concat("ls-tree -zrl ", rev)).Split(new[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string objData in objects)
                {
                    // "100644 blob b17a497cdc6140aa3b9a681344522f44768165ac 2120195\tBin/Dictionaries/de-DE.dic"
                    var dataPack = objData.Split('\t');
                    var data = dataPack[0].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (data[1] == "blob")
                    {
                        int.TryParse(data[3], out var size);
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
            if (MessageBox.Show(this, _areYouSureToDelete.Text, _deleteCaption.Text, MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                StringBuilder sb = new StringBuilder();
                foreach (GitObject gitObject in _gitObjects.Where(gitObject => gitObject.Delete))
                {
                    sb.AppendLine(string.Format("\"{0}\" filter-branch --index-filter \"git rm -r -f --cached --ignore-unmatch {1}\" --prune-empty -- --all",
                        _gitCommands.GitCommand, gitObject.Path));
                }

                sb.AppendLine(string.Format("for /f %%a IN ('\"{0}\" for-each-ref --format=%%^(refname^) refs/original/') DO \"{0}\" update-ref -d %%a",
                    _gitCommands.GitCommand));
                sb.AppendLine(string.Format("\"{0}\" reflog expire --expire=now --all",
                    _gitCommands.GitCommand));
                sb.AppendLine(string.Format("\"{0}\" gc --aggressive --prune=now",
                    _gitCommands.GitCommand));
                _gitUiCommands.GitUICommands.StartBatchFileProcessDialog(sb.ToString());
            }

            Close();
        }
    }
}
