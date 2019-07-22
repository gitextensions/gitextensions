using System;
using System.Diagnostics;
using System.Windows.Forms;
using ResourceManager;

namespace AzureDevOpsCommitHintPlugin
{
    public class LinkLabelOpener : LinkLabel
    {
        private static readonly string LinkInvalid = new TranslationString("The link to open is invalid").Text;
        private static readonly string OpenLinkFailed = new TranslationString("Fail to open the link").Text;
        public LinkLabelOpener()
        {
            Click += LinkLabelOpener_Click;
        }

        private void LinkLabelOpener_Click(object sender, EventArgs e)
        {
            OpenLink();
        }

        public void OpenLink()
        {
            if (Tag == null || !(Tag is string url) || string.IsNullOrWhiteSpace(url))
            {
                MessageBox.Show(LinkInvalid);
                return;
            }

            try
            {
                Process.Start(url);
            }
            catch (Exception)
            {
                MessageBox.Show(OpenLinkFailed);
            }
        }
    }
}
