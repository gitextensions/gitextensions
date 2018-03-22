using System;
using System.Diagnostics;
using System.Windows.Forms;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormMergeSubmodule : GitModuleForm
    {
        private readonly string _filename;
        private readonly TranslationString _stageFilename = new TranslationString("Stage {0}");
        private readonly TranslationString _deleted = new TranslationString("deleted");

        public FormMergeSubmodule(GitUICommands commands, string filename)
            : base(commands)
        {
            InitializeComponent();
            Translate();
            lbSubmodule.Text = filename;
            _filename = filename;
        }

        private void FormMergeSubmodule_Load(object sender, EventArgs e)
        {
            var item = Module.GetConflict(_filename);
            tbBase.Text = item.Base.Hash ?? _deleted.Text;
            tbLocal.Text = item.Local.Hash ?? _deleted.Text;
            tbRemote.Text = item.Remote.Hash ?? _deleted.Text;
            tbCurrent.Text = Module.GetSubmodule(_filename).GetCurrentCheckout();
        }

        private void btRefresh_Click(object sender, EventArgs e)
        {
            tbCurrent.Text = Module.GetSubmodule(_filename).GetCurrentCheckout();
        }

        private void StageSubmodule()
        {
            void ProcessStart(FormStatus form)
            {
                form.AddMessageLine(
                    string.Format(
                        _stageFilename.Text, _filename));
                string output = Module.RunGitCmd("add -- \"" + _filename + "\"");
                form.AddMessageLine(output);
                form.Done(string.IsNullOrEmpty(output));
            }

            using (var process = new FormStatus(ProcessStart, null) { Text = string.Format(_stageFilename.Text, _filename) })
            {
                process.ShowDialogOnError(this);
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
            string[] revisions = { tbLocal.Text, tbRemote.Text };
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