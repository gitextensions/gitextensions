using System;
using System.Diagnostics;
using System.Windows.Forms;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormMergeSubmodule : GitModuleForm
    {
        readonly string _filename;
        private readonly TranslationString _stageFilename = new TranslationString("Stage {0}");
        private readonly TranslationString _deleted = new TranslationString("deleted");

        public FormMergeSubmodule(GitUICommands aCommands, string filename)
            : base(aCommands)
        {
            InitializeComponent();
            Translate();
            lbSubmodule.Text = filename;
            this._filename = filename;
        }

        private void FormMergeSubmodule_Load(object sender, EventArgs e)
        {
            var item = Module.GetConflict(_filename);
            this.tbBase.Text = item.Base.Hash ?? _deleted.Text;
            this.tbLocal.Text = item.Local.Hash ?? _deleted.Text;
            this.tbRemote.Text = item.Remote.Hash ?? _deleted.Text;
            this.tbCurrent.Text = Module.GetSubmodule(_filename).GetCurrentCheckout();
        }

        private void btRefresh_Click(object sender, EventArgs e)
        {
            this.tbCurrent.Text = Module.GetSubmodule(_filename).GetCurrentCheckout();
        }

        private void StageSubmodule()
        {
            var processStart = new FormStatus.ProcessStart(delegate(FormStatus form)
                {
                    form.AddMessageLine(string.Format(_stageFilename.Text,
                        _filename));
                    string output = Module.RunGitCmd("add -- \"" + _filename + "\"");
                    form.AddMessageLine(output);
                    form.Done(string.IsNullOrEmpty(output));
                });
            using (var process = new FormStatus(processStart, null) { Text = string.Format(_stageFilename.Text, _filename) })
                process.ShowDialogOnError(this);
        }

        private void btStageCurrent_Click(object sender, EventArgs e)
        {
            StageSubmodule();
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btOpenSubmodule_Click(object sender, EventArgs e)
        {
            Process process = new Process();
            process.StartInfo.FileName = Application.ExecutablePath;
            process.StartInfo.Arguments = "browse";
            process.StartInfo.WorkingDirectory = Module.GetSubmoduleFullPath(_filename);
            process.Start();
        }

        private void btCheckoutBranch_Click(object sender, EventArgs e)
        {
            string[] revisions = { tbLocal.Text, tbRemote.Text };
            var submoduleCommands = new GitUICommands(Module.GetSubmoduleFullPath(_filename));
            if (!submoduleCommands.StartCheckoutBranch(this, revisions))
                return;
            StageSubmodule();
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}