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
            if (BrowseForPrivateKey.BrowseAndLoad(this))
                CloseForm(true);
        }

        public bool RetryProcess;

        private void Abort_Click(object sender, EventArgs e)
        {
            CloseForm(false);
        }

        private void Retry_Click(object sender, EventArgs e)
        {
            CloseForm(true);
        }

        private void CloseForm(bool shouldRetry)
        {
            RetryProcess = shouldRetry;
            Close();
        }
    }
}
