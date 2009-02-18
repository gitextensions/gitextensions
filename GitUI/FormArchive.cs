using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GitUI
{
    public partial class FormArchive : GitExtensionsForm
    {
        public FormArchive()
        {
            InitializeComponent();
        }

        private void Save_Click(object sender, EventArgs e)
        {
            if (revisionGrid1.GetRevisions().Count != 1)
            {
                MessageBox.Show("Select 1 revision to archive", "Archive");
                return;
            }
            string revision = revisionGrid1.GetRevisions()[0].TreeGuid;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Zip file (*.zip)|*.zip";
            saveFileDialog.Title = "Save archive as";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                new FormProcess("archive --format=zip " + revision + " > \"" + saveFileDialog.FileName + "\"");
                Close();
            }
        }


    }
}
