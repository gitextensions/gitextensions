﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitUI.Script;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormDeleteRemoteBranch : GitModuleForm
    {
        private readonly TranslationString _deleteRemoteBranchesCaption = new TranslationString("Delete remote branches");
        private readonly TranslationString _confirmDeleteUnmergedRemoteBranchMessage =
            new TranslationString("At least one remote branch is unmerged. Are you sure you want to delete it?" + Environment.NewLine + "Deleting a branch can cause commits to be deleted too!");

        private readonly string _defaultRemoteBranch;
        private readonly HashSet<string> _mergedBranches = new HashSet<string>();

        public FormDeleteRemoteBranch(GitUICommands aCommands, string defaultRemoteBranch)
            : base(aCommands)
        {
            InitializeComponent();
            Translate();
            _defaultRemoteBranch = defaultRemoteBranch;
        }

        private void FormDeleteRemoteBranchLoad(object sender, EventArgs e)
        {
            Branches.BranchesToSelect = Module.GetRefs(tags: true, branches: true).Where(h => h.IsRemote).ToList();
            foreach (var branch in Module.GetMergedRemoteBranches())
            {
                _mergedBranches.Add(branch);
            }

            if (_defaultRemoteBranch != null)
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
                    if (MessageBox.Show(this, _confirmDeleteUnmergedRemoteBranchMessage.Text, _deleteRemoteBranchesCaption.Text, MessageBoxButtons.YesNo) != DialogResult.Yes)
                    {
                        return;
                    }
                }

                foreach (var remoteGroup in selectedBranches.GroupBy(b => b.Remote))
                {
                    string remote = remoteGroup.Key;

                    EnsurePageant(remote);

                    var cmd = new GitDeleteRemoteBranchesCmd(remote, remoteGroup);

                    ScriptManager.RunEventScripts(this, ScriptEvent.BeforePush);

                    using (var form = new FormRemoteProcess(Module, cmd.ToLine())
                    {
                        Remote = remote
                    })
                    {
                        form.ShowDialog();

                        if (!Module.InTheMiddleOfAction() && !form.ErrorOccurred())
                        {
                            ScriptManager.RunEventScripts(this, ScriptEvent.AfterPush);
                        }
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
            if (GitCommandHelpers.Plink())
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