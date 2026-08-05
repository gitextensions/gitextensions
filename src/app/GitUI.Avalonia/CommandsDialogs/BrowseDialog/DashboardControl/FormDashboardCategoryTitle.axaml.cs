using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs.BrowseDialog.DashboardControl;

public partial class FormDashboardCategoryTitle : GitExtensionsForm
{
    private readonly TranslationString _categoryNameRequiredText = new("Category name is required");
    private readonly TranslationString _categoryNameExistsText = new("Category name already exists");
    private readonly TranslationString _renameCategoryText = new("Rename category");
    private readonly List<string> _existingCategories = [];

    public FormDashboardCategoryTitle()
    {
        InitializeComponent();
        btnOk.Click += OkButton_Click;
        btnCancel.Click += btnCancel_Click;
        txtCategoryName.TextChanged += txtCategoryName_TextChanged;
        InitializeComplete();
    }

    public FormDashboardCategoryTitle(IEnumerable<string> existingCategories, string? originalName = null)
        : this()
    {
        if (existingCategories is not null)
        {
            _existingCategories.AddRange(existingCategories);
        }

        if (originalName is not null)
        {
            Category = originalName;
            txtCategoryName.Text = originalName;
            txtCategoryName.SelectAll();
            btnOk.IsEnabled = false;
            Text = _renameCategoryText.Text;
        }
    }

    /// <summary>
    /// Gets the new category.
    /// </summary>
    public string? Category { get; private set; }

    private void OkButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(txtCategoryName.Text))
        {
            MessageBoxes.Show(this, _categoryNameRequiredText.Text, lblCategoryName.Text ?? string.Empty, WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Error);
            return;
        }

        if (_existingCategories.Contains(txtCategoryName.Text, StringComparer.Ordinal))
        {
            MessageBoxes.Show(this, _categoryNameExistsText.Text, lblCategoryName.Text ?? string.Empty, WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Error);
            return;
        }

        Category = txtCategoryName.Text;
        DialogResult = WinFormsShims.DialogResult.OK;
    }

    private void btnCancel_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        DialogResult = WinFormsShims.DialogResult.Cancel;
    }

    private void txtCategoryName_TextChanged(object? sender, Avalonia.Controls.TextChangedEventArgs e)
    {
        btnOk.IsEnabled = txtCategoryName.Text != Category;
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(FormDashboardCategoryTitle form)
    {
        internal Avalonia.Controls.TextBox CategoryName => form.txtCategoryName;
        internal Avalonia.Controls.Button Ok => form.btnOk;
        internal Avalonia.Controls.Button Cancel => form.btnCancel;
    }
}
