using Avalonia.Controls;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI.HelperDialogs;
using GitUIPluginInterfaces;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs.BrowseDialog;

public sealed partial class FormBisect : GitModuleForm
{
    // TODO: Improve me
    private readonly TranslationString _bisectStart =
        new("Mark selected revisions as start bisect range?");

    private readonly IRevisionGridInfo _revisionGridInfo = null!;

    // parity-scaffolding: Avalonia's view inventory and designer require a parameterless constructor.
    public FormBisect()
    {
        InitializeComponent();
        InitializeComplete();
    }

    public FormBisect(RevisionGridControl revisionGrid)
        : base(revisionGrid.UICommands, enablePositionRestore: true)
    {
        _revisionGridInfo = revisionGrid;
        InitializeComponent();
        Start.Click += Start_Click;
        Good.Click += Good_Click;
        Bad.Click += Bad_Click;
        Stop.Click += Stop_Click;
        btnSkip.Click += btnSkip_Click;
        InitializeComplete();
        UpdateButtonsState();
    }

    private void UpdateButtonsState()
    {
        bool inTheMiddleOfBisect = Module.InTheMiddleOfBisect();
        Start.IsEnabled = !inTheMiddleOfBisect;
        Good.IsEnabled = inTheMiddleOfBisect;
        Bad.IsEnabled = inTheMiddleOfBisect;
        Stop.IsEnabled = inTheMiddleOfBisect;
        btnSkip.IsEnabled = inTheMiddleOfBisect;
    }

    private void Start_Click(object? sender, EventArgs e)
    {
        FormProcess.ShowDialog(this, UICommands, arguments: Commands.StartBisect(), Module.WorkingDir, input: null, useDialogSettings: true);

        UpdateButtonsState();

        IReadOnlyList<GitRevision> revisions = _revisionGridInfo.GetSelectedRevisions();
        if (revisions.Count > 1)
        {
            if (MessageBoxes.Show(this, _bisectStart.Text, Text ?? string.Empty, WinFormsShims.MessageBoxButtons.YesNo, WinFormsShims.MessageBoxIcon.Question) == WinFormsShims.DialogResult.Yes)
            {
                BisectRange(revisions[0].ObjectId, revisions[^1].ObjectId);
                Close();
            }
        }

        return;

        void BisectRange(ObjectId startObjectId, ObjectId endObjectId)
        {
            ArgumentString command = Commands.ContinueBisect(GitBisectOption.Good, startObjectId);
            bool success = FormProcess.ShowDialog(this, UICommands, arguments: command, Module.WorkingDir, input: null, useDialogSettings: true);
            if (!success)
            {
                return;
            }

            command = Commands.ContinueBisect(GitBisectOption.Bad, endObjectId);
            FormProcess.ShowDialog(this, UICommands, arguments: command, Module.WorkingDir, input: null, useDialogSettings: true);
        }
    }

    private void Good_Click(object? sender, EventArgs e)
    {
        ContinueBisect(GitBisectOption.Good);
    }

    private void Bad_Click(object? sender, EventArgs e)
    {
        ContinueBisect(GitBisectOption.Bad);
    }

    private void Stop_Click(object? sender, EventArgs e)
    {
        FormProcess.ShowDialog(this, UICommands, arguments: Commands.StopBisect(), Module.WorkingDir, input: null, useDialogSettings: false);
        Close();
    }

    private void btnSkip_Click(object? sender, EventArgs e)
    {
        ContinueBisect(GitBisectOption.Skip);
    }

    private void ContinueBisect(GitBisectOption bisectOption)
    {
        FormProcess.ShowDialog(this, UICommands, arguments: Commands.ContinueBisect(bisectOption), Module.WorkingDir, input: null, useDialogSettings: false);
        Close();
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(FormBisect form)
    {
        public Button Start => form.Start;
        public Button Good => form.Good;
        public Button Bad => form.Bad;
        public Button Stop => form.Stop;
        public Button btnSkip => form.btnSkip;
    }
}
