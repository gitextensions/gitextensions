using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Repository;
using GitUI.Properties;
using ResourceManager.Translation;
using Settings = GitCommands.Settings;
using GitUI.Script;

namespace GitUI
{
    public delegate void ConfigureFormPull(FormPull formPull);

    public partial class FormPull : GitExtensionsForm
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

        private readonly TranslationString _cannotLoadPutty =
            new TranslationString("Cannot load SSH key. PuTTY is not configured properly.");

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
        #endregion

        private const string PuttyCaption = "PuTTY";

        private List<GitHead> _heads;
        public bool ErrorOccurred { get; private set; }
        private string branch;

        public FormPull()
        {
            InitializeComponent();
            Translate();

            UpdateRemotesList();

            branch = Settings.Module.GetSelectedBranch();
            string currentBranchRemote = Settings.Module.GetSetting(string.Format("branch.{0}.remote", branch));
            if (currentBranchRemote.IsNullOrEmpty() && _NO_TRANSLATE_Remotes.Items.Count >= 3)
            {
                IList<string> remotes = (IList<string>)_NO_TRANSLATE_Remotes.DataSource;
                int i = remotes.IndexOf("origin");
                _NO_TRANSLATE_Remotes.SelectedIndex = i >= 0 ? i : 1;
            }
            else
                _NO_TRANSLATE_Remotes.Text = currentBranchRemote;
            _NO_TRANSLATE_localBranch.Text = branch;

            Merge.Checked = Settings.PullMerge == Settings.PullAction.Merge;
            Rebase.Checked = Settings.PullMerge == Settings.PullAction.Rebase;
            Fetch.Checked = Settings.PullMerge == Settings.PullAction.Fetch;
            AutoStash.Checked = Settings.AutoStash;
            ErrorOccurred = false;
        }

        private void UpdateRemotesList()
        {
            IList<string> remotes = new List<string>(Settings.Module.GetRemotes());
            remotes.Insert(0, "[ All ]");
            _NO_TRANSLATE_Remotes.DataSource = remotes;
        }

        public DialogResult PullAndShowDialogWhenFailed()
        {
            return PullAndShowDialogWhenFailed(null);
        }
        public DialogResult PullAndShowDialogWhenFailed(IWin32Window owner)
        {
            if (PullChanges(owner))
                return DialogResult.OK;
            else
                return ShowDialog(owner);
        }

        private void BrowseSourceClick(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog { SelectedPath = PullSource.Text };
            if (dialog.ShowDialog(this) == DialogResult.OK)
                PullSource.Text = dialog.SelectedPath;
        }

        private void MergetoolClick(object sender, EventArgs e)
        {
            Settings.Module.RunGitRealCmd("mergetool");

            if (MessageBox.Show(this, _allMergeConflictSolvedQuestion.Text, _allMergeConflictSolvedQuestionCaption.Text,
                                MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;
            new FormCommit().ShowDialog(this);
        }

        private void BranchesDropDown(object sender, EventArgs e)
        {
            if ((PullFromUrl.Checked && string.IsNullOrEmpty(PullSource.Text)) &&
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
                    _heads = Settings.Module.GetHeads(false, true);
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
                    _heads = new List<GitHead>();
                    foreach (var head in Settings.Module.GetHeads(true, true))
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
            _heads.Insert(0, GitHead.NoHead);
            Branches.DataSource = _heads;


            Cursor.Current = Cursors.Default;
        }

        private void PullClick(object sender, EventArgs e)
        {
            if (PullChanges(this))
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private bool AskIfSubmodulesShouldBeInitialized()
        {
            return MessageBox.Show(this, _questionInitSubmodules.Text, _questionInitSubmodulesCaption.Text,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        public bool PullChanges(IWin32Window owner)
        {
            if (!ShouldPullChanges())
                return false;

            UpdateSettingsDuringPull();

            if (!IsCommitNotPushedToRemoteYet())
                return false;

            Repositories.RepositoryHistory.AddMostRecentRepository(PullSource.Text);

            var source = CalculateSource();

            ScriptManager.RunEventScripts(ScriptEvent.BeforePull);

            var stashed = CalculateStashedValue(owner);

            FormProcess process = CreateFormProcess(source);
            ShowProcessDialogBox(owner, source, process);

            return EvaluateProcessDialogResults(owner, process, stashed);
        }

        private bool ShouldPullChanges()
        {
            if (PullFromUrl.Checked && string.IsNullOrEmpty(PullSource.Text))
            {
                MessageBox.Show(this, _selectSourceDirectory.Text);
                return false;
            }
            if (PullFromRemote.Checked && string.IsNullOrEmpty(_NO_TRANSLATE_Remotes.Text) && !PullAll())
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

        private bool IsCommitNotPushedToRemoteYet()
        {
            //ask only if exists commit not pushed to remote yet
            if (Rebase.Checked && PullFromRemote.Checked && MergeCommitExists())
            {
                DialogResult dialogResult = MessageBox.Show(this, _areYouSureYouWantToRebaseMerge.Text,
                                                  _areYouSureYouWantToRebaseMergeCaption.Text,
                                                  MessageBoxButtons.YesNoCancel);
                if (dialogResult == DialogResult.Cancel)
                {
                    Close();
                    return false;
                }
                if (dialogResult != DialogResult.Yes)
                    return false;
            }
            return true;
        }

        private bool EvaluateProcessDialogResults(IWin32Window owner, FormProcess process, bool stashed)
        {
            try
            {
                if (EvaluateResultsBasedOnSettings(stashed, process))
                    return true;
            }
            finally
            {
                if (stashed)
                {
                    bool messageBoxResult =
                        MessageBox.Show(owner, _applyShashedItemsAgain.Text, _applyShashedItemsAgainCaption.Text,
                                        MessageBoxButtons.YesNo) == DialogResult.Yes;
                    if (ShouldStashPop(messageBoxResult, process, true))
                    {
                        new FormProcess("stash pop").ShowDialog(owner);
                        MergeConflictHandler.HandleMergeConflicts(owner);
                    }
                }

                ScriptManager.RunEventScripts(ScriptEvent.AfterPull);
            }

            return false;
        }

        private bool EvaluateResultsBasedOnSettings(bool stashed, FormProcess process)
        {
            if (!Settings.Module.InTheMiddleOfConflictedMerge() &&
                !Settings.Module.InTheMiddleOfRebase() &&
                (process != null && !process.ErrorOccurred()))
            {
                InitModules();
                return true;
            }

            // Rebase failed -> special 'rebase' merge conflict
            if (Rebase.Checked && Settings.Module.InTheMiddleOfRebase())
            {
                GitUICommands.Instance.StartRebaseDialog(null);
                if (!Settings.Module.InTheMiddleOfConflictedMerge() &&
                    !Settings.Module.InTheMiddleOfRebase())
                {
                    return true;
                }
            }
            else
            {
                MergeConflictHandler.HandleMergeConflicts(this);
                if (!Settings.Module.InTheMiddleOfConflictedMerge() &&
                    !Settings.Module.InTheMiddleOfRebase())
                {
                    return true;
                }
            }

            if (!AutoStash.Checked || !stashed || Settings.Module.InTheMiddleOfConflictedMerge() ||
                Settings.Module.InTheMiddleOfRebase())
            {
                return true;
            }
            return false;
        }

        private void ShowProcessDialogBox(IWin32Window owner, string source, FormProcess process)
        {
            if (process == null)
                return;
            if (!PullAll())
                process.Remote = source;
            process.ShowDialog(owner);
            ErrorOccurred = process.ErrorOccurred();
        }

        private bool CalculateStashedValue(IWin32Window owner)
        {
            if (!Fetch.Checked && AutoStash.Checked &&
                Settings.Module.GitStatus(UntrackedFilesMode.No, IgnoreSubmodulesMode.Default).Count > 0)
            {
                GitUICommands.Instance.Stash(owner);
                return true;
            }
            return false;
        }

        private static bool ShouldStashPop(bool messageBoxResult, FormProcess process, bool stashed)
        {
            return stashed && 
                   process != null && 
                   !process.ErrorOccurred() &&
                   !Settings.Module.InTheMiddleOfConflictedMerge() &&
                   !Settings.Module.InTheMiddleOfRebase() &&
                   messageBoxResult;
        }

        private void InitModules()
        {
            if (Fetch.Checked || !File.Exists(Settings.WorkingDir + ".gitmodules"))
                return;
            if (!IsSubmodulesIntialized() && AskIfSubmodulesShouldBeInitialized())
                GitUICommands.Instance.StartUpdateSubmodulesDialog(this);
        }

        private FormProcess CreateFormProcess(string source)
        {
            if (Fetch.Checked)
            {
                return new FormRemoteProcess(Settings.Module.FetchCmd(source, Branches.Text, null));
            }
            var localBranch = CalculateLocalBranch();

            if (Merge.Checked)
               return new FormRemoteProcess(Settings.Module.PullCmd(source, Branches.Text, localBranch, false));
            if (Rebase.Checked)
                return  new FormRemoteProcess(Settings.Module.PullCmd(source, Branches.Text, localBranch, true));
            return null;
        }

        private string CalculateLocalBranch()
        {
            string localBranch = Settings.Module.GetSelectedBranch();
            if (localBranch.Equals("(no branch)", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(Branches.Text))
                localBranch = null;
            return localBranch;
        }

        private string CalculateSource()
        {
            if (PullFromUrl.Checked)
                return PullSource.Text;
            LoadPuttyKey();
            return PullAll() ? "--all" : _NO_TRANSLATE_Remotes.Text;
        }

        private bool MergeCommitExists()
        {
            return Settings.Module.ExistsMergeCommit(CalculateRemoteBranchName(), branch);
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
            string remoteBranchName = Settings.Module.GetSetting("branch." + branch + ".merge");
            if (!remoteBranchName.IsNullOrEmpty())
                remoteBranchName = Settings.Module.RunGitCmd("name-rev --name-only \"" + remoteBranchName + "\"").Trim();
            return remoteBranchName;
        }

        private void UpdateSettingsDuringPull()
        {
            if (Merge.Checked)
                Settings.PullMerge = Settings.PullAction.Merge;
            if (Rebase.Checked)
                Settings.PullMerge = Settings.PullAction.Rebase;
            if (Fetch.Checked)
                Settings.PullMerge = Settings.PullAction.Fetch;

            Settings.AutoStash = AutoStash.Checked;
        }

        private bool IsSubmodulesIntialized()
        {
            // Fast submodules check
            var submodules = Settings.Module.GetSubmodulesNames();
            GitModule submodule = new GitModule();
            foreach (var submoduleName in submodules)
            {
                submodule.WorkingDir = Settings.Module.WorkingDir + submoduleName + Settings.PathSeparator.ToString();
                if (!submodule.ValidWorkingDir())
                    return false;
            }
            return true;
        }

        private void LoadPuttyKey()
        {
            if (!GitCommandHelpers.Plink())
                return;

            if (File.Exists(Settings.Pageant))
                Settings.Module.StartPageantForRemote(_NO_TRANSLATE_Remotes.Text);
            else
                MessageBox.Show(this, _cannotLoadPutty.Text, PuttyCaption);
        }

        private void FormPullLoad(object sender, EventArgs e)
        {
            _NO_TRANSLATE_Remotes.Select();

            Text = string.Format("Pull ({0})", Settings.WorkingDir);
        }

        private void FillPullSourceDropDown()
        {
            PullSource.DataSource = Repositories.RemoteRepositoryHistory.Repositories;
            PullSource.DisplayMember = "Path";
        }

        private void StashClick(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartStashDialog(this);
        }

        private void PullFromRemoteCheckedChanged(object sender, EventArgs e)
        {
            if (!PullFromRemote.Checked)
                return;

            ResetRemoteHeads();
            PullSource.Enabled = false;
            BrowseSource.Enabled = false;
            _NO_TRANSLATE_Remotes.Enabled = true;
            AddRemote.Enabled = true;

            Merge.Enabled = !PullAll();
            Rebase.Enabled = !PullAll();
        }

        private bool PullAll()
        {
            return _NO_TRANSLATE_Remotes.Text.Equals("[ All ]", StringComparison.InvariantCultureIgnoreCase);
        }

        public void SetForFetchAll()
        {
            _NO_TRANSLATE_Remotes.SelectedIndex = 0;
        }

        private void PullFromUrlCheckedChanged(object sender, EventArgs e)
        {
            if (!PullFromUrl.Checked)
                return;

            ResetRemoteHeads();
            PullSource.Enabled = true;
            BrowseSource.Enabled = true;
            _NO_TRANSLATE_Remotes.Enabled = false;
            AddRemote.Enabled = false;

            Merge.Enabled = true;
            Rebase.Enabled = true;

            FillPullSourceDropDown();
        }

        private bool bInternalUpdate;

        private void AddRemoteClick(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartRemotesDialog(this);

            bInternalUpdate = true;
            string text = _NO_TRANSLATE_Remotes.Text;
            UpdateRemotesList();
            _NO_TRANSLATE_Remotes.Text = text;
            bInternalUpdate = false;
        }

        private void MergeCheckedChanged(object sender, EventArgs e)
        {
            PullImage.BackgroundImage = Resources.merge;
        }

        private void RebaseCheckedChanged(object sender, EventArgs e)
        {
            PullImage.BackgroundImage = Resources.Rebase;
        }

        private void FetchCheckedChanged(object sender, EventArgs e)
        {
            PullImage.BackgroundImage = Resources.fetch;
        }

        private void PullSourceValidating(object sender, CancelEventArgs e)
        {
            ResetRemoteHeads();
        }

        private void Remotes_TextChanged(object sender, EventArgs e)
        {
            if (!bInternalUpdate)
                RemotesValidating(null, null);
        }

        private void RemotesValidating(object sender, CancelEventArgs e)
        {
            ResetRemoteHeads();

            Merge.Enabled = !PullAll();
            Rebase.Enabled = !PullAll();
            if (PullAll())
                Fetch.Checked = true;

        }

        private void ResetRemoteHeads()
        {
            if (PullAll())
            {
            }

            Branches.DataSource = null;
            _heads = null;
        }
    }
}
