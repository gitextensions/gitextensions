using ResourceManager;

namespace GitUI.UserControls;

internal sealed partial class OutputHistoryControl : GitExtensionsControl
{
    public OutputHistoryControl()
    {
        InitializeComponent();
        InitializeComplete();
    }
}
