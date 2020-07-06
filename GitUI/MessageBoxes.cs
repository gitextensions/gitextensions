using System;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Config;
using JetBrains.Annotations;
using Microsoft.WindowsAPICodePack.Dialogs;
using ResourceManager;

namespace GitUI
{
    public class MessageBoxes : Translate
    {
        private readonly TranslationString _archiveRevisionCaption = new TranslationString("Archive revision");

        private readonly TranslationString _context = new TranslationString("Context");

        private readonly TranslationString _externalOperationFailure = new TranslationString("External operation failure");

        private readonly TranslationString _externalOperationFailureHint
            = new TranslationString("If you think this was caused by Git Extensions, you can report a bug, or just ignore and continue.");

        private readonly TranslationString _failedToExecute = new TranslationString("Failed to execute");

        private readonly TranslationString _failedToRunShell = new TranslationString("Failed to run shell");

        private readonly TranslationString _notValidGitDirectory = new TranslationString("The current directory is not a valid git repository.");

        private readonly TranslationString _unresolvedMergeConflictsCaption = new TranslationString("Merge conflicts");
        private readonly TranslationString _unresolvedMergeConflicts = new TranslationString("There are unresolved merge conflicts, solve conflicts now?");

        private readonly TranslationString _middleOfRebaseCaption = new TranslationString("Rebase");
        private readonly TranslationString _middleOfRebase = new TranslationString("You are in the middle of a rebase, continue rebase?");

        private readonly TranslationString _middleOfPatchApplyCaption = new TranslationString("Patch apply");
        private readonly TranslationString _middleOfPatchApply = new TranslationString("You are in the middle of a patch apply, continue patch apply?");

        private const string _putty = "PuTTY";
        private readonly TranslationString _pageantNotFound = new TranslationString("Cannot load SSH key. PuTTY is not configured properly.");

        private readonly TranslationString _serverHostkeyNotCachedText =
            new TranslationString("The server's host key is not cached in the registry.\n\nDo you want to trust this host key and then try again?");

        private readonly TranslationString _updateSubmodules = new TranslationString("Update submodules");
        private readonly TranslationString _theRepositorySubmodules = new TranslationString("Update submodules on checkout?");
        private readonly TranslationString _updateSubmodulesToo = new TranslationString("Since this repository has submodules, it's necessary to update them on every checkout.\r\n\r\nThis will just checkout on the submodule the commit determined by the superproject.");
        private readonly TranslationString _rememberChoice = new TranslationString("Remember choice");

        private readonly TranslationString _reason = new TranslationString("Reason");

        private readonly TranslationString _selectOnlyOneOrTwoRevisions = new TranslationString("Select only one or two revisions. Abort.");

        private readonly TranslationString _shellNotFoundCaption = new TranslationString("Shell not found");
        private readonly TranslationString _shellNotFound = new TranslationString("The selected shell is not installed, or is not on your path.");
        private readonly TranslationString _resetChangesCaption = new TranslationString("Reset changes");

        private readonly TranslationString _workingDirectory = new TranslationString("Working directory");

        // internal for FormTranslate
        internal MessageBoxes()
        {
            Translator.Translate(this, AppSettings.CurrentTranslation);
        }

        [CanBeNull] private static MessageBoxes instance;

        private static MessageBoxes Instance => instance ?? (instance = new MessageBoxes());

        public static void FailedToExecute(IWin32Window owner, string executable, Exception ex)
            => ShowError(owner, $"{Instance._failedToExecute.Text} {executable}.{Environment.NewLine}"
                                + $"{Instance._reason.Text}: {Instance.GetText(ex)}");

        public static void FailedToRunShell(IWin32Window owner, string shell, Exception ex)
            => ShowError(owner, $"{Instance._failedToRunShell.Text} {shell.Quote()}.{Environment.NewLine}"
                                + $"{Instance._reason.Text}: {Instance.GetText(ex)}");

        public static void NotValidGitDirectory([CanBeNull] IWin32Window owner)
            => ShowError(owner, Instance._notValidGitDirectory.Text);

        public static void ShowGitConfigurationExceptionMessage(IWin32Window owner, GitConfigurationException exception)
            => Show(owner,
                    string.Format(ResourceManager.Strings.GeneralGitConfigExceptionMessage,
                                  exception.ConfigPath, Environment.NewLine, (exception.InnerException ?? exception).Message),
                    ResourceManager.Strings.GeneralGitConfigExceptionCaption,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

        public static bool MiddleOfRebase(IWin32Window owner)
            => Confirm(owner, Instance._middleOfRebase.Text, Instance._middleOfRebaseCaption.Text);

        public static bool MiddleOfPatchApply(IWin32Window owner)
            => Confirm(owner, Instance._middleOfPatchApply.Text, Instance._middleOfPatchApplyCaption.Text);

        public static void PAgentNotFound(IWin32Window owner)
            => ShowError(owner, Instance._pageantNotFound.Text, _putty);

        public static void SelectOnlyOneOrTwoRevisions(IWin32Window owner)
            => ShowError(owner, Instance._selectOnlyOneOrTwoRevisions.Text, Instance._archiveRevisionCaption.Text);

        public static bool CacheHostkey(IWin32Window owner)
            => Confirm(owner, Instance._serverHostkeyNotCachedText.Text, "SSH");

        public static bool ConfirmResetSelectedFiles(IWin32Window owner, string text)
            => Confirm(owner, text, Instance._resetChangesCaption.Text);

        public static bool ConfirmResolveMergeConflicts(IWin32Window owner)
            => Confirm(owner, Instance._unresolvedMergeConflicts.Text, Instance._unresolvedMergeConflictsCaption.Text);

        public static bool ConfirmUpdateSubmodules(IWin32Window win)
        {
            using var dialog = new TaskDialog
            {
                OwnerWindowHandle = win.Handle,
                Text = Instance._updateSubmodulesToo.Text,
                InstructionText = Instance._theRepositorySubmodules.Text,
                Caption = Instance._updateSubmodules.Text,
                Icon = TaskDialogStandardIcon.Information,
                StandardButtons = TaskDialogStandardButtons.Yes | TaskDialogStandardButtons.No,
                FooterCheckBoxText = Instance._rememberChoice.Text,
                FooterIcon = TaskDialogStandardIcon.Information,
                StartupLocation = TaskDialogStartupLocation.CenterOwner
            };

            bool result = dialog.Show() == TaskDialogResult.Yes;
            if (dialog.FooterCheckBoxChecked == true)
            {
                AppSettings.DontConfirmUpdateSubmodulesOnCheckout = result;
                AppSettings.UpdateSubmodulesOnCheckout = result;
            }

            return result;
        }

        public static void ShellNotFound([CanBeNull] IWin32Window owner)
            => ShowError(owner, Instance._shellNotFound.Text, Instance._shellNotFoundCaption.Text);

        public static void Show([CanBeNull] IWin32Window owner, ExternalOperationException ex, ExternalOperationExceptionFactory.Handling exceptionHandling)
        {
            if (exceptionHandling == ExternalOperationExceptionFactory.Handling.None)
            {
                return;
            }

            string error = Instance.GetText(ex.InnerException ?? ex);
            string message = Instance.GetText(ex, addInnerExceptions: false);
            if (exceptionHandling == ExternalOperationExceptionFactory.Handling.OptionalBugReport)
            {
                message += $"{Environment.NewLine}{Environment.NewLine}{Instance._externalOperationFailureHint.Text}";
            }

            bool handled = true;
            ShowInMainThread(owner, owner =>
            {
                using var dialog = new TaskDialog
                {
                    OwnerWindowHandle = owner?.Handle ?? IntPtr.Zero,
                    Caption = Instance._externalOperationFailure.Text,
                    Icon = TaskDialogStandardIcon.Error,
                    InstructionText = error,
                    Text = message,
                    Cancelable = true
                };
                dialog.AddButton("IgnoreContinue", "Ignore and continue");
                if (exceptionHandling == ExternalOperationExceptionFactory.Handling.OptionalBugReport)
                {
                    dialog.AddButton("Report", "Report bug", () => handled = false);
                }

                return dialog.Show();
            });

            ex.Handled = handled;
        }

        public static void ShowError([CanBeNull] IWin32Window owner, string text, string caption = null)
            => Show(owner, text, caption ?? Strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);

        private static bool Confirm([CanBeNull] IWin32Window owner, string text, string caption)
            => Show(owner, text, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

        private static DialogResult Show([CanBeNull] IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
            => ShowInMainThread(owner, owner => MessageBox.Show(owner, text, caption, buttons, icon));

        private static TResult ShowInMainThread<TResult>([CanBeNull] IWin32Window owner, Func<IWin32Window, TResult> show)
            => ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                owner ??= Form.ActiveForm;

                return show(owner);
            });

        private string GetText(Exception ex, bool addInnerExceptions = true)
        {
            string text;
            switch (ex)
            {
                case ExternalOperationException externalOperationException:
                    text = $"{_failedToExecute.Text}:{Environment.NewLine}{externalOperationException.Command.ShortenTo(1000)}"
                           + $"{Environment.NewLine}{Environment.NewLine}{_workingDirectory.Text}:{Environment.NewLine}{externalOperationException.WorkingDirectory}"
                           + (externalOperationException.Context == null ? string.Empty : $"{Environment.NewLine}{Environment.NewLine}{_context.Text}: {externalOperationException.Context}");
                    break;

                default:
                    text = $"{ex.GetType().Name}: {ex.Message}";
                    break;
            }

            if (addInnerExceptions && ex.InnerException is object)
            {
                text = $"{text}{Environment.NewLine}{Environment.NewLine}{_reason.Text}: {GetText(ex.InnerException, addInnerExceptions)}";
            }

            return text;
        }
    }

    public static class TaskDialogExtensions
    {
        /// <summary>
        /// Sadly does not close the TaskDialog and does not affect the value returned by Show()!
        /// </summary>
        /// <param name="dialog">guess what</param>
        /// <param name="name">button name</param>
        /// <param name="label">button label</param>
        /// <param name="result">result of TaskDialog.Show</param>
        public static void AddButton(this TaskDialog dialog, string name, string label, TaskDialogResult result)
        {
            var button = new TaskDialogCommandLink(name, label);
            button.Click += (s, e) =>
            {
                dialog.Close(result);
            };
            dialog.Controls.Add(button);
        }

        public static void AddButton(this TaskDialog dialog, string name, string label, Action onClick = null)
        {
            var button = new TaskDialogCommandLink(name, label);
            button.Click += (s, e) =>
            {
                dialog.Close();
                onClick?.Invoke();
            };
            dialog.Controls.Add(button);
        }
    }
}
