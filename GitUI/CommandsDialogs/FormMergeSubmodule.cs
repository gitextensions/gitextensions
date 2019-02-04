using System;
using System.Diagnostics;
using System.Windows.Forms;
using GitCommands;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormMergeSubmodule : GitModuleForm
    {
        private readonly TranslationString _stageFilename = new TranslationString("Stage {0}");
        private readonly TranslationString _deleted = new TranslationString("deleted");

        private readonly string _filename;

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormMergeSubmodule()
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
            var item = Module.GetConflict(_filename);

            tbBase.Text = item.Base.ObjectId?.ToString() ?? _deleted.Text;
            tbLocal.Text = item.Local.ObjectId?.ToString() ?? _deleted.Text;
            tbRemote.Text = item.Remote.ObjectId?.ToString() ?? _deleted.Text;
            tbCurrent.Text = Module.GetSubmodule(_filename).GetCurrentCheckout()?.ToString() ?? "";
        }

        private void btRefresh_Click(object sender, EventArgs e)
        {
            tbCurrent.Text = Module.GetSubmodule(_filename).GetCurrentCheckout()?.ToString() ?? "";
        }

        private void StageSubmodule()
        {
            using (var form = new FormStatus(ProcessStart, string.Format(_stageFilename.Text, _filename)))
            {
                form.ShowDialogOnError(this);
            }

            void ProcessStart(FormStatus form)
            {
                form.AddMessageLine(string.Format(_stageFilename.Text, _filename));
                var args = new GitArgumentBuilder("add")
                {
                    "--",
                    _filename.QuoteNE()
                };
                string output = Module.GitExecutable.GetOutput(args);
                form.AddMessageLine(output);
                form.Done(isSuccess: string.IsNullOrWhiteSpace(output));
            }
        }

        private void btStageCurrent_Click(object sender, EventArgs e)
        {
            StageSubmodule();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btOpenSubmodule_Click(object sender, EventArgs e)
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = Application.ExecutablePath,
                    Arguments = "browse",
                    WorkingDirectory = Module.GetSubmoduleFullPath(_filename)
                }
            };

            process.Start();
        }

        private void btCheckoutBranch_Click(object sender, EventArgs e)
        {
            var revisions = new[] { ObjectId.Parse(tbLocal.Text), ObjectId.Parse(tbRemote.Text) };
            var submoduleCommands = new GitUICommands(Module.GetSubmoduleFullPath(_filename));
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