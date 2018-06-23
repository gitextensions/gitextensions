using System;
using System.Linq;
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
            string separator = " ";
            using (var browseDialog = new OpenFileDialog
            {
                Multiselect = true,
                InitialDirectory = "."
            })
            {
                if (browseDialog.ShowDialog(this) == DialogResult.OK)
                {
                    txt_FilePath.Text = string.Join(separator, browseDialog.FileNames.Select(x => $"\"{x}\""));
                }
            }
        }
    }
}
