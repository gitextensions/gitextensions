using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GitUI
{
    public partial class FormDashboardEditor : GitExtensionsForm
    {
        public FormDashboardEditor()
        {
            InitializeComponent();
        }

        private void FormDashboardEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            SavePosition("dashboard-editor");
        }

        private void FormDashboardEditor_Load(object sender, EventArgs e)
        {
            RestorePosition("dashboard-editor");
        }
    }
}
