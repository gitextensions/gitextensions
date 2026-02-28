using GitExtensions.Extensibility.Git;
using GitUIPluginInterfaces;

namespace GitUI.CommandsDialogs;

public partial class FormBlame : GitModuleForm
{
    public string FileName { get; }

    private FormBlame(IGitUICommands commands) : base(commands)
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

        _ = blameControl1.LoadBlameAsync(revision ?? Module.GetRevision(), children: null, fileName, revisionGridInfo: null, revisionGridFileUpdate: null, controlToMask: null, Module.FilesEncoding, initialLine);
        blameControl1.ConfigureRepositoryHostPlugin(PluginRegistry.TryGetGitHosterForModule(Module));
    }

    private void FormBlameLoad(object sender, EventArgs e)
    {
        Text = $"Blame ({FileName})";
    }
}
