using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;

namespace GitUI
{
    /// <summary>
    /// A form that explains that the command needed authentication, and offers to load a private key.
    /// </summary>
    public partial class FormPuttyError : GitExtensionsForm
    {
        /// <summary>Shows the "SSH error" dialog modally, and returns the path to the key, if one was loaded.</summary>
        public static bool AskForKey(IWin32Window parent, [NotNullWhen(returnValue: true)] out string? keyPath)
        {
            using FormPuttyError form = new();
            var result = form.ShowDialog(parent);
            keyPath = form.KeyPath;
            return result == DialogResult.Retry;
        }

        public string? KeyPath { get; private set; }

        public FormPuttyError()
        {
            InitializeComponent();
            InitializeComplete();
        }

        private void LoadSSHKey_Click(object sender, EventArgs e)
        {
            var pathLoaded = BrowseForPrivateKey.BrowseAndLoad(this);
            if (!string.IsNullOrEmpty(pathLoaded))
            {
                KeyPath = pathLoaded;
                DialogResult = DialogResult.Retry;
                Close();
            }
        }
    }
}
