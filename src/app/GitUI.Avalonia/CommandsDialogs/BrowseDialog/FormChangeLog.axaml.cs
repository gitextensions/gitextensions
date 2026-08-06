namespace GitUI.CommandsDialogs.BrowseDialog;

public partial class FormChangeLog : GitExtensionsForm
{
    public FormChangeLog()
        : base(enablePositionRestore: true)
    {
        InitializeComponent();
        InitializeComplete();

        ChangeLog.Text = Properties.Resources.ChangeLog;
    }
}
