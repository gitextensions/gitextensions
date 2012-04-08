using System;
using System.Windows.Forms;

namespace GitUI
{
    /// <summary>
    /// A form that explains that the command needed authentication, and offers to load a private key.
    /// </summary>
    public partial class FormPuttyError : GitExtensionsForm
    {
        public bool KeyLoaded { get; private set; }

        /// <summary>Shows the "SSH error" dialog modally, and returns if a key was loaded.</summary>
        public static bool AskForKey(IWin32Window parent)
        {
            var form = new FormPuttyError();
            form.ShowDialog(parent);

            return form.KeyLoaded;
        }
     
        private FormPuttyError()
        {
            KeyLoaded = false;
            InitializeComponent(); Translate();
        }

        private void LoadSSHKey_Click(object sender, EventArgs e)
        {
            if (BrowseForPrivateKey.BrowseAndLoad(this))
            {
                KeyLoaded = true;
                Close();
            }
        }        

        private void Abort_Click(object sender, EventArgs e)
        {
            Close();
        }     
    }
}
