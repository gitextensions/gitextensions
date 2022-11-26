using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using GitCommands;
using GitCommands.Config;
using GitCommands.Git;
using GitCommands.Remotes;
using GitCommands.UserRepositoryHistory;
using GitExtUtils;
using GitExtUtils.GitUI;
using GitExtUtils.GitUI.Theming;
using GitUI.HelperDialogs;
using GitUI.Infrastructure;
using GitUI.Properties;
using GitUI.Script;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormPull : GitExtensionsDialog
    {
        #region Mnemonics
        // Available: BIJKQVXYZ
        // A Fetch all tags
        // C Stash changes
        // D Prune remote branches and tags
        // E Rebase
        // F Do not merge, only fetch
        // G Manage remotes
        // H Auto stash
        // L Local branch
        // M Merge
        // N Fetch no tag
        // O Remote branch
        // P Prune remote branches
        // R Remote
        // S Solve conflicts
        // T Follow tagopt
        // U URL
        // W Download full history
        #endregion

        #region Translation
        private readonly TranslationString _areYouSureYouWantToRebaseMerge =
            new("The current commit is a merge." + Environment.NewLine +
                                  "Are you sure you want to rebase this merge?");

        private readonly TranslationString _areYouSureYouWantToRebaseMergeCaption =
            new("Rebase merge commit?");

        private readonly TranslationString _allMergeConflictSolvedQuestion =
            new("Are all merge conflicts solved? Do you want to commit?");

        private readonly TranslationString _allMergeConflictSolvedQuestionCaption =
            new("Conflicts solved");

        private readonly TranslationString _applyStashedItemsAgain =
            new("Apply stashed items to working directory again?");

        private readonly TranslationString _applyStashedItemsAgainCaption =
            new("Auto stash");

        private readonly TranslationString _fetchAllBranchesCanOnlyWithFetch =
            new("You can only fetch all remote branches (*) without merge or rebase." +
                                  Environment.NewLine + "If you want to fetch all remote branches, choose fetch." +
                                  Environment.NewLine +
                                  "If you want to fetch and merge a branch, choose a specific branch.");

        private readonly TranslationString _selectRemoteRepository =
            new("Please select a remote repository");

        private readonly TranslationString _selectSourceDirectory =
            new("Please select a source directory");

        private readonly TranslationString _questionInitSubmodules =
            new("The pulled has submodules configured." + Environment.NewLine +
                                   "Do you want to initialize the submodules?" + Environment.NewLine +
                                   "This will initialize and update all submodules recursive.");

        private readonly TranslationString _questionInitSubmodulesCaption =
            new("Submodules");

        private readonly TranslationString _notOnBranch = new("You cannot \"pull\" when git head detached." +
                                  Environment.NewLine + Environment.NewLine + "Do you want to continue?");

        private readonly TranslationString _noRemoteBranch = new("You didn't specify a remote branch");
        private readonly TranslationString _noRemoteBranchMainInstruction = new(
            "You asked to pull from the remote '{0}'," + Environment.NewLine +
            "but did not specify a remote branch." + Environment.NewLine +
            "Because this is not the default configured remote for your local branch," + Environment.NewLine +
            "you must specify a remote branch.");
        private readonly TranslationString _noRemoteBranchForFetchMainInstruction = new(
            "You asked to fetch from the remote '{0}'," + Environment.NewLine +
            "but did not specify a remote branch." + Environment.NewLine +
            "Because this is not the current branch, you must specify a remote branch.");
        private readonly TranslationString _noRemoteBranchButton = new("Pull from {0}");
        private readonly TranslationString _noRemoteBranchForFetchButton = new("Fetch from {0}");
        private readonly TranslationString _noRemoteBranchCaption = new("Remote branch not specified");

        private readonly TranslationString _pruneBranchesCaption = new("Pull was rejected");
        private readonly TranslationString _pruneBranchesMainInstruction = new("Remote branch no longer exist");
        private readonly TranslationString _pruneBranchesBranch =
            new("Do you want to delete all stale remote-tracking branches?");

        private readonly TranslationString _pruneFromCaption = new("Prune remote branches from {0}");

        private readonly TranslationString _hoverShowImageLabelText = new("Hover to see scenario when fast forward is possible.");
        private readonly TranslationString _formTitlePull = new("Pull ({0})");
        private readonly TranslationString _formTitleFetch = new("Fetch ({0})");
        private readonly TranslationString _buttonPull = new("&Pull");
        private readonly TranslationString _buttonFetch = new("&Fetch");

        private readonly TranslationString _pullFetchPruneAllConfirmation = new("Warning! The fetch with prune will remove all the remote-tracking references which no longer exist on remotes. Do you want to proceed?");
        #endregion

        private const string AllRemotes = "[ All ]";

        private readonly IConfigFileRemoteSettingsManager _remotesManager;
        private readonly IFullPathResolver _fullPathResolver;
        private readonly string _branch;

        private List<IGitRef>? _heads;
        private bool _bInternalUpdate;

        public bool ErrorOccurred { get; private set; }

        [Obsolete("For VS designer and translation test only. Do not remove.")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private FormPull()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            InitializeComponent();
        }

        public FormPull(GitUICommands commands, string? defaultRemoteBranch, string? defaultRemote, AppSettings.PullAction pullAction)
            : base(commands, enablePositionRestore: false)
        {
            InitializeComponent();
            InitializeComplete();

            PanelLeftImage.Visible = !AppSettings.DontShowHelpImages;
            PanelLeftImage.IsOnHoverShowImage2NoticeText = _hoverShowImageLabelText.Text;

            _remotesManager = new ConfigFileRemoteSettingsManager(() => Module);
            _branch = Module.GetSelectedBranch();
            BindRemotesDropDown(defaultRemote);

            if (pullAction == AppSettings.PullAction.None)
            {
                pullAction = AppSettings.DefaultPullAction;
            }

            switch (pullAction)
            {
                case AppSettings.PullAction.None:
                    Merge.Checked = true;
                    break;
                case AppSettings.PullAction.Merge:
                    Merge.Checked = true;
                    Prune.Enabled = false;
                    PruneTags.Enabled = false;
                    break;
                case AppSettings.PullAction.Rebase:
                    Rebase.Checked = true;
                    Prune.Enabled = false;
                    PruneTags.Enabled = false;
                    break;
                case AppSettings.PullAction.Fetch:
                    Fetch.Checked = true;
                    Prune.Enabled = true;
                    PruneTags.Enabled = true;
                    break;
                case AppSettings.PullAction.FetchAll:
                    Fetch.Checked = true;
                    _NO_TRANSLATE_Remotes.Text = AllRemotes;
                    break;
                case AppSettings.PullAction.FetchPruneAll:
                    Fetch.Checked = true;
                    Prune.Checked = true;
                    PruneTags.Checked = false;

                    if (string.IsNullOrEmpty(defaultRemote))
                    {
                        _NO_TRANSLATE_Remotes.Text = AllRemotes;
                    }
                    else
                    {
                        _NO_TRANSLATE_Remotes.Text = defaultRemote;
                    }

                    break;
                case AppSettings.PullAction.Default:
                    Debug.Assert(false, "pullAction is not a valid action");
                    break;
            }

            localBranch.Enabled = Fetch.Checked;
            AutoStash.Checked = AppSettings.AutoStash;

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

        private void BindRemotesDropDown(string? selectedRemoteName)
        {
            // refresh registered git remotes
            var remotes = _remotesManager.LoadRemotes(false);

            _NO_TRANSLATE_Remotes.Sorted = false;
            _NO_TRANSLATE_Remotes.DataSource = new[] { new ConfigFileRemote { Name = AllRemotes } }.Union(remotes).ToList();
            _NO_TRANSLATE_Remotes.DisplayMember = nameof(ConfigFileRemote.Name);
            _NO_TRANSLATE_Remotes.SelectedIndex = -1;
            _NO_TRANSLATE_Remotes.ResizeDropDownWidth(AppSettings.BranchDropDownMinWidth, AppSettings.BranchDropDownMaxWidth);

            if (string.IsNullOrEmpty(selectedRemoteName))
            {
                selectedRemoteName = Module.GetSetting(string.Format(SettingKeyString.BranchRemote, _branch));
            }

            var currentBranchRemote = remotes.FirstOrDefault(x => StringComparer.OrdinalIgnoreCase.Equals(x.Name, selectedRemoteName));
            if (currentBranchRemote is not null)
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

        public DialogResult PullAndShowDialogWhenFailed(IWin32Window? owner, string? remote, AppSettings.PullAction pullAction)
        {
            // Special case for "Fetch and prune" and "Fetch and prune all" to make sure user confirms the action.
            if (pullAction == AppSettings.PullAction.FetchPruneAll)
            {
                string messageBoxTitle;
                if (string.IsNullOrEmpty(remote))
                {
                    messageBoxTitle = string.Format(_pruneFromCaption.Text, AllRemotes);
                }
                else
                {
                    messageBoxTitle = string.Format(_pruneFromCaption.Text, remote);
                }

                bool isActionConfirmed = AppSettings.DontConfirmFetchAndPruneAll
                                         || MessageBox.Show(
                                             owner,
                                             _pullFetchPruneAllConfirmation.Text,
                                             messageBoxTitle,
                                             MessageBoxButtons.YesNo) == DialogResult.Yes;

                if (!isActionConfirmed)
                {
                    return DialogResult.Cancel;
                }
            }

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
            Module.RunMergeTool();

            if (MessageBox.Show(this, _allMergeConflictSolvedQuestion.Text, _allMergeConflictSolvedQuestionCaption.Text,
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
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

                if (_heads is null)
                {
                    if (PullFromUrl.Checked)
                    {
                        _heads = Module.GetRefs(RefsFilter.Heads).ToList();
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
                        foreach (var head in Module.GetRefs(RefsFilter.Remotes))
                        {
                            if (!head.Name.StartsWith(_NO_TRANSLATE_Remotes.Text, StringComparison.CurrentCultureIgnoreCase))
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

                Branches.ResizeDropDownWidth(AppSettings.BranchDropDownMinWidth, AppSettings.BranchDropDownMaxWidth);
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

        public DialogResult PullChanges(IWin32Window? owner)
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

            if (!Fetch.Checked && string.IsNullOrWhiteSpace(Branches.Text) && Module.IsDetachedHead())
            {
                TaskDialogPage page = new()
                {
                    Text = _notOnBranch.Text,
                    Heading = TranslatedStrings.ErrorInstructionNotOnBranch,
                    Caption = TranslatedStrings.ErrorCaptionNotOnBranch,
                    Buttons = { TaskDialogButton.Cancel },
                    Icon = TaskDialogIcon.Error,
                    AllowCancel = true,
                    SizeToContent = true
                };
                TaskDialogCommandLinkButton btnCheckout = new(TranslatedStrings.ButtonCheckoutBranch);
                TaskDialogCommandLinkButton btnContinue = new(TranslatedStrings.ButtonContinue);
                page.Buttons.Add(btnCheckout);
                page.Buttons.Add(btnContinue);

                TaskDialogButton result = TaskDialog.ShowDialog(owner?.Handle ?? default, page);
                if (result == TaskDialogButton.Cancel)
                {
                    return DialogResult.Cancel;
                }

                if (result == btnCheckout)
                {
                    if (!UICommands.StartCheckoutBranch(owner))
                    {
                        return DialogResult.Cancel;
                    }
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

            if (!executeBeforeScripts())
            {
                return DialogResult.No;
            }

            var stashed = CalculateStashedValue(owner);

            using var form = CreateFormProcess(source, curLocalBranch, curRemoteBranch);
            if (!IsPullAll())
            {
                form.Remote = source;
            }

            form.ShowDialog(owner);
            ErrorOccurred = form.ErrorOccurred();

            bool executeScripts = false;
            try
            {
                bool aborted = form.DialogResult == DialogResult.Abort;
                executeScripts = !aborted && !ErrorOccurred;

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
                        executeScripts |= CheckMergeConflictsOnError();
                    }
                }
            }
            finally
            {
                if (stashed)
                {
                    PopStash();
                }

                if (executeScripts)
                {
                    executeAfterScripts();
                }
            }

            return DialogResult.OK;

            bool ShouldPullChanges()
            {
                if (PullFromUrl.Checked && string.IsNullOrEmpty(comboBoxPullSource.Text))
                {
                    MessageBox.Show(this, _selectSourceDirectory.Text, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                if (PullFromRemote.Checked && string.IsNullOrEmpty(_NO_TRANSLATE_Remotes.Text) && !IsPullAll())
                {
                    MessageBox.Show(this, _selectRemoteRepository.Text, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                if (!Fetch.Checked && Branches.Text == "*")
                {
                    MessageBox.Show(this, _fetchAllBranchesCanOnlyWithFetch.Text, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                return true;
            }

            string CalculateSource()
            {
                if (PullFromUrl.Checked)
                {
                    return comboBoxPullSource.Text;
                }

                LoadPuttyKey();
                return IsPullAll() ? "--all" : _NO_TRANSLATE_Remotes.Text;
            }

            bool InitModules()
            {
                if (!File.Exists(_fullPathResolver.Resolve(".gitmodules")))
                {
                    return false;
                }

                if (!IsSubmodulesInitialized())
                {
                    // If the "Update submodules on checkout" option is `true`, initialize and update
                    // all submodules. If it's `false` don't initialize/update the submodules. If it's
                    // indeterminate, ask the user what they'd like to do.
                    if (AppSettings.UpdateSubmodulesOnCheckout ?? AppSettings.DontConfirmUpdateSubmodulesOnCheckout ?? AskIfSubmodulesShouldBeInitialized())
                    {
                        UICommands.StartUpdateSubmodulesDialog(this);
                    }

                    return true;
                }

                return false;

                bool IsSubmodulesInitialized()
                {
                    // Fast submodules check
                    return Module.GetSubmodulesLocalPaths()
                        .Select(submoduleName => Module.GetSubmodule(submoduleName))
                        .All(submodule => submodule.IsValidGitWorkingDir());
                }

                bool AskIfSubmodulesShouldBeInitialized()
                {
                    return MessageBox.Show(this, _questionInitSubmodules.Text, _questionInitSubmodulesCaption.Text,
                               MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
                }
            }

            bool CheckMergeConflictsOnError()
            {
                // Rebase failed -> special 'rebase' merge conflict
                if (Rebase.Checked && Module.InTheMiddleOfRebase())
                {
                    return UICommands.StartTheContinueRebaseDialog(owner);
                }
                else if (Module.InTheMiddleOfAction())
                {
                    return MergeConflictHandler.HandleMergeConflicts(UICommands, owner);
                }

                return false;
            }

            void PopStash()
            {
                if (ErrorOccurred || Module.InTheMiddleOfAction())
                {
                    return;
                }

                bool? messageBoxResult = AppSettings.AutoPopStashAfterPull;
                if (messageBoxResult is null)
                {
                    TaskDialogPage page = new()
                    {
                        Text = _applyStashedItemsAgain.Text,
                        Caption = _applyStashedItemsAgainCaption.Text,
                        Buttons = { TaskDialogButton.Yes, TaskDialogButton.No },
                        Icon = TaskDialogIcon.Information,
                        Verification = new TaskDialogVerificationCheckBox
                        {
                            Text = TranslatedStrings.DontShowAgain
                        },
                        SizeToContent = true
                    };

                    messageBoxResult = TaskDialog.ShowDialog(owner?.Handle ?? IntPtr.Zero, page) == TaskDialogButton.Yes;

                    if (page.Verification.Checked)
                    {
                        AppSettings.AutoPopStashAfterPull = messageBoxResult;
                    }
                }

                if ((bool)messageBoxResult)
                {
                    UICommands.StashPop(owner);
                }
            }

            bool executeBeforeScripts()
            {
                // Request to pull/merge in addition to the fetch
                if (!Fetch.Checked)
                {
                    bool success = ScriptManager.RunEventScripts(this, ScriptEvent.BeforePull);
                    if (!success)
                    {
                        return false;
                    }
                }

                return ScriptManager.RunEventScripts(this, ScriptEvent.BeforeFetch);
            }

            void executeAfterScripts()
            {
                ScriptManager.RunEventScripts(this, ScriptEvent.AfterFetch);

                // Request to pull/merge in addition to the fetch
                if (!Fetch.Checked)
                {
                    ScriptManager.RunEventScripts(this, ScriptEvent.AfterPull);
                }
            }
        }

        private void UpdateSettingsDuringPull()
        {
            AppSettings.FormPullAction =
                Merge.Checked ? AppSettings.PullAction.Merge :
                Rebase.Checked ? AppSettings.PullAction.Rebase :
                Fetch.Checked ? AppSettings.FormPullAction = AppSettings.PullAction.Fetch : AppSettings.PullAction.Default;

            AppSettings.AutoStash = AutoStash.Checked;
        }

        private DialogResult ShouldRebaseMergeCommit()
        {
            DialogResult dialogResult;

            // ask only if exists commit not pushed to remote yet
            if (Rebase.Checked && PullFromRemote.Checked && MergeCommitExists())
            {
                dialogResult = MessageBox.Show(this, _areYouSureYouWantToRebaseMerge.Text,
                                                  _areYouSureYouWantToRebaseMergeCaption.Text,
                                                  MessageBoxButtons.YesNoCancel,
                                                  MessageBoxIcon.Warning,
                                                  MessageBoxDefaultButton.Button2);
            }
            else
            {
                dialogResult = DialogResult.Yes;
            }

            return dialogResult;
        }

        private bool CalculateStashedValue(IWin32Window? owner)
        {
            if (!Fetch.Checked && AutoStash.Checked && !Module.IsBareRepository() &&
                Module.GitStatus(UntrackedFilesMode.No, IgnoreSubmodulesMode.All).Count > 0)
            {
                UICommands.StashSave(owner, AppSettings.IncludeUntrackedFilesInAutoStash);
                return true;
            }

            return false;
        }

        private FormProcess CreateFormProcess(string source, string? curLocalBranch, string? curRemoteBranch)
        {
            if (Fetch.Checked)
            {
                return new FormRemoteProcess(UICommands, Module.FetchCmd(source, curRemoteBranch, curLocalBranch, GetTagsArg(), Unshallow.Checked, Prune.Checked, PruneTags.Checked));
            }

            Debug.Assert(Merge.Checked || Rebase.Checked, "Merge.Checked || Rebase.Checked");

            return new FormRemoteProcess(UICommands, Module.PullCmd(source, curRemoteBranch, Rebase.Checked, GetTagsArg(), Unshallow.Checked))
            {
                HandleOnExitCallback = HandlePullOnExit
            };

            bool? GetTagsArg()
            {
                return AllTags.Checked ? true : NoTags.Checked ? false : (bool?)null;
            }

            bool HandlePullOnExit(ref bool isError, FormProcess form)
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
                Regex isRefRemoved = new(@"Your configuration specifies to .* the ref '.*'[\r]?\nfrom the remote, but no such ref was fetched.");

                if (isRefRemoved.IsMatch(form.GetOutputString()))
                {
                    TaskDialogPage page = new()
                    {
                        Text = _pruneBranchesBranch.Text,
                        Caption = _pruneBranchesCaption.Text,
                        Heading = _pruneBranchesMainInstruction.Text,
                        Buttons = { TaskDialogButton.Yes, TaskDialogButton.No, TaskDialogButton.Cancel },
                        Icon = TaskDialogIcon.Information,
                        DefaultButton = TaskDialogButton.No,
                        SizeToContent = true
                    };
                    TaskDialogButton result = TaskDialog.ShowDialog(form.Handle, page);

                    if (result == TaskDialogButton.Yes)
                    {
                        string remote = _NO_TRANSLATE_Remotes.Text;
                        string pruneCmd = "remote prune " + remote;
                        using FormRemoteProcess formPrune = new(UICommands, pruneCmd)
                        {
                            Remote = remote,
                            Text = string.Format(_pruneFromCaption.Text, remote)
                        };
                        formPrune.ShowDialog(form);
                    }
                }

                return false;
            }
        }

        private bool CalculateLocalBranch(string remote, out string? curLocalBranch, out string? curRemoteBranch)
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

            Lazy<string> currentBranchRemote = new(() => Module.GetSetting(string.Format(SettingKeyString.BranchRemote, localBranch.Text)));

            if (_branch == localBranch.Text)
            {
                if (remote == currentBranchRemote.Value || string.IsNullOrEmpty(currentBranchRemote.Value))
                {
                    curLocalBranch = string.IsNullOrEmpty(Branches.Text) ? null : _branch;
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

            if (string.IsNullOrEmpty(Branches.Text) && !string.IsNullOrEmpty(curLocalBranch)
                && remote != currentBranchRemote.Value && !Fetch.Checked)
            {
                TaskDialogPage page = new()
                {
                    Text = string.Format(_noRemoteBranchMainInstruction.Text, remote),
                    Caption = _noRemoteBranchCaption.Text,
                    Heading = _noRemoteBranch.Text,
                    Buttons = { TaskDialogButton.Cancel },
                    Icon = TaskDialogIcon.Information,
                    Verification = new TaskDialogVerificationCheckBox
                    {
                        Text = TranslatedStrings.DontShowAgain
                    },
                    AllowCancel = false,
                    SizeToContent = true
                };

                TaskDialogCommandLinkButton btnPullFrom = new(string.Format(_noRemoteBranchButton.Text, remote + "/" + curLocalBranch));
                page.Buttons.Add(btnPullFrom);

                TaskDialogButton result = TaskDialog.ShowDialog(Handle, page);

                if (result == btnPullFrom)
                {
                    curRemoteBranch = curLocalBranch;
                    return true;
                }

                return false;
            }

            if (string.IsNullOrEmpty(Branches.Text) && !string.IsNullOrEmpty(curLocalBranch) && Fetch.Checked)
            {
                // if local branch eq to current branch and remote branch is not specified
                // then run fetch with no refspec
                if (_branch == curLocalBranch)
                {
                    curLocalBranch = null;
                    return true;
                }

                TaskDialogPage page = new()
                {
                    Text = string.Format(_noRemoteBranchForFetchMainInstruction.Text, remote),
                    Caption = _noRemoteBranchCaption.Text,
                    Heading = _noRemoteBranch.Text,
                    Buttons = { TaskDialogButton.Cancel },
                    Icon = TaskDialogIcon.Information,
                    AllowCancel = false,
                    SizeToContent = true
                };

                TaskDialogCommandLinkButton btnPullFrom = new(string.Format(_noRemoteBranchForFetchButton.Text, remote + "/" + curLocalBranch));
                page.Buttons.Add(btnPullFrom);

                TaskDialogButton result = TaskDialog.ShowDialog(Handle, page);
                if (result == TaskDialogButton.Cancel)
                {
                    return false;
                }

                if (result == btnPullFrom)
                {
                    curRemoteBranch = curLocalBranch;
                    return true;
                }
            }

            return true;
        }

        private bool MergeCommitExists()
        {
            return Module.ExistsMergeCommit(CalculateRemoteBranchName(), _branch);
        }

        private string CalculateRemoteBranchName()
        {
            string remoteBranchName = CalculateRemoteBranchNameBasedOnBranchesText();
            if (string.IsNullOrEmpty(remoteBranchName))
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
            if (!string.IsNullOrEmpty(Branches.Text))
            {
                return Branches.Text;
            }

            string remoteBranchName = Module.GetSetting(string.Format("branch.{0}.merge", _branch));
            if (!string.IsNullOrEmpty(remoteBranchName))
            {
                GitArgumentBuilder args = new("name-rev")
                {
                    "--name-only",
                    remoteBranchName.QuoteNE()
                };
                remoteBranchName = Module.GitExecutable.GetOutput(args).Trim();
            }

            return remoteBranchName;
        }

        private void LoadPuttyKey()
        {
            if (!GitSshHelpers.IsPlink)
            {
                return;
            }

            HashSet<string> files = new(new PathEqualityComparer());
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
                PuttyHelpers.StartPageantIfConfigured(() => sshKeyFile);
            }

            return;

            IEnumerable<string> GetSelectedRemotes()
            {
                if (PullFromUrl.Checked)
                {
                    yield break;
                }

                if (IsPullAll())
                {
                    foreach (var remote in (IEnumerable<ConfigFileRemote>)_NO_TRANSLATE_Remotes.DataSource)
                    {
                        if (!string.IsNullOrWhiteSpace(remote.Name) && remote.Name != AllRemotes)
                        {
                            yield return remote.Name;
                        }
                    }
                }
                else if (!string.IsNullOrWhiteSpace(_NO_TRANSLATE_Remotes.Text))
                {
                    yield return _NO_TRANSLATE_Remotes.Text;
                }
            }
        }

        private void FormPullLoad(object sender, EventArgs e)
        {
            _NO_TRANSLATE_Remotes.Select();

            UpdateFormTitleAndButton();
        }

        private void UpdateFormTitleAndButton()
        {
            var format = Fetch.Checked
                ? _formTitleFetch.Text
                : _formTitlePull.Text;

            Text = string.Format(format, PathUtil.GetDisplayPath(Module.WorkingDir));

            Pull.Text = Fetch.Checked ? _buttonFetch.Text : _buttonPull.Text;
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

        private void PullFromUrlCheckedChanged(object sender, EventArgs e)
        {
            if (!PullFromUrl.Checked)
            {
                return;
            }

            ResetRemoteHeads();

            comboBoxPullSource.Enabled = true;
            folderBrowserButton1.Enabled = true;
            _NO_TRANSLATE_Remotes.SelectionLength = 0;
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
            PanelLeftImage.Image1 = DpiUtil.Scale(Images.HelpPullMerge.AdaptLightness());
            PanelLeftImage.Image2 = DpiUtil.Scale(Images.HelpPullMergeFastForward.AdaptLightness());
            PanelLeftImage.IsOnHoverShowImage2 = true;
            AllTags.Enabled = false;
            Prune.Enabled = false;
            PruneTags.Enabled = false;

            UpdateFormTitleAndButton();
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
            PanelLeftImage.Image1 = DpiUtil.Scale(Images.HelpPullRebase.AdaptLightness());
            PanelLeftImage.IsOnHoverShowImage2 = false;
            AllTags.Enabled = false;
            Prune.Enabled = false;
            PruneTags.Enabled = false;

            UpdateFormTitleAndButton();
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

            PanelLeftImage.Image1 = DpiUtil.Scale(Images.HelpPullFetch.AdaptLightness());
            PanelLeftImage.IsOnHoverShowImage2 = false;
            AllTags.Enabled = true;
            Prune.Enabled = true;
            PruneTags.Enabled = true;

            UpdateFormTitleAndButton();
        }

        private void PullSourceValidating(object sender, CancelEventArgs e)
        {
            ResetRemoteHeads();
        }

        private void Remotes_TextChanged(object sender, EventArgs e)
        {
            if (!_bInternalUpdate)
            {
                RemotesValidating(this, null!);
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
            if (_branch != localBranch.Text.Trim() && string.IsNullOrWhiteSpace(Branches.Text))
            {
                Branches.Text = localBranch.Text;
            }
        }

        private void Prune_CheckedChanged(object sender, EventArgs e)
        {
            PruneTags.Checked = Prune.Checked && PruneTags.Checked;
        }

        private void PruneTags_CheckedChanged(object sender, EventArgs e)
        {
            Prune.Checked = Prune.Checked || PruneTags.Checked;
            AllTags.Checked = AllTags.Checked || PruneTags.Checked;
        }

        internal TestAccessor GetTestAccessor() => new(this);

        internal readonly struct TestAccessor
        {
            private readonly FormPull _form;

            public TestAccessor(FormPull form)
            {
                _form = form;
            }

            public string Title => _form.Text;
            public string FetchTitleText => _form._formTitleFetch.Text;
            public string PullTitleText => _form._formTitlePull.Text;
            public RadioButton Merge => _form.Merge;
            public RadioButton Rebase => _form.Rebase;
            public RadioButton Fetch => _form.Fetch;
            public CheckBox AutoStash => _form.AutoStash;
            public CheckBox Prune => _form.Prune;
            public CheckBox PruneTags => _form.PruneTags;
            public ComboBox Remotes => _form._NO_TRANSLATE_Remotes;
            public TextBox LocalBranch => _form.localBranch;

            public void UpdateSettingsDuringPull() => _form.UpdateSettingsDuringPull();
        }
    }
}
