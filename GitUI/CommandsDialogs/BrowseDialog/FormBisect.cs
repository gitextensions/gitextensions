using System;
using System.Linq;
using System.Windows.Forms;
using GitCommands.Git;
using GitCommands.Git.Commands;
using GitUI.HelperDialogs;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    public sealed partial class FormBisect : GitModuleForm
    {
        // TODO: Improve me
        private readonly TranslationString _bisectStart =
            new("Mark selected revisions as start bisect range?");

        private readonly RevisionGridControl _revisionGrid;

        [Obsolete("For VS designer and translation test only. Do not remove.")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private FormBisect()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            InitializeComponent();
        }

        public FormBisect(RevisionGridControl revisionGrid)
            : base(revisionGrid.UICommands)
        {
            _revisionGrid = revisionGrid;
            InitializeComponent();
            InitializeComplete();
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
            FormProcess.ShowDialog(this, process: null, arguments: GitCommandHelpers.StartBisectCmd(), Module.WorkingDir, input: null, useDialogSettings: true);

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
                bool success = FormProcess.ShowDialog(this, process: null, arguments: command, Module.WorkingDir, input: null, useDialogSettings: true);
                if (!success)
                {
                    return;
                }

                command = GitCommandHelpers.ContinueBisectCmd(GitBisectOption.Bad, endRevision);
                FormProcess.ShowDialog(this, process: null, arguments: command, Module.WorkingDir, input: null, useDialogSettings: true);
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
            FormProcess.ShowDialog(this, process: null, arguments: GitCommandHelpers.StopBisectCmd(), Module.WorkingDir, input: null, useDialogSettings: false);
            Close();
        }

        private void btnSkip_Click(object sender, EventArgs e)
        {
            ContinueBisect(GitBisectOption.Skip);
        }

        private void ContinueBisect(GitBisectOption bisectOption)
        {
            FormProcess.ShowDialog(this, process: null, arguments: GitCommandHelpers.ContinueBisectCmd(bisectOption), Module.WorkingDir, input: null, useDialogSettings: false);
            Close();
        }
    }
}
