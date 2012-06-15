using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;

namespace GitUI
{
    public partial class FormBisect : GitExtensionsForm
    {
        // TODO: Improve me
        private readonly TranslationString _bisectStart =
            new TranslationString("Mark selected revisions as start bisect range?");

        private readonly RevisionGrid _revisionGrid;

        public FormBisect(RevisionGrid revisionGrid)
        {
            InitializeComponent();
            Translate();
            Initialize();
            _revisionGrid = revisionGrid;
        }

        private void Initialize()
        {
            bool inTheMiddleOfBisect = Settings.Module.InTheMiddleOfBisect();
            Start.Enabled = !inTheMiddleOfBisect;
            Good.Enabled = inTheMiddleOfBisect;
            Bad.Enabled = inTheMiddleOfBisect;
            Stop.Enabled = inTheMiddleOfBisect;
        }

        private void Start_Click(object sender, EventArgs e)
        {
            using (var frm = new FormProcess(GitCommandHelpers.StartBisectCmd())) frm.ShowDialog(this);
            Initialize();

            IList<GitRevision> revisions = _revisionGrid.GetSelectedRevisions();
            if (revisions.Count > 1)
            {
                if (MessageBox.Show(this, _bisectStart.Text, Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    BisectRange(revisions[0].Guid, revisions[revisions.Count - 1].Guid);
                    Close();
                }
            }
        }

        private void BisectRange(string startRevision, string endRevision)
        {
            var command = GitCommandHelpers.MarkRevisionBisectCmd(true, startRevision);
            using (var form = new FormProcess(command))
            {
                form.ShowDialog(this);
                if (!form.ErrorOccurred())
                {
                    command = GitCommandHelpers.MarkRevisionBisectCmd(false, endRevision);
                    using (var form2 = new FormProcess(command))
                        form2.ShowDialog(this);
                }
            }
        }

        private void Good_Click(object sender, EventArgs e)
        {
            using (var frm = new FormProcess(GitCommandHelpers.ContinueBisectCmd(true), false)) frm.ShowDialog(this);
            Close();
        }

        private void Bad_Click(object sender, EventArgs e)
        {
            using (var frm = new FormProcess(GitCommandHelpers.ContinueBisectCmd(false), false)) frm.ShowDialog(this);
            Close();
        }

        private void Stop_Click(object sender, EventArgs e)
        {
            using (var frm = new FormProcess(GitCommandHelpers.StopBisectCmd())) frm.ShowDialog(this);
            Close();
        }
    }
}
