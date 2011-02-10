using System;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public partial class FormDashboardEditor : GitExtensionsForm
    {
        public FormDashboardEditor()
        {
            InitializeComponent(); Translate();
        }

        private void FormDashboardEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            SavePosition("dashboard-editor");
            Settings.SaveSettings();
        }

        private void FormDashboardEditor_Load(object sender, EventArgs e)
        {
            RestorePosition("dashboard-editor");
        }
    }
}
