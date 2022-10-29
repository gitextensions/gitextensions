using GitCommands;
using ResourceManager;

namespace GitUI.UserControls;

public partial class ProcessHistoryControl : GitExtensionsControl
{
    public ProcessHistoryControl()
    {
        InitializeComponent();
        TextBox.Font = AppSettings.FixedWidthFont;
        InitializeComplete();
    }
}
