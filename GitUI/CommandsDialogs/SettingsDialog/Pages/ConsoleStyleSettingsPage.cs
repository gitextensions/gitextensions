using System.Windows.Forms;
using GitCommands;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class ConsoleStyleSettingsPage : SettingsPageWithHeader
    {
        public ConsoleStyleSettingsPage()
        {
            InitializeComponent();
            Text = "Console style";
            InitializeComplete();
        }

        protected override void Init(ISettingsPageHost pageHost)
        {
            base.Init(pageHost);

            // Bind settings with controls
            AddSettingBinding(AppSettings.ConEmuStyle, _NO_TRANSLATE_cboStyle);
            AddSettingBinding(AppSettings.ConEmuFontSize, cboFontSize);
        }

        public static SettingsPageReference GetPageReference()
        {
            return new SettingsPageReferenceByType(typeof(ConsoleStyleSettingsPage));
        }
    }
}
