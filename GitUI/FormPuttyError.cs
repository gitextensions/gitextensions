using System;

namespace GitUI
{
    public partial class FormPuttyError : GitExtensionsForm
    {
        public FormPuttyError()
        {
            InitializeComponent(); Translate();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void LoadSSHKey_Click(object sender, EventArgs e)
        {
            new FormLoadPuttySshKey().ShowDialog(this);
        }

        public bool RetryProcess;

        private void Abort_Click(object sender, EventArgs e)
        {
            RetryProcess = false;
            Close();
        }

        private void Retry_Click(object sender, EventArgs e)
        {
            RetryProcess = true;
            Close();
        }
    }
}
