using System.ComponentModel;
using GitCommands.Git;
using GitExtUtils.GitUI.Theming;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.HelperDialogs;
using ResourceManager;

namespace GitUI.UserControls
{
    public partial class InteractiveGitActionControl : GitModuleControl
    {
        private readonly TranslationString _progressMessage = new("{0} is currently in progress.");
        private readonly TranslationString _conflictsMessage = new("There are unresolved merge conflicts.");
        private readonly TranslationString _progressWithConflictsMessage = new("{0} is currently in progress with merge conflicts.");

        private readonly TranslationString _bisect = new("Bisect");
        private readonly TranslationString _rebase = new("Rebase");
        private readonly TranslationString _merge = new("Merge");
        private readonly TranslationString _patch = new("Patch");

        public enum GitAction
        {
            Unknown,
            None,
            Bisect,
            Rebase,
            Merge,
            Patch
        }

        private GitAction _action;
        private bool _hasConflicts;

        private GitModuleForm? Form => FindForm() as GitModuleForm;

        public InteractiveGitActionControl()
        {
            InitializeComponent();
            InitializeComplete();
        }

        // It is possible for a repo to be in a middle of a bisect operation and
        // be in a conflicted state. Hence detect bisect separately from the rest
        // of git actions
        public void RefreshBisect()
        {
            if (!Module.IsValidGitWorkingDir())
            {
                return;
            }

            if (Module.InTheMiddleOfBisect())
            {
                SetGitAction(GitAction.Bisect, false);
                return;
            }

            SetGitAction(GitAction.None, false);
        }

        /// <summary>
        /// Refresh the banner in the revision grid after reactivation.
        /// </summary>
        /// <param name="checkForConflicts">Allow running Git command to check for conflicts.</param>
        public void RefreshGitAction(bool checkForConflicts)
        {
            // get the current state of the repo

            if (!Module.IsValidGitWorkingDir())
            {
                return;
            }

            bool hasConflicts;
            try
            {
                // This command can be executed seemingly in the background (selecting Browse),
                // do not notify the user (this can occur if Git is upgraded).
                // Running Git commands async when restoring may fail.
                hasConflicts = checkForConflicts && Module.InTheMiddleOfConflictedMerge(throwOnErrorExit: false);
            }
            catch (Win32Exception)
            {
                hasConflicts = false;
            }

            if (Module.InTheMiddleOfRebase())
            {
                SetGitAction(GitAction.Rebase, hasConflicts);
                return;
            }

            if (Module.InTheMiddleOfMerge())
            {
                SetGitAction(GitAction.Merge, hasConflicts);
                return;
            }

            if (Module.InTheMiddleOfPatch())
            {
                SetGitAction(GitAction.Patch, hasConflicts);
                return;
            }

            SetGitAction(GitAction.None, hasConflicts);
        }

        private void SetGitAction(GitAction action, bool hasConflicts)
        {
            if ((action == _action) && (hasConflicts == _hasConflicts))
            {
                // nothing to do
                return;
            }

            _action = action;
            _hasConflicts = hasConflicts;

            // remove old controls
            ButtonContainer.Controls.Clear();

            if ((_action == GitAction.None) && !_hasConflicts)
            {
                Visible = false;
                return;
            }

            IconBox.Image = _hasConflicts ? Properties.Images.SolveMerge : Properties.Resources.information;
            BackColor = (_hasConflicts ? System.Drawing.Color.Orange : System.Drawing.Color.LightSkyBlue).AdaptBackColor();
            TextLabel.SetForeColorForBackColor();

            string actionStr = "";

            switch (_action)
            {
                case GitAction.Bisect:
                    actionStr = _bisect.Text;
                    ButtonContainer.Controls.Add(MoreButton);
                    break;
                case GitAction.Rebase:
                    actionStr = _rebase.Text;
                    ButtonContainer.Controls.Add(_hasConflicts ? ResolveButton : ContinueButton);
                    ButtonContainer.Controls.Add(AbortButton);
                    ButtonContainer.Controls.Add(MoreButton);
                    break;
                case GitAction.Merge:
                    actionStr = _merge.Text;
                    ButtonContainer.Controls.Add(_hasConflicts ? ResolveButton : ContinueButton);
                    ButtonContainer.Controls.Add(AbortButton);
                    break;
                case GitAction.Patch:
                    actionStr = _patch.Text;
                    ButtonContainer.Controls.Add(_hasConflicts ? ResolveButton : ContinueButton);
                    ButtonContainer.Controls.Add(AbortButton);
                    ButtonContainer.Controls.Add(MoreButton);
                    break;
                case GitAction.None:
                    // can only get here if hasConflicts so add resolve button
                    ButtonContainer.Controls.Add(ResolveButton);
                    break;
            }

            TextLabel.Text = (_action == GitAction.None) ?
                _conflictsMessage.Text :
                string.Format(
                    _hasConflicts ?
                        _progressWithConflictsMessage.Text :
                        _progressMessage.Text,
                    actionStr);

            ControlDpiExtensions.AdjustForDpiScaling(this);

            Visible = true;
        }

        private void ResolveButton_Click(object sender, EventArgs e)
        {
            Form?.UICommands.StartResolveConflictsDialog(this);
        }

        private void ContinueButton_Click(object sender, EventArgs e)
        {
            if (Form is null)
            {
                return;
            }

            switch (_action)
            {
                case GitAction.Rebase:
                    FormProcess.ShowDialog(Form, UICommands, arguments: Commands.ContinueRebase(), Module.WorkingDir, input: null, useDialogSettings: true);
                    break;
                case GitAction.Merge:
                    FormProcess.ShowDialog(Form, UICommands, arguments: Commands.ContinueMerge(), Module.WorkingDir, input: null, useDialogSettings: true);
                    break;
                case GitAction.Patch:
                    FormProcess.ShowDialog(Form, UICommands, arguments: Commands.Resolved(), Module.WorkingDir, input: null, useDialogSettings: true);
                    break;
                default:
                    return;
            }

            Form.UICommands.RepoChangedNotifier.Notify();
        }

        private void AbortButton_Click(object sender, EventArgs e)
        {
            if (Form is null)
            {
                return;
            }

            switch (_action)
            {
                case GitAction.Rebase:
                    FormProcess.ShowDialog(Form, UICommands, arguments: Commands.AbortRebase(), Module.WorkingDir, input: null, useDialogSettings: true);
                    break;
                case GitAction.Merge:
                    FormProcess.ShowDialog(Form, UICommands, arguments: Commands.AbortMerge(), Module.WorkingDir, input: null, useDialogSettings: true);
                    break;
                case GitAction.Patch:
                    FormProcess.ShowDialog(Form, UICommands, arguments: Commands.Abort(), Module.WorkingDir, input: null, useDialogSettings: true);
                    break;
                default:
                    return;
            }

            Form.UICommands.RepoChangedNotifier.Notify();
        }

        private void MoreButton_Click(object sender, EventArgs e)
        {
            if (Form is null)
            {
                return;
            }

            switch (_action)
            {
                case GitAction.Bisect:
                    if (Form is not FormBrowse formBrowse)
                    {
                        return;
                    }

                    using (FormBisect frm = new(formBrowse.RevisionGridControl))
                    {
                        frm.ShowDialog(this);
                    }

                    Form.UICommands.RepoChangedNotifier.Notify();
                    break;
                case GitAction.Rebase:
                    Form.UICommands.StartTheContinueRebaseDialog(Form);
                    break;
                case GitAction.Patch:
                    Form.UICommands.StartApplyPatchDialog(Form);
                    break;
            }
        }

        internal TestAccessor GetTestAccessor()
            => new(this);

        internal readonly struct TestAccessor
        {
            private readonly InteractiveGitActionControl _interactiveGitActionControl;

            internal TestAccessor(InteractiveGitActionControl interactiveGitActionControl)
            {
                _interactiveGitActionControl = interactiveGitActionControl;
            }

            internal GitAction Action => _interactiveGitActionControl._action;
            internal bool HasConflicts => _interactiveGitActionControl._hasConflicts;

            internal bool Visible => _interactiveGitActionControl.Visible;
            internal ControlCollection Controls => _interactiveGitActionControl.ButtonContainer.Controls;

            internal Button ResolveButton => _interactiveGitActionControl.ResolveButton;
            internal Button ContinueButton => _interactiveGitActionControl.ContinueButton;
            internal Button AbortButton => _interactiveGitActionControl.AbortButton;
            internal Button MoreButton => _interactiveGitActionControl.MoreButton;

            internal void SetGitAction(GitAction action, bool conflicts) => _interactiveGitActionControl.SetGitAction(action, conflicts);
        }
    }
}
