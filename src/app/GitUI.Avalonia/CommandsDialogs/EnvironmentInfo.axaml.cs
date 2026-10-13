using Avalonia;
using Avalonia.Controls;
using GitUI.Compat;

namespace GitUI.CommandsDialogs;

// The label content is the dynamic environment report; the control contributes no XLF keys.
[Untranslated]
public partial class EnvironmentInfo : UserControl
{
    public EnvironmentInfo()
    {
        if (Design.IsDesignMode)
        {
            UserEnvironmentInformation.Initialise(
            "9999999999999999999999999999999999abcdef", true);
        }

        InitializeComponent();
        copyButton.Click += copyButton_Click;

        environmentIssueInfo.Text = UserEnvironmentInformation.GetInformation().Replace("- ", "");
    }

    public void SetCopyButtonTooltip(string tooltip)
    {
        // WinForms used a shared ToolTip component; Avalonia sets the tip directly on the control.
        ToolTip.SetTip(copyButton, tooltip);
    }

    private void copyButton_Click(object? sender, EventArgs e)
    {
        UserEnvironmentInformation.CopyInformation();
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(EnvironmentInfo control)
    {
        public TextBlock EnvironmentIssueInfo => control.environmentIssueInfo;
        public IconButton CopyButton => control.copyButton;
    }
}
