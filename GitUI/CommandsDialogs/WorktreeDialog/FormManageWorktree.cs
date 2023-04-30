using GitCommands;
using GitExtUtils;
using GitExtUtils.GitUI;
using Microsoft;
using ResourceManager;

namespace GitUI.CommandsDialogs.WorktreeDialog
{
    public partial class FormManageWorktree : GitExtensionsDialog
    {
        private readonly TranslationString _switchWorktreeText = new("Are you sure you want to switch to this worktree?");
        private readonly TranslationString _switchWorktreeTitle = new("Open a worktree");
        private readonly TranslationString _deleteWorktreeText = new("Are you sure you want to delete this worktree?");
        private readonly TranslationString _deleteWorktreeTitle = new("Delete a worktree");
        private readonly TranslationString _deleteWorktreeFailedText = new("Failed to delete a worktree");

        private List<WorkTree>? _worktrees;

        public bool ShouldRefreshRevisionGrid { get; private set; }

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormManageWorktree()
        {
            InitializeComponent();
        }

        public FormManageWorktree(GitUICommands commands)
            : base(commands, enablePositionRestore: false)
        {
            InitializeComponent();

            Sha1.Width = DpiUtil.Scale(53);
            Worktrees.AutoGenerateColumns = false;

            Path.DataPropertyName = nameof(WorkTree.Path);
            Type.DataPropertyName = nameof(WorkTree.Type);
            Branch.DataPropertyName = nameof(WorkTree.Branch);
            Sha1.DataPropertyName = nameof(WorkTree.Sha1);

            Worktrees.Columns[3].DefaultCellStyle.Font = AppSettings.MonospaceFont;
            Worktrees.Columns[3].DefaultCellStyle.WrapMode = DataGridViewTriState.True;

            InitializeComplete();
        }

        /// <summary>
        /// If this is not null before showing the dialog the given
        /// remote name will be preselected in the listbox.
        /// </summary>
        public string? PreselectRemoteOnLoad { get; set; }

        protected override void OnRuntimeLoad(EventArgs e)
        {
            base.OnRuntimeLoad(e);

            Initialize();
        }

        private void Initialize()
        {
            GitArgumentBuilder args = new("worktree")
            {
                "list",
                "--porcelain"
            };
            string lines = Module.GitExecutable.GetOutput(args);

            _worktrees = new List<WorkTree>();
            WorkTree? currentWorktree = null;
            foreach (var line in lines.LazySplit('\n'))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var strings = line.Split(' ');
                if (strings[0] == "worktree")
                {
                    currentWorktree = new WorkTree { Path = Module.GetWindowsPath(line.Substring(9)) };
                    currentWorktree.IsDeleted = !Directory.Exists(currentWorktree.Path);
                    _worktrees.Add(currentWorktree);
                }
                else if (strings[0] == "HEAD")
                {
                    Validates.NotNull(currentWorktree);
                    currentWorktree.Sha1 = strings[1];
                }
                else
                {
                    Validates.NotNull(currentWorktree);
                    switch (strings[0])
                    {
                        case "bare":
                            currentWorktree.Type = HeadType.Bare;
                            break;
                        case "branch":
                            currentWorktree.Type = HeadType.Branch;
                            currentWorktree.Branch = CleanBranchName(strings[1]);

                            string? CleanBranchName(string? branch)
                                => branch != null && branch.StartsWith(GitRefName.RefsHeadsPrefix) ? branch.Substring(GitRefName.RefsHeadsPrefix.Length) : branch;

                            break;
                        case "detached":
                            currentWorktree.Type = HeadType.Detached;
                            break;
                    }
                }
            }

            Worktrees.DataSource = _worktrees;

            Font font = Worktrees.DefaultCellStyle.Font;
            Font deletedFont = new(font.FontFamily, font.Size, font.Style | FontStyle.Strikeout);

            for (var i = 0; i < Worktrees.Rows.Count; i++)
            {
                if (_worktrees[i].IsDeleted)
                {
                    Worktrees.Rows[i].DefaultCellStyle.Font = deletedFont;
                }
            }

            buttonPruneWorktrees.Enabled = _worktrees.Skip(1).Any(w => w.IsDeleted);
        }

        /// <summary>
        /// Here are the 3 types of lines return by the `worktree list --porcelain` that should be handled:
        ///
        /// 1:
        /// worktree /path/to/bare-source
        /// bare
        ///
        /// 2:
        /// /worktree /path/to/linked-worktree
        /// /HEAD abcd1234abcd1234abcd1234abcd1234abcd1234
        /// /branch refs/heads/master
        ///
        /// 3:
        /// worktree /path/to/other-linked-worktree
        /// HEAD 1234abc1234abc1234abc1234abc1234abc1234a
        /// detached.
        /// </summary>
        private class WorkTree
        {
            public string? Path { get; set; }
            public HeadType Type { get; set; }
            public string? Sha1 { get; set; }
            public string? Branch { get; set; }
            public bool IsDeleted { get; set; }
        }

        private enum HeadType
        {
            Bare,
            Branch,
            Detached
        }

        private void buttonPruneWorktrees_Click(object sender, EventArgs e) => PruneWorktrees();

        private void PruneWorktrees()
        {
            UICommands.StartCommandLineProcessDialog(this, command: null, "worktree prune");
            Initialize();
        }

        private void buttonDeleteSelectedWorktree_Click(object sender, EventArgs e)
        {
            if (!CanActOnSelectedWorkspace(out var workTree))
            {
                return;
            }

            if (MessageBox.Show(this, _deleteWorktreeText.Text, _deleteWorktreeTitle.Text,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                Validates.NotNull(workTree.Path);

                if (workTree.Path.TryDeleteDirectory(out string? errorMessage))
                {
                    PruneWorktrees();
                }
                else
                {
                    MessageBox.Show(this, $@"{_deleteWorktreeFailedText.Text}: {workTree.Path}{Environment.NewLine}{errorMessage}", TranslatedStrings.Error,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void buttonOpenSelectedWorktree_Click(object sender, EventArgs e)
        {
            if (!CanActOnSelectedWorkspace(out var workTree))
            {
                return;
            }

            if (AppSettings.DontConfirmSwitchWorktree || MessageBox.Show(this,
                    _switchWorktreeText.Text, _switchWorktreeTitle.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (Directory.Exists(workTree.Path))
                {
                    OpenWorktree(workTree.Path);
                }
            }
        }

        private void OpenWorktree(string workTreePath)
        {
            ((FormBrowse)Owner).SetWorkingDir(System.IO.Path.GetFullPath(workTreePath));
            Close();
        }

        private void Worktrees_SelectionChanged(object sender, EventArgs e)
        {
            buttonDeleteSelectedWorktree.Enabled = CanDeleteSelectedWorkspace();
            buttonOpenSelectedWorktree.Enabled = CanActOnSelectedWorkspace(out _);
        }

        private bool CanDeleteSelectedWorkspace()
            => CanActOnSelectedWorkspace(out _) && Worktrees.SelectedRows[0].Index != 0;

        private bool CanActOnSelectedWorkspace(out WorkTree workTree)
        {
            workTree = null;

            if (_worktrees == null || _worktrees.Count == 1 || Worktrees.SelectedRows.Count == 0)
            {
                return false;
            }

            workTree = _worktrees[Worktrees.SelectedRows[0].Index];

            if (workTree.IsDeleted)
            {
                return false;
            }

            return !IsCurrentlyOpenedWorktree(workTree);
        }

        private bool IsCurrentlyOpenedWorktree(WorkTree workTree)
            => new DirectoryInfo(UICommands.GitModule.WorkingDir).FullName.TrimEnd('\\') == new DirectoryInfo(workTree.Path).FullName.TrimEnd('\\');

        private void buttonCreateNewWorktree_Click(object sender, EventArgs e)
        {
            using FormCreateWorktree formCreateWorktree = new(UICommands, _worktrees[0].Path);
            DialogResult dialogResult = formCreateWorktree.ShowDialog(this);
            if (dialogResult != DialogResult.OK)
            {
                return;
            }

            if (formCreateWorktree.OpenWorktree)
            {
                GitModule newModule = new(formCreateWorktree.WorktreeDirectory);
                if (newModule.IsValidGitWorkingDir())
                {
                    OpenWorktree(formCreateWorktree.WorktreeDirectory);
                }
            }
            else
            {
                ShouldRefreshRevisionGrid = true;
                Initialize();
            }
        }
    }
}
