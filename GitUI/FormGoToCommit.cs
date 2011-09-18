using System;
using GitCommands;

namespace GitUI
{
    public sealed partial class FormGoToCommit : GitExtensionsForm
    {
        public FormGoToCommit()
        {
            InitializeComponent();
            Translate();
        }

        public string GetRevision()
        {
            return GitCommandHelpers.RevParse(commitExpression.Text);
        }


        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
