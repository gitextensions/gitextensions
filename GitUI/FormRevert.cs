using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GitUI
{
    public partial class FormRevert : Form
    {
        public FormRevert(string filename)
        {
            FileName = filename;
            InitializeComponent();
        }

        public string FileName { get; set; }

        private void FormRevert_Load(object sender, EventArgs e)
        {
            RevertLabel.Text = "Undo changes made to: " + FileName;
        }

        private void Revert_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to reset the changes of this file?", "Reset", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                string output = GitCommands.GitCommands.ResetFile(FileName);

                if (!string.IsNullOrEmpty(output))
                    MessageBox.Show(output, "Reset changes");

            }
        }
    }
}
