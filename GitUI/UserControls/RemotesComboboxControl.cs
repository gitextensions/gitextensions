using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GitUI.UserControls
{
    public partial class RemotesComboboxControl : GitModuleControl
    {
        public RemotesComboboxControl()
        {
            InitializeComponent();
            Translate();
        }

        public string SelectedRemote { get { return (string)comboBoxRemotes.Text; } set { comboBoxRemotes.Text = value; } }

        private void RemotesComboboxControl_Load(object sender, EventArgs e)
        {
            if (Site != null && Site.DesignMode)
            {
                return;
            }

            comboBoxRemotes.DataSource = Module.GetRemotes();
        }
    }
}
