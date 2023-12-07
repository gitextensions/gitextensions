using GitCommands;
using GitExtensions.Extensibility.Translations;
using GitExtUtils.GitUI;

namespace GitUI.ScriptsEngine
{
    internal partial class FormFilePrompt : GitExtensionsForm, IUserInputPrompt
    {
        public string UserInput { get; private set; } = string.Empty;

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
            using OpenFileDialog browseDialog = new()
            {
                Multiselect = true,
                InitialDirectory = ".",
                AutoUpgradeEnabled = true,
                CheckFileExists = true,
                CheckPathExists = true,
                ValidateNames = true
            };
            if (browseDialog.ShowDialog(this) == DialogResult.OK)
            {
                txtFilePath.Text = string.Join(separator, browseDialog.FileNames.Select(fileName => fileName.Quote()));
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtFilePath.Text))
            {
                UserInput = txtFilePath.Text;
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
