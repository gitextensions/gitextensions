using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GitUI.Properties;

namespace GitUI
{
    public partial class FormChangeLog : GitExtensionsForm
    {
        public FormChangeLog()
        {
            InitializeComponent(); Translate();
        }

        private void FormChangeLog1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SavePosition("change-log");
        }

        private void FormChangeLog1_Load(object sender, EventArgs e)
        {
            RestorePosition("change-log");
            ChangeLog.Text = Resources.ChangeLog;
        }
    }
}
