using System;
using System.Windows.Forms;
using GitCommands;
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
            var args = new GitArgumentBuilder("branch") { "-a" };
            string[] references = _gitUiCommands.GitModule.GitExecutable.GetOutput(args)
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
                        args = new GitArgumentBuilder("branch")
                        {
                            "--track",
                            branchName.Replace($"remotes/{_NO_TRANSLATE_Remote.Text}/", ""),
                            branchName
                        };
                        _gitUiCommands.GitModule.GitExecutable.GetOutput(args);
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
