using System;
using System.Windows.Forms;

namespace GitUI
{
    /// <summary>
    /// A form that explains that the command needed authentication, and offers to load a private key.
    /// </summary>
    public partial class FormPuttyError : GitExtensionsForm
    {
        public string KeyPath { get; private set; }

        /// <summary>Shows the "SSH error" dialog modally, and returns the path to the key, if one was loaded.</summary>
        public static string AskForKey(IWin32Window parent)
        {
            var form = new FormPuttyError();
            form.ShowDialog(parent);

            return form.KeyPath;
        }
     
        private FormPuttyError()
        {
            InitializeComponent();
            Translate();
        }

        private void LoadSSHKey_Click(object sender, EventArgs e)
        {
            var pathLoaded = BrowseForPrivateKey.BrowseAndLoad(this);
            if (!string.IsNullOrEmpty(pathLoaded))
            {
                KeyPath = pathLoaded;
                Close();
            }
        }        

        private void Abort_Click(object sender, EventArgs e)
        {
            Close();
        }     
    }
}
