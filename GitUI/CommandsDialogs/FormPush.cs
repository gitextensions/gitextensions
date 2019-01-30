using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Config;
using GitCommands.Git;
using GitCommands.Remotes;
using GitCommands.UserRepositoryHistory;
using GitExtUtils.GitUI;
using GitUI.Script;
using GitUIPluginInterfaces;
using JetBrains.Annotations;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class FormPush : GitModuleForm
    {
        private const string HeadText = "HEAD";
        private const string AllRefs = "[ All ]";
        private const string LocalColumnName = "Local";
        private const string RemoteColumnName = "Remote";
        private const string NewColumnName = "New";
        private const string PushColumnName = "Push";
        private const string ForceColumnName = "Force";
        private const string DeleteColumnName = "Delete";

        private string _currentBranchName;
        private GitRemote _currentBranchRemote;
        private bool _candidateForRebasingMergeCommit;
        private string _selectedBranch;
        private GitRemote _selectedRemote;
        private string _selectedRemoteBranchName;
        private IReadOnlyList<IGitRef> _gitRefs;
        private readonly IGitRemoteManager _remoteManager;

        public bool ErrorOccurred { get; private set; }

        #region Translation
        private readonly TranslationString _branchNewForRemote =
            new TranslationString("The branch you are about to push seems to be a new branch for the remote." +
                                  Environment.NewLine + "Are you sure you want to push this branch?");

        private readonly TranslationString _pushCaption = new TranslationString("Push");

        private readonly TranslationString _pushToCaption = new TranslationString("Push to {0}");

        private readonly TranslationString _selectDestinationDirectory =
            new TranslationString("Please select a destination directory");

        private readonly TranslationString _errorPushToRemoteCaption = new TranslationString("Push to remote");
        private readonly TranslationString _configureRemote = new TranslationString($"Please configure a remote repository first.{Environment.NewLine}Would you like to do it now?");

        private readonly TranslationString _selectTag =
            new TranslationString("You need to select a tag to push or select \"Push all tags\".");

        private readonly TranslationString _updateTrackingReference =
            new TranslationString("The branch {0} does not have a tracking reference. Do you want to add a tracking reference to {1}?");

        private readonly TranslationString _yes = new TranslationString("Yes");
        private readonly TranslationString _no = new TranslationString("No");

        private readonly TranslationString _pullRepositoryMainInstruction = new TranslationString("Pull latest changes from remote repository");
        private readonly TranslationString _pullRepository =
            new TranslationString("The push was rejected because the tip of your current branch is behind its remote counterpart. " +
                "Merge the remote changes before pushing again.");
        private readonly TranslationString _pullRepositoryButtons = new TranslationString("Pull with the default pull action ({0})|Pull with rebase|Pull with merge|Force push|Cancel");
        private readonly TranslationString _pullActionNone = new TranslationString("none");
        private readonly TranslationString _pullActionFetch = new TranslationString("fetch");
        private readonly TranslationString _pullActionRebase = new TranslationString("rebase");
        private readonly TranslationString _pullActionMerge = new TranslationString("merge");
        private readonly TranslationString _pullRepositoryCaption = new TranslationString("Push was rejected from \"{0}\"");
        private readonly TranslationString _dontShowAgain = new TranslationString("Remember my decision.");
        private readonly TranslationString _useForceWithLeaseInstead =
            new TranslationString("Force push may overwrite changes since your last fetch. Do you want to use the safer force with lease instead?");
        private readonly TranslationString _forceWithLeaseTooltips =
            new TranslationString("Force with lease is a safer way to force push. It ensures you only overwrite work that you have seen in your local repository");

        #endregion

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormPush()
        {
            InitializeComponent();
        }

        public FormPush([NotNull] GitUICommands commands)
            : base(commands)
        {
            InitializeComponent();

            NewColumn.Width = DpiUtil.Scale(97);
            PushColumn.Width = DpiUtil.Scale(36);
            ForceColumn.Width = DpiUtil.Scale(101);
            DeleteColumn.Width = DpiUtil.Scale(108);

            InitializeComplete();

            if (!GitVersion.Current.SupportPushForceWithLease)
            {
                ckForceWithLease.Visible = false;
                ForcePushTags.DataBindings.Add("Checked", ForcePushBranches, "Checked",
                    formattingEnabled: false, updateMode: DataSourceUpdateMode.OnPropertyChanged);
            }
            else
            {
                ForcePushTags.DataBindings.Add("Checked", ckForceWithLease, "Checked",
                    formattingEnabled: false, updateMode: DataSourceUpdateMode.OnPropertyChanged);
                toolTip1.SetToolTip(ckForceWithLease, _forceWithLeaseTooltips.Text);
            }

            // can't be set in OnLoad, because after PushAndShowDialogWhenFailed()
            // they are reset to false
            _remoteManager = new GitRemoteManager(() => Module);
            Init();

            void Init()
            {
                _gitRefs = Module.GetRefs();
                if (GitVersion.Current.SupportPushWithRecursiveSubmodulesCheck)
                {
                    RecursiveSubmodules.Enabled = true;
                    RecursiveSubmodules.SelectedIndex = AppSettings.RecursiveSubmodules;
                    if (!GitVersion.Current.SupportPushWithRecursiveSubmodulesOnDemand)
                    {
                        RecursiveSubmodules.Items.RemoveAt(2);
                    }
                }
                else
                {
                    RecursiveSubmodules.Enabled = false;
                    RecursiveSubmodules.SelectedIndex = 0;
                }

                _currentBranchName = Module.GetSelectedBranch();

                // refresh registered git remotes
                UserGitRemotes = _remoteManager.LoadRemotes(false).ToList();
                BindRemotesDropDown(null);

                UpdateBranchDropDown();
                UpdateRemoteBranchDropDown();

                Push.Focus();

                if (AppSettings.AlwaysShowAdvOpt)
                {
                    ShowOptions_LinkClicked(null, null);
                }
            }
        }

        /// <summary>
        /// Gets the list of remotes configured in .git/config file.
        /// </summary>
        private List<GitRemote> UserGitRemotes { get; set; }

        public DialogResult PushAndShowDialogWhenFailed(IWin32Window owner = null)
        {
            if (!PushChanges(owner))
            {
                return ShowDialog(owner);
            }

            return DialogResult.OK;
        }

        private bool CheckIfRemoteExist()
        {
            if (UserGitRemotes.Count < 1)
            {
                if (MessageBox.Show(this, _configureRemote.Text, _errorPushToRemoteCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
                {
                    OpenRemotesDialogAndRefreshList(null);
                    return UserGitRemotes.Count > 0;
                }

                return false;
            }

            return true;
        }

        public void CheckForceWithLease()
        {
            ckForceWithLease.Checked = true;
        }

        private void OpenRemotesDialogAndRefreshList(string selectedRemoteName)
        {
            if (!UICommands.StartRemotesDialog(this, selectedRemoteName))
            {
                return;
            }

            UserGitRemotes = _remoteManager.LoadRemotes(false).ToList();
            BindRemotesDropDown(selectedRemoteName);
        }

        private void PushClick(object sender, EventArgs e)
        {
            DialogResult = PushChanges(this) ? DialogResult.OK : DialogResult.Abort;
        }

        private void BindRemotesDropDown(string selectedRemoteName)
        {
            _NO_TRANSLATE_Remotes.SelectedIndexChanged -= RemotesUpdated;
            _NO_TRANSLATE_Remotes.TextUpdate -= RemotesUpdated;
            _NO_TRANSLATE_Remotes.Sorted = false;
            _NO_TRANSLATE_Remotes.DataSource = UserGitRemotes;
            _NO_TRANSLATE_Remotes.DisplayMember = nameof(GitRemote.Name);
            _NO_TRANSLATE_Remotes.SelectedIndex = -1;

            _NO_TRANSLATE_Remotes.SelectedIndexChanged += RemotesUpdated;
            _NO_TRANSLATE_Remotes.TextUpdate += RemotesUpdated;

            if (selectedRemoteName.IsNullOrEmpty())
            {
                selectedRemoteName = Module.GetSetting(string.Format(SettingKeyString.BranchRemote, _currentBranchName));
            }

            _currentBranchRemote = UserGitRemotes.FirstOrDefault(x => x.Name.Equals(selectedRemoteName, StringComparison.OrdinalIgnoreCase));
            if (_currentBranchRemote != null)
            {
                _NO_TRANSLATE_Remotes.SelectedItem = _currentBranchRemote;
            }
            else if (UserGitRemotes.Any())
            {
                var defaultRemote = UserGitRemotes.FirstOrDefault(x => x.Name.Equals("origin", StringComparison.OrdinalIgnoreCase));

                // we couldn't find the default assigned remote for the selected branch
                // it is usually gets mapped via FormRemotes -> "default pull behavior" tab
                // so pick the default user remote
                if (defaultRemote == null)
                {
                    _NO_TRANSLATE_Remotes.SelectedIndex = 0;
                }
                else
                {
                    _NO_TRANSLATE_Remotes.SelectedItem = defaultRemote;
                }
            }
            else
            {
                _NO_TRANSLATE_Remotes.SelectedIndex = -1;
            }
        }

        private bool IsBranchKnownToRemote(string remote, string branch)
        {
            var remoteRefs = GetRemoteBranches(remote).Where(r => r.LocalName == branch);
            if (remoteRefs.Any())
            {
                return true;
            }

            var localRefs = _gitRefs.Where(r => r.IsHead && r.Name == branch && r.TrackingRemote == remote);
            return localRefs.Any();
        }

        private bool PushChanges(IWin32Window owner)
        {
            ErrorOccurred = false;
            if (PushToUrl.Checked && string.IsNullOrEmpty(PushDestination.Text))
            {
                MessageBox.Show(owner, _selectDestinationDirectory.Text);
                return false;
            }

            if (/* PushToRemote.Checked */!CheckIfRemoteExist())
            {
                return false;
            }

            var selectedRemoteName = _selectedRemote.Name;
            if (TabControlTagBranch.SelectedTab == TagTab && string.IsNullOrEmpty(TagComboBox.Text))
            {
                MessageBox.Show(owner, _selectTag.Text);
                return false;
            }

            // Extra check if the branch is already known to the remote, give a warning when not.
            // This is not possible when the remote is an URL, but this is ok since most users push to
            // known remotes anyway.
            if (TabControlTagBranch.SelectedTab == BranchTab && PushToRemote.Checked &&
                !Module.IsBareRepository())
            {
                // If the current branch is not the default push, and not known by the remote
                // (as far as we know since we are disconnected....)
                if (_NO_TRANSLATE_Branch.Text != AllRefs &&
                    RemoteBranch.Text != _remoteManager.GetDefaultPushRemote(_selectedRemote, _NO_TRANSLATE_Branch.Text) &&
                    !IsBranchKnownToRemote(selectedRemoteName, RemoteBranch.Text))
                {
                    // Ask if this is really what the user wants
                    if (!AppSettings.DontConfirmPushNewBranch &&
                        MessageBox.Show(owner, _branchNewForRemote.Text, _pushCaption.Text, MessageBoxButtons.YesNo) == DialogResult.No)
                    {
                        return false;
                    }
                }
            }

            if (PushToUrl.Checked)
            {
                var path = PushDestination.Text;
                ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Remotes.AddAsMostRecentAsync(path));
            }

            AppSettings.RecursiveSubmodules = RecursiveSubmodules.SelectedIndex;

            var remote = "";
            string destination;
            if (PushToUrl.Checked)
            {
                destination = PushDestination.Text;
            }
            else
            {
                EnsurePageant(selectedRemoteName);

                destination = selectedRemoteName;
                remote = selectedRemoteName.Trim();
            }

            string pushCmd;
            if (TabControlTagBranch.SelectedTab == BranchTab)
            {
                bool track = ReplaceTrackingReference.Checked;
                if (!track && !string.IsNullOrWhiteSpace(RemoteBranch.Text))
                {
                    GitRef selectedLocalBranch = _NO_TRANSLATE_Branch.SelectedItem as GitRef;
                    track = selectedLocalBranch != null && string.IsNullOrEmpty(selectedLocalBranch.TrackingRemote) &&
                            !UserGitRemotes.Any(x => _NO_TRANSLATE_Branch.Text.StartsWith(x.Name, StringComparison.OrdinalIgnoreCase));
                    var autoSetupMerge = Module.EffectiveConfigFile.GetValue("branch.autoSetupMerge");
                    if (autoSetupMerge.IsNotNullOrWhitespace() && autoSetupMerge.ToLowerInvariant() == "false")
                    {
                        track = false;
                    }

                    if (track && !AppSettings.DontConfirmAddTrackingRef)
                    {
                        var result = MessageBox.Show(owner,
                                                     string.Format(_updateTrackingReference.Text, selectedLocalBranch.Name, RemoteBranch.Text),
                                                     _pushCaption.Text,
                                                     MessageBoxButtons.YesNoCancel);
                        if (result == DialogResult.Cancel)
                        {
                            return false;
                        }

                        track = result == DialogResult.Yes;
                    }
                }

                if (ForcePushBranches.Checked)
                {
                    if (GitVersion.Current.SupportPushForceWithLease)
                    {
                        var choice = MessageBox.Show(owner,
                                                     _useForceWithLeaseInstead.Text,
                                                     "", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question,
                                                     MessageBoxDefaultButton.Button1);
                        switch (choice)
                        {
                            case DialogResult.Yes:
                                ForcePushBranches.Checked = false;
                                ckForceWithLease.Checked = true;
                                break;
                            case DialogResult.Cancel:
                                return false;
                        }
                    }
                }

                if (_NO_TRANSLATE_Branch.Text == AllRefs)
                {
                    pushCmd = Module.PushAllCmd(destination, GetForcePushOption(), track, RecursiveSubmodules.SelectedIndex);
                }
                else
                {
                    pushCmd = Module.PushCmd(destination, _NO_TRANSLATE_Branch.Text, RemoteBranch.Text,
                        GetForcePushOption(), track, RecursiveSubmodules.SelectedIndex);
                }
            }
            else if (TabControlTagBranch.SelectedTab == TagTab)
            {
                string tag = TagComboBox.Text;
                bool pushAllTags = false;
                if (tag == AllRefs)
                {
                    tag = "";
                    pushAllTags = true;
                }

                pushCmd = GitCommandHelpers.PushTagCmd(destination, tag, pushAllTags, GetForcePushOption());
            }
            else
            {
                // Push Multiple Branches Tab selected
                var pushActions = new List<GitPushAction>();
                foreach (DataRow row in _branchTable.Rows)
                {
                    var push = Convert.ToBoolean(row[PushColumnName]);
                    var force = Convert.ToBoolean(row[ForceColumnName]);
                    var delete = Convert.ToBoolean(row[DeleteColumnName]);

                    if (push || force)
                    {
                        pushActions.Add(new GitPushAction(row[LocalColumnName].ToString(), row[RemoteColumnName].ToString(), force));
                    }
                    else if (delete)
                    {
                        pushActions.Add(GitPushAction.DeleteRemoteBranch(row[RemoteColumnName].ToString()));
                    }
                }

                pushCmd = GitCommandHelpers.PushMultipleCmd(destination, pushActions);
            }

            ScriptManager.RunEventScripts(this, ScriptEvent.BeforePush);

            // controls can be accessed only from UI thread
            _selectedBranch = _NO_TRANSLATE_Branch.Text;
            _candidateForRebasingMergeCommit = PushToRemote.Checked && (_selectedBranch != AllRefs) && TabControlTagBranch.SelectedTab == BranchTab;
            _selectedRemoteBranchName = RemoteBranch.Text;

            using (var form = new FormRemoteProcess(Module, pushCmd)
            {
                Remote = remote,
                Text = string.Format(_pushToCaption.Text, destination),
                HandleOnExitCallback = HandlePushOnExit
            })
            {
                form.ShowDialog(owner);
                ErrorOccurred = form.ErrorOccurred();

                if (!Module.InTheMiddleOfAction() && !form.ErrorOccurred())
                {
                    ScriptManager.RunEventScripts(this, ScriptEvent.AfterPush);
                    if (_createPullRequestCB.Checked)
                    {
                        UICommands.StartCreatePullRequest(owner);
                    }

                    return true;
                }
            }

            return false;
        }

        private ForcePushOptions GetForcePushOption()
        {
            if (ForcePushBranches.Checked)
            {
                return ForcePushOptions.Force;
            }

            if (ckForceWithLease.Checked)
            {
                return ForcePushOptions.ForceWithLease;
            }

            return ForcePushOptions.DoNotForce;
        }

        private bool IsRebasingMergeCommit()
        {
            if (AppSettings.DefaultPullAction == AppSettings.PullAction.Rebase &&
                _candidateForRebasingMergeCommit &&
                _selectedBranch == _currentBranchName &&
                _selectedRemote == _currentBranchRemote)
            {
                string remoteBranchName = _selectedRemote + "/" + _selectedRemoteBranchName;
                return Module.ExistsMergeCommit(remoteBranchName, _selectedBranch);
            }

            return false;
        }

        private bool HandlePushOnExit(ref bool isError, FormProcess form)
        {
            if (!isError)
            {
                return false;
            }

            // there is no way to pull to not current branch
            if (_selectedBranch != _currentBranchName)
            {
                return false;
            }

            // auto pull from URL not supported. See https://github.com/gitextensions/gitextensions/issues/1887
            if (!PushToRemote.Checked)
            {
                return false;
            }

            // auto pull only if current branch was rejected
            var isRejected = new Regex(Regex.Escape("! [rejected] ") + ".*" + Regex.Escape(_currentBranchName) + ".*", RegexOptions.Compiled);
            if (isRejected.IsMatch(form.GetOutputString()) && !Module.IsBareRepository())
            {
                IWin32Window owner = form.Owner;
                (var onRejectedPullAction, var forcePush) = AskForAutoPullOnPushRejectedAction(owner);

                if (forcePush)
                {
                    if (!form.ProcessArguments.Contains(" -f ") && !form.ProcessArguments.Contains(" --force"))
                    {
                        Trace.Assert(form.ProcessArguments.StartsWith("push "), "Arguments should start with 'push' command");

                        string forceArg = GitVersion.Current.SupportPushForceWithLease ? " --force-with-lease" : " -f";
                        form.ProcessArguments = form.ProcessArguments.Insert("push".Length, forceArg);
                    }

                    form.Retry();
                    return true;
                }

                if (onRejectedPullAction == AppSettings.PullAction.Default)
                {
                    onRejectedPullAction = AppSettings.DefaultPullAction;
                }

                if (onRejectedPullAction == AppSettings.PullAction.None)
                {
                    return false;
                }

                if (onRejectedPullAction != AppSettings.PullAction.Merge && onRejectedPullAction != AppSettings.PullAction.Rebase)
                {
                    form.AppendOutput(Environment.NewLine +
                        "Automatical pull can only be performed, when the default pull action is either set to Merge or Rebase." +
                        Environment.NewLine + Environment.NewLine);
                    return false;
                }

                if (IsRebasingMergeCommit())
                {
                    form.AppendOutput(Environment.NewLine +
                        "Can not perform automatical pull, when the pull action is set to Rebase " + Environment.NewLine +
                        "and one of the commits that are about to be rebased is a merge commit." +
                        Environment.NewLine + Environment.NewLine);
                    return false;
                }

                UICommands.StartPullDialogAndPullImmediately(
                    out var pullCompleted,
                    owner,
                    _selectedRemoteBranchName,
                    _selectedRemote.Name,
                    onRejectedPullAction);

                if (pullCompleted)
                {
                    form.Retry();
                    return true;
                }
            }

            return false;
        }

        private (AppSettings.PullAction pullAction, bool forcePush) AskForAutoPullOnPushRejectedAction(IWin32Window owner)
        {
            bool forcePush = false;
            AppSettings.PullAction? onRejectedPullAction = AppSettings.AutoPullOnPushRejectedAction;
            if (onRejectedPullAction == null)
            {
                string destination = _NO_TRANSLATE_Remotes.Text;
                string buttons = _pullRepositoryButtons.Text;
                switch (AppSettings.DefaultPullAction)
                {
                    case AppSettings.PullAction.Fetch:
                    case AppSettings.PullAction.FetchAll:
                    case AppSettings.PullAction.FetchPruneAll:
                        buttons = string.Format(buttons, _pullActionFetch.Text);
                        break;
                    case AppSettings.PullAction.Merge:
                        buttons = string.Format(buttons, _pullActionMerge.Text);
                        break;
                    case AppSettings.PullAction.Rebase:
                        buttons = string.Format(buttons, _pullActionRebase.Text);
                        break;
                    default:
                        buttons = string.Format(buttons, _pullActionNone.Text);
                        break;
                }

                int idx = PSTaskDialog.cTaskDialog.ShowCommandBox(owner,
                                string.Format(_pullRepositoryCaption.Text, destination),
                                _pullRepositoryMainInstruction.Text,
                                _pullRepository.Text,
                                "",
                                "",
                                _dontShowAgain.Text,
                                buttons,
                                true,
                                0,
                                0);
                bool rememberDecision = PSTaskDialog.cTaskDialog.VerificationChecked;
                switch (idx)
                {
                    case 0:
                        onRejectedPullAction = AppSettings.PullAction.Default;
                        break;
                    case 1:
                        onRejectedPullAction = AppSettings.PullAction.Rebase;
                        break;
                    case 2:
                        onRejectedPullAction = AppSettings.PullAction.Merge;
                        break;
                    case 3:
                        forcePush = true;
                        break;
                    default:
                        onRejectedPullAction = AppSettings.PullAction.None;
                        break;
                }

                if (rememberDecision)
                {
                    AppSettings.AutoPullOnPushRejectedAction = onRejectedPullAction;
                }
            }

            return (onRejectedPullAction ?? AppSettings.PullAction.None, forcePush);
        }

        private void UpdateBranchDropDown()
        {
            var curBranch = _NO_TRANSLATE_Branch.Text;

            _NO_TRANSLATE_Branch.DisplayMember = nameof(IGitRef.Name);
            _NO_TRANSLATE_Branch.Items.Clear();
            _NO_TRANSLATE_Branch.Items.Add(AllRefs);
            _NO_TRANSLATE_Branch.Items.Add(HeadText);

            if (string.IsNullOrEmpty(curBranch))
            {
                curBranch = _currentBranchName;
                if (curBranch.IndexOfAny("() ".ToCharArray()) != -1)
                {
                    curBranch = HeadText;
                }
            }

            foreach (var head in GetLocalBranches())
            {
                _NO_TRANSLATE_Branch.Items.Add(head);
            }

            _NO_TRANSLATE_Branch.Text = curBranch;

            _NO_TRANSLATE_Branch.ResizeDropDownWidth(AppSettings.BranchDropDownMinWidth, AppSettings.BranchDropDownMaxWidth);
        }

        private IEnumerable<IGitRef> GetLocalBranches()
        {
            return _gitRefs.Where(r => r.IsHead);
        }

        private IEnumerable<IGitRef> GetRemoteBranches(string remoteName)
        {
            return _gitRefs.Where(r => r.IsRemote && r.Remote == remoteName);
        }

        private void PullClick(object sender, EventArgs e)
        {
            UICommands.StartPullDialog(this);
        }

        private void UpdateRemoteBranchDropDown()
        {
            RemoteBranch.Items.Clear();

            if (!string.IsNullOrEmpty(_NO_TRANSLATE_Branch.Text))
            {
                RemoteBranch.Items.Add(_NO_TRANSLATE_Branch.Text);
            }

            if (_selectedRemote != null)
            {
                foreach (var head in GetRemoteBranches(_selectedRemote.Name))
                {
                    if (_NO_TRANSLATE_Branch.Text != head.LocalName)
                    {
                        RemoteBranch.Items.Add(head.LocalName);
                    }
                }

                var remoteBranchesSet = GetRemoteBranches(_selectedRemote.Name).ToHashSet(b => b.LocalName);
                var onlyLocalBranches = GetLocalBranches().Where(b => !remoteBranchesSet.Contains(b.LocalName));

                foreach (var head in onlyLocalBranches)
                {
                    if (_NO_TRANSLATE_Branch.Text != head.LocalName)
                    {
                        RemoteBranch.Items.Add(head.LocalName);
                    }
                }
            }

            RemoteBranch.ResizeDropDownWidth(AppSettings.BranchDropDownMinWidth, AppSettings.BranchDropDownMaxWidth);
        }

        private void BranchSelectedValueChanged(object sender, EventArgs e)
        {
            if (_NO_TRANSLATE_Branch.Text == AllRefs)
            {
                RemoteBranch.Text = "";
                return;
            }

            if (_NO_TRANSLATE_Branch.Text != HeadText)
            {
                if (PushToRemote.Checked)
                {
                    if (_NO_TRANSLATE_Branch.SelectedItem is GitRef branch)
                    {
                        if (_selectedRemote != null)
                        {
                            string defaultRemote = _remoteManager.GetDefaultPushRemote(_selectedRemote,
                                branch.Name);
                            if (!defaultRemote.IsNullOrEmpty())
                            {
                                RemoteBranch.Text = defaultRemote;
                                return;
                            }

                            if (branch.TrackingRemote.Equals(_selectedRemote.Name, StringComparison.OrdinalIgnoreCase))
                            {
                                RemoteBranch.Text = branch.MergeWith;
                                if (!string.IsNullOrEmpty(RemoteBranch.Text))
                                {
                                    return;
                                }
                            }
                        }
                    }
                }

                RemoteBranch.Text = _NO_TRANSLATE_Branch.Text;
            }
        }

        private void FormPushLoad(object sender, EventArgs e)
        {
            _NO_TRANSLATE_Remotes.Select();

            Text = string.Concat(_pushCaption.Text, " (", Module.WorkingDir, ")");

            var gitHoster = PluginRegistry.TryGetGitHosterForModule(Module);
            _createPullRequestCB.Enabled = gitHoster != null;
        }

        private void AddRemoteClick(object sender, EventArgs e)
        {
            OpenRemotesDialogAndRefreshList(_selectedRemote?.Name);
        }

        private void PushToUrlCheckedChanged(object sender, EventArgs e)
        {
            PushDestination.Enabled = PushToUrl.Checked;
            folderBrowserButton1.Enabled = PushToUrl.Checked;
            _NO_TRANSLATE_Remotes.Enabled = PushToRemote.Checked;
            AddRemote.Enabled = PushToRemote.Checked;

            if (PushToUrl.Checked)
            {
                ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    var repositoryHistory = await RepositoryHistoryManager.Remotes.LoadRecentHistoryAsync();

                    await this.SwitchToMainThreadAsync();
                    string prevUrl = PushDestination.Text;
                    PushDestination.DataSource = repositoryHistory;
                    PushDestination.DisplayMember = nameof(Repository.Path);
                    PushDestination.Text = prevUrl;

                    BranchSelectedValueChanged(null, null);
                });
            }
            else
            {
                RemotesUpdated(sender, e);
            }
        }

        private void RemotesUpdated(object sender, EventArgs e)
        {
            _selectedRemote = _NO_TRANSLATE_Remotes.SelectedItem as GitRemote;
            if (_selectedRemote == null)
            {
                return;
            }

            if (TabControlTagBranch.SelectedTab == MultipleBranchTab)
            {
                UpdateMultiBranchView();
            }

            EnableLoadSshButton();

            // update the text box of the Remote Url combobox to show the URL of selected remote
            string pushUrl = _selectedRemote.PushUrl;
            if (pushUrl.IsNullOrEmpty())
            {
                pushUrl = _selectedRemote.Url;
            }

            PushDestination.Text = pushUrl;

            if (string.IsNullOrEmpty(_NO_TRANSLATE_Branch.Text))
            {
                // Doing this makes it pretty easy to accidentally create a branch on the remote.
                // But leaving it blank will do the 'default' thing, meaning all branches are pushed.
                // Solution: when pushing a branch that doesn't exist on the remote, ask what to do
                var currentBranch = new GitRef(Module, null, _currentBranchName, _selectedRemote.Name);
                _NO_TRANSLATE_Branch.Items.Add(currentBranch);
                _NO_TRANSLATE_Branch.SelectedItem = currentBranch;
            }

            BranchSelectedValueChanged(null, null);
        }

        private void EnableLoadSshButton()
        {
            LoadSSHKey.Visible = _selectedRemote?.PuttySshKey.IsNotNullOrWhitespace() ?? false;
        }

        private void LoadSshKeyClick(object sender, EventArgs e)
        {
            StartPageant(_selectedRemote.Name);
        }

        private void StartPageant(string remote)
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

        private void EnsurePageant(string remote)
        {
            if (GitCommandHelpers.Plink())
            {
                StartPageant(remote);
            }
        }

        private void RemotesValidated(object sender, EventArgs e)
        {
            EnableLoadSshButton();
        }

        private void FillTagDropDown()
        {
            // var tags = Module.GetTagHeads(GitModule.GetTagHeadsOption.OrderByCommitDateDescending); // comment out to sort by commit date
            var tags = Module.GetTagRefs(GitModule.GetTagRefsSortOrder.ByName)
                .Select(tag => tag.Name).ToList();
            tags.Insert(0, AllRefs);
            TagComboBox.DataSource = tags;

            TagComboBox.ResizeDropDownWidth(AppSettings.BranchDropDownMinWidth, AppSettings.BranchDropDownMaxWidth);
        }

        private void ForcePushBranchesCheckedChanged(object sender, EventArgs e)
        {
            if (ForcePushBranches.Checked)
            {
                ckForceWithLease.Checked = false;
            }
        }

        #region Multi-Branch Methods

        private DataTable _branchTable;

        private void UpdateMultiBranchView()
        {
            _branchTable = new DataTable();
            _branchTable.Columns.Add(LocalColumnName, typeof(string));
            _branchTable.Columns.Add(RemoteColumnName, typeof(string));
            _branchTable.Columns.Add(NewColumnName, typeof(string));
            _branchTable.Columns.Add(PushColumnName, typeof(bool));
            _branchTable.Columns.Add(ForceColumnName, typeof(bool));
            _branchTable.Columns.Add(DeleteColumnName, typeof(bool));
            _branchTable.ColumnChanged += BranchTable_ColumnChanged;
            LocalColumn.DataPropertyName = LocalColumnName;
            RemoteColumn.DataPropertyName = RemoteColumnName;
            NewColumn.DataPropertyName = NewColumnName;
            PushColumn.DataPropertyName = PushColumnName;
            ForceColumn.DataPropertyName = ForceColumnName;
            DeleteColumn.DataPropertyName = DeleteColumnName;
            BranchGrid.DataSource = new BindingSource { DataSource = _branchTable };

            if (_selectedRemote == null)
            {
                return;
            }

            LoadMultiBranchViewData(_selectedRemote.Name);
        }

        private void LoadMultiBranchViewData(string remote)
        {
            using (WaitCursorScope.Enter(Cursors.AppStarting))
            {
                IReadOnlyList<IGitRef> remoteHeads;
                if (Module.EffectiveSettings.Detailed.GetRemoteBranchesDirectlyFromRemote.ValueOrDefault)
                {
                    EnsurePageant(remote);

                    var formProcess = new FormRemoteProcess(Module, $"ls-remote --heads \"{remote}\"")
                    {
                        Remote = remote
                    };

                    using (formProcess)
                    {
                        formProcess.ShowDialog(this);

                        if (formProcess.ErrorOccurred())
                        {
                            return;
                        }

                        var refList = CleanCommandOutput(formProcess.GetOutputString());

                        remoteHeads = Module.ParseRefs(refList);
                    }
                }
                else
                {
                    // use remote branches from git's local database if there were problems with receiving branches from the remote server
                    remoteHeads = Module.GetRemoteBranches().Where(r => r.Remote == remote).ToList();
                }

                ProcessHeads(remoteHeads);
            }

            return;

            string CleanCommandOutput(string processOutput)
            {
                // Command output consists of lines of format:
                //
                //     <SHA1> \t <full-ref>
                //
                // Such as:
                //
                //     fa77791d780a01a06d1f7d4ccad4ef93ed0ae2fd\trefs/heads/branchName

                int firstTabIdx = processOutput.IndexOf('\t');

                return firstTabIdx == 40
                    ? processOutput
                    : firstTabIdx > 40
                        ? processOutput.Substring(firstTabIdx - 40)
                        : string.Empty;
            }

            void ProcessHeads(IReadOnlyList<IGitRef> remoteHeads)
            {
                var localHeads = GetLocalBranches().ToList();
                var remoteBranches = remoteHeads.ToHashSet(h => h.LocalName);

                // Add all the local branches.
                foreach (var head in localHeads)
                {
                    var remoteName = head.Remote == remote
                        ? head.MergeWith ?? head.Name
                        : head.Name;
                    var isKnownAtRemote = remoteBranches.Contains(remoteName);

                    var row = _branchTable.NewRow();

                    row[ForceColumnName] = false;
                    row[DeleteColumnName] = false;
                    row[LocalColumnName] = head.Name;
                    row[RemoteColumnName] = remoteName;
                    row[NewColumnName] = isKnownAtRemote ? _no.Text : _yes.Text;
                    row[PushColumnName] = isKnownAtRemote;

                    _branchTable.Rows.Add(row);
                }

                // Offer to delete all the left over remote branches.
                foreach (var remoteHead in remoteHeads)
                {
                    if (localHeads.All(h => h.Name != remoteHead.LocalName))
                    {
                        var row = _branchTable.NewRow();

                        row[LocalColumnName] = null;
                        row[RemoteColumnName] = remoteHead.LocalName;
                        row[NewColumnName] = _no.Text;
                        row[PushColumnName] = false;
                        row[ForceColumnName] = false;
                        row[DeleteColumnName] = false;

                        _branchTable.Rows.Add(row);
                    }
                }

                BranchGrid.Enabled = true;
            }
        }

        private static void BranchTable_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            switch (e.Column.ColumnName)
            {
                case PushColumnName:
                    {
                        if ((bool)e.ProposedValue)
                        {
                            e.Row[ForceColumnName] = false;
                            e.Row[DeleteColumnName] = false;
                        }

                        break;
                    }

                case ForceColumnName:
                    {
                        if ((bool)e.ProposedValue)
                        {
                            e.Row[PushColumnName] = false;
                            e.Row[DeleteColumnName] = false;
                        }

                        break;
                    }

                case DeleteColumnName:
                    {
                        if ((bool)e.ProposedValue)
                        {
                            e.Row[PushColumnName] = false;
                            e.Row[ForceColumnName] = false;
                        }

                        break;
                    }
            }
        }

        private void TabControlTagBranch_Selected(object sender, TabControlEventArgs e)
        {
            if (TabControlTagBranch.SelectedTab == MultipleBranchTab)
            {
                UpdateMultiBranchView();
            }
            else if (TabControlTagBranch.SelectedTab == TagTab)
            {
                FillTagDropDown();
            }
            else
            {
                UpdateBranchDropDown();
                UpdateRemoteBranchDropDown();
            }
        }

        private void BranchGrid_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            // Push grid checkbox changes immediately into the underlying data table.
            if (BranchGrid.CurrentCell is DataGridViewCheckBoxCell)
            {
                BranchGrid.EndEdit();
                ((BindingSource)BranchGrid.DataSource).EndEdit();
            }
        }

        #endregion

        private void ShowOptions_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            PushOptionsPanel.Visible = true;
            ShowOptions.Visible = false;
            SetFormSizeToFitAllItems();
        }

        private void SetFormSizeToFitAllItems()
        {
            if (Height < MinimumSize.Height + 50)
            {
                Height = MinimumSize.Height + 50;
            }
        }

        private void _NO_TRANSLATE_Branch_SelectedIndexChanged(object sender, EventArgs e)
        {
            RemoteBranch.Enabled = _NO_TRANSLATE_Branch.Text != AllRefs;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
            }

            base.Dispose(disposing);
        }

        private void ForceWithLeaseCheckedChanged(object sender, EventArgs e)
        {
            if (ckForceWithLease.Checked)
            {
                ForcePushBranches.Checked = false;
            }
        }
    }
}
