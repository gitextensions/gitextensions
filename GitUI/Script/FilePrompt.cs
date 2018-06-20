using System;
using System.Text;
using System.Windows.Forms;

namespace GitUI.Script
{
    public partial class FilePrompt : Form
    {
        public string FileInput { get; private set; } = string.Empty;

        public FilePrompt()
        {
            InitializeComponent();
        }

        private void btn_OK_Click(object sender, EventArgs e)
        {
            FileInput = txt_FilePath.Text;
            Close();
        }

        private void FilePrompt_Shown(object sender, EventArgs e)
        {
            txt_FilePath.Focus();
        }

        private void btn_Browse_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            string separator = "; ";
            using (var browseDialog = new OpenFileDialog
            {
                Multiselect = true,
                InitialDirectory = "."
            })
            {
                if (browseDialog.ShowDialog(this) == DialogResult.OK)
                {
                    if (browseDialog.FileNames.Length > 0)
                    {
                        sb.Append(browseDialog.FileNames[0]);
                    }

                    for (int i = 1; i < browseDialog.FileNames.Length; i++)
                    {
                        sb.Append(separator);
                        sb.Append(browseDialog.FileNames[i]);
                    }
                }

                txt_FilePath.Text = sb.ToString();
            }
        }
    }
}
