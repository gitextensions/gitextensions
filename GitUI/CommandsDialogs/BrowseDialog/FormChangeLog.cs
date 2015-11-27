using System;
using GitUI.Properties;
using ResourceManager;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    public partial class FormChangeLog : GitExtensionsForm
    {
        public FormChangeLog()
            : base(true)
        {
            InitializeComponent();
            Translate();
        }

        private void FormChangeLog1Load(object sender, EventArgs e)
        {
            ChangeLog.Text = Resources.ChangeLog;
        }
    }
}