using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ResourceManager;

namespace GitUI.CommandsDialogs.BrowseDialog.DashboardControl
{
    public partial class FormDashboardCategoryTitle : GitExtensionsForm
    {
        private readonly TranslationString _categoryNameRequiredText = new TranslationString("Category name is required");
        private readonly TranslationString _categoryNameExistsText = new TranslationString("Category name already exists");
        private readonly TranslationString _renameCategoryText = new TranslationString("Rename category");
        private readonly List<string> _existingCategories = new List<string>();

        public FormDashboardCategoryTitle()
        {
            InitializeComponent();
            InitializeComplete();
        }

        public FormDashboardCategoryTitle(IEnumerable<string> existingCategories, string originalName = null)
            : this()
        {
            if (existingCategories != null)
            {
                _existingCategories.AddRange(existingCategories);
            }

            if (originalName != null)
            {
                Category = originalName;
                txtCategoryName.Text = originalName;
                txtCategoryName.SelectAll();
                Text = _renameCategoryText.Text;
            }
        }

        /// <summary>
        /// Gets the new category.
        /// </summary>
        public string Category { get; private set; }

        private void OkButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtCategoryName.Text))
            {
                MessageBox.Show(this, _categoryNameRequiredText.Text, lblCategoryName.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (_existingCategories.Contains(txtCategoryName.Text, StringComparer.Ordinal))
            {
                MessageBox.Show(this, _categoryNameExistsText.Text, lblCategoryName.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Category = txtCategoryName.Text;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void txtCategoryName_TextChanged(object sender, EventArgs e)
        {
            btnOk.Enabled = txtCategoryName.Text != Category;
        }
    }
}
