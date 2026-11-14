using System.ComponentModel;
using GitCommands.Git;
using GitExtensions.Extensibility.Git;
using GitExtUtils.GitUI.Theming;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.HelperDialogs;
using ResourceManager;

namespace GitUI.UserControls;

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

    private readonly CancellationTokenSequence _refreshGitActionSequence = new();

    // The repo which the banner has been shown for
    private IGitModule? _actionModule;
    private GitAction _action;
    private bool _hasConflicts;

    private GitModuleForm? Form => FindForm() as GitModuleForm;

    public InteractiveGitActionControl()
    {
        InitializeComponent();
        BackColor.AdaptBackColor();
        InitializeComplete();

        // The buttons are laid out by the docked ButtonContainer, which is as high as this control
        // and no higher, so the captions would be cut off unless the height the buttons need is
        // reserved here. Measuring covers an enlarged system font as well, which the DPI scaling
        // of the designer values does not see.
        MinimumSize = new Size(0, ButtonContainer.GetPreferredSize(Size.Empty).Height);
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
    /// The banner is updated when the state of the repo is known, which requires a Git command.
    /// The command is run in the background, so that it does not block the UI thread, e.g. while the window is shown for the first time.
    /// The banner of another repo is hidden until then.
    /// </summary>
    /// <param name="checkForConflicts">Allow running Git command to check for conflicts.</param>
    public void RefreshGitAction(bool checkForConflicts)
    {
        // get the current state of the repo

        IGitModule module = Module;
        if (!module.IsValidGitWorkingDir())
        {
            _refreshGitActionSequence.CancelCurrent();
            return;
        }

        CancellationToken cancellationToken = _refreshGitActionSequence.Next();

        if (!ReferenceEquals(module, _actionModule))
        {
            // The buttons act on the repo which is open now, so the banner of the previous repo
            // must not stay usable until the Git command for this repo has finished.
            _actionModule = module;
            SetGitAction(GitAction.None, hasConflicts: false);
        }

        ThreadHelper.FileAndForget(async () =>
        {
            (GitAction action, bool hasConflicts) = GetGitAction(module, checkForConflicts);

            await this.SwitchToMainThreadAsync(cancellationToken);
            SetGitAction(action, hasConflicts);
        });

        return;

        static (GitAction Action, bool HasConflicts) GetGitAction(IGitModule module, bool checkForConflicts)
        {
            bool hasConflicts;
            try
            {
                // This command can be executed seemingly in the background (selecting Browse),
                // do not notify the user (this can occur if Git is upgraded).
                // Running Git commands async when restoring may fail.
                hasConflicts = checkForConflicts && module.InTheMiddleOfConflictedMerge(throwOnErrorExit: false);
            }
            catch (Win32Exception)
            {
                hasConflicts = false;
            }

            if (module.InTheMiddleOfRebase())
            {
                return (GitAction.Rebase, hasConflicts);
            }

            if (module.InTheMiddleOfMerge())
            {
                return (GitAction.Merge, hasConflicts);
            }

            if (module.InTheMiddleOfPatch())
            {
                return (GitAction.Patch, hasConflicts);
            }

            return (GitAction.None, hasConflicts);
        }
    }

    protected override void DisposeCustomResources()
    {
        _refreshGitActionSequence.Dispose();
        base.DisposeCustomResources();
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
