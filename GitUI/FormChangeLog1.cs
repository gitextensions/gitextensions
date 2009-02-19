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
    public partial class FormChangeLog1 : GitExtensionsForm
    {
        public FormChangeLog1()
        {
            InitializeComponent();
        }

        private void FormChangeLog1_Load(object sender, EventArgs e)
        {
            ChangeLog.Text = Resources.ChangeLog;
        }
    }
}
