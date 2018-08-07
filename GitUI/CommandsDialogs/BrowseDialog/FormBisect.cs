using System;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    public sealed partial class FormBisect : GitModuleForm
    {
        // TODO: Improve me
        private readonly TranslationString _bisectStart =
            new TranslationString("Mark selected revisions as start bisect range?");

        private readonly RevisionGridControl _revisionGrid;

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormBisect()
        {
            InitializeComponent();
        }

        private FormBisect(GitUICommands commands)
            : base(commands)
        {
            InitializeComponent();
            InitializeComplete();
        }

        public FormBisect(RevisionGridControl revisionGrid)
            : this(revisionGrid.UICommands)
        {
            _revisionGrid = revisionGrid;
            UpdateButtonsState();
        }

        private void UpdateButtonsState()
        {
            bool inTheMiddleOfBisect = Module.InTheMiddleOfBisect();
            Start.Enabled = !inTheMiddleOfBisect;
            Good.Enabled = inTheMiddleOfBisect;
            Bad.Enabled = inTheMiddleOfBisect;
            Stop.Enabled = inTheMiddleOfBisect;
            btnSkip.Enabled = inTheMiddleOfBisect;
        }

        private void Start_Click(object sender, EventArgs e)
        {
            FormProcess.ShowDialog(this, GitCommandHelpers.StartBisectCmd());
            UpdateButtonsState();

            var revisions = _revisionGrid.GetSelectedRevisions();

            if (revisions.Count > 1)
            {
                if (MessageBox.Show(this, _bisectStart.Text, Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    BisectRange(revisions.First().ObjectId, revisions.Last().ObjectId);
                    Close();
                }
            }

            return;

            void BisectRange(ObjectId startRevision, ObjectId endRevision)
            {
                var command = GitCommandHelpers.ContinueBisectCmd(GitBisectOption.Good, startRevision);
                var errorOccurred = !FormProcess.ShowDialog(this, command);
                if (errorOccurred)
                {
                    return;
                }

                command = GitCommandHelpers.ContinueBisectCmd(GitBisectOption.Bad, endRevision);
                FormProcess.ShowDialog(this, command);
            }
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
