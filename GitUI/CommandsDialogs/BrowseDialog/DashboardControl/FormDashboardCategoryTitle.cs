using System;
using System.Windows.Forms;
using ResourceManager;

namespace GitUI.CommandsDialogs.BrowseDialog.DashboardControl
{
    public partial class FormDashboardCategoryTitle : GitExtensionsForm
    {
        #region Translation
        private readonly TranslationString _needEnterCaptionText =
            new TranslationString("You need to enter a caption.");
        private readonly TranslationString _needEnterCaptionTextCaption =
            new TranslationString("Enter caption");
        #endregion

        public FormDashboardCategoryTitle()
        {
            InitializeComponent();
            Translate();
        }

        public string GetTitle()
        {
            return Title.Text;
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Title.Text))
            {
                MessageBox.Show(this, _needEnterCaptionText.Text, _needEnterCaptionTextCaption.Text, MessageBoxButtons.OK);
                return;
            }

            Close();
        }
    }
}
