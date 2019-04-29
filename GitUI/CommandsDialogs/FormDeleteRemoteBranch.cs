using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git;
using GitUI.Browsing.Dialogs;
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

        private readonly HashSet<string> _mergedBranches = new HashSet<string>();
        private readonly string _defaultRemoteBranch;

        private readonly IScriptManager _scriptManager;
        private readonly IScriptRunner _scriptRunner;

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormDeleteRemoteBranch()
        {
            InitializeComponent();
        }

        public FormDeleteRemoteBranch(GitUICommands commands, string defaultRemoteBranch)
            : base(commands)
        {
            _defaultRemoteBranch = defaultRemoteBranch;
            InitializeComponent();
            InitializeComplete();

            _scriptManager = new ScriptManager();

            var gitUIEventArgs = new GitUIEventArgs(this, UICommands);
            var simpleDialog = new SimpleDialog(this);
            var scriptOptionsParser = new ScriptOptionsParser(simpleDialog);

            _scriptRunner = new ScriptRunner(Module, gitUIEventArgs, scriptOptionsParser, simpleDialog, _scriptManager);
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

                foreach (var (remote, branches) in selectedBranches.GroupBy(b => b.Remote))
                {
                    EnsurePageant(remote);

                    var cmd = new GitDeleteRemoteBranchesCmd(remote, branches.Select(x => x.LocalName));

                    var scripts = _scriptManager.GetScripts()
                        .Where(x => x.Enabled && x.OnEvent == ScriptEvent.BeforePush)
                        .Where(x => x.OnEvent == ScriptEvent.BeforeCheckout);

                    foreach (var script in scripts)
                    {
                        _scriptRunner.RunScript(script);
                    }

                    using (var form = new FormRemoteProcess(Module, cmd.Arguments)
                    {
                        Remote = remote
                    })
                    {
                        form.ShowDialog();

                        if (!Module.InTheMiddleOfAction() && !form.ErrorOccurred())
                        {
                            scripts = _scriptManager.GetScripts()
                                .Where(x => x.Enabled && x.OnEvent == ScriptEvent.AfterPush)
                                .Where(x => x.OnEvent == ScriptEvent.BeforeCheckout);

                            foreach (var script in scripts)
                            {
                                _scriptRunner.RunScript(script);
                            }
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