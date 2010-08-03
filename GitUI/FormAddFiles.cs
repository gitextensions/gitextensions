using System;

namespace GitUI
{
    public partial class FormAddFiles : GitExtensionsForm
    {
        public FormAddFiles()
        {
            InitializeComponent();
            Translate();
        }

        private void AddFilesClick(object sender, EventArgs e)
        {
            if (force.Checked)
                new FormProcess(string.Format("add -f \"{0}\"", Filter.Text)).ShowDialog();
            else
                new FormProcess(string.Format("add \"{0}\"", Filter.Text)).ShowDialog();
            Close();
        }

        private void ShowFilesClick(object sender, EventArgs e)
        {
            new FormProcess(string.Format("add --dry-run \"{0}\"", Filter.Text)).ShowDialog();
        }
    }
}