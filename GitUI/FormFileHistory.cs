using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using GitUI.Editor;
using ICSharpCode.TextEditor.Document;

namespace GitUI
{
    public partial class FormFileHistory : GitExtensionsForm
    {
        private readonly FindAndReplaceForm _findAndReplaceForm = new FindAndReplaceForm();
        private List<GitBlame> _blameList;
        private string _lastRevision;

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

            BlameFile.LineViewerStyle = LineViewerStyle.FullRow;

            BlameCommitter.ActiveTextAreaControl.VScrollBar.Width = 0;
            BlameCommitter.ActiveTextAreaControl.VScrollBar.Visible = false;
            BlameCommitter.ShowLineNumbers = false;
            BlameCommitter.LineViewerStyle = LineViewerStyle.FullRow;
            BlameCommitter.Enabled = false;
            BlameCommitter.ActiveTextAreaControl.TextArea.Dock = DockStyle.Fill;

            BlameFile.ActiveTextAreaControl.VScrollBar.ValueChanged += VScrollBarValueChanged;
            BlameFile.KeyDown += BlameFileKeyUp;
            BlameFile.ActiveTextAreaControl.TextArea.Click += BlameFileClick;
            BlameFile.ActiveTextAreaControl.TextArea.KeyDown += BlameFileKeyUp;
            BlameFile.ActiveTextAreaControl.KeyDown += BlameFileKeyUp;
            BlameFile.ActiveTextAreaControl.TextArea.DoubleClick += ActiveTextAreaControlDoubleClick;

            BlameFile.ActiveTextAreaControl.TextArea.MouseDown += TextAreaMouseDown;
        }

        public FormFileHistory(string fileName) : this(fileName, null)
        {
        }

        public string FileName { get; set; }

        private void LoadFileHistory(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return;

            if (fileName.StartsWith(Settings.WorkingDir, StringComparison.InvariantCultureIgnoreCase))
                fileName = fileName.Substring(Settings.WorkingDir.Length);

            FileName = fileName;

            if (Settings.FollowRenamesInFileHistory)
                FileChanges.Filter = " --name-only --follow -- \"" + fileName + "\"";
            else
                FileChanges.Filter = " -- \"" + fileName + "\"";
        }

        private void TextAreaMouseDown(object sender, MouseEventArgs e)
        {
            if (_blameList == null)
                return;

            if (BlameFile.ActiveTextAreaControl.TextArea.TextView.GetLogicalLine(e.Y) >= _blameList.Count)
                return;

            var newRevision = _blameList[BlameFile.ActiveTextAreaControl.TextArea.TextView.GetLogicalLine(e.Y)].CommitGuid;
            if (_lastRevision == newRevision)
                return;

            _lastRevision = newRevision;
            commitInfo.SetRevision(_lastRevision);
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
            {
                var scrollpos = BlameFile.ActiveTextAreaControl.VScrollBar.Value;
                FillBlameTab(revision.Guid, fileName);
                BlameFile.ActiveTextAreaControl.VScrollBar.Value =
                    BlameFile.ActiveTextAreaControl.VScrollBar.Maximum >= scrollpos
                        ? scrollpos
                        : BlameFile.ActiveTextAreaControl.VScrollBar.Maximum;
            }
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
                                GitCommands.GitCommands.GetSingleDiff(revision1.Guid, revision1.Guid + "^", fileName,
                                                                      Diff.GetExtraDiffArguments()).Text);
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
                                GitCommands.GitCommands.GetSingleDiff(revision1.Guid, revision2.Guid, fileName,
                                                                      Diff.GetExtraDiffArguments()).Text);
                        }
                    }
                    break;
                default:
                    Diff.ViewPatch("You need to select 2 files to view diff.");
                    break;
            }
        }

        private void FillBlameTab(string guid, string fileName)
        {
            var blameCommitter = new StringBuilder();
            var blameFile = new StringBuilder();

            _blameList = GitCommands.GitCommands.Blame(fileName, guid);

            foreach (var blame in _blameList)
            {
                blameCommitter.AppendLine(blame.Author);
                blameFile.AppendLine(blame.Text);
            }

            EditorOptions.SetSyntax(BlameFile, fileName);

            BlameCommitter.Text = blameCommitter.ToString();
            BlameFile.Text = blameFile.ToString();
        }

        private void BlameFileKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.F)
                _findAndReplaceForm.ShowFor(BlameFile, false);
            SyncBlameViews();
        }


        private void VScrollBarValueChanged(object sender, EventArgs e)
        {
            SyncBlameViews();
        }

        private void BlameFileClick(object sender, EventArgs e)
        {
            SyncBlameViews();
        }

        private void SyncBlameViews()
        {
            BlameCommitter.ActiveTextAreaControl.VScrollBar.Value = BlameFile.ActiveTextAreaControl.VScrollBar.Value;
        }

        private void ActiveTextAreaControlDoubleClick(object sender, EventArgs e)
        {
            if (_blameList == null || _blameList.Count < BlameFile.ActiveTextAreaControl.TextArea.Caret.Line)
                return;

            var frm = new FormDiffSmall();
            frm.SetRevision(_blameList[BlameFile.ActiveTextAreaControl.TextArea.Caret.Line].CommitGuid);
            frm.ShowDialog();
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

            var output = GitCommands.GitCommands.OpenWithDifftool(FileName, rev1, rev2);
            if (!string.IsNullOrEmpty(output))
                MessageBox.Show(output);
        }
    }
}