using System;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public partial class FormRevert : GitExtensionsForm
    {
        public FormRevert(string filename)
        {
            FileName = filename;
            InitializeComponent(); Translate();
        }

        public string FileName { get; set; }

        private void FormRevert_Load(object sender, EventArgs e)
        {
            _NO_TRANSLATE_RevertLabel.Text = "Undo changes in: " + FileName + "?";
        }

        private void Revert_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            string output = GitCommandHelpers.ResetFile(FileName);

            if (!string.IsNullOrEmpty(output))
                MessageBox.Show(output, "Reset changes");
            Close();
            Cursor.Current = Cursors.Default;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
