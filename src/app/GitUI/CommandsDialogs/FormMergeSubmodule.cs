using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI.HelperDialogs;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormMergeSubmodule : GitModuleForm
    {
        private readonly TranslationString _stageFilename = new("Stage {0}");
        private readonly TranslationString _deleted = new("deleted");

        private readonly string _filename;

        public FormMergeSubmodule(IGitUICommands commands, string filename)
            : base(commands)
        {
            InitializeComponent();
            InitializeComplete();
            lbSubmodule.Text = filename;
            _filename = filename;
        }

        private void FormMergeSubmodule_Load(object sender, EventArgs e)
        {
            ConflictData item = ThreadHelper.JoinableTaskFactory.Run(() => Module.GetConflictAsync(_filename));

            tbBase.Text = item.Base.ObjectId?.ToString() ?? _deleted.Text;
            tbLocal.Text = item.Local.ObjectId?.ToString() ?? _deleted.Text;
            tbRemote.Text = item.Remote.ObjectId?.ToString() ?? _deleted.Text;
            tbCurrent.Text = Module.GetSubmodule(_filename).GetCurrentCheckout()?.ToString() ?? "";
            btCheckoutBranch.Enabled = item.Base.ObjectId is not null && item.Remote.ObjectId is not null;
        }

        private void btRefresh_Click(object sender, EventArgs e)
        {
            tbCurrent.Text = Module.GetSubmodule(_filename).GetCurrentCheckout()?.ToString() ?? "";
        }

        private void StageSubmodule()
        {
            GitArgumentBuilder args = new("add")
            {
                "--",
                _filename.QuoteNE()
            };
            string output = Module.GitExecutable.GetOutput(args);

            if (string.IsNullOrWhiteSpace(output))
            {
                return;
            }

            string text = string.Format(_stageFilename.Text, _filename);
            FormStatus.ShowErrorDialog(this, UICommands, text, text, output);
        }

        private void btStageCurrent_Click(object sender, EventArgs e)
        {
            StageSubmodule();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btOpenSubmodule_Click(object sender, EventArgs e)
        {
            GitUICommands.LaunchBrowse(workingDir: Module.GetSubmoduleFullPath(_filename));
        }

        private void btCheckoutBranch_Click(object sender, EventArgs e)
        {
            ObjectId[] ids = new[] { ObjectId.Parse(tbLocal.Text), ObjectId.Parse(tbRemote.Text) };
            IGitUICommands submoduleCommands = UICommands.WithWorkingDirectory(Module.GetSubmoduleFullPath(_filename));
            if (!submoduleCommands.StartCheckoutBranch(this, ids))
            {
                return;
            }

            StageSubmodule();
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
