using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using GitUIPluginInterfaces;
using ResourceManager.Translation;

namespace ProxySwitcher
{
    public partial class ProxySwitcherForm : Form
    {
        private readonly IGitPluginSettingsContainer settings;
        private readonly GitUIBaseEventArgs gitUiCommands;
        private readonly IGitModule gitCommands;

        #region Translation
        private readonly TranslationString _pluginDescription = new TranslationString("Proxy Switcher");
        private readonly TranslationString _pleaseSetProxy = new TranslationString("There is no proxy configured. Please set the proxy host in the plugin settings.");
        #endregion

        public ProxySwitcherForm(IGitPluginSettingsContainer settings, GitUIBaseEventArgs gitUiCommands)
        {
            InitializeComponent();

            this.Text = _pluginDescription.Text;
            this.settings = settings;
            this.gitUiCommands = gitUiCommands;
            this.gitCommands = gitUiCommands.GitModule;
        }

        private void ProxySwitcherForm_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(settings.GetSetting(SettingsKey.HttpProxy)))
            {
                MessageBox.Show(this, _pleaseSetProxy.Text, this.Text, MessageBoxButtons.OK);
                this.Close();
            }
            else
            {
                RefreshProxy();
            }
        }

        private void RefreshProxy()
        {
            LocalHttpProxy_TextBox.Text = HidePassword(gitCommands.RunGit("config --get http.proxy"));
            GlobalHttpProxy_TextBox.Text = HidePassword(gitCommands.RunGit("config --global --get http.proxy"));
            ApplyGlobally_CheckBox.Checked = string.Equals(LocalHttpProxy_TextBox.Text, GlobalHttpProxy_TextBox.Text);
        }

        private string HidePassword(string httpProxy)
        {
            return Regex.Replace(httpProxy, ":(.*)@", ":****@");
        }

        private string BuildHttpProxy()
        {
            var sb = new StringBuilder();
            sb.Append("\"");
            var username = settings.GetSetting(SettingsKey.Username);
            if (!string.IsNullOrEmpty(username))
            {
                var password = settings.GetSetting(SettingsKey.Password);
                sb.Append(username);
                if(!string.IsNullOrEmpty(password))
                {
                    sb.Append(":");
                    sb.Append(password);
                }
                sb.Append("@");
            }
            sb.Append(settings.GetSetting(SettingsKey.HttpProxy));
            var port = settings.GetSetting(SettingsKey.HttpProxyPort);
            if (!string.IsNullOrEmpty(port))
            {
                sb.Append(":");
                sb.Append(port);
            }
            sb.Append("\"");
            return sb.ToString();
        }

        private void SetProxy_Button_Click(object sender, EventArgs e)
        {
            var httpproxy = BuildHttpProxy();
            if (ApplyGlobally_CheckBox.Checked)
            {
                gitCommands.RunGit("config --global http.proxy " + httpproxy);
            }
            else
            {
                gitCommands.RunGit("config http.proxy " + httpproxy);
            }
            RefreshProxy();
        }

        private void UnsetProxy_Button_Click(object sender, EventArgs e)
        {
            if (ApplyGlobally_CheckBox.Checked)
            {
                gitCommands.RunGit("config --global --unset http.proxy");
            }
            else
            {
                gitCommands.RunGit("config --unset http.proxy");
            }
            RefreshProxy();
        }
    }
}
