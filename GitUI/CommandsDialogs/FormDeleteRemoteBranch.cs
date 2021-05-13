using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git.Commands;
using GitUI.HelperDialogs;
using GitUI.Script;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormDeleteRemoteBranch : GitModuleForm
    {
        private readonly TranslationString _deleteRemoteBranchesCaption = new("Delete remote branches");
        private readonly TranslationString _confirmDeleteUnmergedRemoteBranchMessage =
            new("At least one remote branch is unmerged. Are you sure you want to delete it?" + Environment.NewLine + "Deleting a branch can cause commits to be deleted too!");

        private readonly HashSet<string> _mergedBranches = new();
        private readonly string _defaultRemoteBranch;

        [Obsolete("For VS designer and translation test only. Do not remove.")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private FormDeleteRemoteBranch()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            InitializeComponent();
        }

        public FormDeleteRemoteBranch(GitUICommands commands, string defaultRemoteBranch)
            : base(commands)
        {
            _defaultRemoteBranch = defaultRemoteBranch;
            InitializeComponent();
            InitializeComplete();
        }

        private void FormDeleteRemoteBranchLoad(object sender, EventArgs e)
        {
            Branches.BranchesToSelect = Module.GetRefs(RefsFilter.Remotes).ToList();
            foreach (var branch in Module.GetMergedRemoteBranches())
            {
                _mergedBranches.Add(branch);
            }

            if (_defaultRemoteBranch is not null)
            {
                Branches.SetSelectedText(_defaultRemoteBranch);
            }
        }

        private void OkClick(object sender, EventArgs e)
        {
            try
            {
                if (!DeleteRemote.Checked)
                {
                    return;
                }

                List<IGitRef> selectedBranches = Branches.GetSelectedBranches().ToList();

                var hasUnmergedBranches = selectedBranches.Any(branch => !_mergedBranches.Contains(branch.CompleteName));

                if (hasUnmergedBranches)
                {
                    if (MessageBox.Show(this, _confirmDeleteUnmergedRemoteBranchMessage.Text, _deleteRemoteBranchesCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    {
                        return;
                    }
                }

                foreach (var (remote, branches) in selectedBranches.GroupBy(b => b.Remote))
                {
                    EnsurePageant(remote);

                    GitDeleteRemoteBranchesCmd cmd = new(remote, branches.Select(x => x.LocalName));

                    bool success = ScriptManager.RunEventScripts(this, ScriptEvent.BeforePush);
                    if (!success)
                    {
                        return;
                    }

                    using FormRemoteProcess form = new(UICommands, process: null, cmd.Arguments)
                    {
                        Remote = remote
                    };
                    form.ShowDialog();

                    if (!Module.InTheMiddleOfAction() && !form.ErrorOccurred())
                    {
                        ScriptManager.RunEventScripts(this, ScriptEvent.AfterPush);
                    }
                }

                UICommands.RepoChangedNotifier.Notify();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }

            Close();
        }

        private void DeleteRemote_CheckedChanged(object sender, EventArgs e)
        {
            Delete.Enabled = DeleteRemote.Checked;
        }

        private void EnsurePageant(string remote)
        {
            if (GitSshHelpers.Plink())
            {
                if (!File.Exists(AppSettings.Pageant))
                {
                    MessageBoxes.PAgentNotFound(this);
                }
                else
                {
                    Module.StartPageantForRemote(remote);
                }
            }
        }
    }
}
