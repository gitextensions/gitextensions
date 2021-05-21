using System;
using System.Drawing;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git.Commands;
using GitCommands.Settings;
using GitExtUtils.GitUI.Theming;
using GitUI.HelperDialogs;
using GitUI.Script;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.Settings;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    /// <summary>Form to merge a branch into the current branch.</summary>
    public partial class FormMergeBranch : GitModuleForm
    {
        private readonly TranslationString _strategyTooltipText = new("resolve " + Environment.NewLine + "This can only resolve two heads (i.e. the current branch and another branch you pulled from) using a 3-way merge algorithm." + Environment.NewLine + "It tries to carefully detect criss-cross merge ambiguities and is considered generally safe and fast. " + Environment.NewLine + Environment.NewLine + "recursive " + Environment.NewLine + "This can only resolve two heads using a 3-way merge algorithm. When there is more than one common ancestor that can be " + Environment.NewLine + "used for 3-way merge, it creates a merged tree of the common ancestors and uses that as the reference tree for the 3-way" + Environment.NewLine + "merge. Additionally this can detect and handle merges involving renames. This is the default merge strategy when pulling or " + Environment.NewLine + "merging one branch. " + Environment.NewLine + Environment.NewLine + "octopus " + Environment.NewLine + "This resolves cases with more than two heads, but refuses to do a complex merge that needs manual resolution. It is " + Environment.NewLine + "primarily meant to be used for bundling topic branch heads together. This is the default merge strategy when pulling or " + Environment.NewLine + "merging more than one branch. " + Environment.NewLine + Environment.NewLine + "ours " + Environment.NewLine + "This resolves any number of heads, but the resulting tree of the merge is always that of the current branch head, effectively " + Environment.NewLine + "ignoring all changes from all other branches. It is meant to be used to supersede old development history of side branches." + Environment.NewLine + Environment.NewLine + "subtree " + Environment.NewLine + "This is a modified recursive strategy. When merging trees A and B, if B corresponds to a subtree of A, B is first adjusted to " + Environment.NewLine + "match the tree structure of A, instead of reading the trees at the same level. This adjustment is also done to the common " + Environment.NewLine + "ancestor tree.");
        private readonly TranslationString _formMergeBranchHoverShowImageLabelText = new("Hover to see scenario when fast forward is possible.");
        private readonly string? _defaultBranch;
        private ICommitMessageManager _commitMessageManager;

        [Obsolete("For VS designer and translation test only. Do not remove.")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private FormMergeBranch()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            InitializeComponent();
        }

        /// <summary>Initializes <see cref="FormMergeBranch"/>.</summary>
        /// <param name="defaultBranch">Branch to merge into the current branch.</param>
        public FormMergeBranch(GitUICommands commands, string? defaultBranch)
            : base(commands)
        {
            InitializeComponent();
            helpImageDisplayUserControl1.Image1 = Properties.Images.HelpCommandMerge.AdaptLightness();
            helpImageDisplayUserControl1.Image2 = Properties.Images.HelpCommandMergeFastForward.AdaptLightness();
            InitializeComplete();

            _commitMessageManager = new CommitMessageManager(Module.WorkingDirGitDir, Module.CommitEncoding);

            currentBranchLabel.Font = new Font(currentBranchLabel.Font, FontStyle.Bold);
            noCommit.Checked = AppSettings.DontCommitMerge;

            helpImageDisplayUserControl1.IsOnHoverShowImage2NoticeText = _formMergeBranchHoverShowImageLabelText.Text;
            helpImageDisplayUserControl1.Visible = !AppSettings.DontShowHelpImages;
            _defaultBranch = defaultBranch;

            IDetachedSettings detachedSettings = Module.GetEffectiveSettings()
                .Detached();

            noFastForward.Checked = detachedSettings.NoFastForwardMerge;

            IDetailedSettings detailedSettings = Module.GetEffectiveSettings()
                .Detailed();

            addLogMessages.Checked = detailedSettings.AddMergeLogMessages;
            nbMessages.Value = detailedSettings.MergeLogMessagesCount;

            advanced.Checked = AppSettings.AlwaysShowAdvOpt;
            advanced_CheckedChanged(this, EventArgs.Empty);

            Branches.Select();
        }

        private void FormMergeBranchLoad(object sender, EventArgs e)
        {
            var selectedHead = Module.GetSelectedBranch();
            currentBranchLabel.Text = selectedHead;

            // Offer rebase on refs also for tags (but not stash, notes etc)
            Branches.BranchesToSelect = Module.GetRefs(RefsFilter.Heads | RefsFilter.Remotes | RefsFilter.Tags);

            if (_defaultBranch is not null)
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
            IDetachedSettings detachedSettings = Module.GetEffectiveSettings()
                .Detached();

            detachedSettings.NoFastForwardMerge = noFastForward.Checked;
            AppSettings.DontCommitMerge = noCommit.Checked;

            bool success = ScriptManager.RunEventScripts(this, ScriptEvent.BeforeMerge);
            if (!success)
            {
                return;
            }

            string? mergeMessagePath = null;
            if (addMergeMessage.Checked)
            {
                // [!] Do not reset the last commit message stored in AppSettings.LastCommitMessage

                _commitMessageManager.WriteCommitMessageToFile(mergeMessage.Text, CommitMessageType.Merge,
                                                               usingCommitTemplate: false,
                                                               ensureCommitMessageSecondLineEmpty: false);
                mergeMessagePath = _commitMessageManager.MergeMessagePath;
            }

            var command = GitCommandHelpers.MergeBranchCmd(Branches.GetSelectedText(),
                                                            fastForward.Checked,
                                                            squash.Checked,
                                                            noCommit.Checked,
                                                            _NO_TRANSLATE_mergeStrategy.Text,
                                                            allowUnrelatedHistories.Checked,
                                                            mergeMessagePath,
                                                            addLogMessages.Checked ? (int)nbMessages.Value : (int?)null);
            success = FormProcess.ShowDialog(this, process: null, arguments: command, Module.WorkingDir, input: null, useDialogSettings: true);

            var wasConflict = MergeConflictHandler.HandleMergeConflicts(UICommands, this, !noCommit.Checked);

            if (success || wasConflict)
            {
                ScriptManager.RunEventScripts(this, ScriptEvent.AfterMerge);
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
            NonDefaultMergeStrategy_CheckedChanged(this, EventArgs.Empty);

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

            IDetailedSettings detailedSettings = Module.GetEffectiveSettings()
                .Detailed();

            detailedSettings.AddMergeLogMessages = addLogMessages.Checked;
        }

        private void addMergeMessage_CheckedChanged(object sender, EventArgs e)
        {
            mergeMessage.Enabled = addMergeMessage.Checked;
        }

        private void nbMessages_ValueChanged(object sender, EventArgs e)
        {
            IDetailedSettings detailedSettings = Module.GetEffectiveSettings()
                .Detailed();

            detailedSettings.MergeLogMessagesCount = Convert.ToInt32(nbMessages.Value);
        }
    }
}
