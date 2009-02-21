using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GitUI
{
    public partial class FormRevert : GitExtensionsForm
    {
        public FormRevert(string filename)
        {
            FileName = filename;
            InitializeComponent();
        }

        public string FileName { get; set; }

        private void FormRevert_Load(object sender, EventArgs e)
        {
            RevertLabel.Text = "Undo changes in: " + FileName + "?";
        }

        private void Revert_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            string output = GitCommands.GitCommands.ResetFile(FileName);

            if (!string.IsNullOrEmpty(output))
                MessageBox.Show(output, "Reset changes");
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
