using GitCommands;
using ResourceManager;

namespace GitUI.UserControls;

internal partial class OutputHistoryControl : GitExtensionsControl
{
    internal OutputHistoryControl()
    {
        InitializeComponent();
        TextBox.Font = AppSettings.FixedWidthFont;
        InitializeComplete();
    }
}
