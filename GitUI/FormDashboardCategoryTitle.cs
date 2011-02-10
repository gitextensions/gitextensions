using System;
using System.Windows.Forms;

namespace GitUI
{
    public partial class FormDashboardCategoryTitle : GitExtensionsForm
    {
        public FormDashboardCategoryTitle()
        {
            InitializeComponent(); Translate();
        }

        public string GetTitle()
        {
            return Title.Text;
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Title.Text))
            {
                MessageBox.Show("You need to enter a caption.", "Enter caption", MessageBoxButtons.OK);
                return;
            }

            Close();
        }
    }
}
