using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using GitUI.Editor;
using ICSharpCode.TextEditor.Document;

namespace GitUI.Blame
{
    public partial class BlameControl : UserControl
    {
        private readonly FindAndReplaceForm _findAndReplaceForm = new FindAndReplaceForm();
        private List<GitBlame> _blameList;
        private string _lastRevision;

        public BlameControl()
        {
            InitializeComponent();

            BlameFile.LineViewerStyle = LineViewerStyle.FullRow;

            BlameCommitter.ActiveTextAreaControl.VScrollBar.Width = 0;
            BlameCommitter.ActiveTextAreaControl.VScrollBar.Visible = false;
            BlameCommitter.ShowLineNumbers = false;
            BlameCommitter.LineViewerStyle = LineViewerStyle.FullRow;
            BlameCommitter.Enabled = false;
            BlameCommitter.ActiveTextAreaControl.TextArea.Dock = DockStyle.Fill;

            BlameFile.IsReadOnly = true;
            BlameFile.ActiveTextAreaControl.VScrollBar.ValueChanged += VScrollBarValueChanged;
            BlameFile.KeyDown += BlameFileKeyUp;
            BlameFile.ActiveTextAreaControl.TextArea.Click += BlameFileClick;
            BlameFile.ActiveTextAreaControl.TextArea.KeyDown += BlameFileKeyUp;
            BlameFile.ActiveTextAreaControl.KeyDown += BlameFileKeyUp;
            BlameFile.ActiveTextAreaControl.TextArea.DoubleClick += ActiveTextAreaControlDoubleClick;
            BlameFile.ActiveTextAreaControl.TextArea.MouseDown += TextAreaMouseDown;
        }

        private void TextAreaMouseDown(object sender, MouseEventArgs e)
        {
            if (_blameList == null)
                return;

            if (BlameFile.ActiveTextAreaControl.TextArea.TextView.GetLogicalLine(e.Y) >= _blameList.Count)
                return;

            var newRevision =
                _blameList[BlameFile.ActiveTextAreaControl.TextArea.TextView.GetLogicalLine(e.Y)].CommitGuid;
            if (_lastRevision == newRevision)
                return;

            _lastRevision = newRevision;
            commitInfo.SetRevision(_lastRevision);
        }

        public void LoadBlame(string guid, string fileName)
        {
            var scrollpos = BlameFile.ActiveTextAreaControl.VScrollBar.Value;

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

            BlameFile.ActiveTextAreaControl.VScrollBar.Value =
                BlameFile.ActiveTextAreaControl.VScrollBar.Maximum >= scrollpos
                    ? scrollpos
                    : BlameFile.ActiveTextAreaControl.VScrollBar.Maximum;
            BlameFile.ActiveControl = BlameFile.ActiveTextAreaControl;
            TextAreaMouseDown(this, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
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
    }
}