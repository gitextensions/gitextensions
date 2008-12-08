using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace GitUI
{
    partial class AboutBox : Form
    {
        public AboutBox()
        {
            InitializeComponent();

        }

        private void okButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void labelVersion_Click(object sender, EventArgs e)
        {

        }

    }
}
