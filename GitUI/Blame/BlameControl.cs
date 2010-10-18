using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using GitUI.Editor;

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
            BlameCommitter.EnableScrollBars(false);
            BlameCommitter.ShowLineNumbers = false;
            BlameFile.ScrollPosChanged += new EventHandler(BlameCommitter_ScrollPosChanged);
            BlameFile.IsReadOnly = true;
            BlameFile.SelectedLineChanged += new SelectedLineChangedHandler(BlameFile_SelectedLineChanged);

            BlameFile.RequestDiffView += ActiveTextAreaControlDoubleClick;
        }

        void BlameFile_SelectedLineChanged(object sender, int selectedLine)
        {
            if (selectedLine >= _blameList.Count)
                return;

            var newRevision =
                _blameList[selectedLine].CommitGuid;
            if (_lastRevision == newRevision)
                return;

            _lastRevision = newRevision;
            commitInfo.SetRevision(_lastRevision);
        }

        void BlameCommitter_ScrollPosChanged(object sender, EventArgs e)
        {
            SyncBlameViews();
        }

        public void LoadBlame(string guid, string fileName)
        {
            var scrollpos = BlameFile.ScrollPos;

            var blameCommitter = new StringBuilder();
            var blameFile = new StringBuilder();

            _blameList = GitCommandHelpers.Blame(fileName, guid);

            foreach (var blame in _blameList)
            {
                blameCommitter.AppendLine(blame.Author);
                blameFile.AppendLine(blame.Text);
            }

            BlameCommitter.ViewText("committer.txt", blameCommitter.ToString());
            BlameFile.ViewText(fileName, blameFile.ToString());
            BlameFile.ScrollPos = scrollpos;

            BlameFile_SelectedLineChanged(null, 0);
        }

        private void SyncBlameViews()
        {
            BlameCommitter.ScrollPos = BlameFile.ScrollPos;
        }

        private void ActiveTextAreaControlDoubleClick(object sender, EventArgs e)
        {
            var frm = new FormDiffSmall();
            frm.SetRevision(_lastRevision);
            frm.ShowDialog();
        }
    }
}
