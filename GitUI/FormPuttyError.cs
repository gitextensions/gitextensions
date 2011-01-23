using System;

namespace GitUI
{
    public partial class FormPuttyError : GitExtensionsForm
    {
        public FormPuttyError()
        {
            InitializeComponent(); Translate();
        }

        private static void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private static void LoadSSHKey_Click(object sender, EventArgs e)
        {
            new FormLoadPuttySshKey().ShowDialog();
        }

        public bool RetryProcess;

        private void button1_Click(object sender, EventArgs e)
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
