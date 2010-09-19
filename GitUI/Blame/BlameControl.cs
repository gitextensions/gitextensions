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
        private List<GitBlame> _blameList;
        private string _lastRevision;

        public BlameControl()
        {
            InitializeComponent();

            BlameCommitter.IsReadOnly = true;
            //BlameCommitter.EnableScrollBars(false);
            BlameCommitter.ShowLineNumbers = false;
            BlameCommitter.ScrollPosChanged += new EventHandler(BlameCommitter_ScrollPosChanged);
            BlameFile.IsReadOnly = true;

            /*
            BlameFile.LineViewerStyle = LineViewerStyle.FullRow;
             * 
            BlameFile.KeyDown += BlameFileKeyUp;
            BlameFile.ActiveTextAreaControl.TextArea.Click += BlameFileClick;
            BlameFile.ActiveTextAreaControl.TextArea.KeyDown += BlameFileKeyUp;
            BlameFile.ActiveTextAreaControl.KeyDown += BlameFileKeyUp;
            BlameFile.ActiveTextAreaControl.TextArea.DoubleClick += ActiveTextAreaControlDoubleClick;
            BlameFile.ActiveTextAreaControl.TextArea.MouseDown += TextAreaMouseDown;*/
        }

        void BlameCommitter_ScrollPosChanged(object sender, EventArgs e)
        {
            SyncBlameViews();
        }

        private void TextAreaMouseDown(object sender, MouseEventArgs e)
        {
            /*
             * if (_blameList == null)
                return;

            if (BlameFile.ActiveTextAreaControl.TextArea.TextView.GetLogicalLine(e.Y) >= _blameList.Count)
                return;

            var newRevision =
                _blameList[BlameFile.ActiveTextAreaControl.TextArea.TextView.GetLogicalLine(e.Y)].CommitGuid;
            if (_lastRevision == newRevision)
                return;

            _lastRevision = newRevision;
            commitInfo.SetRevision(_lastRevision);*/
        }

        public void LoadBlame(string guid, string fileName)
        {
            var scrollpos = BlameFile.ScrollPos;

            var blameCommitter = new StringBuilder();
            var blameFile = new StringBuilder();

            _blameList = GitCommands.GitCommands.Blame(fileName, guid);

            foreach (var blame in _blameList)
            {
                blameCommitter.AppendLine(blame.Author);
                blameFile.AppendLine(blame.Text);
            }

            BlameCommitter.ViewText("committer.txt", blameCommitter.ToString());
            BlameFile.ViewText(fileName, blameFile.ToString());
            BlameFile.ScrollPos = scrollpos;

            TextAreaMouseDown(this, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
        }

        private void BlameFileClick(object sender, EventArgs e)
        {
            SyncBlameViews();
        }

        private void SyncBlameViews()
        {
            BlameCommitter.ScrollPos = BlameFile.ScrollPos;
        }

        private void ActiveTextAreaControlDoubleClick(object sender, EventArgs e)
        {
            /*
            if (_blameList == null || _blameList.Count < BlameFile.ActiveTextAreaControl.TextArea.Caret.Line)
                return;

            var frm = new FormDiffSmall();
            frm.SetRevision(_blameList[BlameFile.ActiveTextAreaControl.TextArea.Caret.Line].CommitGuid);
            frm.ShowDialog();
             */
        }
    }
}