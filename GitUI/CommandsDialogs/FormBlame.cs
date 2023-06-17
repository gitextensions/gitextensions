using GitUIPluginInterfaces;

namespace GitUI.CommandsDialogs
{
    public partial class FormBlame : GitModuleForm
    {
        public string FileName { get; }

        [Obsolete("For VS designer and translation test only. Do not remove.")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private FormBlame()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            InitializeComponent();
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private FormBlame(GitUICommands commands)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
            : base(commands)
        {
            InitializeComponent();
            InitializeComplete();
        }

        public FormBlame(GitUICommands commands, string fileName, GitRevision? revision, int? initialLine = null)
            : this(commands)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            FileName = fileName;

            _ = blameControl1.LoadBlameAsync(revision ?? Module.GetRevision(), children: null, fileName, revisionGridInfo: null, revisionGridUpdate: null, controlToMask: null, Module.FilesEncoding, initialLine);
            blameControl1.ConfigureRepositoryHostPlugin(PluginRegistry.TryGetGitHosterForModule(Module));
        }

        private void FormBlameLoad(object sender, EventArgs e)
        {
            Text = $"Blame ({FileName})";
        }
    }
}
