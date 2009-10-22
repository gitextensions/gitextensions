using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;

using System.Text;
using System.Windows.Forms;
using GitCommands;
using ICSharpCode.TextEditor.Document;

namespace GitUI
{
    public partial class FormFileHistory : GitExtensionsForm
    {
        public FormFileHistory(string fileName):base()
        {
            this.FileName = fileName;

            InitializeComponent();
            FileChanges.Filter = " \"" + fileName + "\"";
            FileChanges.SelectionChanged +=new EventHandler(FileChanges_SelectionChanged);

            BlameFile.LineViewerStyle = ICSharpCode.TextEditor.Document.LineViewerStyle.FullRow;
            BlameCommitter.ActiveTextAreaControl.VScrollBar.Visible = false;
            BlameCommitter.ShowLineNumbers = false;
            BlameCommitter.LineViewerStyle = ICSharpCode.TextEditor.Document.LineViewerStyle.FullRow;
            BlameCommitter.Enabled = false;

            BlameFile.ActiveTextAreaControl.VScrollBar.ValueChanged += new EventHandler(VScrollBar_ValueChanged);
            BlameFile.KeyDown += new KeyEventHandler(BlameFile_KeyUp);
            BlameFile.ActiveTextAreaControl.TextArea.Click += new EventHandler(BlameFile_Click);
            BlameFile.ActiveTextAreaControl.TextArea.KeyDown += new KeyEventHandler(BlameFile_KeyUp);
            BlameFile.ActiveTextAreaControl.KeyDown += new KeyEventHandler(BlameFile_KeyUp);
            BlameFile.ActiveTextAreaControl.TextArea.DoubleClick += new EventHandler(ActiveTextAreaControl_DoubleClick);
        }

        public string FileName { get; set; }

        private void FormFileHistory_Load(object sender, EventArgs e)
        {
            this.Text = "File History (" + FileName + ")";
        }

        private void FileChanges_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void FileChanges_SelectionChanged(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            List<GitRevision> selectedRows = FileChanges.GetRevisions();

            if (selectedRows.Count == 0) return;

            IGitItem revision = selectedRows[0];

            if (tabControl1.SelectedTab == Blame)
            {
                //BlameGrid.DataSource = GitCommands.GitCommands.Blame(FileName, revision.CommitGuid);
                FillBlameTab(revision.Guid);
            }
            if (tabControl1.SelectedTab == ViewTab)
            {
                int scrollpos =View.ScrollPos;

                View.ViewGitItemRevision(FileName, revision.Guid);
                View.ScrollPos = scrollpos;
            }

            if (selectedRows.Count == 2)
            {
                {
                    IGitItem revision1 = selectedRows[0];
                    IGitItem revision2 = selectedRows[1];

                    if (tabControl1.SelectedTab == DiffTab)
                    {
                        Diff diff = new Diff(new DiffDto(revision1.Guid, revision2.Guid, FileName));
                        diff.Execute();
                        ///EditorOptions.SetSyntax(Diff, FileName);
                        Diff.ViewPatch(diff.Dto.Result);
                   }
                }
            }
            else
                if (selectedRows.Count == 1)
            {
                IGitItem revision1 = selectedRows[0];

                if (tabControl1.SelectedTab == DiffTab)
                {
                    Diff diff = new Diff(new DiffDto(revision1.Guid + "^", revision1.Guid, FileName));
                    diff.Execute();
                    Diff.ViewPatch(diff.Dto.Result);
                }
            }
            else
            {
                Diff.ViewPatch("You need to select 2 files to view diff.");
            }
        }

        private List<GitBlame> blameList;
        private void FillBlameTab(string guid)
        {
            StringBuilder blameCommitter = new StringBuilder();
            StringBuilder blameFile = new StringBuilder();

            blameList = GitCommands.GitCommands.Blame(FileName, guid);

            foreach (GitBlame blame in blameList)
            {
                blameCommitter.AppendLine(blame.Author);
                blameFile.AppendLine(blame.Text);
            }

            EditorOptions.SetSyntax(BlameFile, FileName);

            BlameCommitter.Text = blameCommitter.ToString();
            BlameFile.Text = blameFile.ToString();
        }

        FindAndReplaceForm findAndReplaceForm = new FindAndReplaceForm();

        void BlameFile_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.F)
                findAndReplaceForm.ShowFor(BlameFile, false);
            SyncBlameViews();
        }


        void VScrollBar_ValueChanged(object sender, EventArgs e)
        {
            SyncBlameViews();
        }

        void BlameFile_Click(object sender, EventArgs e)
        {
            SyncBlameViews();
        }
        
        private void SyncBlameViews()
        {
            BlameCommitter.ActiveTextAreaControl.VScrollBar.Value = BlameFile.ActiveTextAreaControl.VScrollBar.Value;
            //BlameCommitter.ActiveTextAreaControl.Caret.Line = BlameFile.ActiveTextAreaControl.Caret.Line;
        }

        void ActiveTextAreaControl_DoubleClick(object sender, EventArgs e)
        {
            if (blameList == null || blameList.Count < BlameFile.ActiveTextAreaControl.TextArea.Caret.Line)
                return;

            FormDiffSmall frm = new FormDiffSmall();
            frm.SetRevision(blameList[BlameFile.ActiveTextAreaControl.TextArea.Caret.Line].CommitGuid);
            frm.ShowDialog();
        }


        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            FileChanges_SelectionChanged(sender, e);
        }

        private void FileChanges_DoubleClick(object sender, EventArgs e)
        {
            if (FileChanges.GetRevisions().Count > 0)
            {
                IGitItem revision = FileChanges.GetRevisions()[0];

                FormDiffSmall form = new FormDiffSmall();
                form.SetRevision(revision.Guid);
                form.ShowDialog();
            }
            else
                GitUICommands.Instance.StartCompareRevisionsDialog();

        }

        private void FormFileHistory_Shown(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            //EditorOptions.SetSyntax(View, FileName);

            //FileChanges.DataSource = GitCommands.GitCommands.GetFileChanges(FileName);
        }
    }
}
