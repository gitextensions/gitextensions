using Avalonia.Controls;
using GitCommands;
using GitCommands.Git;
using GitCommands.Settings;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using GitExtUtils;
using GitUI.Compat;
using GitUI.HelperDialogs;
using GitUIPluginInterfaces;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs;

/// <summary>Form to merge a branch into the current branch.</summary>
// Twin of GitUI/CommandsDialogs/FormMergeBranch.cs. Before/after merge event scripts remain
// deferred with the ScriptsEngine tranche; the merge operation itself is fully functional.
public partial class FormMergeBranch : GitModuleForm
{
    private readonly TranslationString _formMergeBranchHoverShowImageLabelText = new("Hover to see scenario when fast forward is possible.");
    private readonly string? _defaultBranch;
    private readonly ICommitMessageManager _commitMessageManager = null!;
    private readonly WinFormsShims.Control _commitMessageManagerOwner = null!;

    public FormMergeBranch()
    {
        InitializeComponent();
        InitializeStaticContent();
        InitializeComplete();
    }

    /// <summary>Initializes <see cref="FormMergeBranch"/>.</summary>
    /// <param name="defaultBranch">Branch to merge into the current branch.</param>
    public FormMergeBranch(IGitUICommands commands, string? defaultBranch)
        : base(commands, true)
    {
        InitializeComponent();
        InitializeStaticContent();

        Ok.Click += OkClick;
        NonDefaultMergeStrategy.IsCheckedChanged += NonDefaultMergeStrategy_CheckedChanged;
        strategyHelp.Click += strategyHelp_LinkClicked;
        advanced.IsCheckedChanged += advanced_CheckedChanged;
        fastForward.IsCheckedChanged += fastForward_CheckedChanged;
        noFastForward.IsCheckedChanged += noFastForward_CheckedChanged;
        addLogMessages.IsCheckedChanged += addMessages_CheckedChanged;
        addMergeMessage.IsCheckedChanged += addMergeMessage_CheckedChanged;
        nbMessages.ValueChanged += nbMessages_ValueChanged;

        _commitMessageManagerOwner = new WinFormsShims.Control();
        _commitMessageManager = new CommitMessageManager(
            _commitMessageManagerOwner,
            Module.WorkingDirGitDir,
            Module.CommitEncoding);

        noCommit.IsChecked = AppSettings.DontCommitMerge;
        helpImageDisplayUserControl1.IsVisible = !AppSettings.DontShowHelpImages;
        _defaultBranch = defaultBranch;

        SettingsSource effectiveSettings = Module.GetEffectiveSettings();
        IDetachedSettings detachedSettings = effectiveSettings.Detached();
        noFastForward.IsChecked = detachedSettings.NoFastForwardMerge;
        fastForward.IsChecked = !detachedSettings.NoFastForwardMerge;
        addLogMessages.IsChecked = DetailedSettings.AddMergeLogMessages.ValueOrDefault(effectiveSettings);
        nbMessages.Value = DetailedSettings.MergeLogMessagesCount.ValueOrDefault(effectiveSettings);
        advanced.IsChecked = AppSettings.AlwaysShowAdvOpt;
        advanced_CheckedChanged(this, EventArgs.Empty);

        AcceptButton = Ok;
        InitializeComplete();
    }

    public override void TranslateItems(GitExtensions.Extensibility.Translations.ITranslation translation)
    {
        base.TranslateItems(translation);
        helpImageDisplayUserControl1.IsOnHoverShowImage2NoticeText = _formMergeBranchHoverShowImageLabelText.Text;
    }

    protected override void OnRuntimeLoad(EventArgs e)
    {
        base.OnRuntimeLoad(e);

        string selectedHead = Module.GetSelectedBranch();
        currentBranchLabel.Text = selectedHead;
        Branches.BranchesToSelect = Module.GetRefs(RefsFilter.Heads | RefsFilter.Remotes | RefsFilter.Tags);

        if (_defaultBranch is not null)
        {
            Branches.SetSelectedText(_defaultBranch);
        }
        else
        {
            string merge = Module.GetRemoteBranch(selectedHead);
            if (!string.IsNullOrEmpty(merge))
            {
                Branches.SetSelectedText(merge);
            }
        }

        Branches.Select();
    }

    protected override void OnClosed(EventArgs e)
    {
        _commitMessageManagerOwner?.Dispose();
        base.OnClosed(e);
    }

    private void InitializeStaticContent()
    {
        _NO_TRANSLATE_mergeStrategy.ItemsSource =
        new[]
        {
            "resolve",
            "recursive",
            "octopus",
            "ours",
            "subtree",
        };
        helpImageDisplayUserControl1.Image1 = Properties.Images.HelpCommandMerge;
        helpImageDisplayUserControl1.Image2 = Properties.Images.HelpCommandMergeFastForward;
    }

    private void OkClick(object? sender, EventArgs e)
    {
        this.InvokeAndForget(PerformMergeAsync);
    }

    private async Task PerformMergeAsync()
    {
        Ok.IsEnabled = false;
        try
        {
            IDetachedSettings detachedSettings = Module.GetEffectiveSettings().Detached();
            detachedSettings.NoFastForwardMerge = noFastForward.IsChecked == true;
            AppSettings.DontCommitMerge = noCommit.IsChecked == true;

            string? mergeMessagePath = null;
            if (addMergeMessage.IsChecked == true)
            {
                await _commitMessageManager.WriteCommitMessageToFileAsync(
                    mergeMessage.Text ?? string.Empty,
                    CommitMessageType.Merge,
                    usingCommitTemplate: false,
                    ensureCommitMessageSecondLineEmpty: false);
                mergeMessagePath = _commitMessageManager.MergeMessagePath;
            }

            ArgumentString command = Commands.MergeBranch(
                Branches.GetSelectedText(),
                fastForward.IsChecked == true,
                squash.IsChecked == true,
                noCommit.IsChecked == true,
                _NO_TRANSLATE_mergeStrategy.Text ?? string.Empty,
                allowUnrelatedHistories.IsChecked == true,
                mergeMessagePath,
                Module.GetPathForGitExecution,
                addLogMessages.IsChecked == true ? Convert.ToInt32(nbMessages.Value) : null);
            bool success = FormProcess.ShowDialog(
                this,
                UICommands,
                arguments: command,
                Module.WorkingDir,
                input: null,
                useDialogSettings: true);

            bool wasConflict = MergeConflictHandler.HandleMergeConflicts(
                UICommands,
                this,
                offerCommit: noCommit.IsChecked != true);

            if (success || wasConflict)
            {
                UICommands.RepoChangedNotifier.Notify();
                Close();
            }
        }
        finally
        {
            if (IsVisible)
            {
                Ok.IsEnabled = true;
            }
        }
    }

    private void NonDefaultMergeStrategy_CheckedChanged(object? sender, EventArgs e)
    {
        bool visible = NonDefaultMergeStrategy.IsChecked == true;
        _NO_TRANSLATE_mergeStrategy.IsVisible = visible;
        strategyHelp.IsVisible = visible;

        if (advanced.IsChecked != true)
        {
            _NO_TRANSLATE_mergeStrategy.Text = string.Empty;
        }
    }

    private void strategyHelp_LinkClicked(object? sender, EventArgs e)
    {
        OsShellUtil.OpenUrlInDefaultBrowser(
            GitUI.UserManual.UserManual.UrlFor("branches", "advanced-merge-options"));
    }

    private void advanced_CheckedChanged(object? sender, EventArgs e)
    {
        advancedPanel.IsVisible = advanced.IsChecked == true;
        NonDefaultMergeStrategy_CheckedChanged(this, EventArgs.Empty);

        if (advanced.IsChecked != true)
        {
            NonDefaultMergeStrategy.IsChecked = false;
            squash.IsChecked = false;
            allowUnrelatedHistories.IsChecked = false;
            addMergeMessage.IsChecked = false;
        }
    }

    private void fastForward_CheckedChanged(object? sender, EventArgs e)
    {
        if (fastForward.IsChecked == true)
        {
            helpImageDisplayUserControl1.IsOnHoverShowImage2 = true;
        }
    }

    private void noFastForward_CheckedChanged(object? sender, EventArgs e)
    {
        bool isChecked = noFastForward.IsChecked == true;
        helpImageDisplayUserControl1.IsOnHoverShowImage2 = !isChecked;
        squash.IsEnabled = !isChecked;

        if (isChecked)
        {
            squash.IsChecked = false;
        }
    }

    private void addMessages_CheckedChanged(object? sender, EventArgs e)
    {
        bool isChecked = addLogMessages.IsChecked == true;
        nbMessages.IsEnabled = isChecked;
        DetailedSettings.AddMergeLogMessages[Module.GetEffectiveSettings()] = isChecked;
    }

    private void addMergeMessage_CheckedChanged(object? sender, EventArgs e)
    {
        mergeMessage.IsEnabled = addMergeMessage.IsChecked == true;
    }

    private void nbMessages_ValueChanged(object? sender, EventArgs e)
    {
        DetailedSettings.MergeLogMessagesCount[Module.GetEffectiveSettings()] = Convert.ToInt32(nbMessages.Value);
    }
}
