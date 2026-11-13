using ResourceManager;

namespace GitUI.UserControls;

internal sealed partial class OutputHistoryControl : GitExtensionsControl
{
    public OutputHistoryControl()
    {
        InitializeComponent();
        Name = nameof(OutputHistoryControl);
        TextBox.Options.EnableHyperlinks = true;
        TextBox.Options.EnableEmailHyperlinks = true;
        TextBox.Options.RequireControlModifierForHyperlinkClick = false;
        InitializeComplete();
    }
}
