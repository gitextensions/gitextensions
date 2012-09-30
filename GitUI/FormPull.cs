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
using GitCommands.Config;

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

        public FormPull(string defaultRemoteBranch)
        {
            InitializeComponent();
            Translate();

            UpdateRemotesList();

            branch = GitModule.Current.GetSelectedBranch();
            string currentBranchRemote = GitModule.Current.GetSetting(string.Format("branch.{0}.remote", branch));
            if (currentBranchRemote.IsNullOrEmpty() && _NO_TRANSLATE_Remotes.Items.Count >= 3)
            {
                IList<string> remotes = (IList<string>)_NO_TRANSLATE_Remotes.DataSource;
                int i = remotes.IndexOf("origin");
                _NO_TRANSLATE_Remotes.SelectedIndex = i >= 0 ? i : 1;
            }
            else
                _NO_TRANSLATE_Remotes.Text = currentBranchRemote;
            _NO_TRANSLATE_localBranch.Text = branch;

            if (! string.IsNullOrEmpty(defaultRemoteBranch))
            {
                Branches.Text = defaultRemoteBranch;
            }

            Merge.Checked = Settings.PullMerge == Settings.PullAction.Merge;
            Rebase.Checked = Settings.PullMerge == Settings.PullAction.Rebase;
            Fetch.Checked = Settings.PullMerge == Settings.PullAction.Fetch;
            AutoStash.Checked = Settings.AutoStash;
            ErrorOccurred = false;
        }

        private void UpdateRemotesList()
        {
            IList<string> remotes = new List<string>(GitModule.Current.GetRemotes());
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

        private void BrowseSourceClick(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog { SelectedPath = PullSource.Text })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                    PullSource.Text = dialog.SelectedPath;
            }
        }

        private void MergetoolClick(object sender, EventArgs e)
        {
            GitModule.Current.RunGitRealCmd("mergetool");

            if (MessageBox.Show(this, _allMergeConflictSolvedQuestion.Text, _allMergeConflictSolvedQuestionCaption.Text,
                                MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;
            using (var frm = new FormCommit()) frm.ShowDialog(this);
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
                    _heads = GitModule.Current.GetHeads(false, true);
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
                    foreach (var head in GitModule.Current.GetHeads(true, true))
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

            Repositories.RepositoryHistory.AddMostRecentRepository(PullSource.Text);

            var source = CalculateSource();

            ScriptManager.RunEventScripts(ScriptEvent.BeforePull);

            var stashed = CalculateStashedValue(owner);

            using (FormProcess process = CreateFormProcess(source))
            {
                ShowProcessDialogBox(owner, source, process);

                return EvaluateProcessDialogResults(owner, process, stashed);
            }
        }

        private bool ShouldPullChanges()
        {
            if (PullFromUrl.Checked && string.IsNullOrEmpty(PullSource.Text))
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
                    bool messageBoxResult =
                        MessageBox.Show(owner, _applyShashedItemsAgain.Text, _applyShashedItemsAgainCaption.Text,
                                        MessageBoxButtons.YesNo) == DialogResult.Yes;
                    if (ShouldStashPop(messageBoxResult, process, true))
                    {
                        FormProcess.ShowDialog(owner, "stash pop");
                        MergeConflictHandler.HandleMergeConflicts(owner, false);
                    }
                }

                ScriptManager.RunEventScripts(ScriptEvent.AfterPull);
            }

            return DialogResult.No;
        }

        private bool EvaluateResultsBasedOnSettings(bool stashed, FormProcess process)
        {
            if (!GitModule.Current.InTheMiddleOfConflictedMerge() &&
                !GitModule.Current.InTheMiddleOfRebase() &&
                (process != null && !process.ErrorOccurred()))
            {
                InitModules();
                return true;
            }

            // Rebase failed -> special 'rebase' merge conflict
            if (Rebase.Checked && GitModule.Current.InTheMiddleOfRebase())
            {
                GitUICommands.Instance.StartRebaseDialog(null);
                if (!GitModule.Current.InTheMiddleOfConflictedMerge() &&
                    !GitModule.Current.InTheMiddleOfRebase())
                {
                    return true;
                }
            }
            else
            {
                MergeConflictHandler.HandleMergeConflicts(this);
                if (!GitModule.Current.InTheMiddleOfConflictedMerge() &&
                    !GitModule.Current.InTheMiddleOfRebase())
                {
                    return true;
                }
            }

            if (!AutoStash.Checked || !stashed || GitModule.Current.InTheMiddleOfConflictedMerge() ||
                GitModule.Current.InTheMiddleOfRebase())
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
                GitModule.Current.GitStatus(UntrackedFilesMode.No, IgnoreSubmodulesMode.Default).Count > 0)
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
                   !GitModule.Current.InTheMiddleOfConflictedMerge() &&
                   !GitModule.Current.InTheMiddleOfRebase() &&
                   messageBoxResult;
        }

        private void InitModules()
        {
            if (Fetch.Checked || !File.Exists(GitModule.CurrentWorkingDir + ".gitmodules"))
                return;
            if (!IsSubmodulesIntialized() && AskIfSubmodulesShouldBeInitialized())
                GitUICommands.Instance.StartUpdateSubmodulesDialog(this);
        }

        private FormProcess CreateFormProcess(string source)
        {
            if (Fetch.Checked)
            {
                return new FormRemoteProcess(GitModule.Current.FetchCmd(source, Branches.Text, null));
            }
            var localBranch = CalculateLocalBranch();

            if (Merge.Checked)
                return new FormRemoteProcess(GitModule.Current.PullCmd(source, Branches.Text, localBranch, false));
            if (Rebase.Checked)
                return new FormRemoteProcess(GitModule.Current.PullCmd(source, Branches.Text, localBranch, true));
            return null;
        }

        private string CalculateLocalBranch()
        {
            string localBranch = GitModule.Current.GetSelectedBranch();
            if (localBranch.Equals("(no branch)", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(Branches.Text))
                localBranch = null;
            return localBranch;
        }

        private string CalculateSource()
        {
            if (PullFromUrl.Checked)
                return PullSource.Text;
            LoadPuttyKey();
            return IsPullAll() ? "--all" : _NO_TRANSLATE_Remotes.Text;
        }

        private bool MergeCommitExists()
        {
            return GitModule.Current.ExistsMergeCommit(CalculateRemoteBranchName(), branch);
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
            string remoteBranchName = GitModule.Current.GetSetting(string.Format("branch.{0}.merge", branch));
            if (!remoteBranchName.IsNullOrEmpty())
                remoteBranchName = GitModule.Current.RunGitCmd(string.Format("name-rev --name-only \"{0}\"", remoteBranchName)).Trim();
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
            var submodules = GitModule.Current.GetSubmodulesLocalPathes();
            foreach (var submoduleName in submodules)
            {
                GitModule submodule = GitModule.Current.GetSubmodule(submoduleName);
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
                GitModule.Current.StartPageantForRemote(_NO_TRANSLATE_Remotes.Text);
            else
                MessageBox.Show(this, _cannotLoadPutty.Text, PuttyCaption);
        }

        private void FormPullLoad(object sender, EventArgs e)
        {
            _NO_TRANSLATE_Remotes.Select();

            Text = string.Format("Pull ({0})", GitModule.CurrentWorkingDir);
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
            {
                return;
            }

            ResetRemoteHeads();

            label3.Visible = true;
            labelRemoteUrl.Visible = true;
            PullSource.Enabled = false;
            BrowseSource.Enabled = false;
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
            _NO_TRANSLATE_Remotes.SelectedIndex = 0;
        }

        private void PullFromUrlCheckedChanged(object sender, EventArgs e)
        {
            if (!PullFromUrl.Checked)
            {
                return;
            }

            ResetRemoteHeads();

            label3.Visible = false;
            labelRemoteUrl.Visible = false;
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
            if (IsPullAll())
            {
                GitUICommands.Instance.StartRemotesDialog(this);
            }
            else
            {
                var selectedRemote = _NO_TRANSLATE_Remotes.Text;
                GitUICommands.Instance.StartRemotesDialog(this, selectedRemote);
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
            {
                RemotesValidating(null, null);
            }
        }

        private void RemotesValidating(object sender, CancelEventArgs e)
        {
            ResetRemoteHeads();

            // update the label text of the Remote Url
            labelRemoteUrl.Text = GitModule.Current.GetPathSetting(
                string.Format(SettingKeyString.RemoteUrl, _NO_TRANSLATE_Remotes.Text));
            label3.Visible = !string.IsNullOrEmpty(labelRemoteUrl.Text);

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
            if (IsPullAll())
            {
                // 2012-08-31: this if statement is empty. Why?
            }

            Branches.DataSource = null;
            _heads = null;
        }
    }
}
