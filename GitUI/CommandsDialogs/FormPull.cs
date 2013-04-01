using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Config;
using GitCommands.Repository;
using GitUI.Properties;
using GitUI.Script;
using ResourceManager.Translation;
using Settings = GitCommands.Settings;

namespace GitUI.CommandsDialogs
{
    public partial class FormPull : GitModuleForm
    {
        #region Translation
        private readonly TranslationString _areYouSureYouWantToRebaseMerge =
            new TranslationString("The current commit is a merge." + Environment.NewLine +
            //"." + Environment.NewLine +
                                "Are you sure you want to rebase this merge?");

        private readonly TranslationString _areYouSureYouWantToRebaseMergeCaption =
            new TranslationString("Rebase merge commit?");

        private readonly TranslationString _allMergeConflictSolvedQuestion =
            new TranslationString("Are all merge conflicts solved? Do you want to commit?");

        private readonly TranslationString _allMergeConflictSolvedQuestionCaption =
            new TranslationString("Conflicts solved");

        private readonly TranslationString _applyShashedItemsAgain =
            new TranslationString("Apply stashed items to working dir again?");

        private readonly TranslationString _applyShashedItemsAgainCaption =
            new TranslationString("Auto stash");

        private readonly TranslationString _fetchAllBranchesCanOnlyWithFetch =
            new TranslationString("You can only fetch all remote branches (*) without merge or rebase." +
                                  Environment.NewLine + "If you want to fetch all remote branches, choose fetch." +
                                  Environment.NewLine +
                                  "If you want to fetch and merge a branch, choose a specific branch.");

        private readonly TranslationString _selectRemoteRepository =
            new TranslationString("Please select a remote repository");

        private readonly TranslationString _selectSourceDirectory =
            new TranslationString("Please select a source directory");

        private readonly TranslationString _questionInitSubmodules =
            new TranslationString("The pulled has submodules configured." + Environment.NewLine +
                                   "Do you want to initialize the submodules?" + Environment.NewLine +
                                   "This will initialize and update all submodules recursive.");

        private readonly TranslationString _questionInitSubmodulesCaption =
            new TranslationString("Submodules");

        private readonly TranslationString _notOnBranchMainInstruction = new TranslationString("You are not working on a branch");
        private readonly TranslationString _notOnBranch = new TranslationString("You cannot \"pull\" when git head detached." +
                                  Environment.NewLine + "" + Environment.NewLine + "Do you want to continue?");
        private readonly TranslationString _notOnBranchButtons = new TranslationString("Checkout branch|Continue");
        private readonly TranslationString _notOnBranchCaption = new TranslationString("Not on a branch");

        private readonly TranslationString _dontShowAgain = new TranslationString("Don't show me this message again.");
        #endregion

        private IList<GitRef> _heads;
        public bool ErrorOccurred { get; private set; }
        private string branch;

        private FormPull()
            : this(null, null)
        { }

        public FormPull(GitUICommands aCommands, string defaultRemoteBranch)
            : base(aCommands)
        {
            InitializeComponent();
            Translate();

            helpImageDisplayUserControl1.Visible = !Settings.DontShowHelpImages;

            if (aCommands != null)
                Init();

            Merge.Checked = Settings.FormPullAction == Settings.PullAction.Merge;
            Rebase.Checked = Settings.FormPullAction == Settings.PullAction.Rebase;
            Fetch.Checked = Settings.FormPullAction == Settings.PullAction.Fetch;
            localBranch.Enabled = Fetch.Checked;
            AutoStash.Checked = Settings.AutoStash;
            ErrorOccurred = false;

            if (!string.IsNullOrEmpty(defaultRemoteBranch))
            {
                Branches.Text = defaultRemoteBranch;
            }
        }

        private void Init()
        {            
            UpdateRemotesList();

            branch = Module.GetSelectedBranch();
            string currentBranchRemote = Module.GetSetting(string.Format("branch.{0}.remote", branch));
            if (currentBranchRemote.IsNullOrEmpty() && _NO_TRANSLATE_Remotes.Items.Count >= 3)
            {
                IList<string> remotes = (IList<string>)_NO_TRANSLATE_Remotes.DataSource;
                int i = remotes.IndexOf("origin");
                _NO_TRANSLATE_Remotes.SelectedIndex = i >= 0 ? i : 1;
            }
            else
                _NO_TRANSLATE_Remotes.Text = currentBranchRemote;
            localBranch.Text = branch;
        }

        private void UpdateRemotesList()
        {
            IList<string> remotes = new List<string>(Module.GetRemotes());
            remotes.Insert(0, "[ All ]");
            _NO_TRANSLATE_Remotes.DataSource = remotes;
        }

        public DialogResult PullAndShowDialogWhenFailed()
        {
            return PullAndShowDialogWhenFailed(null);
        }

        public DialogResult PullAndShowDialogWhenFailed(IWin32Window owner)
        {
            DialogResult result = PullChanges(owner);

            if (result == DialogResult.No)
                result = ShowDialog(owner);
            else
                Close();

            return result;
        }

        private void MergetoolClick(object sender, EventArgs e)
        {
            Module.RunGitRealCmd("mergetool");

            if (MessageBox.Show(this, _allMergeConflictSolvedQuestion.Text, _allMergeConflictSolvedQuestionCaption.Text,
                                MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;
            UICommands.StartCommitDialog(this);
        }

        private void BranchesDropDown(object sender, EventArgs e)
        {
            if ((PullFromUrl.Checked && string.IsNullOrEmpty(comboBoxPullSource.Text)) &&
                (PullFromRemote.Checked && string.IsNullOrEmpty(_NO_TRANSLATE_Remotes.Text)))
            {
                Branches.DataSource = null;
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            LoadPuttyKey();

            if (_heads == null)
            {
                if (PullFromUrl.Checked)
                {
                    _heads = Module.GetHeads(false, true);
                }
                else
                {
                    // The line below is the most reliable way to get a list containing
                    // all remote branches but it is also the slowest.
                    // Heads = GitCommands.GitCommands.GetRemoteHeads(Remotes.Text, false, true);

                    // The code below is a quick way to get a list contains all remote branches.
                    // It only returns the heads that are already known to the repository. This
                    // doesn't return heads that are new on the server. This can be updated using
                    // update branch info in the manage remotes dialog.
                    _heads = new List<GitRef>();
                    foreach (var head in Module.GetHeads(true, true))
                    {
                        if (!head.IsRemote ||
                            !head.Name.StartsWith(_NO_TRANSLATE_Remotes.Text, StringComparison.CurrentCultureIgnoreCase))
                            continue;

                        _heads.Insert(0, head);
                    }
                }
            }
            Branches.DisplayMember = "LocalName";

            //_heads.Insert(0, GitHead.AllHeads); --> disable this because it is only for expert users
            _heads.Insert(0, GitRef.NoHead(Module));
            Branches.DataSource = _heads;


            Cursor.Current = Cursors.Default;
        }

        private void PullClick(object sender, EventArgs e)
        {
            DialogResult dialogResult = PullChanges(this);

            if (dialogResult != DialogResult.No)
            {
                DialogResult = dialogResult;
                Close();
            }
        }

        private bool AskIfSubmodulesShouldBeInitialized()
        {
            return MessageBox.Show(this, _questionInitSubmodules.Text, _questionInitSubmodulesCaption.Text,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        public DialogResult PullChanges(IWin32Window owner)
        {
            if (!ShouldPullChanges())
                return DialogResult.No;

            UpdateSettingsDuringPull();

            DialogResult dr = ShouldRebaseMergeCommit();
            if (dr != DialogResult.Yes)
                return dr;

            if (!Fetch.Checked && Branches.Text.IsNullOrWhiteSpace() && Module.IsDetachedHead())
            {
                int idx = PSTaskDialog.cTaskDialog.ShowCommandBox(owner,
                                                        _notOnBranchCaption.Text,
                                                        _notOnBranchMainInstruction.Text,
                                                        _notOnBranch.Text,
                                                        _notOnBranchButtons.Text,
                                                        true);
                switch (idx)
                {
                    case 0:
                        if (!UICommands.StartCheckoutBranch(owner, ""))
                            return DialogResult.Cancel;
                        break;
                    case -1:
                        return DialogResult.Cancel;
                }
            }

            if (PullFromUrl.Checked)
                Repositories.RepositoryHistory.AddMostRecentRepository(comboBoxPullSource.Text);

            var source = CalculateSource();

            ScriptManager.RunEventScripts(Module, ScriptEvent.BeforePull);

            var stashed = CalculateStashedValue(owner);

            using (FormProcess process = CreateFormProcess(source))
            {
                ShowProcessDialogBox(owner, source, process);

                return EvaluateProcessDialogResults(owner, process, stashed);
            }
        }

        private bool ShouldPullChanges()
        {
            if (PullFromUrl.Checked && string.IsNullOrEmpty(comboBoxPullSource.Text))
            {
                MessageBox.Show(this, _selectSourceDirectory.Text);
                return false;
            }
            if (PullFromRemote.Checked && string.IsNullOrEmpty(_NO_TRANSLATE_Remotes.Text) && !IsPullAll())
            {
                MessageBox.Show(this, _selectRemoteRepository.Text);
                return false;
            }

            if (!Fetch.Checked && Branches.Text == "*")
            {
                MessageBox.Show(this, _fetchAllBranchesCanOnlyWithFetch.Text);
                return false;
            }
            return true;
        }

        private DialogResult ShouldRebaseMergeCommit()
        {
            DialogResult dialogResult;

            //ask only if exists commit not pushed to remote yet
            if (Rebase.Checked && PullFromRemote.Checked && MergeCommitExists())
            {
                dialogResult = MessageBox.Show(this, _areYouSureYouWantToRebaseMerge.Text,
                                                  _areYouSureYouWantToRebaseMergeCaption.Text,
                                                  MessageBoxButtons.YesNoCancel);
            }
            else
                dialogResult = DialogResult.Yes;

            return dialogResult;
        }

        private DialogResult EvaluateProcessDialogResults(IWin32Window owner, FormProcess process, bool stashed)
        {
            try
            {
                if (EvaluateResultsBasedOnSettings(stashed, process))
                    return DialogResult.OK;
            }
            finally
            {
                if (stashed)
                {
                    bool? messageBoxResult = Settings.AutoPopStashAfterPull;
                    if (messageBoxResult == null)
                    {
                        DialogResult res = PSTaskDialog.cTaskDialog.MessageBox(
                            this,
                            _applyShashedItemsAgainCaption.Text,
                            "",
                            _applyShashedItemsAgain.Text,
                            "",
                            "",
                            _dontShowAgain.Text,
                            PSTaskDialog.eTaskDialogButtons.YesNo,
                            PSTaskDialog.eSysIcons.Question,
                            PSTaskDialog.eSysIcons.Question);
                        messageBoxResult = (res == DialogResult.Yes);
                        if (PSTaskDialog.cTaskDialog.VerificationChecked)
                            Settings.AutoPopStashAfterPull = messageBoxResult;
                    }
                    if (ShouldStashPop(messageBoxResult ?? false, process, true))
                    {
                        FormProcess.ShowDialog(owner, Module, "stash pop");
                        MergeConflictHandler.HandleMergeConflicts(UICommands, owner, false);
                    }
                }

                ScriptManager.RunEventScripts(Module, ScriptEvent.AfterPull);
            }

            return DialogResult.No;
        }

        private bool EvaluateResultsBasedOnSettings(bool stashed, FormProcess process)
        {
            if (!Module.InTheMiddleOfConflictedMerge() &&
                !Module.InTheMiddleOfRebase() &&
                (process != null && !process.ErrorOccurred()))
            {
                InitModules();
                return true;
            }

            // Rebase failed -> special 'rebase' merge conflict
            if (Rebase.Checked && Module.InTheMiddleOfRebase())
            {
                UICommands.StartRebaseDialog(null);
                if (!Module.InTheMiddleOfConflictedMerge() &&
                    !Module.InTheMiddleOfRebase())
                {
                    return true;
                }
            }
            else
            {
                MergeConflictHandler.HandleMergeConflicts(UICommands, this);
                if (!Module.InTheMiddleOfConflictedMerge() &&
                    !Module.InTheMiddleOfRebase())
                {
                    return true;
                }
            }

            if (!AutoStash.Checked || !stashed || Module.InTheMiddleOfConflictedMerge() ||
                Module.InTheMiddleOfRebase())
            {
                return true;
            }
            return false;
        }

        private void ShowProcessDialogBox(IWin32Window owner, string source, FormProcess process)
        {
            if (process == null)
                return;
            if (!IsPullAll())
                process.Remote = source;
            process.ShowDialog(owner);
            ErrorOccurred = process.ErrorOccurred();
        }

        private bool CalculateStashedValue(IWin32Window owner)
        {
            if (!Fetch.Checked && AutoStash.Checked &&
                Module.GitStatus(UntrackedFilesMode.No, IgnoreSubmodulesMode.Default).Count > 0)
            {
                UICommands.Stash(owner);
                return true;
            }
            return false;
        }

        private bool ShouldStashPop(bool messageBoxResult, FormProcess process, bool stashed)
        {
            return stashed &&
                   process != null &&
                   !process.ErrorOccurred() &&
                   !Module.InTheMiddleOfConflictedMerge() &&
                   !Module.InTheMiddleOfRebase() &&
                   messageBoxResult;
        }

        private void InitModules()
        {
            if (Fetch.Checked || !File.Exists(Module.WorkingDir + ".gitmodules"))
                return;
            if (!IsSubmodulesIntialized() && AskIfSubmodulesShouldBeInitialized())
                UICommands.StartUpdateSubmodulesDialog(this);
        }

        private FormProcess CreateFormProcess(string source)
        {
            var curLocalBranch = branch == localBranch.Text ? null : localBranch.Text;
            if (Fetch.Checked)
            {
                return new FormRemoteProcess(Module, Module.FetchCmd(source, Branches.Text, curLocalBranch, GetTagsArg()));
            }
            
            Debug.Assert(Merge.Checked || Rebase.Checked);

            curLocalBranch = CalculateLocalBranch();

            return new FormRemoteProcess(Module, Module.PullCmd(source, Branches.Text, curLocalBranch, Rebase.Checked, GetTagsArg()));            
        }

        private bool? GetTagsArg()
        { 
            return AllTags.Checked ? true : NoTags.Checked ? false : (bool?)null;
        }

        private string CalculateLocalBranch()
        {
            if (branch.Equals(GitModule.DetachedBranch, StringComparison.Ordinal) || string.IsNullOrEmpty(Branches.Text))
                branch = null;
            return branch;
        }

        private string CalculateSource()
        {
            if (PullFromUrl.Checked)
                return comboBoxPullSource.Text;
            LoadPuttyKey();
            return IsPullAll() ? "--all" : _NO_TRANSLATE_Remotes.Text;
        }

        private bool MergeCommitExists()
        {
            return Module.ExistsMergeCommit(CalculateRemoteBranchName(), branch);
        }

        private string CalculateRemoteBranchName()
        {
            string remoteBranchName = CalculateRemoteBranchNameBasedOnBranchesText();
            if (remoteBranchName.IsNullOrEmpty())
                return remoteBranchName;
            else
                return _NO_TRANSLATE_Remotes.Text + "/" + remoteBranchName;
        }

        private string CalculateRemoteBranchNameBasedOnBranchesText()
        {
            if (!Branches.Text.IsNullOrEmpty())
            {
                return Branches.Text;
            }
            string remoteBranchName = Module.GetSetting(string.Format("branch.{0}.merge", branch));
            if (!remoteBranchName.IsNullOrEmpty())
                remoteBranchName = Module.RunGitCmd(string.Format("name-rev --name-only \"{0}\"", remoteBranchName)).Trim();
            return remoteBranchName;
        }

        private void UpdateSettingsDuringPull()
        {
            if (Merge.Checked)
                Settings.FormPullAction = Settings.PullAction.Merge;
            if (Rebase.Checked)
                Settings.FormPullAction = Settings.PullAction.Rebase;
            if (Fetch.Checked)
                Settings.FormPullAction = Settings.PullAction.Fetch;

            Settings.AutoStash = AutoStash.Checked;
        }

        private bool IsSubmodulesIntialized()
        {
            // Fast submodules check
            var submodules = Module.GetSubmodulesLocalPathes();
            foreach (var submoduleName in submodules)
            {
                GitModule submodule = Module.GetSubmodule(submoduleName);
                if (!submodule.IsValidGitWorkingDir())
                    return false;
            }
            return true;
        }

        private void LoadPuttyKey()
        {
            if (!GitCommandHelpers.Plink())
                return;

            if (File.Exists(Settings.Pageant))
                Module.StartPageantForRemote(_NO_TRANSLATE_Remotes.Text);
            else
                MessageBoxes.PAgentNotFound(this);
        }

        private void FormPullLoad(object sender, EventArgs e)
        {
            _NO_TRANSLATE_Remotes.Select();

            Text = string.Format("Pull ({0})", Module.WorkingDir);
        }

        private void FillPullSourceDropDown()
        {
            comboBoxPullSource.DataSource = Repositories.RemoteRepositoryHistory.Repositories;
            comboBoxPullSource.DisplayMember = "Path";
        }

        private void StashClick(object sender, EventArgs e)
        {
            UICommands.StartStashDialog(this);
        }

        private void PullFromRemoteCheckedChanged(object sender, EventArgs e)
        {
            if (!PullFromRemote.Checked)
            {
                return;
            }

            ResetRemoteHeads();

            comboBoxPullSource.Enabled = false;
            folderBrowserButton1.Enabled = false;
            _NO_TRANSLATE_Remotes.Enabled = true;
            AddRemote.Enabled = true;

            Merge.Enabled = !IsPullAll();
            Rebase.Enabled = !IsPullAll();
        }

        private bool IsPullAll()
        {
            return _NO_TRANSLATE_Remotes.Text.Equals("[ All ]", StringComparison.InvariantCultureIgnoreCase);
        }

        public void SetForFetchAll()
        {
            _NO_TRANSLATE_Remotes.Text = "[ ALL ]";
        }

        private void PullFromUrlCheckedChanged(object sender, EventArgs e)
        {
            if (!PullFromUrl.Checked)
            {
                return;
            }

            ResetRemoteHeads();

            comboBoxPullSource.Enabled = true;
            folderBrowserButton1.Enabled = true;
            _NO_TRANSLATE_Remotes.Enabled = false;
            AddRemote.Enabled = false;

            Merge.Enabled = true;
            Rebase.Enabled = true;

            FillPullSourceDropDown();
        }

        private bool bInternalUpdate;

        private void AddRemoteClick(object sender, EventArgs e)
        {
            if (IsPullAll())
            {
                UICommands.StartRemotesDialog(this);
            }
            else
            {
                var selectedRemote = _NO_TRANSLATE_Remotes.Text;
                UICommands.StartRemotesDialog(this, selectedRemote);
            }

            bInternalUpdate = true;
            string origText = _NO_TRANSLATE_Remotes.Text;
            UpdateRemotesList();
            if (_NO_TRANSLATE_Remotes.Items.Contains(origText)) // else first item gets selected
            {
                _NO_TRANSLATE_Remotes.Text = origText;
            }
            bInternalUpdate = false;
        }

        private void MergeCheckedChanged(object sender, EventArgs e)
        {
            localBranch.Enabled = false;
            localBranch.Text = branch;
            helpImageDisplayUserControl1.Image1 = Resources.HelpPullMerge;
            helpImageDisplayUserControl1.Image2 = Resources.HelpPullMergeFastForward;
            helpImageDisplayUserControl1.IsOnHoverShowImage2 = true;
            AllTags.Enabled = false;
            if (AllTags.Checked)
                ReachableTags.Checked = true;
        }

        private void RebaseCheckedChanged(object sender, EventArgs e)
        {
            localBranch.Enabled = false;
            localBranch.Text = branch;
            helpImageDisplayUserControl1.Image1 = Resources.HelpPullRebase;
            helpImageDisplayUserControl1.IsOnHoverShowImage2 = false;
            AllTags.Enabled = false;
            if (AllTags.Checked)
                ReachableTags.Checked = true;
        }

        private void FetchCheckedChanged(object sender, EventArgs e)
        {
            helpImageDisplayUserControl1.Image1 = Resources.HelpPullFetch;
            helpImageDisplayUserControl1.IsOnHoverShowImage2 = false;
            localBranch.Enabled = true;
            AllTags.Enabled = true;
        }

        private void PullSourceValidating(object sender, CancelEventArgs e)
        {
            ResetRemoteHeads();
        }

        private void Remotes_TextChanged(object sender, EventArgs e)
        {
            if (!bInternalUpdate)
            {
                RemotesValidating(null, null);
            }
        }

        private void RemotesValidating(object sender, CancelEventArgs e)
        {
            ResetRemoteHeads();

            // update the text box of the Remote Url combobox to show the URL of selected remote
            comboBoxPullSource.Text = Module.GetPathSetting(
                string.Format(SettingKeyString.RemoteUrl, _NO_TRANSLATE_Remotes.Text));

            // update merge options radio buttons
            Merge.Enabled = !IsPullAll();
            Rebase.Enabled = !IsPullAll();
            if (IsPullAll())
            {
                Fetch.Checked = true;
            }
        }

        private void ResetRemoteHeads()
        {
            Branches.DataSource = null;
            _heads = null;
        }
    }
}
