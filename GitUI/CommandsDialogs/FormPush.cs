using System.Data;
using System.Text.RegularExpressions;
using GitCommands;
using GitCommands.Config;
using GitCommands.Git;
using GitCommands.Remotes;
using GitCommands.Settings;
using GitCommands.UserRepositoryHistory;
using GitExtUtils;
using GitExtUtils.GitUI;
using GitUI.HelperDialogs;
using GitUI.Infrastructure;
using GitUI.ScriptsEngine;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.Settings;
using Microsoft;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class FormPush : GitModuleForm
    {
        private const string HeadText = "HEAD";
        private const string AllRefs = "[ All ]";
        private const string LocalColumnName = "Local";
        private const string RemoteColumnName = "Remote";
        private const string AheadColumnName = "New";
        private const string PushColumnName = "Push";
        private const string ForceColumnName = "Force";
        private const string DeleteColumnName = "Delete";

        private string? _currentBranchName;
        private ConfigFileRemote? _currentBranchRemote;
        private bool _candidateForRebasingMergeCommit;
        private string? _selectedBranch;
        private ConfigFileRemote? _selectedRemote;
        private string? _selectedRemoteBranchName;
        private IReadOnlyList<IGitRef>? _gitRefs;
        private readonly IConfigFileRemoteSettingsManager _remotesManager;

        public bool ErrorOccurred { get; private set; }
        private int _pushColumnIndex;

        #region Translation
        private readonly TranslationString _branchNewForRemote =
            new("The branch you are about to push seems to be a new branch for the remote." +
                                  Environment.NewLine + "Are you sure you want to push this branch?");
        private readonly TranslationString _noCurrentBranch =
            new("No branch is selected, cannot push.");

        private readonly TranslationString _pushCaption = new("Push");

        private readonly TranslationString _pushToCaption = new("Push to {0}");

        private readonly TranslationString _selectDestinationDirectory =
            new("Please select a destination directory");

        private readonly TranslationString _errorPushToRemoteCaption = new("Push to remote");
        private readonly TranslationString _configureRemote = new($"Please configure a remote repository first.{Environment.NewLine}Would you like to do it now?");

        private readonly TranslationString _selectTag =
            new("You need to select a tag to push or select \"Push all tags\".");

        private readonly TranslationString _updateTrackingReference =
            new("The branch {0} does not have a tracking reference. Do you want to add a tracking reference to {1}?");

        private readonly TranslationString _pullRepositoryMainMergeInstruction = new("Pull latest changes from remote repository");
        private readonly TranslationString _pullRepositoryMainForceInstruction = new("Push rejected");
        private readonly TranslationString _pullRepositoryMergeInstruction =
            new("The push was rejected because the tip of your current branch is behind its remote counterpart. " +
                "Merge the remote changes before pushing again.");
        private readonly TranslationString _pullRepositoryForceInstruction =
            new("The push was rejected because the tip of your current branch is behind its remote counterpart");
        private readonly TranslationString _pullDefaultButton = new("&Pull with the default pull action ({0})");
        private readonly TranslationString _pullRebaseButton = new("Pull with &rebase");
        private readonly TranslationString _pullMergeButton = new("Pull with &merge");
        private readonly TranslationString _pushForceButton = new("&Force push with lease");
        private readonly TranslationString _pullActionNone = new("none");
        private readonly TranslationString _pullActionFetch = new("fetch");
        private readonly TranslationString _pullActionRebase = new("rebase");
        private readonly TranslationString _pullActionMerge = new("merge");
        private readonly TranslationString _pullRepositoryCaption = new("Push was rejected from \"{0}\"");
        private readonly TranslationString _useForceWithLeaseInstead =
            new("Force push may overwrite changes since your last fetch. Do you want to use the safer force with lease instead?");
        private readonly TranslationString _forceWithLeaseTooltips =
            new("Force with lease is a safer way to force push. It ensures you only overwrite work that you have seen in your local repository");

        #endregion

        public FormPush(GitUICommands commands)
            : base(commands)
        {
            InitializeComponent();

            Push.Text = TranslatedStrings.ButtonPush;

            NewColumn.Width = DpiUtil.Scale(97);
            PushColumn.Width = DpiUtil.Scale(36);
            ForceColumn.Width = DpiUtil.Scale(101);
            DeleteColumn.Width = DpiUtil.Scale(108);

            InitializeComplete();

            ForcePushTags.DataBindings.Add("Checked", ckForceWithLease, "Checked",
                formattingEnabled: false, updateMode: DataSourceUpdateMode.OnPropertyChanged);
            toolTip1.SetToolTip(ckForceWithLease, _forceWithLeaseTooltips.Text);

            // can't be set in OnLoad, because after PushAndShowDialogWhenFailed()
            // they are reset to false
            _remotesManager = new ConfigFileRemoteSettingsManager(() => Module);
            Init();

            void Init()
            {
                _gitRefs = Module.GetRefs(RefsFilter.Heads | RefsFilter.Remotes);
                RecursiveSubmodules.SelectedIndex = AppSettings.RecursiveSubmodules;

                _currentBranchName = Module.GetSelectedBranch();

                // refresh registered git remotes
                UserGitRemotes = _remotesManager.LoadRemotes(false).ToList();
                BindRemotesDropDown(null);

                UpdateBranchDropDown();
                UpdateRemoteBranchDropDown();

                Push.Focus();

                if (AppSettings.AlwaysShowAdvOpt)
                {
                    ShowOptions_LinkClicked(this, null!);
                }

                // Save the value because later the value for all the columns will be at '0'
                _pushColumnIndex = PushColumn.Index;

                PushColumn.HeaderCell.ContextMenuStrip = menuPushSelection;

                // Handle left button click to also open the context menu
                BranchGrid.ColumnHeaderMouseClick += BranchGrid_ColumnHeaderMouseClick;
            }
        }

        /// <summary>
        /// Gets the list of remotes configured in .git/config file.
        /// </summary>
        private List<ConfigFileRemote>? UserGitRemotes { get; set; }

        public DialogResult PushAndShowDialogWhenFailed(IWin32Window? owner = null)
        {
            if (!PushChanges(owner))
            {
                return ShowDialog(owner);
            }

            return DialogResult.OK;
        }

        private bool CheckIfRemoteExist()
        {
            Validates.NotNull(UserGitRemotes);

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

        private void OpenRemotesDialogAndRefreshList(string? selectedRemoteName)
        {
            if (!UICommands.StartRemotesDialog(this, selectedRemoteName))
            {
                return;
            }

            UserGitRemotes = _remotesManager.LoadRemotes(false).ToList();
            BindRemotesDropDown(selectedRemoteName);
        }

        private void PushClick(object sender, EventArgs e)
        {
            DialogResult = PushChanges(this) ? DialogResult.OK : DialogResult.None;
        }

        private void BindRemotesDropDown(string? selectedRemoteName)
        {
            _NO_TRANSLATE_Remotes.SelectedIndexChanged -= RemotesUpdated;
            _NO_TRANSLATE_Remotes.TextUpdate -= RemotesUpdated;
            _NO_TRANSLATE_Remotes.Sorted = false;
            _NO_TRANSLATE_Remotes.DataSource = UserGitRemotes;
            _NO_TRANSLATE_Remotes.DisplayMember = nameof(ConfigFileRemote.Name);
            _NO_TRANSLATE_Remotes.SelectedIndex = -1;

            _NO_TRANSLATE_Remotes.SelectedIndexChanged += RemotesUpdated;
            _NO_TRANSLATE_Remotes.TextUpdate += RemotesUpdated;

            if (string.IsNullOrEmpty(selectedRemoteName))
            {
                selectedRemoteName = Module.GetSetting(string.Format(SettingKeyString.BranchRemote, _currentBranchName));
            }

            _currentBranchRemote = UserGitRemotes.FirstOrDefault(x => StringComparer.OrdinalIgnoreCase.Equals(x.Name, selectedRemoteName));
            if (_currentBranchRemote is not null)
            {
                _NO_TRANSLATE_Remotes.SelectedItem = _currentBranchRemote;
            }
            else if (UserGitRemotes.Any())
            {
                ConfigFileRemote defaultRemote = UserGitRemotes.FirstOrDefault(x => StringComparer.OrdinalIgnoreCase.Equals(x.Name, "origin"));

                // we couldn't find the default assigned remote for the selected branch
                // it is usually gets mapped via FormRemotes -> "default pull behavior" tab
                // so pick the default user remote
                if (defaultRemote is null)
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

        private bool IsBranchKnownToRemote(string? remote, string branch)
        {
            IEnumerable<IGitRef> remoteRefs = GetRemoteBranches(remote).Where(r => r.LocalName == branch);
            if (remoteRefs.Any())
            {
                return true;
            }

            IEnumerable<IGitRef> localRefs = _gitRefs.Where(r => r.IsHead && r.Name == branch && r.TrackingRemote == remote);
            return localRefs.Any();
        }

        private bool PushChanges(IWin32Window? owner)
        {
            ErrorOccurred = false;
            if (PushToUrl.Checked && !Uri.IsWellFormedUriString(PushDestination.Text, UriKind.Absolute))
            {
                MessageBox.Show(owner, _selectDestinationDirectory.Text, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (/* PushToRemote.Checked */!CheckIfRemoteExist())
            {
                return false;
            }

            Validates.NotNull(_selectedRemote);

            string selectedRemoteName = _selectedRemote.Name;
            if (TabControlTagBranch.SelectedTab == TagTab && string.IsNullOrEmpty(TagComboBox.Text))
            {
                MessageBox.Show(owner, _selectTag.Text, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (TabControlTagBranch.SelectedTab == BranchTab && _NO_TRANSLATE_Branch.Text != AllRefs
                && (string.IsNullOrWhiteSpace(_NO_TRANSLATE_Branch.Text)
                    || _NO_TRANSLATE_Branch.Text == DetachedHeadParser.DetachedBranch
                    || string.IsNullOrWhiteSpace(RemoteBranch.Text)
                    || RemoteBranch.Text == DetachedHeadParser.DetachedBranch))
            {
                MessageBox.Show(owner, _noCurrentBranch.Text, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    RemoteBranch.Text != _remotesManager.GetDefaultPushRemote(_selectedRemote, _NO_TRANSLATE_Branch.Text) &&
                    !IsBranchKnownToRemote(selectedRemoteName, RemoteBranch.Text))
                {
                    // Ask if this is really what the user wants
                    if (!AppSettings.DontConfirmPushNewBranch &&
                        MessageBox.Show(owner, _branchNewForRemote.Text, _pushCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    {
                        return false;
                    }
                }
            }

            if (PushToUrl.Checked)
            {
                string path = PushDestination.Text;
                ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Remotes.AddAsMostRecentAsync(path));
            }

            AppSettings.RecursiveSubmodules = RecursiveSubmodules.SelectedIndex;

            string remote = "";
            string destination;
            if (PushToUrl.Checked)
            {
                destination = PushDestination.Text;
            }
            else
            {
                Validates.NotNull(selectedRemoteName);
                StartPageant(selectedRemoteName);

                destination = selectedRemoteName;
                remote = selectedRemoteName.Trim();
            }

            string pushCmd;
            if (TabControlTagBranch.SelectedTab == BranchTab)
            {
                bool track = ReplaceTrackingReference.Checked;
                if (!track && !string.IsNullOrWhiteSpace(RemoteBranch.Text))
                {
                    GitRef? selectedLocalBranch = _NO_TRANSLATE_Branch.SelectedItem as GitRef;
                    track = selectedLocalBranch is not null && string.IsNullOrEmpty(selectedLocalBranch.TrackingRemote) &&
                            !UserGitRemotes.Any(x => _NO_TRANSLATE_Branch.Text.StartsWith(x.Name, StringComparison.OrdinalIgnoreCase));
                    string autoSetupMerge = Module.EffectiveConfigFile.GetValue("branch.autoSetupMerge");
                    if (!string.IsNullOrWhiteSpace(autoSetupMerge) && autoSetupMerge.ToLowerInvariant() == "false")
                    {
                        track = false;
                    }

                    if (track && !AppSettings.DontConfirmAddTrackingRef)
                    {
                        Validates.NotNull(selectedLocalBranch);
                        DialogResult result = MessageBox.Show(owner,
                                                     string.Format(_updateTrackingReference.Text, selectedLocalBranch.Name, RemoteBranch.Text),
                                                     _pushCaption.Text,
                                                     MessageBoxButtons.YesNoCancel,
                                                     MessageBoxIcon.Question,
                                                     MessageBoxDefaultButton.Button1);
                        if (result == DialogResult.Cancel)
                        {
                            return false;
                        }

                        track = result == DialogResult.Yes;
                    }
                }

                if (ForcePushBranches.Checked)
                {
                    DialogResult choice = MessageBox.Show(owner,
                                                 _useForceWithLeaseInstead.Text,
                                                 "Question", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question,
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

                if (_NO_TRANSLATE_Branch.Text == AllRefs)
                {
                    pushCmd = Commands.PushAll(destination,
                                               GetForcePushOption(),
                                               track,
                                               RecursiveSubmodules.SelectedIndex);
                }
                else
                {
                    pushCmd = Commands.Push(destination,
                                            Module.FormatBranchName(_NO_TRANSLATE_Branch.Text),
                                            RemoteBranch.Text,
                                            GetForcePushOption(),
                                            track,
                                            RecursiveSubmodules.SelectedIndex);
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

                pushCmd = Commands.PushTag(destination, tag, pushAllTags, GetForcePushOption());
            }
            else
            {
                // Push Multiple Branches Tab selected
                List<GitPushAction> pushActions = [];
                Validates.NotNull(_branchTable);
                foreach (DataRow row in _branchTable.Rows)
                {
                    bool push = Convert.ToBoolean(row[PushColumnName]);
                    bool force = Convert.ToBoolean(row[ForceColumnName]);
                    bool delete = Convert.ToBoolean(row[DeleteColumnName]);
                    string localBranch = row[LocalColumnName].ToString();
                    string remoteBranch = row[RemoteColumnName].ToString();
                    if (string.IsNullOrWhiteSpace(remoteBranch))
                    {
                        remoteBranch = localBranch;
                    }

                    if (string.IsNullOrWhiteSpace(remoteBranch))
                    {
                        continue;
                    }

                    if (push || force)
                    {
                        pushActions.Add(new GitPushAction(localBranch, remoteBranch, force));
                    }
                    else if (delete)
                    {
                        pushActions.Add(GitPushAction.DeleteRemoteBranch(remoteBranch));
                    }
                }

                pushCmd = Commands.PushMultiple(destination, pushActions);
            }

            bool success = ScriptsRunner.RunEventScripts(ScriptEvent.BeforePush, this);
            if (!success)
            {
                return false;
            }

            // controls can be accessed only from UI thread
            _selectedBranch = _NO_TRANSLATE_Branch.Text;
            _candidateForRebasingMergeCommit = PushToRemote.Checked && (_selectedBranch != AllRefs) && TabControlTagBranch.SelectedTab == BranchTab;
            _selectedRemoteBranchName = RemoteBranch.Text;

            using FormRemoteProcess form = new(UICommands, pushCmd)
            {
                Remote = remote,
                Text = string.Format(_pushToCaption.Text, destination),
                HandleOnExitCallback = HandlePushOnExit
            };

            form.ShowDialog(owner);
            ErrorOccurred = form.ErrorOccurred();

            if (!Module.InTheMiddleOfAction() && !form.ErrorOccurred())
            {
                ScriptsRunner.RunEventScripts(ScriptEvent.AfterPush, this);
                if (_createPullRequestCB.Checked)
                {
                    UICommands.StartCreatePullRequest(owner);
                }

                return true;
            }

            return false;
        }

        private ForcePushOptions GetForcePushOption()
        {
            if (ForcePushBranches.Checked || ForcePushTags.Checked /* tags cannot be pushed using --force-with-lease */)
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

            // if push was rejected, offer force push and for current branch also pull/merge
            // Note that the Git output contains color codes etc too
            Regex isRejected = new($"! \\[rejected\\] .* ((?<currBranch>{Regex.Escape(_currentBranchName)})|.*) -> ");
            Match match = isRejected.Match(form.GetOutputString());
            if (match.Success && !Module.IsBareRepository())
            {
                DebugHelpers.Assert(form.Visible, "The progress dialog must be visible.");

                (AppSettings.PullAction onRejectedPullAction, bool forcePush) = AskForAutoPullOnPushRejectedAction(form, match.Groups["currBranch"].Success);

                if (forcePush)
                {
                    if (!form.ProcessArguments.Contains(" -f ") && !form.ProcessArguments.Contains(" --force"))
                    {
                        // Note that WSL may add other arguments prior to the actual command so "push" may not be first.
                        int pos = form.ProcessArguments.IndexOf("push ");
                        DebugHelpers.Assert(pos >= 0, "Arguments should start with 'push' command");

                        form.ProcessArguments = form.ProcessArguments.Insert(pos + "push ".Length, "--force-with-lease ");
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

                if (onRejectedPullAction is not (AppSettings.PullAction.Merge or AppSettings.PullAction.Rebase))
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

                Validates.NotNull(_selectedRemote);

                DebugHelpers.Assert(form.Visible, "The progress dialog must be visible.");
                UICommands.StartPullDialogAndPullImmediately(
                    out bool pullCompleted,
                    form,
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

        private (AppSettings.PullAction pullAction, bool forcePush) AskForAutoPullOnPushRejectedAction(IWin32Window owner, bool allOptions)
        {
            bool forcePush = false;
            AppSettings.PullAction? onRejectedPullAction = AppSettings.AutoPullOnPushRejectedAction;
            if (onRejectedPullAction is null)
            {
                string destination = _NO_TRANSLATE_Remotes.Text;
                string pullDefaultButtonText;
                switch (AppSettings.DefaultPullAction)
                {
                    case AppSettings.PullAction.Fetch:
                    case AppSettings.PullAction.FetchAll:
                    case AppSettings.PullAction.FetchPruneAll:
                        pullDefaultButtonText = string.Format(_pullDefaultButton.Text, _pullActionFetch.Text);
                        break;
                    case AppSettings.PullAction.Merge:
                        pullDefaultButtonText = string.Format(_pullDefaultButton.Text, _pullActionMerge.Text);
                        break;
                    case AppSettings.PullAction.Rebase:
                        pullDefaultButtonText = string.Format(_pullDefaultButton.Text, _pullActionRebase.Text);
                        break;
                    default:
                        pullDefaultButtonText = string.Format(_pullDefaultButton.Text, _pullActionNone.Text);
                        break;
                }

                TaskDialogPage page = new()
                {
                    Text = allOptions ? _pullRepositoryMergeInstruction.Text : _pullRepositoryForceInstruction.Text,
                    Heading = allOptions ? _pullRepositoryMainMergeInstruction.Text : _pullRepositoryMainForceInstruction.Text,
                    Caption = string.Format(_pullRepositoryCaption.Text, destination),
                    Buttons = { TaskDialogButton.Cancel },
                    Icon = TaskDialogIcon.Error,
                    Verification = new TaskDialogVerificationCheckBox
                    {
                        Text = TranslatedStrings.DontShowAgain
                    },
                    AllowCancel = true,
                    SizeToContent = true
                };
                TaskDialogCommandLinkButton btnPullDefault = new(pullDefaultButtonText);
                TaskDialogCommandLinkButton btnPullRebase = new(_pullRebaseButton.Text);
                TaskDialogCommandLinkButton btnPullMerge = new(_pullMergeButton.Text);
                TaskDialogCommandLinkButton btnPushForce = new(_pushForceButton.Text);
                if (allOptions)
                {
                    page.Buttons.Add(btnPullDefault);
                    page.Buttons.Add(btnPullRebase);
                    page.Buttons.Add(btnPullMerge);
                }

                page.Buttons.Add(btnPushForce);

                DebugHelpers.Assert(owner is not null, "The dialog must be owned by another window! This is a bug, please correct and send a pull request with a fix.");
                TaskDialogButton result = TaskDialog.ShowDialog(owner, page);
                if (result == TaskDialogButton.Cancel)
                {
                    onRejectedPullAction = AppSettings.PullAction.None;
                }
                else if (result == btnPullDefault)
                {
                    onRejectedPullAction = AppSettings.PullAction.Default;
                }
                else if (result == btnPullRebase)
                {
                    onRejectedPullAction = AppSettings.PullAction.Rebase;
                }
                else if (result == btnPullMerge)
                {
                    onRejectedPullAction = AppSettings.PullAction.Merge;
                }
                else if (result == btnPushForce)
                {
                    forcePush = true;
                }

                if (page.Verification.Checked)
                {
                    AppSettings.AutoPullOnPushRejectedAction = onRejectedPullAction;
                }
            }

            return (onRejectedPullAction ?? AppSettings.PullAction.None, forcePush);
        }

        private void UpdateBranchDropDown()
        {
            string curBranch = _NO_TRANSLATE_Branch.Text;

            _NO_TRANSLATE_Branch.DisplayMember = nameof(IGitRef.Name);
            _NO_TRANSLATE_Branch.Items.Clear();
            _NO_TRANSLATE_Branch.Items.Add(AllRefs);
            _NO_TRANSLATE_Branch.Items.Add(HeadText);

            if (string.IsNullOrEmpty(curBranch))
            {
                Validates.NotNull(_currentBranchName);
                curBranch = _currentBranchName;
                if (curBranch.IndexOfAny("() ".ToCharArray()) != -1)
                {
                    curBranch = HeadText;
                }
            }

            _NO_TRANSLATE_Branch.Items.AddRange(GetLocalBranches().ToArray());

            _NO_TRANSLATE_Branch.ResizeDropDownWidth(AppSettings.BranchDropDownMinWidth, AppSettings.BranchDropDownMaxWidth);

            _NO_TRANSLATE_Branch.Text = curBranch;
        }

        private IEnumerable<IGitRef> GetLocalBranches()
        {
            return _gitRefs.Where(r => r.IsHead);
        }

        private IEnumerable<IGitRef> GetRemoteBranches(string? remoteName)
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

            if (_selectedRemote is not null)
            {
                RemoteBranch.Items.AddRange(GetRemoteBranches(_selectedRemote.Name).Select(head => head.LocalName).Where(head => _NO_TRANSLATE_Branch.Text != head).ToArray());

                HashSet<string> remoteBranchesSet = GetRemoteBranches(_selectedRemote.Name).Select(b => b.LocalName).ToHashSet();
                IEnumerable<IGitRef> onlyLocalBranches = GetLocalBranches().Where(b => !remoteBranchesSet.Contains(b.LocalName));

                RemoteBranch.Items.AddRange(onlyLocalBranches.Select(head => head.LocalName).Where(head => _NO_TRANSLATE_Branch.Text != head).ToArray());
            }

            RemoteBranch.ResizeDropDownWidth(AppSettings.BranchDropDownMinWidth, AppSettings.BranchDropDownMaxWidth);

            // Set text again as workaround for appearing focused after setting DropDownWidth
            RemoteBranch.Text = RemoteBranch.Text;
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
                        if (_selectedRemote is not null)
                        {
                            string? defaultRemote = _remotesManager.GetDefaultPushRemote(_selectedRemote, branch.Name);
                            if (!string.IsNullOrEmpty(defaultRemote))
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

            GitUIPluginInterfaces.RepositoryHosts.IRepositoryHostPlugin gitHoster = PluginRegistry.TryGetGitHosterForModule(Module);
            _createPullRequestCB.Enabled = gitHoster is not null;
        }

        private void AddRemoteClick(object sender, EventArgs e)
        {
            OpenRemotesDialogAndRefreshList(_selectedRemote?.Name);
        }

        private void PushToUrlCheckedChanged(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            PushDestination.Enabled = PushToUrl.Checked;
            folderBrowserButton1.Enabled = PushToUrl.Checked;
            _NO_TRANSLATE_Remotes.Enabled = PushToRemote.Checked;
            AddRemote.Enabled = PushToRemote.Checked;

            if (PushToUrl.Checked)
            {
                IList<Repository> repositoryHistory = ThreadHelper.JoinableTaskFactory.Run(RepositoryHistoryManager.Remotes.LoadRecentHistoryAsync);
                string prevUrl = PushDestination.Text;
                PushDestination.DataSource = repositoryHistory;
                PushDestination.DisplayMember = nameof(Repository.Path);
                PushDestination.Text = prevUrl;

                BranchSelectedValueChanged(this, EventArgs.Empty);
            }
            else
            {
                RemotesUpdated(sender, e);
            }
        }

        private void RemotesUpdated(object sender, EventArgs e)
        {
            _selectedRemote = _NO_TRANSLATE_Remotes.SelectedItem as ConfigFileRemote;
            if (_selectedRemote is null)
            {
                return;
            }

            if (TabControlTagBranch.SelectedTab == MultipleBranchTab)
            {
                UpdateMultiBranchView();
            }

            EnableLoadSshButton();

            // update the text box of the Remote Url combobox to show the URL of selected remote
            string? pushUrl = _selectedRemote.PushUrl;
            if (string.IsNullOrEmpty(pushUrl))
            {
                pushUrl = _selectedRemote.Url;
            }

            PushDestination.Text = pushUrl;

            if (string.IsNullOrEmpty(_NO_TRANSLATE_Branch.Text))
            {
                // Doing this makes it pretty easy to accidentally create a branch on the remote.
                // But leaving it blank will do the 'default' thing, meaning all branches are pushed.
                // Solution: when pushing a branch that doesn't exist on the remote, ask what to do
                Validates.NotNull(_currentBranchName);
                Validates.NotNull(_selectedRemote.Name);
                GitRef currentBranch = new(Module, null, _currentBranchName, _selectedRemote.Name);
                _NO_TRANSLATE_Branch.Items.Add(currentBranch);
                _NO_TRANSLATE_Branch.SelectedItem = currentBranch;
            }

            BranchSelectedValueChanged(this, EventArgs.Empty);
        }

        private void EnableLoadSshButton()
        {
            LoadSSHKey.Visible = !string.IsNullOrWhiteSpace(_selectedRemote?.PuttySshKey);
        }

        private void LoadSshKeyClick(object sender, EventArgs e)
        {
            StartPageant(_selectedRemote?.Name);
        }

        private void StartPageant(string? remote)
        {
            if (GitSshHelpers.IsPlink)
            {
                PuttyHelpers.StartPageantIfConfigured(() => Module.GetPuttyKeyFileForRemote(remote));
            }
        }

        private void RemotesValidated(object sender, EventArgs e)
        {
            EnableLoadSshButton();
        }

        private void FillTagDropDown()
        {
            // var tags = Module.GetTagHeads(Module.GetTagHeadsOption.OrderByCommitDateDescending); // comment out to sort by commit date
            List<string> tags = Module.GetRefs(RefsFilter.Tags)
                                      .Select(tag => tag.Name)
                                      .ToList();
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

        private DataTable? _branchTable;

        private void UpdateMultiBranchView()
        {
            _branchTable = new DataTable();
            _branchTable.Columns.Add(LocalColumnName, typeof(string));
            _branchTable.Columns.Add(RemoteColumnName, typeof(string));
            _branchTable.Columns.Add(AheadColumnName, typeof(string));
            _branchTable.Columns.Add(PushColumnName, typeof(bool));
            _branchTable.Columns.Add(ForceColumnName, typeof(bool));
            _branchTable.Columns.Add(DeleteColumnName, typeof(bool));
            _branchTable.ColumnChanged += BranchTable_ColumnChanged;
            LocalColumn.DataPropertyName = LocalColumnName;
            RemoteColumn.DataPropertyName = RemoteColumnName;
            NewColumn.DataPropertyName = AheadColumnName;
            PushColumn.DataPropertyName = PushColumnName;
            ForceColumn.DataPropertyName = ForceColumnName;
            DeleteColumn.DataPropertyName = DeleteColumnName;
            BranchGrid.DataSource = new BindingSource { DataSource = _branchTable };

            if (_selectedRemote?.Name is null)
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
                IDetailedSettings detailedSettings = Module.GetEffectiveSettings()
                    .Detailed();

                if (detailedSettings.GetRemoteBranchesDirectlyFromRemote)
                {
                    StartPageant(remote);

                    FormRemoteProcess formProcess = new(UICommands, $"ls-remote --heads \"{remote}\"")
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

                        string refList = CleanCommandOutput(formProcess.GetOutputString());

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
                        ? processOutput[(firstTabIdx - 40)..]
                        : string.Empty;
            }

            void ProcessHeads(IReadOnlyList<IGitRef> remoteHeads)
            {
                List<IGitRef> localHeads = GetLocalBranches().ToList();
                Dictionary<string, IGitRef> remoteBranches = remoteHeads.ToDictionary(h => h.LocalName, h => h);

                Validates.NotNull(_branchTable);
                _branchTable.BeginLoadData();
                AheadBehindDataProvider aheadBehindDataProvider = new(() => Module.GitExecutable);
                IDictionary<string, AheadBehindData> aheadBehindData = aheadBehindDataProvider.GetData();

                // Add all the local branches.
                foreach (IGitRef head in localHeads)
                {
                    string remoteName = head.Remote == remote
                        ? head.MergeWith ?? head.Name
                        : string.Empty;
                    bool isKnownAtRemote = remoteBranches.ContainsKey(head.Name);
                    DataRow row = _branchTable.NewRow();

                    // Check if aheadBehind is relevant for this branch
                    bool isAheadRemote = (aheadBehindData?.ContainsKey(head.Name) ?? false)
                        && GitRefName.GetRemoteName(aheadBehindData[head.Name].RemoteRef) == remote;

                    row[ForceColumnName] = false;
                    row[DeleteColumnName] = false;
                    row[LocalColumnName] = head.Name;
                    row[RemoteColumnName] = isAheadRemote
                        ? GitRefName.GetRemoteBranch(aheadBehindData![head.Name].RemoteRef)
                        : remoteName;

                    row[AheadColumnName] = isAheadRemote
                        ? aheadBehindData![head.Name].ToDisplay()
                        : !isKnownAtRemote
                        ? string.Empty
                        : head.ObjectId == remoteBranches[head.Name].ObjectId
                        ? "="
                        : "<>";
                    row[PushColumnName] = false;
                    _branchTable.Rows.Add(row);
                }

                // Offer to delete all the left over remote branches.
                foreach (IGitRef remoteHead in remoteHeads)
                {
                    if (localHeads.All(h => h.Name != remoteHead.LocalName))
                    {
                        DataRow row = _branchTable.NewRow();

                        row[LocalColumnName] = null;
                        row[RemoteColumnName] = remoteHead.LocalName;
                        row[AheadColumnName] = string.Empty;
                        row[PushColumnName] = false;
                        row[ForceColumnName] = false;
                        row[DeleteColumnName] = false;

                        _branchTable.Rows.Add(row);
                    }
                }

                _branchTable.EndLoadData();
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

        private void BranchGrid_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            foreach (DataGridViewRow row in BranchGrid.Rows)
            {
                if (row.Cells[0].Value == DBNull.Value)
                {
                    row.Cells[3].ReadOnly = true;
                    row.Cells[4].ReadOnly = true;
                }
            }
        }

        private void BranchGrid_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            if ((e.ColumnIndex == 3 || e.ColumnIndex == 4) && BranchGrid.Rows[e.RowIndex].Cells[0].Value == DBNull.Value)
            {
                e.PaintBackground(e.ClipBounds, true);
                e.Handled = true;
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

        private void BranchGrid_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex == _pushColumnIndex && e.Button == MouseButtons.Left)
            {
                Point locationWhereToOpenContextMenu = BranchGrid.PointToScreen(BranchGrid.Location)
                                                     + new Size(BranchGrid.GetCellDisplayRectangle(_pushColumnIndex, -1, true).Location)
                                                     + new Size(e.Location);
                menuPushSelection.Show(locationWhereToOpenContextMenu);
            }
        }

        private void unselectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetBranchesPushCheckboxesState(_ => false);
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetBranchesPushCheckboxesState(_ => true);
        }

        private void selectTrackedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetBranchesPushCheckboxesState(row =>
                {
                    // Check if the branch is tracked (i.e. not new)
                    return row.Cells[LocalColumn.Name] is DataGridViewTextBoxCell localColumn &&
                           row.Cells[RemoteColumn.Name] is DataGridViewTextBoxCell remoteColumn &&
                           !string.IsNullOrEmpty(localColumn.Value.ToString()) &&
                           !string.IsNullOrEmpty(remoteColumn.Value.ToString());
                });
        }

        private void SetBranchesPushCheckboxesState(Func<DataGridViewRow, bool> willPush)
        {
            // Necessary to end the edit mode of the Cell.
            BranchGrid.EndEdit();

            foreach (DataGridViewRow row in BranchGrid.Rows)
            {
                DataGridViewCheckBoxCell pushCheckBox = row.Cells[PushColumn.Name] as DataGridViewCheckBoxCell;
                if (pushCheckBox is null || !pushCheckBox.Visible)
                {
                    continue;
                }

                pushCheckBox.Value = willPush(row);
            }
        }

        internal TestAccessor GetTestAccessor() => new(this);

        internal readonly struct TestAccessor
        {
            private readonly FormPush _form;

            public TestAccessor(FormPush form)
            {
                _form = form;
            }

            public CheckBox ckForceWithLease => _form.ckForceWithLease;

            public CheckBox ForcePushBranches => _form.ForcePushBranches;

            public CheckBox ForcePushTags => _form.ForcePushTags;

            public ForcePushOptions GetForcePushOption() => _form.GetForcePushOption();
        }
    }
}
