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
            bool inTheMiddleOfBisect = Settings.Module.InTheMiddleOfBisect();
            Start.Enabled = !inTheMiddleOfBisect;
            Good.Enabled = inTheMiddleOfBisect;
            Bad.Enabled = inTheMiddleOfBisect;
            Stop.Enabled = inTheMiddleOfBisect;
        }

        private void Start_Click(object sender, EventArgs e)
        {
            new FormProcess(GitCommandHelpers.StartBisectCmd()).ShowDialog(this);
            Initialize();
        }

        private void Good_Click(object sender, EventArgs e)
        {
            Settings.CloseProcessDialog = false;
            new FormProcess(GitCommandHelpers.ContinueBisectCmd(true)).ShowDialog(this);
            Close();
        }

        private void Bad_Click(object sender, EventArgs e)
        {
            Settings.CloseProcessDialog = false;
            new FormProcess(GitCommandHelpers.ContinueBisectCmd(false)).ShowDialog(this);
            Close();
        }

        private void Stop_Click(object sender, EventArgs e)
        {
            new FormProcess(GitCommandHelpers.StopBisectCmd()).ShowDialog(this);
            Close();
        }
    }
}
