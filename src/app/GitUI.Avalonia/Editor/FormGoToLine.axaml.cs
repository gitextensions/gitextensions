using Avalonia.Controls;
using Avalonia.VisualTree;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.Editor;

/// <summary>Prompts for a one-based line number.</summary>
public partial class FormGoToLine : GitExtensionsForm
{
    /// <summary>Initializes the line-number dialog.</summary>
    public FormGoToLine()
    {
        InitializeComponent();

        okBtn.Click += (_, _) => DialogResult = WinFormsShims.DialogResult.OK;
        cancelBtn.Click += (_, _) => DialogResult = WinFormsShims.DialogResult.Cancel;
        Opened += FormGoToLine_Load;
        AcceptButton = okBtn;

        InitializeComplete();
    }

    /// <summary>Gets the selected one-based line number.</summary>
    public int GetLineNumber()
    {
        return decimal.ToInt32(_NO_TRANSLATE_LineNumberUpDown.Value ?? 1);
    }

    /// <summary>Sets the largest selectable line number.</summary>
    public void SetMaxLineNumber(int maxLineNumber)
    {
        _NO_TRANSLATE_LineNumberUpDown.Maximum = maxLineNumber;
        lineLabel.Text = lineLabel.Text + " (1 - " + maxLineNumber + "):";
    }

    private void FormGoToLine_Load(object? sender, EventArgs e)
    {
        _NO_TRANSLATE_LineNumberUpDown.Focus();
        _NO_TRANSLATE_LineNumberUpDown.ApplyTemplate();
        _NO_TRANSLATE_LineNumberUpDown.GetVisualDescendants().OfType<TextBox>().FirstOrDefault()?.SelectAll();
    }
}
