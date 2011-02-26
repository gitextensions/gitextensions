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

namespace GitUI
{
    public partial class FormPull : GitExtensionsForm
    {
        private const string PuttyCaption = "PuTTY";

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

        private List<GitHead> _heads;

        public FormPull()
        {
            InitializeComponent();
            Translate();

            IList<string> remotes = new List<string>(GitCommandHelpers.GetRemotes());
            remotes.Insert(0, "[ All ]");
            Remotes.DataSource = remotes;
            
        }

        private void BrowseSourceClick(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog { SelectedPath = PullSource.Text };
            if (dialog.ShowDialog() == DialogResult.OK)
                PullSource.Text = dialog.SelectedPath;
        }

        private void MergetoolClick(object sender, EventArgs e)
        {
            GitCommandHelpers.RunRealCmd(Settings.GitCommand, "mergetool");

            if (MessageBox.Show(_allMergeConflictSolvedQuestion.Text, _allMergeConflictSolvedQuestionCaption.Text,
                                MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;
            new FormCommit().ShowDialog();
        }

        private void BranchesDropDown(object sender, EventArgs e)
        {
            if ((PullFromUrl.Checked && string.IsNullOrEmpty(PullSource.Text)) &&
                (PullFromRemote.Checked && string.IsNullOrEmpty(Remotes.Text)))
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
                    _heads = GitCommandHelpers.GetHeads(false, true);
                }
                else
                {
                    // The line below is the most reliable way to get a list containing
                    // all remote branches but it is also the slowest.
                    // Heads = GitCommands.GitCommands.GetRemoteHeads(Remotes.Text, false, true);

                    // The code below is a quick way to get a list containg all remote branches.
                    // It only returns the heads that are allready known to the repository. This
                    // doesn't return heads that are new on the server. This can be updated using
                    // update branch info in the manage remotes dialog.
                    _heads = new List<GitHead>();
                    foreach (var head in GitCommandHelpers.GetHeads(true, true))
                    {
                        if (!head.IsRemote ||
                            !head.Name.StartsWith(Remotes.Text, StringComparison.CurrentCultureIgnoreCase))
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
            if (PullFromUrl.Checked && string.IsNullOrEmpty(PullSource.Text))
            {
                MessageBox.Show(_selectSourceDirectory.Text);
                return;
            }
            if (PullFromRemote.Checked && string.IsNullOrEmpty(Remotes.Text) && !PullAll())
            {
                MessageBox.Show(_selectRemoteRepository.Text);
                return;
            }

            if (!Fetch.Checked && Branches.Text == "*")
            {
                MessageBox.Show(_fetchAllBranchesCanOnlyWithFetch.Text);
                return;
            }

            if (Merge.Checked)
                Settings.PullMerge = "merge";
            if (Rebase.Checked)
                Settings.PullMerge = "rebase";
            if (Fetch.Checked)
                Settings.PullMerge = "fetch";

            Settings.AutoStash = AutoStash.Checked;

            Repositories.RepositoryHistory.AddMostRecentRepository(PullSource.Text);

            string source;

            if (PullFromUrl.Checked)
                source = PullSource.Text;
            else
            {
                LoadPuttyKey();
                source = PullAll() ? "--all" : Remotes.Text;
            }

            var stashed = false;
            if (!Fetch.Checked && AutoStash.Checked && GitCommandHelpers.GitStatus(false).Count > 0)
            {
                new FormProcess("stash save").ShowDialog();
                stashed = true;
            }

            FormProcess process = null;
            if (Fetch.Checked)
            {
                process = new FormProcess(GitCommandHelpers.FetchCmd(source, Branches.Text, null));
            }
            else
            {
                string localBranch = GitCommandHelpers.GetSelectedBranch();
                if (localBranch.Equals("(no branch)", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(Branches.Text))
                    localBranch = null;

                if (Merge.Checked)
                    process = new FormProcess(GitCommandHelpers.PullCmd(source, Branches.Text, localBranch, false));
                else if (Rebase.Checked)
                    process = new FormProcess(GitCommandHelpers.PullCmd(source, Branches.Text, localBranch, true));
            }

            if (process != null)
                process.ShowDialog();

            if (!GitCommandHelpers.InTheMiddleOfConflictedMerge() &&
                !GitCommandHelpers.InTheMiddleOfRebase() &&
                (process != null && !process.ErrorOccurred()))
                Close();

            // Rebase failed -> special 'rebase' merge conflict
            if (Rebase.Checked && GitCommandHelpers.InTheMiddleOfRebase())
            {
                GitUICommands.Instance.StartRebaseDialog(null);
                if (!GitCommandHelpers.InTheMiddleOfConflictedMerge() &&
                    !GitCommandHelpers.InTheMiddleOfRebase())
                    Close();
            }
            else
            {
                MergeConflictHandler.HandleMergeConflicts();
                if (!GitCommandHelpers.InTheMiddleOfConflictedMerge() &&
                    !GitCommandHelpers.InTheMiddleOfRebase())
                    Close();
            }

            if (!AutoStash.Checked || !stashed || GitCommandHelpers.InTheMiddleOfConflictedMerge() ||
                GitCommandHelpers.InTheMiddleOfRebase())
                return;

            if (MessageBox.Show(_applyShashedItemsAgain.Text, _applyShashedItemsAgainCaption.Text,
                                MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;

            new FormProcess("stash pop").ShowDialog();

            MergeConflictHandler.HandleMergeConflicts();
        }

        private void LoadPuttyKey()
        {
            if (!GitCommandHelpers.Plink())
                return;

            if (File.Exists(Settings.Pageant))
                GitCommandHelpers.StartPageantForRemote(Remotes.Text);
            else
                MessageBox.Show(_cannotLoadPutty.Text, PuttyCaption);
        }

        private void FormPullLoad(object sender, EventArgs e)
        {
            Remotes.Select();

            string branch = GitCommandHelpers.GetSelectedBranch();
            Remotes.Text = GitCommandHelpers.GetSetting(string.Format("branch.{0}.remote", branch));

            _NO_TRANSLATE_localBranch.Text = branch;

            Text = string.Format("Pull ({0})", Settings.WorkingDir);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            Merge.Checked = Settings.PullMerge == "merge";
            Rebase.Checked = Settings.PullMerge == "rebase";
            Fetch.Checked = Settings.PullMerge == "fetch";

            AutoStash.Checked = Settings.AutoStash;
        }

        private void PullSourceDropDown(object sender, EventArgs e)
        {
            PullSource.DataSource = Repositories.RepositoryHistory.Repositories;
            PullSource.DisplayMember = "Path";
        }

        private void StashClick(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartStashDialog();
        }

        private void PullFromRemoteCheckedChanged(object sender, EventArgs e)
        {
            if (!PullFromRemote.Checked)
                return;

            ResetRemoteHeads();
            PullSource.Enabled = false;
            BrowseSource.Enabled = false;
            Remotes.Enabled = true;
            AddRemote.Enabled = true;

            Merge.Enabled = !PullAll();
            Rebase.Enabled = !PullAll();
        }

        private bool PullAll()
        {
            return Remotes.Text.Equals("[ All ]", StringComparison.InvariantCultureIgnoreCase);
        }

        private void PullFromUrlCheckedChanged(object sender, EventArgs e)
        {
            if (!PullFromUrl.Checked)
                return;

            ResetRemoteHeads();
            PullSource.Enabled = true;
            BrowseSource.Enabled = true;
            Remotes.Enabled = false;
            AddRemote.Enabled = false;

            Merge.Enabled = true;
            Rebase.Enabled = true;
        }

        private void AddRemoteClick(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartRemotesDialog();
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
