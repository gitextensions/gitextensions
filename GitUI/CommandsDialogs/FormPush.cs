using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Config;
using GitCommands.Repository;
using GitUI.Script;
using ResourceManager.Translation;

namespace GitUI.CommandsDialogs
{
    public partial class FormPush : GitModuleForm
    {
        private const string HeadText = "HEAD";
        private const string AllRefs = "[ All ]";
        private string _currentBranch;
        private string _currentBranchRemote;
        private bool _candidateForRebasingMergeCommit;
        private string _selectedBranch;
        private string _selectedBranchRemote;
        private string _selectedRemoteBranchName;

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
        private readonly TranslationString _pullRepositoryButtons = new TranslationString("Pull with default pull action|Pull with rebase|Pull with merge|Force push|Cancel");
        private readonly TranslationString _pullRepositoryCaption = new TranslationString("Push was rejected from \"{0}\"");
        private readonly TranslationString _dontShowAgain = new TranslationString("Remember my decision.");

        #endregion

        private FormPush()
            : this(null)
        { }

        public FormPush(GitUICommands aCommands)
            : base(aCommands)
        {
            InitializeComponent();
            Translate();

            //can't be set in OnLoad, because after PushAndShowDialogWhenFailed()
            //they are reset to false
            if (aCommands != null)
                Init();
        }

        private void Init()
        {
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

            _currentBranch = Module.GetSelectedBranch();

            _NO_TRANSLATE_Remotes.DataSource = Module.GetRemotes();

            UpdateBranchDropDown();
            UpdateRemoteBranchDropDown();

            Push.Focus();

            _currentBranchRemote = Module.GetSetting(string.Format("branch.{0}.remote", _currentBranch));
            if (_currentBranchRemote.IsNullOrEmpty() && _NO_TRANSLATE_Remotes.Items.Count >= 2)
            {
                IList<string> remotes = (IList<string>)_NO_TRANSLATE_Remotes.DataSource;
                int i = remotes.IndexOf("origin");
                _NO_TRANSLATE_Remotes.SelectedIndex = i >= 0 ? i : 0;
            }
            else
                _NO_TRANSLATE_Remotes.Text = _currentBranchRemote;
            RemotesUpdated(null, null);
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

        private string GetDefaultPushLocal(String remote)
        {
            string localRef = null;

            //Get default push for this remote (if any). Local branch name is left of ":"
            var pushSettingValue = Module.GetSetting(string.Format("remote.{0}.push", remote));
            if (!string.IsNullOrEmpty(pushSettingValue))
            {
                var values = pushSettingValue.Split(':');
                if (values.Length > 0)
                    localRef = values[0];
            }
            return localRef;
        }

        private string GetDefaultPushRemote(String remote)
        {
            string remoteRef = null;

            //Get default push for this remote (if any). Remote branch name is right of ":"
            var pushSettingValue = Module.GetSetting(string.Format("remote.{0}.push", remote));
            if (!string.IsNullOrEmpty(pushSettingValue))
            {
                var values = pushSettingValue.Split(':');
                if (values.Length > 1)
                    remoteRef = values[1];
            }

            return remoteRef;
        }

        private bool PushChanges(IWin32Window owner)
        {
            ErrorOccurred = false;
            if (PushToUrl.Checked && string.IsNullOrEmpty(PushDestination.Text))
            {
                MessageBox.Show(owner, _selectDestinationDirectory.Text);
                return false;
            }
            if (PushToRemote.Checked && string.IsNullOrEmpty(_NO_TRANSLATE_Remotes.Text))
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
                    RemoteBranch.Text != GetDefaultPushRemote(_NO_TRANSLATE_Remotes.Text) &&
                    !Module.GetRefs(true, true).Any(x => x.Remote == _NO_TRANSLATE_Remotes.Text && x.LocalName == RemoteBranch.Text))
                {
                    //Ask if this is really what the user wants
                    if (!AppSettings.DontConfirmPushNewBranch &&
                        MessageBox.Show(owner, _branchNewForRemote.Text, _pushCaption.Text, MessageBoxButtons.YesNo) ==
                            DialogResult.No)
                    {
                        return false;
                    }
                }
            }

            if (PushToUrl.Checked)
                Repositories.AddMostRecentRepository(PushDestination.Text);
            AppSettings.RecursiveSubmodules = RecursiveSubmodules.SelectedIndex;

            var remote = "";
            string destination;
            if (PushToUrl.Checked)
            {
                destination = PushDestination.Text;
            }
            else
            {
                if (GitCommandHelpers.Plink())
                {
                    if (!File.Exists(AppSettings.Pageant))
                        MessageBoxes.PAgentNotFound(owner);
                    else
                        Module.StartPageantForRemote(_NO_TRANSLATE_Remotes.Text);
                }

                destination = _NO_TRANSLATE_Remotes.Text;
                remote = _NO_TRANSLATE_Remotes.Text.Trim();
            }

            string pushCmd;
            if (TabControlTagBranch.SelectedTab == BranchTab)
            {
                bool track = ReplaceTrackingReference.Checked;
                if (!track && !string.IsNullOrWhiteSpace(RemoteBranch.Text))
                {
                    GitRef selectedLocalBranch = _NO_TRANSLATE_Branch.SelectedItem as GitRef;
                    track = selectedLocalBranch != null && string.IsNullOrEmpty(selectedLocalBranch.TrackingRemote);

                    string[] remotes = _NO_TRANSLATE_Remotes.DataSource as string[];
                    if (remotes != null)
                        foreach (string remoteBranch in remotes)
                            if (!string.IsNullOrEmpty(remoteBranch) && _NO_TRANSLATE_Branch.Text.StartsWith(remoteBranch))
                                track = false;

                    if (track && !AppSettings.DontConfirmAddTrackingRef)
                    {
                        DialogResult result = MessageBox.Show(String.Format(_updateTrackingReference.Text, selectedLocalBranch.Name, RemoteBranch.Text), _pushCaption.Text, MessageBoxButtons.YesNoCancel);

                        if (result == DialogResult.Cancel)
                            return false;

                        track = result == DialogResult.Yes;
                    }
                }

                // Try to make source rev into a fully qualified branch name. If that
                // doesn't exist, then it must be something other than a branch, so
                // fall back to using the name just as it was passed in.
                string srcRev = "";
                bool pushAllBranches = false;
                if (_NO_TRANSLATE_Branch.Text == AllRefs)
                    pushAllBranches = true;
                else
                {
                    srcRev = GitCommandHelpers.GetFullBranchName(_NO_TRANSLATE_Branch.Text);
                    if (String.IsNullOrEmpty(Module.RevParse(srcRev)))
                        srcRev = _NO_TRANSLATE_Branch.Text;
                }

                pushCmd = GitCommandHelpers.PushCmd(destination, srcRev, RemoteBranch.Text,
                    pushAllBranches, ForcePushBranches.Checked, track, RecursiveSubmodules.SelectedIndex);
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
                pushCmd = GitCommandHelpers.PushTagCmd(destination, tag, pushAllTags,
                                                       ForcePushBranches.Checked);
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
                        pushActions.Add(new GitPushAction(row["Remote"].ToString()));
                }
                pushCmd = GitCommandHelpers.PushMultipleCmd(destination, pushActions);
            }

            ScriptManager.RunEventScripts(this, ScriptEvent.BeforePush);

            //controls can be accessed only from UI thread
            _selectedBranch = _NO_TRANSLATE_Branch.Text;
            _candidateForRebasingMergeCommit = PushToRemote.Checked && (_selectedBranch != AllRefs) && TabControlTagBranch.SelectedTab == BranchTab;
            _selectedBranchRemote = _NO_TRANSLATE_Remotes.Text;
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

                if (!Module.InTheMiddleOfConflictedMerge() &&
                    !Module.InTheMiddleOfRebase() && !form.ErrorOccurred())
                {
                    ScriptManager.RunEventScripts(this, ScriptEvent.AfterPush);
                    if (_createPullRequestCB.Checked)
                        UICommands.StartCreatePullRequest(owner);
                    return true;
                }
            }

            return false;
        }


        private bool IsRebasingMergeCommit()
        {
            if (AppSettings.FormPullAction == AppSettings.PullAction.Rebase && _candidateForRebasingMergeCommit)
            {
                if (_selectedBranch == _currentBranch && _selectedBranchRemote == _currentBranchRemote)
                {
                    string remoteBranchName = _selectedBranchRemote + "/" + _selectedRemoteBranchName;
                    return Module.ExistsMergeCommit(remoteBranchName, _selectedBranch);
                }
                else
                    return false;
            }
            else
                return false;
        }

        private bool HandlePushOnExit(ref bool isError, FormProcess form)
        {
            if (!isError)
                return false;

            //there is no way to pull to not current branch
            if (_selectedBranch != _currentBranch)
                return false;

            //auto pull from URL not supported. See https://github.com/gitextensions/gitextensions/issues/1887
            if (!PushToRemote.Checked)
                return false;

            //auto pull only if current branch was rejected
            Regex IsRejected = new Regex(Regex.Escape("! [rejected] ") + ".*" + Regex.Escape(_currentBranch) + ".*", RegexOptions.Compiled);

            if (IsRejected.IsMatch(form.GetOutputString()) && !Module.IsBareRepository())
            {
                bool forcePush = false;
                IWin32Window owner = form;
                if (AppSettings.AutoPullOnPushRejectedAction == null)
                {
                    bool cancel = false;
                    string destination = PushToRemote.Checked ? _NO_TRANSLATE_Remotes.Text : PushDestination.Text;
                    int idx = PSTaskDialog.cTaskDialog.ShowCommandBox(owner,
                                    String.Format(_pullRepositoryCaption.Text, destination),
                                    _pullRepositoryMainInstruction.Text,
                                    _pullRepository.Text,
                                    "",
                                    "",
                                    _dontShowAgain.Text,
                                    _pullRepositoryButtons.Text,
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
                            if (Module.LastPullAction == AppSettings.PullAction.None)
                            {
                                return false;
                            }
                            Module.LastPullActionToFormPullAction();
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
                    if (!form.ProcessArguments.Contains(" -f "))
                        form.ProcessArguments = form.ProcessArguments.Replace("push", "push -f");
                    form.Retry();
                    return true;
                }

                if (AppSettings.AutoPullOnPushRejectedAction == AppSettings.PullAction.None)
                    return false;

                if (AppSettings.FormPullAction == AppSettings.PullAction.Fetch)
                {
                    form.AppendOutputLine(Environment.NewLine +
                        "Can not perform auto pull, when merge option is set to fetch.");
                    return false;
                }

                if (IsRebasingMergeCommit())
                {
                    form.AppendOutputLine(Environment.NewLine +
                        "Can not perform auto pull, when merge option is set to rebase " + Environment.NewLine +
                        "and one of the commits that are about to be rebased is a merge.");
                    return false;
                }

                bool pullCompleted;
                UICommands.StartPullDialog(owner, true, null, _selectedBranchRemote, out pullCompleted, false);
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
                curBranch = _currentBranch;
                if (curBranch.IndexOfAny("() ".ToCharArray()) != -1)
                    curBranch = HeadText;
            }

            foreach (var head in Module.GetRefs(false, true))
                _NO_TRANSLATE_Branch.Items.Add(head);

            _NO_TRANSLATE_Branch.Text = curBranch;
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

            foreach (var head in Module.GetRefs(false, true))
                if (!RemoteBranch.Items.Contains(head))
                    RemoteBranch.Items.Add(head);
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
                    if (branch != null && branch.TrackingRemote.Equals(_NO_TRANSLATE_Remotes.Text.Trim()))
                    {
                        RemoteBranch.Text = branch.MergeWith;
                        if (!string.IsNullOrEmpty(RemoteBranch.Text))
                            return;
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
            UICommands.StartRemotesDialog(this, _NO_TRANSLATE_Remotes.Text);
            string origText = _NO_TRANSLATE_Remotes.Text;
            _NO_TRANSLATE_Remotes.DataSource = Module.GetRemotes();
            if (_NO_TRANSLATE_Remotes.Items.Contains(origText)) // else first item gets selected
            {
                _NO_TRANSLATE_Remotes.Text = origText;
            }
        }

        private void PushToRemoteCheckedChanged(object sender, EventArgs e)
        {
            BranchSelectedValueChanged(null, null);
            if (!PushToRemote.Checked)
                return;

            PushDestination.Enabled = false;
            folderBrowserButton1.Enabled = false;
            _NO_TRANSLATE_Remotes.Enabled = true;
            AddRemote.Enabled = true;
        }

        private void PushToUrlCheckedChanged(object sender, EventArgs e)
        {
            if (!PushToUrl.Checked)
                return;

            PushDestination.Enabled = true;
            folderBrowserButton1.Enabled = true;
            _NO_TRANSLATE_Remotes.Enabled = false;
            AddRemote.Enabled = false;

            FillPushDestinationDropDown();
        }

        private void RemotesUpdated(object sender, EventArgs e)
        {
            if (TabControlTagBranch.SelectedTab == MultipleBranchTab)
                UpdateMultiBranchView();

            EnableLoadSshButton();

            // update the text box of the Remote Url combobox to show the URL of selected remote
            {
                string pushUrl = Module.GetPathSetting(string.Format(SettingKeyString.RemotePushUrl, _NO_TRANSLATE_Remotes.Text));
                if (pushUrl.IsNullOrEmpty())
                {
                    pushUrl = Module.GetPathSetting(string.Format(SettingKeyString.RemoteUrl, _NO_TRANSLATE_Remotes.Text));
                }
                PushDestination.Text = pushUrl;
            }

            var pushSettingValue = Module.GetSetting(string.Format("remote.{0}.push", _NO_TRANSLATE_Remotes.Text));

            if (PushToRemote.Checked && !string.IsNullOrEmpty(pushSettingValue))
            {
                string defaultLocal = GetDefaultPushLocal(_NO_TRANSLATE_Remotes.Text);
                string defaultRemote = GetDefaultPushRemote(_NO_TRANSLATE_Remotes.Text);

                RemoteBranch.Text = "";
                if (!string.IsNullOrEmpty(defaultLocal))
                {
                    var currentBranch = new GitRef(Module, null, defaultLocal, _NO_TRANSLATE_Remotes.Text);
                    _NO_TRANSLATE_Branch.Items.Add(currentBranch);
                    _NO_TRANSLATE_Branch.SelectedItem = currentBranch;
                }
                if (!string.IsNullOrEmpty(defaultRemote))
                    RemoteBranch.Text = defaultRemote;
                return;
            }

            if (string.IsNullOrEmpty(_NO_TRANSLATE_Branch.Text))
            {
                // Doing this makes it pretty easy to accidentally create a branch on the remote.
                // But leaving it blank will do the 'default' thing, meaning all branches are pushed.
                // Solution: when pushing a branch that doesn't exist on the remote, ask what to do
                var currentBranch = new GitRef(Module, null, _currentBranch, _NO_TRANSLATE_Remotes.Text);
                _NO_TRANSLATE_Branch.Items.Add(currentBranch);
                _NO_TRANSLATE_Branch.SelectedItem = currentBranch;
                return;
            }

            BranchSelectedValueChanged(null, null);
        }

        private void EnableLoadSshButton()
        {
            LoadSSHKey.Visible = !string.IsNullOrEmpty(Module.GetPuttyKeyFileForRemote(_NO_TRANSLATE_Remotes.Text));
        }

        private void LoadSshKeyClick(object sender, EventArgs e)
        {
            if (!File.Exists(AppSettings.Pageant))
                MessageBoxes.PAgentNotFound(this);
            else
                Module.StartPageantForRemote(_NO_TRANSLATE_Remotes.Text);
        }

        private void RemotesValidated(object sender, EventArgs e)
        {
            EnableLoadSshButton();
        }

        private void FillTagDropDown()
        {
            /// var tags = Module.GetTagHeads(GitModule.GetTagHeadsOption.OrderByCommitDateDescending); // comment out to sort by commit date
            var tags = Module.GetTagRefs(GitModule.GetTagRefsSortOrder.ByName)
                .Select(tag => tag.Name).ToList();
            tags.Insert(0, AllRefs);
            TagComboBox.DataSource = tags;
        }

        private void ForcePushBranchesCheckedChanged(object sender, EventArgs e)
        {
            ForcePushTags.Checked = ForcePushBranches.Checked;
        }

        private void ForcePushTagsCheckedChanged(object sender, EventArgs e)
        {
            ForcePushBranches.Checked = ForcePushTags.Checked;
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

            string remote = _NO_TRANSLATE_Remotes.Text.Trim();
            if (remote == "")
                return;

            var localHeads = Module.GetRefs(false, true);
            var remoteHeads = Module.GetRemoteRefs(remote, false, true);

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
                bool newAtRemote = remoteHeads.Any(h => h.Name == remoteName);
                row["New"] = newAtRemote ? _no.Text : _yes.Text;
                row["Push"] = newAtRemote;

                _branchTable.Rows.Add(row);
            }

            // Offer to delete all the left over remote branches.
            foreach (var remoteHead in remoteHeads)
            {
                GitRef head = remoteHead;
                if (localHeads.All(h => h.Name != head.Name))
                {
                    DataRow row = _branchTable.NewRow();
                    row["Local"] = null;
                    row["Remote"] = remoteHead.Name;
                    row["New"] = _no.Text;
                    row["Push"] = false;
                    row["Force"] = false;
                    row["Delete"] = false;
                    _branchTable.Rows.Add(row);
                }
            }
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
    }
}