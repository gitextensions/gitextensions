using System;
using System.Windows.Forms;
using GitUI.Properties;

namespace GitUI
{
    public partial class FormChangeLog : GitExtensionsForm
    {
        public FormChangeLog()
        {
            InitializeComponent();
            Translate();
        }

        private void FormChangeLog1FormClosing(object sender, FormClosingEventArgs e)
        {
            SavePosition("change-log");
        }

        private void FormChangeLog1Load(object sender, EventArgs e)
        {
            RestorePosition("change-log");
            ChangeLog.Text = Resources.ChangeLog;
        }
    }
}