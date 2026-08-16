using GitCommands;
using GitCommands.Config;
using GitCommands.Settings;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitUI.Compat;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI;

// Reduced twin: generic overloads forward to the shared approved wrapper; domain-specific
// translated helpers join this class as their Avalonia consumers are ported.
public class MessageBoxes : Translate
{
    private readonly TranslationString _cannotFindRevisionFilter = new(@"Revision ""{0}"" is not visible in the revision grid. Remove the revision filter.");
    private readonly TranslationString _cannotFindRevisionCaption = new("Cannot find revision");
    private readonly TranslationString _noRevisionFoundError = new("No revision found.");
    private readonly TranslationString _archiveRevisionCaption = new("Archive revision");

    private readonly TranslationString _failedToRunShell = new("Failed to run shell");
    private readonly TranslationString _reason = new("Reason");
    private readonly TranslationString _selectOnlyOneOrTwoRevisions = new("Select only one or two revisions. Abort.");
    private readonly TranslationString _shellNotFoundCaption = new("Shell not found");
    private readonly TranslationString _shellNotFound = new("The selected shell is not installed, or is not on your path.");

    private readonly TranslationString _confirmBranchCheckoutCaption = new("Confirm checkout");
    private readonly TranslationString _confirmBranchCheckout = new(@"Are you sure you want to check out branch ""{0}""?");

    private readonly TranslationString _retry = new("Retry");
    private readonly TranslationString _retryOpenVisualStudio = new("Visual Studio failed to open the file.\nThis can be caused by an opened dialog window or other pending operations.\nTry again?");

    private readonly TranslationString _unresolvedMergeConflictsCaption = new("Merge conflicts");
    private readonly TranslationString _unresolvedMergeConflicts = new("There are unresolved merge conflicts, solve conflicts now?");

    private readonly TranslationString _middleOfRebaseCaption = new("Rebase");
    private readonly TranslationString _middleOfRebase = new("You are in the middle of a rebase, continue rebase?");

    private readonly TranslationString _middleOfPatchApplyCaption = new("Patch apply");
    private readonly TranslationString _middleOfPatchApply = new("You are in the middle of a patch apply, continue patch apply?");

    private readonly TranslationString _updateSubmodules = new("Update submodules");
    private readonly TranslationString _theRepositorySubmodules = new("Update submodules on checkout?");
    private readonly TranslationString _updateSubmodulesToo = new("Since this repository has submodules, it's necessary to update them on every checkout.\r\n\r\nThis will just checkout on the submodule the commit determined by the superproject.");
    private readonly TranslationString _rememberChoice = new("&Remember choice");
    private readonly TranslationString _cannotOpenSubmoduleCaption = new("Cannot open submodule");
    private readonly TranslationString _cannotOpenGitExtensionsCaption = new("Cannot open Git Extensions");
    private readonly TranslationString _directoryDoesNotExist = new("The directory \"{0}\" does not exist.");
    private readonly TranslationString _submoduleDirectoryDoesNotExist = new("The directory \"{0}\" does not exist for submodule \"{1}\".");

    internal MessageBoxes()
    {
        Translator.Translate(this, AppSettings.CurrentTranslation);
    }

    private static MessageBoxes Instance => field ??= new();

    // parity-scaffolding: lets headless confirmation tests choose a real page action without opening an unattended modal window.
    private static Func<WinFormsShims.IWin32Window?, TaskDialogPage, TaskDialogButton> TaskDialogPresenter { get; set; } = TaskDialog.ShowDialog;

    public static void RevisionFilteredInGrid(WinFormsShims.IWin32Window? owner, ObjectId objectId)
        => ShowError(owner, string.Format(Instance._cannotFindRevisionFilter.Text, objectId.ToShortString()), Instance._cannotFindRevisionCaption.Text);

    public static void CannotFindGitRevision(WinFormsShims.IWin32Window? owner)
        => ShowError(owner, Instance._noRevisionFoundError.Text, Instance._cannotFindRevisionCaption.Text);

    public static void SelectOnlyOneOrTwoRevisions(WinFormsShims.IWin32Window? owner)
        => ShowError(owner, Instance._selectOnlyOneOrTwoRevisions.Text, Instance._archiveRevisionCaption.Text);

    public static void SubmoduleDirectoryDoesNotExist(WinFormsShims.IWin32Window? owner, string directory, string submoduleName)
        => ShowError(owner, string.Format(Instance._submoduleDirectoryDoesNotExist.Text, directory, submoduleName), Instance._cannotOpenSubmoduleCaption.Text);

    public static void SubmoduleDirectoryDoesNotExist(WinFormsShims.IWin32Window? owner, string directory)
        => ShowError(owner, string.Format(Instance._directoryDoesNotExist.Text, directory), Instance._cannotOpenSubmoduleCaption.Text);

    public static void GitExtensionsDirectoryDoesNotExist(WinFormsShims.IWin32Window? owner, string directory)
        => ShowError(owner, string.Format(Instance._directoryDoesNotExist.Text, directory), Instance._cannotOpenGitExtensionsCaption.Text);

    public static void FailedToRunShell(WinFormsShims.IWin32Window? owner, string shell, Exception ex)
        => ShowError(owner, $"{Instance._failedToRunShell.Text} {shell.Quote()}.{Environment.NewLine}"
                            + $"{Instance._reason.Text}: {ex.Message}");

    public static void ShellNotFound(WinFormsShims.IWin32Window? owner)
        => ShowError(owner, Instance._shellNotFound.Text, Instance._shellNotFoundCaption.Text);

    public static void ShowError(WinFormsShims.IWin32Window? owner, string text, string? caption = null)
        => GitExtensions.Extensibility.MessageBoxes.ShowError(owner, text, caption);

    public static void ShowGitConfigurationExceptionMessage(WinFormsShims.IWin32Window? owner, GitConfigurationException exception)
        => Show(
            owner,
            string.Format(
                ResourceManager.TranslatedStrings.GeneralGitConfigExceptionMessage,
                exception.ConfigPath,
                Environment.NewLine,
                (exception.InnerException ?? exception).Message),
            ResourceManager.TranslatedStrings.GeneralGitConfigExceptionCaption,
            WinFormsShims.MessageBoxButtons.OK,
            WinFormsShims.MessageBoxIcon.Warning);

    public static bool MiddleOfRebase(WinFormsShims.IWin32Window? owner)
        => Confirm(owner, Instance._middleOfRebase.Text, Instance._middleOfRebaseCaption.Text);

    public static bool MiddleOfPatchApply(WinFormsShims.IWin32Window? owner)
        => Confirm(owner, Instance._middleOfPatchApply.Text, Instance._middleOfPatchApplyCaption.Text);

    public static bool ConfirmResolveMergeConflicts(WinFormsShims.IWin32Window? owner)
        => ConfirmSuppressible(owner, Instance._unresolvedMergeConflicts.Text, Instance._unresolvedMergeConflictsCaption.Text, AppSettings.DontConfirmResolveConflicts);

    /// <summary>
    ///  Shows a suppressible Yes/No confirmation with a "Don't show again" verification checkbox.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   Returns <see langword="true"/> immediately without prompting when <paramref name="dontConfirm"/> is already set,
    ///   so the suppress guard lives here rather than being duplicated at every call site.
    ///   When the user ticks the checkbox, <paramref name="dontConfirm"/> is persisted.
    ///  </para>
    /// </remarks>
    /// <param name="owner">The window that owns and is blocked by the confirmation.</param>
    /// <param name="text">The message body.</param>
    /// <param name="caption">The dialog caption.</param>
    /// <param name="dontConfirm">The setting that suppresses this confirmation.</param>
    /// <param name="heading">An optional heading shown above the message body.</param>
    /// <param name="icon">The icon to display; defaults to <see cref="TaskDialogIcon.Information"/>.</param>
    /// <param name="footnote">An optional footnote shown at the bottom of the dialog.</param>
    /// <param name="defaultNo">When <see langword="true"/>, the "No" button is the default.</param>
    /// <returns><see langword="true"/> when the user confirms (or the confirmation is suppressed); otherwise <see langword="false"/>.</returns>
    public static bool ConfirmSuppressible(WinFormsShims.IWin32Window? owner, string text, string caption, ISetting<bool> dontConfirm, string? heading = null, TaskDialogIcon? icon = null, string? footnote = null, bool defaultNo = false)
    {
        if (dontConfirm.Value)
        {
            return true;
        }

        TaskDialogPage page = new()
        {
            Text = text,
            Heading = heading,
            Caption = caption,
            Buttons = { TaskDialogButton.Yes, TaskDialogButton.No },
            Icon = icon ?? TaskDialogIcon.Information,
            Verification = new TaskDialogVerificationCheckBox
            {
                Text = TranslatedStrings.DontShowAgain
            },
            SizeToContent = true
        };

        if (footnote is not null)
        {
            page.Footnote = footnote;
        }

        if (defaultNo)
        {
            page.DefaultButton = TaskDialogButton.No;
        }

        bool confirmed = TaskDialogPresenter(owner, page) == TaskDialogButton.Yes;

        if (page.Verification.Checked)
        {
            dontConfirm.Value = true;
        }

        return confirmed;
    }

    public static bool ConfirmBranchCheckout(WinFormsShims.IWin32Window? owner, string branchName)
        => !AppSettings.ConfirmBranchCheckout.Value
           || Confirm(owner, string.Format(Instance._confirmBranchCheckout.Text, branchName), Instance._confirmBranchCheckoutCaption.Text);

    public static bool ConfirmRetryOpenVisualStudio(WinFormsShims.IWin32Window? owner = null)
        => Confirm(owner, Instance._retryOpenVisualStudio.Text, Instance._retry.Text);

    public static bool ConfirmUpdateSubmodules(WinFormsShims.IWin32Window? owner)
    {
        TaskDialogPage page = new()
        {
            Text = Instance._updateSubmodulesToo.Text,
            Heading = Instance._theRepositorySubmodules.Text,
            Caption = Instance._updateSubmodules.Text,
            Icon = TaskDialogIcon.Information,
            Buttons = { TaskDialogButton.Yes, TaskDialogButton.No },
            Verification = new TaskDialogVerificationCheckBox
            {
                Text = Instance._rememberChoice.Text
            },
            SizeToContent = true
        };

        bool result = TaskDialog.ShowDialog(owner, page) == TaskDialogButton.Yes;
        if (page.Verification.Checked)
        {
            AppSettings.DontConfirmUpdateSubmodulesOnCheckout = result;
            AppSettings.UpdateSubmodulesOnCheckout = result;
        }

        return result;
    }

    public static bool Confirm(WinFormsShims.IWin32Window? owner, string text, string caption, WinFormsShims.MessageBoxIcon icon = WinFormsShims.MessageBoxIcon.Question, WinFormsShims.MessageBoxDefaultButton defaultButton = WinFormsShims.MessageBoxDefaultButton.Button1)
        => Show(owner, text, caption, WinFormsShims.MessageBoxButtons.YesNo, icon, defaultButton) == WinFormsShims.DialogResult.Yes;

    public static WinFormsShims.DialogResult Show(
        WinFormsShims.IWin32Window? owner,
        string text,
        string caption,
        WinFormsShims.MessageBoxButtons buttons,
        WinFormsShims.MessageBoxIcon icon,
        WinFormsShims.MessageBoxDefaultButton defaultButton = WinFormsShims.MessageBoxDefaultButton.Button1)
        => GitExtensions.Extensibility.MessageBoxes.Show(owner, text, caption, buttons, icon, defaultButton);

    public static WinFormsShims.DialogResult Show(
        WinFormsShims.IWin32Window? owner,
        string text,
        string caption,
        WinFormsShims.MessageBoxButtons buttons)
        => GitExtensions.Extensibility.MessageBoxes.Show(owner, text, caption, buttons);

    public static WinFormsShims.DialogResult Show(
        string text,
        string caption,
        WinFormsShims.MessageBoxButtons buttons,
        WinFormsShims.MessageBoxIcon icon,
        WinFormsShims.MessageBoxDefaultButton defaultButton)
        => GitExtensions.Extensibility.MessageBoxes.Show(text, caption, buttons, icon, defaultButton);

    public static WinFormsShims.DialogResult Show(
        string text,
        string caption,
        WinFormsShims.MessageBoxButtons buttons,
        WinFormsShims.MessageBoxIcon icon)
        => GitExtensions.Extensibility.MessageBoxes.Show(text, caption, buttons, icon);

    public static WinFormsShims.DialogResult Show(
        string text,
        string caption,
        WinFormsShims.MessageBoxButtons buttons)
        => GitExtensions.Extensibility.MessageBoxes.Show(text, caption, buttons);

    internal static class TestAccessor
    {
        internal static Func<WinFormsShims.IWin32Window?, TaskDialogPage, TaskDialogButton> TaskDialogPresenter
        {
            get => MessageBoxes.TaskDialogPresenter;
            set => MessageBoxes.TaskDialogPresenter = value;
        }
    }
}
