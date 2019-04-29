using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitUI.Browsing.Dialogs;
using GitUI.Script;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    /// <summary>Form to merge a branch into the current branch.</summary>
    public partial class FormMergeBranch : GitModuleForm
    {
        private readonly TranslationString _strategyTooltipText = new TranslationString("resolve " + Environment.NewLine + "This can only resolve two heads (i.e. the current branch and another branch you pulled from) using a 3-way merge algorithm." + Environment.NewLine + "It tries to carefully detect criss-cross merge ambiguities and is considered generally safe and fast. " + Environment.NewLine + Environment.NewLine + "recursive " + Environment.NewLine + "This can only resolve two heads using a 3-way merge algorithm. When there is more than one common ancestor that can be " + Environment.NewLine + "used for 3-way merge, it creates a merged tree of the common ancestors and uses that as the reference tree for the 3-way" + Environment.NewLine + "merge. Additionally this can detect and handle merges involving renames. This is the default merge strategy when pulling or " + Environment.NewLine + "merging one branch. " + Environment.NewLine + Environment.NewLine + "octopus " + Environment.NewLine + "This resolves cases with more than two heads, but refuses to do a complex merge that needs manual resolution. It is " + Environment.NewLine + "primarily meant to be used for bundling topic branch heads together. This is the default merge strategy when pulling or " + Environment.NewLine + "merging more than one branch. " + Environment.NewLine + Environment.NewLine + "ours " + Environment.NewLine + "This resolves any number of heads, but the resulting tree of the merge is always that of the current branch head, effectively " + Environment.NewLine + "ignoring all changes from all other branches. It is meant to be used to supersede old development history of side branches." + Environment.NewLine + Environment.NewLine + "subtree " + Environment.NewLine + "This is a modified recursive strategy. When merging trees A and B, if B corresponds to a subtree of A, B is first adjusted to " + Environment.NewLine + "match the tree structure of A, instead of reading the trees at the same level. This adjustment is also done to the common " + Environment.NewLine + "ancestor tree.");
        private readonly TranslationString _formMergeBranchHoverShowImageLabelText = new TranslationString("Hover to see scenario when fast forward is possible.");
        private readonly string _defaultBranch;

        private readonly IScriptManager _scriptManager;
        private readonly IScriptRunner _scriptRunner;

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormMergeBranch()
        {
            InitializeComponent();
        }

        /// <summary>Initializes <see cref="FormMergeBranch"/>.</summary>
        /// <param name="defaultBranch">Branch to merge into the current branch.</param>
        public FormMergeBranch(GitUICommands commands, string defaultBranch)
            : base(commands)
        {
            InitializeComponent();
            InitializeComplete();

            _scriptManager = new ScriptManager();

            var gitUIEventArgs = new GitUIEventArgs(this, UICommands);
            var simpleDialog = new SimpleDialog(this);
            var scriptOptionsParser = new ScriptOptionsParser(simpleDialog);

            _scriptRunner = new ScriptRunner(Module, gitUIEventArgs, scriptOptionsParser, simpleDialog, _scriptManager);

            currentBranchLabel.Font = new Font(currentBranchLabel.Font, FontStyle.Bold);
            noCommit.Checked = AppSettings.DontCommitMerge;

            helpImageDisplayUserControl1.IsOnHoverShowImage2NoticeText = _formMergeBranchHoverShowImageLabelText.Text;
            helpImageDisplayUserControl1.Visible = !AppSettings.DontShowHelpImages;
            _defaultBranch = defaultBranch;

            noFastForward.Checked = Module.EffectiveSettings.NoFastForwardMerge;
            addLogMessages.Checked = Module.EffectiveSettings.Detailed.AddMergeLogMessages.ValueOrDefault;
            nbMessages.Value = Module.EffectiveSettings.Detailed.MergeLogMessagesCount.ValueOrDefault;

            advanced.Checked = AppSettings.AlwaysShowAdvOpt;
            advanced_CheckedChanged(null, null);

            Branches.Select();
        }

        private void FormMergeBranchLoad(object sender, EventArgs e)
        {
            var selectedHead = Module.GetSelectedBranch();
            currentBranchLabel.Text = selectedHead;

            Branches.BranchesToSelect = Module.GetRefs(true, true);

            if (_defaultBranch != null)
            {
                Branches.SetSelectedText(_defaultBranch);
            }
            else
            {
                string merge = Module.GetRemoteBranch(selectedHead);
                if (!string.IsNullOrEmpty(merge))
                {
                    Branches.SetSelectedText(merge);
                }
            }

            if (!GitVersion.Current.SupportMergeUnrelatedHistory)
            {
                allowUnrelatedHistories.Visible = false;
                allowUnrelatedHistories.Checked = false;
            }

            Branches.Select();
        }

        private void OkClick(object sender, EventArgs e)
        {
            Module.EffectiveSettings.NoFastForwardMerge = noFastForward.Checked;
            AppSettings.DontCommitMerge = noCommit.Checked;

            var scripts = _scriptManager.GetScripts()
                .Where(x => x.Enabled && x.OnEvent == ScriptEvent.BeforeMerge)
                .Where(x => x.OnEvent == ScriptEvent.BeforeCheckout);

            foreach (var script in scripts)
            {
                _scriptRunner.RunScript(script);
            }

            var successfullyMerged = FormProcess.ShowDialog(this, GitCommandHelpers.MergeBranchCmd(Branches.GetSelectedText(),
                                                                                                   fastForward.Checked,
                                                                                                   squash.Checked,
                                                                                                   noCommit.Checked,
                                                                                                   _NO_TRANSLATE_mergeStrategy.Text,
                                                                                                   allowUnrelatedHistories.Checked,
                                                                                                   addMergeMessage.Checked ? mergeMessage.Text : null,
                                                                                                   addLogMessages.Checked ? (int)nbMessages.Value : (int?)null));

            var wasConflict = MergeConflictHandler.HandleMergeConflicts(UICommands, this, !noCommit.Checked);

            if (successfullyMerged || wasConflict)
            {
                scripts = _scriptManager.GetScripts()
                    .Where(x => x.Enabled && x.OnEvent == ScriptEvent.AfterMerge)
                    .Where(x => x.OnEvent == ScriptEvent.AfterMerge);

                foreach (var script in scripts)
                {
                    _scriptRunner.RunScript(script);
                }

                UICommands.RepoChangedNotifier.Notify();
                Close();
            }
        }

        private void NonDefaultMergeStrategy_CheckedChanged(object sender, EventArgs e)
        {
            _NO_TRANSLATE_mergeStrategy.Visible = NonDefaultMergeStrategy.Checked;
            strategyHelp.Visible = NonDefaultMergeStrategy.Checked;

            if (!advanced.Checked)
            {
                _NO_TRANSLATE_mergeStrategy.Text = "";
            }
        }

        private void strategyHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            strategyToolTip.SetToolTip(strategyHelp, _strategyTooltipText.Text);
        }

        private void advanced_CheckedChanged(object sender, EventArgs e)
        {
            advancedPanel.Visible = advanced.Checked;
            NonDefaultMergeStrategy_CheckedChanged(null, null);

            if (!advanced.Checked)
            {
                NonDefaultMergeStrategy.Checked = false;
                squash.Checked = false;
                allowUnrelatedHistories.Checked = false;
                addMergeMessage.Checked = false;
            }
        }

        private void fastForward_CheckedChanged(object sender, EventArgs e)
        {
            helpImageDisplayUserControl1.IsOnHoverShowImage2 = true;
        }

        private void noFastForward_CheckedChanged(object sender, EventArgs e)
        {
            helpImageDisplayUserControl1.IsOnHoverShowImage2 = false;
        }

        private void addMessages_CheckedChanged(object sender, EventArgs e)
        {
            nbMessages.Enabled = addLogMessages.Checked;
            Module.EffectiveSettings.Detailed.AddMergeLogMessages.Value = addLogMessages.Checked;
        }

        private void addMergeMessage_CheckedChanged(object sender, EventArgs e)
        {
            mergeMessage.Enabled = addMergeMessage.Checked;
        }

        private void nbMessages_ValueChanged(object sender, EventArgs e)
        {
            Module.EffectiveSettings.Detailed.MergeLogMessagesCount.Value = Convert.ToInt32(nbMessages.Value);
        }
    }
}