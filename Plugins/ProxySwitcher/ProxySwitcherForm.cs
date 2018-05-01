﻿using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using GitUIPluginInterfaces;
using ResourceManager;
using Settings = GitCommands.AppSettings;

namespace ProxySwitcher
{
    public partial class ProxySwitcherForm : GitExtensionsFormBase
    {
        private readonly ProxySwitcherPlugin _plugin;
        private readonly ISettingsSource _settings;
        private readonly IGitModule _gitCommands;

        #region Translation
        private readonly TranslationString _pluginDescription = new TranslationString("Proxy Switcher");
        private readonly TranslationString _pleaseSetProxy = new TranslationString("There is no proxy configured. Please set the proxy host in the plugin settings.");
        #endregion

        /// <summary>
        /// Default constructor added to register all strings to be translated
        /// Use the other constructor:
        /// ProxySwitcherForm(IGitPluginSettingsContainer settings, GitUIBaseEventArgs gitUiCommands)
        /// </summary>
        public ProxySwitcherForm()
        {
            InitializeComponent();
        }

        public ProxySwitcherForm(ProxySwitcherPlugin plugin, ISettingsSource settings, GitUIEventArgs gitUiCommands)
        {
            InitializeComponent();
            Translate();

            Text = _pluginDescription.Text;
            _plugin = plugin;
            _settings = settings;
            _gitCommands = gitUiCommands.GitModule;
        }

        private void ProxySwitcherForm_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_plugin.HttpProxy.ValueOrDefault(_settings)))
            {
                MessageBox.Show(this, _pleaseSetProxy.Text, Text, MessageBoxButtons.OK);
                Close();
            }
            else
            {
                RefreshProxy();
            }
        }

        private void RefreshProxy()
        {
            LocalHttpProxy_TextBox.Text = HidePassword(_gitCommands.RunGitCmd("config --get http.proxy"));
            GlobalHttpProxy_TextBox.Text = HidePassword(_gitCommands.RunGitCmd("config --global --get http.proxy"));
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
            var httpproxy = BuildHttpProxy();

            var arguments = ApplyGlobally_CheckBox.Checked
                ? $"config --global http.proxy {httpproxy}"
                : $"config http.proxy {httpproxy}";

            _gitCommands.RunGitCmd(arguments);

            RefreshProxy();
        }

        private void UnsetProxy_Button_Click(object sender, EventArgs e)
        {
            var arguments = ApplyGlobally_CheckBox.Checked
                ? "config --global --unset http.proxy"
                : "config --unset http.proxy";

            _gitCommands.RunGitCmd(arguments);

            RefreshProxy();
        }
    }
}
