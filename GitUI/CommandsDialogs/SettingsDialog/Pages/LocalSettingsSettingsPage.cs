using System.Windows.Forms;
using GitCommands;
using GitCommands.Config;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class LocalSettingsSettingsPage : SettingsPageBase
    {
        readonly CommonLogic _commonLogic;
        readonly CheckSettingsLogic _checkSettingsLogic;
        readonly GitModule _gitModule;

        public static SettingsPageReference GetPageReference()
        {
            return new SettingsPageReferenceByType(typeof(LocalSettingsSettingsPage));
        }

        private LocalSettingsSettingsPage()
        {
            InitializeComponent();
            Text = "Local settings";
            Translate();
        }

        public LocalSettingsSettingsPage(CommonLogic commonLogic, CheckSettingsLogic checkSettingsLogic, GitModule gitModule)
            : this()
        {
            _commonLogic = commonLogic;
            _checkSettingsLogic = checkSettingsLogic;
            _gitModule = gitModule;

            _commonLogic.FillEncodings(Local_FilesEncoding);
        }

        public override void OnPageShown()
        {
            bool canFindGitCmd = _checkSettingsLogic.CanFindGitCmd();

            InvalidGitPathLocal.Visible = !canFindGitCmd;

            bool valid = _gitModule.IsValidGitWorkingDir() && canFindGitCmd;
            UserName.Enabled = valid;
            UserEmail.Enabled = valid;
            Editor.Enabled = valid;
            LocalMergeTool.Enabled = valid;
            KeepMergeBackup.Enabled = valid;
            localAutoCrlfFalse.Enabled = valid;
            localAutoCrlfInput.Enabled = valid;
            localAutoCrlfTrue.Enabled = valid;
            NoGitRepo.Visible = !valid;
        }

        protected override void OnLoadSettings()
        {
            _commonLogic.EncodingToCombo(_gitModule.GetFilesEncoding(true), Local_FilesEncoding);

            ConfigFile localConfig = _gitModule.GetLocalConfig();

            UserName.Text = localConfig.GetValue("user.name");
            UserEmail.Text = localConfig.GetValue("user.email");
            Editor.Text = localConfig.GetPathValue("core.editor");
            LocalMergeTool.Text = localConfig.GetValue("merge.tool");

            CommonLogic.SetCheckboxFromString(KeepMergeBackup, localConfig.GetValue("mergetool.keepBackup"));

            string autocrlf = localConfig.GetValue("core.autocrlf").ToLower();
            localAutoCrlfFalse.Checked = autocrlf == "false";
            localAutoCrlfInput.Checked = autocrlf == "input";
            localAutoCrlfTrue.Checked = autocrlf == "true";
        }

        public override void SaveSettings()
        {
            _gitModule.SetFilesEncoding(true, _commonLogic.ComboToEncoding(Local_FilesEncoding));

            if (_checkSettingsLogic.CanFindGitCmd())
            {
                ConfigFile localConfig = _gitModule.GetLocalConfig();

                if (string.IsNullOrEmpty(UserName.Text) || !UserName.Text.Equals(localConfig.GetValue("user.name")))
                    localConfig.SetValue("user.name", UserName.Text);
                if (string.IsNullOrEmpty(UserEmail.Text) || !UserEmail.Text.Equals(localConfig.GetValue("user.email")))
                    localConfig.SetValue("user.email", UserEmail.Text);
                localConfig.SetPathValue("core.editor", Editor.Text);
                localConfig.SetValue("merge.tool", LocalMergeTool.Text);


                if (KeepMergeBackup.CheckState == CheckState.Checked)
                    localConfig.SetValue("mergetool.keepBackup", "true");
                else if (KeepMergeBackup.CheckState == CheckState.Unchecked)
                    localConfig.SetValue("mergetool.keepBackup", "false");

                if (localAutoCrlfFalse.Checked) localConfig.SetValue("core.autocrlf", "false");
                if (localAutoCrlfInput.Checked) localConfig.SetValue("core.autocrlf", "input");
                if (localAutoCrlfTrue.Checked) localConfig.SetValue("core.autocrlf", "true");

                CommonLogic.SetEncoding(_gitModule.GetFilesEncoding(true), localConfig, "i18n.filesEncoding");

                //Only save local settings when we are inside a valid working dir
                if (_gitModule.IsValidGitWorkingDir())
                {
                    localConfig.Save();
                }
            }
        }
    }
}
