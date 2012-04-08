using System;

namespace GitUI
{
    public partial class FormPuttyError : GitExtensionsForm
    {
        public bool RetryProcess;

        public FormPuttyError()
        {
            InitializeComponent(); Translate();
        }
     
        private void LoadSSHKey_Click(object sender, EventArgs e)
        {
            if (BrowseForPrivateKey.BrowseAndLoad(this))
                CloseForm(true);
        }        

        private void Abort_Click(object sender, EventArgs e)
        {
            CloseForm(false);
        }
      
        private void CloseForm(bool shouldRetry)
        {
            RetryProcess = shouldRetry;
            Close();
        }
    }
}
