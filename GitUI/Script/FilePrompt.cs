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

        protected override void OnShown(EventArgs e)
        {
            txt_FilePath.Focus();
        }

        private void Btn_OK_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txt_FilePath.Text))
            {
                FileInput = txt_FilePath.Text;
                DialogResult = DialogResult.OK;
            }

            Close();
        }

        private void Btn_Browse_Click(object sender, EventArgs e)
        {
            const string separator = " ";
            using (var browseDialog = new OpenFileDialog
            {
                Multiselect = true,
                InitialDirectory = "."
            })
            {
                if (browseDialog.ShowDialog(this) == DialogResult.OK)
                {
                    txt_FilePath.Text = string.Join(separator, browseDialog.FileNames.Select(fileName => fileName.Quote()));
                }
            }
        }
    }
}
