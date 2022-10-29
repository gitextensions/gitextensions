using GitCommands;
using ResourceManager;

namespace GitUI.UserControls;

public partial class OutputHistoryControl : GitExtensionsControl
{
    public OutputHistoryControl()
    {
        InitializeComponent();
        TextBox.Font = AppSettings.FixedWidthFont;
        InitializeComplete();
    }
}
