using GitUI.Properties;
using GitUI.UserControls;

namespace GitUI.CommandsDialogs.BrowseDialog;

public partial class FormChangeLog : GitExtensionsForm
{
    public FormChangeLog()
        : base(enablePositionRestore: true)
    {
        InitializeComponent();
        InitializeComplete();

        Load += (s, e) => ChangeLog.MarkdownText = Resources.ChangeLog;
    }
}
