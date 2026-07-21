using Avalonia.Input;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.ScriptsEngine;

// Avalonia twin of GitUI/ScriptsEngine/SimplePrompt.cs.
internal sealed partial class SimplePrompt : GitExtensionsForm, IUserInputPrompt
{
    public string UserInput { get; private set; } = string.Empty;

    public SimplePrompt()
        : this(title: null, label: null, defaultValue: null)
    {
    }

    public SimplePrompt(string? title, string? label, string? defaultValue)
    {
        InitializeComponent();
        AcceptButton = btnOk;
        btnOk.Click += btnOk_Click;
        txtUserInput.KeyDown += txtUserInput_KeyDown;
        InitializeComplete();

        txtUserInput.Text = defaultValue;

        if (!string.IsNullOrWhiteSpace(title))
        {
            Text = title;
        }

        if (!string.IsNullOrWhiteSpace(label))
        {
            labelInput.Text = $"{label}:";
        }
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        txtUserInput.Focus();
    }

    private void btnOk_Click(object? sender, EventArgs e)
    {
        UserInput = txtUserInput.Text ?? string.Empty;
        DialogResult = WinFormsShims.DialogResult.OK;
    }

    private void txtUserInput_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape)
        {
            return;
        }

        if (txtUserInput.SelectionStart == txtUserInput.SelectionEnd)
        {
            DialogResult = WinFormsShims.DialogResult.Cancel;
        }
        else
        {
            txtUserInput.SelectionEnd = txtUserInput.SelectionStart;
        }

        e.Handled = true;
    }
}
