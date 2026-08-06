using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Plugins;
using GitUIPluginInterfaces;

namespace GitUI.CommandsDialogs;

public partial class FormBlame : GitModuleForm
{
    public string? FileName { get; }

    // parity-scaffolding: Avalonia's view inventory and designer require a parameterless constructor.
    public FormBlame()
    {
        InitializeComponent();
        InitializeComplete();
    }

    private FormBlame(IGitUICommands commands)
        : base(commands, enablePositionRestore: false)
    {
        InitializeComponent();
        InitializeComplete();
    }

    public FormBlame(IGitUICommands commands, string fileName, GitRevision? revision, int? initialLine = null)
        : this(commands)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        FileName = fileName;

        // The Avalonia BlameControl.LoadBlameAsync twin drops the WinForms children and
        // controlToMask parameters, which have no Avalonia equivalent.
        _ = blameControl1.LoadBlameAsync(revision ?? Module.GetRevision(), fileName, revisionGridInfo: null, revisionGridFileUpdate: null, Module.FilesEncoding, initialLine, joinableTaskFactory: ThreadHelper.JoinableTaskFactory);
        blameControl1.ConfigureRepositoryHostPlugin(PluginRegistry.TryGetGitHosterForModule(Module));
    }

    // WinForms wired the title to the Load event; the twin uses the runtime-load override.
    protected override void OnRuntimeLoad(EventArgs e)
    {
        base.OnRuntimeLoad(e);
        Text = $"Blame ({FileName})";
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(FormBlame form)
    {
        public Blame.BlameControl BlameControl => form.blameControl1;
    }
}
