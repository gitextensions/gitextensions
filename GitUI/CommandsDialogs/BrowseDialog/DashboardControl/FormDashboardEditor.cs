using System.Windows.Forms;
using GitCommands;
using GitCommands.Properties;

namespace GitUI.CommandsDialogs.BrowseDialog.DashboardControl
{
    public partial class FormDashboardEditor : GitExtensionsForm
    {
        public FormDashboardEditor()
            : base(true)
        {
            InitializeComponent(); Translate();
        }

        private void FormDashboardEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.Default.Save();
        }
    }
}
