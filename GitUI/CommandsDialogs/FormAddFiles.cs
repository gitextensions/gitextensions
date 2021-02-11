using System;
using GitUI.HelperDialogs;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormAddFiles : GitModuleForm
    {
        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormAddFiles()
        {
            InitializeComponent();
        }

        public FormAddFiles(GitUICommands commands, string? addFile = null)
            : base(commands)
        {
            InitializeComponent();
            InitializeComplete();
            Filter.Text = addFile ?? ".";
        }

        private void ShowFilesClick(object sender, EventArgs e)
        {
            string arguments = string.Format("add --dry-run{0} \"{1}\"", force.Checked ? " -f" : "", Filter.Text);
            FormProcess.ShowDialog(this, process: null, arguments, Module.WorkingDir, input: null, useDialogSettings: false);
        }

        private void AddFilesClick(object sender, EventArgs e)
        {
            string arguments = string.Format("add{0} \"{1}\"", force.Checked ? " -f" : "", Filter.Text);
            if (FormProcess.ShowDialog(this, process: null, arguments, Module.WorkingDir, input: null, useDialogSettings: false))
            {
                Close();
            }
        }
    }
}
