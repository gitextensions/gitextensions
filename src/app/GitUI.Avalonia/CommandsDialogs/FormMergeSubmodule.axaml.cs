using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI.HelperDialogs;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs;

// Twin of GitUI/CommandsDialogs/FormMergeSubmodule.cs. The control names and command flow
// stay aligned with WinForms; opening a submodule uses another in-process Avalonia browser.
public sealed partial class FormMergeSubmodule : GitModuleForm
{
    private readonly TranslationString _stageFilename = new("Stage {0}");
    private readonly TranslationString _deleted = new("deleted");

    private readonly string _filename = string.Empty;

    public FormMergeSubmodule()
    {
        InitializeComponent();
        InitializeStaticContent();
        InitializeComplete();
    }

    public FormMergeSubmodule(IGitUICommands commands, string filename)
        : base(commands, true)
    {
        _filename = filename;

        InitializeComponent();
        InitializeStaticContent();
        InitializeComplete();
        lbSubmodule.Text = filename;
    }

    protected override void OnRuntimeLoad(EventArgs e)
    {
        base.OnRuntimeLoad(e);
        RefreshConflict();
    }

    private void InitializeStaticContent()
    {
        btRefresh.Click += btRefresh_Click;
        btStageCurrent.Click += btStageCurrent_Click;
        btOpenSubmodule.Click += btOpenSubmodule_Click;
        btCheckoutBranch.Click += btCheckoutBranch_Click;
        AcceptButton = btOpenSubmodule;
    }

    private void RefreshConflict()
    {
        ConflictData item = ThreadHelper.JoinableTaskFactory.Run(() => Module.GetConflictAsync(_filename));

        tbBase.Text = item.Base.ObjectId.IsZero ? _deleted.Text : item.Base.ObjectId.ToString();
        tbLocal.Text = item.Local.ObjectId.IsZero ? _deleted.Text : item.Local.ObjectId.ToString();
        tbRemote.Text = item.Remote.ObjectId.IsZero ? _deleted.Text : item.Remote.ObjectId.ToString();
        RefreshCurrentRevision();
        btCheckoutBranch.IsEnabled = !item.Base.ObjectId.IsZero && !item.Remote.ObjectId.IsZero;
    }

    private void btRefresh_Click(object? sender, EventArgs e)
        => RefreshCurrentRevision();

    private void RefreshCurrentRevision()
    {
        tbCurrent.Text = Module.GetSubmodule(_filename).GetCurrentCheckout() is { IsZero: false } id
            ? id.ToString()
            : string.Empty;
    }

    private void StageSubmodule()
    {
        GitArgumentBuilder arguments = new("add")
        {
            "--",
            _filename.QuoteNE()
        };
        string output = Module.GitExecutable.GetOutput(arguments);

        if (string.IsNullOrWhiteSpace(output))
        {
            return;
        }

        string text = string.Format(_stageFilename.Text, _filename);
        FormStatus.ShowErrorDialog(this, UICommands, text, text, output);
    }

    private void btStageCurrent_Click(object? sender, EventArgs e)
    {
        StageSubmodule();
        DialogResult = WinFormsShims.DialogResult.OK;
        Close();
    }

    private void btOpenSubmodule_Click(object? sender, EventArgs e)
    {
        FormBrowse form = new(UICommands.WithWorkingDirectory(Module.GetSubmoduleFullPath(_filename)));
        form.Show();
    }

    private void btCheckoutBranch_Click(object? sender, EventArgs e)
    {
        ObjectId[] ids = [ObjectId.Parse(tbLocal.Text!), ObjectId.Parse(tbRemote.Text!)];
        IGitUICommands submoduleCommands = UICommands.WithWorkingDirectory(Module.GetSubmoduleFullPath(_filename));
        if (!submoduleCommands.StartCheckoutBranch(this, ids))
        {
            return;
        }

        StageSubmodule();
        DialogResult = WinFormsShims.DialogResult.OK;
        Close();
    }
}
