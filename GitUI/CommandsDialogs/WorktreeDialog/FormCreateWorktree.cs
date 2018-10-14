using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs.WorktreeDialog
{
    public sealed partial class FormCreateWorktree : GitModuleForm
    {
        private readonly AsyncLoader _branchesLoader = new AsyncLoader();
        private readonly char[] _invalidCharsInPath = Path.GetInvalidFileNameChars();

        private string _initialDirectoryPath;

        public string WorktreeDirectory => newWorktreeDirectory.Text;
        public bool OpenWorktree => openWorktreeCheckBox.Checked;

        public IReadOnlyList<IGitRef> ExistingBranches { get; set; }

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormCreateWorktree()
        {
            InitializeComponent();
        }

        public FormCreateWorktree(GitUICommands commands)
            : base(commands)
        {
            InitializeComponent();
            InitializeComplete();
        }

        private void FormCreateWorktree_Load(object sender, EventArgs e)
        {
            _initialDirectoryPath = GetWorktreeDirectory();
            LoadBranchesAsync();

            string GetWorktreeDirectory()
            {
                return UICommands.GitModule.WorkingDir.TrimEnd('\\', '/');
            }

            void LoadBranchesAsync()
            {
                var selectedBranch = UICommands.GitModule.GetSelectedBranch();
                ExistingBranches = Module.GetRefs(false);
                comboBoxBranches.Text = Strings.LoadingData;
                ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                {
                    await _branchesLoader.LoadAsync(
                        () => ExistingBranches.Where(r => r.Name != selectedBranch).ToList(),
                        list =>
                        {
                            comboBoxBranches.Text = string.Empty;
                            comboBoxBranches.DataSource = list;
                            comboBoxBranches.DisplayMember = nameof(IGitRef.LocalName);
                        });

                    await this.SwitchToMainThreadAsync();
                    if (comboBoxBranches.Items.Count == 0)
                    {
                        radioButtonCreateNewBranch.Checked = true;
                        radioButtonCheckoutExistingBranch.Enabled = false;
                    }
                    else
                    {
                        radioButtonCheckoutExistingBranch.Checked = true;
                    }

                    ValidateWorktreeOptions();
                });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _branchesLoader.Dispose();

                components?.Dispose();
            }

            base.Dispose(disposing);
        }

        private void comboBoxBranches_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                CreateWorktree();
            }
        }

        private void createWorktreeButton_Click(object sender, EventArgs e)
        {
            CreateWorktree();
        }

        private void CreateWorktree()
        {
            // https://git-scm.com/docs/git-worktree

            var args = new GitArgumentBuilder("worktree")
            {
                "add",
                WorktreeDirectory.Quote(),
                { radioButtonCreateNewBranch.Checked, $"-b {textBoxNewBranchName.Text}", ((GitRef)comboBoxBranches.SelectedItem).Name }
            };

            UICommands.StartGitCommandProcessDialog(this, args);
            DialogResult = DialogResult.OK;
        }

        private void ValidateWorktreeOptions()
        {
            comboBoxBranches.Enabled = radioButtonCheckoutExistingBranch.Checked;
            textBoxNewBranchName.Enabled = radioButtonCreateNewBranch.Checked;
            if (radioButtonCheckoutExistingBranch.Checked)
            {
                createWorktreeButton.Enabled = comboBoxBranches.SelectedItem != null;
            }
            else
            {
                createWorktreeButton.Enabled = !(string.IsNullOrWhiteSpace(textBoxNewBranchName.Text)
                                                 || ExistingBranches.Any(b => b.Name == textBoxNewBranchName.Text));
            }

            if (createWorktreeButton.Enabled)
            {
                createWorktreeButton.Enabled = IsTargetFolderValid();
            }

            return;

            bool IsTargetFolderValid()
            {
                if (string.IsNullOrWhiteSpace(newWorktreeDirectory.Text))
                {
                    return false;
                }

                try
                {
                    var directoryInfo = new DirectoryInfo(newWorktreeDirectory.Text);
                    return !directoryInfo.Exists || (!directoryInfo.EnumerateFiles().Any() && !directoryInfo.EnumerateDirectories().Any());
                }
                catch
                {
                    return false;
                }
            }
        }

        private void ValidateWorktreeOptions(object sender, EventArgs e)
        {
            ValidateWorktreeOptions();
        }

        private void UpdateWorktreePathAndValidateWorktreeOptions(object sender, EventArgs e)
        {
            UpdateWorktreePath();

            ValidateWorktreeOptions();

            return;

            void UpdateWorktreePath()
            {
                var branchNameNormalized = NormalizeBranchName(radioButtonCheckoutExistingBranch.Checked
                    ? ((IGitRef)comboBoxBranches.SelectedItem).Name
                    : textBoxNewBranchName.Text);
                newWorktreeDirectory.Text = $"{_initialDirectoryPath}_{branchNameNormalized}";
            }

            string NormalizeBranchName(string branchName)
            {
                return string.Join("_", branchName.Split(_invalidCharsInPath, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
            }
        }
    }
}
