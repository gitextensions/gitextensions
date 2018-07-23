using GitUI.Properties;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    public partial class FormChangeLog : GitExtensionsForm
    {
        public FormChangeLog()
            : base(true)
        {
            InitializeComponent();
            InitializeComplete();

            Load += (s, e) => ChangeLog.Text = Resources.ChangeLog;
        }
    }
}