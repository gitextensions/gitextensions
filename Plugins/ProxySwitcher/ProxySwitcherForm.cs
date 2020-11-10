using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using GitExtensions.Core.Commands;
using GitExtensions.Core.Commands.Events;
using GitExtensions.Core.Module;
using GitExtensions.Core.Settings;
using ProxySwitcher.Properties;

namespace ProxySwitcher
{
    public partial class ProxySwitcherForm : Form
    {
        private readonly Plugin _plugin;
        private readonly ISettingsSource _settings;
        private readonly IGitModule _gitCommands;

        /// <summary>
        /// Default constructor added to register all strings to be translated
        /// Use the other constructor:
        /// ProxySwitcherForm(IGitPluginSettingsContainer settings, GitUIBaseEventArgs gitUiCommands)
        /// </summary>
        public ProxySwitcherForm()
        {
            InitializeComponent();
        }

        public ProxySwitcherForm(Plugin plugin, ISettingsSource settings, GitUIEventArgs gitUiCommands)
        {
            InitializeComponent();

            Text = Strings.FormText;
            ApplyGlobally_CheckBox.Text = Strings.ApplyGloballyCheckBoxText;
            GlobalHttpProxy_Label.Text = Strings.GlobalHttpProxyLabelText;
            LocalHttpProxy_Label.Text = Strings.LocalHttpProxyLabelText;
            SetProxy_Button.Text = Strings.SetProxyButtonText;
            UnsetProxy_Button.Text = Strings.UnsetProxyButtonText;

            _plugin = plugin;
            _settings = settings;
            _gitCommands = gitUiCommands.GitModule;
        }

        private void ProxySwitcherForm_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_plugin.HttpProxy.ValueOrDefault(_settings)))
            {
                MessageBox.Show(this, Strings.PleaseSetProxyText, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
            else
            {
                RefreshProxy();
            }
        }

        private void RefreshProxy()
        {
            var args = new GitArgumentBuilder("config")
            {
                "--get",
                "http.proxy"
            };
            LocalHttpProxy_TextBox.Text = HidePassword(_gitCommands.GitExecutable.GetOutput(args));
            args = new GitArgumentBuilder("config")
            {
                "--global",
                "--get",
                "http.proxy"
            };
            GlobalHttpProxy_TextBox.Text = HidePassword(_gitCommands.GitExecutable.GetOutput(args));
            ApplyGlobally_CheckBox.Checked = string.Equals(LocalHttpProxy_TextBox.Text, GlobalHttpProxy_TextBox.Text);
        }

        private static string HidePassword(string httpProxy)
        {
            return Regex.Replace(httpProxy, ":(.*)@", ":****@");
        }

        private string BuildHttpProxy()
        {
            var sb = new StringBuilder();
            sb.Append("\"");
            var username = _plugin.Username.ValueOrDefault(_settings);
            if (!string.IsNullOrEmpty(username))
            {
                var password = _plugin.Password.ValueOrDefault(_settings);
                sb.Append(username);
                if (!string.IsNullOrEmpty(password))
                {
                    sb.Append(":");
                    sb.Append(password);
                }

                sb.Append("@");
            }

            sb.Append(_plugin.HttpProxy.ValueOrDefault(_settings));
            var port = _plugin.HttpProxyPort.ValueOrDefault(_settings);
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
            var httpProxy = BuildHttpProxy();

            var args = new GitArgumentBuilder("config")
            {
                { ApplyGlobally_CheckBox.Checked, "--global" },
                "http.proxy",
                httpProxy
            };
            _gitCommands.GitExecutable.GetOutput(args);

            RefreshProxy();
        }

        private void UnsetProxy_Button_Click(object sender, EventArgs e)
        {
            var arguments = ApplyGlobally_CheckBox.Checked
                ? "config --global --unset http.proxy"
                : "config --unset http.proxy";

            _gitCommands.GitExecutable.GetOutput(arguments);

            RefreshProxy();
        }
    }
}
