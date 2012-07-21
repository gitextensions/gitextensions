using System;

namespace GitUI
{
    public sealed partial class FormAddFiles : GitExtensionsForm
    {
        public FormAddFiles()
        {
            InitializeComponent();
            Translate();
        }

        private void AddFilesClick(object sender, EventArgs e)
        {
            var argumentFormat = force.Checked ? "add -f \"{0}\"" : "add \"{0}\"";
            FormProcess.ShowDialog(this, string.Format(argumentFormat, Filter.Text));
        }

        private void ShowFilesClick(object sender, EventArgs e)
        {
            FormProcess.ShowDialog(this, string.Format("add --dry-run \"{0}\"", Filter.Text));
        }
    }
}