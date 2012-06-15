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
                using (var frm = new FormProcess(string.Format("add -f \"{0}\"", Filter.Text))) frm.ShowDialog(this);
            else
                using (var frm = new FormProcess(string.Format("add \"{0}\"", Filter.Text))) frm.ShowDialog(this);
            Close();
        }

        private void ShowFilesClick(object sender, EventArgs e)
        {
            using (var frm = new FormProcess(string.Format("add --dry-run \"{0}\"", Filter.Text))) frm.ShowDialog(this);
        }
    }
}