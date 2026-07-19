using Avalonia.Controls;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI.Compat;
using GitUI.HelperDialogs;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs;

// Reduced twin: pushes the current branch to the same branch on a selected remote.
// Remote management, URL/tag/multiple-branch pushes, tracking prompts, scripts, and
// rejected-push recovery remain with later increments.
public sealed partial class FormPush : GitModuleForm
{
    private readonly TranslationString _noCurrentBranch = new("No branch is selected, cannot push.");
    private readonly TranslationString _pushCaption = new("Push");
    private readonly TranslationString _pushToCaption = new("Push to {0}");
    private readonly TranslationString _forceWithLeaseTooltips =
        new("Force with lease is a safer way to force push. It ensures you only overwrite work that you have seen in your local repository");
    private string? _currentBranchName;

    public FormPush()
    {
        InitializeComponent();
        InitializeComplete();
    }

    public FormPush(IGitUICommands commands, string? branchName = null)
        : base(commands, enablePositionRestore: true)
    {
        InitializeComponent();

        Push.Click += PushClick;
        Push.Content = AvaloniaTranslationUtils.ToAvaloniaMnemonics(TranslatedStrings.ButtonPush);

        _currentBranchName = branchName ?? Module.GetSelectedBranch();
        _NO_TRANSLATE_Branch.Text = _currentBranchName;
        RemoteBranch.Text = _currentBranchName;

        foreach (string remoteName in Module.GetRemoteNames())
        {
            _NO_TRANSLATE_Remotes.Items.Add(remoteName);
        }

        string? configuredRemote = string.IsNullOrWhiteSpace(_currentBranchName)
            ? null
            : Module.GetSetting($"branch.{_currentBranchName}.remote");
        _NO_TRANSLATE_Remotes.SelectedItem = _NO_TRANSLATE_Remotes.Items
            .OfType<string>()
            .FirstOrDefault(remote => StringComparer.OrdinalIgnoreCase.Equals(remote, configuredRemote))
            ?? _NO_TRANSLATE_Remotes.Items
                .OfType<string>()
                .FirstOrDefault(remote => StringComparer.OrdinalIgnoreCase.Equals(remote, "origin"));
        _NO_TRANSLATE_Remotes.SelectedIndex = _NO_TRANSLATE_Remotes.SelectedIndex < 0
            && _NO_TRANSLATE_Remotes.ItemCount > 0
                ? 0
                : _NO_TRANSLATE_Remotes.SelectedIndex;

        UpdatePushButton();
        _NO_TRANSLATE_Remotes.SelectionChanged += (_, _) => UpdatePushButton();
        InitializeComplete();
    }

    public bool ErrorOccurred { get; private set; }

    public WinFormsShims.DialogResult PushAndShowDialogWhenFailed(WinFormsShims.IWin32Window? owner = null)
    {
        if (!PushChanges(owner))
        {
            return ShowDialog(owner);
        }

        return WinFormsShims.DialogResult.OK;
    }

    public void CheckForceWithLease()
    {
        ckForceWithLease.IsChecked = true;
    }

    private void PushClick(object? sender, EventArgs e)
    {
        DialogResult = PushChanges(this)
            ? WinFormsShims.DialogResult.OK
            : WinFormsShims.DialogResult.None;
    }

    private bool PushChanges(WinFormsShims.IWin32Window? owner)
    {
        ErrorOccurred = false;
        if (string.IsNullOrWhiteSpace(_currentBranchName))
        {
            MessageBoxes.Show(owner, _noCurrentBranch.Text, TranslatedStrings.Error, WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Error);
            return false;
        }

        if (_NO_TRANSLATE_Remotes.SelectedItem is not string remote)
        {
            ErrorOccurred = true;
            return false;
        }

        ArgumentString pushArguments = Commands.Push(
            remote,
            Module.FormatBranchName(_currentBranchName),
            _currentBranchName,
            ckForceWithLease.IsChecked == true ? ForcePushOptions.ForceWithLease : ForcePushOptions.DoNotForce,
            track: false,
            recursiveSubmodules: 0);

        using FormRemoteProcess form = new(UICommands, pushArguments)
        {
            Remote = remote,
            Text = string.Format(_pushToCaption.Text, remote),
        };
        form.ShowDialog(owner);
        ErrorOccurred = form.ErrorOccurred();
        Module.InvalidateGitSettings();
        return !ErrorOccurred;
    }

    private void UpdatePushButton()
    {
        Push.IsEnabled = !string.IsNullOrWhiteSpace(_currentBranchName)
            && _NO_TRANSLATE_Remotes.SelectedItem is string;
    }

    public override void TranslateItems(GitExtensions.Extensibility.Translations.ITranslation translation)
    {
        base.TranslateItems(translation);
        Push.Content = AvaloniaTranslationUtils.ToAvaloniaMnemonics(TranslatedStrings.ButtonPush);
    }
}
