namespace GitUI.CommandsDialogs.BrowseDialog;

public partial class FormChangeLog : GitExtensionsForm
{
    public FormChangeLog()
        : base(enablePositionRestore: true)
    {
        InitializeComponent();
        InitializeComplete();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        ChangeLog.Text = Properties.Resources.ChangeLog;
    }
}
