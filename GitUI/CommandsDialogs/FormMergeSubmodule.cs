using System;
using System.Windows.Forms;
using GitCommands;
using GitExtUtils;
using GitUI.HelperDialogs;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormMergeSubmodule : GitModuleForm
    {
        private readonly TranslationString _stageFilename = new("Stage {0}");
        private readonly TranslationString _deleted = new("deleted");

        private readonly string _filename;

        [Obsolete("For VS designer and translation test only. Do not remove.")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private FormMergeSubmodule()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            InitializeComponent();
        }

        public FormMergeSubmodule(GitUICommands commands, string filename)
            : base(commands)
        {
            InitializeComponent();
            InitializeComplete();
            lbSubmodule.Text = filename;
            _filename = filename;
        }

        private void FormMergeSubmodule_Load(object sender, EventArgs e)
        {
            var item = ThreadHelper.JoinableTaskFactory.Run(() => Module.GetConflictAsync(_filename));

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
            FormStatus.ShowErrorDialog(this, text, text, output);
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
            var revisions = new[] { ObjectId.Parse(tbLocal.Text), ObjectId.Parse(tbRemote.Text) };
            GitUICommands submoduleCommands = new(Module.GetSubmoduleFullPath(_filename));
            if (!submoduleCommands.StartCheckoutBranch(this, revisions))
            {
                return;
            }

            StageSubmodule();
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
