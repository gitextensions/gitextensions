﻿using GitCommands;
using GitExtUtils;
using GitUIPluginInterfaces;

namespace GitExtensions.Plugins.CreateLocalBranches
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
            GitArgumentBuilder args = new("branch") { "-a" };
            string[] references = _gitUiCommands.GitModule.GitExecutable.GetOutput(args)
                .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            if (references.Length == 0)
            {
                MessageBox.Show(this, "No remote branches found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            MessageBox.Show(this, string.Format("{0} local tracking branches have been created/updated.", references.Length),
                "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        }
    }
}
