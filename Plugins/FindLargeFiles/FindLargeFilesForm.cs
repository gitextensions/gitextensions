using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using GitUIPluginInterfaces;

namespace FindLargeFiles
{
    public sealed partial class FindLargeFilesForm : Form
    {
        private readonly float threshold = 1.0f;
        private readonly GitUIBaseEventArgs gitUiCommands;
        private readonly IGitModule gitCommands;
        private string[] revList;
        private readonly Dictionary<string, GitObject> list = new Dictionary<string, GitObject>();
        private readonly SortableObjectsList gitObjects = new SortableObjectsList();

        public FindLargeFilesForm(float threshold, GitUIBaseEventArgs gitUiEventArgs)
        {
            InitializeComponent();

            this.threshold = threshold;
            this.gitUiCommands = gitUiEventArgs;
            this.gitCommands = gitUiEventArgs.GitModule;
        }

        private void findLargeFilesFunction()
        {
            try
            {
                var data = GetLargeFiles(threshold);
                Dictionary<string, DateTime> revData = new Dictionary<string, DateTime>();
                foreach (var d in data)
                {
                    string commit = d.Commit.First();
                    DateTime date;
                    if (!revData.ContainsKey(commit))
                    {
                        string revDate = gitCommands.RunGitCmd(string.Format("show -s {0} --format=\"%ci\"", commit));
                        DateTime.TryParse(revDate, out date);
                        revData.Add(commit, date);
                    }
                    else
                        date = revData[commit];
                    GitObject curGitObject;
                    if (!list.TryGetValue(d.SHA, out curGitObject))
                    {
                        d.LastCommitDate = date;
                        list.Add(d.SHA, d);
                        BranchesGrid.Invoke((Action)(() => { gitObjects.Add(d); }));
                    }
                    else if (!curGitObject.Commit.Contains(commit))
                    {
                        if (curGitObject.LastCommitDate < date)
                            curGitObject.LastCommitDate = date;
                        BranchesGrid.Invoke((Action)(() => { gitObjects.ResetItem(gitObjects.IndexOf(curGitObject)); }));
                        curGitObject.Commit.Add(commit);
                    }
                }
                string objectsPackDirectory = gitCommands.GetGitDirectory() + "objects/pack/";
                if (Directory.Exists(objectsPackDirectory))
                {
                    var packFiles = Directory.GetFiles(objectsPackDirectory, "pack-*.idx");
                    foreach (var pack in packFiles)
                    {
                        string[] objects = gitCommands.RunGitCmd(string.Concat("verify-pack -v ", pack)).Split('\n');
                        pbRevisions.Invoke((Action)(() => pbRevisions.Value = pbRevisions.Value + (int)((revList.Length * 0.1f) / packFiles.Length)));
                        foreach (var gitobj in objects.Where(x => x.Contains(" blob ")))
                        {
                            string[] dataFields = gitobj.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            GitObject curGitObject;
                            if (list.TryGetValue(dataFields[0], out curGitObject))
                            {
                                int compressedSize = 0;
                                if (Int32.TryParse(dataFields[3], out compressedSize))
                                {
                                    curGitObject.compressedSizeInBytes = compressedSize;
                                    BranchesGrid.Invoke((Action)(() => { gitObjects.ResetItem(gitObjects.IndexOf(curGitObject)); }));
                                }
                            }
                        }
                    }
                }
                pbRevisions.Invoke((Action)(() => pbRevisions.Hide()));
                BranchesGrid.Invoke((Action)(() => BranchesGrid.ReadOnly = false));
            }
            catch
            {
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            revList = gitCommands.RunGitCmd("rev-list HEAD").Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            pbRevisions.Maximum = (int)(revList.Length * 1.1f);
            BranchesGrid.DataSource = gitObjects;
            Thread MyThread = new Thread(findLargeFilesFunction);
            MyThread.Start();
        }

        private IEnumerable<GitObject> GetLargeFiles(float threshold)
        {
            int thresholdSize = (int)(threshold * 1024 * 1024);
            for ( int i = 0; i < revList.Length; i++ )
            {
                pbRevisions.Invoke((Action)(() => pbRevisions.Value = i));
                string rev = revList[i];
                string[] objects = gitCommands.RunGitCmd(string.Concat("ls-tree -zrl ", rev)).Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string objData in objects)
                {
                    // "100644 blob b17a497cdc6140aa3b9a681344522f44768165ac 2120195\tBin/Dictionaries/de-DE.dic"
                    var dataPack = objData.Split('\t');
                    var data = dataPack[0].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (data[1] == "blob")
                    {
                        int size = 0;
                        Int32.TryParse(data[3], out size);
                        if (size >= thresholdSize)
                            yield return new GitObject(data[2], dataPack[1], size, rev);
                    }
                }
            }
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "Are you sure to delete the selected files?", "Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                StringBuilder sb = new StringBuilder();
                foreach (GitObject gitObject in gitObjects.Where(gitObject => gitObject.Delete))
                {
                    sb.AppendLine(String.Format("\"{0}\" filter-branch --index-filter \"git rm -r -f --cached --ignore-unmatch {1}\" --prune-empty -- --all",
                        gitCommands.GitCommand, gitObject.Path));
                }
                sb.AppendLine(String.Format("for /f %%a IN ('\"{0}\" for-each-ref --format=%%^(refname^) refs/original/') DO \"{0}\" update-ref -d %%a",
                        gitCommands.GitCommand));
                sb.AppendLine(String.Format("\"{0}\" reflog expire --expire=now --all",
                    gitCommands.GitCommand));
                sb.AppendLine(String.Format("\"{0}\" gc --aggressive --prune=now",
                    gitCommands.GitCommand));
                gitUiCommands.GitUICommands.StartBatchFileProcessDialog(sb.ToString());
            }
            Close();
        }
    }
}
