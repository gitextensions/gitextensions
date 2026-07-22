using Avalonia.Controls;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUIPluginInterfaces;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.HelperDialogs;

// Twin of GitUI/HelperDialogs/FormResetCurrentBranch.cs. Avalonia has no native title-bar
// Help button, so the same context-sensitive git-reset documentation action is kept in the
// dialog footer while reset behavior remains in the original code-behind shape.
public partial class FormResetCurrentBranch : GitModuleForm
{
    private const string GitResetDocumentationUrl = "https://git-scm.com/docs/git-reset#Documentation/git-reset.txt-";

    private readonly TranslationString _branchInfo = new("Reset branch '{0}' to revision:");
    private readonly TranslationString _resetHardWarning = new("You are about to discard ALL local changes, are you sure?");
    private readonly TranslationString _resetCaption = new("Reset branch");

    public enum ResetType
    {
        Soft,
        Mixed,
        Keep,
        Merge,
        Hard
    }

    public static FormResetCurrentBranch Create(IGitUICommands commands, GitRevision revision, ResetType resetType = ResetType.Soft)
        => new(commands, revision ?? throw new NotSupportedException(TranslatedStrings.NoRevision), resetType);

    public FormResetCurrentBranch()
    {
        InitializeComponent();
        WireControls();
        InitializeComplete();
    }

    private FormResetCurrentBranch(IGitUICommands commands, GitRevision revision, ResetType resetType)
        : base(commands, enablePositionRestore: false)
    {
        Revision = revision;

        InitializeComponent();
        WireControls();
        InitializeComplete();
        SelectResetType(resetType);
    }

    public GitRevision Revision { get; } = null!;

    private void WireControls()
    {
        Ok.Click += Ok_Click;
        Cancel.Click += Cancel_Click;
        _NO_TRANSLATE_Help.Click += FormResetCurrentBranch_HelpButtonClicked;
        AcceptButton = Ok;
    }

    protected override void OnRuntimeLoad(EventArgs e)
    {
        base.OnRuntimeLoad(e);

        if (!TryGetUICommands(out _) || Revision is null)
        {
            return;
        }

        LoadRevisionInfo();
    }

    private void LoadRevisionInfo()
    {
        _NO_TRANSLATE_BranchInfo.Text = string.Format(_branchInfo.Text, Module.GetSelectedBranch());
        commitSummaryUserControl1.Revision = Revision;
    }

    private void Ok_Click(object? sender, EventArgs e)
    {
        bool updateSubmodules = AppSettings.UpdateSubmodulesOnCheckout is true
            && Module.HasSubmodules()
            && Revision.ObjectId != Module.GetCurrentCheckout();

        if (Soft.IsChecked == true)
        {
            FormProcess.ShowDialog(this, UICommands, arguments: Commands.Reset(ResetMode.Soft, Revision.Guid, quiet: false), Module.WorkingDir, input: null, useDialogSettings: true);
        }
        else if (Mixed.IsChecked == true)
        {
            FormProcess.ShowDialog(this, UICommands, arguments: Commands.Reset(ResetMode.Mixed, Revision.Guid, quiet: false), Module.WorkingDir, input: null, useDialogSettings: true);
        }
        else if (Hard.IsChecked == true)
        {
            if (MessageBoxes.Show(
                    this,
                    _resetHardWarning.Text,
                    _resetCaption.Text,
                    WinFormsShims.MessageBoxButtons.YesNo,
                    WinFormsShims.MessageBoxIcon.Exclamation) == WinFormsShims.DialogResult.Yes)
            {
                ObjectId currentCheckout = Module.GetCurrentCheckout();
                bool success = FormProcess.ShowDialog(this, UICommands, arguments: Commands.Reset(ResetMode.Hard, Revision.Guid, quiet: false), Module.WorkingDir, input: null, useDialogSettings: true);
                if (success && currentCheckout != Revision.ObjectId)
                {
                    UICommands.UpdateSubmodules(this);
                }
            }
            else
            {
                return;
            }
        }
        else if (Merge.IsChecked == true)
        {
            FormProcess.ShowDialog(this, UICommands, arguments: Commands.Reset(ResetMode.Merge, Revision.Guid, quiet: false), Module.WorkingDir, input: null, useDialogSettings: true);
        }
        else if (Keep.IsChecked == true)
        {
            FormProcess.ShowDialog(this, UICommands, arguments: Commands.Reset(ResetMode.Keep, Revision.Guid, quiet: false), Module.WorkingDir, input: null, useDialogSettings: true);
        }

        if (updateSubmodules)
        {
            UICommands.StartUpdateSubmodulesDialog(this);
        }

        UICommands.RepoChangedNotifier.Notify();
        DialogResult = WinFormsShims.DialogResult.OK;
        Close();
    }

    private void Cancel_Click(object? sender, EventArgs e)
    {
        DialogResult = WinFormsShims.DialogResult.Cancel;
        Close();
    }

    private void FormResetCurrentBranch_HelpButtonClicked(object? sender, EventArgs e)
        => OsShellUtil.OpenUrlInDefaultBrowser(GetHelpUrl());

    private ArgumentString? BuildResetCommand()
    {
        if (Revision is null)
        {
            return null;
        }

        return GetResetType() switch
        {
            ResetType.Soft => Commands.Reset(ResetMode.Soft, Revision.Guid, quiet: false),
            ResetType.Mixed => Commands.Reset(ResetMode.Mixed, Revision.Guid, quiet: false),
            ResetType.Keep => Commands.Reset(ResetMode.Keep, Revision.Guid, quiet: false),
            ResetType.Merge => Commands.Reset(ResetMode.Merge, Revision.Guid, quiet: false),
            ResetType.Hard => Commands.Reset(ResetMode.Hard, Revision.Guid, quiet: false),
            _ => default(ArgumentString?),
        };
    }

    private string GetHelpUrl()
    {
        string? helpSection = GetResetType() switch
        {
            ResetType.Soft => "--soft",
            ResetType.Mixed => "--mixed",
            ResetType.Keep => "--keep",
            ResetType.Merge => "--merge",
            ResetType.Hard => "--hard",
            _ => null,
        };

        return GitResetDocumentationUrl + helpSection;
    }

    private ResetType? GetResetType()
    {
        if (Soft.IsChecked == true)
        {
            return ResetType.Soft;
        }

        if (Mixed.IsChecked == true)
        {
            return ResetType.Mixed;
        }

        if (Keep.IsChecked == true)
        {
            return ResetType.Keep;
        }

        if (Merge.IsChecked == true)
        {
            return ResetType.Merge;
        }

        if (Hard.IsChecked == true)
        {
            return ResetType.Hard;
        }

        return null;
    }

    private void SelectResetType(ResetType resetType)
    {
        switch (resetType)
        {
            case ResetType.Soft:
                Soft.IsChecked = true;
                break;
            case ResetType.Mixed:
                Mixed.IsChecked = true;
                break;
            case ResetType.Keep:
                Keep.IsChecked = true;
                break;
            case ResetType.Merge:
                Merge.IsChecked = true;
                break;
            case ResetType.Hard:
                Hard.IsChecked = true;
                break;
        }
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(FormResetCurrentBranch form)
    {
        public Button Cancel => form.Cancel;
        public RadioButton Hard => form.Hard;
        public Button Help => form._NO_TRANSLATE_Help;
        public RadioButton Keep => form.Keep;
        public RadioButton Merge => form.Merge;
        public RadioButton Mixed => form.Mixed;
        public Button Ok => form.Ok;
        public RadioButton Soft => form.Soft;
        public string BranchInfo => form._NO_TRANSLATE_BranchInfo.Text ?? string.Empty;
        public ResetType? SelectedResetType => form.GetResetType();
        public GitRevision? SummaryRevision => form.commitSummaryUserControl1.Revision;

        public ArgumentString? BuildResetCommand() => form.BuildResetCommand();
        public string GetHelpUrl() => form.GetHelpUrl();
        public void Load() => form.LoadRevisionInfo();
        public void SelectResetType(ResetType resetType) => form.SelectResetType(resetType);
    }
}
