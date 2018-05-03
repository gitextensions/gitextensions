using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Config;
using GitCommands.Git;
using GitCommands.Remote;
using GitCommands.UserRepositoryHistory;
using GitUI.Properties;
using GitUI.Script;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class FormPull : GitModuleForm
    {
        #region Translation
        private readonly TranslationString _areYouSureYouWantToRebaseMerge =
            new TranslationString("The current commit is a merge." + Environment.NewLine +
                                  "Are you sure you want to rebase this merge?");

        private readonly TranslationString _areYouSureYouWantToRebaseMergeCaption =
            new TranslationString("Rebase merge commit?");

        private readonly TranslationString _allMergeConflictSolvedQuestion =
            new TranslationString("Are all merge conflicts solved? Do you want to commit?");

        private readonly TranslationString _allMergeConflictSolvedQuestionCaption =
            new TranslationString("Conflicts solved");

        private readonly TranslationString _applyShashedItemsAgain =
            new TranslationString("Apply stashed items to working directory again?");

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

        private readonly TranslationString _noRemoteBranch = new TranslationString("You didn't specify a remote branch");
        private readonly TranslationString _noRemoteBranchMainInstruction = new TranslationString(
            "You asked to pull from the remote '{0}'," + Environment.NewLine +
            "but did not specify a remote branch." + Environment.NewLine +
            "Because this is not the default configured remote for your local branch," + Environment.NewLine +
            "you must specify a remote branch.");
        private readonly TranslationString _noRemoteBranchForFetchMainInstruction = new TranslationString(
            "You asked to fetch from the remote '{0}'," + Environment.NewLine +
            "but did not specify a remote branch." + Environment.NewLine +
            "Because this is not the current branch, you must specify a remote branch.");
        private readonly TranslationString _noRemoteBranchButtons = new TranslationString("Pull from {0}|Cancel");
        private readonly TranslationString _noRemoteBranchForFetchButtons = new TranslationString("Fetch from {0}|Cancel");
        private readonly TranslationString _noRemoteBranchCaption = new TranslationString("Remote branch not specified");

        private readonly TranslationString _dontShowAgain = new TranslationString("Don't show me this message again.");

        private readonly TranslationString _pruneBranchesCaption = new TranslationString("Pull was rejected");
        private readonly TranslationString _pruneBranchesMainInstruction = new TranslationString("Remote branch no longer exist");
        private readonly TranslationString _pruneBranchesBranch =
            new TranslationString("Do you want to delete all stale remote-tracking branches?");
        private readonly TranslationString _pruneBranchesButtons = new TranslationString("Deletes stale branches|Cancel");

        private readonly TranslationString _pruneFromCaption = new TranslationString("Prune remote branches from {0}");

        private readonly TranslationString _hoverShowImageLabelText = new TranslationString("Hover to see scenario when fast forward is possible.");
        private readonly TranslationString _formTitlePull = new TranslationString("Pull ({0})");
        private readonly TranslationString _formTitleFetch = new TranslationString("Fetch ({0})");
        #endregion

        public bool ErrorOccurred { get; private set; }
        private List<IGitRef> _heads;
        private string _branch;
        private bool _bInternalUpdate;
        private const string AllRemotes = "[ All ]";
        private readonly IGitRemoteManager _remoteManager;
        private readonly IFullPathResolver _fullPathResolver;

        private FormPull()
            : this(null, null, null)
        {
        }

        public FormPull(GitUICommands commands, string defaultRemoteBranch, string defaultRemote)
            : base(commands)
        {
            InitializeComponent();
            Translate();

            if (commands == null)
            {
                return;
            }

            helpImageDisplayUserControl1.Visible = !AppSettings.DontShowHelpImages;
            helpImageDisplayUserControl1.IsOnHoverShowImage2NoticeText = _hoverShowImageLabelText.Text;

            _remoteManager = new GitRemoteManager(() => Module);
            Init(defaultRemote);

            Merge.Checked = AppSettings.FormPullAction == AppSettings.PullAction.Merge;
            Rebase.Checked = AppSettings.FormPullAction == AppSettings.PullAction.Rebase;
            Fetch.Checked = AppSettings.FormPullAction == AppSettings.PullAction.Fetch;
            localBranch.Enabled = Fetch.Checked;
            AutoStash.Checked = AppSettings.AutoStash;
            Prune.Enabled = AppSettings.FormPullAction == AppSettings.PullAction.Merge || AppSettings.FormPullAction == AppSettings.PullAction.Fetch;

            ErrorOccurred = false;

            if (!string.IsNullOrEmpty(defaultRemoteBranch))
            {
                Branches.Text = defaultRemoteBranch;
            }

            // If this repo is shallow, show an option to Unshallow
            // Detect by presence of the shallow file, not 100% sure it's the best way, but it's created upon shallow cloning and removed upon unshallowing
            bool isRepoShallow = File.Exists(commands.Module.ResolveGitInternalPath("shallow"));
            if (isRepoShallow)
            {
                Unshallow.Visible = true;
            }

            _fullPathResolver = new FullPathResolver(() => Module.WorkingDir);
        }

        private void Init(string defaultRemote)
        {
            _branch = Module.GetSelectedBranch();
            BindRemotesDropDown(defaultRemote);
        }

        private void BindRemotesDropDown(string selectedRemoteName)
        {
            // refresh registered git remotes
            var remotes = _remoteManager.LoadRemotes(false);

            _NO_TRANSLATE_Remotes.Sorted = false;
            _NO_TRANSLATE_Remotes.DataSource = new[] { new GitRemote { Name = AllRemotes } }.Union(remotes).ToList();
            _NO_TRANSLATE_Remotes.DisplayMember = nameof(GitRemote.Name);
            _NO_TRANSLATE_Remotes.SelectedIndex = -1;
            _NO_TRANSLATE_Remotes.ResizeComboBoxDropDownWidth(AppSettings.BranchDropDownMinWidth, AppSettings.BranchDropDownMaxWidth);

            if (selectedRemoteName.IsNullOrEmpty())
            {
                selectedRemoteName = Module.GetSetting(string.Format(SettingKeyString.BranchRemote, _branch));
            }

            var currentBranchRemote = remotes.FirstOrDefault(x => x.Name.Equals(selectedRemoteName, StringComparison.OrdinalIgnoreCase));
            if (currentBranchRemote != null)
            {
                _NO_TRANSLATE_Remotes.SelectedItem = currentBranchRemote;
            }
            else if (remotes.Any())
            {
                // we couldn't find the default assigned remote for the selected branch
                // it is usually gets mapped via FormRemotes -> "default pull behavior" tab
                // so pick the default user remote
                _NO_TRANSLATE_Remotes.SelectedIndex = 1;
            }
            else
            {
                _NO_TRANSLATE_Remotes.SelectedIndex = 0;
            }
        }

        public DialogResult PullAndShowDialogWhenFailed(IWin32Window owner)
        {
            DialogResult result = PullChanges(owner);

            if (result == DialogResult.No)
            {
                result = ShowDialog(owner);
            }
            else
            {
                Close();
            }

            return result;
        }

        private void MergetoolClick(object sender, EventArgs e)
        {
            Module.RunExternalCmdShowConsole(AppSettings.GitCommand, "mergetool");

            if (MessageBox.Show(this, _allMergeConflictSolvedQuestion.Text, _allMergeConflictSolvedQuestionCaption.Text,
                                MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return;
            }

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

            using (WaitCursorScope.Enter())
            {
                LoadPuttyKey();

                if (_heads == null)
                {
                    if (PullFromUrl.Checked)
                    {
                        _heads = Module.GetRefs(false, true).ToList();
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
                        _heads = new List<IGitRef>();
                        foreach (var head in Module.GetRefs(true, true))
                        {
                            if (!head.IsRemote ||
                                !head.Name.StartsWith(_NO_TRANSLATE_Remotes.Text, StringComparison.CurrentCultureIgnoreCase))
                            {
                                continue;
                            }

                            _heads.Insert(0, head);
                        }
                    }
                }

                Branches.DisplayMember = nameof(IGitRef.LocalName);

                ////_heads.Insert(0, GitHead.AllHeads); --> disable this because it is only for expert users
                _heads.Insert(0, GitRef.NoHead(Module));
                Branches.DataSource = _heads;

                ComboBoxHelper.ResizeComboBoxDropDownWidth(Branches, AppSettings.BranchDropDownMinWidth, AppSettings.BranchDropDownMaxWidth);
            }
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

        private bool InitModules()
        {
            if (!File.Exists(_fullPathResolver.Resolve(".gitmodules")))
            {
                return false;
            }

            if (!IsSubmodulesInitialized())
            {
                if (AskIfSubmodulesShouldBeInitialized())
                {
                    UICommands.StartUpdateSubmodulesDialog(this);
                }

                return true;
            }

            return false;
        }

        private void CheckMergeConflictsOnError(IWin32Window owner)
        {
            // Rebase failed -> special 'rebase' merge conflict
            if (Rebase.Checked && Module.InTheMiddleOfRebase())
            {
                UICommands.StartTheContinueRebaseDialog(owner);
            }
            else if (Module.InTheMiddleOfAction())
            {
                MergeConflictHandler.HandleMergeConflicts(UICommands, owner);
            }
        }

        private void PopStash(IWin32Window owner)
        {
            if (ErrorOccurred || Module.InTheMiddleOfAction())
            {
                return;
            }

            bool? messageBoxResult = AppSettings.AutoPopStashAfterPull;
            if (messageBoxResult == null)
            {
                DialogResult res = PSTaskDialog.cTaskDialog.MessageBox(
                    owner,
                    _applyShashedItemsAgainCaption.Text,
                    "",
                    _applyShashedItemsAgain.Text,
                    "",
                    "",
                    _dontShowAgain.Text,
                    PSTaskDialog.eTaskDialogButtons.YesNo,
                    PSTaskDialog.eSysIcons.Question,
                    PSTaskDialog.eSysIcons.Question);
                messageBoxResult = res == DialogResult.Yes;
                if (PSTaskDialog.cTaskDialog.VerificationChecked)
                {
                    AppSettings.AutoPopStashAfterPull = messageBoxResult;
                }
            }

            if ((bool)messageBoxResult)
            {
                UICommands.StashPop(owner);
            }
        }

        public DialogResult PullChanges(IWin32Window owner)
        {
            if (!ShouldPullChanges())
            {
                return DialogResult.No;
            }

            UpdateSettingsDuringPull();

            DialogResult dr = ShouldRebaseMergeCommit();
            if (dr != DialogResult.Yes)
            {
                return dr;
            }

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
                        if (!UICommands.StartCheckoutBranch(owner, new[] { "" }))
                        {
                            return DialogResult.Cancel;
                        }

                        break;
                    case -1:
                        return DialogResult.Cancel;
                }
            }

            if (PullFromUrl.Checked && Directory.Exists(comboBoxPullSource.Text))
            {
                var path = comboBoxPullSource.Text;
                ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Remotes.AddAsMostRecentAsync(path));
            }

            var source = CalculateSource();

            if (!CalculateLocalBranch(source, out var curLocalBranch, out var curRemoteBranch))
            {
                return DialogResult.No;
            }

            ScriptManager.RunEventScripts(this, ScriptEvent.BeforePull);

            var stashed = CalculateStashedValue(owner);

            using (FormProcess process = CreateFormProcess(source, curLocalBranch, curRemoteBranch))
            {
                ShowProcessDialogBox(owner, source, process);

                try
                {
                    bool aborted = process != null && process.DialogResult == DialogResult.Abort;
                    if (!aborted && !Fetch.Checked)
                    {
                        if (!ErrorOccurred)
                        {
                            if (!InitModules())
                            {
                                UICommands.UpdateSubmodules(owner);
                            }
                        }
                        else
                        {
                            CheckMergeConflictsOnError(owner);
                        }
                    }
                }
                finally
                {
                    if (stashed)
                    {
                        PopStash(owner);
                    }

                    ScriptManager.RunEventScripts(this, ScriptEvent.AfterPull);
                }

                return DialogResult.OK;
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

            if (!Fetch.Checked && Branches.Text == @"*")
            {
                MessageBox.Show(this, _fetchAllBranchesCanOnlyWithFetch.Text);
                return false;
            }

            return true;
        }

        private DialogResult ShouldRebaseMergeCommit()
        {
            DialogResult dialogResult;

            // ask only if exists commit not pushed to remote yet
            if (Rebase.Checked && PullFromRemote.Checked && MergeCommitExists())
            {
                dialogResult = MessageBox.Show(this, _areYouSureYouWantToRebaseMerge.Text,
                                                  _areYouSureYouWantToRebaseMergeCaption.Text,
                                                  MessageBoxButtons.YesNoCancel);
            }
            else
            {
                dialogResult = DialogResult.Yes;
            }

            return dialogResult;
        }

        private void ShowProcessDialogBox(IWin32Window owner, string source, FormProcess process)
        {
            if (process == null)
            {
                return;
            }

            if (!IsPullAll())
            {
                process.Remote = source;
            }

            process.ShowDialog(owner);
            ErrorOccurred = process.ErrorOccurred();
        }

        private bool CalculateStashedValue(IWin32Window owner)
        {
            if (!Fetch.Checked && AutoStash.Checked && !Module.IsBareRepository() &&
                Module.GitStatus(UntrackedFilesMode.No, IgnoreSubmodulesMode.All).Count > 0)
            {
                UICommands.StashSave(owner, AppSettings.IncludeUntrackedFilesInAutoStash);
                return true;
            }

            return false;
        }

        private bool IsSubmodulesInitialized()
        {
            // Fast submodules check
            return Module.GetSubmodulesLocalPaths()
                .Select(submoduleName => Module.GetSubmodule(submoduleName))
                .All(submodule => submodule.IsValidGitWorkingDir());
        }

        private FormProcess CreateFormProcess(string source, string curLocalBranch, string curRemoteBranch)
        {
            if (Fetch.Checked)
            {
                return new FormRemoteProcess(Module, Module.FetchCmd(source, curRemoteBranch, curLocalBranch, GetTagsArg(), Unshallow.Checked, Prune.Checked));
            }

            Debug.Assert(Merge.Checked || Rebase.Checked, "Merge.Checked || Rebase.Checked");

            return new FormRemoteProcess(Module, Module.PullCmd(source, curRemoteBranch, Rebase.Checked, GetTagsArg(), Unshallow.Checked, Prune.Checked))
            {
                HandleOnExitCallback = HandlePullOnExit
            };
        }

        private bool HandlePullOnExit(ref bool isError, FormProcess form)
        {
            if (!isError)
            {
                return false;
            }

            if (!PullFromRemote.Checked || string.IsNullOrEmpty(_NO_TRANSLATE_Remotes.Text))
            {
                return false;
            }

            // auto pull only if current branch was rejected
            Regex isRefRemoved = new Regex(@"Your configuration specifies to .* the ref '.*'[\r]?\nfrom the remote, but no such ref was fetched.");

            if (isRefRemoved.IsMatch(form.GetOutputString()))
            {
                int idx = PSTaskDialog.cTaskDialog.ShowCommandBox(form,
                                _pruneBranchesCaption.Text,
                                _pruneBranchesMainInstruction.Text,
                                _pruneBranchesBranch.Text,
                                _pruneBranchesButtons.Text,
                                true);
                if (idx == 0)
                {
                    string remote = _NO_TRANSLATE_Remotes.Text;
                    string pruneCmd = "remote prune " + remote;
                    using (var formPrune = new FormRemoteProcess(Module, pruneCmd)
                    {
                        Remote = remote,
                        Text = string.Format(_pruneFromCaption.Text, remote)
                    })
                    {
                        formPrune.ShowDialog(form);
                    }
                }
            }

            return false;
        }

        private bool? GetTagsArg()
        {
            return AllTags.Checked ? true : NoTags.Checked ? false : (bool?)null;
        }

        private bool CalculateLocalBranch(string remote, out string curLocalBranch, out string curRemoteBranch)
        {
            if (IsPullAll())
            {
                curLocalBranch = null;
                curRemoteBranch = null;
                return true;
            }

            curRemoteBranch = Branches.Text;

            if (DetachedHeadParser.IsDetachedHead(_branch))
            {
                curLocalBranch = null;
                return true;
            }

            var currentBranchRemote = new Lazy<string>(() => Module.GetSetting(string.Format(SettingKeyString.BranchRemote, localBranch.Text)));

            if (_branch == localBranch.Text)
            {
                if (remote == currentBranchRemote.Value || currentBranchRemote.Value.IsNullOrEmpty())
                {
                    curLocalBranch = Branches.Text.IsNullOrEmpty() ? null : _branch;
                }
                else
                {
                    curLocalBranch = localBranch.Text;
                }
            }
            else
            {
                curLocalBranch = localBranch.Text;
            }

            if (Branches.Text.IsNullOrEmpty() && !curLocalBranch.IsNullOrEmpty()
                && remote != currentBranchRemote.Value && !Fetch.Checked)
            {
                int idx = PSTaskDialog.cTaskDialog.ShowCommandBox(this,
                                                        _noRemoteBranchCaption.Text,
                                                        _noRemoteBranch.Text,
                                                        string.Format(_noRemoteBranchMainInstruction.Text, remote),
                                                        string.Format(_noRemoteBranchButtons.Text, remote + "/" + curLocalBranch),
                                                        false);
                switch (idx)
                {
                    case 0:
                        curRemoteBranch = curLocalBranch;
                        return true;
                    default:
                        return false;
                }
            }

            if (Branches.Text.IsNullOrEmpty() && !curLocalBranch.IsNullOrEmpty()
                && Fetch.Checked)
            {
                // if local branch eq to current branch and remote branch is not specified
                // then run fetch with no refspec
                if (_branch == curLocalBranch)
                {
                    curLocalBranch = null;
                    return true;
                }

                int idx = PSTaskDialog.cTaskDialog.ShowCommandBox(this,
                                                        _noRemoteBranchCaption.Text,
                                                        _noRemoteBranch.Text,
                                                        string.Format(_noRemoteBranchForFetchMainInstruction.Text, remote),
                                                        string.Format(_noRemoteBranchForFetchButtons.Text, remote + "/" + curLocalBranch),
                                                        false);
                switch (idx)
                {
                    case 0:
                        curRemoteBranch = curLocalBranch;
                        return true;
                    default:
                        return false;
                }
            }

            return true;
        }

        private string CalculateSource()
        {
            if (PullFromUrl.Checked)
            {
                return comboBoxPullSource.Text;
            }

            LoadPuttyKey();
            return IsPullAll() ? "--all" : _NO_TRANSLATE_Remotes.Text;
        }

        private bool MergeCommitExists()
        {
            return Module.ExistsMergeCommit(CalculateRemoteBranchName(), _branch);
        }

        private string CalculateRemoteBranchName()
        {
            string remoteBranchName = CalculateRemoteBranchNameBasedOnBranchesText();
            if (remoteBranchName.IsNullOrEmpty())
            {
                return remoteBranchName;
            }
            else
            {
                return _NO_TRANSLATE_Remotes.Text + "/" + remoteBranchName;
            }
        }

        private string CalculateRemoteBranchNameBasedOnBranchesText()
        {
            if (!Branches.Text.IsNullOrEmpty())
            {
                return Branches.Text;
            }

            string remoteBranchName = Module.GetSetting(string.Format("branch.{0}.merge", _branch));
            if (!remoteBranchName.IsNullOrEmpty())
            {
                remoteBranchName = Module.RunGitCmd(string.Format("name-rev --name-only \"{0}\"", remoteBranchName)).Trim();
            }

            return remoteBranchName;
        }

        private void UpdateSettingsDuringPull()
        {
            if (Merge.Checked)
            {
                AppSettings.FormPullAction = AppSettings.PullAction.Merge;
            }

            if (Rebase.Checked)
            {
                AppSettings.FormPullAction = AppSettings.PullAction.Rebase;
            }

            if (Fetch.Checked)
            {
                AppSettings.FormPullAction = AppSettings.PullAction.Fetch;
            }

            AppSettings.AutoStash = AutoStash.Checked;
        }

        private IEnumerable<string> GetSelectedRemotes()
        {
            if (PullFromUrl.Checked)
            {
                yield break;
            }

            if (IsPullAll())
            {
                IEnumerable<GitRemote> remotes = (IEnumerable<GitRemote>)_NO_TRANSLATE_Remotes.DataSource;
                foreach (var r in remotes)
                {
                    if (!r.Name.IsNullOrWhiteSpace() && r.Name != AllRemotes)
                    {
                        yield return r.Name;
                    }
                }
            }
            else
            {
                if (!_NO_TRANSLATE_Remotes.Text.IsNullOrWhiteSpace())
                {
                    yield return _NO_TRANSLATE_Remotes.Text;
                }
            }
        }

        private void LoadPuttyKey()
        {
            if (!GitCommandHelpers.Plink())
            {
                return;
            }

            if (File.Exists(AppSettings.Pageant))
            {
                HashSet<string> files = new HashSet<string>(new PathEqualityComparer());
                foreach (var remote in GetSelectedRemotes())
                {
                    var sshKeyFile = Module.GetPuttyKeyFileForRemote(remote);
                    if (!string.IsNullOrEmpty(sshKeyFile))
                    {
                        files.Add(sshKeyFile);
                    }
                }

                foreach (var sshKeyFile in files)
                {
                    if (File.Exists(sshKeyFile))
                    {
                        GitModule.StartPageantWithKey(sshKeyFile);
                    }
                }
            }
            else
            {
                MessageBoxes.PAgentNotFound(this);
            }
        }

        private void FormPullLoad(object sender, EventArgs e)
        {
            _NO_TRANSLATE_Remotes.Select();

            FillFormTitle();
        }

        private void FillFormTitle()
        {
            var format = Fetch.Checked
                ? _formTitleFetch.Text
                : _formTitlePull.Text;

            Text = string.Format(format, Module.WorkingDir);
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
            return _NO_TRANSLATE_Remotes.Text.Equals(AllRemotes, StringComparison.InvariantCultureIgnoreCase);
        }

        public void SetForFetchAll()
        {
            _NO_TRANSLATE_Remotes.Text = AllRemotes;
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

            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                var repositoryHistory = await RepositoryHistoryManager.Remotes.LoadRecentHistoryAsync();

                await this.SwitchToMainThreadAsync();
                string prevUrl = comboBoxPullSource.Text;
                comboBoxPullSource.DataSource = repositoryHistory;
                comboBoxPullSource.DisplayMember = nameof(Repository.Path);
                comboBoxPullSource.Text = prevUrl;
            });
        }

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

            _bInternalUpdate = true;
            string origText = _NO_TRANSLATE_Remotes.Text;
            BindRemotesDropDown(origText);
            _bInternalUpdate = false;
        }

        private void MergeCheckedChanged(object sender, EventArgs e)
        {
            if (!Merge.Checked)
            {
                return;
            }

            localBranch.Enabled = false;
            localBranch.Text = _branch;
            helpImageDisplayUserControl1.Image1 = Resources.HelpPullMerge;
            helpImageDisplayUserControl1.Image2 = Resources.HelpPullMergeFastForward;
            helpImageDisplayUserControl1.IsOnHoverShowImage2 = true;
            AllTags.Enabled = false;
            Prune.Enabled = true;
            if (AllTags.Checked)
            {
                ReachableTags.Checked = true;
            }
        }

        private void RebaseCheckedChanged(object sender, EventArgs e)
        {
            if (!Rebase.Checked)
            {
                return;
            }

            localBranch.Enabled = false;
            localBranch.Text = _branch;
            helpImageDisplayUserControl1.Image1 = Resources.HelpPullRebase;
            helpImageDisplayUserControl1.IsOnHoverShowImage2 = false;
            AllTags.Enabled = false;
            Prune.Enabled = false;
            if (AllTags.Checked)
            {
                ReachableTags.Checked = true;
            }
        }

        private void FetchCheckedChanged(object sender, EventArgs e)
        {
            if (!Fetch.Checked)
            {
                return;
            }

            localBranch.Enabled = true;
            localBranch.Text = string.Empty;
            helpImageDisplayUserControl1.Image1 = Resources.HelpPullFetch;
            helpImageDisplayUserControl1.IsOnHoverShowImage2 = false;
            AllTags.Enabled = true;
            Prune.Enabled = true;
            FillFormTitle();
        }

        private void PullSourceValidating(object sender, CancelEventArgs e)
        {
            ResetRemoteHeads();
        }

        private void Remotes_TextChanged(object sender, EventArgs e)
        {
            if (!_bInternalUpdate)
            {
                RemotesValidating(null, null);
            }
        }

        private void RemotesValidating(object sender, CancelEventArgs e)
        {
            ResetRemoteHeads();

            // update the text box of the Remote Url combobox to show the URL of selected remote
            comboBoxPullSource.Text = Module.GetSetting(
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

        private void localBranch_Leave(object sender, EventArgs e)
        {
            if (_branch != localBranch.Text.Trim() && Branches.Text.IsNullOrWhiteSpace())
            {
                Branches.Text = localBranch.Text;
            }
        }
    }
}
