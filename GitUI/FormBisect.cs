using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git;
using ResourceManager.Translation;

namespace GitUI
{
    public sealed partial class FormBisect : GitExtensionsForm
    {
        // TODO: Improve me
        private readonly TranslationString _bisectStart =
            new TranslationString("Mark selected revisions as start bisect range?");

        private readonly RevisionGrid _revisionGrid;

        public FormBisect(RevisionGrid revisionGrid)
        {
            InitializeComponent();
            Translate();
            _revisionGrid = revisionGrid;
            UpdateButtonsState();
        }

        private void UpdateButtonsState()
        {
            bool inTheMiddleOfBisect = Settings.Module.InTheMiddleOfBisect();
            Start.Enabled = !inTheMiddleOfBisect;
            Good.Enabled = inTheMiddleOfBisect;
            Bad.Enabled = inTheMiddleOfBisect;
            Stop.Enabled = inTheMiddleOfBisect;
            btnSkip.Enabled = inTheMiddleOfBisect;
        }

        private void Start_Click(object sender, EventArgs e)
        {
            using (var frm = new FormProcess(GitCommandHelpers.StartBisectCmd())) frm.ShowDialog(this);
            UpdateButtonsState();

            IList<GitRevision> revisions = _revisionGrid.GetSelectedRevisions();
            if (revisions.Count > 1)
            {
                if (MessageBox.Show(this, _bisectStart.Text, Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    BisectRange(revisions.First().Guid, revisions.Last().Guid);
                    Close();
                }
            }
        }

        private void BisectRange(string startRevision, string endRevision)
        {
            var command = GitCommandHelpers.ContinueBisectCmd(GitBisectOption.Good, startRevision);
            using (var form = new FormProcess(command))
            {
                form.ShowDialog(this);
                if (form.ErrorOccurred())
                    return;
            }

            command = GitCommandHelpers.ContinueBisectCmd(GitBisectOption.Bad, endRevision);
            FormProcess.ShowDialog(this, command);
        }

        private void Good_Click(object sender, EventArgs e)
        {
            ContinueBisect(GitBisectOption.Good);
        }

        private void Bad_Click(object sender, EventArgs e)
        {
            ContinueBisect(GitBisectOption.Bad);
        }

        private void Stop_Click(object sender, EventArgs e)
        {
            FormProcess.ShowDialog(this, GitCommandHelpers.StopBisectCmd());
            Close();
        }

        private void btnSkip_Click(object sender, EventArgs e)
        {
            ContinueBisect(GitBisectOption.Skip);
        }

        private void ContinueBisect(GitBisectOption bisectOption)
        {
            FormProcess.ShowDialog(this, GitCommandHelpers.ContinueBisectCmd(bisectOption), false);
            Close();
        }
    }
}
