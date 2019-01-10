using System.Diagnostics;
using System.Windows.Forms;
using GitUI;

namespace Gerrit
{
    public partial class FormPluginInformation : GitExtensionsForm
    {
        public FormPluginInformation()
        {
            InitializeComponent();
            InitializeComplete();
        }

        public static void ShowSubmitted(IWin32Window owner, string change)
        {
            var form = new FormPluginInformation();

            form._NO_TRANSLATE_TargetLabel.Text = change;
            form._NO_TRANSLATE_TargetLabel.Click += (s, e) => Process.Start(change);

            form.ShowDialog(owner);
        }

        private void _NO_TRANSLATE_TargetLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(@"https://github.com/openstack-infra/git-review#git-review");
        }
    }
}
