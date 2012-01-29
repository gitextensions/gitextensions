using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using PatchApply;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;

namespace GitUI
{
    public sealed partial class FormFileHistory : GitExtensionsForm
    {
        private readonly SynchronizationContext syncContext = SynchronizationContext.Current;

        public FormFileHistory(string fileName, GitRevision revision)
        {
            InitializeComponent();
            FileChanges.SetInitialRevision(revision);
            Translate();

            FileName = fileName;

            Diff.ExtraDiffArgumentsChanged += DiffExtraDiffArgumentsChanged;

            FileChanges.SelectionChanged += FileChangesSelectionChanged;
            FileChanges.DisableContextMenu();

            followFileHistoryToolStripMenuItem.Checked = Settings.FollowRenamesInFileHistory;
            fullHistoryToolStripMenuItem.Checked = Settings.FullHistoryInFileHistory;
        }

        public FormFileHistory(string fileName)
            : this(fileName, null)
        {
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ThreadPool.QueueUserWorkItem(o => LoadFileHistory(FileName));
        }

        private string FileName { get; set; }

        private void LoadFileHistory(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return;

            //Replace windows path seperator to linux path seperator. 
            //This is needed to keep the file history working when started from file tree in
            //browse dialog.
            fileName = fileName.Replace('\\', '/');

            //The section below contains native windows (kernel32) calls
            //and breaks on Linux. Only use it on Windows. Casing is only
            //a Windows problem anyway.
            if (Settings.RunningOnWindows())
            {
                // we will need this later to look up proper casing for the file
                string fullFilePath = fileName;

                if (!fileName.StartsWith(Settings.WorkingDir, StringComparison.InvariantCultureIgnoreCase))
                    fullFilePath = Path.Combine(Settings.WorkingDir, fileName);

                if (File.Exists(fullFilePath))
                {
                    // grab the 8.3 file path
                    var shortPath = new StringBuilder(4096);
                    NativeMethods.GetShortPathName(fullFilePath, shortPath, shortPath.Capacity);

                    // use 8.3 file path to get properly cased full file path
                    var longPath = new StringBuilder(4096);
                    NativeMethods.GetLongPathName(shortPath.ToString(), longPath, longPath.Capacity);

                    // remove the working dir and now we have a properly cased file name.
                    fileName = longPath.ToString().Substring(Settings.WorkingDir.Length);
                }
            }

            if (fileName.StartsWith(Settings.WorkingDir, StringComparison.InvariantCultureIgnoreCase))
                fileName = fileName.Substring(Settings.WorkingDir.Length);

            FileName = fileName;

            string filter;
            if (Settings.FollowRenamesInFileHistory)
            {
                // git log --follow is not working as expected (see  http://kerneltrap.org/mailarchive/git/2009/1/30/4856404/thread)
                //
                // But we can take a more complicated path to get reasonable results:
                //  1. use git log --follow to get all previous filenames of the file we are interested in
                //  2. use git log "list of filesnames" to get the histroy graph 
                //
                // note: This implementation is quite a quick hack (by someone who does not speak C# fluently).
                // 

                var gitGetGraphCommand = new GitCommandsInstance { StreamOutput = true, CollectOutput = false };

                string arg = "log --format=\"%n\" --name-only --follow -- \"" + fileName + "\"";
                Process p = gitGetGraphCommand.CmdStartProcess(Settings.GitCommand, arg);

                // the sequence of (quoted) file names - start with the initial filename for the search.
                string listOfFileNames = "\"" + fileName + "\"";

                // keep a set of the file names already seen
                var setOfFileNames = new HashSet<string> { fileName };

                string line;
                do
                {
                    line = p.StandardOutput.ReadLine();

                    if (!string.IsNullOrEmpty(line))
                    {
                        if (!setOfFileNames.Contains(line))
                        {
                            listOfFileNames = listOfFileNames + " \"" + line + "\"";
                            setOfFileNames.Add(line);
                        }
                    }
                } while (line != null);

                // here we need --name-only to get the previous filenames in the revision graph
                filter = " --name-only --parents -- " + listOfFileNames;
            }
            else
            {
                // --parents doesn't work with --follow enabled, but needed to graph a filtered log
                filter = " --parents -- \"" + fileName + "\"";
            }

            if (Settings.FullHistoryInFileHistory)
            {
                filter = string.Concat(" --full-history --simplify-by-decoration ", filter);
            }

            syncContext.Post(o =>
            {
                FileChanges.Filter = filter;
                FileChanges.AllowGraphWithFilter = true;
                FileChanges.Load();
            }, this);
        }

        private void DiffExtraDiffArgumentsChanged(object sender, EventArgs e)
        {
            UpdateSelectedFileViewers();
        }

        private void FormFileHistoryFormClosing(object sender, FormClosingEventArgs e)
        {
            SavePosition("file-history");
        }

        private void FormFileHistoryLoad(object sender, EventArgs e)
        {
            RestorePosition("file-history");
            Text = string.Format("File History ({0})", FileName);
        }

        private void FileChangesSelectionChanged(object sender, EventArgs e)
        {
            View.SaveCurrentScrollPos();
            Diff.SaveCurrentScrollPos();
            UpdateSelectedFileViewers();
        }

        private void UpdateSelectedFileViewers()
        {
            var selectedRows = FileChanges.GetSelectedRevisions();

            if (selectedRows.Count == 0) return;

            IGitItem revision = selectedRows[0];

            var fileName = revision.Name;

            if (string.IsNullOrEmpty(fileName))
                fileName = FileName;

            Text = string.Format("File History ({0})", fileName);

            if (tabControl1.SelectedTab == Blame)
                blameControl1.LoadBlame(revision.Guid, fileName, FileChanges);
            if (tabControl1.SelectedTab == ViewTab)
            {
                var scrollpos = View.ScrollPos;

                View.ViewGitItemRevision(fileName, revision.Guid);
                View.ScrollPos = scrollpos;
            }

            switch (selectedRows.Count)
            {
                case 1:
                    {
                        IGitItem revision1 = selectedRows[0];

                        if (tabControl1.SelectedTab == DiffTab)
                        {
                            Diff.ViewPatch(
                                () =>
                                {
                                    Patch diff = Settings.Module.GetSingleDiff(revision1.Guid, revision1.Guid + "^", fileName,
                                                                          Diff.GetExtraDiffArguments(), Diff.Encoding);
                                    if (diff == null)
                                        return string.Empty;
                                    return diff.Text;
                                }
                                );
                        }
                    }
                    break;
                case 2:
                    {
                        IGitItem revision1 = selectedRows[0];
                        IGitItem revision2 = selectedRows[1];

                        if (tabControl1.SelectedTab == DiffTab)
                        {
                            Diff.ViewPatch(
                                () =>
                                Settings.Module.GetSingleDiff(revision1.Guid, revision2.Guid, fileName,
                                                                      Diff.GetExtraDiffArguments(), Diff.Encoding).Text);
                        }
                    }
                    break;
                default:
                    Diff.ViewPatch("You need to select 2 files to view diff.");
                    break;
            }
        }


        private void TabControl1SelectedIndexChanged(object sender, EventArgs e)
        {
            FileChangesSelectionChanged(sender, e);
        }

        private void FileChangesDoubleClick(object sender, EventArgs e)
        {
            FileChanges.ViewSelectedRevisions();
        }

        private void OpenWithDifftoolToolStripMenuItemClick(object sender, EventArgs e)
        {
            var selectedRows = FileChanges.GetSelectedRevisions();
            string rev1;
            string rev2;
            switch (selectedRows.Count)
            {
                case 1:
                    {
                        rev1 = selectedRows[0].Guid;
                        var parentGuids = selectedRows[0].ParentGuids;
                        if (parentGuids != null && parentGuids.Length > 0)
                        {
                            rev2 = parentGuids[0];
                        }
                        else
                        {
                            rev2 = rev1;
                        }
                    }
                    break;
                case 0:
                    return;
                default:
                    rev1 = selectedRows[0].Guid;
                    rev2 = selectedRows[1].Guid;
                    break;
            }

            var output = Settings.Module.OpenWithDifftool(FileName, rev1, rev2);
            if (!string.IsNullOrEmpty(output))
                MessageBox.Show(this, output);
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedRows = FileChanges.GetSelectedRevisions();

            if (selectedRows.Count > 0)
            {
                string orgFileName = selectedRows[0].Name;

                if (string.IsNullOrEmpty(orgFileName))
                    orgFileName = FileName;

                string fileName = orgFileName.Replace(Settings.PathSeparatorWrong, Settings.PathSeparator);

                var fileDialog = new SaveFileDialog
                {
                    FileName = Settings.WorkingDir + fileName,
                    AddExtension = true
                };
                fileDialog.DefaultExt = GitCommandHelpers.GetFileExtension(fileDialog.FileName);
                fileDialog.Filter =
                    "Current format (*." +
                    GitCommandHelpers.GetFileExtension(fileDialog.FileName) + ")|*." +
                    GitCommandHelpers.GetFileExtension(fileDialog.FileName) +
                    "|All files (*.*)|*.*";
                if (fileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    Settings.Module.SaveBlobAs(fileDialog.FileName, selectedRows[0].Guid + ":\"" + orgFileName + "\"");
                }
            }
        }

        private void followFileHistoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.FollowRenamesInFileHistory = !Settings.FollowRenamesInFileHistory;
            followFileHistoryToolStripMenuItem.Checked = Settings.FollowRenamesInFileHistory;

            ThreadPool.QueueUserWorkItem(o => LoadFileHistory(FileName));
        }

        private void fullHistoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.FullHistoryInFileHistory = !Settings.FullHistoryInFileHistory;
            fullHistoryToolStripMenuItem.Checked = Settings.FullHistoryInFileHistory;
            ThreadPool.QueueUserWorkItem(o => LoadFileHistory(FileName));
        }

        private void cherryPickThisCommitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedRevisions = FileChanges.GetSelectedRevisions();
            if (selectedRevisions.Count == 1)
            {
                var frm = new FormCherryPickCommitSmall(selectedRevisions[0]);
                frm.ShowDialog(this);
            }
        }

        private void revertCommitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedRevisions = FileChanges.GetSelectedRevisions();
            if (selectedRevisions.Count == 1)
            {
                var frm = new FormRevertCommitSmall(selectedRevisions[0]);
                frm.ShowDialog(this);
            }
        }

        private void viewCommitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileChanges.ViewSelectedRevisions();
        }
    }
}