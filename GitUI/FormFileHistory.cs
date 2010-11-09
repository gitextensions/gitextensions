using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using PatchApply;
using System.Globalization;
using System.Diagnostics;
using System.Collections.Generic;

namespace GitUI
{
    public partial class FormFileHistory : GitExtensionsForm
    {
        public FormFileHistory(string fileName, GitRevision revision)
        {
            InitializeComponent();
            FileChanges.SetInitialRevision(revision);
            Translate();

            if (string.IsNullOrEmpty(fileName))
                return;

            LoadFileHistory(fileName);

            Diff.ExtraDiffArgumentsChanged += DiffExtraDiffArgumentsChanged;

            FileChanges.SelectionChanged += FileChangesSelectionChanged;
            FileChanges.DisableContextMenu();
        }

        public FormFileHistory(string fileName)
            : this(fileName, null)
        {
        }

        public string FileName { get; set; }

        private void LoadFileHistory(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return;

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
                    StringBuilder shortPath = new StringBuilder(4096);
                    NativeMethods.GetShortPathName(fullFilePath, shortPath, shortPath.Capacity);

                    // use 8.3 file path to get properly cased full file path
                    StringBuilder longPath = new StringBuilder(4096);
                    NativeMethods.GetLongPathName(shortPath.ToString(), longPath, longPath.Capacity);

                    // remove the working dir and now we have a properly cased file name.
                    fileName = longPath.ToString().Substring(Settings.WorkingDir.Length);
                }
            }

            if (fileName.StartsWith(Settings.WorkingDir, StringComparison.InvariantCultureIgnoreCase))
                fileName = fileName.Substring(Settings.WorkingDir.Length);

            FileName = fileName;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

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

                GitCommandsInstance gitGetGraphCommand = new GitCommandsInstance();
                gitGetGraphCommand.StreamOutput = true;
                gitGetGraphCommand.CollectOutput = false;

                string arg = "log --format=\"%n\" --name-only --follow -- \"" + FileName + "\"";
                Process p = gitGetGraphCommand.CmdStartProcess(Settings.GitCommand, arg);

                // the sequence of (quoted) file names - start with the initial filename for the search.
                string listOfFileNames = "\"" + FileName + "\"";

                // keep a set of the file names already seen
                HashSet<string> setOfFileNames = new HashSet<string>();
                setOfFileNames.Add(FileName);

                string line;
                do
                {
                    line = p.StandardOutput.ReadLine();

                    if ((line != null) && (line != ""))
                    {
                        if (!setOfFileNames.Contains(line))
                        {
                            listOfFileNames = listOfFileNames + " \"" + line + "\"";
                            setOfFileNames.Add(line);
                        }
                    }
                } while (line != null);

                // here we need --name-only to get the previous filenames in the revision graph
                FileChanges.Filter = " --name-only --parents -- " + listOfFileNames;
                FileChanges.AllowGraphWithFilter = true;
            }
            else
            {
                // --parents doesn't work with --follow enabled, but needed to graph a filtered log
                FileChanges.Filter = " --parents -- \"" + FileName + "\"";
                FileChanges.AllowGraphWithFilter = true;
            }
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
            var selectedRows = FileChanges.GetRevisions();

            if (selectedRows.Count == 0) return;

            IGitItem revision = selectedRows[0];

            var fileName = revision.Name;

            if (string.IsNullOrEmpty(fileName))
                fileName = FileName;

            Text = string.Format("File History ({0})", fileName);

            if (tabControl1.SelectedTab == Blame)
                blameControl1.LoadBlame(revision.Guid, fileName);
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
                                    Patch diff = GitCommandHelpers.GetSingleDiff(revision1.Guid, revision1.Guid + "^", fileName,
                                                                          Diff.GetExtraDiffArguments());
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
                                GitCommandHelpers.GetSingleDiff(revision1.Guid, revision2.Guid, fileName,
                                                                      Diff.GetExtraDiffArguments()).Text);
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
            if (FileChanges.GetRevisions().Count == 0)
            {
                GitUICommands.Instance.StartCompareRevisionsDialog();
                return;
            }

            IGitItem revision = FileChanges.GetRevisions()[0];

            var form = new FormDiffSmall();
            form.SetRevision(revision.Guid);
            form.ShowDialog();
        }

        private void OpenWithDifftoolToolStripMenuItemClick(object sender, EventArgs e)
        {
            var selectedRows = FileChanges.GetRevisions();
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

            var output = GitCommandHelpers.OpenWithDifftool(FileName, rev1, rev2);
            if (!string.IsNullOrEmpty(output))
                MessageBox.Show(output);
        }
    }
}