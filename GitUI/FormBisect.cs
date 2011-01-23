using System;
using GitCommands;

namespace GitUI
{
    public partial class FormBisect : GitExtensionsForm
    {
        public FormBisect()
        {
            InitializeComponent();
            Translate();
            Initialize();
        }

        private void Initialize()
        {
            bool inTheMiddleOfBisect = GitCommandHelpers.InTheMiddleOfBisect();
            Start.Enabled = !inTheMiddleOfBisect;
            Good.Enabled = inTheMiddleOfBisect;
            Bad.Enabled = inTheMiddleOfBisect;
            Stop.Enabled = inTheMiddleOfBisect;
        }

        private void Start_Click(object sender, EventArgs e)
        {
            new FormProcess(GitCommandHelpers.StartBisectCmd()).ShowDialog();
            Initialize();
        }

        private void Good_Click(object sender, EventArgs e)
        {
            Settings.CloseProcessDialog = false;
            new FormProcess(GitCommandHelpers.ContinueBisectCmd(true)).ShowDialog();
            Close();
        }

        private void Bad_Click(object sender, EventArgs e)
        {
            Settings.CloseProcessDialog = false;
            new FormProcess(GitCommandHelpers.ContinueBisectCmd(false)).ShowDialog();
            Close();
        }

        private void Stop_Click(object sender, EventArgs e)
        {
            new FormProcess(GitCommandHelpers.StopBisectCmd()).ShowDialog();
            Close();
        }
    }
}
