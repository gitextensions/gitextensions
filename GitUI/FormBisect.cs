using System;
using GitCommands;

namespace GitUI
{
    public partial class FormBisect : GitExtensionsForm
    {
        static FormBisect()
        {
            bool? cpd = Settings.GetCloseProcessDialog(FormSettingsName());
            if (!cpd.HasValue)
                Settings.SetCloseProcessDialog(FormSettingsName(), false);
        }

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
            new FormProcess(GitCommandHelpers.StartBisectCmd(), PerFormSettingsName()).ShowDialog();
            Initialize();
        }

        private void Good_Click(object sender, EventArgs e)
        {
            new FormProcess(GitCommandHelpers.ContinueBisectCmd(true), PerFormSettingsName()).ShowDialog();
            Close();
        }

        private void Bad_Click(object sender, EventArgs e)
        {
            new FormProcess(GitCommandHelpers.ContinueBisectCmd(false), PerFormSettingsName()).ShowDialog();
            Close();
        }

        private void Stop_Click(object sender, EventArgs e)
        {
            new FormProcess(GitCommandHelpers.StopBisectCmd(), PerFormSettingsName()).ShowDialog();
            Close();
        }


        public override string PerFormSettingsName()
        {
            return FormSettingsName();
        }

        public static string FormSettingsName()
        {
            return typeof(FormBisect).FullName;
        }

    }
}
