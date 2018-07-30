using System.Diagnostics;
using System.Windows.Forms;
using GitUI;

namespace Gerrit
{
    public partial class FormGerritChangeSubmitted : GitExtensionsForm
    {
        public FormGerritChangeSubmitted()
        {
            InitializeComponent();
            InitializeComplete();
        }

        public static void ShowSubmitted(IWin32Window owner, string change)
        {
            var form = new FormGerritChangeSubmitted();

            form._NO_TRANSLATE_TargetLabel.Text = change;
            form._NO_TRANSLATE_TargetLabel.Click += (s, e) => Process.Start(change);

            form.ShowDialog(owner);
        }
    }
}
