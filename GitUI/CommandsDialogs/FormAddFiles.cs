using System;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormAddFiles : GitModuleForm
    {
        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormAddFiles()
        {
            InitializeComponent();
        }

        public FormAddFiles(GitUICommands commands, string addFile = null)
            : base(commands)
        {
            InitializeComponent();
            InitializeComplete();
            Filter.Text = addFile ?? ".";
        }

        private void ShowFilesClick(object sender, EventArgs e)
        {
            FormProcess.ShowDialog(this, string.Format("add --dry-run{0} \"{1}\"", force.Checked ? " -f" : "", Filter.Text), false);
        }

        private void AddFilesClick(object sender, EventArgs e)
        {
            if (FormProcess.ShowDialog(this, string.Format("add{0} \"{1}\"", force.Checked ? " -f" : "", Filter.Text), false))
            {
                Close();
            }
        }
    }
}