using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI.Compat;
using GitUIPluginInterfaces;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.HelperDialogs;

public partial class FormResetAnotherBranch : GitModuleForm
{
    private readonly CancellationTokenSequence _cancellationTokenSequence = new();
    private readonly TranslationString _localRefInvalid = new("The entered value '{0}' is not the name of an existing local branch.");
    private readonly GitRevision _revision = null!;

    private IGitRef[]? _localGitRefs;
    private string? _validatedBranch;

    public static FormResetAnotherBranch Create(IGitUICommands commands, GitRevision revision)
        => new(commands, revision ?? throw new NotSupportedException(TranslatedStrings.NoRevision));

    // parity-scaffolding: Avalonia's view inventory and designer require a parameterless constructor.
    public FormResetAnotherBranch()
    {
        InitializeComponent();
        InitializeComplete();
    }

    private FormResetAnotherBranch(IGitUICommands commands, GitRevision revision)
        : base(commands, enablePositionRestore: true)
    {
        _revision = revision;

        InitializeComponent();

        ActiveControl = Branches;

        Ok.Click += Ok_Click;
        Cancel.Click += Cancel_Click;
        ForceReset.IsCheckedChanged += Validate;
        Branches.DropDownClosed += Validate;
        Branches.GotFocus += Validate;
        Branches.KeyUp += Branches_KeyUp;
        Branches.PropertyChanged += (sender, args) =>
        {
            if (args.Property == ComboBox.TextProperty)
            {
                Validate(sender, EventArgs.Empty);
            }
        };

        InitializeComplete();

        cbxCheckoutBranch.IsChecked = AppSettings.CheckoutOtherBranchAfterReset.Value;
        cbxCheckoutBranch.IsCheckedChanged += (s, e) => AppSettings.CheckoutOtherBranchAfterReset.Value = cbxCheckoutBranch.IsChecked == true;

        Ok.IsEnabled = false;
    }

    private void InitLocalBranchesWithoutCurrent()
    {
        string currentBranch = Module.GetSelectedBranch();
        bool isDetachedHead = currentBranch == DetachedHeadParser.DetachedBranch;

        List<IGitRef> selectedRevisionRemotes = [.. _revision.Refs.Where(r => r.IsRemote)];

        _localGitRefs = [.. Module.GetRefs(RefsFilter.Heads)
            .Where(r => r.IsHead)
            .Where(r => isDetachedHead || r.LocalName != currentBranch)
            .Where(r => _revision.ObjectId != r.ObjectId) // Don't display local branches already at this revision
            .OrderByDescending(r => selectedRevisionRemotes.Any(r.IsTrackingRemote)) // Put local branches that track these remotes first
            .ThenByDescending(r => selectedRevisionRemotes.Any(r2 => r2.LocalName == r.LocalName))];

        if (selectedRevisionRemotes.Count == 1)
        {
            IGitRef availableRemote = selectedRevisionRemotes[0];
            IGitRef[] defaultCandidateRefs = [.. _localGitRefs.Where(r => r.IsTrackingRemote(availableRemote) || r.LocalName == availableRemote.LocalName)];
            if (defaultCandidateRefs.Length == 1)
            {
                Branches.Text = defaultCandidateRefs[0].Name;
            }
        }
    }

    protected override void OnRuntimeLoad(EventArgs e)
    {
        base.OnRuntimeLoad(e);

        InitLocalBranchesWithoutCurrent();

        // WinForms bound the ComboBox items with DisplayMember=Name; the Avalonia editable combo
        // sets its text from the item's string, so the branch names themselves are the items.
        Branches.ItemsSource = _localGitRefs!.Select(r => r.Name).ToList();

        commitSummaryUserControl.Revision = _revision;

        // WinForms opened the drop-down from the first Application.Idle when no default was set.
        Dispatcher.UIThread.Post(() =>
        {
            if (Branches.Text?.Length is null or 0)
            {
                Branches.IsDropDownOpen = true;
            }
        });
    }

    private void Ok_Click(object? sender, EventArgs e)
    {
        IGitRef? gitRefToReset = _localGitRefs!.FirstOrDefault(b => b.Name == Branches.Text);
        if (gitRefToReset is null)
        {
            MessageBoxes.Show(this, string.Format(_localRefInvalid.Text, Branches.Text), TranslatedStrings.Error, WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Error);
            return;
        }

        ArgumentString command = Commands.UpdateRef(gitRefToReset.CompleteName, _revision.ObjectId);
        bool success = FormProcess.ShowDialog(this, UICommands, arguments: command, Module.WorkingDir, input: null, useDialogSettings: true);
        if (success)
        {
            if (cbxCheckoutBranch.IsChecked == true)
            {
                UICommands.StartCheckoutBranch(this, gitRefToReset.Name);
            }

            UICommands.RepoChangedNotifier.Notify();
            Close();
        }
    }

    private void Cancel_Click(object? sender, EventArgs e)
    {
        Close();
    }

    private void Branches_KeyUp(object? sender, KeyEventArgs e)
    {
        if (!Branches.IsDropDownOpen && e.Key is not (Key.LeftAlt or Key.RightAlt or Key.Enter or Key.Escape))
        {
            // The Avalonia editable combo preserves its text and caret across opening, so the
            // WinForms save/restore dance around DroppedDown is not needed here.
            Branches.IsDropDownOpen = true;
        }
    }

    private void Validate(object? sender, EventArgs e)
    {
        string branch = Branches.Text ?? string.Empty;

        if (_localGitRefs is null || (branch == _validatedBranch && ForceReset.IsChecked != true))
        {
            return;
        }

        _validatedBranch = null;
        CancellationToken cancellationToken = _cancellationTokenSequence.Next();

        IGitRef? gitRefToReset = _localGitRefs.FirstOrDefault(b => b.Name == branch);
        SetInvalidBackground(Branches, gitRefToReset is null && !Branches.IsKeyboardFocusWithin);

        Ok.IsEnabled = gitRefToReset is not null && ForceReset.IsChecked == true;
        SetInvalidBackground(Ok, invalid: false);

        if (gitRefToReset is null || ForceReset.IsChecked == true)
        {
            return;
        }

        _validatedBranch = branch;

        ThreadHelper.FileAndForget(async () =>
        {
            ArgumentString command = new GitExtUtils.GitArgumentBuilder("merge-base")
            {
                "--is-ancestor",
                gitRefToReset.CompleteName.QuoteNE(),
                _revision.ObjectId,
            };
            ExecutionResult executionResult = await Module.GitExecutable.ExecuteAsync(command, throwOnErrorExit: false, cancellationToken: cancellationToken);

            await this.SwitchToMainThreadAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            Ok.IsEnabled = executionResult.ExitedSuccessfully;
            if (!executionResult.ExitedSuccessfully)
            {
                SetInvalidBackground(Ok, invalid: true);
            }
        });
    }

    // WinForms tinted the invalid field/button with an adapted LightCoral; the twin uses the
    // shared semantic invalid-input brush and clears back to the theme default when valid.
    private static void SetInvalidBackground(TemplatedControl control, bool invalid)
    {
        if (invalid && ResourceNodeExtensions.TryFindResource(control, "GitExtensionsInvalidFilterBackgroundBrush", out object? resource) && resource is IBrush brush)
        {
            control.Background = brush;
        }
        else
        {
            control.ClearValue(TemplatedControl.BackgroundProperty);
        }
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(FormResetAnotherBranch form)
    {
        public ComboBox Branches => form.Branches;
        public Button Ok => form.Ok;
        public Button Cancel => form.Cancel;
        public CheckBox ForceReset => form.ForceReset;
        public CheckBox CheckoutBranch => form.cbxCheckoutBranch;
        public GitRevision? SummaryRevision => form.commitSummaryUserControl.Revision;

        public void Load() => form.OnRuntimeLoad(EventArgs.Empty);
    }
}
