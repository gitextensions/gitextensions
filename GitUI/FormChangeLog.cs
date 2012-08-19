using System;
using System.Windows.Forms;
using GitUI.Properties;

namespace GitUI
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