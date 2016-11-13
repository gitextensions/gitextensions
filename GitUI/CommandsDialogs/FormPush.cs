using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Repository;
using GitUI.Objects;
using GitUI.Script;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class FormPush : GitModuleForm
    {
        private const string HeadText = "HEAD";
        private const string AllRefs = "[ All ]";
        private string _currentBranchName;
        private GitRemote _currentBranchRemote;
        private bool _candidateForRebasingMergeCommit;
        private string _selectedBranch;
        private GitRemote _selectedRemote;
        private string _selectedRemoteBranchName;
        private IList<IGitRef> _gitRefs;
        private readonly IGitRemoteController _gitRemoteController;

        public bool ErrorOccurred { get; private set; }

        #region Translation
        private readonly TranslationString _branchNewForRemote =
            new TranslationString("The branch you are about to push seems to be a new branch for the remote." +
                                  Environment.NewLine + "Are you sure you want to push this branch?");

        private readonly TranslationString _pushCaption = new TranslationString("Push");

        private readonly TranslationString _pushToCaption = new TranslationString("Push to {0}");

        private readonly TranslationString _selectDestinationDirectory =
            new TranslationString("Please select a destination directory");

        private readonly TranslationString _selectRemote = new TranslationString("Please select a remote repository");

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
        private readonly TranslationString _pullRepositoryButtons = new TranslationString("Pull with last pull action ({0})|Pull with rebase|Pull with merge|Force push|Cancel");
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

        private FormPush()
            : this(null)
        { }

        public FormPush(GitUICommands aCommands)
            : base(aCommands)
        {
            InitializeComponent();
            Translate();

            if (!GitCommandHelpers.VersionInUse.SupportPushForceWithLease)
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


            //can't be set in OnLoad, because after PushAndShowDialogWhenFailed()
            //they are reset to false
            if (aCommands != null)
            {
                _gitRemoteController = new GitRemoteController(Module);
                Init();
            }
        }

        private void Init()
        {
            _gitRefs = Module.GetRefs(false, true);
            if (GitCommandHelpers.VersionInUse.SupportPushWithRecursiveSubmodulesCheck)
            {
                RecursiveSubmodules.Enabled = true;
                RecursiveSubmodules.SelectedIndex = AppSettings.RecursiveSubmodules;
                if (!GitCommandHelpers.VersionInUse.SupportPushWithRecursiveSubmodulesOnDemand)
                    RecursiveSubmodules.Items.RemoveAt(2);
            }
            else
            {
                RecursiveSubmodules.Enabled = false;
                RecursiveSubmodules.SelectedIndex = 0;
            }

            _currentBranchName = Module.GetSelectedBranch();

            // refresh registered git remotes
            _gitRemoteController.LoadRemotes();
            BindRemotesDropDown(_currentBranchName);

            UpdateBranchDropDown();
            UpdateRemoteBranchDropDown();

            Push.Focus();

            if (AppSettings.AlwaysShowAdvOpt)
            {
                ShowOptions_LinkClicked(null, null);
            }
        }

        public DialogResult PushAndShowDialogWhenFailed(IWin32Window owner)
        {
            if (!PushChanges(owner))
                return ShowDialog(owner);
            return DialogResult.OK;
        }

        public DialogResult PushAndShowDialogWhenFailed()
        {
            return PushAndShowDialogWhenFailed(null);
        }

        private void PushClick(object sender, EventArgs e)
        {
            if (PushChanges(this))
                Close();
        }

        private void BindRemotesDropDown(string selectedRemoteName)
        {
            _NO_TRANSLATE_Remotes.SelectedIndexChanged -= RemotesUpdated;
            _NO_TRANSLATE_Remotes.TextUpdate -= RemotesUpdated;
            _NO_TRANSLATE_Remotes.Sorted = false;
            _NO_TRANSLATE_Remotes.DataSource = _gitRemoteController.Remotes;
            _NO_TRANSLATE_Remotes.DisplayMember = "Name";
            _NO_TRANSLATE_Remotes.SelectedIndex = -1;

            _NO_TRANSLATE_Remotes.SelectedIndexChanged += RemotesUpdated;
            _NO_TRANSLATE_Remotes.TextUpdate += RemotesUpdated;

            _currentBranchRemote = _gitRemoteController.Remotes.FirstOrDefault(x => x.Name.Equals(selectedRemoteName, StringComparison.OrdinalIgnoreCase));
            if (_currentBranchRemote != null)
            {
                _NO_TRANSLATE_Remotes.SelectedItem = _currentBranchRemote;
            }
            else if (_gitRemoteController.Remotes.Any())
            {
                // we couldn't find the default assigned remote for the selected branch
                // it is usually gets mapped via FormRemotes -> "default pull behavior" tab
                // so pick the default user remote
                _NO_TRANSLATE_Remotes.SelectedIndex = 0;
            }
            else
            {
                _NO_TRANSLATE_Remotes.SelectedIndex = -1;
            }
        }

        private bool IsBranchKnownToRemote(string remote, string branch)
        {
            var remoteRefs = _gitRefs.Where(r => r.IsRemote && r.LocalName == branch && r.Remote == remote);
            if (remoteRefs.Any())
                return true;

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

            var selectedRemoteName = _selectedRemote.Name;
            if (PushToRemote.Checked && string.IsNullOrEmpty(selectedRemoteName))
            {
                MessageBox.Show(owner, _selectRemote.Text);
                return false;
            }
            if (TabControlTagBranch.SelectedTab == TagTab && string.IsNullOrEmpty(TagComboBox.Text))
            {
                MessageBox.Show(owner, _selectTag.Text);
                return false;
            }

            //Extra check if the branch is already known to the remote, give a warning when not.
            //This is not possible when the remote is an URL, but this is ok since most users push to
            //known remotes anyway.
            if (TabControlTagBranch.SelectedTab == BranchTab && PushToRemote.Checked &&
                !Module.IsBareRepository())
            {
                //If the current branch is not the default push, and not known by the remote
                //(as far as we know since we are disconnected....)
                if (_NO_TRANSLATE_Branch.Text != AllRefs &&
                    RemoteBranch.Text != _gitRemoteController.GetDefaultPushRemote(_selectedRemote, _NO_TRANSLATE_Branch.Text) &&
                    !IsBranchKnownToRemote(selectedRemoteName, RemoteBranch.Text))
                {
                    //Ask if this is really what the user wants
                    if (!AppSettings.DontConfirmPushNewBranch &&
                        DialogResult.No == MessageBox.Show(owner, _branchNewForRemote.Text, _pushCaption.Text, MessageBoxButtons.YesNo))
                    {
                        return false;
                    }
                }
            }

            if (PushToUrl.Checked)
            {
                Repositories.AddMostRecentRepository(PushDestination.Text);
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
                            !_gitRemoteController.Remotes.Any(x => _NO_TRANSLATE_Branch.Text.StartsWith(x.Name, StringComparison.OrdinalIgnoreCase));
                    var autoSetupMerge = Module.EffectiveConfigFile.GetValue("branch.autoSetupMerge");
                    if (autoSetupMerge.IsNotNullOrWhitespace() && autoSetupMerge.ToLowerInvariant() == "false")
                    {
                        track = false;
                    }
                    if (track && !AppSettings.DontConfirmAddTrackingRef)
                    {
                        var result = MessageBox.Show(this,
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
                    if (GitCommandHelpers.VersionInUse.SupportPushForceWithLease)
                    {
                        var choice = MessageBox.Show(this,
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
                    var push = Convert.ToBoolean(row["Push"]);
                    var force = Convert.ToBoolean(row["Force"]);
                    var delete = Convert.ToBoolean(row["Delete"]);

                    if (push || force)
                        pushActions.Add(new GitPushAction(row["Local"].ToString(), row["Remote"].ToString(), force));
                    else if (delete)
                        pushActions.Add(GitPushAction.DeleteRemoteBranch(row["Remote"].ToString()));
                }
                pushCmd = GitCommandHelpers.PushMultipleCmd(destination, pushActions);
            }

            ScriptManager.RunEventScripts(this, ScriptEvent.BeforePush);

            //controls can be accessed only from UI thread
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
                        UICommands.StartCreatePullRequest(owner);
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
            if (AppSettings.FormPullAction == AppSettings.PullAction.Rebase && 
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

            //there is no way to pull to not current branch
            if (_selectedBranch != _currentBranchName)
            {
                return false;
            }

            //auto pull from URL not supported. See https://github.com/gitextensions/gitextensions/issues/1887
            if (!PushToRemote.Checked)
            {
                return false;
            }

            //auto pull only if current branch was rejected
            Regex isRejected = new Regex(Regex.Escape("! [rejected] ") + ".*" + Regex.Escape(_currentBranchName) + ".*", RegexOptions.Compiled);
            if (isRejected.IsMatch(form.GetOutputString()) && !Module.IsBareRepository())
            {
                bool forcePush = false;
                IWin32Window owner = form;
                if (AppSettings.AutoPullOnPushRejectedAction == null)
                {
                    bool cancel = false;
                    string destination = PushToRemote.Checked ? _NO_TRANSLATE_Remotes.Text : PushDestination.Text;
                    string buttons = _pullRepositoryButtons.Text;
                    switch (Module.LastPullAction)
                    {
                        case AppSettings.PullAction.Fetch:
                        case AppSettings.PullAction.FetchAll:
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
                                    String.Format(_pullRepositoryCaption.Text, destination),
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
                            if (rememberDecision)
                            {
                                AppSettings.AutoPullOnPushRejectedAction = AppSettings.PullAction.Default;
                            }
                            break;
                        case 1:
                            AppSettings.FormPullAction = AppSettings.PullAction.Rebase;
                            if (rememberDecision)
                            {
                                AppSettings.AutoPullOnPushRejectedAction = AppSettings.FormPullAction;
                            }
                            break;
                        case 2:
                            AppSettings.FormPullAction = AppSettings.PullAction.Merge;
                            if (rememberDecision)
                            {
                                AppSettings.AutoPullOnPushRejectedAction = AppSettings.FormPullAction;
                            }
                            break;
                        case 3:
                            forcePush = true;
                            break;
                        default:
                            cancel = true;
                            if (rememberDecision)
                            {
                                AppSettings.AutoPullOnPushRejectedAction = AppSettings.PullAction.None;
                            }
                            break;
                    }
                    if (cancel)
                        return false;
                }

                if (forcePush)
                {
                    if (!form.ProcessArguments.Contains(" -f ") && !form.ProcessArguments.Contains(" --force"))
                    {
                        if (GitCommandHelpers.VersionInUse.SupportPushForceWithLease)
                        {
                            form.ProcessArguments = form.ProcessArguments.Replace("push", "push --force-with-lease");
                        }
                        else
                        {
                            form.ProcessArguments = form.ProcessArguments.Replace("push", "push -f");
                        }
                    }
                    form.Retry();
                    return true;
                }

                if (AppSettings.AutoPullOnPushRejectedAction == AppSettings.PullAction.None)
                {
                    return false;
                }

                if (AppSettings.AutoPullOnPushRejectedAction == AppSettings.PullAction.Default)
                {
                    if (Module.LastPullAction == AppSettings.PullAction.None)
                    {
                        return false;
                    }

                    Module.LastPullActionToFormPullAction();
                }

                if (AppSettings.FormPullAction == AppSettings.PullAction.Fetch)
                {
                    form.AppendOutput(Environment.NewLine +
                        "Can not perform auto pull, when merge option is set to fetch.");
                    return false;
                }

                if (IsRebasingMergeCommit())
                {
                    form.AppendOutput(Environment.NewLine +
                        "Can not perform auto pull, when merge option is set to rebase " + Environment.NewLine +
                        "and one of the commits that are about to be rebased is a merge.");
                    return false;
                }

                bool pullCompleted;
                UICommands.StartPullDialog(owner, true, _selectedRemoteBranchName, _selectedRemote.Name, out pullCompleted, false);
                if (pullCompleted)
                {
                    form.Retry();
                    return true;
                }
            }

            return false;
        }

        private void FillPushDestinationDropDown()
        {
            PushDestination.DataSource = Repositories.RemoteRepositoryHistory.Repositories;
            PushDestination.DisplayMember = "Path";
        }

        private void UpdateBranchDropDown()
        {
            var curBranch = _NO_TRANSLATE_Branch.Text;

            _NO_TRANSLATE_Branch.DisplayMember = "Name";
            _NO_TRANSLATE_Branch.Items.Clear();
            _NO_TRANSLATE_Branch.Items.Add(AllRefs);
            _NO_TRANSLATE_Branch.Items.Add(HeadText);

            if (string.IsNullOrEmpty(curBranch))
            {
                curBranch = _currentBranchName;
                if (curBranch.IndexOfAny("() ".ToCharArray()) != -1)
                    curBranch = HeadText;
            }

            foreach (var head in _gitRefs)
                _NO_TRANSLATE_Branch.Items.Add(head);

            _NO_TRANSLATE_Branch.Text = curBranch;

            ComboBoxHelper.ResizeComboBoxDropDownWidth(_NO_TRANSLATE_Branch, AppSettings.BranchDropDownMinWidth, AppSettings.BranchDropDownMaxWidth);
        }

        private void PullClick(object sender, EventArgs e)
        {
            UICommands.StartPullDialog(this);
        }

        private void UpdateRemoteBranchDropDown()
        {
            RemoteBranch.DisplayMember = "Name";
            RemoteBranch.Items.Clear();

            if (!string.IsNullOrEmpty(_NO_TRANSLATE_Branch.Text))
                RemoteBranch.Items.Add(_NO_TRANSLATE_Branch.Text);

            foreach (var head in _gitRefs)
                if (!RemoteBranch.Items.Contains(head))
                    RemoteBranch.Items.Add(head);

            ComboBoxHelper.ResizeComboBoxDropDownWidth(RemoteBranch, AppSettings.BranchDropDownMinWidth, AppSettings.BranchDropDownMaxWidth);
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
                    var branch = _NO_TRANSLATE_Branch.SelectedItem as GitRef;

                    if (branch != null)
                    {
                        string defaultRemote = _gitRemoteController.GetDefaultPushRemote(_selectedRemote, branch.Name);
                        if (!defaultRemote.IsNullOrEmpty())
                        {
                            RemoteBranch.Text = defaultRemote;
                            return;
                        }

                        if (branch.TrackingRemote.Equals(_selectedRemote.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            RemoteBranch.Text = branch.MergeWith;
                            if (!string.IsNullOrEmpty(RemoteBranch.Text))
                                return;
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

            var gitHoster = RepoHosts.TryGetGitHosterForModule(Module);
            _createPullRequestCB.Enabled = gitHoster != null;
        }

        private void AddRemoteClick(object sender, EventArgs e)
        {
            // store the selected remote name
            string selectedRemoteName = _selectedRemote.Name;

            // launch remote management dialog
            UICommands.StartRemotesDialog(this, selectedRemoteName);

            // coming back from the dialog, update the remotes list
            _gitRemoteController.LoadRemotes();
            BindRemotesDropDown(selectedRemoteName);
        }

        private void PushToUrlCheckedChanged(object sender, EventArgs e)
        {
            PushDestination.Enabled = PushToUrl.Checked;
            folderBrowserButton1.Enabled = PushToUrl.Checked;
            _NO_TRANSLATE_Remotes.Enabled = PushToRemote.Checked;
            AddRemote.Enabled = PushToRemote.Checked;

            if (PushToUrl.Checked)
            {
                FillPushDestinationDropDown();
                BranchSelectedValueChanged(null, null);
            }
            else
                RemotesUpdated(sender, e);
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
            LoadSSHKey.Visible = _selectedRemote.PuttySshKey.IsNotNullOrWhitespace();
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

            ComboBoxHelper.ResizeComboBoxDropDownWidth(TagComboBox, AppSettings.BranchDropDownMinWidth, AppSettings.BranchDropDownMaxWidth);
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
            _branchTable.Columns.Add("Local", typeof(string));
            _branchTable.Columns.Add("Remote", typeof(string));
            _branchTable.Columns.Add("New", typeof(string));
            _branchTable.Columns.Add("Push", typeof(bool));
            _branchTable.Columns.Add("Force", typeof(bool));
            _branchTable.Columns.Add("Delete", typeof(bool));
            _branchTable.ColumnChanged += BranchTable_ColumnChanged;
            var bs = new BindingSource { DataSource = _branchTable };
            BranchGrid.DataSource = bs;

            if (_selectedRemote == null)
            {
                return;
            }

            var localHeads = _gitRefs.Where(r => r.IsHead).ToList();
            LoadMultiBranchViewData(_selectedRemote.Name, localHeads);
        }

        private void LoadMultiBranchViewData(string remote, IList<IGitRef> localHeads)
        {
            Cursor = Cursors.AppStarting;
            try
            {
                IList<IGitRef> remoteHeads;
                if (Module.EffectiveSettings.Detailed.GetRemoteBranchesDirectlyFromRemote.ValueOrDefault)
                {
                    EnsurePageant(remote);
                    var cmdGetBranchesFromRemote = "ls-remote --heads \"" + remote + "\"";
                    using (var formProcess = new FormRemoteProcess(Module, cmdGetBranchesFromRemote)
                    {
                        Remote = remote
                    })
                    {

                        formProcess.ShowDialog(this);
                        if (formProcess.ErrorOccurred())
                        {
                            return;
                        }
                        var processOutput = formProcess.GetOutputString();
                        var cmdOutput = TakeCommandOutput(processOutput);
                        remoteHeads = Module.GetTreeRefs(cmdOutput);
                        if (remoteHeads == null)
                            return;
                    }
                }
                else
                {
                    //use remote branches from the git's local database if there were problems with receiving branches from the remote server
                    remoteHeads = Module.GetRemoteBranches().Where(r => r.Remote == remote).ToList();
                }
                ProcessHeads(remote, localHeads, remoteHeads);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private static string TakeCommandOutput(string aProcessOutput)
        {
            //the command output consists of lines in the format:
            //fa77791d780a01a06d1f7d4ccad4ef93ed0ae2fd\trefs/heads/branchName
            int firstTabIdx = aProcessOutput.IndexOf('\t');
            if (firstTabIdx < 40)
            {
                return string.Empty;
            }
            var cmdOutput = aProcessOutput.Substring(firstTabIdx - 40);
            return cmdOutput;
        }

        private void ProcessHeads(string remote, IList<IGitRef> localHeads, IList<IGitRef> remoteHeads)
        {
            var remoteBranches = remoteHeads.ToHashSet(h => h.LocalName);
            // Add all the local branches.
            foreach (var head in localHeads)
            {
                DataRow row = _branchTable.NewRow();
                row["Force"] = false;
                row["Delete"] = false;
                row["Local"] = head.Name;

                string remoteName;
                if (head.Remote == remote)
                    remoteName = head.MergeWith ?? head.Name;
                else
                    remoteName = head.Name;

                row["Remote"] = remoteName;
                bool knownAtRemote = remoteBranches.Contains(remoteName);
                row["New"] = knownAtRemote ? _no.Text : _yes.Text;
                row["Push"] = knownAtRemote;

                _branchTable.Rows.Add(row);
            }

            // Offer to delete all the left over remote branches.
            foreach (var remoteHead in remoteHeads)
            {
                var head = remoteHead;
                if (localHeads.All(h => h.Name != head.LocalName))
                {
                    DataRow row = _branchTable.NewRow();
                    row["Local"] = null;
                    row["Remote"] = remoteHead.LocalName;
                    row["New"] = _no.Text;
                    row["Push"] = false;
                    row["Force"] = false;
                    row["Delete"] = false;
                    _branchTable.Rows.Add(row);
                }
            }

            BranchGrid.Enabled = true;
        }

        static void BranchTable_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            if (e.Column.ColumnName == "Push" && (bool)e.ProposedValue)
            {
                e.Row["Force"] = false;
                e.Row["Delete"] = false;
            }
            if (e.Column.ColumnName == "Force" && (bool)e.ProposedValue)
            {
                e.Row["Push"] = false;
                e.Row["Delete"] = false;
            }
            if (e.Column.ColumnName == "Delete" && (bool)e.ProposedValue)
            {
                e.Row["Push"] = false;
                e.Row["Force"] = false;
            }
        }

        private void TabControlTagBranch_Selected(object sender, TabControlEventArgs e)
        {
            if (TabControlTagBranch.SelectedTab == MultipleBranchTab)
                UpdateMultiBranchView();
            else if (TabControlTagBranch.SelectedTab == TagTab)
                FillTagDropDown();
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
                Height = MinimumSize.Height + 50;
        }

        private void _NO_TRANSLATE_Branch_SelectedIndexChanged(object sender, EventArgs e)
        {
            RemoteBranch.Enabled = (_NO_TRANSLATE_Branch.Text != AllRefs);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                    components.Dispose();
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
