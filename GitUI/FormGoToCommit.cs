using System;
using GitCommands;

namespace GitUI
{
    public sealed partial class FormGoToCommit : GitModuleForm
    {
        public FormGoToCommit(GitUICommands aCommands)
            : base(aCommands)
        {
            InitializeComponent();
            Translate();
        }

        public string GetRevision()
        {
            return Module.RevParse(commitExpression.Text);
        }


        private void goButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
