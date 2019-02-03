using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitExtUtils.GitUI;
using GitUI.Properties;

namespace GitUI.CommandsDialogs.WorktreeDialog
{
    public partial class FormManageWorktree : GitModuleForm
    {
        private List<WorkTree> _worktrees;

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormManageWorktree()
        {
            InitializeComponent();
        }

        public FormManageWorktree(GitUICommands commands)
            : base(commands)
        {
            InitializeComponent();
            Path.Width = DpiUtil.Scale(35);
            Type.Width = DpiUtil.Scale(37);
            Branch.Width = DpiUtil.Scale(46);
            Sha1.Width = DpiUtil.Scale(37);
            IsDeleted.Width = DpiUtil.Scale(50);
            Open.Width = DpiUtil.Scale(39);
            Delete.Width = DpiUtil.Scale(44);
            Worktrees.AutoGenerateColumns = false;
            InitializeComplete();

            Path.DataPropertyName = nameof(WorkTree.Path);
            Type.DataPropertyName = nameof(WorkTree.Type);
            Branch.DataPropertyName = nameof(WorkTree.Branch);
            Sha1.DataPropertyName = nameof(WorkTree.Sha1);
            IsDeleted.DataPropertyName = nameof(WorkTree.IsDeleted);
        }

        private void FormManageWorktree_Load(object sender, EventArgs e)
        {
            Initialize();
        }

        /// <summary>
        /// If this is not null before showing the dialog the given
        /// remote name will be preselected in the listbox
        /// </summary>
        public string PreselectRemoteOnLoad { get; set; }

        private void Initialize()
        {
            var lines = Module.GitExecutable.GetOutput("worktree list --porcelain").Split('\n').GetEnumerator();

            _worktrees = new List<WorkTree>();
            WorkTree currentWorktree = null;
            while (lines.MoveNext())
            {
                var current = (string)lines.Current;
                if (string.IsNullOrWhiteSpace(current))
                {
                    continue;
                }

                var strings = current.Split(' ');
                if (strings[0] == "worktree")
                {
                    currentWorktree = new WorkTree { Path = current.Substring(9) };
                    currentWorktree.IsDeleted = !Directory.Exists(currentWorktree.Path);
                    _worktrees.Add(currentWorktree);
                }
                else if (strings[0] == "HEAD")
                {
                    currentWorktree.Sha1 = strings[1];
                }
                else
                {
                    switch (strings[0])
                    {
                        case "bare":
                            currentWorktree.Type = HeadType.Bare;
                            break;
                        case "branch":
                            currentWorktree.Type = HeadType.Branch;
                            currentWorktree.Branch = strings[1];
                            break;
                        case "detached":
                            currentWorktree.Type = HeadType.Detached;
                            break;
                    }
                }
            }

            Worktrees.DataSource = _worktrees;
            for (var i = 0; i < Worktrees.Rows.Count; i++)
            {
                if (i == 0)
                {
                    Worktrees.Rows[i].Cells["Delete"].Value = Images.Blank;
                    if (IsCurrentlyOpenedWorktree(_worktrees[0]))
                    {
                        Worktrees.Rows[i].Cells["Open"].Value = Images.Blank;
                    }
                }
                else if (!CanDeleteWorkspace(_worktrees[i]))
                {
                    Worktrees.Rows[i].Cells["Open"].Value = Images.Blank;
                    Worktrees.Rows[i].Cells["Delete"].Value = Images.Blank;
                }
            }

            buttonPruneWorktrees.Enabled = _worktrees.Skip(1).Any(w => w.IsDeleted);
        }

        private bool CanDeleteWorkspace(WorkTree workTree)
        {
            if (workTree.IsDeleted)
            {
                return false;
            }

            if (_worktrees.Count == 1)
            {
                return false;
            }

            if (IsCurrentlyOpenedWorktree(workTree))
            {
                return false;
            }

            return true;
        }

        private bool IsCurrentlyOpenedWorktree(WorkTree workTree)
        {
            return new DirectoryInfo(UICommands.GitModule.WorkingDir).FullName.TrimEnd('\\') == new DirectoryInfo(workTree.Path).FullName.TrimEnd('\\');
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
        /// detached
        /// </summary>
        private class WorkTree
        {
            public string Path { get; set; }
            public HeadType Type { get; set; }
            public string Sha1 { get; set; }
            public string Branch { get; set; }
            public bool IsDeleted { get; set; }
        }

        private enum HeadType
        {
            Bare,
            Branch,
            Detached
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonPruneWorktrees_Click(object sender, EventArgs e)
        {
            PruneWorktrees();
        }

        private void PruneWorktrees()
        {
            UICommands.StartCommandLineProcessDialog(this, "git", "worktree prune");
            Initialize();
        }

        private void Worktrees_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex < 5)
            {
                return;
            }

            var workTree = _worktrees[e.RowIndex];
            if (!CanDeleteWorkspace(workTree))
            {
                return;
            }

            if (e.ColumnIndex == 5)
            {
                if (MessageBox.Show(this, "Are you sure you want to switch to this worktree?", "Open a worktree",
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    if (Directory.Exists(workTree.Path))
                    {
                        ((FormBrowse)Owner).SetWorkingDir(System.IO.Path.GetFullPath(workTree.Path));
                        Close();
                    }
                }

                return;
            }

            if (e.ColumnIndex == 6)
            {
                if (e.RowIndex == 0)
                {
                    return;
                }

                if (MessageBox.Show(this, "Are you sure you want to delete this worktree?", "Delete a worktree",
                        MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    if (Directory.Exists(workTree.Path))
                    {
                        Directory.Delete(workTree.Path, true);
                    }

                    PruneWorktrees();
                }
            }
        }
    }
}