using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GitUI.SettingsDialog
{
    public partial class SimpleHelpDisplayDialog : Form
    {
        public SimpleHelpDisplayDialog()
        {
            InitializeComponent();
        }

        public string DialogTitle { get; set; }

        public string ContentText { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Text = DialogTitle;
            textBox1.Text = ContentText;
            textBox1.Select(0, 0);
        }
    }
}
