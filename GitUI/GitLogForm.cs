using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;

using System.Text;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public partial class GitLogForm : GitExtensionsForm
    {
        public GitLogForm()
        {
            InitializeComponent();
        }

        private void GitLogForm_Load(object sender, EventArgs e)
        {
            Log.Text = Settings.GitLog.ToString();
        }
    }
}
