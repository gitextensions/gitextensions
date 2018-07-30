using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitExtUtils.GitUI;
using ResourceManager;

namespace GitUI.Script
{
    public partial class FormFilePrompt : GitExtensionsForm
    {
        public string FileInput { get; private set; } = string.Empty;

        public FormFilePrompt()
        {
            InitializeComponent();
            Translate();
            InitializeComplete();
        }

        protected override void OnRuntimeLoad(EventArgs e)
        {
            base.OnRuntimeLoad(e);

            // scale up for hi DPI
            MaximumSize = DpiUtil.Scale(new Size(800, 116));
            MinimumSize = DpiUtil.Scale(new Size(450, 116));

            txtFilePath.Focus();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            const string separator = " ";
            using (var browseDialog = new OpenFileDialog
            {
                Multiselect = true,
                InitialDirectory = ".",
                AutoUpgradeEnabled = true,
                CheckFileExists = true,
                CheckPathExists = true,
                ValidateNames = true
            })
            {
                if (browseDialog.ShowDialog(this) == DialogResult.OK)
                {
                    txtFilePath.Text = string.Join(separator, browseDialog.FileNames.Select(fileName => fileName.Quote()));
                }
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtFilePath.Text))
            {
                FileInput = txtFilePath.Text;
                DialogResult = DialogResult.OK;
            }
            else
            {
                DialogResult = DialogResult.Cancel;
            }

            Close();
        }

        private void Translate()
        {
            Translator.Translate(this, AppSettings.CurrentTranslation);
        }
    }
}