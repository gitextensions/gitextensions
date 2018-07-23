using System;
using System.Windows.Forms;
using GitUIPluginInterfaces;

namespace CreateLocalBranches
{
    public partial class CreateLocalBranchesForm : ResourceManager.GitExtensionsFormBase
    {
        private readonly GitUIEventArgs _gitUiCommands;

        public CreateLocalBranchesForm(GitUIEventArgs gitUiCommands)
        {
            InitializeComponent();
            InitializeComplete();

            _gitUiCommands = gitUiCommands;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string[] references = _gitUiCommands.GitModule.RunGitCmd("branch -a")
                .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            if (references.Length == 0)
            {
                MessageBox.Show(this, "No remote branches found.");
                DialogResult = DialogResult.Cancel;
                return;
            }

            foreach (string reference in references)
            {
                try
                {
                    string branchName = reference.Trim('*', ' ', '\n', '\r');

                    if (branchName.StartsWith("remotes/" + _NO_TRANSLATE_Remote.Text + "/"))
                    {
                        _gitUiCommands.GitModule.RunGitCmd(string.Concat("branch --track ", branchName.Replace("remotes/" + _NO_TRANSLATE_Remote.Text + "/", ""), " ", branchName));
                    }
                }
                catch
                {
                }
            }

            MessageBox.Show(this, string.Format("{0} local tracking branches have been created/updated.",
                                          references.Length));
            Close();
        }
    }
}
